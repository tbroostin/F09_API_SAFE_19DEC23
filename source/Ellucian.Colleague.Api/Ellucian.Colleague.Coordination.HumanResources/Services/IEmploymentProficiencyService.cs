using Ellucian.Colleague.Coordination.Base.Services;
/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.HumanResources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IEmploymentProficiencyService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentProficiency>> GetEmploymentProficienciesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.EmploymentProficiency> GetEmploymentProficiencyByGuidAsync(string guid);

    }
}
