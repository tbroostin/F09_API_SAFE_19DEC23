using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Repositories;

namespace Ellucian.Colleague.Dtos.F09
{
    public class F09PaymentFormDto
    {
        public string Instructions { get; set; }
        public string TermsConditions { get; set; }

        public string FinancialAidTerms { get; set; }

        public Dictionary<string, F09PaymentOptionDto> PaymentOptions { get; set; }

        public List<string> UnderstandingStatements { get; set; }
        public Dictionary<string, string> PaymentMethods { get; set; }

        public F09PaymentFormDto()
        {
            PaymentOptions = new Dictionary<string, F09PaymentOptionDto>();
            PaymentMethods = new Dictionary<string, string>();
            UnderstandingStatements = new List<string>();
        }
    }
}
