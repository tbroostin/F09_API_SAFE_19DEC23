// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Controllers;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides a API controller for fetching country codes.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CountriesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly ICountriesService _countriesService;

        /// <summary>
        /// Initializes a new instance of the CountriesController class.
        /// </summary>
        /// <param name="countriesService">Service of type <see cref="ICountriesService">ICountriesService</see></param>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public CountriesController(ICountriesService countriesService, IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this._adapterRegistry = adapterRegistry;
            this._referenceDataRepository = referenceDataRepository;
            this._logger = logger;
            this._countriesService = countriesService;
        }

        #region countries

        /// <summary>
        /// Gets information for all Country codes
        /// </summary>
        /// <returns>List of Country Dtos</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Country>> GetAsync()
        {
            try
            {
                var countryDtoCollection = new List<Ellucian.Colleague.Dtos.Base.Country>();
                var countryCollection = await _referenceDataRepository.GetCountryCodesAsync(false);
                // Get the right adapter for the type mapping
                var countryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Ellucian.Colleague.Dtos.Base.Country>();
                // Map the grade entity to the grade DTO
                if (countryCollection != null && countryCollection.Count() > 0)
                {
                    foreach (var country in countryCollection)
                    {
                        countryDtoCollection.Add(countryDtoAdapter.MapToType(country));
                    }
                }

                return countryDtoCollection;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve Countries.");
            }
        }

        /// <summary>
        /// Return all countries
        /// </summary>
        /// <returns>List of Countries <see cref="Dtos.Countries"/> objects representing matching countries</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Countries>> GetCountriesAsync()
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
                var countries = await _countriesService.GetCountriesAsync(bypassCache);

                if (countries != null && countries.Any())
                {
                    AddEthosContextProperties(await _countriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _countriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              countries.Select(a => a.Id).ToList()));
                }
                return countries;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a countries using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired countries</param>
        /// <returns>A countries object <see cref="Dtos.Countries"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Countries> GetCountriesByGuidAsync(string guid)
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
                   await _countriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _countriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _countriesService.GetCountriesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new countries
        /// </summary>
        /// <param name="countries">DTO of the new countries</param>
        /// <returns>A countries object <see cref="Dtos.Countries"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.Countries> PostCountriesAsync([FromBody] Dtos.Countries countries)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing countries
        /// </summary>
        /// <param name="guid">GUID of the countries to update</param>
        /// <param name="countries">DTO of the updated countries</param>
        /// <returns>A countries object <see cref="Dtos.Countries"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Countries> PutCountriesAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Countries countries)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (countries == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null comment argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(countries.Id))
            {
                countries.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(countries.Id, Guid.Empty.ToString())))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != countries.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _countriesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _countriesService.ImportExtendedEthosData(await ExtractExtendedData(await _countriesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var countriesReturn = await _countriesService.PutCountriesAsync(guid,
                    await PerformPartialPayloadMerge(countries, async () => await _countriesService.GetCountriesByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _countriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return countriesReturn;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Delete (DELETE) a countries
        /// </summary>
        /// <param name="guid">GUID to desired countries</param>
        [HttpDelete]
        public async Task DeleteCountriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        #endregion

        #region country-iso-codes

        /// <summary>
        /// Return all countryIsoCodes
        /// </summary>
        /// <returns>List of CountryIsoCodes <see cref="Dtos.CountryIsoCodes"/> objects representing matching countryIsoCodes</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CountryIsoCodes>> GetCountryIsoCodesAsync()
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
                var countryIsoCodes = await _countriesService.GetCountryIsoCodesAsync(bypassCache);

                if (countryIsoCodes != null && countryIsoCodes.Any())
                {
                    AddEthosContextProperties(await _countriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _countriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              countryIsoCodes.Select(a => a.Id).ToList()));
                }
                return countryIsoCodes;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a countryIsoCodes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired countryIsoCodes</param>
        /// <returns>A countryIsoCodes object <see cref="Dtos.CountryIsoCodes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.CountryIsoCodes> GetCountryIsoCodesByGuidAsync(string guid)
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
                   await _countriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _countriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _countriesService.GetCountryIsoCodesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new countryIsoCodes
        /// </summary>
        /// <param name="countryIsoCodes">DTO of the new countryIsoCodes</param>
        /// <returns>A countryIsoCodes object <see cref="Dtos.CountryIsoCodes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.CountryIsoCodes> PostCountryIsoCodesAsync([FromBody] Dtos.CountryIsoCodes countryIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing countryIsoCodes
        /// </summary>
        /// <param name="guid">GUID of the countryIsoCodes to update</param>
        /// <param name="countryIsoCodes">DTO of the updated countryIsoCodes</param>
        /// <returns>A countryIsoCodes object <see cref="Dtos.CountryIsoCodes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.CountryIsoCodes> PutCountryIsoCodesAsync([FromUri] string guid, [FromBody] Dtos.CountryIsoCodes countryIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a countryIsoCodes
        /// </summary>
        /// <param name="guid">GUID to desired countryIsoCodes</param>
        [HttpDelete]
        public async Task DeleteCountryIsoCodesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        #endregion
    }
}