// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of the methods that must be implemented for a Account funds available repository.
    /// </summary>
    public interface IAccountFundsAvailableRepository
    {
        /// <summary>
        /// Returns funds available.
        /// </summary>
        /// <param name="accountingString"></param>
        /// <param name="amount"></param>
        /// <param name="balanceOn"></param>
        /// <param name="projectNumber"></param>
        /// <param name="submittedBy"></param>
        /// <returns></returns>
        Task<Domain.ColleagueFinance.Entities.FundsAvailable> GetAvailableFundsAsync(string accountingString, decimal amount, string year);

        /// <summary>
        /// Returns funds available for a project.
        /// </summary>
        /// <param name="accountingString"></param>
        /// <param name="amountValue"></param>
        /// <param name="projectNumber"></param>
        /// <param name="balanceOn"></param>
        /// <returns></returns>
        Task<FundsAvailable> GetProjectAvailableFundsAsync(string accountingString, decimal amountValue, string projectNumber, DateTime? balanceOn);

        /// <summary>
        /// Gets user key
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<string> GetPersonIdFromGuidAsync(string guid);

        /// <summary>
        /// Gets BPO Id
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        Task<string> GetBpoAsync(string itemNumber);

        /// <summary>
        /// Gets PO Status
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        Task<string> GetPOStatusByItemNumber(string itemNumber);

        /// <summary>
        /// Gets requisition status
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        Task<string> GetReqStatusByItemNumber(string p);

        /// <summary>
        /// Check Available Funds
        /// </summary>
        /// <param name="fundsAvailable"></param>
        /// <param name="purchaseOrderId"></param>
        /// <param name="voucherId"></param>
        /// <param name="blanketPurchaseOrderNumber"></param>
        /// <param name="documentSubmittedBy"></param>
        /// <returns></returns>
        Task<List<FundsAvailable>> CheckAvailableFundsAsync(List<FundsAvailable> fundsAvailable,
          string purchaseOrderId = "", string voucherId = "", string blanketPurchaseOrderNumber = "", string documentSubmittedBy = "", string requisitionId = "", List<string> BpoReqIds = null);


    }
}
