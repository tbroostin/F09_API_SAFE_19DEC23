/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Enumeration of possible types of Assumed Student marital status
    /// </summary>
    [Serializable]
    public enum AssumedStudentMaritalStatus
    {
        /// <summary>
        /// Single
        /// </summary>
        AssumedSingle = 1,

        /// <summary>
        /// MarriedRemarried
        /// </summary>
        AssumedMarriedRemarried = 2,

    }
}
