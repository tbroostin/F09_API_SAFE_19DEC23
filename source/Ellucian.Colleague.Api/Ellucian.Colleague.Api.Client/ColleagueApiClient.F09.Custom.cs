using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;
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
                throw new ArgumentNullException("userProfile", "User Profile cannot be null.");
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
    }
}
