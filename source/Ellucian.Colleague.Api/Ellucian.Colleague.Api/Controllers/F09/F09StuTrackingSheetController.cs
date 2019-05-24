using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Web.Http;
using System.Net;
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
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Colleague.Coordination.F09.Services;

namespace Ellucian.Colleague.Api.Controllers.F09
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class F09StuTrackingSheetController : BaseCompressedApiController
    {
        private readonly IGetF09StuTrackingSheetService _GetF09StuTrackingSheetService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getF09StuTrackingSheetService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public F09StuTrackingSheetController(IGetF09StuTrackingSheetService getF09StuTrackingSheetService, ILogger logger, ApiSettings apiSettings)
        {
            if (getF09StuTrackingSheetService == null) throw new ArgumentNullException(nameof(getF09StuTrackingSheetService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._GetF09StuTrackingSheetService = getF09StuTrackingSheetService;
            this._logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<GetF09StuTrackingSheetResponseDto> GetF09StuTrackingSheetAsync(string Id)
        {
            GetF09StuTrackingSheetResponseDto stuTrackingSheet;
            try
            {
                if (string.IsNullOrEmpty(Id))
                {
                    _logger.Error("F09StuTrackingSheetController-GetF09StuTrackingSheetAsync: Must provide a person id in the request uri");
                    throw new Exception();
                }

                stuTrackingSheet = await _GetF09StuTrackingSheetService.GetF09StuTrackingSheetAsync(Id);

                return stuTrackingSheet;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to get Tracking Sheet information: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

        ///
        /// F09 added on 05-23-2019 for Pdf Student Tracking Sheet project
        ///
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
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF</returns>
        [ParameterSubstitutionFilter]
        public async Task<HttpResponseMessage> GetPdfStudentTrackingSheetAsync(string accountHolderId)
        {
            if (string.IsNullOrEmpty(accountHolderId)) throw CreateHttpResponseException("Account Holder ID must be specified.");

            try
            {
                var responseDto = await _GetF09StuTrackingSheetService.GetPdfStudentTrackingSheetAsync(accountHolderId);

                if (responseDto == null) throw new ApplicationException("Student Tracking Sheet could not be generated.");

                //get the path of the .rdlc template
                var reportPath = HttpContext.Current.Server.MapPath("~/Reports/F09/StudentTrackingSheet.rdlc");

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

                var resourceFilePath = HttpContext.Current.Server.MapPath("~/App_GlobalResources/F09/F09StudentTrackingSheetResources.resx");

                //generate the PDF based on the StudentStatement DTO
                var renderedBytes = _GetF09StuTrackingSheetService.GetStudentTrackingSheetReport(responseDto, reportPath, resourceFilePath,
                    reportLogoPath);

                //create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(renderedBytes);

                var fileNameString = "StudentTrackingSheet" + " " + accountHolderId;
                //var fileNameString = "StudentTrackingSheet" + " " + accountHolderId + " " + timeframeId;
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
                _logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Student Tracking Sheet query parameters are not valid. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Student Tracking Sheet is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (InvalidOperationException ioe)
            {
                _logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of Student Tracking Sheet resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (ApplicationException ae)
            {
                _logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Student Tracking Sheet could not be generated. See log for details.");
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting Student Tracking Sheet resource. See log for details.");
            }
        }

    }
}
