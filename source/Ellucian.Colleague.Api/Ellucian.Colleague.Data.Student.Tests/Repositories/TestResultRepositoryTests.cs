// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class TestResultRepositoryTests
    {
        // Setup Validation Tables used in Repository
        private ApplValcodes NonCourseCategories = new ApplValcodes();
        private IntlParams internationalParameters = new IntlParams();

        TestResultRepository testResultRepo;
        IEnumerable<TestResult> allTestResults;
        TestResult testResult;

        [TestInitialize]
        public async void Initialize()
        {
            testResultRepo = BuildValidTestResultRepository();
            allTestResults = await new TestTestResultRepository().GetAsync();
            testResult = allTestResults.First();
        }

        [TestCleanup]
        public void Cleanup()
        {
            testResultRepo = null;
        }

        [TestMethod]
        [Ignore]
        public async Task Get_VerifyCount()
        {
            try
            {
                IEnumerable<TestResult> testResults = Task.Run(()=> testResultRepo.GetAsync("0000304")).GetAwaiter().GetResult();
                var testdata = allTestResults.Where(r => r.StudentId == "0000304").ToList();
                Assert.AreEqual(testdata.Count(), testResults.Count());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Trace.WriteLine(e.StackTrace);
                Assert.Fail();
            }
                
        }

        [TestMethod]
        public async Task Get_VerifyProperties()
        {
            try
            {
                IEnumerable<TestResult> testResults = Task.Run(() => testResultRepo.GetAsync("0000304")).GetAwaiter().GetResult();
                Assert.IsTrue(testResults.Count() > 0);
                foreach (var result in testResults)
                {
                    var testResultItem = allTestResults.Where(tt => tt.Code == result.Code && tt.StudentId == "0000304").First();
                    Assert.AreEqual(testResultItem.Description, result.Description);
                    Assert.AreEqual(testResultItem.DateTaken, result.DateTaken);
                    Assert.AreEqual(testResultItem.Category, result.Category);
                    Assert.AreEqual(testResultItem.Percentile, result.Percentile);
                    Assert.AreEqual(testResultItem.StatusCode, result.StatusCode);
                    Assert.AreEqual(testResultItem.StatusDate, result.StatusDate);
                    Assert.AreEqual(testResultItem.Score, result.Score);

                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Trace.WriteLine(e.StackTrace);
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task Get_Some_TestResultsReturnsTestResults()
        {
            List<string> ids = new List<string>();
            ids.Add("10001");
            ids.Add("10002");
            IEnumerable<TestResult> testResults = await testResultRepo.GetTestResultsByIdsAsync(ids.ToArray());
            Assert.IsTrue(testResults.Count() == 2);
        }

        [TestMethod]
        public async Task Get_TestResult_WhenNoResultsReturned()
        {
            IEnumerable<TestResult> testResults = await testResultRepo.GetAsync("Junk");
            Assert.AreEqual(0, testResults.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get_EmptyStudentId_TestResultReturnsException()
        {
            IEnumerable<TestResult> testResults = await testResultRepo.GetAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get_NullStudentId_TestResultReturnsException()
        {
            IEnumerable<TestResult> testResults = await testResultRepo.GetAsync(null);
        }

        private TestResultRepository BuildValidTestResultRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            var localCacheMock = new Mock<ObjectCache>();
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Set up transaction manager for mocking 
            var mockManager = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

            // Set up TestResult Response
            IEnumerable<TestResult> testResults = new TestTestResultRepository().GetAsync().Result;
            var stncResponseData = BuildStudentNonCoursesResponse(testResults);

            // Mock Response for getting test scores for a single student
            string criteria = "WITH STNC.PERSON.ID = '0000304'";
            Collection<StudentNonCourses> studentNonCoursesResponse = BuildStudentNonCoursesResponse(testResults.Where(s => s.StudentId == "0000304"));
            dataAccessorMock.Setup<Task<Collection<StudentNonCourses>>>(acc => acc.BulkReadRecordAsync<StudentNonCourses>("STUDENT.NON.COURSES", criteria, true)).Returns(Task.FromResult(studentNonCoursesResponse));
            
            // Set up testResult response for "all" testResult requests
            dataAccessorMock.Setup<Task<Collection<StudentNonCourses>>>(acc => acc.BulkReadRecordAsync<StudentNonCourses>("STUDENT.NON.COURSES", "", true)).Returns(Task.FromResult(stncResponseData));
            
            // Set up testResult response for a list of testResult Ids request
            var stncCollection = new Collection<StudentNonCourses>();
            stncCollection.Add(stncResponseData.ElementAt(0));
            stncCollection.Add(stncResponseData.ElementAt(1));
            string[] inputIds = new string[] { "10001", "10002"};
            string selectCriteria = "WITH STNC.PERSON.ID = '?'";
            // string[] ids = dataReader.Select("STUDENT.NON.COURSES", criteria, studentIds);
            dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("STUDENT.NON.COURSES", selectCriteria, inputIds, "?", true, 425)).Returns(Task.FromResult(inputIds));
            dataAccessorMock.Setup<Task<Collection<StudentNonCourses>>>(acc => acc.BulkReadRecordAsync<StudentNonCourses>("STUDENT.NON.COURSES", inputIds, true)).Returns(Task.FromResult(stncCollection));
            
            criteria = "WITH STNC.PERSON.ID = '10002'";
            dataAccessorMock.Setup<Task<Collection<StudentNonCourses>>>(acc => acc.BulkReadRecordAsync<StudentNonCourses>("STUDENT.NON.COURSES", criteria, true)).Returns(Task.FromResult(stncCollection));


            // Set up testResult response for a single testResult Id request
            var stncSingleCollection = new Collection<StudentNonCourses>();
            stncSingleCollection.Add(stncResponseData.ElementAt(0));
            string inputId = "10001";
            dataAccessorMock.Setup<Task<Collection<StudentNonCourses>>>(acc => acc.BulkReadRecordAsync<StudentNonCourses>("STUDENT.NON.COURSES", inputId, true)).Returns(Task.FromResult(stncSingleCollection));

            // Build Validatation Tables
            NonCourseCategories.Recordkey = "NON.COURSE.CATEGORIES";
            NonCourseCategories.ValMinimumInputString = new List<string>();
            NonCourseCategories.ValInternalCode = new List<string>();
            NonCourseCategories.ValExternalRepresentation = new List<string>();
            NonCourseCategories.ValActionCode1 = new List<string>();
            NonCourseCategories.ValActionCode2 = new List<string>();
            NonCourseCategories.ValActionCode3 = new List<string>();
            NonCourseCategories.ValActionCode4 = new List<string>();
            NonCourseCategories.ValsEntityAssociation = new List<ApplValcodesVals>();
            NonCourseCategories.ValMinimumInputString.Add("A");
            NonCourseCategories.ValMinimumInputString.Add("P");
            NonCourseCategories.ValMinimumInputString.Add("T");
            NonCourseCategories.ValInternalCode.Add("A");
            NonCourseCategories.ValInternalCode.Add("P");
            NonCourseCategories.ValInternalCode.Add("T");
            NonCourseCategories.ValExternalRepresentation.Add("Admissions");
            NonCourseCategories.ValExternalRepresentation.Add("Placement");
            NonCourseCategories.ValExternalRepresentation.Add("Other");
            NonCourseCategories.ValActionCode1.Add("A");
            NonCourseCategories.ValActionCode1.Add("P");
            NonCourseCategories.ValActionCode1.Add("T");

            for ( int x = 0; x < 3; x++ )
            {
                ApplValcodesVals values = new ApplValcodesVals(NonCourseCategories.ValInternalCode[x],
                    NonCourseCategories.ValExternalRepresentation[x],
                    NonCourseCategories.ValActionCode1[x],
                    NonCourseCategories.ValInternalCode[x], "", "", "");
                    // NonCourseCategories.ValActionCode2[x],
                    // NonCourseCategories.ValActionCode3[x],
                    // NonCourseCategories.ValActionCode4[x]);

                NonCourseCategories.ValsEntityAssociation.Add(values);
            }
            
            dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("ST.VALCODES", "NON.COURSE.CATEGORIES", true)).Returns(NonCourseCategories);

            //// mock STUDENT.NON.COURSE.STATUSES valcode response
            //ApplValcodes nonCourseStatusesResponse = new ApplValcodes()
            //{
            //    ValsEntityAssociation = new List<ApplValcodesVals>() {
            //        new ApplValcodesVals() { ValInternalCodeAssocMember = "W", ValActionCode1AssocMember = "1"},
            //        new ApplValcodesVals() { ValInternalCodeAssocMember = "AC", ValActionCode1AssocMember = "2"},
            //        new ApplValcodesVals() { ValInternalCodeAssocMember = "NT", ValActionCode1AssocMember = "3"}
            //    }
            //};
            //dataAccessorMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("ST.VALCODES", "STUDENT.NON.COURSE.STATUSES", true)).Returns(nonCourseStatusesResponse);

            // Construct testResult repository
            testResultRepo = new TestResultRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            
            return testResultRepo;
        }

        private Collection<StudentNonCourses> BuildStudentNonCoursesResponse(IEnumerable<TestResult> testResults)
        {
            Collection<StudentNonCourses> repoTestResults = new Collection<StudentNonCourses>();
            int xKeyCtr = 10001;
            foreach (var testResult in testResults)
            {
                StudentNonCourses test = new StudentNonCourses();
                test.Recordkey = xKeyCtr.ToString();
                xKeyCtr++;
                test.StncStartDate = testResult.DateTaken;
                test.StncScoreDec = testResult.Score;
                test.StncPct = testResult.Percentile;
                test.StncStatus = testResult.StatusCode;
                test.StncStatusDate = testResult.StatusDate;
                switch (testResult.Category)
                {
                    case TestType.Admissions:
                        test.StncCategory = "A";
                        break;
                    case TestType.Placement:
                        test.StncCategory = "P";
                        break;
                    case TestType.Other:
                        test.StncCategory = "O";
                        break;
                    default:
                        break;
                }
                test.StncPersonId = testResult.StudentId;
                test.StncNonCourse = testResult.Code;
                test.StncTitle = testResult.Description;
                List<string> stncSubStudentNcrsIds = new List<string>();
                stncSubStudentNcrsIds.Add("10007");
                stncSubStudentNcrsIds.Add("10008");
                test.StncSubStudentNcrsIds = stncSubStudentNcrsIds;

                List<StudentNonCoursesNonCourseSubs> NonCourseSubsEntityAssociation = new List<StudentNonCoursesNonCourseSubs>();
                if (testResult.SubTests != null)
                {
                    foreach (var result in testResult.SubTests)
                    {
                        string value0 = "";
                        value0 = result.Code;

                        int? value1 = null;
                        if (result.Score != null)
                        {
                            value1 = (int) result.Score;
                        }

                        int? value2 = null;
                        if (result.Percentile != null)
                        {
                            value2 = result.Percentile;
                        }

                        NonCourseSubsEntityAssociation.Add(new StudentNonCoursesNonCourseSubs(value0, value1, value2, null, result.Score));
                    }
                }

                repoTestResults.Add(test);
            }
            return repoTestResults;
        }
    }
}