using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class F09PaymentFormDto
    {
        public string Instructions { get; set; }
        public string TermsConditions { get; set; }

        public string FinancialAidTerms { get; set; }

        public Dictionary<string, string> PaymentOptions { get; }
    }
}
