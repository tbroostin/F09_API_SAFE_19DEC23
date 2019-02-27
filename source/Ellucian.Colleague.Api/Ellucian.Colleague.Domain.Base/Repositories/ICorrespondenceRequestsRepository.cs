// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Defines how a CorrespondenceRequestsRepository must provide access to Correspondence Requests from Colleague.
    /// </summary>
    public interface ICorrespondenceRequestsRepository
    {
        /// <summary>
        /// Get all of a person's correspondence requests.
        /// </summary>
        /// <param name="personId">The Id of the person for whom to get correspondence requests</param>
        /// <returns>A list of CorrespondenceRequest objects</returns>
        Task<IEnumerable<CorrespondenceRequest>> GetCorrespondenceRequestsAsync(string personId);
    }
}
