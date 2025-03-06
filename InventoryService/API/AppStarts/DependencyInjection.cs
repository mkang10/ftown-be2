

using Application.UseCases;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Nest;

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
            services.AddSingleton<ElasticsearchService>();
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddSingleton<IElasticClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var settings = new ConnectionSettings(new Uri(configuration["Elasticsearch:Url"]))
                    .BasicAuthentication(configuration["Elasticsearch:Username"], configuration["Elasticsearch:Password"]) // ✅ Đăng nhập tự động
                    .DisableDirectStreaming();  // ✅ Bật log để debug nếu có lỗi

                return new ElasticClient(settings);
            });

       


            services.AddScoped<GetAllProductsHandler>();
            services.AddScoped<GetProductDetailHandler>();
            services.AddScoped<GetProductVariantByIdHandler>();
            services.AddScoped<GetAllStoresHandler>();
            services.AddScoped<GetStoreByIdHandler>();
            services.AddScoped<CreateStoreHandler>();
            services.AddScoped<UpdateStoreHandler>();
            services.AddScoped<DeleteStoreHandler>();
            services.AddScoped<GetStoreStockByVariantHandler>();
            services.AddScoped<ProductSyncService>();
            services.AddScoped<UpdateStockAfterOrderHandler>();
            services.AddScoped<GetAllProductVariantsByIdsHandler>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost",
                    builder => builder.WithOrigins("http://localhost:3000")
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            // Repository

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IStoreStockRepository, StoreStockRepository>();







            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();

            //services.AddScoped<IUserService, UserServices>();

        }

      
    }
}
