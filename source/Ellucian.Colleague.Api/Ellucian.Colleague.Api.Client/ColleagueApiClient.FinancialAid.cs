/*Copyright 2014-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Utility;
using Newtonsoft.Json;
using Ellucian.Rest.Client.Exceptions;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Api.Client
{
    public partial class ColleagueApiClient
    {
        /// <summary>
        /// Gets all academic progress appeal codes
        /// </summary>
        /// <returns>List of academic progress appeal codes</returns>
        public IEnumerable<AcademicProgressAppealCode> GetAcademicProgressAppealCodes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_academicProgressAppealCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicProgressAppealCode>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get academic progress appeal codes");
                throw;
            }
        }

        /// <summary>
        /// Gets all academic progress appeal codes
        /// </summary>
        /// <returns>List of academic progress appeal codes</returns>
        public async Task<IEnumerable<AcademicProgressAppealCode>> GetAcademicProgressAppealCodesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_academicProgressAppealCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicProgressAppealCode>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get academic progress appeal codes");
                throw;
            }
        }

        /// <summary>
        /// Get a list of all a student's AcademicProgressEvaluation DTOs.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <returns>A list of AcademicProgressEvaluation objects for the student</returns>
        [Obsolete("Obsolete as of API version 1.14. Use GetStudentAcademicProgressEvaluations2Async.")]
        public IEnumerable<AcademicProgressEvaluation> GetStudentAcademicProgressEvaluations(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _academicProgressEvaluationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicProgressEvaluation>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student's academic progress evaluations");
                throw;
            }
        }

        /// <summary>
        /// Get a list of all a student's AcademicProgressEvaluation DTOs.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <returns>A list of AcademicProgressEvaluation objects for the student</returns>
        [Obsolete("Obsolete as of API version 1.14. Use GetStudentAcademicProgressEvaluations2Async.")]
        public async Task<IEnumerable<AcademicProgressEvaluation>> GetStudentAcademicProgressEvaluationsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _academicProgressEvaluationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicProgressEvaluation>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student's academic progress evaluations");
                throw;
            }
        }

        /// <summary>
        /// Get a list of all a student's AcademicProgressEvaluation DTOs.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <returns>A list of AcademicProgressEvaluation2 objects for the student</returns>
        public async Task<IEnumerable<AcademicProgressEvaluation2>> GetStudentAcademicProgressEvaluations2Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _academicProgressEvaluationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicProgressEvaluation2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student's academic progress evaluations");
                throw;
            }
        }

        /// <summary>
        /// Get a list of Academic Progress Statuses. These objects are cached for 24 hours.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AcademicProgressStatus> GetAcademicProgressStatuses()
        {
            
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_academicProgressStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicProgressStatus>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get academic progress statuses");
                throw;
            }
        }

        /// <summary>
        /// Get a list of Academic Progress Statuses. These objects are cached for 24 hours.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<AcademicProgressStatus>> GetAcademicProgressStatusesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_academicProgressStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AcademicProgressStatus>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get academic progress statuses");
                throw;
            }
        }

        /// <summary>
        /// Get the set of average award packages that students receive at this institution
        /// for all years on a student's record 
        /// </summary>
        /// <param name="studentId">The student Id of the average award packages to get</param>
        /// <returns>An IEnumerable of AverageAwardPackage objects</returns>
        public IEnumerable<AverageAwardPackage> GetAverageAwardPackages(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _averageAwardPackgePath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AverageAwardPackage>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find average award packages resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get average award packages");
                throw;
            }
        }

        /// <summary>
        /// Get the set of average award packages that students receive at this institution
        /// for all years on a student's record 
        /// </summary>
        /// <param name="studentId">The student Id of the average award packages to get</param>
        /// <returns>An IEnumerable of AverageAwardPackage objects</returns>
        public async Task<IEnumerable<AverageAwardPackage>> GetAverageAwardPackagesAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _averageAwardPackgePath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AverageAwardPackage>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find average award packages resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get average award packages");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all Award Categories and their descriptions
        /// </summary>
        /// <returns>A list of all Award Category data objects</returns>
        [Obsolete("Obsolete as of API version 1.8. Use GetAwardCategories2.")]
        public IEnumerable<AwardCategory> GetAwardCategories()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_awardCategoriesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardCategory>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award categories resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award categories");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all Award Categories and their descriptions
        /// </summary>
        /// <returns>A list of all Award Category2 data objects</returns>
        public IEnumerable<AwardCategory2> GetAwardCategories2()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(_awardCategoriesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardCategory2>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award categories resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award categories");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all Award Categories and their descriptions
        /// </summary>
        /// <returns>A list of all Award Category2 data objects</returns>
        public async Task<IEnumerable<AwardCategory2>> GetAwardCategories2Async()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(_awardCategoriesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardCategory2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award categories resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award categories");
                throw;
            }
        }

        /// <summary>
        /// Get award letter for a student for a given year.
        /// </summary>
        /// <param name="studentId">The id of the student for whom to get an award letter</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <returns>Am award-letter DTO object</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId or awardYear argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the requested Award Letter resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to get the student's award letter</exception>
        [Obsolete("Obsolete as of API version 1.8. Use GetAwardLetter2.")]
        public AwardLetter GetAwardLetter(string studentId, string awardYear)
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
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardLetter>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letter resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award letter");
                throw;
            }
        }

        /// <summary>
        /// Get award letter for a student for a given year. Letter will be returned even if no awards are present.
        /// </summary>
        /// <param name="studentId">The id of the student for whom to get an award letter</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <returns>Am award-letter DTO object</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId or awardYear argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the requested Award Letter resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to get the student's award letter</exception>        
        [Obsolete("Obsolete as of API version 1.10. Use GetAwardLetter3.")]
        public AwardLetter GetAwardLetter2(string studentId, string awardYear)
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
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardLetter>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letter resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award letter");
                throw;
            }
        }

        /// <summary>
        /// Get award letter for a student for a given year. Letter will be returned even if no awards are present.
        /// </summary>
        /// <param name="studentId">The id of the student for whom to get an award letter</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <returns>An award-letter2 DTO object</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId or awardYear argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the requested Award Letter resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to get the student's award letter</exception>        
        [Obsolete("Obsolete as of API version 1.10. Use GetAwardLetter4Async.")]
        public async Task<AwardLetter2> GetAwardLetter3Async(string studentId, string awardYear)
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
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardLetter2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letter resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award letter");
                throw;
            }
        }

        /// <summary>
        /// Get award letter for a student for a given year. Letter will be returned even if no awards are present.
        /// </summary>
        /// <param name="studentId">The id of the student for whom to get an award letter</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <returns>An AwardLetter3 DTO object</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId or awardYear argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the requested Award Letter resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to get the student's award letter</exception>        

        public async Task<AwardLetter3> GetAwardLetter4Async(string studentId, string awardYear)
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
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion4);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardLetter3>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letter resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award letter");
                throw;
            }
        }

        /// <summary>
        /// Get an Award Letter report for the given student and year.
        /// </summary>
        /// <param name="studentId">The id of the student for whom to get an award letter</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <param name="fileName">The name of the PDF file returned in the byte array.</param>
        /// <returns>A byte array representation of the PDF Award Letter Report</returns>
        /// <exception cref="Exception">Thrown if an error occurred generating the award letter report.</exception>
        [Obsolete("Obsolete as of API version 1.8. Use GetAwardLetter2.")]
        public byte[] GetAwardLetter(string studentId, string awardYear, out string fileName)
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
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v1+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v1+pdf");
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                fileName = response.Content.Headers.ContentDisposition.FileName;
                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get Award Letter Report");
                throw;
            }
        }

        /// <summary>
        /// Get an Award Letter report for the given student and year. Award letter report is returned even if no awards 
        /// are associated with the letter
        /// </summary>
        /// <param name="studentId">The id of the student for whom to get an award letter</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <param name="fileName">The name of the PDF file returned in the byte array.</param>
        /// <returns>A byte array representation of the PDF Award Letter Report</returns>
        /// <exception cref="Exception">Thrown if an error occurred generating the award letter report.</exception>
        [Obsolete("Obsolete as of API version 1.10. Use GetAwardLetterReport3Async.")]
        public byte[] GetAwardLetter2(string studentId, string awardYear, out string fileName)
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
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v2+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v2+pdf");
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                fileName = response.Content.Headers.ContentDisposition.FileName;
                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get Award Letter Report");
                throw;
            }
        }        

        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data.
        /// </summary>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the Award Letters resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to get the student's award letters</exception>
        [Obsolete("Obsolete as of API version 1.8. Use GetAwardLetters2.")]
        public IEnumerable<AwardLetter> GetAwardLetters(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardLetter>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letters resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award letters");
                throw;
            }
        }

        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data. Each letter is returned even if no awards are associated with
        /// that award letter
        /// </summary>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the Award Letters resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to get the student's award letters</exception>
        [Obsolete("Obsolete as of API version 1.10. Use GetAwardLetters3.")]
        public IEnumerable<AwardLetter> GetAwardLetters2(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardLetter>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letters resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award letters");
                throw;
            }
        }

        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data. Each letter is returned even if no awards are associated with
        /// that award letter
        /// </summary>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the Award Letters resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to get the student's award letters</exception>
        [Obsolete("Obsolete as of API version 1.22. Use GetAwardLetters4Async.")]
        public async Task<IEnumerable<AwardLetter2>> GetAwardLetters3Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardLetter2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letters resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award letters");
                throw;
            }
        }

        /// <summary>
        /// Get award letters for a student across all the years a student has
        /// financial aid data. Each letter is returned even if no awards are associated with
        /// that award letter
        /// </summary>
        /// <param name="studentId">The id of the student for whom to get award letters</param>
        /// <returns>A list of award-letter DTO objects</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the Award Letters resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to get the student's award letters</exception>
        public async Task<IEnumerable<AwardLetter3>> GetAwardLetters4Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion4);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardLetter3>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letters resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award letters");
                throw;
            }
        }

        /// <summary>
        /// Get an award letter report for the given student award year.
        /// </summary>
        /// <param name="studentId">student id for whom to retrieve award letter report for</param>
        /// <param name="recordId">award letter history record id</param>
        /// <returns>AwardLetter Report</returns>
        [Obsolete("Obsolete as of API version 1.22. Use GetAwardLetterReport4Async.")]
        public async Task<AwardLetterReport> GetAwardLetterReport3Async(string studentId, string recordId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }            
            if (string.IsNullOrEmpty(recordId))
            {
                throw new ArgumentNullException("recordId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, recordId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v3+pdf");                
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return new AwardLetterReport()
                {
                    FileName = response.Content.Headers.ContentDisposition.FileName,
                    FileContent = await response.Content.ReadAsByteArrayAsync()
                };
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get Award Letter Report");
                throw;
            }
        }

        /// <summary>
        /// Get an award letter report for the given student award year.
        /// </summary>
        /// <param name="studentId">student id for whom to retrieve award letter report for</param>
        /// <param name="recordId">award letter history record id</param>
        /// <returns>AwardLetter Report</returns>
        public async Task<AwardLetterReport> GetAwardLetterReport4Async(string studentId, string recordId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(recordId))
            {
                throw new ArgumentNullException("recordId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, recordId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v4+pdf");
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return new AwardLetterReport()
                {
                    FileName = response.Content.Headers.ContentDisposition.FileName,
                    FileContent = await response.Content.ReadAsByteArrayAsync()
                };
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get Award Letter Report");
                throw;
            }
        }

        /// <summary>
        /// Update a student award letter for a given year.
        /// </summary>
        /// <param name="studentId">The id of the student for whom to update the award letter</param>
        /// <param name="awardYear">The award year for which to update the award letter</param>
        /// <param name="awardLetter">The award letter object containing the data with which to update the database</param>
        /// <returns>An updated award-letter DTO object</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId, awardYear or awardLetter argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the requested Award Letter resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to update the student's award letter</exception>
        [Obsolete("Obsolete as of API version 1.10. Use UpdateAwardLetter2.")]
        public AwardLetter UpdateAwardLetter(string studentId, string awardYear, AwardLetter awardLetter)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (awardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePutRequestWithResponse<AwardLetter>(awardLetter, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardLetter>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letter resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update award letter");
                throw;
            }
        }

        /// <summary>
        /// Update a student award letter for a given year.
        /// </summary>
        /// <param name="studentId">The id of the student for whom to update the award letter</param>
        /// <param name="awardYear">The award year for which to update the award letter</param>
        /// <param name="awardLetter">The award letter object containing the data with which to update the database</param>
        /// <returns>An updated award-letter DTO object</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId, awardYear or awardLetters argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the requested Award Letter resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to update the student's award letter</exception>
        [Obsolete("Obsolete as of API version 1.22. Use UpdateAwardLetter3Async.")]
        public async Task<AwardLetter2> UpdateAwardLetter2Async(string studentId, string awardYear, AwardLetter2 awardLetter)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (awardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecutePutRequestWithResponseAsync<AwardLetter2>(awardLetter, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardLetter2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letter resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update award letter");
                throw;
            }
        }

        /// <summary>
        /// Update a student award letter for a given year.
        /// </summary>
        /// <param name="studentId">The id of the student for whom to update the award letter</param>
        /// <param name="awardYear">The award year for which to update the award letter</param>
        /// <param name="awardLetter">The award letter object containing the data with which to update the database</param>
        /// <returns>An updated awardLetter3 DTO object</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId, awardYear or awardLetters argument is null or empty</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if the API is unable to find the requested Award Letter resource</exception>
        /// <exception cref="Exception">Thrown if the API is unable to update the student's award letter</exception>
        public async Task<AwardLetter3> UpdateAwardLetter3Async(string studentId, string awardYear, AwardLetter3 awardLetter)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (awardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardLettersPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecutePutRequestWithResponseAsync<AwardLetter3>(awardLetter, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardLetter3>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award letter resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update award letter");
                throw;
            }
        }

        /// <summary>
        /// Get a list of AwardLetterConfiguration DTOs. Cached for 24 hours
        /// </summary>
        /// <returns>List of AwardLetterConfiguration DTOs</returns>
        public async Task<IEnumerable<AwardLetterConfiguration>> GetAwardLetterConfigurationsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_awardLetterConfigurationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardLetterConfiguration>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award letter configurations");
                throw;
            }
        }

        public IEnumerable<AwardPackageChangeRequest> GetAwardPackageChangeRequests(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardPackageChangeRequestsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardPackageChangeRequest>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award package change requests");
                throw;
            }
        }

        public async Task<IEnumerable<AwardPackageChangeRequest>> GetAwardPackageChangeRequestsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardPackageChangeRequestsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardPackageChangeRequest>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award package change requests");
                throw;
            }
        }

        public AwardPackageChangeRequest GetAwardPackageChangeRequest(string studentId, string changeRequestId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(changeRequestId))
            {
                throw new ArgumentNullException("changeRequestId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardPackageChangeRequestsPath, changeRequestId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardPackageChangeRequest>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award package change request");
                throw;
            }
        }

        public async Task<AwardPackageChangeRequest> GetAwardPackageChangeRequestAsync(string studentId, string changeRequestId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(changeRequestId))
            {
                throw new ArgumentNullException("changeRequestId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardPackageChangeRequestsPath, changeRequestId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardPackageChangeRequest>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award package change request");
                throw;
            }
        }

        /// <summary>
        /// Creates an award package change request. Asynchronous version
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="changeRequest"></param>
        /// <returns>AwardPackageChangeRequest dto</returns>
        public async Task<AwardPackageChangeRequest> CreateAwardPackageChangeRequestAsync(string studentId, AwardPackageChangeRequest changeRequest, bool updateAllAwards = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (changeRequest == null)
            {
                throw new ArgumentNullException("changeRequest");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardPackageChangeRequestsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(changeRequest, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardPackageChangeRequest>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create award package change request");
                throw;
            }
        }

        /// <summary>
        /// Creats an award package change request
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="changeRequest"></param>
        /// <returns>AwardPackageChangeRequest dto</returns>
        public AwardPackageChangeRequest CreateAwardPackageChangeRequest(string studentId, AwardPackageChangeRequest changeRequest)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (changeRequest == null)
            {
                throw new ArgumentNullException("changeRequest");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardPackageChangeRequestsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(changeRequest, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<AwardPackageChangeRequest>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create award package change request");
                throw;
            }
        }

        /// <summary>
        /// Creates a new student FA checklist for a given year
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="year"></param>
        /// <returns>StudentFinancialAidChecklist object</returns>
        [Obsolete("Obsolete as of API 1.12. Use CreateStudentFinancialAidChecklistAsync.")]
        public StudentFinancialAidChecklist CreateStudentFinancialAidChecklist(string studentId, string year)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (string.IsNullOrEmpty(year))
            {
                throw new ArgumentNullException("year");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _financialAidChecklistPath, year);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(string.Empty, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentFinancialAidChecklist>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create student financial aid checklist");
                throw;
            }
        }

        /// <summary>
        /// Creates a new student FA checklist for a given year
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="year"></param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>StudentFinancialAidChecklist object</returns>
        public async Task<StudentFinancialAidChecklist> CreateStudentFinancialAidChecklistAsync(string studentId, string year, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (string.IsNullOrEmpty(year))
            {
                throw new ArgumentNullException("year");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _financialAidChecklistPath, year);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(string.Empty, combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentFinancialAidChecklist>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create student financial aid checklist");
                throw;
            }
        }

        /// <summary>
        /// Get all student checklists for all active years
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns>List of StudentFinancialAidChecklist objects</returns>
        [Obsolete("Obsolete as of API 1.12. Use GetAllStudentChecklistsAsync.")]
        public IEnumerable<StudentFinancialAidChecklist> GetAllStudentChecklists(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _financialAidChecklistPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentFinancialAidChecklist>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student checklists");
                throw;
            }
        }

        /// <summary>
        /// Get all student checklists for all active years
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>List of StudentFinancialAidChecklist objects</returns>
        public async Task<IEnumerable<StudentFinancialAidChecklist>> GetAllStudentChecklistsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _financialAidChecklistPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentFinancialAidChecklist>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student checklists");
                throw;
            }
        }

        /// <summary>
        /// Get a student checklist for a given year
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="year"></param>
        /// <returns>a StudentFinancialAidChecklist object</returns>
        [Obsolete("Obsolete as of API 1.12. Use GetStudentFinancialAidChecklistAsync.")]
        public StudentFinancialAidChecklist GetStudentFinancialAidChecklist(string studentId, string year)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (string.IsNullOrEmpty(year))
            {
                throw new ArgumentNullException("year");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _financialAidChecklistPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentFinancialAidChecklist>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student checklist");
                throw;
            }

        }

        /// <summary>
        /// Get a student checklist for a given year
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="year"></param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>a StudentFinancialAidChecklist object</returns>
        public async Task<StudentFinancialAidChecklist> GetStudentFinancialAidChecklistAsync(string studentId, string year, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (string.IsNullOrEmpty(year))
            {
                throw new ArgumentNullException("year");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _financialAidChecklistPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentFinancialAidChecklist>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student checklist");
                throw;
            }

        }


        public async Task<IEnumerable<StudentDefaultAwardPeriod>> GetStudentDefaultAwardPeriods(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentDefaultAwardPeriodsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentDefaultAwardPeriod>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student checklist");
                throw;
            }
        }


        /// <summary>
        /// Retrieve all Award Periods from Colleague.
        /// </summary>
        /// <returns>A list of Award Period data objects</returns>
        public IEnumerable<AwardPeriod> GetAwardPeriods()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_awardPeriodsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardPeriod>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award periods resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award periods");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all Award Statuses from Colleague
        /// </summary>
        /// <returns>A list of Award Status data objects</returns>
        public IEnumerable<AwardStatus> GetAwardStatuses()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_awardStatusesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardStatus>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award statuses resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award periods");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all Awards from Colleague
        /// </summary>
        /// <returns>A list of Award data objects</returns>
        [Obsolete("Obsolete as of API version 1.8. Use GetAwards2.")]
        public IEnumerable<Award> GetAwards()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_awardsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Award>>(response.Content.ReadAsStringAsync().Result);
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
        /// Retrieve all Award Types and their descriptions
        /// </summary>
        /// <returns>A list of all Award Types data objects</returns>
        public IEnumerable<AwardType> GetAwardTypes()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_awardTypesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award types resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award types");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all Award Years from Colleague
        /// </summary>
        /// <returns>A list of all Award Year data objects</returns>
        public IEnumerable<AwardYear> GetAwardYears()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_awardYearsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<AwardYear>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find award years resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get award years");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all Budget Components for all award years
        /// </summary>
        /// <returns>A list of all budget components</returns>
        public IEnumerable<BudgetComponent> GetFinancialAidBudgetComponents()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_financialAidBudgetComponentsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<BudgetComponent>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get budget components");
                throw;
            }
        }

        /// <summary>
        /// Get all checklist items
        /// </summary>
        /// <returns>A list of checklist items</returns>
        public IEnumerable<ChecklistItem> GetFinancialAidChecklistItems()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_financialAidChecklistItemsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ChecklistItem>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get checklist items");
                throw;
            }
        }


        /// <summary>
        /// Retrieve a student's FAFSAs for all award years
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to retrieve FAFSAs</param>
        /// <returns>A list of FAFSAs specific to the student</returns>
        public IEnumerable<Fafsa> GetStudentFafsas(string studentId)
        {
            if (studentId == null)
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _fafsasPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Fafsa>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find fafsas resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get fafsas");
                throw;
            }
        }

        /// <summary>
        /// Retrieve a student's FAFSAs for all award years
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to retrieve FAFSAs</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of FAFSAs specific to the student</returns>
        public async Task<IEnumerable<Fafsa>> GetStudentFafsasAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (studentId == null)
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _fafsasPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Fafsa>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find fafsas resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get fafsas");
                throw;
            }
        }

        /// <summary>
        /// Get federally flagged FAFSA Data for a list of students
        /// </summary>
        /// <param name="studentIds">List of Students to get FAFSA data for</param>
        /// <param name="awardYear">Award Year to get FAFSA data for</param>
        /// <param name="term">Term to get award years from</param>
        /// <returns>List of FAFSA objects</returns>
        public IEnumerable<Fafsa> GetFafsaByStudentIds(IEnumerable<string> studentIds, string awardYear = null, string term = null)
        {
            FafsaQueryCriteria criteria = new FafsaQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.AwardYear = awardYear;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Fafsa Data retrieval.");
            }
            if (string.IsNullOrEmpty(awardYear) && string.IsNullOrEmpty(term))
            {
                throw new ArgumentNullException("awardYear", "The award year or term must be specified for Fafsa Data retrieval.");
            }

            try
            {
                // Build url path from qapi path and fafsa path
                string[] pathStrings = new string[] { _qapiPath, _fafsaPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Fafsa>>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find fafsa resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get fafsa data");
                throw;
            }
        }

        /// <summary>
        /// Get federally flagged FAFSA Data for a list of students
        /// </summary>
        /// <param name="studentIds">List of Students to get FAFSA data for</param>
        /// <param name="awardYear">Award Year to get FAFSA data for</param>
        /// <param name="term">Term to get award years from</param>
        /// <returns>List of FAFSA objects</returns>
        public async Task<IEnumerable<Fafsa>> GetFafsaByStudentIdsAsync(IEnumerable<string> studentIds, string awardYear = null, string term = null)
        {
            FafsaQueryCriteria criteria = new FafsaQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.AwardYear = awardYear;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Fafsa Data retrieval.");
            }
            if (string.IsNullOrEmpty(awardYear) && string.IsNullOrEmpty(term))
            {
                throw new ArgumentNullException("awardYear", "The award year or term must be specified for Fafsa Data retrieval.");
            }

            try
            {
                // Build url path from qapi path and fafsa path
                string[] pathStrings = new string[] { _qapiPath, _fafsaPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Fafsa>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find fafsa resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get fafsa data");
                throw;
            }
        }

        /// <summary>
        /// Obsolete as of API version 1.7. Deprecated. 
        /// Retrieve all Financial Aid Applications from Colleague.
        /// </summary>
        /// <returns>A list of Financial Aid Applications data objects</returns>
        [Obsolete("Obsolete as of API version 1.7. Deprecated. Use GetStudentFafsas and GetProfileApplications methods instead.")]
        public IEnumerable<FinancialAidApplication> GetFinancialAidApplications(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _financialAidApplicationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<FinancialAidApplication>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find financial aid applications resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get financial aid applications");
                throw;
            }
        }

        /// <summary>
        /// Get a FinancialAidCounselor
        /// </summary>
        /// <param name="counselorId">Colleague PERSON id of the counselor to get</param>
        /// <returns>FinancialAidCounselor</returns>
        public FinancialAidCounselor GetFinancialAidCounselor(string counselorId)
        {
            if (string.IsNullOrEmpty(counselorId))
            {
                throw new ArgumentNullException("counselorId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_financialAidCounselorsPath, counselorId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<FinancialAidCounselor>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find financial aid counselor resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get financial aid counselor");
                throw;
            }
        }

        /// <summary>
        /// Get Financial Aid Counselors async
        /// </summary>
        /// <param name="criteria">FinancialAidCounselor query criteria</param>
        /// <returns>List of Financial Aid Counselor DTOs</returns>
        public async Task<IEnumerable<FinancialAidCounselor>> GetFinancialAidCounselorsByIdAsync(FinancialAidCounselorQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "criteria cannot be null for Counselors retrieval");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _financialAidCounselorsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<FinancialAidCounselor>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.            
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get financial aid counselors");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all Financial Aid Offices from Colleague.
        /// </summary>
        /// <returns>A list of FinancialAidOffice data objects</returns>
        [Obsolete("Obsolete as of API version 1.14. Deprecated. Use GetFinancialAidOffices2 method instead.")]
        public IEnumerable<FinancialAidOffice> GetFinancialAidOffices()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_financialAidOfficesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<FinancialAidOffice>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find financial aid offices resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get financial aid offices");
                throw;
            }
        }

        [Obsolete("Obsolete as of API version 1.15. Deprecated. Use GetFinancialAidOffices3Async method instead.")]
        public async Task<IEnumerable<FinancialAidOffice2>> GetFinancialAidOffices2Async()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_financialAidOfficesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<FinancialAidOffice2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find financial aid offices resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get financial aid offices");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all Financial Aid Offices from Colleague.
        /// </summary>
        /// <returns>A list of FinancialAidOffice3 DTOs</returns>
        public async Task<IEnumerable<FinancialAidOffice3>> GetFinancialAidOffices3Async()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_financialAidOfficesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<FinancialAidOffice3>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find financial aid offices resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get financial aid offices");
                throw;
            }
        }


        /// <summary>
        /// Query by post method used to get IpedsInstitution objects for the given OPE (Office of Postsecondary Education) Ids
        /// </summary>
        /// <param name="opeIds">List of Office of Postsecondary Education Ids</param>
        /// <returns>The requested IpedsInstitution DTOs</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input argument is null or empty</exception>
        public async Task<IEnumerable<IpedsInstitution>> GetIpedsInstitutionsAsync(IEnumerable<string> opeIds)
        {
            if (opeIds == null || !opeIds.Any())
            {
                throw new ArgumentNullException("opeIds cannot be null or empty");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _ipedsInstitutionsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync<IEnumerable<string>>(opeIds, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<IpedsInstitution>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get specified IPEDS Institutions");
                throw;
            }
        }

        /// <summary>
        /// Retrieve all Links, their Title and Type from Colleague
        /// </summary>
        /// <returns>A list of all Links data objects</returns>
        public IEnumerable<Link> GetLinks()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_linksPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Link>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find the link resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the links");
                throw;
            }
        }

        /// <summary>
        /// Get a LoanRequest with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier of the LoanRequest resource to get</param>
        /// <returns>LoanRequest resource with the specified id.</returns>
        public LoanRequest GetLoanRequest(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_loanRequestsPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<LoanRequest>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find the LoanRequest resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the LoanRequest resource");
                throw;
            }
        }

        /// <summary>
        /// Get a LoanRequest with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier of the LoanRequest resource to get</param>
        /// <returns>LoanRequest resource with the specified id.</returns>
        public async Task<LoanRequest> GetLoanRequestAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_loanRequestsPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<LoanRequest>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find the LoanRequest resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the LoanRequest resource");
                throw;
            }
        }

        /// <summary>
        /// Create a new LoanRequest
        /// </summary>
        /// <param name="newLoanRequest">A LoanRequest DTO containing the StudentId, AwardYear and TotalRequestAmount</param>
        /// <returns>A LoanRequest object committed to the database.</returns>
        public LoanRequest CreateLoanRequest(LoanRequest newLoanRequest)
        {
            if (newLoanRequest == null)
            {
                throw new ArgumentNullException("newLoanRequest");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_loanRequestsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(newLoanRequest, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<LoanRequest>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create the LoanRequest");
                throw;
            }
        }

        /// <summary>
        /// Create a new LoanRequest
        /// </summary>
        /// <param name="newLoanRequest">A LoanRequest DTO containing the StudentId, AwardYear and TotalRequestAmount</param>
        /// <returns>A LoanRequest object committed to the database.</returns>
        public async Task<LoanRequest> CreateLoanRequestAsync(LoanRequest newLoanRequest)
        {
            if (newLoanRequest == null)
            {
                throw new ArgumentNullException("newLoanRequest");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_loanRequestsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(newLoanRequest, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<LoanRequest>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create the LoanRequest");
                throw;
            }
        }

        /// <summary>
        /// Get PROFILE applications for the student id
        /// </summary>
        /// <param name="studentId">person id to get profile applications for</param>
        /// <returns>Enumeration of PROFILE applications</returns>
        public IEnumerable<ProfileApplication> GetProfileApplications(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _profileApplicationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ProfileApplication>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student's profile applications resources");
                throw;
            }
        }

        /// <summary>
        /// Get PROFILE applications for the student id
        /// </summary>
        /// <param name="studentId">person id to get profile applications for</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>Enumeration of PROFILE applications</returns>
        public async Task<IEnumerable<ProfileApplication>> GetProfileApplicationsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _profileApplicationsPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ProfileApplication>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student's profile applications resources");
                throw;
            }
        }

        /// <summary>
        /// Get all student shopping sheets for award years on student's file
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <returns>A list of Shopping sheets for the specified studentId</returns>
        public IEnumerable<ShoppingSheet> GetStudentShoppingSheets(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be null or empty");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentShoppingSheetsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ShoppingSheet>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student shopping sheets");
                throw;
            }
        }

        /// <summary>
        /// Get all student shopping sheets for award years on student's file
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <returns>A list of Shopping sheets for the specified studentId</returns>
        public async Task<IEnumerable<ShoppingSheet>> GetStudentShoppingSheetsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be null or empty");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentShoppingSheetsPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ShoppingSheet>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student shopping sheets");
                throw;
            }
        }

        /// <summary>
        /// Get all student shopping sheets for award years on student's file
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <returns>A list of Shopping sheets for the specified studentId</returns>
        public async Task<IEnumerable<ShoppingSheet2>> GetStudentShoppingSheets2Async(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be null or empty");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentShoppingSheetsPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ShoppingSheet2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student shopping sheets");
                throw;
            }
        }

        /// <summary>
        /// Get all student shopping sheets for award years on student's file
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <returns>A list of Shopping sheets for the specified studentId</returns>
        public async Task<IEnumerable<ShoppingSheet3>> GetStudentShoppingSheets3Async(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be null or empty");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentShoppingSheetsPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ShoppingSheet3>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student shopping sheets");
                throw;
            }
        }

        /// <summary>
        /// Gets all of a student's awards across all active years for which the student has award data.
        /// </summary>
        /// <param name="studentId">The Colleague studentId for which to get awards</param>
        /// <returns>A list of StudentAward objects for the given studentId.</returns>
        public IEnumerable<StudentAward> GetStudentAwards(string studentId)
        {
            //make sure ID is legit
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be null or empty");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentAward>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get student awards");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student awards");
                throw;
            }
        }

        /// <summary>
        /// Gets all of a student's awards across all active years for which the student has award data.
        /// </summary>
        /// <param name="studentId">The Colleague studentId for which to get awards</param>
        /// <returns>A list of StudentAward objects for the given studentId.</returns>
        public async Task<IEnumerable<StudentAward>> GetStudentAwardsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            //make sure ID is legit
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be null or empty");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentAward>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get student awards");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student awards");
                throw;
            }
        }
        /// <summary>
        /// Get a list of student awards for a single award year.
        /// </summary>
        /// <param name="studentId">Id of the student for whom to retrieve data.</param>
        /// <param name="awardYear">Award Year to retrieve award data from</param>
        /// <returns>A list of the student's awards for the given year.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are empty or null.</exception>
        public IEnumerable<StudentAward> GetStudentAwards(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear", "Award Year cannot be null or empty");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentAward>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student awards");
                throw;
            }
        }

        /// <summary>
        /// Get a list of student awards for a single award year.
        /// </summary>
        /// <param name="studentId">Id of the student for whom to retrieve data.</param>
        /// <param name="awardYear">Award Year to retrieve award data from</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of the student's awards for the given year.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are empty or null.</exception>
        public async Task<IEnumerable<StudentAward>> GetStudentAwardsAsync(string studentId, string awardYear, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear", "Award Year cannot be null or empty");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath, awardYear);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentAward>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student awards");
                throw;
            }
        }

        /// <summary>
        /// Get Student Award Data for a list of students
        /// </summary>
        /// <param name="studentIds">List of Students to get Award data for</param>
        /// <param name="awardYear">Award Year to get Award data for</param>
        /// <param name="term">Term (instead of Year) to get Award data for</param>
        /// <returns>List of Student Award Summary Objects</returns>
        public IEnumerable<StudentAwardSummary> GetStudentAwardSummaryByIds(IEnumerable<string> studentIds, string awardYear = null, string term = null)
        {
            StudentAwardSummaryQueryCriteria criteria = new StudentAwardSummaryQueryCriteria();
            criteria.StudentIds = studentIds;
            criteria.AwardYear = awardYear;
            criteria.Term = term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds", "IDs cannot be empty/null for Student Award Data retrieval.");
            }
            if (string.IsNullOrEmpty(awardYear) && string.IsNullOrEmpty(term))
            {
                throw new ArgumentNullException("awardYear", "The award year or term must be specified for Student Award Data retrieval.");
            }
            try
            {
                // Build url path from qapi path and fafsa path
                string[] pathStrings = new string[] { _qapiPath, _studentAwardSummaryPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<StudentAwardSummary>>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to find student award summary resource");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student award summary");
                throw;
            }
        }

        /// <summary>
        /// Update the awards contained in the StudentAwardPackage resource. The StudentAwards contained
        /// in the body of the request must match the resource identifiers in the URL. This performs an all or nothing update. 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardYear"></param>
        /// <param name="studentAwardPackage"></param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>updated award package</returns>
        public StudentAwardPackage PutStudentAwardPackage(string studentId, string awardYear, StudentAwardPackage studentAwardPackage, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (studentAwardPackage == null)
            {
                throw new ArgumentNullException("studentAwardPackage");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath, awardYear);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePutRequestWithResponse<StudentAwardPackage>(studentAwardPackage, combinedUrl);
                var resource = JsonConvert.DeserializeObject<StudentAwardPackage>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to update StudentAwardPackage for Year {0}, and Student {1}", awardYear, studentId));
                throw;
            }
        }

        /// <summary>
        /// Update the awards contained in the StudentAwardPackage resource. The StudentAwards contained
        /// in the body of the request must match the resource identifiers in the URL. This performs an all or nothing update. 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardYear"></param>
        /// <param name="studentAwardPackage"></param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>updatd award package</returns>
        public async Task<StudentAwardPackage> PutStudentAwardPackageAsync(string studentId, string awardYear, StudentAwardPackage studentAwardPackage, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (studentAwardPackage == null)
            {
                throw new ArgumentNullException("studentAwardPackage");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath, awardYear);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync<StudentAwardPackage>(studentAwardPackage, combinedUrl);
                var resource = JsonConvert.DeserializeObject<StudentAwardPackage>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to update StudentAwardPackage for Year {0}, and Student {1}", awardYear, studentId));
                throw;
            }
        }

        /// <summary>
        /// Update a single student award resource
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardYear"></param>
        /// <param name="awardId"></param>
        /// <param name="studentAward"></param>
        /// <returns>updated student award</returns>
        public StudentAward PutStudentAward(string studentId, string awardYear, string awardId, StudentAward studentAward)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId");
            }
            if (studentAward == null)
            {
                throw new ArgumentNullException("studentAward");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath, awardYear, awardId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePutRequestWithResponse<StudentAward>(studentAward, urlPath);
                var resource = JsonConvert.DeserializeObject<StudentAward>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to update StudentAwardPackage for Year {0}, and Student {1}", awardYear, studentId));
                throw;
            }
        }

        /// <summary>
        /// Update a single student award resource
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardYear"></param>
        /// <param name="awardId"></param>
        /// <param name="studentAward"></param>
        /// <returns>updated student award</returns>
        public async Task<StudentAward> PutStudentAwardAsync(string studentId, string awardYear, string awardId, StudentAward studentAward)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId");
            }
            if (studentAward == null)
            {
                throw new ArgumentNullException("studentAward");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath, awardYear, awardId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync<StudentAward>(studentAward, urlPath);
                var resource = JsonConvert.DeserializeObject<StudentAward>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to update StudentAwardPackage for Year {0}, and Student {1}", awardYear, studentId));
                throw;
            }
        }


        /// <summary>
        /// Update all awards for a single award year. This method looks for differences between the given awards and the
        /// current state of those awards in the database. If there are differences, the awards can be updated with new information, 
        /// but only if the updates meet certain criteria.
        /// </summary>
        /// <param name="studentAwards">A list of all awards in a single award year.</param>
        /// <returns>A list of all awards in a single award year containing any actual updates to the awards in the database.</returns>
        [Obsolete("Obsolete as of API 1.7. Use PutStudentAwardPackage instead")]
        public IEnumerable<StudentAward> UpdateStudentAwards(IEnumerable<StudentAward> studentAwards)
        {
            if (studentAwards == null || studentAwards.Count() < 1)
            {
                throw new ArgumentNullException("studentAwards", "StudentAwards cannot be null or empty");
            }

            var studentId = studentAwards.First().StudentId;
            var awardYear = studentAwards.First().AwardYearId;
            if (string.IsNullOrEmpty(studentId))
            {
                throw new InvalidOperationException("StudentAwards must contain a StudentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new InvalidOperationException("StudentAwards must contain an Award Year");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse<IEnumerable<StudentAward>>(studentAwards, urlPath);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentAward>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to update StudentAwards for Year {0}, and Student {1}", awardYear, studentId));
                throw;
            }
        }

        /// <summary>
        /// This method gets a single StudentAward object
        /// </summary>
        /// <param name="studentId">The student</param>
        /// <param name="awardYear">The award year</param>
        /// <param name="awardId">The award id of the StudentAward object to get</param>
        /// <returns>A single StudentAward object based on the criteria in the three arguments</returns>
        /// <exception cref="ArgumentNullException">Thrown if any arguments are empty or null</exception>
        public StudentAward GetStudentAward(string studentId, string awardYear, string awardId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear", "Award Year cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId", "Award Id cannot be null or empty");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath, awardYear, awardId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentAward>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student awards");
                throw;
            }
        }

        /// <summary>
        /// This method gets a single StudentAward object
        /// </summary>
        /// <param name="studentId">The student</param>
        /// <param name="awardYear">The award year</param>
        /// <param name="awardId">The award id of the StudentAward object to get</param>
        /// <returns>A single StudentAward object based on the criteria in the three arguments</returns>
        /// <exception cref="ArgumentNullException">Thrown if any arguments are empty or null</exception>
        public async Task<StudentAward> GetStudentAwardAsync(string studentId, string awardYear, string awardId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear", "Award Year cannot be null or empty");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId", "Award Id cannot be null or empty");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _awardsPath, awardYear, awardId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentAward>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student awards");
                throw;
            }
        }

        /// <summary>
        /// This method attempts to update Colleague with the data in the given StudentAward 
        /// </summary>
        /// <param name="studentAward">StudentAward object containing the data with which to update Colleague</param>
        /// <returns>A StudentAward object containing any updates made to Colleague</returns>
        [Obsolete("Obsolete as of version 1.7. Instead, use PUT students/{studentId}/awards/{year}")]
        public StudentAward UpdateStudentAward(StudentAward studentAward)
        {
            if (studentAward == null)
            {
                throw new ArgumentNullException("studentAward");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentAward.StudentId, _awardsPath, studentAward.AwardYearId, studentAward.AwardId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse<StudentAward>(studentAward, urlPath);
                var resource = JsonConvert.DeserializeObject<StudentAward>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to update StudentAward for Year {0}, Student {1}, Award {2}", studentAward.AwardYearId, studentAward.StudentId, studentAward.AwardId));
                throw;
            }
        }

        /// <summary>
        /// This method gets all of a student's FA years on file
        /// </summary>
        /// <param name="studentId">Student Id for whom to retrieve award years</param>
        /// <returns>A list of the given student's financial aid years</returns>
        [Obsolete("Obsolete as of API version 1.8. Use GetStudentAwardYears2.")]
        public IEnumerable<StudentAwardYear> GetStudentAwardYears(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentAwardYearsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentAwardYear>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student award years");
                throw;
            }
        }

        /// <summary>
        /// This method gets all of a student's FA years on file
        /// </summary>
        /// <param name="studentId">Student Id for whom to retrieve award years</param>
        /// <returns>A list of the given student's financial aid years</returns>
        public IEnumerable<StudentAwardYear2> GetStudentAwardYears2(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentAwardYearsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentAwardYear2>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student award years");
                throw;
            }
        }


        /// <summary>
        /// This method gets all of a student's FA years on file
        /// </summary>
        /// <param name="studentId">Student Id for whom to retrieve award years</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of the given student's financial aid years</returns>
        public async Task<IEnumerable<StudentAwardYear2>> GetStudentAwardYears2Async(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentAwardYearsPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentAwardYear2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student award years");
                throw;
            }
        }

        /// <summary>
        /// This method gets the specified FA award year on student's file
        /// </summary>
        /// <param name="studentId">Student Id for whom to retrieve award years</param>
        /// <param name="awardYear">Award year code for which to retrieve award year data</param>
        /// <returns>specified financial aid year from student's file</returns>
        [Obsolete("Obsolete as of API version 1.8. Use GetStudentAwardYear2.")]
        public StudentAwardYear GetStudentAwardYear(string studentId, string awardYear)
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
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentAwardYearsPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentAwardYear>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get {0} student award year", awardYear));
                throw;
            }
        }

        /// <summary>
        /// This method gets the specified FA award year on student's file
        /// </summary>
        /// <param name="studentId">Student Id for whom to retrieve award years</param>
        /// <param name="awardYear">Award year code for which to retrieve award year data</param>
        /// <returns>specified financial aid year from student's file</returns>
        public StudentAwardYear2 GetStudentAwardYear2(string studentId, string awardYear)
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
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentAwardYearsPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentAwardYear2>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get {0} student award year", awardYear));
                throw;
            }
        }        

        /// <summary>
        /// Updates the paper copy option flag on student's FIN.AID record
        /// </summary>
        /// <param name="studentId">studentId</param>
        /// <param name="studentAwardYear">studentAwardYear dto</param>
        /// <returns>studentAwardYear Dto</returns>
        [Obsolete("Obsolete as of API version 1.8. Use UpdateStudentAwardYear2.")]
        public StudentAwardYear UpdateStudentAwardYear(string studentId, StudentAwardYear studentAwardYear)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentAwardYearsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePutRequestWithResponse<StudentAwardYear>(studentAwardYear, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentAwardYear>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to update {0} student award year", studentAwardYear.Code));
                throw;
            }
        }

        /// <summary>
        /// Updates the paper copy option flag on student's FIN.AID record
        /// </summary>
        /// <param name="studentId">studentId</param>
        /// <param name="studentAwardYear">studentAwardYear2 dto</param>
        /// <returns>studentAwardYear2 Dto</returns>
        public StudentAwardYear2 UpdateStudentAwardYear2(string studentId, StudentAwardYear2 studentAwardYear)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentAwardYearsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecutePutRequestWithResponse<StudentAwardYear2>(studentAwardYear, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentAwardYear2>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to update {0} student award year", studentAwardYear.Code));
                throw;
            }
        }

        /// <summary>
        /// Updates the paper copy option flag on student's FIN.AID record
        /// </summary>
        /// <param name="studentId">studentId</param>
        /// <param name="studentAwardYear">studentAwardYear2 dto</param>
        /// <returns>studentAwardYear2 Dto</returns>
        public async Task<StudentAwardYear2> UpdateStudentAwardYear2Async(string studentId, StudentAwardYear2 studentAwardYear)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentAwardYearsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecutePutRequestWithResponseAsync<StudentAwardYear2>(studentAwardYear, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentAwardYear2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to update {0} student award year", studentAwardYear.Code));
                throw;
            }
        }

        /// <summary>
        /// This method gets all of a student's financial aid documents on file across all FA Years
        /// </summary>
        /// <param name="studentId">Student Id for whom to retrieve documents</param>
        /// <returns>A list of the given student's financial aid documents across all FA Years</returns>
        [Obsolete("Obsolete as of API 1.12. Use GetStudentDocumentsAsync")]
        public IEnumerable<StudentDocument> GetStudentDocuments(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _documentsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentDocument>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student documents");
                throw;
            }
        }

        /// <summary>
        /// This method gets all of a student's financial aid documents on file across all FA Years
        /// </summary>
        /// <param name="studentId">Student Id for whom to retrieve documents</param>
        /// <returns>A list of the given student's financial aid documents across all FA Years</returns>
        public async Task<IEnumerable<StudentDocument>> GetStudentDocumentsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _documentsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentDocument>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student documents");
                throw;
            }
        }

        /// <summary>
        /// Get all of a student's Financial Aid budget components for all award years
        /// </summary>
        /// <param name="studentId">Colleague PERSON id of student for whom to get budget components</param>
        /// <returns></returns>
        public IEnumerable<Dtos.FinancialAid.StudentBudgetComponent> GetStudentFinancialAidBudgetComponents(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _financialAidBudgetComponentsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Dtos.FinancialAid.StudentBudgetComponent>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student financial aid budget components");
                throw;
            }
        }

        /// <summary>
        /// Get all of a student's Financial Aid budget components for all award years
        /// </summary>
        /// <param name="studentId">Colleague PERSON id of student for whom to get budget components</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.FinancialAid.StudentBudgetComponent>> GetStudentFinancialAidBudgetComponentsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string queryString = UrlUtility.BuildEncodedQueryString("getActiveYearsOnly", getActiveYearsOnly.ToString());
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _financialAidBudgetComponentsPath);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Dtos.FinancialAid.StudentBudgetComponent>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student financial aid budget components");
                throw;
            }
        }

        /// <summary>
        /// Get all of a student's Financial Aid budget components for the specified year
        /// </summary>
        /// <param name="studentId">Colleague PERSON id of student for whom to get budget components</param>
        /// <param name="awardYear">award year to retrieve budget for</param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.FinancialAid.StudentBudgetComponent>> GetStudentBudgetComponentsForYearAsync(string studentId, string awardYear)
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
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _financialAidBudgetComponentsPath, awardYear);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Dtos.FinancialAid.StudentBudgetComponent>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student financial aid budget components");
                throw;
            }
        }

        /// <summary>
        /// Gets all of a student's loan limitations for each year a student has awards.
        /// </summary>
        /// <param name="studentId">The student id for whom to retrieve the loan limitations</param>
        /// <returns>A list of a student's loan limitations for all years the student has awards</returns>
        public async Task<IEnumerable<StudentLoanLimitation>> GetStudentLoanLimitationsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _loanLimitsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<StudentLoanLimitation>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student loan limitations");
                throw;
            }
        }

        /// <summary>
        /// Get Lifetime Summary data about a student's loans
        /// </summary>
        /// <param name="studentId">The student id for whom to retrieve data</param>
        /// <returns>An object containing lifetime summary data about a student's loans.</returns>
        [Obsolete("Obsolete as of API 1.12. Use GetStudentLoanSummaryAsync")]
        public StudentLoanSummary GetStudentLoanSummary(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _loanSummaryPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentLoanSummary>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student loan summary");
                throw;
            }
        }

        /// <summary>
        /// Get Lifetime Summary data about a student's loans
        /// </summary>
        /// <param name="studentId">The student id for whom to retrieve data</param>
        /// <returns>An object containing lifetime summary data about a student's loans.</returns>
        public async Task<StudentLoanSummary> GetStudentLoanSummaryAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _loanSummaryPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentLoanSummary>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get student loan summary");
                throw;
            }
        }

        /// <summary>
        /// Create an outside award self-reported by a student
        /// </summary>
        /// <param name="outsideAward">input outside award data</param>
        /// <returns>created outside award</returns>
        public async Task<OutsideAward> CreateOutsideAwardAsync(Dtos.FinancialAid.OutsideAward outsideAward)
        {
            if (outsideAward == null)
            {
                throw new ArgumentNullException("outsideAward");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentOutsideAwardsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(outsideAward, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<OutsideAward>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to create an outside award record");
                throw;
            }
        }

        /// <summary>
        /// Get outside awrd records for the specified student id and award year
        /// </summary>
        /// <param name="studentId">student id for whom to retrieve outside awards</param>
        /// <param name="awardYearCode">award year code</param>
        /// <returns>List of OutsideAward DTOs</returns>
        public async Task<IEnumerable<OutsideAward>> GetOutsideAwardsAsync(string studentId, string awardYearCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentOutsideAwardsPath, awardYearCode);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<OutsideAward>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve outside award records.");
                throw;
            }
        }

        /// <summary>
        /// Delete outside award with the specified record id
        /// </summary>
        /// <param name="studentId">student id award belongs to</param>
        /// <param name="outsideAwardId">outside award record id</param>
        /// <returns></returns>
        public async Task DeleteOutsideAwardAsync(string studentId, string outsideAwardId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(outsideAwardId))
            {
                throw new ArgumentNullException("outsideAwardId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentOutsideAwardsPath, outsideAwardId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteDeleteRequestWithResponseAsync(urlPath, headers: headers);                
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to delete outside award record.");
                throw;
            }
        }

        /// <summary>
        /// Update an outside award with the specified record.
        /// </summary>
        /// <param name="outsideAward">outside award record to use to update.</param>
        /// <returns></returns>
        public async Task<OutsideAward> UpdateOutsideAwardAsync(Dtos.FinancialAid.OutsideAward outsideAward)
        {
            if (outsideAward == null)
            {
                throw new ArgumentNullException("outsideAward");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentOutsideAwardsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync(outsideAward, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<OutsideAward>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to update outside award record.");
                throw;
            }
        }

        /// <summary>
        /// Gets student nslds related information
        /// </summary>
        /// <param name="studentId">student id for whom to retrieve nslds data</param>
        /// <returns>StudentNsldsInformation DTO</returns>
        public async Task<StudentNsldsInformation> GetStudentNsldsInformationAsync(string studentId)
        {
            if(string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentNsldsInformationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentNsldsInformation>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, string.Format("Unable to retrieve student NSLDS information for student {0}.", studentId));
                throw;
            }
        }

        /// <summary>
        /// Search financial aid persons for the specified criteria
        /// </summary>
        /// <param name="criteria">can be a combination of last, first, and middle names, a person id,
        /// or list of person ids</param>
        /// <returns>List of Person DTOs</returns>
        public async Task<IEnumerable<Person>> QueryFinancialAidPersonsAsync(FinancialAidPersonQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "criteria cannot be null for Financial Aid persons retrieval");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _financialAidPersonsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Person>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.            
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get financial aid persons");
                throw;
            }
        }

        /// <summary>
        /// Gets financial aid explanations
        /// </summary>
        /// <returns>a list of FinancialAidExplanation DTOs</returns>
        public async Task<IEnumerable<FinancialAidExplanation>> GetFinancialAidExplanationsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_financialAidExplanationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject <IEnumerable<FinancialAidExplanation>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, string.Format("Unable to retrieve financial aid explanations data."));
                throw;
            }
        }

        /// <summary>
        /// Gets a parent's Profile information
        /// </summary>
        /// <param name="parentId">Id of the parent</param>
        /// <param name="studentId">Id of the student</param>
        /// <param name="useCache">Defaults to true: If true, cached repository data will be returned when possible, otherwise fresh data is returned.</param>
        /// <returns>A Profile dto for the parent ID</returns>
        /// <exception cref="ResourceNotFoundException">Unable to return profile</exception>
        public async Task<Profile> GetFaProfileAsync(string parentId, string studentId, bool useCache = true)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                throw new ArgumentNullException("parentId", "You must provide the person ID to return profile information.");
            }
            try
            {
                // Build url path
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, parentId, studentId, _financialAidChecklistPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers, useCache: useCache);
                return JsonConvert.DeserializeObject<Profile>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Profile for this person");
                throw;
            }
        }

    }
}
