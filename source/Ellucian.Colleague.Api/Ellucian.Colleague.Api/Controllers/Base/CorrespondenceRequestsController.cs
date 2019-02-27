// Copyright 2018 Ellucian Company L.P. and its affiliates.
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

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// The CorrespondenceRequestsController exposes a person's correspondence requests
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
        /// Users may request their own data. 
        /// </accessComments>
        /// <param name="personId">The Id of the person for whom to get correspondence requests</param>
        /// <returns>A list of CorrespondenceRequests objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the personId argument is null, empty or does not match the current user</exception>        
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
    }
}
