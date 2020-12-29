// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A Finance query gl account line item DTO.
    /// </summary>
    public class FinanceQueryGlAccountLineItem
    {   
        /// <summary>
        /// primary gl account.
        /// </summary>
        public FinanceQueryGlAccount GlAccount { get; set; }

        /// <summary>
        /// Whether the umbrella GL account would be visible to the user
        /// given their GL account security setup.
        /// </summary>
        public bool IsUmbrellaVisible { get; set; }

        /// <summary>
        /// whether gl account is umbrella or non-pooled account
        /// given their GL account security setup.
        /// </summary>
        public bool IsUmbrellaAccount { get; set; }

        /// <summary>
        /// List of poolee gl accounts which fall under umbrella gl account.
        /// </summary>
        public List<FinanceQueryGlAccount> Poolees { get; set; }

    }
}


