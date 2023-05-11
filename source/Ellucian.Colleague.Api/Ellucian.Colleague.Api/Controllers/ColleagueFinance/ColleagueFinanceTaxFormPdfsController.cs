// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.Base;
using Ellucian.Data.Colleague.Exceptions;

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
        private readonly IConfigurationService configurationService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger object</param>
        /// <param name="taxFormPdfService">FormT4aPdfData service object</param>
        /// <param name="taxFormConsentService">Form T4A consent service object</param>
        /// <param name="configurationService">Configuation service object</param>
        public ColleagueFinanceTaxFormPdfsController(IAdapterRegistry adapterRegistry, ILogger logger,
            IColleagueFinanceTaxFormPdfService taxFormPdfService, ITaxFormConsentService taxFormConsentService, IConfigurationService configurationService)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormPdfService = taxFormPdfService;
            this.taxFormConsentService = taxFormConsentService;
            this.configurationService = configurationService;
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the T4A tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4A.</param>
        /// <param name="recordId">The record ID where the T4A pdf data is stored</param>  
        /// <accessComments>
        /// Requires permission VIEW.T4A for the recipient.
        /// Requires permission VIEW.T4A for someone who currently has permission to proxy for the recipient requested.
        /// Requires permission VIEW.RECIPIENT.T4A for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>        
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        public async Task<HttpResponseMessage> GetFormT4aPdf2Async(string personId, string recordId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                    throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

                if (string.IsNullOrEmpty(recordId))
                    throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

                var config = await configurationService.GetTaxFormConsentConfiguration2Async(TaxFormTypes.FormT4A);

                // if consents are hidden, don't bother evaluating them
                if (config == null || !config.HideConsent)
                {
                    logger.Debug("Using consents for T4A tax forms");

                    var consents = await taxFormConsentService.Get2Async(personId, TaxFormTypes.FormT4A);
                    consents = consents.OrderByDescending(c => c.TimeStamp);
                    var mostRecentConsent = consents.FirstOrDefault();

                    // Check if the person has consented to receiving their T4A online - if not, throw exception
                    var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent2Async(TaxFormTypes.FormT4A);

                    if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
                    {
                        logger.Debug("Consent is required to view T4A information.");
                        throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
                    }
                }
                
                string pdfTemplatePath = string.Empty;
                var pdfData = await taxFormPdfService.GetFormT4aPdfDataAsync(personId, recordId);
                if (pdfData != null && pdfData.TaxYear != null)
                {
                    logger.Debug("Retrieving T4A PDF for tax year '" + pdfData.TaxYear + "'");
                }
                else
                {
                    logger.Debug("No PDF data and/or tax year found.");
                }

                // Determine which PDF template to use.
                switch (pdfData.TaxYear)
                {
                    case "2022":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2022-T4A.rdlc");
                        break;
                    case "2021":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2021-T4A.rdlc");
                        break;
                    case "2020":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2020-T4A.rdlc");
                        break;
                    case "2019":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2019-T4A.rdlc");
                        break;
                    case "2018":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2018-T4A.rdlc");
                        break;
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
                    case "2009":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2009-T4A.rdlc");
                        break;
                    case "2008":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2008-T4A.rdlc");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.Error(message);
                        throw new ApplicationException(message);
                }
                logger.Debug("Template found. Using '" + (pdfTemplatePath ?? string.Empty) + "'");

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
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the T4A PDF data.", HttpStatusCode.Forbidden);
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
        /// <accessComments>
        /// Requires permission VIEW.1099MISC.
        /// The tax form record requested must belong to the current user.       
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns> 
        public async Task<HttpResponseMessage> Get1099MiscTaxFormPdf2Async(string personId, string recordId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                    throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

                if (string.IsNullOrEmpty(recordId))
                    throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);
            
                var consents = await taxFormConsentService.Get2Async(personId, TaxFormTypes.Form1099MI);
                consents = consents.OrderByDescending(c => c.TimeStamp);
                var mostRecentConsent = consents.FirstOrDefault();

                if (mostRecentConsent == null || !mostRecentConsent.HasConsented)
                {
                    throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
                }

                string pdfTemplatePath = string.Empty;
                var pdfData = await taxFormPdfService.Get1099MiscPdfDataAsync(personId, recordId);
                // Determine which PDF template to use.
                // Any new year has to be added to the list in the ColleagueFinanceConstants class.
                switch (pdfData.TaxYear)
                {
                    case "2009":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2009-1099MI.rdlc");
                        break;
                    case "2008":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2008-1099MI.rdlc");
                        break;
                    case "2018":
                    case "2017":
                    case "2016":
                    case "2015":
                    case "2011":
                    case "2010":
                    case "2012":
                    case "2019":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/20XX-1099MI.rdlc");
                        break;
                    case "2014":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2014-1099MI.rdlc");
                        break;
                    case "2013":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2013-1099MI.rdlc");
                        break;
                    case "2020":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2020-1099MI.rdlc");
                        break;
                    case "2021":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2021-1099MI.rdlc");
                        break;
                    case "2022":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2022-1099MI.rdlc");
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
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the 1099-MISC PDF data.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Error retrieving 1099-MISC PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the 1099-NEC tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-NEC.</param>
        /// <param name="recordId">The record ID where the 1099-NEC pdf data is stored</param>         
        /// <accessComments>
        /// Requires permission VIEW.1099NEC.
        /// The tax form record requested must belong to the current user.       
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns> 
        public async Task<HttpResponseMessage> Get1099NecTaxFormPdfAsync(string personId, string recordId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                    throw CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

                if (string.IsNullOrEmpty(recordId))
                    throw CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);
            
                // Check if the person has consented to receiving their 1099-NEC online - if not, throw exception
                var consents = await taxFormConsentService.Get2Async(personId, TaxFormTypes.Form1099NEC);
                consents = consents.OrderByDescending(c => c.TimeStamp);
                var mostRecentConsent = consents.FirstOrDefault();

                if (mostRecentConsent == null || !mostRecentConsent.HasConsented)
                {
                    throw CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
                }

                string pdfTemplatePath = string.Empty;
                var pdfData = await taxFormPdfService.Get1099NecPdfDataAsync(personId, recordId);

                // Determine which PDF template to use.
                // Any new year has to be added to the list in the ColleagueFinanceConstants class.
                switch (pdfData.TaxYear)
                {
                    case "2020":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2020-1099NEC.rdlc");
                        break;
                    case "2021":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2021-1099NEC.rdlc");
                        break;
                    case "2022":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2022-1099NEC.rdlc");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.Error(message);
                        throw new ApplicationException(message);
                }

                var pdfBytes = taxFormPdfService.Populate1099NecPdf(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(pdfBytes);

                var fileNameString = "TaxForm1099NEC" + "_" + recordId;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileNameString + ".pdf"
                };
                response.Content.Headers.ContentLength = pdfBytes.Length;
                return response;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the 1099-NEC PDF data.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Error retrieving 1099-NEC PDF data.", HttpStatusCode.BadRequest);
            }
        }


        #region OBSOLETE METHODS

        /// <summary>
        /// Returns the data to be printed on the pdf for the T4A tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4A.</param>
        /// <param name="recordId">The record ID where the T4A pdf data is stored</param>  
        /// <accessComments>
        /// Requires permission VIEW.T4A for the recipient.
        /// Requires permission VIEW.T4A for someone who currently has permission to proxy for the recipient requested.
        /// Requires permission VIEW.RECIPIENT.T4A for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>        
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use GetFormT4aPdf2Async instead.")]
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
                    case "2019":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2019-T4A.rdlc");
                        break;
                    case "2018":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2018-T4A.rdlc");
                        break;
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
                    case "2009":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2009-T4A.rdlc");
                        break;
                    case "2008":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2008-T4A.rdlc");
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
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the T4A PDF data.", HttpStatusCode.Forbidden);
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
        /// <accessComments>
        /// Requires permission VIEW.1099MISC.
        /// The tax form record requested must belong to the current user.       
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns> 
        [Obsolete("Obsolete as of API 1.29.1. Use Get1099MiscTaxFormPdf2Async instead.")]
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
                // Any new year has to be added to the list in the ColleagueFinanceConstants class.
                switch (pdfData.TaxYear)
                {
                    case "2009":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2009-1099MI.rdlc");
                        break;
                    case "2008":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2008-1099MI.rdlc");
                        break;
                    case "2018":
                    case "2017":
                    case "2016":
                    case "2015":
                    case "2011":
                    case "2010":
                    case "2012":
                    case "2019":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/20XX-1099MI.rdlc");
                        break;
                    case "2014":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2014-1099MI.rdlc");
                        break;
                    case "2013":
                        pdfTemplatePath = HttpContext.Current.Server.MapPath("~/Reports/ColleagueFinance/2013-1099MI.rdlc");
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
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the 1099-MISC PDF data.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Error retrieving 1099-MISC PDF data.", HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
