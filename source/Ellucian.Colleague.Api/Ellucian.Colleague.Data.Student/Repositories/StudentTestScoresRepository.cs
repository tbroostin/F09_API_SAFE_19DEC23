// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentTestScoresRepository : BaseColleagueRepository, IStudentTestScoresRepository, IEthosExtended
    {
        private ApplValcodes _noncourseCategories;
        private ApplValcodes _studentNonCourseStatuses;
        private ApplValcodes _studentApplicationTestSources;

        private Data.Base.DataContracts.IntlParams internationalParameters;
        private Collection<NonCourses> nonCourseDataContracts;
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        public StudentTestScoresRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        #region ValidationTables
        /// <summary>
        /// Read the Non Course Categories valcode
        /// </summary>
        /// <returns></returns>
        private ApplValcodes GetNonCourseCategories()
        {
            if (_noncourseCategories != null)
            {
                return _noncourseCategories;
            }

            _noncourseCategories = GetOrAddToCache<ApplValcodes>("NonCourseCategories",
                () =>
                {
                    ApplValcodes categoryTable = DataReader.ReadRecord<ApplValcodes>("ST.VALCODES", "NON.COURSE.CATEGORIES");
                    if (categoryTable == null)
                    {
                        var errorMessage = "Unable to access NON.COURSE.CATEGORIES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return categoryTable;
                }, Level1CacheTimeoutValue);
            return _noncourseCategories;
        }

        private ApplValcodes GetStudentNonCourseStatuses()
        {
            if (_studentNonCourseStatuses != null)
            {
                return _studentNonCourseStatuses;
            }

            _studentNonCourseStatuses = GetOrAddToCache<ApplValcodes>("StudentNonCourseStatuses",
                () =>
                {
                    ApplValcodes categoryTable = DataReader.ReadRecord<ApplValcodes>("ST.VALCODES", "STUDENT.NON.COURSE.STATUSES");
                    if (categoryTable == null)
                    {
                        var errorMessage = "Unable to access STUDENT.NON.COURSE.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return categoryTable;
                }, Level1CacheTimeoutValue);
            return _studentNonCourseStatuses;
        }

        private ApplValcodes GetAplicationTestSources()
        {
            if (_studentApplicationTestSources != null)
            {
                return _studentApplicationTestSources;
            }

            _studentApplicationTestSources =  GetOrAddToCache<ApplValcodes>("StudentAplicationTestSources",
                () => 
                {
                    ApplValcodes categoryTable = DataReader.ReadRecord<ApplValcodes>("ST.VALCODES", "APPL.TEST.SOURCES");
                    if (categoryTable == null)
                    {
                        var errorMessage = "Unable to access APPL.TEST.SOURCES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return categoryTable;
                }, Level1CacheTimeoutValue);
            return _studentApplicationTestSources;
        }

        /// <summary>
        /// Read the non courses to find the category for filtering 
       /// </summary>
        /// <returns>non courses data contract</returns>
        private async Task<Collection<NonCourses>> GetNonCourses()
        {
            if (nonCourseDataContracts != null)
            {
                return nonCourseDataContracts;
            }
            nonCourseDataContracts = await DataReader.BulkReadRecordAsync<NonCourses>("");
            return nonCourseDataContracts;
        }


        #endregion

        /// <summary>
        /// Gets student aptitude Assessment for a guid
        /// </summary>
        /// <param name="id">Student Non Course Guid</param>
        /// <returns>List of Test Results</returns>
        public async Task<Domain.Student.Entities.StudentTestScores> GetStudentTestScoresByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                var errorMessage = string.Format("Student Non Courses record with ID : '{0}' is not a valid student aptitude assessment.", id);
                throw new ArgumentException(errorMessage);
            }
            var stuNonCourseId = await GetRecordKeyFromGuidAsync(id);
            try
            {
                if (string.IsNullOrEmpty(stuNonCourseId))
                {
                    var errorMessage = string.Format("Student Non Courses record with ID : '{0}' is not a valid student aptitude assessment.", id);
                    throw new KeyNotFoundException(errorMessage);
                }
                    
                // Get STUDENT.NON.COURSES data for the student
                var tests = new List<Domain.Student.Entities.StudentTestScores>();
                var studentNonCourse = await DataReader.ReadRecordAsync<DataContracts.StudentNonCourses>(stuNonCourseId);
                if ( studentNonCourse == null)
                {
                    throw new KeyNotFoundException("Invalid Student Non Course ID: " + stuNonCourseId);
                }
                if ((!string.IsNullOrEmpty(studentNonCourse.StncPersonId)) && (!string.IsNullOrEmpty(studentNonCourse.StncNonCourse)) && (!string.IsNullOrEmpty(studentNonCourse.StncTitle)))
                {
                    //rather than reading all the non courses, get read one that is needed.
                    //var nonCourse = (await GetNonCourses()).FirstOrDefault(cat => cat.Recordkey == studentNonCourse.StncNonCourse);
                    var nonCourse = await DataReader.ReadRecordAsync < DataContracts.NonCourses > (studentNonCourse.StncNonCourse);
                    if (nonCourse == null)
                    {
                        var errorMessage = string.Format("Student Non Courses record with ID : '{0}' has an invalid non course '{1}'", studentNonCourse.RecordGuid, studentNonCourse.StncNonCourse);
                        throw new ArgumentException(errorMessage);
                    }

                    // Restrict to Admissions, Placement, and Other test types
                    var codeAssoc = GetNonCourseCategories().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == nonCourse.NcrsCategory).FirstOrDefault();
                    if (codeAssoc == null)
                    {
                        var errorMessage = string.Format("Student Non Courses record with ID : '{0}' has an invalid category of '{1}'", studentNonCourse.RecordGuid, nonCourse.NcrsCategory);
                        logger.Warn(errorMessage);
                        throw new ArgumentException(errorMessage);
                    }
                    if (codeAssoc != null && ((codeAssoc.ValActionCode1AssocMember == "A") || (codeAssoc.ValActionCode1AssocMember == "P") || (codeAssoc.ValActionCode1AssocMember == "T")))
                    {
                        var studentNonCoursesData = new Collection<DataContracts.StudentNonCourses>() { studentNonCourse };
                        tests = (BuildStudentTestScores(studentNonCoursesData.ToList())).ToList();
                    }
                    else
                    {
                        var errorMessage = string.Format("Student Non Courses record with ID : '{0}' is not a valid student aptitude assessment.", studentNonCourse.RecordGuid);
                        throw new ArgumentException(errorMessage);
                    }
                }
                else
                {
                    var errorMessage = string.Format("Student Non Courses record with ID : '{0}' is missing required data.", studentNonCourse.RecordGuid);
                    throw new ArgumentException(errorMessage);
                }

             return tests.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

         /// <summary>
        /// Gets student aptitude Assessments
        /// </summary>
        /// <param name="id">Student Non Course Guid</param>
        /// <returns>List of Test Results</returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>> GetStudentTestScoresAsync(string studentFilter, int offset, int limit, bool bypassCache = false)
        {
            try
            {
                string criteria = "";
                if (!string.IsNullOrEmpty(studentFilter))
                {
                    // Get student ID from GUID
                    var stuPersonId= await GetRecordKeyFromGuidAsync(studentFilter);
                    criteria += "WITH STNC.PERSON.ID EQ '" + stuPersonId + "' ";
                }
                criteria += "WITH STNC.NON.COURSE NE '' AND STNC.SCORE.DEC NE ''";
                var ids = await DataReader.SelectAsync("STUDENT.NON.COURSES", criteria);
                Array.Sort(ids);
                var studentNonCourseData = new Collection<DataContracts.StudentNonCourses>();
                for (int i = 0; i < ids.Count(); i += readSize)
                {
                    var subList = ids.Skip(i).Take(readSize).ToArray();
                    var allStudentNonCourseData = await DataReader.BulkReadRecordAsync<DataContracts.StudentNonCourses>("STUDENT.NON.COURSES", subList);
                    foreach (var stncData in allStudentNonCourseData)
                    {
                        // Make sure we are not trying to process a sub test of another test record
                        if ((!string.IsNullOrEmpty(stncData.StncPersonId)) && (!string.IsNullOrEmpty(stncData.StncNonCourse)) && (!string.IsNullOrEmpty(stncData.StncTitle)))
                        {
                            //rather than using the STNC.CATEGORY, we will use the category from the NON Course itself
                            var nonCourse = (await GetNonCourses()).FirstOrDefault(cat => cat.Recordkey == stncData.StncNonCourse);
                            if (nonCourse == null)
                            {
                                var errorMessage = string.Format("Student Non Courses record with ID : '{0}' has an invalid non course '{1}'", stncData.RecordGuid, stncData.StncNonCourse);
                                throw new ArgumentException(errorMessage);
                            }

                            // Restrict to Admissions, Placement, and Other test types
                            var codeAssoc = GetNonCourseCategories().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == nonCourse.NcrsCategory).FirstOrDefault();
                            if (codeAssoc == null)
                            {
                                var errorMessage = string.Format("Student Non Courses record with ID : '{0}' has an invalid category of '{1}'", stncData.RecordGuid, nonCourse.NcrsCategory);
                                logger.Warn(errorMessage);
                                throw new ArgumentException(errorMessage);
                            }
                            if (codeAssoc != null && ((codeAssoc.ValActionCode1AssocMember == "A") || (codeAssoc.ValActionCode1AssocMember == "P") || (codeAssoc.ValActionCode1AssocMember == "T")))
                            {
                                studentNonCourseData.Add(stncData);
                            }


                        }
                    }
                }
                var totalCount = studentNonCourseData.Count();
                var studentNonCourseDataSubList = studentNonCourseData.Skip(offset).Take(limit).ToList();
                var studentNonCourseEntities = BuildStudentTestScores(studentNonCourseDataSubList);
                return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(studentNonCourseEntities, totalCount);
                
            }
            catch (ArgumentException e)
            {
                throw e;
            }
         
        }

        private IEnumerable<Domain.Student.Entities.StudentTestScores> BuildStudentTestScores(List<DataContracts.StudentNonCourses> studentNonCoursesData)
        {
            var tests = new List<Domain.Student.Entities.StudentTestScores>();
            if (studentNonCoursesData != null && studentNonCoursesData.Count() > 0)
            {
                // Build TestResult Data from student non courses selected
                foreach (var stncData in studentNonCoursesData)
                {

                    try
                    {
                        Domain.Student.Entities.StudentTestScores stncTest = new Domain.Student.Entities.StudentTestScores(stncData.RecordGuid, stncData.StncPersonId, stncData.StncNonCourse, stncData.StncTitle, stncData.StncStartDate.GetValueOrDefault(DateTime.MinValue));

                        stncTest.Score = stncData.StncScoreDec;
                        stncTest.Percentile1 = stncData.StncPct;
                        stncTest.Percentile2 = stncData.StncPct2;
                        stncTest.StatusCode = stncData.StncStatus;
                        //get the special processing for the status
                        if (!string.IsNullOrEmpty(stncData.StncStatus))
                        {
                            var statusCodeAssoc = GetStudentNonCourseStatuses().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == stncData.StncStatus).FirstOrDefault();
                            if (statusCodeAssoc == null)
                            {
                                var errorMessage = string.Format("Student Non Courses record with ID : '{0}' has an invalid status of '{1}'", stncData.RecordGuid, stncData.StncStatus);
                                logger.Warn(errorMessage);
                                throw new ArgumentException(errorMessage);
                            }
                            else
                            {
                                stncTest.StatusCodeSpProcessing = statusCodeAssoc.ValActionCode1AssocMember;
                            }
                        }
                        stncTest.StatusDate = stncData.StncStatusDate;
                        stncTest.FormNo = stncData.StncTestFormNo;
                        stncTest.FormName = stncData.StncTestFormName;
                        stncTest.SpecialFactors = stncData.StncSpecialFactors;
                        stncTest.Source = stncData.StncSource;

                        if (!string.IsNullOrWhiteSpace(stncData.StncSource))
                        {
                            var statusCodeAssoc = GetAplicationTestSources().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == stncData.StncSource).FirstOrDefault();
                            if (statusCodeAssoc == null)
                            {
                                var errorMessage = string.Format("Student Non Courses record with ID : '{0}' has an invalid source of '{1}'", stncData.RecordGuid, stncData.StncSource);
                                logger.Warn(errorMessage);
                                throw new ArgumentException(errorMessage);
                            }
                            else
                            {
                                stncTest.ApplicationTestSource = statusCodeAssoc.ValActionCode1AssocMember.Equals("1", StringComparison.OrdinalIgnoreCase) ? "unofficial" : "official";
                            }
                        }
                        else
                        {
                            stncTest.ApplicationTestSource = string.Empty;
                        }
                        tests.Add(stncTest);
                    }

                    catch (ArgumentException e)
                    {
                        throw e;
                    }

                    catch (Exception ex)
                    {
                        var errormsg = string.Format("Student Non Courses record with ID : '{0}' is invalid.", stncData.RecordGuid);
                        var formattedNonCourse = ObjectFormatter.FormatAsXml(stncData);
                        var errorMessage = string.Format("{0}" + Environment.NewLine + "{1}", errormsg, formattedNonCourse);

                        // Log the original exception and a serialized version of the test results
                        logger.Error(ex.ToString());
                        logger.Error(errorMessage);
                        logger.Error(ex.GetBaseException().Message);
                        logger.Error(ex.GetBaseException().StackTrace);
                        throw new ArgumentException(errormsg);
                    }



                }
            }
            return tests;
        }

        public async Task<StudentTestScores> UpdateStudentTestScoresAsync(StudentTestScores studentTestScoresEntity)
        {
            if (studentTestScoresEntity == null)
                throw new ArgumentNullException("studentTestScoresEntity", "Must provide a studentTestScoresEntity to update.");
            if (string.IsNullOrEmpty(studentTestScoresEntity.Guid))
                throw new ArgumentNullException("studentTestScoresEntity", "Must provide the guid of the studentTestScoresEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var studentTestScoresId = await GetRecordKeyFromGuidAsync(studentTestScoresEntity.Guid);

            if (!string.IsNullOrEmpty(studentTestScoresId))
            {

                var updateRequest = BuildStudentAptitudeAssessmentRequest(studentTestScoresEntity);

                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateStudentAptitudeAssessmentRequest, UpdateStudentAptitudeAssessmentResponse>(updateRequest);

                if (updateResponse.UpdateStudentAptitudeAsessmentErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating studentTestScores '{0}':", studentTestScoresEntity.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.UpdateStudentAptitudeAsessmentErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));

                    logger.Error(errorMessage);
                    throw exception;
                }
                // get the updated entity from the database
                return await this.GetStudentTestScoresByGuidAsync(studentTestScoresEntity.Guid);
            }

            // perform a create instead
            return await CreateStudentTestScoresAsync(studentTestScoresEntity);
        }

        private UpdateStudentAptitudeAssessmentRequest BuildStudentAptitudeAssessmentRequest(StudentTestScores studentTestScoresEntity)
        {
            var updateStudentAptitudeAssessmentRequest = new UpdateStudentAptitudeAssessmentRequest();
            updateStudentAptitudeAssessmentRequest.StncGuid = studentTestScoresEntity.Guid;
            updateStudentAptitudeAssessmentRequest.StncNonCourse = studentTestScoresEntity.Code;
            updateStudentAptitudeAssessmentRequest.StncPct = studentTestScoresEntity.Percentile1;
            updateStudentAptitudeAssessmentRequest.StncPct2 = studentTestScoresEntity.Percentile2;
            updateStudentAptitudeAssessmentRequest.StncPersonId = studentTestScoresEntity.StudentId;
            updateStudentAptitudeAssessmentRequest.StncScoreDec = studentTestScoresEntity.Score;
            updateStudentAptitudeAssessmentRequest.StncSource = studentTestScoresEntity.Source;
            updateStudentAptitudeAssessmentRequest.StncSpecialFactors = studentTestScoresEntity.SpecialFactors;
            updateStudentAptitudeAssessmentRequest.StncStartDate = studentTestScoresEntity.DateTaken;
            updateStudentAptitudeAssessmentRequest.StncTestFormName = studentTestScoresEntity.FormName;
            updateStudentAptitudeAssessmentRequest.StncTestFormNo = studentTestScoresEntity.FormNo;
            updateStudentAptitudeAssessmentRequest.StudentNonCoursesId = studentTestScoresEntity.RecordKey;
            updateStudentAptitudeAssessmentRequest.StncDerivedReported = studentTestScoresEntity.ApplicationTestSource;
            updateStudentAptitudeAssessmentRequest.StncDerivedStatus = studentTestScoresEntity.StatusCode;

            // Get Extended Data names and values
            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateStudentAptitudeAssessmentRequest.ExtendedNames = extendedDataTuple.Item1;
                updateStudentAptitudeAssessmentRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            return updateStudentAptitudeAssessmentRequest;
            
        }

        public async Task<StudentTestScores> CreateStudentTestScoresAsync(StudentTestScores studentTestScoresEntity)
        {
            if (studentTestScoresEntity == null)
                throw new ArgumentNullException("studentTestScoresEntity", "Must provide a studentTestScoresEntity to create.");

            var createRequest = BuildStudentAptitudeAssessmentRequest(studentTestScoresEntity);
            createRequest.StudentNonCoursesId = null;
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<UpdateStudentAptitudeAssessmentRequest, UpdateStudentAptitudeAssessmentResponse>(createRequest);

            if (createResponse.UpdateStudentAptitudeAsessmentErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred creating studentTestScores '{0}':", studentTestScoresEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                createResponse.UpdateStudentAptitudeAsessmentErrors.ForEach(e => exception.AddError(new RepositoryError("studentTestScores", e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created studentTestScores from the database
            return await GetStudentTestScoresByGuidAsync(createResponse.StncGuid);

        }

        /// <summary>
        /// Deletes student apptitude assessment
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task DeleteAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            string studentTestScoreKey = await GetStudentTestScoresIdFromGuidAsync(guid);
            if (string.IsNullOrEmpty(studentTestScoreKey))
            {
                var errorMessage = string.Format("Student Non Courses record not found for ID : '{0}' .", guid);
                throw new KeyNotFoundException(errorMessage);
            }

            var request = new DeleteStudentAptitudeAssessmentRequest()
            {
                StncGuid = guid,
                StudentNonCoursesId = studentTestScoreKey
            };

            var response = await transactionInvoker.ExecuteAsync<DeleteStudentAptitudeAssessmentRequest, DeleteStudentAptitudeAssessmentResponse>(request);

            if (response.DeleteStudentAptitudeAssessmentErrors != null && response.DeleteStudentAptitudeAssessmentErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred deleting student aptitude assessment '{0}':", guid);
                var exception = new RepositoryException(errorMessage);
                response.DeleteStudentAptitudeAssessmentErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }
        }

        /// <summary>
        /// Get a studentTestScores id by guid
        /// </summary>
        /// <param name="guid">guid</param>
        /// <returns>id</returns>
        public async Task<string> GetStudentTestScoresIdFromGuidAsync(string guid)
        {      
            return await GetRecordKeyFromGuidAsync(guid);
        }
    }
}