using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvEtl.Models
{
    public record EmployeeValid : EmployeeRaw
    {
        public string FullName { get; init; } = string.Empty;
    }
}
