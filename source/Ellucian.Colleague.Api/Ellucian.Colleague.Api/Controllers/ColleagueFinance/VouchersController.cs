// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

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

        /// <summary>
        /// Retrieves list of voucher summary
        /// </summary>
        /// <param name="personId">ID logged in user</param>
        /// <returns>list of Voucher Summary DTO</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission VIEW.VOUCHER.
        /// </accessComments>
        [HttpGet]
        public async Task<IEnumerable<VoucherSummary>> GetVoucherSummariesAsync([FromUri] string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                string message = "person Id must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var Voucher = await voucherService.GetVoucherSummariesAsync(personId);
                return Voucher;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the Voucher summary.", HttpStatusCode.Forbidden);
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
                throw CreateHttpResponseException("Unable to get the Voucher summary.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create / Update a voucher.
        /// </summary>
        /// <param name="voucherCreateUpdateRequest">The voucher create update request DTO.</param>        
        /// <returns>The voucher create response DTO.</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission CREATE.UPDATE.VOUCHER.
        /// </accessComments>
        [HttpPost]
        public async Task<Dtos.ColleagueFinance.VoucherCreateUpdateResponse> PostVoucherAsync([FromBody] Dtos.ColleagueFinance.VoucherCreateUpdateRequest voucherCreateUpdateRequest)
        {
            if (voucherCreateUpdateRequest == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid voucher.", HttpStatusCode.BadRequest);
            }
            try
            {
                return await voucherService.CreateUpdateVoucherAsync(voucherCreateUpdateRequest);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to create/update the voucher.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to create/update the voucher.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets a payment address of person for voucher
        /// </summary>
        /// <returns> Payment address DTO</returns>
        /// <accessComments>
        /// Requires  permission CREATE.UPDATE.VOUCHER.
        /// </accessComments>
        [HttpGet]
        public async Task<VendorsVoucherSearchResult> GetReimbursePersonAddressForVoucherAsync()
        {
            try
            {
                return await voucherService.GetReimbursePersonAddressForVoucherAsync();
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the person address.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get the person address.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Void a Voucher.
        /// </summary>
        /// <param name="voucherVoidRequest">The voucher void request DTO.</param>        
        /// <returns>The voucher void response DTO.</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission CREATE.UPDATE.VOUCHER.
        /// </accessComments>
        [HttpPost]
        public async Task<Dtos.ColleagueFinance.VoucherVoidResponse> VoidVoucherAsync([FromBody] Dtos.ColleagueFinance.VoucherVoidRequest voucherVoidRequest)
        {
            if (voucherVoidRequest == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid voucher detail.", HttpStatusCode.BadRequest);
            }
            try
            {
                return await voucherService.VoidVoucherAsync(voucherVoidRequest);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to void the voucher.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to void the voucher.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves list of vouchers
        /// </summary>
        /// <param name="vendorId">Vendor id</param>
        /// <param name="invoiceNo">Invoice number</param>
        /// <returns>List of <see cref="Voucher2">Vouchers</see></returns>
        /// <accessComments>
        /// Requires permission VIEW.VOUCHER.
        /// </accessComments>
        public async Task<IEnumerable<Voucher2>> GetVouchersByVendorAndInvoiceNoAsync(string vendorId, string invoiceNo)
        {
            if (string.IsNullOrEmpty(vendorId))
            {
                string message = "vendor Id must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(invoiceNo))
            {
                string message = "invoice number must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var voucherIds = await voucherService.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNo);
                return voucherIds;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument to query the voucher.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw CreateHttpResponseException();
            }
        }
    }
}