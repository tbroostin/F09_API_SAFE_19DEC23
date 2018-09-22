// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Define required WorkTask repository methods
    /// </summary>
    public interface IWorkTaskRepository
    {
        /// <summary>
        /// Get the worktasks for the specified person and roles
        /// </summary>
        /// <param name="personId">ID of the person</param>
        /// <param name="roleIds">IDs (not titles) of the roles</param>
        /// <returns></returns>
        Task<List<WorkTask>> GetAsync(string personId, List<string> roleIds);
    }
}
