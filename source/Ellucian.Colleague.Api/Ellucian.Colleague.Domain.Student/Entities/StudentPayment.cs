// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// This is a transaction for payments submitted through the Data Model.
    /// </summary>
    [Serializable]
    public class StudentPayment
    {
        /// <summary>
        /// Unique identifier (GUID) for the payment transaction.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Person ID for this payment.
        /// </summary>
        public string PersonId { get; private set; }

        /// <summary>
        /// AR Type Code used in this payment.
        /// </summary>
        public string AccountsReceivableTypeCode { get; set; }

        /// <summary>
        /// The AR Code to use for this payment.
        /// </summary>
        public string AccountsReceivableCode { get; set; }

        /// <summary>
        /// The Distribution Method used to store in Cash receipts
        /// </summary>
        public string DistributionCode { get; set; }

        /// <summary>
        /// The Academic Term to associate the payment to.
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// The payment type.  One of, "tuition", "fee", "housing", or "meal".
        /// </summary>
        public string PaymentType { get; private set; }

        /// <summary>
        /// The Due Date or the date that this is paymentd on.
        /// </summary>
        public DateTime PaymentDate { get; private set; }

        /// <summary>
        /// Comments to be recorded with the payment.
        /// </summary>
        public List<string> Comments { get; set; }

        /// <summary>
        /// Paymentd Amount (exclusive of Quantity).
        /// </summary>
        public decimal? PaymentAmount { get; set; }

        /// <summary>
        /// Payment Currency (only USD and CAD are supported).
        /// </summary>
        public string PaymentCurrency { get; set; }

        /// <summary>
        /// Once posted to Misc. Payments, this is the key to AR.PAYMENTS.
        /// </summary>
        public string PaymentID { get; set; }

        /// <summary>
        /// Indicate if student payment came from elevate.
        /// </summary>
        public bool ChargeFromElevate { get; set; }

        /// <summary>
        /// The usage associated with the charge (i.e. tax reporting only)
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// The date the charge orginated for sonsideration in tax report generation.
        /// </summary>
        public DateTime? OriginatedOn { get; set; }

        /// <summary>
        /// The override description associated with the charge.
        /// </summary>
        public string OverrideDescription { get; set; }


        public string RecordKey { get; set; }

        /// <summary>
        /// Constructor initializes the StudentPayments transaction object.
        /// </summary>
        public StudentPayment(string personId, string paymentType, DateTime? paymentDate)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID is required when creating a StudentPayment.");
            }
            if (string.IsNullOrEmpty(paymentType))
            {
                throw new ArgumentNullException("paymentType", "Payment Type is required when creating a StudentPayment.");
            }
            if (!paymentDate.HasValue)
            {
                paymentDate = DateTime.Today;
            }
            PersonId = personId;
            PaymentType = paymentType;
            PaymentDate = paymentDate.Value;
            Comments = new List<string>();
        }
    }
}
