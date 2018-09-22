using Ellucian.Colleague.Dtos.Base;
/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.HumanResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IPayrollDepositDirectiveService
    {
        Task<IEnumerable<PayrollDepositDirective>> GetPayrollDepositDirectivesAsync();

        Task<PayrollDepositDirective> GetPayrollDepositDirectiveAsync(string id);

        Task<IEnumerable<PayrollDepositDirective>> UpdatePayrollDepositDirectivesAsync(string token, IEnumerable<PayrollDepositDirective> payrollDepositDirectives);

        Task<PayrollDepositDirective> UpdatePayrollDepositDirectiveAsync(string token, PayrollDepositDirective payrollDepositDirective);

        Task<PayrollDepositDirective> CreatePayrollDepositDirectiveAsync(string token, PayrollDepositDirective payrollDepositDirective);

        Task<bool> DeletePayrollDepositDirectiveAsync(string token, string id);

        Task<bool> DeletePayrollDepositDirectivesAsync(string token, IEnumerable<string> ids);

        Task<BankingAuthenticationToken> AuthenticateCurrentUserAsync(string depositDirectiveId, string accountId);
    }
}
