

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
            // Repository

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IStoreStockRepository, StoreStockRepository>();


            // Handler

            services.AddScoped<GetAllProductsHandler>();
            services.AddScoped<GetProductDetailHandler>();
            services.AddScoped<GetProductVariantByIdHandler>();
            services.AddScoped<GetAllStoresHandler>();
            services.AddScoped<GetStoreByIdHandler>();
            services.AddScoped<CreateStoreHandler>();
            services.AddScoped<UpdateStoreHandler>();
            services.AddScoped<DeleteStoreHandler>();
            services.AddScoped<GetStoreStockByVariantHandler>();



            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();

            //services.AddScoped<IUserService, UserServices>();

        }

      
    }
}
