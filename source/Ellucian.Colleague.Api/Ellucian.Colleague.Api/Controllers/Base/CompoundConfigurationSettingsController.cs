//Copyright 2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to CompoundConfigurationSettings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CompoundConfigurationSettingsController : BaseCompressedApiController
    {
        private readonly ICompoundConfigurationSettingsService _compoundConfigurationSettingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CompoundConfigurationSettingsController class.
        /// </summary>
        /// <param name="compoundConfigurationSettingsService">Service of type <see cref="ICompoundConfigurationSettingsService">ICompoundConfigurationSettingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CompoundConfigurationSettingsController(ICompoundConfigurationSettingsService compoundConfigurationSettingsService, ILogger logger)
        {
            _compoundConfigurationSettingsService = compoundConfigurationSettingsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all compoundConfigurationSettings
        /// </summary>
        /// <returns>List of CompoundConfigurationSettings <see cref="Dtos.CompoundConfigurationSettings"/> objects representing matching compoundConfigurationSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(CompoundConfigurationSettings))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CompoundConfigurationSettings>> GetCompoundConfigurationSettingsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            List<CompoundConfigurationSettingsEthos> resourcesFilter = new List<CompoundConfigurationSettingsEthos>();
            var criteriaObject = GetFilterObject<CompoundConfigurationSettings>(_logger, "criteria");
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos);
            }
            try
            {
                var compoundConfigurationSettings = await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsAsync(resourcesFilter, bypassCache);

                if (compoundConfigurationSettings != null && compoundConfigurationSettings.Any())
                {
                    AddEthosContextProperties(await _compoundConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _compoundConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              compoundConfigurationSettings.Select(a => a.Id).ToList()));
                }
                return compoundConfigurationSettings;
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
        /// Read (GET) a compoundConfigurationSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired compoundConfigurationSettings</param>
        /// <returns>A compoundConfigurationSettings object <see cref="Dtos.CompoundConfigurationSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.CompoundConfigurationSettings> GetCompoundConfigurationSettingsByGuidAsync(string guid)
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
                   await _compoundConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _compoundConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsByGuidAsync(guid);
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

        #region GET  compound-cConfiguration-settings-options
        /// <summary>
        /// Return all compoundConfigurationSettings options
        /// </summary>
        /// <returns>List of compoundConfigurationSettings <see cref="Dtos.CompoundConfigurationSettingsOptions"/> objects representing matching compoundConfigurationSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.CompoundConfigurationSettingsOptions))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]

        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CompoundConfigurationSettingsOptions>> GetCompoundConfigurationSettingsOptionsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            List<CompoundConfigurationSettingsEthos> resourcesFilter = new List<CompoundConfigurationSettingsEthos>();
            var criteriaObject = GetFilterObject<CompoundConfigurationSettings>(_logger, "criteria");
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos);
            }
            try
            {
                var compoundConfigurationSettingsOptions = await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsOptionsAsync(resourcesFilter, bypassCache);

                if (compoundConfigurationSettingsOptions != null && compoundConfigurationSettingsOptions.Any())
                {
                    AddEthosContextProperties(await _compoundConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _compoundConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              compoundConfigurationSettingsOptions.Select(a => a.Id).ToList()));
                }
                return compoundConfigurationSettingsOptions;
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
        /// Read (GET) a compoundConfigurationSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired compoundConfigurationSettings</param>
        /// <returns>A compoundConfigurationSettings object <see cref="Dtos.ConfigurationSettingsOptions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.CompoundConfigurationSettingsOptions> GetCompoundConfigurationSettingsOptionsByGuidAsync(string guid)
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
                   await _compoundConfigurationSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _compoundConfigurationSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsOptionsByGuidAsync(guid, bypassCache);
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
        /// Update (PUT) an existing CompoundConfigurationSettings
        /// </summary>
        /// <param name="guid">GUID of the compoundConfigurationSettings to update</param>
        /// <param name="compoundConfigurationSettings">DTO of the updated compoundConfigurationSettings</param>
        /// <returns>A CompoundConfigurationSettings object <see cref="Dtos.CompoundConfigurationSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.CompoundConfigurationSettings> PutCompoundConfigurationSettingsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.CompoundConfigurationSettings compoundConfigurationSettings)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (compoundConfigurationSettings == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null compoundConfigurationSettings argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(compoundConfigurationSettings.Id))
            {
                compoundConfigurationSettings.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, compoundConfigurationSettings.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _compoundConfigurationSettingsService.ImportExtendedEthosData(await ExtractExtendedData(await _compoundConfigurationSettingsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                return await _compoundConfigurationSettingsService.UpdateCompoundConfigurationSettingsAsync(
                  await PerformPartialPayloadMerge(compoundConfigurationSettings, async () => await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsByGuidAsync(guid, true),
                  await _compoundConfigurationSettingsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger));
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
        /// Create (POST) a new compoundConfigurationSettings
        /// </summary>
        /// <param name="compoundConfigurationSettings">DTO of the new compoundConfigurationSettings</param>
        /// <returns>A compoundConfigurationSettings object <see cref="Dtos.CompoundConfigurationSettings"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.CompoundConfigurationSettings> PostCompoundConfigurationSettingsAsync(Dtos.CompoundConfigurationSettings compoundConfigurationSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a compoundConfigurationSettings
        /// </summary>
        /// <param name="guid">GUID to desired compoundConfigurationSettings</param>
        /// <returns>HttpResponseMessage</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteCompoundConfigurationSettingsAsync([FromUri] string guid)
        {
            //Delete is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing compoundConfigurationSettingsOptions
        /// </summary>
        /// <param name="guid">GUID of the compound configuration settings options to update</param>
        /// <param name="compoundConfigurationSettingsOptions">DTO of the new compoundConfigurationSettingsOptions</param>
        /// <returns>A compoundConfigurationSettingsOptions object <see cref="Dtos.CompoundConfigurationSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        public async Task<Dtos.MappingSettingsOptions> PutCompoundConfigurationSettingsOptionsAsync([FromUri] string guid, [FromBody] Dtos.CompoundConfigurationSettingsOptions 
            compoundConfigurationSettingsOptions)
        {
            var exception = new IntegrationApiException();
            exception.AddError(new IntegrationApiError("Invalid.Operation", "Invalid operation for alternate view of resource.",
                "The update operation is only available when requesting compound-configuration-settings."));
            throw CreateHttpResponseException(exception, HttpStatusCode.NotAcceptable);
        }

        /// <summary>
        /// Create (POST) a new compoundConfigurationSettingsOptions
        /// </summary>
        /// <param name="compoundConfigurationSettingsOptions">DTO of the new compoundConfigurationSettings</param>
        /// <returns>A compoundConfigurationSettingsOptions object <see cref="Dtos.CompoundConfigurationSettings"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.CompoundConfigurationSettingsOptions> PostCompoundConfigurationSettingsOptionsAsync(Dtos.CompoundConfigurationSettingsOptions compoundConfigurationSettingsOptions)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

    }
}