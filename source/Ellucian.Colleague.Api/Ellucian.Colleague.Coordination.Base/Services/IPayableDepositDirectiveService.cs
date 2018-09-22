/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface to a PaybleDepositDirectiveService
    /// </summary>
    public interface IPayableDepositDirectiveService
    {
        /// <summary>
        /// Get the Payable Deposit Directives for the current user
        /// </summary>
        /// <returns>A list of PayableDepositDirectives</returns>
        Task<IEnumerable<PayableDepositDirective>> GetPayableDepositDirectivesAsync();

        /// <summary>
        /// Get a specific Payable Deposit Directive for the current user with the payableDepositDirectiveId
        /// </summary>
        /// <param name="payableDepositDirectiveId">The id of the payable deposit directive to get</param>
        /// <returns>A PayableDepositDirective</returns>
        Task<PayableDepositDirective> GetPayableDepositDirectiveAsync(string payableDepositDirectiveId);

        /// <summary>
        /// Create a new Payable Deposit Directive for the current user
        /// </summary>
        /// <param name="newPayableDepositDirective"></param>
        /// <returns></returns>
        Task<PayableDepositDirective> CreatePayableDepositDirectiveAsync(string token, PayableDepositDirective newPayableDepositDirective);

        /// <summary>
        /// Update a payable deposit directive of the current user
        /// </summary>
        /// <param name="updatedPayableDepositDirective"></param>
        /// <returns></returns>
        Task<PayableDepositDirective> UpdatePayableDepositDirectiveAsync(string token, PayableDepositDirective updatedPayableDepositDirective);

        /// <summary>
        /// Delete a payable deposit directive of the current user
        /// </summary>
        /// <param name="payableDepositDirectiveId"></param>
        /// <returns></returns>
        Task DeletePayableDepositDirectiveAsync(string token, string payableDepositDirectiveId);

        /// <summary>
        /// Get an authentication token for updating or creating a payable deposit directive
        /// </summary>
        /// <param name="directiveId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        Task<BankingAuthenticationToken> AuthenticatePayableDepositDirectiveAsync(string depositDirectiveId, string accountId, string addressId);
    }
}
