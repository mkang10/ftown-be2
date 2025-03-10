﻿

using Application.UseCases;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
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

            // use DI here
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IEditProfileRepository, EditProfileRepository>();
            services.AddScoped<EditProfileHandler>();
            services.AddScoped<GetCustomerProfileHandler>();
            services.AddScoped<ICartRepository, CartRepository>();  
            services.AddScoped<GetShoppingCartHandler>();
            services.AddSingleton<IRedisCacheService, RedisCacheService>();

            //services.AddScoped<IOrderRepository, OrderRepository>();



            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();

            //services.AddScoped<IUserService, UserServices>();

        }

      
    }
}
