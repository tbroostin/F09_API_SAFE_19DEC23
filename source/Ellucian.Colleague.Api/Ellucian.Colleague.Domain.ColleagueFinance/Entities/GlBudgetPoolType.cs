// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Indicates whether a GL account is part of a General Ledger Budget pool,
    /// and in what capacity.
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
