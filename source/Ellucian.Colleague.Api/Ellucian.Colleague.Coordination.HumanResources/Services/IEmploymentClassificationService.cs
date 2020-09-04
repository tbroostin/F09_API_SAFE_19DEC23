/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IEmploymentClassificationService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentClassification>> GetEmploymentClassificationsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.EmploymentClassification> GetEmploymentClassificationByGuidAsync(string guid);

    }
}
