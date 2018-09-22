/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers.Base
{

    /// <summary>
    /// These routes are obsolete. Use PayableDepositDirective routes instead
    /// </summary>
    [Obsolete("Obsolete as of API 1.116")]
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PayableDepositAccountsController : BaseCompressedApiController
    {
        private readonly ILogger logger;

        /// <summary>
        /// Instantiate a new PayableDepositAccountsController
        /// </summary>
        /// <param name="logger"></param>
        public PayableDepositAccountsController(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// This route is obsolete as of API 1.16 for security reasons. Use GetPayableDepositDirectives instead
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpGet]
        public async Task<IEnumerable<PayableDepositAccount>> GetPayableDepositsAsync()
        {
            await Task.FromResult<IEnumerable<PayableDepositAccount>>(null);
            SetResourceLocationHeader("GetPayableDepositDirectives");
            throw CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);
        }

        /// <summary>
        /// This route is obsolete as of API 1.16 for security reasons. Use GetPayableDepositDirective instead
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpGet]
        public async Task<PayableDepositAccount> GetPayableDepositAsync([FromUri] string id)
        {

            await Task.FromResult<PayableDepositAccount>(null);
            SetResourceLocationHeader("GetPayableDepositDirective", new { id = id });
            throw CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);

        }

        /// <summary>
        /// This route is obsolete as of API 1.16 for security reasons. Use CreatePayableDepositDirective instead
        /// </summary>
        /// <param name="payableDepositAccount"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpPost]
        public async Task<HttpResponseMessage> CreatePayableDepositAsync([FromBody] PayableDepositAccount payableDepositAccount)
        {
            await Task.FromResult<PayableDepositAccount>(null);
            SetResourceLocationHeader("CreatePayableDepositDirective");
            throw CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);
        }

        /// <summary>
        /// This route is obsolete as of API 1.16 for security reasons. Use UpdatePayableDepositDirective instead
        /// </summary>
        /// <param name="updatedPayableDepositAccount"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpPost]
        public async Task<PayableDepositAccount> UpdatePayableDepositAsync([FromBody] PayableDepositAccount updatedPayableDepositAccount)
        {
            await Task.FromResult<PayableDepositAccount>(null);
            SetResourceLocationHeader("UpdatePayableDepositDirective", new { id = updatedPayableDepositAccount.PayableDeposits[0].Id });
            throw CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);
        }

        /// <summary>
        /// This route is obsolete as of API 1.16 for security reasons. Use DeletePayableDepositDirective instead.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeletePayableDepositAsync([FromUri] string id)
        {
            await Task.FromResult<PayableDepositAccount>(null);
            SetResourceLocationHeader("DeletePayableDepositDirective", new { id = id });
            throw CreateHttpResponseException("Route has been removed for security reasons", HttpStatusCode.MovedPermanently);
        }

    }
}

