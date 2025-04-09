

using Application.Interfaces;
using Application.UseCases;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.AppStarts
{
    public static class DependencyInjectionContainers
    {

        public static void InstallService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true; ;
                options.LowercaseQueryStrings = true;
            });
            services.AddDbContext<FtownContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DBDefault"));
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            //services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddScoped<IConversationService, ConversationHandler>();
            services.AddScoped<IMessageService, MessageHandler>();

            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();


            // use DI here
            //services.AddScoped<IUserService, UserServices>();
            services.AddScoped<IMessageRepository, ConversationRepository>();

        }
    }
}
