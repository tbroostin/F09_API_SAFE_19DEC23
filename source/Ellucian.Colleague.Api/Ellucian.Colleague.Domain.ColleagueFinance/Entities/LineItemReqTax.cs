using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a line item for an Accounts Payable or Purchasing document.
    /// </summary>
    [Serializable]
    public class LineItemReqTax
    {
        /// <summary>
        /// Private variable to store the tax code.
        /// </summary>
        private readonly string taxCode;

        /// <summary>
        /// Private variable to store the tax code.
        /// </summary>
        private readonly string taxCodeDescription;
        /// <summary>
        /// This is the line item tax code GL number.
        /// </summary>
        public string TaxReqTaxCode { get; set; }

        /// <summary>
        /// This is the line item tax code GL number.
        /// </summary>
        public string TaxReqTaxCodeDescription { get; set; }

        /// This constructor initializes a line item tax codes domain entity.
        /// </summary>
        /// <param name="taxCode">This is the line item tax code.</param>
        /// <param name="taxAmount">This is the line item tax amount.</param>
        /// /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public LineItemReqTax(string taxCode)
        {
            if (string.IsNullOrEmpty(taxCode))
            {
                throw new ArgumentNullException("taxCode", "Tax code is a required field");
            }
            this.TaxReqTaxCode = taxCode;
            
        }
    }
}
