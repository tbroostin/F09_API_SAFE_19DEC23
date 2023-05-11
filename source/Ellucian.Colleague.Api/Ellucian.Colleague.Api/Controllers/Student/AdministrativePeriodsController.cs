//Copyright 2018-2022 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdministrativePeriods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdministrativePeriodsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdministrativePeriodsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public AdministrativePeriodsController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all person-employment-references
        /// </summary>
        /// <returns>All <see cref="AdministrativePeriod">AdministrativePeriods</see></returns>
        public async Task<IEnumerable<AdministrativePeriod>> GetAdministrativePeriodsAsync()
        {
            return new List<AdministrativePeriod>();
        }

        /// <summary>
        /// Retrieve (GET) an existing person-employment-references
        /// </summary>
        /// <param name="guid">GUID of the person-employment-references to get</param>
        /// <returns>A AdministrativePeriod object <see cref="AdministrativePeriod"/> in EEDM format</returns>
        [HttpGet]
        public async Task<AdministrativePeriod> GetAdministrativePeriodByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No administrative period was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new AdministrativePeriod
        /// </summary>
        /// <param name="AdministrativePeriod">DTO of the new AdministrativePeriod</param>
        /// <returns>A AdministrativePeriod object <see cref="AdministrativePeriod"/> in EEDM format</returns>
        [HttpPost]
        public async Task<AdministrativePeriod> PostAdministrativePeriodsAsync([FromBody] AdministrativePeriod AdministrativePeriod)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing AdministrativePeriod
        /// </summary>
        /// <param name="guid">GUID of the AdministrativePeriod to update</param>
        /// <param name="AdministrativePeriod">DTO of the updated AdministrativePeriod</param>
        /// <returns>A AdministrativePeriod object <see cref="AdministrativePeriod"/> in EEDM format</returns>
        [HttpPut]
        public async Task<AdministrativePeriod> PutAdministrativePeriodAsync([FromUri] string guid, [FromBody] AdministrativePeriod AdministrativePeriod)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a AdministrativePeriod
        /// </summary>
        /// <param name="guid">GUID to desired AdministrativePeriod</param>
        [HttpDelete]
        public async Task DeleteAdministrativePeriodAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}