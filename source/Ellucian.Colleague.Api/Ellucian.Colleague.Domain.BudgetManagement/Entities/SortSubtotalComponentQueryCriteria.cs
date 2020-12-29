// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Sort and Subtotal Component query filter criteria.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Constructor that initializes a sort/subtotal component query criteria domain entity.
        /// </summary>
        /// <param name="componentName">Name of the component to sort/subtotal.</param>
        public SortSubtotalComponentQueryCriteria()
        {
            this.IsDisplaySubTotal = false;
        }
    }
}
