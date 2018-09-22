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
using Ellucian.Colleague.Dtos.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to ProficiencyLicensingAuthorities
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class ProficiencyLicensingAuthoritiesController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ProficiencyLicensingAuthoritiesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public ProficiencyLicensingAuthoritiesController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all proficiency-licensing-authorities
        /// </summary>
        /// <returns>All <see cref="ProficiencyLicensingAuthority">ProficiencyLicensingAuthorities</see></returns>
        public async Task<IEnumerable<ProficiencyLicensingAuthority>> GetProficiencyLicensingAuthoritiesAsync()
        {
            return new List<ProficiencyLicensingAuthority>();
        }

        /// <summary>
        /// Retrieve (GET) an existing proficiency-licensing-authorities
        /// </summary>
        /// <param name="guid">GUID of the proficiency-licensing-authority to get</param>
        /// <returns>A proficiencyLicensingAuthority object <see cref="ProficiencyLicensingAuthority"/> in EEDM format</returns>
        [HttpGet]
        public async Task<ProficiencyLicensingAuthority> GetProficiencyLicensingAuthorityByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new Exception(string.Format("No proficiency-licensing-authorities was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new proficiencyLicensingAuthority
        /// </summary>
        /// <param name="proficiencyLicensingAuthority">DTO of the new proficiencyLicensingAuthority</param>
        /// <returns>A proficiencyLicensingAuthority object <see cref="ProficiencyLicensingAuthority"/> in EEDM format</returns>
        [HttpPost]
        public async Task<ProficiencyLicensingAuthority> PostProficiencyLicensingAuthorityAsync([FromBody] ProficiencyLicensingAuthority proficiencyLicensingAuthority)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing proficiencyLicensingAuthority
        /// </summary>
        /// <param name="guid">GUID of the proficiencyLicensingAuthorities to update</param>
        /// <param name="proficiencyLicensingAuthority">DTO of the updated proficiencyLicensingAuthority</param>
        /// <returns>A proficiencyLicensingAuthority object <see cref="ProficiencyLicensingAuthority"/> in EEDM format</returns>
        [HttpPut]
        public async Task<ProficiencyLicensingAuthority> PutProficiencyLicensingAuthorityAsync([FromUri] string guid, [FromBody] ProficiencyLicensingAuthority proficiencyLicensingAuthority)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a proficiencyLicensingAuthorities
        /// </summary>
        /// <param name="guid">GUID to desired proficiencyLicensingAuthority</param>
        [HttpDelete]
        public async Task DeleteProficiencyLicensingAuthorityAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}