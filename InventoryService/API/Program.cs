using API.AppStarts;
using Infrastructure;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

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
builder.Services.AddSwaggerGen();

var app = builder.Build();
//using (var scope = app.Services.CreateScope())
//{
//    var productSyncService = scope.ServiceProvider.GetRequiredService<ProductSyncService>();

//    try
//    {
//        await productSyncService.SyncProductsToElasticsearch();
//        Console.WriteLine("? D? li?u ?ã ???c ??ng b? lên Elasticsearch thành công!");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"? L?i khi ??ng b? d? li?u lên Elasticsearch: {ex.Message}");
//    }
//}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowLocalhost");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
