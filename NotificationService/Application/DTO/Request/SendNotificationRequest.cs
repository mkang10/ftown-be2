using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class SendNotificationRequest
    {
        public int AccountId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public int? TargetId { get; set; }
        public string TargetType { get; set; }
    }
}
