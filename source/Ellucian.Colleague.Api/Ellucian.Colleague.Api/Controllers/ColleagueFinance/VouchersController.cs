// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for vouchers.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class VouchersController : BaseCompressedApiController
    {
        private readonly IVoucherService voucherService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the VouchersController object.
        /// </summary>
        /// <param name="voucherService">Voucher service object</param>
        /// <param name="logger">Logger object</param>
        public VouchersController(IVoucherService voucherService, ILogger logger)
        {
            this.voucherService = voucherService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified voucher.
        /// </summary>
        /// <param name="voucherId">ID of the requested voucher.</param>
        /// <returns>Voucher DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.VOUCHER, and requires access to at least one of the
        /// general ledger numbers on the voucher line items.
        /// </accessComments>
        [Obsolete("Obsolete as of API verson 1.15; use version 2 of this endpoint")]
        public async Task<Voucher> GetVoucherAsync(string voucherId)
        {
            if (string.IsNullOrEmpty(voucherId))
            {
                string message = "A Voucher ID must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var voucher = await voucherService.GetVoucherAsync(voucherId);
                return voucher;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the voucher.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, knfex.Message);
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ApplicationException aex)
            {
                logger.Error(aex, aex.Message);
                throw CreateHttpResponseException("Invalid data in record.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw CreateHttpResponseException("Unable to get the voucher.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a specified voucher.
        /// </summary>
        /// <param name="voucherId">ID of the requested voucher.</param>
        /// <returns>Voucher DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.VOUCHER, and requires access to at least one of the
        /// general ledger numbers on the voucher line items.
        /// </accessComments>
        public async Task<Voucher2> GetVoucher2Async(string voucherId)
        {
            if (string.IsNullOrEmpty(voucherId))
            {
                string message = "A Voucher ID must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var voucher = await voucherService.GetVoucher2Async(voucherId);
                return voucher;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the voucher.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, knfex.Message);
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the voucher.", HttpStatusCode.BadRequest);
            }
        }
    }
}