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
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to InstructorTenureTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class InstructorTenureTypesController : BaseCompressedApiController
    {
        private readonly IInstructorTenureTypesService _instructorTenureTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructorTenureTypesController class.
        /// </summary>
        /// <param name="instructorTenureTypesService">Service of type <see cref="IInstructorTenureTypesService">IInstructorTenureTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public InstructorTenureTypesController(IInstructorTenureTypesService instructorTenureTypesService, ILogger logger)
        {
            _instructorTenureTypesService = instructorTenureTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all instructorTenureTypes
        /// </summary>
        /// <returns>List of InstructorTenureTypes <see cref="Dtos.InstructorTenureTypes"/> objects representing matching instructorTenureTypes</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.InstructorTenureTypes>> GetInstructorTenureTypesAsync()
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                return await _instructorTenureTypesService.GetInstructorTenureTypesAsync(bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a instructorTenureTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired instructorTenureTypes</param>
        /// <returns>A instructorTenureTypes object <see cref="Dtos.InstructorTenureTypes"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.InstructorTenureTypes> GetInstructorTenureTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _instructorTenureTypesService.GetInstructorTenureTypesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new instructorTenureTypes
        /// </summary>
        /// <param name="instructorTenureTypes">DTO of the new instructorTenureTypes</param>
        /// <returns>A instructorTenureTypes object <see cref="Dtos.InstructorTenureTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.InstructorTenureTypes> PostInstructorTenureTypesAsync([FromBody] Dtos.InstructorTenureTypes instructorTenureTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing instructorTenureTypes
        /// </summary>
        /// <param name="guid">GUID of the instructorTenureTypes to update</param>
        /// <param name="instructorTenureTypes">DTO of the updated instructorTenureTypes</param>
        /// <returns>A instructorTenureTypes object <see cref="Dtos.InstructorTenureTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.InstructorTenureTypes> PutInstructorTenureTypesAsync([FromUri] string guid, [FromBody] Dtos.InstructorTenureTypes instructorTenureTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a instructorTenureTypes
        /// </summary>
        /// <param name="guid">GUID to desired instructorTenureTypes</param>
        [HttpDelete]
        public async Task DeleteInstructorTenureTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}