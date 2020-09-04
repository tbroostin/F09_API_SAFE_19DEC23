//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to CollectionConfigurationSettings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CollectionConfigurationSettingsController : BaseCompressedApiController
    {
        private readonly ICollectionConfigurationSettingsService _collectionConfigurationSettingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CollectionConfigurationSettingsController class.
        /// </summary>
        /// <param name="collectionConfigurationSettingsService">Service of type <see cref="ICollectionConfigurationSettingsService">ICollectionConfigurationSettingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CollectionConfigurationSettingsController(ICollectionConfigurationSettingsService collectionConfigurationSettingsService, ILogger logger)
        {
            _collectionConfigurationSettingsService = collectionConfigurationSettingsService;
            _logger = logger;
        }

        #region GET collection-configuration-settings
        /// <summary>
        /// Return all collectionConfigurationSettings
        /// </summary>
        /// <returns>List of CollectionConfigurationSettings <see cref="Dtos.CollectionConfigurationSettings"/> objects representing matching collectionConfigurationSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.CollectionConfigurationSettings))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.CollectionConfigurationSettings>> GetCollectionConfigurationSettingsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            List<Dtos.DefaultSettingsEthos> resourcesFilter = new List<Dtos.DefaultSettingsEthos>();
            var criteriaObject = GetFilterObject<Dtos.CollectionConfigurationSettingsOptions>(_logger, "criteria");
            if (CheckForEmptyFilterParameters())
                return new List<Dtos.CollectionConfigurationSettings>();
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos);
            }
            try
            {
                var collectionConfigurationSettings = await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsAsync(resourcesFilter, bypassCache);

                if (collectionConfigurationSettings != null && collectionConfigurationSettings.Any())
                {
                    AddEthosContextProperties(await _collectionConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _collectionConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              collectionConfigurationSettings.Select(a => a.Id).ToList()));
                }
                return collectionConfigurationSettings;
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
        /// Read (GET) a collectionConfigurationSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired collectionConfigurationSettings</param>
        /// <returns>A collectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.CollectionConfigurationSettings> GetCollectionConfigurationSettingsByGuidAsync(string guid)
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
                   await _collectionConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _collectionConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsByGuidAsync(guid, bypassCache);
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
        #endregion

        #region GET collection-configuration-settings-options
        /// <summary>
        /// Return all collectionConfigurationSettings options
        /// </summary>
        /// <returns>List of CollectionConfigurationSettings <see cref="Dtos.CollectionConfigurationSettingsOptions"/> objects representing matching collectionConfigurationSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.CollectionConfigurationSettingsOptions))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CollectionConfigurationSettingsOptions>> GetCollectionConfigurationSettingsOptionsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            List<Dtos.DefaultSettingsEthos> resourcesFilter = new List<Dtos.DefaultSettingsEthos>();
            var criteriaObject = GetFilterObject<Dtos.CollectionConfigurationSettingsOptions>(_logger, "criteria");
            if (CheckForEmptyFilterParameters())
                return new List<Dtos.CollectionConfigurationSettingsOptions>();
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos);
            }
            try
            {
                var collectionConfigurationSettings = await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsOptionsAsync(resourcesFilter, bypassCache);

                if (collectionConfigurationSettings != null && collectionConfigurationSettings.Any())
                {
                    AddEthosContextProperties(await _collectionConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _collectionConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              collectionConfigurationSettings.Select(a => a.Id).ToList()));
                }
                return collectionConfigurationSettings;
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
        /// Read (GET) a collectionConfigurationSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired collectionConfigurationSettings</param>
        /// <returns>A collectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettingsOptions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.CollectionConfigurationSettingsOptions> GetCollectionConfigurationSettingsOptionsByGuidAsync(string guid)
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
                   await _collectionConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _collectionConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsOptionsByGuidAsync(guid, bypassCache);
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

        #endregion

        /// <summary>
        /// Update (PUT) an existing CollectionConfigurationSettings
        /// </summary>
        /// <param name="guid">GUID of the collectionConfigurationSettings to update</param>
        /// <param name="collectionConfigurationSettings">DTO of the updated collectionConfigurationSettings</param>
        /// <returns>A CollectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.CollectionConfigurationSettings> PutCollectionConfigurationSettingsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.CollectionConfigurationSettings collectionConfigurationSettings)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (collectionConfigurationSettings == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null collectionConfigurationSettings argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(collectionConfigurationSettings.Id))
            {
                collectionConfigurationSettings.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, collectionConfigurationSettings.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                var configSettings = await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsByGuidAsync(guid, true);
                var mergedSettings = await PerformPartialPayloadMerge(collectionConfigurationSettings, configSettings,
                  await _collectionConfigurationSettingsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger);

                if (configSettings != null && mergedSettings != null)
                {
                    IntegrationApiException exception = null;

                    if (configSettings.Source != null && mergedSettings.Source != null)
                    {
                        // Check for missing source.value properties
                        var invalidValues = mergedSettings.Source.Where(cs => string.IsNullOrEmpty(cs.Value));
                        if (invalidValues != null && invalidValues.Any())
                        {
                            if (exception == null) exception = new IntegrationApiException();
                            exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The value property is required when defining a sourceSettings object."));
                        }
                        else
                        {
                            // Check for duplicate source settings values
                            var uniqueValues = mergedSettings.Source.Select(cs => cs.Value).Distinct();
                            if (uniqueValues.Count() != mergedSettings.Source.Count())
                            {
                                if (exception == null) exception = new IntegrationApiException();
                                exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "Duplicate values are not allowed when defining a source settings value."));
                            }
                            foreach (var source in configSettings.Source)
                            {
                                var matchingSource = mergedSettings.Source.Where(ms => ms.Value.Equals(source.Value, StringComparison.OrdinalIgnoreCase));
                                foreach (var matchSource in matchingSource)
                                {
                                    if (matchSource.Title != source.Title)
                                    {
                                        if (exception == null) exception = new IntegrationApiException();
                                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The source settings title cannot be changed for a collection configuration setting."));
                                    }
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(mergedSettings.Title) && !mergedSettings.Title.Equals(configSettings.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The title cannot be changed for a collection configuration setting."));
                    }
                    if (!string.IsNullOrEmpty(mergedSettings.Description) && !mergedSettings.Description.Equals(configSettings.Description, StringComparison.OrdinalIgnoreCase))
                    {
                        if ( exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The description cannot be changed for a collection configuration setting."));         
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                }

                return await _collectionConfigurationSettingsService.UpdateCollectionConfigurationSettingsAsync(mergedSettings);
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
            catch (ConfigurationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new collectionConfigurationSettings
        /// </summary>
        /// <param name="collectionConfigurationSettings">DTO of the new collectionConfigurationSettings</param>
        /// <returns>A collectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettings"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.CollectionConfigurationSettings> PostCollectionConfigurationSettingsAsync(Dtos.CollectionConfigurationSettings collectionConfigurationSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing collectionConfigurationSettingsOptions
        /// </summary>
        /// <param name="collectionConfigurationSettings">DTO of the new collectionConfigurationSettings</param>
        /// <returns>A collectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        public async Task<Dtos.CollectionConfigurationSettingsOptions> PutCollectionConfigurationSettingsOptionsAsync(Dtos.CollectionConfigurationSettingsOptions collectionConfigurationSettings)
        {
            //Put is not supported for Colleague but EEDM requires full crud support.
            var exception = new IntegrationApiException();
            exception.AddError(new IntegrationApiError("Invalid.Operation",
                "Invalid operation for alternate view of resource.",
                "The Update operation is only available when requesting the collection-configuration-settings representation."));
            throw CreateHttpResponseException(exception, HttpStatusCode.NotAcceptable);
        }

        /// <summary>
        /// Create (POST) a new collectionConfigurationSettingsOptions
        /// </summary>
        /// <param name="collectionConfigurationSettings">DTO of the new collectionConfigurationSettings</param>
        /// <returns>A collectionConfigurationSettings object <see cref="Dtos.CollectionConfigurationSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.CollectionConfigurationSettingsOptions> PostCollectionConfigurationSettingsOptionsAsync(Dtos.CollectionConfigurationSettingsOptions collectionConfigurationSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a collectionConfigurationSettings
        /// </summary>
        /// <param name="guid">GUID to desired collectionConfigurationSettings</param>
        /// <returns>HttpResponseMessage</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteCollectionConfigurationSettingsAsync([FromUri] string guid)
        {
            //Delete is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

    }
}