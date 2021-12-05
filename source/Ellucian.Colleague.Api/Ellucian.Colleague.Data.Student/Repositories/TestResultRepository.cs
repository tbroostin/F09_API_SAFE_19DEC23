// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
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

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class TestResultRepository : BaseColleagueRepository, ITestResultRepository
    {
        private ApplValcodes _noncourseCategories;
        private Data.Base.DataContracts.IntlParams internationalParameters;

        public TestResultRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region ValidationTables
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

        /// <summary>
        /// Read the international parameters records to extract date format used
        /// locally and setup in the INTL parameters.
        /// </summary>
        /// <returns>International Parameters with date properties</returns>
       

        private Ellucian.Colleague.Data.Base.DataContracts.IntlParams GetInternationalParameters()
        {
            if (internationalParameters != null)
            {
                return internationalParameters;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            internationalParameters = GetOrAddToCache<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>("InternationalParameters",
                 () =>
                 {
                     Data.Base.DataContracts.IntlParams intlParams = DataReader.ReadRecord<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                     if (intlParams == null)
                     {
                         var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
                         logger.Info(errorMessage);
                         // If we cannot read the international parameters default to US with a / delimiter.
                         // throw new Exception(errorMessage);
                         Data.Base.DataContracts.IntlParams newIntlParams = new Data.Base.DataContracts.IntlParams();
                         newIntlParams.HostShortDateFormat = "MDY";
                         newIntlParams.HostDateDelimiter = "/";
                         intlParams = newIntlParams;
                     }
                     return intlParams;
                 }, Level1CacheTimeoutValue);
            return internationalParameters;
        }
        
        
        #endregion

        /// <summary>
        /// Gets the test results for a specific student id. 
        /// </summary>
        /// <param name="studentId">Student Id for whom test results are requested. Required.</param>
        /// <returns>List of Test Results</returns>
        public async Task<IEnumerable<TestResult>> GetAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "student id may not be null or empty");
            }
            string criteria = "WITH STNC.PERSON.ID = '" + studentId + "'";
            IEnumerable<TestResult> tests = new List<TestResult>();

            try
            {
                // Get STUDENT.NON.COURSES data for the student
                Collection<StudentNonCourses> studentNonCoursesData;
                studentNonCoursesData = await DataReader.BulkReadRecordAsync<StudentNonCourses>("STUDENT.NON.COURSES", criteria);
                if (studentNonCoursesData != null && studentNonCoursesData.Count() > 0)
                {
                    tests = BuildTestResults(studentNonCoursesData);
                }
            }
            catch (Exception ex)
            {
                //throw new ArgumentException("Student Non-Courses record for " + studentId + " are missing.");
                var errorMessage = string.Format("Student Non-Courses records for '{0}' are missing.", studentId);
                logger.Error(ex.ToString());
                logger.Error(errorMessage);
            }
            return tests;
        }

        public async Task<IEnumerable<TestResult>> GetTestResultsByIdsAsync(string[] studentIds)
        {
            var tests = new List<TestResult>();

            var maxIds = studentIds.Count();
            if (studentIds == null || maxIds <= 0)
            {
                throw new ArgumentNullException("studentIds", "student ids may not be null");
            }

            try
            {
                string criteria = "WITH STNC.PERSON.ID = '?'";
                string[] ids = await DataReader.SelectAsync("STUDENT.NON.COURSES", criteria, studentIds);

                // Get STUDENT.NON.COURSES data for the student
                Collection<StudentNonCourses> studentNonCoursesData = await DataReader.BulkReadRecordAsync<StudentNonCourses>("STUDENT.NON.COURSES", ids);
                if (studentNonCoursesData != null && studentNonCoursesData.Count() > 0)
                {
                    IEnumerable<TestResult> studentTests = new List<TestResult>();
                    studentTests = BuildTestResults(studentNonCoursesData);

                    if (studentTests != null && studentTests.Count() > 0)
                    {
                        foreach (var studentTest in studentTests)
                        {
                            tests.Add(studentTest);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new ArgumentException("Student Non-Courses record for Students are missing.");
                var errorMessage = string.Format("Student Non-Courses records for selected students cannot be read.");
                logger.Error(ex, errorMessage);
            }
            return tests;
        }

        private IEnumerable<TestResult> BuildTestResults(Collection<StudentNonCourses> studentNonCoursesData)
        {
            var tests = new List<TestResult>();
            if (studentNonCoursesData != null && studentNonCoursesData.Count() > 0)
            {
                // Build a list of All SubTest Ids from each student non course selected
                // so we can exclude them from processing on their own instead of as a sub
                // test of the main test.
                var stncSubTestIds = new List<string>();
                foreach (var stncData in studentNonCoursesData)
                {
                    foreach (var subTestId in stncData.StncSubStudentNcrsIds)
                    {
                        stncSubTestIds.Add(subTestId);
                    }
                }
                // Build TestResult Data from student non courses selected
                foreach (var stncData in studentNonCoursesData)
                {
                    // Make sure we are not trying to process a sub test of another test record
                    if (stncSubTestIds.Where(f => f.Equals(stncData.Recordkey)).Count() <= 0)
                    {
                        if ((!string.IsNullOrEmpty(stncData.StncPersonId)) && (!string.IsNullOrEmpty(stncData.StncNonCourse)) && (!string.IsNullOrEmpty(stncData.StncTitle)))
                        {
                            // Restrict to Admissions, Placement, and Other test types
                            var codeAssoc = GetNonCourseCategories().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == stncData.StncCategory).FirstOrDefault();
                            if (codeAssoc == null)
                            {
                                var errorMessage = string.Format("Student ID: '{0}' Student Non Course Record '{1}' has an invalid category of '{2}'", stncData.StncPersonId, stncData.Recordkey, stncData.StncCategory);
                                logger.Error(errorMessage);
                            }
                            if (codeAssoc != null && ((codeAssoc.ValActionCode1AssocMember == "A") || (codeAssoc.ValActionCode1AssocMember == "P") || (codeAssoc.ValActionCode1AssocMember == "T")))
                            {
                                try
                                {
                                    TestType testCategory = TestType.Other;
                                    if (codeAssoc.ValActionCode1AssocMember == "A")
                                    {
                                        testCategory = TestType.Admissions;
                                    }
                                    else if (codeAssoc.ValActionCode1AssocMember == "P")
                                    {
                                        testCategory = TestType.Placement;
                                    }
                                    TestResult stncTest = new TestResult(stncData.StncPersonId, stncData.StncNonCourse, stncData.StncTitle, stncData.StncStartDate.GetValueOrDefault(DateTime.MinValue), testCategory);


                                    stncTest.Score = stncData.StncScoreDec;
                                    stncTest.Percentile = stncData.StncPct;
                                    stncTest.StatusCode = stncData.StncStatus;
                                    stncTest.StatusDate = stncData.StncStatusDate;
                                    //
                                    // Build test sub components table
                                    //
                                    if (stncData.StncSubcomponents != null && stncData.StncSubcomponents.Count > 0)
                                    {
                                        var subComponents = new List<ComponentTest>();
                                        if (stncData.StncSubcomponents != null)
                                        {
                                            int numNonCourseSubs = stncData.StncSubcomponents.Count;
                                            for (int i = 0; i < numNonCourseSubs; i++)
                                            {
                                                string value0 = "";
                                                value0 = stncData.StncSubcomponents[i];

                                                decimal? value1 = null;
                                                if (stncData.StncSubcompScoresDec != null && i < stncData.StncSubcompScoresDec.Count)
                                                {
                                                    value1 = stncData.StncSubcompScoresDec[i];
                                                }

                                                int? value2 = null;
                                                if (stncData.StncSubcomponentPcts != null && i < stncData.StncSubcomponentPcts.Count)
                                                {
                                                    value2 = stncData.StncSubcomponentPcts[i];
                                                }
                                                var newTest = new ComponentTest(value0, value1, value2);

                                                subComponents.Add(newTest);
                                            }
                                            stncTest.ComponentTests = subComponents;
                                        }
                                    }
                                    //
                                    // Build Sub-Tests tables
                                    //
                                    if (stncData.StncSubStudentNcrsIds != null && stncData.StncSubStudentNcrsIds.Count > 0)
                                    {
                                        var subTests = new List<SubTestResult>();
                                        if (stncData.StncSubStudentNcrsIds != null)
                                        {
                                            // Get STUDENT.NON.COURSES data for the student
                                            foreach (var stncRecordKey in stncData.StncSubStudentNcrsIds)
                                            {
                                                StudentNonCourses stncSubData = studentNonCoursesData.Where(s => s.Recordkey.Equals(stncRecordKey)).FirstOrDefault();
                                                if (stncSubData != null)
                                                {
                                                    SubTestResult stncSubTest = new SubTestResult(stncSubData.StncNonCourse, stncSubData.StncTitle, stncSubData.StncStartDate.GetValueOrDefault(DateTime.MinValue));

                                                    stncSubTest.Score = stncSubData.StncScoreDec;
                                                    stncSubTest.Percentile = stncSubData.StncPct;
                                                    stncSubTest.StatusCode = stncSubData.StncStatus;
                                                    stncSubTest.StatusDate = stncSubData.StncStatusDate;

                                                    subTests.Add(stncSubTest);
                                                }
                                            }
                                        }
                                        stncTest.SubTests = subTests;
                                    }
                                    tests.Add(stncTest);
                                }
                                catch (Exception ex)
                                {
                                    var errormsg = string.Format("Student ID: '{0}' Student Non Course '{1}' is invalid.", stncData.StncPersonId, stncData.Recordkey);
                                    var formattedNonCourse = ObjectFormatter.FormatAsXml(stncData);
                                    var errorMessage = string.Format("{0}" + Environment.NewLine + "{1}", errormsg, formattedNonCourse);
                                    
                                    // Log the original exception and a serialized version of the test results
                                    logger.Error(ex.ToString());
                                    logger.Error(errorMessage);
                                    logger.Error(ex.GetBaseException().Message);
                                    logger.Error(ex.GetBaseException().StackTrace);
                                }
                            }
                        }
                    }
                }
            }
            return tests;
        }
    }
}