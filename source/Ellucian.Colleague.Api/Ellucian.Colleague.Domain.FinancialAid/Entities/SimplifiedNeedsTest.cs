/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Enumeration of possible types of Simplified needs test
    /// </summary>
    [Serializable]
    public enum SimplifiedNeedsTest
    {
        /// <summary>
        /// Met
        /// </summary>
        SntMet = 1,

        /// <summary>
        /// Not met
        /// </summary>
        SntNotMet = 2,

    }
}