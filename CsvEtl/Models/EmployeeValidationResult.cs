using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvEtl.Models
{
    /// <summary>
    /// Result from validation of single unique employee
    /// </summary>
    public class EmployeeValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public EmployeeRaw Employee { get; set; } = null!;

        public string ErrorMessage => string.Join("; ", Errors);
    }
}
