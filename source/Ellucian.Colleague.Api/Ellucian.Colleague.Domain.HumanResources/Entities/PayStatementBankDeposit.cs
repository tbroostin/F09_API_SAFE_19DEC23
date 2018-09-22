/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Base.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// A bank deposit for the Pay Statement
    /// </summary>
    [Serializable]
    public class PayStatementBankDeposit
    {
        /// <summary>
        /// Name of bank for this deposit
        /// </summary>
        public string BankName { get; private set; }

        /// <summary>
        /// The type of bank account
        /// </summary>
        public BankAccountType BankAccountType { get; private set; }

        /// <summary>
        /// The last 4 of bank account number
        /// </summary>
        public string AccountIdLastFour { get; private set; }

        /// <summary>
        /// Amount for this deposit
        /// </summary>
        public decimal? DepositAmount { get; private set; }

        /// <summary>
        /// Create a line item for a bank deposit
        /// </summary>
        /// <param name="bankName"></param>
        /// <param name="bankAccountType"></param>
        /// <param name="depositAmount"></param>
        public PayStatementBankDeposit(string bankName, BankAccountType bankAccountType, string accountLastFour, decimal? depositAmount)
        {
            if (string.IsNullOrWhiteSpace(bankName))
            {
                bankName = string.Empty;
            }

            BankName = bankName;
            BankAccountType = bankAccountType;
            AccountIdLastFour = accountLastFour;
            DepositAmount = depositAmount;
        }
    }
}
