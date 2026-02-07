using Application.Constants;
using Application.DTOs.Testing;
using Application.Interfaces;
using Application.Responses;
using Domain.Entities.Testing;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Infrastructure.Services;

public class TestService(
    ApplicationDbContext context, 
    IQuestionService questionService, 
    IAiService aiService, 
    IRedListService redListService,
    IScoringService scoringService,
    IGamificationService gamificationService) : ITestService
{
    public async Task<Response<Guid>> StartTestAsync(Guid userId, StartTestRequest request)
    {
        var questionIds = new List<long>();
        int? templateId = null;

        if (request.Mode == TestMode.SubjectTest)
        {
            if (request.SubjectId == null)
                return new Response<Guid>(HttpStatusCode.BadRequest, "Фан интихоб нашудааст");

            var questions = await questionService.GetTestQuestionsAsync(userId, request.SubjectId.Value, 15);
            questionIds.AddRange(questions.Select(q => q.Id));
        }
        else
        {
            if (!request.ClusterId.HasValue)
                return new Response<Guid>(HttpStatusCode.BadRequest, "Кластер интихоб нашудааст");

            var template = await context.TestTemplates
                .FirstOrDefaultAsync(t => t.ClusterId == request.ClusterId && t.ComponentType == request.ComponentType);
            
            if (template == null)
                return new Response<Guid>(HttpStatusCode.BadRequest, "Template барои ин кластер ё қисм ёфт нашуд");

            templateId = template.Id;

            var clusterSubjects = await context.ClusterSubjects
                .Where(cs => cs.ClusterId == request.ClusterId && cs.ComponentType == request.ComponentType)
                .OrderBy(cs => cs.DisplayOrder)
                .ToListAsync();
            
            foreach (var cs in clusterSubjects)
            {
                var singleChoice = await questionService.GetTestQuestionsAsync(userId, cs.SubjectId, template.SingleChoiceCount, QuestionType.SingleChoice);
                var closedAnswer = await questionService.GetTestQuestionsAsync(userId, cs.SubjectId, template.ClosedAnswerCount, QuestionType.ClosedAnswer);
                var matching = await questionService.GetTestQuestionsAsync(userId, cs.SubjectId, template.MatchingCount, QuestionType.Matching);
                
                questionIds.AddRange(singleChoice.Select(q => q.Id));
                questionIds.AddRange(closedAnswer.Select(q => q.Id));
                questionIds.AddRange(matching.Select(q => q.Id));
            }
        }

        if (questionIds.Count == 0)
            return new Response<Guid>(HttpStatusCode.BadRequest, "Саволҳо ёфт нашуданд");

        var session = new TestSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Mode = request.Mode,
            ClusterId = request.ClusterId,
            ComponentType = request.ComponentType,
            SubjectId = request.SubjectId,
            TestTemplateId = templateId,
            QuestionIdsJson = JsonSerializer.Serialize(questionIds),
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        context.TestSessions.Add(session);
        await context.SaveChangesAsync();

        return new Response<Guid>(session.Id);
    }

    public async Task<Response<List<QuestionWithAnswersDto>>> GetTestQuestionsAsync(Guid testSessionId)
    {
        var session = await context.TestSessions.FindAsync(testSessionId);
        if (session == null)
            return new Response<List<QuestionWithAnswersDto>>(HttpStatusCode.NotFound, "Тест ёфт нашуд");

        var questionIds = JsonSerializer.Deserialize<List<long>>(session.QuestionIdsJson);
        
        var questions = await context.Questions
            .Include(q => q.Subject)
            .Include(q => q.Answers)
            .Where(q => questionIds!.Contains(q.Id))
            .Select(q => new QuestionWithAnswersDto
            {
                Id = q.Id,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                Topic = q.Topic,
                Difficulty = q.Difficulty,
                Type = q.Type,
                Answers = q.Answers.Select(a => new AnswerOptionDto 
                { 
                    Id = a.Id, 
                    Text = a.Text,
                    MatchPairText = a.MatchPairText
                }).ToList()
            })
            .ToListAsync();

        var userRedList = await context.RedListQuestions
            .Where(rl => rl.UserId == session.UserId && questionIds.Contains(rl.QuestionId))
            .ToDictionaryAsync(rl => rl.QuestionId, rl => rl.ConsecutiveCorrectCount);

        foreach (var question in questions)
        {
            if (userRedList.TryGetValue(question.Id, out int count))
            {
                question.IsInRedList = true;
                question.RedListCorrectCount = count;
            }

            if (question.Type == QuestionType.ClosedAnswer)
            {
                question.Answers = new List<AnswerOptionDto>();
            }
            else if (question.Type == QuestionType.Matching)
            {
                var pool = question.Answers
                    .Where(a => !string.IsNullOrWhiteSpace(a.MatchPairText))
                    .Select(a => a.MatchPairText!)
                    .OrderBy(_ => Guid.NewGuid())
                    .ToList();
                
                question.MatchOptions = pool;

                foreach (var answer in question.Answers)
                {
                    answer.MatchPairText = null;
                }
                
                question.Answers = question.Answers.OrderBy(_ => Guid.NewGuid()).ToList();
            }
            else
            {
                question.Answers = question.Answers.OrderBy(_ => Guid.NewGuid()).ToList();
            }
        }

        return new Response<List<QuestionWithAnswersDto>>(questions);
    }

    public async Task<Response<AnswerFeedbackDto>> SubmitAnswerAsync(Guid userId, SubmitAnswerRequest request)
    {
        var session = await context.TestSessions.FindAsync(request.TestSessionId);
        if (session == null || session.UserId != userId)
            return new Response<AnswerFeedbackDto>(HttpStatusCode.NotFound, "Тест ёфт нашуд");

        if (session.IsCompleted)
            return new Response<AnswerFeedbackDto>(HttpStatusCode.BadRequest, "Тест аллакай тамом шудааст");

        var existingAnswer = await context.UserAnswers
            .FirstOrDefaultAsync(ua => ua.TestSessionId == request.TestSessionId 
                && ua.QuestionId == request.QuestionId);

        if (existingAnswer != null)
            return new Response<AnswerFeedbackDto>(HttpStatusCode.BadRequest, "Шумо аллакай ба ин савол ҷавоб додаед");

        var question = await context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId);

        if (question == null)
            return new Response<AnswerFeedbackDto>(HttpStatusCode.NotFound, "Савол ёфт нашуд");

        bool isCorrect = false;
        long? correctAnswerId = null;
        string? correctAnswerText = null;
        string? chosenAnswerText = null;
        
        int? matchingScore = null;
        int? matchingCorrectCount = null;
        int? matchingTotalCount = null;

        string? matchingDebugInfo = null;

        var feedback = new AnswerFeedbackDto();

        if (question.Type == QuestionType.SingleChoice)
        {
            var correctOption = question.Answers.FirstOrDefault(a => a.IsCorrect);
            correctAnswerId = correctOption?.Id;
            correctAnswerText = correctOption?.Text;
            isCorrect = correctAnswerId == request.ChosenAnswerId;
            chosenAnswerText = question.Answers.FirstOrDefault(a => a.Id == request.ChosenAnswerId)?.Text;
        }
        else if (question.Type == QuestionType.ClosedAnswer)
        {
            var correctOption = question.Answers.FirstOrDefault(a => a.IsCorrect);
            correctAnswerText = correctOption?.Text;
            isCorrect = correctOption?.Text.Trim().Equals(request.TextResponse?.Trim(), StringComparison.OrdinalIgnoreCase) ?? false;
            chosenAnswerText = request.TextResponse;
        }
        else if (question.Type == QuestionType.Matching)
        {
            var userResponse = request.TextResponse ?? "";
            
            Console.WriteLine($"[Matching] User Response: {userResponse}");

            var userPairs = userResponse
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(p => p.Split(':'))
                .Where(parts => parts.Length == 2)
                .Select(parts => new { Left = parts[0].Trim().ToLowerInvariant(), Right = parts[1].Trim().ToLowerInvariant() })
                .ToList();

            var validationPairs = new List<PairValidationDto>();
            int correctPairsCount = 0;
            var leftOptions = question.Answers.Where(a => !string.IsNullOrWhiteSpace(a.MatchPairText)).ToList();

            foreach (var leftOption in leftOptions)
            {
                var leftIdStr = leftOption.Id.ToString();
                var leftTextLower = leftOption.Text.Trim().ToLowerInvariant();
                var correctRightLower = leftOption.MatchPairText!.Trim().ToLowerInvariant();

                var userMatch = userPairs.FirstOrDefault(up => 
                    up.Left == leftIdStr || 
                    up.Left == leftTextLower);
                
                bool isPairCorrect = userMatch != null && 
                                   userMatch.Right == correctRightLower;
                
                if (isPairCorrect) correctPairsCount++;

                Console.WriteLine($"[Matching] Left: {leftOption.Text} ({leftOption.Id}), Correct: {leftOption.MatchPairText}, User: {userMatch?.Right ?? "N/A"}, Result: {isPairCorrect}");

                validationPairs.Add(new PairValidationDto
                {
                    LeftSide = leftOption.Text,
                    RightSide = userMatch?.Right ?? string.Empty,
                    CorrectRightSide = leftOption.MatchPairText,
                    IsCorrect = isPairCorrect
                });
            }

            int totalPairsCount = leftOptions.Count;
            int score = Math.Min(correctPairsCount * ScoringConstants.ScoreMatchingPair, ScoringConstants.ScoreMatchingMax);
            
            matchingCorrectCount = correctPairsCount;
            matchingTotalCount = totalPairsCount;
            matchingScore = score;
            
            isCorrect = correctPairsCount == totalPairsCount;
            chosenAnswerText = userResponse;
            correctAnswerText = string.Join(", ", leftOptions.Select(a => $"{a.Text}: {a.MatchPairText}"));

            feedback.ValidationPairs = validationPairs;
        }

        var userAnswer = new UserAnswer
        {
            TestSessionId = request.TestSessionId,
            QuestionId = request.QuestionId,
            ChosenAnswerId = request.ChosenAnswerId,
            TextResponse = request.TextResponse,
            IsCorrect = isCorrect,
            TimeSpentSeconds = request.TimeSpentSeconds
        };

        context.UserAnswers.Add(userAnswer);
        await context.SaveChangesAsync();

        await redListService.ProcessAnswerAsync(userId, request.QuestionId, isCorrect);

        string feedbackText = string.Empty;
        if (request.RequestAiFeedback)
        {
            feedbackText = isCorrect 
                ? await aiService.GetMotivationAsync(question.Content, chosenAnswerText ?? "")
                : await aiService.GetExplanationAsync(question.Content, correctAnswerText ?? "", chosenAnswerText ?? "");
        }
        
        if (matchingDebugInfo != null)
        {
            feedbackText += matchingDebugInfo;
        }

        feedback.IsCorrect = isCorrect;
        feedback.CorrectAnswerId = isCorrect ? null : correctAnswerId;
        feedback.CorrectAnswerText = isCorrect ? null : correctAnswerText;
        feedback.FeedbackText = feedbackText;
        feedback.Score = matchingScore;
        feedback.MaxScore = matchingScore.HasValue ? ScoringConstants.ScoreMatchingMax : null;
        feedback.CorrectPairsCount = matchingCorrectCount;
        feedback.TotalPairsCount = matchingTotalCount;

        return new Response<AnswerFeedbackDto>(feedback);
    }

    public async Task<Response<TestResultDto>> FinishTestAsync(Guid userId, Guid testSessionId)
    {
        var session = await context.TestSessions
            .Include(t => t.Answers)
            .FirstOrDefaultAsync(t => t.Id == testSessionId && t.UserId == userId);

        if (session == null)
            return new Response<TestResultDto>(HttpStatusCode.NotFound, "Тест ёфт нашуд");

        if (session.IsCompleted)
            return new Response<TestResultDto>(HttpStatusCode.BadRequest, "Тест аллакай тамом шудааст");

        session.FinishedAt = DateTime.UtcNow;
        session.IsCompleted = true;

        var questionIds = session.Answers.Select(a => a.QuestionId).ToList();
        var questions = await context.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync();

        int totalScore = 0;
        int maxPossibleScore = 0;

        foreach (var answer in session.Answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question != null)
            {
                totalScore += scoringService.CalculateQuestionScore(question, answer);
                maxPossibleScore += scoringService.GetMaxScoreForQuestion(question);
            }
        }

        session.TotalScore = totalScore;

        int xpEarned = await gamificationService.ProcessTestSessionEndAsync(userId, session);

        await context.SaveChangesAsync();

        var questionContents = questions.ToDictionary(q => q.Id, q => q.Content);

        var summary = session.Answers.Select(a => (
            Question: questionContents.GetValueOrDefault(a.QuestionId) ?? "Савол ёфт нашуд",
            IsCorrect: a.IsCorrect
        )).ToList();

        var aiAnalysis = await aiService.AnalyzeTestResultAsync(session.TotalScore, questionIds.Count, summary);

        var totalQuestions = session.Answers.Count;
        var percentage = maxPossibleScore > 0 ? (double)totalScore / maxPossibleScore * 100 : 0;

        var result = new TestResultDto
        {
            TestSessionId = testSessionId,
            TotalScore = totalScore,
            CorrectAnswers = session.Answers.Count(a => a.IsCorrect),
            TotalQuestions = totalQuestions,
            Percentage = percentage,
            IsPassed = percentage >= 60,
            XPEarned = xpEarned,
            AiAnalysis = aiAnalysis,
            Results = session.Answers.Select(a => new QuestionResultDto
            {
                QuestionId = a.QuestionId,
                IsCorrect = a.IsCorrect,
                UserAnswerId = a.ChosenAnswerId,
                CorrectAnswerId = context.AnswerOptions
                    .Where(ao => ao.QuestionId == a.QuestionId && ao.IsCorrect)
                    .Select(ao => ao.Id)
                    .FirstOrDefault()
            }).ToList()
        };

        return new Response<TestResultDto>(result);
    }

    public async Task<Response<List<TestSessionDto>>> GetUserTestHistoryAsync(Guid userId, int page, int pageSize)
    {
        var sessions = await context.TestSessions
            .Where(t => t.UserId == userId && t.IsCompleted)
            .OrderByDescending(t => t.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TestSessionDto
            {
                Id = t.Id,
                Mode = t.Mode,
                ClusterNumber = t.ClusterNumber,
                TotalScore = t.TotalScore,
                StartedAt = t.StartedAt,
                FinishedAt = t.FinishedAt
            })
            .ToListAsync();

        return new Response<List<TestSessionDto>>(sessions);
    }
}
