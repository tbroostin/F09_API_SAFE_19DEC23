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
using Ellucian.Colleague.Dtos.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentProficiencyLevels
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentProficiencyLevelsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentProficiencyLevelsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public EmploymentProficiencyLevelsController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all employment-proficiency-levels
        /// </summary>
        /// <returns>All <see cref="EmploymentProficiencyLevel">EmploymentProficiencyLevels</see></returns>
        public async Task<IEnumerable<EmploymentProficiencyLevel>> GetEmploymentProficiencyLevelsAsync()
        {
            return new List<EmploymentProficiencyLevel>();
        }

        /// <summary>
        /// Retrieve (GET) an existing employment-proficiency-level
        /// </summary>
        /// <param name="guid">GUID of the employment-proficiency-levels to get</param>
        /// <returns>A employmentProficiencyLevels object <see cref="EmploymentProficiencyLevel"/> in EEDM format</returns>
        [HttpGet]
        public async Task<EmploymentProficiencyLevel> GetEmploymentProficiencyLevelByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No employment-proficiency-levels were found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new employmentProficiencyLevel
        /// </summary>
        /// <param name="employmentProficiencyLevel">DTO of the new employmentProficiencyLevel</param>
        /// <returns>A employmentProficiencyLevels object <see cref="EmploymentProficiencyLevels"/> in EEDM format</returns>
        [HttpPost]
        public async Task<EmploymentProficiencyLevel> PostEmploymentProficiencyLevelAsync([FromBody] EmploymentProficiencyLevel employmentProficiencyLevel)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentProficiencyLevels
        /// </summary>
        /// <param name="guid">GUID of the employmentProficiencyLevels to update</param>
        /// <param name="employmentProficiencyLevel">DTO of the updated employmentProficiencyLevels</param>
        /// <returns>A employmentProficiencyLevel object <see cref="EmploymentProficiencyLevel"/> in EEDM format</returns>
        [HttpPut]
        public async Task<EmploymentProficiencyLevel> PutEmploymentProficiencyLevelAsync([FromUri] string guid, [FromBody] EmploymentProficiencyLevel employmentProficiencyLevel)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentProficiencyLevels
        /// </summary>
        /// <param name="guid">GUID to desired employmentProficiencyLevel</param>
        [HttpDelete]
        public async Task DeleteEmploymentProficiencyLevelAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}