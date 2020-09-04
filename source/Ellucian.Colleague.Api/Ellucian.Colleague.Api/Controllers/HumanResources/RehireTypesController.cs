// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Rehire Types data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RehireTypesController : BaseCompressedApiController
    {
        private readonly IRehireTypeService _rehireTypeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RehireTypesController class.
        /// </summary>
        /// <param name="rehireTypeService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public RehireTypesController(IRehireTypeService rehireTypeService, ILogger logger)
        {
            _rehireTypeService = rehireTypeService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves all rehire types.
        /// </summary>
        /// <returns>All RehireType objects.</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RehireType>> GetRehireTypesAsync()
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
                var allRehireTypes = await _rehireTypeService.GetRehireTypesAsync(bypassCache);

                if (allRehireTypes != null && allRehireTypes.Any())
                {
                    AddEthosContextProperties(await _rehireTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _rehireTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              allRehireTypes.Select(a => a.Id).ToList()));
                }

                return allRehireTypes;                
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves a rehire type by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.RehireType">RehireType.</see></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.RehireType> GetRehireTypeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _rehireTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _rehireTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _rehireTypeService.GetRehireTypeByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a RehireType.
        /// </summary>
        /// <param name="rehireType"><see cref="RehireType">RehireType</see> to update</param>
        /// <returns>Newly updated <see cref="RehireType">RehireType</see></returns>
        [HttpPut]
        public async Task<Dtos.RehireType> PutRehireTypeAsync([FromBody] Dtos.RehireType rehireType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a RehireType.
        /// </summary>
        /// <param name="rehireType"><see cref="RehireType">RehireType</see> to create</param>
        /// <returns>Newly created <see cref="RehireType">RehireType</see></returns>
        [HttpPost]
        public async Task<Dtos.RehireType> PostRehireTypeAsync([FromBody] Dtos.RehireType rehireType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing RehireType
        /// </summary>
        /// <param name="id">Id of the RehireType to delete</param>
        [HttpDelete]
        public async Task DeleteRehireTypeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
