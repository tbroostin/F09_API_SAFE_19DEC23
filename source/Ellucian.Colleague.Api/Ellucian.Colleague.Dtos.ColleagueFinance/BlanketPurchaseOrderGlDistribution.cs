// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is the Blanket Purchase Order GL distribution DTO
    /// </summary>
    public class BlanketPurchaseOrderGlDistribution
    {
        /// <summary>
        /// This is the GL account for the GL distribution
        /// </summary>
        public string GlAccount { get; set; }

        /// <summary>
        /// This is the GL account for the GL distribution formatted for display
        /// </summary>
        public string FormattedGlAccount { get; set; }

        /// <summary>
        /// This is the description for the GL distribution
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is the project number for the GL distribution
        /// </summary>
        public string ProjectNumber { get; set; }

        /// <summary>
        /// This is the project line item item code for the GL distribution
        /// </summary>
        public string ProjectLineItemCode { get; set; }

        /// <summary>
        /// This is the GL encumbered amount for the GL distribution
        /// </summary>
        public decimal EncumberedAmount { get; set; }

        /// <summary>
        /// This is the GL expensed amount for the GL distribution
        /// </summary>
        public decimal ExpensedAmount { get; set; }
    }
}
