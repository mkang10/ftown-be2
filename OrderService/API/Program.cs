using API.AppStarts;
using Application.Interfaces;
using CloudinaryDotNet;
using Infrastructure.HelperServices;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ??ng k� c?u h�nh CloudinarySettings
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);

// ??ng k� Cloudinary d??i d?ng Singleton, s? d?ng IOptions ?? l?y c?u h�nh
builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    if (string.IsNullOrEmpty(settings.CloudName) ||
        string.IsNullOrEmpty(settings.ApiKey) ||
        string.IsNullOrEmpty(settings.ApiSecret))
    {
        throw new ArgumentException("CloudinarySettings kh�ng ???c c?u h�nh ?�ng trong appsettings.json");
    }

    var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
    return new Cloudinary(account);
});

// ??ng k� CloudinaryService
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

var redisConfig = builder.Configuration.GetSection("Redis");
string redisConnectionString = $"{redisConfig["Host"]}:{redisConfig["Port"]},password={redisConfig["Password"]}";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = redisConfig["InstanceName"];
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConfig = builder.Configuration.GetSection("Redis");
    var redisConnection = $"{redisConfig["Host"]}:{redisConfig["Port"]},password={redisConfig["Password"]}";

    var configuration = ConfigurationOptions.Parse(redisConnection, true);
    return ConnectionMultiplexer.Connect(configuration);
});
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
