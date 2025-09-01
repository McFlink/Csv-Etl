using CsvEtl.Extensions;
using CsvEtl.Services; // AddDataProcessingServices
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CsvEtl;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Registrera allt via din extension och bind "Etl" till EtlOptions
            builder.Services.AddDataProcessingServices(builder.Configuration);

            var app = builder.Build();

            // Kör din processor
            var processor = app.Services.GetRequiredService<IEmployeeProcessor>();
            var result = await processor.ProcessAsync(args);

            result.PrintSummary();
            return result.Success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[FATAL] {ex.Message}");
            return 1;
        }
    }
}
