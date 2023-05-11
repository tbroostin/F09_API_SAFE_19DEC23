// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using System.ComponentModel;
using Ellucian.Web.License;
using System.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Api.Licensing;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using System.Net;
using Ellucian.Web.Security;
using System;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for receive procurement
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class ReceiveProcurementsController : BaseCompressedApiController
    {
        private readonly IReceiveProcurementsService receiveProcurementsService;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// This constructor initializes the ReceiveProcurementsController object
        /// </summary>
        /// <param name="receiveProcurementsService">ReceiveProcurements service object</param>
        /// <param name="logger">Logger object</param>
        public ReceiveProcurementsController(IReceiveProcurementsService receiveProcurementsService, ILogger logger)
        {
            this.receiveProcurementsService = receiveProcurementsService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves list of procurement receiving items
        /// </summary>
        /// <param name="personId">ID logged in user</param>
        /// <returns>list of Procurement Receving Items DTO</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission UPDATE.RECEIVING
        /// </accessComments>
        [HttpGet]
        public async Task<IEnumerable<ReceiveProcurementSummary>> GetReceiveProcurementsByPersonIdAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                string message = "person Id must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                var receiveProcurement = await receiveProcurementsService.GetReceiveProcurementsByPersonIdAsync(personId);
                return receiveProcurement;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the purchase order for receiving items.", HttpStatusCode.Forbidden);
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
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the purchase order for receiving items.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procurementAcceptOrReturnItemInformationRequest">Procurement accept return request DTO</param>
        /// <returns>Procurement accept return response DTO</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission UPDATE.RECEIVING
        /// </accessComments>
        [HttpPost]
        public async Task<ProcurementAcceptReturnItemInformationResponse> PostAcceptOrReturnProcurementItemsAsync([FromBody] Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationRequest procurementAcceptOrReturnItemInformationRequest)
        {
            if (procurementAcceptOrReturnItemInformationRequest == null)
            {
                string message = "Must provide a procurementAcceptOrReturnItemInformationRequest object";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                return await receiveProcurementsService.AcceptOrReturnProcurementItemsAsync(procurementAcceptOrReturnItemInformationRequest);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to accept/return procurement items.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to accept/return the procurement items.", HttpStatusCode.BadRequest);
            }
        }
    }
}