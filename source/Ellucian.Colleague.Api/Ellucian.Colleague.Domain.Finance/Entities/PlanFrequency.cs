// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Indicates the frequency by which a payment plan's scheduled payments are calculated
    /// </summary>
    [Serializable]
    public enum PlanFrequency
    {
        /// <summary>
        /// Weekly - scheduled payment due every 7 days / 1 week
        /// </summary>
        Weekly,

        /// <summary>
        /// Biweekly - scheduled payment due every 14 days / 2 weeks
        /// </summary>
        Biweekly,

        /// <summary>
        /// Monthly - scheduled payment due once per month
        /// </summary>
        Monthly,

        /// <summary>
        /// Yearly - scheduled payment due once per year
        /// </summary>
        Yearly,

        /// <summary>
        /// Custom - payment plan schedule determined by a custom frequency subroutine
        /// </summary>
        Custom
    }
}
