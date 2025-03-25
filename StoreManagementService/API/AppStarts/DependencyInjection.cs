

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


            services.AddScoped<IImportRepos, ImportRepos>();
            services.AddScoped<IStaffDetailRepository, StaffDetailRepository>();
            services.AddScoped<IAuditLogRepos, AuditLogRepos>();
            services.AddScoped<IWareHouseStockRepos, WareHouseStockRepos>();

            services.AddScoped<CreateImportHandler>();
            services.AddScoped<GetImportHandler>();
            services.AddScoped<GetAllStaffHandler>();
            services.AddScoped<AssignStaffHandler>();

            services.AddScoped<GetAllProductHandler>();
            services.AddScoped<ImportDoneHandler>();
            services.AddScoped<ImportIncompletedHandler>();
            services.AddScoped<ImportShortageHandler>();

            services.AddScoped<GetAllStaffImportHandler>();






            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();

            //services.AddScoped<IUserService, UserServices>();

        }


    }
}
