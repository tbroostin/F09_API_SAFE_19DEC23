/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Enumeration of possible types of Assumed citizenship status
    /// </summary>
    [Serializable]
    public enum AssumedCitizenshipStatus
    {
        /// <summary>
        /// Citizen
        /// </summary>
        AssumedCitizen = 1,

        /// <summary>
        /// Non-citizen
        /// </summary>
        AssumedEligibleNoncitizen = 2,

    }
}
