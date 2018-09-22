// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
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
    /// Provide access to grade mode
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GradeModesController :BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GradeModeController class.
        /// </summary>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public GradeModesController(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves information for all grade modes.
        /// </summary>
        /// <returns>All <see cref="Dtos.GradeMode">GradeModes</see></returns>
        public async Task<IEnumerable<Dtos.GradeMode>> GetGradeModesAsync()
        {
            //GET is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        
        }

        /// <summary>
        /// Retrieves information for all grade modes.
        /// </summary>
        /// <returns>All <see cref="Dtos.GradeMode">GradeModes</see></returns>
        public async Task<IEnumerable<Dtos.GradeMode>> GetGradeModes2Async()
        {
            return new List<Dtos.GradeMode>();
        }

        /// <summary>
        /// Retrieves grade mode by id
        /// </summary>
        /// <param name="id">The id of the grade mode</param>
        /// <returns>The requested <see cref="Dtos.GradeMode">GradeMode</see></returns>
        public async Task<Dtos.GradeMode> GetGradeModeByIdAsync(string id)
        {
            //GET by id is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        
        }


        /// <summary>
        /// Retrieves grade mode by id
        /// </summary>
        /// <param name="id">The id of the grade mode</param>
        /// <returns>The requested <see cref="Dtos.GradeMode">GradeMode</see></returns>
        public async Task<Dtos.GradeMode> GetGradeModeById2Async(string id)
        {
            try
            {
                throw new Exception(string.Format("No grade mode was found for guid {0}.", id));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), System.Net.HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Create a grade mode
        /// </summary>
        /// <param name="gradeMode">grade</param>
        /// <returns>A section object <see cref="Dtos.GradeMode"/> in HeDM format</returns>
        public async Task<Dtos.GradeMode> PostGradeModeAsync([FromBody] Ellucian.Colleague.Dtos.GradeMode gradeMode)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update a grade mode
        /// </summary>
        /// <param name="id">desired id for a grade mode</param>
        /// <param name="gradeMode">grade mode</param>
        /// <returns>A section object <see cref="Dtos.GradeMode"/> in HeDM format</returns>
        public async Task<Dtos.GradeMode> PutGradeModeAsync([FromUri] string id, [FromBody] Dtos.GradeMode gradeMode)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a grade mode
        /// </summary>
        /// <param name="id">id to desired grade mode</param>
        /// <returns>A section object <see cref="Dtos.GradeMode"/> in HeDM format</returns>
        [HttpDelete]
        public async Task<Dtos.GradeMode> DeleteGradeModeByIdAsync(string id)
        {
            //Delete is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}