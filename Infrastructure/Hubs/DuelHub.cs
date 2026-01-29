using Microsoft.AspNetCore.SignalR;
using Infrastructure.Services;
using Application.DTOs.Duel;
using Application.DTOs.Testing;
using Domain.Enums;
using System.Collections.Concurrent;

namespace Infrastructure.Hubs;

public class DuelHub(DuelManager duelManager) : Hub
{
    public async Task FindMatch(string userId, string userName, string? profilePicture, int subjectId)
    {
        var session = await duelManager.FindMatchAsync(Context.ConnectionId, userId, userName, profilePicture, subjectId);
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
        var session = duelManager.GetSession(sessionId);
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
                duelManager.RemoveSession(sessionId);
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
            // Send QuestionStartEvent with wrapped DTO
            var questionEvent = new QuestionStartEvent
            {
                Question = session.Questions[0],
                QuestionIndex = 0,
                TimeLimitSeconds = 30
            };
            await Clients.Group(sessionId).SendAsync("QuestionStart", questionEvent);
        }
    }

    // Cache for storing individual player results before sending RoundResult
    private readonly ConcurrentDictionary<string, Dictionary<string, DuelSubmissionResult>> _answerCache = new();

    public async Task SubmitAnswer(string sessionId, string userId, int qIndex, string choice)
    {
        Console.WriteLine($"[DuelHub] SubmitAnswer: Session={sessionId}, User={userId}, Q={qIndex}");
        var result = await duelManager.SubmitAnswerAsync(sessionId, userId, qIndex, choice);
        var session = duelManager.GetSession(sessionId);
        
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
        // CRITICAL: Manual dictionary check to avoid GetOrAdd race condition
        lock (_answerCache)
        {
            // Check if dictionary exists, create if not
            if (!_answerCache.TryGetValue(cacheKey, out var roundCache))
            {
                roundCache = new Dictionary<string, DuelSubmissionResult>();
                _answerCache[cacheKey] = roundCache;
                Console.WriteLine($"[DuelHub] CACHE CREATE: Key={cacheKey}");
            }
            
            roundCache[userId] = result;
            Console.WriteLine($"[DuelHub] CACHE STORE: Key={cacheKey}, UserId={userId}, IsCorrect={result.IsCorrect}, AddedScore={result.AddedScore}, CacheSize={roundCache.Count}");
        }

        // REVIEW PHASE: When both players have answered
        if (result.BothAnswered)
        {
            Console.WriteLine($"[DuelHub] Both answered. Sending RoundResult, then waiting 3s...");
            
            // Determine player IDs
            bool isPlayer1 = session.Player1.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase);
            string player1Id = session.Player1.UserId;
            string player2Id = session.Player2.UserId;

            Console.WriteLine($"[DuelHub] Player IDs: P1={player1Id}, P2={player2Id}, CurrentSubmitter={userId}, IsP1={isPlayer1}");

            // Retrieve both players' results from cache (ATOMIC READ)
            DuelSubmissionResult? player1Result = null;
            DuelSubmissionResult? player2Result = null;

            lock (_answerCache)
            {
                if (_answerCache.TryGetValue(cacheKey, out var roundCache))
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

            // Fallback if cache lookup fails
            if (player1Result == null || player2Result == null)
            {
                Console.WriteLine($"[DuelHub] WARNING: Incomplete cache! P1={player1Result != null}, P2={player2Result != null}");
                Console.WriteLine($"[DuelHub] This indicates Player {(player1Result == null ? "1" : "2")} is using old frontend code or experienced network issues");
                
                // CRITICAL: We cannot send accurate RoundResult without both results
                // The current `result` only contains the LAST player's answer
                // We need to use DuelSession scores as source of truth instead
                
                if (player1Result == null)
                {
                    // Player 1's result is missing - infer from session scores
                    Console.WriteLine($"[DuelHub] Inferring P1 result from session: P1Score={session.Player1TotalScore}");
                    player1Result = new DuelSubmissionResult
                    {
                        Success = true,
                        IsCorrect = session.Player1TotalScore > 0, // Rough inference
                        AddedScore = session.Player1TotalScore,
                        CurrentScore = session.Player1TotalScore,
                        OpponentScore = session.Player2TotalScore
                    };
                }
                
                if (player2Result == null)
                {
                    // Player 2's result is missing - infer from session scores
                    Console.WriteLine($"[DuelHub] Inferring P2 result from session: P2Score={session.Player2TotalScore}");
                    player2Result = new DuelSubmissionResult
                    {
                        Success = true,
                        IsCorrect = session.Player2TotalScore > 0, // Rough inference
                        AddedScore = session.Player2TotalScore,
                        CurrentScore = session.Player2TotalScore,
                        OpponentScore = session.Player1TotalScore
                    };
                }
            }

            // Build RoundResult for Player 1
            var p1RoundResult = new RoundResultEvent
            {
                IsCorrect = player1Result.IsCorrect,
                CorrectAnswerId = result.CorrectAnswerId,
                CorrectPairIds = result.CorrectPairIds,
                MyTotalScore = session.Player1TotalScore,
                OpponentTotalScore = session.Player2TotalScore,
                PointsEarned = player1Result.AddedScore,
                IsRoundOver = true,
                IsDuelFinished = result.IsDuelFinished
            };
            
            var p2RoundResult = new RoundResultEvent
            {
                IsCorrect = player2Result.IsCorrect,
                CorrectAnswerId = result.CorrectAnswerId,
                CorrectPairIds = result.CorrectPairIds,
                MyTotalScore = session.Player2TotalScore,
                OpponentTotalScore = session.Player1TotalScore,
                PointsEarned = player2Result.AddedScore,
                IsRoundOver = true,
                IsDuelFinished = result.IsDuelFinished
            };

            await Clients.Client(session.Player1.ConnectionId).SendAsync("RoundResult", p1RoundResult);
            await Clients.Client(session.Player2.ConnectionId).SendAsync("RoundResult", p2RoundResult);

            _answerCache.TryRemove(cacheKey, out _);

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
                Console.WriteLine($"[DuelHub] Sending next question: Index {session.CurrentQuestionIndex}");
                var nextQuestionEvent = new QuestionStartEvent
                {
                    Question = session.Questions[session.CurrentQuestionIndex],
                    QuestionIndex = session.CurrentQuestionIndex,
                    TimeLimitSeconds = 30
                };
                await Clients.Group(sessionId).SendAsync("QuestionStart", nextQuestionEvent);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var session = await duelManager.HandlePlayerDisconnectAsync(Context.ConnectionId);
        if (session != null)
        {
            await Clients.Group(session.SessionId).SendAsync("DuelFinished", session);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
