using API.AppStarts;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

//Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy => policy
            .WithOrigins("http://localhost:3000", "http://localhost:5000", "https://ftown-admin-dhww.vercel.app", "https://ftown-admin.vercel.app") // Thêm nguồn mới
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add depen
builder.Services.InstallService(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();

app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
