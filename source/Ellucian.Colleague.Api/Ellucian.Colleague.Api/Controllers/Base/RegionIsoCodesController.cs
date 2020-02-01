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
    /// Provides access to RegionIsoCodes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RegionIsoCodesController : BaseCompressedApiController
    {
        
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RegionIsoCodesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public RegionIsoCodesController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all region-iso-codes
        /// </summary>
        /// <returns>All <see cref="Dtos.RegionIsoCodes">RegionIsoCodes</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        public async Task<IEnumerable<RegionIsoCodes>> GetRegionIsoCodesAsync()
        {
            return new List<RegionIsoCodes>();
        }

        /// <summary>
        /// Retrieve (GET) an existing region-iso-codes
        /// </summary>
        /// <param name="guid">GUID of the region-iso-codes to get</param>
        /// <returns>A regionIsoCodes object <see cref="Dtos.RegionIsoCodes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        public async Task<Dtos.RegionIsoCodes> GetRegionIsoCodesByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new Exception(string.Format("No region-iso-codes was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new regionIsoCodes
        /// </summary>
        /// <param name="regionIsoCodes">DTO of the new regionIsoCodes</param>
        /// <returns>A regionIsoCodes object <see cref="Dtos.RegionIsoCodes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.RegionIsoCodes> PostRegionIsoCodesAsync([FromBody] Dtos.RegionIsoCodes regionIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing regionIsoCodes
        /// </summary>
        /// <param name="guid">GUID of the regionIsoCodes to update</param>
        /// <param name="regionIsoCodes">DTO of the updated regionIsoCodes</param>
        /// <returns>A regionIsoCodes object <see cref="Dtos.RegionIsoCodes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.RegionIsoCodes> PutRegionIsoCodesAsync([FromUri] string guid, [FromBody] Dtos.RegionIsoCodes regionIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a regionIsoCodes
        /// </summary>
        /// <param name="guid">GUID to desired regionIsoCodes</param>
        [HttpDelete]
        public async Task DeleteRegionIsoCodesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}