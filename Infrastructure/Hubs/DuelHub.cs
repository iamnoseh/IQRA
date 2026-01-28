using Microsoft.AspNetCore.SignalR;
using Infrastructure.Services;
using Application.DTOs.Duel;

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
            
            await Clients.Group(session.SessionId).SendAsync("MatchFound", session);
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
            await Clients.Group(sessionId).SendAsync("QuestionStart", session.Questions[0], 0);
        }
    }

    public async Task SubmitAnswer(string sessionId, string userId, int qIndex, string choice)
    {
        Console.WriteLine($"[DuelHub] SubmitAnswer: Session={sessionId}, User={userId}, Q={qIndex}");
        var (success, newScore, bothAnswered, isDuelFinished) = await duelManager.SubmitAnswerAsync(sessionId, userId, qIndex, choice);
        
        if (success)
        {
            await Clients.Group(sessionId).SendAsync("ScoreUpdate", userId, newScore);

            if (isDuelFinished)
            {
                var session = duelManager.GetSession(sessionId);
                await Clients.Group(sessionId).SendAsync("DuelFinished", session);
            }
            else if (bothAnswered)
            {
                var session = duelManager.GetSession(sessionId);
                await Task.Delay(1000); 
                await Clients.Group(sessionId).SendAsync("QuestionStart", 
                    session!.Questions[session.CurrentQuestionIndex], 
                    session.CurrentQuestionIndex);
            }
        }
    }
}
