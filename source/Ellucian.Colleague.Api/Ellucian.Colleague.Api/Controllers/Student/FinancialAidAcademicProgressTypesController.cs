//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to FinancialAidAcademicProgressTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidAcademicProgressTypesController : BaseCompressedApiController
    {
        private readonly IFinancialAidAcademicProgressTypesService _financialAidAcademicProgressTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidAcademicProgressTypesController class.
        /// </summary>
        /// <param name="financialAidAcademicProgressTypesService">Service of type <see cref="IFinancialAidAcademicProgressTypesService">IFinancialAidAcademicProgressTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public FinancialAidAcademicProgressTypesController(IFinancialAidAcademicProgressTypesService financialAidAcademicProgressTypesService, ILogger logger)
        {
            _financialAidAcademicProgressTypesService = financialAidAcademicProgressTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all financialAidAcademicProgressTypes
        /// </summary>
        /// <returns>List of FinancialAidAcademicProgressTypes <see cref="Dtos.FinancialAidAcademicProgressTypes"/> objects representing matching financialAidAcademicProgressTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressTypes>> GetFinancialAidAcademicProgressTypesAsync()
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
                var progressTypes = await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesAsync(bypassCache);

                if (progressTypes != null && progressTypes.Any())
                {
                    AddEthosContextProperties(
                      await _financialAidAcademicProgressTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _financialAidAcademicProgressTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                          progressTypes.Select(i => i.Id).ToList()));
                }
                return progressTypes;
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
        /// Read (GET) a financialAidAcademicProgressTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired financialAidAcademicProgressTypes</param>
        /// <returns>A financialAidAcademicProgressTypes object <see cref="Dtos.FinancialAidAcademicProgressTypes"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.FinancialAidAcademicProgressTypes> GetFinancialAidAcademicProgressTypesByGuidAsync(string guid)
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
                var progressType = await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesByGuidAsync(guid);
                if (progressType != null)
                {

                    AddEthosContextProperties(await _financialAidAcademicProgressTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _financialAidAcademicProgressTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { progressType.Id }));
                }
                return progressType;
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
        /// Create (POST) a new financialAidAcademicProgressTypes
        /// </summary>
        /// <param name="financialAidAcademicProgressTypes">DTO of the new financialAidAcademicProgressTypes</param>
        /// <returns>A financialAidAcademicProgressTypes object <see cref="Dtos.FinancialAidAcademicProgressTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.FinancialAidAcademicProgressTypes> PostFinancialAidAcademicProgressTypesAsync([FromBody] Dtos.FinancialAidAcademicProgressTypes financialAidAcademicProgressTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing financialAidAcademicProgressTypes
        /// </summary>
        /// <param name="guid">GUID of the financialAidAcademicProgressTypes to update</param>
        /// <param name="financialAidAcademicProgressTypes">DTO of the updated financialAidAcademicProgressTypes</param>
        /// <returns>A financialAidAcademicProgressTypes object <see cref="Dtos.FinancialAidAcademicProgressTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.FinancialAidAcademicProgressTypes> PutFinancialAidAcademicProgressTypesAsync([FromUri] string guid, [FromBody] Dtos.FinancialAidAcademicProgressTypes financialAidAcademicProgressTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a financialAidAcademicProgressTypes
        /// </summary>
        /// <param name="guid">GUID to desired financialAidAcademicProgressTypes</param>
        [HttpDelete]
        public async Task DeleteFinancialAidAcademicProgressTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}