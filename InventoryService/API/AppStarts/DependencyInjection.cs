

using Application.Interfaces;
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
            services.AddScoped<IExcelRepo, ExcelRepository>();
            services.AddScoped<IExcelService, ExcelHandler>();
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

            //Handler    

            services.AddScoped<GetAllProductsHandler>();
            services.AddScoped<GetProductDetailHandler>();
            services.AddScoped<GetProductVariantByIdHandler>();
            services.AddScoped<GetWarehouseByIdHandler>();
            services.AddScoped<GetWareHouseStockByVariantHandler>();
            services.AddScoped<ProductSyncService>();
            services.AddScoped<UpdateStockAfterOrderHandler>();
            services.AddScoped<GetAllProductVariantsByIdsHandler>();
            services.AddScoped<GetStockQuantityHandler>();
            services.AddScoped<GetProductVariantByDetailsHandler>();
            services.AddScoped<CreateProductHandler>();
            services.AddScoped<RedisHandler>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost",
                    builder => builder.WithOrigins("http://localhost:3000")
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            // Repository

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IWareHousesStockRepository, WareHousesStockRepository>();
            services.AddScoped<IRedisRepository, RedisRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>();


            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();

            //services.AddScoped<IUserService, UserServices>();

        }

      
    }
}
