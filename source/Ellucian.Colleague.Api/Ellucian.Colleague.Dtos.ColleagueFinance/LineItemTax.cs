// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Line item tax DTO.
    /// </summary>
    public class LineItemTax
    {
        /// <summary>
        /// This is the line item tax code.
        /// </summary>
        public string TaxCode { get; set; }

        /// <summary>
        /// This is the line item tax amount.
        /// </summary>
        public decimal TaxAmount { get; set; }
    }
}
