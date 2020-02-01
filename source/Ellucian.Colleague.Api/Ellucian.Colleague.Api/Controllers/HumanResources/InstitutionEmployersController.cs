//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
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
using Newtonsoft.Json;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to InstitutionEmployers
    /// </summary>
    [System.Web.Http.Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class InstitutionEmployersController : BaseCompressedApiController
    {
        private readonly IInstitutionEmployersService _institutionEmployersService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstitutionEmployersController class.
        /// </summary>
        /// <param name="institutionEmployersService">Service of type <see cref="IInstitutionEmployersService">IInstitutionEmployersService</see></param>
        /// <param name="logger">Interface to logger</param>
        public InstitutionEmployersController(IInstitutionEmployersService institutionEmployersService, ILogger logger)
        {
            _institutionEmployersService = institutionEmployersService;
            _logger = logger;
        }

        /// <summary>
        /// Return all InstitutionEmployers
        /// </summary>
        /// <returns>Returns list of InstitutionEmployers <see cref="Dtos.InstitutionEmployers"/> objects representing matching institutionEmployers</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.InstitutionEmployers))]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.InstitutionEmployers>> GetInstitutionEmployersAsync(QueryStringFilter criteria)
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
                    return new List<Dtos.InstitutionEmployers>(new List<Dtos.InstitutionEmployers>());
                var items = await _institutionEmployersService.GetInstitutionEmployersAsync(bypassCache);
                if (criteriaObj != null && !string.IsNullOrEmpty(criteriaObj.Code) && items != null && items.Any())
                {
                    code = criteriaObj.Code;
                    items = items.Where(c => c.Code == code);
                }
                AddEthosContextProperties(await _institutionEmployersService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _institutionEmployersService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  items.Select(a => a.Id).ToList()));
                return items;
            }
            catch (JsonSerializationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch
                (KeyNotFoundException e)
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
        /// Read (GET) an InstitutionEmployers using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionEmployers</param>
        /// <returns>An InstitutionEmployers DTO object <see cref="Dtos.InstitutionEmployers"/> in EEDM format</returns>
        [System.Web.Http.HttpGet, EedmResponseFilter]
        public async Task<Dtos.InstitutionEmployers> GetInstitutionEmployersByGuidAsync(string guid)
        {
            try
            {
                AddDataPrivacyContextProperty((await _institutionEmployersService.GetDataPrivacyListByApi(GetRouteResourceName())).ToList());
                return await _institutionEmployersService.GetInstitutionEmployersByGuidAsync(guid);
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
        /// Create (POST) a new institutionEmployers
        /// </summary>
        /// <param name="institutionEmployers">DTO of the new institutionEmployers</param>
        /// <returns>An InstitutionEmployers DTO object <see cref="Dtos.InstitutionEmployers"/> in EEDM format</returns>
        [System.Web.Http.HttpPost]
        public async Task<Dtos.InstitutionEmployers> PostInstitutionEmployersAsync([FromBody] Dtos.InstitutionEmployers institutionEmployers)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing institutionEmployers
        /// </summary>
        /// <param name="guid">GUID of the institutionEmployers to update</param>
        /// <param name="institutionEmployers">DTO of the updated institutionEmployers</param>
        /// <returns>An InstitutionEmployers DTO object <see cref="Dtos.InstitutionEmployers"/> in EEDM format</returns>
        [System.Web.Http.HttpPut]
        public async Task<Dtos.InstitutionEmployers> PutInstitutionEmployersAsync([FromUri] string guid, [FromBody] Dtos.InstitutionEmployers institutionEmployers)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        
        /// <summary>
        /// Delete (DELETE) a institutionEmployers
        /// </summary>
        /// <param name="guid">GUID to desired institutionEmployers</param>
        [System.Web.Http.HttpDelete]
        public async Task DeleteInstitutionEmployersAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
 }