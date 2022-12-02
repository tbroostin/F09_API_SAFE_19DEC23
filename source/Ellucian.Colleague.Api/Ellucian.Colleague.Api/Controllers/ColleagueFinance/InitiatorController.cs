// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.

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
    /// The controller for initiator
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class InitiatorController : BaseCompressedApiController
    {
        private readonly IInitiatorService initiatorService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the InitiatorController object
        /// </summary>
        /// <param name="initiatorService">Initiator service object</param>
        /// <param name="logger">Logger object</param>
        public InitiatorController(IInitiatorService initiatorService, ILogger logger)
        {
            this.initiatorService = initiatorService;
            this.logger = logger;
        }

        /// <summary>
        /// Get the list of initiators based on keyword search.
        /// </summary>
        /// <param name="queryKeyword">parameter for passing search keyword</param>
        /// <returns>The initiator search results</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.ANY.PERSON or CREATE.UPDATE.REQUISITION or CREATE.UPDATE.PURCHASE.ORDER.
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.34. Use QueryInitiatorByKeywordAsync.")]
        [HttpGet]
        public async Task<IEnumerable<Initiator>> GetInitiatorByKeywordAsync(string queryKeyword)
        {

            if (string.IsNullOrEmpty(queryKeyword))
            {
                string message = "query keyword is required to query.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var initiatorSearchResults = await initiatorService.QueryInitiatorByKeywordAsync(queryKeyword);
                return initiatorSearchResults;
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, "Invalid argument.");
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, "Insufficient permissions to get the initiator info.");
                throw CreateHttpResponseException("Insufficient permissions to get the initiator info.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, "Record not found.");
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Debug(csee, "Session expired - unable to search initiator.");
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to search initiator");
                throw CreateHttpResponseException("Unable to search initiator", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the list of initiators based on keyword search.
        /// </summary>
        /// <param name="criteria">KeywordSearchCriteria parameter for passing search keyword</param>
        /// <returns> The initiator search results</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.ANY.PERSON or CREATE.UPDATE.REQUISITION or CREATE.UPDATE.PURCHASE.ORDER.
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<Initiator>> QueryInitiatorByKeywordAsync([FromBody] KeywordSearchCriteria criteria)
        {
            if (criteria == null || string.IsNullOrEmpty(criteria.Keyword))
            {
                string message = "query keyword is required to query.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var initiatorSearchResults = await initiatorService.QueryInitiatorByKeywordAsync(criteria.Keyword);
                return initiatorSearchResults;
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, "Invalid argument.");
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, "Insufficient permissions to get the initiator info.");
                throw CreateHttpResponseException("Insufficient permissions to get the initiator info.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, "Record not found.");
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Debug(csee, "Session expired - unable to search initiator.");
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to search initiator");
                throw CreateHttpResponseException("Unable to search initiator", HttpStatusCode.BadRequest);
            }
        }
    }
}