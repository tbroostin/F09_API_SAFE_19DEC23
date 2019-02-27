// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Client.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Rest.Client.Exceptions;
using Ellucian.Web.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client
{
    public partial class ColleagueApiClient
    {
        /// <summary>
        /// Retrieves faculty members by ID.
        /// </summary>
        /// <param name="id">The IDs of the faculty members</param>
        /// <returns>The Faculty records with the specified IDs</returns>
        public Faculty GetFaculty(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Faculty retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_facultyPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Faculty>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Faculty");
                throw;
            }
        }
        /// <summary>
        /// Retrieves faculty members by ID async.
        /// </summary>
        /// <param name="id">The IDs of the faculty members</param>
        /// <returns>The Faculty records with the specified IDs</returns>
        public async Task<Faculty> GetFacultyAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Faculty retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_facultyPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Faculty>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Faculty");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Faculty Objects without cache based on a Post transaction with multiple keys
        /// </summary>
        /// <param name="courseIds">Post in Body a list of faculty keys</param>
        /// <returns>list of faculty objects</returns>
        public IEnumerable<Faculty> QueryFaculty(FacultyQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Criteria cannot be empty/null for Faculty query.");
            }
            try
            {
                // Build url path from qapi path and faculty path
                string[] pathStrings = new string[] { _qapiPath, _facultyPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Faculty>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve faculty.");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Faculty Objects without cache based on a Post transaction with multiple keys async
        /// </summary>
        /// <param name="courseIds">Post in Body a list of faculty keys</param>
        /// <returns>list of faculty objects</returns>
        public async Task<IEnumerable<Faculty>> QueryFacultyAsync(FacultyQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Criteria cannot be empty/null for Faculty query.");
            }
            try
            {
                // Build url path from qapi path and faculty path
                string[] pathStrings = new string[] { _qapiPath, _facultyPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Faculty>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve faculty.");
                throw;
            }
        }
        /// <summary>
        /// Return a list of Faculty keys for either Advisors, Faculty or both.
        /// </summary>
        /// <returns>List of Faculty IDs</returns>
        public IEnumerable<string> SearchFacultyIds(bool includeFacultyOnly = false, bool includeAdvisorOnly = true)
        {
            try
            {
                var criteria = new FacultyQueryCriteria();
                criteria.IncludeFacultyOnly = includeFacultyOnly;
                criteria.IncludeAdvisorOnly = includeAdvisorOnly;

                string[] pathStrings = new string[] { _qapiPath, _facultyIdsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                IEnumerable<string> studentIds = null;
                studentIds = JsonConvert.DeserializeObject<IEnumerable<string>>(response.Content.ReadAsStringAsync().Result);
                return studentIds;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Faculty IDs");
                throw;
            }
        }
        /// <summary>
        /// Return a list of Faculty keys for either Advisors, Faculty or both async.
        /// </summary>
        /// <returns>List of Faculty IDs</returns>
        public async Task<IEnumerable<string>> SearchFacultyIdsAsync(bool includeFacultyOnly = false, bool includeAdvisorOnly = true)
        {
            try
            {
                var criteria = new FacultyQueryCriteria();
                criteria.IncludeFacultyOnly = includeFacultyOnly;
                criteria.IncludeAdvisorOnly = includeAdvisorOnly;

                string[] pathStrings = new string[] { _qapiPath, _facultyIdsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                IEnumerable<string> studentIds = null;
                studentIds = JsonConvert.DeserializeObject<IEnumerable<string>>(await response.Content.ReadAsStringAsync());
                return studentIds;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Faculty IDs");
                throw;
            }
        }
        /// <summary>
        /// Gets all grades out of the repository. Grade objects have an ID which maps to a grade such as &quot;A&quot;, along with a few other properties.
        /// </summary>
        /// <returns>A collection of all defined grades</returns>
        public IEnumerable<Grade> GetGrades()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_gradesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Grade>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Grade>");
                throw;
            }
        }
        /// <summary>
        /// Gets all grades out of the repository. Grade objects have an ID which maps to a grade such as &quot;A&quot;, along with a few other properties async.
        /// </summary>
        /// <returns>A collection of all defined grades</returns>
        public async Task<IEnumerable<Grade>> GetGradesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_gradesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Grade>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Grade>");
                throw;
            }
        }

        /// <summary>
        /// Gets grades for multiple students in a Pilot-specific format
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="term">An optional term</param>
        /// <returns>A collection of PilotGrades</returns>
        public IEnumerable<PilotGrade> GetPilotGrades(IEnumerable<string> studentIds, string term = null)
        {

            GradeQueryCriteria criteria = new GradeQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Grade retrieval.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _gradesPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPilotVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PilotGrade>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve grades.");
                throw;
            }
        }

        /// <summary>
        /// Gets grades for multiple students in a Pilot-specific format, asyncronously
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="term">An optional term</param>
        /// <returns>A collection of PilotGrades</returns>
        public async Task<IEnumerable<PilotGrade>> GetPilotGradesAsync(IEnumerable<string> studentIds, string term = null)
        {
            GradeQueryCriteria criteria = new GradeQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for grade retrieval.");
            }
            try
            {
                // Build url path from qapi path and grades path
                string[] pathStrings = new string[] { _qapiPath, _gradesPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPilotVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PilotGrade>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve grades.");
                throw;
            }
        }

        /// <summary>
        /// Gets student terms GPA for multiple students for Pilot
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="term">term code</param>
        /// <returns>A collection of student term GPA by academic term/level</returns>
        public IEnumerable<PilotStudentTermLevelGpa> GetPilotStudentTermsGpas(IEnumerable<string> studentIds, string term)
        {
            StudentTermsQueryCriteria criteria = new StudentTermsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<PilotStudentTermLevelGpa> retrieval.");
            }
            if (term == null)
            {
                throw new ArgumentNullException("term", "Term cannot be empty/null for IEnumerable<PilotStudentTermLevelGpa> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student terms GPA path
                string[] pathStrings = new string[] { _qapiPath, _studentTermsGpaPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPilotVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PilotStudentTermLevelGpa>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student term GPAs.");
                throw;
            }
        }

        /// <summary>
        /// Gets student terms GPA for multiple students for Pilot, asyncronously
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="term">term code</param>
        /// <returns>A collection of student term GPA by academic term/level</returns>
        public async Task<IEnumerable<PilotStudentTermLevelGpa>> GetPilotStudentTermsGpasAsync(IEnumerable<string> studentIds, string term)
        {
            StudentTermsQueryCriteria criteria = new StudentTermsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<PilotStudentTermLevelGpa> retrieval.");
            }
            if (term == null)
            {
                throw new ArgumentNullException("term", "Term cannot be empty/null for IEnumerable<PilotStudentTermLevelGpa> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student terms GPA path
                string[] pathStrings = new string[] { _qapiPath, _studentTermsGpaPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPilotVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PilotStudentTermLevelGpa>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student term GPAs.");
                throw;
            }
        }

        /// <summary>
        /// Get a book by ID
        /// </summary>
        /// <returns>Returns the book</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public Book GetBook(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Book retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_bookPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Book>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Book");
                throw;
            }
        }
        /// <summary>
        /// Get a book by ID async
        /// </summary>
        /// <returns>Returns the book</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<Book> GetBookAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Book retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_bookPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Book>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Book");
                throw;
            }
        }

        /// <summary>
        /// Get books by Ids or by query string
        /// </summary>
        /// <param name="ids">IDs of books to be retrieved</param>
        /// <param name="queryString">Query string to be used for searches against book titles, authors, and International Standard Book Numbers (ISBN)</param>
        /// <param name="useCache">Boolean indicates whether to get cached books or books from database</param>
        /// <returns>Returns books for the specified Ids - if a book is not found it will not be in the list of results  </returns>
        public async Task<IEnumerable<Book>> QueryBooksByPostAsync(List<string> ids, string queryString, bool useCache = true)
        {
            if (string.IsNullOrEmpty(queryString) && (ids == null))
            {
                throw new ArgumentException("Must provide either list of ids or a query string. ");
            }
            try
            {
                // Build url path from qapi path and books path
                string[] pathStrings = new string[] { _qapiPath, _bookPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var criteria = new BookQueryCriteria() { Ids = ids, QueryString = queryString };
                // Use URL path and criteria to call web api method
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers, useCache: useCache);

                return JsonConvert.DeserializeObject<IEnumerable<Book>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve requested books.");
                throw;
            }
        }

        /// <summary>
        /// Get the academic history for the provided student
        /// </summary>
        /// <returns>Returns the academic history Version 2</returns>
        /// <param name="id">The student's ID for whom academic history is being requested</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public AcademicHistory2 GetAcademicHistory2(string id, bool bestFit = false, bool filter = true, string term = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Academic History retrieval.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString("bestFit", bestFit.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "academic-credits");
                var queryString = UrlUtility.BuildEncodedQueryString("bestFit", bestFit.ToString(), "filter", filter.ToString(), "term", term);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AcademicHistory2>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Academic History");
                throw;
            }
        }
        /// <summary>
        /// Get the academic history for the provided student async
        /// </summary>
        /// <returns>Returns the academic history Version 2</returns>
        /// <param name="id">The student's ID for whom academic history is being requested</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.11. Use GetAcademicHistory3Async.")]
        public async Task<AcademicHistory2> GetAcademicHistory2Async(string id, bool bestFit = false, bool filter = true, string term = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Academic History retrieval.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString("bestFit", bestFit.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "academic-credits");
                var queryString = UrlUtility.BuildEncodedQueryString("bestFit", bestFit.ToString(), "filter", filter.ToString(), "term", term);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AcademicHistory2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Academic History");
                throw;
            }
        }

        /// <summary>
        /// Get the academic history for the provided student async
        /// </summary>
        /// <returns>Returns the academic history Version 3</returns>
        /// <param name="id">The student's ID for whom academic history is being requested</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.18. Use GetAcademicHistory4Async.")]
        public async Task<AcademicHistory3> GetAcademicHistory3Async(string id, bool bestFit = false, bool filter = true, string term = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Academic History retrieval.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString("bestFit", bestFit.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "academic-credits");
                var queryString = UrlUtility.BuildEncodedQueryString("bestFit", bestFit.ToString(), "filter", filter.ToString(), "term", term);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AcademicHistory3>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Academic History");
                throw;
            }
        }

        /// <summary>
        /// Get the academic history for the provided student async
        /// </summary>
        /// <returns>Returns the academic history Version 3</returns>
        /// <param name="id">The student's ID for whom academic history is being requested</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <param name="includeDrops">(Optional) used to include dropped academic credits</param>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<AcademicHistory4> GetAcademicHistory4Async(string id, bool bestFit = false, bool filter = true, string term = null, bool includeDrops = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Academic History retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "academic-credits");
                var queryString = UrlUtility.BuildEncodedQueryString("bestFit", bestFit.ToString(), "filter", filter.ToString(), "term", term, "includeDrops", includeDrops.ToString());
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion4);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AcademicHistory4>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Academic History");
                throw;
            }
        }


        /// <summary>
        /// Get the academic history for the provided student
        /// </summary>
        /// <returns>Returns the academic history</returns>
        /// <param name="id">The student's ID for whom academic history is being requested</param>
        /// <param name="bestFit">Boolean flag that indicates if non-term based credit should be placed in the closest appropriate term </param>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.5. Use GetAcademicHistory2.")]
        public AcademicHistory GetAcademicHistory(string id, bool bestFit = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for AcademicHistory retrieval.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString("bestFit", bestFit.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "academic-credits");
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AcademicHistory>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get AcademicHistory");
                throw;
            }
        }
        /// <summary>
        /// Get the academic history for the provided student
        /// </summary>
        /// <returns>Returns the academic history</returns>
        /// <param name="id">The student's ID for whom academic history is being requested</param>
        /// <param name="bestFit">Boolean flag that indicates if non-term based credit should be placed in the closest appropriate term </param>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.5. Use GetAcademicHistory2Async.")]
        public async Task<AcademicHistory> GetAcademicHistoryAsync(string id, bool bestFit = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for AcademicHistory retrieval.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString("bestFit", bestFit.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "academic-credits");
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AcademicHistory>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get AcademicHistory");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Academic Levels
        /// </summary>
        /// <returns>Returns the academic levels</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<AcademicLevel> GetAcademicLevels()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_academicLevelsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicLevel>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<AcademicLevel>");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Academic Levels async.
        /// </summary>
        /// <returns>Returns the academic levels</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<AcademicLevel>> GetAcademicLevelsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_academicLevelsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicLevel>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<AcademicLevel>");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Academic Levels
        /// </summary>
        /// <returns>Returns the academic levels</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Affiliation> GetAffiliations()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_affiliationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Affiliation>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Affiliation>");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Academic Levels async
        /// </summary>
        /// <returns>Returns the academic levels</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Affiliation>> GetAffiliationsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_affiliationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Affiliation>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Affiliation>");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Academic Levels
        /// </summary>
        /// <returns>Returns the academic levels</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<ClassLevel> GetClassLevels()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_classLevelsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ClassLevel>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<ClassLevel>");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Academic Levels async.
        /// </summary>
        /// <returns>Returns the academic levels</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ClassLevel>> GetClassLevelsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_classLevelsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ClassLevel>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<ClassLevel>");
                throw;
            }
        }
        /// <summary>
        /// Get course levels
        /// </summary>
        /// <returns>Returns a set of course levels</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<CourseLevel> GetCourseLevels()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_courseLevelsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CourseLevel>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<CourseLevel>");
                throw;
            }
        }
        /// <summary>
        /// Get course levels async.
        /// </summary>
        /// <returns>Returns a set of course levels</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<CourseLevel>> GetCourseLevelsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_courseLevelsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CourseLevel>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<CourseLevel>");
                throw;
            }
        }
        /// <summary>
        /// Get course types
        /// </summary>
        /// <returns>Returns the set of course types</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<CourseType> GetCourseTypes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_courseTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CourseType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<CourseType>");
                throw;
            }
        }
        /// <summary>
        /// Get course types async.
        /// </summary>
        /// <returns>Returns the set of course types</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<CourseType>> GetCourseTypesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_courseTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CourseType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<CourseType>");
                throw;
            }
        }
        /// <summary>
        /// Get course topic codes
        /// </summary>
        /// <returns>Returns the set of course topic codes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<TopicCode> GetTopicCodes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_topicCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<TopicCode>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<TopicCode>");
                throw;
            }
        }
        /// <summary>
        /// Get course topic codes async.
        /// </summary>
        /// <returns>Returns the set of course topic codes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<TopicCode>> GetTopicCodesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_topicCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<TopicCode>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<TopicCode>");
                throw;
            }
        }
        /// <summary>
        /// Get a requirement
        /// </summary>
        /// <returns>Returns a requirement</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public Requirement GetRequirement(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Requirement retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_requirementsPath, UrlParameterUtility.EncodeWithSubstitution(id));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Requirement>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Requirement");
                throw;
            }
        }
        /// <summary>
        /// Get a requirement async.
        /// </summary>
        /// <returns>Returns a requirement</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<Requirement> GetRequirementAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Requirement retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_requirementsPath, UrlParameterUtility.EncodeWithSubstitution(id));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Requirement>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Requirement");
                throw;
            }
        }
        /// <summary>
        /// Get a list of requirements
        /// </summary>
        /// <param name="requirementIds">The IDs of the requirements requested</param>
        /// <returns>Returns a list of requirements</returns>
        /// <exception cref="ArgumentNullException">The requirementIds must be provided.</exception>
        /// <exception cref="Exception">Problem occurred during processing.</exception>
        public IEnumerable<Requirement> QueryRequirements(List<string> requirementIds)
        {
            if (requirementIds == null || requirementIds.Count() == 0)
            {
                throw new ArgumentNullException("requirementIds", "List of requirement Ids cannot be empty/null for Requirement retrieval.");
            }
            try
            {
                // Build url path from qapi path and requirements path
                string[] pathStrings = new string[] { _qapiPath, _requirementsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var criteria = new RequirementQueryCriteria() { RequirementIds = requirementIds };
                // Use URL path and criteria to call web api method
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);

                return JsonConvert.DeserializeObject<IEnumerable<Requirement>>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get specified Requirements");
                throw;
            }
        }
        /// <summary>
        /// Get a list of requirements async.
        /// </summary>
        /// <param name="requirementIds">The IDs of the requirements requested</param>
        /// <returns>Returns a list of requirements</returns>
        /// <exception cref="ArgumentNullException">The requirementIds must be provided.</exception>
        /// <exception cref="Exception">Problem occurred during processing.</exception>
        public async Task<IEnumerable<Requirement>> QueryRequirementsAsync(List<string> requirementIds)
        {
            if (requirementIds == null || requirementIds.Count() == 0)
            {
                throw new ArgumentNullException("requirementIds", "List of requirement Ids cannot be empty/null for Requirement retrieval.");
            }
            try
            {
                // Build url path from qapi path and requirements path
                string[] pathStrings = new string[] { _qapiPath, _requirementsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var criteria = new RequirementQueryCriteria() { RequirementIds = requirementIds };
                // Use URL path and criteria to call web api method
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                return JsonConvert.DeserializeObject<IEnumerable<Requirement>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get specified Requirements");
                throw;
            }
        }
        /// <summary>
        /// Get a student's programs
        /// </summary>
        /// <returns>Returns the set of student's programs</returns>
        /// <param name="id">The ID of the student whose programs are being requested</param>
        /// <param name="currentOnly">Boolean that indicates whether to get only current programs, or current and past programs</param>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<StudentProgram> GetStudentPrograms(string id, bool currentOnly = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for IEnumerable<StudentProgram> retrieval.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString(new[] { "currentOnly", currentOnly.ToString() });
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "programs");
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentProgram>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<StudentProgram>");
                throw;
            }
        }
        /// <summary>
        /// Get a student's programs async.
        /// </summary>
        /// <returns>Returns the set of student's programs</returns>
        /// <param name="id">The ID of the student whose programs are being requested</param>
        /// <param name="currentOnly">Boolean that indicates whether to get only current programs, or current and past programs</param>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.10. Use version 2 of this method instead.")]
        public async Task<IEnumerable<StudentProgram>> GetStudentProgramsAsync(string id, bool currentOnly = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for IEnumerable<StudentProgram> retrieval.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString(new[] { "currentOnly", currentOnly.ToString() });
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "programs");
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentProgram>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<StudentProgram>");
                throw;
            }
        }

        /// <summary>
        /// Get a student's programs async.
        /// </summary>
        /// <returns>Returns the set of student's programs</returns>
        /// <param name="id">The ID of the student whose programs are being requested</param>
        /// <param name="currentOnly">Boolean that indicates whether to get only current programs, or current and past programs</param>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<StudentProgram2>> GetStudentPrograms2Async(string id, bool currentOnly = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for IEnumerable<StudentProgram2> retrieval.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString(new[] { "currentOnly", currentOnly.ToString() });
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id, "programs");
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentProgram2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<StudentProgram2>");
                throw;
            }
        }
        /// <summary>
        /// Get all academic programs
        /// </summary>
        /// <returns>Returns a set of academic programs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Program> GetPrograms()
        {

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_programsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Program>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Program>");
                throw;
            }
        }
        /// <summary>
        /// Get all academic programs async.
        /// </summary>
        /// <returns>Returns a set of academic programs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Program>> GetProgramsAsync()
        {

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_programsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Program>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Program>");
                throw;
            }
        }
        /// <summary>
        /// Get all active programs, version 2
        /// </summary>
        /// <returns>Returns the set of active programs</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Program> GetActivePrograms()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_programsPath, "active");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Program>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Program>");
                throw;
            }
        }
        /// <summary>
        /// Get all active programs, version 2 async
        /// </summary>
        /// <returns>Returns the set of active programs</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Program>> GetActiveProgramsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_programsPath, "active");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Program>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Program>");
                throw;
            }
        }
        /// <summary>
        /// Get a program by id
        /// </summary>
        /// <returns>Returns a program</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public Program GetProgram(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Program retrieval.");
            }
            try
            {
                var queryString = UrlUtility.BuildEncodedQueryString("id", id);
                string urlPath = UrlUtility.CombineUrlPathAndArguments(_programsPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath);
                var resource = JsonConvert.DeserializeObject<Program>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Program");
                throw;
            }
        }
        /// <summary>
        /// Get a program by id async.
        /// </summary>
        /// <returns>Returns a program</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<Program> GetProgramAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Program retrieval.");
            }
            try
            {
                var queryString = UrlUtility.BuildEncodedQueryString("id", id);
                string urlPath = UrlUtility.CombineUrlPathAndArguments(_programsPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath);
                var resource = JsonConvert.DeserializeObject<Program>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Program");
                throw;
            }
        }

        /// <summary>
        /// Get the program requirements for the given program/catalog
        /// </summary>
        /// <returns>Returns the program's requirements</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public ProgramRequirements GetProgramRequirements(string program, string catalog)
        {
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "program cannot be empty/null for ProgramRequirements retrieval.");
            }
            if (string.IsNullOrEmpty(catalog))
            {
                throw new ArgumentNullException("catalog", "catalog cannot be empty/null for ProgramRequirements retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_programsPath, UrlParameterUtility.EncodeWithSubstitution(program), UrlParameterUtility.EncodeWithSubstitution(catalog));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ProgramRequirements>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramRequirements");
                throw;
            }
        }
        /// <summary>
        /// Get the program requirements for the given program/catalog async.
        /// </summary>
        /// <returns>Returns the program's requirements</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<ProgramRequirements> GetProgramRequirementsAsync(string program, string catalog)
        {
            if (string.IsNullOrEmpty(program))
            {
                throw new ArgumentNullException("program", "program cannot be empty/null for ProgramRequirements retrieval.");
            }
            if (string.IsNullOrEmpty(catalog))
            {
                throw new ArgumentNullException("catalog", "catalog cannot be empty/null for ProgramRequirements retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_programsPath, UrlParameterUtility.EncodeWithSubstitution(program), UrlParameterUtility.EncodeWithSubstitution(catalog));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ProgramRequirements>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ProgramRequirements");
                throw;
            }
        }
        /// <summary>
        /// Get a course by id
        /// </summary>
        /// <returns>Returns a course</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public Course2 GetCourse(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Course retrieval.");
            }
            try
            {
                var queryString = UrlUtility.BuildEncodedQueryString("courseId", id);
                var urlPath = UrlUtility.CombineUrlPathAndArguments(_coursesPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Course2>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Course");
                throw;
            }
        }
        /// <summary>
        /// Get a course by id async.
        /// </summary>
        /// <returns>Returns a course</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<Course2> GetCourseAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Course retrieval.");
            }
            try
            {
                var queryString = UrlUtility.BuildEncodedQueryString("courseId", id);
                var urlPath = UrlUtility.CombineUrlPathAndArguments(_coursesPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Course2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Course");
                throw;
            }
        }
        /// <summary>
        /// Get courses using the given course ids.
        /// </summary>
        /// <returns>Returns a list of <see cref="Course2">course2 objects</see>"/></returns>
        /// <exception cref="ArgumentNullException">The resource courseIds must be provided.</exception>
        public IEnumerable<Course2> QueryCourses2(CourseQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Course query criteria cannot be null.");
            }
            try
            {
                // Build url path from qapi path and courses path
                string[] pathStrings = new string[] { _qapiPath, _coursesPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                // Use URL path and request data to call web api method (including query string)
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);

                return JsonConvert.DeserializeObject<IEnumerable<Course2>>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get requested Course2 objects");
                throw;
            }
        }
        /// <summary>
        /// Get courses using the given course ids async.
        /// </summary>
        /// <returns>Returns a list of <see cref="Course2">course2 objects</see>"/></returns>
        /// <exception cref="ArgumentNullException">The resource courseIds must be provided.</exception>
        public async Task<IEnumerable<Course2>> QueryCourses2Async(CourseQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Course query criteria cannot be null.");
            }
            try
            {
                // Build url path from qapi path and courses path
                string[] pathStrings = new string[] { _qapiPath, _coursesPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                // Use URL path and request data to call web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                return JsonConvert.DeserializeObject<IEnumerable<Course2>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get requested Course2 objects");
                throw;
            }
        }
        /// <summary>
        /// Search for Students.
        /// </summary>
        /// <param name="studentQuery">Search criteria for Students</param>
        /// <returns>List of Student IDs</returns>
        public IEnumerable<string> SearchStudentIds(StudentQuery studentQuery)
        {
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _studentIdsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse<StudentQuery>(studentQuery, urlPath, headers: headers);
                IEnumerable<string> studentIds = null;
                studentIds = JsonConvert.DeserializeObject<IEnumerable<string>>(response.Content.ReadAsStringAsync().Result);
                return studentIds;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Student IDs");
                throw;
            }
        }
        /// <summary>
        /// Search for Students async.
        /// </summary>
        /// <param name="studentQuery">Search criteria for Students</param>
        /// <returns>List of Student IDs</returns>
        public async Task<IEnumerable<string>> SearchStudentIdsAsync(StudentQuery studentQuery)
        {
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _studentIdsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse<StudentQuery>(studentQuery, urlPath, headers: headers);
                IEnumerable<string> studentIds = null;
                studentIds = JsonConvert.DeserializeObject<IEnumerable<string>>(await response.Content.ReadAsStringAsync());
                return studentIds;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Student IDs");
                throw;
            }
        }
        /// <summary>
        /// Search for course, brings back a page containing courses that met criteria and associated filters.
        /// </summary>
        /// <param name="courseIds">List of course Ids</param>
        /// <param name="subjects">List of subjects</param>
        /// <param name="academicLevels">List of academic levels</param>
        /// <param name="courseLevels">List of course levels</param>
        /// <param name="courseTypes">List of course types (section)</param>
        /// <param name="topicCodes">List of topic codes</param>
        /// <param name="terms">List of terms (section)</param>
        /// <param name="days">List of meeting days (section)</param>
        /// <param name="locations">List of locations (course and section)</param>
        /// <param name="faculty">List of faculty (section)</param>
        /// <param name="startTime">Start time (section)</param>
        /// <param name="endTime">End time (section)</param>
        /// <param name="keyword">Search string (course and section)</param>
        /// <param name="requirementGroup">Requirement Group Id</param>
        /// <param name="requirementCode">Requirement Code</param>
        /// <param name="sectionIds">List of Section Ids</param>
        /// <param name="onlineCategory">Online Category</param>
        /// <param name="pageSize">Number of items to return per page</param>
        /// <param name="pageIndex">Page number</param>
        /// <returns><see cref="CoursePage2">CoursePage2</see> containing the list of course Ids, section Ids and filters</returns>
        public CoursePage2 SearchCourses(IEnumerable<string> courseIds, IEnumerable<string> subjects, IEnumerable<string> academicLevels, IEnumerable<string> courseLevels, IEnumerable<string> courseTypes, IEnumerable<string> topicCodes, IEnumerable<string> terms, IEnumerable<string> days, IEnumerable<string> locations, IEnumerable<string> faculty, int? startTime, int? endTime, string keyword, RequirementGroup requirementGroup, string requirementCode, IEnumerable<string> sectionIds, IEnumerable<string> onlineCategories, int pageSize, int pageIndex)
        {
            if (keyword == null) keyword = string.Empty;

            try
            {
                // Build url path + subject
                var queryString = UrlUtility.BuildEncodedQueryString("pageSize", pageSize.ToString(), "pageIndex", pageIndex.ToString());
                var urlPath = UrlUtility.CombineUrlPathAndArguments(_coursesSearchPath, queryString);

                var criteria = new CourseSearchCriteria();

                if (!string.IsNullOrEmpty(keyword))
                {
                    criteria.Keyword = keyword.Replace("/", "_~");
                }

                if (requirementGroup != null)
                {
                    criteria.RequirementGroup = new RequirementGroup();
                    if (!string.IsNullOrEmpty(requirementGroup.RequirementCode))
                    {
                        criteria.RequirementGroup.RequirementCode = requirementGroup.RequirementCode.Replace("/", "_~");
                    }
                    criteria.RequirementGroup.SubRequirementId = requirementGroup.SubRequirementId;
                    criteria.RequirementGroup.GroupId = requirementGroup.GroupId;
                }

                if (!string.IsNullOrEmpty(requirementCode))
                {
                    criteria.RequirementCode = requirementCode.Replace("/", "_~");
                }

                criteria.CourseIds = courseIds;
                criteria.Subjects = subjects;
                criteria.AcademicLevels = academicLevels;
                criteria.CourseLevels = courseLevels;
                criteria.CourseTypes = courseTypes;
                criteria.TopicCodes = topicCodes;
                criteria.Terms = terms;
                criteria.DaysOfWeek = days;
                criteria.Locations = locations;
                criteria.Faculty = faculty;
                criteria.EarliestTime = startTime == null ? 0 : (int)startTime;
                criteria.LatestTime = endTime == null ? 0 : (int)endTime;
                criteria.SectionIds = sectionIds;
                criteria.OnlineCategories = onlineCategories;
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                // Use URL path to call web api method (including query string)
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);

                var courses = JsonConvert.DeserializeObject<CoursePage2>(response.Content.ReadAsStringAsync().Result);

                return courses;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }


        /// <summary>
        /// Search for course, brings back a page containing courses that met criteria and associated filters async.
        /// </summary>
        /// <param name="courseIds">List of course Ids</param>
        /// <param name="subjects">List of subjects</param>
        /// <param name="academicLevels">List of academic levels</param>
        /// <param name="courseLevels">List of course levels</param>
        /// <param name="courseTypes">List of course types (section)</param>
        /// <param name="topicCodes">List of topic codes</param>
        /// <param name="terms">List of terms (section)</param>
        /// <param name="days">List of meeting days (section)</param>
        /// <param name="locations">List of locations (course and section)</param>
        /// <param name="faculty">List of faculty (section)</param>
        /// <param name="startTime">Start time (section)</param>
        /// <param name="endTime">End time (section)</param>
        /// <param name="keyword">Search string (course and section)</param>
        /// <param name="requirementGroup">Requirement Group Id</param>
        /// <param name="requirementCode">Requirement Code</param>
        /// <param name="sectionIds">List of Section Ids</param>
        /// <param name="onlineCategories">Online Category</param>
        /// <param name="pageSize">Number of items to return per page</param>
        /// <param name="pageIndex">Page number</param>
        /// <returns><see cref="CoursePage2">CoursePage2</see> containing the list of course Ids, section Ids and filters</returns>
        public async Task<CoursePage2> SearchCoursesAsync(IEnumerable<string> courseIds, IEnumerable<string> subjects, IEnumerable<string> academicLevels, IEnumerable<string> courseLevels, IEnumerable<string> courseTypes, IEnumerable<string> topicCodes, IEnumerable<string> terms, IEnumerable<string> days, IEnumerable<string> locations, IEnumerable<string> faculty, int? startTime, int? endTime, string keyword, RequirementGroup requirementGroup, string requirementCode, IEnumerable<string> sectionIds, IEnumerable<string> onlineCategories, int pageSize, int pageIndex)
        {
            if (keyword == null) keyword = string.Empty;

            try
            {
                // Build url path + subject
                var queryString = UrlUtility.BuildEncodedQueryString("pageSize", pageSize.ToString(), "pageIndex", pageIndex.ToString());
                var urlPath = UrlUtility.CombineUrlPathAndArguments(_coursesSearchPath, queryString);

                var criteria = new CourseSearchCriteria();

                if (!string.IsNullOrEmpty(keyword))
                {
                    criteria.Keyword = keyword.Replace("/", "_~");
                }

                if (requirementGroup != null)
                {
                    criteria.RequirementGroup = new RequirementGroup();
                    if (!string.IsNullOrEmpty(requirementGroup.RequirementCode))
                    {
                        criteria.RequirementGroup.RequirementCode = requirementGroup.RequirementCode.Replace("/", "_~");
                    }
                    criteria.RequirementGroup.SubRequirementId = requirementGroup.SubRequirementId;
                    criteria.RequirementGroup.GroupId = requirementGroup.GroupId;
                }

                if (!string.IsNullOrEmpty(requirementCode))
                {
                    criteria.RequirementCode = requirementCode.Replace("/", "_~");
                }

                criteria.CourseIds = courseIds;
                criteria.Subjects = subjects;
                criteria.AcademicLevels = academicLevels;
                criteria.CourseLevels = courseLevels;
                criteria.CourseTypes = courseTypes;
                criteria.TopicCodes = topicCodes;
                criteria.Terms = terms;
                criteria.DaysOfWeek = days;
                criteria.Locations = locations;
                criteria.Faculty = faculty;
                criteria.EarliestTime = startTime == null ? 0 : (int)startTime;
                criteria.LatestTime = endTime == null ? 0 : (int)endTime;
                criteria.SectionIds = sectionIds;
                criteria.OnlineCategories = onlineCategories;
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                // Use URL path to call web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                var courses = JsonConvert.DeserializeObject<CoursePage2>(await response.Content.ReadAsStringAsync());

                return courses;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Search for course, brings back a page containing courses that met criteria and associated filters async.
        /// </summary>
        /// <param name="courseIds">List of course Ids</param>
        /// <param name="subjects">List of subjects</param>
        /// <param name="academicLevels">List of academic levels</param>
        /// <param name="courseLevels">List of course levels</param>
        /// <param name="courseTypes">List of course types (section)</param>
        /// <param name="topicCodes">List of topic codes</param>
        /// <param name="terms">List of terms (section)</param>
        /// <param name="days">List of meeting days (section)</param>
        /// <param name="locations">List of locations (course and section)</param>
        /// <param name="faculty">List of faculty (section)</param>
        /// <param name="startTime">Start time (section)</param>
        /// <param name="endTime">End time (section)</param>
        /// <param name="keyword">Search string (course and section)</param>
        /// <param name="requirementGroup">Requirement Group Id</param>
        /// <param name="requirementCode">Requirement Code</param>
        /// <param name="sectionIds">List of Section Ids</param>
        /// <param name="onlineCategories">Online Category</param>
        /// <param name="openSections">Flag to retrieve only sections that are open </param>
        /// <param name="sectionStartDate">Earliest first meeting time of the section.</param>
        /// <param name="sectionEndDate">Lastest last meeting time for the section</param>
        /// <param name="pageSize">Number of items to return per page</param>
        /// <param name="pageIndex">Page number</param>
        /// <returns><see cref="CoursePage2">CoursePage2</see> containing the list of course Ids, section Ids and filters</returns>
        public async Task<CoursePage2> SearchCoursesAsync(IEnumerable<string> courseIds, IEnumerable<string> subjects, IEnumerable<string> academicLevels, IEnumerable<string> courseLevels, IEnumerable<string> courseTypes, IEnumerable<string> topicCodes, IEnumerable<string> terms, IEnumerable<string> days, IEnumerable<string> locations, IEnumerable<string> faculty, int? startTime, int? endTime, string keyword, RequirementGroup requirementGroup, string requirementCode, IEnumerable<string> sectionIds, IEnumerable<string> onlineCategories, bool? openSections, DateTime? sectionStartDate, DateTime? sectionEndDate, int pageSize, int pageIndex)
        {
            if (keyword == null) keyword = string.Empty;

            try
            {
                // Build url path + subject
                var queryString = UrlUtility.BuildEncodedQueryString("pageSize", pageSize.ToString(), "pageIndex", pageIndex.ToString());
                var urlPath = UrlUtility.CombineUrlPathAndArguments(_coursesSearchPath, queryString);

                var criteria = new CourseSearchCriteria();

                if (!string.IsNullOrEmpty(keyword))
                {
                    criteria.Keyword = keyword.Replace("/", "_~");
                }

                if (requirementGroup != null)
                {
                    criteria.RequirementGroup = new RequirementGroup();
                    if (!string.IsNullOrEmpty(requirementGroup.RequirementCode))
                    {
                        criteria.RequirementGroup.RequirementCode = requirementGroup.RequirementCode.Replace("/", "_~");
                    }
                    criteria.RequirementGroup.SubRequirementId = requirementGroup.SubRequirementId;
                    criteria.RequirementGroup.GroupId = requirementGroup.GroupId;
                }

                if (!string.IsNullOrEmpty(requirementCode))
                {
                    criteria.RequirementCode = requirementCode.Replace("/", "_~");
                }

                criteria.CourseIds = courseIds;
                criteria.Subjects = subjects;
                criteria.AcademicLevels = academicLevels;
                criteria.CourseLevels = courseLevels;
                criteria.CourseTypes = courseTypes;
                criteria.TopicCodes = topicCodes;
                criteria.Terms = terms;
                criteria.DaysOfWeek = days;
                criteria.Locations = locations;
                criteria.Faculty = faculty;
                criteria.EarliestTime = startTime == null ? 0 : (int)startTime;
                criteria.LatestTime = endTime == null ? 0 : (int)endTime;
                criteria.SectionIds = sectionIds;
                criteria.OnlineCategories = onlineCategories;
                criteria.OpenSections = openSections.HasValue ? openSections.Value : false;
                criteria.SectionStartDate = sectionStartDate;
                criteria.SectionEndDate = sectionEndDate;
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                // Use URL path to call web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                var courses = JsonConvert.DeserializeObject<CoursePage2>(await response.Content.ReadAsStringAsync());

                return courses;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }




        /// <summary>
        /// Gets the Section objects for all of the provided course IDs. Use this method if you have Course objects but do
        /// not have their associated Section information.
        /// </summary>
        /// <param name="ids">A list of course IDs for which to retrieve Section information</param>
        /// <param name="useCache">Defaults to true: If true, cached repository data will be returned when possible, otherwise fresh data is returned.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided list of course IDs is null</exception>
        /// <returns>List of <see cref="Section2">Section2</see> objects</returns>
        [Obsolete("Obsolete as of API 1.5. Use GetSectionsByCourse3.")]
        public IEnumerable<Section2> GetSectionsByCourse(List<string> courseIds, bool useCache = true)
        {
            if (courseIds == null)
            {
                throw new ArgumentNullException("courseIds", "courseIds cannot be empty/null.");
            }
            if (courseIds.Count == 0)
            {
                return new List<Section2>();
            }

            string query = UrlUtility.BuildEncodedQueryString("courseIds", string.Join(",", courseIds));
            string urlPath = UrlUtility.CombineUrlPath(_coursesPath, "sections");
            urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            try
            {
                var response = ExecuteGetRequestWithResponse(urlPath, useCache: useCache, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Section2>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }
        /// <summary>
        /// Gets the Section objects for all of the provided course IDs. Use this method if you have Course objects but do
        /// not have their associated Section information async.
        /// </summary>
        /// <param name="ids">A list of course IDs for which to retrieve Section information</param>
        /// <param name="useCache">Defaults to true: If true, cached repository data will be returned when possible, otherwise fresh data is returned.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided list of course IDs is null</exception>
        /// <returns>List of <see cref="Section2">Section2</see> objects</returns>
        [Obsolete("Obsolete as of API 1.5. Use GetSectionsByCourse3Async")]
        public async Task<IEnumerable<Section2>> GetSectionsByCourseAsync(List<string> courseIds, bool useCache = true)
        {
            if (courseIds == null)
            {
                throw new ArgumentNullException("courseIds", "courseIds cannot be empty/null.");
            }
            if (courseIds.Count == 0)
            {
                return new List<Section2>();
            }

            string query = UrlUtility.BuildEncodedQueryString("courseIds", string.Join(",", courseIds));
            string urlPath = UrlUtility.CombineUrlPath(_coursesPath, "sections");
            urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, useCache: useCache, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Section2>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }
        /// <summary>
        /// Gets the Section objects for all of the provided course IDs. Use this method if you have Course objects but do
        /// not have their associated Section information.
        /// </summary>
        /// <param name="ids">A list of course IDs for which to retrieve Section information</param>
        /// <param name="useCache">Defaults to true: If true, cached repository data will be returned when possible, otherwise fresh data is returned.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided list of course IDs is null</exception>
        /// <returns>List of <see cref="Section3">Section3</see> objects</returns>
        public IEnumerable<Section3> GetSectionsByCourse3(List<string> courseIds, bool useCache = true)
        {
            if (courseIds == null)
            {
                throw new ArgumentNullException("courseIds", "courseIds cannot be empty/null.");
            }
            if (courseIds.Count == 0)
            {
                return new List<Section3>();
            }

            string query = UrlUtility.BuildEncodedQueryString("courseIds", string.Join(",", courseIds));
            string urlPath = UrlUtility.CombineUrlPath(_coursesPath, "sections");
            urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);

            try
            {
                var response = ExecuteGetRequestWithResponse(urlPath, useCache: useCache, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Section3>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }
        /// <summary>
        /// Gets the Section objects for all of the provided course IDs. Use this method if you have Course objects but do
        /// not have their associated Section information async.
        /// </summary>
        /// <param name="ids">A list of course IDs for which to retrieve Section information</param>
        /// <param name="useCache">Defaults to true: If true, cached repository data will be returned when possible, otherwise fresh data is returned.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided list of course IDs is null</exception>
        /// <returns>List of <see cref="Section3">Section3</see> objects</returns>
        public async Task<IEnumerable<Section3>> GetSectionsByCourse3Async(List<string> courseIds, bool useCache = true)
        {
            if (courseIds == null)
            {
                throw new ArgumentNullException("courseIds", "courseIds cannot be empty/null.");
            }
            if (courseIds.Count == 0)
            {
                return new List<Section3>();
            }

            string query = UrlUtility.BuildEncodedQueryString("courseIds", string.Join(",", courseIds));
            string urlPath = UrlUtility.CombineUrlPath(_coursesPath, "sections");
            urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);

            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, useCache: useCache, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Section3>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }
        /// <summary>
        /// Get a sections
        /// </summary>
        /// <returns><see cref="Section2">Section2</see> object</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.5. Use GetSection3.")]
        public Section2 GetSection(string id, bool useCache = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Section retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sectionsPath, UrlParameterUtility.EncodeWithSubstitution(id));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers, useCache: useCache);
                var resource = JsonConvert.DeserializeObject<Section2>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Section2");
                throw;
            }
        }
        /// <summary>
        /// Get a sections async.
        /// </summary>
        /// <returns><see cref="Section2">Section2</see> object</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.5. Use GetSection3Async.")]
        public async Task<Section2> GetSectionAsync(string id, bool useCache = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Section retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sectionsPath, UrlParameterUtility.EncodeWithSubstitution(id));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers, useCache: useCache);
                var resource = JsonConvert.DeserializeObject<Section2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Section2");
                throw;
            }
        }
        /// <summary>
        /// Get a section 
        /// </summary>
        /// <returns><see cref="Section3">Section3</see> object</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public Section3 GetSection3(string id, bool useCache = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Section retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sectionsPath, UrlParameterUtility.EncodeWithSubstitution(id));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers, useCache: useCache);
                var resource = JsonConvert.DeserializeObject<Section3>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Section3");
                throw;
            }
        }
        /// <summary>
        /// Get a section  async.
        /// </summary>
        /// <returns><see cref="Section3">Section3</see> object</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<Section3> GetSection3Async(string id, bool useCache = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Section retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sectionsPath, UrlParameterUtility.EncodeWithSubstitution(id));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers, useCache: useCache);
                var resource = JsonConvert.DeserializeObject<Section3>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Section3");
                throw;
            }
        }
        /// <summary>
        /// Get a set of sections
        /// </summary>
        /// <returns>Returns a set of <see cref="Section2">Section2</see> items</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.5. Use GetSections3.")]
        public IEnumerable<Section2> GetSections(List<string> ids, bool useCache = true)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new ArgumentNullException("ids", "IDs cannot be empty/null for IEnumerable<Section2> retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _sectionsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse<List<string>>(ids, urlPath, headers: headers, useCache: useCache);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Section2>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Section2>");
                throw;
            }
        }
        // <summary>
        /// Get a set of sections async.
        /// </summary>
        /// <returns>Returns a set of <see cref="Section2">Section2</see> items</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.5. Use GetSections3Async.")]
        public async Task<IEnumerable<Section2>> GetSectionsAsync(List<string> ids, bool useCache = true)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new ArgumentNullException("ids", "IDs cannot be empty/null for IEnumerable<Section2> retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _sectionsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync<List<string>>(ids, urlPath, headers: headers, useCache: useCache);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Section2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Section2>");
                throw;
            }
        }
        /// <summary>
        /// Get a set of sections
        /// </summary>
        /// <param name="ids">List of section Ids</param>
        /// <param name="useCache">Boolean indicates whether to get cached sections or sections from database (for seat counts)</param>
        /// <returns>Returns a set of <see cref="Section3">Section3</see> items</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.6. Use GetSections4.")]
        public IEnumerable<Section3> GetSections3(List<string> ids, bool useCache = true)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new ArgumentNullException("ids", "IDs cannot be empty/null for IEnumerable<Section3> retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _sectionsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecutePostRequestWithResponse<List<string>>(ids, urlPath, headers: headers, useCache: useCache);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Section3>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Section3>");
                throw;
            }
        }
        /// <summary>
        /// Get a set of sections async.
        /// </summary>
        /// <param name="ids">List of section Ids</param>
        /// <param name="useCache">Boolean indicates whether to get cached sections or sections from database (for seat counts)</param>
        /// <returns>Returns a set of <see cref="Section3">Section3</see> items</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.6. Use GetSections4Async.")]
        public async Task<IEnumerable<Section3>> GetSections3Async(List<string> ids, bool useCache = true)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new ArgumentNullException("ids", "IDs cannot be empty/null for IEnumerable<Section3> retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _sectionsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecutePostRequestWithResponseAsync<List<string>>(ids, urlPath, headers: headers, useCache: useCache);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Section3>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Section3>");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Section Objects based on a Post transaction with multiple keys
        /// </summary>
        /// <param name="ids">List of section Ids</param>
        /// <param name="useCache">Boolean indicates whether to get cached sections or sections from database (for seat counts)</param>
        /// <param name="bestFit">Boolean indicates whether to assign a term to sections that do not have one</param>
        /// <returns>Returns a set of <see cref="Section3">Section3</see> items</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Section3> GetSections4(IEnumerable<string> sectionIds, bool useCache = true, bool bestFit = false)
        {

            if (sectionIds == null)
            {
                throw new ArgumentNullException("sectionIds", "IDs cannot be empty/null for Section retrieval.");
            }
            SectionsQueryCriteria criteria = new SectionsQueryCriteria();
            criteria.SectionIds = sectionIds;
            criteria.BestFit = bestFit;
            try
            {
                // Build url path from qapi path and sections path
                string[] pathStrings = new string[] { _qapiPath, _sectionsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers, useCache: useCache);
                return JsonConvert.DeserializeObject<IEnumerable<Section3>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve sections.");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Section Objects based on a Post transaction with multiple keys async.
        /// </summary>
        /// <param name="ids">List of section Ids</param>
        /// <param name="useCache">Boolean indicates whether to get cached sections or sections from database (for seat counts)</param>
        /// <param name="bestFit">Boolean indicates whether to assign a term to sections that do not have one</param>
        /// <returns>Returns a set of <see cref="Section3">Section3</see> items</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Section3>> GetSections4Async(IEnumerable<string> sectionIds, bool useCache = true, bool bestFit = false)
        {

            if (sectionIds == null)
            {
                throw new ArgumentNullException("sectionIds", "IDs cannot be empty/null for Section retrieval.");
            }
            SectionsQueryCriteria criteria = new SectionsQueryCriteria();
            criteria.SectionIds = sectionIds;
            criteria.BestFit = bestFit;
            try
            {
                // Build url path from qapi path and sections path
                string[] pathStrings = new string[] { _qapiPath, _sectionsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers, useCache: useCache);
                return JsonConvert.DeserializeObject<IEnumerable<Section3>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve sections.");
                throw;
            }
        }
        /// <summary>
        /// Given a set of section IDs, return an iCal representation of those sections' meetings
        /// </summary>
        /// <param name="sectionIds">A set of section IDs</param>
        /// <returns>An iCal that contains those sections' meetings. iCal is a text (string) format for exchanging meeting data.</returns>
        public string GetSectionEvents(List<String> sectionIds)
        {
            if (sectionIds == null || sectionIds.Count == 0)
            {
                throw new ArgumentNullException("sectionIds", "sectionIds cannot be null or empty.");
            }

            try
            {
                string ids = string.Empty;
                for (int i = 0; i < sectionIds.Count; i++)
                {
                    ids += sectionIds.ElementAt(i);
                    if (i < sectionIds.Count - 1 && sectionIds.Count > 1) ids += ",";
                }

                // TODO: use query string instead of placing IDs in the path
                // BAD: sections/123,124,125/calendar
                // BETTER: sections/calendar?sections=123,124,125
                string urlPath = UrlUtility.CombineUrlPath(_sectionsPath, UrlParameterUtility.EncodeWithSubstitution(ids), "calendar");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                // UI is expecting the iCal string, not the JSON response object
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<EventsICal>(response.Content.ReadAsStringAsync().Result);
                return resource.iCal;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return string.Empty;
            }
        }
        /// <summary>
        /// Given a set of section IDs, return an iCal representation of those sections' meetings async.
        /// </summary>
        /// <param name="sectionIds">A set of section IDs</param>
        /// <returns>An iCal that contains those sections' meetings. iCal is a text (string) format for exchanging meeting data.</returns>
        public async Task<string> GetSectionEventsAsync(List<String> sectionIds)
        {
            if (sectionIds == null || sectionIds.Count == 0)
            {
                throw new ArgumentNullException("sectionIds", "sectionIds cannot be null or empty.");
            }

            try
            {
                string ids = string.Empty;
                for (int i = 0; i < sectionIds.Count; i++)
                {
                    ids += sectionIds.ElementAt(i);
                    if (i < sectionIds.Count - 1 && sectionIds.Count > 1) ids += ",";
                }

                // TODO: use query string instead of placing IDs in the path
                // BAD: sections/123,124,125/calendar
                // BETTER: sections/calendar?sections=123,124,125
                string urlPath = UrlUtility.CombineUrlPath(_sectionsPath, UrlParameterUtility.EncodeWithSubstitution(ids), "calendar");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                // UI is expecting the iCal string, not the JSON response object
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<EventsICal>(await response.Content.ReadAsStringAsync());
                return resource.iCal;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// Given a section ID return list of section meeting instances - i.e. specific class meeting times.  
        /// </summary>
        /// <param name="sectionIds">A set of section IDs</param>
        /// <returns>A list of section meeting instance Dtos.</returns>
        public async Task<IEnumerable<SectionMeetingInstance>> GetSectionMeetingInstancesAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "sectionId cannot be null or empty.");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sectionsPath, sectionId, _sectionMeetingInstancesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                // UI is expecting a list of section meeting instance Dtos
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<SectionMeetingInstance>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Retrieves student attendances for a specific section 
        /// </summary>
        /// <param name="sectionId">section Id (required)</param>
        /// <param name="includeCrossListedSections">If yes, attendances for sections crosslisted to the sectionId will also be included</param>
        /// <param name="attendanceDate">Attendance date if only one date is requested. If omitted, all student attendances will be returned.</param>
        /// <returns>At list of student attendance Dtos</returns>
        public async Task<IEnumerable<StudentAttendance>> QueryStudentAttendancesAsync(string sectionId, bool includeCrossListedSections, DateTime? attendanceDate)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Must provide a section Id to retrieve student attendances");
            }
            StudentAttendanceQueryCriteria criteria = new StudentAttendanceQueryCriteria();
            criteria.SectionId = sectionId;
            criteria.IncludeCrossListedAttendances = includeCrossListedSections;
            criteria.AttendanceDate = attendanceDate;

            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _studentAttendancesPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentAttendance>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student attendances.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves student attendances for a given studentId for the given sectionIds.
        /// If  sectionIds is not provided then attendances from all the student's sections is returned. 
        /// </summary>
        /// <param name="sectionId">section Id (required)</param>
        /// <returns> <see cref="StudentSectionsAttendances"/> section wise student's attendances</returns>
        public async Task<StudentSectionsAttendances> QueryStudentSectionAttendancesAsync(string studentId, IEnumerable<string> sectionIds)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Must provide a student Id to retrieve student attendances");
            }
            StudentSectionAttendancesQueryCriteria criteria = new StudentSectionAttendancesQueryCriteria();
            if (sectionIds != null && sectionIds.Any())
            {
                criteria.SectionIds.AddRange(sectionIds);
            }
            criteria.StudentId = studentId;
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _studentSectionAttendancesPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<StudentSectionsAttendances>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve  attendances for student "+studentId);
                throw;
            }
        }



        /// <summary>
        /// Update a student attendance information async.
        /// </summary>
        /// <param name="studentAttendance">A StudentAttendance object.</param>
        /// <returns>An updated StudentAttendance object.</returns>
        public async Task<StudentAttendance> UpdateStudentAttendanceAsync(StudentAttendance studentAttendance)
        {
            if (studentAttendance == null)
            {
                throw new ArgumentNullException("studentAttendance", "studentAttendance cannot be null.");
            }
            try
            {
                // Build url path
                var urlPath = _studentAttendancesPath;
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync<StudentAttendance>(studentAttendance, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<StudentAttendance>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update Student Attendance information.");
                throw;
            }
        }

        /// <summary>
        /// Update a section attendance information async.
        /// </summary>
        /// <param name="studentAttendance">A SectionAttendance object.</param>
        /// <returns>An SectionAttendanceResponse DTO that itemizes the items that were successful and the failures.</returns>
        public async Task<SectionAttendanceResponse> UpdateSectionAttendancesAsync(SectionAttendance sectionAttendance)
        {
            if (sectionAttendance == null)
            {
                throw new ArgumentNullException("sectionAttendance", "sectionAttendance cannot be null.");
            }
            try
            {
                // Build url path
                var urlPath = _sectionAttendancesPath;
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync<SectionAttendance>(sectionAttendance, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<SectionAttendanceResponse>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update Section Attendance information.");
                throw;
            }
        }

        /// <summary>
        /// update student grades
        /// </summary>
        /// <param name="sectionId">section id</param>
        /// <param name="sectionGrades">student grades</param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.13. Use PutCollectionOfStudentGrades4Async for non-ILP caller, PutIlpCollectionOfGrades1Async for ILP caller")]
        public async Task<IEnumerable<SectionGradeResponse>> PutCollectionOfStudentGrades3Async(string sectionId, SectionGrades3 sectionGrades)
        {
            if (sectionId == null)
            {
                throw new ArgumentNullException("sectionId", "Section ID cannot be null");
            }

            if (sectionGrades == null)
            {
                throw new ArgumentNullException("sectionGrades", "Section Grades object cannot be null");
            }

            var urlPath = UrlUtility.CombineUrlPath(new[] { _sectionsPath, sectionId, "grades" });
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);

            IEnumerable<SectionGradeResponse> updatedResponse = null;

            try
            {
                var response = await ExecutePutRequestWithResponseAsync<SectionGrades3>(sectionGrades, urlPath, headers: headers);
                updatedResponse = JsonConvert.DeserializeObject<IEnumerable<SectionGradeResponse>>(await response.Content.ReadAsStringAsync());
            }

            // If the HTTP request fails, the grades weren't updated successfull
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("Grades for section {0} failed.", sectionId), hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return updatedResponse;
        }

        /// <summary>
        /// Update student grades from non-ILP caller.
        /// </summary>
        /// <param name="sectionId">section id</param>
        /// <param name="sectionGrades">student grades</param>
        /// <returns></returns>
        public async Task<IEnumerable<SectionGradeResponse>> PutCollectionOfStudentGrades4Async(string sectionId, SectionGrades3 sectionGrades)
        {
            if (sectionId == null)
            {
                throw new ArgumentNullException("sectionId", "Section ID cannot be null");
            }

            if (sectionGrades == null)
            {
                throw new ArgumentNullException("sectionGrades", "Section Grades object cannot be null");
            }

            var urlPath = UrlUtility.CombineUrlPath(new[] { _sectionsPath, sectionId, "grades" });
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion4);

            IEnumerable<SectionGradeResponse> updatedResponse = null;

            try
            {
                var response = await ExecutePutRequestWithResponseAsync<SectionGrades3>(sectionGrades, urlPath, headers: headers);
                updatedResponse = JsonConvert.DeserializeObject<IEnumerable<SectionGradeResponse>>(await response.Content.ReadAsStringAsync());
            }

            // If the HTTP request fails, the grades weren't updated successfull
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("Grades for section {0} failed.", sectionId), hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return updatedResponse;
        }

        /// <summary>
        /// Update student grades from ILP caller.
        /// </summary>
        /// <param name="sectionId">section id</param>
        /// <param name="sectionGrades">student grades</param>
        /// <returns></returns>
        public async Task<IEnumerable<SectionGradeResponse>> PutIlpCollectionOfStudentGrades1Async(string sectionId, SectionGrades3 sectionGrades)
        {
            if (sectionId == null)
            {
                throw new ArgumentNullException("sectionId", "Section ID cannot be null");
            }

            if (sectionGrades == null)
            {
                throw new ArgumentNullException("sectionGrades", "Section Grades object cannot be null");
            }

            var urlPath = UrlUtility.CombineUrlPath(new[] { _sectionsPath, sectionId, "grades" });
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderIlpVersion1);

            IEnumerable<SectionGradeResponse> updatedResponse = null;

            try
            {
                var response = await ExecutePutRequestWithResponseAsync<SectionGrades3>(sectionGrades, urlPath, headers: headers);
                updatedResponse = JsonConvert.DeserializeObject<IEnumerable<SectionGradeResponse>>(await response.Content.ReadAsStringAsync());
            }

            // If the HTTP request fails, the grades weren't updated successfull
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("Grades for section {0} failed.", sectionId), hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

            return updatedResponse;
        }

        /// <summary>
        /// Asynchronously returns the list of session cycles
        /// </summary>
        /// <returns>The requested list of <see cref="SessionCycle">SessionCycles</see></returns>
        public async Task<IEnumerable<SessionCycle>> GetSessionCyclesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_sessionCyclesPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<IEnumerable<SessionCycle>>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the session cycle list.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns the list of yearly cycles
        /// </summary>
        /// <returns>The requested list of <see cref="YearlyCycle">YearlyCycles</see></returns>
        public async Task<IEnumerable<YearlyCycle>> GetYearlyCyclesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_yearlyCyclesPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<IEnumerable<YearlyCycle>>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the yearly cycle list.");
                throw;
            }
        }

        /// <summary>
        /// Get subjects
        /// </summary>
        /// <returns>Returns all subjects</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Subject> GetSubjects()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_subjectsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Subject>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Subject>");
                throw;
            }
        }
        /// <summary>
        /// Get subjects async
        /// </summary>
        /// <returns>Returns all subjects</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Subject>> GetSubjectsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_subjectsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Subject>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Subject>");
                throw;
            }
        }
        /// <summary>
        /// Get instructional methods
        /// </summary>
        /// <returns>A set of instructional methods</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<InstructionalMethod> GetInstructionalMethods()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_instructionalMethodsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<InstructionalMethod>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<InstructionalMethod>");
                throw;
            }
        }
        /// <summary>
        /// Get instructional methods async.
        /// </summary>
        /// <returns>A set of instructional methods</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<InstructionalMethod>> GetInstructionalMethodsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_instructionalMethodsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<InstructionalMethod>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<InstructionalMethod>");
                throw;
            }
        }
        /// <summary>
        /// Get majors
        /// </summary>
        /// <returns>Returns majors</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Major> GetMajors()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_majorsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Major>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Major>");
                throw;
            }
        }
        /// <summary>
        /// Get majors async.
        /// </summary>
        /// <returns>Returns majors</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Major>> GetMajorsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_majorsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Major>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Major>");
                throw;
            }
        }
        /// <summary>
        /// Get minors
        /// </summary>
        /// <returns>Returns minors</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Minor> GetMinors()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_minorsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Minor>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Minor>");
                throw;
            }
        }
        /// <summary>
        /// Get minors async.
        /// </summary>
        /// <returns>Returns minors</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Minor>> GetMinorsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_minorsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Minor>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Minor>");
                throw;
            }
        }
        /// <summary>
        /// Get CCDs
        /// </summary>
        /// <returns>Returns CCDs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Ccd> GetCcds()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_ccdsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ccd>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Ccd>");
                throw;
            }
        }
        /// <summary>
        /// Get CCDs async.
        /// </summary>
        /// <returns>Returns CCDs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Ccd>> GetCcdsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_ccdsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ccd>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Ccd>");
                throw;
            }
        }
        /// <summary>
        /// Get specializations
        /// </summary>
        /// <returns>Returns specializations</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Specialization> GetSpecializations()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_specializationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Specialization>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Specialization>");
                throw;
            }
        }
        /// <summary>
        /// Get specializations async.
        /// </summary>
        /// <returns>Returns specializations</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Specialization>> GetSpecializationsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_specializationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Specialization>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Specialization>");
                throw;
            }
        }
        /// <summary>
        /// Get degrees
        /// </summary>
        /// <returns>Returns degrees</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Degree> GetDegrees()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Degree>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Degree>");
                throw;
            }
        }
        /// <summary>
        /// Get degrees async.
        /// </summary>
        /// <returns>Returns degrees</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Degree>> GetDegreesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Degree>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Degree>");
                throw;
            }
        }
        /// <summary>
        /// Get list of students by a query.
        /// For a termId, return students registered in that term.
        /// </summary>
        /// <returns>Returns a list of students for given selection criteria</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        //public Student PostStudentIds([FromBody] StudentQuery studentQuery)
        //{
        //    if (string.IsNullOrEmpty(studentQuery))
        //    {
        //        throw new ArgumentNullException("termId", "Term ID cannot be empty/null for Student retrieval.");
        //    }
        //    try
        //    {
        //        string urlPath = UrlUtility.CombineUrlPath(_studentIdsPath, studentQuery);
        //        var headers = new NameValueCollection();
        //        headers.Add(MediaTypeHeaderKey, _mediaTypeHeaderVersion1);
        //        var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
        //        var resource = JsonConvert.DeserializeObject<Student>(response.Content.ReadAsStringAsync().Result);
        //        return resource;
        //    }
        //    // Log any exception, then rethrow it and let calling code determine how to handle it.
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Unable to get Students");
        //        throw;
        //    }
        //}
        /// <summary>
        /// Get all terms
        /// </summary>
        /// <returns>Returns a set of all terms</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Term> GetTerms(DateTime? startsOnOrAfter = null)
        {
            try
            {
                string urlPath = "";
                if (startsOnOrAfter.HasValue)
                {
                    IDictionary<string, string> arguments = new Dictionary<string, string>();
                    arguments.Add("startsOnOrAfter", startsOnOrAfter.Value.ToString("s"));
                    urlPath = UrlUtility.CombineEncodedUrlPathAndArguments(_termsPath, arguments);
                }
                else
                {
                    urlPath = UrlUtility.CombineUrlPath(_termsPath);
                }

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Term>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Term>");
                throw;
            }
        }
        /// <summary>
        /// Get all terms async.
        /// </summary>
        /// <returns>Returns a set of all terms</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Term>> GetTermsAsync(DateTime? startsOnOrAfter = null)
        {
            try
            {
                string urlPath = "";
                if (startsOnOrAfter.HasValue)
                {
                    IDictionary<string, string> arguments = new Dictionary<string, string>();
                    arguments.Add("startsOnOrAfter", startsOnOrAfter.Value.ToString("s"));
                    urlPath = UrlUtility.CombineEncodedUrlPathAndArguments(_termsPath, arguments);
                }
                else
                {
                    urlPath = UrlUtility.CombineUrlPath(_termsPath);
                }

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Term>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Term>");
                throw;
            }
        }
        /// <summary>
        /// Get the requested terms
        /// </summary>
        /// <returns>Returns the set of requested terms</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public Term GetTerm(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Id cannot be empty/null for IEnumerable<Term> retrieval.");
            }
            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "id", id });
                var urlPath = UrlUtility.CombineUrlPathAndArguments(_termsPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Term>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Term");
                throw;
            }
        }
        /// <summary>
        /// Get the requested terms async.
        /// </summary>
        /// <returns>Returns the set of requested terms</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<Term> GetTermAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Id cannot be empty/null for IEnumerable<Term> retrieval.");
            }
            try
            {
                var query = UrlUtility.BuildEncodedQueryString(new[] { "id", id });
                var urlPath = UrlUtility.CombineUrlPathAndArguments(_termsPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Term>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Term");
                throw;
            }
        }
        /// <summary>
        /// Gets a list of all terms that are valid for planning (i.e. can appear on a degree plan).
        /// </summary>
        /// <returns>The full list of planning terms</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Term> GetPlanningTerms()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_termsPath, "planning");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Term>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Term>");
                throw;
            }
        }
        /// <summary>
        /// Gets a list of all terms that are valid for planning (i.e. can appear on a degree plan) async.
        /// </summary>
        /// <returns>The full list of planning terms</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Term>> GetPlanningTermsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_termsPath, "planning");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Term>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Term>");
                throw;
            }
        }
        /// <summary>
        /// Gets a list of all terms that are valid for registration
        /// </summary>
        /// <returns>The full list of registration terms</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Term> GetRegistrationTerms()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_termsPath, "registration");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Term>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Term>");
                throw;
            }
        }
        /// <summary>
        /// Gets a list of all terms that are valid for registration async.
        /// </summary>
        /// <returns>The full list of registration terms</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Term>> GetRegistrationTermsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_termsPath, "registration");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Term>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Term>");
                throw;
            }
        }
        /// <summary>
        /// Returns registration eligibility information for a student
        /// </summary>
        /// <param name="studentId">Id of student to check registration eligibility for</param>
        /// <returns><see cref="RegistrationEligibility">Registration Eligibility Information</see> including messages returned by 
        /// the student eligibility check and booleans indicating whether the student is eligible and whether the user can override and
        /// register the student even if the student is ineligible.</returns>
        public RegistrationEligibility CheckRegistrationEligibility(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id must contain a valid value.");
            }

            var urlPath = UrlUtility.CombineUrlPath(new[] { _studentsPath, studentId, "registration-eligibility" });

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            try
            {
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var registrationEligibility = JsonConvert.DeserializeObject<RegistrationEligibility>(response.Content.ReadAsStringAsync().Result);
                return registrationEligibility;
            }
            catch (Exception exception)
            {
                logger.Error(exception.ToString());
                throw new InvalidOperationException("Unable to determine registration eligibility.", exception);
            }
        }
        /// <summary>
        /// Returns registration eligibility information for a student async.
        /// </summary>
        /// <param name="studentId">Id of student to check registration eligibility for</param>
        /// <returns><see cref="RegistrationEligibility">Registration Eligibility Information</see> including messages returned by 
        /// the student eligibility check and booleans indicating whether the student is eligible and whether the user can override and
        /// register the student even if the student is ineligible.</returns>
        public async Task<RegistrationEligibility> CheckRegistrationEligibilityAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id must contain a valid value.");
            }

            var urlPath = UrlUtility.CombineUrlPath(new[] { _studentsPath, studentId, "registration-eligibility" });

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var registrationEligibility = JsonConvert.DeserializeObject<RegistrationEligibility>(await response.Content.ReadAsStringAsync());
                return registrationEligibility;
            }
            catch (Exception exception)
            {
                logger.Error(exception.ToString());
                throw new InvalidOperationException("Unable to determine registration eligibility.", exception);
            }
        }
        /// <summary>
        /// Get the set of Admitted Statuses
        /// </summary>
        /// <returns>Returns the admitted statuses</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<AdmittedStatus> GetAdmittedStatuses()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_admittedStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AdmittedStatus>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get admitted statuses");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Admitted Statuses async.
        /// </summary>
        /// <returns>Returns the admitted statuses</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<AdmittedStatus>> GetAdmittedStatusesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_admittedStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AdmittedStatus>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get admitted statuses");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Application Statuses
        /// </summary>
        /// <returns>Returns the application statuses</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<ApplicationStatus> GetApplicationStatuses()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_applicationStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ApplicationStatus>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get application statuses");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Application Statuses async.
        /// </summary>
        /// <returns>Returns the application statuses</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ApplicationStatus>> GetApplicationStatusesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_applicationStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ApplicationStatus>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get application statuses");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Application Status Categories
        /// </summary>
        /// <returns>Returns the application status categories</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<ApplicationStatusCategory> GetApplicationStatusCategories()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_applicationStatusCategoriesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ApplicationStatusCategory>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get application status categories");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Application Status Categories async.
        /// </summary>
        /// <returns>Returns the application status categories</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ApplicationStatusCategory>> GetApplicationStatusCategoriesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_applicationStatusCategoriesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ApplicationStatusCategory>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get application status categories");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Application Influences
        /// </summary>
        /// <returns>Returns the application influences</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<ApplicationInfluence> GetApplicationInfluences()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_applicationInfluencesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ApplicationInfluence>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get application influences");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Application Influences async.
        /// </summary>
        /// <returns>Returns the application influences</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ApplicationInfluence>> GetApplicationInfluencesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_applicationInfluencesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ApplicationInfluence>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get application influences");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Career Goals
        /// </summary>
        /// <returns>Returns the career goals</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<CareerGoal> GetCareerGoals()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_careerGoalsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CareerGoal>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get career goals");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Career Goals async.
        /// </summary>
        /// <returns>Returns the career goals</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<CareerGoal>> GetCareerGoalsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_careerGoalsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CareerGoal>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get career goals");
                throw;
            }
        }
        /// <summary>
        /// Get the set of External Transcript Statuses
        /// </summary>
        /// <returns>Returns the external transcript statuses</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<ExternalTranscriptStatus> GetExternalTranscriptStatuses()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_externalTranscriptStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ExternalTranscriptStatus>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get transcript statuses");
                throw;
            }
        }
        /// <summary>
        /// Get the set of External Transcript Statuses async.
        /// </summary>
        /// <returns>Returns the external transcript statuses</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ExternalTranscriptStatus>> GetExternalTranscriptStatusesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_externalTranscriptStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ExternalTranscriptStatus>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get transcript statuses");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Student Loads
        /// </summary>
        /// <returns>Returns the student loads</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<StudentLoad> GetStudentLoads()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentLoadsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentLoad>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student loads");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Student Loads async.
        /// </summary>
        /// <returns>Returns the student loads</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<StudentLoad>> GetStudentLoadsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentLoadsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentLoad>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student loads");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Transcript Categories
        /// </summary>
        /// <returns>Returns the transcript categories</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<TranscriptCategory> GetTranscriptCategories()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_transcriptCategoriesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<TranscriptCategory>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get transcript categories");
                throw;
            }
        }
        /// <summary>
        /// Get the set of Transcript Categories async.
        /// </summary>
        /// <returns>Returns the transcript categories</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<TranscriptCategory>> GetTranscriptCategoriesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_transcriptCategoriesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<TranscriptCategory>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get transcript categories");
                throw;
            }
        }
        /// <summary>
        /// Updates an application status from Recruiter.
        /// </summary>
        /// <param name="updatedApplication">The updated application</param>
        public void UpdateApplication(Application updatedApplication)
        {
            if (updatedApplication == null)
            {
                throw new ArgumentNullException("updatedApplication", "Updated application cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterApplicationStatusesPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = ExecutePostRequestWithResponse<Application>(updatedApplication, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update application status");
                throw;
            }
        }
        /// <summary>
        /// Updates an application status from Recruiter async.
        /// </summary>
        /// <param name="updatedApplication">The updated application</param>
        public async Task UpdateApplicationAsync(Application updatedApplication)
        {
            if (updatedApplication == null)
            {
                throw new ArgumentNullException("updatedApplication", "Updated application cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterApplicationStatusesPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = await ExecutePostRequestWithResponseAsync<Application>(updatedApplication, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update application status");
                throw;
            }
        }
        /// <summary>
        /// Imports an application from Recruiter.
        /// </summary>
        /// <param name="importedApplication">The imported application</param>
        public void ImportApplication(Application importedApplication)
        {
            if (importedApplication == null)
            {
                throw new ArgumentNullException("importedApplication", "Imported application cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterApplicationsPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = ExecutePostRequestWithResponse<Application>(importedApplication, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to import application");
                throw;
            }
        }
        /// <summary>
        /// Imports an application from Recruiter async.
        /// </summary>
        /// <param name="importedApplication">The imported application</param>
        public async Task ImportApplicationAsync(Application importedApplication)
        {
            if (importedApplication == null)
            {
                throw new ArgumentNullException("importedApplication", "Imported application cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterApplicationsPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = await ExecutePostRequestWithResponseAsync<Application>(importedApplication, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to import application");
                throw;
            }
        }
        /// <summary>
        /// Imports communication history from Recruiter.
        /// </summary>
        /// <param name="importedCommunicationHistory">The imported communication history</param>
        public void ImportCommunicationHistory(CommunicationHistory importedCommunicationHistory)
        {
            if (importedCommunicationHistory == null)
            {
                throw new ArgumentNullException("importedCommunicationHistory", "Imported communication history cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterCommunicationHistoryPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = ExecutePostRequestWithResponse<CommunicationHistory>(importedCommunicationHistory, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to import communication history");
                throw;
            }
        }
        /// <summary>
        /// Imports communication history from Recruiter.
        /// </summary>
        /// <param name="importedCommunicationHistory">The imported communication history</param>
        public async Task ImportCommunicationHistoryAsync(CommunicationHistory importedCommunicationHistory)
        {
            if (importedCommunicationHistory == null)
            {
                throw new ArgumentNullException("importedCommunicationHistory", "Imported communication history cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterCommunicationHistoryPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = await ExecutePostRequestWithResponseAsync<CommunicationHistory>(importedCommunicationHistory, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to import communication history");
                throw;
            }
        }
        /// <summary>
        /// Requests communication history from Colleague to Recruiter.
        /// </summary>
        /// <param name="requestedCommunicationHistory">The communication history request</param>
        public void RequestCommunicationHistory(CommunicationHistory requestedCommunicationHistory)
        {
            if (requestedCommunicationHistory == null)
            {
                throw new ArgumentNullException("requestedCommunicationHistory", "Communication history request cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterCommunicationHistoryRequestPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = ExecutePostRequestWithResponse<CommunicationHistory>(requestedCommunicationHistory, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to request communication history");
                throw;
            }
        }
        /// <summary>
        /// Requests communication history from Colleague to Recruiter async.
        /// </summary>
        /// <param name="requestedCommunicationHistory">The communication history request</param>
        public async Task RequestCommunicationHistoryAsync(CommunicationHistory requestedCommunicationHistory)
        {
            if (requestedCommunicationHistory == null)
            {
                throw new ArgumentNullException("requestedCommunicationHistory", "Communication history request cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterCommunicationHistoryRequestPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = await ExecutePostRequestWithResponseAsync<CommunicationHistory>(requestedCommunicationHistory, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to request communication history");
                throw;
            }
        }
        /// <summary>
        /// Imports test score from Recruiter.
        /// </summary>
        /// <param name="importedTestScore">The imported test score</param>
        public void ImportTestScore(TestScore importedTestScore)
        {
            if (importedTestScore == null)
            {
                throw new ArgumentNullException("importedTestScore", "Imported test score cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterTestScoresPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = ExecutePostRequestWithResponse<TestScore>(importedTestScore, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to import test score");
                throw;
            }
        }
        /// <summary>
        /// Imports test score from Recruiter async.
        /// </summary>
        /// <param name="importedTestScore">The imported test score</param>
        public async Task ImportTestScoreAsync(TestScore importedTestScore)
        {
            if (importedTestScore == null)
            {
                throw new ArgumentNullException("importedTestScore", "Imported test score cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterTestScoresPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = await ExecutePostRequestWithResponseAsync<TestScore>(importedTestScore, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to import test score");
                throw;
            }
        }
        /// <summary>
        /// Imports transcript course from Recruiter.
        /// </summary>
        /// <param name="importedTranscriptCourse">The imported transcript course</param>
        public void ImportTranscriptCourse(TranscriptCourse importedTranscriptCourse)
        {
            if (importedTranscriptCourse == null)
            {
                throw new ArgumentNullException("importedTranscriptCourse", "Imported transcript course cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterTranscriptCoursesPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = ExecutePostRequestWithResponse<TranscriptCourse>(importedTranscriptCourse, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to import transcript course");
                throw;
            }
        }
        /// <summary>
        /// Imports transcript course from Recruiter async.
        /// </summary>
        /// <param name="importedTranscriptCourse">The imported transcript course</param>
        public async Task ImportTranscriptCourseAsync(TranscriptCourse importedTranscriptCourse)
        {
            if (importedTranscriptCourse == null)
            {
                throw new ArgumentNullException("importedTranscriptCourse", "Imported transcript course cannot be null");
            }

            string urlPath = UrlUtility.CombineUrlPath(_recruiterTranscriptCoursesPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = await ExecutePostRequestWithResponseAsync<TranscriptCourse>(importedTranscriptCourse, urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to import transcript course");
                throw;
            }
        }
        // <summary>
        // Posts connection status from Colleague to Recruiter.
        // </summary>       
        public ConnectionStatus PostConnectionStatus(ConnectionStatus connectionStatus)
        {
            string urlPath = UrlUtility.CombineUrlPath(_recruiterConnectionStatusPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            ConnectionStatus newConnectionStatus = null;

            try
            {
                var response = ExecutePostRequestWithResponse<ConnectionStatus>(connectionStatus, urlPath, headers: headers);
                newConnectionStatus = JsonConvert.DeserializeObject<ConnectionStatus>(response.Content.ReadAsStringAsync().Result);
                return newConnectionStatus;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to post connection status");
                throw;
            }
        }
        // <summary>
        // Posts connection status from Colleague to Recruiter async.
        // </summary>       
        public async Task<ConnectionStatus> PostConnectionStatusAsync(ConnectionStatus connectionStatus)
        {
            string urlPath = UrlUtility.CombineUrlPath(_recruiterConnectionStatusPath);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            ConnectionStatus newConnectionStatus = null;

            try
            {
                var response = await ExecutePostRequestWithResponseAsync<ConnectionStatus>(connectionStatus, urlPath, headers: headers);
                newConnectionStatus = JsonConvert.DeserializeObject<ConnectionStatus>(await response.Content.ReadAsStringAsync());
                return newConnectionStatus;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to post connection status");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Academic History for a list of Student Ids
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) if true, include active credits only</param>
        /// <param name="term">(Optional) Term filter for academic history</param>
        /// <returns>list of Academic History objects</returns>
        [Obsolete("Obsolete as of API 1.18. Use GetAcademicHistoryByIds2.")]
        public IEnumerable<AcademicHistoryBatch> GetAcademicHistoryByIds(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, string term = null)
        {
            AcademicHistoryQueryCriteria criteria = new AcademicHistoryQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.BestFit = bestFit;
            criteria.Filter = filter;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Academic History retrieval.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _academicHistoryPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<AcademicHistoryBatch>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve academic histories.");
                throw;
            }
        }

        /// <summary>
        /// Retrieve Academic History for a list of Student Ids
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) if true, include active credits only</param>
        /// <param name="term">(Optional) Term filter for academic history</param>
        /// <returns>list of Academic History objects</returns>
        public async Task<IEnumerable<AcademicHistoryBatch2>> GetAcademicHistoryByIds2(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, string term = null)
        {
            AcademicHistoryQueryCriteria criteria = new AcademicHistoryQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.BestFit = bestFit;
            criteria.Filter = filter;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Academic History retrieval.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _academicHistoryPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<AcademicHistoryBatch2>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve academic histories.");
                throw;
            }
        }

        /// <summary>
        /// Retrieve Academic History for a list of Student Ids async.
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) if true, include active credits only</param>
        /// <param name="term">(Optional) Term filter for academic history</param>
        /// <returns>list of Academic History objects</returns>
        public async Task<IEnumerable<AcademicHistoryBatch>> GetAcademicHistoryByIdsAsync(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, string term = null)
        {
            AcademicHistoryQueryCriteria criteria = new AcademicHistoryQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.BestFit = bestFit;
            criteria.Filter = filter;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Academic History retrieval.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _academicHistoryPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<AcademicHistoryBatch>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve academic histories.");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Academic History for a list of Student Ids async.
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) if true, include active credits only</param>
        /// <param name="term">(Optional) Term filter for academic history</param>
        /// <returns>list of Academic History objects</returns>
        [Obsolete("Obsolete as of API 1.10. Use GetAcademicHistoryLevel2ByIdsAsync.")]
        public async Task<IEnumerable<AcademicHistoryLevel>> GetAcademicHistoryLevelByIdsAsync(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, string term = null)
        {
            AcademicHistoryQueryCriteria criteria = new AcademicHistoryQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.BestFit = bestFit;
            criteria.Filter = filter;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Academic History Level retrieval.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _academicHistoryLevelPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<AcademicHistoryLevel>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve academic history levels.");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Academic History for a list of Student Ids
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) if true, include active credits only</param>
        /// <param name="term">(Optional) Term filter for academic history</param>
        /// <returns>list of Academic History objects</returns>
        [Obsolete("Obsolete as of API 1.10. Use GetAcademicHistoryLevel2ByIds.")]
        public IEnumerable<AcademicHistoryLevel> GetAcademicHistoryLevelByIds(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, string term = null)
        {
            AcademicHistoryQueryCriteria criteria = new AcademicHistoryQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.BestFit = bestFit;
            criteria.Filter = filter;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Academic History Level retrieval.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _academicHistoryLevelPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<AcademicHistoryLevel>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve academic history levels.");
                throw;
            }
        }

        /// <summary>
        /// Retrieve Academic History for a list of Student Ids async.
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) if true, include active credits only</param>
        /// <param name="term">(Optional) Term filter for academic history</param>
        /// <returns>list of AcademicHistoryLevel2 DTO objects</returns>
        public async Task<IEnumerable<AcademicHistoryLevel2>> GetAcademicHistoryLevel2ByIdsAsync(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, string term = null)
        {
            AcademicHistoryQueryCriteria criteria = new AcademicHistoryQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.BestFit = bestFit;
            criteria.Filter = filter;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Academic History Level retrieval.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _academicHistoryLevelPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<AcademicHistoryLevel2>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve academic history levels.");
                throw;
            }
        }

        /// <summary>
        /// Retrieve Academic History for a list of Student Ids
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) if true, include active credits only</param>
        /// <param name="term">(Optional) Term filter for academic history</param>
        /// <returns>list of Academic History objects</returns>
        public IEnumerable<AcademicHistoryLevel2> GetAcademicHistoryLevel2ByIds(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, string term = null)
        {
            AcademicHistoryQueryCriteria criteria = new AcademicHistoryQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.BestFit = bestFit;
            criteria.Filter = filter;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Academic History Level retrieval.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _academicHistoryLevelPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<AcademicHistoryLevel2>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve academic history levels.");
                throw;
            }
        }

        /// <summary>
        /// Retrieve Pilot Academic History for a list of Student Ids, async version
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) if true, include active credits only</param>
        /// <param name="term">(Optional) Term filter for academic history</param>
        /// <param name="includeStudentSections">(Optional) Whether to include student sections in results.</param>
        /// <returns>list of Pilot Academic History objects</returns>
        public async Task<IEnumerable<PilotAcademicHistoryLevel>> GetPilotAcademicHistoryLevelByIdsAsync(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, string term = null, bool includeStudentSections = false)
        {
            AcademicHistoryQueryCriteria criteria = new AcademicHistoryQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.BestFit = bestFit;
            criteria.Filter = filter;
            criteria.Term = term;
            criteria.IncludeStudentSections = includeStudentSections;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Academic History Level retrieval.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _academicHistoryLevelPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPilotVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PilotAcademicHistoryLevel>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve academic history levels.");
                throw;
            }
        }

        /// <summary>
        /// Retrieve Pilot Academic History for a list of Student Ids
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) if true, include active credits only</param>
        /// <param name="term">(Optional) Term filter for academic history</param>
        /// <param name="includeStudentSections">(Optional) Whether to include student sections in results.</param>
        /// <returns>list of Pilot Academic History objects</returns>
        public IEnumerable<PilotAcademicHistoryLevel> GetPilotAcademicHistoryLevelByIds(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, string term = null, bool includeStudentSections = false)
        {
            AcademicHistoryQueryCriteria criteria = new AcademicHistoryQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.BestFit = bestFit;
            criteria.Filter = filter;
            criteria.Term = term;
            criteria.IncludeStudentSections = includeStudentSections;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Academic History Level retrieval.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _academicHistoryLevelPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPilotVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PilotAcademicHistoryLevel>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve academic history levels.");
                throw;
            }
        }

        /// <summary>
        /// Get a list of invalid student Enrollments
        /// </summary>
        /// <param name="enrollmentKeys">Contains Student ID and Section ID to find enrollment of a student in a class.</param>
        /// <returns>List of Invalid Student Enrollment Keys.</returns>
        public IEnumerable<StudentEnrollment> GetInvalidStudentEnrollment(IEnumerable<StudentEnrollment> enrollmentKeys)
        {
            if (enrollmentKeys == null)
            {
                throw new ArgumentNullException("enrollmentKeys", "Keys cannot be empty/null for Get Invalid Student Enrollment.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _studentEnrollmentKeysPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(enrollmentKeys, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentEnrollment>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student enrollment validations.");
                throw;
            }
        }
        /// <summary>
        /// Get a list of invalid student Enrollments async.
        /// </summary>
        /// <param name="enrollmentKeys">Contains Student ID and Section ID to find enrollment of a student in a class.</param>
        /// <returns>List of Invalid Student Enrollment Keys.</returns>
        public async Task<IEnumerable<StudentEnrollment>> GetInvalidStudentEnrollmentAsync(IEnumerable<StudentEnrollment> enrollmentKeys)
        {
            if (enrollmentKeys == null)
            {
                throw new ArgumentNullException("enrollmentKeys", "Keys cannot be empty/null for Get Invalid Student Enrollment.");
            }
            try
            {
                // Build url path from qapi path and academic history path
                string[] pathStrings = new string[] { _qapiPath, _studentEnrollmentKeysPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(enrollmentKeys, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentEnrollment>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student enrollment validations.");
                throw;
            }
        }
        /// <summary>
        /// Get a student's programs
        /// </summary>
        /// <returns>Returns the set of student's programs</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<StudentProgram2> GetStudentProgramsByIds(IEnumerable<string> studentIds, string term = null, bool includeHistory = false)
        {
            StudentProgramsQueryCriteria criteria = new StudentProgramsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Term = term;
            criteria.IncludeInactivePrograms = true;
            criteria.IncludeHistory = includeHistory;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<StudentProgram2> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student programs path
                string[] pathStrings = new string[] { _qapiPath, _studentProgramsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentProgram2>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student programs.");
                throw;
            }
        }
        /// <summary>
        /// Get a student's programs async
        /// </summary>
        /// <returns>Returns the set of student's programs</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<StudentProgram2>> GetStudentProgramsByIdsAsync(IEnumerable<string> studentIds, string term = null, bool includeHistory = false)
        {
            StudentProgramsQueryCriteria criteria = new StudentProgramsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Term = term;
            criteria.IncludeInactivePrograms = true;
            criteria.IncludeHistory = includeHistory;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<StudentProgram2> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student programs path
                string[] pathStrings = new string[] { _qapiPath, _studentProgramsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentProgram2>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student programs.");
                throw;
            }
        }
        /// <summary>
        /// Get Student Standings (Academic Standings) for a list of students
        /// </summary>
        /// <param name="studentIds">List of Student keys</param>
        /// <param name="term">(Optional) Term filter for student standings</param>
        /// <param name="currentTerm">(Optional) Current term to determine current academic standing</param>
        /// <returns>StudentStanding DTO objects</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<StudentStanding> GetStudentStandings(IEnumerable<string> studentIds, string term = null, string currentTerm = null)
        {
            StudentStandingsQueryCriteria criteria = new StudentStandingsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Term = term;
            criteria.CurrentTerm = currentTerm;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<StudentStanding> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student standings path
                string[] pathStrings = new string[] { _qapiPath, _studentStandingsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentStanding>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student standings.");
                throw;
            }
        }
        /// <summary>
        /// Get Student Standings (Academic Standings) for a list of students async.
        /// </summary>
        /// <param name="studentIds">List of Student keys</param>
        /// <param name="term">(Optional) Term filter for student standings</param>
        /// /// <param name="currentTerm">(Optional) Current term to determine current academic standing</param>
        /// <returns>StudentStanding DTO objects</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<StudentStanding>> GetStudentStandingsAsync(IEnumerable<string> studentIds, string term = null, string currentTerm = null)
        {
            StudentStandingsQueryCriteria criteria = new StudentStandingsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Term = term;
            criteria.CurrentTerm = currentTerm;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<StudentStanding> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student standings path
                string[] pathStrings = new string[] { _qapiPath, _studentStandingsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentStanding>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student standings.");
                throw;
            }
        }
        /// <summary>
        /// Get Student Terms (Student Load, etc.) for a list of students
        /// </summary>
        /// <param name="studentIds">List of Student keys</param>
        /// <param name="acadLevel">(Optional) Academic Level filter for student terms</param>
        /// <param name="term">(Optional) Term filter for student terms</param>
        /// <returns>StudentTerms DTO objects</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<StudentTerm> GetStudentTermsByStudentIds(IEnumerable<string> studentIds, string acadLevel = null, string term = null)
        {
            StudentTermsQueryCriteria criteria = new StudentTermsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.AcademicLevel = acadLevel;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<StudentTerm> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student standings path
                string[] pathStrings = new string[] { _qapiPath, _studentTermsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentTerm>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student terms.");
                throw;
            }
        }
        /// <summary>
        /// Get Student Terms (Student Load, etc.) for a list of students async.
        /// </summary>
        /// <param name="studentIds">List of Student keys</param>
        /// <param name="acadLevel">(Optional) Academic Level filter for student terms</param>
        /// <param name="term">(Optional) Term filter for student terms</param>
        /// <returns>StudentTerms DTO objects</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<StudentTerm>> GetStudentTermsByStudentIdsAsync(IEnumerable<string> studentIds, string acadLevel = null, string term = null)
        {
            StudentTermsQueryCriteria criteria = new StudentTermsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.AcademicLevel = acadLevel;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<StudentTerm> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student standings path
                string[] pathStrings = new string[] { _qapiPath, _studentTermsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentTerm>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student terms.");
                throw;
            }
        }
        /// <summary>
        /// Get Student Restrictions for a list of students
        /// </summary>
        /// <param name="studentIds">List of Student keys</param>
        /// <returns>PersonRestriction DTO objects</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<PersonRestriction> GetStudentRestrictionsByStudentIds(IEnumerable<string> studentIds)
        {
            StudentRestrictionsQueryCriteria criteria = new StudentRestrictionsQueryCriteria();
            criteria.StudentIds = studentIds;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<StudentStanding> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student standings path
                string[] pathStrings = new string[] { _qapiPath, _studentRestrictionsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PersonRestriction>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student restrictions.");
                throw;
            }
        }
        /// <summary>
        /// Get Student Restrictions for a list of students async.
        /// </summary>
        /// <param name="studentIds">List of Student keys</param>
        /// <returns>PersonRestriction DTO objects</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<PersonRestriction>> GetStudentRestrictionsByStudentIdsAsync(IEnumerable<string> studentIds)
        {
            StudentRestrictionsQueryCriteria criteria = new StudentRestrictionsQueryCriteria();
            criteria.StudentIds = studentIds;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "ID cannot be empty/null for IEnumerable<StudentStanding> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student standings path
                string[] pathStrings = new string[] { _qapiPath, _studentRestrictionsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PersonRestriction>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student restrictions.");
                throw;
            }
        }
        /// <summary>
        /// Get Student Restrictions for a student
        /// </summary>
        /// <param name="studentIds">Student ID</param>
        /// <returns>PersonRestriction DTO objects</returns>
        /// <exception cref="ArgumentNullException">The resource studentId must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception> 
        public IEnumerable<PersonRestriction> GetStudentRestrictions(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "ID cannot be empty/null for IEnumerable<PersonRestriction> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student standings path
                string[] pathStrings = new string[] { _studentsPath, studentId + "/restrictions" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers, useCache: false);
                return JsonConvert.DeserializeObject<IEnumerable<PersonRestriction>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student restrictions for student " + studentId + ".");
                throw;
            }
        }

        /// <summary>
        /// Get Student Restrictions for a student async.
        /// </summary>
        /// <param name="studentIds">Student ID</param>
        /// <param name="useCache">Use Cache</param>
        /// <returns>PersonRestriction DTO objects</returns>
        /// <exception cref="ArgumentNullException">The resource studentId must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>        
        public async Task<IEnumerable<PersonRestriction>> GetStudentRestrictionsAsync(string studentId, bool useCache = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "ID cannot be empty/null for IEnumerable<PersonRestriction> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student standings path
                string[] pathStrings = new string[] { _studentsPath, studentId + "/restrictions" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers, useCache: useCache);
                return JsonConvert.DeserializeObject<IEnumerable<PersonRestriction>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student restrictions for student " + studentId + ".");
                throw;
            }
        }

        /// <summary>
        /// Get Student Restrictions for a student async.
        /// </summary>
        /// <param name="studentIds">Student ID</param>
        /// <param name="useCache">Use Cache</param>
        /// <returns>PersonRestriction DTO objects</returns>
        /// <exception cref="ArgumentNullException">The resource studentId must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>        
        public async Task<IEnumerable<PersonRestriction>> GetStudentRestrictions2Async(string studentId, bool useCache = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "ID cannot be empty/null for IEnumerable<PersonRestriction> retrieval.");
            }
            try
            {
                // Build url path from qapi path and student standings path
                string[] pathStrings = new string[] { _studentsPath, studentId + "/restrictions" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers, useCache: useCache);
                return JsonConvert.DeserializeObject<IEnumerable<PersonRestriction>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student restrictions for student " + studentId + ".");
                throw;
            }
        }

        /// <summary>
        /// Get Course Objects without cache from a list of keys using a Post Transaction
        /// </summary>
        /// <param name="courseIds">List of Courses keys</param>
        /// <returns>List of Course Objects</returns>
        public IEnumerable<Course2> GetCoursesById(IEnumerable<string> courseIds)
        {
            CourseQueryCriteria criteria = new CourseQueryCriteria();
            criteria.CourseIds = courseIds;

            if (courseIds == null)
            {
                throw new ArgumentNullException("courseIds", "IDs cannot be empty/null for Course retrieval.");
            }
            try
            {
                // Build url path from qapi path and faculty path
                string[] pathStrings = new string[] { _qapiPath, _coursesPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Build url path

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Course2>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve courses.");
                throw;
            }
        }
        /// <summary>
        /// Get Course Objects without cache from a list of keys using a Post Transaction async.
        /// </summary>
        /// <param name="courseIds">List of Courses keys</param>
        /// <returns>List of Course Objects</returns>
        public async Task<IEnumerable<Course2>> GetCoursesByIdAsync(IEnumerable<string> courseIds)
        {
            CourseQueryCriteria criteria = new CourseQueryCriteria();
            criteria.CourseIds = courseIds;

            if (courseIds == null)
            {
                throw new ArgumentNullException("courseIds", "IDs cannot be empty/null for Course retrieval.");
            }
            try
            {
                // Build url path from qapi path and faculty path
                string[] pathStrings = new string[] { _qapiPath, _coursesPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Build url path

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Course2>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve courses.");
                throw;
            }
        }

        
        /// <summary>
        /// Retrieve Student Objects in Batch without cache based on a Post transaction with multiple keys async.
        /// </summary>
        /// <param name="studentIds">Post a list of student keys</param>
        /// <returns>list of StudentBatch3 objects</returns>
        public async Task<IEnumerable<StudentBatch3>> QueryStudentsById4Async(IEnumerable<string> studentIds)
        {
            StudentQueryCriteria criteria = new StudentQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.InheritFromPerson = false;
            criteria.GetDegreePlan = false;

            if (studentIds == null || studentIds.Count() == 0)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Student retrieval.");
            }
            try
            {
                // Build url path from qapi path and student path
                string[] pathStrings = new string[] { _qapiPath, _studentsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian-batch.v4+json");
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentBatch3>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve students.");
                throw;
            }
        }

        /// <summary>
        /// Retrieve Student Objects in Batch without cache based on a Post transaction with multiple keys
        /// </summary>
        /// <param name="studentIds">Post a list of student keys</param>
        /// <returns>list of StudentBatch objects</returns>
        public IEnumerable<StudentBatch3> QueryStudents4ById(IEnumerable<string> studentIds, string term)
        {
            StudentQueryCriteria criteria = new StudentQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.InheritFromPerson = false;
            criteria.GetDegreePlan = false;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Student retrieval.");
            }
            try
            {
                // Build url path from qapi path and student path
                string[] pathStrings = new string[] { _qapiPath, _studentsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian-batch.v4+json");
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentBatch3>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve students.");
                throw;
            }
        }

        /// <summary>
        /// Retrieve Student Affiliation Objects in Batch without cache based on a Post transaction with multiple keys
        /// </summary>
        /// <param name="studentIds">Post a list of student keys</param>
        /// <param name="term">Restrict selection of records to a specific term (Optional)</param>
        /// <param name="affiliationId">Restrict selection of records to a specific affiliation key (Optional)</param>
        /// <returns>list of StudentAffiliation objects</returns>
        public IEnumerable<StudentAffiliation> QueryStudentAffiliations(IEnumerable<string> studentIds, string term = null, string affiliationId = null)
        {
            StudentAffiliationQueryCriteria criteria = new StudentAffiliationQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Term = term;
            criteria.AffiliationCode = affiliationId;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Student Affiliation retrieval.");
            }
            try
            {
                // Build url path from qapi path and student path
                string[] pathStrings = new string[] { _qapiPath, _studentAffiliationsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentAffiliation>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student affiliations.");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Student Affiliation Objects in Batch without cache based on a Post transaction with multiple keys async.
        /// </summary>
        /// <param name="studentIds">Post a list of student keys</param>
        /// <param name="term">Restrict selection of records to a specific term (Optional)</param>
        /// <param name="affiliationId">Restrict selection of records to a specific affiliation key (Optional)</param>
        /// <returns>list of StudentAffiliation objects</returns>
        public async Task<IEnumerable<StudentAffiliation>> QueryStudentAffiliationsAsync(IEnumerable<string> studentIds, string term = null, string affiliationId = null)
        {
            StudentAffiliationQueryCriteria criteria = new StudentAffiliationQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Term = term;
            criteria.AffiliationCode = affiliationId;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Student Affiliation retrieval.");
            }
            try
            {
                // Build url path from qapi path and student path
                string[] pathStrings = new string[] { _qapiPath, _studentAffiliationsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentAffiliation>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student affiliations.");
                throw;
            }
        }
        /// <summary>
        /// Get the Education History from a list of Student Ids
        /// </summary>
        /// <param name="studentIds">List of Student Keys</param>
        /// <returns>List of Student Program objects</returns>
        public IEnumerable<EducationHistory> GetEducationHistoryByIds(IEnumerable<string> studentIds)
        {
            EducationHistoryQueryCriteria criteria = new EducationHistoryQueryCriteria();
            criteria.StudentIds = studentIds;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Education History retrieval.");
            }
            try
            {
                // Build url path from qapi path and test results path
                string[] pathStrings = new string[] { _qapiPath, _educationHistoryPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<EducationHistory>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve education histories.");
                throw;
            }
        }
        /// <summary>
        /// Get the Education History from a list of Student Ids async.
        /// </summary>
        /// <param name="studentIds">List of Student Keys</param>
        /// <returns>List of Student Program objects</returns>
        public async Task<IEnumerable<EducationHistory>> GetEducationHistoryByIdsAsync(IEnumerable<string> studentIds)
        {
            EducationHistoryQueryCriteria criteria = new EducationHistoryQueryCriteria();
            criteria.StudentIds = studentIds;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Education History retrieval.");
            }
            try
            {
                // Build url path from qapi path and test results path
                string[] pathStrings = new string[] { _qapiPath, _educationHistoryPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<EducationHistory>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve education histories.");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Faculty Objects without cache based on a Post transaction with multiple keys
        /// </summary>
        /// <param name="courseIds">Post in Body a list of faculty keys</param>
        /// <returns>list of faculty objects</returns>
        public IEnumerable<Faculty> GetFacultyByIds(IEnumerable<string> facultyIds)
        {
            FacultyQueryCriteria criteria = new FacultyQueryCriteria();
            criteria.FacultyIds = facultyIds;

            if (facultyIds == null)
            {
                throw new ArgumentNullException("facultyIds", "IDs cannot be empty/null for Faculty retrieval.");
            }
            try
            {
                // Build url path from qapi path and faculty path
                string[] pathStrings = new string[] { _qapiPath, _facultyPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Faculty>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve faculty.");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Faculty Objects without cache based on a Post transaction with multiple keys async.
        /// </summary>
        /// <param name="courseIds">Post in Body a list of faculty keys</param>
        /// <returns>list of faculty objects</returns>
        public async Task<IEnumerable<Faculty>> GetFacultyByIdsAsync(IEnumerable<string> facultyIds)
        {
            FacultyQueryCriteria criteria = new FacultyQueryCriteria();
            criteria.FacultyIds = facultyIds;

            if (facultyIds == null)
            {
                throw new ArgumentNullException("facultyIds", "IDs cannot be empty/null for Faculty retrieval.");
            }
            try
            {
                // Build url path from qapi path and faculty path
                string[] pathStrings = new string[] { _qapiPath, _facultyPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Faculty>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve faculty.");
                throw;
            }
        }
        /// <summary>
        /// Get all Academic Programs
        /// </summary>
        /// <returns>The set of all AcademicPrograms in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<AcademicProgram> GetAcademicPrograms()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_academicProgramsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicProgram>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get AcademicPrograms");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get AcademicPrograms");
                throw;
            }
        }
        /// <summary>
        /// Get all Academic Programs async.
        /// </summary>
        /// <returns>The set of all AcademicPrograms in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<AcademicProgram>> GetAcademicProgramsAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_academicProgramsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicProgram>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get AcademicPrograms");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get AcademicPrograms");
                throw;
            }
        }

        /// <summary>
        /// Get all Academic Catalogs async.
        /// </summary>
        /// <returns>The set of all AcademicCatalogs in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Catalog>> GetAllAcademicCatalogsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_academicCatalogsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Catalog>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get AcademicPrograms");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get AcademicPrograms");
                throw;
            }
        }

        /// <summary>
        /// Get all Advisor Types
        /// </summary>
        /// <returns>The set of all AdvisorTypes in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<AdvisorType> GetAdvisorTypes()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_advisorTypesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AdvisorType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get AdvisorTypes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get AdvisorTypes");
                throw;
            }
        }
        /// <summary>
        /// Get all Advisor Types async.
        /// </summary>
        /// <returns>The set of all AdvisorTypes in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<AdvisorType>> GetAdvisorTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_advisorTypesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AdvisorType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get AdvisorTypes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get AdvisorTypes");
                throw;
            }
        }
        /// <summary>
        /// Get all Credit Types
        /// </summary>
        /// <returns>The set of all CreditTypes in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<CredType> GetCreditTypes()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_creditTypesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CredType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get CreditTypes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get CreditTypes");
                throw;
            }
        }
        /// <summary>
        /// Get all Credit Types async.
        /// </summary>
        /// <returns>The set of all CreditTypes in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<CredType>> GetCreditTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_creditTypesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CredType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get CreditTypes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get CreditTypes");
                throw;
            }
        }
        /// <summary>
        /// Get all Federal Course Classifications
        /// </summary>
        /// <returns>The set of all FederalCourseClassifications in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<FederalCourseClassification> GetFederalCourseClassifications()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_federalCourseClassificationsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<FederalCourseClassification>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get FederalCourseClassifications");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get FederalCourseClassifications");
                throw;
            }
        }
        /// <summary>
        /// Get all Federal Course Classifications
        /// </summary>
        /// <returns>The set of all FederalCourseClassifications in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<FederalCourseClassification>> GetFederalCourseClassificationsAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_federalCourseClassificationsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<FederalCourseClassification>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get FederalCourseClassifications");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get FederalCourseClassifications");
                throw;
            }
        }
        /// <summary>
        /// Get all Local Course Classifications
        /// </summary>
        /// <returns>The set of all LocalCourseClassifications in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<LocalCourseClassification> GetLocalCourseClassifications()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_localCourseClassificationsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<LocalCourseClassification>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get LocalCourseClassifications");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get LocalCourseClassifications");
                throw;
            }
        }
        /// <summary>
        /// Get all Local Course Classifications async.
        /// </summary>
        /// <returns>The set of all LocalCourseClassifications in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<LocalCourseClassification>> GetLocalCourseClassificationsAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_localCourseClassificationsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<LocalCourseClassification>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get LocalCourseClassifications");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get LocalCourseClassifications");
                throw;
            }
        }
        /// <summary>
        /// Get all Restriction Types
        /// </summary>
        /// <returns>The set of all RestrictionTypes in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<RestrictionType> GetRestrictionTypes()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_restrictionTypesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<RestrictionType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get RestrictionTypes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get RestrictionTypes");
                throw;
            }
        }
        /// <summary>
        /// Get all Restriction Types
        /// </summary>
        /// <returns>The set of all RestrictionTypes in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<RestrictionType>> GetRestrictionTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_restrictionTypesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<RestrictionType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get RestrictionTypes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get RestrictionTypes");
                throw;
            }
        }
        /// <summary>
        /// Get all Student Types
        /// </summary>
        /// <returns>The set of all StudentTypes in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<StudentType> GetStudentTypes()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_studentTypesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get StudentTypes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get StudentTypes");
                throw;
            }
        }
        /// <summary>
        /// Get all Student Types
        /// </summary>
        /// <returns>The set of all StudentTypes in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<StudentType>> GetStudentTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_studentTypesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get StudentTypes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get StudentTypes");
                throw;
            }
        }
        /// <summary>
        /// Get a list of Student Programs from a list of Student Ids
        /// </summary>
        /// <param name="studentIds">List of Student Keys</param>
        /// <param name="type">Test Type of admissions, placement, or other</param>
        /// <returns>List of Student Program objects</returns>
        [Obsolete("Obsolete as of API 1.9. Use GetTestResults2ByIdsAsync.")]
        public IEnumerable<TestResult> GetTestResultsByIds(IEnumerable<string> studentIds, string type)
        {
            TestResultsQueryCriteria criteria = new TestResultsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Type = type;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for TestResults retrieval.");
            }
            try
            {
                // Build url path from qapi path and test results path
                string[] pathStrings = new string[] { _qapiPath, _testResultsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<TestResult>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve test results.");
                throw;
            }
        }
        /// <summary>
        /// Get a list of test results from a list of Student Ids
        /// </summary>
        /// <param name="studentIds">List of Student Keys</param>
        /// <param name="type">Test Type of admissions, placement, or other</param>
        /// <returns>List of Test Result Dtos</returns>
        [Obsolete("Obsolete as of API 1.15. Use GetTestResults2ByIdsAsync.")]
        public async Task<IEnumerable<TestResult>> GetTestResultsByIdsAsync(IEnumerable<string> studentIds, string type)
        {
            TestResultsQueryCriteria criteria = new TestResultsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Type = type;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for TestResults retrieval.");
            }
            try
            {
                // Build url path from qapi path and test results path
                string[] pathStrings = new string[] { _qapiPath, _testResultsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<TestResult>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve test results.");
                throw;
            }
        }
        /// <summary>
        /// Get a list of test results from a list of Student Ids
        /// </summary>
        /// <param name="studentIds">List of Student Keys</param>
        /// <param name="type">Test Type of admissions, placement, or other</param>
        /// <returns>List of Test Result Dtos</returns>
        public async Task<IEnumerable<TestResult2>> GetTestResults2ByIdsAsync(IEnumerable<string> studentIds, string type)
        {
            TestResultsQueryCriteria criteria = new TestResultsQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.Type = type;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for TestResults retrieval.");
            }
            try
            {
                // Build url path from qapi path and test results path
                string[] pathStrings = new string[] { _qapiPath, _testResultsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<TestResult2>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve test results.");
                throw;
            }
        }

        /// <summary>
        /// Get the test results for a single student.
        /// </summary>
        /// <param name="id">Student Id for whom the test results are requested</param>
        /// <returns>A list of objects for all of the TestResults for this student.</returns>
        [Obsolete("Obsolete as of API 1.9. Use GetTestResults2Async.")]
        public IEnumerable<TestResult> GetTestResults(string studentId, string type)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "You must provide the student Id to return test results.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId + "/test-results");
                if (!string.IsNullOrEmpty(type))
                {
                    var queryString = UrlUtility.BuildEncodedQueryString("type", type);
                    urlPath = urlPath + queryString;
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<TestResult>>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get TestResults");
                throw;
            }
        }

        /// <summary>
        /// Get the test results for a single student.
        /// </summary>
        /// <param name="id">Student Id for whom the test results are requested</param>
        /// <returns>A list of objects for all of the TestResults for this student.</returns>
        [Obsolete("Obsolete as of API 1.15. Use GetTestResults2Async.")]
        public async Task<IEnumerable<TestResult>> GetTestResultsAsync(string studentId, string type)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "You must provide the student Id to return test results.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId + "/test-results");
                if (!string.IsNullOrEmpty(type))
                {
                    var queryString = UrlUtility.BuildEncodedQueryString("type", type);
                    urlPath = urlPath + queryString;
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<TestResult>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get TestResults");
                throw;
            }
        }
        /// <summary>
        /// Get the test results for a single student.
        /// </summary>
        /// <param name="id">Student Id for whom the test results are requested</param>
        /// <returns>A list of objects for all of the TestResults for this student.</returns>
        public async Task<IEnumerable<TestResult2>> GetTestResults2Async(string studentId, string type)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "You must provide the student Id to return test results.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId + "/test-results");
                if (!string.IsNullOrEmpty(type))
                {
                    var queryString = UrlUtility.BuildEncodedQueryString("type", type);
                    urlPath = urlPath + queryString;
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<TestResult2>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get TestResults");
                throw;
            }
        }
        /// <summary>
        /// Get tests
        /// </summary>
        /// <returns>Returns tests</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Test> GetTests()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_testsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Test>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Test>");
                throw;
            }
        }
        /// <summary>
        /// Get tests
        /// </summary>
        /// <returns>Returns tests</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Test>> GetTestsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_testsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Test>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Test>");
                throw;
            }
        }
        /// <summary>
        /// Get Non-course Statuses
        /// </summary>
        /// <returns>Returns non-course statuses</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<NoncourseStatus> GetNoncourseStatuses()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_noncourseStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<NoncourseStatus>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<NoncourseStatuses>");
                throw;
            }
        }
        /// <summary>
        /// Get Non-course Statuses
        /// </summary>
        /// <returns>Returns non-course statuses</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<NoncourseStatus>> GetNoncourseStatusesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_noncourseStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<NoncourseStatus>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<NoncourseStatuses>");
                throw;
            }
        }
        /// <summary>
        /// Get all academic standings
        /// </summary>
        /// <returns>Returns the academic standings</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<AcademicStanding> GetAcademicStandings()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_academicStandingsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicStanding>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get academic standings");
                throw;
            }
        }
        /// <summary>
        /// Get all academic standings
        /// </summary>
        /// <returns>Returns the academic standings</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<AcademicStanding>> GetAcademicStandingsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_academicStandingsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicStanding>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get academic standings");
                throw;
            }
        }
        /// <summary>
        /// Get an Applicant by id
        /// </summary>
        /// <param name="applicantId">The applicant's Colleague PERSON id</param>
        /// <returns>An Applicant object</returns>
        public Applicant GetApplicant(string applicantId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_applicantPath, applicantId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Applicant>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get Applicant {0}", applicantId));
                throw;
            }

        }
        /// <summary>
        /// Get a person's emergency information.
        /// </summary>
        /// <param name="studentId">ID of the student whose emergency information is requested.</param>
        /// <returns>An EmergencyInformation object</returns>
        [Obsolete("Obsolete as of API 1.9. Use GetPersonEmergencyInformation.")]
        public EmergencyInformation GetEmergencyInformation(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "You must provide the student ID to return emergency information.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId + "/emergency-information");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<EmergencyInformation>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get EmergencyInformation for this person");
                throw;
            }
        }

        /// <summary>
        /// Update a person's emergency information.
        /// </summary>
        /// <param name="emergencyInformation">An EmergencyInformation object.</param>
        /// <returns>An updated EmergencyInformation object.</returns>
        [Obsolete("Obsolete as of API 1.9. Use UpdatePersonEmergencyInformation.")]
        public EmergencyInformation UpdateEmergencyInformation(EmergencyInformation emergencyInformation)
        {
            if (emergencyInformation == null)
            {
                throw new ArgumentNullException("emergencyInformation", "emergencyInformation cannot be null.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_studentsPath, emergencyInformation.PersonId + "/emergency-information");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePutRequestWithResponse<EmergencyInformation>(emergencyInformation, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<EmergencyInformation>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get PersonEmergencyInformation");
                throw;
            }
        }
        /// <summary>
        /// Get an Applicant by id
        /// </summary>
        /// <param name="applicantId">The applicant's Colleague PERSON id</param>
        /// <returns>An Applicant object</returns>
        public async Task<Applicant> GetApplicantAsync(string applicantId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_applicantPath, applicantId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Applicant>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get Applicant {0}", applicantId));
                throw;
            }

        }
        /// <summary>
        /// Process registration requests for student 
        /// </summary>
        /// <param name="studentId">Id of the student being registered</param>
        /// <param name="sectionRegistrations">Section registration items being submitted for registration</param>
        /// <returns>A registration Response containing any registration messages</returns>
        public RegistrationResponse Register(string studentId, IEnumerable<SectionRegistration> sectionRegistrations)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Must provide a student Id.");
            }
            if (sectionRegistrations == null || sectionRegistrations.Count() == 0)
            {
                throw new ArgumentNullException("sectionRegistrations", "sectionRegistration must contain at least 1 value.");
            }

            var urlPath = UrlUtility.CombineUrlPath(new[] { _studentsPath, studentId, "register" });

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = ExecutePutRequestWithResponse<IEnumerable<SectionRegistration>>(sectionRegistrations, urlPath, headers: headers);
                var messages = JsonConvert.DeserializeObject<RegistrationResponse>(response.Content.ReadAsStringAsync().Result);
                return messages;
            }
            catch (Exception exception)
            {
                logger.Error(exception.ToString());
                throw new InvalidOperationException("Unable to register.", exception);
            }
        }
        /// <summary>
        /// Process registration requests for student async.
        /// </summary>
        /// <param name="studentId">Id of the student being registered</param>
        /// <param name="sectionRegistrations">Section registration items being submitted for registration</param>
        /// <returns>A registration Response containing any registration messages</returns>
        public async Task<RegistrationResponse> RegisterAsync(string studentId, IEnumerable<SectionRegistration> sectionRegistrations)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Must provide a student Id.");
            }
            if (sectionRegistrations == null || sectionRegistrations.Count() == 0)
            {
                throw new ArgumentNullException("sectionRegistrations", "sectionRegistration must contain at least 1 value.");
            }

            var urlPath = UrlUtility.CombineUrlPath(new[] { _studentsPath, studentId, "register" });

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = await ExecutePutRequestWithResponseAsync<IEnumerable<SectionRegistration>>(sectionRegistrations, urlPath, headers: headers);
                var messages = JsonConvert.DeserializeObject<RegistrationResponse>(await response.Content.ReadAsStringAsync());
                return messages;
            }
            catch (Exception exception)
            {
                logger.Error(exception.ToString());
                throw new InvalidOperationException("Unable to register.", exception);
            }
        }
        public IEnumerable<TranscriptGrouping> GetSelectableTranscriptGroupings()
        {
            var urlPath = UrlUtility.CombineUrlPath(new[] { _transcriptGroupingsPath + "/selectable" });

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var transcriptGroupings = JsonConvert.DeserializeObject<IEnumerable<TranscriptGrouping>>(response.Content.ReadAsStringAsync().Result);
                return transcriptGroupings;
            }
            catch (Exception exception)
            {
                logger.Error(exception.ToString());
                throw new InvalidOperationException("Unable to get transcript groupings.", exception);
            }
        }
        public async Task<IEnumerable<TranscriptGrouping>> GetSelectableTranscriptGroupingsAsync()
        {
            var urlPath = UrlUtility.CombineUrlPath(new[] { _transcriptGroupingsPath + "/selectable" });

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var transcriptGroupings = JsonConvert.DeserializeObject<IEnumerable<TranscriptGrouping>>(await response.Content.ReadAsStringAsync());
                return transcriptGroupings;
            }
            catch (Exception exception)
            {
                logger.Error(exception.ToString());
                throw new InvalidOperationException("Unable to get transcript groupings.", exception);
            }
        }
        /// <summary>
        /// Get a student's transcript viewing restrictions
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <returns>A list of transcript viewing restrictions. If none it is an empty list.</returns>
        /// <summary>
        /// Get a student's transcript viewing restrictions
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <returns>Information used to determine if a student should be prevented from seeing or requesting their transcript.</returns>
        [Obsolete("Obsolete as of API 1.13. Use GetTranscriptRestrictions2Async.")]
        public TranscriptAccess GetTranscriptRestrictions2(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null to get transcript restrictions.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, "transcript-restrictions");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var transcriptAccess = JsonConvert.DeserializeObject<TranscriptAccess>(response.Content.ReadAsStringAsync().Result);
                return transcriptAccess;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get transcript restrictions.");
                throw new InvalidOperationException("Unable to get transcript restrictions.", ex);
            }
        }

        /// <summary>
        /// Get a student's transcript viewing restrictions
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <returns>A list of transcript viewing restrictions. If none it is an empty list.</returns>
        [Obsolete("Obsolete as of API 1.9. Use / GetTranscriptRestrictions2.")]
        public IEnumerable<TranscriptRestriction> GetTranscriptRestrictions(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null to get transcript restrictions.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, "transcript-restrictions");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var transcriptRestrictions = JsonConvert.DeserializeObject<IEnumerable<TranscriptRestriction>>(response.Content.ReadAsStringAsync().Result);
                return transcriptRestrictions;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get transcript restrictions.");
                throw new InvalidOperationException("Unable to get transcript restrictions.", ex);
            }
        }
        /// <summary>
        /// Get a student's transcript viewing restrictions
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <returns>A list of transcript viewing restrictions. If none it is an empty list.</returns>
        public async Task<IEnumerable<TranscriptRestriction>> GetTranscriptRestrictionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null to get transcript restrictions.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, "transcript-restrictions");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var transcriptRestrictions = JsonConvert.DeserializeObject<IEnumerable<TranscriptRestriction>>(await response.Content.ReadAsStringAsync());
                return transcriptRestrictions;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get transcript restrictions.");
                throw new InvalidOperationException("Unable to get transcript restrictions.", ex);
            }
        }
        /// <summary>
        /// Get a student's transcript viewing restrictions asynchronously. 
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <returns>A list of transcript viewing restrictions. If none it is an empty list.</returns>
        /// <summary>
        /// Get a student's transcript viewing restrictions
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <returns>Information used to determine if a student should be prevented from seeing or requesting their transcript.</returns>
        public async Task<TranscriptAccess> GetTranscriptRestrictions2Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null to get transcript restrictions.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, "transcript-restrictions");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var transcriptAccess = JsonConvert.DeserializeObject<TranscriptAccess>(await response.Content.ReadAsStringAsync());
                return transcriptAccess;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get transcript restrictions.");
                throw new InvalidOperationException("Unable to get transcript restrictions.", ex);
            }
        }
        /// <summary>
        /// Gets all the section transfer statuses
        /// </summary>
        /// <returns>A collection of all defined section transfer statuses</returns>
        public IEnumerable<SectionTransferStatus> GetSectionTransferStatuses()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sectionTransferStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<SectionTransferStatus>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get section transfer statuses");
                throw;
            }
        }
        /// <summary>
        /// Gets all the section transfer statuses
        /// </summary>
        /// <returns>A collection of all defined section transfer statuses</returns>
        public async Task<IEnumerable<SectionTransferStatus>> GetSectionTransferStatusesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sectionTransferStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<SectionTransferStatus>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get section transfer statuses");
                throw;
            }
        }
        /// <summary>
        /// Gets a faculty member's sections - either all or for selected dates
        /// </summary>
        /// <param name="facultyId">Id of the faculty for whom sections are desired. Required.</param>
        /// <param name="startDate">Optional, ISO-8601 short date format, yyyy-mm-dd, defaults to current date.</param>
        /// <param name="endDate">Optional, ISO-8601 short date format, yyyy-mm-dd, defaults to current date+90 days. Must be greater than start date if specified.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <returns>List of <see cref="Section3">Section3 Dtos</see></returns>
        public IEnumerable<Section3> GetFacultySections3(string facultyId, DateTime? startDate = null, DateTime? endDate = null, bool bestFit = false)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyId", "Faculty ID cannot be empty/null to retrieve faculty sections.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_facultyPath, facultyId, "sections");
                List<string> parameters = new List<string>() { "bestFit", bestFit.ToString() };
                if (startDate.HasValue)
                {
                    parameters.Add("startDate");
                    parameters.Add(startDate.Value.ToString("s"));
                }
                if (endDate.HasValue)
                {
                    parameters.Add("endDate");
                    parameters.Add(endDate.Value.ToString("s"));
                }
                var queryString = UrlUtility.BuildEncodedQueryString(parameters.ToArray());
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = ExecuteGetRequestWithResponse(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Section3>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Faculty Sections");
                throw;
            }
        }
        /// <summary>
        /// Gets a faculty member's sections - either all or for selected dates
        /// </summary>
        /// <param name="facultyId">Id of the faculty for whom sections are desired. Required.</param>
        /// <param name="startDate">Optional, ISO-8601 short date format, yyyy-mm-dd, defaults to current date.</param>
        /// <param name="endDate">Optional, ISO-8601 short date format, yyyy-mm-dd, defaults to current date+90 days. Must be greater than start date if specified.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <returns>List of <see cref="Section3">Section3 Dtos</see></returns>
        public async Task<IEnumerable<Section3>> GetFacultySections3Async(string facultyId, DateTime? startDate = null, DateTime? endDate = null, bool bestFit = false)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyId", "Faculty ID cannot be empty/null to retrieve faculty sections.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_facultyPath, facultyId, "sections");
                List<string> parameters = new List<string>() { "bestFit", bestFit.ToString() };
                if (startDate.HasValue)
                {
                    parameters.Add("startDate");
                    parameters.Add(startDate.Value.ToString("s"));
                }
                if (endDate.HasValue)
                {
                    parameters.Add("endDate");
                    parameters.Add(endDate.Value.ToString("s"));
                }
                var queryString = UrlUtility.BuildEncodedQueryString(parameters.ToArray());
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Section3>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Faculty Sections");
                throw;
            }
        }
        /// <summary>
        /// Gets a faculty member's sections either by date range or system parameters.  If a start date is not supplied it will retrieve sections based on the allowed
        /// terms defined on the RGWP, CSWP and GRWP parameter forms.
        /// </summary>
        /// <param name="facultyId">Id of the faculty for whom sections are desired. Required.</param>
        /// <param name="startDate">Optional, ISO-8601 short date format, yyyy-mm-dd</param>
        /// <param name="endDate">Optional, ISO-8601 short date format, yyyy-mm-dd, if start date is supplied this defaults to current date+90 days. Must be greater than start date if specified.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <param name="useCache">Flag indicating whether or not to use cached <see cref="Section3">course section</see> data. Defaults to true.</param>
        /// <returns>List of <see cref="Section3">Section3 Dtos</see></returns>
        public async Task<IEnumerable<Section3>> GetFacultySections4Async(string facultyId, DateTime? startDate = null, DateTime? endDate = null, bool bestFit = false, bool useCache = true)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyId", "Faculty ID cannot be empty/null to retrieve faculty sections.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_facultyPath, facultyId, "sections");
                List<string> parameters = new List<string>() { "bestFit", bestFit.ToString() };
                if (startDate.HasValue)
                {
                    parameters.Add("startDate");
                    parameters.Add(startDate.Value.ToString("s"));
                }
                if (endDate.HasValue)
                {
                    parameters.Add("endDate");
                    parameters.Add(endDate.Value.ToString("s"));
                }
                var queryString = UrlUtility.BuildEncodedQueryString(parameters.ToArray());
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion4);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers, useCache: useCache);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Section3>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Faculty Sections");
                throw;
            }
        }
        /// <summary>
        /// Returns a list of waiver objects for the given section.
        /// </summary>
        /// <param name="sectionId">Id of the section</param>
        /// <returns>List of <see cref="StudentWaiver">Waiver</see> objects</returns>
        public IEnumerable<StudentWaiver> GetSectionStudentWaivers(string sectionId)
        {
            try
            {
                if (string.IsNullOrEmpty(sectionId))
                {
                    throw new ArgumentNullException("sectionId", "Section ID cannot be empty/null to retrieve section waivers.");
                }
                string[] pathStrings = new string[] { _sectionsPath, UrlParameterUtility.EncodeWithSubstitution(sectionId), "student-waivers" };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentWaiver>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Section Waivers");
                throw;
            }
        }
        /// <summary>
        /// Returns a list of waiver objects for the given section.
        /// </summary>
        /// <param name="sectionId">Id of the section</param>
        /// <returns>List of <see cref="StudentWaiver">Waiver</see> objects</returns>
        public async Task<IEnumerable<StudentWaiver>> GetSectionStudentWaiversAsync(string sectionId)
        {
            try
            {
                if (string.IsNullOrEmpty(sectionId))
                {
                    throw new ArgumentNullException("sectionId", "Section ID cannot be empty/null to retrieve section waivers.");
                }
                string[] pathStrings = new string[] { _sectionsPath, UrlParameterUtility.EncodeWithSubstitution(sectionId), "student-waivers" };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentWaiver>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Section Waivers");
                throw;
            }
        }

        /// <summary>
        /// Returns a list of waiver objects for the given student.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>List of <see cref="StudentWaiver">Waiver</see> objects</returns>
        public async Task<IEnumerable<StudentWaiver>> GetStudentWaiversAsync(string studentId)
        {
            try
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    throw new ArgumentNullException("sectionId", "Student ID cannot be empty/null to retrieve section waivers.");
                }
                string[] pathStrings = new string[] { _studentsPath, UrlParameterUtility.EncodeWithSubstitution(studentId), "student-waivers" };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentWaiver>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student Waivers");
                throw;
            }
        }
        /// <summary>
        /// Returns a list of waiver reason codes and descriptions
        /// </summary>
        /// <returns>List of <see cref="StudentWaiverReason">WaiverReason</see> codes and descriptions</returns>
        public IEnumerable<StudentWaiverReason> GetStudentWaiverReasons()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentWaiverReasonsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentWaiverReason>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Student Waiver Reasons");
                throw;
            }
        }
        /// <summary>
        /// Returns a list of waiver reason codes and descriptions
        /// </summary>
        /// <returns>List of <see cref="StudentWaiverReason">WaiverReason</see> codes and descriptions</returns>
        public async Task<IEnumerable<StudentWaiverReason>> GetStudentWaiverReasonsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentWaiverReasonsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentWaiverReason>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Student Waiver Reasons");
                throw;
            }
        }
        public RegistrationOptions GetRegistrationOptions(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "You must provide the student Id to get registartion options.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId + "/registration-options");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<RegistrationOptions>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student's registration options");
                throw;
            }
        }
        public async Task<RegistrationOptions> GetRegistrationOptionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "You must provide the student Id to get registartion options.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId + "/registration-options");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<RegistrationOptions>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student's registration options");
                throw;
            }
        }
        /// <summary>
        /// Get the unofficial transcript for the specified student
        /// </summary>
        /// <param name="studentId">Student Id for whom the test results are requested</param>
        /// <param name="transcriptGrouping">Transcript grouping of the requested transcript</param>
        /// <param name="fileName">Filename of the pdf provided - output</param>
        /// <returns>Returns the student's unofficial transcript</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public byte[] GetUnofficialTranscript(string studentId, string transcriptGrouping, out string fileName)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null for unofficial transcript retrieval.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString("transcriptGrouping", transcriptGrouping);
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, "unofficial-transcript");
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
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
                logger.Error(ex, "Unable to get the unofficial transcript");
                throw;
            }
        }
        /// <summary>
        /// Get the unofficial transcript for the specified student asynchronously
        /// </summary>
        /// <param name="studentId">Student Id for whom the test results are requested</param>
        /// <param name="transcriptGrouping">Transcript grouping of the requested transcript</param>
        /// <param name="fileName">Filename of the pdf provided - output</param>
        /// <returns>Returns the student's unofficial transcript</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<Tuple<byte[], string>> GetUnofficialTranscriptAsync(string studentId, string transcriptGrouping)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null for unofficial transcript retrieval.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString("transcriptGrouping", transcriptGrouping);
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, "unofficial-transcript");
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add("Accept", "application/pdf");
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var fileName = response.Content.Headers.ContentDisposition.FileName;
                var resource = await response.Content.ReadAsByteArrayAsync();
                Tuple<byte[], string> fileInfoResource = new Tuple<byte[], string>(resource, fileName);
                return fileInfoResource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the unofficial transcript");
                throw;
            }
        }
        /// <summary>
        /// Gets the permissions for the active faculty. This can only be run for the logged-in faculty, and not for any faculty by ID.
        /// </summary>
        /// <returns>A set of strings enumerating the set of permissions allowed for the currently-logged-in faculty</returns>
        public IEnumerable<string> GetFacultyPermissions()
        {
            string urlPath = UrlUtility.CombineUrlPath(_facultyPath, "permissions");

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            IEnumerable<string> permissions = null;
            try
            {
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                permissions = JsonConvert.DeserializeObject<IEnumerable<string>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Faculty Permissions");
                throw;
            }

            return permissions;
        }
        /// <summary>
        /// Gets the permissions for the active faculty. This can only be run for the logged-in faculty, and not for any faculty by ID async.
        /// </summary>
        /// <returns>A set of strings enumerating the set of permissions allowed for the currently-logged-in faculty</returns>
        public async Task<IEnumerable<string>> GetFacultyPermissionsAsync()
        {
            string urlPath = UrlUtility.CombineUrlPath(_facultyPath, "permissions");

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            IEnumerable<string> permissions = null;
            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                permissions = JsonConvert.DeserializeObject<IEnumerable<string>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Faculty Permissions");
                throw;
            }

            return permissions;
        }
        /// <summary>
        /// Gets the permissions for the faculty. This can only be run for the logged-in faculty
        /// </summary>
        /// <returns>A FacultyPermission object with booleans that describe what the faculty can do.</returns>
        public async Task<FacultyPermissions> GetFacultyPermissions2Async()
        {
            string urlPath = UrlUtility.CombineUrlPath(_facultyPath, "permissions");

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            FacultyPermissions permissions = null;
            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                permissions = JsonConvert.DeserializeObject<FacultyPermissions>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while retrieving Faculty Permissions.");
                throw;
            }

            return permissions;
        }
        /// <summary>
        /// Client method to Post (create) a new waiver. 
        /// </summary>
        /// <param name="studentWaiver">Waiver object</param>
        /// <returns>Created Waiver object</returns>
        public StudentWaiver AddStudentWaiver(StudentWaiver studentWaiver)
        {
            try
            {
                if (studentWaiver == null)
                {
                    throw new ArgumentNullException("studentWaiver", "StudentWaiver cannot be null");
                }

                try
                {
                    string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentWaiver.StudentId, "student-waiver");
                    var headers = new NameValueCollection();
                    headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                    var response = ExecutePostRequestWithResponse<StudentWaiver>(studentWaiver, urlPath, headers: headers);
                    var resource = JsonConvert.DeserializeObject<StudentWaiver>(response.Content.ReadAsStringAsync().Result);
                    return studentWaiver;
                }
                // If the HTTP request fails, the waiver probably wasn't created successfully...
                catch (HttpRequestFailedException hre)
                {
                    logger.Error(hre.ToString());
                    throw new InvalidOperationException(string.Format("StudentWaiver creation failed."), hre);
                }
                // HTTP request successful, but some other problem encountered...
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create StudentWaiver");
                throw;
            }
        }
        /// <summary>
        /// Client method to Post (create) a new waiver. 
        /// </summary>
        /// <param name="studentWaiver">Waiver object</param>
        /// <returns>Created Waiver object</returns>
        public async Task<StudentWaiver> AddStudentWaiverAsync(StudentWaiver studentWaiver)
        {
            try
            {
                if (studentWaiver == null)
                {
                    throw new ArgumentNullException("studentWaiver", "StudentWaiver cannot be null");
                }

                try
                {
                    string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentWaiver.StudentId, "student-waiver");
                    var headers = new NameValueCollection();
                    headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                    var response = await ExecutePostRequestWithResponseAsync<StudentWaiver>(studentWaiver, urlPath, headers: headers);
                    var resource = JsonConvert.DeserializeObject<StudentWaiver>(await response.Content.ReadAsStringAsync());
                    return studentWaiver;
                }
                // If the HTTP request fails, the waiver probably wasn't created successfully...
                catch (HttpRequestFailedException hre)
                {
                    logger.Error(hre.ToString());
                    throw new InvalidOperationException(string.Format("StudentWaiver creation failed."), hre);
                }
                // HTTP request successful, but some other problem encountered...
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create StudentWaiver");
                throw;
            }
        }
        /// <summary>
        /// Returns a StudentWaiver as requested by Id
        /// </summary>
        /// <returns>The requested <see cref="StudentWaiver">StudentWaiver</see> object</returns>
        public StudentWaiver GetStudentWaiver(string studentId, string waiverId)
        {
            try
            {
                if (string.IsNullOrEmpty(waiverId))
                {
                    throw new ArgumentNullException("waiverId", "Waiver Id must be specified.");
                }
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, "student-waiver", waiverId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<StudentWaiver>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the requested Waiver");
                throw;
            }
        }
        /// <summary>
        /// Returns a StudentWaiver as requested by Id async.
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public async Task<StudentWaiver> GetStudentWaiverAsync(string studentId, string waiverId)
        {
            try
            {
                if (string.IsNullOrEmpty(waiverId))
                {
                    throw new ArgumentNullException("waiverId", "Waiver Id must be specified.");
                }
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, "student-waiver", waiverId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<StudentWaiver>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the requested Waiver");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Section Registration Date Objects based on a Post transaction with multiple keys
        /// </summary>
        /// <param name="ids">List of section Ids</param>
        /// <returns>Returns a set of <see cref="SectionRegistrationDate">SectionRegistrationDate</see> items</returns>
        public IEnumerable<SectionRegistrationDate> GetSectionRegistrationDates(IEnumerable<string> sectionIds)
        {

            if (sectionIds == null || sectionIds.Count() == 0)
            {
                throw new ArgumentNullException("sectionIds", "IDs cannot be empty/null for Section Registration Date retrieval.");
            }
            SectionDateQueryCriteria criteria = new SectionDateQueryCriteria();
            criteria.SectionIds = sectionIds;
            try
            {
                // Build url path from qapi path and sections and registration-dates pieces
                string[] pathStrings = new string[] { _qapiPath, _sectionsPath, "registration-dates" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);


                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<SectionRegistrationDate>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve section registration dates.");
                throw;
            }
        }
        /// <summary>
        /// Retrieve Section Registration Date Objects based on a Post transaction with multiple keys async.
        /// </summary>
        /// <param name="ids">List of section Ids</param>
        /// <returns>Returns a set of <see cref="SectionRegistrationDate">SectionRegistrationDate</see> items</returns>
        public async Task<IEnumerable<SectionRegistrationDate>> GetSectionRegistrationDatesAsync(IEnumerable<string> sectionIds)
        {

            if (sectionIds == null || sectionIds.Count() == 0)
            {
                throw new ArgumentNullException("sectionIds", "IDs cannot be empty/null for Section Registration Date retrieval.");
            }
            SectionDateQueryCriteria criteria = new SectionDateQueryCriteria();
            criteria.SectionIds = sectionIds;
            try
            {
                // Build url path from qapi path and sections and registration-dates pieces
                string[] pathStrings = new string[] { _qapiPath, _sectionsPath, "registration-dates" };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);


                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<SectionRegistrationDate>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve section registration dates.");
                throw;
            }
        }
        /// <summary>
        /// Returns a list of petition status codes and descriptions
        /// </summary>
        /// <returns>List of <see cref="PetitionStatus">PetitionStatus</see> codes and descriptions</returns>
        public IEnumerable<PetitionStatus> GetPetitionStatuses()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_petitionStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PetitionStatus>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Petition Statuses");
                throw;
            }
        }
        /// <summary>
        /// Returns a list of petition status codes and descriptions async.
        /// </summary>
        /// <returns>List of <see cref="PetitionStatus">PetitionStatus</see> codes and descriptions</returns>
        public async Task<IEnumerable<PetitionStatus>> GetPetitionStatusesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_petitionStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PetitionStatus>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Petition Statuses");
                throw;
            }
        }
        /// <summary>
        /// Returns a list of student petition reason codes and descriptions
        /// </summary>
        /// <returns>List of <see cref="StudentPetitionReason">StudentPetitionReason</see> codes and descriptions</returns>
        public IEnumerable<StudentPetitionReason> GetStudentPetitionReasons()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentPetitionReasonsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentPetitionReason>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Student Petition Reasons");
                throw;
            }
        }
        /// <summary>
        /// Returns a list of student petition reason codes and descriptions async.
        /// </summary>
        /// <returns>List of <see cref="StudentPetitionReason">StudentPetitionReason</see> codes and descriptions</returns>
        public async Task<IEnumerable<StudentPetitionReason>> GetStudentPetitionReasonsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentPetitionReasonsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentPetitionReason>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Student Petition Reasons");
                throw;
            }
        }
        /// <summary>
        /// Get a student by ID
        /// </summary>
        /// <returns>Returns a student</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public Student GetStudent(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Student retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Student>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Student");
                throw;
            }
        }
        /// <summary>
        /// Get a student by ID
        /// </summary>
        /// <returns>Returns a student</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<Student> GetStudentAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Student retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Student>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Student");
                throw;
            }
        }

        /// <summary>
        /// Get a person's emergency information async.
        /// </summary>
        /// <param name="studentId">ID of the student whose emergency information is requested.</param>
        /// <returns>An EmergencyInformation object</returns>
        public async Task<EmergencyInformation> GetEmergencyInformationAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "You must provide the student ID to return emergency information.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId + "/emergency-information");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<EmergencyInformation>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get EmergencyInformation for this person");
                throw;
            }
        }

        /// <summary>
        /// Update a person's emergency information async.
        /// </summary>
        /// <param name="emergencyInformation">An EmergencyInformation object.</param>
        /// <returns>An updated EmergencyInformation object.</returns>
        public async Task<EmergencyInformation> UpdateEmergencyInformationAsync(EmergencyInformation emergencyInformation)
        {
            if (emergencyInformation == null)
            {
                throw new ArgumentNullException("emergencyInformation", "emergencyInformation cannot be null.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_studentsPath, emergencyInformation.PersonId + "/emergency-information");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync<EmergencyInformation>(emergencyInformation, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<EmergencyInformation>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get PersonEmergencyInformation");
                throw;
            }
        }
        /// <summary>
        /// Retrieve faculty section permissions - consent & petitions
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public SectionPermission GetSectionPermissions(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Section ID cannot be empty/null to retrieve section permissions.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sectionsPath, sectionId, "section-permission");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<SectionPermission>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the section permissions");
                throw;
            }

        }
        /// <summary>
        /// Retrieve faculty section permissions - consent & petitions async.
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public async Task<SectionPermission> GetSectionPermissionsAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Section ID cannot be empty/null to retrieve section permissions.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sectionsPath, sectionId, "section-permission");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<SectionPermission>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the section permissions");
                throw;
            }

        }
        /// <summary>
        /// Client method to Post (add) a new student petition. 
        /// </summary>
        /// <param name="studentPetitionToAdd">StudentPetition object</param>
        /// <returns>The StudentPetition object created</returns>
        public StudentPetition AddStudentPetition(StudentPetition studentPetitionToAdd)
        {
            try
            {
                if (studentPetitionToAdd == null)
                {
                    throw new ArgumentNullException("studentPetitionToAdd", "studentPetitionToAdd cannot be null");
                }

                try
                {
                    string urlPath = UrlUtility.CombineUrlPath(_studentPetitionsPath);
                    var headers = new NameValueCollection();
                    headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                    var response = ExecutePostRequestWithResponse<StudentPetition>(objectToSend: studentPetitionToAdd, urlPath: urlPath, headers: headers);
                    var resource = JsonConvert.DeserializeObject<StudentPetition>(response.Content.ReadAsStringAsync().Result);
                    return resource;
                }
                // If the HTTP request fails, the student petition probably wasn't created successfully...
                catch (HttpRequestFailedException hre)
                {
                    logger.Error(hre.ToString());
                    throw new InvalidOperationException(string.Format("Student Petition add failed."), hre);
                }
                // HTTP request successful, but some other problem encountered...
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create Student Petition");
                throw;
            }
        }
        /// <summary>
        /// Client method to Post (add) a new student petition async. 
        /// </summary>
        /// <param name="studentPetitionToAdd">StudentPetition object</param>
        /// <returns>The StudentPetition object created</returns>
        public async Task<StudentPetition> AddStudentPetitionAsync(StudentPetition studentPetitionToAdd)
        {
            try
            {
                if (studentPetitionToAdd == null)
                {
                    throw new ArgumentNullException("studentPetitionToAdd", "studentPetitionToAdd cannot be null");
                }

                try
                {
                    string urlPath = UrlUtility.CombineUrlPath(_studentPetitionsPath);
                    var headers = new NameValueCollection();
                    headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                    var response = await ExecutePostRequestWithResponseAsync<StudentPetition>(objectToSend: studentPetitionToAdd, urlPath: urlPath, headers: headers);
                    var resource = JsonConvert.DeserializeObject<StudentPetition>(await response.Content.ReadAsStringAsync());
                    return resource;
                }
                // If the HTTP request fails, the student petition probably wasn't created successfully...
                catch (HttpRequestFailedException hre)
                {
                    logger.Error(hre.ToString());
                    throw new InvalidOperationException(string.Format("Student Petition add failed."), hre);
                }
                // HTTP request successful, but some other problem encountered...
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create Student Petition");
                throw;
            }
        }
        /// <summary>
        /// Returns a StudentPetition as requested by type and Id
        /// </summary>
        /// <returns>The requested <see cref="StudentPetition">StudentPetition</see> object</returns>
        public StudentPetition GetStudentPetition(string studentPetitionId, string sectionId, StudentPetitionType studentPetitionType)
        {
            try
            {
                if (string.IsNullOrEmpty(studentPetitionId))
                {
                    throw new ArgumentNullException("studentPetitionId", "StudentPetition Id must be specified.");
                }
                if (string.IsNullOrEmpty(sectionId))
                {
                    throw new ArgumentNullException("sectionId", "Section Id must be specified.");
                }
                string urlPath = UrlUtility.CombineUrlPath(_studentPetitionsPath, studentPetitionId);
                var queryString = UrlUtility.BuildEncodedQueryString("sectionId", sectionId, "type", studentPetitionType.ToString());
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<StudentPetition>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the requested Student Petition");
                throw;
            }
        }

        /// <summary>
        /// Returns a StudentPetition as requested by type and Id asynchronously
        /// </summary>
        /// <returns>The requested <see cref="StudentPetition">StudentPetition</see> object</returns>
        public async Task<StudentPetition> GetStudentPetitionAsync(string studentPetitionId, string sectionId, StudentPetitionType studentPetitionType)
        {
            try
            {
                if (string.IsNullOrEmpty(studentPetitionId))
                {
                    throw new ArgumentNullException("studentPetitionId", "StudentPetition Id must be specified.");
                }
                if (string.IsNullOrEmpty(sectionId))
                {
                    throw new ArgumentNullException("sectionId", "Section Id must be specified.");
                }
                string urlPath = UrlUtility.CombineUrlPath(_studentPetitionsPath, studentPetitionId);
                var queryString = UrlUtility.BuildEncodedQueryString("sectionId", sectionId, "type", studentPetitionType.ToString());
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<StudentPetition>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the requested Student Petition");
                throw;
            }
        }
        /// <summary>
        /// Returns a Petitions and consents as requested by student id asynchronously
        /// </summary>
        /// <returns>The requested List of <see cref="StudentPetition">StudentPetition</see> object</returns>
        public async Task<IEnumerable<StudentPetition>> GetStudentPetitionsAsync(string studentId)
        {
            try
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    throw new ArgumentNullException("studentId", "student Id must be specified.");
                }

                //string urlPath = _studentPetitionsPath;
                string urlPath = UrlUtility.CombineUrlPath(_studentPetitionsPath, studentId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<List<StudentPetition>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the requested Student Petition");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns a graduation configuration with all needed information to render a new graduation application
        /// </summary>
        /// <returns>The requested <see cref="GraduationConfiguration">GraduationConfiguration</see> object</returns>
        public async Task<GraduationConfiguration> GetGraduationConfigurationAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_graduationConfigurationPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<GraduationConfiguration>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the graduation configuration information.");
                throw;
            }
        }

        /// <summary>
        /// Returns a graduation application for a student for a particular program Code asynchronously.
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <param name="programCode">program Code that student belongs to</param>
        /// <returns>The requested <see cref="GraduationApplication">Graduation Application</see> object</returns>
        public async Task<GraduationApplication> RetrieveGraduationApplicationAsync(string studentId, string programCode)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _programsPath, UrlParameterUtility.EncodeWithSubstitution(programCode), _graduationApplicationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var graduationApplication = JsonConvert.DeserializeObject<GraduationApplication>(await responseString.Content.ReadAsStringAsync());
                return graduationApplication;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get the graduation application for student Id {0} and program Code{1} ", studentId, programCode));
                throw;
            }
        }

        /// <summary>
        /// Returns a graduation application for a student for a particular program Code.
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <param name="programCode">program Code that student belongs to</param>
        /// <returns>The requested <see cref="GraduationApplication">Graduation Application</see> object</returns>
        public GraduationApplication RetrieveGraduationApplication(string studentId, string programCode)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _programsPath, UrlParameterUtility.EncodeWithSubstitution(programCode), _graduationApplicationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var graduationApplication = JsonConvert.DeserializeObject<GraduationApplication>(responseString.Content.ReadAsStringAsync().Result);

                return graduationApplication;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get the graduation application for student Id {0} and program Code{1} ", studentId, programCode));
                throw;
            }
        }

        /// <summary>
        /// Returns a graduation application for a student asynchronously.
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <returns>The requested List of <see cref="GraduationApplication">Graduation Application</see> objects</returns>
        public async Task<List<GraduationApplication>> RetrieveGraduationApplicationsAsync(string studentId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _graduationApplicationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var graduationApplications = JsonConvert.DeserializeObject<List<GraduationApplication>>(await responseString.Content.ReadAsStringAsync());
                return graduationApplications;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get the graduation applications for student Id {0} ", studentId));
                throw;
            }
        }

        /// <summary>
        /// Returns a graduation applications for a student.
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <returns>The requested List of <see cref="GraduationApplication">Graduation Application</see> objects</returns>
        public List<GraduationApplication> RetrieveGraduationApplication(string studentId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _graduationApplicationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var graduationApplications = JsonConvert.DeserializeObject<List<GraduationApplication>>(responseString.Content.ReadAsStringAsync().Result);
                return graduationApplications;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get the graduation application for student Id {0}", studentId));
                throw;
            }
        }

        /// <summary>
        /// Client method to Post (create) a new graduation application asynchronously. 
        /// </summary>
        /// <param name="graduationApplication">Graduation Application object</param>
        /// <returns>Created Graduation Application object</returns>
        public async Task<GraduationApplication> CreateGraduationApplicationAsync(GraduationApplication graduationApplication)
        {
            // Throw exception if incoming graduation application is null
            if (graduationApplication == null)
            {
                throw new ArgumentNullException("graduationApplication", "Graduation Application object must be provided.");
            }

            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrEmpty(graduationApplication.StudentId) || string.IsNullOrEmpty(graduationApplication.ProgramCode) || string.IsNullOrEmpty(graduationApplication.GraduationTerm))
            {
                throw new ArgumentException("Graduation Application is missing a required property.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, graduationApplication.StudentId, _programsPath, UrlParameterUtility.EncodeWithSubstitution(graduationApplication.ProgramCode), _graduationApplicationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync<GraduationApplication>(graduationApplication, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<GraduationApplication>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre, string.Format("Request failed for graduation application for student Id{0} in program Code {1}. Request Error Code occured is {2}", graduationApplication.StudentId, graduationApplication.ProgramCode, hre.StatusCode.ToString()));
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the graduation application " + graduationApplication);
                throw;
            }
        }

        /// <summary>
        /// Client method to Post (create) a new graduation application. 
        /// </summary>
        /// <param name="graduationApplication">Graduation Application object</param>
        /// <returns>Created Graduation Application object</returns>
        private GraduationApplication CreateGraduationApplication(GraduationApplication graduationApplication)
        {
            // Throw exception if incoming graduation application is null
            if (graduationApplication == null)
            {
                throw new ArgumentNullException("graduationApplication", "Graduation Application object must be provided.");
            }

            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrEmpty(graduationApplication.StudentId) || string.IsNullOrEmpty(graduationApplication.ProgramCode) || string.IsNullOrEmpty(graduationApplication.GraduationTerm))
            {
                throw new ArgumentException("Graduation Application is missing a required property.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, graduationApplication.StudentId, _programsPath, UrlParameterUtility.EncodeWithSubstitution(graduationApplication.ProgramCode), _graduationApplicationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse<GraduationApplication>(graduationApplication, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<GraduationApplication>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // If the HTTP request fails, the graduation application wasn't created successfully...
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre, string.Format("Request failed for graduation application for student Id{0} in program Code {1}. Request Error Code occured is {2}", graduationApplication.StudentId, graduationApplication.ProgramCode, hre.StatusCode.ToString()));

                throw;
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the graduation application " + graduationApplication);
                throw;
            }
        }
        /// <summary>
        /// Asynchronously returns a list of cap sizes
        /// </summary>
        /// <returns>The requested list of <see cref="CapSize">Cap Sizes</see></returns>
        public async Task<IEnumerable<CapSize>> GetCapSizesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_capSizesPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<IEnumerable<CapSize>>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the cap size list.");
                throw;
            }
        }
        /// <summary>
        /// Asynchronously returns a list of gown sizes
        /// </summary>
        /// <returns>The requested list of <see cref="GownSize">Gown Sizes</see></returns>
        public async Task<IEnumerable<GownSize>> GetGownSizesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_gownSizesPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<IEnumerable<GownSize>>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the gown size list.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns a list of section roster students
        /// </summary>
        /// <param name="sectionId">ID of the course section for which roster students will be retrieved</param>
        /// <returns>The requested list of <see cref="RosterStudent">RosterStudent</see> objects</returns>
        public async Task<IEnumerable<RosterStudent>> GetSectionRosterStudentsAsync(string sectionId)
        {
            try
            {
                if (string.IsNullOrEmpty(sectionId))
                {
                    throw new ArgumentNullException("sectionId", "Section ID cannot be empty/null to retrieve roster students.");
                }
                string[] pathStrings = new string[] { _sectionsPath, UrlParameterUtility.EncodeWithSubstitution(sectionId), "roster" };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<RosterStudent>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Roster Students");
                throw;
            }
        }

        /// <summary>
        /// Get a <see cref="SectionRoster"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="SectionRoster"/></returns>
        public async Task<SectionRoster> GetSectionRosterStudents2Async(string sectionId)
        {
            try
            {
                if (string.IsNullOrEmpty(sectionId))
                {
                    throw new ArgumentNullException("sectionId", "Cannot build a section roster without a course section ID.");
                }
                string[] pathStrings = new string[] { _sectionsPath, UrlParameterUtility.EncodeWithSubstitution(sectionId), "roster" };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<SectionRoster>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve section roster.");
                throw;
            }
        }


        /// <summary>
        /// Async client method to update an existing graduation application. 
        /// </summary>
        /// <param name="graduationApplication">Graduation Application object</param>
        /// <returns>Updated Graduation Application object</returns>
        public async Task<GraduationApplication> UpdateGraduationApplicationAsync(GraduationApplication graduationApplication)
        {
            // Throw exception if incoming graduation application is null
            if (graduationApplication == null)
            {
                throw new ArgumentNullException("graduationApplication", "Graduation Application object must be provided.");
            }

            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrEmpty(graduationApplication.StudentId) || string.IsNullOrEmpty(graduationApplication.ProgramCode) || string.IsNullOrEmpty(graduationApplication.GraduationTerm))
            {
                throw new ArgumentException("Graduation Application is missing a required property.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, graduationApplication.StudentId, _programsPath, UrlParameterUtility.EncodeWithSubstitution(graduationApplication.ProgramCode), _graduationApplicationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync<GraduationApplication>(graduationApplication, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<GraduationApplication>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // If the HTTP request fails, the graduation application wasn't updated successfully...
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre, string.Format("Request failed for updating graduation application for student Id{0} in program Code {1}. Request Error Code occured is {2}", graduationApplication.StudentId, graduationApplication.ProgramCode, hre.StatusCode.ToString()));

                throw;
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the updated graduation application " + graduationApplication);
                throw;
            }
        }

        /// <summary>
        /// Returns graduation application fee information for a student for a particular program Code asynchronously.
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <param name="programCode">program Code that student is applying for</param>
        /// <returns>The requested <see cref="GraduationApplicationFee">Graduation Application Fee</see> object</returns>
        public async Task<GraduationApplicationFee> GetGraduationApplicationFeeAsync(string studentId, string programCode)
        {
            // Throw exception if incoming student or program code is not supplied
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("You must supply both student Id and program code to calculate graduation application fee.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_graduationApplicationFeesPath, studentId, UrlParameterUtility.EncodeWithSubstitution(programCode));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var graduationApplicationFee = JsonConvert.DeserializeObject<GraduationApplicationFee>(await responseString.Content.ReadAsStringAsync());
                return graduationApplicationFee;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get graduation application fee information for student Id {0} and program Code{1} ", studentId, programCode));
                throw;
            }
        }

        /// <summary>
        /// Client method to Post (create) a new student request. 
        /// </summary>
        /// <param name="studentRequest">Student Request object</param>
        /// <returns>Created a new Student Request object</returns>
        public async Task<StudentTranscriptRequest> AddStudentTranscriptRequestAsync(StudentTranscriptRequest studentRequest)
        {
            try
            {
                if (studentRequest == null)
                {
                    throw new ArgumentNullException("studentRequest", "studentRequest cannot be null");
                }

                try
                {
                    string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentRequest.StudentId, "student-transcript-request");
                    var headers = new NameValueCollection();
                    headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                    var response = await ExecutePostRequestWithResponseAsync<StudentTranscriptRequest>(studentRequest, urlPath, headers: headers);
                    var resource = JsonConvert.DeserializeObject<StudentTranscriptRequest>(await response.Content.ReadAsStringAsync());
                    return resource;
                }
                // If the HTTP request fails, the waiver probably wasn't created successfully...
                catch (HttpRequestFailedException hre)
                {
                    logger.Error(hre.ToString());
                    throw new InvalidOperationException(string.Format("StudentRequest creation failed."), hre);
                }
                // HTTP request successful, but some other problem encountered...
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create Student Transcript request");
                throw;
            }
        }

        ///// <summary>
        ///// Client method to Post (create) a new student request. 
        ///// </summary>
        ///// <param name="studentRequest">Student Request object</param>
        ///// <returns>Created a new Student Request object</returns>
        public async Task<StudentEnrollmentRequest> AddStudentEnrollmentRequestAsync(StudentEnrollmentRequest studentRequest)
        {
            try
            {
                if (studentRequest == null)
                {
                    throw new ArgumentNullException("studentRequest", "studentRequest cannot be null");
                }

                try
                {
                    string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentRequest.StudentId, "student-enrollment-request");
                    var headers = new NameValueCollection();
                    headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                    var response = await ExecutePostRequestWithResponseAsync<StudentEnrollmentRequest>(studentRequest, urlPath, headers: headers);
                    var resource = JsonConvert.DeserializeObject<StudentEnrollmentRequest>(await response.Content.ReadAsStringAsync());
                    return resource;
                }
                // If the HTTP request fails, the waiver probably wasn't created successfully...
                catch (HttpRequestFailedException hre)
                {
                    logger.Error(hre.ToString());
                    throw new InvalidOperationException(string.Format("StudentRequest creation failed."), hre);
                }
                // HTTP request successful, but some other problem encountered...
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create Student Transcript request");
                throw;
            }
        }


        /// <summary>
        /// Get a student Transcript Request by RequestId
        /// </summary>
        /// <param name="requestId">Request Id</param>
        /// <returns>Returns a student Transcript Request</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<StudentTranscriptRequest> GetStudentTranscriptRequestAsync(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Student transcript retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, "student-transcript-request", requestId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentTranscriptRequest>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Student Transcript Request");
                throw;
            }
        }

        /// <summary>
        /// Get a student Enrollment Request by RequestId
        /// </summary>
        /// <param name="requestId">Request Id</param>
        /// <returns>Returns a student Enrollment Request</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<StudentEnrollmentRequest> GetStudentEnrollmentRequestAsync(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Student Enrollment retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, "student-enrollment-request", requestId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentEnrollmentRequest>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Student Enrollment Request");
                throw;
            }
        }


        /// <summary>
        /// Asynchronously returns a student request configuration with all needed information to render a new transcript request or enrollment request
        /// </summary>
        /// <returns>The requested <see cref="StudentRequestConfiguration">StudentRequestConfiguration</see> object</returns>
        public async Task<StudentRequestConfiguration> GetStudentRequestConfigurationAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                string urlPath = UrlUtility.CombineUrlPath(_configurationPath, _studentRequestPath);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<StudentRequestConfiguration>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the student request configuration information.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns a list of request hold types
        /// </summary>
        /// <returns>The requested list of <see cref="GownSize">HoldRequestType</see></returns>
        public async Task<IEnumerable<HoldRequestType>> GetHoldRequestTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_holdRequestTypesPath, headers: headers);
                var requestTypes = JsonConvert.DeserializeObject<IEnumerable<HoldRequestType>>(await responseString.Content.ReadAsStringAsync());

                return requestTypes;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the list of hold request types.");
                throw;
            }
        }

        /// <summary>
        /// Get all student enrollment requests for a single student
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>Returns a list of student enrollment requests</returns>
        /// <exception cref="ArgumentNullException">The student id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The student requests cannot be retrieved.</exception>
        public async Task<List<StudentEnrollmentRequest>> GetStudentEnrollmentRequestsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null for Student Enrollment requests retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentEnrollmentRequests);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<List<StudentEnrollmentRequest>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Student Enrollment Requests");
                throw;
            }
        }

        /// <summary>
        /// Get all student transcript requests for a single student
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>Returns a list of student transcript requests</returns>
        /// <exception cref="ArgumentNullException">The student id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The student requests cannot be retrieved.</exception>
        public async Task<List<StudentTranscriptRequest>> GetStudentTranscriptRequestsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be empty/null for student transcript requests retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentTranscriptRequests);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<List<StudentTranscriptRequest>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Student Transcript Requests");
                throw;
            }
        }


        /// <summary>
        /// Returns student request fee(transcript requests and enrollment verificaion requests) information for a student for a particular request Id asynchronously.
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <param name="requestId">request Id that student is applying for</param>
        /// <returns>The requested <see cref="StudentRequestFee">Student Request Fee</see> object</returns>
        public async Task<StudentRequestFee> GetStudentRequestFeeAsync(string studentId, string requestId)
        {
            // Throw exception if incoming student or program code is not supplied
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentNullException("You must supply both student Id and program code to calculate student request fee.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentRequestPath, requestId, _studentRequestFeesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var studentRequestFee = JsonConvert.DeserializeObject<StudentRequestFee>(await responseString.Content.ReadAsStringAsync());
                return studentRequestFee;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get student request fee information for student Id {0} and request Id{1} ", studentId, requestId));
                throw;
            }
        }

        /// <summary>
        /// Returns academic credit information asynchronously. Primarily useful for credits by section.
        /// </summary>
        /// <param name="criteria">Criteria that identifies which credits are desired. At least one section is required.</param>
        /// <returns>The list of requested <see cref="AcademicCredit2">Academic Credit</see> objects</returns>
        [Obsolete("Obsolete as of API 1.18. Use QueryAcademicCredits2Async.")]
        public async Task<IEnumerable<AcademicCredit2>> QueryAcademicCreditsAsync(AcademicCreditQueryCriteria criteria, int? offset = null, int? limit = null)
        {
            // Throw exception if section Id is not supplied
            if (criteria == null || criteria.SectionIds == null || !criteria.SectionIds.Any())
            {
                throw new ArgumentNullException("You must supply a criteria with at least one section Id to retrieve aademic credit information.");
            }
            try
            {

                var queryString = UrlUtility.BuildEncodedQueryString("offset", offset.HasValue ? offset.Value.ToString() : null, "limit", limit.HasValue ? limit.Value.ToString() : null);


                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _academicCreditsPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, combinedUrl, headers: headers);
                var academicCredits = JsonConvert.DeserializeObject<IEnumerable<AcademicCredit2>>(await response.Content.ReadAsStringAsync());
                return academicCredits;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to query academic credit information."));
                throw;
            }
        }

        /// <summary>
        /// Returns academic credit information asynchronously. Primarily useful for credits by section.
        /// </summary>
        /// <param name="criteria">Criteria that identifies which credits are desired. At least one section is required.</param>
        /// <returns>The list of requested <see cref="AcademicCredit2">Academic Credit</see> objects</returns>
        public async Task<IEnumerable<AcademicCredit3>> QueryAcademicCredits2Async(AcademicCreditQueryCriteria criteria, int? offset = null, int? limit = null)
        {
            // Throw exception if section Id is not supplied
            if (criteria == null || criteria.SectionIds == null || !criteria.SectionIds.Any())
            {
                throw new ArgumentNullException("You must supply a criteria with at least one section Id to retrieve aademic credit information.");
            }
            try
            {

                var queryString = UrlUtility.BuildEncodedQueryString("offset", offset.HasValue ? offset.Value.ToString() : null, "limit", limit.HasValue ? limit.Value.ToString() : null);


                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _academicCreditsPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecutePostRequestWithResponseAsync(criteria, combinedUrl, headers: headers);
                var academicCredits = JsonConvert.DeserializeObject<IEnumerable<AcademicCredit3>>(await response.Content.ReadAsStringAsync());
                return academicCredits;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to query academic credit information."));
                throw;
            }
        }


        /// <summary>
        /// Asynchronously returns faculty grading configuration with all needed information to render faculty grade view
        /// </summary>
        /// <returns>The requested <see cref="FacultyGradingConfiguration">FacultyGradingConfiguration</see> object</returns>
        public async Task<FacultyGradingConfiguration> GetFacultyGradingConfigurationAsync()
        {
            try
            {
                string[] pathStrings = new string[] { _configurationPath, _facultyGradingPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<FacultyGradingConfiguration>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get faculty grading configuration information.");
                throw;
            }
        }

        /// <summary>
        /// Get a 1098-T tax form PDF
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1098-T.</param>
        /// <param name="recordId">The record ID where the 1098-T pdf data is stored</param>
        /// <returns>Byte array containing PDF data</returns>
        public async Task<byte[]> Get1098tTaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId cannot be null or empty.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("id", "Record ID cannot be null or empty.");

            try
            {
                // Build url path and create and execute a request to get the tax form pdf
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, personId, _taxForm1098tPdfPath, recordId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v1+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v1+pdf");
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve 1098-T tax form pdf.");
                throw;
            }
        }

        /// <summary>
        /// Get a T2202A tax form PDF
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T2202A.</param>
        /// <param name="recordId">The record ID where the T2202A pdf data is stored</param>
        /// <returns>Byte array containing PDF data</returns>
        public async Task<byte[]> GetT2202aTaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId cannot be null or empty.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("id", "Record ID cannot be null or empty.");

            try
            {
                // Build url path and create and execute a request to get the tax form pdf
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, personId, _taxFormT2202aPdfPath, recordId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v1+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v1+pdf");
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve T2202A tax form pdf.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns the course catalog configuration with all needed information for catalog searches
        /// </summary>
        /// <returns>The requested <see cref="CourseCatalogConfiguration">CourseCatalogConfiguration</see> object</returns>
        public async Task<CourseCatalogConfiguration> GetCourseCatalogConfigurationAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                string urlPath = UrlUtility.CombineUrlPath(_configurationPath, _courseCatalogPath);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<CourseCatalogConfiguration>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the course catalog configuration information.");
                throw;
            }
        }

        /// <summary>
        /// Update a book assignment for a section.
        /// </summary>
        /// <param name="textbook">The textbook whose assignment to a specific section is being updated.</param>
        /// <returns>An updated <see cref="Section3"/> object.</returns>
        public async Task<Section3> UpdateSectionBookAsync(SectionTextbook textbook, bool useCache = true)
        {
            if (textbook == null)
            {
                throw new ArgumentNullException("textbook", "Textbook may not be null");
            }
            if (string.IsNullOrEmpty(textbook.SectionId))
            {
                throw new ArgumentNullException("textbook.SectionId", "Section Id may not be null or empty");
            }

            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_sectionTextbooksPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync(textbook, urlPath, headers: headers);
                var requestTypes = JsonConvert.DeserializeObject<Section3>(await response.Content.ReadAsStringAsync());

                return requestTypes;
            }

            // If the HTTP request fails, the SectionTextbook was probably not added to the Section
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("SectionTextbook update to Section {0} failed.", textbook.SectionId), hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Gets a list of all book options.
        /// </summary>
        /// <returns>The full list of book options</returns>
        public async Task<IEnumerable<BookOption>> GetBookOptionsAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_bookOptionsPath, headers: headers);
                var requestTypes = JsonConvert.DeserializeObject<IEnumerable<BookOption>>(await responseString.Content.ReadAsStringAsync());

                return requestTypes;
            }
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException("Failed to get list of all book options.", hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the list of book options.");
                throw;
            }
        }

        /// <summary>
        /// Gets a list of all attendance categories.
        /// </summary>
        /// <returns>The full list of book options</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AttendanceCategories>> GetAttendanceCategoriesAsync()
        {
            try
            {
                var responseString = await ExecuteGetRequestWithResponseAsync(_attendanceCategoriesPath);
                var requestTypes = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.AttendanceCategories>>(await responseString.Content.ReadAsStringAsync());

                return requestTypes;
            }
            catch (HttpRequestFailedException hre)
            {
                logger.Error(hre.ToString());
                throw new InvalidOperationException("Failed to get list of attendance categories.", hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the list of attendance categories.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all nonacademic attendance event types.
        /// </summary>
        /// <returns>All <see cref="NonAcademicAttendanceEventType">nonacademic attendance event type</see> codes and descriptions.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<NonAcademicAttendanceEventType>> GetNonAcademicAttendanceEventTypesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_nonAcademicAttendanceEventTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<NonAcademicAttendanceEventType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get nonacademic attendance event types");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person
        /// </summary>
        /// <param name="personId">Unique identifier for the person whose requirements are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<NonAcademicAttendanceRequirement>> GetNonAcademicAttendanceRequirementsAsync(string personId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, personId, _nonAcademicAttendanceRequirementsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<NonAcademicAttendanceRequirement>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, String.Format("Unable to get nonacademic attendance requirements for person {0}", personId));
                throw;
            }
        }

        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendance">nonacademic events attended</see> for a person
        /// </summary>
        /// <param name="personId">Unique identifier for the person whose nonacademic attendances are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendance">nonacademic events attended</see> for a person</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<NonAcademicAttendance>> GetNonAcademicAttendancesAsync(string personId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, personId, _nonAcademicAttendancesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<NonAcademicAttendance>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, String.Format("Unable to get nonacademic events attended for person {0}", personId));
                throw;
            }
        }
        /// <summary>
        /// Retrieves the <see cref="NonAcademicEvent">nonacademic events</see> for a specific list of event Ids
        /// </summary>
        /// <param name="eventIds">List of Ids to retrieve. Required.</param>
        /// <returns>The requested <see cref="NonAcademicEvent">nonacademic events</see></returns>
        public async Task<IEnumerable<NonAcademicEvent>> QueryNonacademicEventsByIdsAsync(List<string> eventIds)
        {

            if (eventIds == null || !eventIds.Any())
            {
                throw new ArgumentNullException("eventIds", "Must include at least one event Id for retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _nonacademicEventsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var criteria = new NonAcademicEventQueryCriteria() { EventIds = eventIds };
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<NonAcademicEvent>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, String.Format("Unable to get nonacademic events with ids: " + string.Join(", ", eventIds)));
                throw;
            }
        }

        /*************************       FA Related methods used by FA and SF       ************************/
        /// <summary>
        /// Retrieve all Awards from Colleague
        /// </summary>
        /// <returns>A list of Award2 data objects</returns>
        public IEnumerable<Award2> GetAwards2()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(_awardsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Award2>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find awards resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get awards");
                throw;
            }
        }

        /// <summary>
        /// This method gets the specified FA award year on student's file
        /// </summary>
        /// <param name="studentId">Student Id for whom to retrieve award years</param>
        /// <param name="awardYear">Award year code for which to retrieve award year data</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>specified financial aid year from student's file</returns>
        public async Task<StudentAwardYear2> GetStudentAwardYear2Async(string studentId, string awardYear, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentAwardYearsPath, awardYear);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentAwardYear2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get {0} student award year", awardYear));
                throw;
            }
        }

        /// <summary>
        /// Searches for advisees within an advisor's assigned advisee list or within the global pool (depending on advisor's permissions) async.
        /// </summary>
        /// <param name="studentKeyword">The search string used when searching students by name or ID</param>        
        /// <returns>A list of matching students, which may be empty</returns>
        /// <exception cref="System.ArgumentException">Thrown when studentKeyword is not supplied.</exception>
        /// <exception cref="Ellucian.Colleague.Api.Client.Exceptions.AdvisingException">Thrown when the search fails</exception>
        public async Task<IEnumerable<Student>> QueryStudentsSearchAsync(string studentKeyword, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            if (string.IsNullOrEmpty(studentKeyword))
            {
                throw new ArgumentException("Must provide studentKeyword.");
            }

            string query = UrlUtility.BuildEncodedQueryString(new[] { "pageSize", pageSize.ToString(), "pageIndex", pageIndex.ToString() });

            // Build url path from qapi path and student programs path
            string[] pathStrings = new string[] { _qapiPath, _studentsPath };
            var urlPath = UrlUtility.CombineUrlPath(pathStrings);
            urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            IEnumerable<Student> students = null;
            var criteria = new StudentSearchCriteria();
            criteria.StudentKeyword = studentKeyword;

            try
            {
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new ApplicationException("The request is unauthorized.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    students = JsonConvert.DeserializeObject<IEnumerable<Student>>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new ApplicationException("The request is unauthorized.");
                }

                return students;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw new ApplicationException("The search has failed : " + ex.Message);
            }
        }

        /// <summary>
        /// Update an add authorization asynchronousy.
        /// </summary>
        /// <param name="addAuthorization">An add authorization DTO to be updated.</param>
        /// <returns>The updated AddAuthorization DTO.</returns>
        public async Task<AddAuthorization> UpdateAddAuthorizationAsync(AddAuthorization addAuthorization)
        {
            if (addAuthorization == null)
            {
                throw new ArgumentNullException("addAuthorization", "addAuthorization cannot be null.");
            }
            try
            {
                // Build url path
                var urlPath = _addAuthorizationsPath;
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync<AddAuthorization>(addAuthorization, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<AddAuthorization>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update add authorization information.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns the registration configuration with parameters that control how to registration is configured.
        /// </summary>
        /// <returns>The requested <see cref="RegistrationConfiguration">RegistrationConfiguration</see> object</returns>
        public async Task<RegistrationConfiguration> GetRegistrationConfigurationAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                string urlPath = UrlUtility.CombineUrlPath(_configurationPath, _registrationPath);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<RegistrationConfiguration>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the registration configuration information.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns add authorizations for a specific section
        /// </summary>
        /// <param name="sectionId">The section Id of section for which authorizations are requested</param>
        /// <returns>The requested <see cref="AddAuthorization">Add Authorization</see> objects</returns>
        public async Task<IEnumerable<AddAuthorization>> GetSectionAddAuthorizationsAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                logger.Error("Unable to get the section add authorizations information.");
                throw new ArgumentNullException("sectionId", "Must provide a sectionId to retrieve section add authorizations.");
            }

            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                string urlPath = UrlUtility.CombineUrlPath(_sectionsPath, UrlParameterUtility.EncodeWithSubstitution(sectionId), _addAuthorizationsPath);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var addAuthorizations = JsonConvert.DeserializeObject<IEnumerable<AddAuthorization>>(await responseString.Content.ReadAsStringAsync());

                return addAuthorizations;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get add authorizations for section " + sectionId);
                throw;
            }
        }


        /// <summary>
        /// Asynchronously returns a list of drop reasons
        /// </summary>
        /// <returns>The requested list of <see cref="DropReason">Drop Reasons</see></returns>
        public async Task<IEnumerable<DropReason>> GetDropReasonsAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_dropReasonsPath, headers: headers);
                var dropReasons = JsonConvert.DeserializeObject<IEnumerable<DropReason>>(await responseString.Content.ReadAsStringAsync());

                return dropReasons;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the drop reasons list.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns an add authorization by its id.
        /// </summary>
        /// <param name="id">The Id of the add authorization requested</param>
        /// <returns>The requested <see cref="AddAuthorization">Add Authorization</see> object</returns>
        public async Task<AddAuthorization> GetAddAuthorizationAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                logger.Error("Id is required to get an the add authorizations.");
                throw new ArgumentNullException("id", "Must provide an id to retrieve an add authorization.");
            }

            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                string urlPath = UrlUtility.CombineUrlPath(_addAuthorizationsPath, UrlParameterUtility.EncodeWithSubstitution(id));
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var addAuthorization = JsonConvert.DeserializeObject<AddAuthorization>(await responseString.Content.ReadAsStringAsync());

                return addAuthorization;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get add authorizations with id " + id);
                throw;
            }
        }

        /// <summary>
        /// Create a new add authorization for a student in a section. 
        /// </summary>
        /// <param name="addAuthorizationInput">Input for a new add authorization. Must have  section Id and student Id.</param>
        /// <returns>Newly created add authorization</returns>
        public async Task<AddAuthorization> CreateAddAuthorizationAsync(AddAuthorizationInput addAuthorizationInput)
        {

            if (addAuthorizationInput == null)
            {
                throw new ArgumentNullException("addAuthorizationInput", "AddAuthorizationInput cannot be null ");
            }
            if (string.IsNullOrEmpty(addAuthorizationInput.StudentId) || string.IsNullOrEmpty(addAuthorizationInput.SectionId))
            {
                throw new ArgumentException("AddAuthorizationInput must have a student Id and section Id. ");
            }
            try
            {
                //string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentWaiver.StudentId, "student-waiver");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync<AddAuthorizationInput>(addAuthorizationInput, _addAuthorizationsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AddAuthorization>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // If the HTTP request fails, the add authorization wasn't created
            catch (HttpRequestFailedException hre)
            {
                if (hre.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    logger.Error(hre.ToString() + " Student " + addAuthorizationInput.StudentId + " already has authorization for section " + addAuthorizationInput.SectionId);
                    throw new ExistingResourceException();
                }
                // Otherwise
                logger.Error(hre.ToString());
                throw new InvalidOperationException(string.Format("Add Authorization creation failed."), hre);
            }
            // HTTP request successful, but some other problem encountered.
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }

        }
        /// <summary>
        /// Retrieve sections calendar schedules in iCal format
        /// </summary>
        /// <param name="criteria" <see cref="SectionEventsICalQueryCriteria"/> >Post in Body a list of section ids or date ranges</param>
        /// <returns>iCal in string format</returns>
        public async Task<string> QuerySectionEventsICalAsync(SectionEventsICalQueryCriteria criteria)
        {
            // Throw exception if section Id is not supplied
            if (criteria == null || criteria.SectionIds == null || !criteria.SectionIds.Any())
            {
                throw new ArgumentNullException("You must supply a criteria with at least one section Id to retrieve section Ical from calendar schedules.");
            }
            try
            {

                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _sectionsPath, _iCalPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<EventsICal>(await response.Content.ReadAsStringAsync());
                return resource.iCal;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to query sections iCal information."));
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns graduation application eligibilty information for a student 
        /// </summary>
        /// <param name="studentId">(Required) Id of student</param>
        /// <param name="programCodes">List of program Codes for which graduation application eligibility is desired. Must provide at least 1</param>
        /// <returns>List of <see cref="GraduationApplicationProgramEligibility">Graduation Application Program Eligibility</see> objects</returns>
        public async Task<IEnumerable<GraduationApplicationProgramEligibility>> QueryGraduationApplicationEligibilityAsync(string studentId, List<string> programCodes)
        {
            // Throw exception if incoming student or program code is not supplied
            if (string.IsNullOrEmpty(studentId) || programCodes == null || !programCodes.Any())
            {
                throw new ArgumentNullException("You must supply a student Id and at least one program code to get graduation application eligiblity.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _graduationApplicationEligibilityPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var criteria = new GraduationApplicationEligibilityCriteria() { StudentId = studentId, ProgramCodes = programCodes };
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<GraduationApplicationProgramEligibility>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve graduation application eligibility.");
                throw new ApplicationException("Unable to retrieve graduation application eligibility.");
            }
        }

    }
}

