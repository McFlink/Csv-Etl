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
            // Bind "Etl" → EtlOptions och validera vid uppstart
            services.AddOptions<EtlOptions>()
                .Bind(configuration.GetSection("Etl"))
                .Validate(o =>
                {
                    try { o.Validate(); return true; } catch { return false; }
                }, "EtlOptions validation failed")
                .ValidateOnStart();

            // Registrera övrigt
            services.AddScoped<IEmployeeProcessor, EmployeeProcessor>();
            services.AddScoped<ICsvService, CsvService>();
            services.AddScoped<EmployeeValidator>();

            return services;
        }
    }
}
