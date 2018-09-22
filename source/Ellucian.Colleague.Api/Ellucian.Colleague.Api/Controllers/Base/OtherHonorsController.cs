// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
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

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Controller for Other Honors
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class OtherHonorsController : BaseCompressedApiController
    {
       private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IOtherHonorService _otherHonorService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the Other HonorController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="otherHonorService">Service of type <see cref="IOtherHonorService">IOtherHonorService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public OtherHonorsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IOtherHonorService otherHonorService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _otherHonorService = otherHonorService;
            _logger = logger;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Other Honors
        /// </summary>
        /// <returns>All <see cref="OtherHonors">OtherHonors.</see></returns>        
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.OtherHonor>> GetOtherHonorAsync()
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var items = await _otherHonorService.GetOtherHonorsAsync(bypassCache);

                AddEthosContextProperties(
                  await _otherHonorService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _otherHonorService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Other Honor by ID.
        /// </summary>
        /// <returns>A <see cref="OtherHonors">OtherHonor.</see></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.OtherHonor> GetOtherHonorByIdAsync(string id)
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
                AddEthosContextProperties(
                  await _otherHonorService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _otherHonorService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _otherHonorService.GetOtherHonorByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a OtherHonor.
        /// </summary>
        /// <param name="otherHonor"><see cref="OtherHonor">OtherHonor</see> to update</param>
        /// <returns>Newly updated <see cref="OtherHonor">OtherHonor</see></returns>
        [HttpPut]
        public Dtos.OtherHonor PutOtherHonors([FromBody] Dtos.OtherHonor otherHonor)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a OtherHonor.
        /// </summary>
        /// <param name="otherHonor"><see cref="OtherHonor">OtherHonor</see> to create</param>
        /// <returns>Newly created <see cref="OtherHonor">OtherHonor</see></returns>
        [HttpPost]
        public Dtos.OtherHonor PostOtherHonors([FromBody] Dtos.OtherHonor otherHonor)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing OtherHonor
        /// </summary>
        /// <param name="id">Id of the OtherHonor to delete</param>
        [HttpDelete]
        public Dtos.OtherHonor DeleteOtherHonors(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
