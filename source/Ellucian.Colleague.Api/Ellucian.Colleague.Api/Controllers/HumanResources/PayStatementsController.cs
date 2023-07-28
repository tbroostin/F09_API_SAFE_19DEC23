/* Copyright 2017-2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using slf4net;
using System;
using System.Collections.Generic;
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
    /// Pay Statements Controller routes requests for interacting with pay statements
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayStatementsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPayStatementService payStatementService;
        private readonly ApiSettings apiSettings;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="payStatementService">pay statement service</param>
        /// <param name="apiSettings">api settings</param>
        public PayStatementsController(ILogger logger, IPayStatementService payStatementService, ApiSettings apiSettings)
        {
            this.logger = logger;
            this.payStatementService = payStatementService;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Gets a list of summaries of all pay statements for the current user, or, if you have the proper permissions, for any
        /// employee.
        /// A summary object corresponds to a pay statement record. You can use the id of the PayStatement in a request for a
        /// Pay Statement Report.
        /// Use the filter start and end dates to filter out 
        /// A successful request will return a status code 200 response and a list of pay statement summary object.    
        /// </summary>
        /// <param name="employeeId">optional: the employee identifier</param>
        /// <param name="hasOnlineConsent">optional: whether the employee has consented to viewing statements online</param>
        /// <param name="payCycleId">optional: the pay cycle in which the employee was paid that generated this statement</param>
        /// <param name="payDate">optional: the date the employee was paid</param>
        /// <param name="startDate">optional: start date to filter pay statements by</param>
        /// <param name="endDate">optional: end date to filter pay statements by</param>
        /// <returns>An array of PayStatementSummary objects for the requested parameters</returns>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets a list of summaries of all pay statements for the current user, or, if you have the proper permissions, for any employee.",
            HttpMethodDescription = "Gets a list of summaries of all pay statements for the current user, or, if you have the proper permissions, for any employee.")]
        public async Task<IEnumerable<PayStatementSummary>> GetPayStatementSummariesAsync(
            [FromUri(Name = "employeeId")] string employeeId = null,
            [FromUri(Name = "hasOnlineConsent")] bool? hasOnlineConsent = null,
            [FromUri(Name = "payDate")] DateTime? payDate = null,
            [FromUri(Name = "payCycleId")] string payCycleId = null,
            [FromUri(Name = "startDate")] DateTime? startDate = null,
            [FromUri(Name = "endDate")] DateTime? endDate = null)
        {
            try
            {
                logger.Debug("************Start- Process to get pay statement summary - Start************");
                var paystatementsummary= await payStatementService.GetPayStatementSummariesAsync(
                    string.IsNullOrEmpty(employeeId) ? null : new List<string>() { employeeId },
                    hasOnlineConsent,
                    payDate,
                    payCycleId,
                    startDate,
                    endDate
                );
                logger.Debug("************End- Process to pay statement summary - End************");

                return paystatementsummary;
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("Invalid arguments at some level of the request", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Current user does not have permission to view requested data", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query multiple Pay Statement PDFs for the given ids
        /// </summary>
        /// <param name="ids">a list of Pay Statement ids to generate into a single PDF</param>
        /// <returns>a single PDF containing the pay statements specified in the list of ids</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> QueryPayStatementPdfs([FromBody] IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw CreateHttpResponseException("ids are required in request body");
            }

            try
            {
                logger.Debug("************Start- Process to generate multiple PDF statement - Start************");
                var pathToReportTemplate = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/PayStatement.rdlc");

                //get the path of the school's logo
                var reportLogoPath = string.Empty;
                if (!string.IsNullOrEmpty(apiSettings.ReportLogoPath))
                {
                    reportLogoPath = apiSettings.ReportLogoPath;
                    if (!reportLogoPath.StartsWith("~"))
                    {
                        reportLogoPath = "~" + reportLogoPath;
                    }

                    reportLogoPath = HttpContext.Current.Server.MapPath(reportLogoPath);
                }

                var reportBytes = await payStatementService.GetPayStatementPdf(ids, pathToReportTemplate, reportLogoPath);
                var response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new ByteArrayContent(reportBytes);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "Multiple",
                };
                response.Content.Headers.ContentLength = reportBytes.LongLength;
                logger.Debug("************End - Process to generate multiple PDF statement - End************");
                return response;
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("Invalid arguments at some level of the request", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Current user does not have permission to view requested data", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get Pay Statement PDF for the given id. The requested statement must 
        /// be owned by the authenticated user, or the user must have the proper permissions.
        /// </summary>
        /// <param name="id">The id of the requested Pay Statement</param>
        /// <returns>An HttpResponseMessage with the Content property containing a byte[] of the PDF</returns>
        public async Task<HttpResponseMessage> GetPayStatementPdf(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                logger.Debug("************The id of the requested Pay Statement is null or empty************");
                throw CreateHttpResponseException("id is required in request");
            }
            try
            {
                logger.Debug("************Start - Paystatement PDF process - Start************");
                var pathToReportTemplate = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/PayStatement.rdlc");

                //get the path of the school's logo
                var reportLogoPath = string.Empty;
                if (!string.IsNullOrEmpty(apiSettings.ReportLogoPath))
                {
                    reportLogoPath = apiSettings.ReportLogoPath;
                    if (!reportLogoPath.StartsWith("~"))
                    {
                        reportLogoPath = "~" + reportLogoPath;
                    }

                    reportLogoPath = HttpContext.Current.Server.MapPath(reportLogoPath);
                }
                
                var reportTuple = await payStatementService.GetPayStatementPdf(id, pathToReportTemplate, reportLogoPath);
                var response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new ByteArrayContent(reportTuple.Item2);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = reportTuple.Item1
                };
                response.Content.Headers.ContentLength = reportTuple.Item2.LongLength;
                logger.Debug("************End -Paystatement PDF process ends successfully - End ************");
                return response;
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("Invalid arguments at some level of the request", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Current user does not have permission to view requested data", HttpStatusCode.Forbidden);
            }
            catch (LocalProcessingException lpe)
            {
                var message = "Exception occurred rendering the PDF: " + lpe.Message;
                logger.Error(lpe, "Exception occurred rendering the PDF");
                if (lpe.InnerException != null)
                {
                    message += " :: " + lpe.InnerException.Message;
                    logger.Error(lpe.InnerException, "Inner exception of PDF Render");
                }
                throw CreateHttpResponseException(message);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred", HttpStatusCode.BadRequest);
            }
        }

        #region Experience end-points
        /// <summary>
        /// This ethos end-point will return the pay statement information for a given id.
        /// The requested pay statement must be owned by the authenticated user.
        /// </summary>
        /// <param name="id">The id of the requested pay statement.</param>      
        /// <returns>The requested PayStatementInformation DTO</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "This ethos end-point will return the pay statement information for a given id.",
           HttpMethodDescription = "This ethos end-point will return the pay statement information for a given id.")]
        public async Task<PayStatementInformation> GetPayStatementInformationAsync(string id)
        {
            logger.Debug("********* Start - Process to get pay statement information - Start *********");
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    logger.Debug("id cannot be null");
                    throw new ArgumentNullException("id", "id cannot be null");
                }

                var payStatementInformation = await payStatementService.GetPayStatementInformationAsync(id);
                logger.Debug("********* End - Process to get pay statement information - End *********");
                return payStatementInformation;
            }
            catch (PermissionsException e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ApplicationException e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>        
        /// Get all PayStatementInformation
        /// </summary>
        /// <returns>PayStatementInformation objects</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "This ethos end-point will return all the pay statement information.",
          HttpMethodDescription = "This ethos end-point will return all the pay statement information.")]
        public async Task<IEnumerable<PayStatementInformation>> GetAllPayStatementInformationAsync()
        {
            // GetAll is not supported for Colleague/Experience but ethos requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion
    }
}