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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to employee termination reason data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentTerminationReasonsController : BaseCompressedApiController
    {
        private readonly IEmploymentStatusEndingReasonService _employmentStatusEndingReasonService;
        private readonly ILogger _logger;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="employmentStatusEndingReasonService"></param>
        /// <param name="logger"></param>
        public EmploymentTerminationReasonsController(IEmploymentStatusEndingReasonService employmentStatusEndingReasonService, ILogger logger)
        {
            _employmentStatusEndingReasonService = employmentStatusEndingReasonService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves all employment termination reasons.
        /// </summary>
        /// <returns>All employmentT termination reason objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentStatusEndingReason>> GetEmploymentTerminationReasonsAsync()
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
                return await _employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonsAsync(bypassCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves a employment termination reason by Id.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.EmploymentStatusEndingReason">EmploymentStatusEndingReason.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentStatusEndingReason> GetEmploymentTerminationReasonByIdAsync(string id)
        {
            try
            {
                return await _employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates (PUT) an employment termination reason.
        /// </summary>
        /// <param name="employmentStatusEndingReason"><see cref="EmploymentStatusEndingReason">EmploymentStatusEndingReason</see> to update</param>
        /// <returns>Newly updated <see cref="EmploymentStatusEndingReason">EmploymentStatusEndingReason</see></returns>
        [HttpPut]
        public async Task<Dtos.EmploymentStatusEndingReason> PutEmploymentTerminationReasonAsync([FromBody] Dtos.EmploymentStatusEndingReason employmentStatusEndingReason)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates (POST) an employment termination reason.
        /// </summary>
        /// <param name="employmentStatusEndingReason"><see cref="EmploymentStatusEndingReason">EmploymentStatusEndingReason</see> to create</param>
        /// <returns>Newly created <see cref="EmploymentStatusEndingReason">EmploymentStatusEndingReason</see></returns>
        [HttpPost]
        public async Task<Dtos.EmploymentStatusEndingReason> PostEmploymentTerminationReasonAsync([FromBody] Dtos.EmploymentStatusEndingReason employmentStatusEndingReason)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing employment termination reason.
        /// </summary>
        /// <param name="id">Id of the employment termination reason to delete.</param>
        [HttpDelete]
        public async Task DeleteEmploymentTerminationReasonAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
