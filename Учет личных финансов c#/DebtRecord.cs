using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Учет_личных_финансов_c_
{
    public class DebtRecord
    {
        public int Id { get; set; }
        public string Person { get; set; }
        public decimal Amount { get; set; }
        public bool IsOwedToMe { get; set; } // true - мне должны, false - я должен
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
        public bool IsSettled { get; internal set; }
    }
}
