using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class ChatRequest
    {
        public string Message { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatServices _chatService;

        public ChatController(IChatServices chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("sse")]
        public async Task StreamChatSse([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            // Đặt Content-Type cho SSE
            Response.ContentType = "text/event-stream";

            // Gọi dịch vụ chat, mỗi khi nhận được một chunk dữ liệu, định dạng theo chuẩn SSE (data: ...\n\n)
            await _chatService.StreamChatAsync(request.Message, async chunk =>
            {
                // Định dạng chunk theo chuẩn SSE
                string sseData = $"data: {chunk}\n\n";
                await Response.WriteAsync(sseData, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }, cancellationToken);
        }
    }
}
