using System.Collections.Concurrent;
using Application.DTOs.Duel;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public class DuelManager(IServiceScopeFactory scopeFactory)
{
    private readonly ConcurrentDictionary<string, DuelSession> _sessions = new();
    private readonly ConcurrentDictionary<int, ConcurrentQueue<PlayerInfo>> _waitingQueues = new();
    private readonly object _lock = new();

    public async Task<DuelSession?> FindMatchAsync(string connectionId, string userId, string userName, string? profilePicture, int subjectId)
    {
        // Гирифтани навбат (queue) барои фанни мушаххас
        var queue = _waitingQueues.GetOrAdd(subjectId, _ => new ConcurrentQueue<PlayerInfo>());

        DuelSession? session = null;

        lock (_lock)
        {
            if (queue.Any(p => p.UserId == userId)) return null;

            if (queue.TryDequeue(out var opponent))
            {
                // Матч ёфт шуд! Сар кардани сессия.
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
                    Status = DuelStatus.Starting
                };
                
                _sessions[session.SessionId] = session;
                return session;
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
            // Саволҳоро аз базаи маълумот бор мекунем
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
                .OrderBy(_ => Guid.NewGuid()) // Safe for most providers including Postgres
                .Take(15)
                .ToListAsync();

            if (_sessions.TryGetValue(sessionId, out var session))
            {
                if (dbQuestions.Count == 0)
                {
                    Console.WriteLine($"[DuelManager] WARNING: No questions found for SubjectId {subjectId}!");
                    session.Questions = new List<DuelQuestionDto>();
                    session.QuestionsReady = true;
                    return;
                }

                session.Questions = dbQuestions.Select(q => 
                {
                    var dto = new DuelQuestionDto
                    {
                        Id = q.Id,
                        Content = q.Content,
                        SubjectName = q.Subject?.Name ?? "Unknown Subject",
                        Topic = q.Topic,
                        ImageUrl = q.ImageUrl,
                        Type = (int)q.Type,
                        Answers = q.Answers.Select(a => new DuelAnswerOptionDto
                        {
                            Id = a.Id,
                            Text = a.Text,
                            MatchPairText = a.MatchPairText 
                        }).ToList()
                    };

                    if (q.Type == Domain.Enums.QuestionType.Matching)
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
                session.QuestionsReady = true; // Mark as ready even on error to let the hub handle it (as 0 questions)
            }
        }
    }

    public DuelSession? GetSession(string sessionId) => _sessions.GetValueOrDefault(sessionId);

    public async Task<(bool success, int score, bool bothAnswered, bool isDuelFinished)> SubmitAnswerAsync(
        string sessionId, string userId, int qIndex, string answer)
    {
        if (!_sessions.TryGetValue(sessionId, out var session)) 
            return (false, 0, false, false);

        if (session.Questions.Count == 0 || session.CurrentQuestionIndex != qIndex || session.Status != DuelStatus.InProgress) 
            return (false, 0, false, false);

        var questionDto = session.Questions[qIndex];
        var isPlayer1 = session.Player1.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase);
        var isPlayer2 = session.Player2.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase);

        if (!isPlayer1 && !isPlayer2) return (false, 0, false, false);
        if (isPlayer1 && session.Player1Answered) return (false, 0, false, false);
        if (isPlayer2 && session.Player2Answered) return (false, 0, false, false);

        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var dbQuestion = await context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == questionDto.Id);

        if (dbQuestion == null) return (false, 0, false, false);

        bool isCorrect = false;
        if (dbQuestion.Type == Domain.Enums.QuestionType.Matching)
        {
            var userPairs = answer.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split(':'))
                .Where(parts => parts.Length == 2)
                .Select(parts => new { Id = parts[0].Trim(), Right = parts[1].Trim() })
                .ToList();

            var leftOptions = dbQuestion.Answers.Where(a => !string.IsNullOrEmpty(a.MatchPairText)).ToList();
            int correctCount = 0;

            foreach (var lo in leftOptions)
            {
                var up = userPairs.FirstOrDefault(p => p.Id == lo.Id.ToString());
                if (up != null && up.Right.Equals(lo.MatchPairText, StringComparison.OrdinalIgnoreCase))
                {
                    correctCount++;
                }
            }
            isCorrect = correctCount == leftOptions.Count;
        }
        else if (dbQuestion.Type == Domain.Enums.QuestionType.SingleChoice)
        {
            if (long.TryParse(answer, out long choiceId))
            {
                var correctOption = dbQuestion.Answers.FirstOrDefault(a => a.IsCorrect);
                isCorrect = correctOption != null && correctOption.Id == choiceId;
            }
        }
        else if (dbQuestion.Type == Domain.Enums.QuestionType.ClosedAnswer)
        {
            var correctOption = dbQuestion.Answers.FirstOrDefault(a => a.IsCorrect);
            isCorrect = correctOption != null && correctOption.Text.Trim().Equals(answer.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        var player = isPlayer1 ? session.Player1 : session.Player2;
        if (isCorrect)
        {
            player.Score += 10;
        }

        if (isPlayer1) session.Player1Answered = true;
        if (isPlayer2) session.Player2Answered = true;
        
        session.AnsweredCount++;
        int finalScore = player.Score;
        bool bothAnswered = false;
        bool isDuelFinished = false;

        if (session.AnsweredCount >= 2)
        {
            bothAnswered = true;
            session.Player1Answered = false;
            session.Player2Answered = false;
            session.AnsweredCount = 0;
            session.CurrentQuestionIndex++;

            if (session.CurrentQuestionIndex >= session.Questions.Count)
            {
                session.Status = DuelStatus.Finished;
                isDuelFinished = true;
                _ = ProcessGameEndAsync(session);
            }
        }

        return (true, finalScore, bothAnswered, isDuelFinished);
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

            int p1XP = p1.Score;
            int p2XP = p2.Score;

            if (p1.Score > p2.Score) p1XP += 50;
            else if (p2.Score > p1.Score) p2XP += 50;

            Console.WriteLine($"[DuelManager] Updating XP: {p1.UserName} ({p1.UserId}) +{p1XP}, {p2.UserName} ({p2.UserId}) +{p2XP}");

            await gamification.UpdateUserXpAsync(Guid.Parse(p1.UserId), p1XP);
            await gamification.UpdateUserXpAsync(Guid.Parse(p2.UserId), p2XP);
            
            if (p1.Score > p2.Score) 
                await gamification.ProcessDuelResultAsync(Guid.Parse(p1.UserId), Guid.Parse(p2.UserId));
            else if (p2.Score > p1.Score) 
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
}
