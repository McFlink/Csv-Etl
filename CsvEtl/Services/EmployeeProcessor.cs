using CsvEtl.Configuration;
using CsvEtl.Models;
using CsvEtl.Validators;
using Microsoft.Extensions.Options;

namespace CsvEtl.Services;

/// <summary>
/// Orkestrator - koordinerar alla andra tjänster
/// Detta är din gamla Program.cs logik, bara uppdelad i metoder
/// </summary>
public class EmployeeProcessor : IEmployeeProcessor
{
    private readonly ICsvService _csvService;
    private readonly EmployeeValidator _validator;
    private readonly EtlOptions _options;

    public EmployeeProcessor(
        ICsvService csvService,
        EmployeeValidator validator,
        IOptions<EtlOptions> options)
    {
        _csvService = csvService;
        _validator = validator;
        _options = options.Value;
    }

    public async Task<ProcessingResult> ProcessAsync(string[] args)
    {
        var result = new ProcessingResult();

        try
        {
            // 1. Parsa kommandoradsargument
            ParseCommandLineArgs(args);

            // 2. Validera att input-filen finns
            if (!File.Exists(_options.InputPath))
            {
                result.ErrorMessages.Add($"Hittar inte inputfil: {_options.InputPath}");
                return result;
            }

            // 3. Förbered output-filer
            await PrepareOutputFilesAsync();

            // 4. Processa data (huvudlogiken)
            await ProcessDataAsync(result);  // ← Nu skickar vi med result

            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add($"Oväntat fel: {ex.Message}");
            return result;
        }
    }

    private void ParseCommandLineArgs(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--min-country")
            {
                _options.MinCountry = args[i + 1];
            }
        }
    }

    private async Task PrepareOutputFilesAsync()
    {
        // Skapa mappar
        Directory.CreateDirectory(Path.GetDirectoryName(_options.OutputCsvPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(_options.OutputJsonl)!);
        Directory.CreateDirectory(Path.GetDirectoryName(_options.ErrorLogsPath)!);

        // Rensa filer
        await File.WriteAllTextAsync(_options.OutputCsvPath, string.Empty);
        await File.WriteAllTextAsync(_options.OutputJsonl, string.Empty);
        await File.WriteAllTextAsync(_options.ErrorLogsPath, string.Empty);

        // Skriv CSV-header
        await _csvService.WriteHeaderAsync(_options.OutputCsvPath);
    }

    private async Task ProcessDataAsync(ProcessingResult result)
    {
        // Läs alla employees från input-filen
        var employees = await _csvService.ReadEmployeesAsync(_options.InputPath);

        foreach (var employee in employees)
        {
            result.TotalRecords++;

            // Kolla om den ska filtreras bort pga --min-country
            if (_validator.ShouldFilterOut(employee))
            {
                continue; // Hoppa över utan att räkna som rejected
            }

            // Validera
            var validationResult = _validator.Validate(employee);

            if (!validationResult.IsValid)
            {
                result.RejectedRecords++;
                await _csvService.WriteErrorLogAsync(_options.ErrorLogsPath, result.TotalRecords, validationResult);
                continue;
            }

            // Transformera och skriv output
            var validEmployee = _validator.Transform(employee);

            await _csvService.WriteEmployeeAsync(_options.OutputCsvPath, validEmployee);
            await _csvService.WriteJsonLineAsync(_options.OutputJsonl, validEmployee);

            result.AcceptedRecords++;
        }
    }
}