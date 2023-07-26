// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Enumeration of possible types of an Citizenship Status for CALISIR
    /// </summary>
    [Serializable]
    public enum AidApplicationCitizenshipStatus
    {
        /// <summary>
        /// Citizen
        /// </summary>
        Citizen = 1,

        /// <summary>
        /// Non-citizen
        /// </summary>
        NonCitizen = 2,

        /// <summary>
        /// Non-citizen
        /// </summary>
        NotEligible = 3,
    }
}
