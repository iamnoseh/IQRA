using System.Collections.Concurrent;
using Application.DTOs.Duel;
using Application.DTOs.Testing;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Infrastructure.Services;

public class DuelManager(IServiceScopeFactory scopeFactory)
{
    private readonly ConcurrentDictionary<string, DuelSession> _sessions = new();
    private readonly ConcurrentDictionary<int, ConcurrentQueue<PlayerInfo>> _waitingQueues = new();
    private readonly Lock _lock = new();

    public async Task<DuelSession?> FindMatchAsync(string connectionId, string userId, string userName, string? profilePicture, int subjectId)
    {
        var queue = _waitingQueues.GetOrAdd(subjectId, _ => new ConcurrentQueue<PlayerInfo>());

        DuelSession? session = null;

        lock (_lock)
        {
            if (queue.Any(p => p.UserId == userId)) return null;

            if (queue.TryDequeue(out var opponent))
            {
                session = new DuelSession
                {
                    Player1 = opponent,
                    Player2 = new PlayerInfo 
                    { 
                        ConnectionId = connectionId, 
                        UserId = userId, 
                        UserName = userName,
                        ProfilePicture = profilePicture
                    },
                    Status = DuelStatus.Starting,
                    MatchFoundAt = DateTime.UtcNow // Added for timeout tracking
                };
                
                _sessions[session.SessionId] = session;
            
            }

            var player = new PlayerInfo 
            { 
                ConnectionId = connectionId, 
                UserId = userId, 
                UserName = userName,
                ProfilePicture = profilePicture
            };
            queue.Enqueue(player);
        }

        if (session != null)
        {
            await StartSessionAsync(session.SessionId, subjectId);
        }

        return session;
    }

    private async Task StartSessionAsync(string sessionId, int subjectId)
    {
        try
        {
            Console.WriteLine($"[DuelManager] StartSessionAsync: Fetching questions for SubjectId={subjectId}");
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var dbQuestions = await context.Questions
                .AsNoTracking()
                .Include(q => q.Answers)
                .Include(q => q.Subject)
                .Where(q => q.SubjectId == subjectId)
                .OrderBy(_ => Guid.NewGuid()) 
                .Take(15)
                .ToListAsync();

            if (_sessions.TryGetValue(sessionId, out var session))
            {
                if (dbQuestions.Count == 0)
                {
                    Console.WriteLine($"[DuelManager] WARNING: No questions found for SubjectId {subjectId}!");
                    session.Questions = new List<QuestionWithAnswersDto>();
                    session.QuestionsReady = true;
                    return;
                }

                session.Questions = dbQuestions.Select(q => 
                {
                    var dto = new QuestionWithAnswersDto
                    {
                        Id = q.Id,
                        Content = q.Content,
                        SubjectName = q.Subject.Name,
                        Topic = q.Topic,
                        ImageUrl = q.ImageUrl,
                        Type = q.Type,
                        Difficulty = q.Difficulty,
                        SubjectId = q.SubjectId,
                        IsInRedList = false,
                        RedListCorrectCount = 0,
                        Answers = q.Answers.Select(a => new AnswerOptionDto
                        {
                            Id = a.Id,
                            Text = a.Text,
                            MatchPairText = null
                        }).ToList()
                    };

                    if (q.Type == QuestionType.Matching)
                    {
                        dto.MatchOptions = q.Answers
                            .Where(a => !string.IsNullOrWhiteSpace(a.MatchPairText))
                            .Select(a => a.MatchPairText!)
                            .OrderBy(_ => Guid.NewGuid())
                            .ToList();
                        dto.Answers = dto.Answers.OrderBy(_ => Guid.NewGuid()).ToList();
                    }
                    else
                    {
                        dto.Answers = dto.Answers.OrderBy(_ => Guid.NewGuid()).ToList();
                    }

                    return dto;
                }).ToList();
                
                session.QuestionsReady = true;
                Console.WriteLine($"[DuelManager] Questions ready for session {sessionId}. Count={session.Questions.Count}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DuelManager] ERROR in StartSessionAsync: {ex.Message}");
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.QuestionsReady = true; 
            }
        }
    }

    public DuelSession? GetSession(string sessionId) => _sessions.GetValueOrDefault(sessionId);

