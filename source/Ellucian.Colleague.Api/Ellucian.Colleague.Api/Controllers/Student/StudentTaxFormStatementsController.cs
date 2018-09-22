// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// This is the controller for the type of Student Tax Form Statements.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTaxFormStatementsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IStudentTaxFormStatementService taxFormStatementService;

        /// <summary>
        /// Initialize the Student Tax Form Statement controller.
        /// </summary>
        public StudentTaxFormStatementsController(IAdapterRegistry adapterRegistry, ILogger logger, IStudentTaxFormStatementService taxFormStatementService)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormStatementService = taxFormStatementService;
        }

        /// <summary>
        /// Returns a set of 1098 tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of 1098 tax form statements</returns>
        /// <accessComments>
        /// In order to access 1098 statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewStudent1098
        /// 2. Have the View1098 permission, and be requesting their own data
        /// 3. Be acting as a Person Proxy for the person whose data they are requesting
        /// </accessComments>
        public async Task<IEnumerable<TaxFormStatement2>> Get1098Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);
            
            try
            {
                return await taxFormStatementService.GetAsync(personId, TaxForms.Form1098);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to view 1098 data.", HttpStatusCode.Forbidden);
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
                throw CreateHttpResponseException("Unable to get the 1098 statements.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns a set of T2202A tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of T2202A tax form statements</returns>
        /// <accessComments>
        /// In order to access T2202A statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewStudentT2202A
        /// 2. Have the ViewT2202A permission, and be requesting their own data
        /// </accessComments>
        public async Task<IEnumerable<TaxFormStatement2>> GetT2202aAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);
            
            try
            {
                return await taxFormStatementService.GetAsync(personId, TaxForms.FormT2202A);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to view T2202 data.", HttpStatusCode.Forbidden);
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
                throw CreateHttpResponseException("Unable to get the T2202 statements.", HttpStatusCode.BadRequest);
            }
        }
    }
}