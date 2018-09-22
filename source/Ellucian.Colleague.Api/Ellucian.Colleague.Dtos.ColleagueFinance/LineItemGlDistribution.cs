// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is the line item GL distribution DTO.
    /// </summary>
    public class LineItemGlDistribution
    {
        /// <summary>
        /// This is the GL account for the line item GL distribution.
        /// </summary>
        public string GlAccount { get; set; }

        /// <summary>
        /// This is the GL account for the line item GL distribution formatted for display.
        /// </summary>
        public string FormattedGlAccount { get; set; }

        /// <summary>
        /// This is the project number for the line item GL distribution.
        /// </summary>
        public string ProjectNumber { get; set; }

        /// <summary>
        /// This is the project line item item code for the line item GL distribution.
        /// </summary>
        public string ProjectLineItemCode { get; set; }

        /// <summary>
        /// This is the GL quantity for the line item GL distribution.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// This is the GL amount for the line item GL distribution.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
