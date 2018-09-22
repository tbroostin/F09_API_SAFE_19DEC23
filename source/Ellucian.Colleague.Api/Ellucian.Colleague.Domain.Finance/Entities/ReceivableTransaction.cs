// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public abstract class ReceivableTransaction
    {
        private string _id;
        /// <summary>
        /// Transaction ID
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    throw new InvalidOperationException("ID already defined for receivable transaction.");
                }
                _id = value;
            }
        }

        private readonly string _personId;
        /// <summary>
        /// Person ID
        /// </summary>
        public string PersonId { get { return _personId; } }
        
        private readonly string _receivableType;
        /// <summary>
        /// Receivable Type
        /// </summary>
        public string ReceivableType { get { return _receivableType; } }
        
        private readonly string _termId;
        /// <summary>
        /// Term ID
        /// </summary>
        public string TermId { get { return _termId; } }
        
        private string _referenceNumber;
        /// <summary>
        /// Reference Number
        /// </summary>
        public string ReferenceNumber 
        { 
            get { return _referenceNumber; }
            set
            {
                if (!string.IsNullOrEmpty(_referenceNumber))
                {
                    throw new InvalidOperationException("Reference number already defined for receivable transaction.");
                }
                _referenceNumber = value;
            }
        }

        private readonly DateTime _date;
        /// <summary>
        /// Date
        /// </summary>
        public DateTime Date { get { return _date; } }

        /// <summary>
        /// External System
        /// </summary>
        public string ExternalSystem { get; private set; }
        
        /// <summary>
        /// External System ID
        /// </summary>
        public string ExternalIdentifier { get; private set; }
        
        /// <summary>
        /// Archived indicator
        /// </summary>
        public bool IsArchived { get; set; }
        
        /// <summary>
        /// Location
        /// </summary>
        public string Location { get; set; }

        public ReceivableTransactionType TransactionType
        {
            get
            {
                if (this.GetType() == typeof(ReceivableInvoice)) return ReceivableTransactionType.Invoice;
                if (this.GetType() == typeof(ReceiptPayment)) return ReceivableTransactionType.ReceiptPayment;
                if (this.GetType() == typeof(FinancialAidPayment)) return ReceivableTransactionType.FinancialAid;
                if (this.GetType() == typeof(SponsorInvoice)) return ReceivableTransactionType.SponsoredBilling;
                if (this.GetType() == typeof(PayrollDeduction)) return ReceivableTransactionType.PayrollDeduction;
                if (this.GetType() == typeof(TransferInvoice)) return ReceivableTransactionType.Transfer;
                if (this.GetType() == typeof(DepositAllocation)) return ReceivableTransactionType.DepositAllocation;
                if (this.GetType() == typeof(PaymentRefund)) return ReceivableTransactionType.Refund;
                return ReceivableTransactionType.Unknown;
            }
        }

        protected ReceivableTransaction(string id, string referenceNumber, string personId, string receivableType, string termId, DateTime date)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID cannot be null");
            }
            if (string.IsNullOrEmpty(receivableType))
            {
                throw new ArgumentNullException("receivableType", "Receivable Type cannot be null");
            }
            if (date == default(DateTime))
            {
                throw new ArgumentOutOfRangeException("date");
            }

            _id = id;
            _referenceNumber = referenceNumber;
            _personId = personId;
            _receivableType = receivableType;
            _termId = termId;
            _date = date;

            IsArchived = false;
        }

        /// <summary>
        /// Add an external system and external system ID to the ReceivableTransaction
        /// </summary>
        /// <param name="externalSystem">External system</param>
        /// <param name="externalIdentifier">ID external system's transaction</param>
        public void AddExternalSystemAndId(string externalSystem, string externalIdentifier)
        {
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
