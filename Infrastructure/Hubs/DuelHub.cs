using Microsoft.AspNetCore.SignalR;
using Infrastructure.Services;
using Application.DTOs.Duel;
using Application.DTOs.Testing;
using Domain.Enums;
using System.Collections.Concurrent;

namespace Infrastructure.Hubs;

public class DuelHub : Hub
{
    private readonly DuelManager _duelManager;
    private readonly IHubContext<DuelHub> _hubContext;
    
    // Cache for storing individual player results before sending RoundResult
    private static readonly ConcurrentDictionary<string, Dictionary<string, DuelSubmissionResult>> _roundAnswerCache = new();
    
    public DuelHub(DuelManager duelManager, IHubContext<DuelHub> hubContext)
    {
        _duelManager = duelManager;
        _hubContext = hubContext;
    }
    
    public async Task FindMatch(string userId, string userName, string? profilePicture, int subjectId)
    {
        var session = await _duelManager.FindMatchAsync(Context.ConnectionId, userId, userName, profilePicture, subjectId);
        if (session != null)
        {
            await Groups.AddToGroupAsync(session.Player1.ConnectionId, session.SessionId);
            await Groups.AddToGroupAsync(session.Player2.ConnectionId, session.SessionId);
            
            // Send MatchFoundEvent
            var matchEvent = new MatchFoundEvent
            {
                SessionId = session.SessionId,
                Player1 = session.Player1,
                Player2 = session.Player2,
                Status = session.Status
            };
            await Clients.Group(session.SessionId).SendAsync("MatchFound", matchEvent);
        }
        else
        {
            await Clients.Caller.SendAsync("WaitingForMatch");
        }
    }

    public async Task ClientReady(string sessionId)
    {
        var session = _duelManager.GetSession(sessionId);
        if (session == null) 
        {
            Console.WriteLine($"[DuelHub] ClientReady: Session {sessionId} not found.");
            return;
        }

        // SAFETY TIMEOUT: Check if 10 seconds have passed since match found
        if (session.MatchFoundAt.HasValue)
        {
            var elapsed = DateTime.UtcNow - session.MatchFoundAt.Value;
            if (elapsed.TotalSeconds > 10)
            {
                Console.WriteLine($"[DuelHub] Session {sessionId} timed out (elapsed: {elapsed.TotalSeconds}s)");
                await Clients.Group(sessionId).SendAsync("DuelError", "Connection Timeout: Questions failed to load");
                _duelManager.RemoveSession(sessionId);
                return;
            }
        }

        bool shouldStart = false;
        bool hasError = false;
        string? errorMessage = null;

        lock (session)
        {
            Console.WriteLine($"[DuelHub] ClientReady: User={Context.ConnectionId}, Session={sessionId}, Status={session.Status}");
            
            if (Context.ConnectionId == session.Player1.ConnectionId) 
            {
                session.Player1.IsReady = true;
                Console.WriteLine("[DuelHub] Player 1 Ready");
            }
            else if (Context.ConnectionId == session.Player2.ConnectionId) 
            {
                session.Player2.IsReady = true;
                Console.WriteLine("[DuelHub] Player 2 Ready");
            }
            else 
            {
                Console.WriteLine($"[DuelHub] ClientReady: ConnectionId mismatch! Hub={Context.ConnectionId}, P1={session.Player1.ConnectionId}, P2={session.Player2.ConnectionId}");
            }

            if (session.Player1.IsReady && session.Player2.IsReady && session.QuestionsReady && session.Status == DuelStatus.Starting)
            {
                if (session.Questions.Count == 0)
                {
                    Console.WriteLine($"[DuelHub] ERROR: Session {sessionId} has 0 questions! Cannot start.");
                    hasError = true;
                    errorMessage = "Дар ин фан саволҳо ёфт нашуданд.";
                }
                else
                {
                    session.Status = DuelStatus.InProgress;
                    session.CurrentQuestionIndex = 0;
                    shouldStart = true;
                    Console.WriteLine($"[DuelHub] Starting Game for session {sessionId}");
                }
            }
            else 
            {
                Console.WriteLine($"[DuelHub] Waiting: P1Ready={session.Player1.IsReady}, P2Ready={session.Player2.IsReady}, QReady={session.QuestionsReady}, Status={session.Status}");
            }
        }

        if (hasError)
        {
            await Clients.Group(sessionId).SendAsync("DuelError", errorMessage);
        }
        else if (shouldStart)
        {
            // Start 30-second timer for first question
            await StartQuestionWithTimer(session, 0);
        }
    }

