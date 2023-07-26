// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Enumeration of possible types of JudgementIndicator for CALISIR
    /// </summary>
    [Serializable]
    public enum JudgementIndicator
    {
        /// <summary>
        /// AdjustmentProcessed
        /// </summary>
        AdjustmentProcessed = 1,

        /// <summary>
        /// AdjustmentFailed
        /// </summary>
        AdjustmentFailed = 2
    }
}