    public async Task<DuelSubmissionResult> SubmitAnswerAsync(
        string sessionId, string userId, int qIndex, string answer)
    {
        var result = new DuelSubmissionResult();
        if (!_sessions.TryGetValue(sessionId, out var session)) 
            return result;

        if (session.Questions.Count == 0 || session.CurrentQuestionIndex != qIndex || session.Status != DuelStatus.InProgress) 
            return result;

        var questionDto = session.Questions[qIndex];
        var isPlayer1 = session.Player1.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase);
        var isPlayer2 = session.Player2.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase);

        if (!isPlayer1 && !isPlayer2) return result;
        if (isPlayer1 && session.Player1Answered) return result;
        if (isPlayer2 && session.Player2Answered) return result;

        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var dbQuestion = await context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == questionDto.Id);

        if (dbQuestion == null) return result;

        bool isCorrect = false;
        int playerScoreToAdd = 0;
        if (dbQuestion.Type == QuestionType.Matching)
        {
            var userPairs = new List<(string Id, string Right)>();

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                // Deserialize to a flexible object first to handle potential number/string issues if needed, 
                // but explicit class with string properties is better if we trust the input is string-coercible.
                // Using Dictionary<string,object> + ToString() is safest for "Right" value being number or string.
                
                var parsed = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(answer, options);
                
                Console.WriteLine($"[DuelManager] JSON Parsed. Count: {parsed?.Count ?? 0}");

                if (parsed != null)
                {
                    foreach (var item in parsed)
                    {
                        string? id = null;
                        string? right = null;

                        if (item.TryGetValue("id", out var idObj)) id = idObj.ToString();
                        if (item.TryGetValue("right", out var rightObj)) right = rightObj.ToString();

                        if (!string.IsNullOrWhiteSpace(id) && right != null)
                        {
                            userPairs.Add((id, right));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DuelManager] JSON Parse Error: {ex.Message}. Fallback to legacy split.");
                // Fallback to legacy "LeftId:RightText,..." format
                // WARNING: If the input WAS json but failed, this split will produce garbage. 
                // We should only fallback if it doesn't look like JSON (doesn't start with [).
                
                if (!answer.Trim().StartsWith("[")) 
                {
                    userPairs = answer.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Split(':'))
                        .Where(parts => parts.Length >= 2) 
                        .Select(parts => (parts[0].Trim(), string.Join(":", parts.Skip(1)).Trim())) 
                        .ToList();
                }
            }

            var leftOptions = dbQuestion.Answers.Where(a => !string.IsNullOrEmpty(a.MatchPairText)).ToList();
            int correctCount = 0;

            foreach (var lo in leftOptions)
            {
                var leftIdStr = lo.Id.ToString();
                var leftTextLower = lo.Text.Trim().ToLowerInvariant();
                var correctRightLower = lo.MatchPairText!.Trim().ToLowerInvariant();

                // Find user pair where Key matches ID OR Text
                var up = userPairs.FirstOrDefault(p => 
                    p.Id.Equals(leftIdStr, StringComparison.OrdinalIgnoreCase) || 
                    p.Id.Equals(leftTextLower, StringComparison.OrdinalIgnoreCase) ||
                    p.Id.Trim().ToLowerInvariant() == leftTextLower // robustness check
                );

                if (up.Id != null) // Found a match attempt
                {
                    // Check if value matches
                    if (up.Right.Trim().ToLowerInvariant() == correctRightLower)
                    {
                        correctCount++;
                        result.CorrectPairIds.Add(lo.Id.ToString()); 
                    }
                }
            }
            
            if (leftOptions.Count > 0)
            {
                var score = (double)correctCount / leftOptions.Count * 10.0;
                playerScoreToAdd = (int)Math.Round(score, MidpointRounding.AwayFromZero);
            }
            isCorrect = correctCount == leftOptions.Count; 
        }
        else if (dbQuestion.Type == QuestionType.SingleChoice)
        {
            if (long.TryParse(answer, out long choiceId))
            {
                var correctOption = dbQuestion.Answers.FirstOrDefault(a => a.IsCorrect);
                if (correctOption != null) result.CorrectAnswerId = correctOption.Id; // Send correct answer ID
                
                isCorrect = correctOption != null && correctOption.Id == choiceId;
                if (isCorrect) playerScoreToAdd = 10;
            }
        }
        else if (dbQuestion.Type == QuestionType.ClosedAnswer)
        {
            var correctOption = dbQuestion.Answers.FirstOrDefault(a => a.IsCorrect);
             // For closed answer, maybe send text back? simplified for now.
             
            isCorrect = correctOption != null && correctOption.Text.Trim().Equals(answer.Trim(), StringComparison.OrdinalIgnoreCase);
            if (isCorrect) playerScoreToAdd = 10;
        }

        // FIXED: Update session-level scores instead of player.Score
        if (isPlayer1)
        {
            session.Player1TotalScore += playerScoreToAdd;
        }
        else
        {
            session.Player2TotalScore += playerScoreToAdd;
        }

        Console.WriteLine($"[DuelManager] ANSWER PROCESSED: UserId={userId}, IsP1={isPlayer1}, IsCorrect={isCorrect}, PointsAdded={playerScoreToAdd}, NewTotalScore={(isPlayer1 ? session.Player1TotalScore : session.Player2TotalScore)}");

        if (isPlayer1) session.Player1Answered = true;
        if (isPlayer2) session.Player2Answered = true;
        
        session.AnsweredCount++;
        int currentPlayerScore = isPlayer1 ? session.Player1TotalScore : session.Player2TotalScore;
        int opponentScore = isPlayer1 ? session.Player2TotalScore : session.Player1TotalScore;
        bool bothAnswered = false;
        bool isDuelFinished = false;

        if (session.AnsweredCount >= 2)
        {
            result.BothAnswered = true;
            session.Player1Answered = false;
            session.Player2Answered = false;
            session.AnsweredCount = 0;
            session.CurrentQuestionIndex++;

            if (session.CurrentQuestionIndex >= session.Questions.Count)
            {
                session.Status = DuelStatus.Finished;
                result.IsDuelFinished = true;
                _ = ProcessGameEndAsync(session);
            }
        }

        result.Success = true;
        result.CurrentScore = currentPlayerScore;
        result.OpponentScore = opponentScore; // Added for RoundResult event
        result.AddedScore = playerScoreToAdd;
        result.IsCorrect = isCorrect;
        
        Console.WriteLine($"[DuelManager] RESULT CREATED: IsCorrect={result.IsCorrect}, AddedScore={result.AddedScore}, CurrentScore={result.CurrentScore}, BothAnswered={result.BothAnswered}");
        
        return result;
    }

    private async Task ProcessGameEndAsync(DuelSession session)
    {
        try
        {
            Console.WriteLine($"[DuelManager] ProcessGameEndAsync: Session={session.SessionId}");
            using var scope = scopeFactory.CreateScope();
            var gamification = scope.ServiceProvider.GetRequiredService<IGamificationService>();

            var p1 = session.Player1;
            var p2 = session.Player2;

            // Use session-level scores
            int p1Xp = session.Player1TotalScore;
            int p2Xp = session.Player2TotalScore;

            if (session.Player1TotalScore > session.Player2TotalScore) p1Xp += 50;
            else if (session.Player2TotalScore > session.Player1TotalScore) p2Xp += 50;

            Console.WriteLine($"[DuelManager] Updating XP: {p1.UserName} ({p1.UserId}) +{p1Xp}, {p2.UserName} ({p2.UserId}) +{p2Xp}");

            await gamification.UpdateUserXpAsync(Guid.Parse(p1.UserId), p1Xp);
            await gamification.UpdateUserXpAsync(Guid.Parse(p2.UserId), p2Xp);
            
            if (session.Player1TotalScore > session.Player2TotalScore) 
                await gamification.ProcessDuelResultAsync(Guid.Parse(p1.UserId), Guid.Parse(p2.UserId));
            else if (session.Player2TotalScore > session.Player1TotalScore) 
                await gamification.ProcessDuelResultAsync(Guid.Parse(p2.UserId), Guid.Parse(p1.UserId));
            
            Console.WriteLine("[DuelManager] ProcessGameEndAsync completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DuelManager] ERROR in ProcessGameEndAsync: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    public void RemoveSession(string sessionId) => _sessions.TryRemove(sessionId, out _);

    public async Task<DuelSession?> HandlePlayerDisconnectAsync(string connectionId)
    {
        // Find session where this player is involved
        var session = _sessions.Values.FirstOrDefault(s => 
            s.Player1.ConnectionId == connectionId || s.Player2.ConnectionId == connectionId);

        if (session != null)
        {
            // If already finished, minimal action
            if (session.Status == DuelStatus.Finished) return null;

            Console.WriteLine($"[DuelManager] Player disconnected: {connectionId}. Ending session {session.SessionId}");
            
            session.Status = DuelStatus.Finished;
            
            // Allow the *other* player to claim win or just finalize
            await ProcessGameEndAsync(session);
            
            return session;
        }
        return null;
    }
}
