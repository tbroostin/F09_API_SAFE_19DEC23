// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Tax Codes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CommerceTaxCodesController : BaseCompressedApiController
    {
        private readonly ICommerceTaxCodeService _taxCodeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TaxCodesController class.
        /// </summary>
        /// <param name="taxCodeService">Service of type <see cref="ICommerceTaxCodeService">ITaxCodeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public CommerceTaxCodesController(ICommerceTaxCodeService taxCodeService, ILogger logger)
        {
            _taxCodeService = taxCodeService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8</remarks>
        /// <summary>
        /// Retrieves all tax codes.
        /// </summary>
        /// <returns>All CommerceTaxCode objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CommerceTaxCode>> GetCommerceTaxCodesAsync()
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

                var items = await _taxCodeService.GetCommerceTaxCodesAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _taxCodeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _taxCodeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(a => a.Id).ToList()));
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8</remarks>
        /// <summary>
        /// Retrieves a commerce tax code by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CommerceTaxCode">CommerceTaxCode.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.CommerceTaxCode> GetCommerceTaxCodeByIdAsync(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

            try
            {
                var item = await _taxCodeService.GetCommerceTaxCodeByGuidAsync(id);

                if (item != null)
                {
                    AddEthosContextProperties(await _taxCodeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _taxCodeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { item.Id }));
                }

                return item;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates a CommerceTaxCode.
        /// </summary>
        /// <param name="taxCode"><see cref="CommerceTaxCode">CommerceTaxCode</see> to update</param>
        /// <returns>Newly updated <see cref="CommerceTaxCode">CommerceTaxCode</see></returns>
        [HttpPut]
        public async Task<Dtos.CommerceTaxCode> PutCommerceTaxCodeAsync([FromBody] Dtos.CommerceTaxCode taxCode)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a CommerceTaxCode.
        /// </summary>
        /// <param name="taxCode"><see cref="CommerceTaxCode">CommerceTaxCode</see> to create</param>
        /// <returns>Newly created <see cref="CommerceTaxCode">CommerceTaxCode</see></returns>
        [HttpPost]
        public async Task<Dtos.CommerceTaxCode> PostCommerceTaxCodeAsync([FromBody] Dtos.CommerceTaxCode taxCode)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing CommerceTaxCode
        /// </summary>
        /// <param name="id">Id of the CommerceTaxCode to delete</param>
        [HttpDelete]
        public async Task DeleteCommerceTaxCodeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
