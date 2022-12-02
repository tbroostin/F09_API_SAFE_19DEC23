// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Client.Exceptions;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Rest.Client.Exceptions;
using Ellucian.Web.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Client.Core;

namespace Ellucian.Colleague.Api.Client
{
    public partial class ColleagueApiClient
    {
        /// <summary>
        /// Retrieve all curriculum track codes available for a student for a given academic program code.
        /// </summary>
        /// <param name="studentId">The id for the student for which the curriculum tracks should be returned.</param>
        /// <param name="programCode">The code for the program for which the curriculum tracks should be returned.</param>
        /// <returns>A collection of <see cref="CurriculumTrack">curriculum tracks</see> for student and a given program code.</returns>
        public async Task<IEnumerable<CurriculumTrack>> QueryCurriculumTracksForStudentByProgramAsync(CurriculumTrackQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Criteria cannot be null for Curriculum Track query.");
            }
            if (string.IsNullOrWhiteSpace(criteria.StudentId))
            {
                throw new ArgumentNullException("studentId", "Student Id is required to get program curriculum tracks.");
            }
            if (string.IsNullOrWhiteSpace(criteria.ProgramCode))
            {
                throw new ArgumentNullException("programCode", "Program Code is required to get program curriculum tracks.");
            }

            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_qapiPath, _curriculumTrackPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath: urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CurriculumTrack>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while querying curriculum tracks");
                throw;
            }

            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a curriculum track for the student " + criteria.StudentId + " and program code " + criteria.ProgramCode);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve curriculum tracks.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a sample degree plan and shows how it would look applied to the student's degree plan but does NOT same the change.
        /// </summary>
        /// <param name="id">The degree plan ID</param>
        /// <param name="program">The program code</param>
        /// <param name="catalog">The catalog ID</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID, Program, or Catalog are empty/null</exception>
        /// <returns>The updated DegreePlan DTO</returns>
        [Obsolete("Obsolete as of API version 1.5. Use PreviewSampleDegreePlan3.")]
        public DegreePlanPreview2 PreviewSampleDegreePlan(int degreePlanId, string program, string termCode)
        {
            //make sure parameters are legit
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan ID must be provided and be non zero.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program code cannot be null or empty");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term code cannot be null or empty");
            }

            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "programCode", program, "firstTermCode", termCode });
                var urlPath = UrlUtility.CombineUrlPath(new[] { _degreePlansPath, degreePlanId.ToString(), "preview-sample" });
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanPreview2>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a sample plan to load for program " + program);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate a Sample Plan for review.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a sample degree plan and shows how it would look applied to the student's degree plan but does NOT same the change.
        /// </summary>
        /// <param name="id">The degree plan ID</param>
        /// <param name="program">The program code</param>
        /// <param name="catalog">The catalog ID</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID, Program, or Catalog are empty/null</exception>
        /// <returns>The updated DegreePlan DTO</returns>
        [Obsolete("Obsolete as of API version 1.5. Use PreviewSampleDegreePlan3Async.")]
        public async Task<DegreePlanPreview2> PreviewSampleDegreePlanAsync(int degreePlanId, string program, string termCode)
        {
            //make sure parameters are legit
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan ID must be provided and be non zero.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program code cannot be null or empty");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term code cannot be null or empty");
            }

            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "programCode", program, "firstTermCode", termCode });
                var urlPath = UrlUtility.CombineUrlPath(new[] { _degreePlansPath, degreePlanId.ToString(), "preview-sample" });
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanPreview2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a sample plan to load for program " + program);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate a Sample Plan for review.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a sample degree plan and shows how it would look applied to the student's degree plan but does NOT same the change.
        /// </summary>
        /// <param name="id">The degree plan ID</param>
        /// <param name="program">The program code</param>
        /// <param name="catalog">The catalog ID</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID, Program, or Catalog are empty/null</exception>
        /// <returns>The updated DegreePlan DTO</returns>
        [Obsolete("Obsolete as of API version 1.6. Use PreviewSampleDegreePlan4.")]
        public DegreePlanPreview3 PreviewSampleDegreePlan3(int degreePlanId, string program, string termCode)
        {
            //make sure parameters are legit
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan ID must be provided and be non zero.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program code cannot be null or empty");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term code cannot be null or empty");
            }

            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "programCode", program, "firstTermCode", termCode });
                var urlPath = UrlUtility.CombineUrlPath(new[] { _degreePlansPath, degreePlanId.ToString(), "preview-sample" });
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanPreview3>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a sample plan to load for program " + program);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate a Sample Plan for review.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a sample degree plan and shows how it would look applied to the student's degree plan but does NOT same the change async.
        /// </summary>
        /// <param name="id">The degree plan ID</param>
        /// <param name="program">The program code</param>
        /// <param name="catalog">The catalog ID</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID, Program, or Catalog are empty/null</exception>
        /// <returns>The updated DegreePlan DTO</returns>
        [Obsolete("Obsolete as of API version 1.6. Use PreviewSampleDegreePlan4.")]
        public async Task<DegreePlanPreview3> PreviewSampleDegreePlan3Async(int degreePlanId, string program, string termCode)
        {
            //make sure parameters are legit
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan ID must be provided and be non zero.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program code cannot be null or empty");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term code cannot be null or empty");
            }

            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "programCode", program, "firstTermCode", termCode });
                var urlPath = UrlUtility.CombineUrlPath(new[] { _degreePlansPath, degreePlanId.ToString(), "preview-sample" });
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanPreview3>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a sample plan to load for program " + program);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate a Sample Plan for review.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a sample degree plan and shows how it would look applied to the student's degree plan but does NOT same the change.
        /// </summary>
        /// <param name="id">The degree plan ID</param>
        /// <param name="program">The program code</param>
        /// <param name="catalog">The catalog ID</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID, Program, or Catalog are empty/null</exception>
        /// <returns>The updated DegreePlan DTO</returns>
        public DegreePlanPreview4 PreviewSampleDegreePlan4(int degreePlanId, string program, string termCode)
        {
            //make sure parameters are legit
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan ID must be provided and be non zero.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program code cannot be null or empty");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term code cannot be null or empty");
            }

            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "programCode", program, "firstTermCode", termCode });
                var urlPath = UrlUtility.CombineUrlPath(new[] { _degreePlansPath, degreePlanId.ToString(), "preview-sample" });
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion4);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanPreview4>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a sample plan to load for program " + program);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate a Sample Plan for review.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a sample degree plan and shows how it would look applied to the student's degree plan but does NOT same the change async.
        /// </summary>
        /// <param name="id">The degree plan ID</param>
        /// <param name="program">The program code</param>
        /// <param name="catalog">The catalog ID</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID, Program, or Catalog are empty/null</exception>
        /// <returns>The updated DegreePlan DTO</returns>
        [Obsolete("Obsolete as of API version 1.11. Use PreviewSampleDegreePlan5Async.")]
        public async Task<DegreePlanPreview4> PreviewSampleDegreePlan4Async(int degreePlanId, string program, string termCode)
        {
            //make sure parameters are legit
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan ID must be provided and be non zero.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program code cannot be null or empty");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term code cannot be null or empty");
            }

            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "programCode", program, "firstTermCode", termCode });
                var urlPath = UrlUtility.CombineUrlPath(new[] { _degreePlansPath, degreePlanId.ToString(), "preview-sample" });
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion4);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanPreview4>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a sample plan to load for program " + program);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate a Sample Plan for review.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a sample degree plan and shows how it would look applied to the student's degree plan but does NOT same the change async.
        /// </summary>
        /// <param name="id">The degree plan ID</param>
        /// <param name="program">The program code</param>
        /// <param name="catalog">The catalog ID</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID, Program, or Catalog are empty/null</exception>
        /// <returns>The DegreePlanPreview DTO</returns>
        [Obsolete("Obsolete as of API version 1.18. Use PreviewSampleDegreePlan6Async.")]
        public async Task<DegreePlanPreview5> PreviewSampleDegreePlan5Async(int degreePlanId, string program, string termCode)
        {
            //make sure parameters are legit
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan ID must be provided and be non zero.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program code cannot be null or empty");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term code cannot be null or empty");
            }

            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "programCode", program, "firstTermCode", termCode });
                var urlPath = UrlUtility.CombineUrlPath(new[] { _degreePlansPath, degreePlanId.ToString(), "preview-sample" });
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion5);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanPreview5>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a sample plan to load for program " + program);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate a Sample Plan for review.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a sample degree plan and shows how it would look applied to the student's degree plan but does NOT same the change async.
        /// </summary>
        /// <param name="id">The degree plan ID</param>
        /// <param name="program">The program code</param>
        /// <param name="catalog">The catalog ID</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID, Program, or Catalog are empty/null</exception>
        /// <returns>The DegreePlanPreview DTO</returns>
        [Obsolete("Obsolete as of API version 1.32. Use PreviewSampleDegreePlan7Async.")]
        public async Task<DegreePlanPreview6> PreviewSampleDegreePlan6Async(int degreePlanId, string program, string termCode)
        {
            //make sure parameters are legit
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan ID must be provided and be non zero.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program code cannot be null or empty");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term code cannot be null or empty");
            }

            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "programCode", program, "firstTermCode", termCode });
                var urlPath = UrlUtility.CombineUrlPath(new[] { _degreePlansPath, degreePlanId.ToString(), "preview-sample" });
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion6);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanPreview6>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a sample plan to load for program " + program);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate a Sample Plan for review.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a sample degree plan and shows how it would look applied to the student's degree plan but does NOT apply the change.
        /// </summary>
        /// <param name="degreePlanId">The degree plan ID</param>
        /// <param name="curriculumTrackCode">The curriculum track code</param>
        /// <param name="termCode">The code for the term at which to start the sample plan</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided id, or curriculumTrackCode are empty/null</exception>
        /// <returns>The DegreePlanPreview DTO</returns>
        [Obsolete("Obsolete as of API version 1.33. Use PreviewSampleDegreePlan8Async.")]
        public async Task<DegreePlanPreview6> PreviewSampleDegreePlan7Async(int degreePlanId, string curriculumTrackCode, string termCode)
        {
            //make sure parameters are legit
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan ID must be provided and be non zero.");
            }
            if (string.IsNullOrEmpty(curriculumTrackCode))
            {
                throw new ArgumentNullException("curriculumTrackCode", "Curriculum Track Code cannot be null or empty");
            }
            
            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "curriculumTrackCode", curriculumTrackCode, "firstTermCode", termCode });
                var urlPath = UrlUtility.CombineUrlPath(new[] { _degreePlansPath, degreePlanId.ToString(), "preview-sample" });
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion7);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanPreview6>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a sample plan to load for curriculum track code " + curriculumTrackCode);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate a Sample Plan for review.");
                throw;
            }
        }

        /// <summary>
        /// Retrieve a sample degree plan (curriculum track) for the given curriculum track code, using the provided term code as the first term.
        /// The student's plan is unchanged in the database.
        /// </summary>
        /// <param name="degreePlanId">The degree plan id of the plan to use as the basis for the plan preview.</param>
        /// <param name="curriculumTrackCode">The code of the curriculum Track from which the sample plan will be derived.</param>
        /// <param name="firstTermCode">The code for the term at which to start the sample plan</param>
        /// <param name="programCode">Academic program to evaluate when considering the student's academic history</param>
        /// <returns>A degree plan preview which contains both a limited preview of courses suggested along with a version of the student's <see cref="DegreePlan4">DegreePlan</see> now including the overlaid sample degree plan.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a sample degree plan is not found, either for the specified program or a default sample</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the degree plan id or programCode is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to view the degree plan</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        public async Task<DegreePlanPreview7> PreviewSampleDegreePlan8Async(int degreePlanId, string curriculumTrackCode, string firstTermCode, string programCode)
        {
            //make sure parameters are legit
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan ID must be provided and be non zero.");
            }
            if (string.IsNullOrEmpty(curriculumTrackCode))
            {
                throw new ArgumentNullException("curriculumTrackCode", "Curriculum Track Code cannot be null or empty");
            }

            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "curriculumTrackCode", curriculumTrackCode, "firstTermCode", firstTermCode, "programCode", programCode });
                var urlPath = UrlUtility.CombineUrlPath(new[] { _degreePlansPath, degreePlanId.ToString(), "preview-sample" });
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion8);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanPreview7>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while previewing degree plan");
                throw;
            }

            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find a sample plan to load for curriculum track code " + curriculumTrackCode);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate a Sample Plan for review.");
                throw;
            }
        }


        /// <summary>
        /// OBSOLETE as f API version 1.2. Use PreviewSampleDegreePlan and UpdateDegreePlan
        /// Attempts to hydrate a student's degree plan with a sample plan from the academic program catalog.
        /// </summary>
        /// <param name="id">The student ID</param>
        /// <param name="program">The program code</param>
        /// <param name="catalog">The catalog ID</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID, Program, or Catalog are empty/null</exception>
        /// <returns>The updated DegreePlan DTO</returns>
        [Obsolete("Obsolete as of API version 1.2")]
        public DegreePlan LoadSampleDegreePlan(string id, string program)
        {
            //make sure parameters are legit
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Student ID cannot be null or empty");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program code cannot be null or empty");
            }

            LoadDegreePlanRequest request = new LoadDegreePlanRequest()
            {
                ProgramCode = program,
                StudentId = id
            };

            string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath, "apply-sample");

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            DegreePlan degreePlan = null;

            var response = ExecutePutRequestWithResponse<LoadDegreePlanRequest>(request, urlPath, headers: headers);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new DegreePlanException() { Code = DegreePlanExceptionCodes.SamplePlanNotFound };
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                degreePlan = JsonConvert.DeserializeObject<DegreePlan>(response.Content.ReadAsStringAsync().Result);
                if (degreePlan == null)
                {
                    throw new InvalidOperationException("Unable to update degree plan.");
                }
            }
            else
            {
                throw new InvalidOperationException("Unable to update degree plan.");
            }

            return degreePlan;
        }

        /// <summary>
        /// Retrieve degree plans submitted for review
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>list of degree plan objects</returns>
        public async Task<IEnumerable<DegreePlanReviewRequest>> QueryReviewRequestedDegreePlans(DegreePlansSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Criteria cannot be empty/null for degree plan query.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString(new[] { "pageSize", pageSize.ToString(), "pageIndex", pageIndex.ToString() });

                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _degreePlansPath);
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<DegreePlanReviewRequest>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve QueryReviewRequestedDegreePlans.");
                throw;
            }
        }

        /// <summary>
        /// Retrieve degree plans submitted for review
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>list of degree plan objects</returns>
        public async Task<IEnumerable<DegreePlanReviewRequest>> QueryReviewRequestedDegreePlansForExactMatchAsync(DegreePlansSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Criteria cannot be empty/null for degree plan query.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString(new[] { "pageSize", pageSize.ToString(), "pageIndex", pageIndex.ToString() });

                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _degreePlansPath);
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeEllucianPersonSearchExactMatchFormat);

                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<DegreePlanReviewRequest>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while retrieving degree plans submitted for review");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve QueryReviewRequestedDegreePlans.");
                throw;
            }
        }

        /// <summary>
        /// Create/Update Degree plan review assignment
        /// </summary>
        /// <param name="degreePlanReviewRequest"></param>
        /// <returns></returns>
        public async Task<DegreePlanReviewRequest> PostReviewRequestedDegreePlan(DegreePlanReviewRequest degreePlanReviewRequest)
        {
            if (degreePlanReviewRequest == null)
            {
                throw new ArgumentNullException("degreePlanReviewRequest", "degreePlanReviewRequest cannot be null ");
            }
            if (string.IsNullOrEmpty(degreePlanReviewRequest.Id))
            {
                throw new ArgumentException("degreePlanReviewRequest must have a degree plan id.");
            }
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync<DegreePlanReviewRequest>(degreePlanReviewRequest, "degree-plan-review-request", headers: headers);
                var resource = JsonConvert.DeserializeObject<DegreePlanReviewRequest>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while create/update of degree plan review assignment");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to create degree plan review assignment.");
                throw;
            }
        }

        /// Creates a new Degree Plan Archive for the given degree plan.
        /// </summary>
        /// <param name="studentId">The degree plan being archived</param>
        /// <exception cref="System.InvalidOperationException">Thrown when a degree plan archive cannot be created for the degree plan</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when there is some other problem</exception>
        /// <returns>The newly-created Degree Plan</returns>
        [Obsolete("Obsolete with API 1.5. Use ArchiveDegreePlan2")]
        public DegreePlanArchive ArchiveDegreePlan(DegreePlan2 degreePlan)
        {
            //make sure degree plan has been provided
            if (degreePlan == null)
            {
                throw new ArgumentNullException("degreePlan", "degreePlan cannot be null");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath);
                urlPath = UrlUtility.CombineUrlPath(urlPath, degreePlan.Id.ToString(), "archive");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = ExecutePostRequestWithResponse<DegreePlan2>(degreePlan, urlPath, headers: headers);
                var archivedDegreePlan = JsonConvert.DeserializeObject<DegreePlanArchive>(response.Content.ReadAsStringAsync().Result);

                return archivedDegreePlan;
            }
            // If the HTTP request fails, the degree plan archive probably wasn't created successfully...
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("Degree Plan Archive creation failed.", degreePlan.Id), hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        [Obsolete("Obsolete with API 1.5. Use ArchiveDegreePlan2Async")]
        public async Task<DegreePlanArchive> ArchiveDegreePlanAsync(DegreePlan2 degreePlan)
        {
            //make sure degree plan has been provided
            if (degreePlan == null)
            {
                throw new ArgumentNullException("degreePlan", "degreePlan cannot be null");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath);
                urlPath = UrlUtility.CombineUrlPath(urlPath, degreePlan.Id.ToString(), "archive");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = await ExecutePostRequestWithResponseAsync<DegreePlan2>(degreePlan, urlPath, headers: headers);
                var archivedDegreePlan = JsonConvert.DeserializeObject<DegreePlanArchive>(await response.Content.ReadAsStringAsync());

                return archivedDegreePlan;
            }
            // If the HTTP request fails, the degree plan archive probably wasn't created successfully...
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("Degree Plan Archive creation failed.", degreePlan.Id), hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        /// Creates a new Degree Plan Archive for the given degree plan.
        /// </summary>
        /// <param name="degreePlan">The degree plan being archived</param>
        /// <exception cref="System.InvalidOperationException">Thrown when a degree plan archive cannot be created for the degree plan</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when there is some other problem</exception>
        /// <returns>The newly-created Degree Plan archive</returns>
        [Obsolete("Obsolete with API 1.7. Use ArchiveDegreePlan3")]
        public DegreePlanArchive2 ArchiveDegreePlan2(DegreePlan3 degreePlan)
        {
            //make sure degree plan has been provided
            if (degreePlan == null)
            {
                throw new ArgumentNullException("degreePlan", "degreePlan cannot be null");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath);
                urlPath = UrlUtility.CombineUrlPath(urlPath, degreePlan.Id.ToString(), "archive");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                var response = ExecutePostRequestWithResponse<DegreePlan3>(degreePlan, urlPath, headers: headers);
                var archivedDegreePlan = JsonConvert.DeserializeObject<DegreePlanArchive2>(response.Content.ReadAsStringAsync().Result);

                return archivedDegreePlan;
            }
            // If the HTTP request fails, the degree plan archive probably wasn't created successfully...
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("Degree Plan Archive creation failed.", degreePlan.Id), hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        /// Creates a new Degree Plan Archive for the given degree plan.
        /// </summary>
        /// <param name="degreePlan">The degree plan being archived</param>
        /// <exception cref="System.InvalidOperationException">Thrown when a degree plan archive cannot be created for the degree plan</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when there is some other problem</exception>
        /// <returns>The newly-created Degree Plan archive</returns>
        [Obsolete("Obsolete with API 1.7. Use ArchiveDegreePlan3Async")]

        public async Task<DegreePlanArchive2> ArchiveDegreePlan2Async(DegreePlan3 degreePlan)
        {
            //make sure degree plan has been provided
            if (degreePlan == null)
            {
                throw new ArgumentNullException("degreePlan", "degreePlan cannot be null");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath);
                urlPath = UrlUtility.CombineUrlPath(urlPath, degreePlan.Id.ToString(), "archive");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                var response = await ExecutePostRequestWithResponseAsync<DegreePlan3>(degreePlan, urlPath, headers: headers);
                var archivedDegreePlan = JsonConvert.DeserializeObject<DegreePlanArchive2>(await response.Content.ReadAsStringAsync());

                return archivedDegreePlan;
            }
            // If the HTTP request fails, the degree plan archive probably wasn't created successfully...
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("Degree Plan Archive creation failed.", degreePlan.Id), hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }
        /// Creates a new Degree Plan Archive for the given degree plan.
        /// </summary>
        /// <param name="degreePlan">The degree plan being archived</param>
        /// <exception cref="System.InvalidOperationException">Thrown when a degree plan archive cannot be created for the degree plan</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when there is some other problem</exception>
        /// <returns>The newly-created Degree Plan archive</returns>

        public DegreePlanArchive2 ArchiveDegreePlan3(DegreePlan4 degreePlan)
        {
            //make sure degree plan has been provided
            if (degreePlan == null)
            {
                throw new ArgumentNullException("degreePlan", "degreePlan cannot be null");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath);
                urlPath = UrlUtility.CombineUrlPath(urlPath, degreePlan.Id.ToString(), "archive");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);

                var response = ExecutePostRequestWithResponse<DegreePlan4>(degreePlan, urlPath, headers: headers);
                var archivedDegreePlan = JsonConvert.DeserializeObject<DegreePlanArchive2>(response.Content.ReadAsStringAsync().Result);

                return archivedDegreePlan;
            }
            // If the HTTP request fails, the degree plan archive probably wasn't created successfully...
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("Degree Plan Archive creation failed.", degreePlan.Id), hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }
        /// Creates a new Degree Plan Archive for the given degree plan async.
        /// </summary>
        /// <param name="degreePlan">The degree plan being archived</param>
        /// <exception cref="System.InvalidOperationException">Thrown when a degree plan archive cannot be created for the degree plan</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when there is some other problem</exception>
        /// <returns>The newly-created Degree Plan archive</returns>
        public async Task<DegreePlanArchive2> ArchiveDegreePlan3Async(DegreePlan4 degreePlan)
        {
            //make sure degree plan has been provided
            if (degreePlan == null)
            {
                throw new ArgumentNullException("degreePlan", "degreePlan cannot be null");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath);
                urlPath = UrlUtility.CombineUrlPath(urlPath, degreePlan.Id.ToString(), "archive");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);

                var response = await ExecutePostRequestWithResponseAsync<DegreePlan4>(degreePlan, urlPath, headers: headers);
                var archivedDegreePlan = JsonConvert.DeserializeObject<DegreePlanArchive2>(await response.Content.ReadAsStringAsync());

                return archivedDegreePlan;
            }
            // If the HTTP request fails, the degree plan archive probably wasn't created successfully...
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("Degree Plan Archive creation failed.", degreePlan.Id), hre);
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while degree plan archive creation");
                throw;
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Gets an Advisor by ID.
        /// </summary>
        /// <param name="advisorId">The ID of the advisor</param>
        /// <returns>The corresponding Advisor record if found, otherwise null</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID is null</exception>
        /// <exception cref="Ellucian.Colleague.Api.Client.Exceptions.AdvisingException">Thrown when the Advisor cannot be retrieved</exception>
        public Advisor GetAdvisor(string advisorId)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException(advisorId, "advisorId cannot be empty/null for advisor retrieval.");
            }

            string urlPath = UrlUtility.CombineUrlPath(_advisorsPath, advisorId);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            Advisor advisor = null;

            try
            {
                AddLoggingRestrictions(ref headers, Core.LoggingRestrictions.DoNotLogResponseContent);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    advisor = JsonConvert.DeserializeObject<Advisor>(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    advisor = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                advisor = null;
            }

            return advisor;
        }

        /// <summary>
        /// Gets an Advisor by ID async.
        /// </summary>
        /// <param name="advisorId">The ID of the advisor</param>
        /// <returns>The corresponding Advisor record if found, otherwise null</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID is null</exception>
        /// <exception cref="Ellucian.Colleague.Api.Client.Exceptions.AdvisingException">Thrown when the Advisor cannot be retrieved</exception>
        public async Task<Advisor> GetAdvisorAsync(string advisorId)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException(advisorId, "advisorId cannot be empty/null for advisor retrieval.");
            }

            string urlPath = UrlUtility.CombineUrlPath(_advisorsPath, advisorId);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            Advisor advisor = null;

            try
            {
                AddLoggingRestrictions(ref headers, Core.LoggingRestrictions.DoNotLogResponseContent);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    advisor = JsonConvert.DeserializeObject<Advisor>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    advisor = null;
                }
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while retrieving an advisor by id");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                advisor = null;
            }

            return advisor;
        }

        /// <summary>
        /// Gets an Advisee for an advisor.
        /// </summary>
        /// <param name="advisorId">The ID of the advisor</param>
        /// <param name="adviseeId">The ID of the advisee</param>
        /// <returns>The Advisee record if found, otherwise null</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID is null</exception>
        /// <exception cref="Ellucian.Colleague.Api.Client.Exceptions.AdvisingException">Thrown when the Advisor cannot be retrieved</exception>
        public Advisee GetAdvisee(string advisorId, string adviseeId)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException(advisorId, "advisorId cannot be empty/null for advisee retrieval.");
            }

            if (string.IsNullOrEmpty(adviseeId))
            {
                throw new ArgumentNullException(adviseeId, "adviseeId cannot be empty/null for advisee retrieval.");
            }

            string urlPath = UrlUtility.CombineUrlPath(_advisorsPath, advisorId);
            urlPath = UrlUtility.CombineUrlPath(urlPath, adviseeId);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
            // Do not log the response body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogResponseContent);

            Advisee advisee = null;

            try
            {
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    advisee = JsonConvert.DeserializeObject<Advisee>(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    advisee = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                advisee = null;
            }

            return advisee;
        }

        /// <summary>
        /// Gets an Advisee for an advisor async.
        /// </summary>
        /// <param name="advisorId">The ID of the advisor</param>
        /// <param name="adviseeId">The ID of the advisee</param>
        /// <returns>The Advisee record if found, otherwise null</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided ID is null</exception>
        /// <exception cref="Ellucian.Colleague.Api.Client.Exceptions.AdvisingException">Thrown when the Advisor cannot be retrieved</exception>
        public async Task<Advisee> GetAdviseeAsync(string advisorId, string adviseeId)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException(advisorId, "advisorId cannot be empty/null for advisee retrieval.");
            }

            if (string.IsNullOrEmpty(adviseeId))
            {
                throw new ArgumentNullException(adviseeId, "adviseeId cannot be empty/null for advisee retrieval.");
            }

            string urlPath = UrlUtility.CombineUrlPath(_advisorsPath, advisorId);
            urlPath = UrlUtility.CombineUrlPath(urlPath, adviseeId);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
            // Do not log the response body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogResponseContent);

            Advisee advisee = null;

            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    advisee = JsonConvert.DeserializeObject<Advisee>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    advisee = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                advisee = null;
            }

            return advisee;
        }

        /// <summary>
        /// Gets the assigned advisees for a particular advisor id Async.
        /// </summary>
        /// <param name="advisorId">Id of the advisor</param>
        /// <param name="pageSize">Number of items to return per page</param>
        /// <param name="pageIndex">Index of the page</param>
        /// <param name="activeAdviseesOnly">Return only active advisees, excluding former and future advisees.</param>
        /// <returns>A list of advisees currently assigned to the advisor, which may be empty</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the advisorId is null or empty</exception>
        /// <exception cref="Ellucian.Colleague.Api.Client.Exceptions.AdvisingException">Thrown when the Advisees cannot be returned</exception>
        public async Task<IEnumerable<Advisee>> GetAdviseesAsync(string advisorId, int pageSize = int.MaxValue, int pageIndex = 1, bool activeAdviseesOnly = false)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException(advisorId, "advisorId cannot be empty/null for advisee retrieval.");
            }

            string query = UrlUtility.BuildEncodedQueryString(new[] { "pageSize", pageSize.ToString(), "pageIndex", pageIndex.ToString(), "activeAdviseesOnly", activeAdviseesOnly.ToString() });

            string urlPath = UrlUtility.CombineUrlPath(_advisorsPath, advisorId, "advisees");
            urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
            // Do not log the response body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogResponseContent);

            IEnumerable<Advisee> advisees = null;
            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    advisees = JsonConvert.DeserializeObject<IEnumerable<Advisee>>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
                }
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while retrieving assigned advisees");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw new AdvisingException(AdvisingExceptionCodes.SearchFailed);
            }

            return advisees;
        }

        /// <summary>
        /// Get a program evaluation
        /// </summary>
        /// <returns>Returns a program evaluation</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public ProgramEvaluation GetProgramEvaluation(string id, string program)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program cannot be empty/null for program results retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "evaluation");
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, UrlUtility.BuildEncodedQueryString("program", program));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ProgramEvaluation>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluation");
                throw;
            }
        }

        /// <summary>
        /// Get a program evaluation async.
        /// </summary>
        /// <returns>Returns a program evaluation</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<ProgramEvaluation> GetProgramEvaluationAsync(string id, string program)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program cannot be empty/null for program results retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "evaluation");
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, UrlUtility.BuildEncodedQueryString("program", program));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ProgramEvaluation>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluation");
                throw;
            }
        }

        /// <summary>
        /// Get a program evaluation async.
        /// </summary>
        /// <returns>Returns a program evaluation</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<ProgramEvaluation2> GetProgramEvaluation2Async(string id, string program)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program cannot be empty/null for program results retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "evaluation");
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, UrlUtility.BuildEncodedQueryString("program", program));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ProgramEvaluation2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluation");
                throw;
            }
        }

        /// <summary>
        /// Get a program evaluation async.
        /// </summary>
        /// <returns><see cref="ProgramEvaluation3"/>Returns a program evaluation</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<ProgramEvaluation3> GetProgramEvaluation3Async(string id, string program, string catalogYear = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program cannot be empty/null for program results retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, _evaluationsPath);
                string query = UrlUtility.BuildEncodedQueryString(new[] { "program", program, "catalogYear", catalogYear });

                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ProgramEvaluation3>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluation");
                throw;
            }
        }

        /// <summary>
        /// Get a program evaluation async.
        /// </summary>
        /// <returns><see cref="ProgramEvaluation4"/>Returns a program evaluation</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<ProgramEvaluation4> GetProgramEvaluation4Async(string id, string program, string catalogYear = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program cannot be empty/null for program results retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, _evaluationsPath);
                string query = UrlUtility.BuildEncodedQueryString(new[] { "program", program, "catalogYear", catalogYear });

                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion4);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ProgramEvaluation4>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while retrieving program evaluation for student: " + id);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluation");
                throw;
            }
        }

        /// <summary>
        /// Get a list of program evaluations for the given student
        /// </summary>
        /// <param name="id">The ID of the student</param>
        /// <param name="programCodes">The list of codes of the Programs to be evaluated</param>
        /// <returns>Returns a list of program evaluations</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<ProgramEvaluation> QueryProgramEvaluations(string id, List<string> programCodes)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (programCodes == null || programCodes.Count() == 0)
            {
                throw new ArgumentNullException("programCodes", "Program Codes cannot be empty/null for program results retrieval.");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _studentsPath, id, "evaluation" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Build url path

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(programCodes, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<ProgramEvaluation>>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluations");
                throw;
            }
        }

        /// <summary>
        /// Get a list of program evaluations for the given student async.
        /// </summary>
        /// <param name="id">The ID of the student</param>
        /// <param name="programCodes">The list of codes of the Programs to be evaluated</param>
        /// <returns>Returns a list of program evaluations</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ProgramEvaluation>> QueryProgramEvaluationsAsync(string id, List<string> programCodes)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (programCodes == null || programCodes.Count() == 0)
            {
                throw new ArgumentNullException("programCodes", "Program Codes cannot be empty/null for program results retrieval.");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _studentsPath, id, "evaluation" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Build url path

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(programCodes, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<ProgramEvaluation>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluations");
                throw;
            }
        }

        /// <summary>
        /// Get a list of program evaluations for the given student async.
        /// </summary>
        /// <param name="id">The ID of the student</param>
        /// <param name="programCodes">The list of codes of the Programs to be evaluated</param>
        /// <returns>Returns a list of program evaluations</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ProgramEvaluation2>> QueryProgramEvaluations2Async(string id, List<string> programCodes)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (programCodes == null || programCodes.Count() == 0)
            {
                throw new ArgumentNullException("programCodes", "Program Codes cannot be empty/null for program results retrieval.");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _studentsPath, id, "evaluation" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Build url path

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecutePostRequestWithResponseAsync(programCodes, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<ProgramEvaluation2>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluations");
                throw;
            }
        }

        /// <summary>
        /// Get a list of program evaluations for the given student async.
        /// </summary>
        /// <param name="id">The ID of the student</param>
        /// <param name="programCodes">The list of codes of the Programs to be evaluated</param>
        /// <returns><see cref="ProgramEvaluation3"/>Returns a list of program evaluations</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ProgramEvaluation3>> QueryProgramEvaluations3Async(string id, List<string> programCodes)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (programCodes == null || programCodes.Count() == 0)
            {
                throw new ArgumentNullException("programCodes", "Program Codes cannot be empty/null for program results retrieval.");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _studentsPath, id, "evaluation" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Build url path

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecutePostRequestWithResponseAsync(programCodes, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<ProgramEvaluation3>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluations");
                throw;
            }
        }

        /// <summary>
        /// Get a list of program evaluations for the given student async.
        /// </summary>
        /// <param name="id">The ID of the student</param>
        /// <param name="programCodes">The list of codes of the Programs to be evaluated</param>
        /// <returns><see cref="ProgramEvaluation4"/>Returns a list of program evaluations</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ProgramEvaluation4>> QueryProgramEvaluations4Async(string id, List<string> programCodes)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (programCodes == null || programCodes.Count() == 0)
            {
                throw new ArgumentNullException("programCodes", "Program Codes cannot be empty/null for program results retrieval.");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _studentsPath, id, "evaluation" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Build url path

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion4);
                var response = await ExecutePostRequestWithResponseAsync(programCodes, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<ProgramEvaluation4>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluations");
                throw;
            }
        }

        /// <summary>
        /// For the given student and program, returns the appropriate notice text.
        /// </summary>
        /// <param name="studentId">The ID of the student</param>
        /// <param name="programCode">The code of the Program</param>
        /// <returns>List of <see cref="EvaluationNotice">Evaluation Notices</see></returns>
        public IEnumerable<EvaluationNotice> GetProgramEvaluationNotices(string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null for student program notice retrieval.");
            }
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program Code cannot be empty/null for student program notice retrieval.");
            }
            try
            {
                // url utility encode with substitution instead of build encoded query string and re-do api doc
                var urlPath = UrlUtility.CombineUrlPath(new[] { _studentsPath, studentId, "evaluation-notices", UrlParameterUtility.EncodeWithSubstitution(programCode) });
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<EvaluationNotice>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get EvaluationNotices");
                throw;
            }
        }

        /// <summary>
        /// For the given student and program, returns the appropriate notice text async.
        /// </summary>
        /// <param name="studentId">The ID of the student</param>
        /// <param name="programCode">The code of the Program</param>
        /// <returns>List of <see cref="EvaluationNotice">Evaluation Notices</see></returns>
        public async Task<IEnumerable<EvaluationNotice>> GetProgramEvaluationNoticesAsync(string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null for student program notice retrieval.");
            }
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program Code cannot be empty/null for student program notice retrieval.");
            }
            try
            {
                // url utility encode with substitution instead of build encoded query string and re-do api doc
                var urlPath = UrlUtility.CombineUrlPath(new[] { _studentsPath, studentId, "evaluation-notices", UrlParameterUtility.EncodeWithSubstitution(programCode) });
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<EvaluationNotice>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while retrieving evaluation notices for student: " + studentId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get EvaluationNotices");
                throw;
            }
        }

        /// <summary>
        /// Get all degree plan archives for a specified degree plan Id.
        /// </summary>
        /// <returns>Returns a list of degree plan archive DTOs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete with API 1.5. Use GetArchivedPlans2")]
        public IEnumerable<DegreePlanArchive> GetArchivedPlans(string degreePlanId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath, degreePlanId);
                urlPath = UrlUtility.CombineUrlPath(urlPath, "archives");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<DegreePlanArchive>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Degree Plan Archives");
                throw;
            }
        }

        /// <summary>
        /// Get all degree plan archives for a specified degree plan Id async.
        /// </summary>
        /// <returns>Returns a list of degree plan archive DTOs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete with API 1.5. Use GetArchivedPlans2Async")]
        public async Task<IEnumerable<DegreePlanArchive>> GetArchivedPlansAsync(string degreePlanId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath, degreePlanId);
                urlPath = UrlUtility.CombineUrlPath(urlPath, "archives");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<DegreePlanArchive>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Degree Plan Archives");
                throw;
            }
        }

        /// <summary>
        /// Get all degree plan archives for a specified degree plan Id.
        /// </summary>
        /// <returns>Returns a list of <see cref="DegreePlanArchive2">degree plan archive</see> DTOs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<DegreePlanArchive2> GetArchivedPlans2(string degreePlanId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath, degreePlanId);
                urlPath = UrlUtility.CombineUrlPath(urlPath, "archives");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<DegreePlanArchive2>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Degree Plan Archives");
                throw;
            }
        }

        /// <summary>
        /// Get all degree plan archives for a specified degree plan Id async.
        /// </summary>
        /// <returns>Returns a list of <see cref="DegreePlanArchive2">degree plan archive</see> DTOs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<DegreePlanArchive2>> GetArchivedPlans2Async(string degreePlanId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlansPath, degreePlanId);
                urlPath = UrlUtility.CombineUrlPath(urlPath, "archives");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<DegreePlanArchive2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception occurred while retrieving archived plans");
                throw;
            }

            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Degree Plan Archives");
                throw;
            }
        }
        
        /// <summary>
        /// Get plan archive report for a specified archive Id.
        /// </summary>
        /// <returns>Returns a degree plan archive report</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public byte[] GetArchivedPlanReport(string degreePlanArchiveId, out string fileName)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlanArchivesPath, degreePlanArchiveId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add("Accept", "application/pdf");
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                fileName = response.Content.Headers.ContentDisposition.FileName;
                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Degree Plan Archive Report");
                throw;
            }
        }

        /// <summary>
        /// Get plan archive report for a specified archive Id async.
        /// </summary>
        /// <returns>Returns a degree plan archive report</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<Tuple<byte[], string>> GetArchivedPlanReportAsync(string degreePlanArchiveId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreePlanArchivesPath, degreePlanArchiveId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add("Accept", "application/pdf");
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var fileName = response.Content.Headers.ContentDisposition.FileName;
                var resource = await response.Content.ReadAsByteArrayAsync();
                return new Tuple<byte[], string>(resource, fileName);
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while retrieving archived plan report");
                throw;
            }

            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Degree Plan Archive Report");
                throw;
            }
        }

        /// <summary>
        /// Searches for advisees within an advisor's assigned advisee list or within the global pool (depending on advisor's permissions).
        /// </summary>
        /// <param name="adviseeKeyword">The search string used when searching advisees by name or ID</param>
        /// <param name="advisorKeyword">The search string used when searching advisees by advisor name or Id. Either an advisee keyword or an advisorKeyword must be supplied but not both.</param>
        /// <returns>A list of matching student advisees, which may be empty</returns>
        /// <exception cref="System.ArgumentException">Thrown when neither adviseeKeyword nor advisorKeyword is supplied. Must provide one.</exception>
        /// <exception cref="Ellucian.Colleague.Api.Client.Exceptions.AdvisingException">Thrown when the Advisee search fails</exception>
        public IEnumerable<Advisee> QueryAdvisees(string adviseeKeyword, string advisorKeyword, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            if (string.IsNullOrEmpty(adviseeKeyword) && string.IsNullOrEmpty(advisorKeyword))
            {
                throw new ArgumentException("Must provide either adviseeKeyword or advisorKeyword.");
            }
            if (!string.IsNullOrEmpty(adviseeKeyword) && !string.IsNullOrEmpty(advisorKeyword))
            {
                throw new ArgumentException("Must provide either adviseeKeyword or advisorKeyword but not both.");
            }
            string query = UrlUtility.BuildEncodedQueryString(new[] { "pageSize", pageSize.ToString(), "pageIndex", pageIndex.ToString() });

            string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _adviseesPath);
            urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            IEnumerable<Advisee> advisees = null;
            var criteria = new AdviseeSearchCriteria();
            criteria.AdviseeKeyword = adviseeKeyword;
            criteria.AdvisorKeyword = advisorKeyword;

            // Do not log the response body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogResponseContent);

            try
            {
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    advisees = JsonConvert.DeserializeObject<IEnumerable<Advisee>>(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
                }

                return advisees;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw new AdvisingException(AdvisingExceptionCodes.SearchFailed, ex);
            }
        }

        /// <summary>
        /// Searches for advisees within an advisor's assigned advisee list or within the global pool (depending on advisor's permissions) async.
        /// </summary>
        /// <param name="adviseeKeyword">The search string used when searching advisees by name or ID</param>
        /// <param name="advisorKeyword">The search string used when searching advisees by advisor name or Id. Either an advisee keyword or an advisorKeyword must be supplied but not both.</param>
        /// <returns>A list of matching student advisees, which may be empty</returns>
        /// <exception cref="System.ArgumentException">Thrown when neither adviseeKeyword nor advisorKeyword is supplied. Must provide one.</exception>
        /// <exception cref="Ellucian.Colleague.Api.Client.Exceptions.AdvisingException">Thrown when the Advisee search fails</exception>
        public async Task<IEnumerable<Advisee>> QueryAdviseesAsync(string adviseeKeyword, string advisorKeyword, int pageSize = int.MaxValue, int pageIndex = 1, bool includeActiveAdviseesOnly = false)
        {
            if (string.IsNullOrEmpty(adviseeKeyword) && string.IsNullOrEmpty(advisorKeyword))
            {
                throw new ArgumentException("Must provide either adviseeKeyword or advisorKeyword.");
            }
            if (!string.IsNullOrEmpty(adviseeKeyword) && !string.IsNullOrEmpty(advisorKeyword))
            {
                throw new ArgumentException("Must provide either adviseeKeyword or advisorKeyword but not both.");
            }
            string query = UrlUtility.BuildEncodedQueryString(new[] { "pageSize", pageSize.ToString(), "pageIndex", pageIndex.ToString() });

            string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _adviseesPath);
            urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            IEnumerable<Advisee> advisees = null;
            var criteria = new AdviseeSearchCriteria();
            criteria.AdviseeKeyword = adviseeKeyword;
            criteria.AdvisorKeyword = advisorKeyword;
            criteria.IncludeActiveAdviseesOnly = includeActiveAdviseesOnly;
            // Do not log the response body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogResponseContent);

            try
            {
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    advisees = JsonConvert.DeserializeObject<IEnumerable<Advisee>>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
                }

                return advisees;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw new AdvisingException(AdvisingExceptionCodes.SearchFailed, ex);
            }
        }

        /// <summary>
        /// Searches for advisees within an advisor's assigned advisee list or within the global pool (depending on advisor's permissions) async.
        /// </summary>
        /// <param name="adviseeKeyword">The search string used when searching advisees by name or ID</param>
        /// <param name="advisorKeyword">The search string used when searching advisees by advisor name or Id. Either an advisee keyword or an advisorKeyword must be supplied but not both.</param>
        /// <returns>A list of matching student advisees, which may be empty</returns>
        /// <exception cref="System.ArgumentException">Thrown when neither adviseeKeyword nor advisorKeyword is supplied. Must provide one.</exception>
        /// <exception cref="Ellucian.Colleague.Api.Client.Exceptions.AdvisingException">Thrown when the Advisee search fails</exception>
        public async Task<IEnumerable<Advisee>> QueryAdviseesForExactMatchAsync(string adviseeKeyword, string advisorKeyword, int pageSize = int.MaxValue, int pageIndex = 1, bool includeActiveAdviseesOnly = false)
        {
            if (string.IsNullOrEmpty(adviseeKeyword) && string.IsNullOrEmpty(advisorKeyword))
            {
                throw new ArgumentException("Must provide either adviseeKeyword or advisorKeyword.");
            }
            if (!string.IsNullOrEmpty(adviseeKeyword) && !string.IsNullOrEmpty(advisorKeyword))
            {
                throw new ArgumentException("Must provide either adviseeKeyword or advisorKeyword but not both.");
            }
            string query = UrlUtility.BuildEncodedQueryString(new[] { "pageSize", pageSize.ToString(), "pageIndex", pageIndex.ToString() });

            string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _adviseesPath);
            urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeEllucianPersonSearchExactMatchFormat);

            IEnumerable<Advisee> advisees = null;
            var criteria = new AdviseeSearchCriteria();
            criteria.AdviseeKeyword = adviseeKeyword;
            criteria.AdvisorKeyword = advisorKeyword;
            criteria.IncludeActiveAdviseesOnly = includeActiveAdviseesOnly;
            // Do not log the response body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogResponseContent);

            try
            {
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    advisees = JsonConvert.DeserializeObject<IEnumerable<Advisee>>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
                }

                return advisees;
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while searching advisees");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw new AdvisingException(AdvisingExceptionCodes.SearchFailed, ex);
            }
        }

        /// <summary>
        /// Request for a list of advisors by Advisor Id
        /// </summary>
        /// <param name="advisorIds"></param>
        /// <returns></returns>
        public IEnumerable<Advisor> QueryAdvisors(List<string> advisorIds, bool onlyActiveAdvisees = false)
        {
            if (advisorIds == null || advisorIds.Count() == 0)
            {
                throw new ArgumentException("advisorIds", "Must provide a list of advisors.");
            }
            AdvisorQueryCriteria criteria = new AdvisorQueryCriteria() { OnlyActiveAdvisees = onlyActiveAdvisees };
            criteria.AdvisorIds = advisorIds;
            try
            {
                // Build url path from qapi path and advisors path
                string[] pathStrings = new string[] { _qapiPath, _advisorsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Advisor>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Request for a list of advisors by Advisor Id async
        /// </summary>
        /// <param name="advisorIds"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as API 1.19, use QueryAdvisors2Async instead.")]
        public async Task<IEnumerable<Advisor>> QueryAdvisorsAsync(List<string> advisorIds)
        {
            if (advisorIds == null || advisorIds.Count() == 0)
            {
                throw new ArgumentException("advisorIds", "Must provide a list of advisors.");
            }
            AdvisorQueryCriteria criteria = new AdvisorQueryCriteria();
            criteria.AdvisorIds = advisorIds;
            try
            {
                // Build url path from qapi path and advisors path
                string[] pathStrings = new string[] { _qapiPath, _advisorsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Advisor>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Retrieves basic advisor information for a the given Advisor query criteria (list of advisor ids). 
        /// Use for name identification only. Does not confirm authorization to perform as an advisor.
        /// This is intended to retrieve merely reference information (name, email) for any person who may have currently 
        /// or previously performed the functions of an advisor. If a specified ID not found to be a potential advisor,
        /// does not cause an exception, item is simply not returned in the list.
        /// </summary>
        /// <param name="advisorIds">Advisor IDs for whom data will be retrieved</param>
        /// <returns>A list of <see cref="Advisor">Advisor</see> objects containing advisor name</returns>
        public async Task<IEnumerable<Advisor>> QueryAdvisors2Async(List<string> advisorIds)
        {
            if (advisorIds == null || !advisorIds.Any())
            {
                throw new ArgumentNullException("advisorIds", "At least one advisor ID must be provided to search for advisors.");
            }
            try
            {
                // Build url path from qapi path and advisors path
                string[] pathStrings = new string[] { _qapiPath, _advisorsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecutePostRequestWithResponseAsync(advisorIds, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Advisor>>(await response.Content.ReadAsStringAsync());
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception occurred while querying advisors");
                throw;
            }

            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve advisor data.");
                throw;
            }
        }

        /// <summary>
        /// Get a planning student by ID
        /// </summary>
        /// <returns>Returns a planning student</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public PlanningStudent GetPlanningStudent(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for PlanningStudent retrieval.");
            }
            try
            {
                string[] pathStrings = new string[] { _studentsPath, id };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPlanningVersion1);
                AddLoggingRestrictions(ref headers, Core.LoggingRestrictions.DoNotLogResponseContent);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<PlanningStudent>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get PlanningStudent");
                throw;
            }
        }

        /// <summary>
        /// query planning students.
        /// </summary>
        /// <param name="studentIds"></param>
        /// <returns></returns>
        public IEnumerable<PlanningStudent> QueryPlanningStudents(IEnumerable<string> studentIds)
        {
            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Student retrieval.");
            }
            try
            {
                PlanningStudentCriteria criteria = new PlanningStudentCriteria();
                criteria.StudentIds = studentIds;
                // Build url path from qapi path and student path
                string[] pathStrings = new string[] { _qapiPath, _studentsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPlanningVersion1);
                AddLoggingRestrictions(ref headers, Core.LoggingRestrictions.DoNotLogResponseContent);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PlanningStudent>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve students.");
                throw;
            }
        }

        /// <summary>
        /// query planning students asynchronously
        /// </summary>
        /// <param name="studentIds"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PlanningStudent>> QueryPlanningStudentsAsync(IEnumerable<string> studentIds)
        {
            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Student retrieval.");
            }
            try
            {
                PlanningStudentCriteria criteria = new PlanningStudentCriteria();
                criteria.StudentIds = studentIds;
                // Build url path from qapi path and student path
                string[] pathStrings = new string[] { _qapiPath, _studentsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPlanningVersion1);
                AddLoggingRestrictions(ref headers, Core.LoggingRestrictions.DoNotLogResponseContent);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PlanningStudent>>(await response.Content.ReadAsStringAsync());
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while retrieving planning students");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve students.");
                throw;
            }
        }

        /// <summary>
        /// Gets the set of permissions for the active advisor. This can only be run for the logged-in advisor, and not for any advisor by ID.
        /// If the currently-logged-in user is not an advisor then an AdvisingException with the proper code will be thrown.
        /// </summary>
        /// <returns>A set of strings enumerating the set of permissions allowed for the currently-logged-in advisor</returns>
        [Obsolete("Obsolete as of Colleague Web API 1.21. Use GetAdvisingPermissions2Async")]
        public async Task<IEnumerable<string>> GetAdvisorPermissionsAsync()
        {
            string urlPath = UrlUtility.CombineUrlPath(_advisorsPath, "permissions");

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            IEnumerable<string> permissions = null;

            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    permissions = JsonConvert.DeserializeObject<IEnumerable<string>>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw new AdvisingException(AdvisingExceptionCodes.UnauthorizedAdvisor);
            }

            return permissions;
        }

        /// <summary>
        /// Returns the advising permissions for the authenticated user.
        /// </summary>
        /// <returns>Advising permissions for the authenticated user.</returns>
        public async Task<AdvisingPermissions> GetAdvisingPermissions2Async()
        {
            string urlPath = UrlUtility.CombineUrlPath(_advisorsPath, "permissions");

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            AdvisingPermissions permissions = null;

            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                permissions = JsonConvert.DeserializeObject<AdvisingPermissions>(response.Content.ReadAsStringAsync().Result);
                return permissions;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while retrieving advising permissions.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the configuration information for Student Planning.
        /// </summary>
        /// <returns>The <see cref="PlanningConfiguration">Student Planning configuration</see> data</returns>
        public async Task<PlanningConfiguration> GetPlanningConfigurationAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_configurationPath, _planningPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<PlanningConfiguration>(responseString.Content.ReadAsStringAsync().Result);

                return configuration;
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while retrieving planning configuration");
                throw;
            }

            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student planning configuration data.");
                throw;
            }
        }

        /// <summary>
        /// Posts a <see cref="Dtos.Planning.CompletedAdvisement">completed advisement</see>
        /// </summary>
        /// <param name="studentId">ID of the student whose advisement is being marked complete</param>
        /// <param name="completeAdvisement">A <see cref="Dtos.Planning.CompletedAdvisement">completed advisement</see></param>
        /// <returns>An <see cref="Dtos.Planning.Advisee">advisee</see></returns>
        public async Task<Advisee> PostCompletedAdvisementAsync(string studentId, CompletedAdvisement completeAdvisement)
        {
            if (studentId == null)
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null when posting completed advisements.");
            }
            try
            {
                string[] pathStrings = new string[] { _studentsPath, studentId, _advisementsComplete };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                // Do not log the response body
                AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogResponseContent);

                var response = await ExecutePostRequestWithResponseAsync(completeAdvisement, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<Advisee>(await response.Content.ReadAsStringAsync());
            }
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while posting completed advisement for student " + studentId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to post completed advisement for student " + studentId);
                throw;
            }
        }

        /// <summary>
        /// Applicant's evaluation result
        /// </summary>
        /// <param name="id">id of an applicant</param>
        /// <param name="programCodes">list of programs codes to evaluate</param>
        /// <returns></returns>
        public async Task<IEnumerable<ProgramEvaluation4>> QueryApplicantEvaluationsAsync(string id, List<string> programCodes)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (programCodes == null || programCodes.Count() == 0)
            {
                throw new ArgumentNullException("programCodes", "Program Codes cannot be empty/null for program results retrieval.");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _applicantPath, id, "evaluation" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Build url path

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(programCodes, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<ProgramEvaluation4>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluations for the applicant:" + id);
                throw;
            }
        }

        /// <summary>
        /// Get a program evaluation async.
        /// </summary>
        /// <returns><see cref="ProgramEvaluation4"/>Returns a program evaluation</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<ProgramEvaluation4> GetApplicantProgramEvaluationAsync(string id, string program, string catalogYear = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Applicant ID cannot be empty/null for ProgramEvaluation retrieval.");
            }
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "Program cannot be empty/null for program results retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_applicantPath, id, _evaluationsPath);
                string query = UrlUtility.BuildEncodedQueryString(new[] { "program", program, "catalogYear", catalogYear });

                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ProgramEvaluation4>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while retrieving program evaluation for student: " + id);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramEvaluation for an applicant");
                throw;
            }
        }

        /// <summary>
        /// Get the academic credits for the provided applicant
        /// </summary>
        /// <returns>Returns the list of  academic credits </returns>
        /// <param name="id">The applicant's ID for whom academic credits are being requested</param>
        /// <param name="filterCredits">(Optional) used to filter to active credit only.</param>
        /// <param name="includeDrops">to include dropped courses or not</param>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ApplicantAcademicCredit>> GetApplicantAcademicCreditsAsync(string id, bool filterCredits = true, bool includeDrops = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Academic History retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_applicantPath, id, "academic-credits");
                var queryString = UrlUtility.BuildEncodedQueryString("filterCredits", filterCredits.ToString(), "includeDrops", includeDrops.ToString());
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers:headers);
                return JsonConvert.DeserializeObject<IEnumerable<ApplicantAcademicCredit>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (LoginException lex)
            {
                logger.Error(lex, "Timeout exception has occurred while retrieving applicant's academic history.");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get applicant's Academic History");
                throw;
            }
        }

        /// <summary>
        /// Get applicant 's programs
        /// Include Inactive programs flag works in conjunctions with currentOnly flag such as:
        /// When includeInactivePrograms is set to true then only those inactive programs that are not yet ended will be included if currentOnly flag is true otherwise all inactive programs will be included.
        /// When includeInactivePrograms is set to false but currentOnly is true then it means only those inactive programs will be included that are in past ended
        /// </summary>
        /// <returns>Returns the set of student's programs</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ApplicantStudentProgram>> GetApplicantProgramsAsync(string applicantId, bool includeInactivePrograms = false, bool currentOnly = true)
        {
            
            if (applicantId == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<StudentProgram2> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student programs path
                string urlPath = UrlUtility.CombineUrlPath(_applicantPath, applicantId, "programs");
                var queryString = UrlUtility.BuildEncodedQueryString("includeInactivePrograms", includeInactivePrograms.ToString(), "currentOnly", currentOnly.ToString());
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<ApplicantStudentProgram>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve applicant's  programs.");
                throw;
            }
        }
    }
}