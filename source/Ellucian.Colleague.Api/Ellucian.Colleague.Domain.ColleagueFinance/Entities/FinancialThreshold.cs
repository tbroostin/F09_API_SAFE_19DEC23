// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// The available categories for Financial Health Indicators.
    /// </summary>
    [Serializable]
    public enum FinancialThreshold
    {
        /// <summary>
        /// Financial amounts are under the specified threshold.
        /// </summary>
        UnderThreshold,

        /// <summary>
        /// Financial amounts are near the specified threshold.
        /// </summary>
        NearThreshold,

        /// <summary>
        /// Financial amounts are over the specified threshold.
        /// </summary>
        OverThreshold
    }
}