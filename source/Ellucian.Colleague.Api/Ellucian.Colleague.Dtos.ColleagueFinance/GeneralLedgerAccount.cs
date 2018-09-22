// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Describes a GL account.
    /// </summary>
    public class GeneralLedgerAccount
    {
        /// <summary>
        /// GL number (internal format).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Description of the GL number.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Formatted GL number.
        /// </summary>
        public string FormattedId { get; set; }
    }
}