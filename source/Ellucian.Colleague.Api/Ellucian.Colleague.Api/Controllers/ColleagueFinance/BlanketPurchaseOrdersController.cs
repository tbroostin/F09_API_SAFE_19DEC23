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
    /// This is the controller for blanket purchase orders.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class BlanketPurchaseOrdersController : BaseCompressedApiController
    {
        private readonly IBlanketPurchaseOrderService blanketPurchaseOrderService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the BlanketPurchaseOrdersController object.
        /// </summary>
        /// <param name="blanketPurchaseOrderService">Blanket purchase order service object</param>
        /// <param name="logger">Logger object</param>
        public BlanketPurchaseOrdersController(IBlanketPurchaseOrderService blanketPurchaseOrderService, ILogger logger)
        {
            this.blanketPurchaseOrderService = blanketPurchaseOrderService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified blanket purchase order.
        /// </summary>
        /// <param name="blanketPurchaseOrderId">ID of the requested blanket purchase order.</param>
        /// <returns>Blanket purchase order DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.BLANKET.PURCHASE.ORDER, and requires access to at least one of the
        /// general ledger numbers on the blanket purchase order.
        /// </accessComments>
        public async Task<BlanketPurchaseOrder> GetBlanketPurchaseOrderAsync(string blanketPurchaseOrderId)
        {
            if (string.IsNullOrEmpty(blanketPurchaseOrderId))
            {
                string message = "A Blanket Purchase Order ID must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var bpo = await blanketPurchaseOrderService.GetBlanketPurchaseOrderAsync(blanketPurchaseOrderId);
                return bpo;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the blanket purchase order.", HttpStatusCode.Forbidden);
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
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get the blanket purchase order.", HttpStatusCode.BadRequest);
            }
        }
    }
}