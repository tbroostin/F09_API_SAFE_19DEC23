/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Web;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// AwardLettersController exposes actions to interact with AwardLetter resources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardLettersController : BaseCompressedApiController
    {
        private readonly IAwardLetterService awardLetterService;

        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// AwardLettersController constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="awardLetterService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public AwardLettersController(IAdapterRegistry adapterRegistry, IAwardLetterService awardLetterService, ILogger logger, ApiSettings apiSettings)
        {
            this.awardLetterService = awardLetterService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.apiSettings = apiSettings;
        }

        #region Obsolete methods

        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        [Obsolete("Obsolete as of Api version 1.9, use version 2 of this API")]
        public IEnumerable<AwardLetter> GetAwardLetters(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return awardLetterService.GetAwardLetters(studentId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetters resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetters resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetters. See log for details.");
            }
        }

        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data. Award letter objects might contain no awards if none found
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        [Obsolete("Obsolete as of Api version 1.10, use version 3 of this API")]
        public IEnumerable<AwardLetter> GetAwardLetters2(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return awardLetterService.GetAwardLetters2(studentId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetters resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetters resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetters. See log for details.");
            }
        }

        /// <summary>
        /// Get a student's award letter in JSON format for a single award year.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to retrieve an award letter</param>
        /// <param name="awardYear">The award year of the award letter to get</param>
        /// <returns>An AwardLetter DTO object.</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.9, use version 2 of this API")]
        public AwardLetter GetAwardLetter(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                return awardLetterService.GetAwardLetters(studentId, awardYear);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Get a student's award letter in JSON format for a single award year. Award letter is returned even if no awards are
        /// associated with the letter
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to retrieve an award letter</param>
        /// <param name="awardYear">The award year of the award letter to get</param>
        /// <returns>An AwardLetter DTO object.</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.10, use version 3 of this API")]
        public AwardLetter GetAwardLetter2(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                return awardLetterService.GetAwardLetters2(studentId, awardYear);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Get a single award letter as a byte array representation of a PDF file for a student for a particular award year.  
        /// Client should indicate the header value - Accept: application/pdf.
        /// A suggested filename for the report is located in the ContentDisposition.Filename header 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get an award letter</param>
        /// <param name="awardYear">The award year for which to get award letter data</param>
        /// <returns>An HttpResponseMessage containing byte array representing a PDF</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.9, use version 2 of this API")]
        public HttpResponseMessage GetAwardLetterReport(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                //get award letter DTO
                var awardLetterDto = awardLetterService.GetAwardLetters(studentId, awardYear);
                if (awardLetterDto == null) throw new KeyNotFoundException();

                //get Path of the .rdlc template
                var reportPath = HttpContext.Current.Server.MapPath("~/Reports/FinancialAid/AwardLetter.rdlc");

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

                //generate the pdf based on the award letter DTO
                var renderedBytes = awardLetterService.GetAwardLetters(awardLetterDto, reportPath, reportLogoPath);                

                //create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(renderedBytes);

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Regex.Replace(
                    ("AwardLetter" + " " + studentId + " " + awardYear + " " + awardLetterDto.Date.ToShortDateString()),
                    "[^a-zA-Z0-9_]",
                    "_")
                    + ".pdf"
                };
                response.Content.Headers.ContentLength = renderedBytes.Length;
                return response;
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Get a single award letter as a byte array representation of a PDF file for a student for a particular award year.
        /// An award letter object that is used to create the report might come back with no awards if
        /// none were found for the year.
        /// Client should indicate the header value - Accept: application/pdf.
        /// A suggested filename for the report is located in the ContentDisposition.Filename header 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get an award letter</param>
        /// <param name="awardYear">The award year for which to get award letter data</param>
        /// <returns>An HttpResponseMessage containing byte array representing a PDF</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.10, use GetAwardLetterReport3Async")]
        public HttpResponseMessage GetAwardLetterReport2(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                //get award letter DTO
                var awardLetterDto = awardLetterService.GetAwardLetters2(studentId, awardYear);
                if (awardLetterDto == null) throw new KeyNotFoundException();

                //get Path of the .rdlc template
                var reportPath = HttpContext.Current.Server.MapPath("~/Reports/FinancialAid/AwardLetter.rdlc");

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

                //generate the pdf based on the award letter DTO
                var renderedBytes = awardLetterService.GetAwardLetters(awardLetterDto, reportPath, reportLogoPath);

                //create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(renderedBytes);

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Regex.Replace(
                    ("AwardLetter" + " " + studentId + " " + awardYear + " " + awardLetterDto.Date.ToShortDateString()),
                    "[^a-zA-Z0-9_]",
                    "_")
                    + ".pdf"
                };
                response.Content.Headers.ContentLength = renderedBytes.Length;
                return response;
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Update a student's award letter. This update permits changes to the award letter's AcceptedDate
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">Student's Colleague PERSON id. Must match awardLetter's studentId</param>
        /// <param name="awardYear">AwardYear of award letter to update. Must match awardLetter's awardYear</param>
        /// <param name="awardLetter">AwardLetter DTO containing data which which to update the database</param>
        /// <returns>An updated AwardLetter DTO</returns>
        [HttpPut]
        [Obsolete("Obsolete as of Api version 1.10, use version 2 of this API")]
        public AwardLetter UpdateAwardLetter([FromUri] string studentId, [FromUri] string awardYear, [FromBody] AwardLetter awardLetter)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYear cannot be null or empty");
            }
            if (awardLetter == null)
            {
                throw CreateHttpResponseException("awardLetter cannot be null");
            }
            if (awardYear != awardLetter.AwardYearCode)
            {
                var message = string.Format("AwardYear {0} in URI does not match AwardYear {1} of awardLetter in request body", awardYear, awardLetter.AwardYearCode);
                logger.Error(message);
                throw CreateHttpResponseException(message);
            }

            try
            {
                return awardLetterService.UpdateAwardLetter(awardLetter);
            }
            catch (ArgumentException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("awardLetter in request body contains invalid attribute values. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to update AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (OperationCanceledException oce)
            {
                logger.Error(oce, oce.Message);
                throw CreateHttpResponseException("AwardLetter Update request was canceled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }

        }


        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data. Award letter objects might contain no awards if none found
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetters4Async")]
        public async Task<IEnumerable<AwardLetter2>> GetAwardLetters3Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return await awardLetterService.GetAwardLetters3Async(studentId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetters resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetters resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetters. See log for details.");
            }
        }

        /// <summary>
        /// Get a student's award letter in JSON format for a single award year. Award letter is returned even if no awards are
        /// associated with the letter
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to retrieve an award letter</param>
        /// <param name="awardYear">The award year of the award letter to get</param>
        /// <returns>An AwardLetter DTO object.</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetter4Async")]
        public async Task<AwardLetter2> GetAwardLetter3Async(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                return await awardLetterService.GetAwardLetter3Async(studentId, awardYear);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Get a single award letter as a byte array representation of a PDF file for a student for a particular award year.
        /// Client should indicate the header value - Accept: application/pdf.
        /// A suggested filename for the report is located in the ContentDisposition.Filename header 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">student id for whom to retrieve the report</param>        
        /// <param name="awardLetterId">id of the award letter history record</param>
        /// <returns>An HttpResponseMessage containing byte array representing a PDF</returns>
        [HttpGet]
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetterReport4Async")]
        public async Task<HttpResponseMessage> GetAwardLetterReport3Async(string studentId, string awardLetterId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }            
            if (string.IsNullOrEmpty(awardLetterId))
            {
                throw CreateHttpResponseException("awardLetterId cannot be null or empty");
            }

            try
            {
                //get award letter DTO
                var awardLetterDto = await awardLetterService.GetAwardLetterByIdAsync(studentId, awardLetterId);
                if (awardLetterDto == null) throw new KeyNotFoundException();

                //get Path of the .rdlc template
                var reportPath = HttpContext.Current.Server.MapPath("~/Reports/FinancialAid/AwardLetter2.rdlc");

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

                //generate the pdf based on the award letter DTO
                var renderedBytes = await awardLetterService.GetAwardLetterReport3Async(awardLetterDto, reportPath, reportLogoPath);                
                
                //create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(renderedBytes);

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Regex.Replace(
                    ("AwardLetter" + " " + studentId + " " + awardLetterDto.AwardLetterYear + " " + awardLetterDto.CreatedDate.Value.ToShortDateString()),
                    "[^a-zA-Z0-9_]",
                    "_")
                    + ".pdf"
                };
                response.Content.Headers.ContentLength = renderedBytes.Length;
                return response;
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Update a student's award letter. This update permits changes to the award letter's AcceptedDate
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">Student's Colleague PERSON id. Must match awardLetter's studentId</param>
        /// <param name="awardYear">AwardYear of award letter to update. Must match awardLetter's awardYear</param>
        /// <param name="awardLetter">AwardLetter DTO containing data which which to update the database</param>
        /// <returns>An updated AwardLetter DTO</returns>
        [HttpPut]
        [Obsolete("Obsolete as of Api version 1.22, use UpdateAwardLetter3Async")]
        public async Task<AwardLetter2> UpdateAwardLetter2Async([FromUri] string studentId, [FromUri] string awardYear, [FromBody] AwardLetter2 awardLetter)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYear cannot be null or empty");
            }
            if (awardLetter == null)
            {
                throw CreateHttpResponseException("awardLetter cannot be null");
            }
            if (awardYear != awardLetter.AwardLetterYear)
            {
                var message = string.Format("AwardYear {0} in URI does not match AwardYear {1} of awardLetter in request body", awardYear, awardLetter.AwardLetterYear);
                logger.Error(message);
                throw CreateHttpResponseException(message);
            }

            try
            {
                return await awardLetterService.UpdateAwardLetter2Async(awardLetter);
            }
            catch (ArgumentException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("awardLetter in request body contains invalid attribute values. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to update AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (OperationCanceledException oce)
            {
                logger.Error(oce, oce.Message);
                throw CreateHttpResponseException("AwardLetter Update request was canceled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }

        }

        #endregion

        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data. Award letter objects might contain no awards if none found
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        public async Task<IEnumerable<AwardLetter3>> GetAwardLetters4Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return await awardLetterService.GetAwardLetters4Async(studentId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetters resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetters resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetters. See log for details.");
            }
        }

        /// <summary>
        /// Get a student's award letter in JSON format for a single award year. Award letter is returned even if no awards are
        /// associated with the letter
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to retrieve an award letter</param>
        /// <param name="awardYear">The award year of the award letter to get</param>
        /// <returns>An AwardLetter3 DTO object.</returns>
        [HttpGet]
        public async Task<AwardLetter3> GetAwardLetter4Async(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYear cannot be null or empty");
            }

            try
            {
                return await awardLetterService.GetAwardLetter4Async(studentId, awardYear);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Get a single award letter as a byte array representation of a PDF file for a student for a particular award year.
        /// Client should indicate the header value - Accept: application/pdf.
        /// A suggested filename for the report is located in the ContentDisposition.Filename header 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">student id for whom to retrieve the report</param>        
        /// <param name="awardLetterId">id of the award letter history record</param>
        /// <returns>An HttpResponseMessage containing byte array representing a PDF</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetAwardLetterReport4Async(string studentId, string awardLetterId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardLetterId))
            {
                throw CreateHttpResponseException("awardLetterId cannot be null or empty");
            }

            try
            {
                //get award letter DTO
                var awardLetterDto = await awardLetterService.GetAwardLetterById2Async(studentId, awardLetterId);
                if (awardLetterDto == null) throw new KeyNotFoundException();

                var reportPath = "";
                //get Path of the .rdlc template
                if (awardLetterDto.AwardLetterHistoryType == "OLTR")
                {
                    reportPath = HttpContext.Current.Server.MapPath("~/Reports/FinancialAid/ConfigurableOfferLetter.rdlc");
                }
                else
                {
                    reportPath = HttpContext.Current.Server.MapPath("~/Reports/FinancialAid/AwardLetter3.rdlc");
                }
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

                //generate the pdf based on the award letter DTO
                var renderedBytes = await awardLetterService.GetAwardLetterReport4Async(awardLetterDto, reportPath, reportLogoPath);

                //create and return the HTTP response object
                var response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(renderedBytes);

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Regex.Replace(
                    ("AwardLetter" + " " + studentId + " " + awardLetterDto.AwardLetterYear + " " + awardLetterDto.CreatedDate.Value.ToShortDateString()),
                    "[^a-zA-Z0-9_]",
                    "_")
                    + ".pdf"
                };
                response.Content.Headers.ContentLength = renderedBytes.Length;
                return response;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Invalid operation based on state of AwardLetter resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }
        }

        /// <summary>
        /// Update a student's award letter. This update permits changes to the award letter's AcceptedDate
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">Student's Colleague PERSON id. Must match awardLetter's studentId</param>
        /// <param name="awardYear">AwardYear of award letter to update. Must match awardLetter's awardYear</param>
        /// <param name="awardLetter">AwardLetter3 DTO containing data which which to update the database</param>
        /// <returns>An updated AwardLetter3 DTO</returns>
        [HttpPut]
        public async Task<AwardLetter3> UpdateAwardLetter3Async([FromUri] string studentId, [FromUri] string awardYear, [FromBody] AwardLetter3 awardLetter)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYear cannot be null or empty");
            }
            if (awardLetter == null)
            {
                throw CreateHttpResponseException("awardLetter cannot be null");
            }
            if (awardYear != awardLetter.AwardLetterYear)
            {
                var message = string.Format("AwardYear {0} in URI does not match AwardYear {1} of awardLetter in request body", awardYear, awardLetter.AwardLetterYear);
                logger.Error(message);
                throw CreateHttpResponseException(message);
            }

            try
            {
                return await awardLetterService.UpdateAwardLetter3Async(awardLetter);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("awardLetter in request body contains invalid attribute values. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AwardLetter resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to update AwardLetter resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (OperationCanceledException oce)
            {
                logger.Error(oce, oce.Message);
                throw CreateHttpResponseException("AwardLetter Update request was canceled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardLetter resource. See log for details.");
            }

        }

    }
}