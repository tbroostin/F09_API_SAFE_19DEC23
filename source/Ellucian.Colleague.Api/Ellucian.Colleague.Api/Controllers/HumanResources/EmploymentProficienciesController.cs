// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Exceptions;
using System.Net;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Employee Proficiencies data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentProficienciesController : BaseCompressedApiController
    {
        private readonly IEmploymentProficiencyService _employmentProficiencyService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmployeeProficienciesController class.
        /// </summary>
        /// <param name="employmentProficienciesService">Service of type <see cref="IEmploymentProficiencyService">IEmploymentProficienciesService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public EmploymentProficienciesController(IEmploymentProficiencyService employmentProficienciesService, ILogger logger)
        {
            _employmentProficiencyService = employmentProficienciesService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 10</remarks>
        /// <summary>
        /// Retrieves all employment proficiencies.
        /// </summary>
        /// <returns>All EmploymentProficiency objects.</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentProficiency>> GetEmploymentProficienciesAsync()
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
                var dtos = await _employmentProficiencyService.GetEmploymentProficienciesAsync(bypassCache);

                AddEthosContextProperties(
                    await _employmentProficiencyService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _employmentProficiencyService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        dtos.Select(i => i.Id).ToList()));

                return dtos;

            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 10</remarks>
        /// <summary>
        /// Retrieves a employment proficiency by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.EmploymentProficiency">EmploymentProficiency.</see></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.EmploymentProficiency> GetEmploymentProficiencyByIdAsync(string id)
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
                var empProf = await _employmentProficiencyService.GetEmploymentProficiencyByGuidAsync(id); 

                if (empProf != null)
                {

                    AddEthosContextProperties(await _employmentProficiencyService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _employmentProficiencyService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { empProf.Id }));
                }
                return empProf;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates a EmploymentProficiency.
        /// </summary>
        /// <param name="employmentProficiency"><see cref="Dtos.EmploymentProficiency">EmploymentProficiency</see> to update</param>
        /// <returns>Newly updated <see cref="Dtos.EmploymentProficiency">EmploymentProficiency</see></returns>
        [HttpPut]
        public async Task<Dtos.EmploymentProficiency> PutEmploymentProficiencyAsync([FromBody] Dtos.EmploymentProficiency employmentProficiency)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a EmploymentProficiency.
        /// </summary>
        /// <param name="employmentProficiency"><see cref="Dtos.EmploymentProficiency">EmploymentProficiency</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.EmploymentProficiency">EmploymentProficiency</see></returns>
        [HttpPost]
        public async Task<Dtos.EmploymentProficiency> PostEmploymentProficiencyAsync([FromBody] Dtos.EmploymentProficiency employmentProficiency)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing EmploymentProficiency
        /// </summary>
        /// <param name="id">Id of the EmploymentProficiency to delete</param>
        [HttpDelete]
        public async Task DeleteEmploymentProficiencyAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
