// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is the GL distribution for a line item.
    /// </summary>
    [Serializable]
    public class LineItemGlDistribution : GlAccount
    {
        /// <summary>
        /// Formatted GL account number for display.
        /// </summary>
        public string GetFormattedMaskedGlAccount(IEnumerable<string> majorComponentStartPosition)
        {
            // First, format the GL account number.
            string formattedGlAccount = GetFormattedGlAccount(majorComponentStartPosition);

            // If the user does not have access to the GL number, replace everything except hyphens with pound signs.
            if (Masked)
                formattedGlAccount = new Regex("[^-]").Replace(formattedGlAccount, "#");
            
            return formattedGlAccount;
        }

        /// <summary>
        /// Will this GL number be visible to the client?
        /// </summary>
        public bool Masked { get; set; }

        /// <summary>
        /// Project ID for the line item GL distribution.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// This is the line item GL distribution project number.
        /// </summary>
        public string ProjectNumber { get; set; }

        /// <summary>
        /// This is the line item GL distribution project line item ID.
        /// </summary>
        public string ProjectLineItemId { get; set; }

        /// <summary>
        /// This is the line item GL distribution project line item item code.
        /// </summary>
        public string ProjectLineItemCode { get; set; }

        /// <summary>
        /// Private variable to store the GL quantity.
        /// </summary>
        private readonly decimal quantity;

        /// <summary>
        /// Public getter for the private GL quantity variable.
        /// </summary>
        public decimal Quantity { get { return quantity; } }

        /// <summary>
        /// Private variable to store the GL amount.
        /// </summary>
        private readonly decimal amount;

        /// <summary>
        /// Public getter for the private GL amount variable.
        /// </summary>
        public decimal Amount { get { return amount; } }

        /// <summary>
        /// Private variable to store the GL percent.
        /// </summary>
        private readonly decimal percent;

        /// <summary>
        /// Public getter for the private GL percent variable.
        /// </summary>
        public decimal Percent { get { return percent; } }

        /// <summary>
        /// The purchase order submitted by operator for funds availability checking.
        /// </summary>
        public string SubmittedBy { get; set; }

        /// <summary>
        /// The GL account budget amount.
        /// </summary>
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// The GL account encumbrance amount. 
        /// </summary>
        public decimal EncumbranceAmount { get; set; }

        /// <summary>
        /// The GL account requisition amount. 
        /// </summary>
        public decimal RequisitionAmount { get; set; }

        /// <summary>
        /// The GL account actual amount.
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// This constructor initializes a Line Item general ledger distribution domain entity.
        /// </summary>
        /// <param name="glAccount">This is the line item general ledger distribution general ledger account.</param>
        /// <param name="quantity">This is the line item general ledger distribution quantity.</param>
        /// <param name="amount">This is the line item general ledger distribution amount.</param>
        /// /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public LineItemGlDistribution(string glAccount, decimal quantity, decimal amount)
            : base(glAccount)
        {
            this.quantity = quantity;
            this.amount = amount;
            this.Masked = false;
        }

        /// <summary>
        /// This constructor initializes a Line Item general ledger distribution domain entity.
        /// </summary>
        /// <param name="glAccount">This is the line item general ledger distribution general ledger account.</param>
        /// <param name="quantity">This is the line item general ledger distribution quantity.</param>
        /// <param name="amount">This is the line item general ledger distribution amount.</param>
        /// <param name="percent">This is the line item general ledger distribution percent.</param>
        /// /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public LineItemGlDistribution(string glAccount, decimal quantity, decimal amount, decimal percent)
            : base(glAccount)
        {
            this.quantity = quantity;
            this.amount = amount;
            this.percent = percent;
            this.Masked = false;
        }
    }
}
