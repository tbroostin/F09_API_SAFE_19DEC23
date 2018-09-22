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
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to ShipToDestinations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class ShipToDestinationsController : BaseCompressedApiController
    {
        private readonly IShipToDestinationsService _shipToDestinationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ShipToDestinationsController class.
        /// </summary>
        /// <param name="shipToDestinationsService">Service of type <see cref="IShipToDestinationsService">IShipToDestinationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public ShipToDestinationsController(IShipToDestinationsService shipToDestinationsService, ILogger logger)
        {
            _shipToDestinationsService = shipToDestinationsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all shipToDestinations
        /// </summary>
        /// <returns>List of ShipToDestinations <see cref="Dtos.ShipToDestinations"/> objects representing matching shipToDestinations</returns>
        [HttpGet]
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ShipToDestinations>> GetShipToDestinationsAsync()
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
                var dtos = await _shipToDestinationsService.GetShipToDestinationsAsync(bypassCache);

                AddEthosContextProperties(
                    await _shipToDestinationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _shipToDestinationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        dtos.Select(i => i.Id).ToList()));

                return dtos;
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
        /// Read (GET) a shipToDestinations using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired shipToDestinations</param>
        /// <returns>A shipToDestinations object <see cref="Dtos.ShipToDestinations"/> in EEDM format</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.ShipToDestinations> GetShipToDestinationsByGuidAsync(string guid)
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
                AddEthosContextProperties(
                    await _shipToDestinationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _shipToDestinationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _shipToDestinationsService.GetShipToDestinationsByGuidAsync(guid);
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
        /// Create (POST) a new shipToDestinations
        /// </summary>
        /// <param name="shipToDestinations">DTO of the new shipToDestinations</param>
        /// <returns>A shipToDestinations object <see cref="Dtos.ShipToDestinations"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.ShipToDestinations> PostShipToDestinationsAsync([FromBody] Dtos.ShipToDestinations shipToDestinations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing shipToDestinations
        /// </summary>
        /// <param name="guid">GUID of the shipToDestinations to update</param>
        /// <param name="shipToDestinations">DTO of the updated shipToDestinations</param>
        /// <returns>A shipToDestinations object <see cref="Dtos.ShipToDestinations"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.ShipToDestinations> PutShipToDestinationsAsync([FromUri] string guid, [FromBody] Dtos.ShipToDestinations shipToDestinations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a shipToDestinations
        /// </summary>
        /// <param name="guid">GUID to desired shipToDestinations</param>
        [HttpDelete]
        public async Task DeleteShipToDestinationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}