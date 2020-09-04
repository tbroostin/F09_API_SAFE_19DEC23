// Copyright 2020 Ellucian Company L.P. and its affiliates.

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
    /// This is the controller for vendor commodity.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class VendorCommodityController : BaseCompressedApiController
    {
        private readonly IVendorCommodityService vendorCommodityService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the VendorCommodityController object.
        /// </summary>
        /// <param name="vendorCommodityService">vendor commodity service object</param>
        /// <param name="logger">Logger object</param>
        public VendorCommodityController(IVendorCommodityService vendorCommodityService, ILogger logger)
        {
            this.vendorCommodityService = vendorCommodityService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns a vendor and commodity code association.
        /// </summary>
        /// <param name="vendorId">vendor id.</param>        
        /// <param name="commodityCode">Commodity code.</param>
        /// <returns>VendorCommodity Dto.</returns>
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.VENDOR, CREATE.UPDATE.VOUCHER, CREATE.UPDATE.REQUISITION and CREATE.UPDATE.PURCHASE.ORDER
        /// </accessComments>
        [HttpGet]
        public async Task<VendorCommodity> GetVendorCommodityAsync(string vendorId, string commodityCode)
        {
            if (string.IsNullOrEmpty(vendorId) && string.IsNullOrEmpty(commodityCode))
            {
                string message = "vendorId and commodityCode must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var vendorCommoditiesDto = await vendorCommodityService.GetVendorCommodityAsync(vendorId, commodityCode);
                return vendorCommoditiesDto;
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, "Invalid argument.");
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, "Insufficient permissions to get the vendor commodities.");
                throw CreateHttpResponseException("Insufficient permissions to get the vendor commodities.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, "Record not found.");
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get VendorCommodities.");
                throw CreateHttpResponseException("Unable to get vendor commodities.", HttpStatusCode.BadRequest);
            }
        }

    }
}