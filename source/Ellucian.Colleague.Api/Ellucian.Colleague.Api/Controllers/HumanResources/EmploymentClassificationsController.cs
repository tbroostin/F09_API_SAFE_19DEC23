// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Employee Classifications data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentClassificationsController : BaseCompressedApiController
    {
        private readonly IEmploymentClassificationService _employmentClassificationService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmployeeClassificationsController class.
        /// </summary>
        /// <param name="employmentClassificationsService">Service of type <see cref="IEmploymentClassificationService">IEmploymentClassificationsService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public EmploymentClassificationsController(IEmploymentClassificationService employmentClassificationsService, ILogger logger)
        {
            _employmentClassificationService = employmentClassificationsService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves all employment classifications.
        /// </summary>
        /// <returns>All EmploymentClassification objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentClassification>> GetEmploymentClassificationsAsync()
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
                return await _employmentClassificationService.GetEmploymentClassificationsAsync(bypassCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves a employment classification by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.EmploymentClassification">EmploymentClassification.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentClassification> GetEmploymentClassificationByIdAsync(string id)
        {
            try
            {
                return await _employmentClassificationService.GetEmploymentClassificationByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates a EmploymentClassification.
        /// </summary>
        /// <param name="employmentClassification"><see cref="Dtos.EmploymentClassification">EmploymentClassification</see> to update</param>
        /// <returns>Newly updated <see cref="Dtos.EmploymentClassification">EmploymentClassification</see></returns>
        [HttpPut]
        public async Task<Dtos.EmploymentClassification> PutEmploymentClassificationAsync([FromBody] Dtos.EmploymentClassification employmentClassification)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a EmploymentClassification.
        /// </summary>
        /// <param name="employmentClassification"><see cref="Dtos.EmploymentClassification">EmploymentClassification</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.EmploymentClassification">EmploymentClassification</see></returns>
        [HttpPost]
        public async Task<Dtos.EmploymentClassification> PostEmploymentClassificationAsync([FromBody] Dtos.EmploymentClassification employmentClassification)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing EmploymentClassification
        /// </summary>
        /// <param name="id">Id of the EmploymentClassification to delete</param>
        [HttpDelete]
        public async Task DeleteEmploymentClassificationAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
