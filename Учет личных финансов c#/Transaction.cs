using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Учет_личных_финансов_c_
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } 
        public string Category { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime Date { get; set; }
        public string Currency { get; internal set; }
    }
}
