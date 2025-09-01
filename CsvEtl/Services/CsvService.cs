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
/// Ansvarar BARA för CSV-operationer och fil I/O
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
    /// Läser alla employees från CSV-fil
    /// </summary>
    public async Task<IEnumerable<EmployeeRaw>> ReadEmployeesAsync(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var employees = new List<EmployeeRaw>();

        try
        {
            // Samma setup som din original-kod
            await using var inputStream = File.OpenRead(filePath);
            using var reader = new StreamReader(inputStream, Encoding.UTF8);
            using var csv = new CsvReader(reader, _csvConfig);

            // Läs header
            await csv.ReadAsync();
            csv.ReadHeader();

            // Läs alla rader
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
                    // Om en rad är korrupt, logga och fortsätt
                    Console.WriteLine($"Varning: Kunde inte läsa rad {csv.Context.Parser.Row}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Fel vid läsning av CSV-fil '{filePath}': {ex.Message}", ex);
        }

        return employees;
    }

    /// <summary>
    /// Skriver CSV-header till output-fil
    /// </summary>
    public async Task WriteHeaderAsync(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        try
        {
            await using var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            using var csv = new CsvWriter(writer, _csvConfig);

            // Samma headers som i din original-kod
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
    /// Skriver en employee till CSV-fil (append)
    /// </summary>
    public async Task WriteEmployeeAsync(string filePath, EmployeeValid employee)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        try
        {
            await using var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            using var csv = new CsvWriter(writer, _csvConfig);

            // Samma ordning som header
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
            throw new InvalidOperationException($"Fel vid skrivning av employee till CSV '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Skriver en employee som JSON-line till JSONL-fil
    /// </summary>
    public async Task WriteJsonLineAsync(string filePath, EmployeeValid employee)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        try
        {
            await using var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false));

            // Samma serialisering som i original-koden
            var json = JsonSerializer.Serialize(employee, new JsonSerializerOptions { WriteIndented = false });
            await writer.WriteLineAsync(json);
            await writer.FlushAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Fel vid skrivning av JSON-line till '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Skriver fel-logg för en rejected employee
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
            throw new InvalidOperationException($"Fel vid skrivning av error-log till '{filePath}': {ex.Message}", ex);
        }
    }
}