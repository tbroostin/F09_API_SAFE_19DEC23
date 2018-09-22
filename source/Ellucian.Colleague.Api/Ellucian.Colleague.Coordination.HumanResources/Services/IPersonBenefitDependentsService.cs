//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for PersonBenefitDependents services
    /// </summary>
    public interface IPersonBenefitDependentsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonBenefitDependents>, int>> GetPersonBenefitDependentsAsync(int offset, int limit, bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.PersonBenefitDependents> GetPersonBenefitDependentsByGuidAsync(string id);
    }
}
