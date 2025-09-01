using CsvEtl.Models;

namespace CsvEtl.Services
{
    /// <summary>
    /// Huvudansvar: Orchestrerar hela processen från A till Ö
    /// </summary>
    public interface IEmployeeProcessor
    {
        Task<ProcessingResult> ProcessAsync(string[] args);
    }
}