    private async Task StartQuestionWithTimer(DuelSession session, int questionIndex)
    {
        // Cancel any existing timer
        session.QuestionTimerCts?.Cancel();
        session.QuestionTimerCts = new CancellationTokenSource();
        var cts = session.QuestionTimerCts;
        
        session.CurrentQuestionStartedAt = DateTime.UtcNow;
        
        // Send QuestionStartEvent
        var questionEvent = new QuestionStartEvent
        {
            Question = session.Questions[questionIndex],
            QuestionIndex = questionIndex,
            TimeLimitSeconds = 30
        };
        await Clients.Group(session.SessionId).SendAsync("QuestionStart", questionEvent);
        
        // Start background timer
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(30000, cts.Token); // 30 seconds
                
                if (cts.Token.IsCancellationRequested) return;
                
                // Timer expired - force timeout for players who haven't answered
                Console.WriteLine($"[DuelHub] TIMEOUT: 30 seconds passed for session {session.SessionId}, Q{questionIndex}");
                
                await HandleQuestionTimeout(session, questionIndex);
            }
            catch (TaskCanceledException)
            {
                // Timer was cancelled (both players answered in time)
                Console.WriteLine($"[DuelHub] Timer cancelled for session {session.SessionId}, Q{questionIndex}");
            }
        });
    }

    private async Task HandleQuestionTimeout(DuelSession session, int questionIndex)
    {
        // Check if session is still valid
        if (session.Status != DuelStatus.InProgress || session.CurrentQuestionIndex != questionIndex)
            return;

        bool p1TimedOut = !session.Player1Answered;
        bool p2TimedOut = !session.Player2Answered;

        Console.WriteLine($"[DuelHub] Timeout check: P1TimedOut={p1TimedOut}, P2TimedOut={p2TimedOut}");

        // Force timeout for players who haven't answered
        if (p1TimedOut)
        {
            var p1Result = _duelManager.ForceTimeoutForPlayer(session.SessionId, session.Player1.UserId, questionIndex);
            if (p1Result.Success)
            {
                await _hubContext.Clients.Client(session.Player1.ConnectionId).SendAsync("AnswerResult", p1Result);
            }
        }

        if (p2TimedOut)
        {
            var p2Result = _duelManager.ForceTimeoutForPlayer(session.SessionId, session.Player2.UserId, questionIndex);
            if (p2Result.Success)
            {
                await _hubContext.Clients.Client(session.Player2.ConnectionId).SendAsync("AnswerResult", p2Result);
            }
        }

        // Refresh session state after forced timeouts
        var updatedSession = _duelManager.GetSession(session.SessionId);
        if (updatedSession == null) return;

        // If both have now answered (including timeouts), process round end
        if (updatedSession.AnsweredCount >= 2 || (p1TimedOut && p2TimedOut) || 
            (!p1TimedOut && p2TimedOut && updatedSession.Player1Answered) ||
            (p1TimedOut && !p2TimedOut && updatedSession.Player2Answered))
        {
            await ProcessRoundEnd(updatedSession, questionIndex);
        }
    }

    private async Task ProcessRoundEnd(DuelSession session, int questionIndex)
    {
        // Get results from session
        var player1Result = session.Player1LastResult ?? new DuelSubmissionResult { Success = true, IsCorrect = false, AddedScore = 0 };
        var player2Result = session.Player2LastResult ?? new DuelSubmissionResult { Success = true, IsCorrect = false, AddedScore = 0 };

        // Build RoundResult for both players
        var p1RoundResult = new RoundResultEvent
        {
            IsCorrect = player1Result.IsCorrect,
            CorrectAnswerId = player1Result.CorrectAnswerId,
            CorrectPairIds = player1Result.CorrectPairIds,
            MyTotalScore = session.Player1TotalScore,
            OpponentTotalScore = session.Player2TotalScore,
            PointsEarned = player1Result.AddedScore,
            IsRoundOver = true,
            IsDuelFinished = session.Status == DuelStatus.Finished
        };
        
        var p2RoundResult = new RoundResultEvent
        {
            IsCorrect = player2Result.IsCorrect,
            CorrectAnswerId = player2Result.CorrectAnswerId,
            CorrectPairIds = player2Result.CorrectPairIds,
            MyTotalScore = session.Player2TotalScore,
            OpponentTotalScore = session.Player1TotalScore,
            PointsEarned = player2Result.AddedScore,
            IsRoundOver = true,
            IsDuelFinished = session.Status == DuelStatus.Finished
        };

        await _hubContext.Clients.Client(session.Player1.ConnectionId).SendAsync("RoundResult", p1RoundResult);
        await _hubContext.Clients.Client(session.Player2.ConnectionId).SendAsync("RoundResult", p2RoundResult);

        // Reset round state
        session.Player1Answered = false;
        session.Player2Answered = false;
        session.AnsweredCount = 0;
        session.CurrentQuestionIndex++;

        await Task.Delay(3000);

        if (session.CurrentQuestionIndex >= session.Questions.Count || session.Status == DuelStatus.Finished)
        {
            session.Status = DuelStatus.Finished;
            Console.WriteLine($"[DuelHub] Duel finished. Sending DuelFinished event.");
            var finishedEvent = new DuelFinishedEvent
            {
                SessionId = session.SessionId,
                Player1 = session.Player1,
                Player2 = session.Player2,
                Player1TotalScore = session.Player1TotalScore,
                Player2TotalScore = session.Player2TotalScore,
                WinnerId = session.Player1TotalScore > session.Player2TotalScore ? session.Player1.UserId : 
                           session.Player2TotalScore > session.Player1TotalScore ? session.Player2.UserId : null,
                Status = DuelStatus.Finished
            };
            await _hubContext.Clients.Group(session.SessionId).SendAsync("DuelFinished", finishedEvent);
        }
        else
        {
            // Start next question with timer
            await StartQuestionWithTimer(session, session.CurrentQuestionIndex);
        }
    }

    public async Task SubmitAnswer(string sessionId, string userId, int qIndex, string choice)
    {
        Console.WriteLine($"[DuelHub] SubmitAnswer: Session={sessionId}, User={userId}, Q={qIndex}");
        var result = await _duelManager.SubmitAnswerAsync(sessionId, userId, qIndex, choice);
        var session = _duelManager.GetSession(sessionId);
        
        if (!result.Success || session == null)
        {
            Console.WriteLine($"[DuelHub] SubmitAnswer failed or session not found");
            return;
        }

        // Send immediate feedback to the caller
        await Clients.Caller.SendAsync("AnswerResult", result);

        // Create cache key for this round
        string cacheKey = $"{sessionId}_Q{qIndex}";

        // Store this player's result in cache (FULLY ATOMIC)
        lock (_roundAnswerCache)
        {
            if (!_roundAnswerCache.TryGetValue(cacheKey, out var roundCache))
            {
                roundCache = new Dictionary<string, DuelSubmissionResult>();
                _roundAnswerCache[cacheKey] = roundCache;
                Console.WriteLine($"[DuelHub] CACHE CREATE: Key={cacheKey}");
            }
            
            roundCache[userId] = result;
            Console.WriteLine($"[DuelHub] CACHE STORE: Key={cacheKey}, UserId={userId}, IsCorrect={result.IsCorrect}, AddedScore={result.AddedScore}, CacheSize={roundCache.Count}");
        }

        // REVIEW PHASE: When both players have answered
        if (result.BothAnswered)
        {
            // Cancel the timer since both answered
            session.QuestionTimerCts?.Cancel();

            Console.WriteLine($"[DuelHub] Both answered. Sending RoundResult, then waiting 3s...");
            
            // Determine player IDs
            bool isPlayer1 = session.Player1.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase);
            string player1Id = session.Player1.UserId;
            string player2Id = session.Player2.UserId;

            Console.WriteLine($"[DuelHub] Player IDs: P1={player1Id}, P2={player2Id}, CurrentSubmitter={userId}, IsP1={isPlayer1}");

            // Retrieve both players' results from cache (ATOMIC READ)
            DuelSubmissionResult? player1Result = null;
            DuelSubmissionResult? player2Result = null;

            lock (_roundAnswerCache)
            {
                if (_roundAnswerCache.TryGetValue(cacheKey, out var roundCache))
                {
                    Console.WriteLine($"[DuelHub] CACHE RETRIEVAL: CacheKey={cacheKey}, CacheSize={roundCache.Count}");
                    foreach (var kvp in roundCache)
                    {
                        Console.WriteLine($"[DuelHub] CACHE ENTRY: UserId={kvp.Key}, IsCorrect={kvp.Value.IsCorrect}, AddedScore={kvp.Value.AddedScore}");
                    }

                    roundCache.TryGetValue(player1Id, out player1Result);
                    roundCache.TryGetValue(player2Id, out player2Result);
                }
                else
                {
                    Console.WriteLine($"[DuelHub] ERROR: Cache key not found! Key={cacheKey}");
                }
            }

            Console.WriteLine($"[DuelHub] RETRIEVED P1Result: {(player1Result != null ? $"IsCorrect={player1Result.IsCorrect}, Score={player1Result.AddedScore}" : "NULL")}");
            Console.WriteLine($"[DuelHub] RETRIEVED P2Result: {(player2Result != null ? $"IsCorrect={player2Result.IsCorrect}, Score={player2Result.AddedScore}" : "NULL")}");

            // Fallback if cache lookup fails - use session's stored results
            if (player1Result == null || player2Result == null)
            {
                Console.WriteLine($"[DuelHub] WARNING: Incomplete cache! P1={player1Result != null}, P2={player2Result != null}");
                Console.WriteLine($"[DuelHub] Using session's stored round results as fallback");
                
                if (player1Result == null && session.Player1LastResult != null)
                {
                    Console.WriteLine($"[DuelHub] Using session data for P1: IsCorrect={session.Player1LastResult.IsCorrect}, Score={session.Player1LastResult.AddedScore}");
                    player1Result = session.Player1LastResult;
                }
                
                if (player2Result == null && session.Player2LastResult != null)
                {
                    Console.WriteLine($"[DuelHub] Using session data for P2: IsCorrect={session.Player2LastResult.IsCorrect}, Score={session.Player2LastResult.AddedScore}");
                    player2Result = session.Player2LastResult;
                }
            }

            // Build RoundResult for Player 1
            var p1RoundResult = new RoundResultEvent
            {
                IsCorrect = player1Result?.IsCorrect ?? false,
                CorrectAnswerId = result.CorrectAnswerId,
                CorrectPairIds = result.CorrectPairIds,
                MyTotalScore = session.Player1TotalScore,
                OpponentTotalScore = session.Player2TotalScore,
                PointsEarned = player1Result?.AddedScore ?? 0,
                IsRoundOver = true,
                IsDuelFinished = result.IsDuelFinished
            };
            
            var p2RoundResult = new RoundResultEvent
            {
                IsCorrect = player2Result?.IsCorrect ?? false,
                CorrectAnswerId = result.CorrectAnswerId,
                CorrectPairIds = result.CorrectPairIds,
                MyTotalScore = session.Player2TotalScore,
                OpponentTotalScore = session.Player1TotalScore,
                PointsEarned = player2Result?.AddedScore ?? 0,
                IsRoundOver = true,
                IsDuelFinished = result.IsDuelFinished
            };

            await Clients.Client(session.Player1.ConnectionId).SendAsync("RoundResult", p1RoundResult);
            await Clients.Client(session.Player2.ConnectionId).SendAsync("RoundResult", p2RoundResult);

            _roundAnswerCache.TryRemove(cacheKey, out _);

            await Task.Delay(3000);

            if (result.IsDuelFinished)
            {
                Console.WriteLine($"[DuelHub] Duel finished. Sending DuelFinished event.");
                var finishedEvent = new DuelFinishedEvent
                {
                    SessionId = session.SessionId,
                    Player1 = session.Player1,
                    Player2 = session.Player2,
                    Player1TotalScore = session.Player1TotalScore,
                    Player2TotalScore = session.Player2TotalScore,
                    WinnerId = session.Player1TotalScore > session.Player2TotalScore ? session.Player1.UserId : 
                               session.Player2TotalScore > session.Player1TotalScore ? session.Player2.UserId : null,
                    Status = DuelStatus.Finished
                };
                await Clients.Group(sessionId).SendAsync("DuelFinished", finishedEvent);
            }
            else
            {
                // Start next question with timer
                await StartQuestionWithTimer(session, session.CurrentQuestionIndex);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var session = await _duelManager.HandlePlayerDisconnectAsync(Context.ConnectionId);
        if (session != null)
        {
            // Cancel any pending timer
            session.QuestionTimerCts?.Cancel();
            
            // Determine winner (the player who didn't disconnect)
            string? winnerId = null;
            string winnerName = "";
            
            if (session.DisconnectedPlayerId == session.Player1.UserId)
            {
                winnerId = session.Player2.UserId;
                winnerName = session.Player2.UserName;
            }
            else if (session.DisconnectedPlayerId == session.Player2.UserId)
            {
                winnerId = session.Player1.UserId;
                winnerName = session.Player1.UserName;
            }
            
            Console.WriteLine($"[DuelHub] Player disconnected. Winner: {winnerName} ({winnerId})");
            
            var finishedEvent = new DuelFinishedEvent
            {
                SessionId = session.SessionId,
                Player1 = session.Player1,
                Player2 = session.Player2,
                Player1TotalScore = session.Player1TotalScore,
                Player2TotalScore = session.Player2TotalScore,
                WinnerId = winnerId,
                Status = DuelStatus.Finished
            };
            
            await Clients.Group(session.SessionId).SendAsync("DuelFinished", finishedEvent);
            await Clients.Group(session.SessionId).SendAsync("OpponentDisconnected", new { 
                WinnerId = winnerId, 
                WinnerName = winnerName,
                Message = "Ҳарифи шумо аз бозӣ баромад. Шумо ғолиб шудед!"
            });
        }
        await base.OnDisconnectedAsync(exception);
    }
}
