//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PersonPublications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonPublicationsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonPublicationsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public PersonPublicationsController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all person-publications
        /// </summary>
        /// <returns>All <see cref="Dtos.PersonPublications">PersonPublications</see></returns>
        public async Task<IEnumerable<Dtos.PersonPublications>> GetPersonPublicationsAsync()
        {
            return new List<Dtos.PersonPublications>();
        }

        /// <summary>
        /// Retrieve (GET) an existing person-publications
        /// </summary>
        /// <param name="guid">GUID of the person-publications to get</param>
        /// <returns>A personPublications object <see cref="Dtos.PersonPublications"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.PersonPublications> GetPersonPublicationsByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No person-publications was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new personPublications
        /// </summary>
        /// <param name="personPublications">DTO of the new personPublications</param>
        /// <returns>A personPublications object <see cref="Dtos.PersonPublications"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PersonPublications> PostPersonPublicationsAsync([FromBody] Dtos.PersonPublications personPublications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personPublications
        /// </summary>
        /// <param name="guid">GUID of the personPublications to update</param>
        /// <param name="personPublications">DTO of the updated personPublications</param>
        /// <returns>A personPublications object <see cref="Dtos.PersonPublications"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PersonPublications> PutPersonPublicationsAsync([FromUri] string guid, [FromBody] Dtos.PersonPublications personPublications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personPublications
        /// </summary>
        /// <param name="guid">GUID to desired personPublications</param>
        [HttpDelete]
        public async Task DeletePersonPublicationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}