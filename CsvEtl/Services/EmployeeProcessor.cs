using CsvEtl.Configuration;
using CsvEtl.Models;
using CsvEtl.Validators;
using Microsoft.Extensions.Options;

namespace CsvEtl.Services;

/// <summary>
/// Orchestrator - Coordinates all other services
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
            // 1. Parse command line argument
            ParseCommandLineArgs(args);

            // 2. Validate input file exists
            if (!File.Exists(_options.InputPath))
            {
                result.ErrorMessages.Add($"Cannot find input file: {_options.InputPath}");
                return result;
            }

            // 3. Prepare output files
            await PrepareOutputFilesAsync();

            // 4. Process data (main logic)
            await ProcessDataAsync(result);  // Also send results

            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add($"Unexpected error: {ex.Message}");
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
        // Create folders
        Directory.CreateDirectory(Path.GetDirectoryName(_options.OutputCsvPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(_options.OutputJsonl)!);
        Directory.CreateDirectory(Path.GetDirectoryName(_options.ErrorLogsPath)!);

        // Clear files
        await File.WriteAllTextAsync(_options.OutputCsvPath, string.Empty);
        await File.WriteAllTextAsync(_options.OutputJsonl, string.Empty);
        await File.WriteAllTextAsync(_options.ErrorLogsPath, string.Empty);

        // Write CSV header
        await _csvService.WriteHeaderAsync(_options.OutputCsvPath);
    }

    private async Task ProcessDataAsync(ProcessingResult result)
    {
        // Read all employees from input file
        var employees = await _csvService.ReadEmployeesAsync(_options.InputPath);

        foreach (var employee in employees)
        {
            result.TotalRecords++;

            // Check if filtered away due to --min-country
            if (_validator.ShouldFilterOut(employee))
            {
                continue; // Skip without count as rejected
            }

            // Validate
            var validationResult = _validator.Validate(employee);

            if (!validationResult.IsValid)
            {
                result.RejectedRecords++;
                await _csvService.WriteErrorLogAsync(_options.ErrorLogsPath, result.TotalRecords, validationResult);
                continue;
            }

            // Transform and write output
            var validEmployee = _validator.Transform(employee);

            await _csvService.WriteEmployeeAsync(_options.OutputCsvPath, validEmployee);
            await _csvService.WriteJsonLineAsync(_options.OutputJsonl, validEmployee);

            result.AcceptedRecords++;
        }
    }
}