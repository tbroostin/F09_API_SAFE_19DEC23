// Copyright 2014 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// These GL class values determine the type of GL accounts.
    /// </summary>
    [Serializable]
    public enum GlClass
    {
        /// <summary>
        /// This is the expense type GL class
        /// </summary>
        Expense,

        /// <summary>
        /// This is the asset type GL class
        /// </summary>
        Asset,

        /// <summary>
        /// This is the revenue type GL class
        /// </summary>
        Revenue,

        /// <summary>
        /// This is the liability type GL class
        /// </summary>
        Liability,

        /// <summary>
        /// This is the fund balance type GL class
        /// </summary>
        FundBalance
    }
}
