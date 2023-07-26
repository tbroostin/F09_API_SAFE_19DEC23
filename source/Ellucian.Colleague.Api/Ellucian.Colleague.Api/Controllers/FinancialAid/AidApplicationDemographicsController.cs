// Copyright 2022 Ellucian Company L.P. and its affiliates.
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Provides access to Aid Application Demographics data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Metadata(ApiDescription = "Provides access to aid application demographics.", ApiDomain = "FA")]
    public class AidApplicationDemographicsController : BaseCompressedApiController
    {
        private readonly IAidApplicationDemographicsService _aidApplicationDemographicsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AidApplicationDemographicsController class.
        /// </summary>
        /// <param name="aidApplicationDemographicsService">Aid Application Demographics service<see cref="IAidApplicationDemographicsService">IAidApplicationDemographicsService</see></param>
        /// <param name="logger">Logger<see cref="ILogger">ILogger</see></param>
        public AidApplicationDemographicsController(IAidApplicationDemographicsService aidApplicationDemographicsService, ILogger logger)
        {
            _aidApplicationDemographicsService = aidApplicationDemographicsService;
            _logger = logger;
        }


        /// <summary>
        /// Read (GET) a AidApplicationDemographics using a Id
        /// </summary>
        /// <param name="id">Id to desired aid application demographics</param>
        /// <returns>A aidApplicationDemographics object <see cref="AidApplicationDemographics"/> in EEDM format</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.AID.APPL.DEMO can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewAidApplicationDemographics)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get the aid application demographics record by ID.",
            HttpMethodDescription = "Get the aid application demographics record by ID.", HttpMethodPermission = "VIEW.AID.APPL.DEMO")]
        public async Task<AidApplicationDemographics> GetAidApplicationDemographicsByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The ID must be specified in the request URL.")));
            }
            try
            {
                _aidApplicationDemographicsService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
                   await _aidApplicationDemographicsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _aidApplicationDemographicsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _aidApplicationDemographicsService.GetAidApplicationDemographicsByIdAsync(id);
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
        /// Return all AidApplicationDemographics
        /// </summary>
        /// <returns>List of AidApplicationDemographics <see cref="AidApplicationDemographics"/> objects representing all aid application demographics</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.AID.APPL.DEMO can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewAidApplicationDemographics)]
        [QueryStringFilterFilter("criteria", typeof(AidApplicationDemographics))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get all aid application demographics by criteria.",
            HttpMethodDescription = "Get all aid application demographics records matching the criteria.", HttpMethodPermission = "VIEW.AID.APPL.DEMO")]
        public async Task<IHttpActionResult> GetAidApplicationDemographicsAsync(Paging page, QueryStringFilter criteria)
        {
            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var criteriaFilter = GetFilterObject<AidApplicationDemographics>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<AidApplicationDemographics>>(new List<AidApplicationDemographics>(), page, 0, this.Request);


                _aidApplicationDemographicsService.ValidatePermissions(GetPermissionsMetaData());

                var pageOfItems = await _aidApplicationDemographicsService.GetAidApplicationDemographicsAsync(page.Offset, page.Limit, criteriaFilter);

                AddEthosContextProperties(await _aidApplicationDemographicsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                              await _aidApplicationDemographicsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<AidApplicationDemographics>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Create (POST) a new aid application demographics
        /// </summary>
        /// <param name="aidApplicationDemographics">DTO of the new aid application demographics</param>
        /// <returns>A AidApplicationDemographics object <see cref="AidApplicationDemographics"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationDemographics)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Create (POST) a new aid application demographics record.",
            HttpMethodDescription = "Create (POST) a new aid application demographics record.", HttpMethodPermission = "UPDATE.AID.APPL.DEMO")]

        public async Task<AidApplicationDemographics> PostAidApplicationDemographicsAsync([FromBody] AidApplicationDemographics aidApplicationDemographics)
        {
            if (aidApplicationDemographics == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null aidApplicationDemographics argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                _aidApplicationDemographicsService.ValidatePermissions(GetPermissionsMetaData());

                //call import extend method that needs the extracted extension data and the config
                await _aidApplicationDemographicsService.ImportExtendedEthosData(await ExtractExtendedData(await _aidApplicationDemographicsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var aidApplicationDemographicsDto = await _aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographics);

                //store dataprivacy list and get the extended data to store

                AddEthosContextProperties(await _aidApplicationDemographicsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _aidApplicationDemographicsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { aidApplicationDemographicsDto.Id }));

                return aidApplicationDemographicsDto;
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
        /// Update (PUT) an existing aid application demographics
        /// </summary>
        /// <param name="id">Id of the aid application demographics to update</param>
        /// <param name="aidApplicationDemographics">DTO of the updated aid application demographics</param>
        /// <returns>A AidApplicationDemographics object <see cref="AidApplicationDemographics"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationDemographics)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Update (PUT) an existing aid application demographics record.",
            HttpMethodDescription = "Update (PUT) an existing aid application demographics record.", HttpMethodPermission = "UPDATE.AID.APPL.DEMO")]
        public async Task<AidApplicationDemographics> PutAidApplicationDemographicsAsync([FromUri] string id, [ModelBinder(typeof(EthosEnabledBinder))] AidApplicationDemographics aidApplicationDemographics)
        {

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The Id must be specified in the request URL.")));
            }
            if (aidApplicationDemographics == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null request body", 
                    IntegrationApiUtility.GetDefaultApiError("The request body must be specified in the request.")));

            }
            if (string.IsNullOrEmpty(aidApplicationDemographics.Id))
            {
                aidApplicationDemographics.Id = id;
            }

            if (id != aidApplicationDemographics.Id)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Id mismatch error",
                    IntegrationApiUtility.GetDefaultApiError("The Id sent in request URL is not the same Id passed in request body.")));
            }
            try
            {
                _aidApplicationDemographicsService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _aidApplicationDemographicsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _aidApplicationDemographicsService.ImportExtendedEthosData(await ExtractExtendedData(await _aidApplicationDemographicsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                // get original DTO
                AidApplicationDemographics origAidApplicationDemographics = null;
                try
                {
                    origAidApplicationDemographics = await _aidApplicationDemographicsService.GetAidApplicationDemographicsByIdAsync(id);
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
                var mergedAidApplicationDemo =
                    await PerformPartialPayloadMerge(aidApplicationDemographics, origAidApplicationDemographics, dpList, _logger);

                if (origAidApplicationDemographics != null && mergedAidApplicationDemo != null && origAidApplicationDemographics.PersonId != mergedAidApplicationDemo.PersonId)
                {
                    throw new IntegrationApiException("Person ID cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to personId is not allowed."));
                }
                if  (origAidApplicationDemographics != null && mergedAidApplicationDemo != null && origAidApplicationDemographics.AidYear != mergedAidApplicationDemo.AidYear)
                {
                    throw new IntegrationApiException("aidYear cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to aidYear is not allowed."));
                }
                if  (origAidApplicationDemographics != null && mergedAidApplicationDemo != null && origAidApplicationDemographics.ApplicationType != mergedAidApplicationDemo.ApplicationType)
                {
                    throw new IntegrationApiException("applicationType cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to applicationType is not allowed."));
                }
                var aidApplicationDemoReturn = await _aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(id, mergedAidApplicationDemo);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _aidApplicationDemographicsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _aidApplicationDemographicsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { aidApplicationDemoReturn.Id }));

                return aidApplicationDemoReturn;
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
        /// Delete (DELETE) an existing aid application demographics
        /// </summary>
        /// <param name="id">Id of the aid application demographics to update</param>
        [HttpDelete]
        public Task DeleteAidApplicationDemographicsAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
