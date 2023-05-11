//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentOrganizations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentOrganizationsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentOrganizationsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public EmploymentOrganizationsController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all employment-organizations
        /// </summary>
        /// <returns>All <see cref="Dtos.EmploymentOrganizations">EmploymentOrganizations</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.EmploymentOrganizations))]
        public async Task<IEnumerable<Dtos.EmploymentOrganizations>> GetEmploymentOrganizationsAsync(QueryStringFilter criteria)
        {
            var criteriaObj = GetFilterObject<Dtos.EmploymentDepartments>(_logger, "criteria");
            CheckForEmptyFilterParameters();
            return new List<Dtos.EmploymentOrganizations>();
        }

        /// <summary>
        /// Retrieve (GET) an existing employment-organizations
        /// </summary>
        /// <param name="guid">GUID of the employment-organizations to get</param>
        /// <returns>A employmentOrganizations object <see cref="Dtos.EmploymentOrganizations"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        public async Task<Dtos.EmploymentOrganizations> GetEmploymentOrganizationsByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No employment-organizations was found for guid '{0}'.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new employmentOrganizations
        /// </summary>
        /// <param name="employmentOrganizations">DTO of the new employmentOrganizations</param>
        /// <returns>A employmentOrganizations object <see cref="Dtos.EmploymentOrganizations"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.EmploymentOrganizations> PostEmploymentOrganizationsAsync([FromBody] Dtos.EmploymentOrganizations employmentOrganizations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentOrganizations
        /// </summary>
        /// <param name="guid">GUID of the employmentOrganizations to update</param>
        /// <param name="employmentOrganizations">DTO of the updated employmentOrganizations</param>
        /// <returns>A employmentOrganizations object <see cref="Dtos.EmploymentOrganizations"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        public async Task<Dtos.EmploymentOrganizations> PutEmploymentOrganizationsAsync([FromUri] string guid, [FromBody] Dtos.EmploymentOrganizations employmentOrganizations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentOrganizations
        /// </summary>
        /// <param name="guid">GUID to desired employmentOrganizations</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        public async Task DeleteEmploymentOrganizationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}