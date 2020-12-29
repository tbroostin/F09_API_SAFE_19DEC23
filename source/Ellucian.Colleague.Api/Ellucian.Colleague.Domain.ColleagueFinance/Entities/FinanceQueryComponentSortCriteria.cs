// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Finance Query Component Sort Criteria
    /// </summary>
    [Serializable]
    public class FinanceQueryComponentSortCriteria
    {
        /// <summary>
        /// Component Name
        /// </summary>
        public string ComponentName { get { return componentName; } }
        private readonly string componentName;
        /// <summary>
        /// Order of the Component
        /// </summary>
        public short Order { get; set; }
        /// <summary>
        /// Flag to determine whether subtotalling is also required/displayed for this sort component
        /// </summary>
        public bool? IsDisplaySubTotal { get; set; }


        /// <summary>
        /// Constructor that initializes a finance query component sort criteria domain entity.
        /// </summary>
        /// <param name="componentName">Name of the finance query component sort criteria.</param>
        /// <param name="order">Order of the finance query component in sort criteria.</param>
        public FinanceQueryComponentSortCriteria(string componentName, short order, bool? isDisplaySubTotal=false)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                throw new ArgumentNullException("componentName", "Component name is a required field.");
            }
            //if (order==0)
            //{
            //    throw new ArgumentNullException("order", "Order is a required field.");
            //}
            this.componentName = componentName;
            this.Order = order;
            this.IsDisplaySubTotal = isDisplaySubTotal;
        }
    }
}
