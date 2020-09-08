//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentFrequencies
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentFrequenciesController : BaseCompressedApiController
    {
        private readonly IEmploymentFrequenciesService _employmentFrequenciesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentFrequenciesController class.
        /// </summary>
        /// <param name="employmentFrequenciesService">Service of type <see cref="IEmploymentFrequenciesService">IEmploymentFrequenciesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EmploymentFrequenciesController(IEmploymentFrequenciesService employmentFrequenciesService, ILogger logger)
        {
            _employmentFrequenciesService = employmentFrequenciesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all employmentFrequencies
        /// </summary>
                /// <returns>List of EmploymentFrequencies <see cref="Dtos.EmploymentFrequencies"/> objects representing matching employmentFrequencies</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentFrequencies>> GetEmploymentFrequenciesAsync()
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
                AddDataPrivacyContextProperty((await _employmentFrequenciesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                var employmentFrequencies = await _employmentFrequenciesService.GetEmploymentFrequenciesAsync(bypassCache);

                if (employmentFrequencies != null && employmentFrequencies.Any())
                {
                    AddEthosContextProperties(await _employmentFrequenciesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _employmentFrequenciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              employmentFrequencies.Select(a => a.Id).ToList()));
                }

                return employmentFrequencies;                
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
        /// Read (GET) a employmentFrequencies using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employmentFrequencies</param>
        /// <returns>A employmentFrequencies object <see cref="Dtos.EmploymentFrequencies"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.EmploymentFrequencies> GetEmploymentFrequenciesByGuidAsync(string guid)
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
               AddDataPrivacyContextProperty((await _employmentFrequenciesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                   await _employmentFrequenciesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _employmentFrequenciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _employmentFrequenciesService.GetEmploymentFrequenciesByGuidAsync(guid);
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
        /// Create (POST) a new employmentFrequencies
        /// </summary>
        /// <param name="employmentFrequencies">DTO of the new employmentFrequencies</param>
        /// <returns>A employmentFrequencies object <see cref="Dtos.EmploymentFrequencies"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.EmploymentFrequencies> PostEmploymentFrequenciesAsync([FromBody] Dtos.EmploymentFrequencies employmentFrequencies)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentFrequencies
        /// </summary>
        /// <param name="guid">GUID of the employmentFrequencies to update</param>
        /// <param name="employmentFrequencies">DTO of the updated employmentFrequencies</param>
        /// <returns>A employmentFrequencies object <see cref="Dtos.EmploymentFrequencies"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.EmploymentFrequencies> PutEmploymentFrequenciesAsync([FromUri] string guid, [FromBody] Dtos.EmploymentFrequencies employmentFrequencies)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentFrequencies
        /// </summary>
        /// <param name="guid">GUID to desired employmentFrequencies</param>
        [HttpDelete]
        public async Task DeleteEmploymentFrequenciesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}