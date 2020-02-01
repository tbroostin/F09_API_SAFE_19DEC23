//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Subregions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class SubregionsController : BaseCompressedApiController
    {
        
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SubregionsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public SubregionsController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all subregions
        /// </summary>
        /// <returns>All <see cref="Dtos.Subregions">Subregions</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        public async Task<IEnumerable<Subregions>> GetSubregionsAsync()
        {
            return new List<Subregions>();
        }

        /// <summary>
        /// Retrieve (GET) an existing subregions
        /// </summary>
        /// <param name="guid">GUID of the subregions to get</param>
        /// <returns>A subregions object <see cref="Dtos.Subregions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        public async Task<Dtos.Subregions> GetSubregionsByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new Exception(string.Format("No subregions was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new subregions
        /// </summary>
        /// <param name="subregions">DTO of the new subregions</param>
        /// <returns>A subregions object <see cref="Dtos.Subregions"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.Subregions> PostSubregionsAsync([FromBody] Dtos.Subregions subregions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing subregions
        /// </summary>
        /// <param name="guid">GUID of the subregions to update</param>
        /// <param name="subregions">DTO of the updated subregions</param>
        /// <returns>A subregions object <see cref="Dtos.Subregions"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.Subregions> PutSubregionsAsync([FromUri] string guid, [FromBody] Dtos.Subregions subregions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a subregions
        /// </summary>
        /// <param name="guid">GUID to desired subregions</param>
        [HttpDelete]
        public async Task DeleteSubregionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}