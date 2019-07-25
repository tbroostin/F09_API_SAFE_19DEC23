using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories;
using Ellucian.Web.Utility;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Api.Client
{
    public partial class ColleagueApiClient
    {
        // F09 added here on 04-01-2019
        public async Task<GetF09StuTrackingSheetResponseDto> GetF09StuTrackingSheetAsync(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                throw new ArgumentNullException("Id", "ID cannot be empty/null for Tracking Sheet retrieval.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(F09StuTrackingSheet, Id);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(baseUrl, headers: headers);
                var stuTrackingSheet = JsonConvert.DeserializeObject<GetF09StuTrackingSheetResponseDto>(await response.Content.ReadAsStringAsync());

                return stuTrackingSheet;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to GetF09StuTrackingSheetAsync");
                throw;
            }
        }

        ///
        /// F09 added on 05-20-2019 for Pdf Student Tracking Sheet project
        /// 
        /// <summary>
        /// Get an student's accounts receivable report for the given timeframe
        /// </summary>
        /// <param name="accountHolderId">ID of the student for whom the statement will be generated</param>
        /// <param name="fileName">The name of the PDF file returned in the byte array.</param>
        /// <returns>A byte array representation of the PDF Award Letter Report</returns>
        /// <exception cref="Exception">Thrown if an error occurred generating the student tracking sheet report.</exception>
        public byte[] GetF09PdfStudentTrackingSheet(string accountHolderId, out string fileName)
        {
            if (string.IsNullOrEmpty(accountHolderId))
            {
                throw new ArgumentNullException("accountHolderId", "Account Holder ID cannot be null or empty.");
            }

            try
            {
                // Build url path from qapi path and student statements path
                var baseUrl = UrlUtility.CombineUrlPath(F09GetPdfStudentTrackingSheet, accountHolderId);
                var queryString = string.Empty;

                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(baseUrl, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v1+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v1+pdf");
                var response = ExecuteGetRequestWithResponse(combinedUrl, headers: headers);
                fileName = response.Content.Headers.ContentDisposition.FileName;
                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student tracking sheet report.");
                throw;
            }
        }

        // F09 added on 04-10-2019
        public async Task<F09AdminTrackingSheetResponseDto> GetF09AdminTrackingSheetAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "ID cannot be empty/null for User Profile retrieval.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(F09AdminTrackingSheet, personId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(baseUrl, headers: headers);
                var tracking = JsonConvert.DeserializeObject<F09AdminTrackingSheetResponseDto>(await response.Content.ReadAsStringAsync());

                return tracking;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to GetF09F09AdminTrackingSheetAsync");
                throw;
            }
        }

        public async Task<GetActiveRestrictionsResponseDto> GetF09ActiveRestrictionsAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "ID cannot be empty/null for User Profile retrieval.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(F09ActiveRestrictions, personId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(baseUrl, headers: headers);
                var myProfile = JsonConvert.DeserializeObject<GetActiveRestrictionsResponseDto>(await response.Content.ReadAsStringAsync());

                return myProfile;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to GetF09ActiveRestrictionsAsync");
                throw;
            }
        }

        public async Task<UpdateStudentRestrictionResponseDto> GetF09StudentRestrictionAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "ID cannot be empty/null for User Profile retrieval.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(F09StudentRestriction, personId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(baseUrl, headers: headers);
                var myProfile = JsonConvert.DeserializeObject<UpdateStudentRestrictionResponseDto>(await response.Content.ReadAsStringAsync());

                return myProfile;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to GetF09StudentRestrictionAsync");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutF09StudentRestrictionAsync(UpdateStudentRestrictionRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("userProfile", "User Profile cannot be null.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(F09StudentRestriction);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = await ExecutePutRequestWithResponseAsync(request, baseUrl, headers: headers);
                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to PutF09StudentRestrictionAsync.");
                throw;
            }
        }

        // F09 added on 03-14-2019
        public async Task<ScholarshipApplicationResponseDto> GetF09ScholarshipApplicationAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "ID cannot be empty/null for User Profile retrieval.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(F09GetScholarshipApplication, personId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(baseUrl, headers: headers);
                var myProfile = JsonConvert.DeserializeObject<ScholarshipApplicationResponseDto>(await response.Content.ReadAsStringAsync());

                return myProfile;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to GetF09ScholarshipApplicationAsync");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutF09ScholarshipApplicationAsync(ScholarshipApplicationRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("scholarshipApplicationRequest", "Scholarship Application Request cannot be null.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(F09UpdateScholarshipApplication);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = await ExecutePutRequestWithResponseAsync(request, baseUrl, headers: headers);
                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to PutF09ScholarshipApplicationAsync.");
                throw;
            }
        }

        // F09 added on 05-13-2019
        public async Task<DirectoriesResponseDto> GetF09StudentAlumniDirectoriesAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "ID cannot be empty/null for User Profile retrieval.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(F09GetStudentAlumniDirectories, personId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(baseUrl, headers: headers);
                var myProfile = JsonConvert.DeserializeObject<DirectoriesResponseDto>(await response.Content.ReadAsStringAsync());

                return myProfile;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to GetF09StudentAlumniDirectoriesAsync");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutF09StudentAlumniDirectoriesAsync(DirectoriesRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("directoriesRequest", "Directories Request cannot be null.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(F09UpdateStudentAlumniDirectories);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = await ExecutePutRequestWithResponseAsync(request, baseUrl, headers: headers);
                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to PutF09StudentAlumniDirectoriesAsync.");
                throw;
            }
        }

        ///
        /// F09 added on 05-05-2019 for Demo Reporting Project
        /// 
        /// <summary>
        /// Get an student's accounts receivable report for the given timeframe
        /// </summary>
        /// <param name="accountHolderId">ID of the student for whom the statement will be generated</param>
        /// <param name="fileName">The name of the PDF file returned in the byte array.</param>
        /// <returns>A byte array representation of the PDF Award Letter Report</returns>
        /// <exception cref="Exception">Thrown if an error occurred generating the student statement report.</exception>
        public byte[] GetF09StudentStatement(string accountHolderId, out string fileName)
        {
            if (string.IsNullOrEmpty(accountHolderId))
            {
                throw new ArgumentNullException("accountHolderId", "Account Holder ID cannot be null or empty.");
            }
   
            try
            {
                // Build url path from qapi path and student statements path
                var baseUrl = UrlUtility.CombineUrlPath(F09GetStudentStatement, accountHolderId);
                var queryString = string.Empty;

                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(baseUrl, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v1+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v1+pdf");
                var response = ExecuteGetRequestWithResponse(combinedUrl, headers: headers);
                fileName = response.Content.Headers.ContentDisposition.FileName;
                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student statement.");
                throw;
            }
        }

        // F09 teresa@toad-code.com 05/21/19
        public async Task<F09SsnResponseDto> GetF09SsnAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "ID cannot be empty/null for F09Ssn.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(getF09Ssn, personId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(baseUrl, headers: headers);
                var dtoResponse = JsonConvert.DeserializeObject<F09SsnResponseDto>(await response.Content.ReadAsStringAsync());

                return dtoResponse;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to GetF09SsnAsync");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutF09SsnAsync(F09SsnRequestDto dtoRequest)
        {
            if (dtoRequest == null)
            {
                throw new ArgumentNullException("F09SsnRequest", "F09Ssn Request cannot be null.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(updateF09Ssn);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var dtoResponse = await ExecutePutRequestWithResponseAsync(dtoRequest, baseUrl, headers: headers);
                return dtoResponse;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to PutF09SsnAsync.");
                throw;
            }
        }

        // F09 teresa@toad-code.com 06/17/19
        public async Task<F09KaSelectResponseDto> GetF09KaSelectAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "ID cannot be empty/null for F09KaSelect.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(getF09KaSelect, personId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(baseUrl, headers: headers);
                var dtoResponse = JsonConvert.DeserializeObject<F09KaSelectResponseDto>(await response.Content.ReadAsStringAsync());

                return dtoResponse;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to GetF09KaSelectAsync");
                throw;
            }
        }

        // F09 teresa@toad-code.com 06/18/19
        public async Task<dtoF09KaGradingResponse> GetF09KaGradingAsync(string stcId)
        {
            if (string.IsNullOrEmpty(stcId))
            {
                throw new ArgumentNullException("stcId", "stcID cannot be empty/null for F09KaGrading.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(getF09KaGrading, stcId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(baseUrl, headers: headers);
                var dtoResponse = JsonConvert.DeserializeObject<dtoF09KaGradingResponse>(await response.Content.ReadAsStringAsync());

                return dtoResponse;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to GetF09KaGradingAsync");
                throw;
            }
        }

        // F09 teresa@toad-code.com 06/18/19
        public async Task<HttpResponseMessage> PutF09KaGradingAsync(dtoF09KaGradingRequest dtoRequest)
        {
            if (dtoRequest == null)
            {
                throw new ArgumentNullException("F09KaGradingRequest", "F09KaGrading Request cannot be null.");
            }
            try
            {
                var baseUrl = UrlUtility.CombineUrlPath(updateF09KaGrading);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var dtoResponse = await ExecutePutRequestWithResponseAsync(dtoRequest, baseUrl, headers: headers);
                return dtoResponse;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to PutF09KaGradingAsync.");
                throw;
            }
        }
    }
}
