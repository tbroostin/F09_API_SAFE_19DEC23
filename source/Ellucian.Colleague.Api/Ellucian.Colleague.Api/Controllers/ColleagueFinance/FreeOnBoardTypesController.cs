//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to FreeOnBoardTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class FreeOnBoardTypesController : BaseCompressedApiController
    {
        private readonly IFreeOnBoardTypesService _freeOnBoardTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FreeOnBoardTypesController class.
        /// </summary>
        /// <param name="freeOnBoardTypesService">Service of type <see cref="IFreeOnBoardTypesService">IFreeOnBoardTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public FreeOnBoardTypesController(IFreeOnBoardTypesService freeOnBoardTypesService, ILogger logger)
        {
            _freeOnBoardTypesService = freeOnBoardTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all freeOnBoardTypes
        /// </summary>
        /// <returns>List of FreeOnBoardTypes <see cref="Dtos.FreeOnBoardTypes"/> objects representing matching freeOnBoardTypes</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FreeOnBoardTypes>> GetFreeOnBoardTypesAsync()
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
                var items = await _freeOnBoardTypesService.GetFreeOnBoardTypesAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _freeOnBoardTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _freeOnBoardTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(a => a.Id).ToList()));
                }

                return items;
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
        /// Read (GET) a freeOnBoardTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired freeOnBoardTypes</param>
        /// <returns>A freeOnBoardTypes object <see cref="Dtos.FreeOnBoardTypes"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.FreeOnBoardTypes> GetFreeOnBoardTypesByGuidAsync(string guid)
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
                return await _freeOnBoardTypesService.GetFreeOnBoardTypesByGuidAsync(guid);
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
        /// Create (POST) a new freeOnBoardTypes
        /// </summary>
        /// <param name="freeOnBoardTypes">DTO of the new freeOnBoardTypes</param>
        /// <returns>A freeOnBoardTypes object <see cref="Dtos.FreeOnBoardTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.FreeOnBoardTypes> PostFreeOnBoardTypesAsync([FromBody] Dtos.FreeOnBoardTypes freeOnBoardTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing freeOnBoardTypes
        /// </summary>
        /// <param name="guid">GUID of the freeOnBoardTypes to update</param>
        /// <param name="freeOnBoardTypes">DTO of the updated freeOnBoardTypes</param>
        /// <returns>A freeOnBoardTypes object <see cref="Dtos.FreeOnBoardTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.FreeOnBoardTypes> PutFreeOnBoardTypesAsync([FromUri] string guid, [FromBody] Dtos.FreeOnBoardTypes freeOnBoardTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a freeOnBoardTypes
        /// </summary>
        /// <param name="guid">GUID to desired freeOnBoardTypes</param>
        [HttpDelete]
        public async Task DeleteFreeOnBoardTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}