// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

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
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;

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
        const string AllStudentTestScoresCache = "AllStudentTestScores";

        const int AllStudentTestScoresCacheTimeout = 20;

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

        private ApplValcodes GetApplicationTestSources()
        {
            if (_studentApplicationTestSources != null)
            {
                return _studentApplicationTestSources;
            }

            _studentApplicationTestSources = GetOrAddToCache<ApplValcodes>("StudentAplicationTestSources",
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
                var errorMessage = "ID is required to get a student aptitude assessment.";
                throw new ArgumentException(errorMessage);
            }
            var stuNonCourseId = await GetRecordKeyFromGuidAsync(id);

            if (string.IsNullOrEmpty(stuNonCourseId))
            {
                var errorMessage = string.Format("Student Non Courses record with ID : '{0}' is not a valid student aptitude assessment.", id);
                throw new KeyNotFoundException(errorMessage);
            }

            // Get STUDENT.NON.COURSES data for the student
            var tests = new List<Domain.Student.Entities.StudentTestScores>();
            var studentNonCourse = await DataReader.ReadRecordAsync<DataContracts.StudentNonCourses>(stuNonCourseId);
            if (studentNonCourse == null)
            {
                throw new KeyNotFoundException("Invalid Student Non Course ID: " + stuNonCourseId);
            }
            if ((!string.IsNullOrEmpty(studentNonCourse.StncPersonId)) && (!string.IsNullOrEmpty(studentNonCourse.StncNonCourse)) && (!string.IsNullOrEmpty(studentNonCourse.StncTitle) && (!string.IsNullOrEmpty(studentNonCourse.StncCategory))))
            {

                // Restrict to Admissions, Placement, and Other test types
                var nonCourses = GetNonCourseCategories();
                if (nonCourses == null)
                {
                    var errorMessage = "NON.COURSE.CATEGORIES not found.";
                    throw new RepositoryException(errorMessage);
                }
                var codeAssoc = nonCourses.ValsEntityAssociation
                    .Where(v => v.ValInternalCodeAssocMember.Equals(studentNonCourse.StncCategory)).FirstOrDefault();
                if (codeAssoc == null)
                {
                    var errorMessage = string.Format("Student Non Courses record with ID : '{0}' has an invalid category of '{1}'", studentNonCourse.RecordGuid, studentNonCourse.StncCategory);
                    throw new RepositoryException(errorMessage);
                }
                if (codeAssoc != null && ((codeAssoc.ValActionCode1AssocMember == "A") || (codeAssoc.ValActionCode1AssocMember == "P") || (codeAssoc.ValActionCode1AssocMember == "T")))
                {
                    var studentNonCoursesData = new Collection<DataContracts.StudentNonCourses>() { studentNonCourse };
                    tests = (BuildStudentTestScores(studentNonCoursesData.ToList())).ToList();
                }
                else
                {
                    var errorMessage = string.Format("Student Non Courses record with ID : '{0}' is not a valid student aptitude assessment.", studentNonCourse.RecordGuid);
                    throw new RepositoryException(errorMessage);
                }
            }
            else
            {
                var errorMessage = string.Format("Student Non Courses record with ID : '{0}' is missing required data.", studentNonCourse.RecordGuid);
                throw new RepositoryException(errorMessage);
            }

            return tests.FirstOrDefault();

        }

        /// <summary>
        /// Gets student aptitude Assessments
        /// </summary>
        /// <param name="id">Student Non Course Guid</param>
        /// <returns>List of Test Results</returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>> GetStudentTestScoresAsync(string studentId, int offset, int limit, bool bypassCache = false)
        {

            string criteria = "";
            List<string> limitingKeys = new List<string>();
            var studentNonCourseEntities = new List<Domain.Student.Entities.StudentTestScores>();
            string nonCourseTypes = string.Empty;
            var repositoryException = new RepositoryException();
            //get the list of valid non courses type
            // Restrict to Admissions, Placement, and Other test types

            var codeAssoc = GetNonCourseCategories().ValsEntityAssociation.Where(sp1 => sp1.ValActionCode1AssocMember == "A" || sp1.ValActionCode1AssocMember == "P" || sp1.ValActionCode1AssocMember == "T");
            if (codeAssoc != null && codeAssoc.Any())
            {
                foreach (var val in codeAssoc)
                {
                    if (!string.IsNullOrEmpty(val.ValInternalCodeAssocMember))
                        nonCourseTypes = string.Concat(nonCourseTypes, "'", val.ValInternalCodeAssocMember, "'");
                }
            }
            else
                return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(new List<Domain.Student.Entities.StudentTestScores>(), 0);
            //apply student filters
            if (!string.IsNullOrEmpty(studentId))
            {
                // Get student ID from GUID
                criteria = "WITH STNC.PERSON.ID EQ '" + studentId + "' ";
                limitingKeys = (await DataReader.SelectAsync("STUDENT.NON.COURSES", criteria)).ToList();
                if (limitingKeys == null || !limitingKeys.Any())
                    return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(new List<Domain.Student.Entities.StudentTestScores>(), 0);
            }
            //we just want those tests that have the scores. we are going to do this query first as this field is indexed. 
            //criteria = "WITH STNC.SCORE.DEC NE ''";
            //limitingKeys = (await DataReader.SelectAsync("STUDENT.NON.COURSES", limitingKeys != null && limitingKeys.Any() ? limitingKeys.ToArray() : null, criteria)).ToList();
            //if (limitingKeys == null || !limitingKeys.Any())
            //    return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(new List<Domain.Student.Entities.StudentTestScores>(), 0);
            //we are only looking for those non courses that are of type test
            criteria = string.Concat("WITH STNC.SCORE.DEC NE '' AND WITH STNC.CATEGORY EQ ", nonCourseTypes);
            limitingKeys = (await DataReader.SelectAsync("STUDENT.NON.COURSES", limitingKeys != null && limitingKeys.Any() ? limitingKeys.ToArray() : null, criteria)).ToList();
            if (limitingKeys == null || !limitingKeys.Any())
                return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(new List<Domain.Student.Entities.StudentTestScores>(), 0);
            limitingKeys.Sort();
            var totalCount = limitingKeys.Count();
            var studentNonCourseDataSubList = limitingKeys.Skip(offset).Take(limit).ToArray();
            var results = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentNonCourses>("STUDENT.NON.COURSES", studentNonCourseDataSubList);
            if (results.Equals(default(BulkReadOutput<DataContracts.StudentNonCourses>)))
                return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(new List<Domain.Student.Entities.StudentTestScores>(), 0);
            if (results.InvalidKeys.Any() || results.InvalidRecords.Any())
            {
                if (results.InvalidKeys.Any())
                {
                    repositoryException.AddErrors(results.InvalidKeys
                        .Select(key => new RepositoryError("invalid.key",
                        string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                }
                if (results.InvalidRecords.Any())
                {
                    repositoryException.AddErrors(results.InvalidRecords
                       .Select(r => new RepositoryError("invalid.record",
                       string.Format("Error: '{0}'. Entity: 'STUDENT.NON.COURSES', Record ID: '{1}' ", r.Value, r.Key))
                       { }));
                }
                throw repositoryException;
            }
            try
            {
                studentNonCourseEntities = BuildStudentTestScores(results.BulkRecordsRead.ToList()).ToList();

            }
            catch (RepositoryException ex)
            {
                repositoryException.AddErrors(ex.Errors);
            }
            if (repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(studentNonCourseEntities, totalCount);

        }


        /// <summary>
        /// Gets student aptitude Assessments
        /// </summary>
        /// <param name="id">Student Non Course Guid</param>
        /// <returns>List of Test Results</returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>> GetStudentTestScores2Async(int offset, int limit, string studentId = "",
            string assessmentId = "", string[] filterPersonIds = null, string personFilter = "", bool bypassCache = false)
        {

            string criteria = "";
            int totalCount = 0;
            string studentTestScoresCacheKey = CacheSupport.BuildCacheKey(AllStudentTestScoresCache,
                studentId, assessmentId, filterPersonIds, personFilter);
            string[] subList = null;
            var studentNonCourseEntities = new List<Domain.Student.Entities.StudentTestScores>();
            string nonCourseTypes = string.Empty;
            var repositoryException = new RepositoryException();
            //get the list of valid non courses type
            // Restrict to Admissions, Placement, and Other test types

            var nonCourseCategories = this.GetNonCourseCategories();
            if (nonCourseCategories == null)
            {
                return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(new List<Domain.Student.Entities.StudentTestScores>(), 0);
            }
            var codeAssoc = nonCourseCategories.ValsEntityAssociation.Where(sp1 => sp1.ValActionCode1AssocMember == "A" || sp1.ValActionCode1AssocMember == "P" || sp1.ValActionCode1AssocMember == "T");
            if (codeAssoc != null && codeAssoc.Any())
            {
                foreach (var val in codeAssoc)
                {
                    if (!string.IsNullOrEmpty(val.ValInternalCodeAssocMember))
                        nonCourseTypes = string.Concat(nonCourseTypes, "'", val.ValInternalCodeAssocMember, "'");
                }
            }
            else
                return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(new List<Domain.Student.Entities.StudentTestScores>(), 0);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                studentTestScoresCacheKey,
                "",
                offset,
                limit,
                AllStudentTestScoresCacheTimeout,

                async () =>
                {
                    string[] limitingKeys = new string[] { };
                    if (filterPersonIds != null && filterPersonIds.ToList().Any())
                    {
                        // Set limiting keys to previously retrieved personIds from SAVE.LIST.PARMS
                        var personCriteria = "WITH PST.STUDENT.NON.COURSES NE '' BY.EXP PST.STUDENT.NON.COURSES SAVING PST.STUDENT.NON.COURSES";
                        limitingKeys = await DataReader.SelectAsync("PERSON.ST", filterPersonIds, personCriteria);
                        if (limitingKeys == null || !limitingKeys.Any())                       
                            return new CacheSupport.KeyCacheRequirements()
                            {
                                NoQualifyingRecords = true
                            };
                    }
                    //apply student filters
                    if (!string.IsNullOrEmpty(studentId))
                    {
                        // Select the student's record matching the person specified in the filter.
                        criteria = "WITH STNC.PERSON.ID EQ '" + studentId + "' ";
                        limitingKeys = await DataReader.SelectAsync("STUDENT.NON.COURSES", limitingKeys != null && limitingKeys.Any() ? limitingKeys.ToArray() : null, criteria);
                        if (limitingKeys == null || !limitingKeys.Any())
                            return new CacheSupport.KeyCacheRequirements()
                            {
                                NoQualifyingRecords = true
                            };
                    }

                    if (!string.IsNullOrEmpty(assessmentId))
                    {
                        // Select the records matching the noncourse specified in the filter
                        criteria = "WITH STNC.NON.COURSE EQ '" + assessmentId + "' AND STNC.SCORE.DEC NE ''";
                        limitingKeys = await DataReader.SelectAsync("STUDENT.NON.COURSES", limitingKeys != null && limitingKeys.Any() ? limitingKeys.ToArray() : null, criteria);
                        if (limitingKeys == null || !limitingKeys.Any())
                            return new CacheSupport.KeyCacheRequirements()
                            {
                                NoQualifyingRecords = true
                            };
                    }

                    //we are only looking for those non courses that are of type test
                    criteria = string.Concat("WITH STNC.SCORE.DEC NE '' AND WITH STNC.CATEGORY EQ ", nonCourseTypes);
                    limitingKeys = await DataReader.SelectAsync("STUDENT.NON.COURSES", limitingKeys != null && limitingKeys.Any() ? limitingKeys.ToArray() : null, criteria);
                    if (limitingKeys == null || !limitingKeys.Any())
                        return new CacheSupport.KeyCacheRequirements()
                        {
                            NoQualifyingRecords = true
                        };

                    var requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = limitingKeys.Distinct().ToList(),
                    };

                    return requirements;
                });

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(new List<Domain.Student.Entities.StudentTestScores>(), 0);
            }
            subList = keyCache.Sublist.ToArray();

            totalCount = keyCache.TotalCount.Value;
            var results = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentNonCourses>("STUDENT.NON.COURSES", subList);
            if (results.Equals(default(BulkReadOutput<DataContracts.StudentNonCourses>)))
                return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(new List<Domain.Student.Entities.StudentTestScores>(), 0);
            if (results.InvalidKeys.Any() || results.InvalidRecords.Any())
            {
                if (results.InvalidKeys.Any())
                {
                    repositoryException.AddErrors(results.InvalidKeys
                        .Select(key => new RepositoryError("invalid.key",
                        string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                }
                if (results.InvalidRecords.Any())
                {
                    repositoryException.AddErrors(results.InvalidRecords
                       .Select(r => new RepositoryError("invalid.record",
                       string.Format("Error: '{0}'. Entity: 'STUDENT.NON.COURSES', Record ID: '{1}' ", r.Value, r.Key))
                       {  }));
                }
                throw repositoryException;
            }
            try
            {
                studentNonCourseEntities = BuildStudentTestScores(results.BulkRecordsRead.ToList()).ToList();
                
            }
            catch (RepositoryException ex)
            {
                repositoryException.AddErrors(ex.Errors);
            }
            if (repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return new Tuple<IEnumerable<Domain.Student.Entities.StudentTestScores>, int>(studentNonCourseEntities, totalCount);

        }

        private IEnumerable<Domain.Student.Entities.StudentTestScores> BuildStudentTestScores(List<DataContracts.StudentNonCourses> studentNonCoursesData)
        {
            var tests = new List<Domain.Student.Entities.StudentTestScores>();
            var repositoryException = new RepositoryException();
            if (studentNonCoursesData != null && studentNonCoursesData.Any())
            {
                // Build TestResult Data from student non courses selected
                foreach (var stncData in studentNonCoursesData)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(stncData.StncPersonId))
                        {
                            repositoryException.AddError(new RepositoryError("Bad.Data", "Missing Student ID")
                            {
                                SourceId = stncData != null ? stncData.Recordkey : "",
                                Id = stncData != null ? stncData.RecordGuid : ""
                            });
                        }
                        if (string.IsNullOrEmpty(stncData.StncNonCourse))
                        {
                            repositoryException.AddError(new RepositoryError("Bad.Data", "Missing NonCourse code")
                            {
                                SourceId = stncData != null ? stncData.Recordkey : "",
                                Id = stncData != null ? stncData.RecordGuid : ""
                            });
                        }
                        if (string.IsNullOrEmpty(stncData.StncTitle))
                        {
                            repositoryException.AddError(new RepositoryError("Bad.Data", "Missing Title/Description")
                            {
                                SourceId = stncData != null ? stncData.Recordkey : "",
                                Id = stncData != null ? stncData.RecordGuid : ""
                            });
                        }
                        if (string.IsNullOrEmpty(stncData.RecordGuid))
                        {
                            repositoryException.AddError(new RepositoryError("Bad.Data", "Missing RecordGuid")
                            {
                                SourceId = stncData != null ? stncData.Recordkey : "",
                                Id = stncData != null ? stncData.RecordGuid : ""
                            });
                        }
                        if (stncData.StncStartDate == null)
                        {
                            repositoryException.AddError(new RepositoryError("Bad.Data", "Missing start date")
                            {
                                SourceId = stncData != null ? stncData.Recordkey : "",
                                Id = stncData != null ? stncData.RecordGuid : ""
                            });
                        }
                        // check for required fields, otherwise the domain entity constructor will throw errors.
                        if (repositoryException.Errors.Any())
                        {
                            throw repositoryException;
                        }
                        var stncTest = new Domain.Student.Entities.StudentTestScores(stncData.RecordGuid, stncData.StncPersonId, stncData.StncNonCourse, stncData.StncTitle, stncData.StncStartDate.GetValueOrDefault(DateTime.MinValue));

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
                                var errorMessage = string.Format("Student Non Courses record has an invalid status: '{0}'", stncData.StncStatus);
                                
                                repositoryException.AddError(new RepositoryError("Bad.Data", errorMessage)
                                {
                                    SourceId = stncData != null ? stncData.Recordkey : "",
                                    Id = stncData != null ? stncData.RecordGuid : ""
                                });

                            }
                            else
                            {
                                stncTest.StatusCodeSpProcessing = statusCodeAssoc.ValActionCode1AssocMember;
                                stncTest.StatusCodeSpProcessing2 = statusCodeAssoc.ValActionCode2AssocMember;
                            }
                        }
                        stncTest.StatusDate = stncData.StncStatusDate;
                        stncTest.FormNo = stncData.StncTestFormNo;
                        stncTest.FormName = stncData.StncTestFormName;
                        stncTest.SpecialFactors = stncData.StncSpecialFactors;
                        stncTest.Source = stncData.StncSource;

                        if (!string.IsNullOrWhiteSpace(stncData.StncSource))
                        {
                            var statusCodeAssoc = GetApplicationTestSources().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == stncData.StncSource).FirstOrDefault();
                            if (statusCodeAssoc == null)
                            {
                                var errorMessage = string.Format("Student Non Courses record has an invalid source: '{0}'", stncData.StncSource);
                                repositoryException.AddError(new RepositoryError("Bad.Data", errorMessage)
                                {
                                    SourceId = stncData != null ? stncData.Recordkey : "",
                                    Id = stncData != null ? stncData.RecordGuid : ""
                                });
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

                    catch (Exception ex)
                    {
                        repositoryException.AddError(new RepositoryError("Bad.Data", ex.Message)
                        {
                            SourceId = stncData != null ? stncData.Recordkey : "",
                            Id = stncData != null ? stncData.RecordGuid : ""
                        });
                    }
                }
                if (repositoryException.Errors.Any())
                {
                    throw repositoryException;
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
                    updateResponse.UpdateStudentAptitudeAsessmentErrors.ForEach(e => exception.AddError(
                        new RepositoryError("Create.Update.Exception", 
                        string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));

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

            // If we don't have a source but we do have "unofficial" in reported, find the first source that represents "unofficial".
            if (string.IsNullOrWhiteSpace(studentTestScoresEntity.Source) && !string.IsNullOrWhiteSpace(studentTestScoresEntity.ApplicationTestSource) && studentTestScoresEntity.ApplicationTestSource.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                var statusCodeAssoc = GetApplicationTestSources().ValsEntityAssociation.Where(v => v.ValActionCode1AssocMember.Equals("1", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (statusCodeAssoc == null)
                {
                    var errorMessage = string.Format("Error(s) occurred updating studentTestScores '{0}':", studentTestScoresEntity.Guid);
                    var exception = new RepositoryException(errorMessage);
                    exception.AddError(new RepositoryError("Bad.Data", "An 'unofficial' assessment requires a match to source however, no source was found that represents 'unofficial'. "));
                    throw exception;
                }
                else
                {
                    updateStudentAptitudeAssessmentRequest.StncSource = statusCodeAssoc.ValInternalCodeAssocMember;
                }
            }

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
                createResponse.UpdateStudentAptitudeAsessmentErrors.ForEach(e => exception.AddError(
                    new RepositoryError("Create.Update.Exception", e.ErrorMessages)));
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
                response.DeleteStudentAptitudeAssessmentErrors.ForEach(e => exception.AddError(
                    new RepositoryError("Delete.Exception", string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
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