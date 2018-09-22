// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Threading.Tasks;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// This is the controller for Tax Form Consents.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class TaxFormConsentsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly ITaxFormConsentService taxFormConsentService;

        /// <summary>
        /// This constructor initializes the Tax Form Consent controller.
        /// </summary>
        public TaxFormConsentsController(IAdapterRegistry adapterRegistry, ILogger logger, ITaxFormConsentService taxFormConsentService)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormConsentService = taxFormConsentService;
        }

        /// <summary>
        /// This method gets Tax Form Consent information for a specified person and tax form ID (W-2, 1095-C, etc.).
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxFormId">Tax Form ID</param>
        /// <returns>A set of Tax Form Consents</returns>
        /// <accessComments>
        /// In order to access consent data, the user must meet one of the following conditions:
        /// 1. Have the admin permission (depending on tax form, either ViewEmployeeW2, ViewEmployee1095C, ViewStudent1098, ViewEmployeeT4, ViewRecipientT4A, or ViewStudentT2202A)
        /// 2. Have the normal permission (depending on tax form, either ViewW2, View1095C, View1098, ViewT4, ViewT4A, ViewT2202A or View1099MISC), and be requesting their own data
        /// 3. Be a Person Proxy for the user whose 1098 they are requesting (only applies to 1098)
        /// </accessComments>
        public async Task<IEnumerable<TaxFormConsent>> GetAsync(string personId, TaxForms taxFormId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                    throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

                var taxFormConsents = await taxFormConsentService.GetAsync(personId, taxFormId);
                return taxFormConsents;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to access tax form consents.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the tax form consent", HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// This method stores a new Tax Form Consent entry.
        /// </summary>
        /// <param name="newTaxFormConsent">TaxFormConsent DTO</param>
        /// <returns>TaxFormConsent DTO</returns>
        [HttpPost]
        public async Task<TaxFormConsent> PostAsync([FromBody]TaxFormConsent newTaxFormConsent)
        {
            try
            {
                var taxFormConsent = await taxFormConsentService.PostAsync(newTaxFormConsent);
                return taxFormConsent;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to save tax form consents.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to save the tax form consent.", HttpStatusCode.BadRequest);
            }
        }
    }
}