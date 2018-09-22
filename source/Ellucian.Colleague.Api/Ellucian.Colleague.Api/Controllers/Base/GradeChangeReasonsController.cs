// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provide access to grade change reason
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GradeChangeReasonsController :BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private IGradeChangeReasonService _gradeChangeReasonService;

        /// <summary>
        /// Initializes a new instance of the GradeChangeReasonController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="gradeChangeReasonService">Service of type <see cref="IGradeChangeReasonService">IGradeChangeReasonService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public GradeChangeReasonsController(IAdapterRegistry adapterRegistry, IGradeChangeReasonService gradeChangeReasonService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _gradeChangeReasonService = gradeChangeReasonService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves information for all grade change reasons.
        /// </summary>
        /// <returns>All <see cref="Dtos.GradeChangeReason">GradeChangeReasons</see></returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.GradeChangeReason>> GetGradeChangeReasonsAsync()
        {
            bool bypassCache = false;
            if(Request.Headers.CacheControl != null)
            {
                if(Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                return await _gradeChangeReasonService.GetAsync(bypassCache);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves grade change reason by id
        /// </summary>
        /// <param name="id">The id of the grade change reason</param>
        /// <returns>The requested <see cref="Dtos.GradeChangeReason">GradeChangeReason</see></returns>
        public async Task<Dtos.GradeChangeReason> GetGradeChangeReasonByIdAsync(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException(id, "Grade Change Reason id cannot be null or empty");
                }
                return await _gradeChangeReasonService.GetGradeChangeReasonByIdAsync(id);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Create a grade change reason
        /// </summary>
        /// <param name="gradeChangeReason">grade</param>
        /// <returns>A section object <see cref="Dtos.GradeChangeReason"/> in HeDM format</returns>
        public async Task<Dtos.GradeChangeReason> PostGradeChangeReasonAsync([FromBody] Ellucian.Colleague.Dtos.GradeChangeReason gradeChangeReason)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update a grade change reason
        /// </summary>
        /// <param name="id">desired id for a grade change reason</param>
        /// <param name="gradeChangeReason">grade change reason</param>
        /// <returns>A section object <see cref="Dtos.GradeChangeReason"/> in HeDM format</returns>
        public async Task<Dtos.GradeChangeReason> PutGradeChangeReasonAsync([FromUri] string id, [FromBody] Dtos.GradeChangeReason gradeChangeReason)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a grade change reason
        /// </summary>
        /// <param name="id">id to desired grade change reason</param>
        /// <returns>A section object <see cref="Dtos.GradeChangeReason"/> in HeDM format</returns>
        [HttpDelete]
        public async Task<Dtos.GradeChangeReason> DeleteGradeChangeReasonByIdAsync(string id)
        {
            //Delete is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}