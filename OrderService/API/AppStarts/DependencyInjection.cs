﻿

using Application.Interfaces;
using Application.UseCases;
using Domain.Interfaces;
using Infrastructure.Clients;
using Infrastructure.DBContext;
using Infrastructure.HelperServices;
using Infrastructure.Repositories;
using Infrastructure.Repository;
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

            //Inject Services
            
            services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7264/api/");
            });
            services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7265/api/");
            });
            services.AddHttpClient<INotificationClient, NotificationServiceClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7270/api/");
            });
            services.AddHttpClient<IPayOSService, PayOSService>(client =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var payOSBaseUrl = configuration["PayOS:ApiUrl"];
                client.BaseAddress = new Uri(payOSBaseUrl);
            });

            // use DI here

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddScoped<IOrderProcessingHelper, OrderProcessingHelper>();
            //Repository

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IShippingAddressRepository, ShippingAddressRepository>();
            services.AddScoped<IReturnOrderRepository, ReturnOrderRepository>();
            services.AddScoped<IRedisRepository, RedisRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            //Handler

            services.AddScoped<CreateOrderHandler>();
            services.AddScoped<GetShippingAddressHandler>();
            services.AddScoped<GetOrdersByStatusHandler>();
            services.AddScoped<GetSelectedCartItemsHandler>();
            services.AddScoped<CheckOutHandler>();
            services.AddScoped<ShippingCostHandler>();
            services.AddScoped<GetOrderDetailHandler>();
            services.AddScoped<UpdateOrderStatusHandler>();
            services.AddScoped<GetOrderItemsHandler>();
            services.AddScoped<GetReturnableOrdersHandler>();
            services.AddScoped<GetOrderItemsForReturnHandler>();
            services.AddScoped<ProcessReturnCheckoutHandler>(); 
            services.AddScoped<SubmitReturnRequestHandler>();
            services.AddScoped<RedisHandler>();
            services.AddScoped<AuditLogHandler>();

            //GHN
            services.AddScoped<GHNLogHandler>();
            services.AddScoped<IGHNLogRepository, GHNLogRepository>();



            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();


        }

      
    }
}
