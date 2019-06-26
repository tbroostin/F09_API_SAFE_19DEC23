using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    public class F09PaymentInvoice
    {
        public string Term { get; set; }
        public string StudentId { get; set; }
        public string ArCode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Distribution { get; set; }
        public string ArType { get; set; }
        public string Mnemonic { get; set; }

        public string PaymentMethod { get; set; }
    }
}
