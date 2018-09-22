// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a transaction posted to the General Ledger through the Data Model.
    /// </summary>
    [Serializable]
    public class GenLedgrTransactionDetail
    {
        /// <summary>
        /// An optional tracking number for the detailed accounting line.
        /// </summary>
        public int? SequenceNumber { get; set; }

        /// <summary>
        /// The accounting string associated with the transaction detail line.
        /// </summary>
        public string ProjectId { get; private set; }

        /// <summary>
        /// The description of the detailed accounting line.
        /// </summary>
        public GlAccount GlAccount { get; private set; }

        /// <summary>
        /// The type of detailed accounting line (credit/debit).
        /// </summary>
        public CreditOrDebit Type { get; private set; }

        /// <summary>
        /// The amount of the credit or debit.
        /// </summary>
        public AmountAndCurrency Amount { get; private set; }

        /// <summary>
        /// ID of the person submitting the GL Transaction
        /// </summary>
        public string SubmittedBy { get; set; }

        /// <summary>
        /// The number of gift units associated with the transaction.
        /// </summary>
        public string EncGiftUnits { get; set; }
        public string EncRefNumber { get; set; }
        public string EncLineItemNumber { get; set; }
        public int? EncSequenceNumber { get; set; }
        public string EncAdjustmentType { get; set; }
        public string EncCommitmentType { get; set; }

        /// <summary>
        /// Constructor initializes the General Ledger transaction object.
        /// </summary>

        public GenLedgrTransactionDetail(string accountNumber, string projectId, string description, CreditOrDebit type, AmountAndCurrency amount)
        {
            if (string.IsNullOrEmpty(accountNumber))
            {
                throw new ArgumentNullException("accountingString", "The accounting string must be specified.");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description", "The description must be specified.");
            }

            if (accountNumber.Contains("*"))
            {
                var accountNumberWithProject = accountNumber.Split("*".ToCharArray());
                accountNumber = accountNumberWithProject[0].ToString();
                projectId = accountNumberWithProject[1].ToString();
            }
            GlAccount = new GlAccount(accountNumber);
            GlAccount.GlAccountDescription = description;
            ProjectId = projectId;
            Type = type;
            Amount = amount;
        }
    }
}
