using CsvEtl.Configuration;              // EtlOptions
using CsvEtl.Services;                   // CsvService, EmployeeProcessor
using CsvEtl.Validators;                 // EmployeeValidator
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CsvEtl.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDataProcessingServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind "Etl" -> EtlOptions and validate at start
            services.AddOptions<EtlOptions>()
                .Bind(configuration.GetSection("Etl"))
                .Validate(o =>
                {
                    try { o.Validate(); return true; } catch { return false; }
                }, "EtlOptions validation failed")
                .ValidateOnStart();

            // Register rest of services
            services.AddScoped<IEmployeeProcessor, EmployeeProcessor>();
            services.AddScoped<ICsvService, CsvService>();
            services.AddScoped<EmployeeValidator>();

            return services;
        }
    }
}
