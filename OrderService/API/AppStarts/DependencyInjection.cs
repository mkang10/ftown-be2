

using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Clients;
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
            //Repository

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IShippingAddressRepository, ShippingAddressRepository>();
            services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
            //Handler

            services.AddScoped<CreateOrderHandler>();
            services.AddScoped<ProcessPaymentHandler>();
            services.AddScoped<AutoSelectStoreHandler>();
            services.AddScoped<GetShippingAddressHandler>();
            services.AddScoped<GetOrderHistoryHandler>();
            services.AddScoped<GetOrdersByStatusHandler>();

            



            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();


        }

      
    }
}
