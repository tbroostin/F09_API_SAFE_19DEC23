using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    public class F09PaymentInvoice
    {
        public string StudentId { get; set; }

        public string PaymentMethod { get; set; }

        public string InvoiceId { get; set; }
        public string Distribution { get; set; }
    }
}
