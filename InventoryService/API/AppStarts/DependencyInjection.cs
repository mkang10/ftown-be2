

using Application.Interfaces;
using Application.UseCases;
using CloudinaryDotNet;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
            services.AddSingleton<ElasticsearchService>();
            services.AddScoped<IPromotionService, PromotionService>();
			services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddSingleton<IElasticClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var settings = new ConnectionSettings(new Uri(configuration["Elasticsearch:Url"]))
                    .BasicAuthentication(configuration["Elasticsearch:Username"], configuration["Elasticsearch:Password"]) // ✅ Đăng nhập tự động
                    .DisableDirectStreaming();  // ✅ Bật log để debug nếu có lỗi

                return new ElasticClient(settings);
            });
            services.AddScoped<ISizeAndColorRepository, ColorAndSizeRepository>();

            //Handler    
            services.AddScoped<ColorHandler>();
            services.AddScoped<SizeHandler>();


            services.AddScoped<GetAllProductsHandler>();
            services.AddScoped<FilterProductHandler>();
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
            services.AddScoped<GetFavoriteProductsHandler>();
            services.AddScoped<AddFavoriteHandler>();
            services.AddScoped<RemoveFavoriteHandler>();
            services.AddScoped<GetTopSellingProductHandler>();
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
