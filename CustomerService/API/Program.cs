using API.AppStarts;
using Application.Interfaces;
using Infrastructure.Clients;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5000", "https://ftown-client-product.vercel.app")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // N?u dùng cookie ho?c auth header
    });
});
// Add services to the container.
var redisConfig = builder.Configuration.GetSection("Redis");
var redisConnection = $"{redisConfig["Host"]}:{redisConfig["Port"]},password={redisConfig["Password"]}";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = redisConfig["InstanceName"];
});
builder.Services.AddCloudinary();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConfig = builder.Configuration.GetSection("Redis");
    var redisConnection = $"{redisConfig["Host"]}:{redisConfig["Port"]},password={redisConfig["Password"]}";

    var configuration = ConfigurationOptions.Parse(redisConnection, true);
    return ConnectionMultiplexer.Connect(configuration);
});
// Add depen
builder.Services.InstallService(builder.Configuration);
builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7265/api/");
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}
app.UseCors("AllowSpecificOrigins");


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
