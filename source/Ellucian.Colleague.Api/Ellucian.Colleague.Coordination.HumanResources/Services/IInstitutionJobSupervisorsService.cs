//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for InstitutionJobSupervisors Service
    /// </summary>
    public interface IInstitutionJobSupervisorsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobSupervisors>, int>> GetInstitutionJobSupervisorsAsync(int offset, int limit, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.InstitutionJobSupervisors> GetInstitutionJobSupervisorsByGuidAsync(string guid);

        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobSupervisors>, int>> GetInstitutionJobSupervisors2Async(int offset, int limit, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.InstitutionJobSupervisors> GetInstitutionJobSupervisorsByGuid2Async(string guid);
    }
}