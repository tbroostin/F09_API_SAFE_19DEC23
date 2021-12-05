// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is the project line item GL Account.
    /// (this class is moved to Colleague Finance module from Projects Accounting module)
    /// </summary>
    public class ProjectLineItemGlAccount
    {
        /// <summary>
        /// This is the GL account for the project line item
        /// </summary>
        public string GlAccount { get; set; }

        /// <summary>
        /// This is the GL account for the project line item formatted for display.
        /// </summary>
        public string FormattedGlAccount { get; set; }

        /// <summary>
        /// This the GL account description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The total actuals amount for this GL account
        /// </summary>
        public decimal Actuals { get; set; }

        /// <summary>
        /// The total encumbrances amount for this GL account
        /// </summary>
        public decimal Encumbrances { get; set; }

        /// <summary>
        /// List of encumbrance and requisition transactions for this GL Account
        /// </summary>
        public List<GlTransaction> EncumbranceGlTransactions { get; set; }

        /// <summary>
        /// List of actuals transactions for this GL Account
        /// </summary>
        public List<GlTransaction> ActualsGlTransactions { get; set; }

        /// <summary>
        /// GL Account active flag.
        /// </summary>
        public bool IsGlActive { get; set; }
    }
}
