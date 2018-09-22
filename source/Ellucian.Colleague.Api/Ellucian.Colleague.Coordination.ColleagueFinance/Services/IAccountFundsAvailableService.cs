// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for the AccountingString service.
    /// </summary>
    public interface IAccountFundsAvailableService
    {
        /// <summary>
        /// Returns accounts funds available
        /// </summary>
        /// <param name="accountingStringValue"></param>
        /// <param name="amountValue"></param>
        /// <param name="balanceOn"></param>
        /// <param name="submittedByValue"></param>
        /// <returns></returns>
        Task<AccountFundsAvailable> GetAccountFundsAvailableByFilterCriteriaAsync(string accountingStringValue, decimal amountValue, DateTime? balanceOn, string submittedByValue);

        //Task<AccountFundsAvailable_Transactions> CheckAccountFundsAvailable_TransactionsAsync(AccountFundsAvailable_Transactions transaction);
        Task<AccountFundsAvailable_Transactions> CheckAccountFundsAvailable_Transactions2Async(AccountFundsAvailable_Transactions transaction);
        Task<AccountFundsAvailable_Transactions2> CheckAccountFundsAvailable_Transactions3Async(AccountFundsAvailable_Transactions2 transaction);
    }
}
