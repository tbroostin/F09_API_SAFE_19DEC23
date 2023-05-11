//Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
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

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to ProspectOpportunitySources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ProspectOpportunitySourcesController : BaseCompressedApiController
    {
        
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ProspectOpportunitySourcesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public ProspectOpportunitySourcesController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all prospect-opportunity-sources
        /// </summary>
        /// <returns>All <see cref="Dtos.ProspectOpportunitySources">ProspectOpportunitySources</see></returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<IEnumerable<Dtos.ProspectOpportunitySources>> GetProspectOpportunitySourcesAsync()
        {
            return new List<Dtos.ProspectOpportunitySources>();
        }

        /// <summary>
        /// Retrieve (GET) an existing prospect-opportunity-sources
        /// </summary>
        /// <param name="guid">GUID of the prospect-opportunity-sources to get</param>
        /// <returns>A prospectOpportunitySources object <see cref="Dtos.ProspectOpportunitySources"/> in EEDM format</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.ProspectOpportunitySources> GetProspectOpportunitySourcesByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No prospect-opportunity-sources was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new prospectOpportunitySources
        /// </summary>
        /// <param name="prospectOpportunitySources">DTO of the new prospectOpportunitySources</param>
        /// <returns>A prospectOpportunitySources object <see cref="Dtos.ProspectOpportunitySources"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.ProspectOpportunitySources> PostProspectOpportunitySourcesAsync([FromBody] Dtos.ProspectOpportunitySources prospectOpportunitySources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing prospectOpportunitySources
        /// </summary>
        /// <param name="guid">GUID of the prospectOpportunitySources to update</param>
        /// <param name="prospectOpportunitySources">DTO of the updated prospectOpportunitySources</param>
        /// <returns>A prospectOpportunitySources object <see cref="Dtos.ProspectOpportunitySources"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.ProspectOpportunitySources> PutProspectOpportunitySourcesAsync([FromUri] string guid, [FromBody] Dtos.ProspectOpportunitySources prospectOpportunitySources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a prospectOpportunitySources
        /// </summary>
        /// <param name="guid">GUID to desired prospectOpportunitySources</param>
        [HttpDelete]
        public async Task DeleteProspectOpportunitySourcesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}