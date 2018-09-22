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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Buyers
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class BuyersController : BaseCompressedApiController
    {
        private readonly IBuyersService _buyersService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BuyersController class.
        /// </summary>
        /// <param name="buyersService">Service of type <see cref="IBuyersService">IBuyersService</see></param>
        /// <param name="logger">Interface to logger</param>
        public BuyersController(IBuyersService buyersService, ILogger logger)
        {
            _buyersService = buyersService;
            _logger = logger;
        }

        /// <summary>
        /// Return all buyers
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
                /// <returns>List of Buyers <see cref="Dtos.Buyers"/> objects representing matching buyers</returns>
        [HttpGet]    
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetBuyersAsync(Paging page)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _buyersService.GetBuyersAsync(page.Offset, page.Limit, bypassCache);


                AddEthosContextProperties(
                  await _buyersService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _buyersService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));


                return new PagedHttpActionResult<IEnumerable<Dtos.Buyers>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a buyers using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired buyers</param>
        /// <returns>A buyers object <see cref="Dtos.Buyers"/> in EEDM format</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.Buyers> GetBuyersByGuidAsync(string guid)
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
                var buyer =  await _buyersService.GetBuyersByGuidAsync(guid);  // TODO: Honor bypassCache

                if (buyer != null)
                {

                    AddEthosContextProperties(await _buyersService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _buyersService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { buyer.Id }));
                }
                return buyer;
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
        /// Create (POST) a new buyers
        /// </summary>
        /// <param name="buyers">DTO of the new buyers</param>
        /// <returns>A buyers object <see cref="Dtos.Buyers"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.Buyers> PostBuyersAsync([FromBody] Dtos.Buyers buyers)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing buyers
        /// </summary>
        /// <param name="guid">GUID of the buyers to update</param>
        /// <param name="buyers">DTO of the updated buyers</param>
        /// <returns>A buyers object <see cref="Dtos.Buyers"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.Buyers> PutBuyersAsync([FromUri] string guid, [FromBody] Dtos.Buyers buyers)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a buyers
        /// </summary>
        /// <param name="guid">GUID to desired buyers</param>
        [HttpDelete]
        public async Task DeleteBuyersAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}