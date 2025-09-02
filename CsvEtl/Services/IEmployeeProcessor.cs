using CsvEtl.Models;

namespace CsvEtl.Services
{
    /// <summary>
    /// Main responsibility: Orchesters whole process from A to Z
    /// </summary>
    public interface IEmployeeProcessor
    {
        Task<ProcessingResult> ProcessAsync(string[] args);
    }
}
