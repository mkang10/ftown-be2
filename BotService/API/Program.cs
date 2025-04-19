using API.AppStarts;
using API.Chathub;
using Domain.Interfaces;
using Infrastructure;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// ==== 1. Cấu hình các dịch vụ ====
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.InstallService(builder.Configuration); // Cấu hình DI custom

builder.Services.AddSignalR(); // SignalR
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(
                                "http://127.0.0.1:5500",
                                "http://localhost:5500",
                                "https://localhost:7009",
                                "http://localhost:5000"
                            )
       
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ==== 2. Build ứng dụng ====
var app = builder.Build();

// ==== 3. Cấu hình middleware ====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();

// ==== 4. Map các endpoint ====
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chathub");
    endpoints.MapHub<BotHub>("/bothub");

});

// ==== 5. Kích hoạt WebSocket + Endpoint custom ====
app.UseWebSockets(); // Phải đặt trước khi map WebSocket endpoint

app.Map("/ws/chat", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var userIdStr = context.Request.Query["userId"];
    if (!int.TryParse(userIdStr, out var userId))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Missing or invalid userId");
        return;
    }

    var socket = await context.WebSockets.AcceptWebSocketAsync();
    var chatSvc = context.RequestServices.GetRequiredService<ChatAppService>();

    var buffer = new byte[4096];
    while (socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Text)
        {
            var userMsg = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

            // Gửi lại message của user
            var userPayload = System.Text.Json.JsonSerializer.Serialize(new
            {
                sender = "user",
                message = userMsg
            });
            await socket.SendAsync(
                new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(userPayload)),
                WebSocketMessageType.Text, true, CancellationToken.None);

            // Lấy reply từ bot
            var botReply = await chatSvc.GetFullReplyAsync(userId, userMsg, CancellationToken.None);

            var botPayload = System.Text.Json.JsonSerializer.Serialize(new
            {
                sender = "assistant",
                message = botReply
            });
            await socket.SendAsync(
                new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(botPayload)),
                WebSocketMessageType.Text, true, CancellationToken.None);
        }
        else if (result.MessageType == WebSocketMessageType.Close)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
        }
    }
});

app.Run();
