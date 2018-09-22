// Copyright 2015-18 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Geographic Area Types data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GeographicAreasController : BaseCompressedApiController
    {
        private readonly IGeographicAreaService _geographicAreaService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GeographicAreasController class.
        /// </summary>
        /// <param name="geographicAreaService">Service of type <see cref="IGeographicAreaService">IGeographicAreaService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public GeographicAreasController(IGeographicAreaService geographicAreaService, ILogger logger)
        {
            _geographicAreaService = geographicAreaService;
            _logger = logger;
        }
        
        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves all geographic areas.
        /// </summary>
        /// <returns>All GeographicArea objects.</returns>
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetGeographicAreasAsync(Paging page)
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _geographicAreaService.GetGeographicAreasAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _geographicAreaService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _geographicAreaService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.GeographicArea>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves a geographic area by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.GeographicAreas">GeographicArea.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.GeographicArea> GetGeographicAreaByIdAsync(string id)
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var area = await _geographicAreaService.GetGeographicAreaByGuidAsync(id);

                if (area != null)
                {

                    AddEthosContextProperties(await _geographicAreaService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _geographicAreaService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { area.Id }));
                }


                return area;
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
        /// Updates a GeographicArea.
        /// </summary>
        /// <param name="geographicArea"><see cref="GeographicArea">GeographicArea</see> to update</param>
        /// <returns>Newly updated <see cref="GeographicArea">GeographicArea</see></returns>
        [HttpPut]
        public async Task<Dtos.GeographicArea> PutGeographicAreaAsync([FromBody] Dtos.GeographicArea geographicArea)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a GeographicArea.
        /// </summary>
        /// <param name="geographicArea"><see cref="GeographicArea">GeographicArea</see> to create</param>
        /// <returns>Newly created <see cref="GeographicArea">GeographicArea</see></returns>
        [HttpPost]
        public async Task<Dtos.GeographicArea> PostGeographicAreaAsync([FromBody] Dtos.GeographicArea geographicArea)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing GeographicArea
        /// </summary>
        /// <param name="id">Id of the GeographicArea to delete</param>
        [HttpDelete]
        public async Task DeleteGeographicAreaAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
