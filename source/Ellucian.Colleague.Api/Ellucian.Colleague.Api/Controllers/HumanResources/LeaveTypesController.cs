//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

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
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to LeaveTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class LeaveTypesController : BaseCompressedApiController
    {
        private readonly ILeaveTypesService _leaveTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the LeaveTypesController class.
        /// </summary>
        /// <param name="leaveTypesService">Service of type <see cref="ILeaveTypesService">ILeaveTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public LeaveTypesController(ILeaveTypesService leaveTypesService, ILogger logger)
        {
            _leaveTypesService = leaveTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all leaveTypes
        /// </summary>
        /// <returns>List of LeaveTypes <see cref="Dtos.LeaveTypes"/> objects representing matching leaveTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.LeaveTypes>> GetLeaveTypesAsync()
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
                var temp = await _leaveTypesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache);
                AddDataPrivacyContextProperty((await _leaveTypesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                var leaveTypes = await _leaveTypesService.GetLeaveTypesAsync(bypassCache);

                if (leaveTypes != null && leaveTypes.Any())
                {
                    AddEthosContextProperties(await _leaveTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _leaveTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              leaveTypes.Select(a => a.Id).ToList()));
                }

                return leaveTypes;                
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
        /// Read (GET) a leaveTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired leaveTypes</param>
        /// <returns>A leaveTypes object <see cref="Dtos.LeaveTypes"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.LeaveTypes> GetLeaveTypesByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
               AddDataPrivacyContextProperty((await _leaveTypesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                    await _leaveTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _leaveTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _leaveTypesService.GetLeaveTypesByGuidAsync(guid);
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
        /// Create (POST) a new leaveTypes
        /// </summary>
        /// <param name="leaveTypes">DTO of the new leaveTypes</param>
        /// <returns>A leaveTypes object <see cref="Dtos.LeaveTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.LeaveTypes> PostLeaveTypesAsync([FromBody] Dtos.LeaveTypes leaveTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing leaveTypes
        /// </summary>
        /// <param name="guid">GUID of the leaveTypes to update</param>
        /// <param name="leaveTypes">DTO of the updated leaveTypes</param>
        /// <returns>A leaveTypes object <see cref="Dtos.LeaveTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.LeaveTypes> PutLeaveTypesAsync([FromUri] string guid, [FromBody] Dtos.LeaveTypes leaveTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a leaveTypes
        /// </summary>
        /// <param name="guid">GUID to desired leaveTypes</param>
        [HttpDelete]
        public async Task DeleteLeaveTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}