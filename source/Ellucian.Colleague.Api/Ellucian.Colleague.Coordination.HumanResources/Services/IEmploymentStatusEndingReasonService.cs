//Copyright 2016-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IEmploymentStatusEndingReasonService : IBaseService
    {
        Task<IEnumerable<Dtos.EmploymentStatusEndingReason>> GetEmploymentStatusEndingReasonsAsync(bool bypassCache = false);
        Task<Dtos.EmploymentStatusEndingReason> GetEmploymentStatusEndingReasonByIdAsync(string id);
    }
}
