using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Web;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Configuration;
using System.Text.RegularExpressions;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Coordination.Finance;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Colleague.Coordination.F09.Services;
using static System.String;

namespace Ellucian.Colleague.Api.Controllers.F09
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class F09TuitionPaymentController: BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IF09TuitionPaymentPlanService _tuitionPaymentPlanService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tuitionPaymentPlanService"></param>
        /// <param name="logger"></param>
        public F09TuitionPaymentController(IF09TuitionPaymentPlanService tuitionPaymentPlanService, ILogger logger)
        {
            _tuitionPaymentPlanService = tuitionPaymentPlanService;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="HttpResponseException"></exception>
        public async Task<F09PaymentFormDto> GetFormInformationAsync(string studentId)
        {
            F09PaymentFormDto ret = null;
            try
            {
                if (IsNullOrWhiteSpace(studentId)) { throw new ArgumentNullException(nameof(studentId));}

                ret = await _tuitionPaymentPlanService.GetTuitionPaymentFormAsync(studentId);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.NotFound);
            }
            catch(Exception e)
            {
                _logger.Error(e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentPlan"></param>
        /// <returns></returns>
        public async Task<F09PaymentInvoiceDto> PostPaymentPlanAsync(F09TuitionPaymentPlanDto paymentPlan)
        {
            if(paymentPlan == null) throw new ArgumentNullException(nameof(paymentPlan));
            if(IsNullOrWhiteSpace(paymentPlan.StudentId)) throw new ArgumentNullException(nameof(paymentPlan.StudentId), "Student Id is Required");

            F09PaymentInvoiceDto invoice = null;
            try
            {
                invoice = await _tuitionPaymentPlanService.SubmitTuitionPaymentFormAsync(paymentPlan);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.NotFound);
            }
            catch(Exception e)
            {
                _logger.Error(e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

            return invoice;
        }
    }
}