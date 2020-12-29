// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Linq;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// This is the controller for tax form pdf.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTaxFormPdfsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IStudentTaxFormPdfService taxFormPdfService;
        private readonly ITaxFormConsentService taxFormConsentService;
        private readonly IConfigurationService configurationService;

        /// <summary>
        /// Initialize the Tax Form pdf controller.
        /// </summary>
        public StudentTaxFormPdfsController(IAdapterRegistry adapterRegistry, ILogger logger, IStudentTaxFormPdfService taxFormPdfService, ITaxFormConsentService taxFormConsentService, IConfigurationService configurationService)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormPdfService = taxFormPdfService;
            this.taxFormConsentService = taxFormConsentService;
            this.configurationService = configurationService;
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the 1098 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1098.</param>
        /// <param name="recordId">The record ID where the 1098 pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.1098 for the student.
        /// Requires permission VIEW.1098 for someone who currently has permission to proxy for the student requested.
        /// Requires permission VIEW.STUDENT.1098 for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        public async Task<HttpResponseMessage> Get1098TaxFormPdf2Async(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.Get2Async(personId, TaxFormTypes.Form1098);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // Check if the person has consented to receiving their 1098 online OR if the user is an admin user (which ignores the consent check)
            // If the consent check fails and the client requires consent to view 1098 forms online (T9TD and T9ED) throw an "Unauthorized" HTTP exception.
            // Note that the required permission checks are handled during data retrieval (in the "Get1098TaxFormData" method).
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent2Async(TaxFormTypes.Form1098);
            if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                var configuration = await configurationService.GetTaxFormConsentConfiguration2Async(TaxFormTypes.Form1098);

                if (!configuration.IsBypassingConsentPermitted)
                {
                    throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
                }
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.Get1098TaxFormData(personId, recordId);
                if (pdfData != null && !String.IsNullOrEmpty(pdfData.TaxFormName))
                {
                    pdfTemplatePath = GetPdfTemplatePath(pdfData);
                }

                var pdfBytes = taxFormPdfService.Populate1098Report(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxForm1098" + "_" + recordId;
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
            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Error retrieving 1098 PDF data.", HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw CreateHttpResponseException("Error retrieving 1098 PDF data.", HttpStatusCode.BadRequest);
            }
        }

        private string GetPdfTemplatePath(Domain.Student.Entities.Form1098PdfData pdfData)
        {
            string pdfTemplatePath;
            // Determine which PDF template to use.
            switch (pdfData.TaxYear)
            {
                case "2020":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2020-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2019":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2019-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2018":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2018-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2017":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2017-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2016":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2016-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2015":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2015-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2014":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2014-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2013":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2013-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2012":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2012-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2011":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2011-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2010":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2010-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2009":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2009-" + pdfData.TaxFormName + ".rdlc");
                    break;
                case "2008":
                    pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2008-" + pdfData.TaxFormName + ".rdlc");
                    break;
                default:
                    var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                    logger.Error(message);
                    throw new ApplicationException(message);
            }

            return pdfTemplatePath;
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the T2202a tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T2202a.</param>
        /// <param name="recordId">The record ID where the T2202a pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.T2202A for the student.
        /// Requires permission VIEW.T2202A for someone who currently has permission to proxy for the student requested.
        /// Requires permission VIEW.STUDENT.T2202A for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        public async Task<HttpResponseMessage> GetT2202aTaxFormPdf2Async(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.Get2Async(personId, TaxFormTypes.FormT2202A);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // ************* T4s and T2202As are special cases based on CRA regulations! *************
            // Check if the person has explicitly withheld consent to receiving their T2202a online - if they opted out, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent2Async(TaxFormTypes.FormT2202A);
            if ((mostRecentConsent != null && !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.GetT2202aTaxFormData(personId, recordId);

                // Determine which PDF template to use.
                int taxYear;

                if (Int32.TryParse(pdfData.TaxYear, out taxYear))
                {
                    if (taxYear >= 2008 && taxYear <= 2017)
                    {
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/20XX-T2202a.rdlc");
                    }
                    else if (taxYear == 2018)
                    {
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2018-T2202a.rdlc");
                    }
                    else if (taxYear == 2019)
                    {
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2019-T2202.rdlc");
                    }
                    else if (taxYear == 2020)
                    {
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2020-T2202.rdlc");
                    }
                }
                else
                {
                    var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                    logger.Error(message);
                    throw new ApplicationException(message);
                }

                var pdfBytes = new byte[0];
                pdfBytes = taxFormPdfService.PopulateT2202aReport(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxFormT2202a" + "_" + recordId;
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
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error retrieving T2202A PDF data.", HttpStatusCode.BadRequest);
            }
        }


        #region OBSOLETE METHODS

        /// <summary>
        /// Returns the data to be printed on the pdf for the 1098 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1098.</param>
        /// <param name="recordId">The record ID where the 1098 pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.1098 for the student.
        /// Requires permission VIEW.1098 for someone who currently has permission to proxy for the student requested.
        /// Requires permission VIEW.STUDENT.1098 for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get1098TaxFormPdf2Async instead.")]
        public async Task<HttpResponseMessage> Get1098TaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.Form1098);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // Check if the person has consented to receiving their 1098 online - if not, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms.Form1098);
            if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.Get1098TaxFormData(personId, recordId);
                if (pdfData != null && !String.IsNullOrEmpty(pdfData.TaxFormName))
                {
                    pdfTemplatePath = GetPdfTemplatePath(pdfData);
                }

                var pdfBytes = taxFormPdfService.Populate1098Report(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxForm1098" + "_" + recordId;
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
            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Error retrieving 1098 PDF data.", HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw CreateHttpResponseException("Error retrieving 1098 PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the T2202a tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T2202a.</param>
        /// <param name="recordId">The record ID where the T2202a pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.T2202A for the student.
        /// Requires permission VIEW.T2202A for someone who currently has permission to proxy for the student requested.
        /// Requires permission VIEW.STUDENT.T2202A for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use GetT2202aTaxFormPdf2Async instead.")]
        public async Task<HttpResponseMessage> GetT2202aTaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.FormT2202A);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // ************* T4s and T2202As are special cases based on CRA regulations! *************
            // Check if the person has explicitly withheld consent to receiving their T2202a online - if they opted out, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms.FormT2202A);
            if ((mostRecentConsent != null && !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.GetT2202aTaxFormData(personId, recordId);

                // Determine which PDF template to use.
                int taxYear;

                if (Int32.TryParse(pdfData.TaxYear, out taxYear))
                {
                    if (taxYear >= 2008 && taxYear <= 2017)
                    {
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/20XX-T2202a.rdlc");
                    }
                    else if (taxYear == 2018)
                    {
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2018-T2202a.rdlc");
                    }
                    else if (taxYear == 2019)
                    {
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/Student/2019-T2202.rdlc");
                    }
                }
                else
                {
                    var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                    logger.Error(message);
                    throw new ApplicationException(message);
                }

                var pdfBytes = new byte[0];
                pdfBytes = taxFormPdfService.PopulateT2202aReport(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxFormT2202a" + "_" + recordId;
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
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error retrieving T2202A PDF data.", HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
