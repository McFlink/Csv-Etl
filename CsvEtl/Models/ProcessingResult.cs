namespace CsvEtl.Models;

/// <summary>
/// Returneras från ProcessAsync för att visa vad som hände
/// </summary>
public class ProcessingResult
{
    public int TotalRecords { get; set; }
    public int AcceptedRecords { get; set; }
    public int RejectedRecords { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    public bool Success => ErrorMessages.Count == 0;

    // Hjälpmetod för att skriva ut resultat
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