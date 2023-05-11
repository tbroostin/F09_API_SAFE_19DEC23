//Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Regions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RegionsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RegionsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public RegionsController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all regions
        /// </summary>
        /// <returns>All <see cref="Dtos.Regions">Regions</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        public async Task<IEnumerable<Regions>> GetRegionsAsync()
        {
            return new List<Regions>();
        }

        /// <summary>
        /// Retrieve (GET) an existing regions
        /// </summary>
        /// <param name="guid">GUID of the regions to get</param>
        /// <returns>A regions object <see cref="Dtos.Regions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        public async Task<Dtos.Regions> GetRegionsByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No regions was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new regions
        /// </summary>
        /// <param name="regions">DTO of the new regions</param>
        /// <returns>A regions object <see cref="Dtos.Regions"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.Regions> PostRegionsAsync([FromBody] Dtos.Regions regions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing regions
        /// </summary>
        /// <param name="guid">GUID of the regions to update</param>
        /// <param name="regions">DTO of the updated regions</param>
        /// <returns>A regions object <see cref="Dtos.Regions"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.Regions> PutRegionsAsync([FromUri] string guid, [FromBody] Dtos.Regions regions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a regions
        /// </summary>
        /// <param name="guid">GUID to desired regions</param>
        [HttpDelete]
        public async Task DeleteRegionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}