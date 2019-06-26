using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    public class F09PaymentForm
    {
        public string Instructions { get; set; }
        public string TermsConditions { get; set; }

        public string FinancialAidTerms { get; set; }

        public Dictionary<string, string> PaymentOptions { get; set; }

    }
}
