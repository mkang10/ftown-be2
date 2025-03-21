using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.SignalR;
using Application.DTO.Request;

namespace Application.UseCases
{
    public class SendNotificationHandler
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        public SendNotificationHandler(INotificationRepository notificationRepository, IHubContext<NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        public async Task<Notification> Handle(SendNotificationRequest request)
        {
            var notification = new Notification
            {
                AccountId = request.AccountId,
                Title = request.Title,
                Content = request.Message,
                NotificationType = request.NotificationType,
                TargetId = request.TargetId,
                TargetType = request.TargetType,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            await _notificationRepository.AddNotificationAsync(notification);

            // Gửi thông báo real-time qua SignalR
            await _hubContext.Clients.User(request.AccountId.ToString())
                .SendAsync("ReceiveNotification", request.Title, request.Message);

            return notification;
        }
    }
}
