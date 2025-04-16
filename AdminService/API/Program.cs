using API.AppStarts;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Cấu hình CORS cho phép các nguồn được chỉ định
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5000", "https://ftown-admin.vercel.app/") // Thêm nguồn mới
              .AllowAnyMethod()
              .AllowAnyHeader();
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
