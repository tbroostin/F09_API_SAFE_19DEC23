// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Filter criteria for the working budget.
    /// </summary>
    [Serializable]
    public class WorkingBudgetQueryCriteria
    {
        /// <summary>
        /// Start position of the line items to return.
        /// </summary>
        public int StartLineItem { get; set; }

        /// <summary>
        /// Total number of budget line items in the working budget.
        /// </summary>
        public int LineItemCount { get; set; }

        /// <summary>
        /// List of budget officer IDs to filter the results.
        /// </summary>
        public List<string> BudgetOfficerIds { get; set; }

        /// <summary>
        /// List of budget reporting unit IDs to filter the results.
        /// </summary>
        public List<string> BudgetReportingUnitIds { get; set; }

        /// <summary>
        /// Whether to also include any descendants for the selected reporting units.
        /// </summary>
        public bool IncludeBudgetReportingUnitsChildren { get; set; }

        /// <summary>
        /// A list of component filter criteria.
        /// </summary>
        public IEnumerable<ComponentQueryCriteria> ComponentCriteria { get; set; }

        /// <summary>
        /// A list of sort/subtotal component filter criteria.
        /// </summary>
        public IEnumerable<SortSubtotalComponentQueryCriteria> SortSubtotalComponentQueryCriteria { get; set; }

        /// <summary>
        /// Constructor that initializes a cost center query criteria domain entity.
        /// </summary>
        /// <param name="componentCriteria">IEnumerable component query criteria.</param>
        public WorkingBudgetQueryCriteria(IEnumerable<ComponentQueryCriteria> componentCriteria)
        {
            if (componentCriteria == null)
            {
                throw new ArgumentNullException("componentCriteria");
            }
            ComponentCriteria = new List<ComponentQueryCriteria>(componentCriteria);
            BudgetOfficerIds = new List<string>();
            BudgetReportingUnitIds = new List<string>();
            SortSubtotalComponentQueryCriteria = new List<SortSubtotalComponentQueryCriteria>();
        }
    }
}
