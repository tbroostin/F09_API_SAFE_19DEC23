// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.
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
        /// <summary>
        /// Notifies the back end office when a new attachment has been uploaded by a self-service user for a correspondence request
        /// </summary>
        /// <param name="personId">Person Id of the correspondence request being updated</param>
        /// <param name="communicationCode">Communication code of the correspondence request being updated</param>
        /// <param name="assignDate">Assign Date of the correspondence request being updated</param>
        /// <param name="instance">Instance of the correspondence request being updated</param>
        /// <returns>The correspondence request that was notified of attachment</returns>
        Task<CorrespondenceRequest> AttachmentNotificationAsync(string personId, string communicationCode, DateTime? assignDate, string instance);
    }
}
