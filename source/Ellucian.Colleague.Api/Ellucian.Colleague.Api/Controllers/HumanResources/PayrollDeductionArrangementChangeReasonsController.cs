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
    /// Exposes payroll deduction arrangement change reasons data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayrollDeductionArrangementChangeReasonsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPayrollDeductionArrangementChangeReasonsService _payrollDeductionArrangementChangeReasonsService;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="payrollDeductionArrangementChangeReasonsService"></param>
        public PayrollDeductionArrangementChangeReasonsController(ILogger logger, IPayrollDeductionArrangementChangeReasonsService payrollDeductionArrangementChangeReasonsService)
        {
            this.logger = logger;
            this._payrollDeductionArrangementChangeReasonsService = payrollDeductionArrangementChangeReasonsService;
        }

        /// <summary>
        /// Returns all payroll deduction arrangement change reasons.
        /// </summary>
        /// <returns></returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.PayrollDeductionArrangementChangeReason>> GetAllPayrollDeductionArrangementChangeReasonsAsync()
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
                var payrollDeductionArrangementChangeReasons = await _payrollDeductionArrangementChangeReasonsService.GetPayrollDeductionArrangementChangeReasonsAsync(bypassCache);

                if (payrollDeductionArrangementChangeReasons != null && payrollDeductionArrangementChangeReasons.Any())
                {
                    AddEthosContextProperties(await _payrollDeductionArrangementChangeReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _payrollDeductionArrangementChangeReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              payrollDeductionArrangementChangeReasons.Select(a => a.Id).ToList()));
                }
                return payrollDeductionArrangementChangeReasons;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting payroll deduction arrangement change reasons.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns a payroll deduction arrangement change reason.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PayrollDeductionArrangementChangeReason> GetPayrollDeductionArrangementChangeReasonByIdAsync([FromUri] string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _payrollDeductionArrangementChangeReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _payrollDeductionArrangementChangeReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _payrollDeductionArrangementChangeReasonsService.GetPayrollDeductionArrangementChangeReasonByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e, "No payroll deduction arrangement change reasons was found for guid " + id + ".");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting payroll deduction arrangement change reason.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// PutPayrollDeductionArrangementChangeReasonAsync
        /// </summary>
        /// <param name="payrollDeductionArrangementChangeReason"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<Dtos.PayrollDeductionArrangementChangeReason> PutPayrollDeductionArrangementChangeReasonAsync([FromBody] Dtos.PayrollDeductionArrangementChangeReason payrollDeductionArrangementChangeReason)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// PostPayrollDeductionArrangementChangeReasonAsync
        /// </summary>
        /// <param name="payrollDeductionArrangementChangeReason"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Dtos.PayrollDeductionArrangementChangeReason> PostPayrollDeductionArrangementChangeReasonAsync([FromBody] Dtos.PayrollDeductionArrangementChangeReason payrollDeductionArrangementChangeReason)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// DeletePayrollDeductionArrangementChangeReasonAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeletePayrollDeductionArrangementChangeReasonAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}