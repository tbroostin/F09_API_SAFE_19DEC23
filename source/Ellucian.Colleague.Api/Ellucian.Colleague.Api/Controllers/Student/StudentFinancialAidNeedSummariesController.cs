//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
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

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentFinancialAidNeedSummaries
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidNeedSummariesController : BaseCompressedApiController
    {
        private readonly IStudentFinancialAidNeedSummaryService StudentFinancialAidNeedSummaryService;
        //private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the StudentFinancialAidNeedSummariesController class.
        /// </summary>
        /// <param name="StudentFinancialAidNeedSummaryService">StudentFinancialAidNeedSummaryService</param>
        /// <param name="logger">Interface to logger</param>
        public StudentFinancialAidNeedSummariesController(IStudentFinancialAidNeedSummaryService StudentFinancialAidNeedSummaryService,
            ILogger logger)
        {
            this.StudentFinancialAidNeedSummaryService = StudentFinancialAidNeedSummaryService;
            this.logger = logger;
        }

        /// <summary>
        /// Return all StudentFinancialAidNeedSummaries
        /// </summary>
        /// <returns>List of StudentFinancialAidNeedSummaries</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetStudentFinancialAidNeedSummariesAsync(Paging page)
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
                var pageOfItems = await StudentFinancialAidNeedSummaryService.GetAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                  await StudentFinancialAidNeedSummaryService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await StudentFinancialAidNeedSummaryService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidNeedSummary>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a StudentFinancialAidNeedSummaries using a GUID
        /// </summary>
        /// <param name="id">GUID to desired StudentFinancialAidNeedSummaries</param>
        /// <returns>A single StudentFinancialAidNeedSummaries object</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentFinancialAidNeedSummary> GetStudentFinancialAidNeedSummariesByGuidAsync(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                 await StudentFinancialAidNeedSummaryService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await StudentFinancialAidNeedSummaryService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { id }));
                return await StudentFinancialAidNeedSummaryService.GetByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new StudentFinancialAidNeedSummaries
        /// </summary>
        /// <param name="StudentFinancialAidNeedSummaries">DTO of the new StudentFinancialAidNeedSummaries</param>
        /// <returns>A single StudentFinancialAidNeedSummaries object</returns>
        [HttpPost]
        public async Task<Dtos.StudentFinancialAidNeedSummary> CreateAsync([FromBody] Dtos.StudentFinancialAidNeedSummary StudentFinancialAidNeedSummaries)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing StudentFinancialAidNeedSummaries
        /// </summary>
        /// <param name="id">GUID of the StudentFinancialAidNeedSummaries to update</param>
        /// <param name="StudentFinancialAidNeedSummaries">DTO of the updated StudentFinancialAidNeedSummarys</param>
        /// <returns>A single StudentFinancialAidNeedSummaries object</returns>
        [HttpPut]
        public async Task<Dtos.StudentFinancialAidNeedSummary> UpdateAsync([FromUri] string id, [FromBody] Dtos.StudentFinancialAidNeedSummary StudentFinancialAidNeedSummaries)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a StudentFinancialAidNeedSummaries
        /// </summary>
        /// <param name="id">GUID to desired StudentFinancialAidNeedSummaries</param>
        [HttpDelete]
        public async Task DeleteAsync(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
