// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Visa Type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class VisaTypesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly IDemographicService _demographicService;

        /// <summary>
        /// Initializes a new instance of the VisaTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public VisaTypesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IDemographicService demographicService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _demographicService = demographicService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Visa Types.
        /// </summary>
        /// <returns>All <see cref="VisaType">Visa Type codes and descriptions.</see></returns>
        public IEnumerable<VisaType> Get()
        {
            var visaTypeCollection = _referenceDataRepository.VisaTypes;

            // Get the right adapter for the type mapping
            var visaTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.VisaType, VisaType>();

            // Map the visaType entity to the program DTO
            var visaTypeDtoCollection = new List<VisaType>();
            foreach (var visaType in visaTypeCollection)
            {
                visaTypeDtoCollection.Add(visaTypeDtoAdapter.MapToType(visaType));
            }

            return visaTypeDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Visa Types
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Dtos.VisaType">Visa Types.</see></returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.VisaType>> GetVisaTypesAsync()
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
                return await _demographicService.GetVisaTypesAsync(bypassCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Visa Type by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.VisaType">Visa Type.</see></returns>
        public async Task<Dtos.VisaType> GetVisaTypeByIdAsync(string id)
        {
            try
            {
                return await _demographicService.GetVisaTypeByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>        
        /// Creates an Visa Type
        /// </summary>
        /// <param name="visaType"><see cref="Dtos.VisaType">VisaType</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.VisaType">VisaType</see></returns>
        [HttpPost]
        public async Task<Dtos.VisaType> PostVisaTypeAsync([FromBody] Dtos.VisaType visaType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>        
        /// Updates an Visa Type.
        /// </summary>
        /// <param name="id">Id of the Visa Type to update</param>
        /// <param name="visaType"><see cref="Dtos.VisaType">VisaType</see> to create</param>
        /// <returns>Updated <see cref="Dtos.VisaType">VisaType</see></returns>
        [HttpPut]
        public async Task<Dtos.VisaType> PutVisaTypeAsync([FromUri] string id, [FromBody] Dtos.VisaType visaType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Visa Type
        /// </summary>
        /// <param name="id">Id of the Visa Type to delete</param>
        [HttpDelete]
        public async Task DeleteVisaTypeAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
