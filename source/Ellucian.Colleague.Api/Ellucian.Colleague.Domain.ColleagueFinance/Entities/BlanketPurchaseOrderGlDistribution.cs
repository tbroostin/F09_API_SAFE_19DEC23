// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is the GL distribution for a blanket purchase order
    /// </summary>
    [Serializable]
    public class BlanketPurchaseOrderGlDistribution : GlAccount
    {
        /// <summary>
        /// Project ID for the blanket purchase order GL distribution
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// This is the blanket purchase order GL distribution project number
        /// </summary>
        public string ProjectNumber { get; set; }

        /// <summary>
        /// This is the blanket purchase order GL distribution project line item ID
        /// </summary>
        public string ProjectLineItemId { get; set; }

        /// <summary>
        /// This is the blanket purchase order GL distribution project line item item code
        /// </summary>
        public string ProjectLineItemCode { get; set; }

        /// <summary>
        /// Private variable for the blanket purchase order GL distribution encumbered amount
        /// </summary>
        private readonly decimal encumberedAmount;

        /// <summary>
        /// Public getter for the blanket purchase order general ledger distribution encumbered amount
        /// </summary>
        public decimal EncumberedAmount { get { return encumberedAmount; } }

        /// <summary>
        /// This is the blanket purchase order general ledger distribution expensed amount
        /// </summary>
        public decimal ExpensedAmount { get; set; }

        /// <summary>
        /// The percentage of the line item amount allocated to the accounting string.
        /// </summary>
        public decimal Percentage { get; set; }

        /// <summary>
        /// This constructor initializes a blanket purchase order general ledger distribution domain entity
        /// </summary>
        /// <param name="glAccount">This is the blanket purchase order general ledger distribution general ledger account</param>
        /// <param name="encumberedAmount">This is the blanket purchase order general ledger distribution encumbered amount</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null</exception>
        public BlanketPurchaseOrderGlDistribution(string glAccount, decimal encumberedAmount)
            : base(glAccount)
        {
            this.encumberedAmount = encumberedAmount;
        }
    }
}
