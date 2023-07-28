/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Enumeration of possible types of Assumed Parent marital status
    /// </summary>
    [Serializable]
    public enum AssumedParentMaritalStatus
    {
        /// <summary>
        /// MarriedRemarried
        /// </summary>
        AssumedMarriedRemarried = 1,

        /// <summary>
        /// Single
        /// </summary>
        AssumedSingle = 2,

    }
}