using CsvEtl.Models;
using CsvEtl.Configuration;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace CsvEtl.Validators;

// <summary>
/// Ansvar: BARA validering av employee-data
/// Ingen I/O, inga side effects - bara ren validering
/// </summary>
public class EmployeeValidator
{
    private readonly EtlOptions _options;
    private readonly Regex _emailRegex;

    public EmployeeValidator(IOptions<EtlOptions> options)
    {
        _options = options.Value;
        _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    }

    /// <summary>
    /// Validerar en employee-record och returnerar alla fel som hittas
    /// </summary>
    /// <param name="employee">Employee att validera</param>
    /// <returns>EmployeeValidationResult med fel (tom lista = giltig)</returns>
    public EmployeeValidationResult Validate(EmployeeRaw employee)
    {
        var errors = new List<string>();

        // Validera FirstName
        if (string.IsNullOrWhiteSpace(employee.FirstName))
        {
            errors.Add("FirstName saknas");
        }

        // Validera LastName
        if (string.IsNullOrWhiteSpace(employee.LastName))
        {
            errors.Add("LastName saknas");
        }

        // Validera Email
        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            errors.Add("Email saknas");
        }
        else if (!_emailRegex.IsMatch(employee.Email))
        {
            errors.Add("Email har ogiltigt format");
        }

        // Validera Country
        if (string.IsNullOrWhiteSpace(employee.Country))
        {
            errors.Add("Country saknas");
        }
        else if (!_options.AllowedCountries.Contains(employee.Country))
        {
            errors.Add($"Ogiltigt land '{employee.Country}' (tillåtna: {string.Join(", ", _options.AllowedCountries)})");
        }

        return new EmployeeValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Employee = employee
        };
    }

    /// <summary>
    /// Kontrollerar om employee ska filtreras bort baserat på --min-country
    /// </summary>
    /// <param name="employee">Employee att kontrollera</param>
    /// <returns>true om den ska filtreras bort, false om den ska behållas</returns>
    public bool ShouldFilterOut(EmployeeRaw employee)
    {
        if (string.IsNullOrWhiteSpace(_options.MinCountry))
        {
            return false; // Ingen filtrering
        }

        return !string.Equals(employee.Country, _options.MinCountry, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Transformerar en giltig EmployeeRaw till EmployeeValid
    /// </summary>
    /// <param name="raw">Den validerade raw employee</param>
    /// <returns>Transformerad employee</returns>
    public EmployeeValid Transform(EmployeeRaw raw)
    {
        return new EmployeeValid
        {
            Id = raw.Id,
            FirstName = raw.FirstName,
            LastName = raw.LastName,
            Email = raw.Email.ToLowerInvariant(),           // Normalisera email
            Country = raw.Country.ToUpperInvariant(),        // Normalisera land
            FullName = $"{raw.LastName}, {raw.FirstName}"    // Skapa fullname
        };
    }
}