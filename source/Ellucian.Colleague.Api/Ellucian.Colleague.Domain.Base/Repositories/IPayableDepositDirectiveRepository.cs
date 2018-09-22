/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IPayableDepositDirectiveRepository
    {
        Task<PayableDepositDirectiveCollection> GetPayableDepositDirectivesAsync(string payeeId, string payableDepositDirectiveId = "");

        Task<PayableDepositDirective> CreatePayableDepositDirectiveAsync(PayableDepositDirective newPayableDepositDirective);

        Task<PayableDepositDirective> UpdatePayableDepositDirectiveAsync(PayableDepositDirective inputPayableDepositDirective);

        Task DeletePayableDepositDirectiveAsync(string payableDepositDirectiveId);

        Task<BankingAuthenticationToken> AuthenticatePayableDepositDirectiveAsync(string payeeId, string payableDepositDirectiveId, string accountId, string addressId);

    }
}
