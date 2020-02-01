// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Contains configuration information for each of comparables used in generating the budget.
    /// </summary>
    public class BudgetConfigurationComparable
    {
        /// <summary>
        /// The budget comparable order.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// The budget comparable ID.
        /// </summary>
        public string ComparableId { get; set; }

        /// <summary>
        /// The budget comparable year.
        /// </summary>
        public string ComparableYear { get; set; }

        /// <summary>
        /// The budget comparable header.
        /// </summary>
        public string ComparableHeader { get; set; }
    }
}

