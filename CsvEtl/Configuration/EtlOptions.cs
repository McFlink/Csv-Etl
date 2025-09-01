namespace CsvEtl.Configuration;

/// <summary>
/// Konfiguration för ETL-processen
/// Följer best practices för namngivning och datatyper
/// </summary>
public class EtlOptions
{
    // Filsökvägar - använd Path.Combine för OS-kompatibilitet
    public string InputPath { get; set; } = Path.Combine("input", "employees.csv");
    public string OutputCsvPath { get; set; } = Path.Combine("output", "employees_valid.csv");
    public string OutputJsonl { get; set; } = Path.Combine("output", "employees_valid.jsonl");
    public string ErrorLogsPath { get; set; } = Path.Combine("logs", "errors.txt");

    // HashSet för effektiva lookups + case-insensitive jämförelse
    public HashSet<string> AllowedCountries { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "SE", "NO", "DK", "FI"
    };

    // Kommandoradsparametrar (sätts runtime)
    public string? MinCountry { get; set; }

    // Funktions-flaggor
    public bool RequireTopLevelDomainInEmail { get; set; } = true;
    public bool TrimFields { get; set; } = true;
    public bool IgnoreBadData { get; set; } = true;

    // Validation method (optional men användbart)
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(InputPath))
            throw new InvalidOperationException("InputPath cannot be null or empty");

        if (AllowedCountries.Count == 0)
            throw new InvalidOperationException("At least one allowed country must be specified");
    }
}