﻿/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Provides access to Aid Application Additional Info data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Metadata(ApiDescription = "Provides access to additional aid application information.", ApiDomain = "FA")]
    public class AidApplicationAdditionalInfoController : BaseCompressedApiController
    {   
        private readonly IAidApplicationAdditionalInfoService _aidApplicationAdditionalInfoService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AidApplicationAdditionalInfoController class.
        /// </summary>
        /// <param name="aidApplicationAdditionalInfoService">Aid Application Additional Info Service of type <see cref="IAidApplicationAdditionalInfoService">IAidApplicationAdditionalInfoService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AidApplicationAdditionalInfoController(IAidApplicationAdditionalInfoService aidApplicationAdditionalInfoService, ILogger logger)
        {
            _aidApplicationAdditionalInfoService = aidApplicationAdditionalInfoService;
            _logger = logger;
        }

        /// <summary>
        /// Return all aidApplicationAdditionalInfo
        /// </summary>
        /// <returns>List of AidApplicationAdditionalInfo <see cref="AidApplicationAdditionalInfo"/> objects representing all aid application additional info</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.AID.APPL.ADDITIONAL can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewAidApplicationAdditionalInfo)]
        [QueryStringFilterFilter("criteria", typeof(AidApplicationAdditionalInfo))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get all additional aid application information by criteria.",
            HttpMethodDescription = "Get all additional aid application information records matching the criteria.", HttpMethodPermission = "VIEW.AID.APPL.ADDITIONAL")]
        public async Task<IHttpActionResult> GetAidApplicationAdditionalInfoAsync(Paging page, QueryStringFilter criteria)
        {
            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var criteriaFilter = GetFilterObject<AidApplicationAdditionalInfo>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<AidApplicationAdditionalInfo>>(new List<AidApplicationAdditionalInfo>(), page, 0, this.Request);


                _aidApplicationAdditionalInfoService.ValidatePermissions(GetPermissionsMetaData());

                var pageOfItems = await _aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(page.Offset, page.Limit, criteriaFilter);

                
                AddEthosContextProperties(await _aidApplicationAdditionalInfoService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                              await _aidApplicationAdditionalInfoService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));
                
                return new PagedHttpActionResult<IEnumerable<AidApplicationAdditionalInfo>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) an aidApplicationAdditionalInfo using an Id
        /// </summary>
        /// <param name="id">Id to desired aidApplicationAdditionalInfo</param>
        /// <returns>An aidApplicationAdditionalInfo object <see cref="AidApplicationAdditionalInfo"/> in EEDM format</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.AID.APPL.ADDITIONAL can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewAidApplicationAdditionalInfo)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get the additional aid application information record by ID.",
            HttpMethodDescription = "Get the additional aid application information record by ID.", HttpMethodPermission = "VIEW.AID.APPL.ADDITIONAL")]
        public async Task<AidApplicationAdditionalInfo> GetAidApplicationAdditionalInfoByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The ID must be specified in the request URL.")));
            }
            try
            {
                _aidApplicationAdditionalInfoService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
                   await _aidApplicationAdditionalInfoService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _aidApplicationAdditionalInfoService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }


        /// <summary>
        /// Create (POST) additional aid application information
        /// </summary>
        /// <param name="aidApplicationAdditionalInfo">DTO of the new aidApplicationAdditionalInfo</param>
        /// <returns>An aidApplicationAdditionalInfo object <see cref="AidApplicationAdditionalInfo"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter, 
            PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationAdditionalInfo)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Create (POST)a new additional aid application information record.",
            HttpMethodDescription = "Create (POST) a new additional aid application information record.", HttpMethodPermission = "UPDATE.AID.APPL.ADDITIONAL")]
        public async Task<AidApplicationAdditionalInfo> PostAidApplicationAdditionalInfoAsync([FromBody] AidApplicationAdditionalInfo aidApplicationAdditionalInfo)
        {
            try
            {
                if (aidApplicationAdditionalInfo == null)
                {
                    throw new IntegrationApiException("Null aidApplicationAdditionalInfo argument",
                        IntegrationApiUtility.GetDefaultApiError("The request body is required."));
                }
                _aidApplicationAdditionalInfoService.ValidatePermissions(GetPermissionsMetaData());                

                //call import extend method that needs the extracted extension data and the config
                await _aidApplicationAdditionalInfoService.ImportExtendedEthosData(await ExtractExtendedData(await _aidApplicationAdditionalInfoService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var aidApplicationAdditionalInfoDto = await _aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfo);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _aidApplicationAdditionalInfoService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _aidApplicationAdditionalInfoService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { aidApplicationAdditionalInfoDto.Id }));

                return aidApplicationAdditionalInfoDto;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing aid application additional info
        /// </summary>
        /// <param name="id">Id of the aid application additional info to update</param>
        /// <param name="aidApplicationAdditionalInfo">DTO of the updated aid application additional info</param>
        /// <returns>A aidApplicationAdditionalInfo object <see cref="AidApplicationAdditionalInfo"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationAdditionalInfo)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Update (PUT) an existing additional aid application information record.",
            HttpMethodDescription = "Update (PUT) an existing additional aid application information record.", HttpMethodPermission = "UPDATE.AID.APPL.ADDITIONAL")]
        public async Task<AidApplicationAdditionalInfo> PutAidApplicationAdditionalInfoAsync([FromUri] string id, [ModelBinder(typeof(EthosEnabledBinder))] AidApplicationAdditionalInfo aidApplicationAdditionalInfo)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The Id must be specified in the request URL.")));
            }
            if (aidApplicationAdditionalInfo == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null request body",
                    IntegrationApiUtility.GetDefaultApiError("The request body must be specified in the request.")));
            }

            if (string.IsNullOrEmpty(aidApplicationAdditionalInfo.Id))
            {
                aidApplicationAdditionalInfo.Id = id;
            }
            if (string.IsNullOrEmpty(aidApplicationAdditionalInfo.AppDemoId))
            {
                aidApplicationAdditionalInfo.AppDemoId = id;
            }

            if (id != aidApplicationAdditionalInfo.Id)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Id mismatch error",
                    IntegrationApiUtility.GetDefaultApiError("The Id sent in request URL is not the same Id passed in request body.")));
            }
            if (!string.IsNullOrEmpty(aidApplicationAdditionalInfo.AppDemoId) && (aidApplicationAdditionalInfo.AppDemoId != aidApplicationAdditionalInfo.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Invalid Appdemo Id",
                IntegrationApiUtility.GetDefaultApiError("The AppDemoId needs to match Id.")));
            }
            try
            {
                _aidApplicationAdditionalInfoService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _aidApplicationAdditionalInfoService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _aidApplicationAdditionalInfoService.ImportExtendedEthosData(await ExtractExtendedData(await _aidApplicationAdditionalInfoService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                // get original DTO
                AidApplicationAdditionalInfo origAidApplicationAdditionalInfo = null;
                try
                {
                    origAidApplicationAdditionalInfo = await _aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoByIdAsync(id);
                }
                catch (KeyNotFoundException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw ex;
                }


                
                //get the merged DTO. 

                var mergedAdditionalInfo =
                    await PerformPartialPayloadMerge(aidApplicationAdditionalInfo, origAidApplicationAdditionalInfo, dpList, _logger);

                if (origAidApplicationAdditionalInfo != null && mergedAdditionalInfo != null && mergedAdditionalInfo.PersonId != origAidApplicationAdditionalInfo.PersonId)
                {
                    throw new IntegrationApiException("Person ID cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to personId is not allowed."));
                }

                if (origAidApplicationAdditionalInfo != null && mergedAdditionalInfo != null && origAidApplicationAdditionalInfo.AidYear != mergedAdditionalInfo.AidYear)
                {
                    throw new IntegrationApiException("aidYear cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to aidYear is not allowed."));
                }
                if (origAidApplicationAdditionalInfo != null && mergedAdditionalInfo != null && origAidApplicationAdditionalInfo.ApplicationType != mergedAdditionalInfo.ApplicationType)
                {
                    throw new IntegrationApiException("applicationType cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to applicationType is not allowed."));
                }
                var additionalInfoReturn = await _aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(id, mergedAdditionalInfo);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _aidApplicationAdditionalInfoService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _aidApplicationAdditionalInfoService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { additionalInfoReturn.Id }));

                return additionalInfoReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        /// <summary>
        /// Delete (DELETE) an existing aid application additional info
        /// </summary>
        /// <param name="id">Id of the aid application additional info to update</param>
        [HttpDelete]
        public Task DeleteAidApplicationAdditionalInfoAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}