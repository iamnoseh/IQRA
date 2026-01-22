using Application.Interfaces;
using Application.DTOs.Gamification;
using Application.Responses;
using Domain.Entities.Gamification;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class NotificationService(ApplicationDbContext context) : INotificationService
{
    public async Task SendToUserAsync(Guid userId, string title, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task CreateSystemAlertAsync(string title, string message)
    {
        var notification = new Notification
        {
            UserId = null, // System alert
            Title = title,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task<Response<List<NotificationDto>>> GetUserNotificationsAsync(Guid userId)
    {
        var notifications = await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return new Response<List<NotificationDto>>(notifications);
    }

    public async Task<Response<bool>> MarkAsReadAsync(Guid notificationId)
    {
        var notification = await context.Notifications.FindAsync(notificationId);
        if (notification == null)
            return new Response<bool>(System.Net.HttpStatusCode.NotFound, "Навсозӣ ёфт нашуд");

        notification.IsRead = true;
        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }
}
