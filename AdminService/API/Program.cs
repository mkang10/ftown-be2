using API.AppStarts;
using CloudinaryDotNet;
using Domain.DTO.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Application.Service;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Cấu hình CORS cho phép các nguồn được chỉ định
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5000", "https://ftown-admin.vercel.app", "https://ftown-admin-dhww.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition");

    });
});

// Cấu hình Redis từ file cấu hình
var redisSection = builder.Configuration.GetSection("Redis");
var redisConnection = $"{redisSection["Host"]}:{redisSection["Port"]},password={redisSection["Password"]}";

// Thêm StackExchange.Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = redisSection["InstanceName"];
});
builder.Services.AddCloudinary();

// Thêm IConnectionMultiplexer làm Singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConfig = builder.Configuration.GetSection("Redis");
    var redisConn = $"{redisConfig["Host"]}:{redisConfig["Port"]},password={redisConfig["Password"]}";
    var configurationOptions = ConfigurationOptions.Parse(redisConn, true);
    return ConnectionMultiplexer.Connect(configurationOptions);
});

// Cài đặt các dịch vụ phụ thuộc từ API.AppStarts
builder.Services.InstallService(builder.Configuration);

// Thêm dịch vụ Controller và Swagger/OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đọc thông tin cấu hình Cloudinary từ key "CloudinarySettings"
var cloudName = builder.Configuration["CloudinarySettings:CloudName"];
var apiKey = builder.Configuration["CloudinarySettings:ApiKey"];
var apiSecret = builder.Configuration["CloudinarySettings:ApiSecret"];

// Kiểm tra cấu hình Cloudinary
if (string.IsNullOrWhiteSpace(cloudName) ||
    string.IsNullOrWhiteSpace(apiKey) ||
    string.IsNullOrWhiteSpace(apiSecret))
{
    throw new ArgumentException("Cloudinary configuration is incomplete. Please check CloudName, ApiKey, and ApiSecret in CloudinarySettings.");
}

// Khởi tạo Account và Cloudinary
var account = new Account(cloudName, apiKey, apiSecret);
var cloudinary = new Cloudinary(account);

// Đăng ký Cloudinary dưới dạng Singleton (chỉ một lần)
builder.Services.AddSingleton(cloudinary);

var app = builder.Build();

// Cấu hình pipeline HTTP request
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
