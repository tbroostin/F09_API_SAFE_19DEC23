// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// These GL access levels indicate whether GL access needs to be evaluated
    /// when viewing some procurement documents.
    /// </summary>
    [Serializable]
    public enum GlAccessLevel
    {
        /// <summary>
        /// This indicates that the user has full general ledger access.
        /// </summary>
        Full_Access,

        /// <summary>
        /// This indicates that the user may have some general ledger access, and it needs to be evaluated.
        /// </summary>
        Possible_Access,

        /// <summary>
        /// This indicates that the user has no general ledger access.
        /// </summary>
        No_Access,
    }
}
