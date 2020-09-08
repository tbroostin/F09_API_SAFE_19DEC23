// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is the tax code information for a line item.
    /// </summary>
    [Serializable]
    public class LineItemTax
    {
        /// <summary>
        /// Private variable to store the tax code.
        /// </summary>
        private readonly string taxCode;

        /// <summary>
        /// Public getter for the private tax code variable.
        /// </summary>
        public string TaxCode { get { return taxCode;} }

        /// <summary>
        /// This is the line item tax code amount.
        /// </summary>
        public decimal? TaxAmount { get; set; }

        /// <summary>
        /// This is the line item tax code GL number.
        /// </summary>
        public string TaxGlNumber { get; set; }

         /// <summary>
        /// GL line number
        /// </summary>
        public string LineGlNumber { get; set; }


        /// <summary>
        /// This constructor initializes a line item tax codes domain entity.
        /// </summary>
        /// <param name="taxCode">This is the line item tax code.</param>
        /// <param name="taxAmount">This is the line item tax amount.</param>
        /// /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public LineItemTax(string taxCode, decimal? taxAmount)
        {
            if (string.IsNullOrEmpty(taxCode))
            {
                throw new ArgumentNullException("taxCode", "Tax code is a required field");
            }
            this.taxCode = taxCode;
            this.TaxAmount = taxAmount;
        }
    }
}
