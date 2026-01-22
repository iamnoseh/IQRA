using Application.DTOs.Gamification;
using Application.Responses;

namespace Application.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(Guid userId, string title, string message);
    Task CreateSystemAlertAsync(string title, string message);
    Task<Response<List<NotificationDto>>> GetUserNotificationsAsync(Guid userId);
    Task<Response<bool>> MarkAsReadAsync(Guid notificationId);
}
