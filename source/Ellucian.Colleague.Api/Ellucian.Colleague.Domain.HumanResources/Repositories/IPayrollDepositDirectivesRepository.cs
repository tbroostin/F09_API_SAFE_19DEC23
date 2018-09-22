/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPayrollDepositDirectivesRepository
    {
        Task<PayrollDepositDirectiveCollection> GetPayrollDepositDirectivesAsync(string employeeId);
        Task<PayrollDepositDirective> GetPayrollDepositDirectiveAsync(string id, string employeeId);
        Task<PayrollDepositDirectiveCollection> UpdatePayrollDepositDirectivesAsync(PayrollDepositDirectiveCollection payrollDepositDirective);
        Task<PayrollDepositDirective> CreatePayrollDepositDirectiveAsync(PayrollDepositDirective payrollDepositDirective);
        Task<bool> DeletePayrollDepositDirectiveAsync(string id, string employeeId);
        Task<bool> DeletePayrollDepositDirectivesAsync(IEnumerable<string> ids, string employeeId);
        Task<BankingAuthenticationToken> AuthenticatePayrollDepositDirective(string employeeId, string depositDirectiveId, string accountId);
    }
}
