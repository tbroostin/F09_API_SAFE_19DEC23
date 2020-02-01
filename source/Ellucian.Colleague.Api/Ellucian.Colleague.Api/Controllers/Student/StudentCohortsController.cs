// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
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

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Accesses Student cohorts data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentCohortsController : BaseCompressedApiController
    {
        private readonly IStudentService _studentService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentsCohortsController class.
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="studentService">studentService</param>
        /// <param name="logger">logger</param>
        public StudentCohortsController(IAdapterRegistry adapterRegistry, IStudentService studentService, ILogger logger)
        {
            _studentService = studentService;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Gets all student cohorts
        /// </summary>
        /// <returns></returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentCohort>> GetStudentCohortsAsync()
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
                var items = await _studentService.GetAllStudentCohortsAsync(bypassCache);

                AddEthosContextProperties(
                    await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return items;
            }
            catch  (KeyNotFoundException e)
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
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Retrieves a student cohort by guid.
        /// </summary>
        /// <param name="id">Guid of students cohort to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.StudentCohort">student cohort.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.StudentCohort> GetStudentCohortByIdAsync(string id)
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
                AddEthosContextProperties(
                    await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _studentService.GetStudentCohortByGuidAsync(id, bypassCache);
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
        /// POST student cohort
        /// </summary>
        /// <param name="studentCohort"></param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.StudentCohort">student cohort.</see></returns>
        [HttpPost]
        public async Task<Dtos.StudentCohort> PostStudentCohortAsync([FromBody] Dtos.StudentCohort studentCohort)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// PUT student cohort
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentCohort"></param>
        /// <returns>Dtos.StudentsCohort</returns>
        [HttpPut]
        public async Task<Dtos.StudentCohort> PutStudentCohortAsync([FromUri] string id, [FromBody] Dtos.StudentCohort studentCohort)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete student cohort
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteStudentCohortAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}