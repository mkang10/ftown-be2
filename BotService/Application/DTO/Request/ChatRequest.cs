using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class ChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; }
    }

    public class ChatCompletionDelta
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class ChatCompletionChoice
    {
        [JsonPropertyName("delta")]
        public ChatCompletionDelta Delta { get; set; }
    }

    public class ChatCompletionChunk
    {
        [JsonPropertyName("choices")]
        public List<ChatCompletionChoice> Choices { get; set; }
    }
}

