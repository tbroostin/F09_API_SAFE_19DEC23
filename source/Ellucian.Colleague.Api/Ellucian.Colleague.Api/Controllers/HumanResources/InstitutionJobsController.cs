//Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

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
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to InstitutionJobs
    /// </summary>
    [System.Web.Http.Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class InstitutionJobsController : BaseCompressedApiController
    {
        private readonly IInstitutionJobsService _institutionJobsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstitutionJobsController class.
        /// </summary>
        /// <param name="institutionJobsService">Service of type <see cref="IInstitutionJobsService">IInstitutionJobsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public InstitutionJobsController(IInstitutionJobsService institutionJobsService, ILogger logger)
        {
            _institutionJobsService = institutionJobsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all InstitutionJobs
        /// </summary>
        /// <returns>List of InstitutionJobs <see cref="Dtos.InstitutionJobs"/> objects representing matching institutionJobs</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.InstitutionJobs))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetInstitutionJobsAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;

            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }
            string person = string.Empty, employer = string.Empty, position = string.Empty, department = string.Empty,
                startOn = string.Empty, endOn = string.Empty, status = string.Empty, classification = string.Empty,
                preference = string.Empty;

            var criteriaValues = GetFilterObject<Dtos.InstitutionJobs>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionJobs>>(new List<Dtos.InstitutionJobs>(), page, 0, this.Request);

            if (criteriaValues != null)
            {
                if (criteriaValues.Person != null && !string.IsNullOrEmpty(criteriaValues.Person.Id))
                    person = criteriaValues.Person.Id;
                if (criteriaValues.Employer != null && !string.IsNullOrEmpty(criteriaValues.Employer.Id))
                    employer = criteriaValues.Employer.Id;
                if (criteriaValues.Position != null && !string.IsNullOrEmpty(criteriaValues.Position.Id))
                    position = criteriaValues.Position.Id;
                if (criteriaValues.Department != null && !string.IsNullOrEmpty(criteriaValues.Department))
                    department = criteriaValues.Department;
                if (criteriaValues.StartOn != null && criteriaValues.StartOn != default(DateTime))
                    startOn = criteriaValues.StartOn.ToShortDateString();
                if (criteriaValues.EndOn != null)
                    endOn = criteriaValues.EndOn.Value.ToShortDateString();
                if (criteriaValues.Status != null)
                    status = criteriaValues.Status.ToString();
                if (criteriaValues.Classification != null && !string.IsNullOrEmpty(criteriaValues.Classification.Id))
                    classification = criteriaValues.Classification.Id;
                if (criteriaValues.Preference != null && criteriaValues.Preference != Dtos.EnumProperties.JobPreference.NotSet)
                    preference = criteriaValues.Preference.ToString();
            }
            try
            {
               var pageOfItems = await _institutionJobsService.GetInstitutionJobsAsync(page.Offset, page.Limit, person, employer, position,
                    department, startOn, endOn, status, classification, preference, bypassCache);
               
                AddEthosContextProperties(
                  await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionJobs>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Return all InstitutionJobs
        /// </summary>
        /// <returns>List of InstitutionJobs <see cref="Dtos.InstitutionJobs2"/> objects representing matching institutionJobs</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.InstitutionJobs2))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetInstitutionJobs2Async(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;

            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }
            string person = string.Empty, employer = string.Empty, position = string.Empty, department = string.Empty,
                startOn = string.Empty, endOn = string.Empty, status = string.Empty, classification = string.Empty,
                preference = string.Empty;

            var criteriaValues = GetFilterObject<Dtos.InstitutionJobs2>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionJobs2>>(new List<Dtos.InstitutionJobs2>(), page, 0, this.Request);

            if (criteriaValues != null)
            {
                if (criteriaValues.Person != null && !string.IsNullOrEmpty(criteriaValues.Person.Id))
                    person = criteriaValues.Person.Id;
                if (criteriaValues.Employer != null && !string.IsNullOrEmpty(criteriaValues.Employer.Id))
                    employer = criteriaValues.Employer.Id;
                if (criteriaValues.Position != null && !string.IsNullOrEmpty(criteriaValues.Position.Id))
                    position = criteriaValues.Position.Id;
                if (criteriaValues.Department != null && !string.IsNullOrEmpty(criteriaValues.Department))
                    department = criteriaValues.Department;
                if (criteriaValues.StartOn != null && criteriaValues.StartOn != default(DateTime))
                    startOn = criteriaValues.StartOn.ToShortDateString();
                if (criteriaValues.Status != null)
                    status = criteriaValues.Status.ToString();
                if (criteriaValues.EndOn != null)
                    endOn = criteriaValues.EndOn.Value.ToShortDateString();
                if (criteriaValues.Classification != null && !string.IsNullOrEmpty(criteriaValues.Classification.Id))
                    classification = criteriaValues.Classification.Id;
                if (criteriaValues.Preference != Dtos.EnumProperties.JobPreference.NotSet)
                    preference = criteriaValues.Preference.ToString();
            }
            try
            {
                var pageOfItems = await _institutionJobsService.GetInstitutionJobs2Async(page.Offset, page.Limit, person, employer, position,
                    department, startOn, endOn, status, classification, preference, bypassCache);

                AddEthosContextProperties(
                 await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Return all InstitutionJobs
        /// </summary>
        /// <returns>List of InstitutionJobs <see cref="Dtos.InstitutionJobs3"/> objects representing matching institutionJobs</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.InstitutionJobs3))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetInstitutionJobs3Async(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;

            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }
            string person = string.Empty, employer = string.Empty, position = string.Empty, department = string.Empty,
                startOn = string.Empty, endOn = string.Empty, status = string.Empty, classification = string.Empty,
                preference = string.Empty;

            var criteriaValues = GetFilterObject<Dtos.InstitutionJobs3>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionJobs3>>(new List<Dtos.InstitutionJobs3>(), page, 0, this.Request);

            var filterQualifiers = GetFilterQualifiers(_logger);

            if (criteriaValues != null)
            {
                if (criteriaValues.Person != null && !string.IsNullOrEmpty(criteriaValues.Person.Id))
                    person = criteriaValues.Person.Id;
                if (criteriaValues.Employer != null && !string.IsNullOrEmpty(criteriaValues.Employer.Id))
                    employer = criteriaValues.Employer.Id;
                if (criteriaValues.Position != null && !string.IsNullOrEmpty(criteriaValues.Position.Id))
                    position = criteriaValues.Position.Id;
                if (criteriaValues.Department != null && !string.IsNullOrEmpty(criteriaValues.Department.Id))
                    department = criteriaValues.Department.Id;
                if (criteriaValues.StartOn != null && criteriaValues.StartOn != default(DateTime))
                    startOn = criteriaValues.StartOn.ToShortDateString();
                if (criteriaValues.EndOn != null)
                    endOn = criteriaValues.EndOn.Value.ToShortDateString();
                if (criteriaValues.Status != null)
                    status = criteriaValues.Status.ToString();
                if (criteriaValues.Classification != null && !string.IsNullOrEmpty(criteriaValues.Classification.Id))
                    classification = criteriaValues.Classification.Id;
                if (criteriaValues.Preference != Dtos.EnumProperties.JobPreference2.NotSet)
                    preference = criteriaValues.Preference.ToString();
            }
            try
            {
                var pageOfItems = await _institutionJobsService.GetInstitutionJobs3Async(page.Offset, page.Limit, person, employer, position,
                    department, startOn, endOn, status, classification, preference, bypassCache, filterQualifiers);

                AddEthosContextProperties(
                 await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) an InstitutionJobs using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        /// <returns>An InstitutionJobs DTO object <see cref="Dtos.InstitutionJobs"/> in EEDM format</returns>
        [System.Web.Http.HttpGet, EedmResponseFilter]
        public async Task<Dtos.InstitutionJobs> GetInstitutionJobsByGuidAsync(string guid)
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
                  await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _institutionJobsService.GetInstitutionJobsByGuidAsync(guid);
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
        /// Read (GET) an InstitutionJobs using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        /// <returns>An InstitutionJobs DTO object <see cref="Dtos.InstitutionJobs2"/> in EEDM format</returns>
        [System.Web.Http.HttpGet, EedmResponseFilter]
        public async Task<Dtos.InstitutionJobs2> GetInstitutionJobsByGuid2Async(string guid)
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
                  await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _institutionJobsService.GetInstitutionJobsByGuid2Async(guid);
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
        /// Read (GET) an InstitutionJobs using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        /// <returns>An InstitutionJobs DTO object <see cref="Dtos.InstitutionJobs3"/> in EEDM format</returns>
        [System.Web.Http.HttpGet, EedmResponseFilter]
        public async Task<Dtos.InstitutionJobs3> GetInstitutionJobsByGuid3Async(string guid)
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
                  await _institutionJobsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _institutionJobsService.GetInstitutionJobsByGuid3Async(guid);
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
        /// <returns> V8 and V11 of institution jobs is not supported, Returns an error</returns>
        [System.Web.Http.HttpPost]
        public async Task<Dtos.InstitutionJobs> PostInstitutionJobsAsync([FromBody] Dtos.InstitutionJobs institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create (POST) a new institutionJobs
        /// </summary>
        /// <param name="institutionJobs">DTO of the new institutionJobs</param>
        /// <returns> V8 and V11 of institution jobs is not supported, Returns an error</returns>
        [System.Web.Http.HttpPost]
        public async Task<Dtos.InstitutionJobs2> PostInstitutionJobs2Async([FromBody] Dtos.InstitutionJobs2 institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create (POST) a new institutionJobs v12
        /// </summary>
        /// <param name="institutionJobs">DTO of the new institutionJobs</param>
        /// <returns>An InstitutionJobs DTO object <see cref="Dtos.InstitutionJobs3"/> in EEDM format</returns>
        [System.Web.Http.HttpPost, EedmResponseFilter]
        public async Task<Dtos.InstitutionJobs3> PostInstitutionJobs3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.InstitutionJobs3 institutionJobs)
        {
           
            if (institutionJobs == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null institutionJobs argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
          
            try
            {
                if (institutionJobs.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("institutionJobsDto", "Nil GUID must be used in POST operation.");
                }
                ValidateInstitutionJobs2(institutionJobs);

                //call import extend method that needs the extracted extension data and the config
                await _institutionJobsService.ImportExtendedEthosData(await ExtractExtendedData(await _institutionJobsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var institutionJobsReturn = await _institutionJobsService.PostInstitutionJobsAsync(institutionJobs);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _institutionJobsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { institutionJobsReturn.Id }));

                return institutionJobsReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                if (e.Errors == null || e.Errors.Count <= 0)
                {
                    throw CreateHttpResponseException(e.Message);
                }
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }

        }

        /// <summary>
        /// Update (PUT) an existing institutionJobs
        /// </summary>
        /// <param name="guid">GUID of the institutionJobs to update</param>
        /// <param name="institutionJobs">DTO of the updated institutionJobs</param>
        /// <returns>V8 and V11 of institution jobs is not supported, Returns an error</returns>
        [System.Web.Http.HttpPut]
        public async Task<Dtos.InstitutionJobs> PutInstitutionJobsAsync([FromUri] string guid, [FromBody] Dtos.InstitutionJobs institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing institutionJobs
        /// </summary>
        /// <param name="guid">GUID of the institutionJobs to update</param>
        /// <param name="institutionJobs">DTO of the updated institutionJobs</param>
        /// <returns>V8 and V11 of institution jobs is not supported, Returns an error</returns>
        [System.Web.Http.HttpPut]
        public async Task<Dtos.InstitutionJobs2> PutInstitutionJobs2Async([FromUri] string guid, [FromBody] Dtos.InstitutionJobs2 institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing institutionJobs
        /// </summary>
        /// <param name="guid">GUID of the institutionJobs to update</param>
        /// <param name="institutionJobs">DTO of the updated institutionJobs</param>
        /// <returns>An InstitutionJobs DTO object <see cref="Dtos.InstitutionJobs3"/> in EEDM format</returns>
        [System.Web.Http.HttpPut, EedmResponseFilter]
        public async Task<Dtos.InstitutionJobs3> PutInstitutionJobs3Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.InstitutionJobs3 institutionJobs)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (institutionJobs == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null institutionJobs argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(institutionJobs.Id))
            {
                institutionJobs.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(institutionJobs.Id, Guid.Empty.ToString())))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != institutionJobs.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _institutionJobsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);    
                            
                //call import extend method that needs the extracted extension dataa and the config
                await _institutionJobsService.ImportExtendedEthosData(await ExtractExtendedData(await _institutionJobsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var institutionJobsReturn = await _institutionJobsService.PutInstitutionJobsAsync(
                  await PerformPartialPayloadMerge(institutionJobs, async () => await _institutionJobsService.GetInstitutionJobsByGuid3Async(guid, true),
                  dpList, _logger));

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                   await _institutionJobsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { institutionJobsReturn.Id }));

                return institutionJobsReturn;

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                if (e.Errors == null || e.Errors.Count <= 0)
                {
                    throw CreateHttpResponseException(e.Message);
                }
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
       
        /// <summary>
        /// Delete (DELETE) a institutionJobs
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        [System.Web.Http.HttpDelete]
        public async Task DeleteInstitutionJobsAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Helper method to validate Institution-Jobs PUT/POST.
        /// </summary>
        /// <param name="institutionJobs"><see cref="Dtos.InstitutionJobs"/>InstitutionJobs DTO object of type</param>
        private void ValidateInstitutionJobs(Dtos.InstitutionJobs institutionJobs)
        {

            if (institutionJobs == null)
            {
                throw new ArgumentNullException("institutionJobs", "The body is required when submitting an institutionJobs. ");
            }

        }

        /// <summary>
        /// Helper method to validate Institution-Jobs PUT/POST.
        /// </summary>
        /// <param name="institutionJobs"><see cref="Dtos.InstitutionJobs3"/>InstitutionJobs DTO object of type</param>
        private void ValidateInstitutionJobs2(Dtos.InstitutionJobs3 institutionJobs)
        {
            if (institutionJobs == null)
            {
                throw new ArgumentNullException("institutionJobs", "The body is required when submitting an institutionJobs. ");
            }

        }

    }
}