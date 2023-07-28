/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Enumeration of possible types of EFC Change flag
    /// </summary>
    [Serializable]
    public enum EfcChangeFlag
    {
        /// <summary>
        /// Increase
        /// </summary>
        EfcIncrease = 1,

        /// <summary>
        /// Decrease
        /// </summary>
        EfcDecrease = 2,

    }
}