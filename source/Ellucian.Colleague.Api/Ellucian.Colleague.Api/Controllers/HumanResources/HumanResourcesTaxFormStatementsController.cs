// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// This is the controller for the type of Tax Form Statements.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class HumanResourcesTaxFormStatementsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IHumanResourcesTaxFormStatementService taxFormStatementService;

        /// <summary>
        /// Initialize the Tax Form Statement controller.
        /// </summary>
        public HumanResourcesTaxFormStatementsController(IAdapterRegistry adapterRegistry, ILogger logger, IHumanResourcesTaxFormStatementService taxFormStatementService)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormStatementService = taxFormStatementService;
        }

        /// <summary>
        /// Returns W-2 tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of W-2 tax form statements</returns>
        /// <accessComments>
        /// In order to access W-2 statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewEmployeeW2
        /// 2. Have the ViewW2 permission, and be requesting their own data
        /// </accessComments>
        public async Task<IEnumerable<TaxFormStatement2>> GetW2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return await taxFormStatementService.GetAsync(personId, TaxForms.FormW2);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to access tax form statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.Error(arex, arex.Message);
                throw CreateHttpResponseException("Invalid data.", HttpStatusCode.BadRequest);
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
                throw CreateHttpResponseException("Unable to get the W-2 statements", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns 1095-C tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of 1095-C tax form statements</returns>
        /// <accessComments>
        /// In order to access 1095-C statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewEmployee1095C
        /// 2. Have the View1095C permission, and be requesting their own data
        /// </accessComments>
        public async Task<IEnumerable<TaxFormStatement2>> Get1095cAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return await taxFormStatementService.GetAsync(personId, TaxForms.Form1095C);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to access tax form statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.Error(arex, arex.Message);
                throw CreateHttpResponseException("Invalid data.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the 1095-C statements", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns T4 tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of T4 tax form statements</returns>
        /// <accessComments>
        /// In order to access T4 statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewEmployeeT4
        /// 2. Have the ViewT4 permission, and be requesting their own data
        /// </accessComments>
        public async Task<IEnumerable<TaxFormStatement2>> GetT4Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            
            try
            {
                return await taxFormStatementService.GetAsync(personId, TaxForms.FormT4);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to access tax form statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.Error(arex, arex.Message);
                throw CreateHttpResponseException("Invalid data.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the T4 statements", HttpStatusCode.BadRequest);
            }
        }
    }
}