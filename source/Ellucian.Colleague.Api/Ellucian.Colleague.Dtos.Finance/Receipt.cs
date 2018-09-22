// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A receipt
    /// </summary>
    public class Receipt
    {
        /// <summary>
        /// Identifier of the receipt
        /// </summary>
        public string Id {get; set;}

        /// <summary>
        /// Receipt Number
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Payer of the receipt
        /// </summary>
        public string PayerId { get; set;}

        /// <summary>
        /// Name of the receipt payer
        /// </summary>
        public string PayerName { get; set; }

        /// <summary>
        /// Distribution of the receipt
        /// </summary>
        public string DistributionCode { get; set;}

        /// <summary>
        /// Effective date of the receipt
        /// </summary>
        public DateTime Date { get ; set;}

        /// <summary>
        /// Cashier issuing the receipt
        /// </summary>
        public string CashierId {get; set;}

        /// <summary>
        /// Deposits deriving from the receipt
        /// </summary>
        public List<string> DepositIds { get; set; }

        /// <summary>
        /// Non-cash payments being receipted
        /// </summary>
        public List<NonCashPayment> NonCashPayments { get; set; }

        /// <summary>
        /// External System
        /// </summary>
        public string ExternalSystem { get; set; }

        /// <summary>
        /// External System ID
        /// </summary>
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Total amount of all non-cash payments on the receipt
        /// </summary>
        public decimal TotalNonCashPaymentAmount { get; set;}
    }
}
