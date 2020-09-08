//Copyright 2016-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes Employment Leave of Absence Reasons data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentLeaveOfAbsenceReasonsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IEmploymentStatusEndingReasonService _employmentStatusEndingReasonService;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="employmentStatusEndingReasonService"></param>
        public EmploymentLeaveOfAbsenceReasonsController(ILogger logger, IEmploymentStatusEndingReasonService employmentStatusEndingReasonService)
        {
            this.logger = logger;
            this._employmentStatusEndingReasonService = employmentStatusEndingReasonService;
        }

        /// <summary>
        /// Returns all employment leave of absence reasons
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.EmploymentStatusEndingReason>> GetAllEmploymentLeaveOfAbsenceReasonsAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                var employmentLeaveOfAbsenceReasons = await _employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonsAsync(bypassCache);

                if (employmentLeaveOfAbsenceReasons != null && employmentLeaveOfAbsenceReasons.Any())
                {
                    AddEthosContextProperties(await _employmentStatusEndingReasonService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _employmentStatusEndingReasonService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              employmentLeaveOfAbsenceReasons.Select(a => a.Id).ToList()));
                }

                return employmentLeaveOfAbsenceReasons;                
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting employment leave of absence reasons.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns an employment leave of absence reason.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.EmploymentStatusEndingReason> GetEmploymentLeaveOfAbsenceReasonByIdAsync([FromUri] string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _employmentStatusEndingReasonService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _employmentStatusEndingReasonService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e, "No employment leave of absence reason was found for guid " + id + ".");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting employment leave of absence reason.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// PutEmploymentLeaveOfAbsenceReasonAsync.
        /// </summary>
        /// <param name="employmentStatusEndingReason"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<Dtos.EmploymentStatusEndingReason> PutEmploymentLeaveOfAbsenceReasonAsync([FromBody] Dtos.EmploymentStatusEndingReason employmentStatusEndingReason)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// PostEmploymentLeaveOfAbsenceReasonAsync.
        /// </summary>
        /// <param name="employmentStatusEndingReason"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Dtos.EmploymentStatusEndingReason> PostEmploymentLeaveOfAbsenceReasonAsync([FromBody] Dtos.EmploymentStatusEndingReason employmentStatusEndingReason)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// DeleteEmploymentLeaveOfAbsenceReasonAsync.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteEmploymentLeaveOfAbsenceReasonAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}