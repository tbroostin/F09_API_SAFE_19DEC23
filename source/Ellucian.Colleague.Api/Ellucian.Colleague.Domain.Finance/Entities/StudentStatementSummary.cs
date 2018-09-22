// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Summary portion of a student statement
    /// </summary>
    [Serializable]
    public class StudentStatementSummary
    {
        private readonly List<ActivityTermItem> _chargeInformation = new List<ActivityTermItem>();
        private readonly List<ActivityTermItem> _nonChargeInformation = new List<ActivityTermItem>();
        private readonly decimal _paymentPlanAdjustmentsAmount;
        private readonly decimal _currentDepositsDueAmount;

        /// <summary>
        /// Collection of accounts receivable charge group descriptions and their overall group type balances
        /// </summary>
        public IEnumerable<ActivityTermItem> ChargeInformation { get { return _chargeInformation; } }

        /// <summary>
        /// Collection of accounts receivable non-charge activity descriptions, consolidated by activity type, 
        /// and their overall activity type balances
        /// </summary>
        public IEnumerable<ActivityTermItem> NonChargeInformation { get { return _nonChargeInformation; } }

        /// <summary>
        /// Total amount of any payment plan adjustments, equal to the amounts of all scheduled payments not currently due 
        /// in the statement term or period. Overdue scheduled payments and the next unpaid scheduled payment on each plan are currently due.
        /// </summary>
        public decimal PaymentPlanAdjustmentsAmount { get { return _paymentPlanAdjustmentsAmount; } }

        /// <summary>
        /// Total amount due for all deposits due in the statement term or period
        /// </summary>
        public decimal CurrentDepositsDueAmount { get { return _currentDepositsDueAmount; } }

        /// <summary>
        /// Range of dates for the summary portion of the statement
        /// </summary>
        public string SummaryDateRange { get; set; }

        /// <summary>
        /// Description of the term or period for a statement
        /// </summary>
        public string TimeframeDescription { get; set; }

        /// <summary>
        /// Constructor for StudentStatementSummary
        /// </summary>
        /// <param name="chargeInformation">Collection of accounts receivable charge group items</param>
        /// <param name="nonChargeInformation">Collection of accounts receivable non-charge activity items</param>
        /// <param name="paymentPlanAdjustmentsAmount">Total amount of any payment plan adjustments, equal to the amounts of all scheduled payments not currently due 
        /// in the statement term or period. Overdue scheduled payments and the next unpaid scheduled payment on each plan are currently due.</param>
        /// <param name="currentDepositsDueAmount">Total amount due for all deposits due in the statement term or period</param>
        public StudentStatementSummary(IEnumerable<ActivityTermItem> chargeInformation,
            IEnumerable<ActivityTermItem> nonChargeInformation, decimal paymentPlanAdjustmentsAmount, decimal currentDepositsDueAmount)
        {
            if (chargeInformation == null)
            {
                throw new ArgumentNullException("chargeInformation", "Charge information cannot be null.");
            }
            if (nonChargeInformation == null)
            {
                throw new ArgumentNullException("nonChargeInformation", "Non-Charge information cannot be null.");
            }

            _chargeInformation.AddRange(chargeInformation);
            if (_chargeInformation.Count == 0)
            {
                _chargeInformation.Add(new ActivityTermItem());
            }
            _nonChargeInformation.AddRange(nonChargeInformation);
            if (_nonChargeInformation.Count == 0)
            {
                _nonChargeInformation.Add(new ActivityTermItem());
            }
            _paymentPlanAdjustmentsAmount = paymentPlanAdjustmentsAmount;
            _currentDepositsDueAmount = currentDepositsDueAmount;
            SummaryDateRange = string.Empty;
        }
    }
}
