// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to general ledger objects.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class GeneralLedgerAccountsController : BaseCompressedApiController
    {
        private readonly IGeneralLedgerAccountService generalLedgerAccountService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the GL account controller.
        /// </summary>
        /// <param name="generalLedgerAccountService">General ledger account service object</param>
        /// <param name="logger">Logger object</param>
        public GeneralLedgerAccountsController(IGeneralLedgerAccountService generalLedgerAccountService, ILogger logger)
        {
            this.generalLedgerAccountService = generalLedgerAccountService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves the list of active expense GL account DTOs for which the user has access.
        /// </summary>
        /// <param name="glClass">Optional: null for all the user GL accounts, expense for only the expense type GL accounts.</param>
        /// <returns>A collection of expense GL account DTOs for the user.</returns>
        /// <accessComments>
        /// No permission is needed. The user can only access those GL accounts
        /// for which they have GL account security access granted.
        /// </accessComments>
        [HttpGet]
        public async Task<IEnumerable<GlAccount>> GetUserGeneralLedgerAccountsAsync([FromUri(Name = "glClass")] string glClass)
        {
            {
                try
                {
                    Stopwatch watch = null;
                    if (logger.IsInfoEnabled)
                    {
                        watch = new Stopwatch();
                        watch.Start();
                    }

                    var glUserAccounts = await generalLedgerAccountService.GetUserGeneralLedgerAccountsAsync(glClass);

                    if (logger.IsInfoEnabled)
                    {
                        watch.Stop();
                        logger.Info("GL account LookUp CONTROLLER timing: GetUserGeneralLedgerAccountsAsync completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
                    }

                    return glUserAccounts;
                }
                catch (ConfigurationException cnex)
                {
                    logger.Error(cnex, cnex.Message);
                    throw CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
                }
                catch (ArgumentNullException anex)
                {
                    logger.Error(anex, anex.Message);
                    throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
                }
                // Application exceptions will be caught below.
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    throw CreateHttpResponseException("Unable to get the GL accounts.", HttpStatusCode.BadRequest);
                }
            }
        }

        /// <summary>
        /// Retrieves a single general ledger object using the supplied GL account ID.
        /// </summary>
        /// <param name="generalLedgerAccountId">General ledger account ID.</param>
        /// <returns>General ledger account DTO.</returns>
        /// <accessComments>
        /// The user can only access those GL accounts for which they have
        /// GL account security access granted.
        /// </accessComments>
        public async Task<GeneralLedgerAccount> GetAsync(string generalLedgerAccountId)
        {
            try
            {
                return await generalLedgerAccountService.GetAsync(generalLedgerAccountId);
            }
            catch (ConfigurationException cnex)
            {
                logger.Error(cnex, cnex.Message);
                throw CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Debug(csee, "Session expired - unable to get the GL account.");
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get the GL account.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Validate a GL account. 
        /// If there is a fiscal year, it will also validate it for that year.
        /// </summary>
        /// <param name="generalLedgerAccountId">GL account ID.</param>
        /// <param name="fiscalYear">Optional; General Ledger fiscal year.</param>
        /// <returns>A <see cref="GlAccountValidationResponse">DTO.</see>/></returns>
        /// <accessComments>
        /// The user can only access those GL accounts for which they have
        /// GL account security access granted.
        /// </accessComments>     
        public async Task<GlAccountValidationResponse> GetGlAccountValidationAsync(string generalLedgerAccountId,
            [FromUri(Name = "fiscalYear")] string fiscalYear)
        {
            try
            {
                return await generalLedgerAccountService.ValidateGlAccountAsync(generalLedgerAccountId, fiscalYear);
            }
            catch (ConfigurationException cnex)
            {
                logger.Error(cnex, cnex.Message);
                throw CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "==> generalLedgerAccountService.ValidateGlAccountAsync session expired <==");
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to validate the GL account.", HttpStatusCode.BadRequest);
            }
        }
    }
}