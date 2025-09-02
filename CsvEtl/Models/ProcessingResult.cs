namespace CsvEtl.Models;

/// <summary>
/// Return from ProcessAsync to show outcome
/// </summary>
public class ProcessingResult
{
    public int TotalRecords { get; set; }
    public int AcceptedRecords { get; set; }
    public int RejectedRecords { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    public bool Success => ErrorMessages.Count == 0;

    // Helper to print results
    public void PrintSummary()
    {
        Console.WriteLine($"Klar. Totalt lästa: {TotalRecords}, godkända: {AcceptedRecords}, avvisade: {RejectedRecords}");

        if (!Success)
        {
            Console.WriteLine("Fel uppstod:");
            foreach (var error in ErrorMessages)
            {
                Console.WriteLine($"  - {error}");
            }
        }
    }
}