//Copyright 2018 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to PurchasingArrangements
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class PurchasingArrangementsController : BaseCompressedApiController
    {
        
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PurchasingArrangementsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public PurchasingArrangementsController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all purchasing-arrangements
        /// </summary>
        /// <returns>All <see cref="Dtos.PurchasingArrangement">PurchasingArrangements</see></returns>
        public async Task<IEnumerable<Dtos.PurchasingArrangement>> GetPurchasingArrangementsAsync()
        {
            return new List<Dtos.PurchasingArrangement>();
        }

        /// <summary>
        /// Retrieve (GET) an existing purchasing-arrangements
        /// </summary>
        /// <param name="guid">GUID of the purchasing-arrangements to get</param>
        /// <returns>A purchasingArrangements object <see cref="Dtos.PurchasingArrangement"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.PurchasingArrangement> GetPurchasingArrangementsByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new Exception(string.Format("No purchasing-arrangements was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new purchasingArrangements
        /// </summary>
        /// <param name="purchasingArrangements">DTO of the new purchasingArrangements</param>
        /// <returns>A purchasingArrangements object <see cref="Dtos.PurchasingArrangement"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PurchasingArrangement> PostPurchasingArrangementsAsync([FromBody] Dtos.PurchasingArrangement purchasingArrangements)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing purchasingArrangement
        /// </summary>
        /// <param name="guid">GUID of the purchasingArrangement to update</param>
        /// <param name="purchasingArrangement">DTO of the updated purchasingArrangement</param>
        /// <returns>A purchasingArrangements object <see cref="Dtos.PurchasingArrangement"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PurchasingArrangement> PutPurchasingArrangementsAsync([FromUri] string guid, [FromBody] Dtos.PurchasingArrangement purchasingArrangement)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a purchasingArrangement
        /// </summary>
        /// <param name="guid">GUID to desired purchasingArrangement</param>
        [HttpDelete]
        public async Task DeletePurchasingArrangementAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}