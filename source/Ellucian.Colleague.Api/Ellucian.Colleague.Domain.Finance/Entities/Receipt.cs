﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A Cash Receipt entity
    /// </summary>
    [Serializable]
    public class Receipt
    {
        private string _id;
        /// <summary>
        /// Identifier of the receipt
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    throw new InvalidOperationException("Id is already defined for receipt.");
                }
                _id = value;
            }
        }

        private string _referenceNumber;
        /// <summary>
        /// Receipt reference number, usually generated by Colleague
        /// </summary>
        public string ReferenceNumber
        {
            get { return _referenceNumber; }
            set
            {
                if (!string.IsNullOrEmpty(_referenceNumber))
                {
                    throw new InvalidOperationException("Reference number is already defined for receipt.");
                }
                _referenceNumber = value;
            }
        }

        private readonly string _payerId;
        /// <summary>
        /// Payer of the receipt
        /// </summary>
        public string PayerId { get { return _payerId; } }

        private string _payerName;
        /// <summary>
        /// Name of the receipt payer
        /// </summary>
        public string PayerName
        {
            get { return _payerName; }
            set
            {
                if (!string.IsNullOrEmpty(_payerName))
                {
                    throw new InvalidOperationException("Payer name already defined for receipt.");
                }
                _payerName = value;
            }
        }

        private readonly string _distributionCode;
        /// <summary>
        /// Distribution of the receipt
        /// </summary>
        public string DistributionCode { get { return _distributionCode; } }

        private readonly DateTime _date;
        /// <summary>
        /// Effective date of the receipt
        /// </summary>
        public DateTime Date { get { return _date; } }

        private string _cashierId;
        /// <summary>
        /// Identifier of the cashier issuing the receipt
        /// </summary>
        public string CashierId
        {
            get { return _cashierId; }
            set
            {
                if (!string.IsNullOrEmpty(_cashierId))
                {
                    throw new InvalidOperationException("Cashier already defined for receipt.");
                }
                _cashierId = value;
            }
        }

        private readonly List<string> _depositIds = new List<string>();
        /// <summary>
        /// Deposits deriving from the receipt
        /// </summary>
        public ReadOnlyCollection<string> DepositIds { get; private set; }

        private readonly List<NonCashPayment> _nonCashPayments = new List<NonCashPayment>();
        /// <summary>
        /// Non-cash payments being receipted
        /// </summary>
        public ReadOnlyCollection<NonCashPayment> NonCashPayments { get; private set; }

        /// <summary>
        /// External System
        /// </summary>
        public string ExternalSystem { get; private set; }

        /// <summary>
        /// External System ID
        /// </summary>
        public string ExternalIdentifier { get; private set; }

        /// <summary>
        /// Total amount of all non-cash payments on the receipt
        /// </summary>
        public decimal TotalNonCashPaymentAmount { get { return NonCashPayments.Sum(x => x.Amount); } }

        /// <summary>
        /// Public constructor for a Receipt
        /// </summary>
        /// <param name="id">Identifier of the receipt</param>
        /// <param name="referenceNumber">Reference number of the receipt</param>
        /// <param name="date">The date the receipt was paid</param>
        /// <param name="payerId">Payer of the receipt</param>
        /// <param name="distribution">Distribution of the receipt</param>
        /// <param name="deposits">Deposits deriving from the receipt</param>
        /// <param name="nonCashPayments">Non-cash payments being receipted</param>
        public Receipt(string id, string referenceNumber, DateTime date, string payerId, string distribution, IEnumerable<string> deposits, IEnumerable<NonCashPayment> nonCashPayments)
        {
            if (date == default(DateTime))
        {
                throw new ArgumentOutOfRangeException("date");
            }
            if (string.IsNullOrEmpty(payerId))
            {
                throw new ArgumentNullException("payerId", "Payer Id cannot be null.");
            }
            if (string.IsNullOrEmpty(distribution))
            {
                throw new ArgumentNullException("distribution", "Distribution cannot be null");
            }
            if (!string.IsNullOrEmpty(id) && (deposits == null || !deposits.Any()))
            {
                throw new ArgumentNullException("deposits", "At least one deposit must be included on an existing receipt.");
            }
            if (string.IsNullOrEmpty(id) && deposits != null && deposits.Any())
            {
                throw new ArgumentException("No existing deposits may be included on a new receipt.", "deposits");
            }
            if (nonCashPayments == null || !nonCashPayments.Any())
            {
                throw new ArgumentNullException("nonCashPayments", "At least one payment must be made.");
            }

            _id = id;
            _referenceNumber = referenceNumber;
            _date = date;
            _payerId = payerId;
            _distributionCode = distribution;

            if (deposits != null)
            {
                foreach (var deposit in deposits)
                {
                    AddDeposit(deposit);
                }
            }

            foreach (var payment in nonCashPayments)
            {
                AddNonCashPayment(payment);
            }

            _id = id;
            _date = date;
            _payerId = payerId;
            _distributionCode = distribution;
            DepositIds = _depositIds.AsReadOnly();
            NonCashPayments = _nonCashPayments.AsReadOnly();
        }

        /// <summary>
        /// Link this receipt to a deposit
        /// </summary>
        /// <param name="deposit">Identifier of the deposit to link</param>
        public void AddDeposit(string deposit)
        {
            if (string.IsNullOrEmpty(deposit))
            {
                throw new ArgumentNullException("deposit", "Deposit cannot be null");
            }

            if (!_depositIds.Contains(deposit))
            {
                _depositIds.Add(deposit);
            }
        }

        /// <summary>
        /// Add a non-cash payment to this receipt
        /// </summary>
        /// <param name="payment">The payment to link</param>
        private void AddNonCashPayment(NonCashPayment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException("payment", "Non-cash payment cannot be null");
            }

            // there is no way to distinguish between two non-cash payments.  Even the payment methods
            // and amounts can be the same, such as when splitting a payment across two credit cards.
            _nonCashPayments.Add(payment);
        }

        /// <summary>
        /// Add an external system and external system ID to the receipt
        /// </summary>
        /// <param name="externalSystem">External system</param>
        /// <param name="externalIdentifier">Identifier of external system's deposit</param>
        public void AddExternalSystemAndId(string externalSystem, string externalIdentifier)
        {
            if (!string.IsNullOrEmpty(ExternalSystem))
            {
                throw new InvalidOperationException("External system already defined for receipt.");
            }
            if (!string.IsNullOrEmpty(ExternalIdentifier))
            {
                throw new InvalidOperationException("External identifier already defined for receipt.");
            }
            if (string.IsNullOrEmpty(externalSystem))
            {
                throw new ArgumentNullException("externalSystem", "External System cannot be null.");
            }
            if (string.IsNullOrEmpty(externalIdentifier))
            {
                throw new ArgumentNullException("externalIdentifier", "External Identifier cannot be null.");
            }

            ExternalIdentifier = externalIdentifier;
            ExternalSystem = externalSystem;
        }
    }
}
