// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Sort and Subtotal Component query filter criteria.
    /// </summary>
    public class SortSubtotalComponentQueryCriteria
    {
        /// <summary>
        /// The subtotal name.
        /// </summary>
        public string SubtotalName { get; set; }

        /// <summary>
        /// Contains BO for budget officer or a GL for an account component/subcomponent.
        /// </summary>
        public string SubtotalType { get; set; }

        /// <summary>
        /// Order of the Component.
        /// </summary>
        public short Order { get; set; }

        /// <summary>
        /// Flag to determine whether subtotalling is also required/displayed for this sort component.
        /// </summary>
        public bool? IsDisplaySubTotal { get; set; }
    }
}