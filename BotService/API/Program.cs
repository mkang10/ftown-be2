using API.AppStarts;
using Domain.Interfaces;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Thêm các dịch vụ cần thiết vào container
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Đăng ký dịch vụ chat với các tham số cấu hình (base URL và apiKey)
builder.Services.AddScoped<IChatServices>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    return new DeepSeekService(httpClient, "http://127.0.0.1:1234/v1", "LMstudio");
});
// Add services to the container.

// Add depen
builder.Services.InstallService(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
