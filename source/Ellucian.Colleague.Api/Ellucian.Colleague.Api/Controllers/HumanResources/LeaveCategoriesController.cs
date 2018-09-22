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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to LeaveCategories
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class LeaveCategoriesController : BaseCompressedApiController
    {
        
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the LeaveCategoriesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public LeaveCategoriesController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all leave-categories
        /// </summary>
        /// <returns>All <see cref="Dtos.LeaveCategories">LeaveCategories</see></returns>
        public async Task<IEnumerable<LeaveCategories>> GetLeaveCategoriesAsync()
        {
            return new List<LeaveCategories>();
        }

        /// <summary>
        /// Retrieve (GET) an existing leave-categories
        /// </summary>
        /// <param name="guid">GUID of the leave-categories to get</param>
        /// <returns>A leaveCategories object <see cref="Dtos.LeaveCategories"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.LeaveCategories> GetLeaveCategoriesByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new Exception(string.Format("No leave-categories was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new leaveCategories
        /// </summary>
        /// <param name="leaveCategories">DTO of the new leaveCategories</param>
        /// <returns>A leaveCategories object <see cref="Dtos.LeaveCategories"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.LeaveCategories> PostLeaveCategoriesAsync([FromBody] Dtos.LeaveCategories leaveCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing leaveCategories
        /// </summary>
        /// <param name="guid">GUID of the leaveCategories to update</param>
        /// <param name="leaveCategories">DTO of the updated leaveCategories</param>
        /// <returns>A leaveCategories object <see cref="Dtos.LeaveCategories"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.LeaveCategories> PutLeaveCategoriesAsync([FromUri] string guid, [FromBody] Dtos.LeaveCategories leaveCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a leaveCategories
        /// </summary>
        /// <param name="guid">GUID to desired leaveCategories</param>
        [HttpDelete]
        public async Task DeleteLeaveCategoriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}