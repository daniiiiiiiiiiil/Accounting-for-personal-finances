using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Учет_личных_финансов_c_
{
    public class RecurringPayment
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string Frequency { get; set; }
        public DateTime NextPaymentDate { get; set; }
    }
}
