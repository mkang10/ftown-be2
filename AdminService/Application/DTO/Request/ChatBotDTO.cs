using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class ChatBotDTO
    {
        public string? Key { get; set; }

        public string? BaseUrl { get; set; }

        public string? Context { get; set; }

        public bool IsDefault { get; set; }
    }
    public class ChatBotListDTO
    {
        public int ChatBotId { get; set; }

        public string? Key { get; set; }

        public string? BaseUrl { get; set; }

        public string? Context { get; set; }

        public bool IsDefault { get; set; }
    }

    public class ChatBotDStatusTO
    {
        public bool IsDefault { get; set; }
    }
}
