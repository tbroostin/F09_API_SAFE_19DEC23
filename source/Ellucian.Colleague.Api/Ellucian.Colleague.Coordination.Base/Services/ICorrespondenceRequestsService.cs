// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for a CorrespondenceRequestsService
    /// </summary>
    public interface ICorrespondenceRequestsService
    {
        /// <summary>
        /// Get all of a person's correspondence requests.
        /// </summary>
        /// <param name="personId">The Id of the person for whom to get correspondence requests</param>
        /// <returns>A list of CorrespondenceRequest DTO objects</returns>        
        Task<IEnumerable<CorrespondenceRequest>> GetCorrespondenceRequestsAsync(string personID);
    }
}
