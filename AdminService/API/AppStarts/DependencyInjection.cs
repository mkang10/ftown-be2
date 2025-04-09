
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

            // use DI here
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            services.AddScoped<IImportRepos, InventoryImportRepository>();
            services.AddScoped<IAuditLogRepos, AuditLogRepository>();
            services.AddScoped<IDispatchRepos, DispatchRepos>();
            services.AddScoped<IImportStoreRepos, ImportStoreRepos>();
            services.AddScoped<IStoreExportRepos, StoreExportRepos>();
            services.AddScoped<ITransferRepos, TransferRepos>();

            services.AddScoped<ApproveHandler>();
            services.AddScoped<RejectHandler>();
            services.AddScoped<GetAllImportHandler>();
            services.AddScoped<GetImportDetailHandler>();
  services.AddScoped<IUserManagementService, GetAccountHandler>();
            services.AddScoped<IRoleService, GetRoleHandler>();
            services.AddScoped<CreateImportHandler>();
            services.AddScoped<GetWareHouseHandler>();
            services.AddScoped<TransferHandler>();
            services.AddScoped<GetAllTransferHandler>();



            services.AddScoped<IUserManagementRepository, UserManagementRepository>();


            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();

            //services.AddScoped<IUserService, UserServices>();

        }


    }
}

