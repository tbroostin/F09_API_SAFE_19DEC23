// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentDegreePlanRepositoryTests
    {
        [TestClass]
        public class StudentDegreePlanGetTests
        {
            Mock<IColleagueDataReader> dataAccessorMock;
            StudentDegreePlanRepository studentDegreePlanRepo;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;

            [TestInitialize]
            public async void Initialize()
            {
                // Build degree plan repository
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                apiSettingsMock = new ApiSettings("null");

                studentDegreePlanRepo = await BuildValidDegreePlanRepository();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanRepo = null;
                cacheProviderMock = null;
                dataAccessorMock = null;
                transFactoryMock = null;
            }

            [TestMethod]
            public async Task GetDegreePlanByPlanId_TestId()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                Assert.AreEqual(2, degreePlan.Id);
            }

            [TestMethod]
            public async Task GetDegreePlanByPlanId_TestPersonId()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                Assert.AreEqual("0000894", degreePlan.PersonId);
            }

            [TestMethod]
            public async Task GetDegreePlanByPlanId_TestVersion()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                Assert.AreEqual(1, degreePlan.Version);
            }

            [TestMethod]
            public async Task GetDegreePlanWithTerms_TestTerms()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                Assert.AreEqual(7, degreePlan.TermIds.Count());
            }

            [TestMethod]
            public async Task GetDegreePlanWithNoTerms_TestTerms()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(4);
                Assert.AreEqual(0, degreePlan.TermIds.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task Get_Single_DegreePlanNotFoundThrowsException()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(1111);
            }

            [TestMethod]
            public async Task GetDegreePlan_PlannedCourses()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                var courses = degreePlan.GetPlannedCourses("2008/FA");
                Assert.AreEqual(3, courses.Count());
            }

            [TestMethod]
            public async Task GetDegreePlan_NonTermPlannedCourses()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(3);
                Assert.AreEqual(1, degreePlan.NonTermPlannedCourses.Count());
            }

            [TestMethod]
            public async Task GetDegreePlan_NonTermPlannedCoursesEmpty()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                Assert.AreEqual(0, degreePlan.NonTermPlannedCourses.Count());
            }

            [TestMethod]
            public async Task GetDegreePlanWithTerms_TestPlannedCourseProperties()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(3);
                var courses = degreePlan.GetPlannedCourses("2008/FA");
                Assert.AreEqual(3, courses.Count());
                Assert.AreEqual("100", courses.First(pc => pc.CourseId == "130").SectionId);
                Assert.AreEqual(4.0m, courses.First(pc => pc.CourseId == "143").Credits);
                Assert.AreEqual(GradingType.Audit, courses.First(pc => pc.CourseId == "139").GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.Active, courses.First(pc => pc.SectionId == "200").WaitlistedStatus);
                Assert.AreEqual(true, courses.First(pc => pc.CourseId == "139").IsProtected);
            }

            [TestMethod]
            public async Task GetDegreePlanWithTerms_TestCoursesAndNullSection()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(3);
                var courses = degreePlan.GetPlannedCourses("2009/SP");
                Assert.IsTrue(courses.Count() >= 2);
                Assert.IsNull(courses.First(pc => pc.CourseId == "117").SectionId);
            }

            [TestMethod]
            public async Task GetDegreePlanWithTerms_TestNullCredits()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                var courses = degreePlan.GetPlannedCourses("2009/SP");
                Assert.IsNull(courses.First(pc => pc.CourseId == "110").Credits);
            }

            [TestMethod]
            public async Task GetDegreePlan_Approvals()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                Assert.AreEqual(2, degreePlan.Approvals.Count());
            }

            [TestMethod]
            public async Task GetDegreePlan_ApprovalDate()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                var approval = degreePlan.Approvals.First();
                Assert.AreEqual(new DateTime(2008, 06, 01, 10, 0, 0), approval.Date);
            }

            [TestMethod]
            public async Task GetDegreePlan_ApprovalStatus()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                var approval = degreePlan.Approvals.First();
                Assert.AreEqual(DegreePlanApprovalStatus.Approved, approval.Status);
            }

            [TestMethod]
            public async Task GetDegreePlan_ApprovalPersonId()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                var approval = degreePlan.Approvals.First();
                Assert.AreEqual("00004001", approval.PersonId);
            }

            [TestMethod]
            public async Task GetDegreePlan_ApprovalTermCode()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                var approval = degreePlan.Approvals.First();
                Assert.AreEqual("2008/FA", approval.TermCode);
            }

            [TestMethod]
            public async Task GetDegreePlan_ApprovalCourseId()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                var approval = degreePlan.Approvals.First();
                Assert.AreEqual("130", approval.CourseId);
            }

            [TestMethod]
            public async Task GetDegreePlansForStudents()
            {
                dataAccessorMock.Setup(dc => dc.SelectAsync("DEGREE_PLAN", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(new string[] { "802", "808" });
                dataAccessorMock.Setup(dc => dc.BulkReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", new string[] { "802", "808" }, It.IsAny<bool>())).ReturnsAsync(new Collection<DataContracts.DegreePlan>() { BuildDegreePlanResponse(await new TestStudentDegreePlanRepository().GetAsync(802)), BuildDegreePlanResponse(await new TestStudentDegreePlanRepository().GetAsync(808)) });

                dataAccessorMock.Setup(dc => dc.SelectAsync("DEGREE_PLAN_TERMS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(new string[] { "802", "808" });
                dataAccessorMock.Setup(dc => dc.BulkReadRecordAsync<DataContracts.DegreePlanTerms>(new string[] { "802", "808" }, It.IsAny<bool>())).ReturnsAsync(BuildDegreePlanTermsResponse(new List<DegreePlan>() { await new TestStudentDegreePlanRepository().GetAsync(802), await new TestStudentDegreePlanRepository().GetAsync(808) }));

                dataAccessorMock.Setup(dc => dc.SelectAsync("WAIT.LIST", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(new string[] { "00004002", "00004008" });
                dataAccessorMock.Setup(dc => dc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.WaitList>(new string[] { "00004002", "00004008" }, It.IsAny<bool>())).ReturnsAsync(BuildWaitListResponse(await new TestStudentDegreePlanRepository().GetAsync(3)));

                dataAccessorMock.Setup(dc => dc.SelectAsync("DEGREE_PLAN_COMMENT", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(new string[] { "802", "808" });
                dataAccessorMock.Setup(dc => dc.BulkReadRecordAsync<DataContracts.DegreePlanComment>(new string[] { "802", "808" }, It.IsAny<bool>())).ReturnsAsync(BuildDegreePlanCommentResponse(await new TestStudentDegreePlanRepository().GetAsync(802)));

                dataAccessorMock.Setup(dc => dc.SelectAsync("DEGREE_PLAN_RSTR_CMT", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(new string[] { "802", "808" });
                dataAccessorMock.Setup(dc => dc.BulkReadRecordAsync<DataContracts.DegreePlanRstrCmt>(new string[] { "802", "808" }, It.IsAny<bool>())).ReturnsAsync(BuildDegreePlanRstrCmtResponse(await new TestStudentDegreePlanRepository().GetAsync(802)));

                IEnumerable<DegreePlan> degreeplans = await studentDegreePlanRepo.GetAsync(new List<string>() { "00004002", "00004008" });
                Assert.AreEqual("00004002", degreeplans.ElementAt(0).PersonId);
                Assert.AreEqual("00004008", degreeplans.ElementAt(1).PersonId);
            }

            [TestMethod]
            public async Task GetDegreePlansForEmptyListOfStudentsReturnsEmptyList()
            {
                IEnumerable<DegreePlan> degreePlans = await studentDegreePlanRepo.GetAsync(new List<string>());
                Assert.AreEqual(0, degreePlans.Count());
            }

            [TestMethod]
            public async Task GetDegreePlansForNullListOfStudentsReturnsEmptyList()
            {
                List<string> nullList = null;
                IEnumerable<DegreePlan> degreePlans = await studentDegreePlanRepo.GetAsync(nullList);
                Assert.AreEqual(0, degreePlans.Count());
            }

            [TestMethod]
            public async Task NoDegreePlanforStudentReturnsNull()
            {
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", It.IsAny<string>(), true)).ReturnsAsync(new Collection<DataContracts.DegreePlan>());
                IEnumerable<DegreePlan> degreeplans = await studentDegreePlanRepo.GetAsync(new List<string>() { "000011111" });
                Assert.AreEqual(0, degreeplans.Count());
            }

            [TestMethod]
            public async Task GetDegreePlan_AddsWaitlistedSectionToPlannedCourse()
            {
                // Verifies that a section waitlisted outside of the degree plan will be connected to the course
                // When the degree plan is brought in by the repository.
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(3);
                var courses = degreePlan.GetPlannedCourses("2009/SP");
                var plannedCourse = courses.First(pc => pc.CourseId == "333");
                Assert.AreEqual("500", plannedCourse.SectionId);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister, plannedCourse.WaitlistedStatus);
            }

            [TestMethod]
            public async Task GetDegreePlan_AddsWaitlistedCourseToDegreePlan()
            {
                // Verifies that a section waitlisted outside of the degree plan will be added to the
                // degree plan by adding a course and the waitlisted section.
                // Further, even if the course has already been planned once with an associated section, 
                // another course will be added to the degree plan and associated with the waitlisted section.
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(3);
                var courses = degreePlan.GetPlannedCourses("2009/SP");
                var plannedCourses = courses.Where(pc => pc.CourseId == "42");
                Assert.AreEqual(2, plannedCourses.Count());
                var pc1 = plannedCourses.First(pc => pc.SectionId == "600");
                Assert.AreEqual("600", pc1.SectionId);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.Active, pc1.WaitlistedStatus);
                var pc2 = plannedCourses.First(pc => pc.SectionId == "700");
                Assert.AreEqual("700", pc2.SectionId);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister, pc2.WaitlistedStatus);
            }

            [TestMethod]
            public async Task GetDegreePlan_CommentsIncluded()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(3);
                Assert.AreEqual(2, degreePlan.Notes.Count());
                Assert.AreEqual(1, degreePlan.Notes.ElementAt(0).Id);
                Assert.AreEqual("0000013", degreePlan.Notes.ElementAt(0).PersonId);
                Assert.AreEqual("This is comment number one", degreePlan.Notes.ElementAt(0).Text);
                Assert.AreEqual(new DateTime(2012, 12, 27), degreePlan.Notes.ElementAt(0).Date.GetValueOrDefault().Date);
                Assert.AreEqual(new TimeSpan(10, 31, 01), degreePlan.Notes.ElementAt(0).Date.GetValueOrDefault().TimeOfDay);
            }

            [TestMethod]
            public async Task GetDegreePlan_CommentValueMarksConvertedToNewLine()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(3);
                var multilineComment = degreePlan.Notes.Where(n => n.PersonId == "0000014").FirstOrDefault();
                Assert.IsNotNull(multilineComment);
                Assert.AreEqual("Second comment Line 1\nSecond comment Line 2\nSecond comment Line 3", multilineComment.Text);
                Assert.AreEqual(new DateTime(2013, 01, 13), multilineComment.Date.GetValueOrDefault().Date);
                Assert.AreEqual(new TimeSpan(13, 29, 12), multilineComment.Date.GetValueOrDefault().TimeOfDay);
            }

            [TestMethod]
            public async Task GetDegreePlan_RestrictedCommentsIncluded()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(3);
                Assert.AreEqual(2, degreePlan.RestrictedNotes.Count());
                Assert.AreEqual(1, degreePlan.RestrictedNotes.ElementAt(0).Id);
                Assert.AreEqual("0000013", degreePlan.RestrictedNotes.ElementAt(0).PersonId);
                Assert.AreEqual("This is restricted comment number one", degreePlan.RestrictedNotes.ElementAt(0).Text);
                Assert.AreEqual(new DateTime(2013, 12, 27), degreePlan.RestrictedNotes.ElementAt(0).Date.GetValueOrDefault().Date);
                Assert.AreEqual(new TimeSpan(11, 31, 01), degreePlan.RestrictedNotes.ElementAt(0).Date.GetValueOrDefault().TimeOfDay);
            }

            [TestMethod]
            public async Task GetDegreePlan_RestrictedCommentValueMarksConvertedToNewLine()
            {
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(3);
                var multilineComment = degreePlan.RestrictedNotes.Where(n => n.PersonId == "0000014").FirstOrDefault();
                Assert.IsNotNull(multilineComment);
                Assert.AreEqual("Restricted Comment 2 Line 1\nRestricted comment 2 Line 2\nRestricted comment 2 Line 3", multilineComment.Text);
                Assert.AreEqual(new DateTime(2014, 01, 13), multilineComment.Date.GetValueOrDefault().Date);
                Assert.AreEqual(new TimeSpan(14, 29, 12), multilineComment.Date.GetValueOrDefault().TimeOfDay);
            }

            [TestMethod]
            public async Task Get_DegreePlan_DuplicateTerm()
            {
                // Setup repo response for getting the terms for the degree plans above. In this case there are two terms duplicated.
                var duplicatePlanTermsResponse = new Collection<DataContracts.DegreePlanTerms>() { new DataContracts.DegreePlanTerms() { DptDegreePlan = "2", DptTerm = "2014/FA", Recordkey = "555" }, new DataContracts.DegreePlanTerms() { DptDegreePlan = "2", DptTerm = "2014/FA", Recordkey = "556" } };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.DegreePlanTerms>(It.IsAny<string>(), true)).ReturnsAsync(duplicatePlanTermsResponse);
                DegreePlan degreePlan = await studentDegreePlanRepo.GetAsync(2);
                Assert.AreEqual(1, degreePlan.TermIds.Count());
            }

            private async Task<StudentDegreePlanRepository> BuildValidDegreePlanRepository()
            {
                // Set up data accessor for mocking (needed for get)
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var testStudentDegreePlanRepository = new TestStudentDegreePlanRepository();
                // Setup repo response for getting a single plan by plan Id (plan has terms) - PLAN 2
                var degreePlan1 = await testStudentDegreePlanRepository.GetAsync(2);
                var degreePlanResponse1 = BuildDegreePlanResponse(degreePlan1);
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", "2", true)).ReturnsAsync(degreePlanResponse1);

                // Setup repo response for getting a single plan by plan Id (plan has no terms) - PLAN 4
                var degreePlan4 = await testStudentDegreePlanRepository.GetAsync(4);
                var degreePlanResponse2 = BuildDegreePlanResponse(degreePlan4);
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", "4", true)).ReturnsAsync(degreePlanResponse2);

                // Setup repo response for getting a single plan by plan Id (plan has terms, courses, sections) - PLAN 3
                var degreePlan3 = await testStudentDegreePlanRepository.GetAsync(3);
                var degreePlanResponse3 = BuildDegreePlanResponse(degreePlan3);
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", "3", true)).ReturnsAsync(degreePlanResponse3);

                // Setup repo response for getting the terms for the degree plans above.
                var degreePlanTermsResponse = BuildDegreePlanTermsResponse(new List<DegreePlan>() { degreePlan1, degreePlan3 });
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.DegreePlanTerms>(It.IsAny<string>(), true)).ReturnsAsync(degreePlanTermsResponse);

                // Set up repo response for getting waitlist info for the degree plan
                Collection<DataContracts.WaitList> waitlistResponseData = BuildWaitListResponse(degreePlan3);
                dataAccessorMock.Setup(wl => wl.BulkReadRecordAsync<DataContracts.WaitList>(It.IsAny<string>(), true)).ReturnsAsync(waitlistResponseData);

                // Set up repo response for degree plan comments
                dataAccessorMock.Setup(dpc => dpc.BulkReadRecordAsync<DataContracts.DegreePlanComment>(It.IsAny<string>(), false))
                    .ReturnsAsync(BuildDegreePlanCommentResponse(degreePlan3));

                // Set up repo response for degree plan restricted comments
                dataAccessorMock.Setup(dpc => dpc.BulkReadRecordAsync<DataContracts.DegreePlanRstrCmt>(It.IsAny<string>(), false))
                    .ReturnsAsync(BuildDegreePlanRstrCmtResponse(degreePlan3));

                // Set up repo response for waitlist statuses
                ApplValcodes waitlistCodeResponse = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "1" },
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValActionCode1AssocMember = "2"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "D", ValActionCode1AssocMember = "3"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "P", ValActionCode1AssocMember = "4"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "5"}}
                };
                dataAccessorMock.Setup(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "WAIT.LIST.STATUSES", true)).ReturnsAsync(waitlistCodeResponse);

                // Set up response for get (many student) degree plans
                var degreePlans = new Collection<DataContracts.DegreePlan>();
                var degreePlan5 = await testStudentDegreePlanRepository.GetAsync(802);
                degreePlans.Add(BuildDegreePlanResponse(degreePlan5));

                var degreePlan6 = await testStudentDegreePlanRepository.GetAsync(808);
                degreePlans.Add(BuildDegreePlanResponse(degreePlan6));
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", It.Is<string>(s => s.Contains("00004002")), true)).ReturnsAsync(degreePlans);

                studentDegreePlanRepo = new StudentDegreePlanRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
                return studentDegreePlanRepo;
            }

            private DataContracts.DegreePlan BuildDegreePlanResponse(DegreePlan degreePlan)
            {
                DataContracts.DegreePlan degreePlanResponse = new DataContracts.DegreePlan();
                degreePlanResponse.Recordkey = degreePlan.Id.ToString();
                degreePlanResponse.DpStudentId = degreePlan.PersonId;
                degreePlanResponse.DpVersionNumber = degreePlan.Version.ToString();
                degreePlanResponse.DpApprovalsEntityAssociation = new List<DataContracts.DegreePlanDpApprovals>();

                var approvals = degreePlan.Approvals;
                foreach (var item in approvals)
                {
                    var approval = new DataContracts.DegreePlanDpApprovals();
                    approval.DpApprovalDateAssocMember = item.Date.DateTime;
                    approval.DpApprovalPersonIdAssocMember = item.PersonId;
                    approval.DpApprovalStatusAssocMember = item.Status.ToString();
                    approval.DpApprovalTimeAssocMember = item.Date.DateTime;
                    approval.DpApprovalTermIdAssocMember = item.TermCode;
                    approval.DpApprovalCourseIdAssocMember = item.CourseId;
                    degreePlanResponse.DpApprovalsEntityAssociation.Add(approval);
                }

                return degreePlanResponse;
            }

            private Collection<DataContracts.DegreePlanTerms> BuildDegreePlanTermsResponse(List<DegreePlan> degreePlans)
            {
                List<DataContracts.DegreePlanTerms> degreePlanTermsList = new List<DataContracts.DegreePlanTerms>();
                foreach (var degreePlan in degreePlans)
                {
                    foreach (var term in degreePlan.TermIds)
                    {
                        DataContracts.DegreePlanTerms dpterm = new DataContracts.DegreePlanTerms();
                        dpterm.DptTerm = term;
                        dpterm.DptDegreePlan = degreePlan.Id.ToString();
                        IEnumerable<PlannedCourse> plannedCourses = degreePlan.GetPlannedCourses(term);
                        dpterm.PlannedCoursesEntityAssociation = new List<DataContracts.DegreePlanTermsPlannedCourses>();
                        foreach (var pc in plannedCourses)
                        {
                            string isAlt = "N";
                            string gradingType = null;
                            switch (pc.GradingType)
                            {
                                case Ellucian.Colleague.Domain.Student.Entities.GradingType.PassFail:
                                    gradingType = "P";
                                    break;
                                case Ellucian.Colleague.Domain.Student.Entities.GradingType.Audit:
                                    gradingType = "A";
                                    break;
                                default:
                                    break;
                            }
                            string isProtected = pc.IsProtected == true ? "Y" : "N";
                            dpterm.PlannedCoursesEntityAssociation.Add(new DataContracts.DegreePlanTermsPlannedCourses(pc.CourseId, pc.SectionId, pc.Credits, isAlt, gradingType, null, null, null, isProtected));
                        }
                        degreePlanTermsList.Add(dpterm);
                    }
                    foreach (var npc in degreePlan.NonTermPlannedCourses)
                    {
                        DataContracts.DegreePlanTerms dpterm = new DataContracts.DegreePlanTerms();
                        dpterm.DptDegreePlan = degreePlan.Id.ToString();
                        dpterm.PlannedCoursesEntityAssociation = new List<DataContracts.DegreePlanTermsPlannedCourses>();

                        string isAlt = "N";
                        string gradingType = null;
                        switch (npc.GradingType)
                        {
                            case Ellucian.Colleague.Domain.Student.Entities.GradingType.PassFail:
                                gradingType = "P";
                                break;
                            case Ellucian.Colleague.Domain.Student.Entities.GradingType.Audit:
                                gradingType = "A";
                                break;
                            default:
                                break;
                        }
                        dpterm.PlannedCoursesEntityAssociation.Add(new DataContracts.DegreePlanTermsPlannedCourses(npc.CourseId, npc.SectionId, npc.Credits, isAlt, gradingType, null, null, null, null));
                        degreePlanTermsList.Add(dpterm);
                    }
                }
                Collection<DataContracts.DegreePlanTerms> degreePlanTermsResponse = new Collection<DataContracts.DegreePlanTerms>(degreePlanTermsList);
                return degreePlanTermsResponse;
            }
            private Collection<DataContracts.WaitList> BuildWaitListResponse(DegreePlan degreePlan)
            {
                Collection<DataContracts.WaitList> waitResponse = new Collection<DataContracts.WaitList>();
                foreach (var term in degreePlan.TermIds)
                {
                    IEnumerable<PlannedCourse> plannedCourses = degreePlan.GetPlannedCourses(term);
                    foreach (var pc in plannedCourses)
                    {
                        if (!string.IsNullOrEmpty(pc.SectionId) && pc.WaitlistedStatus == Domain.Student.Entities.DegreePlans.WaitlistStatus.Active)
                        {
                            var wl = new DataContracts.WaitList();
                            wl.Recordkey = pc.SectionId;
                            wl.WaitCourse = pc.CourseId;
                            wl.WaitCourseSection = pc.SectionId;
                            wl.WaitStatus = "A";
                            wl.WaitCred = pc.Credits;
                            wl.WaitTerm = term;
                            wl.WaitStudent = degreePlan.PersonId;
                            waitResponse.Add(wl);
                        }
                    }
                }

                if (degreePlan.Id == 3)
                {
                    // Add waitlist section for planned course 333 (no section specified in plan)
                    var wl1 = new DataContracts.WaitList();
                    wl1.Recordkey = "500";
                    wl1.WaitCourse = "333";
                    wl1.WaitCourseSection = "500";
                    wl1.WaitStatus = "P";
                    wl1.WaitCred = 3m;
                    wl1.WaitTerm = "2009/SP";
                    wl1.WaitStudent = degreePlan.PersonId;
                    waitResponse.Add(wl1);

                    // Add waitlist section for unplanned course 42
                    var wl2 = new DataContracts.WaitList();
                    wl2.Recordkey = "600";
                    wl2.WaitCourse = "42";
                    wl2.WaitCourseSection = "600";
                    wl2.WaitStatus = "A";
                    wl2.WaitCred = 3m;
                    wl2.WaitTerm = "2009/SP";
                    wl2.WaitStudent = degreePlan.PersonId;
                    waitResponse.Add(wl2);

                    // Add second waitlist section for unplanned course 42
                    var wl3 = new DataContracts.WaitList();
                    wl3.Recordkey = "700";
                    wl3.WaitCourse = "42";
                    wl3.WaitCourseSection = "700";
                    wl3.WaitStatus = "P";
                    wl3.WaitCred = 3m;
                    wl3.WaitTerm = "2009/SP";
                    wl3.WaitStudent = degreePlan.PersonId;
                    waitResponse.Add(wl3);
                }

                return waitResponse;
            }

            private Collection<DataContracts.DegreePlanComment> BuildDegreePlanCommentResponse(DegreePlan degreePlan)
            {
                var comments = new Collection<DataContracts.DegreePlanComment>();

                var comment1 = new DataContracts.DegreePlanComment();
                comment1.Recordkey = "1";
                comment1.DpcDegreePlan = degreePlan.Id.ToString();
                comment1.DegreePlanCommentAddopr = "0000013";
                comment1.DpcText = "This is comment number one";
                comment1.DegreePlanCommentAdddate = new DateTime(2012, 12, 27);
                comment1.DegreePlanCommentAddtime = new DateTime(2001, 01, 01, 10, 31, 01);

                comments.Add(comment1);

                var comment2 = new DataContracts.DegreePlanComment();
                comment2 = new DataContracts.DegreePlanComment();
                comment2.Recordkey = "999999";
                comment2.DpcDegreePlan = degreePlan.Id.ToString();
                comment2.DegreePlanCommentAddopr = "0000014";
                comment2.DpcText = "Second comment Line 1ýSecond comment Line 2ýSecond comment Line 3";
                comment2.DegreePlanCommentAdddate = new DateTime(2013, 01, 13);
                comment2.DegreePlanCommentAddtime = new DateTime(2001, 01, 01, 13, 29, 12);

                comments.Add(comment2);

                return comments;
            }

            private Collection<DataContracts.DegreePlanRstrCmt> BuildDegreePlanRstrCmtResponse(DegreePlan degreePlan)
            {
                var comments = new Collection<DataContracts.DegreePlanRstrCmt>();

                var comment1 = new DataContracts.DegreePlanRstrCmt();
                comment1.Recordkey = "1";
                comment1.DprcDegreePlan = degreePlan.Id.ToString();
                comment1.DegreePlanRstrCmtAddopr = "0000013";
                comment1.DprcText = "This is restricted comment number one";
                comment1.DegreePlanRstrCmtAdddate = new DateTime(2013, 12, 27);
                comment1.DegreePlanRstrCmtAddtime = new DateTime(2001, 01, 01, 11, 31, 01);

                comments.Add(comment1);

                var comment2 = new DataContracts.DegreePlanRstrCmt();
                comment2 = new DataContracts.DegreePlanRstrCmt();
                comment2.Recordkey = "999999";
                comment2.DprcDegreePlan = degreePlan.Id.ToString();
                comment2.DegreePlanRstrCmtAddopr = "0000014";
                comment2.DprcText = "Restricted Comment 2 Line 1ýRestricted comment 2 Line 2ýRestricted comment 2 Line 3";
                comment2.DegreePlanRstrCmtAdddate = new DateTime(2014, 01, 13);
                comment2.DegreePlanRstrCmtAddtime = new DateTime(2001, 01, 01, 14, 29, 12);

                comments.Add(comment2);

                return comments;
            }
        }

        [TestClass]
        public class StudentDegreePlanAddTests
        {
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;
            StudentDegreePlanRepository studentDegreePlanRepo;
            AddDegreePlanRequest addRequest;

            [TestInitialize]
            public async void Initialize()
            {
                // Build degree plan repository
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                apiSettingsMock = new ApiSettings("null");
                studentDegreePlanRepo = await BuildValidDegreePlanRepository();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanRepo = null;
                cacheProviderMock = null;
                dataAccessorMock = null;
                transFactoryMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Add_DegreePlanNoPersonIDThrowsException()
            {
                var plan = new DegreePlan("");
                await studentDegreePlanRepo.AddAsync(plan);
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingDegreePlanException))]
            public async Task AddDegreePlan_StudentHasPlan_ThrowsExistingException()
            {
                var plan = new DegreePlan("0009999");
                await studentDegreePlanRepo.AddAsync(plan);
            }

            [TestMethod]
            [ExpectedException(typeof(RecordLockException))]
            public async Task AddDegreePlan_StudentLocked_ThrowsLockException()
            {
                var plan = new DegreePlan("0009991");
                await studentDegreePlanRepo.AddAsync(plan);
            }

            [TestMethod]
            public async Task Add_NewDegreePlanTerms_StudentWithNoPlan()
            {
                var plan = new DegreePlan("0000894");
                var newPlan = await studentDegreePlanRepo.AddAsync(plan);
                Assert.AreEqual(7, newPlan.TermIds.Count()); // this is the number of terms on the degree plan set up as the mocked response.
            }

            private async Task<StudentDegreePlanRepository> BuildValidDegreePlanRepository()
            {
                // Set up data accessor for mocking (needed for get)
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up transaction manager for mocking (needed for add and update)
                var mockManager = new Mock<IColleagueTransactionInvoker>();

                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                var testStudentDegreePlanRepository = new TestStudentDegreePlanRepository();
                // Setup repo response for getting a single plan by plan Id (plan has terms)
                var degreePlan1 = await testStudentDegreePlanRepository.GetAsync(2);
                var degreePlanResponse1 = BuildDegreePlanResponse(degreePlan1);
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", "2", true)).ReturnsAsync(degreePlanResponse1);

                // Setup repo response for getting the terms for the degree plan above.
                var degreePlanTermsResponse = BuildDegreePlanTermsResponse(degreePlan1);
                dataAccessorMock.Setup<Task<Collection<DataContracts.DegreePlanTerms>>>(acc => acc.BulkReadRecordAsync<DataContracts.DegreePlanTerms>(It.IsAny<string>(), true)).ReturnsAsync(degreePlanTermsResponse);

                Collection<DataContracts.WaitList> waitlistResponseData = new Collection<DataContracts.WaitList>();
                dataAccessorMock.Setup<Task<Collection<DataContracts.WaitList>>>(wl => wl.BulkReadRecordAsync<DataContracts.WaitList>(It.IsAny<string>(), true)).ReturnsAsync(waitlistResponseData);

                // Set up repo response for degree plan comments
                dataAccessorMock.Setup<Task<Collection<DataContracts.DegreePlanComment>>>(dpc => dpc.BulkReadRecordAsync<DataContracts.DegreePlanComment>(It.IsAny<string>(), false))
                    .ReturnsAsync(BuildDegreePlanCommentResponse(degreePlan1));

                // Set up repo response for degree plan restricted comments
                dataAccessorMock.Setup<Task<Collection<DataContracts.DegreePlanRstrCmt>>>(dpc => dpc.BulkReadRecordAsync<DataContracts.DegreePlanRstrCmt>(It.IsAny<string>(), false))
                    .ReturnsAsync(BuildDegreePlanRstrCmtResponse(degreePlan1));

                // Set up repo response for waitlist statuses
                ApplValcodes waitlistCodeResponse = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "1" },
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValActionCode1AssocMember = "2"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "D", ValActionCode1AssocMember = "3"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "P", ValActionCode1AssocMember = "4"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "5"}}
                };
                dataAccessorMock.Setup(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "WAIT.LIST.STATUSES", true)).ReturnsAsync(waitlistCodeResponse);

                // Use Callback to Capture Request built during Add (when the student has no existing plan)
                AddDegreePlanResponse updateResponse = new AddDegreePlanResponse() { DegreePlanId = "2", AErrorMessage = null };
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddDegreePlanRequest, AddDegreePlanResponse>(It.Is<AddDegreePlanRequest>(r => r.StudentId == "0000894"))).ReturnsAsync(updateResponse).Callback<AddDegreePlanRequest>(req => addRequest = req);

                // Use Callback to Capture Request built during Add (when the student has an existing plan)
                AddDegreePlanResponse updateResponse2 = new AddDegreePlanResponse() { AErrorMessage = "Student already has a plan.", ExistingDegreePlanId = "99" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddDegreePlanRequest, AddDegreePlanResponse>(It.Is<AddDegreePlanRequest>(r => r.StudentId == "0009999"))).ReturnsAsync(updateResponse2).Callback<AddDegreePlanRequest>(req => addRequest = req);

                // Use Callback to Capture Request built during Add (when the student has an existing plan)
                AddDegreePlanResponse updateResponse3 = new AddDegreePlanResponse() { AErrorMessage = "While adding degree plan a lock was found on STUDENTS record-0009991" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddDegreePlanRequest, AddDegreePlanResponse>(It.Is<AddDegreePlanRequest>(r => r.StudentId == "0009991"))).ReturnsAsync(updateResponse3).Callback<AddDegreePlanRequest>(req => addRequest = req);

                studentDegreePlanRepo = new StudentDegreePlanRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
                return studentDegreePlanRepo;
            }

            private DataContracts.DegreePlan BuildDegreePlanResponse(DegreePlan degreePlan)
            {
                DataContracts.DegreePlan degreePlanResponse = new DataContracts.DegreePlan();
                degreePlanResponse.Recordkey = degreePlan.Id.ToString();
                degreePlanResponse.DpStudentId = degreePlan.PersonId;
                degreePlanResponse.DpVersionNumber = degreePlan.Version.ToString();
                return degreePlanResponse;
            }

            private Collection<DataContracts.DegreePlanTerms> BuildDegreePlanTermsResponse(DegreePlan degreePlan)
            {
                List<DataContracts.DegreePlanTerms> degreePlanTermsList = new List<DataContracts.DegreePlanTerms>();
                foreach (var term in degreePlan.TermIds)
                {
                    DataContracts.DegreePlanTerms dpterm = new DataContracts.DegreePlanTerms();
                    dpterm.DptTerm = term;
                    dpterm.DptDegreePlan = degreePlan.Id.ToString();
                    IEnumerable<PlannedCourse> plannedCourses = degreePlan.GetPlannedCourses(term);
                    dpterm.PlannedCoursesEntityAssociation = new List<DataContracts.DegreePlanTermsPlannedCourses>();
                    foreach (var pc in plannedCourses)
                    {
                        string isAlt = "N";
                        string gradingType = null;
                        switch (pc.GradingType)
                        {
                            case GradingType.PassFail:
                                gradingType = "P";
                                break;
                            case GradingType.Audit:
                                gradingType = "A";
                                break;
                            default:
                                break;
                        }
                        dpterm.PlannedCoursesEntityAssociation.Add(new DataContracts.DegreePlanTermsPlannedCourses(pc.CourseId, pc.SectionId, pc.Credits, isAlt, gradingType, null, null, null, null));
                    }
                    degreePlanTermsList.Add(dpterm);
                }
                foreach (var npc in degreePlan.NonTermPlannedCourses)
                {
                    DataContracts.DegreePlanTerms dpterm = new DataContracts.DegreePlanTerms();
                    dpterm.DptDegreePlan = degreePlan.Id.ToString();
                    dpterm.PlannedCoursesEntityAssociation = new List<DataContracts.DegreePlanTermsPlannedCourses>();

                    string isAlt = "N";
                    string gradingType = null;
                    switch (npc.GradingType)
                    {
                        case GradingType.PassFail:
                            gradingType = "P";
                            break;
                        case GradingType.Audit:
                            gradingType = "A";
                            break;
                        default:
                            break;
                    }
                    dpterm.PlannedCoursesEntityAssociation.Add(new DataContracts.DegreePlanTermsPlannedCourses(npc.CourseId, npc.SectionId, npc.Credits, isAlt, gradingType, null, null, null, null));
                    degreePlanTermsList.Add(dpterm);
                }
                Collection<DataContracts.DegreePlanTerms> degreePlanTermsResponse = new Collection<DataContracts.DegreePlanTerms>(degreePlanTermsList);
                return degreePlanTermsResponse;
            }

            private Collection<DataContracts.DegreePlanComment> BuildDegreePlanCommentResponse(DegreePlan degreePlan)
            {
                var comments = new Collection<DataContracts.DegreePlanComment>();

                var comment1 = new DataContracts.DegreePlanComment();
                comment1.Recordkey = "1";
                comment1.DpcDegreePlan = degreePlan.Id.ToString();
                comment1.DegreePlanCommentAddopr = "0000013";
                comment1.DpcText = "This is comment number one";
                comment1.DegreePlanCommentAdddate = new DateTime(2012, 12, 27);
                comment1.DegreePlanCommentAddtime = new DateTime(2001, 01, 01, 10, 31, 01);

                comments.Add(comment1);

                var comment2 = new DataContracts.DegreePlanComment();
                comment2 = new DataContracts.DegreePlanComment();
                comment2.Recordkey = "999999";
                comment2.DpcDegreePlan = degreePlan.Id.ToString();
                comment2.DegreePlanCommentAddopr = "0000013";
                comment2.DpcText = "This is the second comment";
                comment2.DegreePlanCommentAdddate = new DateTime(2013, 01, 13);
                comment2.DegreePlanCommentAddtime = new DateTime(2001, 01, 01, 13, 29, 12);

                comments.Add(comment2);

                return comments;
            }

            private Collection<DataContracts.DegreePlanRstrCmt> BuildDegreePlanRstrCmtResponse(DegreePlan degreePlan)
            {
                var comments = new Collection<DataContracts.DegreePlanRstrCmt>();

                return comments;
            }
        }

        [TestClass]
        public class StudentDegreePlanUpdateTests
        {
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;
            StudentDegreePlanRepository studentDegreePlanRepo;
            UpdateDegreePlanRequest updateRequest;

            [TestInitialize]
            public void Initialize()
            {
                // Build degree plan repository
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                apiSettingsMock = new ApiSettings("null");

                studentDegreePlanRepo = BuildValidDegreePlanRepository();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanRepo = null;
            }

            [TestMethod]
            public async Task Update_DegreePlan()
            {
                var plan = new DegreePlan(2, "0000894", 2);
                PlannedCourse pc1 = new PlannedCourse("111", null);
                PlannedCourse pc2 = new PlannedCourse("222", null);
                plan.AddCourse(pc1, "2012/FA");
                plan.AddCourse(pc2, "2012/FA");
                var updatedPlan = await studentDegreePlanRepo.UpdateAsync(plan);
                Assert.AreEqual("2", updateRequest.DegreePlanId);
            }

            [TestMethod]
            public async Task Update_DegreePlan_with_Warnings()
            {
                var plan = new DegreePlan(67890, "0009999", 14);
                await studentDegreePlanRepo.UpdateAsync(plan);
                Assert.AreEqual("67890", updateRequest.DegreePlanId);
                loggerMock.Verify(l => l.Info(It.Is<string>(str => str.StartsWith("One or more warnings were generated when updating degree plan"))));
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task Update_DegreePlanNotFoundThrowsException()
            {
                var plan = new DegreePlan(99, "0009999", 14);
                await studentDegreePlanRepo.UpdateAsync(plan);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Update_DegreePlan_KeyNotFoundException()
            {
                var plan = new DegreePlan(12345, "0009999", 14);
                await studentDegreePlanRepo.UpdateAsync(plan);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task Update_DegreePlanLocked()
            {
                var plan = new DegreePlan(10, "0009999", 14);
                await studentDegreePlanRepo.UpdateAsync(plan);
            }

            [TestMethod]
            public async Task Update_DegreePlanUpdateRequest_Version()
            {
                var plan = new DegreePlan(2, "0000894", 3);
                PlannedCourse pc1 = new PlannedCourse("111", null);
                PlannedCourse pc2 = new PlannedCourse("222", null);
                plan.AddCourse(pc1, "2012/FA");
                plan.AddCourse(pc2, "2012/FA");
                var updatedPlan = await studentDegreePlanRepo.UpdateAsync(plan);
                Assert.AreEqual("3", updateRequest.Version);
            }

            [TestMethod]
            public async Task Update_DegreePlanUpdateRequest_TermCourses()
            {
                // Verify that the term courses are in the update request object
                var plan = new DegreePlan(2, "0000894", 3);
                var time = DateTime.Now;
                PlannedCourse pc1 = new PlannedCourse("111", null, GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "12345", time);
                PlannedCourse pc2 = new PlannedCourse("222", "2221", GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "12345", time) { IsProtected = true };
                PlannedCourse pc3 = new PlannedCourse("333", "3331", GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "12345", time) { IsProtected = false };
                pc2.Credits = 3m;
                plan.AddCourse(pc1, "2012/FA");
                plan.AddCourse(pc2, "2013/SP");
                plan.AddCourse(pc3, "2013/SP");
                var updatedPlan = await studentDegreePlanRepo.UpdateAsync(plan);
                // Check that there are two term courses, verify the data only for the second item.
                Assert.AreEqual(3, updateRequest.TermCourses.Count());
                Assert.AreEqual("2013/SP", updateRequest.TermCourses.ElementAt(1).TermIds);
                Assert.AreEqual("222", updateRequest.TermCourses.ElementAt(1).CourseIds);
                Assert.AreEqual("2221", updateRequest.TermCourses.ElementAt(1).SectionIds);
                Assert.AreEqual(3m, updateRequest.TermCourses.ElementAt(1).Credits);
                Assert.AreEqual("12345", updateRequest.TermCourses.ElementAt(1).AddedBy);
                Assert.AreEqual(time.Date, updateRequest.TermCourses.ElementAt(1).AddedOnDate.Value.Date);
                Assert.AreEqual(time, updateRequest.TermCourses.ElementAt(1).AddedOnTime);
                Assert.AreEqual("Y", updateRequest.TermCourses.ElementAt(1).Protected);
                Assert.AreEqual("", updateRequest.TermCourses.ElementAt(0).Protected);
                Assert.AreEqual("N", updateRequest.TermCourses.ElementAt(2).Protected);
            }

            [TestMethod]
            public async Task Update_DegreePlanUpdateRequest_NontermPlannedCourses()
            {
                // Verify that the nonterm planned courses are in the update request object
                var plan = new DegreePlan(2, "0000894", 3);
                var time = DateTime.Now;
                PlannedCourse pc1 = new PlannedCourse("111", "1111", GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "12345", time);
                PlannedCourse pc2 = new PlannedCourse("222", "2221", GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "12345", time) { IsProtected = true };
                pc2.Credits = 3m;
                plan.NonTermPlannedCourses.Add(pc1);
                plan.NonTermPlannedCourses.Add(pc2);
                var updatedPlan = await studentDegreePlanRepo.UpdateAsync(plan);
                // Check that there are two term courses, verify the data only for the second item.
                Assert.AreEqual(2, updateRequest.TermCourses.Count());
                Assert.AreEqual(null, updateRequest.TermCourses.ElementAt(1).TermIds);
                Assert.AreEqual("222", updateRequest.TermCourses.ElementAt(1).CourseIds);
                Assert.AreEqual("2221", updateRequest.TermCourses.ElementAt(1).SectionIds);
                Assert.AreEqual(3m, updateRequest.TermCourses.ElementAt(1).Credits);
                Assert.AreEqual("12345", updateRequest.TermCourses.ElementAt(1).AddedBy);
                Assert.AreEqual(time.Date, updateRequest.TermCourses.ElementAt(1).AddedOnDate.Value.Date);
                Assert.AreEqual(time, updateRequest.TermCourses.ElementAt(1).AddedOnTime);
                Assert.AreEqual("Y", updateRequest.TermCourses.ElementAt(1).Protected);
                Assert.AreEqual("", updateRequest.TermCourses.ElementAt(0).Protected);
            }

            [TestMethod]
            public async Task Update_DegreePlanUpdateRequest_Notes()
            {
                // Verify that the notes and restricted notes are in the update request object
                var plan = new DegreePlan(2, "0000894", 3);
                var time = DateTime.Now;
                plan.Notes.Add(new DegreePlanNote("Public Note 1"));
                plan.Notes.Add(new DegreePlanNote("Public Note 2"));
                plan.RestrictedNotes.Add(new DegreePlanNote("Restricted Note 1"));
                plan.RestrictedNotes.Add(new DegreePlanNote("Restricted Note 2"));
                var updatedPlan = await studentDegreePlanRepo.UpdateAsync(plan);
                // Check that there are two notes and 2 restricted notes, verify the data 
                Assert.AreEqual(2, updateRequest.CommentText.Count());
                Assert.AreEqual(2, updateRequest.RestrictedCommentText.Count());
                Assert.AreEqual("Public Note 1", updateRequest.CommentText.ElementAt(0));
                Assert.AreEqual("Public Note 2", updateRequest.CommentText.ElementAt(1));
                Assert.AreEqual("Restricted Note 1", updateRequest.RestrictedCommentText.ElementAt(0));
                Assert.AreEqual("Restricted Note 2", updateRequest.RestrictedCommentText.ElementAt(1));
            }

            private StudentDegreePlanRepository BuildValidDegreePlanRepository()
            {
                // Set up data accessor for mocking (needed for get)
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up transaction manager for mocking (needed for update)
                var mockManager = new Mock<IColleagueTransactionInvoker>();

                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Set up a valid Update response that returns plan with ID 2.
                // Also captures the UpdateDegreePlanRequest object built by the update method, puts its data into the updateRequest object for examination by tests
                UpdateDegreePlanResponse updatePlanResponse = new UpdateDegreePlanResponse();
                updatePlanResponse.DegreePlanId = "2";
                updatePlanResponse.AErrorMessage = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateDegreePlanRequest, UpdateDegreePlanResponse>(It.Is<UpdateDegreePlanRequest>(r => r.DegreePlanId == "2"))).ReturnsAsync(updatePlanResponse).Callback<UpdateDegreePlanRequest>(req => updateRequest = req);

                UpdateDegreePlanResponse updatePlanResponse2 = new UpdateDegreePlanResponse();
                updatePlanResponse2.DegreePlanId = "67890";
                updatePlanResponse2.AErrorMessage = null;
                updatePlanResponse2.AlWarningMessages = new List<string>() { "Warning 1", "Warning 2" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateDegreePlanRequest, UpdateDegreePlanResponse>(It.Is<UpdateDegreePlanRequest>(r => r.DegreePlanId == "67890"))).ReturnsAsync(updatePlanResponse2).Callback<UpdateDegreePlanRequest>(req => updateRequest = req);


                // Setup repo response for getting a single plan by plan Id (plan has terms)
                var degreePlanResponse1 = new DataContracts.DegreePlan();
                degreePlanResponse1.Recordkey = "2";
                degreePlanResponse1.DpStudentId = "0000894";
                degreePlanResponse1.DpVersionNumber = "1";
                dataAccessorMock.Setup<Task<DataContracts.DegreePlan>>(acc => acc.ReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", "2", true)).ReturnsAsync(degreePlanResponse1);

                var degreePlanResponse2 = new DataContracts.DegreePlan();
                degreePlanResponse2.Recordkey = "67890";
                degreePlanResponse2.DpStudentId = "0000894";
                degreePlanResponse2.DpVersionNumber = "1";
                dataAccessorMock.Setup<Task<DataContracts.DegreePlan>>(acc => acc.ReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", "67890", true)).ReturnsAsync(degreePlanResponse2);


                // Setup repo response for getting the terms for the degree plan above.
                var dpterm1 = new DataContracts.DegreePlanTerms();
                dpterm1.Recordkey = "1";
                dpterm1.DptDegreePlan = "2";
                dpterm1.DptTerm = "2012/FA";
                var degreePlanTermsResponse = new Collection<DataContracts.DegreePlanTerms>() { dpterm1 };
                dataAccessorMock.Setup<Task<Collection<DataContracts.DegreePlanTerms>>>(acc => acc.BulkReadRecordAsync<DataContracts.DegreePlanTerms>(It.Is<string>(str => str.Contains("2")), true)).ReturnsAsync(degreePlanTermsResponse);

                var dpterm2 = new DataContracts.DegreePlanTerms();
                dpterm2.Recordkey = "167890";
                dpterm2.DptDegreePlan = "67890";
                dpterm2.DptTerm = "2012/FA";
                var degreePlanTermsResponse2 = new Collection<DataContracts.DegreePlanTerms>() { dpterm2 };
                dataAccessorMock.Setup<Task<Collection<DataContracts.DegreePlanTerms>>>(acc => acc.BulkReadRecordAsync<DataContracts.DegreePlanTerms>(It.Is<string>(str => str.Contains("67890")), true)).ReturnsAsync(degreePlanTermsResponse2);

                Collection<DataContracts.WaitList> waitlistResponseData = new Collection<DataContracts.WaitList>();
                dataAccessorMock.Setup<Task<Collection<DataContracts.WaitList>>>(wl => wl.BulkReadRecordAsync<DataContracts.WaitList>(It.IsAny<string>(), true)).ReturnsAsync(waitlistResponseData);

                Collection<DataContracts.DegreePlanComment> commentResponseData = new Collection<DataContracts.DegreePlanComment>();
                commentResponseData.Add(new DataContracts.DegreePlanComment() { Recordkey = "999999", DpcText = "This is a degree plan comment with a large ID." });
                dataAccessorMock.Setup<Task<Collection<DataContracts.DegreePlanComment>>>(dc => dc.BulkReadRecordAsync<DataContracts.DegreePlanComment>(It.IsAny<string>(), false)).ReturnsAsync(commentResponseData);

                Collection<DataContracts.DegreePlanRstrCmt> rstrCmtResponseData = new Collection<DataContracts.DegreePlanRstrCmt>();
                rstrCmtResponseData.Add(new DataContracts.DegreePlanRstrCmt() { Recordkey = "999997", DprcText = "This is a degree plan restricted comment with a large ID." });
                dataAccessorMock.Setup<Task<Collection<DataContracts.DegreePlanRstrCmt>>>(dc => dc.BulkReadRecordAsync<DataContracts.DegreePlanRstrCmt>(It.IsAny<string>(), false)).ReturnsAsync(rstrCmtResponseData);

                // Set up repo response for waitlist statuses
                ApplValcodes waitlistCodeResponse = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "1" },
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValActionCode1AssocMember = "2"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "D", ValActionCode1AssocMember = "3"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "P", ValActionCode1AssocMember = "4"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "5"}}
                };
                dataAccessorMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "WAIT.LIST.STATUSES", true)).ReturnsAsync(waitlistCodeResponse);

                // Mock the response for plan update 99 to indicate version mismatch. Use Callback to Capture Request built during Update (when something goes awry)
                UpdateDegreePlanResponse updateResponse2 = new UpdateDegreePlanResponse() { DegreePlanId = "99", AErrorMessage = "Degree Plan update had version incompatibility. Update not performed." };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateDegreePlanRequest, UpdateDegreePlanResponse>(It.Is<UpdateDegreePlanRequest>(r => r.DegreePlanId == "99"))).ReturnsAsync(updateResponse2).Callback<UpdateDegreePlanRequest>(req => updateRequest = req);

                // Mock the response for plan update 10 to indicate record lock. Use Callback to Capture Request built during Update (when something goes awry)
                UpdateDegreePlanResponse updateResponse3 = new UpdateDegreePlanResponse() { DegreePlanId = "10", AErrorMessage = "Degree Plan locked." };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateDegreePlanRequest, UpdateDegreePlanResponse>(It.Is<UpdateDegreePlanRequest>(r => r.DegreePlanId == "10"))).ReturnsAsync(updateResponse3).Callback<UpdateDegreePlanRequest>(req => updateRequest = req);

                // Mock the response for plan update 12345 to indicate key not found. Use Callback to Capture Request built during Update (when something goes awry)
                UpdateDegreePlanResponse updateResponse4 = new UpdateDegreePlanResponse() { DegreePlanId = "12345", AErrorMessage = "No Degree Plan found" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateDegreePlanRequest, UpdateDegreePlanResponse>(It.Is<UpdateDegreePlanRequest>(r => r.DegreePlanId == "12345"))).ReturnsAsync(updateResponse4).Callback<UpdateDegreePlanRequest>(req => updateRequest = req);

                // Mock the response for plan update 99 to indicate warnings generated. Use Callback to Capture Request built during Update (when something goes awry)
                UpdateDegreePlanResponse updateResponse5 = new UpdateDegreePlanResponse() { DegreePlanId = "67890", AlWarningMessages = new List<string>() { "Course is invalid", "Section is invalid" } };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateDegreePlanRequest, UpdateDegreePlanResponse>(It.Is<UpdateDegreePlanRequest>(r => r.DegreePlanId == "67890"))).ReturnsAsync(updateResponse5).Callback<UpdateDegreePlanRequest>(req => updateRequest = req);


                // Set up a valid Update response that returns plan with ID 3.
                UpdateDegreePlanResponse updatePlanResponse4 = new UpdateDegreePlanResponse();
                updatePlanResponse4.DegreePlanId = "3";
                updatePlanResponse4.AErrorMessage = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateDegreePlanRequest, UpdateDegreePlanResponse>(It.Is<UpdateDegreePlanRequest>(r => r.DegreePlanId == "3"))).ReturnsAsync(updatePlanResponse).Callback<UpdateDegreePlanRequest>(req => updateRequest = req);
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateDegreePlanRequest, UpdateDegreePlanResponse>(It.Is<UpdateDegreePlanRequest>(r => r.DegreePlanId == "67890"))).ReturnsAsync(updatePlanResponse2).Callback<UpdateDegreePlanRequest>(req => updateRequest = req);
                studentDegreePlanRepo = new StudentDegreePlanRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
                return studentDegreePlanRepo;
            }
        }
    }
}
