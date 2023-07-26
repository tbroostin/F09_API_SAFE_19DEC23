// Copyright 2023 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to Aid Application Results data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Metadata(ApiDescription = "Provides access to aid application results.", ApiDomain = "FA")]
    public class AidApplicationResultsController : BaseCompressedApiController
    {
        private readonly IAidApplicationResultsService _aidApplicationResultsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AidApplicationResultsController class.
        /// </summary>
        /// <param name="aidApplicationResultsService">Aid Application Results service<see cref="IAidApplicationResultsService">IAidApplicationResultsService</see></param>
        /// <param name="logger">Logger<see cref="ILogger">ILogger</see></param>
        public AidApplicationResultsController(IAidApplicationResultsService aidApplicationResultsService, ILogger logger)
        {
            _aidApplicationResultsService = aidApplicationResultsService;
            _logger = logger;
        }


        /// <summary>
        /// Read (GET) a AidApplicationResults using a Id
        /// </summary>
        /// <param name="id">Id to desired aid application results</param>
        /// <returns>A aidApplicationResults object <see cref="AidApplicationResults"/> in EEDM format</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.AID.APPLICATION.RESULTS can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewAidApplicationResults)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get the aid application results record by ID.",
            HttpMethodDescription = "Get the aid application results record by ID.", HttpMethodPermission = "VIEW.AID.APPLICATION.RESULTS")]
        public async Task<AidApplicationResults> GetAidApplicationResultsByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The ID must be specified in the request URL.")));
            }
            try
            {
                _aidApplicationResultsService.ValidatePermissions(GetPermissionsMetaData());

                AddEthosContextProperties(
                   await _aidApplicationResultsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _aidApplicationResultsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _aidApplicationResultsService.GetAidApplicationResultsByIdAsync(id);
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
        /// Return all AidApplicationResults
        /// </summary>
        /// <returns>List of AidApplicationResults <see cref="AidApplicationResults"/> objects representing all aid application results</returns>
        /// <accessComments>
        /// Authenticated users with VIEW.AID.APPLICATION.RESULTS can query.
        /// </accessComments>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewAidApplicationResults)]
        [QueryStringFilterFilter("criteria", typeof(AidApplicationResults))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Get all aid application results records by criteria.",
            HttpMethodDescription = "Get all aid application results records matching the criteria.", HttpMethodPermission = "VIEW.AID.APPLICATION.RESULTS")]
        public async Task<IHttpActionResult> GetAidApplicationResultsAsync(Paging page, QueryStringFilter criteria)
        {
            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var criteriaFilter = GetFilterObject<AidApplicationResults>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<AidApplicationResults>>(new List<AidApplicationResults>(), page, 0, this.Request);


                _aidApplicationResultsService.ValidatePermissions(GetPermissionsMetaData());

                var pageOfItems = await _aidApplicationResultsService.GetAidApplicationResultsAsync(page.Offset, page.Limit, criteriaFilter);

                AddEthosContextProperties(await _aidApplicationResultsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                              await _aidApplicationResultsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<AidApplicationResults>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Create (POST) aid application results
        /// </summary>
        /// <param name="aidApplicationResults">DTO of the new aidApplicationResults</param>
        /// <returns>An aidApplicationResults object <see cref="AidApplicationResults"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter,
            PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationResults)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Create (POST) aid application results record.",
            HttpMethodDescription = "Create (POST) a new aid application results record.", HttpMethodPermission = "UPDATE.AID.APPLICATION.RESULTS")]
        public async Task<AidApplicationResults> PostAidApplicationResultsAsync([FromBody] AidApplicationResults aidApplicationResults)
        {
            if (aidApplicationResults == null)
            {
                throw new IntegrationApiException("Null aidApplicationResults argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required."));
            }
            try
            {                
                _aidApplicationResultsService.ValidatePermissions(GetPermissionsMetaData());

                //call import extend method that needs the extracted extension data and the config
                await _aidApplicationResultsService.ImportExtendedEthosData(await ExtractExtendedData(await _aidApplicationResultsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var aidApplicationResultsDto = await _aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResults);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _aidApplicationResultsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _aidApplicationResultsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { aidApplicationResultsDto.Id }));

                return aidApplicationResultsDto;
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
        /// Update (PUT) an existing aid application results
        /// </summary>
        /// <param name="id">Id of the aid application results to update</param>
        /// <param name="aidApplicationResults">DTO of the updated aid application results</param>
        /// <returns>A aidApplicationResults object <see cref="AidApplicationResults"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationResults)]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Update (PUT) aid application results record.",
            HttpMethodDescription = "Update (PUT) an existing aid application results record.", HttpMethodPermission = "UPDATE.AID.APPLICATION.RESULTS")]
        public async Task<AidApplicationResults> PutAidApplicationResultsAsync([FromUri] string id, [ModelBinder(typeof(EthosEnabledBinder))] AidApplicationResults aidApplicationResults)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The Id must be specified in the request URL.")));
            }
            if (aidApplicationResults == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null request body",
                    IntegrationApiUtility.GetDefaultApiError("The request body must be specified in the request.")));
            }

            if (string.IsNullOrEmpty(aidApplicationResults.Id))
            {
                aidApplicationResults.Id = id;
            }
            if (string.IsNullOrEmpty(aidApplicationResults.AppDemoId))
            {
                aidApplicationResults.AppDemoId = id;
            }

            if (id != aidApplicationResults.Id)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Id mismatch error",
                    IntegrationApiUtility.GetDefaultApiError("The Id sent in request URL is not the same Id passed in request body.")));
            }
            if (!string.IsNullOrEmpty(aidApplicationResults.AppDemoId) && (aidApplicationResults.AppDemoId != aidApplicationResults.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Invalid Appdemo Id",
                IntegrationApiUtility.GetDefaultApiError("The AppDemoId needs to match with Id.")));
            }
            try
            {
                _aidApplicationResultsService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _aidApplicationResultsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _aidApplicationResultsService.ImportExtendedEthosData(await ExtractExtendedData(await _aidApplicationResultsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                // get original DTO
                AidApplicationResults origAidApplicationResults = null;
                try
                {
                    origAidApplicationResults = await _aidApplicationResultsService.GetAidApplicationResultsByIdAsync(id);
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

                var mergedResults =
                    await PerformPartialPayloadMerge(aidApplicationResults, origAidApplicationResults, dpList, _logger);

                if (origAidApplicationResults != null && mergedResults != null && mergedResults.PersonId != origAidApplicationResults.PersonId)
                {
                    throw new IntegrationApiException("Person ID cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to personId is not allowed."));
                }

                if (origAidApplicationResults != null && mergedResults != null && origAidApplicationResults.AidYear != mergedResults.AidYear)
                {
                    throw new IntegrationApiException("aidYear cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to aidYear is not allowed."));
                }
                if (origAidApplicationResults != null && mergedResults != null && origAidApplicationResults.ApplicationType != mergedResults.ApplicationType)
                {
                    throw new IntegrationApiException("applicationType cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to applicationType is not allowed."));
                }
                if (origAidApplicationResults != null && mergedResults != null && origAidApplicationResults.ApplicantAssignedId != mergedResults.ApplicantAssignedId)
                {
                    throw new IntegrationApiException("applicantAssignedId cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to applicantAssignedId is not allowed."));
                }
                if (origAidApplicationResults != null && mergedResults != null && origAidApplicationResults.AppDemoId != mergedResults.AppDemoId)
                {
                    throw new IntegrationApiException("AppDemoId cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("Update to appDemoId is not allowed."));
                }
                var resultsReturn = await _aidApplicationResultsService.PutAidApplicationResultsAsync(id, mergedResults);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _aidApplicationResultsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _aidApplicationResultsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { resultsReturn.Id }));

                return resultsReturn;
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
        /// Delete (DELETE) an existing aid application results
        /// </summary>
        /// <param name="id">Id of the aid application results to update</param>
        [HttpDelete]
        public Task DeleteAidApplicationResultsAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }        
    }
}
