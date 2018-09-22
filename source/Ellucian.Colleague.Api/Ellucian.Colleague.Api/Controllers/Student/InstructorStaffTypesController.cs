//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
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
    /// Provides access to InstructorStaffTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructorStaffTypesController : BaseCompressedApiController
    {
        private readonly IInstructorStaffTypesService _instructorStaffTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructorStaffTypesController class.
        /// </summary>
        /// <param name="instructorStaffTypesService">Service of type <see cref="IInstructorStaffTypesService">IInstructorStaffTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public InstructorStaffTypesController(IInstructorStaffTypesService instructorStaffTypesService, ILogger logger)
        {
            _instructorStaffTypesService = instructorStaffTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all instructorStaffTypes
        /// </summary>
        /// <returns>List of InstructorStaffTypes <see cref="Dtos.InstructorStaffTypes"/> objects representing matching instructorStaffTypes</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.InstructorStaffTypes>> GetInstructorStaffTypesAsync()
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
                return await _instructorStaffTypesService.GetInstructorStaffTypesAsync(bypassCache);
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
        /// Read (GET) a instructorStaffTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired instructorStaffTypes</param>
        /// <returns>A instructorStaffTypes object <see cref="Dtos.InstructorStaffTypes"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.InstructorStaffTypes> GetInstructorStaffTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _instructorStaffTypesService.GetInstructorStaffTypesByGuidAsync(guid);
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
        /// Create (POST) a new instructorStaffTypes
        /// </summary>
        /// <param name="instructorStaffTypes">DTO of the new instructorStaffTypes</param>
        /// <returns>A instructorStaffTypes object <see cref="Dtos.InstructorStaffTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.InstructorStaffTypes> PostInstructorStaffTypesAsync([FromBody] Dtos.InstructorStaffTypes instructorStaffTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing instructorStaffTypes
        /// </summary>
        /// <param name="guid">GUID of the instructorStaffTypes to update</param>
        /// <param name="instructorStaffTypes">DTO of the updated instructorStaffTypes</param>
        /// <returns>A instructorStaffTypes object <see cref="Dtos.InstructorStaffTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.InstructorStaffTypes> PutInstructorStaffTypesAsync([FromUri] string guid, [FromBody] Dtos.InstructorStaffTypes instructorStaffTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a instructorStaffTypes
        /// </summary>
        /// <param name="guid">GUID to desired instructorStaffTypes</param>
        [HttpDelete]
        public async Task DeleteInstructorStaffTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}