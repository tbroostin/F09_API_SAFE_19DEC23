// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Net.Http;
using System.Web;
using System.Net.Http.Headers;
using System.Net;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Linq;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to FormT4aPdfData objects.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class ColleagueFinanceTaxFormPdfsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IColleagueFinanceTaxFormPdfService taxFormPdfService;
        private readonly ITaxFormConsentService taxFormConsentService;

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger object</param>
        /// <param name="taxFormPdfService">FormT4aPdfData service object</param>
        /// <param name="taxFormConsentService">Form T4A consent service object</param>
        public ColleagueFinanceTaxFormPdfsController(IAdapterRegistry adapterRegistry, ILogger logger,
            IColleagueFinanceTaxFormPdfService taxFormPdfService, ITaxFormConsentService taxFormConsentService)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormPdfService = taxFormPdfService;
            this.taxFormConsentService = taxFormConsentService;
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the T4A tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4A.</param>
        /// <param name="recordId">The record ID where the T4A pdf data is stored</param>         
        /// <returns>HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> GetFormT4aPdfAsync(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.FormT4A);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // Check if the person has consented to receiving their T4A online - if not, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms.FormT4A);
            if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.GetFormT4aPdfDataAsync(personId, recordId);

                // Determine which PDF template to use.
                switch (pdfData.TaxYear)
                {
                    case "2017":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2017-T4A.rdlc");
                        break;
                    case "2016":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2016-T4A.rdlc");
                        break;
                    case "2015":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2015-T4A.rdlc");
                        break;
                    case "2014":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2014-T4A.rdlc");
                        break;
                    case "2013":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2013-T4A.rdlc");
                        break;
                    case "2012":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2012-T4A.rdlc");
                        break;
                    case "2011":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2011-T4A.rdlc");
                        break;
                    case "2010":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2010-T4A.rdlc");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.Error(message);
                        throw new ApplicationException(message);
                }

                var pdfBytes = taxFormPdfService.PopulateT4aPdf(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxFormT4A" + "_" + recordId;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileNameString + ".pdf"
                };
                response.Content.Headers.ContentLength = pdfBytes.Length;
                return response;
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw CreateHttpResponseException("Error retrieving T4A PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the 1099-MISC tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-MISC.</param>
        /// <param name="recordId">The record ID where the 1099-MISC pdf data is stored</param>         
        /// <returns>HttpResponseMessage</returns>
        /// <accessComments>
        /// In order to access 1099-Misc tax form PDF, the user shoud have the View.1099MISC permission and be requesting their own data
        /// </accessComments>
        public async Task<HttpResponseMessage> Get1099MiscTaxFormPdfAsync(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.Form1099MI);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            if (mostRecentConsent == null || !mostRecentConsent.HasConsented)
            {
                throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.Get1099MiscPdfDataAsync(personId, recordId);
                // Determine which PDF template to use.
                switch (pdfData.TaxYear)
                {
                    case "2017":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/20XX-1099MI.rdlc");
                        break;               
                 
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.Error(message);
                        throw new ApplicationException(message);
                }

                var pdfBytes = taxFormPdfService.Populate1099MiscPdf(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxForm1099MI" + "_" + recordId;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileNameString + ".pdf"
                };
                response.Content.Headers.ContentLength = pdfBytes.Length;
                return response;
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw CreateHttpResponseException("Error retrieving 1099-MISC PDF data.", HttpStatusCode.BadRequest);
            }
        }
    }
}
