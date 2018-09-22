// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// GL activity query input criteria.
    /// </summary>
    public class GlActivityDetailQueryCriteria
    {
        /// <summary>
        /// List of IDs. It is not used because we have to select records for a GL account.
        /// </summary>
        public List<string> Ids { get; set; }

        /// <summary>
        /// The GL account for which we want to obtain activity detail. Required
        /// </summary>
        public string GlAccount { get; set; }

        /// <summary>
        /// The fiscal year for the activity detail. Required
        /// </summary>
        public string FiscalYear { get; set; }
    }
}
