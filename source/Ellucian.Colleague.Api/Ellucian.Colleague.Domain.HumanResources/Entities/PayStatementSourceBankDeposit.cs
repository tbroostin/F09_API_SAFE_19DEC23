using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Pay Statement soure bank deposit
    /// </summary>
    [Serializable]
    public class PayStatementSourceBankDeposit
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
        public PayStatementSourceBankDeposit(string bankName, BankAccountType bankAccountType, string accountLastFour, decimal? depositAmount)
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
