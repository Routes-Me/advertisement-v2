using AdvertisementService.Abstraction;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace AdvertisementService.Extensions
{
    public static class ServiceConfiguration
    {
        internal static void AddApiVersion(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });
        }
        internal static void AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            var dependenciessSection = configuration.GetSection("Dependencies");
            services.Configure<Dependencies>(dependenciessSection);

            var azureConfigSection = configuration.GetSection("AzureStorageBlobConfig");
            services.Configure<AzureStorageBlobConfig>(azureConfigSection);
            var azureConfig = azureConfigSection.Get<AzureStorageBlobConfig>();
        }

        internal static void AddController(this IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
        }

        internal static void AddDbContexts(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AdvertisementContext>(options =>
            {
                options.UseMySql(configuration.GetConnectionString("DefaultConnection"));
            });
        }

        internal static void AddInjections(this IServiceCollection services)
        {


            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IMediaTypeConversionRepository, MediaTypeConversionRepository>();
        }
        internal static void Cors(this IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));


        }
    }
}
