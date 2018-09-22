// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Web;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Configuration;
using System.Text.RegularExpressions;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Coordination.Finance;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// StudentStatementsController exposes the StudentStatement endpoint
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    public class StudentStatementsController : BaseCompressedApiController
    {
        private readonly IStudentStatementService statementService;

        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// StudentStatementsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="statementService">Interface to Student Statement Coordination Service</param>
        /// <param name="logger">Logger</param>
        /// <param name="apiSettings">ERP API Settings</param>
        public StudentStatementsController(IAdapterRegistry adapterRegistry, IStudentStatementService statementService, ILogger logger, ApiSettings apiSettings)
        {
            this.statementService = statementService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Get a student's accounts receivable statement as a byte array representation of a PDF file for a timeframe.  
        /// Client should indicate the header value - Accept: application/pdf.
        /// A suggested filename for the report is located in the ContentDisposition.Filename header 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY 
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="accountHolderId">ID of the student for whom the statement will be generated</param>
        /// <param name="timeframeId">ID of the timeframe for which the statement will be generated</param>
        /// <param name="startDate">Date on which the supplied timeframe starts</param>
        /// <param name="endDate">Date on which the supplied timeframe ends</param>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF</returns>
        [ParameterSubstitutionFilter]
        public async Task<HttpResponseMessage> GetStudentStatementAsync(string accountHolderId, string timeframeId, DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(accountHolderId)) throw CreateHttpResponseException("Account Holder ID must be specified.");
            if (string.IsNullOrEmpty(timeframeId)) throw CreateHttpResponseException("Timeframe ID must be specified.");

            try
            {
                //get Student Statement DTO
                var statementDto = await statementService.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                if (statementDto == null) throw new ApplicationException("Student Statement could not be generated.");

                //get the path of the .rdlc template
                var reportPath = HttpContext.Current.Server.MapPath("~/Reports/Finance/StudentStatement.rdlc");

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

                var resourceFilePath = HttpContext.Current.Server.MapPath("~/App_GlobalResources/Finance/StatementResources.resx");

                //generate the PDF based on the StudentStatement DTO
                var renderedBytes = statementService.GetStudentStatementReport(statementDto, reportPath, resourceFilePath, 
                    reportLogoPath);

                //create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(renderedBytes);

                var fileNameString = "StudentStatement" + " " + accountHolderId + " " + timeframeId;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Regex.Replace(fileNameString, "[^a-zA-Z0-9_]", "_") + ".pdf"
                };
                response.Content.Headers.ContentLength = renderedBytes.Length;
                return response;
            }
            catch (ArgumentException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Student Statement query parameters are not valid. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Student Statement is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (InvalidOperationException ioe)
            {
                logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of Student Statement resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Student Statement could not be generated. See log for details.");
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting StudentStatement resource. See log for details.");
            }
        }
    }
}