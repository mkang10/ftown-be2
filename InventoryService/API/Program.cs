using API.AppStarts;
using Infrastructure;
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
var connStr = builder.Configuration.GetConnectionString("DBDefault");
Console.WriteLine(">> ?ang dùng connection string: " + connStr);
// Add services to the container
var redisConfig = builder.Configuration.GetSection("Redis");
string redisConnectionString = $"{redisConfig["Host"]}:{redisConfig["Port"]},password={redisConfig["Password"]}";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = redisConfig["InstanceName"];
});
// dang ký Redis ConnectionMultiplexer vào DI Container
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
// Add depen
builder.Services.InstallService(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "InventoryService API",
        Version = "v1"
    });
});


var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "InventoryService API v1");
    c.RoutePrefix = "swagger"; 
});

app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
