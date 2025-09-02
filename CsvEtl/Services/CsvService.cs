using CsvEtl.Configuration;
using CsvEtl.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace CsvEtl.Services;

/// <summary>
/// Only responsible for csv operations and file I/O
/// </summary>
public class CsvService : ICsvService
{
    private readonly EtlOptions _options;
    private readonly CsvConfiguration _csvConfig;

    public CsvService(IOptions<EtlOptions>options)
    {
        _options = options.Value;

        _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = _options.TrimFields ? TrimOptions.Trim : TrimOptions.None,
            BadDataFound = _options.IgnoreBadData ? null : (args) => {},
            MissingFieldFound = null,
            HeaderValidated = null
        };
    }

    /// <summary>
    /// Read all employees from csv file
    /// </summary>
    public async Task<IEnumerable<EmployeeRaw>> ReadEmployeesAsync(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var employees = new List<EmployeeRaw>();

        try
        {
            await using var inputStream = File.OpenRead(filePath);
            using var reader = new StreamReader(inputStream, Encoding.UTF8);
            using var csv = new CsvReader(reader, _csvConfig);

            // Read header
            await csv.ReadAsync();
            csv.ReadHeader();

            // Read all rows
            while (await csv.ReadAsync())
            {
                try
                {
                    var employee = new EmployeeRaw
                    {
                        Id = csv.GetField<int>("Id"),
                        FirstName = csv.GetField("FirstName") ?? string.Empty,
                        LastName = csv.GetField("LastName") ?? string.Empty,
                        Email = csv.GetField("Email") ?? string.Empty,
                        Country = csv.GetField("Country") ?? string.Empty
                    };

                    employees.Add(employee);
                }
                catch (Exception ex)
                {
                    // If a row is corrupt, log and continue
                    Console.WriteLine($"Warning: could not read row {csv.Context.Parser.Row}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error while reading csv file '{filePath}': {ex.Message}", ex);
        }

        return employees;
    }

    /// <summary>
    /// Writes csv header to output file
    /// </summary>
    public async Task WriteHeaderAsync(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        try
        {
            await using var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            using var csv = new CsvWriter(writer, _csvConfig);

            csv.WriteField("Id");
            csv.WriteField("FirstName");
            csv.WriteField("LastName");
            csv.WriteField("Email");
            csv.WriteField("Country");
            csv.WriteField("FullName");

            await csv.NextRecordAsync();
            await writer.FlushAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Fel vid skrivning av CSV-header till '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Writes one employee to csv file
    /// </summary>
    public async Task WriteEmployeeAsync(string filePath, EmployeeValid employee)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        try
        {
            await using var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            using var csv = new CsvWriter(writer, _csvConfig);

            // Same order as header
            csv.WriteField(employee.Id);
            csv.WriteField(employee.FirstName);
            csv.WriteField(employee.LastName);
            csv.WriteField(employee.Email);
            csv.WriteField(employee.Country);
            csv.WriteField(employee.FullName);

            await csv.NextRecordAsync();
            await writer.FlushAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error while writing employee to csv file '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Writes an employee as JSON-line to JSONL-file
    /// </summary>
    public async Task WriteJsonLineAsync(string filePath, EmployeeValid employee)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        try
        {
            await using var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false));

            var json = JsonSerializer.Serialize(employee, new JsonSerializerOptions { WriteIndented = false });
            await writer.WriteLineAsync(json);
            await writer.FlushAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error writing JSON line to '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Writes error log för rejected employee
    /// </summary>
    public async Task WriteErrorLogAsync(string filePath, int rowNumber, EmployeeValidationResult validationResult)
    {
        try
        {
            await using var writer = new StreamWriter(filePath, append: true, Encoding.UTF8);

            var employee = validationResult.Employee;
            var errorMessage = $"Row {rowNumber}: {validationResult.ErrorMessage} | Data: {employee.Id},{employee.FirstName},{employee.LastName},{employee.Email},{employee.Country}";

            await writer.WriteLineAsync(errorMessage);
            await writer.FlushAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error when writing error log to '{filePath}': {ex.Message}", ex);
        }
    }
}