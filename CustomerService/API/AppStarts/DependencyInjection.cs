

using Application.Interfaces;
using Application.UseCases;
using CloudinaryDotNet;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.HelperServices;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

            // use DI here
            // Cloudinary configuration
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
                if (string.IsNullOrEmpty(settings.CloudName) ||
                    string.IsNullOrEmpty(settings.ApiKey) ||
                    string.IsNullOrEmpty(settings.ApiSecret))
                {
                    throw new ArgumentException("CloudinarySettings không được cấu hình đúng trong appsettings.json");
                }
                var account = new CloudinaryDotNet.Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
                return new Cloudinary(account);
            });
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<EditProfileHandler>();
            services.AddScoped<GetCustomerProfileHandler>();
            services.AddScoped<ICartRepository, CartRepository>();  
            services.AddScoped<GetShoppingCartHandler>();
            services.AddScoped<InteractionHandler>();
            services.AddScoped<SuggestProductsHandler>();
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICommentService, FeedbackHandler>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<PreferredStyleHandler>();
            services.AddScoped<IReplyFeedbackService, ReplyHandler>();
            services.AddScoped<ICustomerProfileDataService, CustomerProfileDataService>();
            services.AddScoped<ICustomerRecentClickService, CustomerRecentClickService>();
            //services.AddScoped<IOrderRepository, OrderRepository>();



            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();

            //services.AddScoped<IUserService, UserServices>();

        }

      
    }
}
