using CsvEtl.Models;
using System.ComponentModel.DataAnnotations;

namespace CsvEtl.Services
{
    /// <summary>
    /// Responsibility: Everything related to csv - read, write, configure
    /// </summary>
    public interface ICsvService
    {
        Task<IEnumerable<EmployeeRaw>> ReadEmployeesAsync(string filePath);
        Task WriteEmployeeAsync(string filePath, EmployeeValid employee);
        Task WriteJsonLineAsync(string filePath, EmployeeValid employee);
        Task WriteHeaderAsync(string filePath);
        Task WriteErrorLogAsync(string filePath, int rowNumber, EmployeeValidationResult validationResult);
    }
}
