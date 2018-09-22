using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public class FinancialAidFund : GuidCodeItem
    {
        /// <summary>
        /// Description of financial aid fund classifications.
        /// </summary>
        public string Description2 { get; set; }

        //public FinancialAidFundsSource Source { get; set; }
        public string Source { get; set; }

        //public FinancialAidFundsAidType AidType { get; set; }
        public string AidType { get; set; }

        public string FundingType { get; set; }

        public string CategoryCode { get; set; }

        public string HostCountry { get; set; }

        //public FinancialAidFundsPrivacy Privacy { get; set; }

        //Financials properties
        public List<string> FinancialYearID { get; set; }

        public List<string> FinancialOfficeID { get; set; }

        public List<double> FinancialBudgetedAmountValue { get; set; }

        public List<FinancialCurrencyType> FinancialCurrencyType { get; set; }

        /// <summary>
        /// Constructor for FinancialAidFunds
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public FinancialAidFund(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
