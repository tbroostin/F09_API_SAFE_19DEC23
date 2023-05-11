// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for the type of Tax Form Statements.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class ColleagueFinanceTaxFormStatementsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IColleagueFinanceTaxFormStatementService taxFormStatementService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initialize the Tax Form Statement controller.
        /// </summary>
        public ColleagueFinanceTaxFormStatementsController(IAdapterRegistry adapterRegistry, ILogger logger, IColleagueFinanceTaxFormStatementService taxFormStatementService)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormStatementService = taxFormStatementService;
        }

        /// <summary>
        /// Returns T4A tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of T4A tax form statements</returns>
        /// <accessComments>
        /// In order to access T4A statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewRecipientT4A
        /// 2. Have the ViewT4A permission, and be requesting their own data
        /// </accessComments>
        public async Task<IEnumerable<TaxFormStatement3>> GetT4a2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return await taxFormStatementService.Get2Async(personId, Domain.Base.TaxFormTypes.FormT4A);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to access T4A statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.Error(arex, arex.Message);
                throw CreateHttpResponseException("Invalid tax form.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentException agex)
            {
                logger.Error(agex, agex.Message);
                throw CreateHttpResponseException("Invalid tax form.", HttpStatusCode.BadRequest);
            }
            // Application and Null Reference exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get T4A statements", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns 1099MI tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of 1099MI tax form statements</returns>
        /// <accessComments>
        /// In order to access 1099MI statement data, the user shoud have the View.1099MISC permission and be requesting their own data
        /// </accessComments>
        public async Task<IEnumerable<TaxFormStatement3>> Get1099Mi2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return await taxFormStatementService.Get2Async(personId, Domain.Base.TaxFormTypes.Form1099MI);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to access 1099-MISC statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.Error(arex, arex.Message);
                throw CreateHttpResponseException("Invalid tax form.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application and Null Reference exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get 1099-MISC statements", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns 1099NEC tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of 1099NEC tax form statements</returns>
        /// <accessComments>
        /// In order to access 1099NEC statement data, the user shoud have the View.1099NEC permission and be requesting their own data
        /// </accessComments>
        public async Task<IEnumerable<TaxFormStatement3>> Get1099NecAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return await taxFormStatementService.Get2Async(personId, Domain.Base.TaxFormTypes.Form1099NEC);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to access 1099-NEC statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.Error(arex, arex.Message);
                throw CreateHttpResponseException("Invalid tax form.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application and Null Reference exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get 1099-NEC statements", HttpStatusCode.BadRequest);
            }
        }


        #region OBSOLETE METHODS

        /// <summary>
        /// Returns T4A tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of T4A tax form statements</returns>
        /// <accessComments>
        /// In order to access T4A statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewRecipientT4A
        /// 2. Have the ViewT4A permission, and be requesting their own data
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.29.1. Use GetT4a2Async instead.")]
        public async Task<IEnumerable<TaxFormStatement2>> GetT4aAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return await taxFormStatementService.GetAsync(personId, TaxForms.FormT4A);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to access T4A statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.Error(arex, arex.Message);
                throw CreateHttpResponseException("Invalid tax form.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentException agex)
            {
                logger.Error(agex, agex.Message);
                throw CreateHttpResponseException("Invalid tax form.", HttpStatusCode.BadRequest);
            }
            // Application and Null Reference exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get T4A statements", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns 1099MI tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of 1099MI tax form statements</returns>
        /// <accessComments>
        /// In order to access 1099MI statement data, the user shoud have the View.1099MISC permission and be requesting their own data
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.29.1. Use Get1099Mi2Async instead.")]
        public async Task<IEnumerable<TaxFormStatement2>> Get1099MIAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return await taxFormStatementService.GetAsync(personId, TaxForms.Form1099MI);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to access 1099-MISC statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.Error(arex, arex.Message);
                throw CreateHttpResponseException("Invalid tax form.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application and Null Reference exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get 1099-MISC statements", HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}