// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Box number and its amount for a tax form.
    /// </summary>
    [Serializable]
    public class TaxFormBoxesPdfData
    {
        /// <summary>
        /// Box number in the tax form.
        /// </summary>
        public string BoxNumber { get { return this.boxNumber; } }
        private readonly string boxNumber;

        /// <summary>
        /// Box amount.
        /// </summary>
        public decimal Amount { get { return this.amount; } }
        private decimal amount;

        /// <summary>
        /// Initializes a new instance of the Tax Form Box pdf data.
        /// </summary>
        /// <param name="BoxNumber">A box number.</param>
        /// <param name="Amount">A box amount.</param>
        public TaxFormBoxesPdfData(string boxNumber, decimal amount)
        {
            if (string.IsNullOrEmpty(boxNumber))
            {
                throw new ArgumentNullException("boxNumber", "The box number is required.");
            }

            this.boxNumber = boxNumber;
            this.amount = amount;
        }

        /// <summary>
        /// Add an amount to an existing box entity.
        /// </summary>
        /// <param name="boxAmount">Amount to add.</param>
        public void AddAmount(decimal boxAmount)
        {
            amount += boxAmount;
        }
    }
}