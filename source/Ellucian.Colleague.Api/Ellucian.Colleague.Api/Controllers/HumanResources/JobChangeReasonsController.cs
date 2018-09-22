// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Job Change Reasons data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class JobChangeReasonsController : BaseCompressedApiController
    {
        private readonly IJobChangeReasonService _jobChangeReasonService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the JobChangeReasonsController class.
        /// </summary>
        /// <param name="jobChangeReasonService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public JobChangeReasonsController(IJobChangeReasonService jobChangeReasonService, ILogger logger)
        {
            _jobChangeReasonService = jobChangeReasonService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves all rehire types.
        /// </summary>
        /// <returns>All JobChangeReason objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.JobChangeReason>> GetJobChangeReasonsAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                return await _jobChangeReasonService.GetJobChangeReasonsAsync(bypassCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves a rehire type by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.JobChangeReason">JobChangeReason.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.JobChangeReason> GetJobChangeReasonByIdAsync(string id)
        {
            try
            {
                return await _jobChangeReasonService.GetJobChangeReasonByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates a JobChangeReason.
        /// </summary>
        /// <param name="jobChangeReason"><see cref="JobChangeReason">JobChangeReason</see> to update</param>
        /// <returns>Newly updated <see cref="JobChangeReason">JobChangeReason</see></returns>
        [HttpPut]
        public async Task<Dtos.JobChangeReason> PutJobChangeReasonAsync([FromBody] Dtos.JobChangeReason jobChangeReason)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a JobChangeReason.
        /// </summary>
        /// <param name="jobChangeReason"><see cref="JobChangeReason">JobChangeReason</see> to create</param>
        /// <returns>Newly created <see cref="JobChangeReason">JobChangeReason</see></returns>
        [HttpPost]
        public async Task<Dtos.JobChangeReason> PostJobChangeReasonAsync([FromBody] Dtos.JobChangeReason jobChangeReason)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing JobChangeReason
        /// </summary>
        /// <param name="id">Id of the JobChangeReason to delete</param>
        [HttpDelete]
        public async Task DeleteJobChangeReasonAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
