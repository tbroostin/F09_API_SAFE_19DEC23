// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IWorkTaskService
    {
        /// <summary>
        /// Returns the work tasks for the given user and roles.
        /// </summary>
        /// <param name="personIds"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        Task<List<Dtos.Base.WorkTask>> GetAsync(string personId);
    }
}
