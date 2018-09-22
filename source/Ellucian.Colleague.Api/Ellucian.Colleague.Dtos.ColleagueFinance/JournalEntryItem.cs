// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A line item for a journal entry
    /// </summary>
    public class JournalEntryItem
    {
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The GL account - unformatted - for this journal entry item
        /// </summary>
        public string GlAccount { get; set; }

        /// <summary>
        /// The GL account - formatted for display - for this journal entry item
        /// </summary>
        public string FormattedGlAccount { get; set; }

        /// <summary>
        /// Description for the GL Account
        /// </summary>
        public string GlAccountDescription { get; set; }

        /// <summary>
        /// The project number for this journal entry item
        /// </summary>
        public string ProjectNumber { get; set; }

        /// <summary>
        /// The project line item item code for this journal entry item
        /// </summary>
        public string ProjectLineItemCode { get; set; }

        /// <summary>
        /// The debit for this journal entry item
        /// </summary>
        public decimal? Debit { get; set; }

        /// <summary>
        /// The debit for this journal entry item
        /// </summary>
        public decimal? Credit { get; set; }

    }
}
