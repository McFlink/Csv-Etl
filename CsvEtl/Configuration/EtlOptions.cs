namespace CsvEtl.Configuration;

/// <summary>
/// Konfiguration för ETL-processen
/// Följer best practices för namngivning och datatyper
/// </summary>
public class EtlOptions
{
    // Default paths (overwritten by config)
    public string InputPath { get; set; } = Path.Combine("input", "employees.csv");
    public string OutputCsvPath { get; set; } = Path.Combine("output", "employees_valid.csv");
    public string OutputJsonl { get; set; } = Path.Combine("output", "employees_valid.jsonl");
    public string ErrorLogsPath { get; set; } = Path.Combine("logs", "errors.txt");

    // HashSet for lookups + case-insensitive comparison
    public HashSet<string> AllowedCountries { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "SE", "NO", "DK", "FI"
    };

    // Command line parameter (set at runtime)
    public string? MinCountry { get; set; }

    // Function flags
    public bool RequireTopLevelDomainInEmail { get; set; } = true;
    public bool TrimFields { get; set; } = true;
    public bool IgnoreBadData { get; set; } = true;

    // Validation method (optional but useful)
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(InputPath))
            throw new InvalidOperationException("InputPath cannot be null or empty");

        if (AllowedCountries.Count == 0)
            throw new InvalidOperationException("At least one allowed country must be specified");
    }
}