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

        if (session.MatchFoundAt.HasValue && session.Status == DuelStatus.Starting)
        {
            var elapsed = DateTime.UtcNow - session.MatchFoundAt.Value;
            if (elapsed.TotalSeconds > 10)
            {
                Console.WriteLine($"[DuelHub] Session {sessionId} initial connection timed out (elapsed: {elapsed.TotalSeconds}s)");
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
        
        var questionEvent = new QuestionStartEvent
        {
            Question = session.Questions[questionIndex],
            QuestionIndex = questionIndex,
            TimeLimitSeconds = 30
        };
        await Clients.Group(session.SessionId).SendAsync("QuestionStart", questionEvent);
        
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(30000, cts.Token);
                
                if (cts.Token.IsCancellationRequested)
                {
                    Console.WriteLine($"[DuelHub] Timer cancelled for session {session.SessionId}, Q{questionIndex}");
                    return;
                }
                
                if (session.Player1Answered && session.Player2Answered)
                {
                    Console.WriteLine($"[DuelHub] Timer expired but both already answered for session {session.SessionId}, Q{questionIndex}");
                    return;
                }
                
                Console.WriteLine($"[DuelHub] TIMEOUT: 30 seconds passed for session {session.SessionId}, Q{questionIndex}");
                
                await HandleQuestionTimeout(session, questionIndex);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"[DuelHub] Timer cancelled for session {session.SessionId}, Q{questionIndex}");
            }
        });
    }

    private async Task HandleQuestionTimeout(DuelSession session, int questionIndex)
    {
        if (session.Status != DuelStatus.InProgress || session.CurrentQuestionIndex != questionIndex)
        {
            Console.WriteLine($"[DuelHub] Timeout ignored: Status={session.Status}, CurrentQ={session.CurrentQuestionIndex}, TimeoutQ={questionIndex}");
            return;
        }

        bool p1TimedOut = !session.Player1Answered;
        bool p2TimedOut = !session.Player2Answered;

        var elapsed = DateTime.UtcNow - (session.CurrentQuestionStartedAt ?? DateTime.UtcNow);
        Console.WriteLine($"[DuelHub] Timeout Check: Start={session.CurrentQuestionStartedAt:HH:mm:ss.fff}, Now={DateTime.UtcNow:HH:mm:ss.fff}, Elapsed={elapsed.TotalSeconds:F1}s, P1TimedOut={p1TimedOut}, P2TimedOut={p2TimedOut}");

        if (p1TimedOut && p2TimedOut)
        {
            Console.WriteLine($"[DuelHub] DOUBLE TIMEOUT: Both players failed to answer Q{questionIndex}. Ending duel as draw.");
            
            var result1 = _duelManager.ForceTimeoutForPlayer(session.SessionId, session.Player1.UserId, questionIndex);
            var result2 = _duelManager.ForceTimeoutForPlayer(session.SessionId, session.Player2.UserId, questionIndex);
            
            await _hubContext.Clients.Client(session.Player1.ConnectionId).SendAsync("AnswerResult", result1);
            await _hubContext.Clients.Client(session.Player2.ConnectionId).SendAsync("AnswerResult", result2);

            var updatedSession = _duelManager.GetSession(session.SessionId);
            if (updatedSession != null)
            {
                await ProcessRoundEnd(updatedSession, questionIndex);
            }
        }
        else if (p1TimedOut || p2TimedOut)
        {
            if (p1TimedOut)
            {
                var result = _duelManager.ForceTimeoutForPlayer(session.SessionId, session.Player1.UserId, questionIndex);
                await _hubContext.Clients.Client(session.Player1.ConnectionId).SendAsync("AnswerResult", result);
            }
            if (p2TimedOut)
            {
                var result = _duelManager.ForceTimeoutForPlayer(session.SessionId, session.Player2.UserId, questionIndex);
                await _hubContext.Clients.Client(session.Player2.ConnectionId).SendAsync("AnswerResult", result);
            }

            var updatedSession = _duelManager.GetSession(session.SessionId);
            if (updatedSession != null)
            {
                if (updatedSession.Status == DuelStatus.Finished)
                {
                    await SendDuelFinished(updatedSession);
                }
                else if (updatedSession.AnsweredCount >= 2)
                {
                    await ProcessRoundEnd(updatedSession, questionIndex);
                }
            }
        }
    }

    private async Task ProcessRoundEnd(DuelSession session, int questionIndex)
    {

        var player1Result = session.Player1LastResult ?? new DuelSubmissionResult { Success = true, IsCorrect = false, AddedScore = 0 };
        var player2Result = session.Player2LastResult ?? new DuelSubmissionResult { Success = true, IsCorrect = false, AddedScore = 0 };

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

        session.Player1Answered = false;
        session.Player2Answered = false;
        session.AnsweredCount = 0;
        session.CurrentQuestionIndex++;

        await Task.Delay(3000);

        if (session.CurrentQuestionIndex >= session.Questions.Count || session.Status == DuelStatus.Finished)
        {
            await SendDuelFinished(session);
        }
        else
        {
            await StartQuestionWithTimer(session, session.CurrentQuestionIndex);
        }
    }

    private async Task SendDuelFinished(DuelSession session)
    {
        session.Status = DuelStatus.Finished;
        Console.WriteLine($"[DuelHub] Sending DuelFinished event for session {session.SessionId}");


        string? winnerId = null;
        if (!string.IsNullOrEmpty(session.TimedOutPlayerId))
        {
            winnerId = session.TimedOutPlayerId == session.Player1.UserId ? session.Player2.UserId : session.Player1.UserId;
        }
        else if (!string.IsNullOrEmpty(session.DisconnectedPlayerId))
        {
            winnerId = session.DisconnectedPlayerId == session.Player1.UserId ? session.Player2.UserId : session.Player1.UserId;
        }
        else if (session.Player1TotalScore > session.Player2TotalScore)
        {
            winnerId = session.Player1.UserId;
        }
        else if (session.Player2TotalScore > session.Player1TotalScore)
        {
            winnerId = session.Player2.UserId;
        }

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
        
        await _hubContext.Clients.Group(session.SessionId).SendAsync("DuelFinished", finishedEvent);
        
        if (!string.IsNullOrEmpty(session.TimedOutPlayerId))
        {
            string winnerName = session.TimedOutPlayerId == session.Player1.UserId ? session.Player2.UserName : session.Player1.UserName;
            await _hubContext.Clients.Group(session.SessionId).SendAsync("DuelError", $"Бозӣ тамом шуд! Яке аз бозигарон дар вақташ ҷавоб надод. Ғолиб: {winnerName}");
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

        await Clients.Caller.SendAsync("AnswerResult", result);

        string cacheKey = $"{sessionId}_Q{qIndex}";

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

        if (result.BothAnswered)
        {
            session.QuestionTimerCts?.Cancel();

            Console.WriteLine($"[DuelHub] Both answered. Sending RoundResult, then waiting 3s...");
            
            
            bool isPlayer1 = session.Player1.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase);
            string player1Id = session.Player1.UserId;
            string player2Id = session.Player2.UserId;

            Console.WriteLine($"[DuelHub] Player IDs: P1={player1Id}, P2={player2Id}, CurrentSubmitter={userId}, IsP1={isPlayer1}");

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
                await SendDuelFinished(session);
            }
            else
            {
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
            
            await SendDuelFinished(session);

            // Notify about the disconnection specifically
            string? winnerId = session.DisconnectedPlayerId == session.Player1.UserId ? session.Player2.UserId : session.Player1.UserId;
            string winnerName = session.DisconnectedPlayerId == session.Player1.UserId ? session.Player2.UserName : session.Player1.UserName;

            await Clients.Group(session.SessionId).SendAsync("OpponentDisconnected", new { 
                WinnerId = winnerId, 
                WinnerName = winnerName,
                Message = "Ҳарифи шумо аз бозӣ баромад. Шумо ғолиб шудед!"
            });
        }
        await base.OnDisconnectedAsync(exception);
    }
}
