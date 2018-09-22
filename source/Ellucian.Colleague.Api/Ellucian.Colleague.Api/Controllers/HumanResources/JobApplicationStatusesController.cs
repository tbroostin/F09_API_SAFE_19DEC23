//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to JobApplicationStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class JobApplicationStatusesController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the JobApplicationStatusesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public JobApplicationStatusesController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all job-application-statuses
        /// </summary>
        /// <returns>All <see cref="Dtos.JobApplicationStatuses">JobApplicationStatuses</see></returns>
        public async Task<IEnumerable<JobApplicationStatuses>> GetJobApplicationStatusesAsync()
        {
            return new List<JobApplicationStatuses>();
        }

        /// <summary>
        /// Retrieve (GET) an existing job-application-statuses
        /// </summary>
        /// <param name="guid">GUID of the job-application-statuses to get</param>
        /// <returns>A jobApplicationStatuses object <see cref="Dtos.JobApplicationStatuses"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.JobApplicationStatuses> GetJobApplicationStatusesByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new Exception(string.Format("No job-application-statuses was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new jobApplicationStatuses
        /// </summary>
        /// <param name="jobApplicationStatuses">DTO of the new jobApplicationStatuses</param>
        /// <returns>A jobApplicationStatuses object <see cref="Dtos.JobApplicationStatuses"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.JobApplicationStatuses> PostJobApplicationStatusesAsync([FromBody] Dtos.JobApplicationStatuses jobApplicationStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing jobApplicationStatuses
        /// </summary>
        /// <param name="guid">GUID of the jobApplicationStatuses to update</param>
        /// <param name="jobApplicationStatuses">DTO of the updated jobApplicationStatuses</param>
        /// <returns>A jobApplicationStatuses object <see cref="Dtos.JobApplicationStatuses"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.JobApplicationStatuses> PutJobApplicationStatusesAsync([FromUri] string guid, [FromBody] Dtos.JobApplicationStatuses jobApplicationStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a jobApplicationStatuses
        /// </summary>
        /// <param name="guid">GUID to desired jobApplicationStatuses</param>
        [HttpDelete]
        public async Task DeleteJobApplicationStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}