//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
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
    /// Provides access to FinancialAidApplications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidApplicationsController : BaseCompressedApiController
    {
        private readonly IStudentFinancialAidApplicationService studentFinancialAidApplicationService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidApplicationsController class.
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>   
        /// <param name="studentFinancialAidApplicationService">StudentFinancialAidApplicationService</param>
        /// <param name="logger">Interface to logger</param>
        public StudentFinancialAidApplicationsController(IAdapterRegistry adapterRegistry,
            IStudentFinancialAidApplicationService studentFinancialAidApplicationService,
            ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.studentFinancialAidApplicationService = studentFinancialAidApplicationService;
            this.logger = logger;
        }

        /// <summary>
        /// Return all financialAidApplications
        /// </summary>
        /// <returns>List of FinancialAidApplications <see cref="Dtos.FinancialAidApplication"/> objects representing matching financialAidApplications</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetAsync(Paging page)
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
                var pageOfItems = await studentFinancialAidApplicationService.GetAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await studentFinancialAidApplicationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidApplicationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.FinancialAidApplication>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Read (GET) a financialAidApplications using a GUID
        /// </summary>
        /// <param name="id">GUID to desired financialAidApplications</param>
        /// <returns>A financialAidApplications object <see cref="Dtos.FinancialAidApplication"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.FinancialAidApplication> GetByIdAsync(string id)
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
                    await studentFinancialAidApplicationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidApplicationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await studentFinancialAidApplicationService.GetByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Create (POST) a new financialAidApplications
        /// </summary>
        /// <param name="financialAidApplications">DTO of the new financialAidApplications</param>
        /// <returns>A financialAidApplications object <see cref="Dtos.FinancialAidApplication"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.FinancialAidApplication> CreateAsync([FromBody] Dtos.FinancialAidApplication financialAidApplications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing financialAidApplications
        /// </summary>
        /// <param name="id">GUID of the financialAidApplications to update</param>
        /// <param name="financialAidApplications">DTO of the updated financialAidApplications</param>
        /// <returns>A financialAidApplications object <see cref="Dtos.FinancialAidApplication"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.FinancialAidApplication> UpdateAsync([FromUri] string id, [FromBody] Dtos.FinancialAidApplication financialAidApplications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a financialAidApplications
        /// </summary>
        /// <param name="id">GUID to desired financialAidApplications</param>
        [HttpDelete]
        public async Task DeleteAsync(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
