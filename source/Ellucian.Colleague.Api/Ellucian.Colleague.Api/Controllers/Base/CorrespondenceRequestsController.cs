// Copyright 2018-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Data.Colleague.Exceptions;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// The CorrespondenceRequestsController provides access to retrieve and update a person's correspondence requests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CorrespondenceRequestsController : BaseCompressedApiController
    {
        private readonly ICorrespondenceRequestsService CorrespondenceRequestsService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// Dependency Injection constructor for StudentDocumentsController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="correspondenceRequestsService"></param>
        /// <param name="logger"></param>
        public CorrespondenceRequestsController(IAdapterRegistry adapterRegistry, ICorrespondenceRequestsService correspondenceRequestsService, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            CorrespondenceRequestsService = correspondenceRequestsService;
            Logger = logger;
        }

        /// <summary>
        /// Get all of a person's correspondence requests.
        /// </summary>
        /// <accessComments>
        /// Users may request their own correspondence requests.
        /// Proxy users who have been granted General Required Documents (CORD) proxy access permission
        /// may view the grantor's correspondence requests.
        /// </accessComments>
        /// <param name="personId">The Id of the person for whom to get correspondence requests</param>
        /// <returns>A list of CorrespondenceRequests objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the personId argument is null, empty,
        /// or the user does not have access to the person's correspondence requests</exception>        
        public async Task<IEnumerable<CorrespondenceRequest>> GetCorrespondenceRequestsAsync([FromUri] string personId = "")
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw CreateHttpResponseException("personId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return await CorrespondenceRequestsService.GetCorrespondenceRequestsAsync(personId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw CreateHttpResponseException(csee.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                Logger.Error(pe, pe.Message);
                throw CreateHttpResponseException(pe.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting CorrespondenceRequests resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Used to notify back office users when a self-service user has uploaded a new attachment associated with one of their correspondence requests.
        /// If a status code has been specified, the status of the correspondence request will also be changed.
        /// </summary>
        /// <accessComments>
        /// Users may submit attachment notifications for their own correspondence requests.
        /// </accessComments>
        /// <param name="attachmentNotification">Object that contains the person Id, Communication code and optionally the assign date of the correspondence request.</param>
        /// <returns>The CorrespondenceRequest notified of an attachment</returns>
        /// <exception cref="HttpResponseException">Thrown if the personId or communication code properties of the input object are null, empty,
        /// if the correspondence request cannot be updated due to a record lock, 
        /// or if the user does not have access to the person's correspondence requests</exception> 
        public async Task<CorrespondenceRequest> PutAttachmentNotificationAsync(CorrespondenceAttachmentNotification attachmentNotification)
        {
            CorrespondenceRequest returnDto = null;
            if (attachmentNotification == null || string.IsNullOrEmpty(attachmentNotification.PersonId) || string.IsNullOrEmpty(attachmentNotification.CommunicationCode))
            {
                throw CreateHttpResponseException("Must provide person Id and communication code.", System.Net.HttpStatusCode.BadRequest);
            }
            try
            {
                returnDto = await CorrespondenceRequestsService.AttachmentNotificationAsync(attachmentNotification);
            }
            catch (RecordLockException ioex)
            {
                // Record lock - status could not be updated
                Logger.Error(ioex, "PutAttachmentNotificationAsync failed due to a record lock.");
                throw CreateHttpResponseException(ioex.Message, System.Net.HttpStatusCode.Conflict);
            }
            catch (PermissionsException peex)
            {
                Logger.Error(peex, "PutAttachmentNotificationAsync failed due to a permission problem.");
                throw CreateHttpResponseException(peex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException nfex)
            {
                // Record lock - status could not be updated
                Logger.Error(nfex, "PutAttachmentNotificationAsync failed due to the record not found.");
                throw CreateHttpResponseException(nfex.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "PutAttachmentNotificationAsync failed due to a repository error.");
                throw CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
            return returnDto;
        }
    }
}
