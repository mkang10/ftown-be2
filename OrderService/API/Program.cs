using API.AppStarts;
using Application.Interfaces;
using CloudinaryDotNet;
using Infrastructure;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();

// Ki?m tra n?u c?u h�nh Cloudinary b? thi?u ho?c kh�ng h?p l?
if (cloudinarySettings == null ||
    string.IsNullOrEmpty(cloudinarySettings.CloudName) ||
    string.IsNullOrEmpty(cloudinarySettings.ApiKey) ||
    string.IsNullOrEmpty(cloudinarySettings.ApiSecret))
{
    throw new ArgumentException("CloudinarySettings kh�ng ???c c?u h�nh ?�ng trong appsettings.json");
}

// ??ng k� CloudinarySettings v�o DI container
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);

// ??ng k� Cloudinary v?i t�i kho?n ???c kh?i t?o t? c?u h�nh
var cloudinaryAccount = new Account(
    cloudinarySettings.CloudName,
    cloudinarySettings.ApiKey,
    cloudinarySettings.ApiSecret
);
var cloudinary = new Cloudinary(cloudinaryAccount);

builder.Services.AddSingleton(cloudinary);

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
