using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// 
    /// </summary>
    public class LineItemReqTax
    {
        /// <summary>
        /// This is the line item tax code.
        /// </summary>
        public string TaxReqTaxCode { get; set; }

        /// <summary>
        /// This is the line item tax code GL number.
        /// </summary>
        public string TaxReqTaxCodeDescription { get; set; }
    }
}
