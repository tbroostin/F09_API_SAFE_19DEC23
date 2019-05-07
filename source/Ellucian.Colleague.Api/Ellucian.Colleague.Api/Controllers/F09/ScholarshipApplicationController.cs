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
    public class ScholarshipApplicationController : BaseCompressedApiController
    {
        private readonly IScholarshipApplicationService _UpdateScholarshipApplicationService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateScholarshipApplicationService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public ScholarshipApplicationController(IScholarshipApplicationService updateScholarshipApplicationService, ILogger logger, ApiSettings apiSettings)
        {
            if (updateScholarshipApplicationService == null) throw new ArgumentNullException(nameof(ScholarshipApplicationService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._UpdateScholarshipApplicationService = updateScholarshipApplicationService;
            this._logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ScholarshipApplicationResponseDto> GetScholarshipApplicationAsync(string personId)
        {
            ScholarshipApplicationResponseDto profile;

            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    _logger.Error("F09-ScholarshipApplicationController-GetScholarshipApplicationAsync: Must provide a person id in the request uri");
                    throw new Exception();
                }

                profile = await _UpdateScholarshipApplicationService.GetScholarshipApplicationAsync(personId);

                return profile;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to get Scholarship Application information: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates certain person profile information: AddressConfirmationDateTime, EmailAddressConfirmationDateTime,
        /// PhoneConfirmationDateTime, EmailAddresses, Personal Phones and Addresses. LastChangedDateTime must match the last changed timestamp on the database
        /// Person record to ensure updates not occurring from two different sources at the same time. If no changes are found, a NotModified Http status code
        /// is returned. If required by configuration, users must be set up with permissions to perform these updates: UPDATE.OWN.EMAIL, UPDATE.OWN.PHONE, and 
        /// UPDATE.OWN.ADDRESS. 
        /// </summary>
        /// <param name="request"><see cref="Dtos.Base.Profile">Profile</see> to use to update</param>
        /// <returns>Newly updated <see cref="Dtos.Base.Profile">Profile</see></returns>
        [HttpPut]
        public async Task<ScholarshipApplicationResponseDto> PutScholarshipApplicationAsync([FromBody] ScholarshipApplicationRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    _logger.Error("F09-ScholarshipApplicationController-PutScholarshipApplicationAsync: Must provide a profile in the request body");
                    throw new Exception();
                }
                if (string.IsNullOrEmpty(request.Id))
                {
                    _logger.Error("F09-ScholarshipApplicationController-PutScholarshipApplicationAsync: Must provide a person Id in the request body");
                    throw new Exception();
                }

                var profile = await _UpdateScholarshipApplicationService.UpdateScholarshipApplicationAsync(request);

                return profile;
            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to update Scholarship Application information: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

        ///
        /// F09 added on 05-04-2019 for Demo Reporting Project
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
        public async Task<HttpResponseMessage> GetStudentStatementAsync(string accountHolderId)
        {
            if (string.IsNullOrEmpty(accountHolderId)) throw CreateHttpResponseException("Account Holder ID must be specified.");
            //if (string.IsNullOrEmpty(timeframeId)) throw CreateHttpResponseException("Timeframe ID must be specified.");

            try
            {
                var responseDto = await _UpdateScholarshipApplicationService.GetScholarshipApplicationAsync(accountHolderId);
                var requestDto = this.CreateScholarshipApplicationRequestDto(responseDto, "Submit.Soft.Questions");
                var profile = await _UpdateScholarshipApplicationService.UpdateScholarshipApplicationAsync(requestDto);

                //get Student Statement DTO
                var statementDto = new ScholarshipApplicationStudentStatementDto();
                statementDto.StudentId = accountHolderId;
                statementDto.StudentName = profile.StudentName;
                statementDto.Awards = profile.Awards;

                //var statementDto = await statementService.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                if (statementDto == null) throw new ApplicationException("Student Statement could not be generated.");

                //get the path of the .rdlc template
                var reportPath = HttpContext.Current.Server.MapPath("~/Reports/F09/StudentStatement.rdlc");

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

                var resourceFilePath = HttpContext.Current.Server.MapPath("~/App_GlobalResources/F09/F09StatementResources.resx");

                //generate the PDF based on the StudentStatement DTO
                var renderedBytes = _UpdateScholarshipApplicationService.GetStudentStatementReport(statementDto, reportPath, resourceFilePath,
                    reportLogoPath);

                //create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(renderedBytes);

                var fileNameString = "StudentStatement" + " " + accountHolderId;
                //var fileNameString = "StudentStatement" + " " + accountHolderId + " " + timeframeId;
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
                throw CreateHttpResponseException("Student Statement query parameters are not valid. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Student Statement is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (InvalidOperationException ioe)
            {
                _logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of Student Statement resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (ApplicationException ae)
            {
                _logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Student Statement could not be generated. See log for details.");
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting StudentStatement resource. See log for details.");
            }
        }

        private ScholarshipApplicationRequestDto CreateScholarshipApplicationRequestDto(ScholarshipApplicationResponseDto response, string requestType)
        {
            ScholarshipApplicationRequestDto dto = new ScholarshipApplicationRequestDto();
            dto.Id = response.Id;
            dto.RequestType = requestType;
            dto.XfstId = response.XfstId;
            dto.XfstRefName = response.XfstRefName;
            dto.XfstSelfRate = response.XfstSelfRate;
            dto.XfstSelfRateDesc = response.XfstSelfRateDesc;
            dto.XfstResearchInt = response.XfstResearchInt;
            dto.XfstDissTopic = response.XfstDissTopic;
            dto.XfstFinSit = response.XfstFinSit;
            dto.Awards = new List<ScholarshipApplicationAwardsDto>();
            dto.SoftQs = new List<ScholarshipApplicationSoftQDto>();

            return dto;
        }
    }
}
