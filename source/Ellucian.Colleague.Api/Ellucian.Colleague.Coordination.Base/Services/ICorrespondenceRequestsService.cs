// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.
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

        /// <summary>
        /// Used to notify back office users when a self-service user has uploaded a new attachment associated with one of their correspondence requests.
        /// If a status code has been specified, the status of the correspondence request will also be changed.
        /// </summary>
        /// <accessComments>
        /// Users may submit attachment notifications for their own correspondence requests.
        /// </accessComments>
        /// <param name="attachmentNotification">Object that contains the person Id, Communication code and optionally the assign date of the correspondence request.</param>
        /// <returns></returns>
        Task<CorrespondenceRequest> AttachmentNotificationAsync(CorrespondenceAttachmentNotification attachmentNotification);
    }
}
