using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvEtl.Models
{
    /// <summary>
    /// Resultat från validering av en enskild employee
    /// Döpt till EmployeeValidationResult för att undvika naming conflicts
    /// </summary>
    public class EmployeeValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public EmployeeRaw Employee { get; set; } = null!;

        public string ErrorMessage => string.Join("; ", Errors);
    }
}
