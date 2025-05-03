using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Учет_личных_финансов_c_
{
    internal class BudgetDisplay
    {
        public string Category { get; set; }
        public decimal Limit { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
    }
}
