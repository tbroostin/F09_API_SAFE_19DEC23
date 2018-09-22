/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Represents a bank deposit for the Pay Statement
    /// </summary>
    public class PayStatementBankDeposit
    {
        /// <summary>
        /// Name of bank for this deposit
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// The type of bank account
        /// </summary>
        public BankAccountType BankAccountType { get; set; }

        /// <summary>
        /// The last 4 of bank account number
        /// </summary>
        public string AccountIdLastFour { get; set; }

        /// <summary>
        /// Amount for this deposit
        /// </summary>
        public decimal? DepositAmount { get; set; }
    }
}
