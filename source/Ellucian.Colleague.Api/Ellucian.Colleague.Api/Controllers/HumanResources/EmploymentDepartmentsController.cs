//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
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
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentDepartments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentDepartmentsController : BaseCompressedApiController
    {
        private readonly IEmploymentDepartmentsService _employmentDepartmentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentDepartmentsController class.
        /// </summary>
        /// <param name="employmentDepartmentsService">Service of type <see cref="IEmploymentDepartmentsService">IEmploymentDepartmentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EmploymentDepartmentsController(IEmploymentDepartmentsService employmentDepartmentsService, ILogger logger)
        {
            _employmentDepartmentsService = employmentDepartmentsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all employmentDepartments
        /// </summary>
        /// <returns>List of EmploymentDepartments <see cref="Dtos.EmploymentDepartments"/> objects representing matching employmentDepartments</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.EmploymentDepartments))]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentDepartments>> GetEmploymentDepartmentsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            string code = string.Empty;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var criteriaObj = GetFilterObject<Dtos.EmploymentDepartments>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.EmploymentDepartments>(new List<Dtos.EmploymentDepartments>());
                var items = await _employmentDepartmentsService.GetEmploymentDepartmentsAsync(bypassCache);
                if (criteriaObj != null && !string.IsNullOrEmpty(criteriaObj.Code) && items != null && items.Any())
                {
                    code = criteriaObj.Code;
                    items = items.Where(c => c.Code == code);                    
                }
                AddEthosContextProperties(await _employmentDepartmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _employmentDepartmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  items.Select(a => a.Id).ToList()));

                return items;
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
        /// Read (GET) a employmentDepartments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employmentDepartments</param>
        /// <returns>A employmentDepartments object <see cref="Dtos.EmploymentDepartments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.EmploymentDepartments> GetEmploymentDepartmentsByGuidAsync(string guid)
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
                var employmentDepartment = await _employmentDepartmentsService.GetEmploymentDepartmentsByGuidAsync(guid);

                if (employmentDepartment != null)
                {

                    AddEthosContextProperties(await _employmentDepartmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _employmentDepartmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { employmentDepartment.Id }));
                }

                return employmentDepartment;
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
        /// Create (POST) a new employmentDepartments
        /// </summary>
        /// <param name="employmentDepartments">DTO of the new employmentDepartments</param>
        /// <returns>A employmentDepartments object <see cref="Dtos.EmploymentDepartments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.EmploymentDepartments> PostEmploymentDepartmentsAsync([FromBody] Dtos.EmploymentDepartments employmentDepartments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentDepartments
        /// </summary>
        /// <param name="guid">GUID of the employmentDepartments to update</param>
        /// <param name="employmentDepartments">DTO of the updated employmentDepartments</param>
        /// <returns>A employmentDepartments object <see cref="Dtos.EmploymentDepartments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        public async Task<Dtos.EmploymentDepartments> PutEmploymentDepartmentsAsync([FromUri] string guid, [FromBody] Dtos.EmploymentDepartments employmentDepartments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentDepartments
        /// </summary>
        /// <param name="guid">GUID to desired employmentDepartments</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        public async Task DeleteEmploymentDepartmentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}