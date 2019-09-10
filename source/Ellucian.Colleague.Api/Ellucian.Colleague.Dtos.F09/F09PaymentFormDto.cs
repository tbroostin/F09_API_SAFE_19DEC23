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

        public Dictionary<string, string> PaymentOptions { get; set; }

        public List<string> UnderstandingStatements { get; set; }
        public Dictionary<string, string> PaymentMethods { get; set; }

        public string PaymentOptionSelected { get; set; }

        public F09PaymentFormDto()
        {
            PaymentOptions = new Dictionary<string, string>();
            PaymentMethods = new Dictionary<string, string>();
            UnderstandingStatements = new List<string>();
        }
    }
}
