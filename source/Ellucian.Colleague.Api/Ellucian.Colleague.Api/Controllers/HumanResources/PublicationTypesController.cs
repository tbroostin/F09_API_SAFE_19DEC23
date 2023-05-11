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
    /// Provides access to PublicationTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PublicationTypesController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PublicationTypesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public PublicationTypesController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all publication-types
        /// </summary>
        /// <returns>All <see cref="Dtos.PublicationTypes">PublicationTypes</see></returns>
        public async Task<IEnumerable<Dtos.PublicationTypes>> GetPublicationTypesAsync()
        {
            return new List<Dtos.PublicationTypes>();
        }

        /// <summary>
        /// Retrieve (GET) an existing publication-types
        /// </summary>
        /// <param name="guid">GUID of the publication-types to get</param>
        /// <returns>A publicationTypes object <see cref="Dtos.PublicationTypes"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.PublicationTypes> GetPublicationTypesByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No publication-types was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new publicationTypes
        /// </summary>
        /// <param name="publicationTypes">DTO of the new publicationTypes</param>
        /// <returns>A publicationTypes object <see cref="Dtos.PublicationTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PublicationTypes> PostPublicationTypesAsync([FromBody] Dtos.PublicationTypes publicationTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing publicationTypes
        /// </summary>
        /// <param name="guid">GUID of the publicationTypes to update</param>
        /// <param name="publicationTypes">DTO of the updated publicationTypes</param>
        /// <returns>A publicationTypes object <see cref="Dtos.PublicationTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PublicationTypes> PutPublicationTypesAsync([FromUri] string guid, [FromBody] Dtos.PublicationTypes publicationTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a publicationTypes
        /// </summary>
        /// <param name="guid">GUID to desired publicationTypes</param>
        [HttpDelete]
        public async Task DeletePublicationTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}