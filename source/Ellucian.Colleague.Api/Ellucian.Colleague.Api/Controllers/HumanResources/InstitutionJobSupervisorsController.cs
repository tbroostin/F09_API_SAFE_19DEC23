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
using System.Net;
using System.Linq;
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
    /// Provides access to InstitutionJobSupervisors
    /// </summary>
    [System.Web.Http.Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class InstitutionJobSupervisorsController : BaseCompressedApiController
    {
        private readonly IInstitutionJobSupervisorsService _institutionJobSupervisorsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstitutionJobSupervisorsController class.
        /// </summary>
        /// <param name="institutionJobSupervisorsService">Service of type <see cref="IInstitutionJobSupervisorsService">IInstitutionJobSupervisorsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public InstitutionJobSupervisorsController(IInstitutionJobSupervisorsService institutionJobSupervisorsService, ILogger logger)
        {
            _institutionJobSupervisorsService = institutionJobSupervisorsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all InstitutionJobSupervisors
        /// </summary>
        /// <returns>List of InstitutionJobSupervisors <see cref="Dtos.InstitutionJobSupervisors"/> objects representing matching institutionJobs</returns>
        [System.Web.Http.HttpGet]
        [EedmResponseFilter]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetInstitutionJobSupervisorsAsync(Paging page)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _institutionJobSupervisorsService.GetInstitutionJobSupervisorsAsync(page.Offset, page.Limit, bypassCache);


                AddEthosContextProperties(
                    await _institutionJobSupervisorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionJobSupervisorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobSupervisors>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) an InstitutionJobSupervisors using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        /// <returns>An InstitutionJobSupervisors DTO object <see cref="Dtos.InstitutionJobSupervisors"/> in EEDM format</returns>
        [System.Web.Http.HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.InstitutionJobSupervisors> GetInstitutionJobSupervisorsByGuidAsync(string guid)
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
                   await _institutionJobSupervisorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _institutionJobSupervisorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));

                return await _institutionJobSupervisorsService.GetInstitutionJobSupervisorsByGuidAsync(guid);
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
        /// Return all InstitutionJobSupervisors
        /// </summary>
        /// <returns>List of InstitutionJobSupervisors <see cref="Dtos.InstitutionJobSupervisors"/> objects representing matching institutionJobs</returns>
        [System.Web.Http.HttpGet]
        [EedmResponseFilter]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetInstitutionJobSupervisors2Async(Paging page)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _institutionJobSupervisorsService.GetInstitutionJobSupervisors2Async(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _institutionJobSupervisorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionJobSupervisorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobSupervisors>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) an InstitutionJobSupervisors using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        /// <returns>An InstitutionJobSupervisors DTO object <see cref="Dtos.InstitutionJobSupervisors"/> in EEDM format</returns>
        [System.Web.Http.HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.InstitutionJobSupervisors> GetInstitutionJobSupervisorsByGuid2Async(string guid)
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
                    await _institutionJobSupervisorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionJobSupervisorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _institutionJobSupervisorsService.GetInstitutionJobSupervisorsByGuid2Async(guid);
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
        /// Create (POST) a new institutionJobs
        /// </summary>
        /// <param name="institutionJobs">DTO of the new institutionJobs</param>
        /// <returns>An InstitutionJobSupervisors DTO object <see cref="Dtos.InstitutionJobSupervisors"/> in EEDM format</returns>
        [System.Web.Http.HttpPost]
        public async Task<Dtos.InstitutionJobSupervisors> PostInstitutionJobSupervisorsAsync([FromBody] Dtos.InstitutionJobSupervisors institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing institutionJobs
        /// </summary>
        /// <param name="guid">GUID of the institutionJobs to update</param>
        /// <param name="institutionJobs">DTO of the updated institutionJobs</param>
        /// <returns>An InstitutionJobSupervisors DTO object <see cref="Dtos.InstitutionJobSupervisors"/> in EEDM format</returns>
        [System.Web.Http.HttpPut]
        public async Task<Dtos.InstitutionJobSupervisors> PutInstitutionJobSupervisorsAsync([FromUri] string guid, [FromBody] Dtos.InstitutionJobSupervisors institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a institutionJobs
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        [System.Web.Http.HttpDelete]
        public async Task DeleteInstitutionJobSupervisorsAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
    
}