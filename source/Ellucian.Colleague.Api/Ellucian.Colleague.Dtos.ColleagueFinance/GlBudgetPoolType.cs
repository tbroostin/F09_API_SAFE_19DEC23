// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This enumeration contains the available types of a GL with regards to a budget pool.
    /// </summary>
    [Serializable]
    public enum GlBudgetPoolType
    {
        /// <summary>
        /// Not part part of a General Ledger Budget pool
        /// </summary>
        None,

        /// <summary>
        /// Part of a General Ledger Budget pool as a polee.
        /// </summary>
        Poolee,

        /// <summary>
        /// Part of a General Ledger Budget pool as an umbrella.
        /// </summary>
        Umbrella
    }
}
