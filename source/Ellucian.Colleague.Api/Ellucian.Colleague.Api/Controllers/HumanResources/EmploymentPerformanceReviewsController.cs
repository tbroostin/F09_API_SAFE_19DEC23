//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

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
using System.Net.Http;
using Ellucian.Colleague.Domain.Base.Exceptions;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentPerformanceReviews
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentPerformanceReviewsController : BaseCompressedApiController
    {
        private readonly IEmploymentPerformanceReviewsService _employmentPerformanceReviewsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentPerformanceReviewsController class.
        /// </summary>
        /// <param name="employmentPerformanceReviewsService">Service of type <see cref="IEmploymentPerformanceReviewsService">IEmploymentPerformanceReviewsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EmploymentPerformanceReviewsController(IEmploymentPerformanceReviewsService employmentPerformanceReviewsService, ILogger logger)
        {
            _employmentPerformanceReviewsService = employmentPerformanceReviewsService;
            _logger = logger;
        }

        #region GET Methods
        /// <summary>
        /// Return all employmentPerformanceReviews
        /// </summary>
        /// <param name="page">Page of items for Paging</param>
        /// <returns>List of EmploymentPerformanceReviews <see cref="Dtos.EmploymentPerformanceReviews"/> objects representing matching employmentPerformanceReviews</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewEmploymentPerformanceReview,
            HumanResourcesPermissionCodes.CreateUpdateEmploymentPerformanceReview })]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetEmploymentPerformanceReviewsAsync(Paging page)
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

            try
            {
                _employmentPerformanceReviewsService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _employmentPerformanceReviewsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _employmentPerformanceReviewsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.EmploymentPerformanceReviews>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a employmentPerformanceReviews using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employmentPerformanceReviews</param>
        /// <returns>A employmentPerformanceReviews object <see cref="Dtos.EmploymentPerformanceReviews"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { HumanResourcesPermissionCodes.ViewEmploymentPerformanceReview,
            HumanResourcesPermissionCodes.CreateUpdateEmploymentPerformanceReview })]
        [EedmResponseFilter]
        public async Task<Dtos.EmploymentPerformanceReviews> GetEmploymentPerformanceReviewsByGuidAsync(string guid)
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
                _employmentPerformanceReviewsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _employmentPerformanceReviewsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _employmentPerformanceReviewsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsByGuidAsync(guid);
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        #endregion

        #region POST Method
        /// <summary>
        /// Create (POST) a new employmentPerformanceReviews
        /// </summary>
        /// <param name="employmentPerformanceReviews">DTO of the new employmentPerformanceReviews</param>
        /// <returns>An EmploymentPerformanceReviews DTO object <see cref="Dtos.EmploymentPerformanceReviews"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, EedmResponseFilter,PermissionsFilter(HumanResourcesPermissionCodes.CreateUpdateEmploymentPerformanceReview) ]
        public async Task<Dtos.EmploymentPerformanceReviews> PostEmploymentPerformanceReviewsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.EmploymentPerformanceReviews employmentPerformanceReviews)
        {

            if (employmentPerformanceReviews == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null employmentPerformanceReviews argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            try
            {
                _employmentPerformanceReviewsService.ValidatePermissions(GetPermissionsMetaData());
                ValidateEmploymentPerformanceReviews(employmentPerformanceReviews);

                //call import extend method that needs the extracted extension data and the config
                await _employmentPerformanceReviewsService.ImportExtendedEthosData(await ExtractExtendedData(await _employmentPerformanceReviewsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                // Create the performance review
                var performanceReview = await _employmentPerformanceReviewsService.PostEmploymentPerformanceReviewsAsync(employmentPerformanceReviews);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _employmentPerformanceReviewsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _employmentPerformanceReviewsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { performanceReview.Id }));

                return performanceReview;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        #endregion

        #region PUT Method
        /// <summary>
        /// Update (PUT) an existing employmentPerformanceReviews
        /// </summary>
        /// <param name="guid">GUID of the employmentPerformanceReviews to update</param>
        /// <param name="employmentPerformanceReviews">DTO of the updated employmentPerformanceReviews</param>
        /// <returns>An EmploymentPerformanceReviews DTO object <see cref="Dtos.EmploymentPerformanceReviews"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter, PermissionsFilter(HumanResourcesPermissionCodes.CreateUpdateEmploymentPerformanceReview)]
        public async Task<Dtos.EmploymentPerformanceReviews> PutEmploymentPerformanceReviewsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.EmploymentPerformanceReviews employmentPerformanceReviews)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (employmentPerformanceReviews == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null employmentPerformanceReviews argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(employmentPerformanceReviews.Id))
            {
                employmentPerformanceReviews.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(employmentPerformanceReviews.Id, Guid.Empty.ToString())))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != employmentPerformanceReviews.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _employmentPerformanceReviewsService.ValidatePermissions(GetPermissionsMetaData());
                await DoesUpdateViolateDataPrivacySettings(employmentPerformanceReviews, await _employmentPerformanceReviewsService.GetDataPrivacyListByApi(GetRouteResourceName(), true), _logger);

                ValidateEmploymentPerformanceReviews(employmentPerformanceReviews);
                
                //get Data Privacy List
                var dpList = await _employmentPerformanceReviewsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _employmentPerformanceReviewsService.ImportExtendedEthosData(await ExtractExtendedData(await _employmentPerformanceReviewsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                // Update the performance review
                var performanceReview = await _employmentPerformanceReviewsService.PutEmploymentPerformanceReviewsAsync(guid,
                    await PerformPartialPayloadMerge(employmentPerformanceReviews,
                    async() => await _employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsByGuidAsync(guid), dpList, _logger));

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                   await _employmentPerformanceReviewsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { performanceReview.Id }));

                return performanceReview;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        #endregion

        #region DELETE method
        /// <summary>
        /// Deletes employment performance review based on id
        /// </summary>
        /// <param name="guid">id</param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete, PermissionsFilter(HumanResourcesPermissionCodes.DeleteEmploymentPerformanceReview)]
        public async Task DeleteEmploymentPerformanceReviewsAsync(string guid)
        {
            try
            {
                _employmentPerformanceReviewsService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("Employment performance review guid cannot be null or empty");
                }
                await _employmentPerformanceReviewsService.DeleteEmploymentPerformanceReviewAsync(guid);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        #endregion

        #region Helper Methods
        /// <summary>
        /// Helper method to validate employment-performance-reviews PUT/POST.
        /// </summary>
        /// <param name="employmentPerformanceReviews"><see cref="Dtos.EmploymentPerformanceReviews"/>EmploymentPerformanceReviews DTO object of type</param>
        private void ValidateEmploymentPerformanceReviews(Dtos.EmploymentPerformanceReviews employmentPerformanceReviews)
        {

            if (employmentPerformanceReviews == null)
            {
                throw new ArgumentNullException("employmentPerformanceReviews", "The body is required when submitting an employmentPerformanceReviews. ");
            }

        }
        #endregion
    }
}