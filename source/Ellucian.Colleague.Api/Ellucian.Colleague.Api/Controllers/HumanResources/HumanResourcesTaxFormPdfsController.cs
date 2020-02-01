// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// This is the controller for tax form pdf.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class HumanResourcesTaxFormPdfsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IHumanResourcesTaxFormPdfService taxFormPdfService;
        private readonly ITaxFormConsentService taxFormConsentService;

        /// <summary>
        /// Initialize the Tax Form pdf controller.
        /// </summary>
        public HumanResourcesTaxFormPdfsController(IAdapterRegistry adapterRegistry, ILogger logger, IHumanResourcesTaxFormPdfService taxFormPdfService, ITaxFormConsentService taxFormConsentService)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormPdfService = taxFormPdfService;
            this.taxFormConsentService = taxFormConsentService;
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the W-2 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the W-2.</param>
        /// <param name="recordId">The record ID where the W-2 pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.W2 for the employee.
        /// Requires permission VIEW.W2 for someone who currently has permission to proxy for the employee requested.
        /// Requires permission VIEW.EMPLOYEE.W2 for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>          
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        public async Task<HttpResponseMessage> GetW2TaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.FormW2);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // Check if the person has consented to receiving their W2 online - if not, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms.FormW2);
            if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.GetW2TaxFormDataAsync(personId, recordId);
                var guamFlag = await taxFormPdfService.GetW2GuamFlag();
                
                // Determine which PDF template to use.
                switch (pdfData.TaxYear)
                {
                    case "2019":
                        if (guamFlag)
                        {
                            pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2019-W2-Guam.rdlc");
                        }
                        else
                        {
                            pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2019-W2-W2ST.rdlc");
                        }
                        break;
                    case "2018":
                        if (guamFlag)
                        {
                            pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2018-W2-Guam.rdlc");
                        }
                        else
                        {
                            pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2018-W2-W2ST.rdlc");
                        }
                        break;
                    case "2017":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2017-W2-W2ST.rdlc");
                        break;
                    case "2016":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2016-W2-W2ST.rdlc");
                        break;
                    case "2015":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2015-W2-W2ST.rdlc");
                        break;
                    case "2014":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2014-W2-W2ST.rdlc");
                        break;
                    case "2013":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2013-W2-W2ST.rdlc");
                        break;
                    case "2012":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2012-W2-W2ST.rdlc");
                        break;
                    case "2011":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2011-W2-W2ST.rdlc");
                        break;
                    case "2010":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2010-W2.rdlc");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.Error(message);
                        throw new ApplicationException(message);
                }
                
                var pdfBytes = taxFormPdfService.PopulateW2PdfReport(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxFormW2" + "_" + recordId;
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
                throw CreateHttpResponseException("Error retrieving W-2 PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the W-2c tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the W-2c.</param>
        /// <param name="recordId">The record ID where the W-2c pdf data is stored</param>
        /// <returns>HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> GetW2cTaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.FormW2C);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // Check if the person has consented to receiving their W2c online - if not, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms.FormW2C);
            if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.GetW2cTaxFormData(personId, recordId);

                // Determine which PDF template to use.
                pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2014-W2c-8.rdlc");

                var pdfBytes = taxFormPdfService.PopulateW2cPdfReport(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxFormW2c" + "_" + recordId;
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
                throw CreateHttpResponseException("Error retrieving W-2c PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the 1095-C tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1095-C.</param>
        /// <param name="recordId">ID of the record where the 1095-C pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.1095C for the employee.
        /// Requires permission VIEW.1095C for someone who currently has permission to proxy for the employee requested.
        /// Requires permission VIEW.EMPLOYEE.1095C for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>         
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        public async Task<HttpResponseMessage> Get1095cTaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.Form1095C);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // Check if the person has consented to receiving their 1095C online - if not, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms.Form1095C);
            if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.Get1095cTaxFormDataAsync(personId, recordId);

                switch (pdfData.TaxYear)
                {
                    case "2019":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2019-1095C.rdlc");
                        break;
                    case "2018":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2018-1095C.rdlc");
                        break;
                    case "2017":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2017-1095C.rdlc");
                        break;
                    case "2016":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2016-1095C.rdlc");
                        break;
                    case "2015":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2015-1095C.rdlc");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.Error(message);
                        throw new ApplicationException(message);
                }

                var pdfBytes = taxFormPdfService.Populate1095tReport(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxForm1095c" + "_" + recordId;
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
                throw CreateHttpResponseException("Error retrieving 1095-C PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the T4 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4.</param>
        /// <param name="recordId">ID of the record where the T4 pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.T4.
        /// Requires permission VIEW.T4 for someone who currently has permission to proxy for the employee requested.
        /// Requires permission VIEW.EMPLOYEE.T4 for admin view.       
        /// The tax form record requested must belong to the person ID requested.       
        /// </accessComments>         
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        public async Task<HttpResponseMessage> GetT4TaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.FormT4);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // ************* T4s and T2202As are special cases based on CRA regulations! *************
            // Check if the person has explicitly withheld consent to receiving their T4 online - if they opted out, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms.FormT4);
            if ((mostRecentConsent != null && !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.GetT4TaxFormDataAsync(personId, recordId);

                switch (pdfData.TaxYear)
                {
                    case "2019":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2019-T4.rdlc");
                        break;
                    case "2018":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2018-T4.rdlc");
                        break;
                    case "2017":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2017-T4.rdlc");
                        break;
                    case "2016":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2016-T4.rdlc");
                        break;
                    case "2015":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2015-T4.rdlc");
                        break;
                    case "2014":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2014-T4.rdlc");
                        break;
                    case "2013":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2013-T4.rdlc");
                        break;
                    case "2012":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2012-T4.rdlc");
                        break;
                    case "2011":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2011-T4.rdlc");
                        break;
                    case "2010":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/2010-T4.rdlc");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.Error(message);
                        throw new ApplicationException(message);
                }

                var pdfBytes = taxFormPdfService.PopulateT4PdfReport(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxFormT4" + "_" + recordId;
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
                throw CreateHttpResponseException("Error retrieving T4 PDF data.", HttpStatusCode.BadRequest);
            }
        }
    }
}