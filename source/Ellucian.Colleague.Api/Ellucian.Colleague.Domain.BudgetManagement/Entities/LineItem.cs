// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Contains either a Budget line item or a subtotal but not both. 
    /// </summary>
    [Serializable]
    public class LineItem
    {
        /// <summary>
        /// The sequence number for this line item within the working budget.
        /// It goes from 1 to the number of returned items which will be the 
        /// number of requested budget line items for the page plus, if the user
        /// requested any subtotals, any number of subtotals to be displayed on the page. 
        /// </summary>
        public int SequenceNumber { get { return sequenceNumber; } }
        private int sequenceNumber { get; set; }

        /// <summary>
        /// A budget line item for the working budget.
        /// </summary>
        public BudgetLineItem BudgetLineItem { get; set; }

        /// <summary>
        /// A subtotal line item for the working budget.
        /// </summary>
        public SubtotalLineItem SubtotalLineItem { get; set; }

        /// <summary>
        /// Constructor that initializes a working budget entitys
        /// </summary>
        public LineItem(int sequenceNumber)
        {
            this.sequenceNumber = sequenceNumber;
        }
    }
}
