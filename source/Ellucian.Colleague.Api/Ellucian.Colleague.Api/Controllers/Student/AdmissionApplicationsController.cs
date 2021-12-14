﻿//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http;
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionApplications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationsController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationsService _admissionApplicationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationsController class.
        /// </summary>
        /// <param name="admissionApplicationsService">Service of type <see cref="IAdmissionApplicationsService">IAdmissionApplicationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AdmissionApplicationsController(IAdmissionApplicationsService admissionApplicationsService, ILogger logger)
        {
            _admissionApplicationsService = admissionApplicationsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all admissionApplications
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet,PermissionsFilter(new string[] {StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications})]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetAdmissionApplicationsAsync(Paging page)
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
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }


                var pageOfItems = await _admissionApplicationsService.GetAdmissionApplicationsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                  await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.AdmissionApplication>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request); 
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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Return all admissionApplications
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet,PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications})]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetAdmissionApplications2Async(Paging page)
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
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _admissionApplicationsService.GetAdmissionApplications2Async(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                  await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.AdmissionApplication2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Return all admissionApplications
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria">Filtering Criteria</param>
        /// <param name="personFilter">PersonFilter Named Query</param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet,PermissionsFilter(new string[] {StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications})]        
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AdmissionApplication3))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        public async Task<IHttpActionResult> GetAdmissionApplications3Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var filterCriteria = GetFilterObject<Dtos.AdmissionApplication3>(_logger, "criteria");
            var personFilterFilter = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");

            if (CheckForEmptyFilterParameters())
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.AdmissionApplication3>>(new List<Dtos.AdmissionApplication3>(), page, 0, this.Request);
            }

            try
            {
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _admissionApplicationsService.GetAdmissionApplications3Async(page.Offset, page.Limit, filterCriteria, personFilterFilter, bypassCache);

                AddEthosContextProperties(
                  await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.AdmissionApplication3>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Read (GET) a admissionApplications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] {StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications })]
        public async Task<Dtos.AdmissionApplication> GetAdmissionApplicationsByGuidAsync(string guid)
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
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache), 
                    await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));
                return await _admissionApplicationsService.GetAdmissionApplicationsByGuidAsync(guid);
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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Read (GET) a admissionApplications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication2"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] {StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications})]
        public async Task<Dtos.AdmissionApplication2> GetAdmissionApplicationsByGuid2Async(string guid)
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
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                     await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));
                return await _admissionApplicationsService.GetAdmissionApplicationsByGuid2Async(guid);
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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Read (GET) a admissionApplications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication3"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] {StudentPermissionCodes.ViewApplications , StudentPermissionCodes.UpdateApplications})]
        public async Task<Dtos.AdmissionApplication3> GetAdmissionApplicationsByGuid3Async(string guid)
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
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                     await _admissionApplicationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));
                return await _admissionApplicationsService.GetAdmissionApplicationsByGuid3Async(guid);
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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Create (POST) a new admissionApplications
        /// </summary>
        /// <param name="admissionApplications">DTO of the new admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AdmissionApplication> PostAdmissionApplicationsAsync([FromBody] Dtos.AdmissionApplication admissionApplications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create (POST) a new admissionApplication
        /// </summary>
        /// <param name="admissionApplication">DTO of the new admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication2"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter,PermissionsFilter(StudentPermissionCodes.UpdateApplications)]
        public async Task<Dtos.AdmissionApplication2> PostAdmissionApplications2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplication2 admissionApplication)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (admissionApplication == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid admission application.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(admissionApplication.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null admissionApplications id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (admissionApplication.Id     != Guid.Empty.ToString())
            {
                throw new ArgumentNullException("admissionApplicationsDto", "On a post you can not define a GUID.");
            }

            try
            {
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _admissionApplicationsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var admissionApplicationReturn = await _admissionApplicationsService.CreateAdmissionApplicationAsync(admissionApplication, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _admissionApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { admissionApplicationReturn.Id }));

                return admissionApplicationReturn;
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
        /// Update (PUT) an existing admissionApplications
        /// </summary>
        /// <param name="guid">GUID of the admissionApplications to update</param>
        /// <param name="admissionApplications">DTO of the updated admissionApplications</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplication"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AdmissionApplication> PutAdmissionApplicationsAsync([FromUri] string guid, [FromBody] Dtos.AdmissionApplication admissionApplications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing AdmissionApplication
        /// </summary>
        /// <param name="guid">GUID of the admissionApplications to update</param>
        /// <param name="admissionApplication">DTO of the updated admissionApplications</param>
        /// <returns>A AdmissionApplications object <see cref="Dtos.AdmissionApplication2"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.UpdateApplications)]
        public async Task<Dtos.AdmissionApplication2> PutAdmissionApplications2Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplication2 admissionApplication)
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
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (admissionApplication == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null admissionApplications argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(admissionApplication.Id))
            {
                admissionApplication.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, admissionApplication.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _admissionApplicationsService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _admissionApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _admissionApplicationsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var admissionApplicationReturn =await _admissionApplicationsService.UpdateAdmissionApplicationAsync(guid,
                  await PerformPartialPayloadMerge(admissionApplication, async () => await _admissionApplicationsService.GetAdmissionApplicationsByGuid2Async(guid),
                  dpList, _logger), bypassCache);

                AddEthosContextProperties(dpList, 
                    await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));


                return admissionApplicationReturn;
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
            catch(InvalidOperationException e)
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
        /// Delete (DELETE) a admissionApplications
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplications</param>
        [HttpDelete]
        public async Task DeleteAdmissionApplicationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #region  Admission Applications Submissions

        /// <summary>
        /// Create (POST) a new admissionApplicationSubmission
        /// </summary>
        /// <param name="admissionApplication">DTO of the new admissionApplicationsSubmissions</param>
        /// <returns>A admissionApplications object <see cref="Dtos.AdmissionApplicationSubmission"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.AdmissionApplication3> PostAdmissionApplicationsSubmissionsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplicationSubmission admissionApplication)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (admissionApplication == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid admission application submission.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(admissionApplication.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null admissionApplicationsSubmission id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _admissionApplicationsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var admissionApplicationReturn = await _admissionApplicationsService.CreateAdmissionApplicationsSubmissionAsync(admissionApplication, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _admissionApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { admissionApplicationReturn.Id }));

                return admissionApplicationReturn;
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
        /// Update (PUT) an existing AdmissionApplicationsSubmissions
        /// </summary>
        /// <param name="guid">GUID of the admissionApplications to update</param>
        /// <param name="admissionApplicationsSubmissions">DTO of the updated admissionApplications</param>
        /// <returns>A AdmissionApplications object <see cref="Dtos.AdmissionApplication3"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.AdmissionApplication3> PutAdmissionApplicationsSubmissionsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplicationSubmission admissionApplicationsSubmissions)
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
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (admissionApplicationsSubmissions == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null admissionApplicationsSubmissions argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }            

            if (string.IsNullOrEmpty(admissionApplicationsSubmissions.Id))
            {
                admissionApplicationsSubmissions.Id = guid.ToLowerInvariant();
            }

            try
            {
                //get Data Privacy List
                var dpList = await _admissionApplicationsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _admissionApplicationsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                
                var admissionApplicationSubmissionsOrig = await _admissionApplicationsService.GetAdmissionApplicationsSubmissionsByGuidAsync(guid, true);

                var mergedAdmissionApplicationSubmissions = await PerformPartialPayloadMerge(admissionApplicationsSubmissions, admissionApplicationSubmissionsOrig, dpList, _logger);

                // Error if attempt is made to unset an existing educational goal.
                if (admissionApplicationSubmissionsOrig != null)
                {
                    if (admissionApplicationSubmissionsOrig.EducationalGoal != null && !string.IsNullOrEmpty(admissionApplicationSubmissionsOrig.EducationalGoal.Id))
                    {
                        if (mergedAdmissionApplicationSubmissions != null)
                        {
                            if (mergedAdmissionApplicationSubmissions.EducationalGoal == null || string.IsNullOrEmpty(mergedAdmissionApplicationSubmissions.EducationalGoal.Id))
                            {
                                throw new IntegrationApiException("Missing educationalGoal",
                                    IntegrationApiUtility.GetDefaultApiError("The educationalGoal is stored to APP.ORIG.EDUC.GOAL and may not be unset."));
                            }
                        }
                    }
                }

                var admissionApplicationReturn = await _admissionApplicationsService.UpdateAdmissionApplicationsSubmissionAsync(guid,
                    mergedAdmissionApplicationSubmissions, bypassCache);

                AddEthosContextProperties(dpList,
                    await _admissionApplicationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));


                return admissionApplicationReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
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
            catch (InvalidOperationException e)
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
    }
}