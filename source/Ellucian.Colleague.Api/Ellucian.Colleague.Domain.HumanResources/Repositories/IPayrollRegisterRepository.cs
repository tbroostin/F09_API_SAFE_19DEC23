using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPayrollRegisterRepository
    {
        Task<IEnumerable<PayrollRegisterEntry>> GetPayrollRegisterByEmployeeIdsAsync(IEnumerable<string> employeeIds, DateTime? startDate = null, DateTime? endDate = null);

        //Task<IEnumerable<PayrollRegisterEntry>> GetPayrollRegisterAsync(string employeeId, string paycheckReferenceId, string payStatementReferenceId);
    }
}
