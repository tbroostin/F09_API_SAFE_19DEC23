// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Web.Cache;
using System.Threading.Tasks;
using System.Threading;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{

    /// <summary>
    /// CourseRepositoryTests
    /// </summary>
    [TestClass]
    public class CourseRepositoryTests
    {

        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<IColleagueTransactionInvoker> transInvokerMock;
        //Mock<ObjectCache> localCacheMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<ILogger> loggerMock;
        IEnumerable<Course> allCourses;
        Dictionary<string, Course> allCoursesDict;
        Collection<Courses> coursesResponseData;
        CourseRepository courseRepo;
        ApiSettings apiSettingsMock;
        UpdateCoursesRequest courseRequest;
        UpdateCoursesResponse updateResponse;

        [TestInitialize]
        public async void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            transInvokerMock = new Mock<IColleagueTransactionInvoker>();

            // Build Courses responses used for mocking
            allCourses = await new TestCourseRepository().GetAsync();
            // Build courses dict, response from cache
            allCoursesDict = new Dictionary<string, Course>();
            foreach (var crs in allCourses)
            {
                allCoursesDict[crs.Id] = crs;
            }
            // Repository response data
            coursesResponseData = BuildCoursesResponse(allCourses);

            courseRepo = BuildValidCourseRepository();

            courseRequest = new UpdateCoursesRequest()
            {
                CrsGuid = allCourses.FirstOrDefault().Guid,
            };
            updateResponse = new UpdateCoursesResponse()
            {
                CrsGuid = allCourses.FirstOrDefault().Guid,
            };
            //InitilizeCacheProvider();
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(new string[] { "1", "2", "3" }));

            dataAccessorMock.Setup(acc => acc.Select(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        var course = coursesResponseData.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                        result.Add(gl.Guid, course == null ? null : new GuidLookupResult() { Entity = "COURSES", PrimaryKey = course.Recordkey });
                    }
                    return result;
                });
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<Courses>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                (id, repl) => Task.FromResult(coursesResponseData.FirstOrDefault(c => c.Recordkey == id)));
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns<string>(
                id => Task.FromResult(coursesResponseData.FirstOrDefault(c => c.Recordkey == id)));

        }



        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataAccessorMock = null;
            cacheProviderMock = null;
            //localCacheMock = null;
            coursesResponseData = null;
            allCourses = null;
            courseRepo = null;
        }

        [TestMethod]
        public async Task CourseRepository_GetNonCacheAsync()
        {
            string empty = string.Empty;
            var actuals = await courseRepo.GetNonCacheAsync(empty, empty, empty, empty, empty, empty, empty, empty, empty, empty);

            Assert.IsNotNull(actuals);
        }


        [TestMethod]
        public async Task CourseRepository_GetPagedCoursesAsync()
        {
            var expected = coursesResponseData.FirstOrDefault();

            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).ReturnsAsync(new string[] { expected.Recordkey });
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));
            expected.CrsStartDate = DateTime.Now.AddDays(-1);
            expected.CrsEndDate = DateTime.Now.AddDays(1);
            string empty = string.Empty;
            var actuals = await courseRepo.GetPagedCoursesAsync(0, 10, empty, empty, null, null, empty, null, empty, empty, empty, empty);

            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task CourseRepository_GetAsync_NoCriteria()
        {
            string empty = string.Empty;
            var actuals = await courseRepo.GetAsync(empty, empty, empty, empty, empty, empty, empty, empty, empty, empty);

            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task CourseRepository_GetAsync_WithCriteria()
        {
            string empty = string.Empty;
            string[] ids = coursesResponseData.Where(i => i.CrsSubject.Equals("HIST")).Select(c => c.Recordkey).ToArray();
            var courseList = coursesResponseData.Where(i => i.CrsSubject.Equals("HIST", StringComparison.OrdinalIgnoreCase)).ToList();
            System.Collections.ObjectModel.Collection<Courses> courses = new Collection<Courses>(courseList);

            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "WITH CRS.SUBJECT EQ 'HIST'")).Returns(Task.FromResult(ids));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", ids, true)).Returns(Task.FromResult(courses));

            var actuals = await courseRepo.GetAsync("HIST", empty, empty, empty, empty, empty, empty, empty, empty, empty);

            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task CourseRepository_CreateCourseAsync()
        {
            string empty = string.Empty;
            string[] ids = coursesResponseData.Where(i => i.CrsSubject.Equals("HIST")).Select(c => c.Recordkey).ToArray();
            var courseList = coursesResponseData.Where(i => i.CrsSubject.Equals("HIST", StringComparison.OrdinalIgnoreCase)).ToList();
            System.Collections.ObjectModel.Collection<Courses> courses = new Collection<Courses>(courseList);

            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "WITH CRS.SUBJECT EQ 'HIST'")).Returns(Task.FromResult(ids));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", ids, true)).Returns(Task.FromResult(courses));
            transInvokerMock.Setup(i => i.ExecuteAsync<UpdateCoursesRequest, UpdateCoursesResponse>(It.IsAny<UpdateCoursesRequest>())).ReturnsAsync(updateResponse);

            var actuals = await courseRepo.CreateCourseAsync(allCourses.FirstOrDefault(i => i.SubjectCode.Equals("HIST", StringComparison.OrdinalIgnoreCase)));

            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task CourseRepository_GetAsync_WithAllCriteria()
        {
            string empty = string.Empty;
            string[] ids = coursesResponseData.Where(i => i.CrsSubject.Equals("HIST")).Select(c => c.Recordkey).ToArray();
            var courseList = coursesResponseData.Where(i => i.CrsSubject.Equals("HIST", StringComparison.OrdinalIgnoreCase)).ToList();
            System.Collections.ObjectModel.Collection<Courses> courses = new Collection<Courses>(courseList);

            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "WITH CRS.SUBJECT EQ 'HIST'")).Returns(Task.FromResult(ids));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", ids, true)).Returns(Task.FromResult(courses));

            var actuals = await courseRepo.GetAsync("HIST", "1", "1", "1", "1", "1", "1", "1", "1", "1");

            Assert.IsNotNull(actuals);
        }

        //[TestMethod]
        //public async Task Get_Coreqs()
        //{
        //    dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", It.IsAny<string[]>(), true)).Returns(new Collection<AcadReqmts>());
        //    string courseId = "7702";
        //    Course course = await courseRepo.GetAsync(courseId);
        //    Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
        //    Assert.AreEqual(checkCourse.Corequisites.Count(), course.Corequisites.Count());
        //    Assert.AreEqual(checkCourse.Corequisites.ElementAt(0).Id, course.Corequisites.ElementAt(0).Id);
        //    Assert.AreEqual(checkCourse.Corequisites.ElementAt(1).Required, course.Corequisites.ElementAt(1).Required);
        //}

        [TestMethod]
        public async Task CourseRepository_Get_All_ReturnsAllCourses()
        {

            var courses = await courseRepo.GetAsync();
            Assert.IsTrue(courses.Count() > 40);
        }

        [TestMethod]
        public async Task CourseRepository_Get_Single_ReturnsSingleCourse()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.Title, course.Title);
        }

        [TestMethod]
        public async Task CourseRepository_Get_Id()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.Id, course.Id);
        }

        [TestMethod]
        public async Task CourseRepository_Get_Title()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.Title, course.Title);
        }

        [TestMethod]
        public async Task CourseRepository_Get_Description()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.Description, course.Description);
        }

        [TestMethod]
        public async Task CourseRepository_Get_DepartmentCodes()
        {
            string courseId = "139";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.DepartmentCodes[0], course.DepartmentCodes[0]);
            Assert.AreEqual(checkCourse.DepartmentCodes[1], course.DepartmentCodes[1]);
        }

        [TestMethod]
        public async Task CourseRepository_Get_SubjectCode()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.SubjectCode, course.SubjectCode);
        }

        [TestMethod]
        public async Task CourseRepository_Get_Number()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.Number, course.Number);
        }

        [TestMethod]
        public async Task CourseRepository_Get_AcademicLevelCode()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.AcademicLevelCode, course.AcademicLevelCode);
        }

        [TestMethod]
        public async Task CourseRepository_Get_CourseLevelCodes()
        {
            string courseId = "87";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.CourseLevelCodes.ElementAt(0), course.CourseLevelCodes.ElementAt(0));
            Assert.AreEqual(checkCourse.CourseLevelCodes.ElementAt(1), course.CourseLevelCodes.ElementAt(1));
        }

        [TestMethod]
        public async Task CourseRepository_Get_CEUs()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.Ceus, course.Ceus);
        }

        [TestMethod]
        public async Task CourseRepository_Get_MinCredits()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.MinimumCredits, course.MinimumCredits);
        }

        [TestMethod]
        public async Task CourseRepository_Get_MaxCredits()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.MaximumCredits, course.MaximumCredits);
        }

        [TestMethod]
        public async Task CourseRepository_Get_VariableCreditIncrement()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Assert.AreEqual(1.0m, course.VariableCreditIncrement);
        }

        [TestMethod]
        public async Task CourseRepository_Get_LocationCodes()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.LocationCodes.ElementAt(0), course.LocationCodes.ElementAt(0));
            Assert.AreEqual(checkCourse.LocationCodes.ElementAt(1), course.LocationCodes.ElementAt(1));
        }

        [TestMethod]
        public async Task CourseRepository_Get_Status()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.Status, course.Status);
        }

        [TestMethod]
        public async Task CourseRepository_Get_StartDate()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.StartDate, course.StartDate);
        }

        [TestMethod]
        public async Task CourseRepository_Get_EndDate()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.EndDate, course.EndDate);
        }

        [TestMethod]
        public async Task CourseRepository_Get_Requisites()
        {
            // Note: When data is in the "old" format - all prereqs are assumed to be required.
            string courseId = "186";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.Requisites.Count(), course.Requisites.Count());
            Requisite checkReq = checkCourse.Requisites.ElementAt(0);
            Requisite req = course.Requisites.ElementAt(0);
            Assert.AreEqual(checkReq.RequirementCode, req.RequirementCode);
            Assert.IsTrue(checkReq.IsRequired);
            Assert.AreEqual(checkReq.CompletionOrder, req.CompletionOrder);
            Assert.AreEqual(false, req.IsProtected);
        }

        [TestMethod]
        public async Task CourseRepository_Get_Types()
        {
            // In BuildValidCourseRepository, empty value also added to CrsCourseTypes to make sure repository logic handles that case
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.Types.Count(), course.Types.Count());
            for (int i = 0; i < checkCourse.Types.Count(); i++)
            {
                Assert.IsTrue(course.Types.Contains(checkCourse.Types.ElementAt(i)));
            }
        }

        [TestMethod]
        public async Task CourseRepository_Get_LocalCreditType()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.IsTrue(!string.IsNullOrEmpty(course.LocalCreditType));
            Assert.AreEqual(checkCourse.LocalCreditType, course.LocalCreditType);
        }


        [TestMethod]
        public async Task CourseRepository_Get_CourseApprovalWithNoStatus()
        {
            string courseId = "186";
            Course course = await courseRepo.GetAsync(courseId);
            Assert.IsNotNull(course);
            // a course approval with an invalid status will only log the error, but will still return
            Assert.AreEqual(0, course.CourseApprovals.Count());
        }



        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseRepository_Get_Single_CourseNotFoundThrowsException()
        {
            Course course = await courseRepo.GetAsync("3333");
        }

        [TestMethod]
        public async Task CourseRepository_Get_Many_ReturnsRequestedCourses()
        {
            // Set up repo response for multiple course request, all found
            var courseIds = new Collection<string>() { "46", "87" };
            var courses = await courseRepo.GetAsync(courseIds);
            Assert.AreEqual(courseIds.Count(), courses.Count());
            Assert.AreEqual("American History From WWI", courses.Where(c => c.Id == "87").First().Title);
            Assert.AreEqual("Intermediate Mathematics", courses.Where(c => c.Id == "46").First().Title);
        }

        [TestMethod]
        public async Task CourseRepository_Get_Many_ReturnsPartialListIfCourseNotFound()
        {
            var courseIds = new Collection<string>() { "46", "3333" };
            var courses = await courseRepo.GetAsync(courseIds);
            Assert.IsTrue(courses is List<Course>);
            Assert.IsTrue(courses.Count() == 1);
        }

        [TestMethod]
        public async Task CourseRepository_Get_Many_ReturnsEmptyListIfNoCoursesFound()
        {
            var courseIds = new Collection<string>() { "2222", "3333" };
            var courses = await courseRepo.GetAsync(courseIds);
            Assert.IsTrue(courses is List<Course>);
            Assert.IsTrue(courses.Count() == 0);
        }

        [TestMethod]
        public async Task CourseRepository_Get_Many_ReturnsEmptyListIfNoIdsSpecified()
        {
            var courseIds = new Collection<string>();
            var courses = await courseRepo.GetAsync(courseIds);
            Assert.AreEqual(0, courses.Count());
        }


        [TestMethod]
        public async Task CourseRepository_GetPseudoCourseFalse()
        {
            var course = await courseRepo.GetAsync("200");
            Assert.IsFalse(course.IsPseudoCourse);
        }

        [TestMethod]
        public async Task CourseRepository_GetPseudoCourseTrue()
        {
            var courses = await courseRepo.GetAsync(new List<string>() { "201", "202" });
            Assert.IsTrue(courses.ElementAt(0).IsPseudoCourse);
            Assert.IsTrue(courses.ElementAt(1).IsPseudoCourse);
        }

        [TestMethod]
        public async Task CourseRepository_GetAsync_WithLocationCycleRestrictions()
        {
            string courseId = "46";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(checkCourse.LocationCycleRestrictions.Count(), course.LocationCycleRestrictions.Count());
            foreach (var rest in checkCourse.LocationCycleRestrictions)
            {
                var courseRestriction = course.LocationCycleRestrictions.Where(lc => lc.Location == rest.Location).FirstOrDefault();
                Assert.IsNotNull(courseRestriction);
                Assert.AreEqual(rest.SessionCycle, courseRestriction.SessionCycle);
                Assert.AreEqual(rest.YearlyCycle, courseRestriction.YearlyCycle);
            }

        }

        [TestMethod]
        public async Task CourseRepository_Get_WithoutLocationCycleRestrictions()
        {
            string courseId = "139";
            Course course = await courseRepo.GetAsync(courseId);
            Course checkCourse = allCourses.Where(c => c.Id == courseId).First();
            Assert.AreEqual(0, course.LocationCycleRestrictions.Count());
        }

        [TestMethod]
        public async Task CourseRepository_Get_All_WritesToCache()
        {
            // Set up local cache mock to respond to cache request:
            //  -to "Contains" request, return "false" to indicate item is not in cache
            //  -to cache "Get" request, return null so we know it's getting data from "repository"
            string cacheKey = courseRepo.BuildFullCacheKey("AllCourses");
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(false);
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(null);
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
                ));
            // return response to data accessor request.
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));

            // But after data accessor read, set up mocking so we can verify the list of courses was written to the cache
            cacheProviderMock.Setup(x => x.Add(cacheKey, It.IsAny<Dictionary<string, Course>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();
            // Verify that course was returned
            var courses = await courseRepo.GetAsync();
            Assert.IsTrue(courses.Count() >= 40);

            // Verify that the course2 item was added to the cache after it was read from the repository
            cacheProviderMock.Verify(m => m.Add(cacheKey, It.IsAny<Dictionary<string, Course>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
        }

        [TestMethod]
        public async Task CourseRepository_Get_All_GetsCachedCourses()
        {
            // Set up local cache mock to respond to cache request:
            //  -to "Contains" request, return "true" to indicate item is in cache
            //  -to "Get" request, return the cache item (in this case the "AllCourses" cache item)
            string cacheKey = courseRepo.BuildFullCacheKey("AllCourses");
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(allCoursesDict).Verifiable();
            // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(new string[] { }));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(new Collection<Courses>()));
            // Assert that proper course was returned
            var courses = await courseRepo.GetAsync();
            Assert.IsTrue(courses.Count() >= 40);
            // Verify that Get was called to get the courses from cache
            cacheProviderMock.Verify(m => m.Get(cacheKey, null));
        }


        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task CourseRepository_InvalidRepository_ThrowsException()
        {
            var courseRepo = BuildInvalidCourseRepository();
            var courses = await courseRepo.GetAsync();
        }

        [TestMethod]
        public async Task CourseRepository_Get_SessionCycleDefaultsToNull()
        {
            string courseId = "46";
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));
            var result = await courseRepo.GetAsync(courseId);
            Assert.AreEqual(null, result.TermsOffered);
        }

        [TestMethod]
        public async Task CourseRepository_Get_SessionCycleGetsDescription()
        {
            string courseId = "87";
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));
            dataAccessorMock.Setup<Task<Collection<SessionCycles>>>(acc => acc.BulkReadRecordAsync<SessionCycles>("SESSION.CYCLES", "", true)).Returns(Task.FromResult(BuildSessionCyclesResponse()));
            var result = await courseRepo.GetAsync(courseId);
            Assert.AreEqual("Fall Term Only", result.TermsOffered);
        }

        [TestMethod]
        public async Task CourseRepository_Get_SessionCyclesNoErrorIfNoneInRepository()
        {
            string courseId = "87";
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));
            dataAccessorMock.Setup<Task<Collection<SessionCycles>>>(acc => acc.BulkReadRecordAsync<SessionCycles>("SESSION.CYCLES", "", true)).Returns(Task.FromResult(BuildNullSessionCyclesResponse()));
            var result = await courseRepo.GetAsync(courseId);
            Assert.AreEqual(null, result.TermsOffered);
        }

        [TestMethod]
        public async Task CourseRepository_Get_YearlyCycleGetsDescription()
        {
            string courseId = "87";
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));
            dataAccessorMock.Setup<Task<Collection<YearlyCycles>>>(acc => acc.BulkReadRecordAsync<YearlyCycles>("YEARLY.CYCLES", "", true)).Returns(Task.FromResult(BuildYearlyCyclesResponse()));
            var result = await courseRepo.GetAsync(courseId);
            Assert.AreEqual("Every Other Year", result.YearsOffered);
        }

        [TestMethod]
        public async Task CourseRepository_Get_YearlyCyclesNoErrorIfNoneInRepository()
        {
            string courseId = "87";
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));
            dataAccessorMock.Setup<Task<Collection<YearlyCycles>>>(acc => acc.BulkReadRecordAsync<YearlyCycles>("YEARLY.CYCLES", "", true)).Returns(Task.FromResult(BuildNullYearlyCyclesResponse()));
            var result = await courseRepo.GetAsync(courseId);
            Assert.AreEqual(null, result.YearsOffered);
        }

        [TestMethod]
        public async Task CourseRepository_Get_TermSessionCycleDefaultsToNull()
        {
            string courseId = "46";
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));
            var result = await courseRepo.GetAsync(courseId);
            Assert.AreEqual(0, result.TermSessionCycle.Count());
        }

        [TestMethod]
        public async Task CourseRepository_Get_TermSessionCycleGetsCode()
        {
            string courseId = "87";
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));
            var result = await courseRepo.GetAsync(courseId);
            Assert.AreEqual("F", result.TermSessionCycle);
        }

        [TestMethod]
        public async Task CourseRepository_Get_TermYearlyCycleDefaultsToEmpty()
        {
            string courseId = "46";
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));
            var result = await courseRepo.GetAsync(courseId);
            Assert.AreEqual(0, result.TermYearlyCycle.Count());
        }

        [TestMethod]
        public async Task CourseRepository_Get_TermYearlyCycleGetsCode()
        {
            string courseId = "87";
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));
            var result = await courseRepo.GetAsync(courseId);
            Assert.AreEqual("A", result.TermYearlyCycle);
        }

        // The following tests verify that all courses returned with "Get" regardless
        // of status or dates. (The search service is responsible for filtering out
        // inactive/obsolete courses. The "Get" needs to get them all because it is
        // also used to retrieve academic credit information.)
        [TestMethod]
        public async Task CourseRepository_Get_CurrentStatusWithValidStartNullEndDate_Included()
        {
            string courseId = "7703";
            // Test course with manipulated data
            var course1 = allCourses.Where(c => c.Id == courseId).First();
            // Additional course
            var course2 = allCourses.Where(c => c.Id == "46").First();
            var coursesResponse = BuildCoursesResponse(new List<Course>() { course1, course2 });
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponse.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponse));
            var result = await courseRepo.GetAsync();
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(courseId, result.ElementAt(0).Id);
            Assert.AreEqual("46", result.ElementAt(1).Id);
        }

        [TestMethod]
        public async Task CourseRepository_Get_CurrentStatusWithValidStartAndEndDate_Included()
        {
            string courseId = "7704";
            // Test course with manipulated data
            var course1 = allCourses.Where(c => c.Id == courseId).First();
            // Additional course
            var course2 = allCourses.Where(c => c.Id == "46").First();
            var coursesResponse = BuildCoursesResponse(new List<Course>() { course1, course2 });
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponse.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponse));
            var result = await courseRepo.GetAsync();
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(courseId, result.ElementAt(0).Id);
            Assert.AreEqual("46", result.ElementAt(1).Id);
        }

        [TestMethod]
        public async Task CourseRepository_Get_NotCurrentStatusWithValidStartNullEndDate_Included()
        {
            string courseId = "7705";
            // Test course with manipulated data
            var course1 = allCourses.Where(c => c.Id == courseId).First();
            // Additional course
            var course2 = allCourses.Where(c => c.Id == "46").First();
            var coursesResponse = BuildCoursesResponse(new List<Course>() { course1, course2 });
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponse.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponse));
            var result = await courseRepo.GetAsync();
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("46", result.ElementAt(1).Id);
        }

        [TestMethod]
        public async Task CourseRepository_Get_NotCurrentStatusWithValidStartAndEndDate_Included()
        {
            string courseId = "7706";
            // Test course with manipulated data
            var course1 = allCourses.Where(c => c.Id == courseId).First();
            // Additional course
            var course2 = allCourses.Where(c => c.Id == "46").First();
            var coursesResponse = BuildCoursesResponse(new List<Course>() { course1, course2 });
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponse.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponse));
            var result = await courseRepo.GetAsync();
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("46", result.ElementAt(1).Id);
        }

        // Note on Equates tests: See BuildCoursesResponse and BuildCourseEquateCodesResponse methods for setup
        // of the equates used in these tests. 

        [TestMethod]
        public async Task CourseRepository_EquatedCourseIds_DeterminesDirectEquates()
        {
            // These two courses are set up below as direct equates: DANC-100 and DANC-200
            string course1Id = "122";
            var course1 = allCourses.Where(c => c.Id == course1Id).First();
            string course2Id = "28";
            var course2 = allCourses.Where(c => c.Id == course2Id).First();
            var coursesResponse = BuildCoursesResponse(new List<Course>() { course1, course2 });
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponse.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponse));
            // Get courses from repo
            var result1 = await courseRepo.GetAsync(course1Id);
            var result2 = await courseRepo.GetAsync(course2Id);
            // Assert the two courses are found to be equates of each other
            Assert.AreEqual(1, result1.EquatedCourseIds.Count());
            Assert.AreEqual("28", result1.EquatedCourseIds.ElementAt(0));
            Assert.AreEqual(1, result2.EquatedCourseIds.Count());
            Assert.AreEqual("122", result2.EquatedCourseIds.ElementAt(0));
        }

        [TestMethod]
        public async Task CourseRepository_EquatedCourseIds_DeterminesMultipleHierarchicalEquates()
        {
            // One course (MATH 350) has two course equates, but the courses it is equated to do not have same equate codes so they are not equates
            // Arrange--Arrange response from the repository
            var allCourses = await new TestCourseRepository().GetAsync();
            var coursesResponse = BuildCoursesResponse(allCourses);
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponse.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponse));
            // Act--Get courses from repo
            var result1 = await courseRepo.GetAsync("186"); // Math 350 has no equates
            var result2 = await courseRepo.GetAsync("306");
            var result3 = await courseRepo.GetAsync("213");
            // Assert-- Math 350 has no equates
            Assert.AreEqual(0, result1.EquatedCourseIds.Count());
            // Math 103 and math 103 have no equates
            Assert.AreEqual(0, result2.EquatedCourseIds.Count());
            Assert.AreEqual(0, result3.EquatedCourseIds.Count());
        }

        [TestMethod]
        public async Task CourseRepository_EquatedCourseIds_DeterminesSingleHierarchicalEquate()
        {
            // MATH 491 is an equate for one other course (MATH 371), but MATH 371 has two equates so they are not equates
            // Arrange--Set up response from the repository
            var allCourses = await new TestCourseRepository().GetAsync();
            var coursesResponse = BuildCoursesResponse(allCourses);
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponse.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponse));
            // Act--Get courses from repo
            var result0 = await courseRepo.GetAsync("226"); // MATH 491 has no equates
            var result4 = await courseRepo.GetAsync("353"); // MATH 371 has no equates
            // Assert-- Math 491 has no equates
            Assert.AreEqual(0, result0.EquatedCourseIds.Count());
            // Assert-- Math 491 is an equate of Math 371
            Assert.AreEqual(0, result4.EquatedCourseIds.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CourseRepository_GetCourseByGuid_NullId()
        {
            Course course = await courseRepo.GetCourseByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CourseRepository_GetCourseByGuid_EmptyId()
        {
            Course course = await courseRepo.GetCourseByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CourseRepository_GetCourseByGuid_InvalidId()
        {
            Course course = await courseRepo.GetCourseByGuidAsync("3333");
        }

        [TestMethod]
        public async Task CourseRepository_GetCourseByGuid_Valid()
        {
            string courseId = "46";
            string guid = coursesResponseData.FirstOrDefault(c => c.Recordkey == courseId).RecordGuid;
            Course course = await courseRepo.GetCourseByGuidAsync(guid);
            Assert.AreEqual(courseId, course.Id);
            Assert.AreEqual(guid, course.Guid);
        }

        [TestMethod]
        public async Task CourseRepository_GetCoursesByIdAsync_NullListOfIds()
        {
            var courses = await courseRepo.GetCoursesByIdAsync(null);
            Assert.AreEqual(0, courses.Count());
        }

        [TestMethod]
        public async Task CourseRepository_GetCoursesByIdAsync_EmptyListOfIds()
        {
            var courses = await courseRepo.GetCoursesByIdAsync(new List<string>());
            Assert.AreEqual(0, courses.Count());
        }

        [TestMethod]
        public async Task CourseRepository_GetCoursesByIdAsync_IdsNotInCourses()
        {
            Collection<Courses> emptyResponse = new Collection<Courses>();
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(emptyResponse));
            var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "Junk1", "Junk2" });
            Assert.AreEqual(0, courses.Count());
        }

        [TestMethod]
        public async Task CourseRepository_GetCoursesByIdAsync_Success()
        {
            var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "139", "42" });
            Assert.AreEqual(2, courses.Count());
        }

        #region CourseContstructorExceptionTests
        // Removed constructor throw for now, was breaking self service.  Will
        // revisit with 1.25 error standards.
        // Test to prove the constructor throw is gone:
        [TestMethod]
        //public async Task CourseRepository_GetCoursesWithBadCourseAsync_Success()
        //{
        //    var courses = await courseRepo.GetAsync();
        //    var course99 = courses.FirstOrDefault(c => c.Id == "7439");
        //    var course100 = courses.FirstOrDefault(c => c.Id == "7440");
        //    Assert.IsNotNull(course99);
        //    Assert.IsNull(course100);
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Bad_date()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsStartDate = DateTime.Now.AddDays(1);
        //    expected.CrsEndDate = DateTime.Now.AddDays(-1);
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Bad_ShortTitle()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsShortTitle = "";
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Bad_Departments()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CourseDeptsEntityAssociation = null;
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Bad_Subject()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsSubject = null;
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Bad_Number()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsNo = "";
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Bad_AcadLevelCode()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsAcadLevel = "";
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Bad_LevelCodes()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsLevels = null;
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Missing_Min_Cred_And_CEU()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsMinCred = null;
        //    expected.CrsCeus = null;
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Bad_MinCred()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsMinCred = -1;
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Bad_CEU()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsCeus = -1;
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Bad_Max_credits()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsMaxCred = 1;
        //    expected.CrsMinCred = 2;
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}
        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task CourseRepository_Get_Null_subject()
        //{
        //    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        //    expected.CrsSubject = "";
        //    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        //    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        //}

        //// The way the controller is coded - there is no way the approval assn will be null when it gets
        //// to the constructor.  
        ////[TestMethod]
        ////[ExpectedException(typeof(RepositoryException))]
        ////public async Task CourseRepository_Get_Bad_Approvals()
        ////{
        ////    var expected = coursesResponseData.First(crd => crd.Recordkey == "46");
        ////    expected.ApprovalStatusEntityAssociation = null;
        ////    dataAccessorMock.Setup<Task<Courses>>(acc => acc.ReadRecordAsync<Courses>("COURSES", It.IsAny<string>(), true)).Returns(Task.FromResult(expected));
        ////    var courses = await courseRepo.GetCoursesByIdAsync(new List<string>() { "46" });
        ////}
        #endregion

        private CourseRepository BuildValidCourseRepository()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            // Cache Mock
            //localCacheMock = new Mock<ObjectCache>();
            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
                ));
            // Set up data accessor for mocking 
            dataAccessorMock = new Mock<IColleagueDataReader>();
            apiSettingsMock = new ApiSettings("null");

            // CourseStatuses mock
            ApplValcodes courseStatusesResponse = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "1" },
                                                                      new ApplValcodesVals() { ValInternalCodeAssocMember = "T", ValActionCode1AssocMember = "2"},
                                                                      new ApplValcodesVals() { ValInternalCodeAssocMember = "P", ValActionCode1AssocMember = ""}}
            };
            dataAccessorMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.STATUSES", true)).Returns(Task.FromResult(courseStatusesResponse));

            // course types
            dataAccessorMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", true)).Returns(Task.FromResult(BuildCourseTypesValcode()));

            // credit types
            var creditTypesResponse = BuildCreditTypes();
            dataAccessorMock.Setup<Task<Collection<CredTypes>>>(ct => ct.BulkReadRecordAsync<CredTypes>("CRED.TYPES", "", true)).Returns(Task.FromResult(creditTypesResponse));

            // course parameters - unconverted "NULL" response
            var courseParameters = BuildCourseParametersUnConvertedResponse();
            dataAccessorMock.Setup<Task<CdDefaults>>(acc => acc.ReadRecordAsync<CdDefaults>("ST.PARMS", "CD.DEFAULTS", true)).Returns(Task.FromResult(courseParameters));

            // Set up repo response for "all" courses requests
            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));

            // Set up repo response for related requisite requirements
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(new Collection<AcadReqmts>()));

            // Set up course equates response
            var courseEquatesResponse = BuildCourseEquateCodesResponse();
            dataAccessorMock.Setup<Task<Collection<CourseEquateCodes>>>(acc => acc.BulkReadRecordAsync<CourseEquateCodes>("COURSE.EQUATE.CODES", "", true)).Returns(Task.FromResult(courseEquatesResponse));

            // Setup localCacheMock as the object for the CacheProvider
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            //setup empty dataaccess mocks for session cycles and yearly cycles
            dataAccessorMock.Setup<Task<Collection<SessionCycles>>>(acc => acc.BulkReadRecordAsync<SessionCycles>("SESSION.CYCLES", "", true)).Returns(Task.FromResult<Collection<SessionCycles>>(null));
            dataAccessorMock.Setup<Task<Collection<YearlyCycles>>>(acc => acc.BulkReadRecordAsync<YearlyCycles>("YEARLY.CYCLES", "", true)).Returns(Task.FromResult<Collection<YearlyCycles>>(null));

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transInvokerMock.Object);

            // Construct course repository
            courseRepo = new CourseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);



            return courseRepo;
        }

        private CourseRepository BuildInvalidCourseRepository()
        {
            // var transFactoryMock = new Mock<IColleagueTransactionFactory>();
            apiSettingsMock = new ApiSettings("null");

            // Set up data accessor for mocking 
            //var dataAccessorMock = new Mock<IColleagueDataReader>();
            //transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // course parameters - unconverted response
            var courseParameters = BuildCourseParametersNullResponse();
            dataAccessorMock.Setup<Task<CdDefaults>>(acc => acc.ReadRecordAsync<CdDefaults>("ST.PARMS", "CD.DEFAULTS", true)).Returns(Task.FromResult(courseParameters));

            // Set up repo response for "all" courses requests
            Exception expectedFailure = new Exception("fail");
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Throws(expectedFailure);
            //  dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Throws<Exception>();
            // Cache Mock
            var localCacheMock = new Mock<ObjectCache>();
            // Cache Provider Mock
            var cacheProviderMock = new Mock<ICacheProvider>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
           x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
           .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
               null,
               new SemaphoreSlim(1, 1)
               ));

            // Construct course repository
            courseRepo = new CourseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

            return courseRepo;
        }


        private Collection<Courses> BuildCoursesResponse(IEnumerable<Course> courses)
        {
            var courseStatuses = BuildCourseStatusesResponse();
            Collection<Courses> repoCourses = new Collection<Courses>();
            foreach (var course in courses)
            {
                var repoCrs = new Courses();
                repoCrs.Recordkey = course.Id.ToString();
                repoCrs.RecordGuid = course.Guid;
                repoCrs.CrsAcadLevel = course.AcademicLevelCode;
                repoCrs.CrsCeus = course.Ceus;
                repoCrs.CrsDepts = course.DepartmentCodes.ToList();
                repoCrs.CrsLevels = course.CourseLevelCodes.ToList();
                repoCrs.CrsMinCred = course.MinimumCredits;
                repoCrs.CrsNo = course.Number;
                repoCrs.CrsSubject = course.SubjectCode;
                repoCrs.CrsShortTitle = course.Title;
                repoCrs.CrsDesc = course.Description;
                repoCrs.CrsMaxCred = course.MaximumCredits;
                repoCrs.CrsVarCredIncrement = course.VariableCreditIncrement;
                repoCrs.CrsLocations = course.LocationCodes.ToList();
                repoCrs.CrsStatus = new List<string>() { (courseStatuses[course.Status]) };
                repoCrs.CrsStartDate = course.StartDate;
                repoCrs.CrsEndDate = course.EndDate;
                var prereq = course.Requisites.Where(r => r.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.Previous).FirstOrDefault();
                if (prereq != null)
                {
                    repoCrs.CrsPrereqs = prereq.RequirementCode;
                }
                repoCrs.CrsReqs = new List<string>();

                repoCrs.CourseDeptsEntityAssociation = new List<CoursesCourseDepts>();
                if (course.Departments != null && course.Departments.Count > 0)
                {
                    foreach (var od in course.Departments)
                    {
                        repoCrs.CourseDeptsEntityAssociation.Add(new CoursesCourseDepts(od.AcademicDepartmentCode, od.ResponsibilityPercentage));
                    }
                }

                repoCrs.ApprovalStatusEntityAssociation = new List<CoursesApprovalStatus>();
                if (course.CourseApprovals != null && course.CourseApprovals.Count > 0)
                {
                    foreach (var ca in course.CourseApprovals)
                    {
                        switch (course.Id)
                        {
                            case "186":
                                repoCrs.ApprovalStatusEntityAssociation.Add(new CoursesApprovalStatus() { CrsApprovalAgencyIdsAssocMember = ca.ApprovingPersonId });
                                break;
                            default:
                                repoCrs.ApprovalStatusEntityAssociation.Add(new CoursesApprovalStatus(ca.StatusCode, ca.ApprovingPersonId, ca.ApprovingAgencyId, ca.Date, ca.StatusDate));
                                break;
                        }
                    }
                }

                // Set up repository course equate codes.
                // See BuildCourseEquateCodesResponse method for the course equate codes records
                // See CourseRepository FindEquates for an explanation of how course equates are unraveled.
                switch (course.Id)
                {
                    case "186":
                        // MATH 350
                        repoCrs.CrsEquateCodes = new List<string>() { "1", "2" };
                        break;
                    case "306":
                        // MATH 102
                        repoCrs.CrsEquateCodes = new List<string>() { "1" };
                        break;
                    case "213":
                        // MATH 103
                        repoCrs.CrsEquateCodes = new List<string>() { "2" };
                        break;
                    case "353":
                        // MATH 371
                        repoCrs.CrsEquateCodes = new List<string>() { "3" };
                        break;
                    case "226":
                        // MATH 491
                        repoCrs.CrsEquateCodes = new List<string>() { "3", "4" };
                        break;
                    case "122":
                        // DANC 100
                        repoCrs.CrsEquateCodes = new List<string>() { "5" };
                        break;
                    case "28":
                        // DANC 200
                        repoCrs.CrsEquateCodes = new List<string>() { "5" };
                        break;
                    default:
                        repoCrs.CrsEquateCodes = new List<string>();
                        break;
                }

                // Add special data to data accessor record to test session and yearly cycles
                switch (course.Id)
                {
                    case "87":
                        repoCrs.CrsSessionCycle = "F";
                        repoCrs.CrsYearlyCycle = "A";
                        break;
                    default:
                        repoCrs.CrsSessionCycle = "";
                        repoCrs.CrsYearlyCycle = "";
                        break;
                }

                // Add coreq association information
                if (course.Requisites != null)
                {
                    var coreqs = course.Requisites.Where(r => r.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.Concurrent);

                    repoCrs.CourseCoreqsEntityAssociation = new List<CoursesCourseCoreqs>();
                    foreach (var coreq in coreqs)
                    {
                        repoCrs.CourseCoreqsEntityAssociation.Add(new CoursesCourseCoreqs(coreq.CorequisiteCourseId, (coreq.IsRequired == true ? "Y" : "")));
                    }
                }

                // course types
                repoCrs.CrsCourseTypes = course.Types.ToList();
                if (course.Id == "200")
                {
                    repoCrs.CrsCourseTypes = new List<string>() { "CORE" };
                }
                else if (course.Id == "201")
                {
                    repoCrs.CrsCourseTypes = new List<string>() { "PSE" };
                }
                else if (course.Id == "202")
                {
                    repoCrs.CrsCourseTypes = new List<string>() { "CORE", "PSE", "REM" };
                }

                // local credit type
                repoCrs.CrsCredType = course.LocalCreditType;

                // Location Cycle restrictions - Course 46 has this in the TestCourseRepository
                repoCrs.CourseLocationCyclesEntityAssociation = new List<CoursesCourseLocationCycles>();
                foreach (var clc in course.LocationCycleRestrictions)
                {
                    repoCrs.CourseLocationCyclesEntityAssociation.Add(new CoursesCourseLocationCycles(clc.Location, clc.SessionCycle, clc.YearlyCycle));
                }

                repoCourses.Add(repoCrs);
            }
            return repoCourses;
        }

        private Collection<SessionCycles> BuildSessionCyclesResponse()
        {
            var sessionCycles = new Collection<SessionCycles>();

            var sc1 = new SessionCycles();
            sc1.Recordkey = "F";
            sc1.ScDesc = "Fall Term Only";
            sessionCycles.Add(sc1);

            var sc2 = new SessionCycles();
            sc2.Recordkey = "EI";
            sc2.ScDesc = "Even Year Intercession";
            sessionCycles.Add(sc2);

            return sessionCycles;

        }

        private Collection<SessionCycles> BuildNullSessionCyclesResponse()
        {
            Collection<SessionCycles> sessionCycles = new Collection<SessionCycles>();
            sessionCycles = null;
            return sessionCycles;
        }

        private Collection<YearlyCycles> BuildYearlyCyclesResponse()
        {
            var yearlyCycles = new Collection<YearlyCycles>();

            var yc1 = new YearlyCycles();
            yc1.Recordkey = "A";
            yc1.YcDesc = "Every Other Year";
            yearlyCycles.Add(yc1);

            var yc2 = new YearlyCycles();
            yc2.Recordkey = "B";
            yc2.YcDesc = "Every Third Year";
            yearlyCycles.Add(yc2);

            return yearlyCycles;

        }

        private Collection<YearlyCycles> BuildNullYearlyCyclesResponse()
        {
            Collection<YearlyCycles> yearlyCycles = new Collection<YearlyCycles>();
            yearlyCycles = null;
            return yearlyCycles;
        }

        // This is just temporary logic to handle a proposed course statuses valcode
        // to convert between course status and the valcode internal code.
        private Dictionary<CourseStatus, string> BuildCourseStatusesResponse()
        {
            var courseStatuses = new Dictionary<CourseStatus, string>();
            courseStatuses.Add(CourseStatus.Active, "A");
            courseStatuses.Add(CourseStatus.Terminated, "X");
            courseStatuses.Add(CourseStatus.Unknown, "P");
            return courseStatuses;
        }

        private Collection<CourseEquateCodes> BuildCourseEquateCodesResponse()
        {
            // See the course repository method FindEquates for an explanation of this setup
            var courseEquateCodes = new Collection<CourseEquateCodes>();
            courseEquateCodes.Add(new CourseEquateCodes() { Recordkey = "1", CecCourses = new List<string>() { "186", "306" } }); //Math 350 can be taken instead of Math 102 (but not vice-versa)
            courseEquateCodes.Add(new CourseEquateCodes() { Recordkey = "2", CecCourses = new List<string>() { "186", "213" } }); //Math 350 can be taken instead of Math 103 (but not vice-versa)
            courseEquateCodes.Add(new CourseEquateCodes() { Recordkey = "3", CecCourses = new List<string>() { "226", "353" } }); //Math 491 can be taken instead of Math 371 (but not vice-versa, see below)
            courseEquateCodes.Add(new CourseEquateCodes() { Recordkey = "4", CecCourses = new List<string>() { "226" } }); // Equate code created so that Math 371 cannot be an equate of Math 491
            courseEquateCodes.Add(new CourseEquateCodes() { Recordkey = "5", CecCourses = new List<string>() { "122", "28" } }); //DANC 100 equates directly to DANC 200 and vice-versa
            return courseEquateCodes;
        }

        private ApplValcodes BuildCourseTypesValcode()
        {
            ApplValcodes courseTypesValcode = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "CORE", ValActionCode1AssocMember = "" },
                                                                      new ApplValcodesVals() { ValInternalCodeAssocMember = "PSE", ValActionCode1AssocMember = "p"},
                                                                      new ApplValcodesVals() { ValInternalCodeAssocMember = "REM", ValActionCode1AssocMember = ""}}
            };
            return courseTypesValcode;
        }

        private Collection<CredTypes> BuildCreditTypes()
        {
            // See the course repository method FindEquates for an explanation of this setup
            var credTypes = new Collection<CredTypes>();
            credTypes.Add(new CredTypes() { Recordkey = "LG,", CrtpCategory = "O", CrtpDesc = "Test for Ldg W/ , and " });
            credTypes.Add(new CredTypes() { Recordkey = "C", CrtpCategory = "I", CrtpDesc = "Credit-Not Degree Applicable" });
            credTypes.Add(new CredTypes() { Recordkey = "CE", CrtpCategory = "C", CrtpDesc = "Continuing Education" });
            credTypes.Add(new CredTypes() { Recordkey = "D", CrtpCategory = "I", CrtpDesc = "Credit-Degree Applicable" });
            credTypes.Add(new CredTypes() { Recordkey = "HS,", CrtpCategory = "O", CrtpDesc = "High School" });
            credTypes.Add(new CredTypes() { Recordkey = "IN", CrtpCategory = "I", CrtpDesc = "Institutional" });
            credTypes.Add(new CredTypes() { Recordkey = "LE", CrtpCategory = "O", CrtpDesc = "Life Experience" });
            credTypes.Add(new CredTypes() { Recordkey = "N", CrtpCategory = "O", CrtpDesc = "Noncredit (not Community Srvc)" });
            credTypes.Add(new CredTypes() { Recordkey = "NC", CrtpCategory = "O", CrtpDesc = "Noncourse" });
            credTypes.Add(new CredTypes() { Recordkey = "RE", CrtpCategory = "O", CrtpDesc = "Remedial" });
            credTypes.Add(new CredTypes() { Recordkey = "TR", CrtpCategory = "T", CrtpDesc = "Transfer" });
            credTypes.Add(new CredTypes() { Recordkey = "WFD", CrtpCategory = "C", CrtpDesc = "Workforce Development" });
            return credTypes;
        }

        private CdDefaults BuildCourseParametersUnConvertedResponse()
        {
            // Unconverted response
            CdDefaults defaults = new CdDefaults();
            defaults.Recordkey = "CD.DEFAULTS";
            defaults.CdReqsConvertedFlag = "";
            return defaults;
        }

        private CdDefaults BuildCourseParametersNullResponse()
        {
            // null response - could not read CdDefaults
            CdDefaults defaults = null;
            return defaults;
        }

    }

    [TestClass]
    public class ConvertedCourseRepositoryTests
    {
        Mock<IColleagueTransactionFactory> transFactoryMock;
        //Mock<ObjectCache> localCacheMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<ILogger> loggerMock;
        IEnumerable<Course> allCourses;
        IDictionary<string, Course> allCoursesDict;
        Collection<Courses> coursesResponseData;
        Collection<Ellucian.Colleague.Data.Student.DataContracts.AcadReqmts> requisiteRequirementResponseData;
        CourseRepository convertedCourseRepo;
        ApiSettings apiSettingsMock;

        [TestInitialize]
        public async void Initialize()
        {
            loggerMock = new Mock<ILogger>();

            // Build Courses responses used for mocking
            allCourses = await new TestCourseRepository().GetAsync();
            // Build courses dict, response from cache
            allCoursesDict = new Dictionary<string, Course>();
            foreach (var crs in allCourses)
            {
                allCoursesDict[crs.Id] = crs;
            }
            // Repository response data
            coursesResponseData = BuildCoursesResponse(allCourses);

            convertedCourseRepo = BuildValidCourseRepository_Converted();

            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(new string[] { "1", "2", "3" }));

        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataAccessorMock = null;
            cacheProviderMock = null;
            //localCacheMock = null;
            coursesResponseData = null;
            allCourses = null;
            convertedCourseRepo = null;
        }

        [TestMethod]
        public async Task ConvertedCourseRepository_Get_Requisites_Prereq_NewFormat()
        {
            // Set up repo response for related requisite requirements
            requisiteRequirementResponseData = BuildAcadReqmtsResponse();
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(requisiteRequirementResponseData));
            var course = await convertedCourseRepo.GetAsync("87");
            Assert.AreEqual(1, course.Requisites.Count());
            var req = course.Requisites.ElementAt(0);
            Assert.AreEqual("PREREQ2", req.RequirementCode);
            Assert.IsFalse(req.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Previous, req.CompletionOrder);
            Assert.IsTrue(req.IsProtected);
        }

        [TestMethod]
        public async Task ConvertedCourseRepository_Get_Requisites_Coreq1_NewFormat()
        {
            // Set up repo response for related requisite requirements
            requisiteRequirementResponseData = BuildAcadReqmtsResponse();
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", It.IsAny<string[]>(), true)).Returns(Task.FromResult((requisiteRequirementResponseData)));
            var course = await convertedCourseRepo.GetAsync("333");
            Assert.AreEqual(1, course.Requisites.Count());
            var req = course.Requisites.ElementAt(0);
            Assert.AreEqual("COREQ1", req.RequirementCode);
            Assert.IsTrue(req.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Concurrent, req.CompletionOrder);
            Assert.IsFalse(req.IsProtected);
        }


        [TestMethod]
        public async Task ConvertedCourseRepository_Get_Requisites_Coreq2_NewFormat()
        {
            // Set up repo response for related requisite requirements
            requisiteRequirementResponseData = BuildAcadReqmtsResponse();
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(requisiteRequirementResponseData));
            var course = await convertedCourseRepo.GetAsync("21");
            Assert.AreEqual(2, course.Requisites.Count());
            var req1 = course.Requisites.ElementAt(0);
            Assert.AreEqual("COREQ2", req1.RequirementCode);
            Assert.IsFalse(req1.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Concurrent, req1.CompletionOrder);
            var req2 = course.Requisites.ElementAt(1);
            Assert.AreEqual("REQ1", req2.RequirementCode);
            Assert.IsTrue(req2.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, req2.CompletionOrder);
            Assert.IsFalse(req2.IsProtected);
        }

        [TestMethod]
        public async Task ConvertedCourseRepository_Get_Requisites_NewFormat_RequirementNotFound()
        {
            // override the requirement response to be none.
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(new Collection<AcadReqmts>()));

            var course = await convertedCourseRepo.GetAsync("87");
            Assert.AreEqual(0, course.Requisites.Count());
        }

        /// <summary>
        /// The following method builds the same course repository but is assuming the requisites in Colleague
        /// are in the new format (have been converted) and there are requirements that match up.
        /// </summary>
        /// <returns></returns>
        private CourseRepository BuildValidCourseRepository_Converted()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            // Cache Mock
            //localCacheMock = new Mock<ObjectCache>();
            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
                ));


            // Set up data accessor for mocking 
            dataAccessorMock = new Mock<IColleagueDataReader>();
            apiSettingsMock = new ApiSettings("null");

            // CourseStatuses mock
            ApplValcodes courseStatusesResponse = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "1" },
                                                                      new ApplValcodesVals() { ValInternalCodeAssocMember = "T", ValActionCode1AssocMember = "2"},
                                                                      new ApplValcodesVals() { ValInternalCodeAssocMember = "P", ValActionCode1AssocMember = ""}}
            };
            dataAccessorMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.STATUSES", true)).Returns(Task.FromResult(courseStatusesResponse));

            // course types
            dataAccessorMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", true)).Returns(Task.FromResult(BuildCourseTypesValcode()));

            // credit types
            var creditTypesResponse = BuildCreditTypes();
            dataAccessorMock.Setup<Task<Collection<CredTypes>>>(ct => ct.BulkReadRecordAsync<CredTypes>("CRED.TYPES", "", true)).Returns(Task.FromResult(creditTypesResponse));

            // course parameters - converted response
            var convertedCourseParameters = BuildCourseParametersConvertedResponse();
            dataAccessorMock.Setup<Task<CdDefaults>>(acc => acc.ReadRecordAsync<CdDefaults>("ST.PARMS", "CD.DEFAULTS", true)).Returns(Task.FromResult(convertedCourseParameters));

            // Set up repo response for "all" courses requests

            dataAccessorMock.Setup(acc => acc.SelectAsync("COURSES", "")).Returns(Task.FromResult(coursesResponseData.Select(c => c.Recordkey).ToArray()));
            dataAccessorMock.Setup<Task<Collection<Courses>>>(acc => acc.BulkReadRecordAsync<Courses>("COURSES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(coursesResponseData));

            // Set up course equates response
            var courseEquatesResponse = BuildCourseEquateCodesResponse();
            dataAccessorMock.Setup<Task<Collection<CourseEquateCodes>>>(acc => acc.BulkReadRecordAsync<CourseEquateCodes>("COURSE.EQUATE.CODES", "", true)).Returns(Task.FromResult(courseEquatesResponse));

            // Setup localCacheMock as the object for the CacheProvider
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            //setup empty dataaccess mocks for session cycles and yearly cycles
            dataAccessorMock.Setup<Task<Collection<SessionCycles>>>(acc => acc.BulkReadRecordAsync<SessionCycles>("SESSION.CYCLES", "", true)).Returns(Task.FromResult<Collection<SessionCycles>>(null));
            dataAccessorMock.Setup<Task<Collection<YearlyCycles>>>(acc => acc.BulkReadRecordAsync<YearlyCycles>("YEARLY.CYCLES", "", true)).Returns(Task.FromResult<Collection<YearlyCycles>>(null));

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Construct course repository
            convertedCourseRepo = new CourseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

            return convertedCourseRepo;
        }

        private Collection<Courses> BuildCoursesResponse(IEnumerable<Course> courses)
        {
            var courseStatuses = BuildCourseStatusesResponse();
            Collection<Courses> repoCourses = new Collection<Courses>();
            foreach (var course in courses)
            {
                var repoCrs = new Courses();
                repoCrs.Recordkey = course.Id.ToString();
                repoCrs.CrsAcadLevel = course.AcademicLevelCode;
                repoCrs.CrsCeus = course.Ceus;
                repoCrs.CrsDepts = course.DepartmentCodes.ToList();
                repoCrs.CrsLevels = course.CourseLevelCodes.ToList();
                repoCrs.CrsMinCred = course.MinimumCredits;
                repoCrs.CrsNo = course.Number;
                repoCrs.CrsSubject = course.SubjectCode;
                repoCrs.CrsShortTitle = course.Title;
                repoCrs.CrsDesc = course.Description;
                repoCrs.CrsMaxCred = course.MaximumCredits;
                repoCrs.CrsVarCredIncrement = course.VariableCreditIncrement;
                repoCrs.CrsLocations = course.LocationCodes.ToList();
                repoCrs.CrsStatus = new List<string>() { (courseStatuses[course.Status]) };
                repoCrs.CrsStartDate = course.StartDate;
                repoCrs.CrsEndDate = course.EndDate;
                repoCrs.CrsBillingCred = course.BillingCredits;

                repoCrs.CrsReqs = new List<string>();
                if (course.Requisites.Count() > 0)
                {
                    foreach (var req in course.Requisites)
                    {
                        repoCrs.CrsReqs.Add(req.RequirementCode);
                    }
                }

                repoCrs.CourseDeptsEntityAssociation = new List<CoursesCourseDepts>();
                if (course.Departments != null && course.Departments.Count > 0)
                {
                    foreach (var od in course.Departments)
                    {
                        repoCrs.CourseDeptsEntityAssociation.Add(new CoursesCourseDepts(od.AcademicDepartmentCode, od.ResponsibilityPercentage));
                    }
                }

                repoCrs.ApprovalStatusEntityAssociation = new List<CoursesApprovalStatus>();
                if (course.CourseApprovals != null && course.CourseApprovals.Count > 0)
                {
                    foreach (var ca in course.CourseApprovals)
                    {
                        repoCrs.ApprovalStatusEntityAssociation.Add(new CoursesApprovalStatus(ca.StatusCode, ca.ApprovingPersonId, ca.ApprovingAgencyId, ca.Date, ca.StatusDate));
                    }
                }

                // Set up repository course equate codes.
                // See BuildCourseEquateCodesResponse method for the course equate codes records
                // See CourseRepository FindEquates for an explanation of how course equates are unraveled.
                switch (course.Id)
                {
                    case "186":
                        // MATH 350
                        repoCrs.CrsEquateCodes = new List<string>() { "1", "2" };
                        break;
                    case "306":
                        // MATH 102
                        repoCrs.CrsEquateCodes = new List<string>() { "1" };
                        break;
                    case "213":
                        // MATH 103
                        repoCrs.CrsEquateCodes = new List<string>() { "2" };
                        break;
                    case "353":
                        // MATH 371
                        repoCrs.CrsEquateCodes = new List<string>() { "3" };
                        break;
                    case "226":
                        // MATH 491
                        repoCrs.CrsEquateCodes = new List<string>() { "3", "4" };
                        break;
                    case "122":
                        // DANC 100
                        repoCrs.CrsEquateCodes = new List<string>() { "5" };
                        break;
                    case "28":
                        // DANC 200
                        repoCrs.CrsEquateCodes = new List<string>() { "5" };
                        break;
                    default:
                        repoCrs.CrsEquateCodes = new List<string>();
                        break;
                }

                // Add special data to data accessor record to test session and yearly cycles
                switch (course.Id)
                {
                    case "87":
                        repoCrs.CrsSessionCycle = "F";
                        repoCrs.CrsYearlyCycle = "A";
                        break;
                    default:
                        repoCrs.CrsSessionCycle = "";
                        repoCrs.CrsYearlyCycle = "";
                        break;
                }

                // Add coreq association information
                if (course.Requisites != null)
                {
                    var coreqs = course.Requisites.Where(r => r.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.Concurrent);
                    repoCrs.CourseCoreqsEntityAssociation = new List<CoursesCourseCoreqs>();
                    foreach (var coreq in coreqs)
                    {
                        repoCrs.CourseCoreqsEntityAssociation.Add(new CoursesCourseCoreqs(coreq.CorequisiteCourseId, (coreq.IsRequired == true ? "Y" : "")));
                    }
                }

                // course types
                repoCrs.CrsCourseTypes = course.Types.ToList();
                if (course.Id == "200")
                {
                    repoCrs.CrsCourseTypes = new List<string>() { "CORE" };
                }
                else if (course.Id == "201")
                {
                    repoCrs.CrsCourseTypes = new List<string>() { "PSE" };
                }
                else if (course.Id == "202")
                {
                    repoCrs.CrsCourseTypes = new List<string>() { "CORE", "PSE", "REM" };
                }

                // Local credit type
                repoCrs.CrsCredType = course.LocalCreditType;

                repoCourses.Add(repoCrs);
            }
            return repoCourses;
        }

        private Collection<SessionCycles> BuildSessionCyclesResponse()
        {
            var sessionCycles = new Collection<SessionCycles>();

            var sc1 = new SessionCycles();
            sc1.Recordkey = "F";
            sc1.ScDesc = "Fall Term Only";
            sessionCycles.Add(sc1);

            var sc2 = new SessionCycles();
            sc2.Recordkey = "EI";
            sc2.ScDesc = "Even Year Intercession";
            sessionCycles.Add(sc2);

            return sessionCycles;

        }

        private Collection<SessionCycles> BuildNullSessionCyclesResponse()
        {
            Collection<SessionCycles> sessionCycles = new Collection<SessionCycles>();
            sessionCycles = null;
            return sessionCycles;
        }

        private Collection<YearlyCycles> BuildYearlyCyclesResponse()
        {
            var yearlyCycles = new Collection<YearlyCycles>();

            var yc1 = new YearlyCycles();
            yc1.Recordkey = "A";
            yc1.YcDesc = "Every Other Year";
            yearlyCycles.Add(yc1);

            var yc2 = new YearlyCycles();
            yc2.Recordkey = "B";
            yc2.YcDesc = "Every Third Year";
            yearlyCycles.Add(yc2);

            return yearlyCycles;

        }

        private Collection<YearlyCycles> BuildNullYearlyCyclesResponse()
        {
            Collection<YearlyCycles> yearlyCycles = new Collection<YearlyCycles>();
            yearlyCycles = null;
            return yearlyCycles;
        }

        // This is just temporary logic to handle a proposed course statuses valcode
        // to convert between course status and the valcode internal code.
        private Dictionary<CourseStatus, string> BuildCourseStatusesResponse()
        {
            var courseStatuses = new Dictionary<CourseStatus, string>();
            courseStatuses.Add(CourseStatus.Active, "A");
            courseStatuses.Add(CourseStatus.Terminated, "X");
            courseStatuses.Add(CourseStatus.Unknown, "P");
            return courseStatuses;
        }

        private Collection<CourseEquateCodes> BuildCourseEquateCodesResponse()
        {
            // See the course repository method FindEquates for an explanation of this setup
            var courseEquateCodes = new Collection<CourseEquateCodes>();
            courseEquateCodes.Add(new CourseEquateCodes() { Recordkey = "1", CecCourses = new List<string>() { "186", "306" } }); //Math 350 can be taken instead of Math 102 (but not vice-versa)
            courseEquateCodes.Add(new CourseEquateCodes() { Recordkey = "2", CecCourses = new List<string>() { "186", "213" } }); //Math 350 can be taken instead of Math 103 (but not vice-versa)
            courseEquateCodes.Add(new CourseEquateCodes() { Recordkey = "3", CecCourses = new List<string>() { "226", "353" } }); //Math 491 can be taken instead of Math 371 (but not vice-versa, see below)
            courseEquateCodes.Add(new CourseEquateCodes() { Recordkey = "4", CecCourses = new List<string>() { "226" } }); // Equate code created so that Math 371 cannot be an equate of Math 491
            courseEquateCodes.Add(new CourseEquateCodes() { Recordkey = "5", CecCourses = new List<string>() { "122", "28" } }); //DANC 100 equates directly to DANC 200 and vice-versa
            return courseEquateCodes;
        }

        private ApplValcodes BuildCourseTypesValcode()
        {
            ApplValcodes courseTypesValcode = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "CORE", ValActionCode1AssocMember = "" },
                                                                      new ApplValcodesVals() { ValInternalCodeAssocMember = "PSE", ValActionCode1AssocMember = "p"},
                                                                      new ApplValcodesVals() { ValInternalCodeAssocMember = "REM", ValActionCode1AssocMember = ""}}
            };
            return courseTypesValcode;
        }

        private Collection<CredTypes> BuildCreditTypes()
        {
            // See the course repository method FindEquates for an explanation of this setup
            var credTypes = new Collection<CredTypes>();
            credTypes.Add(new CredTypes() { Recordkey = "LG,", CrtpCategory = "O", CrtpDesc = "Test for Ldg W/ , and " });
            credTypes.Add(new CredTypes() { Recordkey = "C", CrtpCategory = "I", CrtpDesc = "Credit-Not Degree Applicable" });
            credTypes.Add(new CredTypes() { Recordkey = "CE", CrtpCategory = "C", CrtpDesc = "Continuing Education" });
            credTypes.Add(new CredTypes() { Recordkey = "D", CrtpCategory = "I", CrtpDesc = "Credit-Degree Applicable" });
            credTypes.Add(new CredTypes() { Recordkey = "HS,", CrtpCategory = "O", CrtpDesc = "High School" });
            credTypes.Add(new CredTypes() { Recordkey = "IN", CrtpCategory = "I", CrtpDesc = "Institutional" });
            credTypes.Add(new CredTypes() { Recordkey = "LE", CrtpCategory = "O", CrtpDesc = "Life Experience" });
            credTypes.Add(new CredTypes() { Recordkey = "N", CrtpCategory = "O", CrtpDesc = "Noncredit (not Community Srvc)" });
            credTypes.Add(new CredTypes() { Recordkey = "NC", CrtpCategory = "O", CrtpDesc = "Noncourse" });
            credTypes.Add(new CredTypes() { Recordkey = "RE", CrtpCategory = "O", CrtpDesc = "Remedial" });
            credTypes.Add(new CredTypes() { Recordkey = "TR", CrtpCategory = "T", CrtpDesc = "Transfer" });
            credTypes.Add(new CredTypes() { Recordkey = "WFD", CrtpCategory = "C", CrtpDesc = "Workforce Development" });
            return credTypes;
        }

        private CdDefaults BuildCourseParametersConvertedResponse()
        {
            // Converted Response
            CdDefaults defaults = new CdDefaults();
            defaults.Recordkey = "CD.DEFAULTS";
            defaults.CdReqsConvertedFlag = "Y";
            return defaults;
        }

        private CdDefaults BuildCourseParametersNullResponse()
        {
            // null response - could not read CdDefaults
            CdDefaults defaults = null;
            return defaults;
        }

        private Collection<AcadReqmts> BuildAcadReqmtsResponse()
        {
            Collection<AcadReqmts> acadReqmtsResponse = new Collection<AcadReqmts>();
            // Previous, Required
            var acadReqmts1 = new AcadReqmts() { Recordkey = "PREREQ1", AcrReqsTiming = "P", AcrReqsEnforcement = "RQ", AcrReqsProtectFlag = "" };
            acadReqmtsResponse.Add(acadReqmts1);
            // Previous, Recommended
            var acadReqmts2 = new AcadReqmts() { Recordkey = "PREREQ2", AcrReqsTiming = "P", AcrReqsEnforcement = "RM", AcrReqsProtectFlag = "Y" };
            acadReqmtsResponse.Add(acadReqmts2);
            // Concurrent, Required
            var acadReqmts3 = new AcadReqmts() { Recordkey = "COREQ1", AcrReqsTiming = "C", AcrReqsEnforcement = "RQ", AcrReqsProtectFlag = "N" };
            acadReqmtsResponse.Add(acadReqmts3);
            // Concurrent, Recommended
            var acadReqmts4 = new AcadReqmts() { Recordkey = "COREQ2", AcrReqsTiming = "C", AcrReqsEnforcement = "RM", AcrReqsProtectFlag = "" };
            acadReqmtsResponse.Add(acadReqmts4);
            //  Previous or Concurrent, Required
            var acadReqmts5 = new AcadReqmts() { Recordkey = "REQ1", AcrReqsTiming = "E", AcrReqsEnforcement = "RQ", AcrReqsProtectFlag = "" };
            acadReqmtsResponse.Add(acadReqmts5);
            return acadReqmtsResponse;
        }
    }
}