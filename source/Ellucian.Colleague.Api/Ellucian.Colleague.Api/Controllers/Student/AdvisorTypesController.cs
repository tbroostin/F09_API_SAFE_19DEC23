// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Advisor Type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdvisorTypesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IAdvisorTypesService _advisorTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdvisorTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="advisorTypesService">Service of type <see cref="IAdvisorTypesService">IAdvisorTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AdvisorTypesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IAdvisorTypesService advisorTypesService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _advisorTypesService = advisorTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all of the Advisor Types
        /// </summary>
        /// <returns>All <see cref="AdvisorType">AdvisorTypes</see></returns>
        public async Task<IEnumerable<AdvisorType>> GetAsync()
        {
            try
            {
                var advisorTypeCollection = await _referenceDataRepository.GetAdvisorTypesAsync();

                // Get the right adapter for the type mapping
                var advisorTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AdvisorType, AdvisorType>();

                // Map the AdvisorType entity to the program DTO
                var advisorTypeDtoCollection = new List<AdvisorType>();
                foreach (var advisorType in advisorTypeCollection)
                {
                    advisorTypeDtoCollection.Add(advisorTypeDtoAdapter.MapToType(advisorType));
                }

                return advisorTypeDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving advisor types";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Exception occurred while retrieving advisor types";
                _logger.Error(ex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Return all advisorTypes
        /// </summary>
        /// <returns>List of AdvisorTypes <see cref="Dtos.AdvisorTypes"/> objects representing matching advisorTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdvisorTypes>> GetAdvisorTypesAsync()
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
                var advisorTypes = await _advisorTypesService.GetAdvisorTypesAsync(bypassCache);

                if (advisorTypes != null && advisorTypes.Any())
                {
                    AddEthosContextProperties(await _advisorTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _advisorTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              advisorTypes.Select(a => a.Id).ToList()));
                }

                return advisorTypes;                
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
        /// Read (GET) a advisorTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired advisorTypes</param>
        /// <returns>A advisorTypes object <see cref="Dtos.AdvisorTypes"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AdvisorTypes> GetAdvisorTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _advisorTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _advisorTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _advisorTypesService.GetAdvisorTypesByGuidAsync(guid);
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
        /// Create (POST) a new advisorTypes
        /// </summary>
        /// <param name="advisorTypes">DTO of the new advisorTypes</param>
        /// <returns>A advisorTypes object <see cref="Dtos.AdvisorTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AdvisorTypes> PostAdvisorTypesAsync([FromBody] Dtos.AdvisorTypes advisorTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing advisorTypes
        /// </summary>
        /// <param name="guid">GUID of the advisorTypes to update</param>
        /// <param name="advisorTypes">DTO of the updated advisorTypes</param>
        /// <returns>A advisorTypes object <see cref="Dtos.AdvisorTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AdvisorTypes> PutAdvisorTypesAsync([FromUri] string guid, [FromBody] Dtos.AdvisorTypes advisorTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a advisorTypes
        /// </summary>
        /// <param name="guid">GUID to desired advisorTypes</param>
        [HttpDelete]
        public async Task DeleteAdvisorTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
