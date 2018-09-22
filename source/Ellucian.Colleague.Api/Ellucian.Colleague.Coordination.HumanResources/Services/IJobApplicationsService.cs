//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for JobApplications services
    /// </summary>
    public interface IJobApplicationsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.JobApplications>, int>> GetJobApplicationsAsync(int offset, int limit, bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.JobApplications> GetJobApplicationsByGuidAsync(string id);
    }
}
