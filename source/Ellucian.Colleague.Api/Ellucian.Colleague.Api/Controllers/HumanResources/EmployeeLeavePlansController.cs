/*Copyright 2017-2018 Ellucian Company L.P. and its affiliates.*/

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
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using System.Collections;
using Ellucian.Colleague.Dtos.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmployeeLeavePlans
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeeLeavePlansController : BaseCompressedApiController
    {
        private readonly IEmployeeLeavePlansService employeeLeavePlansService;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the EmployeeLeavePlansController class.
        /// </summary>
        /// <param name="employeeLeavePlansService">Service of type <see cref="IEmployeeLeavePlansService">IEmployeeLeavePlansService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EmployeeLeavePlansController(IEmployeeLeavePlansService employeeLeavePlansService, ILogger logger)
        {
            this.employeeLeavePlansService = employeeLeavePlansService;
            this.logger = logger;
        }

        /// <summary>
        /// Return all employeeLeavePlans for EEDM
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
                /// <returns>List of EmployeeLeavePlans <see cref="Dtos.EmployeeLeavePlans"/> objects representing matching employeeLeavePlans</returns>
        [HttpGet, EedmResponseFilter]       
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetEmployeeLeavePlansAsync(Paging page)
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
                    page = new Paging(200, 0);
                }

                var pageOfItems = await employeeLeavePlansService.GetEmployeeLeavePlansAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await employeeLeavePlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await employeeLeavePlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));
                
                return new PagedHttpActionResult<IEnumerable<Dtos.EmployeeLeavePlans>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Return all EmployeeLeavePlan objects that you have permission to access. As an employeee, you will have access to only your leave plans.
        /// As a supervisor (or the proxy of a supervisor), you will have access to the leave plans of your (or your proxy's) direct reports.
        /// This is used by Self Service
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for passing the effective person id in a proxy scenario</param>
        /// <accessComments>
        /// 1. As an employee you have access to your own leave plans
        /// 2. As a supervisor with the APPROVE.REJECT.TIME.ENTRY permission, you have access to your own leave plans and your supervisees' leave plans
        /// 3. As the proxy of a supervisor, you have access to that supervisor's leave plans and that supervisor's supervisees' leave plans.
        /// 4. As an admin, you have access to anyone's leave plans
        /// </accessComments>
        /// <returns>A collection of EmployeeLeavePlan objects</returns>
        [HttpGet]
        public async Task<IEnumerable<EmployeeLeavePlan>> GetEmployeeLeavePlansV2Async(string effectivePersonId = null)
        {
            try
            {
                return await employeeLeavePlansService.GetEmployeeLeavePlansV2Async(effectivePersonId: effectivePersonId, bypassCache: false);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Read (GET) a employeeLeavePlans using a GUID for EEDM
        /// </summary>
        /// <param name="guid">GUID to desired employeeLeavePlans</param>
        /// <returns>A employeeLeavePlans object <see cref="Dtos.EmployeeLeavePlans"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.EmployeeLeavePlans> GetEmployeeLeavePlansByGuidAsync(string guid)
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
                    await employeeLeavePlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await employeeLeavePlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await employeeLeavePlansService.GetEmployeeLeavePlansByGuidAsync(guid);
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
        /// Create (POST) a new employeeLeavePlans for EEDM
        /// </summary>
        /// <param name="employeeLeavePlans">DTO of the new employeeLeavePlans</param>
        /// <returns>A employeeLeavePlans object <see cref="Dtos.EmployeeLeavePlans"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.EmployeeLeavePlans> PostEmployeeLeavePlansAsync([FromBody] Dtos.EmployeeLeavePlans employeeLeavePlans)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employeeLeavePlans for EEDM
        /// </summary>
        /// <param name="guid">GUID of the employeeLeavePlans to update</param>
        /// <param name="employeeLeavePlans">DTO of the updated employeeLeavePlans</param>
        /// <returns>A employeeLeavePlans object <see cref="Dtos.EmployeeLeavePlans"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.EmployeeLeavePlans> PutEmployeeLeavePlansAsync([FromUri] string guid, [FromBody] Dtos.EmployeeLeavePlans employeeLeavePlans)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employeeLeavePlans for EEDM
        /// </summary>
        /// <param name="guid">GUID to desired employeeLeavePlans</param>
        [HttpDelete]
        public async Task DeleteEmployeeLeavePlansAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}