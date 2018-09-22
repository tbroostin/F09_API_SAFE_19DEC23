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
    /// The controller for journal entries
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class JournalEntriesController : BaseCompressedApiController
    {
        private readonly IJournalEntryService journalEntryService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the JournalEntriesController object
        /// </summary>
        /// <param name="journalEntryService">Journal Entry service object</param>
        /// <param name="logger">Logger object</param>
        public JournalEntriesController(IJournalEntryService journalEntryService, ILogger logger)
        {
            this.journalEntryService = journalEntryService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified journal entry
        /// </summary>
        /// <param name="journalEntryId">The requested journal entry ID</param>
        /// <returns>A Journal Entry DTO</returns>
        /// <accessComments>
        /// Requires permission VIEW.JOURNAL.ENTRY, and requires access to at least one of the
        /// general ledger numbers on the journal entry.
        /// </accessComments>
        public async Task<JournalEntry> GetJournalEntryAsync(string journalEntryId)
        {
            if (string.IsNullOrEmpty(journalEntryId))
            {
                string message = "A Journal Number must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var journalEntry = await journalEntryService.GetJournalEntryAsync(journalEntryId);
                return journalEntry;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the journal entry.", HttpStatusCode.Forbidden);
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
                throw CreateHttpResponseException("Unable to get the journal entry.", HttpStatusCode.BadRequest);
            }
        }
    }
}