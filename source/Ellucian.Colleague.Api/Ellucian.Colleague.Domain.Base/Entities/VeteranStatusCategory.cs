// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible Veteran Status Categories
    [Serializable]
    public enum VeteranStatusCategory
    {
        /// <summary>
        /// nonVeteran
        /// </summary>
        Nonveteran,

        /// <summary>
        /// activeDuty
        /// </summary>
        Activeduty,

        /// <summary>
        /// protectedVeteran
        /// </summary>
        Protectedveteran,

        /// <summary>
        /// nonProtectedVeteran
        /// </summary>
        Nonprotectedveteran
    }
}