// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// A single line in a budget adjustment.
    /// </summary>
    [Serializable]
    public class AdjustmentLine
    {
        /// <summary>
        /// GL number from or to which money is being moved.
        /// </summary>
        public string GlNumber { get { return glNumber; } }
        private readonly string glNumber;

        /// <summary>
        /// Amount of money being moved out of the account.
        /// </summary>
        public decimal FromAmount { get { return fromAmount; } }
        private readonly decimal fromAmount;

        /// <summary>
        /// Amount of money being moved into the account.
        /// </summary>
        public decimal ToAmount { get { return toAmount; } }
        private readonly decimal toAmount;

        /// <summary>
        /// Initialize adjustment line.
        /// </summary>
        /// <param name="glAccount">GL account</param>
        public AdjustmentLine(string glAccount, decimal fromAmount, decimal toAmount)
        {
            if (string.IsNullOrEmpty(glAccount))
            {
                throw new ArgumentNullException("glAccount", "glAccount is required");
            }

            // Ensure the line has one and only one non-zero amount.
            if ((fromAmount != 0 && toAmount != 0) || (fromAmount == 0 && toAmount == 0))
            {
                throw new ArgumentException("Adjustment may have only a 'from' or 'to' amount");
            }

            // Disallow negative amounts.
            if (fromAmount < 0 || toAmount < 0)
            {
                throw new ArgumentException("'From' and 'To' amounts must be positive.");
            }

            this.glNumber = glAccount;
            this.fromAmount = fromAmount;
            this.toAmount = toAmount;
        }
    }
}