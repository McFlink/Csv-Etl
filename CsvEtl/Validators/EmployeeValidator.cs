using CsvEtl.Models;
using CsvEtl.Configuration;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace CsvEtl.Validators;

// <summary>
/// Responsibility: ONLY validation of employee data
/// No file I/O, no side effect, just pure validation
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
    /// Validates an employee record and returns all errors found
    /// </summary>
    /// <param name="employee">Employee to validate</param>
    /// <returns>EmployeeValidationResult with errors (empty list = valid)</returns>
    public EmployeeValidationResult Validate(EmployeeRaw employee)
    {
        var errors = new List<string>();

        // Validate fields
        if (string.IsNullOrWhiteSpace(employee.FirstName))
        {
            errors.Add("FirstName saknas");
        }

        if (string.IsNullOrWhiteSpace(employee.LastName))
        {
            errors.Add("LastName saknas");
        }

        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            errors.Add("Email saknas");
        }
        else if (!_emailRegex.IsMatch(employee.Email))
        {
            errors.Add("Email har ogiltigt format");
        }

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
    /// Check if employee should be filtered based on --min-country
    /// </summary>
    /// <param name="employee">Employee to check</param>
    /// <returns>true if filtered away, false if it's a keeper</returns>
    public bool ShouldFilterOut(EmployeeRaw employee)
    {
        if (string.IsNullOrWhiteSpace(_options.MinCountry))
        {
            return false; // No filtering
        }

        return !string.Equals(employee.Country, _options.MinCountry, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Transforms a valid EmployeeRaw to EmployeeValid
    /// </summary>
    /// <param name="raw">The validated raw employee</param>
    /// <returns>Transformed employee</returns>
    public EmployeeValid Transform(EmployeeRaw raw)
    {
        return new EmployeeValid
        {
            Id = raw.Id,
            FirstName = raw.FirstName,
            LastName = raw.LastName,
            Email = raw.Email.ToLowerInvariant(),           // Normalize email
            Country = raw.Country.ToUpperInvariant(),        // Normalize country
            FullName = $"{raw.LastName}, {raw.FirstName}"    // Create fullname
        };
    }
}