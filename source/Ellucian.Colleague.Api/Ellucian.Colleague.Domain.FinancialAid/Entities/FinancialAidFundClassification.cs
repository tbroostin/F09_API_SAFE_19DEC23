using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public class FinancialAidFundClassification : GuidCodeItem
    {
        /// <summary>
        /// Funding type code of financial aid fund classifications.
        /// </summary>
        public string FundingTypeCode { get; set; }

        /// <summary>
        /// Description of financial aid fund classifications.
        /// </summary>
        public string Description2 { get; set; }

        /// <summary>
        /// Constructor for FinancialAidFundClassifications
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public FinancialAidFundClassification(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
