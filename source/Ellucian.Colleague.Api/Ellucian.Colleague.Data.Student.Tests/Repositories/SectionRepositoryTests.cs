// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class SectionRepositoryTests : BaseRepositorySetup
    {
        public TestSpecialDaysRepository specialDaysTestData;


        protected void MainInitialize()
        {
            base.MockInitialize();

            specialDaysTestData = new TestSpecialDaysRepository();

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<Base.DataContracts.CampusSpecialDay>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => Task.FromResult(
                    new Collection<CampusSpecialDay>(specialDaysTestData.specialDayRecords.Where(rec => ids.Contains(rec.Recordkey)).ToList())
                ));
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "CALENDAR.DAY.TYPES", true))
                .Returns<string, string, bool>((e, r, b) => Task.FromResult(specialDaysTestData.CalendarDayTypes));

        }

        [TestClass]
        public class SectionRepository_TestAllFields : SectionRepositoryTests
        {
            /// <summary>
            /// Essentially this is a test of GetSectionAsync (which uses GetNonCachedSectionsAsync)
            /// </summary>

            protected SectionRepository sectionRepo;
            protected Mock<IStudentRepositoryHelper> stuRepoHelperMock;
            protected IStudentRepositoryHelper stuRepoHelper;
            protected CourseSections cs;
            protected CourseSecMeeting csm;
            protected CourseSecFaculty csf;
            protected Section result;
            protected CdDefaults cdDefaults;
            protected PortalSites ps;
            protected string csId;

            [TestInitialize]
            public async void Initialize()
            {
                MainInitialize();
                stuRepoHelperMock = new Mock<IStudentRepositoryHelper>();
                stuRepoHelper = stuRepoHelperMock.Object;
                csId = "12345";

                cs = new CourseSections()
                {
                    RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                    Recordkey = csId,
                    RecordModelName = "sections",
                    SecAcadLevel = "UG",
                    SecActiveStudents = new List<string>(),
                    SecAllowAuditFlag = "N",
                    SecAllowPassNopassFlag = "N",
                    SecAllowWaitlistFlag = "Y",
                    SecBookOptions = new List<string>() { "R", "O", "C" },
                    SecBooks = new List<string>() { "Book 1", "Book 2", "Book 3" },
                    SecCapacity = 30,
                    SecCeus = null,
                    SecCloseWaitlistFlag = "Y",
                    SecCourse = "210",
                    SecCourseLevels = new List<string>() { "100" },
                    SecCourseTypes = new List<string>() { "STND", "HONOR", "ICON2", "ICON3", "ICON4", "ICON5" },
                    SecCredType = "IN",
                    SecEndDate = new DateTime(2014, 12, 15),
                    SecFaculty = new List<string>(),
                    SecFacultyConsentFlag = "Y",
                    SecGradeScheme = "UGR",
                    SecGradeSubschemesId = "UGS",
                    SecInstrMethods = new List<string>() { "LEC", "LAB" },
                    SecLocation = "MAIN",
                    SecMaxCred = 6m,
                    SecMeeting = new List<string>(),
                    SecMinCred = 3m,
                    SecName = "MATH-4350-01",
                    SecNo = "01",
                    SecNoWeeks = 10,
                    SecOnlyPassNopassFlag = "N",
                    SecPortalSite = csId,
                    SecShortTitle = "Statistics",
                    SecStartDate = DateTime.Today.AddDays(-10),
                    SecTerm = "2014/FA",
                    SecTopicCode = "ABC",
                    SecVarCredIncrement = 1m,
                    SecWaitlistMax = 10,
                    SecWaitlistRating = "SR",
                    SecXlist = null,
                    SecHideInCatalog = "Y",
                    SecOtherRegBillingRates = new List<string>() { "123", "124" },
                    SecSynonym = "Synonym",
                    SecAttendTrackingType = "A", // Corresponds to HoursByDateWithoutSectionMeeting in AttendanceTrackingType enum
                    SecShowDropRosterFlag = null,
                };
                cs.SecEndDate = cs.SecStartDate.Value.AddDays(69);
                cs.SecContactEntityAssociation = new List<CourseSectionsSecContact>();
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LEC", 20.00m, 45.00m, "T", 37.50m));
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LAB", 10.00m, 15.00m, "T", 45.00m));
                cs.SecDepartmentsEntityAssociation = new List<CourseSectionsSecDepartments>();
                cs.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("MATH", 75m));
                cs.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("PSYC", 25m));
                cs.SecStatusesEntityAssociation = new List<CourseSectionsSecStatuses>();
                cs.SecStatusesEntityAssociation.Add(new CourseSectionsSecStatuses(new DateTime(2001, 5, 15), "A"));
                // Instr methods association - instructional method and load
                cs.SecContactEntityAssociation = new List<CourseSectionsSecContact>();
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LEC", 20.00m, 0m, "", 0m));
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LAB", 10.00m, 0m, "", 0m));
                // Pointer to CourseSecFaculty
                cs.SecFaculty.Add("1");
                // Pointer to CourseSecMeeting
                cs.SecMeeting.Add("1");

                BuildLdmConfiguration(dataReaderMock, out cdDefaults);

                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);

                // Set up repo response for course.sec.meeting
                csm = new CourseSecMeeting()
                {
                    Recordkey = "1",
                    CsmInstrMethod = "LEC",
                    CsmCourseSection = "12345",
                    CsmStartDate = DateTime.Today,
                    CsmEndDate = DateTime.Today.AddDays(27),
                    CsmStartTime = (new DateTime(1, 1, 1, 10, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmEndTime = (new DateTime(1, 1, 1, 11, 20, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmMonday = "Y"
                };
                MockRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", csm);

                // Set up repo response for course.sec.faculty
                csf = new CourseSecFaculty()
                {
                    Recordkey = "1",
                    CsfInstrMethod = "LEC",
                    CsfCourseSection = "12345",
                    CsfFaculty = "FAC1",
                    CsfFacultyPct = 100m,
                    CsfStartDate = cs.SecStartDate,
                    CsfEndDate = cs.SecEndDate,
                };
                MockRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", csf);

                MockRecordsAsync<CourseSecXlists>("COURSE.SEC.XLISTS", new Collection<CourseSecXlists>());
                MockRecordsAsync<CourseSecPending>("COURSE.SEC.PENDING", new Collection<CourseSecPending>());
                ps = new PortalSites() { Recordkey = csId, PsLearningProvider = "MOODLE", PsPrtlSiteGuid = csId };
                MockRecordsAsync<PortalSites>("PORTAL.SITES", new Collection<PortalSites>() { ps });
                MockRecordsAsync<WaitList>("WAIT.LIST", new Collection<WaitList>());
                MockRecordsAsync<AcadReqmts>("ACAD.REQMTS", new Collection<AcadReqmts>());

                MockRecordAsync<Dflts>("CORE.PARMS", new Dflts() { Recordkey = "DEFAULTS", DfltsCampusCalendar = "CAL" });
                // Mock data needed to read campus calendar
                var startTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 06, 00, 00);
                var endTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 23, 30, 00);
                MockRecordAsync<Data.Base.DataContracts.CampusCalendar>("CAL", new Data.Base.DataContracts.CampusCalendar() { Recordkey = "CAL", CmpcDesc = "Calendar", CmpcDayStartTime = startTime, CmpcDayEndTime = endTime, CmpcBookPastNoDays = "30", CmpcSpecialDays = specialDaysTestData.CampusSpecialDayIds });

                // Set up response for instructional methods and ST web defaults
                MockRecordsAsync<InstrMethods>("INSTR.METHODS", BuildValidInstrMethodResponse());
                // Set up repo response for GUID course types - the read record and the select
                var allCourseTypes = new TestCourseTypeRepository().Get().ToList();
                var courseTypeValcodeResponse = BuildValcodeResponse(allCourseTypes);

                dataReaderMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", It.IsAny<bool>())).ReturnsAsync(courseTypeValcodeResponse);
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(rkla =>
                {
                    var selectResult = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var rkl in rkla)
                    {
                        selectResult.Add(rkl.ResultKey, new RecordKeyLookupResult() { Guid = Guid.NewGuid().ToString().ToLowerInvariant() });
                    }
                    return Task.FromResult(selectResult);
                });


                // Set up repo response for section statuses
                var sectionStatuses = new ApplValcodes();
                sectionStatuses.ValsEntityAssociation = new List<ApplValcodesVals>();
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("A", "Active", "1", "A", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("I", "Inactive", "2", "I", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Cancelled", "", "C", "", "", ""));
                dataReaderMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", true)).ReturnsAsync(sectionStatuses);

                // Set up repo response for book options
                var bookOptions = new ApplValcodes();
                bookOptions.ValsEntityAssociation = new List<ApplValcodesVals>();
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("R", "Required", "1", "R", "", "", ""));
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Recommended", "2", "C", "", "", ""));
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("O", "Optional", "2", "O", "", "", ""));
                dataReaderMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BOOK.OPTION", true)).ReturnsAsync(bookOptions);

                // Set up repo response for reg billing rates
                Collection<RegBillingRates> rbrs = new Collection<RegBillingRates>()
                {
                    new RegBillingRates()
                    {
                        Recordkey = "123",
                        RgbrAmtCalcType = "A",
                        RgbrArCode = "ABC",
                        RgbrChargeAmt = 50m,
                        RgbrRule = "RULE1",
                    },
                    new RegBillingRates()
                    {
                        Recordkey = "124",
                        RgbrAmtCalcType = "F",
                        RgbrArCode = "DEF",
                        RgbrCrAmt = 100m
                    },
                };
                dataReaderMock.Setup<Task<Collection<RegBillingRates>>>(cacc => cacc.BulkReadRecordAsync<RegBillingRates>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(rbrs);

                //setup mocking for Stweb Defaults
                var stWebDflt = BuildStwebDefaults(); ;
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                    );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                   );

                // Mock the data readers needed for the hiding sections tests
                var regUser = new RegUsers();
                regUser.Recordkey = "REGUSERID";
                regUser.RguRegControls = new List<string>() { "REGCTLID", "OTHER" };
                dataReaderMock.Setup<Task<RegUsers>>(cacc => cacc.ReadRecordAsync<RegUsers>("REG.USERS", "REGUSERID", false)).ReturnsAsync(regUser);
                var regCtl = new RegControls();
                regCtl.Recordkey = "REGCTLID";
                regCtl.RgcSectionLookupCriteria = new List<string>() { "WITH CRS.EXTERNAL.SOURCE=''", "AND WITH SEC.COURSE.TYPES NE 'PSE'" };
                dataReaderMock.Setup<Task<RegControls>>(cacc => cacc.ReadRecordAsync<RegControls>("REG.CONTROLS", "REGCTLID", false)).ReturnsAsync(regCtl);
                // The following id is viewable in the catalog - the rest aren't
                var viewableIds = new string[] { "1" };
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(viewableIds);


                // Mock the trxn getting the waitlist status
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Mock the read of instructional methods

                InstrMethods lec = new InstrMethods()
                {
                    InmDesc = "LEC",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                    Recordkey = "LEC"
                };
                InstrMethods lab = new InstrMethods()
                {
                    InmDesc = "LAB",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                    Recordkey = "LAB"
                };
                InstrMethods onl = new InstrMethods()
                {
                    InmDesc = "ONL",
                    InmOnline = "Y",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                    Recordkey = "ONL"
                };
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));


                GetSectionWaitlistStatusResponse wlResp = new GetSectionWaitlistStatusResponse() { ErrorMessages = new List<string>(), Status = "Wlst" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetSectionWaitlistStatusRequest, GetSectionWaitlistStatusResponse>(It.IsAny<GetSectionWaitlistStatusRequest>())).ReturnsAsync(wlResp);


                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    ));
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                result.NumberOnWaitlist = 10;
                result.PermittedToRegisterOnWaitlist = 3;
                result.ReservedSeats = 5;
                result.GlobalCapacity = 40;
                result.GlobalWaitlistMaximum = 15;
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Id()
            {
                Assert.AreEqual(cs.Recordkey, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void SectionRepository_TestAllFields_IdChange()
            {
                result.Id = "2";
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Guid()
            {
                Assert.AreEqual(cs.RecordGuid, result.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void SectionRepository_TestAllFields_GuidChange()
            {
                result.Guid = Guid.NewGuid().ToString().ToLowerInvariant();
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_CourseId()
            {
                Assert.AreEqual(cs.SecCourse, result.CourseId);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Number()
            {
                Assert.AreEqual(cs.SecNo, result.Number);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_StartDate()
            {
                Assert.AreEqual(cs.SecStartDate, result.StartDate);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_MinimumCredits()
            {
                Assert.AreEqual(cs.SecMinCred, result.MinimumCredits);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Ceus()
            {
                Assert.AreEqual(cs.SecCeus, result.Ceus);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Title()
            {
                Assert.AreEqual(cs.SecShortTitle, result.Title);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Departments_Count()
            {
                Assert.AreEqual(cs.SecDepartmentsEntityAssociation.Count, result.Departments.Count);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Departments_Type()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result.Departments, typeof(OfferingDepartment));
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Departments_AcademicDepartmentCode()
            {
                CollectionAssert.AreEqual(cs.SecDepartmentsEntityAssociation.Select(x => x.SecDeptsAssocMember).ToList(),
                    result.Departments.Select(x => x.AcademicDepartmentCode).ToList());
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Departments_ResponsibilityPercentage()
            {
                Assert.AreEqual(100m, result.Departments.Sum(x => x.ResponsibilityPercentage));
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_CourseLevelCodes_Count()
            {
                Assert.AreEqual(cs.SecCourseLevels.Count, result.CourseLevelCodes.Count);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_CourseLevelCodes_Type()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result.CourseLevelCodes, cs.SecCourseLevels.AsQueryable().ElementType);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_CourseLevelCodes()
            {
                CollectionAssert.AreEqual(cs.SecCourseLevels, result.CourseLevelCodes);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_AcademicLevelCode()
            {
                Assert.AreEqual(cs.SecAcadLevel, result.AcademicLevelCode);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_AllowPassNoPass()
            {
                Assert.AreEqual(cs.SecAllowPassNopassFlag == "Y", result.AllowPassNoPass);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_AllowAudit()
            {
                Assert.AreEqual(cs.SecAllowAuditFlag == "Y", result.AllowAudit);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_OnlyPassNopass()
            {
                Assert.AreEqual(cs.SecOnlyPassNopassFlag == "Y", result.OnlyPassNoPass);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_AllowWaitlist()
            {
                Assert.AreEqual(cs.SecAllowWaitlistFlag == "Y", result.AllowWaitlist);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_IsInstructorConsentRequired()
            {
                Assert.AreEqual(cs.SecFacultyConsentFlag == "Y", result.IsInstructorConsentRequired);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_WaitlistClosed()
            {
                Assert.AreEqual(cs.SecCloseWaitlistFlag == "Y", result.WaitlistClosed);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_CreditTypeCode()
            {
                Assert.AreEqual(cs.SecCredType, result.CreditTypeCode);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Statuses_Count()
            {
                Assert.AreEqual(cs.SecStatusesEntityAssociation.Count, result.Statuses.Count);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Statuses_Type()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result.Statuses, typeof(SectionStatusItem));
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Statuses_Code()
            {
                CollectionAssert.AreEqual(cs.SecStatusesEntityAssociation.Select(x => x.SecStatusAssocMember).ToList(),
                    result.Statuses.Select(s => s.StatusCode).ToList());
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Statuses_StatusDate()
            {
                CollectionAssert.AreEqual(cs.SecStatusesEntityAssociation.Select(x => x.SecStatusDateAssocMember).ToList(),
                    result.Statuses.Select(s => s.Date).ToList());
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_GradeSchemeCode()
            {
                Assert.AreEqual(cs.SecGradeScheme, result.GradeSchemeCode);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_GradeSubschemeCode()
            {
                Assert.AreEqual(cs.SecGradeSubschemesId, result.GradeSubschemeCode);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Synonym()
            {
                Assert.AreEqual(cs.SecSynonym, result.Synonym);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_TermId()
            {
                Assert.AreEqual(cs.SecTerm, result.TermId);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_MaximumCredits()
            {
                Assert.AreEqual(cs.SecMaxCred, result.MaximumCredits);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_VariableCreditIncrement()
            {
                Assert.AreEqual(cs.SecVarCredIncrement, result.VariableCreditIncrement);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Location()
            {
                Assert.AreEqual(cs.SecLocation, result.Location);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_EndDate()
            {
                Assert.AreEqual(cs.SecEndDate, result.EndDate);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_FirstMeetingDate()
            {
                Assert.AreEqual(cs.SecFirstMeetingDate ?? cs.SecStartDate, result.FirstMeetingDate);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_LastMeetingDate()
            {
                Assert.AreEqual(cs.SecLastMeetingDate ?? cs.SecEndDate, result.LastMeetingDate);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_WaitlistRatingCode()
            {
                Assert.AreEqual(cs.SecWaitlistRating, result.WaitlistRatingCode);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Name()
            {
                Assert.AreEqual(cs.SecName, result.Name);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_FacultyIds_Count()
            {
                Assert.AreEqual(cs.SecFaculty.Count, result.FacultyIds.Count);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_FacultyIds_Type()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result.FacultyIds, typeof(string));
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Faculty_Count()
            {
                Assert.AreEqual(cs.SecFaculty.Count, result.Faculty.Count);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Faculty_Type()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result.Faculty, typeof(SectionFaculty));
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Faculty_Contents()
            {
                CollectionAssert.AreEqual(cs.SecFaculty, result.Faculty.Select(f => f.Id).ToList());
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_SectionBook_Count()
            {
                Assert.AreEqual(cs.SecBooks.Count, result.Books.Count);
                Assert.AreEqual(cs.SecBookOptions.Count, result.Books.Count);
                for (int i = 0; i < cs.SecBookOptions.Count; i++)
                {
                    if (cs.SecBookOptions[i] == "R")
                    {
                        Assert.AreEqual("R", result.Books[i].RequirementStatusCode);
                        Assert.IsTrue(result.Books[i].IsRequired);
                    }
                    else
                    {
                        Assert.IsFalse(result.Books[i].IsRequired);
                        if (cs.SecBookOptions[i] == "C")
                        {
                            Assert.AreEqual("C", result.Books[i].RequirementStatusCode);
                        }
                        else
                        {
                            Assert.AreEqual("O", result.Books[i].RequirementStatusCode);
                        }
                    }
                }
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_SectionBook_Type()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result.Books, typeof(SectionBook));
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_SectionBook_BookId()
            {
                CollectionAssert.AreEqual(cs.SecBooks, result.Books.Select(x => x.BookId).ToList());
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_SectionBook_IsRequired()
            {
                CollectionAssert.AreEqual(cs.SecBookOptions.ConvertAll(b => b == "R"), result.Books.Select(x => x.IsRequired).ToList());
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_CourseTypeCodes_Count()
            {
                Assert.AreEqual(cs.SecCourseTypes.Count, result.CourseTypeCodes.Count);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_CourseTypeCodes_Type()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result.CourseTypeCodes, cs.SecCourseTypes.AsQueryable().ElementType);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_CourseTypeCodes_Content()
            {
                CollectionAssert.AreEqual(cs.SecCourseTypes, result.CourseTypeCodes.ToList());
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_TopicCode()
            {
                Assert.AreEqual(cs.SecTopicCode, result.TopicCode);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_NumberOfWeeks()
            {
                Assert.AreEqual(cs.SecNoWeeks, result.NumberOfWeeks);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_OnlineCategory()
            {
                Assert.AreEqual(OnlineCategory.NotOnline, result.OnlineCategory);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_ActiveStudentIds_Count()
            {
                Assert.AreEqual(cs.SecActiveStudents.Count, result.ActiveStudentIds.Count);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_ActiveStudentIds_Type()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result.ActiveStudentIds, cs.SecActiveStudents.AsQueryable().ElementType);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_ActiveStudentIds_Content()
            {
                CollectionAssert.AreEqual(cs.SecActiveStudents, result.ActiveStudentIds.ToList());
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_LearningProvider()
            {
                Assert.AreEqual(ps.PsLearningProvider, result.LearningProvider);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_LearningProviderSiteId()
            {
                Assert.AreEqual(ps.PsPrtlSiteGuid, result.LearningProviderSiteId);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_NumberOnWaitList()
            {
                Assert.AreEqual(10, result.NumberOnWaitlist);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_PermittedToRegisterOnWaitlist()
            {
                Assert.AreEqual(3, result.PermittedToRegisterOnWaitlist);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_ReservedSeats()
            {
                Assert.AreEqual(5, result.ReservedSeats);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_SectionCapacity()
            {
                Assert.AreEqual(cs.SecCapacity, result.SectionCapacity);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_WaitlistMaximum()
            {
                Assert.AreEqual(cs.SecWaitlistMax, result.WaitlistMaximum);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_GlobalCapacity()
            {
                Assert.AreEqual(40, result.GlobalCapacity);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_GlobalWaitlistMaximum()
            {
                Assert.AreEqual(15, result.GlobalWaitlistMaximum);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Capacity()
            {
                Assert.AreEqual(40, result.Capacity);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Available()
            {
                Assert.AreEqual(cs.SecCapacity - cs.SecActiveStudents.Count - 5 - 3, result.Available);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_WaitlistAvailable()
            {
                Assert.IsFalse(result.WaitlistAvailable);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_Waitlisted()
            {
                Assert.AreEqual(10, result.Waitlisted);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_CurrentStatus()
            {
                Assert.AreEqual(SectionStatus.Active, result.CurrentStatus);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_IsActive()
            {
                Assert.IsTrue(result.IsActive);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_HideInCatalog_True()
            {
                Assert.IsTrue(result.HideInCatalog);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_ShowSpecialIcon_True()
            {
                // Section 12345 (csId) has COURSE.TYPE STND AND HONORS and HONORS has special processing I1. So Show Icon is true.
                Assert.IsTrue(result.ShowSpecialIcon);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_ShowSpecialIcon2_True()
            {
                // Section 12345 (csId) has COURSE.TYPE STND AND HONORS and HONORS has special processing I2. So Show Icon is true.
                Assert.IsTrue(result.ShowSpecialIcon2);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_ShowSpecialIcon3_True()
            {
                // Section 12345 (csId) has COURSE.TYPE STND AND HONORS and HONORS has special processing I3. So Show Icon is true.
                Assert.IsTrue(result.ShowSpecialIcon3);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_ShowSpecialIcon4_True()
            {
                // Section 12345 (csId) has COURSE.TYPE STND AND HONORS and HONORS has special processing I4. So Show Icon is true.
                Assert.IsTrue(result.ShowSpecialIcon4);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_ShowSpecialIcon5_True()
            {
                // Section 12345 (csId) has COURSE.TYPE STND AND HONORS and HONORS has special processing I5. So Show Icon is true.
                Assert.IsTrue(result.ShowSpecialIcon5);
            }

            [TestMethod]
            public async Task SectionRepository_TestAllFields_ShowSpecialIcon_False()
            {
                // Section 54321 (csId2) has COURSE.TYPE STND AND HONORS and HONORS has special processing I. So Show Icon is true.
                string csId2 = "54321";

                CourseSections cs2 = new CourseSections()
                {
                    RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                    Recordkey = csId2,
                    RecordModelName = "sections",
                    SecAcadLevel = "UG",
                    SecActiveStudents = new List<string>(),
                    SecAllowAuditFlag = "N",
                    SecAllowPassNopassFlag = "N",
                    SecAllowWaitlistFlag = "Y",
                    SecBookOptions = new List<string>() { "R", "O", "C" },
                    SecBooks = new List<string>() { "Book 1", "Book 2", "Book 3" },
                    SecCapacity = 30,
                    SecCeus = null,
                    SecCloseWaitlistFlag = "Y",
                    SecCourse = "210",
                    SecCourseLevels = new List<string>() { "100" },
                    SecCourseTypes = new List<string>() { "STND" },
                    SecCredType = "IN",
                    SecEndDate = new DateTime(2014, 12, 15),
                    SecFaculty = new List<string>(),
                    SecFacultyConsentFlag = "Y",
                    SecGradeScheme = "UGR",
                    SecGradeSubschemesId = "UGS",
                    SecInstrMethods = new List<string>() { "LEC", "LAB" },
                    SecLocation = "MAIN",
                    SecMaxCred = 6m,
                    SecMeeting = new List<string>(),
                    SecMinCred = 3m,
                    SecName = "MATH-4350-01",
                    SecNo = "01",
                    SecNoWeeks = 10,
                    SecOnlyPassNopassFlag = "N",
                    SecPortalSite = csId,
                    SecShortTitle = "Statistics",
                    SecStartDate = DateTime.Today.AddDays(-10),
                    SecTerm = "2014/FA",
                    SecTopicCode = "ABC",
                    SecVarCredIncrement = 1m,
                    SecWaitlistMax = 10,
                    SecWaitlistRating = "SR",
                    SecXlist = null,
                    SecHideInCatalog = "Y",
                    SecOtherRegBillingRates = new List<string>() { "123", "124" },
                    SecSynonym = "Synonym",
                    SecAttendTrackingType = "A" // Corresponds to HoursByDateWithoutSectionMeeting in AttendanceTrackingType enum
                };
                cs2.SecEndDate = cs.SecStartDate.Value.AddDays(69);
                cs2.SecContactEntityAssociation = new List<CourseSectionsSecContact>();
                cs2.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LEC", 20.00m, 45.00m, "T", 37.50m));
                cs2.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LAB", 10.00m, 15.00m, "T", 45.00m));
                cs2.SecDepartmentsEntityAssociation = new List<CourseSectionsSecDepartments>();
                cs2.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("MATH", 75m));
                cs2.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("PSYC", 25m));
                cs2.SecStatusesEntityAssociation = new List<CourseSectionsSecStatuses>();
                cs2.SecStatusesEntityAssociation.Add(new CourseSectionsSecStatuses(new DateTime(2001, 5, 15), "A"));
                // Instr methods association - instructional method and load
                cs2.SecContactEntityAssociation = new List<CourseSectionsSecContact>();
                cs2.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LEC", 20.00m, 0m, "", 0m));
                cs2.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LAB", 10.00m, 0m, "", 0m));
                // Pointer to CourseSecFaculty
                cs2.SecFaculty.Add("1");
                // Pointer to CourseSecMeeting
                cs2.SecMeeting.Add("1");

                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs2, cs2.RecordGuid);
                var result2 = await sectionRepo.GetSectionAsync(csId2);
                Assert.IsFalse(result2.ShowSpecialIcon);
            }


            [TestMethod]
            public void SectionRepository_MeetingLoadFactor_MeetingsCount()
            {
                Assert.AreEqual(cs.SecMeeting.Count, result.Meetings.Count);
            }

            [TestMethod]
            public void SectionRepository_MeetingLoadFactor_FacultyCount()
            {
                Assert.AreEqual(cs.SecFaculty.Count, result.Faculty.Count);
            }

            [TestMethod]
            public void SectionRepository_MeetingLoadFactor_Updated()
            {
                // Act - get the section
                //result = await sectionRepo.GetSectionAsync("12345");

                // Assert - verify load properly calculated and updated
                Assert.IsTrue(result.Meetings.Count() == 1);
                Assert.AreEqual(cs.SecContactEntityAssociation[0].SecInstrMethodsAssocMember, result.Meetings[0].FacultyRoster[0].InstructionalMethodCode);
                Assert.AreEqual(cs.SecContactEntityAssociation[0].SecLoadAssocMember.GetValueOrDefault(), result.Meetings[0].FacultyRoster[0].MeetingLoadFactor);
            }

            [TestMethod]
            public void SectionRepository_TotalMeetingMinutes_Updated()
            {
                // Act - get the section
                //result = await sectionRepo.GetSectionAsync("12345");

                // Assert - verify load properly calculated and updated
                int meetingTime = (int)(csm.CsmEndTime.Value - csm.CsmStartTime.Value).TotalMinutes;
                int numDays = (int)(csm.CsmEndDate.Value - csm.CsmStartDate.Value).TotalDays + 1;
                int time = meetingTime * numDays * CountMeetingDaysPerWeek(csm) / 7;
                Assert.IsTrue(result.Meetings.Count == 1);
                Assert.AreEqual(cs.SecContactEntityAssociation[0].SecInstrMethodsAssocMember, result.Meetings[0].FacultyRoster[0].InstructionalMethodCode);
                Assert.AreEqual(cs.SecContactEntityAssociation[0].SecLoadAssocMember.GetValueOrDefault(), result.Meetings[0].FacultyRoster[0].MeetingLoadFactor);
                Assert.AreEqual(time, result.Meetings[0].TotalMeetingMinutes);
            }

            [TestMethod]
            public async Task SectionRepository_SectionCharges_Null_SecOtherRegBillingRates()
            {
                cs.SecOtherRegBillingRates = null;
                dataReaderMock.Setup<Task<Collection<RegBillingRates>>>(cacc => cacc.BulkReadRecordAsync<RegBillingRates>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);

                Assert.IsFalse(result.SectionCharges.Any());
            }

            [TestMethod]
            public async Task SectionRepository_SectionCharges_Empty_SecOtherRegBillingRates()
            {
                cs.SecOtherRegBillingRates = new List<string>();
                dataReaderMock.Setup<Task<Collection<RegBillingRates>>>(cacc => cacc.BulkReadRecordAsync<RegBillingRates>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<RegBillingRates>());
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new Ellucian.Colleague.Domain.Student.Tests.TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);

                Assert.IsFalse(result.SectionCharges.Any());
            }

            [TestMethod]
            public async Task SectionRepository_SectionCharges_DataReader_returns_Null_RegBillingRates_Collection()
            {
                dataReaderMock.Setup<Task<Collection<RegBillingRates>>>(cacc => cacc.BulkReadRecordAsync<RegBillingRates>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                Assert.AreEqual(0, result.SectionCharges.Count());
            }

            [TestMethod]
            public async Task SectionRepository_SectionCharges_Invalid_SectionCharge_Skipped()
            {
                var rbrs = new Collection<RegBillingRates>()
                {
                    new RegBillingRates()
                    {
                        Recordkey = "123",
                        RgbrArCode = null,
                        RgbrChargeAmt = 50m,
                        RgbrAmtCalcType = "F",
                        RgbrRule = "RULE1"
                    },
                    new RegBillingRates()
                    {
                        Recordkey = "124",
                        RgbrArCode = "ABC",
                        RgbrChargeAmt = 50m,
                        RgbrAmtCalcType = "F",
                        RgbrRule = "RULE1"
                    }
                };
                dataReaderMock.Setup<Task<Collection<RegBillingRates>>>(cacc => cacc.BulkReadRecordAsync<RegBillingRates>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(rbrs);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                Assert.AreEqual(1, result.SectionCharges.Count);
            }

            [TestMethod]
            public async Task SectionRepository_SectionCharges_Valid()
            {
                // Set up repo response for reg billing rates
                Collection<RegBillingRates> rbrs = new Collection<RegBillingRates>()
                {
                    new RegBillingRates()
                    {
                        Recordkey = "123",
                        RgbrAmtCalcType = "A",
                        RgbrArCode = "ABC",
                        RgbrChargeAmt = 50m,
                        RgbrRule = "RULE1",
                    },
                    new RegBillingRates()
                    {
                        Recordkey = "124",
                        RgbrAmtCalcType = "F",
                        RgbrArCode = "DEF",
                        RgbrCrAmt = 100m
                    },
                };
                dataReaderMock.Setup<Task<Collection<RegBillingRates>>>(cacc => cacc.BulkReadRecordAsync<RegBillingRates>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(rbrs);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);

                Assert.AreEqual(2, result.SectionCharges.Count);
                Assert.AreEqual(rbrs[0].Recordkey, result.SectionCharges[0].Id);
                Assert.AreEqual(rbrs[0].RgbrArCode, result.SectionCharges[0].ChargeCode);
                Assert.AreEqual(rbrs[0].RgbrChargeAmt, result.SectionCharges[0].BaseAmount);
                Assert.IsFalse(result.SectionCharges[0].IsFlatFee);
                Assert.IsTrue(result.SectionCharges[0].IsRuleBased);
                Assert.AreEqual(rbrs[1].Recordkey, result.SectionCharges[1].Id);
                Assert.AreEqual(rbrs[1].RgbrArCode, result.SectionCharges[1].ChargeCode);
                Assert.AreEqual(0m - rbrs[1].RgbrCrAmt, result.SectionCharges[1].BaseAmount);
                Assert.IsTrue(result.SectionCharges[1].IsFlatFee);
                Assert.IsFalse(result.SectionCharges[1].IsRuleBased);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_AttendanceTrackingType()
            {
                Assert.AreEqual(AttendanceTrackingType.HoursByDateWithoutSectionMeeting, result.AttendanceTrackingType);
            }

            [TestMethod]
            public async Task SectionRepository_TestAllFields_ShowDropRoster_ResponseYes()
            {
                cs.SecShowDropRosterFlag = "Y";
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                var result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsTrue(result.ShowDropRoster);
            }

            [TestMethod]
            public async Task SectionRepository_TestAllFields_ShowDropRoster_ResponseLowercaseYes()
            {
                cs.SecShowDropRosterFlag = "y";
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                var result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsTrue(result.ShowDropRoster);
            }

            [TestMethod]
            public async Task SectionRepository_TestAllFields_ShowDropRoster_ResponseNo()
            {
                cs.SecShowDropRosterFlag = "N";
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                var result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsFalse(result.ShowDropRoster);
            }

            [TestMethod]
            public async Task SectionRepository_TestAllFields_ShowDropRoster_ResponseLowercaseNo()
            {
                cs.SecShowDropRosterFlag = "n";
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                var result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsFalse(result.ShowDropRoster);
            }

            [TestMethod]
            public async Task SectionRepository_TestAllFields_ShowDropRoster_ResponseInvalidValue()
            {
                cs.SecShowDropRosterFlag = "Z";
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                var result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsFalse(result.ShowDropRoster);
            }

            [TestMethod]
            public async Task SectionRepository_TestAllFields_ShowDropRoster_ResponseWhitespace()
            {
                cs.SecShowDropRosterFlag = " ";
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                var result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsFalse(result.ShowDropRoster);
            }

            [TestMethod]
            public async Task SectionRepository_TestAllFields_ShowDropRoster_ResponseEmtpy()
            {
                cs.SecShowDropRosterFlag = string.Empty;
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                var result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsFalse(result.ShowDropRoster);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_ShowDropRoster_ResponseNull()
            {
                // Section 12345 (csId) has SecShowDropRosterFlag set to null
                Assert.IsFalse(result.ShowDropRoster);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_InstructionalMethods_PickedFromSectionMeetings()
            {
                Assert.IsNotNull(result.SectionInstructionalMethods);
                Assert.AreEqual(1, result.SectionInstructionalMethods.Count);
                Assert.AreEqual("LEC", result.SectionInstructionalMethods[0].Code);

            }
            [TestMethod]
            public async Task SectionRepository_TestAllFields_InstructionalMethods_PickedFromInstructionMethods_WhenSectionMeetingsIsNull()
            {
                cs.SecMeeting = null;
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsNotNull(result.SectionInstructionalMethods);
                Assert.AreEqual(2, result.SectionInstructionalMethods.Count);
                Assert.AreEqual("LEC", result.SectionInstructionalMethods[0].Code);
                Assert.AreEqual("LAB", result.SectionInstructionalMethods[1].Code);
            }
            [TestMethod]
            public async Task SectionRepository_TestAllFields_InstructionalMethods_IsEmpty_WhenSectionMeetingsIsNull_And_WhenInstructionalMethodsIsNUll()
            {
                cs.SecMeeting = null;
                cs.SecInstrMethods = new List<string>();
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsNotNull(result.SectionInstructionalMethods);
                Assert.AreEqual(0, result.SectionInstructionalMethods.Count);
            }

            [TestMethod]
            public void SectionRepository_TestAllFields_OnlineCategory_PickedFromSectionMeetings()
            {
                //section meeting is set to LEC (which is not online)
                Assert.AreEqual(OnlineCategory.NotOnline, result.OnlineCategory);
            }
            [TestMethod]
            public async Task SectionRepository_TestAllFields_OnlineCategory_PickedFromSectionMeetings_WithSectionMeetingsAsOnline()
            {
                Collection<CourseSecMeeting> meetings = new Collection<CourseSecMeeting>();
                // Set up repo response for course.sec.meeting to online classes
                csm = new CourseSecMeeting()
                {
                    Recordkey = "1",
                    CsmInstrMethod = "ONL",
                    CsmCourseSection = "12345",
                    CsmStartDate = DateTime.Today,
                    CsmEndDate = DateTime.Today.AddDays(27),
                    CsmStartTime = (new DateTime(1, 1, 1, 10, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmEndTime = (new DateTime(1, 1, 1, 11, 20, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmMonday = "Y"
                };
                CourseSecMeeting csm2 = new CourseSecMeeting()
                {
                    Recordkey = "2",
                    CsmInstrMethod = "ONL",//ONLINE
                    CsmCourseSection = "12345",
                    CsmStartDate = DateTime.Today,
                    CsmEndDate = DateTime.Today.AddDays(20),
                    CsmStartTime = (new DateTime(1, 2, 1, 10, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmEndTime = (new DateTime(1, 2, 1, 11, 20, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmMonday = "Y"
                };
                meetings.Add(csm);
                meetings.Add(csm2);
                dataReaderMock.Setup<Task<Collection<CourseSecMeeting>>>(acc => acc.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(meetings));
                cs.SecMeeting.Add("1");
                cs.SecMeeting.Add("2");
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                Assert.AreEqual(OnlineCategory.Online, result.OnlineCategory);
            }
            [TestMethod]
            public async Task SectionRepository_TestAllFields_OnlineCategory_PickedFromSectionMeetings_WithSectionMeetingsAsHybrid()
            {
                Collection<CourseSecMeeting> meetings = new Collection<CourseSecMeeting>();
                // Set up repo response for course.sec.meeting to online classes
                csm = new CourseSecMeeting()
                {
                    Recordkey = "1",
                    CsmInstrMethod = "LEC", //LEC
                    CsmCourseSection = "12345",
                    CsmStartDate = DateTime.Today,
                    CsmEndDate = DateTime.Today.AddDays(27),
                    CsmStartTime = (new DateTime(1, 1, 1, 10, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmEndTime = (new DateTime(1, 1, 1, 11, 20, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmMonday = "Y"
                };
                CourseSecMeeting csm2 = new CourseSecMeeting()
                {
                    Recordkey = "2",
                    CsmInstrMethod = "ONL",//ONLINE,
                    CsmCourseSection = "12345",
                    CsmStartDate = DateTime.Today,
                    CsmEndDate = DateTime.Today.AddDays(20),
                    CsmStartTime = (new DateTime(1, 2, 1, 10, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmEndTime = (new DateTime(1, 2, 1, 11, 20, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmMonday = "Y"
                };
                meetings.Add(csm);
                meetings.Add(csm2);
                dataReaderMock.Setup<Task<Collection<CourseSecMeeting>>>(acc => acc.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(meetings));
                cs.SecMeeting.Add("1");
                cs.SecMeeting.Add("2");
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                Assert.AreEqual(OnlineCategory.Hybrid, result.OnlineCategory);
            }

            [TestMethod]
            public async Task SectionRepository_TestAllFields_OnlineCategory_PickedFromInstructionMethods_WhenSectionMeetingsIsNull()
            {
                cs.SecMeeting = null;
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new Ellucian.Colleague.Domain.Student.Tests.TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsNotNull(result.InstructionalMethods);
                Assert.AreEqual(2, result.InstructionalMethods.Count);
                Assert.AreEqual("LEC", result.InstructionalMethods[0]);
                Assert.AreEqual("LAB", result.InstructionalMethods[1]);
                Assert.AreEqual(OnlineCategory.NotOnline, result.OnlineCategory);
            }
            [TestMethod]
            public async Task SectionRepository_TestAllFields_OnlineCategory_PickedFromInstructionMethods_Online_WhenSectionMeetingsIsNull()
            {
                cs.SecMeeting = null;
                cs.SecInstrMethods = new List<string>() { "ONL" };
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsNotNull(result.InstructionalMethods);
                Assert.AreEqual(1, result.InstructionalMethods.Count);
                Assert.AreEqual("ONL", result.InstructionalMethods[0]);
                Assert.AreEqual(OnlineCategory.Online, result.OnlineCategory);
            }
            [TestMethod]
            public async Task SectionRepository_TestAllFields_OnlineCategory_PickedFromInstructionMethods_Hybrid_WhenSectionMeetingsIsNull()
            {
                cs.SecMeeting = null;
                cs.SecInstrMethods = new List<string>() { "ONL", "LEC" };
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsNotNull(result.InstructionalMethods);
                Assert.AreEqual(2, result.InstructionalMethods.Count);
                Assert.AreEqual("ONL", result.InstructionalMethods[0]);
                Assert.AreEqual("LEC", result.InstructionalMethods[1]);
                Assert.AreEqual(OnlineCategory.Hybrid, result.OnlineCategory);
            }
            [TestMethod]
            public async Task SectionRepository_TestAllFields_OnlineCategory_IsEmpty_WhenSectionMeetingsIsNull_And_WhenInstructionalMethodsIsNUll()
            {
                cs.SecMeeting = null;
                cs.SecInstrMethods = new List<string>();
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                result = await sectionRepo.GetSectionAsync(csId);
                Assert.IsNotNull(result.InstructionalMethods);
                Assert.AreEqual(0, result.InstructionalMethods.Count);
                Assert.AreEqual(OnlineCategory.NotOnline, result.OnlineCategory);
            }
        }

        [TestClass]
        public class SectionRepository_TestGuidLookups : SectionRepositoryTests
        {
            string id, guid, id2, guid2, id3, guid3;
            GuidLookup guidLookup;
            GuidLookupResult guidLookupResult;
            Dictionary<string, GuidLookupResult> guidLookupDict;
            RecordKeyLookup recordLookup;
            RecordKeyLookupResult recordLookupResult;
            Dictionary<string, RecordKeyLookupResult> recordLookupDict;
            SectionRepository sectionRepo;

            [TestInitialize]
            public void Initialize()
            {
                base.MainInitialize();

                // Set up for GUID lookups
                id = "12345";
                id2 = "9876";
                id3 = "0012345";

                guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                guidLookup = new GuidLookup(guid);
                guidLookupResult = new GuidLookupResult() { Entity = "COURSE.SECTIONS", PrimaryKey = id };
                guidLookupDict = new Dictionary<string, GuidLookupResult>();
                recordLookup = new RecordKeyLookup("COURSE.SECTIONS", id, false);
                recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                    {
                        if (gla.Any(gl => gl.Guid == guid))
                        {
                            guidLookupDict.Add(guid, guidLookupResult);
                        }
                        if (gla.Any(gl => gl.Guid == guid2))
                        {
                            guidLookupDict.Add(guid2, null);
                        }
                        if (gla.Any(gl => gl.Guid == guid3))
                        {
                            guidLookupDict.Add(guid3, new GuidLookupResult() { Entity = "PERSON", PrimaryKey = id3 });
                        }
                        return Task.FromResult(guidLookupDict);
                    });
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(rkla =>
                    {
                        if (rkla.Any(rkl => rkl.PrimaryKey == id))
                        {
                            recordLookupDict.Add(recordLookup.ResultKey, recordLookupResult);
                        }
                        if (rkla.Any(rkl => rkl.PrimaryKey == id2))
                        {
                            recordLookupDict.Add("COURSE.SECTIONS+" + id2, null);
                        }
                        if (rkla.Any(rkl => rkl.PrimaryKey == id3))
                        {
                            recordLookupDict.Add("PERSON+" + id3, new RecordKeyLookupResult() { Guid = guid3 });
                        }
                        return Task.FromResult(recordLookupDict);
                    });

                //setup mocking for Stweb Defaults
                var stWebDflt = BuildStwebDefaults(); ;
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, idd, repl) => Task.FromResult((stWebDflt.Recordkey == idd) ? stWebDflt : null)
                    );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (idd, repl) => Task.FromResult((stWebDflt.Recordkey == idd) ? stWebDflt : null)
                   );
                // Mock the trxn getting the waitlist status
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Mock the read of instructional methods

                InstrMethods lec = new InstrMethods()
                {
                    InmDesc = "LEC",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                    Recordkey = "LEC"
                };
                InstrMethods lab = new InstrMethods()
                {
                    InmDesc = "LAB",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                    Recordkey = "LAB"
                };
                InstrMethods onl = new InstrMethods()
                {
                    InmDesc = "ONL",
                    InmOnline = "Y",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                    Recordkey = "ONL"
                };

                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));


                GetSectionWaitlistStatusResponse wlResp = new GetSectionWaitlistStatusResponse() { ErrorMessages = new List<string>(), Status = "Wlst" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetSectionWaitlistStatusRequest, GetSectionWaitlistStatusResponse>(It.IsAny<GetSectionWaitlistStatusRequest>())).ReturnsAsync(wlResp);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
                ));
                sectionRepo = new SectionRepository(cacheProvider, transFactory, logger, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
            }

            [TestMethod]
            public async Task SectionRepository_TestGuidLookups_GuidLookupSuccess()
            {
                var result = await sectionRepo.GetSectionIdFromGuidAsync(guid);
                Assert.AreEqual(id, result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_TestGuidLookups_GuidNull()
            {
                var result = await sectionRepo.GetSectionIdFromGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_TestGuidLookups_GuidEmpty()
            {
                var result = await sectionRepo.GetSectionIdFromGuidAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRepository_TestGuidLookups_GuidLookupFailure()
            {
                var result = await sectionRepo.GetSectionIdFromGuidAsync(guid2);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRepository_TestGuidLookups_GuidLookup_KeyNotFound()
            {
                var result = await sectionRepo.GetSectionIdFromGuidAsync(guid2);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_TestGuidLookups_GuidLookupWrongFile()
            {
                var result = await sectionRepo.GetSectionIdFromGuidAsync(guid3);
            }

            [TestMethod]
            public async Task SectionRepository_TestGuidLookups_RecordKeyLookupSuccess()
            {
                var result = await sectionRepo.GetSectionGuidFromIdAsync(id);
                Assert.AreEqual(guid, result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_TestGuidLookups_RecordKeyLookupFailure_IdNull()
            {
                var result = await sectionRepo.GetSectionGuidFromIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_TestGuidLookups_RecordKeyLookupFailure_IdEmpty()
            {
                var result = await sectionRepo.GetSectionGuidFromIdAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_TestGuidLookups_RecordKeyLookupFailure()
            {
                var result = await sectionRepo.GetSectionGuidFromIdAsync(id2);
            }
        }

        [TestClass]
        public class SectionRepository_GeneralTests : SectionRepositoryTests
        {
            protected IEnumerable<Section> regSections;
            protected Dictionary<string, Section> regSectionsDict;
            protected List<Term> registrationTerms = new List<Term>();
            protected Collection<CourseSections> sectionsResponseData;
            protected Collection<CourseSecMeeting> sectionMeetingResponseData;
            protected Collection<CourseSecFaculty> sectionFacultyResponseData;
            protected List<StudentCourseSectionStudents> studentCourseSecResponseData;
            protected DeleteInstructionalEventRequest request;
            protected DeleteInstructionalEventResponse response;
            protected Collection<PortalSites> portalSitesResponseData;
            protected Collection<CourseSecXlists> crosslistResponseData;
            protected Collection<CourseSecPending> pendingResponseData;
            protected Collection<WaitList> waitlistResponseData;
            //   ApiSettings apiSettingsMock;
            protected CdDefaults cdDefaults;
            String guid;
            String id;

            protected SectionRepository sectionRepo;

            [TestInitialize]
            public async void Initialize()
            {
                base.MainInitialize();
                Term term1 = new Term("2012/FA", "Fall 2012", new DateTime(2012, 9, 1), new DateTime(2012, 12, 15), 2012, 1, true, true, "2012/FA", true);
                Term term2 = new Term("2013/SP", "Spring 2013", new DateTime(2013, 1, 1), new DateTime(2013, 5, 15), 2012, 2, true, true, "2013/SP", true);
                registrationTerms.Add(term1);
                registrationTerms.Add(term2);

                // Set up for GUID lookups
                id = "12345";
                guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();


                // Build Section responses used for mocking
                regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(registrationTerms);
                regSectionsDict = new Dictionary<string, Section>();
                foreach (var sec in regSections)
                {
                    regSectionsDict[sec.Id] = sec;
                }
                sectionsResponseData = BuildSectionsResponse(regSections);
                sectionMeetingResponseData = BuildSectionMeetingsResponse(regSections);
                sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                studentCourseSecResponseData = BuildStudentCourseSecStudents(regSections);
                portalSitesResponseData = BuildPortalSitesResponse(regSections);
                crosslistResponseData = BuildCrosslistResponse(regSections);
                pendingResponseData = BuildPendingSectionResponse(regSections);
                waitlistResponseData = BuildWaitlistResponse(regSections);

                request = new DeleteInstructionalEventRequest();
                response = new DeleteInstructionalEventResponse()
                {
                    DeleteInstructionalEventErrors = new List<DeleteInstructionalEventErrors>(),
                    DeleteInstructionalEventWarnings = new List<DeleteInstructionalEventWarnings>()
                };

                // Build section repository
                sectionRepo = BuildValidSectionRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                //localCacheMock = null;
                sectionsResponseData = null;
                regSections = null;
                sectionRepo = null;
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_ReturnsAll()
            {
                var sections = await sectionRepo.GetRegistrationSectionsAsync(registrationTerms);
                Assert.AreEqual(regSections.Count(), sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_HideInCatalog_Sections()
            {
                var expectedHiddenSections = regSections.Where(s => s.HideInCatalog);
                var sections = await sectionRepo.GetRegistrationSectionsAsync(registrationTerms);
                var actualHiddenSections = sections.Where(s => s.HideInCatalog);
                Assert.AreEqual(expectedHiddenSections.Count(), actualHiddenSections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_ReturnsNone()
            {
                var sections = await sectionRepo.GetRegistrationSectionsAsync(new List<Term>());
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_TestProperties()
            {
                IEnumerable<Section> sections = await sectionRepo.GetRegistrationSectionsAsync(registrationTerms);
                Section testSection = sections.ElementAt(0);
                Section sec = regSections.Where(s => s.Id == testSection.Id).First();
                Assert.AreEqual(testSection.AcademicLevelCode, sec.AcademicLevelCode);
                Assert.AreEqual(testSection.Ceus, sec.Ceus);
                Assert.AreEqual(testSection.CourseId, sec.CourseId);
                Assert.AreEqual(testSection.CourseLevelCodes.Count(), sec.CourseLevelCodes.Count());
                Assert.AreEqual(testSection.Departments.Count(), sec.Departments.Count());
                Assert.AreEqual(testSection.MinimumCredits, sec.MinimumCredits);
                Assert.AreEqual(testSection.MaximumCredits, sec.MaximumCredits);
                Assert.AreEqual(testSection.VariableCreditIncrement, sec.VariableCreditIncrement);
                Assert.AreEqual(testSection.Number, sec.Number);
                Assert.AreEqual(testSection.TermId, sec.TermId);
                Assert.AreEqual(testSection.StartDate, sec.StartDate);
                Assert.AreEqual(testSection.Location, sec.Location);
                Assert.AreEqual(testSection.Title, sec.Title);
                Assert.AreEqual(testSection.EndDate, sec.EndDate);
                Assert.AreEqual(testSection.AllowWaitlist, sec.AllowWaitlist);
                Assert.AreEqual(testSection.WaitlistAvailable, sec.WaitlistAvailable);
            }


            [TestMethod]
            public async Task SectionRepository_Get_CachedRegistrationSections()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "AllRegistrationSections" cache item)
                string cacheKey = sectionRepo.BuildFullCacheKey("AllRegistrationSections");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(regSectionsDict).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", "")).Returns(Task.FromResult(new string[] { "1", "2" }));

                dataReaderMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<CourseSections>>(new Collection<CourseSections>()));

                // Assert that proper course was returned
                var sections = await sectionRepo.GetRegistrationSectionsAsync(registrationTerms);
                Assert.IsTrue(sections.Count() >= 40);
                // Verify that Get was called to get the courses from cache
                cacheProviderMock.Verify(m => m.Get(cacheKey, null));
            }

            [TestMethod]
            public async Task SectionRepository_GetSingleSection_TestProperties()
            {
                List<string> courseIds = new List<string>() { "139", "42" };
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms);
                Section section = sections.Where(s => s.Id == "1").FirstOrDefault();
                Assert.AreEqual("1", section.Id);
                Assert.AreEqual("2012/FA", section.TermId);
                Assert.IsTrue(section.IsActive);
                Assert.IsTrue(section.AllowPassNoPass);
                Assert.IsTrue(section.AllowAudit);
                Assert.IsFalse(section.OnlyPassNoPass);
                Assert.IsFalse(section.AllowWaitlist);
                Assert.IsFalse(section.IsInstructorConsentRequired);
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsCached_NoSectionsFound()
            {
                IEnumerable<string> courseIds = new List<string>() { "99999", "JUNK" };
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionCached_ZeroCourseIds()
            {
                IEnumerable<string> courseIds = new List<string>();
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsCached_NullCourseIds()
            {
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsCachedAsync(null, registrationTerms);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsCached_SectionsReturned()
            {
                List<string> courseIds = new List<string>() { "139", "42" };
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms);
                Assert.AreEqual(18, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsCached_SectionMeetings()
            {
                List<string> courseIds = new List<string>() { "42" };
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms);
                var section = sections.ElementAt(0);
                var mt = section.Meetings.ElementAt(0);
                var time = mt.StartTime;
                Assert.AreEqual(1, section.Meetings.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsCached_NoAudit_NoPassFail()
            {
                List<string> courseIds = new List<string>() { "7272" };
                var section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.Number == "99").FirstOrDefault();
                Assert.IsFalse(section.AllowAudit);
                Assert.IsFalse(section.AllowPassNoPass);
                Assert.IsFalse(section.OnlyPassNoPass);
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsCached_CourseCoreqs_PreConversion()
            {
                // Check course requisites on the Section. This scenario will only happen preconversion
                // because course coreqs on the section level will be represented by requisite codes once converted.
                BuildCourseParametersUnConvertedResponse(cdDefaults);
                dataReaderMock.Setup<Task<CdDefaults>>(acc => acc.ReadRecordAsync<CdDefaults>("ST.PARMS", "CD.DEFAULTS", true)).ReturnsAsync(cdDefaults);

                List<string> courseIds = new List<string>() { "7703" };
                // Get the first section from the repo for course "7703" (they all have the same coreq setup)
                var section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).First();
                // Check that the expected course corequisites are found in the section's list of requisites
                var req1 = section.Requisites.Where(r => r.CorequisiteCourseId == "42").FirstOrDefault();
                Assert.AreEqual(true, req1.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, req1.CompletionOrder);
                var req2 = section.Requisites.Where(r => r.CorequisiteCourseId == "21").FirstOrDefault();
                Assert.AreEqual(false, req2.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, req2.CompletionOrder);
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsCached_SectionCoreqs_PostConversion()
            {
                List<string> courseIds = new List<string>() { "7703" };
                // Get the first section from the repo for course "7703" (they all have the same coreq setup)
                List<Term> terms = new List<Term>() { registrationTerms.ElementAt(0) };
                // Get a section for this course for any term
                var section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, terms)).First();

                // multi-section requisite has all sections and number needed
                var req1 = section.SectionRequisites.Where(r => r.CorequisiteSectionIds.Count() > 1).FirstOrDefault();
                Assert.IsNotNull(req1);
                Assert.AreEqual(2, req1.NumberNeeded);
                Assert.IsTrue(req1.IsRequired);
                Assert.AreEqual(3, req1.CorequisiteSectionIds.Count());
                // See TestSectionRepository, scan for "7703" to find the requisites built for all sections of this course
                var sec = (await sectionRepo.GetCourseSectionsCachedAsync(new List<string>() { "87" }, terms)).Where(s => s.Number == "02").First(); // HIST-400-02
                Assert.IsTrue(req1.CorequisiteSectionIds.Contains(sec.Id));
                sec = (await sectionRepo.GetCourseSectionsCachedAsync(new List<string>() { "154" }, terms)).Where(s => s.Number == "01").First(); // PHYS-100-01
                Assert.IsTrue(req1.CorequisiteSectionIds.Contains(sec.Id));
                sec = (await sectionRepo.GetCourseSectionsCachedAsync(new List<string>() { "333" }, terms)).Where(s => s.Number == "03").First(); // MATH-152-03
                Assert.IsTrue(req1.CorequisiteSectionIds.Contains(sec.Id));

                // Verify that the recommended section requisite has the expected data
                var reqs = section.SectionRequisites.Where(r => r.CorequisiteSectionIds.Count() == 1);
                Assert.AreEqual(2, reqs.Count());

                // Get the section cited in this requisite, verify that it is the correct section
                sec = (await sectionRepo.GetCourseSectionsCachedAsync(new List<string>() { "91" }, terms)).Where(s => s.Number == "02").First(); // MATH-400-02
                var req2 = reqs.Where(r => r.CorequisiteSectionIds.ElementAt(0) == sec.Id).FirstOrDefault();
                Assert.IsFalse(req2.IsRequired);
                Assert.AreEqual(1, req2.NumberNeeded);
                sec = (await sectionRepo.GetCourseSectionsCachedAsync(new List<string>() { "159" }, terms)).Where(s => s.Number == "01").First(); // MATH-152-03
                req2 = reqs.Where(r => r.CorequisiteSectionIds.ElementAt(0) == sec.Id).FirstOrDefault();
                Assert.IsFalse(req2.IsRequired);
                Assert.AreEqual(1, req2.NumberNeeded);

            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsCached_SectionCoreqs_PreConversion()
            {
                BuildCourseParametersUnConvertedResponse(cdDefaults);
                dataReaderMock.Setup<Task<CdDefaults>>(acc => acc.ReadRecordAsync<CdDefaults>("ST.PARMS", "CD.DEFAULTS", true)).ReturnsAsync(cdDefaults);

                List<string> courseIds = new List<string>() { "7703" };
                // Get the first section from the repo for course "7703" (they all have the same coreq setup)
                List<Term> terms = new List<Term>() { registrationTerms.ElementAt(0) };
                // Get a section for this course for any term
                var section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, terms)).First();

                // Preconversion, an individual section requisite is created for each item, because there is no such concept as "2 out of 3" yet.
                // verify that the required section requisite has the expected data
                var secReqs = section.SectionRequisites.Where(r => r.CorequisiteSectionIds != null);

                // Three individual required section requisites
                var sec = (await sectionRepo.GetCourseSectionsCachedAsync(new List<string>() { "87" }, terms)).Where(s => s.Number == "02").First(); // HIST-400-02
                var req1 = secReqs.Where(r => r.CorequisiteSectionIds.Contains(sec.Id)).FirstOrDefault();
                Assert.IsTrue(req1.IsRequired);
                Assert.AreEqual(1, req1.NumberNeeded);

                sec = (await sectionRepo.GetCourseSectionsCachedAsync(new List<string>() { "154" }, terms)).Where(s => s.Number == "01").First(); // PHYS-100-01
                req1 = secReqs.Where(r => r.CorequisiteSectionIds.Contains(sec.Id)).FirstOrDefault();
                Assert.IsTrue(req1.IsRequired);
                Assert.AreEqual(1, req1.NumberNeeded);

                sec = (await sectionRepo.GetCourseSectionsCachedAsync(new List<string>() { "333" }, terms)).Where(s => s.Number == "03").First(); // MATH-152-03
                req1 = secReqs.Where(r => r.CorequisiteSectionIds.Contains(sec.Id)).FirstOrDefault();
                Assert.IsTrue(req1.IsRequired);
                Assert.AreEqual(1, req1.NumberNeeded);

                // Two Individual recommended section requisites
                sec = (await sectionRepo.GetCourseSectionsCachedAsync(new List<string>() { "91" }, terms)).Where(s => s.Number == "02").First(); // MATH-400-02
                req1 = secReqs.Where(r => r.CorequisiteSectionIds.ElementAt(0) == sec.Id).FirstOrDefault();
                Assert.IsFalse(req1.IsRequired);
                Assert.AreEqual(1, req1.NumberNeeded);

                sec = (await sectionRepo.GetCourseSectionsCachedAsync(new List<string>() { "159" }, terms)).Where(s => s.Number == "01").First(); // SOCI-100-01
                req1 = secReqs.Where(r => r.CorequisiteSectionIds.ElementAt(0) == sec.Id).FirstOrDefault();
                Assert.IsFalse(req1.IsRequired);
                Assert.AreEqual(1, req1.NumberNeeded);
            }

            [TestMethod]
            public async Task SectionRepository_IncludesAcadReqmtReqsWhenOverrideIsTrue()
            {
                // This section set up with a requisite with a requirement code and override set to true. Verify requisite is included
                List<string> courseIds = new List<string>() { "7706" };
                var section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.Number == "04").First();
                // Check that the completion order and required flags are converted properly, based on the ACAD.REQMTS response.
                var req = section.Requisites.Where(r => r.RequirementCode == "PREREQ1").FirstOrDefault();
                Assert.AreEqual(true, req.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.Previous, req.CompletionOrder);
                req = section.Requisites.Where(r => r.RequirementCode == "COREQ2").FirstOrDefault();
                Assert.AreEqual(false, req.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.Concurrent, req.CompletionOrder);
                req = section.Requisites.Where(r => r.RequirementCode == "REQ1").FirstOrDefault();
                Assert.AreEqual(true, req.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, req.CompletionOrder);
            }

            [TestMethod]
            public async Task SectionRepository_IgnoresAcadReqmtReqsWhenOverrideIsFalse()
            {
                // This section set up with a requisite with a requirement code and override set to false. Verify requisite is ignored
                List<string> courseIds = new List<string>() { "7706" };
                var section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.Number == "05").First();
                var req = section.Requisites.Where(r => !string.IsNullOrEmpty(r.RequirementCode)).FirstOrDefault();
                Assert.IsNull(req);
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionCached_Books()
            {
                List<string> courseIds = new List<string>() { "7702" };
                var section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).First();
                Assert.AreEqual(2, section.Books.Count());
                Assert.AreEqual("111", section.Books.ElementAt(0).BookId);
                Assert.AreEqual("222", section.Books.ElementAt(1).BookId);
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsCached_CourseTypes()
            {
                List<string> courseIds = new List<string>() { "42" };
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms);
                var section = sections.ElementAt(0);
                Assert.AreEqual("STND", section.CourseTypeCodes.ElementAt(0));
                Assert.AreEqual("WR", section.CourseTypeCodes.ElementAt(1));
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_ActiveStudents()
            {
                IEnumerable<Section> sections = await sectionRepo.GetRegistrationSectionsAsync(null);
                foreach (Section section in sections)
                {
                    if (Int16.Parse(section.Id) % 2 == 0)
                    {
                        Assert.AreEqual(section.ActiveStudentIds.Count(), 1);
                    }
                    else
                    {
                        Assert.AreEqual(section.ActiveStudentIds.Count(), 0);
                    }
                }
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionCached_NoPtlSite()
            {
                List<string> courseIds = new List<string>() { "7272" };
                Section section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.Number == "99").First();
                Assert.IsNull(section.LearningProviderSiteId);
                Assert.IsNull(section.LearningProvider);
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionCached_PtlSiteNoLearningProvider()
            {
                List<string> courseIds = new List<string>() { "7272" };
                Section section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.Number == "98").First();
                Assert.AreEqual("SHAREPOINT", section.LearningProvider);
                Assert.AreEqual(section.Id, section.LearningProviderSiteId);
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionCached_PtlSiteWithLearningProvider()
            {
                List<string> courseIds = new List<string>() { "7272" };
                Section section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.Number == "97").First();
                Assert.AreEqual("MOODLE", section.LearningProvider);
                Assert.AreEqual(section.Id, section.LearningProviderSiteId);
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionCached_NoXlist()
            {
                List<string> courseIds = new List<string>() { "7702" };
                Section section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).First();
                Assert.IsNull(section.PrimarySectionId);
                Assert.AreEqual(0, section.CrossListedSections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionCached_PrimaryInXlist()
            {
                List<string> courseIds = new List<string>() { "7272" };
                Section section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.Number == "98").First();
                Assert.AreEqual(section.PrimarySectionId, section.Id);
                Assert.AreEqual(2, section.CrossListedSections.Count());
            }
            [TestMethod]
            public async Task SectionRepository_GetCourseSectionCached_CrossListedAttendanceTrackingTypes()
            {
                // Primary section should have HoursBysectionMeeting
                List<string> courseIds = new List<string>() { "7272" };
                Section section = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.Number == "98").First();
                Assert.AreEqual(AttendanceTrackingType.HoursBySectionMeeting, section.AttendanceTrackingType);
                // Secondary section should follow the primary
                courseIds = new List<string>() { "139" };
                Section secondarySection = (await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.Number == "01").First();
                Assert.AreEqual(AttendanceTrackingType.HoursBySectionMeeting, secondarySection.AttendanceTrackingType);
            }

            [TestMethod]
            public async Task SectionRepository_HandlesMissingINTERNATIONALParam()
            {
                // Should return sections without error.
                var param = new Data.Base.DataContracts.IntlParams();
                param = null;
                dataReaderMock.Setup<Task<Data.Base.DataContracts.IntlParams>>(iacc => iacc.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(param);
                List<string> courseIds = new List<string>() { "42" };
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsCachedAsync(courseIds, registrationTerms);
                Assert.AreEqual(9, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCachedSections_SectionIdsNull()
            {
                IEnumerable<Section> sections = await sectionRepo.GetCachedSectionsAsync(null);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCachedSections_SectionIdsZero()
            {
                IEnumerable<string> sectionIds = new List<string>();
                IEnumerable<Section> sections = await sectionRepo.GetCachedSectionsAsync(sectionIds);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCachedSections_Successful()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "AllRegistrationSections" cache item)
                string cacheKey = sectionRepo.BuildFullCacheKey("AllRegistrationSections");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(regSectionsDict).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", "")).Returns(Task.FromResult(new string[] { "1", "2" }));
                dataReaderMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<CourseSections>>(new Collection<CourseSections>()));

                // Assert sections are returned
                IEnumerable<string> sectionIds = new List<string> { "1", "2" };
                var sections = await sectionRepo.GetCachedSectionsAsync(sectionIds);
                Assert.AreEqual(2, sections.Count());
                // Verify that Get was called to get the courses from cache
                cacheProviderMock.Verify(m => m.Get(cacheKey, null));
            }

            [TestMethod]
            public async Task SectionRepository_GetCachedSections_GetsSectionFromArchivedSectionCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item from the archive cache
                var recordKey = "99999";
                string cacheKey = "ArchivedSection" + recordKey;
                string fullCacheKey = sectionRepo.BuildFullCacheKey(cacheKey);
                cacheProviderMock.Setup(x => x.Contains(fullCacheKey, null)).Returns(true);
                var sec = new Section(recordKey, "1", "01", new DateTime(2012, 1, 1), 3m, 0m, "course1", "IN", new List<OfferingDepartment>() { new OfferingDepartment("HIST", 100m) }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) });
                cacheProviderMock.Setup(x => x.Get(fullCacheKey, null)).Returns(sec).Verifiable();
                cacheProviderMock.Setup(x => x.Add(fullCacheKey, It.IsAny<Object>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Act--Execute repo method to get cached section. We know this one is not a registration section.
                IEnumerable<string> sectionIds = new List<string> { recordKey };
                var sections = await sectionRepo.GetCachedSectionsAsync(sectionIds);

                // Assert--the requested section is returned
                Assert.AreEqual(1, sections.Count());
                Assert.AreEqual(recordKey, sections.ElementAt(0).Id);
                // Verify that Get was called with the section ID to get the section from the archive cache
                cacheProviderMock.Verify(x => x.Get(fullCacheKey, null));
                // Verify that the Add was called with the section to update the cache
                cacheProviderMock.Verify(x => x.Add(fullCacheKey, It.IsAny<Object>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task SectionRepository_GetCachedSections_GetsSectionFromDatabaseIfNotInArchivedSectionCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                var recordKey = "99999";
                string fullCacheKey = sectionRepo.BuildFullCacheKey("ArchivedSection" + recordKey);
                cacheProviderMock.Setup(x => x.Contains(fullCacheKey, null)).Returns(false);

                // Mock return of a DataContract CourseSection from bulk read 
                CourseSections cs = sectionsResponseData.Where(s => s.Recordkey == "1").First();
                cs.Recordkey = recordKey; // Insert the recordkey we want
                dataReaderMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(new Collection<CourseSections>() { cs }));

                // Set up mocking to verify the section was written to the archive cache
                cacheProviderMock.Setup(x => x.Add(fullCacheKey, It.IsAny<Section>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Act--Execute repo method to get cached section. We know this one is not a registration section.
                IEnumerable<string> sectionIds = new List<string> { recordKey };
                var sections = await sectionRepo.GetCachedSectionsAsync(sectionIds);

                // Assert--the requested section is returned
                Assert.AreEqual(1, sections.Count());
                Assert.AreEqual(recordKey, sections.ElementAt(0).Id);

                // Verify that the section was also added to the cache
                cacheProviderMock.Verify(m => m.Add(fullCacheKey, It.IsAny<Section>(), It.IsAny<CacheItemPolicy>(), null));
            }

            // TODO: SSS This test broken by Async changes, needs to be resolved
            [TestMethod]
            public async Task SectionRepository_GetCachedSections_GetsSectionFromDatabase_OnlyOnline()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                var recordKey = "99999";
                string fullCacheKey = sectionRepo.BuildFullCacheKey("ArchivedSection" + recordKey);
                cacheProviderMock.Setup(x => x.Contains(fullCacheKey, null)).Returns(false);

                // Mock return of a DataContract CourseSection from bulk read 
                CourseSections cs = sectionsResponseData.Where(s => s.Recordkey == "1").First();
                cs.Recordkey = recordKey; // Insert the recordkey we want
                dataReaderMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(new Collection<CourseSections>() { cs }));

                // Mock up appropriate SectionMeetings
                Collection<CourseSecMeeting> courseSecMeetings = new Collection<CourseSecMeeting>();
                courseSecMeetings.Add(new CourseSecMeeting() { Recordkey = "1", CsmInstrMethod = "ONL", CsmFriday = "Y", CsmCourseSection = recordKey, CsmFrequency = "W", CsmStartDate = new DateTime(2012, 9, 1), CsmEndDate = new DateTime(2012, 12, 12) });
                dataReaderMock.Setup<Task<Collection<CourseSecMeeting>>>(acc => acc.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(courseSecMeetings));


                // Set up mocking to verify the section was written to the archive cache
                cacheProviderMock.Setup(x => x.Add(fullCacheKey, It.IsAny<Section>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Act--Execute repo method to get cached section. We know this one is not a registration section.
                IEnumerable<string> sectionIds = new List<string> { recordKey };
                var sections = await sectionRepo.GetCachedSectionsAsync(sectionIds);

                // Assert--the requested section is online
                Assert.AreEqual(1, sections.Count());
                var section = sections.ElementAt(0);
                Assert.AreEqual(OnlineCategory.Online, section.OnlineCategory);
                Assert.IsTrue(section.Meetings[0].IsOnline);
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_NullFacultyDataResponse_SectionsReturned()
            {
                // Mock null faculty repo response
                sectionFacultyResponseData = null;
                dataReaderMock.Setup<Task<Collection<CourseSecFaculty>>>(facc => facc.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionFacultyResponseData));
                IEnumerable<Section> sections = await sectionRepo.GetRegistrationSectionsAsync(registrationTerms);
                Section testSection = sections.ElementAt(0);
                Assert.IsNotNull(testSection);
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_NullRosterDataResponse_SectionsReturned()
            {
                // Mock null roster response
                studentCourseSecResponseData = null;

                dataReaderMock.Setup(ac => ac.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), "SAVING SCS.COURSE.SECTION")).ReturnsAsync(() => null);
                dataReaderMock.Setup(ac => ac.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), "SAVING SCS.STUDENT")).ReturnsAsync(() => null);
                IEnumerable<Section> sections = await sectionRepo.GetRegistrationSectionsAsync(registrationTerms);
                Section testSection = sections.ElementAt(0);
                Assert.IsNotNull(testSection);
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_NullPendingDataResponse_SectionsReturned()
            {
                // Mock null pending data response
                pendingResponseData = null;
                dataReaderMock.Setup<Task<Collection<CourseSecPending>>>(csp => csp.BulkReadRecordAsync<CourseSecPending>("COURSE.SEC.PENDING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(pendingResponseData));
                IEnumerable<Section> sections = await sectionRepo.GetRegistrationSectionsAsync(registrationTerms);
                Section testSection = sections.ElementAt(0);
                Assert.IsNotNull(testSection);
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_NullWaitlistDataReponse_SectionsReturned()
            {
                // Mock null waitlist data response
                waitlistResponseData = null;
                dataReaderMock.Setup<Task<Collection<WaitList>>>(wl => wl.BulkReadRecordAsync<WaitList>("WAIT.LIST", It.IsAny<string>(), true)).Returns(Task.FromResult(waitlistResponseData));
                IEnumerable<Section> sections = await sectionRepo.GetRegistrationSectionsAsync(registrationTerms);
                Section testSection = sections.ElementAt(0);
                Assert.IsNotNull(testSection);
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_NullPortalDataResponse_SectionsReturned()
            {
                // Mock null portal site response
                portalSitesResponseData = null;
                dataReaderMock.Setup<Task<Collection<PortalSites>>>(ps => ps.BulkReadRecordAsync<PortalSites>("PORTAL.SITES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(portalSitesResponseData));
                IEnumerable<Section> sections = await sectionRepo.GetRegistrationSectionsAsync(registrationTerms);
                Section testSection = sections.ElementAt(0);
                Assert.IsNotNull(testSection);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionMeetingAsync_NullException()
            {
                await sectionRepo.GetSectionMeetingAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRepository_GetSectionMeetingAsync_KeyNotFoundException()
            {
                await sectionRepo.GetSectionMeetingAsync("dsdsa");
            }

            [TestMethod]
            public async Task GetCachedSections_GetsChangedRegistrationSections()
            {
                // Get the list of registration sections from cache
                var allCacheKey = sectionRepo.BuildFullCacheKey("AllRegistrationSections");
                var cacheDateKey = sectionRepo.BuildFullCacheKey("AllRegistrationSectionsCacheDate");
                // Create a small dict to be returned as "AllRegistrationSections".
                var sectionsDict = new Dictionary<string, Section>();
                sectionsDict["2"] = regSections.Where(s => s.Id == "2").First();
                sectionsDict["3"] = regSections.Where(s => s.Id == "3").First();
                sectionsDict["4"] = regSections.Where(s => s.Id == "4").First();
                var testLocation = "SOUTH";
                sectionsDict["4"].Location = testLocation; // Override original name so we can make sure it was overlaid.
                                                           // Set up the response from the all registration sections cache
                cacheProviderMock.Setup(x => x.Contains(allCacheKey, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(allCacheKey, null)).Returns(sectionsDict);
                // Set up the response of the cache date
                cacheProviderMock.Setup(x => x.Contains(cacheDateKey, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(cacheDateKey, null)).Returns(DateTime.Now);
                // Set up ChangedRegistrationsCache to respond that it does ont exist so it will have to be built.
                string changedCacheKey = sectionRepo.BuildFullCacheKey("ChangedRegistrationSections");
                cacheProviderMock.Setup(x => x.Contains(changedCacheKey, null)).Returns(false);
                // Mock return of a course section Id from the select for changed course sections
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).Returns(Task.FromResult(new List<string>() { "4", "9" }.ToArray()));
                // Then noncached sections is called and should take care of getting sections "4" and "9".
                sectionsResponseData = new Collection<CourseSections>(sectionsResponseData.Where(s => s.Recordkey == "4" || s.Recordkey == "9").ToList());
                dataReaderMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionsResponseData));

                // Get the current DateTime, for use in later Asserts
                var beforeUpdate = DateTime.Now;
                Thread.Sleep(2000);

                // Act--Execute repo method to get registration sections
                var sections = await sectionRepo.GetRegistrationSectionsAsync(registrationTerms);

                // Assert--the original 3 sections, one overlaid (4) and one new one(9).
                Assert.AreEqual(4, sections.Count());
                Assert.IsNotNull(sections.Where(s => s.Id == "2").First());
                Assert.IsNotNull(sections.Where(s => s.Id == "3").First());
                var section4 = sections.Where(s => s.Id == "4").First();
                Assert.IsNotNull(section4);
                Assert.AreEqual("MAIN", section4.Location);
                Assert.IsNotNull(sections.Where(s => s.Id == "9").First());

                // Assert that the DateTime the changedRegistrationCache was updated is more recent than beforeUpdate
                var afterUpdate = sectionRepo.GetChangedRegistrationSectionsCacheBuildTime();
                Assert.IsTrue(beforeUpdate < afterUpdate);
            }

            [TestMethod]
            public async Task SectionRepository_TestGuidLookupsforSectionMeeting_GuidLookupSuccess()
            {
                var result = await sectionRepo.GetSectionMeetingByGuidAsync(guid);
                Assert.AreEqual(id, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_TestGuidLookupsforSectionMeeting_RecordKeyLookupFailure_IdNull()
            {
                var result = await sectionRepo.GetSectionMeetingByGuidAsync(null);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionMeeting()
            {
                CourseSecMeeting csm = sectionMeetingResponseData.FirstOrDefault();
                CourseSecFaculty csf = sectionFacultyResponseData.FirstOrDefault();

                var tuple = await sectionRepo.GetSectionMeetingAsync(10, 10, csm.CsmCourseSection, csm.CsmStartDate.ToString(), csm.CsmEndDate.ToString(), csm.CsmStartTime.ToString(), csm.CsmEndTime.ToString(), new List<string>() { csm.CsmBldg }, new List<string>() { csm.CsmRoom }, new List<string>() { csf.CsfFaculty }, "");
                IEnumerable<SectionMeeting> secMeets = tuple.Item1;
                Assert.AreEqual(csm.CsmCourseSection, secMeets.FirstOrDefault().SectionId);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionMeeting2Async()
            {
                CourseSecMeeting csm = sectionMeetingResponseData.FirstOrDefault();
                CourseSecFaculty csf = sectionFacultyResponseData.FirstOrDefault();

                var tuple = await sectionRepo.GetSectionMeetingAsync(10, 10, csm.CsmCourseSection, csm.CsmStartDate.ToString(), csm.CsmEndDate.ToString(), csm.CsmStartTime.ToString(), csm.CsmEndTime.ToString(), new List<string>(), new List<string>(), new List<string>(), "2017/FA");
                IEnumerable<SectionMeeting> secMeets = tuple.Item1;
                Assert.AreEqual(csm.CsmCourseSection, secMeets.FirstOrDefault().SectionId);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionMeeting2Async_NoPaging()
            {
                CourseSecMeeting csm = sectionMeetingResponseData.FirstOrDefault();
                CourseSecFaculty csf = sectionFacultyResponseData.FirstOrDefault();

                var tuple = await sectionRepo.GetSectionMeetingAsync(0, 0, csm.CsmCourseSection, csm.CsmStartDate.ToString(), csm.CsmEndDate.ToString(), csm.CsmStartTime.ToString(), csm.CsmEndTime.ToString(), new List<string>(), new List<string>(), new List<string>(), "2017/FA");
                IEnumerable<SectionMeeting> secMeets = tuple.Item1;
                Assert.AreEqual(csm.CsmCourseSection, secMeets.FirstOrDefault().SectionId);
            }

            private SectionRepository BuildValidSectionRepository()
            {

                BuildLdmConfiguration(dataReaderMock, out cdDefaults);

                // Set up repo response for "all" section requests
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).Returns(Task.FromResult(sectionsResponseData.Select(c => c.Recordkey).ToArray()));
                dataReaderMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionsResponseData));

                // Set up repo response for single section request
                CourseSections cs = sectionsResponseData.Where(s => s.Recordkey == "1").First();
                dataReaderMock.Setup<Task<CourseSections>>(acc => acc.ReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string>(), true)).Returns(Task.FromResult(cs));
                CourseSecXlists csxl = crosslistResponseData.Where(s => s.CsxlCourseSections.Contains("1")).First();
                dataReaderMock.Setup<Task<CourseSecXlists>>(acc => acc.ReadRecordAsync<CourseSecXlists>("COURSE.SEC.XLISTS", It.IsAny<string>(), true)).Returns(Task.FromResult(csxl));

                // Set up repo response for filters on section or term
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), "WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING")).ReturnsAsync(sectionMeetingResponseData.Select(c => c.Recordkey).ToArray());
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", "WITH TERM = '2012/FA' WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING")).ReturnsAsync(sectionMeetingResponseData.Select(c => c.Recordkey).ToArray());
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", "WITH TERM = '2013/SP' WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING")).ReturnsAsync(sectionMeetingResponseData.Select(c => c.Recordkey).ToArray());
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", "WITH TERM = '2014/FA' WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING")).ReturnsAsync(sectionMeetingResponseData.Select(c => c.Recordkey).ToArray());
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", "WITH TERM = '2017/FA' WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING")).ReturnsAsync(sectionMeetingResponseData.Select(c => c.Recordkey).ToArray());

                // Set up repo response for "all" meeting requests
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SEC.MEETING", It.IsAny<string>())).Returns(Task.FromResult(sectionMeetingResponseData.Select(c => c.Recordkey).ToArray()));
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<string>())).Returns(Task.FromResult(sectionMeetingResponseData.Select(c => c.Recordkey).ToArray()));
                dataReaderMock.Setup<Task<Collection<CourseSecMeeting>>>(macc => macc.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionMeetingResponseData));
                dataReaderMock.Setup<Task<Collection<CourseSecMeeting>>>(macc => macc.BulkReadRecordAsync<CourseSecMeeting>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionMeetingResponseData));
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<CourseSecMeeting>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionMeetingResponseData);
                // Set up repo response for "all" faculty
                dataReaderMock.Setup<Task<Collection<CourseSecFaculty>>>(facc => facc.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionFacultyResponseData));
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sectionFacultyResponseData.Select(c => c.CsfCourseSection).ToArray());

                dataReaderMock.Setup(ac => ac.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), "SAVING SCS.COURSE.SECTION")).ReturnsAsync(studentCourseSecResponseData.Select(c => c.CourseSectionIds).ToArray());
                dataReaderMock.Setup(ac => ac.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), "SAVING SCS.STUDENT")).ReturnsAsync(studentCourseSecResponseData.Select(c => c.StudentIds).ToArray());

                transManagerMock.Setup(t => t.ExecuteAsync<DeleteInstructionalEventRequest, DeleteInstructionalEventResponse>(It.IsAny<DeleteInstructionalEventRequest>())).Returns(Task.FromResult(response));

                dataReaderMock.Setup<Task<Collection<PortalSites>>>(ps => ps.BulkReadRecordAsync<PortalSites>("PORTAL.SITES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(portalSitesResponseData));
                dataReaderMock.Setup<Task<Collection<CourseSecXlists>>>(sxl => sxl.BulkReadRecordAsync<CourseSecXlists>("COURSE.SEC.XLISTS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(crosslistResponseData));
                dataReaderMock.Setup<Task<Collection<CourseSecPending>>>(csp => csp.BulkReadRecordAsync<CourseSecPending>("COURSE.SEC.PENDING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(pendingResponseData));
                dataReaderMock.Setup<Task<Collection<WaitList>>>(wl => wl.BulkReadRecordAsync<WaitList>("WAIT.LIST", It.IsAny<string>(), true)).Returns(Task.FromResult(waitlistResponseData));

                // Set up repo response for the temporary international parameter item
                Data.Base.DataContracts.IntlParams intlParams = new Data.Base.DataContracts.IntlParams();
                intlParams.HostDateDelimiter = "/";
                intlParams.HostShortDateFormat = "MDY";
                dataReaderMock.Setup<Task<Data.Base.DataContracts.IntlParams>>(iacc => iacc.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);
                // Setup localCacheMock as the object for the CacheProvider

                // Set up course defaults response (indicates if coreq conversion has taken place)
                BuildCourseParametersConvertedResponse(cdDefaults);
                dataReaderMock.Setup<Task<CdDefaults>>(acc => acc.ReadRecordAsync<CdDefaults>("ST.PARMS", "CD.DEFAULTS", true)).ReturnsAsync(cdDefaults);

                // Set up acad reqmts response (for section requisite codes)
                var acadReqmtsResponse = BuildAcadReqmtsResponse();
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(acadReqmtsResponse));

                //// Set up response for instructional methods
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<InstrMethods>("INSTR.METHODS", "", true)).ReturnsAsync(BuildValidInstrMethodResponse());

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                // Mock data needed to read campus calendar
                var startTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 06, 00, 00);
                var endTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 23, 30, 00);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true))
                    .ReturnsAsync(new Dflts() { DfltsCampusCalendar = "CAL" });
                dataReaderMock.Setup(r => r.ReadRecordAsync<Data.Base.DataContracts.CampusCalendar>("CAL", true))
                    .ReturnsAsync(new Data.Base.DataContracts.CampusCalendar() { Recordkey = "CAL", CmpcDesc = "Calendar", CmpcDayStartTime = startTime, CmpcDayEndTime = endTime, CmpcBookPastNoDays = "30", CmpcSpecialDays = specialDaysTestData.CampusSpecialDayIds });
                // Set up repo response for section statuses
                var sectionStatuses = new ApplValcodes();
                sectionStatuses.ValsEntityAssociation = new List<ApplValcodesVals>();
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("A", "Active", "1", "A", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("I", "Inactive", "2", "I", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Cancelled", "", "C", "", "", ""));
                dataReaderMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", true)).ReturnsAsync(sectionStatuses);

                // Set up repo response for book options
                var bookOptions = new ApplValcodes();
                bookOptions.ValsEntityAssociation = new List<ApplValcodesVals>();
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("R", "Required", "1", "R", "", "", ""));
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Recommended", "2", "C", "", "", ""));
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("O", "Optional", "2", "O", "", "", ""));
                dataReaderMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BOOK.OPTION", true)).ReturnsAsync(bookOptions);

                // Set up repo response for reg billing rates
                Collection<RegBillingRates> rbrs = new Collection<RegBillingRates>()
                {
                    new RegBillingRates()
                    {
                        Recordkey = "123",
                        RgbrAmtCalcType = "A",
                        RgbrArCode = "ABC",
                        RgbrChargeAmt = 50m,
                        RgbrRule = "RULE1",
                    },
                    new RegBillingRates()
                    {
                        Recordkey = "124",
                        RgbrAmtCalcType = "F",
                        RgbrArCode = "DEF",
                        RgbrCrAmt = 100m
                    },
                };
                dataReaderMock.Setup<Task<Collection<RegBillingRates>>>(cacc => cacc.BulkReadRecordAsync<RegBillingRates>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(rbrs);


                //setup mocking for CourseSecMeeting
                var crsSecMeet = BuildCourseSecMeetingDefaults();
                dataReaderMock.Setup<Task<CourseSecMeeting>>(dr => dr.ReadRecordAsync<CourseSecMeeting>(It.IsAny<GuidLookup>(), true)).Returns(Task.FromResult(crsSecMeet));

                //setup mocking for Stweb Defaults
                var stWebDflt = BuildStwebDefaults(); ;
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                    );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                   );
                MockRecordsAsync<InstrMethods>("INSTR.METHODS", BuildValidInstrMethodResponse());
                // Mock the trxn getting the waitlist status
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Mock the read of instructional methods


                InstrMethods lec = new InstrMethods()
                {
                    InmDesc = "LEC",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                    Recordkey = "LEC"
                };
                InstrMethods lab = new InstrMethods()
                {
                    InmDesc = "LAB",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                    Recordkey = "LAB"
                };
                InstrMethods onl = new InstrMethods()
                {
                    InmDesc = "ONL",
                    InmOnline = "Y",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                    Recordkey = "ONL"
                };
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));


                GetSectionWaitlistStatusResponse wlResp = new GetSectionWaitlistStatusResponse() { ErrorMessages = new List<string>(), Status = "Wlst" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetSectionWaitlistStatusRequest, GetSectionWaitlistStatusResponse>(It.IsAny<GetSectionWaitlistStatusRequest>())).ReturnsAsync(wlResp);


                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new Ellucian.Colleague.Domain.Student.Tests.TestTermRepository(), apiSettings);

                return sectionRepo;
            }

            private SectionRepository BuildInvalidSectionRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();
                apiSettings = new ApiSettings("null");

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for "all" section requests
                Exception expectedFailure = new Exception("fail");
                dataAccessorMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), true)).Throws(expectedFailure);

                // Cache Mock
                var localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                var cacheProviderMock = new Mock<ICacheProvider>();
                //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);
                // Mock the trxn getting the waitlist status
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Mock the read of instructional methods


                InstrMethods lec = new InstrMethods()
                {
                    InmDesc = "LEC",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                    Recordkey = "LEC"
                };
                InstrMethods lab = new InstrMethods()
                {
                    InmDesc = "LAB",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                    Recordkey = "LAB"
                };
                InstrMethods onl = new InstrMethods()
                {
                    InmDesc = "ONL",
                    InmOnline = "Y",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                    Recordkey = "ONL"
                };
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));


                GetSectionWaitlistStatusResponse wlResp = new GetSectionWaitlistStatusResponse() { ErrorMessages = new List<string>(), Status = "Wlst" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetSectionWaitlistStatusRequest, GetSectionWaitlistStatusResponse>(It.IsAny<GetSectionWaitlistStatusRequest>())).ReturnsAsync(wlResp);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                  x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                  .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                return sectionRepo;
            }

            private Collection<CourseSections> BuildSectionsResponse(IEnumerable<Section> sections)
            {
                Collection<CourseSections> repoSections = new Collection<CourseSections>();
                foreach (var section in sections)
                {
                    var crsSec = new CourseSections();
                    crsSec.RecordGuid = section.Guid;
                    crsSec.Recordkey = section.Id.ToString();
                    crsSec.SecAcadLevel = section.AcademicLevelCode;
                    crsSec.SecCeus = section.Ceus;
                    crsSec.SecCourse = section.CourseId.ToString();
                    crsSec.SecCourseLevels = section.CourseLevelCodes.ToList();
                    crsSec.SecDepts = section.Departments.Select(x => x.AcademicDepartmentCode).ToList();
                    crsSec.SecLocation = section.Location;
                    crsSec.SecMaxCred = section.MaximumCredits;
                    crsSec.SecVarCredIncrement = section.VariableCreditIncrement;
                    crsSec.SecMinCred = section.MinimumCredits;
                    crsSec.SecNo = section.Number;
                    crsSec.SecShortTitle = section.Title;
                    crsSec.SecDepartmentsEntityAssociation = section.Departments.Select(x => new CourseSectionsSecDepartments(x.AcademicDepartmentCode, x.ResponsibilityPercentage)).ToList();
                    crsSec.SecCredType = section.CreditTypeCode;
                    crsSec.SecStatusesEntityAssociation = section.Statuses.Select(x => new CourseSectionsSecStatuses(x.Date, ConvertSectionStatusToCode(x.Status))).ToList();
                    crsSec.SecStartDate = section.StartDate;
                    crsSec.SecEndDate = section.EndDate;
                    crsSec.SecTerm = section.TermId;
                    crsSec.SecOnlyPassNopassFlag = section.OnlyPassNoPass == true ? "Y" : "N";
                    crsSec.SecAllowPassNopassFlag = section.AllowPassNoPass == true ? "Y" : "N";
                    crsSec.SecAllowAuditFlag = section.AllowAudit == true ? "Y" : "N";
                    var csm = new List<string>() { "1", "2" };
                    crsSec.SecMeeting = csm;
                    if (section.CourseId == "7272" && section.Number == "98")
                    {
                        // Used to test that the secondary section which is PresentAbsent follows the primary.
                        crsSec.SecAttendTrackingType = "S";
                    }
                    var csf = new List<string>() { "1", "2" };
                    crsSec.SecFaculty = csf;
                    var sas = new List<string>() { "1", "2" };
                    crsSec.SecActiveStudents = sas;
                    crsSec.SecCourseTypes = section.CourseTypeCodes.ToList();
                    crsSec.SecAllowWaitlistFlag = section.AllowWaitlist == true ? "Y" : "N";
                    crsSec.SecFacultyConsentFlag = section.IsInstructorConsentRequired ? "Y" : "N";
                    crsSec.SecName = section.Name;
                    crsSec.SecNoWeeks = section.NumberOfWeeks;
                    crsSec.SecWaitlistRating = section.WaitlistRatingCode;
                    crsSec.SecHideInCatalog = section.HideInCatalog ? "Y" : string.Empty;
                    crsSec.SecInstrMethods = new List<string>() { "LEC" };


                    // Reconstruct all Colleague requisite and corequisite fields from the data in the Requisites 
                    // post-conversion fields
                    crsSec.SecReqs = new List<string>();
                    crsSec.SecRecommendedSecs = new List<string>();
                    crsSec.SecCoreqSecs = new List<string>();
                    crsSec.SecMinNoCoreqSecs = null;
                    crsSec.SecOverrideCrsReqsFlag = section.OverridesCourseRequisites == true ? "Y" : "";
                    // pre-conversion fields
                    crsSec.SecCourseCoreqsEntityAssociation = new List<CourseSectionsSecCourseCoreqs>();
                    crsSec.SecCoreqsEntityAssociation = new List<CourseSectionsSecCoreqs>();

                    foreach (var req in section.Requisites)
                    {
                        if (!string.IsNullOrEmpty(req.RequirementCode))
                        {
                            // post-conversion
                            crsSec.SecReqs.Add(req.RequirementCode);
                            // pre-conversion -- this does not convert
                        }
                        else if (!string.IsNullOrEmpty(req.CorequisiteCourseId))
                        {
                            // post-conversion -- this does not convert
                            // pre-conversion
                            crsSec.SecCourseCoreqsEntityAssociation.Add(
                                new CourseSectionsSecCourseCoreqs(req.CorequisiteCourseId, (req.IsRequired == true) ? "Y" : "")
                            );
                        }

                    }

                    foreach (var req in section.SectionRequisites)
                    {
                        if (req.CorequisiteSectionIds != null && req.CorequisiteSectionIds.Count() > 1)
                        {
                            // these are all required
                            foreach (var secId in req.CorequisiteSectionIds)
                            {
                                // postconversion
                                crsSec.SecCoreqSecs.Add(secId);
                                // pre-conversion. number needed does not convert
                                crsSec.SecCoreqsEntityAssociation.Add(
                                    new CourseSectionsSecCoreqs(secId, "Y")
                                    );
                            }
                            // The minimum number is associated with the entire list of SecCoreqSecs
                            // postconversion
                            var numNeeded = (crsSec.SecMinNoCoreqSecs.HasValue && crsSec.SecMinNoCoreqSecs > 0 ? crsSec.SecMinNoCoreqSecs : 0);
                            numNeeded += req.NumberNeeded;
                            crsSec.SecMinNoCoreqSecs = numNeeded;
                        }
                        else
                        {
                            if (req.IsRequired)
                            {
                                // pre-conversion--each requisite section has a required flag
                                crsSec.SecCoreqsEntityAssociation.Add(
                                    new CourseSectionsSecCoreqs(req.CorequisiteSectionIds.ElementAt(0), "Y")
                                    );
                                // post-conversion--all required sections--add to list and increment counter
                                crsSec.SecCoreqSecs.Add(req.CorequisiteSectionIds.ElementAt(0));
                                var numNeeded = crsSec.SecMinNoCoreqSecs.HasValue && crsSec.SecMinNoCoreqSecs > 0 ? crsSec.SecMinNoCoreqSecs : 0;
                                numNeeded += 1;
                                crsSec.SecMinNoCoreqSecs = numNeeded;
                            }
                            else
                            {
                                // pre-conversion
                                crsSec.SecCoreqsEntityAssociation.Add(
                                    new CourseSectionsSecCoreqs(req.CorequisiteSectionIds.ElementAt(0), "N")
                                    );
                                // post-conversion
                                crsSec.SecRecommendedSecs.Add(req.CorequisiteSectionIds.ElementAt(0));
                            }
                        }
                    }

                    if (section.Books != null)
                    {
                        crsSec.SecBooks = new List<string>();
                        crsSec.SecBookOptions = new List<string>();
                        foreach (var book in section.Books)
                        {
                            crsSec.SecBooks.Add(book.BookId);
                            if (book.IsRequired == true)
                            {
                                crsSec.SecBookOptions.Add(book.RequirementStatusCode);
                            }
                            else
                            {
                                crsSec.SecBookOptions.Add(book.RequirementStatusCode);
                            }
                        }
                    }
                    crsSec.SecPortalSite = ""; // (!string.IsNullOrEmpty(section.LearningProvider) ? crsSec.Recordkey : "");
                    if (!string.IsNullOrEmpty(section.LearningProvider))
                    {
                        crsSec.SecPortalSite = section.Id;
                    }
                    else
                    {
                        crsSec.SecPortalSite = "";
                    }
                    crsSec.SecXlist = ""; // (!string.IsNullOrEmpty(section.PrimarySectionId) && section.PrimarySectionId.Equals(section.Id)) ? "" : crsSec.Recordkey;
                    if (!string.IsNullOrEmpty(section.PrimarySectionId))
                    {
                        crsSec.SecXlist = section.Id;
                    }
                    else
                    {
                        crsSec.SecXlist = "";
                    }

                    repoSections.Add(crsSec);
                }
                return repoSections;
            }

            private Collection<CourseSecMeeting> BuildSectionMeetingsResponse(IEnumerable<Section> sections)
            {
                Collection<CourseSecMeeting> repoSecMeetings = new Collection<CourseSecMeeting>();
                int crsSecMId = 0;
                foreach (var section in sections)
                {
                    foreach (var mt in section.Meetings)
                    {
                        var crsSecM = new CourseSecMeeting();
                        crsSecMId += 1;
                        crsSecM.Recordkey = crsSecMId.ToString();
                        if (!string.IsNullOrEmpty(mt.Room))
                        {
                            crsSecM.CsmBldg = "ABLE";
                            crsSecM.CsmRoom = "A100";
                        }
                        crsSecM.CsmCourseSection = section.Id;
                        crsSecM.CsmInstrMethod = mt.InstructionalMethodCode;
                        crsSecM.CsmStartTime = mt.StartTime.HasValue ? mt.StartTime.Value.DateTime : (DateTime?)null;
                        crsSecM.CsmEndTime = mt.EndTime.HasValue ? mt.EndTime.Value.DateTime : (DateTime?)null;
                        crsSecM.CsmStartDate = mt.StartDate;
                        crsSecM.CsmEndDate = mt.EndDate;
                        crsSecM.CsmFrequency = mt.Frequency;
                        foreach (var d in mt.Days)
                        {
                            switch (d)
                            {
                                case DayOfWeek.Friday:
                                    crsSecM.CsmFriday = "Y";
                                    break;
                                case DayOfWeek.Monday:
                                    crsSecM.CsmMonday = "Y";
                                    break;
                                case DayOfWeek.Saturday:
                                    crsSecM.CsmSaturday = "Y";
                                    break;
                                case DayOfWeek.Sunday:
                                    crsSecM.CsmSunday = "Y";
                                    break;
                                case DayOfWeek.Thursday:
                                    crsSecM.CsmThursday = "Y";
                                    break;
                                case DayOfWeek.Tuesday:
                                    crsSecM.CsmTuesday = "Y";
                                    break;
                                case DayOfWeek.Wednesday:
                                    crsSecM.CsmWednesday = "Y";
                                    break;
                                default:
                                    break;
                            }
                        }
                        repoSecMeetings.Add(crsSecM);
                    }

                }
                return repoSecMeetings;
            }

            private Collection<CourseSecFaculty> BuildSectionFacultyResponse(IEnumerable<Section> sections)
            {
                Collection<CourseSecFaculty> repoSecFaculty = new Collection<CourseSecFaculty>();
                int crsSecFId = 0;
                foreach (var section in sections)
                {
                    foreach (var fac in section.FacultyIds)
                    {
                        var crsSecF = new CourseSecFaculty();
                        crsSecFId += 1;
                        crsSecF.Recordkey = crsSecFId.ToString();
                        crsSecF.CsfCourseSection = section.Id;
                        crsSecF.CsfFaculty = fac;
                        repoSecFaculty.Add(crsSecF);
                    }

                }
                return repoSecFaculty;
            }

            private List<StudentCourseSectionStudents> BuildStudentCourseSecStudents(IEnumerable<Section> sections)
            {
                var studentCourseSectionStudents = new List<StudentCourseSectionStudents>();
                Collection<StudentCourseSec> repoSCS = new Collection<StudentCourseSec>();
                foreach (var section in sections)
                {
                    foreach (var stu in section.ActiveStudentIds)
                    {
                        StudentCourseSectionStudents scss = new StudentCourseSectionStudents();
                        scss.CourseSectionIds = section.Id;
                        scss.StudentIds = stu;
                        studentCourseSectionStudents.Add(scss);
                    }
                }
                return studentCourseSectionStudents;
            }

            private Collection<PortalSites> BuildPortalSitesResponse(IEnumerable<Section> sections)
            {
                Collection<PortalSites> repoPS = new Collection<PortalSites>();
                foreach (var section in sections)
                {
                    if (section.CourseId == "7272")
                    {
                        var ps = new PortalSites();
                        // normally some thing like "HIST-190-001-cs11347", but mock portal site Id with section ID
                        ps.Recordkey = section.Id;
                        if (section.Number == "98")
                        {
                            ps.PsLearningProvider = "";
                            ps.PsPrtlSiteGuid = section.Id;
                        }
                        if (section.Number == "97")
                        {
                            ps.PsLearningProvider = "MOODLE";
                            ps.PsPrtlSiteGuid = section.Id;
                        }
                        repoPS.Add(ps);
                    }
                }
                return repoPS;
            }

            private Collection<CourseSecXlists> BuildCrosslistResponse(IEnumerable<Section> sections)
            {
                // currently built only for ILP testing
                Collection<CourseSecXlists> repoXL = new Collection<CourseSecXlists>();
                foreach (var section in sections)
                {
                    if (section.CourseId == "7272" && (section.Number == "98" || section.Number == "97"))
                    {
                        var xl = new CourseSecXlists();
                        xl.Recordkey = section.Id;
                        if (section.Number == "98")
                        {
                            xl.CsxlPrimarySection = section.Id;
                            xl.CsxlCourseSections = new List<string>() { section.Id, "1", "5" };
                            xl.CsxlCapacity = 20;
                        }
                        if (section.Number == "97")
                        {
                            xl.CsxlPrimarySection = section.Id;
                            xl.CsxlCourseSections = new List<string>() { "2", section.Id, "6" };
                            xl.CsxlCapacity = 100;
                        }
                        repoXL.Add(xl);
                    }
                }
                return repoXL;
            }

            private Collection<CourseSecPending> BuildPendingSectionResponse(IEnumerable<Section> sections)
            {
                Collection<CourseSecPending> repoPending = new Collection<CourseSecPending>();
                foreach (var section in sections)
                {
                    if (section.CourseId == "7272" && (section.Number == "98" || section.Number == "97"))
                    {
                        var cp = new CourseSecPending();
                        cp.Recordkey = section.Id;
                        if (section.Number == "98")
                        {
                            cp.CspReservedSeats = 1;
                        }
                        if (section.Number == "97")
                        {
                            cp.CspReservedSeats = 2;
                        }
                        repoPending.Add(cp);
                    }
                }
                return repoPending;
            }

            private Collection<WaitList> BuildWaitlistResponse(IEnumerable<Section> sections)
            {
                Collection<WaitList> repoWaitlist = new Collection<WaitList>();
                foreach (var section in sections)
                {
                    if (section.CourseId == "7272" && (section.Number == "98" || section.Number == "97"))
                    {
                        var wl = new WaitList();
                        wl.Recordkey = section.Id;
                        if (section.Number == "98")
                        {
                            wl.WaitCourseSection = section.Id;
                            wl.WaitStatus = "P";
                            wl.WaitStudent = "111111";
                        }
                        if (section.Number == "97")
                        {
                            wl.WaitCourseSection = section.Id;
                            wl.WaitStatus = "P";
                            wl.WaitStudent = "22222";
                        }
                        repoWaitlist.Add(wl);
                    }
                }
                return repoWaitlist;
            }

            private Collection<AcadReqmts> BuildAcadReqmtsResponse()
            {
                Collection<AcadReqmts> acadReqmtsResponse = new Collection<AcadReqmts>();
                // Previous, Required
                var acadReqmts1 = new AcadReqmts() { Recordkey = "PREREQ1", AcrReqsTiming = "P", AcrReqsEnforcement = "RQ" };
                acadReqmtsResponse.Add(acadReqmts1);
                // Previous, Recommended
                var acadReqmts2 = new AcadReqmts() { Recordkey = "PREREQ2", AcrReqsTiming = "P", AcrReqsEnforcement = "RM" };
                acadReqmtsResponse.Add(acadReqmts2);
                // Concurrent, Required
                var acadReqmts3 = new AcadReqmts() { Recordkey = "COREQ1", AcrReqsTiming = "C", AcrReqsEnforcement = "RQ" };
                acadReqmtsResponse.Add(acadReqmts3);
                // Concurrent, Recommended
                var acadReqmts4 = new AcadReqmts() { Recordkey = "COREQ2", AcrReqsTiming = "C", AcrReqsEnforcement = "RM" };
                acadReqmtsResponse.Add(acadReqmts4);
                //  Previous or Concurrent, Required
                var acadReqmts5 = new AcadReqmts() { Recordkey = "REQ1", AcrReqsTiming = "E", AcrReqsEnforcement = "RQ" };
                acadReqmtsResponse.Add(acadReqmts5);
                return acadReqmtsResponse;
            }

            private CdDefaults BuildCourseParametersConvertedResponse(CdDefaults defaults)
            {
                if (defaults == null)
                    defaults = BuildCdDefaults();
                // Converted Response
                defaults.CdReqsConvertedFlag = "Y";
                return defaults;
            }

            private CdDefaults BuildCourseParametersUnConvertedResponse(CdDefaults defaults)
            {
                if (defaults == null)
                    defaults = BuildCdDefaults();
                defaults.CdReqsConvertedFlag = "";
                return defaults;
            }

        }

        [TestClass]
        public class SectionRepository_GetNonCachedSectionsTests : SectionRepositoryTests
        {
            // Broke out these tests so I could better control the subset of records and the data.
            Collection<CourseSections> sectionsResponseData;
            Collection<CourseSecMeeting> sectionMeetingResponseData;
            Collection<CourseSecFaculty> sectionFacultyResponseData;
            List<StudentCourseSectionStudents> studentCourseSecResponseData;
            Collection<PortalSites> portalSitesResponseData;
            Collection<CourseSecXlists> crosslistResponseData;
            Collection<CourseSecPending> pendingResponseData;
            Collection<WaitList> waitlistResponseData;
            IEnumerable<Section> reqSections;
            List<Term> registrationTerms = new List<Term>();
            CdDefaults cdDefaults;

            SectionRepository sectionRepo;

            [TestInitialize]
            public async void Initialize()
            {
                base.MainInitialize();
                // Build Section responses used for mocking
                IEnumerable<string> sectionIdsRequested = new List<string>() { "1", "2", "3", "5", "4", "6", "24", "36" };
                reqSections = await new TestSectionRepository().GetNonCachedSectionsAsync(sectionIdsRequested);
                Term term1 = new Term("2012/FA", "Fall 2012", new DateTime(2012, 9, 1), new DateTime(2012, 12, 15), 2012, 1, true, true, "2012/FA", true);
                Term term2 = new Term("2013/SP", "Spring 2013", new DateTime(2013, 1, 1), new DateTime(2013, 5, 15), 2012, 2, true, true, "2013/SP", true);
                registrationTerms.Add(term1);
                registrationTerms.Add(term2);
                sectionsResponseData = BuildSectionsResponse(reqSections);
                sectionMeetingResponseData = BuildSectionMeetingsResponse(reqSections);
                sectionFacultyResponseData = BuildSectionFacultyResponse(reqSections);
                studentCourseSecResponseData = BuildStudentCourseSecStudents(reqSections);
                portalSitesResponseData = BuildPortalSitesResponse(reqSections);
                crosslistResponseData = BuildCrosslistResponse(reqSections);
                pendingResponseData = BuildPendingSectionResponse(reqSections);
                waitlistResponseData = BuildWaitlistResponse(reqSections);

                // Build section repository
                sectionRepo = BuildValidSectionRepository();

                // Mock the trxn getting the waitlist status
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Mock the read of instructional methods


                InstrMethods lec = new InstrMethods()
                {
                    InmDesc = "LEC",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                    Recordkey = "LEC"
                };
                InstrMethods lab = new InstrMethods()
                {
                    InmDesc = "LAB",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                    Recordkey = "LAB"
                };
                InstrMethods onl = new InstrMethods()
                {
                    InmDesc = "ONL",
                    InmOnline = "Y",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                    Recordkey = "ONL"
                };
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));


                GetSectionWaitlistStatusResponse wlResp = new GetSectionWaitlistStatusResponse() { ErrorMessages = new List<string>(), Status = "Wlst" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetSectionWaitlistStatusRequest, GetSectionWaitlistStatusResponse>(It.IsAny<GetSectionWaitlistStatusRequest>())).ReturnsAsync(wlResp);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                sectionsResponseData = null;
                sectionRepo = null;
            }

            [TestMethod]
            public async Task SectionRepository_GetNonCachedSections_NullSectionIds()
            {
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(null);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetNonCachedSections_NoSectionsFound()
            {
                IEnumerable<string> sectionIds = new List<string>() { "99999", "JUNK" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetNonCachedSections_ZeroSectionIds()
            {
                IEnumerable<string> sectionIds = new List<string>();
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetNonCachedSections_SectionsReturned()
            {
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Assert.AreEqual(3, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetNonCachedSections_TestProperties()
            {
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section section = sections.Where(s => s.Id == "1").FirstOrDefault();
                Assert.AreEqual("1", section.Id);
                Assert.AreEqual("2012/FA", section.TermId);
                Assert.IsTrue(section.IsActive);
                Assert.IsTrue(section.AllowPassNoPass);
                Assert.IsTrue(section.AllowAudit);
                Assert.IsFalse(section.OnlyPassNoPass);
                Assert.IsFalse(section.AllowWaitlist);
                Assert.AreEqual(20, section.Capacity);
                Assert.IsFalse(section.HideRequisiteWaiver);
                Assert.IsFalse(section.HideStudentPetition);
                Assert.IsFalse(section.HideFacultyConsent);
            }

            // TODO: SSS This test broken by Async changes, needs to be resolved
            [TestMethod]
            public async Task SectionRepository_GetNonCachedSections_OnlineIndicators()
            {
                // Section 1 will be Online entirely
                // Section 2 will be Hybrid
                // Section 3 will not be online line (one non online meeting)
                // Section 5 has no meetings - not online

                List<string> sectionIds = new List<string>() { "1", "2", "3", "5" };

                // Set up the section meetings we want to go with these sections.
                Collection<CourseSecMeeting> sectionMeetings = new Collection<CourseSecMeeting>();
                sectionMeetings.Add(new CourseSecMeeting() { Recordkey = "11", RecordGuid = Guid.NewGuid().ToString(), CsmCourseSection = "1", CsmInstrMethod = "ONL", CsmFriday = "Y", CsmStartDate = new DateTime(2012, 9, 11), CsmEndDate = new DateTime(2012, 12, 12), CsmFrequency = "W" });
                sectionMeetings.Add(new CourseSecMeeting() { Recordkey = "22", RecordGuid = Guid.NewGuid().ToString(), CsmCourseSection = "2", CsmInstrMethod = "LEC", CsmFriday = "Y", CsmStartDate = new DateTime(2012, 9, 11), CsmEndDate = new DateTime(2012, 12, 12), CsmFrequency = "W" });
                sectionMeetings.Add(new CourseSecMeeting() { Recordkey = "23", RecordGuid = Guid.NewGuid().ToString(), CsmCourseSection = "2", CsmInstrMethod = "ONL", CsmFriday = "Y", CsmStartDate = new DateTime(2012, 9, 11), CsmEndDate = new DateTime(2012, 12, 12), CsmFrequency = "W" });
                sectionMeetings.Add(new CourseSecMeeting() { Recordkey = "33", RecordGuid = Guid.NewGuid().ToString(), CsmCourseSection = "3", CsmInstrMethod = "LEC", CsmFriday = "Y", CsmStartDate = new DateTime(2012, 9, 11), CsmEndDate = new DateTime(2012, 12, 12), CsmFrequency = "W" });
                dataReaderMock.Setup<Task<Collection<CourseSecMeeting>>>(macc => macc.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionMeetings));
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section section1 = sections.Where(s => s.Id == "1").FirstOrDefault();
                Assert.AreEqual(OnlineCategory.Online, section1.OnlineCategory);
                Assert.IsTrue(section1.Meetings.ElementAt(0).IsOnline);
                Section section2 = sections.Where(s => s.Id == "2").FirstOrDefault();
                Assert.AreEqual(OnlineCategory.Hybrid, section2.OnlineCategory);
                Section section3 = sections.Where(s => s.Id == "3").FirstOrDefault();
                Assert.AreEqual(OnlineCategory.NotOnline, section3.OnlineCategory);
                Section section5 = sections.Where(s => s.Id == "5").FirstOrDefault();
                Assert.AreEqual(OnlineCategory.NotOnline, section5.OnlineCategory);
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsNonCached_CourseIdsNull()
            {
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsNonCachedAsync(null, registrationTerms);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsNonCached_CourseIdsZero()
            {
                IEnumerable<string> courseIds = new List<string>();
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsNonCachedAsync(courseIds, registrationTerms);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsNonCached_TermsNull()
            {
                IEnumerable<string> courseIds = new List<string>() { "123" };
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsNonCachedAsync(courseIds, null);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsNonCached_TermsZero()
            {
                IEnumerable<Term> termIds = new List<Term>();
                IEnumerable<string> courseIds = new List<string>() { "123" };
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsNonCachedAsync(courseIds, termIds);
                Assert.AreEqual(0, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetCourseSectionsNonCached_ReturnsResults()
            {
                IEnumerable<string> courseIds = new List<string>() { "123" };
                IEnumerable<Section> sections = await sectionRepo.GetCourseSectionsNonCachedAsync(courseIds, registrationTerms);
                Assert.AreEqual(8, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetNoncachedSections_NullFacultyDataResponse_SectionsReturned()
            {
                // Mock null faculty repo response
                sectionFacultyResponseData = null;
                dataReaderMock.Setup<Task<Collection<CourseSecFaculty>>>(facc => facc.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionFacultyResponseData));
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Assert.AreEqual(3, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetNoncachedSections_NullRosterDataResponse_SectionsReturned()
            {
                // Mock null roster response
                studentCourseSecResponseData = null;

                dataReaderMock.Setup(ac => ac.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), "SAVING SCS.COURSE.SECTION")).ReturnsAsync(() => null);
                dataReaderMock.Setup(ac => ac.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), "SAVING SCS.STUDENT")).ReturnsAsync(() => null);
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Assert.AreEqual(3, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetNoncachedSections_NullPendingDataResponse_SectionsReturned()
            {
                // Mock null pending data response
                pendingResponseData = null;
                dataReaderMock.Setup<Task<Collection<CourseSecPending>>>(csp => csp.BulkReadRecordAsync<CourseSecPending>("COURSE.SEC.PENDING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(pendingResponseData));
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Assert.AreEqual(3, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetNoncachedSections_NullWaitlistDataReponse_SectionsReturned()
            {
                // Mock null waitlist data response
                waitlistResponseData = null;
                dataReaderMock.Setup<Task<Collection<WaitList>>>(wl => wl.BulkReadRecordAsync<WaitList>("WAIT.LIST", It.IsAny<string>(), true)).Returns(Task.FromResult(waitlistResponseData));
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Assert.AreEqual(3, sections.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetNoncachedSections_NullPortalDataResponse_SectionsReturned()
            {
                // Mock null portal site response
                portalSitesResponseData = null;
                dataReaderMock.Setup<Task<Collection<PortalSites>>>(ps => ps.BulkReadRecordAsync<PortalSites>("PORTAL.SITES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(portalSitesResponseData));
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Assert.AreEqual(3, sections.Count());
            }

            [TestMethod]
            public async Task StWebDefaultsNotFound()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                (param, id, repl) => Task.FromResult(new StwebDefaults())
                );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (id, repl) => Task.FromResult(new StwebDefaults())
                   );
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section sec = sections.Where(s => s.Id == "1").FirstOrDefault();
                Assert.AreEqual(null, sec.BookstoreURL);
            }

            [TestMethod]
            public async Task NoBookstoreTemplateDefined()
            {
                StwebDefaults stwebDefaults = new StwebDefaults();
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section sec = sections.Where(s => s.Id == "1").FirstOrDefault();
                Assert.AreEqual(null, sec.BookstoreURL);
            }

            [TestMethod]
            public async Task BookstoreTemplate_RemovedValueMarks()
            {
                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebBookstoreUrlTemplate = "abc" + DmiString._VM + "zyx";
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section sec = sections.Where(s => s.Id == "1").FirstOrDefault();
                Assert.AreEqual("abczyx", sec.BookstoreURL);
            }

            [TestMethod]
            public async Task TemplateHasNoQueryParmeters()
            {
                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebBookstoreUrlTemplate = "abc";
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section sec = sections.Where(s => s.Id == "1").FirstOrDefault();
                Assert.AreEqual("abc", sec.BookstoreURL);
            }

            [TestMethod]
            public async Task MultipleSubstitutions_Successful()
            {
                string template = "abc?a={4}&b={5}&c={0}&d={1}&e={3}&f={2}&a={4}&b={5}&c={0}&d={1}&e={3}&f={2}"; StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebBookstoreUrlTemplate = template;
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section sec = sections.Where(s => s.Id == "1").FirstOrDefault();
                Assert.AreEqual("abc?a=1&b=139&c=2012%2fFA&d=MATH&e=01&f=1000&a=1&b=139&c=2012%2fFA&d=MATH&e=01&f=1000", sec.BookstoreURL);
            }
            [TestMethod]
            public async Task MultipleSubstitutions_Successful_with_Location()
            {
                string template = "abc?a={4}&b={5}&c={0}&d={1}&e={3}&f={2}&a={4}&b={5}&c={0}&d={1}&e={3}&f={2}&location={6}"; StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebBookstoreUrlTemplate = template;
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                List<string> sectionIds = new List<string>() { "4", "6" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section sec = sections.Where(s => s.Id == "4").FirstOrDefault();
                Assert.AreEqual("abc?a=4&b=42&c=2012%2fFA&d=&e=01&f=&a=4&b=42&c=2012%2fFA&d=&e=01&f=&location=MAIN", sec.BookstoreURL);
            }
            [TestMethod]
            public async Task MultipleSubstitutions_Successful_with_Empty_Location()
            {
                string template = "abc?a={4}&b={5}&c={0}&d={1}&e={3}&f={2}&a={4}&b={5}&c={0}&d={1}&e={3}&f={2}&location={6}"; StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebBookstoreUrlTemplate = template;
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                List<string> sectionIds = new List<string>() { "24", "36" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section sec = sections.Where(s => s.Id == "24").FirstOrDefault();
                Assert.AreEqual("abc?a=24&b=143&c=2012%2fFA&d=&e=03&f=&a=24&b=143&c=2012%2fFA&d=&e=03&f=&location=", sec.BookstoreURL);
            }
            [TestMethod]
            public async Task MultipleSubstitutions_Successful_with_null_Location()
            {
                string template = "abc?a={4}&b={5}&c={0}&d={1}&e={3}&f={2}&a={4}&b={5}&c={0}&d={1}&e={3}&f={2}&location={6}"; StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebBookstoreUrlTemplate = template;
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                List<string> sectionIds = new List<string>() { "1", "2", "3" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section sec = sections.Where(s => s.Id == "1").FirstOrDefault();
                Assert.AreEqual("abc?a=1&b=139&c=2012%2fFA&d=MATH&e=01&f=1000&a=1&b=139&c=2012%2fFA&d=MATH&e=01&f=1000&location=", sec.BookstoreURL);
            }

            [TestMethod]
            public async Task MultipleSubstitutions_For_BarnesNoble_link()
            {
                string template = "http://blueridge.bncollege.com/webapp/wcs/stores/servlet/TBListView?catalogId=10001&storeId=66236&termMapping=Y&courseXml=<?xml version=\"1.0\" encoding=\"UTF - 8\" ?> <textbookorder><campus name=\"{6}\"><courses><course dept=\"{1}\" num=\"{2}\" sect=\"{3}\" term=\"{0}\"/></courses></campus></textbookorder>";



                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebBookstoreUrlTemplate = template;
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                List<string> sectionIds = new List<string>() { "4", "6" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section sec = sections.Where(s => s.Id == "6").FirstOrDefault();
                Assert.AreEqual("http://blueridge.bncollege.com/webapp/wcs/stores/servlet/TBListView?catalogId=10001&storeId=66236&termMapping=Y&courseXml=<?xml version=\"1.0\" encoding=\"UTF - 8\" ?> <textbookorder><campus name=\"MAIN\"><courses><course dept=\"ART\" num=\"2233\" sect=\"03\" term=\"2012%2fFA\"/></courses></campus></textbookorder>", sec.BookstoreURL);
            }

            [TestMethod]
            public async Task Validate_PrimarySectionMeetings_WithDefaultFlagY_CrossListedMeetingEmpty_PrimarySectionHaveMeetings()
            {
                //global parameter is Y and cross-listed paramter is empty
                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebUsePrimSecMtgFlag = "y";
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                //crosslist repsonse data is for section id =1 with recordkey=232 that is crosslisted to 1,5 sections
                crosslistResponseData[0].CsxlPrimSecMngOvrdeFlag = string.Empty;
                //primary section is 1-> crosslisted to 1 and 5 sectionIds
                //course.section have crosslisted id ="232" for section id = 1
                //primary section have meeting ids 1 & 2
                var primarySectionData = sectionsResponseData.Where(s => s.Recordkey == "1").First();
                primarySectionData.SecXlist = "232";
                //make a section that is cross-listed section and do not have meetings
                var crosslistedSectionData = sectionsResponseData.Where(s => s.Recordkey == "5").First();
                crosslistedSectionData.SecMeeting = null;
                //modifying the reference of meetings to have primary section id. such as primary section =1 have two meeting data
                var meetingData = sectionMeetingResponseData.Where(s => s.CsmCourseSection == "5").First();
                meetingData.CsmCourseSection = "1";
                var meetingData1 = sectionMeetingResponseData.Where(s => s.CsmCourseSection == "4").First();
                meetingData1.CsmCourseSection = "1";


                List<string> sectionIds = new List<string>() { "1", "2", "3", "5" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section primarySection = sections.Where(s => s.Id == "1").FirstOrDefault();
                Section crosslistedSection = sections.Where(s => s.Id == "5").FirstOrDefault();
                Assert.AreEqual("1", primarySection.Id);
                Assert.AreEqual(2, primarySection.Meetings.Count());
                Assert.AreEqual(0, primarySection.PrimarySectionMeetings.Count());

                Assert.AreEqual("5", crosslistedSection.Id);
                Assert.AreEqual(0, crosslistedSection.Meetings.Count());
                Assert.AreEqual(2, crosslistedSection.PrimarySectionMeetings.Count());

                Assert.AreEqual(primarySection.Meetings[0].Id, crosslistedSection.PrimarySectionMeetings[0].Id);
                Assert.AreEqual(primarySection.Meetings[0].Days, crosslistedSection.PrimarySectionMeetings[0].Days);
                Assert.AreEqual(primarySection.Meetings[0].StartDate, crosslistedSection.PrimarySectionMeetings[0].StartDate);
                Assert.AreEqual(primarySection.Meetings[0].StartTime, crosslistedSection.PrimarySectionMeetings[0].StartTime);
                Assert.AreEqual(primarySection.Meetings[0].EndDate, crosslistedSection.PrimarySectionMeetings[0].EndDate);
                Assert.AreEqual(primarySection.Meetings[0].EndTime, crosslistedSection.PrimarySectionMeetings[0].EndTime);
                Assert.AreEqual(primarySection.Meetings[0].InstructionalMethodCode, crosslistedSection.PrimarySectionMeetings[0].InstructionalMethodCode);
                Assert.AreEqual(primarySection.Meetings[0].Room, crosslistedSection.PrimarySectionMeetings[0].Room);


                Assert.AreEqual(primarySection.Meetings[1].Id, crosslistedSection.PrimarySectionMeetings[1].Id);
                Assert.AreEqual(primarySection.Meetings[1].Days, crosslistedSection.PrimarySectionMeetings[1].Days);
                Assert.AreEqual(primarySection.Meetings[1].StartDate, crosslistedSection.PrimarySectionMeetings[1].StartDate);
                Assert.AreEqual(primarySection.Meetings[1].StartTime, crosslistedSection.PrimarySectionMeetings[1].StartTime);
                Assert.AreEqual(primarySection.Meetings[1].EndDate, crosslistedSection.PrimarySectionMeetings[1].EndDate);
                Assert.AreEqual(primarySection.Meetings[1].EndTime, crosslistedSection.PrimarySectionMeetings[1].EndTime);
                Assert.AreEqual(primarySection.Meetings[1].InstructionalMethodCode, crosslistedSection.PrimarySectionMeetings[1].InstructionalMethodCode);
                Assert.AreEqual(primarySection.Meetings[1].Room, crosslistedSection.PrimarySectionMeetings[1].Room);


            }

            [TestMethod]
            public async Task Validate_PrimarySectionMeetings_WithDefaultFlagY_CrossListedHaveMeeting_PrimarySectionHaveMeetings()
            {
                //global parameter is N and cross-listed paramter is empty
                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebUsePrimSecMtgFlag = "y";
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                //crosslist repsonse data is for section id =1 with recordkey=232 that is crosslisted to 1,5 sections
                crosslistResponseData[0].CsxlPrimSecMngOvrdeFlag = string.Empty;
                //primary section is 1-> crosslisted to 1 and 5 sectionIds
                //course.section have crosslisted id ="232" for section id = 1
                //primary section have meeting ids 1 & 2
                var primarySectionData = sectionsResponseData.Where(s => s.Recordkey == "1").First();
                primarySectionData.SecXlist = "232";
                var meetingData1 = sectionMeetingResponseData.Where(s => s.CsmCourseSection == "4").First();
                meetingData1.CsmCourseSection = "1";


                List<string> sectionIds = new List<string>() { "1", "2", "3", "5" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section primarySection = sections.Where(s => s.Id == "1").FirstOrDefault();
                Section crosslistedSection = sections.Where(s => s.Id == "5").FirstOrDefault();
                Assert.AreEqual("1", primarySection.Id);
                Assert.AreEqual(1, primarySection.Meetings.Count());
                Assert.AreEqual(0, primarySection.PrimarySectionMeetings.Count());

                Assert.AreEqual("5", crosslistedSection.Id);
                Assert.AreEqual(1, crosslistedSection.Meetings.Count());
                Assert.AreEqual(0, crosslistedSection.PrimarySectionMeetings.Count());

            }

            [TestMethod]
            public async Task Validate_PrimarySectionMeetings_WithOverrideFlagY_CrossListedMeetingEmpty_PrimarySectionHaveMeetings()
            {
                //global parameter is Y and cross-listed paramter is empty
                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebUsePrimSecMtgFlag = "n";
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                //crosslist repsonse data is for section id =1 with recordkey=232 that is crosslisted to 1,5 sections
                crosslistResponseData[0].CsxlPrimSecMngOvrdeFlag = "y";
                //primary section is 1-> crosslisted to 1 and 5 sectionIds
                //course.section have crosslisted id ="232" for section id = 1
                //primary section have meeting ids 1 & 2
                var primarySectionData = sectionsResponseData.Where(s => s.Recordkey == "1").First();
                primarySectionData.SecXlist = "232";
                //make a section that is cross-listed section and do not have meetings
                var crosslistedSectionData = sectionsResponseData.Where(s => s.Recordkey == "5").First();
                crosslistedSectionData.SecMeeting = null;
                //modifying the reference of meetings to have primary section id. such as primary section =1 have two meeting data
                var meetingData = sectionMeetingResponseData.Where(s => s.CsmCourseSection == "5").First();
                meetingData.CsmCourseSection = "1";
                var meetingData1 = sectionMeetingResponseData.Where(s => s.CsmCourseSection == "4").First();
                meetingData1.CsmCourseSection = "1";


                List<string> sectionIds = new List<string>() { "1", "2", "3", "5" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section primarySection = sections.Where(s => s.Id == "1").FirstOrDefault();
                Section crosslistedSection = sections.Where(s => s.Id == "5").FirstOrDefault();
                Assert.AreEqual("1", primarySection.Id);
                Assert.AreEqual(2, primarySection.Meetings.Count());
                Assert.AreEqual(0, primarySection.PrimarySectionMeetings.Count());

                Assert.AreEqual("5", crosslistedSection.Id);
                Assert.AreEqual(0, crosslistedSection.Meetings.Count());
                Assert.AreEqual(2, crosslistedSection.PrimarySectionMeetings.Count());

                Assert.AreEqual(primarySection.Meetings[0].Id, crosslistedSection.PrimarySectionMeetings[0].Id);
                Assert.AreEqual(primarySection.Meetings[0].Days, crosslistedSection.PrimarySectionMeetings[0].Days);
                Assert.AreEqual(primarySection.Meetings[0].StartDate, crosslistedSection.PrimarySectionMeetings[0].StartDate);
                Assert.AreEqual(primarySection.Meetings[0].StartTime, crosslistedSection.PrimarySectionMeetings[0].StartTime);
                Assert.AreEqual(primarySection.Meetings[0].EndDate, crosslistedSection.PrimarySectionMeetings[0].EndDate);
                Assert.AreEqual(primarySection.Meetings[0].EndTime, crosslistedSection.PrimarySectionMeetings[0].EndTime);
                Assert.AreEqual(primarySection.Meetings[0].InstructionalMethodCode, crosslistedSection.PrimarySectionMeetings[0].InstructionalMethodCode);
                Assert.AreEqual(primarySection.Meetings[0].Room, crosslistedSection.PrimarySectionMeetings[0].Room);


                Assert.AreEqual(primarySection.Meetings[1].Id, crosslistedSection.PrimarySectionMeetings[1].Id);
                Assert.AreEqual(primarySection.Meetings[1].Days, crosslistedSection.PrimarySectionMeetings[1].Days);
                Assert.AreEqual(primarySection.Meetings[1].StartDate, crosslistedSection.PrimarySectionMeetings[1].StartDate);
                Assert.AreEqual(primarySection.Meetings[1].StartTime, crosslistedSection.PrimarySectionMeetings[1].StartTime);
                Assert.AreEqual(primarySection.Meetings[1].EndDate, crosslistedSection.PrimarySectionMeetings[1].EndDate);
                Assert.AreEqual(primarySection.Meetings[1].EndTime, crosslistedSection.PrimarySectionMeetings[1].EndTime);
                Assert.AreEqual(primarySection.Meetings[1].InstructionalMethodCode, crosslistedSection.PrimarySectionMeetings[1].InstructionalMethodCode);
                Assert.AreEqual(primarySection.Meetings[1].Room, crosslistedSection.PrimarySectionMeetings[1].Room);


            }

            [TestMethod]
            public async Task Validate_PrimarySectionMeetings_WithOverrideN_CrossListedMeetingEmtpy_PrimarySectionHaveMeetings()
            {
                //global parameter is N and cross-listed paramter is empty
                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebUsePrimSecMtgFlag = "y";
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                //crosslist repsonse data is for section id =1 with recordkey=232 that is crosslisted to 1,5 sections
                crosslistResponseData[0].CsxlPrimSecMngOvrdeFlag = "n";
                //primary section is 1-> crosslisted to 1 and 5 sectionIds
                //course.section have crosslisted id ="232" for section id = 1
                //primary section have meeting ids 1 & 2
                var crosslistedSectionData = sectionsResponseData.Where(s => s.Recordkey == "5").First();
                crosslistedSectionData.SecMeeting = null;

                var primarySectionData = sectionsResponseData.Where(s => s.Recordkey == "1").First();
                primarySectionData.SecXlist = "232";
                var meetingData = sectionMeetingResponseData.Where(s => s.CsmCourseSection == "5").First();
                meetingData.CsmCourseSection = "1";
                var meetingData1 = sectionMeetingResponseData.Where(s => s.CsmCourseSection == "4").First();
                meetingData1.CsmCourseSection = "1";


                List<string> sectionIds = new List<string>() { "1", "2", "3", "5" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section primarySection = sections.Where(s => s.Id == "1").FirstOrDefault();
                Section crosslistedSection = sections.Where(s => s.Id == "5").FirstOrDefault();
                Assert.AreEqual("1", primarySection.Id);
                Assert.AreEqual(2, primarySection.Meetings.Count());
                Assert.AreEqual(0, primarySection.PrimarySectionMeetings.Count());

                Assert.AreEqual("5", crosslistedSection.Id);
                Assert.AreEqual(0, crosslistedSection.Meetings.Count());
                Assert.AreEqual(0, crosslistedSection.PrimarySectionMeetings.Count());

            }

            [TestMethod]
            public async Task Validate_PrimarySectionMeetings_WithNoFlags_CrossListedMeetingEmtpy_PrimarySectionHaveMeetings()
            {
                //global parameter is N and cross-listed paramter is empty
                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.StwebUsePrimSecMtgFlag = string.Empty;
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stwebDefaults));
                //crosslist repsonse data is for section id =1 with recordkey=232 that is crosslisted to 1,5 sections
                crosslistResponseData[0].CsxlPrimSecMngOvrdeFlag = string.Empty;
                //primary section is 1-> crosslisted to 1 and 5 sectionIds
                //course.section have crosslisted id ="232" for section id = 1
                //primary section have meeting ids 1 & 2
                var crosslistedSectionData = sectionsResponseData.Where(s => s.Recordkey == "5").First();
                crosslistedSectionData.SecMeeting = null;

                var primarySectionData = sectionsResponseData.Where(s => s.Recordkey == "1").First();
                primarySectionData.SecXlist = "232";
                var meetingData = sectionMeetingResponseData.Where(s => s.CsmCourseSection == "5").First();
                meetingData.CsmCourseSection = "1";
                var meetingData1 = sectionMeetingResponseData.Where(s => s.CsmCourseSection == "4").First();
                meetingData1.CsmCourseSection = "1";


                List<string> sectionIds = new List<string>() { "1", "2", "3", "5" };
                IEnumerable<Section> sections = await sectionRepo.GetNonCachedSectionsAsync(sectionIds);
                Section primarySection = sections.Where(s => s.Id == "1").FirstOrDefault();
                Section crosslistedSection = sections.Where(s => s.Id == "5").FirstOrDefault();
                Assert.AreEqual("1", primarySection.Id);
                Assert.AreEqual(2, primarySection.Meetings.Count());
                Assert.AreEqual(0, primarySection.PrimarySectionMeetings.Count());

                Assert.AreEqual("5", crosslistedSection.Id);
                Assert.AreEqual(0, crosslistedSection.Meetings.Count());
                Assert.AreEqual(0, crosslistedSection.PrimarySectionMeetings.Count());

            }
            private SectionRepository BuildValidSectionRepository()
            {
                apiSettings = new ApiSettings("null");

                BuildLdmConfiguration(dataReaderMock, out cdDefaults);

                // Set up repo response for initial section request (1, 2, 3)
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).Returns(Task.FromResult(sectionsResponseData.Select(c => c.Recordkey).ToArray()));
                dataReaderMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), true))
                    .Returns<string, string[], bool>((file, ids, flag) => Task.FromResult(new Collection<CourseSections>(sectionsResponseData.Where(x => ids.Contains(x.Recordkey)).ToList())));

                // Set up repo response for "all" meeting requests
                dataReaderMock.Setup<Task<Collection<CourseSecMeeting>>>(macc => macc.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionMeetingResponseData));

                // Set up repo response for "all" faculty
                dataReaderMock.Setup<Task<Collection<CourseSecFaculty>>>(facc => facc.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionFacultyResponseData));

                dataReaderMock.Setup(ac => ac.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), "SAVING SCS.COURSE.SECTION")).ReturnsAsync(studentCourseSecResponseData.Select(c => c.CourseSectionIds).ToArray());
                dataReaderMock.Setup(ac => ac.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), "SAVING SCS.STUDENT")).ReturnsAsync(studentCourseSecResponseData.Select(c => c.StudentIds).ToArray());

                dataReaderMock.Setup<Task<Collection<PortalSites>>>(ps => ps.BulkReadRecordAsync<PortalSites>("PORTAL.SITES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(portalSitesResponseData));
                dataReaderMock.Setup<Task<Collection<CourseSecXlists>>>(sxl => sxl.BulkReadRecordAsync<CourseSecXlists>("COURSE.SEC.XLISTS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(crosslistResponseData));
                dataReaderMock.Setup<Task<Collection<CourseSecPending>>>(csp => csp.BulkReadRecordAsync<CourseSecPending>("COURSE.SEC.PENDING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(pendingResponseData));
                dataReaderMock.Setup<Task<Collection<WaitList>>>(wl => wl.BulkReadRecordAsync<WaitList>("WAIT.LIST", It.IsAny<string>(), true)).Returns(Task.FromResult(waitlistResponseData));

                // Set up repo response for section statuses
                var sectionStatuses = new ApplValcodes();
                sectionStatuses.ValsEntityAssociation = new List<ApplValcodesVals>();
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("A", "Active", "1", "A", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("I", "Inactive", "2", "I", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Cancelled", "", "C", "", "", ""));
                dataReaderMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", true)).ReturnsAsync(sectionStatuses);

                // Set up repo response for the temporary international parameter item
                Data.Base.DataContracts.IntlParams intlParams = new Data.Base.DataContracts.IntlParams();
                intlParams.HostDateDelimiter = "/";
                intlParams.HostShortDateFormat = "MDY";
                dataReaderMock.Setup<Task<Data.Base.DataContracts.IntlParams>>(iacc => iacc.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);

                // Set up course defaults response (indicates if coreq conversion has taken place)
                BuildCourseParametersConvertedResponse(cdDefaults);
                dataReaderMock.Setup<Task<CdDefaults>>(acc => acc.ReadRecordAsync<CdDefaults>("ST.PARMS", "CD.DEFAULTS", true)).ReturnsAsync(cdDefaults);

                // Set up instructional method response (indicates if sections are online)
                MockRecordsAsync("INSTR.METHODS", BuildValidInstrMethodResponse());

                //setup mocking for Stweb Defaults
                var stWebDflt = BuildStwebDefaults(); ;
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                    );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                   );

                // Mock the trxn getting the waitlist status
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Mock the read of instructional methods


                InstrMethods lec = new InstrMethods()
                {
                    InmDesc = "LEC",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                    Recordkey = "LEC"
                };
                InstrMethods lab = new InstrMethods()
                {
                    InmDesc = "LAB",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                    Recordkey = "LAB"
                };
                InstrMethods onl = new InstrMethods()
                {
                    InmDesc = "ONL",
                    InmOnline = "Y",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                    Recordkey = "ONL"
                };
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));

                GetSectionWaitlistStatusResponse wlResp = new GetSectionWaitlistStatusResponse() { ErrorMessages = new List<string>(), Status = "Wlst" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetSectionWaitlistStatusRequest, GetSectionWaitlistStatusResponse>(It.IsAny<GetSectionWaitlistStatusRequest>())).ReturnsAsync(wlResp);

                // Set up repo response for reg billing rates
                Collection<RegBillingRates> rbrs = new Collection<RegBillingRates>()
                {
                    new RegBillingRates()
                    {
                        Recordkey = "123",
                        RgbrAmtCalcType = "A",
                        RgbrArCode = "ABC",
                        RgbrChargeAmt = 50m,
                        RgbrRule = "RULE1",
                    },
                    new RegBillingRates()
                    {
                        Recordkey = "124",
                        RgbrAmtCalcType = "F",
                        RgbrArCode = "DEF",
                        RgbrCrAmt = 100m
                    },
                };
                dataReaderMock.Setup<Task<Collection<RegBillingRates>>>(cacc => cacc.BulkReadRecordAsync<RegBillingRates>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(rbrs);
                var bookOptions = new ApplValcodes();
                bookOptions.ValsEntityAssociation = new List<ApplValcodesVals>();
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("R", "Required", "1", "R", "", "", ""));
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Recommended", "2", "C", "", "", ""));
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("O", "Optional", "2", "O", "", "", ""));
                dataReaderMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BOOK.OPTION", true)).ReturnsAsync(bookOptions);
                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                return sectionRepo;
            }

            private SectionRepository BuildInvalidSectionRepository()
            {
                apiSettings = new ApiSettings("null");

                // Set up repo response for "all" section requests
                Exception expectedFailure = new Exception("fail");
                dataReaderMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), true)).Throws(expectedFailure);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                return sectionRepo;
            }

            private Collection<CourseSections> BuildSectionsResponse(IEnumerable<Section> sections)
            {
                var repoSections = new Collection<CourseSections>();
                foreach (var section in sections)
                {
                    var crsSec = new CourseSections();
                    crsSec.RecordGuid = section.Guid;
                    crsSec.Recordkey = section.Id;
                    crsSec.SecAcadLevel = section.AcademicLevelCode;
                    crsSec.SecCeus = section.Ceus;
                    crsSec.SecCourse = section.CourseId;
                    crsSec.SecCourseLevels = section.CourseLevelCodes.ToList();
                    crsSec.SecDepartmentsEntityAssociation = section.Departments.Select(x => new CourseSectionsSecDepartments(x.AcademicDepartmentCode, x.ResponsibilityPercentage)).ToList();
                    crsSec.SecLocation = section.Location;
                    crsSec.SecMaxCred = section.MaximumCredits;
                    crsSec.SecVarCredIncrement = section.VariableCreditIncrement;
                    crsSec.SecMinCred = section.MinimumCredits;
                    crsSec.SecNo = section.Number;
                    crsSec.SecShortTitle = section.Title;
                    crsSec.SecCredType = section.CreditTypeCode;
                    crsSec.SecStartDate = section.StartDate;
                    crsSec.SecEndDate = section.EndDate;
                    crsSec.SecTerm = section.TermId;
                    crsSec.SecOnlyPassNopassFlag = section.OnlyPassNoPass ? "Y" : "N";
                    crsSec.SecAllowPassNopassFlag = section.AllowPassNoPass ? "Y" : "N";
                    crsSec.SecAllowAuditFlag = section.AllowAudit ? "Y" : "N";
                    var csm = new List<string>() { "1", "2" };
                    crsSec.SecMeeting = csm;
                    var csf = new List<string>() { "1", "2" };
                    crsSec.SecFaculty = csf;
                    var sas = new List<string>() { "1", "2" };
                    crsSec.SecActiveStudents = sas;
                    crsSec.SecStatusesEntityAssociation = section.Statuses.Select(x => new CourseSectionsSecStatuses(x.Date, ConvertSectionStatusToCode(x.Status))).ToList();
                    crsSec.SecCourseTypes = section.CourseTypeCodes.ToList();
                    crsSec.SecAllowWaitlistFlag = section.AllowWaitlist ? "Y" : "N";

                    // Reconstruct all Colleague requisite and corequisite fields from the data in the Requisites 
                    // post-conversion fields
                    crsSec.SecReqs = new List<string>();
                    crsSec.SecRecommendedSecs = new List<string>();
                    crsSec.SecCoreqSecs = new List<string>();
                    crsSec.SecMinNoCoreqSecs = null;
                    crsSec.SecOverrideCrsReqsFlag = section.OverridesCourseRequisites ? "Y" : "";
                    // pre-conversion fields
                    crsSec.SecCourseCoreqsEntityAssociation = new List<CourseSectionsSecCourseCoreqs>();
                    crsSec.SecCoreqsEntityAssociation = new List<CourseSectionsSecCoreqs>();

                    foreach (var req in section.Requisites)
                    {
                        if (!string.IsNullOrEmpty(req.RequirementCode))
                        {
                            // post-conversion
                            crsSec.SecReqs.Add(req.RequirementCode);
                            // pre-conversion -- this does not convert
                        }
                        else if (!string.IsNullOrEmpty(req.CorequisiteCourseId))
                        {
                            // post-conversion -- this does not convert
                            // pre-conversion
                            crsSec.SecCourseCoreqsEntityAssociation.Add(
                                new CourseSectionsSecCourseCoreqs(req.CorequisiteCourseId, (req.IsRequired) ? "Y" : "")
                            );
                        }

                    }

                    foreach (var req in section.SectionRequisites)
                    {
                        if (req.CorequisiteSectionIds != null && req.CorequisiteSectionIds.Count() > 0)
                        {
                            foreach (var secId in req.CorequisiteSectionIds)
                            {
                                // post-conversion--put required and recommended into two separate lists
                                if (req.IsRequired)
                                {
                                    crsSec.SecCoreqSecs.Add(secId);
                                    // The minimum number is associated with the entire list of SecCoreqSecs
                                    crsSec.SecMinNoCoreqSecs = req.NumberNeeded;
                                }
                                else
                                {
                                    crsSec.SecRecommendedSecs.Add(secId);
                                }
                                // pre-conversion--each requisite section has a required flag
                                crsSec.SecCoreqsEntityAssociation.Add(
                                    new CourseSectionsSecCoreqs(secId, (req.IsRequired) ? "Y" : "")
                                    );
                            }
                        }
                    }

                    if (section.Books != null)
                    {
                        crsSec.SecBooks = new List<string>();
                        crsSec.SecBookOptions = new List<string>();
                        foreach (var book in section.Books)
                        {
                            crsSec.SecBooks.Add(book.BookId);

                            crsSec.SecBookOptions.Add(book.RequirementStatusCode);



                        }
                    }
                    crsSec.SecPortalSite = ""; // (!string.IsNullOrEmpty(section.LearningProvider) ? crsSec.Recordkey : "");
                    if (!string.IsNullOrEmpty(section.LearningProvider))
                    {
                        crsSec.SecPortalSite = section.Id;
                    }
                    else
                    {
                        crsSec.SecPortalSite = "";
                    }
                    if (section.Id == "1")
                    {
                        crsSec.SecXlist = "234";
                        crsSec.SecCapacity = 10;
                        crsSec.SecSubject = "MATH";
                        crsSec.SecCourseNo = "1000";
                    }
                    if (section.Id == "6")
                    {
                        crsSec.SecSubject = "ART";
                        crsSec.SecCourseNo = "2233";
                    }

                    crsSec.SecInstrMethods = new List<string>() { "LEC" };
                    repoSections.Add(crsSec);
                }
                return repoSections;
            }

            private Collection<CourseSecMeeting> BuildSectionMeetingsResponse(IEnumerable<Section> sections)
            {
                Collection<CourseSecMeeting> repoSecMeetings = new Collection<CourseSecMeeting>();
                int crsSecMId = 0;
                foreach (var section in sections)
                {
                    foreach (var mt in section.Meetings)
                    {
                        var crsSecM = new CourseSecMeeting();
                        crsSecMId += 1;
                        crsSecM.Recordkey = crsSecMId.ToString();
                        if (!string.IsNullOrEmpty(mt.Room))
                        {
                            crsSecM.CsmBldg = "ABLE";
                            crsSecM.CsmRoom = "A100";
                        }
                        crsSecM.CsmCourseSection = section.Id;
                        crsSecM.CsmInstrMethod = mt.InstructionalMethodCode;
                        crsSecM.CsmStartTime = mt.StartTime.HasValue ? mt.StartTime.Value.DateTime : (DateTime?)null;
                        crsSecM.CsmEndTime = mt.EndTime.HasValue ? mt.EndTime.Value.DateTime : (DateTime?)null;
                        foreach (var d in mt.Days)
                        {
                            switch (d)
                            {
                                case DayOfWeek.Friday:
                                    crsSecM.CsmFriday = "Y";
                                    break;
                                case DayOfWeek.Monday:
                                    crsSecM.CsmMonday = "Y";
                                    break;
                                case DayOfWeek.Saturday:
                                    crsSecM.CsmSaturday = "Y";
                                    break;
                                case DayOfWeek.Sunday:
                                    crsSecM.CsmSunday = "Y";
                                    break;
                                case DayOfWeek.Thursday:
                                    crsSecM.CsmThursday = "Y";
                                    break;
                                case DayOfWeek.Tuesday:
                                    crsSecM.CsmTuesday = "Y";
                                    break;
                                case DayOfWeek.Wednesday:
                                    crsSecM.CsmWednesday = "Y";
                                    break;
                                default:
                                    break;
                            }
                        }
                        repoSecMeetings.Add(crsSecM);
                    }

                }
                return repoSecMeetings;
            }

            private Collection<CourseSecFaculty> BuildSectionFacultyResponse(IEnumerable<Section> sections)
            {
                Collection<CourseSecFaculty> repoSecFaculty = new Collection<CourseSecFaculty>();
                int crsSecFId = 0;
                foreach (var section in sections)
                {
                    foreach (var fac in section.FacultyIds)
                    {
                        var crsSecF = new CourseSecFaculty();
                        crsSecFId += 1;
                        crsSecF.Recordkey = crsSecFId.ToString();
                        crsSecF.CsfCourseSection = section.Id;
                        crsSecF.CsfFaculty = fac;
                        repoSecFaculty.Add(crsSecF);
                    }

                }
                return repoSecFaculty;
            }

            private List<StudentCourseSectionStudents> BuildStudentCourseSecStudents(IEnumerable<Section> sections)
            {
                var studentCourseSectionStudents = new List<StudentCourseSectionStudents>();
                foreach (var section in sections)
                {
                    foreach (var stu in section.ActiveStudentIds)
                    {
                        var scss = new StudentCourseSectionStudents();
                        scss.CourseSectionIds = section.Id;
                        scss.StudentIds = stu;
                        studentCourseSectionStudents.Add(scss);
                    }
                }
                return studentCourseSectionStudents;
            }

            private Collection<PortalSites> BuildPortalSitesResponse(IEnumerable<Section> sections)
            {
                var repoPS = new Collection<PortalSites>();
                foreach (var section in sections)
                {
                    if (section.CourseId == "7272")
                    {
                        var ps = new PortalSites();
                        // normally some thing like "HIST-190-001-cs11347", but mock portal site Id with section ID
                        ps.Recordkey = section.Id;
                        if (section.Number == "98")
                        {
                            ps.PsLearningProvider = "";
                            ps.PsPrtlSiteGuid = section.Id;
                        }
                        if (section.Number == "97")
                        {
                            ps.PsLearningProvider = "MOODLE";
                            ps.PsPrtlSiteGuid = section.Id;
                        }
                        repoPS.Add(ps);
                    }
                }
                return repoPS;
            }

            private Collection<CourseSecXlists> BuildCrosslistResponse(IEnumerable<Section> sections)
            {
                // currently built only for ILP testing
                var repoXL = new Collection<CourseSecXlists>();
                foreach (var section in sections)
                {
                    if (section.Id == "1")
                    {
                        var xl = new CourseSecXlists();
                        xl.Recordkey = "232";
                        xl.CsxlPrimarySection = section.Id;
                        xl.CsxlCourseSections = new List<string>() { section.Id, "5" };
                        xl.CsxlCapacity = 20;
                        xl.CsxlWaitlistMax = 5;
                        xl.CsxlWaitlistFlag = "Y";
                        repoXL.Add(xl);
                    }
                }
                return repoXL;
            }

            private Collection<CourseSecPending> BuildPendingSectionResponse(IEnumerable<Section> sections)
            {
                var repoPending = new Collection<CourseSecPending>();
                foreach (var section in sections)
                {
                    if (section.CourseId == "7272" && (section.Number == "98" || section.Number == "97"))
                    {
                        var cp = new CourseSecPending();
                        cp.Recordkey = section.Id;
                        if (section.Number == "98")
                        {
                            cp.CspReservedSeats = 1;
                        }
                        if (section.Number == "97")
                        {
                            cp.CspReservedSeats = 2;
                        }
                        repoPending.Add(cp);
                    }
                }
                return repoPending;
            }

            private Collection<WaitList> BuildWaitlistResponse(IEnumerable<Section> sections)
            {
                var repoWaitlist = new Collection<WaitList>();
                foreach (var section in sections)
                {
                    if (section.CourseId == "7272" && (section.Number == "98" || section.Number == "97"))
                    {
                        var wl = new WaitList();
                        wl.Recordkey = section.Id;
                        if (section.Number == "98")
                        {
                            wl.WaitCourseSection = section.Id;
                            wl.WaitStatus = "P";
                            wl.WaitStudent = "111111";
                        }
                        if (section.Number == "97")
                        {
                            wl.WaitCourseSection = section.Id;
                            wl.WaitStatus = "P";
                            wl.WaitStudent = "22222";
                        }
                        repoWaitlist.Add(wl);
                    }
                }
                return repoWaitlist;
            }

            private void BuildCourseParametersConvertedResponse(CdDefaults defaults)
            {
                // Converted Response
                defaults.CdReqsConvertedFlag = "Y";
            }
        }

        [TestClass]
        public class SectionRepository_PostSectionMeeting : SectionRepositoryTests
        {
            string secGuid, meet1Guid, meet2Guid, meet3Guid;
            Section section;
            SectionMeeting meeting1, meeting2, meeting3, secMeet;
            CourseSecMeeting csm;
            CourseSecFaculty csf;
            string faculty1, faculty2, faculty3;
            SectionFaculty lecFaculty, labFaculty1, labFaculty2, semFaculty;
            DateTime secStartDate, secEndDate, semStartDate, semEndDate;
            DateTimeOffset? lecStartTime, lecEndTime, labStartTime, labEndTime, semStartTime, semEndTime;
            List<OfferingDepartment> depts = new List<OfferingDepartment>();
            List<SectionStatusItem> statuses = new List<SectionStatusItem>();
            List<DayOfWeek> MWF = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
            List<DayOfWeek> TTh = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday };
            List<DayOfWeek> Wed = new List<DayOfWeek>() { DayOfWeek.Wednesday };
            List<SectionMeeting> meetings = new List<SectionMeeting>();
            List<SectionFaculty> faculty = new List<SectionFaculty>();

            UpdateInstructionalEventRequest request;
            UpdateInstructionalEventResponse response;
            SectionRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();

                secGuid = Guid.NewGuid().ToString();
                secStartDate = new DateTime(2014, 9, 2);
                secEndDate = new DateTime(2014, 12, 5);
                depts.Add(new OfferingDepartment("RECR", 75m));
                depts.Add(new OfferingDepartment("CECR", 25m));
                var courseLevelCodes = new List<string>() { "100", "CE" };
                statuses.Add(new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2011, 9, 28)));
                section = new Section("1", "1", "01", secStartDate, 3.00m, null, "Underwater Basketweaving", "IN", depts, courseLevelCodes, "CE", statuses) { Guid = secGuid };

                faculty1 = "1234567";
                faculty2 = "2345678";
                faculty3 = "3456789";
                meet1Guid = Guid.NewGuid().ToString();
                meet2Guid = Guid.NewGuid().ToString();
                meet3Guid = Guid.NewGuid().ToString();
                lecStartTime = (new DateTime(1, 1, 1, 9, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                lecEndTime = (new DateTime(1, 1, 1, 9, 50, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                labStartTime = (new DateTime(1, 1, 1, 13, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                labEndTime = (new DateTime(1, 1, 1, 16, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                meeting1 = new SectionMeeting("1", "1", "LEC", secStartDate, secEndDate, "W") { Days = MWF, StartTime = lecStartTime, EndTime = lecEndTime, Room = "ARM*240", Load = 20m, IsOnline = false, Guid = meet1Guid };
                meeting1.AddFacultyId(faculty1);
                meeting2 = new SectionMeeting("2", "1", "LAB", secStartDate, secEndDate, "W") { Days = TTh, StartTime = labStartTime, EndTime = labEndTime, Room = "ARM*131", Load = 10m, IsOnline = false, Guid = meet2Guid };
                meeting2.AddFacultyId(faculty1);
                meeting2.AddFacultyId(faculty2);

                semStartDate = new DateTime(2014, 10, 1);
                semEndDate = new DateTime(2014, 10, 29);
                semStartTime = (new DateTime(1, 1, 1, 19, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                semEndTime = (new DateTime(1, 1, 1, 22, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                meeting3 = new SectionMeeting(null, "1", "SEM", semStartDate, semEndDate, "W") { Days = Wed, StartTime = semStartTime, EndTime = semEndTime, Room = "JFK*200", Load = 5m, IsOnline = false, Guid = meet3Guid };
                meeting3.AddFacultyId(faculty3);

                lecFaculty = new SectionFaculty("1", "1", faculty1, "LEC", secStartDate, secEndDate, 100m) { LoadFactor = 20m };
                labFaculty1 = new SectionFaculty("2", "1", faculty1, "LAB", secStartDate, secEndDate, 50m) { LoadFactor = 5m };
                labFaculty2 = new SectionFaculty("3", "1", faculty2, "LAB", secStartDate, secEndDate, 50m) { LoadFactor = 5m };
                semFaculty = new SectionFaculty(null, "1", faculty3, "SEM", semStartDate, semEndDate, 100m) { LoadFactor = 5m };

                meeting1.AddSectionFaculty(lecFaculty);
                meeting2.AddSectionFaculty(labFaculty1);
                meeting2.AddSectionFaculty(labFaculty2);
                meeting3.AddSectionFaculty(semFaculty);

                section.AddSectionMeeting(meeting1);
                section.AddSectionMeeting(meeting2);
                section.AddSectionMeeting(meeting3);
                section.AddSectionFaculty(lecFaculty);
                section.AddSectionFaculty(labFaculty1);
                section.AddSectionFaculty(labFaculty2);
                section.AddSectionFaculty(semFaculty);

                meetings.Add(meeting1);
                meetings.Add(meeting2);
                meetings.Add(meeting3);
                faculty.Add(lecFaculty);
                faculty.Add(labFaculty1);
                faculty.Add(labFaculty2);
                faculty.Add(semFaculty);

                request = new UpdateInstructionalEventRequest();
                response = new UpdateInstructionalEventResponse()
                {
                    UpdateInstructionalEventErrors = new List<UpdateInstructionalEventErrors>(),
                    UpdateInstructionalEventWarnings = new List<UpdateInstructionalEventWarnings>()
                };

                //newMeeting = new SectionMeeting("5", "1", "SEM", semStartDate, semEndDate, "W") { Days = Wed, StartTime = semStartTime, EndTime = semEndTime, Room = "JFK*200", Load = 5m, IsOnline = false, Guid = Guid.NewGuid().ToString().ToLowerInvariant() };
                csm = new CourseSecMeeting()
                {
                    CsmBldg = "JFK",
                    CsmCourseSection = "1",
                    CsmEndDate = semEndDate,
                    CsmEndTime = semEndTime.Value.DateTime,
                    CsmFaculty = new List<string>() { faculty3 },
                    CsmFrequency = "W",
                    CsmFriday = "N",
                    CsmInstrMethod = "SEM",
                    CsmLoad = 5m,
                    CsmMonday = "N",
                    CsmRoom = "200",
                    CsmSaturday = "N",
                    CsmStartDate = semStartDate,
                    CsmStartTime = semStartTime.Value.DateTime,
                    CsmSunday = "N",
                    CsmThursday = "N",
                    CsmTuesday = "N",
                    CsmWednesday = "Y",
                    RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                    Recordkey = "4"
                };
                csf = new CourseSecFaculty()
                {
                    CsfCourseSection = "1",
                    CsfEndDate = semEndDate,
                    CsfFaculty = faculty3,
                    CsfFacultyLoad = 5m,
                    CsfFacultyPct = 100m,
                    CsfInstrMethod = "SEM",
                    CsfStartDate = semStartDate,
                    Recordkey = "5"
                };

                CdDefaults cdDefaults;
                BuildLdmConfiguration(dataReaderMock, out cdDefaults);

                dataReaderMock.Setup(r => r.ReadRecordAsync<CourseSecMeeting>(It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                    {
                        if (id == "4")
                        {
                            return Task.FromResult(csm);
                        }
                        var mtg = meetings.FirstOrDefault(x => x.Id == id);
                        if (mtg == null) return Task.FromResult(new CourseSecMeeting());

                        var room = mtg.Room.Contains('*') ? mtg.Room.Split('*') : new string[2] { mtg.Room, string.Empty };
                        return Task.FromResult(new CourseSecMeeting()
                        {
                            Recordkey = mtg.Id,
                            RecordGuid = mtg.Guid,
                            CsmBldg = room[0],
                            CsmCourseSection = mtg.SectionId,
                            CsmEndDate = mtg.EndDate,
                            CsmEndTime = mtg.EndTime.Value.DateTime,
                            CsmFaculty = mtg.FacultyIds.ToList(),
                            CsmFrequency = mtg.Frequency,
                            CsmInstrMethod = mtg.InstructionalMethodCode,
                            CsmLoad = mtg.Load,
                            CsmRoom = room[1],
                            CsmStartDate = mtg.StartDate,
                            CsmStartTime = mtg.StartTime.Value.DateTime,
                            CsmSunday = mtg.Days.Contains(DayOfWeek.Sunday) ? "Y" : "N",
                            CsmMonday = mtg.Days.Contains(DayOfWeek.Monday) ? "Y" : "N",
                            CsmTuesday = mtg.Days.Contains(DayOfWeek.Tuesday) ? "Y" : "N",
                            CsmWednesday = mtg.Days.Contains(DayOfWeek.Wednesday) ? "Y" : "N",
                            CsmThursday = mtg.Days.Contains(DayOfWeek.Thursday) ? "Y" : "N",
                            CsmFriday = mtg.Days.Contains(DayOfWeek.Friday) ? "Y" : "N",
                            CsmSaturday = mtg.Days.Contains(DayOfWeek.Saturday) ? "Y" : "N"
                        });
                    });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<CourseSecFaculty>(It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                    {
                        var results = new Collection<CourseSecFaculty>() { csf };
                        foreach (var fac in faculty)
                        {
                            results.Add(new CourseSecFaculty()
                            {
                                CsfCourseSection = request.CsmCourseSection,
                                CsfEndDate = fac.EndDate,
                                CsfFaculty = fac.FacultyId,
                                CsfFacultyLoad = fac.LoadFactor,
                                CsfFacultyPct = fac.ResponsibilityPercentage,
                                CsfInstrMethod = fac.InstructionalMethodCode,
                                CsfStartDate = fac.StartDate,
                                CsfPacLpAsgmt = fac.ContractAssignment,
                                CsfTeachingArrangement = fac.TeachingArrangementCode,
                                Recordkey = fac.Id
                            });
                        }
                        return Task.FromResult(results);
                    });
                transManagerMock.Setup(t => t.ExecuteAsync<UpdateInstructionalEventRequest, UpdateInstructionalEventResponse>(It.IsAny<UpdateInstructionalEventRequest>())).Returns(Task.FromResult(response));

                // Set up response for instructional methods
                var instrMethods = BuildValidInstrMethodResponse();
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>("INSTR.METHODS", "", true)).ReturnsAsync(instrMethods);

                //setup mocking for Stweb Defaults
                var stWebDflt = BuildStwebDefaults(); ;
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                    );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                   );
                repository = new SectionRepository(cacheProvider, transFactory, logger, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);



            }

            [TestMethod]
            public async Task SectionRepository_PostSectionMeeting_AddNewMeeting()
            {
                response.CourseSecMeetingId = "4";

                secMeet = await repository.PostSectionMeetingAsync(section, meet3Guid);
                Assert.AreEqual("4", secMeet.Id);
            }

            [TestMethod]
            public async Task SectionRepository_PostSectionMeeting_ChangeMeetingDates()
            {
                response.CourseSecMeetingId = "1";

                var start = secStartDate.AddDays(30);
                var end = secEndDate.AddDays(30);
                section.Meetings[0].StartDate = start;
                section.Meetings[0].EndDate = end;
                secMeet = await repository.PostSectionMeetingAsync(section, meet1Guid);
                Assert.AreEqual(start, secMeet.StartDate);
                Assert.AreEqual(end, secMeet.EndDate);
            }

            [TestMethod]
            public async Task SectionRepository_PostSectionMeeting_ChangeMeetingTimes()
            {
                response.CourseSecMeetingId = "1";

                var start = lecStartTime.Value.AddHours(1);
                var end = lecEndTime.Value.AddHours(1);
                section.Meetings[0].StartTime = start;
                section.Meetings[0].EndTime = end;
                secMeet = await repository.PostSectionMeetingAsync(section, meet1Guid);
                Assert.AreEqual(start.ToLocalTime(), secMeet.StartTime.Value.ToLocalTime());
                Assert.AreEqual(end.ToLocalTime(), secMeet.EndTime.Value.ToLocalTime());
            }

            [TestMethod]
            public async Task SectionRepository_PostSectionMeeting_ChangeMeetingRoom()
            {
                response.CourseSecMeetingId = "1";

                var room = "ARM*140";
                section.Meetings[0].Room = room;
                secMeet = await repository.PostSectionMeetingAsync(section, meet1Guid);
                Assert.AreEqual(room, secMeet.Room);
            }

            [TestMethod]
            public async Task SectionRepository_PostSectionMeeting_ChangeMeetingFaculty()
            {
                response.CourseSecMeetingId = "1";

                section.Meetings[0].RemoveFacultyId(faculty1);
                section.Meetings[0].AddFacultyId(faculty3);
                secMeet = await repository.PostSectionMeetingAsync(section, meet1Guid);
                Assert.AreEqual(1, secMeet.FacultyIds.Count);
                Assert.AreEqual(faculty3, secMeet.FacultyIds[0]);
            }
        }

        [TestClass]
        public class SectionRepository_PutSectionMeeting : SectionRepositoryTests
        {
            string secGuid, meet1Guid, meet2Guid, meet3Guid;
            Section section;
            SectionMeeting meeting1, meeting2, meeting3, secMeet;
            IEnumerable<SectionMeeting> secMeets;
            CourseSecMeeting csm;
            CourseSecFaculty csf;
            string faculty1, faculty2, faculty3;
            SectionFaculty lecFaculty, labFaculty1, labFaculty2, semFaculty;
            DateTime secStartDate, secEndDate, semStartDate, semEndDate;
            DateTimeOffset? lecStartTime, lecEndTime, labStartTime, labEndTime, semStartTime, semEndTime;
            List<OfferingDepartment> depts = new List<OfferingDepartment>();
            List<SectionStatusItem> statuses = new List<SectionStatusItem>();
            List<DayOfWeek> MWF = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
            List<DayOfWeek> TTh = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday };
            List<DayOfWeek> Wed = new List<DayOfWeek>() { DayOfWeek.Wednesday };
            List<SectionMeeting> meetings = new List<SectionMeeting>();
            List<SectionFaculty> faculty = new List<SectionFaculty>();

            UpdateInstructionalEventRequest request;
            UpdateInstructionalEventResponse response;
            SectionRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();

                secGuid = Guid.NewGuid().ToString();
                secStartDate = new DateTime(2014, 9, 2);
                secEndDate = new DateTime(2014, 12, 5);
                depts.Add(new OfferingDepartment("RECR", 75m));
                depts.Add(new OfferingDepartment("CECR", 25m));
                var courseLevelCodes = new List<string>() { "100", "CE" };
                statuses.Add(new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2011, 9, 28)));
                section = new Section("1", "1", "01", secStartDate, 3.00m, null, "Underwater Basketweaving", "IN", depts, courseLevelCodes, "CE", statuses) { Guid = secGuid };

                faculty1 = "1234567";
                faculty2 = "2345678";
                faculty3 = "3456789";
                meet1Guid = Guid.NewGuid().ToString();
                meet2Guid = Guid.NewGuid().ToString();
                meet3Guid = Guid.NewGuid().ToString();
                lecStartTime = (new DateTime(1, 1, 1, 9, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                lecEndTime = (new DateTime(1, 1, 1, 9, 50, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                labStartTime = (new DateTime(1, 1, 1, 13, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                labEndTime = (new DateTime(1, 1, 1, 16, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                meeting1 = new SectionMeeting("1", "1", "LEC", secStartDate, secEndDate, "W") { Days = MWF, StartTime = lecStartTime, EndTime = lecEndTime, Room = "ARM*240", Load = 20m, IsOnline = false, Guid = meet1Guid };
                meeting1.AddFacultyId(faculty1);
                meeting2 = new SectionMeeting("2", "1", "LAB", secStartDate, secEndDate, "W") { Days = TTh, StartTime = labStartTime, EndTime = labEndTime, Room = "ARM*131", Load = 10m, IsOnline = false, Guid = meet2Guid };
                meeting2.AddFacultyId(faculty1);
                meeting2.AddFacultyId(faculty2);

                semStartDate = new DateTime(2014, 10, 1);
                semEndDate = new DateTime(2014, 10, 29);
                semStartTime = (new DateTime(1, 1, 1, 19, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                semEndTime = (new DateTime(1, 1, 1, 22, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                meeting3 = new SectionMeeting(null, "1", "SEM", semStartDate, semEndDate, "W") { Days = Wed, StartTime = semStartTime, EndTime = semEndTime, Room = "JFK*200", Load = 5m, IsOnline = false, Guid = meet3Guid };
                meeting3.AddFacultyId(faculty3);

                lecFaculty = new SectionFaculty("1", "1", faculty1, "LEC", secStartDate, secEndDate, 100m) { LoadFactor = 20m };
                labFaculty1 = new SectionFaculty("2", "1", faculty1, "LAB", secStartDate, secEndDate, 50m) { LoadFactor = 5m };
                labFaculty2 = new SectionFaculty("3", "1", faculty2, "LAB", secStartDate, secEndDate, 50m) { LoadFactor = 5m };
                semFaculty = new SectionFaculty(null, "1", faculty3, "SEM", semStartDate, semEndDate, 100m) { LoadFactor = 5m };

                meeting1.AddSectionFaculty(lecFaculty);
                meeting2.AddSectionFaculty(labFaculty1);
                meeting2.AddSectionFaculty(labFaculty2);
                meeting3.AddSectionFaculty(semFaculty);

                section.AddSectionMeeting(meeting1);
                section.AddSectionMeeting(meeting2);
                section.AddSectionMeeting(meeting3);
                section.AddSectionFaculty(lecFaculty);
                section.AddSectionFaculty(labFaculty1);
                section.AddSectionFaculty(labFaculty2);
                section.AddSectionFaculty(semFaculty);

                meetings.Add(meeting1);
                meetings.Add(meeting2);
                meetings.Add(meeting3);
                faculty.Add(lecFaculty);
                faculty.Add(labFaculty1);
                faculty.Add(labFaculty2);
                faculty.Add(semFaculty);

                request = new UpdateInstructionalEventRequest();
                response = new UpdateInstructionalEventResponse()
                {
                    UpdateInstructionalEventErrors = new List<UpdateInstructionalEventErrors>(),
                    UpdateInstructionalEventWarnings = new List<UpdateInstructionalEventWarnings>()
                };

                //newMeeting = new SectionMeeting("5", "1", "SEM", semStartDate, semEndDate, "W") { Days = Wed, StartTime = semStartTime, EndTime = semEndTime, Room = "JFK*200", Load = 5m, IsOnline = false, Guid = Guid.NewGuid().ToString().ToLowerInvariant() };
                csm = new CourseSecMeeting()
                {
                    CsmBldg = "JFK",
                    CsmCourseSection = "1",
                    CsmEndDate = semEndDate,
                    CsmEndTime = semEndTime.Value.DateTime,
                    CsmFaculty = new List<string>() { faculty3 },
                    CsmFrequency = "W",
                    CsmFriday = "N",
                    CsmInstrMethod = "SEM",
                    CsmLoad = 5m,
                    CsmMonday = "N",
                    CsmRoom = "200",
                    CsmSaturday = "N",
                    CsmStartDate = semStartDate,
                    CsmStartTime = semStartTime.Value.DateTime,
                    CsmSunday = "N",
                    CsmThursday = "N",
                    CsmTuesday = "N",
                    CsmWednesday = "Y",
                    RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                    Recordkey = "4"
                };
                csf = new CourseSecFaculty()
                {
                    CsfCourseSection = "1",
                    CsfEndDate = semEndDate,
                    CsfFaculty = faculty3,
                    CsfFacultyLoad = 5m,
                    CsfFacultyPct = 100m,
                    CsfInstrMethod = "SEM",
                    CsfStartDate = semStartDate,
                    Recordkey = "5"
                };

                CdDefaults cdDefaults;
                BuildLdmConfiguration(dataReaderMock, out cdDefaults);

                dataReaderMock.Setup(r => r.ReadRecordAsync<CourseSecMeeting>(It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                {
                    if (id == "4")
                    {
                        return Task.FromResult(csm);
                    }
                    var mtg = meetings.FirstOrDefault(x => x.Id == id);
                    if (mtg == null) return Task.FromResult(new CourseSecMeeting());

                    var room = mtg.Room.Contains('*') ? mtg.Room.Split('*') : new string[2] { mtg.Room, string.Empty };
                    return Task.FromResult(new CourseSecMeeting()
                    {
                        Recordkey = mtg.Id,
                        RecordGuid = mtg.Guid,
                        CsmBldg = room[0],
                        CsmCourseSection = mtg.SectionId,
                        CsmEndDate = mtg.EndDate,
                        CsmEndTime = mtg.EndTime.Value.DateTime,
                        CsmFaculty = mtg.FacultyIds.ToList(),
                        CsmFrequency = mtg.Frequency,
                        CsmInstrMethod = mtg.InstructionalMethodCode,
                        CsmLoad = mtg.Load,
                        CsmRoom = room[1],
                        CsmStartDate = mtg.StartDate,
                        CsmStartTime = mtg.StartTime.Value.DateTime,
                        CsmSunday = mtg.Days.Contains(DayOfWeek.Sunday) ? "Y" : "N",
                        CsmMonday = mtg.Days.Contains(DayOfWeek.Monday) ? "Y" : "N",
                        CsmTuesday = mtg.Days.Contains(DayOfWeek.Tuesday) ? "Y" : "N",
                        CsmWednesday = mtg.Days.Contains(DayOfWeek.Wednesday) ? "Y" : "N",
                        CsmThursday = mtg.Days.Contains(DayOfWeek.Thursday) ? "Y" : "N",
                        CsmFriday = mtg.Days.Contains(DayOfWeek.Friday) ? "Y" : "N",
                        CsmSaturday = mtg.Days.Contains(DayOfWeek.Saturday) ? "Y" : "N"
                    });
                });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<CourseSecFaculty>(It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                {
                    var results = new Collection<CourseSecFaculty>() { csf };
                    foreach (var fac in faculty)
                    {
                        results.Add(new CourseSecFaculty()
                        {
                            CsfCourseSection = request.CsmCourseSection,
                            CsfEndDate = fac.EndDate,
                            CsfFaculty = fac.FacultyId,
                            CsfFacultyLoad = fac.LoadFactor,
                            CsfFacultyPct = fac.ResponsibilityPercentage,
                            CsfInstrMethod = fac.InstructionalMethodCode,
                            CsfStartDate = fac.StartDate,
                            CsfPacLpAsgmt = fac.ContractAssignment,
                            CsfTeachingArrangement = fac.TeachingArrangementCode,
                            Recordkey = fac.Id
                        });
                    }
                    return Task.FromResult(results);
                });
                transManagerMock.Setup(t => t.ExecuteAsync<UpdateInstructionalEventRequest, UpdateInstructionalEventResponse>(It.IsAny<UpdateInstructionalEventRequest>())).Returns(Task.FromResult(response));

                // Set up response for instructional methods
                var instrMethods = BuildValidInstrMethodResponse();
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>("INSTR.METHODS", "", true)).ReturnsAsync(instrMethods);

                //setup mocking for Stweb Defaults
                var stWebDflt = BuildStwebDefaults(); ;
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                    );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                   );
                repository = new SectionRepository(cacheProvider, transFactory, logger, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);



            }

            [TestMethod]
            public async Task SectionRepository_PutSectionMeeting_AddNewMeeting()
            {
                response.CourseSecMeetingId = "4";

                secMeet = await repository.PutSectionMeetingAsync(section, meet3Guid);
                Assert.AreEqual("4", secMeet.Id);
            }

            [TestMethod]
            public async Task SectionRepository_PutSectionMeeting_ChangeMeetingDates()
            {
                response.CourseSecMeetingId = "1";

                var start = secStartDate.AddDays(30);
                var end = secEndDate.AddDays(30);
                section.Meetings[0].StartDate = start;
                section.Meetings[0].EndDate = end;
                secMeet = await repository.PutSectionMeetingAsync(section, meet1Guid);
                Assert.AreEqual(start, secMeet.StartDate);
                Assert.AreEqual(end, secMeet.EndDate);
            }

            [TestMethod]
            public async Task SectionRepository_PutSectionMeeting_ChangeMeetingTimes()
            {
                response.CourseSecMeetingId = "1";

                var start = lecStartTime.Value.AddHours(1);
                var end = lecEndTime.Value.AddHours(1);
                section.Meetings[0].StartTime = start;
                section.Meetings[0].EndTime = end;
                secMeet = await repository.PutSectionMeetingAsync(section, meet1Guid);
                Assert.AreEqual(start.ToLocalTime(), secMeet.StartTime.Value.ToLocalTime());
                Assert.AreEqual(end.ToLocalTime(), secMeet.EndTime.Value.ToLocalTime());
            }

            [TestMethod]
            public async Task SectionRepository_PutSectionMeeting_ChangeMeetingRoom()
            {
                response.CourseSecMeetingId = "1";

                var room = "ARM*140";
                section.Meetings[0].Room = room;
                secMeet = await repository.PutSectionMeetingAsync(section, meet1Guid);
                Assert.AreEqual(room, secMeet.Room);
            }

            [TestMethod]
            public async Task SectionRepository_PutSectionMeeting_ChangeMeetingFaculty()
            {
                response.CourseSecMeetingId = "1";

                section.Meetings[0].RemoveFacultyId(faculty1);
                section.Meetings[0].AddFacultyId(faculty3);
                secMeet = await repository.PutSectionMeetingAsync(section, meet1Guid);
                Assert.AreEqual(1, secMeet.FacultyIds.Count);
                Assert.AreEqual(faculty3, secMeet.FacultyIds[0]);
            }
        }

        [TestClass]
        public class SectionRepository_PutPostSectionMeeting_V11 : SectionRepositoryTests
        {
            string secGuid, meet1Guid, meet2Guid, meet3Guid;
            Section section;
            SectionMeeting meeting1, meeting2, meeting3, secMeet;
            CourseSecMeeting csm;
            CourseSecFaculty csf;
            DateTime secStartDate, secEndDate, semStartDate, semEndDate;
            DateTimeOffset? lecStartTime, lecEndTime, labStartTime, labEndTime, semStartTime, semEndTime;
            List<OfferingDepartment> depts = new List<OfferingDepartment>();
            List<SectionStatusItem> statuses = new List<SectionStatusItem>();
            List<DayOfWeek> MWF = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
            List<DayOfWeek> TTh = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday };
            List<DayOfWeek> Wed = new List<DayOfWeek>() { DayOfWeek.Wednesday };
            List<SectionMeeting> meetings = new List<SectionMeeting>();

            UpdateInstructionalEventV2Request request;
            UpdateInstructionalEventV2Response response;
            SectionRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();

                secGuid = Guid.NewGuid().ToString();
                secStartDate = new DateTime(2014, 9, 2);
                secEndDate = new DateTime(2014, 12, 5);
                depts.Add(new OfferingDepartment("RECR", 75m));
                depts.Add(new OfferingDepartment("CECR", 25m));
                var courseLevelCodes = new List<string>() { "100", "CE" };
                statuses.Add(new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2011, 9, 28)));
                section = new Section("1", "1", "01", secStartDate, 3.00m, null, "Underwater Basketweaving", "IN", depts, courseLevelCodes, "CE", statuses) { Guid = secGuid };

                meet1Guid = Guid.NewGuid().ToString();
                meet2Guid = Guid.NewGuid().ToString();
                meet3Guid = Guid.NewGuid().ToString();
                lecStartTime = (new DateTime(1, 1, 1, 9, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                lecEndTime = (new DateTime(1, 1, 1, 9, 50, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                labStartTime = (new DateTime(1, 1, 1, 13, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                labEndTime = (new DateTime(1, 1, 1, 16, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                meeting1 = new SectionMeeting("1", "1", "LEC", secStartDate, secEndDate, "W") { Days = MWF, StartTime = lecStartTime, EndTime = lecEndTime, Room = "ARM*240", Load = 20m, IsOnline = false, Guid = meet1Guid };
                meeting2 = new SectionMeeting("2", "1", "LAB", secStartDate, secEndDate, "W") { Days = TTh, StartTime = labStartTime, EndTime = labEndTime, Room = "ARM*131", Load = 10m, IsOnline = false, Guid = meet2Guid };

                semStartDate = new DateTime(2014, 10, 1);
                semEndDate = new DateTime(2014, 10, 29);
                semStartTime = (new DateTime(1, 1, 1, 19, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                semEndTime = (new DateTime(1, 1, 1, 22, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                meeting3 = new SectionMeeting(null, "1", "SEM", semStartDate, semEndDate, "W") { Days = Wed, StartTime = semStartTime, EndTime = semEndTime, Room = "JFK*200", Load = 5m, IsOnline = false, Guid = meet3Guid };

                section.AddSectionMeeting(meeting1);
                section.AddSectionMeeting(meeting2);
                section.AddSectionMeeting(meeting3);

                meetings.Add(meeting1);
                meetings.Add(meeting2);
                meetings.Add(meeting3);

                request = new UpdateInstructionalEventV2Request();
                response = new UpdateInstructionalEventV2Response()
                {
                    UpdateInstructionalEventErrors2 = new List<UpdateInstructionalEventErrors2>(),
                    UpdateInstructionalEventWarnings2 = new List<UpdateInstructionalEventWarnings2>()
                };

                csm = new CourseSecMeeting()
                {
                    CsmBldg = "JFK",
                    CsmCourseSection = "1",
                    CsmEndDate = semEndDate,
                    CsmEndTime = semEndTime.Value.DateTime,
                    //CsmFaculty = new List<string>() { faculty3 },
                    CsmFrequency = "W",
                    CsmFriday = "N",
                    CsmInstrMethod = "SEM",
                    CsmLoad = 5m,
                    CsmMonday = "N",
                    CsmRoom = "200",
                    CsmSaturday = "N",
                    CsmStartDate = semStartDate,
                    CsmStartTime = semStartTime.Value.DateTime,
                    CsmSunday = "N",
                    CsmThursday = "N",
                    CsmTuesday = "N",
                    CsmWednesday = "Y",
                    RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                    Recordkey = "4"
                };

                CdDefaults cdDefaults;
                BuildLdmConfiguration(dataReaderMock, out cdDefaults);

                dataReaderMock.Setup(r => r.ReadRecordAsync<CourseSecMeeting>(It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                {
                    if (id == "4")
                    {
                        return Task.FromResult(csm);
                    }
                    var mtg = meetings.FirstOrDefault(x => x.Id == id);
                    if (mtg == null) return Task.FromResult(new CourseSecMeeting());

                    var room = mtg.Room.Contains('*') ? mtg.Room.Split('*') : new string[2] { mtg.Room, string.Empty };
                    return Task.FromResult(new CourseSecMeeting()
                    {
                        Recordkey = mtg.Id,
                        RecordGuid = mtg.Guid,
                        CsmBldg = room[0],
                        CsmCourseSection = mtg.SectionId,
                        CsmEndDate = mtg.EndDate,
                        CsmEndTime = mtg.EndTime.Value.DateTime,
                        CsmFaculty = mtg.FacultyIds.ToList(),
                        CsmFrequency = mtg.Frequency,
                        CsmInstrMethod = mtg.InstructionalMethodCode,
                        CsmLoad = mtg.Load,
                        CsmRoom = room[1],
                        CsmStartDate = mtg.StartDate,
                        CsmStartTime = mtg.StartTime.Value.DateTime,
                        CsmSunday = mtg.Days.Contains(DayOfWeek.Sunday) ? "Y" : "N",
                        CsmMonday = mtg.Days.Contains(DayOfWeek.Monday) ? "Y" : "N",
                        CsmTuesday = mtg.Days.Contains(DayOfWeek.Tuesday) ? "Y" : "N",
                        CsmWednesday = mtg.Days.Contains(DayOfWeek.Wednesday) ? "Y" : "N",
                        CsmThursday = mtg.Days.Contains(DayOfWeek.Thursday) ? "Y" : "N",
                        CsmFriday = mtg.Days.Contains(DayOfWeek.Friday) ? "Y" : "N",
                        CsmSaturday = mtg.Days.Contains(DayOfWeek.Saturday) ? "Y" : "N"
                    });
                });

                transManagerMock.Setup(t => t.ExecuteAsync<UpdateInstructionalEventV2Request, UpdateInstructionalEventV2Response>(It.IsAny<UpdateInstructionalEventV2Request>())).ReturnsAsync(response);

                // Set up response for instructional methods
                var instrMethods = BuildValidInstrMethodResponse();
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>("INSTR.METHODS", "", true)).ReturnsAsync(instrMethods);

                //setup mocking for Stweb Defaults
                var stWebDflt = BuildStwebDefaults(); ;
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                    );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                   );
                repository = new SectionRepository(cacheProvider, transFactory, logger, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
            }

            [TestMethod]
            public async Task SectionRepository_PutSectionMeeting2Async_AddNewMeeting()
            {
                response.CourseSecMeetingId = "4";

                secMeet = await repository.PutSectionMeeting2Async(section, meet3Guid);
                Assert.AreEqual("4", secMeet.Id);
            }

            [TestMethod]
            public async Task SectionRepository_PostSectionMeeting2Async_AddNewMeeting()
            {
                response.CourseSecMeetingId = "4";
                var courseLevelCodes = new List<string>() { "100", "CE" };
                section = new Section("1", "1", "01", secStartDate, 3.00m, null, "Underwater Basketweaving", "IN", depts, courseLevelCodes, "CE", statuses) { Guid = Guid.Empty.ToString() };
                var meeting = new SectionMeeting("1", "1", "LEC", secStartDate, secEndDate, "W") { Days = MWF, StartTime = lecStartTime, EndTime = lecEndTime, Room = "ARM*240", Load = 20m, IsOnline = false, Guid = Guid.Empty.ToString() };
                section.AddSectionMeeting(meeting);
                secMeet = await repository.PostSectionMeeting2Async(section, Guid.Empty.ToString());
                Assert.AreEqual("4", secMeet.Id);
            }

            [TestMethod]
            public async Task SectionRepository_PutSectionMeeting2Async_ChangeMeetingDates()
            {
                response.CourseSecMeetingId = "1";

                var start = secStartDate.AddDays(30);
                var end = secEndDate.AddDays(30);
                section.Meetings[0].StartDate = start;
                section.Meetings[0].EndDate = end;
                secMeet = await repository.PutSectionMeeting2Async(section, meet1Guid);
                Assert.AreEqual(start, secMeet.StartDate);
                Assert.AreEqual(end, secMeet.EndDate);
            }

            [TestMethod]
            public async Task SectionRepository_PutSectionMeeting_ChangeMeetingTimes()
            {
                response.CourseSecMeetingId = "1";

                var start = lecStartTime.Value.AddHours(1);
                var end = lecEndTime.Value.AddHours(1);
                section.Meetings[0].StartTime = start;
                section.Meetings[0].EndTime = end;
                secMeet = await repository.PutSectionMeeting2Async(section, meet1Guid);
                Assert.AreEqual(start.ToLocalTime(), secMeet.StartTime.Value.ToLocalTime());
                Assert.AreEqual(end.ToLocalTime(), secMeet.EndTime.Value.ToLocalTime());
            }

            [TestMethod]
            public async Task SectionRepository_PutSectionMeeting2Async_ChangeMeetingRoom()
            {
                response.CourseSecMeetingId = "1";

                var room = "ARM*140";
                section.Meetings[0].Room = room;
                secMeet = await repository.PutSectionMeeting2Async(section, meet1Guid);
                Assert.AreEqual(room, secMeet.Room);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_PutSectionMeeting2Async_NullSection_Exception()
            {
                secMeet = await repository.PutSectionMeeting2Async(null, meet1Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_PutSectionMeeting2Async_NullMeetingId_Exception()
            {
                secMeet = await repository.PutSectionMeeting2Async(section, null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_PutSectionMeeting2Async_NullMeeting_Exception()
            {
                secMeet = await repository.PutSectionMeeting2Async(section, Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_PutSectionMeeting2Async_With_UpdateInstructionalEventWarnings2_UpdateInstructionalEventErrors2()
            {
                response.UpdateInstructionalEventWarnings2 = new List<UpdateInstructionalEventWarnings2>()
                {
                    new UpdateInstructionalEventWarnings2() { WarningCodes = "1", WarningMessages = "WarningMessages" }
                };
                response.UpdateInstructionalEventErrors2 = new List<UpdateInstructionalEventErrors2>()
                {
                    new UpdateInstructionalEventErrors2()
                    {
                        ErrorCodes = "1",
                        ErrorMessages = "ErrorMessages"
                    }
                };
                secMeet = await repository.PutSectionMeeting2Async(section, meet1Guid);
            }
        }

        [TestClass]
        public class SectionRepository_DeleteSectionMeeting : SectionRepositoryTests
        {
            string secGuid, meet1Guid, meet2Guid, meet3Guid;
            Section section;
            SectionMeeting meeting1, meeting2, meeting3, secMeet;
            IEnumerable<SectionMeeting> secMeets;
            CourseSecMeeting csm;
            CourseSecFaculty csf;
            string faculty1, faculty2, faculty3;
            SectionFaculty lecFaculty, labFaculty1, labFaculty2, semFaculty;
            DateTime secStartDate, secEndDate, semStartDate, semEndDate;
            DateTimeOffset? lecStartTime, lecEndTime, labStartTime, labEndTime, semStartTime, semEndTime;
            List<OfferingDepartment> depts = new List<OfferingDepartment>();
            List<SectionStatusItem> statuses = new List<SectionStatusItem>();
            List<DayOfWeek> MWF = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
            List<DayOfWeek> TTh = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday };
            List<DayOfWeek> Wed = new List<DayOfWeek>() { DayOfWeek.Wednesday };
            List<SectionMeeting> meetings = new List<SectionMeeting>();
            List<SectionFaculty> faculty = new List<SectionFaculty>();

            DeleteInstructionalEventRequest request;
            DeleteInstructionalEventResponse response;
            SectionRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();

                secGuid = Guid.NewGuid().ToString();
                secStartDate = new DateTime(2014, 9, 2);
                secEndDate = new DateTime(2014, 12, 5);
                depts.Add(new OfferingDepartment("RECR", 75m));
                depts.Add(new OfferingDepartment("CECR", 25m));
                var courseLevelCodes = new List<string>() { "100", "CE" };
                statuses.Add(new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2011, 9, 28)));
                section = new Section("1", "1", "01", secStartDate, 3.00m, null, "Underwater Basketweaving", "IN", depts, courseLevelCodes, "CE", statuses) { Guid = secGuid };

                faculty1 = "1234567";
                faculty2 = "2345678";
                faculty3 = "3456789";
                meet1Guid = Guid.NewGuid().ToString();
                meet2Guid = Guid.NewGuid().ToString();
                meet3Guid = Guid.NewGuid().ToString();
                lecStartTime = (new DateTime(1, 1, 1, 9, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                lecEndTime = (new DateTime(1, 1, 1, 9, 50, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                labStartTime = (new DateTime(1, 1, 1, 13, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                labEndTime = (new DateTime(1, 1, 1, 16, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                meeting1 = new SectionMeeting("1", "1", "LEC", secStartDate, secEndDate, "W") { Days = MWF, StartTime = lecStartTime, EndTime = lecEndTime, Room = "ARM*240", Load = 20m, IsOnline = false, Guid = meet1Guid };
                meeting1.AddFacultyId(faculty1);
                meeting2 = new SectionMeeting("2", "1", "LAB", secStartDate, secEndDate, "W") { Days = TTh, StartTime = labStartTime, EndTime = labEndTime, Room = "ARM*131", Load = 10m, IsOnline = false, Guid = meet2Guid };
                meeting2.AddFacultyId(faculty1);
                meeting2.AddFacultyId(faculty2);

                semStartDate = new DateTime(2014, 10, 1);
                semEndDate = new DateTime(2014, 10, 29);
                semStartTime = (new DateTime(1, 1, 1, 19, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                semEndTime = (new DateTime(1, 1, 1, 22, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone);
                meeting3 = new SectionMeeting(null, "1", "SEM", semStartDate, semEndDate, "W") { Days = Wed, StartTime = semStartTime, EndTime = semEndTime, Room = "JFK*200", Load = 5m, IsOnline = false, Guid = meet3Guid };
                meeting3.AddFacultyId(faculty3);

                lecFaculty = new SectionFaculty("1", "1", faculty1, "LEC", secStartDate, secEndDate, 100m) { LoadFactor = 20m };
                labFaculty1 = new SectionFaculty("2", "1", faculty1, "LAB", secStartDate, secEndDate, 50m) { LoadFactor = 5m };
                labFaculty2 = new SectionFaculty("3", "1", faculty2, "LAB", secStartDate, secEndDate, 50m) { LoadFactor = 5m };
                semFaculty = new SectionFaculty(null, "1", faculty3, "SEM", semStartDate, semEndDate, 100m) { LoadFactor = 5m };

                meeting1.AddSectionFaculty(lecFaculty);
                meeting2.AddSectionFaculty(labFaculty1);
                meeting2.AddSectionFaculty(labFaculty2);
                meeting3.AddSectionFaculty(semFaculty);

                section.AddSectionMeeting(meeting1);
                section.AddSectionMeeting(meeting2);
                section.AddSectionMeeting(meeting3);
                section.AddSectionFaculty(lecFaculty);
                section.AddSectionFaculty(labFaculty1);
                section.AddSectionFaculty(labFaculty2);
                section.AddSectionFaculty(semFaculty);

                meetings.Add(meeting1);
                meetings.Add(meeting2);
                meetings.Add(meeting3);
                faculty.Add(lecFaculty);
                faculty.Add(labFaculty1);
                faculty.Add(labFaculty2);
                faculty.Add(semFaculty);

                request = new DeleteInstructionalEventRequest();
                response = new DeleteInstructionalEventResponse()
                {
                    DeleteInstructionalEventErrors = new List<DeleteInstructionalEventErrors>(),
                    DeleteInstructionalEventWarnings = new List<DeleteInstructionalEventWarnings>()
                };

                //newMeeting = new SectionMeeting("5", "1", "SEM", semStartDate, semEndDate, "W") { Days = Wed, StartTime = semStartTime, EndTime = semEndTime, Room = "JFK*200", Load = 5m, IsOnline = false, Guid = Guid.NewGuid().ToString().ToLowerInvariant() };
                csm = new CourseSecMeeting()
                {
                    CsmBldg = "JFK",
                    CsmCourseSection = "1",
                    CsmEndDate = semEndDate,
                    CsmEndTime = semEndTime.Value.DateTime,
                    CsmFaculty = new List<string>() { faculty3 },
                    CsmFrequency = "W",
                    CsmFriday = "N",
                    CsmInstrMethod = "SEM",
                    CsmLoad = 5m,
                    CsmMonday = "N",
                    CsmRoom = "200",
                    CsmSaturday = "N",
                    CsmStartDate = semStartDate,
                    CsmStartTime = semStartTime.Value.DateTime,
                    CsmSunday = "N",
                    CsmThursday = "N",
                    CsmTuesday = "N",
                    CsmWednesday = "Y",
                    RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                    Recordkey = "4"
                };
                csf = new CourseSecFaculty()
                {
                    CsfCourseSection = "1",
                    CsfEndDate = semEndDate,
                    CsfFaculty = faculty3,
                    CsfFacultyLoad = 5m,
                    CsfFacultyPct = 100m,
                    CsfInstrMethod = "SEM",
                    CsfStartDate = semStartDate,
                    Recordkey = "5"
                };

                CdDefaults cdDefaults;
                BuildLdmConfiguration(dataReaderMock, out cdDefaults);

                dataReaderMock.Setup(r => r.ReadRecordAsync<CourseSecMeeting>(It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                {
                    if (id == "4")
                    {
                        return Task.FromResult(csm);
                    }
                    var mtg = meetings.FirstOrDefault(x => x.Id == id);
                    if (mtg == null) return Task.FromResult(new CourseSecMeeting());

                    var room = mtg.Room.Contains('*') ? mtg.Room.Split('*') : new string[2] { mtg.Room, string.Empty };
                    return Task.FromResult(new CourseSecMeeting()
                    {
                        Recordkey = mtg.Id,
                        RecordGuid = mtg.Guid,
                        CsmBldg = room[0],
                        CsmCourseSection = mtg.SectionId,
                        CsmEndDate = mtg.EndDate,
                        CsmEndTime = mtg.EndTime.Value.DateTime,
                        CsmFaculty = mtg.FacultyIds.ToList(),
                        CsmFrequency = mtg.Frequency,
                        CsmInstrMethod = mtg.InstructionalMethodCode,
                        CsmLoad = mtg.Load,
                        CsmRoom = room[1],
                        CsmStartDate = mtg.StartDate,
                        CsmStartTime = mtg.StartTime.Value.DateTime,
                        CsmSunday = mtg.Days.Contains(DayOfWeek.Sunday) ? "Y" : "N",
                        CsmMonday = mtg.Days.Contains(DayOfWeek.Monday) ? "Y" : "N",
                        CsmTuesday = mtg.Days.Contains(DayOfWeek.Tuesday) ? "Y" : "N",
                        CsmWednesday = mtg.Days.Contains(DayOfWeek.Wednesday) ? "Y" : "N",
                        CsmThursday = mtg.Days.Contains(DayOfWeek.Thursday) ? "Y" : "N",
                        CsmFriday = mtg.Days.Contains(DayOfWeek.Friday) ? "Y" : "N",
                        CsmSaturday = mtg.Days.Contains(DayOfWeek.Saturday) ? "Y" : "N"
                    });
                });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<CourseSecFaculty>(It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                {
                    var results = new Collection<CourseSecFaculty>() { csf };
                    foreach (var fac in faculty)
                    {
                        results.Add(new CourseSecFaculty()
                        {
                            //CsfCourseSection = request.CsmCourseSection,
                            CsfEndDate = fac.EndDate,
                            CsfFaculty = fac.FacultyId,
                            CsfFacultyLoad = fac.LoadFactor,
                            CsfFacultyPct = fac.ResponsibilityPercentage,
                            CsfInstrMethod = fac.InstructionalMethodCode,
                            CsfStartDate = fac.StartDate,
                            CsfPacLpAsgmt = fac.ContractAssignment,
                            CsfTeachingArrangement = fac.TeachingArrangementCode,
                            Recordkey = fac.Id
                        });
                    }
                    return Task.FromResult(results);
                });
                transManagerMock.Setup(t => t.ExecuteAsync<DeleteInstructionalEventRequest, DeleteInstructionalEventResponse>(It.IsAny<DeleteInstructionalEventRequest>())).Returns(Task.FromResult(response));

                // Set up response for instructional methods
                var instrMethods = BuildValidInstrMethodResponse();
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>("INSTR.METHODS", "", true)).ReturnsAsync(instrMethods);

                //setup mocking for Stweb Defaults
                var stWebDflt = BuildStwebDefaults(); ;
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                    );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                   );
                repository = new SectionRepository(cacheProvider, transFactory, logger, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);



            }

            [TestMethod]
            public async Task SectionRepository_DeleteSectionMeeting_AddNewMeeting()
            {
                await repository.DeleteSectionMeetingAsync(meet3Guid, faculty);
            }
        }

        [TestClass]
        public class SectionRepository_GradeImportTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataReaderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;
            SectionRepository sectionRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                apiSettingsMock = new ApiSettings("null");

                var transactionResponse = GetTransactionResponse();
                var grades = GetSectionGrades();
                // Identify the request based on the SectionGrades entity returned by GetSectionGrades by having 9 items to post and the sectionID.
                // Return GetTransactionResponse
                transManagerMock.Setup<Task<ImportGradesFromILPResponse>>(mgr => mgr.ExecuteAsync<ImportGradesFromILPRequest, ImportGradesFromILPResponse>(
                    It.Is<ImportGradesFromILPRequest>(x => x.ItemsToPostInput.Count() == 9 &&
                        x.SectionId == grades.SectionId)
                    )).Returns(Task.FromResult(transactionResponse));

                // When the SectionId is "TrueForceVerify", and 8 items to post, ForceNoVerify is true, and CheckForLocks is true, 
                // return an error message "WasTrue"
                ItemsToPostOutput trueOutput = new ItemsToPostOutput { ItemOutPerson = "Person1", ItemErrorMsg = "WasTrue", ItemOutStatus = "failure" };
                ImportGradesFromILPResponse trueResponse =
                    new ImportGradesFromILPResponse { SectionId = "TrueForceVerify", ItemsToPostOutput = new List<ItemsToPostOutput>() { trueOutput } };
                transManagerMock.Setup<Task<ImportGradesFromILPResponse>>(mgr => mgr.ExecuteAsync<ImportGradesFromILPRequest, ImportGradesFromILPResponse>(
                    It.Is<ImportGradesFromILPRequest>(x => x.ItemsToPostInput.Count() == 8 &&
                        x.SectionId == "TrueForceVerify" && x.ForceNoVerify == true && x.CheckForLocks == true)
                    )).Returns(Task.FromResult(trueResponse));

                // Same but return "WasFalse" when SectionId is "FalseForceVerify", ForceNoVerify is false, and CheckForLocks is false
                ItemsToPostOutput falseOutput = new ItemsToPostOutput { ItemOutPerson = "Person1", ItemErrorMsg = "WasFalse", ItemOutStatus = "failure" };
                ImportGradesFromILPResponse falseResponse =
                    new ImportGradesFromILPResponse { SectionId = "FalseForceVerify", ItemsToPostOutput = new List<ItemsToPostOutput>() { falseOutput } };
                transManagerMock.Setup<Task<ImportGradesFromILPResponse>>(mgr => mgr.ExecuteAsync<ImportGradesFromILPRequest, ImportGradesFromILPResponse>(
                    It.Is<ImportGradesFromILPRequest>(x => x.ItemsToPostInput.Count() == 8 &&
                        x.SectionId == "FalseForceVerify" && x.ForceNoVerify == false && x.CheckForLocks == false)
                    )).Returns(Task.FromResult(falseResponse));

                // When the SectionId is "TestCallerType", and 8 items to post, ForceNoVerify is true, and CheckForLocks is true, and CallerType is "ILP",
                // return an error message "ILPCaller"
                ItemsToPostOutput ilpOutput = new ItemsToPostOutput { ItemOutPerson = "Person1", ItemErrorMsg = "IlpCaller", ItemOutStatus = "failure" };
                ImportGradesFromILPResponse ilpResponse =
                    new ImportGradesFromILPResponse { SectionId = "TestCallerType", ItemsToPostOutput = new List<ItemsToPostOutput>() { ilpOutput } };
                transManagerMock.Setup<Task<ImportGradesFromILPResponse>>(mgr => mgr.ExecuteAsync<ImportGradesFromILPRequest, ImportGradesFromILPResponse>(
                    It.Is<ImportGradesFromILPRequest>(x => x.ItemsToPostInput.Count() == 8 &&
                        x.SectionId == "TestCallerType" && x.ForceNoVerify == true && x.CheckForLocks == true && x.CallerType == "ILP")
                    )).Returns(Task.FromResult(ilpResponse));

                // When the SectionId is "TestCallerType", and 8 items to post, ForceNoVerify is true, and CheckForLocks is true, and CallerType is "Standard"                ,
                // return an error message "StandardCaller"
                ItemsToPostOutput standardCallerOutput = new ItemsToPostOutput { ItemOutPerson = "Person1", ItemErrorMsg = "StandardCaller", ItemOutStatus = "failure" };
                ImportGradesFromILPResponse standardResponse =
                    new ImportGradesFromILPResponse { SectionId = "TestCallerType", ItemsToPostOutput = new List<ItemsToPostOutput>() { standardCallerOutput } };
                transManagerMock.Setup<Task<ImportGradesFromILPResponse>>(mgr => mgr.ExecuteAsync<ImportGradesFromILPRequest, ImportGradesFromILPResponse>(
                    It.Is<ImportGradesFromILPRequest>(x => x.ItemsToPostInput.Count() == 8 &&
                        x.SectionId == "TestCallerType" && x.ForceNoVerify == true && x.CheckForLocks == true && x.CallerType == "Standard")
                    )).Returns(Task.FromResult(standardResponse));


                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                // Mock the read of instructional methods


                InstrMethods lec = new InstrMethods()
                {
                    InmDesc = "LEC",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                    Recordkey = "LEC"
                };
                InstrMethods lab = new InstrMethods()
                {
                    InmDesc = "LAB",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                    Recordkey = "LAB"
                };
                InstrMethods onl = new InstrMethods()
                {
                    InmDesc = "ONL",
                    InmOnline = "Y",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                    Recordkey = "ONL"
                };
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                GetSectionWaitlistStatusResponse wlResp = new GetSectionWaitlistStatusResponse() { ErrorMessages = new List<string>(), Status = "Wlst" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetSectionWaitlistStatusRequest, GetSectionWaitlistStatusResponse>(It.IsAny<GetSectionWaitlistStatusRequest>())).ReturnsAsync(wlResp);


                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettingsMock);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                cacheProviderMock = null;
                transManagerMock = null;
                sectionRepo = null;
                apiSettingsMock = null;
            }

            [TestMethod]
            public async Task SectionRepository_ImportGrades()
            {
                // Test basic execution. Mocked ImportGradesAsync should return a response with the same
                // student ID as GetSectionGrades that is passed in as the request.
                var grades = GetSectionGrades();
                var response = await sectionRepo.ImportGradesAsync(grades, false, true, GradesPutCallerTypes.ILP, false);

                foreach (var r in response.StudentResponses)
                {
                    Assert.AreEqual(GetSectionGrades().StudentGrades[0].StudentId, r.StudentId);
                }
            }

            [TestMethod]
            public async Task SectionRepository_GradesForceNoVerifyTrueFlagAndCheckForLocksTrueFlagPassedSuccessfully()
            {
                var grades = GetSectionGrades();
                // Remove a midterm grade to get the total items to post down to 8
                grades.StudentGrades[0].MidtermGrade1 = null;
                grades.SectionId = "TrueForceVerify";
                // Tests that ForceNoVerify is passed to the CTX when true
                SectionGradeSectionResponse response = await sectionRepo.ImportGradesAsync(grades, true, true, GradesPutCallerTypes.ILP, false);
                Assert.AreEqual(response.StudentResponses.First().Errors[0].Message, "WasTrue");
            }

            [TestMethod]
            public async Task SectionRepository_GradesForceNoVerifyFalseFlagAndCheckForLocksFalseFlagPassedSuccessfully()
            {
                var grades = GetSectionGrades();
                // Remove a midterm grade to get the total items to post down to 8
                grades.StudentGrades[0].MidtermGrade1 = null;
                grades.SectionId = "FalseForceVerify";
                // Tests that ForceNoVerify is passed to the CTX when false
                SectionGradeSectionResponse response = await sectionRepo.ImportGradesAsync(grades, false, false, GradesPutCallerTypes.ILP, false);
                Assert.AreEqual(response.StudentResponses.First().Errors[0].Message, "WasFalse");
            }

            [TestMethod]
            public async Task SectionRepository_GradesIlpCallerPassedSuccessfully()
            {
                var grades = GetSectionGrades();
                // Remove a midterm grade to get the total items to post down to 8
                grades.StudentGrades[0].MidtermGrade1 = null;
                grades.SectionId = "TestCallerType";
                // Tests that ForceNoVerify is passed to the CTX when false
                SectionGradeSectionResponse response = await sectionRepo.ImportGradesAsync(grades, true, true, GradesPutCallerTypes.ILP, false);
                Assert.AreEqual(response.StudentResponses.First().Errors[0].Message, "IlpCaller");
            }

            [TestMethod]
            public async Task SectionRepository_GradesStandardCallerPassedSuccessfully()
            {
                var grades = GetSectionGrades();
                // Remove a midterm grade to get the total items to post down to 8
                grades.StudentGrades[0].MidtermGrade1 = null;
                grades.SectionId = "TestCallerType";
                // Tests that ForceNoVerify is passed to the CTX when false
                SectionGradeSectionResponse response = await sectionRepo.ImportGradesAsync(grades, true, true, GradesPutCallerTypes.Standard, false);
                Assert.AreEqual(response.StudentResponses.First().Errors[0].Message, "StandardCaller");
            }


            private SectionGrades GetSectionGrades()
            {
                var grades = new SectionGrades();
                grades.SectionId = "123";
                var grade = new StudentGrade();
                grade.StudentId = "101";
                grade.MidtermGrade1 = "A";
                grade.MidtermGrade2 = "B";
                grade.MidtermGrade3 = "C";
                grade.MidtermGrade4 = "D";
                grade.MidtermGrade5 = "E";
                grade.MidtermGrade6 = "F";
                grade.FinalGrade = "G";
                grade.FinalGradeExpirationDate = DateTime.Now;
                grade.LastAttendanceDate = DateTime.Now;
                grade.NeverAttended = true;
                grades.StudentGrades = new List<StudentGrade>();
                grades.StudentGrades.Add(grade);

                return grades;
            }

            private ImportGradesFromILPResponse GetTransactionResponse()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();
                response.SectionId = GetSectionGrades().SectionId;
                response.ErrorCode = null; // success

                response.ItemsToPostOutput.Add(new ItemsToPostOutput() { ItemOutStatus = "success", ItemOutPerson = "101" });

                return response;
            }
        }

        [TestClass]
        public class SectionRepository_GradesBuildImportPostItemsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;
            PrivateObject sectionRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                apiSettingsMock = new ApiSettings("null");

                //sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
                sectionRepo = new PrivateObject(typeof(SectionRepository), cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettingsMock);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                       x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                       .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                cacheProviderMock = null;
                transManagerMock = null;
                sectionRepo = null;
                apiSettingsMock = null;
            }

            [TestMethod]
            public void SectionRepository_GradesBuildImportPostItems()
            {
                var grade = GetStudentGrade();
                var postItems = (List<ItemsToPostInput>)sectionRepo.Invoke("BuildImportPostItems", grade);
                Assert.IsTrue(postItems.All(x => x.ItemPerson == grade.StudentId));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "MidtermGrade1" && x.ItemValue == grade.MidtermGrade1));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "MidtermGrade2" && x.ItemValue == grade.MidtermGrade2));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "MidtermGrade3" && x.ItemValue == grade.MidtermGrade3));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "MidtermGrade4" && x.ItemValue == grade.MidtermGrade4));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "MidtermGrade5" && x.ItemValue == grade.MidtermGrade5));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "MidtermGrade6" && x.ItemValue == grade.MidtermGrade6));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "FinalGrade" && x.ItemValue == grade.FinalGrade + "|" + grade.FinalGradeExpirationDate.Value.ToString("yyyy/MM/dd")));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "LastAttendanceDate" && x.ItemValue == grade.LastAttendanceDate.Value.ToString("yyyy/MM/dd")));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "NeverAttended" && x.ItemValue == "1"));

                grade = GetMinimumStudentGrade();
                postItems = (List<ItemsToPostInput>)sectionRepo.Invoke("BuildImportPostItems", grade);
                Assert.IsTrue(postItems.All(x => x.ItemPerson == grade.StudentId));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "MidtermGrade1" && x.ItemValue == grade.MidtermGrade1));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "NeverAttended" && x.ItemValue == "0"));

                grade = GetStudentGradeWithClearFlags();
                postItems = (List<ItemsToPostInput>)sectionRepo.Invoke("BuildImportPostItems", grade);
                Assert.IsTrue(postItems.All(x => x.ItemPerson == grade.StudentId));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "FinalGrade" && x.ItemValue == grade.FinalGrade + "|"));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "LastAttendanceDate" && x.ItemValue == ""));

                grade = GetStudentGradeOnlyExpireDate();
                postItems = (List<ItemsToPostInput>)sectionRepo.Invoke("BuildImportPostItems", grade);
                Assert.IsTrue(postItems.All(x => x.ItemPerson == grade.StudentId));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "FinalGradeExpirationDate" && x.ItemValue == grade.FinalGradeExpirationDate.Value.ToString("yyyy/MM/dd")));

                grade = GetStudentGradeOnlyClearExpireFlag();
                postItems = (List<ItemsToPostInput>)sectionRepo.Invoke("BuildImportPostItems", grade);
                Assert.IsTrue(postItems.All(x => x.ItemPerson == grade.StudentId));
                Assert.IsTrue(postItems.Any(x => x.ItemCode == "FinalGradeExpirationDate" && x.ItemValue == ""));
            }

            [TestMethod]
            public void SectionRepository_GradesConvertTransactionOutputToDomainEntities()
            {
                ImportGradesFromILPResponse transactionOutput = new ImportGradesFromILPResponse();

                // empty input and output
                var sectionGradeResponse = (SectionGradeSectionResponse)sectionRepo.Invoke("ConvertImportOutputToDomainEntities", transactionOutput);
                Assert.AreEqual(sectionGradeResponse.StudentResponses.Count(), 0);
                Assert.AreEqual(sectionGradeResponse.InformationalMessages.Count(), 0);


                // single success record
                transactionOutput = GetSingleSuccessResponse_ForSingleStudent();
                sectionGradeResponse = (SectionGradeSectionResponse)sectionRepo.Invoke("ConvertImportOutputToDomainEntities", transactionOutput);
                Assert.AreEqual(transactionOutput.ItemsToPostOutput[0].ItemOutPerson, sectionGradeResponse.StudentResponses[0].StudentId);
                Assert.AreEqual("success", sectionGradeResponse.StudentResponses[0].Status);

                // single failure record
                transactionOutput = GetSingleFailureResponse_ForSingleStudent();
                sectionGradeResponse = (SectionGradeSectionResponse)sectionRepo.Invoke("ConvertImportOutputToDomainEntities", transactionOutput);
                Assert.AreEqual(transactionOutput.ItemsToPostOutput[0].ItemOutPerson, sectionGradeResponse.StudentResponses[0].StudentId);
                Assert.AreEqual("failure", sectionGradeResponse.StudentResponses[0].Status);
                Assert.IsTrue(sectionGradeResponse.StudentResponses[0].Errors.Count() == 1);
                Assert.AreEqual(transactionOutput.ItemsToPostOutput[0].ItemOutCode, sectionGradeResponse.StudentResponses[0].Errors[0].Property);
                Assert.AreEqual(transactionOutput.ItemsToPostOutput[0].ItemErrorMsg, sectionGradeResponse.StudentResponses[0].Errors[0].Message);

                // mixed response for single person
                transactionOutput = GetMixedResponse_ForSingleStudent();
                sectionGradeResponse = (SectionGradeSectionResponse)sectionRepo.Invoke("ConvertImportOutputToDomainEntities", transactionOutput);
                Assert.AreEqual(transactionOutput.ItemsToPostOutput[0].ItemOutPerson, sectionGradeResponse.StudentResponses[0].StudentId);
                Assert.AreEqual("failure", sectionGradeResponse.StudentResponses[0].Status);
                Assert.IsTrue(sectionGradeResponse.StudentResponses[0].Errors.Count() == 1);
                Assert.AreEqual(transactionOutput.ItemsToPostOutput.First(x => x.ItemOutStatus == "failure").ItemOutCode, sectionGradeResponse.StudentResponses[0].Errors[0].Property);
                Assert.AreEqual(transactionOutput.ItemsToPostOutput.First(x => x.ItemOutStatus == "failure").ItemErrorMsg, sectionGradeResponse.StudentResponses[0].Errors[0].Message);

                // multiple success for single person
                transactionOutput = GetMultiSuccessResponse_ForSingleStudent();
                sectionGradeResponse = (SectionGradeSectionResponse)sectionRepo.Invoke("ConvertImportOutputToDomainEntities", transactionOutput);
                Assert.AreEqual(transactionOutput.ItemsToPostOutput[0].ItemOutPerson, sectionGradeResponse.StudentResponses[0].StudentId);
                Assert.AreEqual("success", sectionGradeResponse.StudentResponses[0].Status);

                // multiple failures for single person
                transactionOutput = GetMultiFailureResponse_ForSingleStudent();
                sectionGradeResponse = (SectionGradeSectionResponse)sectionRepo.Invoke("ConvertImportOutputToDomainEntities", transactionOutput);
                Assert.AreEqual(transactionOutput.ItemsToPostOutput[0].ItemOutPerson, sectionGradeResponse.StudentResponses[0].StudentId);
                Assert.AreEqual("failure", sectionGradeResponse.StudentResponses[0].Status);
                Assert.IsTrue(sectionGradeResponse.StudentResponses[0].Errors.Count() > 1);
                transactionOutput.ItemsToPostOutput.ForEach(x =>
                {
                    Assert.IsTrue(sectionGradeResponse.StudentResponses[0].Errors.Any(z => z.Property == x.ItemOutCode));
                    Assert.IsTrue(sectionGradeResponse.StudentResponses[0].Errors.Any(z => z.Message == x.ItemErrorMsg));
                });

                // two people, both success
                transactionOutput = GetSuccessResponse_ForMultiStudent();
                sectionGradeResponse = (SectionGradeSectionResponse)sectionRepo.Invoke("ConvertImportOutputToDomainEntities", transactionOutput);
                transactionOutput.ItemsToPostOutput.ForEach(x =>
                {
                    Assert.IsTrue(sectionGradeResponse.StudentResponses.Any(z => z.StudentId == x.ItemOutPerson && z.Status == "success"));
                });

                // two people, both failure
                transactionOutput = GetFailureResponse_ForMultiStudent();
                sectionGradeResponse = (SectionGradeSectionResponse)sectionRepo.Invoke("ConvertImportOutputToDomainEntities", transactionOutput);
                transactionOutput.ItemsToPostOutput.ForEach(x =>
                {
                    Assert.IsTrue(sectionGradeResponse.StudentResponses.Any(z => z.StudentId == x.ItemOutPerson && z.Status == "failure"));
                    Assert.IsTrue(sectionGradeResponse.StudentResponses.Any(z => z.StudentId == x.ItemOutPerson && z.Errors.Any(y => y.Property == x.ItemOutCode)));
                    Assert.IsTrue(sectionGradeResponse.StudentResponses.Any(z => z.StudentId == x.ItemOutPerson && z.Errors.Any(y => y.Message == x.ItemErrorMsg)));
                });

                // two people, one success, one failure
                transactionOutput = GetFailureResponse_ForMultiStudent();
                sectionGradeResponse = (SectionGradeSectionResponse)sectionRepo.Invoke("ConvertImportOutputToDomainEntities", transactionOutput);
                transactionOutput.ItemsToPostOutput.ForEach(x =>
                {
                    Assert.IsTrue(sectionGradeResponse.StudentResponses.Any(z => z.StudentId == x.ItemOutPerson && z.Status == x.ItemOutStatus));

                    if (x.ItemOutStatus == "success")
                    {
                        Assert.IsTrue(sectionGradeResponse.StudentResponses.First(z => z.StudentId == x.ItemOutStatus).Errors.Count() == 0);
                    }

                    if (x.ItemOutStatus == "failure")
                    {
                        Assert.IsTrue(sectionGradeResponse.StudentResponses.Any(z => z.StudentId == x.ItemOutPerson && z.Errors.Any(y => y.Property == x.ItemOutCode)));
                        Assert.IsTrue(sectionGradeResponse.StudentResponses.Any(z => z.StudentId == x.ItemOutPerson && z.Errors.Any(y => y.Message == x.ItemErrorMsg)));
                    }
                });
            }

            private StudentGrade GetStudentGrade()
            {
                var grade = new StudentGrade();
                grade.StudentId = "101";
                grade.MidtermGrade1 = "A";
                grade.MidtermGrade2 = "B";
                grade.MidtermGrade3 = "C";
                grade.MidtermGrade4 = "D";
                grade.MidtermGrade5 = "E";
                grade.MidtermGrade6 = "F";
                grade.FinalGrade = "G";
                grade.FinalGradeExpirationDate = DateTime.Now;
                grade.LastAttendanceDate = DateTime.Now;
                grade.NeverAttended = true;
                return grade;
            }

            private StudentGrade GetMinimumStudentGrade()
            {
                var grade = new StudentGrade();
                grade.StudentId = "102";
                grade.MidtermGrade1 = "A";
                grade.FinalGradeExpirationDate = null;
                grade.LastAttendanceDate = null;
                grade.NeverAttended = false;
                return grade;
            }

            private StudentGrade GetStudentGradeWithClearFlags()
            {
                var grade = new StudentGrade();
                grade.StudentId = "105";
                grade.FinalGrade = "I";
                grade.ClearLastAttendanceDateFlag = true;
                grade.ClearFinalGradeExpirationDateFlag = true;
                return grade;
            }

            private StudentGrade GetStudentGradeOnlyExpireDate()
            {
                var grade = new StudentGrade();
                grade.StudentId = "12345";
                grade.FinalGradeExpirationDate = DateTime.Now;
                return grade;
            }

            private StudentGrade GetStudentGradeOnlyClearExpireFlag()
            {
                var grade = new StudentGrade();
                grade.StudentId = "23456";
                grade.ClearFinalGradeExpirationDateFlag = true;
                return grade;
            }

            private ImportGradesFromILPResponse GetSingleSuccessResponse_ForSingleStudent()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();
                ItemsToPostOutput outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "success";
                response.ItemsToPostOutput.Add(outputItem);
                return response;
            }

            private ImportGradesFromILPResponse GetSingleFailureResponse_ForSingleStudent()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();
                ItemsToPostOutput outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "failure";
                outputItem.ItemErrorMsg = "some error message";
                outputItem.ItemOutCode = "some output code";
                response.ItemsToPostOutput.Add(outputItem);
                return response;
            }

            private ImportGradesFromILPResponse GetMixedResponse_ForSingleStudent()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();

                ItemsToPostOutput outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "success";
                response.ItemsToPostOutput.Add(outputItem);

                outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "failure";
                outputItem.ItemErrorMsg = "some error message";
                outputItem.ItemOutCode = "some output code";
                response.ItemsToPostOutput.Add(outputItem);

                return response;
            }

            private ImportGradesFromILPResponse GetMultiSuccessResponse_ForSingleStudent()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();
                ItemsToPostOutput outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "success";

                outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "success";

                response.ItemsToPostOutput.Add(outputItem);
                return response;
            }

            private ImportGradesFromILPResponse GetMultiFailureResponse_ForSingleStudent()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();
                ItemsToPostOutput outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "failure";
                outputItem.ItemErrorMsg = "some error message";
                outputItem.ItemOutCode = "some output code";
                response.ItemsToPostOutput.Add(outputItem);

                outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "failure";
                outputItem.ItemErrorMsg = "some other error message";
                outputItem.ItemOutCode = "some other output code";
                response.ItemsToPostOutput.Add(outputItem);

                return response;
            }

            private ImportGradesFromILPResponse GetSuccessResponse_ForMultiStudent()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();
                ItemsToPostOutput outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "success";
                response.ItemsToPostOutput.Add(outputItem);

                outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "2";
                outputItem.ItemOutStatus = "success";
                response.ItemsToPostOutput.Add(outputItem);

                return response;
            }

            private ImportGradesFromILPResponse GetFailureResponse_ForMultiStudent()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();
                ItemsToPostOutput outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "failure";
                outputItem.ItemErrorMsg = "some error message";
                outputItem.ItemOutCode = "some output code";
                response.ItemsToPostOutput.Add(outputItem);

                outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "2";
                outputItem.ItemOutStatus = "failure";
                outputItem.ItemErrorMsg = "some other error message";
                outputItem.ItemOutCode = "some other output code";
                response.ItemsToPostOutput.Add(outputItem);

                return response;
            }

            private ImportGradesFromILPResponse GetMixedResponse_ForMultiStudent()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();
                ItemsToPostOutput outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "1";
                outputItem.ItemOutStatus = "failure";
                outputItem.ItemErrorMsg = "some error message";
                outputItem.ItemOutCode = "some output code";
                response.ItemsToPostOutput.Add(outputItem);

                outputItem = new ItemsToPostOutput();
                outputItem.ItemOutPerson = "2";
                outputItem.ItemOutStatus = "success";
                response.ItemsToPostOutput.Add(outputItem);

                return response;
            }
        }

        [TestClass]
        public class SectionRepository_GradeImportFailureTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;
            SectionRepository sectionRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                apiSettingsMock = new ApiSettings("null");

                var transactionResponse = GetTransactionResponse();
                var grades = GetSectionGrades();
                // Identify transaction request based on GetSectionGrades by having 9 items and the section ID. Return GetTransactionResponse which contains
                // failures.
                transManagerMock.Setup<Task<ImportGradesFromILPResponse>>(mgr => mgr.ExecuteAsync<ImportGradesFromILPRequest, ImportGradesFromILPResponse>(
                    It.Is<ImportGradesFromILPRequest>(x => x.ItemsToPostInput.Count() == 9 &&
                        x.SectionId == grades.SectionId)
                    )).Returns(Task.FromResult(transactionResponse));

                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettingsMock);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                cacheProviderMock = null;
                transManagerMock = null;
                sectionRepo = null;
                apiSettingsMock = null;
            }

            [TestMethod]
            public async Task SectionRepository_ImportGradesFailure()
            {
                var grades = GetSectionGrades();
                var response = await sectionRepo.ImportGradesAsync(grades, false, true, GradesPutCallerTypes.ILP, false);
                var expectedData = GetTransactionResponse();

                foreach (var r in response.StudentResponses)
                {
                    Assert.AreEqual(GetSectionGrades().StudentGrades[0].StudentId, r.StudentId);
                    Assert.AreEqual("failure", r.Status);
                    Assert.IsTrue(r.Errors.Count() > 0);
                }
            }

            private SectionGrades GetSectionGrades()
            {
                var grades = new SectionGrades();
                grades.SectionId = "123";
                var grade = new StudentGrade();
                grade.StudentId = "101";
                grade.MidtermGrade1 = "A";
                grade.MidtermGrade2 = "B";
                grade.MidtermGrade3 = "C";
                grade.MidtermGrade4 = "D";
                grade.MidtermGrade5 = "E";
                grade.MidtermGrade6 = "F";
                grade.FinalGrade = "G";
                grade.FinalGradeExpirationDate = DateTime.Now;
                grade.ClearFinalGradeExpirationDateFlag = false;
                grade.LastAttendanceDate = DateTime.Now;
                grade.ClearLastAttendanceDateFlag = false;
                grade.NeverAttended = true;
                grades.StudentGrades = new List<StudentGrade>();
                grades.StudentGrades.Add(grade);

                return grades;
            }

            private ImportGradesFromILPResponse GetTransactionResponse()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();
                response.SectionId = GetSectionGrades().SectionId;

                response.ItemsToPostOutput.Add(new ItemsToPostOutput() { ItemOutStatus = "failure", ItemOutCode = "Midterm1", ItemOutPerson = GetSectionGrades().StudentGrades[0].StudentId, ItemErrorMsg = "some error message" });
                response.ItemsToPostOutput.Add(new ItemsToPostOutput() { ItemOutStatus = "failure", ItemOutCode = "Midterm2", ItemOutPerson = GetSectionGrades().StudentGrades[0].StudentId, ItemErrorMsg = "some error message" });
                response.ItemsToPostOutput.Add(new ItemsToPostOutput() { ItemOutStatus = "failure", ItemOutCode = "Midterm3", ItemOutPerson = GetSectionGrades().StudentGrades[0].StudentId, ItemErrorMsg = "some error message" });
                response.ItemsToPostOutput.Add(new ItemsToPostOutput() { ItemOutStatus = "failure", ItemOutCode = "Midterm4", ItemOutPerson = GetSectionGrades().StudentGrades[0].StudentId, ItemErrorMsg = "some error message" });
                response.ItemsToPostOutput.Add(new ItemsToPostOutput() { ItemOutStatus = "failure", ItemOutCode = "Midterm5", ItemOutPerson = GetSectionGrades().StudentGrades[0].StudentId, ItemErrorMsg = "some error message" });
                response.ItemsToPostOutput.Add(new ItemsToPostOutput() { ItemOutStatus = "failure", ItemOutCode = "SOME-OTHER-CODE", ItemOutPerson = GetSectionGrades().StudentGrades[0].StudentId, ItemErrorMsg = "some error message" });

                return response;
            }
        }

        [TestClass]
        public class SectionRepository_GradeImportExceptionTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;
            SectionRepository sectionRepo;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            Mock<IColleagueDataReader> dataReaderMock;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                apiSettingsMock = new ApiSettings("null");
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                  x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                  .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                var transactionResponse = GetTransactionErrorResponse();


                // Mock the read of instructional methods


                InstrMethods lec = new InstrMethods()
                {
                    InmDesc = "LEC",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                    Recordkey = "LEC"
                };
                InstrMethods lab = new InstrMethods()
                {
                    InmDesc = "LAB",
                    InmOnline = "N",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                    Recordkey = "LAB"
                };
                InstrMethods onl = new InstrMethods()
                {
                    InmDesc = "ONL",
                    InmOnline = "Y",
                    RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                    Recordkey = "ONL"
                };
                dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));
                // Mock grade import CTX to return an error code, which should cause the repository to throw an exception.
                transManagerMock.Setup<Task<ImportGradesFromILPResponse>>(mgr => mgr.ExecuteAsync<ImportGradesFromILPRequest, ImportGradesFromILPResponse>(
                    It.IsAny<ImportGradesFromILPRequest>()
                    )).Returns(Task.FromResult(transactionResponse));
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettingsMock);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                cacheProviderMock = null;
                sectionRepo = null;
                apiSettingsMock = null;
            }

            [TestClass]
            public class GetNonCachedFacultySectionsTests
            {
                // Broke out these tests to control the subset of records and the data. Not as detailed
                // as GetNonCachedSectionsTests as the GetNonCachedFacultySections being tested selects
                // sectionIds and defers to GetNonCachedSections to marshal the details of the sections 
                // returned (that repo method has its own set of tests)
                Mock<IColleagueTransactionFactory> transFactoryMock;
                Mock<IColleagueTransactionInvoker> mockManager;
                Mock<ICacheProvider> cacheProviderMock;
                Mock<IColleagueDataReader> dataAccessorMock;
                Mock<ILogger> loggerMock;
                Collection<CourseSections> sectionsResponseData;
                Collection<CourseSecMeeting> sectionMeetingResponseData;
                Collection<CourseSecFaculty> sectionFacultyResponseData;
                Collection<CourseSecFaculty> sectionFaculty0000049Term2012FAResponseData;
                Collection<CourseSecFaculty> sectionFaculty0000049Term2013SPResponseData;
                Collection<CourseSecFaculty> sectionFaculty0000049MultiTermResponseData;
                List<StudentCourseSectionStudents> studentCourseSecResponseData;
                Collection<PortalSites> portalSitesResponseData;
                Collection<CourseSecXlists> crosslistResponseData;
                Collection<CourseSecPending> pendingResponseData;
                Collection<WaitList> waitlistResponseData;
                IEnumerable<Section> reqSections;
                List<Term> registrationTerms = new List<Term>();
                ApiSettings apiSettingsMock;
                string noDataFacId = "9999999";
                string goodFacId = "0000049";
                Term term0;
                Term term1;
                Term term2;

                SectionRepository sectionRepo;

                [TestInitialize]
                public void Initialize()
                {
                    loggerMock = new Mock<ILogger>();
                    mockManager = new Mock<IColleagueTransactionInvoker>();
                    dataAccessorMock = new Mock<IColleagueDataReader>();
                    transFactoryMock = new Mock<IColleagueTransactionFactory>();

                    term0 = new Term("2012/S2", "Summer 2012-2", new DateTime(2012, 7, 5), new DateTime(2012, 8, 15), 2012, 0, true, true, "2012/S2", true);
                    term1 = new Term("2012/FA", "Fall 2012", new DateTime(2012, 9, 1), new DateTime(2012, 12, 15), 2012, 1, true, true, "2012/FA", true);
                    term2 = new Term("2013/SP", "Spring 2013", new DateTime(2013, 1, 1), new DateTime(2013, 5, 15), 2012, 2, true, true, "2013/SP", true);
                    // Mock the read of instructional methods


                    InstrMethods lec = new InstrMethods()
                    {
                        InmDesc = "LEC",
                        InmOnline = "N",
                        RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                        Recordkey = "LEC"
                    };
                    InstrMethods lab = new InstrMethods()
                    {
                        InmDesc = "LAB",
                        InmOnline = "N",
                        RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                        Recordkey = "LAB"
                    };
                    InstrMethods onl = new InstrMethods()
                    {
                        InmDesc = "ONL",
                        InmOnline = "Y",
                        RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                        Recordkey = "ONL"
                    };
                    dataAccessorMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));

                    // Set up dataAccessorMock as the object for the DataAccessor
                    transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                    GetSectionWaitlistStatusResponse wlResp = new GetSectionWaitlistStatusResponse() { ErrorMessages = new List<string>(), Status = "Wlst" };
                    mockManager.Setup(mgr => mgr.ExecuteAsync<GetSectionWaitlistStatusRequest, GetSectionWaitlistStatusResponse>(It.IsAny<GetSectionWaitlistStatusRequest>())).ReturnsAsync(wlResp);

                }

                [TestCleanup]
                public void Cleanup()
                {
                    transFactoryMock = null;
                    dataAccessorMock = null;
                    cacheProviderMock = null;
                    sectionsResponseData = null;
                    sectionMeetingResponseData = null;
                    sectionFacultyResponseData = null;
                    sectionFaculty0000049Term2012FAResponseData = null;
                    sectionFaculty0000049Term2013SPResponseData = null;
                    sectionFaculty0000049MultiTermResponseData = null;
                    studentCourseSecResponseData = null;
                    portalSitesResponseData = null;
                    crosslistResponseData = null;
                    pendingResponseData = null;
                    waitlistResponseData = null;
                    reqSections = null;
                    registrationTerms = null;

                    sectionRepo = null;
                }

                private async Task SetupGetNonCachedFacultySectionsData(List<string> sectionIdsToRetrieve)
                {
                    reqSections = await new TestSectionRepository().GetNonCachedSectionsAsync(sectionIdsToRetrieve);
                    registrationTerms.Add(term1);
                    registrationTerms.Add(term2);
                    sectionsResponseData = BuildSectionsResponse(reqSections);
                    sectionMeetingResponseData = BuildSectionMeetingsResponse(reqSections);
                    sectionFacultyResponseData = BuildSectionFacultyResponse(reqSections);
                    sectionFaculty0000049Term2012FAResponseData = BuildSectionFaculty0000049Term2012FAResponse(reqSections);
                    sectionFaculty0000049Term2013SPResponseData = BuildSectionFaculty0000049Term2013SPResponse(reqSections);
                    sectionFaculty0000049MultiTermResponseData = BuildSectionFaculty0000049MultiTermResponse(reqSections);
                    sectionFacultyResponseData = BuildSectionFacultyResponse(reqSections);
                    studentCourseSecResponseData = BuildStudentCourseSecStudents(reqSections);
                    portalSitesResponseData = BuildPortalSitesResponse(reqSections);
                    crosslistResponseData = BuildCrosslistResponse(reqSections);
                    pendingResponseData = BuildPendingSectionResponse(reqSections);
                    waitlistResponseData = BuildWaitlistResponse(reqSections);

                    // Build section repository
                    sectionRepo = BuildValidSectionRepository();
                }

                [TestMethod]
                public async Task GetNonCachedFacultySections_NullTermList()
                {
                    await SetupGetNonCachedFacultySectionsData(new List<string>() { "1" });
                    IEnumerable<Section> sections = await sectionRepo.GetNonCachedFacultySectionsAsync(null, "1");
                    Assert.AreEqual(0, sections.Count());
                }

                [TestMethod]
                public async Task GetNonCachedFacultySections_EmptyTermList()
                {
                    await SetupGetNonCachedFacultySectionsData(new List<string>() { "1" });
                    List<Term> terms = new List<Term>();
                    IEnumerable<Section> sections = await sectionRepo.GetNonCachedFacultySectionsAsync(terms, "1");
                    Assert.AreEqual(0, sections.Count());
                }

                [TestMethod]
                public async Task GetNonCachedFacultySections_NullFacultyId()
                {
                    await SetupGetNonCachedFacultySectionsData(new List<string>() { "1" });
                    List<Term> terms = new List<Term>() { term0 };
                    IEnumerable<Section> sections = await sectionRepo.GetNonCachedFacultySectionsAsync(terms, null);
                    Assert.AreEqual(0, sections.Count());
                }

                [TestMethod]
                public async Task GetNonCachedFacultySections_EmptyFacultyId()
                {
                    await SetupGetNonCachedFacultySectionsData(new List<string>() { "1" });
                    List<Term> terms = new List<Term>() { term0 };
                    IEnumerable<Section> sections = await sectionRepo.GetNonCachedFacultySectionsAsync(terms, "");
                    Assert.AreEqual(0, sections.Count());
                }

                [TestMethod]
                public async Task GetNonCachedFacultySections_NoSectionsFound()
                {
                    await SetupGetNonCachedFacultySectionsData(new List<string>() { "1" });
                    List<Term> terms = new List<Term>() { term0 };
                    Collection<CourseSecFaculty> noCSF = new Collection<CourseSecFaculty>();
                    string criteria = "WITH CSF.FACULTY EQ '" + noDataFacId + "' AND CSF.START.DATE GE '07/05/2012' AND CSF.END.DATE LE '08/15/2012' AND CSF.SECTION.TERM EQ '2012/S2'''";
                    dataAccessorMock.Setup<Task<Collection<CourseSecFaculty>>>(csf => csf.BulkReadRecordAsync<CourseSecFaculty>(criteria, true)).Returns(Task.FromResult<Collection<CourseSecFaculty>>(noCSF));
                    IEnumerable<Section> sections = await sectionRepo.GetNonCachedFacultySectionsAsync(terms, noDataFacId);
                    Assert.AreEqual(0, sections.Count());
                }

                [TestMethod]
                public async Task GetNonCachedFacultySections_2012FASectionsReturned()
                {
                    var sectionIdsRequested = (await new TestSectionRepository().GetAsync()).
                        Where(s => s.TermId == "2012/FA" && s.FacultyIds.Contains(goodFacId)).Select(s => s.Id).ToList();
                    await SetupGetNonCachedFacultySectionsData(sectionIdsRequested);
                    List<Term> terms = new List<Term>() { term1 };
                    IEnumerable<Section> sections = await sectionRepo.GetNonCachedFacultySectionsAsync(terms, goodFacId);
                    Assert.AreEqual(sectionFaculty0000049Term2012FAResponseData.Count(), sections.Count());
                }

                [TestMethod]
                public async Task GetNonCachedFacultySections_2013SPSectionsReturned()
                {
                    var sectionIdsRequested = (await new TestSectionRepository().GetAsync()).
                        Where(s => s.TermId == "2013/SP" && s.FacultyIds.Contains(goodFacId)).Select(s => s.Id).ToList();
                    SetupGetNonCachedFacultySectionsData(sectionIdsRequested);
                    List<Term> terms = new List<Term>() { term2 };
                    IEnumerable<Section> sections = await sectionRepo.GetNonCachedFacultySectionsAsync(terms, goodFacId);
                    Assert.AreEqual(sectionFaculty0000049Term2013SPResponseData.Count(), sections.Count());
                }

                [TestMethod]
                public async Task GetNonCachedFacultySections_MultiTermSectionsReturned()
                {
                    var sectionIdsRequested = (await new TestSectionRepository().GetAsync()).
                        Where(s => (s.TermId == "2013/SP" || s.TermId == "2012/FA") && s.FacultyIds.Contains(goodFacId)).Select(s => s.Id).ToList();
                    SetupGetNonCachedFacultySectionsData(sectionIdsRequested);
                    List<Term> terms = new List<Term>() { term1, term2 };
                    IEnumerable<Section> sections = await sectionRepo.GetNonCachedFacultySectionsAsync(terms, goodFacId);
                    int multiCount = sectionFaculty0000049Term2012FAResponseData.Count() + sectionFaculty0000049Term2013SPResponseData.Count();
                    Assert.AreEqual(multiCount, sections.Count());
                }
                [TestMethod]
                public async Task GetNonCachedFacultySections_ValidateSectionBooksOption()
                {
                    var sectionIdsRequested = (await new TestSectionRepository().GetAsync()).
                        Where(s => s.TermId == "2012/FA" && s.FacultyIds.Contains(goodFacId)).Select(s => s.Id).ToList();
                    await SetupGetNonCachedFacultySectionsData(sectionIdsRequested);
                    List<Term> terms = new List<Term>() { term1 };
                    IEnumerable<Section> sections = await sectionRepo.GetNonCachedFacultySectionsAsync(terms, goodFacId);
                    //have books with valid  book options
                    Section sec1 = sections.Where(sec => sec.CourseId == "7702").FirstOrDefault();
                    //have books with null and empty book options
                    Section sec2 = sections.Where(sec => sec.CourseId == "7705").FirstOrDefault();
                    //have books with null book option collection
                    Section sec3 = sections.Where(sec => sec.Id == "2").FirstOrDefault();
                    //have no books at all
                    Section sec4 = sections.Where(sec => sec.Id == "11").FirstOrDefault();
                    Assert.AreEqual(sec1.Books.Count, 2);
                    // Returns a count of 2 for sec2 and sec3 for SEC.BOOKS without associated SEC.BOOK.OPTIONS (which is valid in Colleague)
                    Assert.AreEqual(sec2.Books.Count, 2);
                    Assert.AreEqual(sec3.Books.Count, 2);
                    Assert.AreEqual(sec4.Books.Count, 0);
                    Assert.AreEqual(sectionFaculty0000049Term2012FAResponseData.Count(), sections.Count());
                }

                private SectionRepository BuildValidSectionRepository()
                {
                    // transaction factory mock
                    transFactoryMock = new Mock<IColleagueTransactionFactory>();

                    apiSettingsMock = new ApiSettings("null");

                    transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                    // Cache Mock
                    //localCacheMock = new Mock<ObjectCache>();
                    // Cache Provider Mock
                    cacheProviderMock = new Mock<ICacheProvider>();
                    // Set up data accessor for mocking 
                    dataAccessorMock = new Mock<IColleagueDataReader>();
                    cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
                        null,
                        new SemaphoreSlim(1, 1)
                        ));

                    // Set up repo response for initial section request (1, 2, 3)
                    dataAccessorMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).Returns(Task.FromResult(sectionsResponseData.Select(c => c.Recordkey).ToArray()));
                    dataAccessorMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionsResponseData));

                    // Set up repo response for "all" meeting requests
                    dataAccessorMock.Setup<Task<Collection<CourseSecMeeting>>>(macc => macc.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionMeetingResponseData));

                    // Set up repo response for "all" faculty
                    dataAccessorMock.Setup<Task<Collection<CourseSecFaculty>>>(facc => facc.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), true)).Returns(Task.FromResult(sectionFacultyResponseData));
                    string criteria0 = "WITH CSF.FACULTY EQ '0000049' AND CSF.START.DATE GE '07/05/2012' AND CSF.END.DATE LE '08/15/2012'";
                    dataAccessorMock.Setup<Task<Collection<CourseSecFaculty>>>(csf => csf.BulkReadRecordAsync<CourseSecFaculty>(criteria0, true)).Returns(Task.FromResult(new Collection<CourseSecFaculty>()));
                    string criteria1 = "WITH CSF.FACULTY EQ '0000049' AND CSF.START.DATE GE '09/01/2012' AND CSF.END.DATE LE '12/15/2012' AND CSF.SECTION.TERM EQ '2012/FA'''";
                    dataAccessorMock.Setup<Task<Collection<CourseSecFaculty>>>(csf => csf.BulkReadRecordAsync<CourseSecFaculty>(criteria1, true)).Returns(Task.FromResult(sectionFaculty0000049Term2012FAResponseData));
                    string criteria2 = "WITH CSF.FACULTY EQ '0000049' AND CSF.START.DATE GE '01/01/2013' AND CSF.END.DATE LE '05/15/2013' AND CSF.SECTION.TERM EQ '2013/SP'''";
                    dataAccessorMock.Setup<Task<Collection<CourseSecFaculty>>>(csf => csf.BulkReadRecordAsync<CourseSecFaculty>(criteria2, true)).Returns(Task.FromResult(sectionFaculty0000049Term2013SPResponseData));
                    string criteria3 = "WITH CSF.FACULTY EQ '0000049' AND CSF.START.DATE GE '09/01/2012' AND CSF.END.DATE LE '05/15/2013' AND CSF.SECTION.TERM EQ '2012/FA' '2013/SP'''";
                    dataAccessorMock.Setup<Task<Collection<CourseSecFaculty>>>(csf => csf.BulkReadRecordAsync<CourseSecFaculty>(criteria3, true)).Returns(Task.FromResult(sectionFaculty0000049MultiTermResponseData));

                    dataAccessorMock.Setup(ac => ac.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), "SAVING SCS.COURSE.SECTION")).ReturnsAsync(studentCourseSecResponseData.Select(c => c.CourseSectionIds).ToArray());
                    dataAccessorMock.Setup(ac => ac.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string[]>(), "SAVING SCS.STUDENT")).ReturnsAsync(studentCourseSecResponseData.Select(c => c.StudentIds).ToArray());

                    dataAccessorMock.Setup<Task<Collection<PortalSites>>>(ps => ps.BulkReadRecordAsync<PortalSites>("PORTAL.SITES", It.IsAny<string[]>(), true)).Returns(Task.FromResult(portalSitesResponseData));
                    dataAccessorMock.Setup<Task<Collection<CourseSecXlists>>>(sxl => sxl.BulkReadRecordAsync<CourseSecXlists>("COURSE.SEC.XLISTS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(crosslistResponseData));
                    dataAccessorMock.Setup<Task<Collection<CourseSecPending>>>(csp => csp.BulkReadRecordAsync<CourseSecPending>("COURSE.SEC.PENDING", It.IsAny<string[]>(), true)).Returns(Task.FromResult(pendingResponseData));
                    dataAccessorMock.Setup<Task<Collection<WaitList>>>(wl => wl.BulkReadRecordAsync<WaitList>("WAIT.LIST", It.IsAny<string>(), true)).Returns(Task.FromResult(waitlistResponseData));

                    var stWebDflt = BuildStwebDefaults(); ;
                    dataAccessorMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                        (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                        );

                    dataAccessorMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                       (id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                       );


                    // Set up repo response for section statuses
                    var sectionStatuses = new ApplValcodes();
                    sectionStatuses.ValsEntityAssociation = new List<ApplValcodesVals>();
                    sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("A", "Active", "1", "A", "", "", ""));
                    sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("I", "Inactive", "2", "I", "", "", ""));
                    sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Cancelled", "", "C", "", "", ""));
                    dataAccessorMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", true)).Returns(Task.FromResult(sectionStatuses));

                    // Set up repo response for waitlist statuses
                    ApplValcodes waitlistCodeResponse = new ApplValcodes()
                    {
                        ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "1" },
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValActionCode1AssocMember = "2"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "D", ValActionCode1AssocMember = "3"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "P", ValActionCode1AssocMember = "4"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "5"}}
                    };
                    dataAccessorMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "WAIT.LIST.STATUSES", true)).Returns(Task.FromResult(waitlistCodeResponse));

                    // Set up repo response for the temporary international parameter item
                    Data.Base.DataContracts.IntlParams intlParams = new Data.Base.DataContracts.IntlParams();
                    intlParams.HostDateDelimiter = "/";
                    intlParams.HostShortDateFormat = "MDY";
                    dataAccessorMock.Setup<Task<Data.Base.DataContracts.IntlParams>>(iacc => iacc.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);

                    // Set up course defaults response (indicates if coreq conversion has taken place)
                    var courseParameters = BuildCourseParametersConvertedResponse();
                    dataAccessorMock.Setup<Task<CdDefaults>>(acc => acc.ReadRecordAsync<CdDefaults>("ST.PARMS", "CD.DEFAULTS", true)).ReturnsAsync(courseParameters);

                    // Set up instructional method response (indicates if sections are online)
                    var instrMethods = BuildValidInstrMethodResponse();
                    dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<InstrMethods>("INSTR.METHODS", "", true)).ReturnsAsync(instrMethods);

                    GetSectionWaitlistStatusResponse wlResp = new GetSectionWaitlistStatusResponse() { ErrorMessages = new List<string>(), Status = "Wlst" };
                    mockManager.Setup(mgr => mgr.ExecuteAsync<GetSectionWaitlistStatusRequest, GetSectionWaitlistStatusResponse>(It.IsAny<GetSectionWaitlistStatusRequest>())).ReturnsAsync(wlResp);

                    // Mock the read of instructional methods


                    InstrMethods lec = new InstrMethods()
                    {
                        InmDesc = "LEC",
                        InmOnline = "N",
                        RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                        Recordkey = "LEC"
                    };
                    InstrMethods lab = new InstrMethods()
                    {
                        InmDesc = "LAB",
                        InmOnline = "N",
                        RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                        Recordkey = "LAB"
                    };
                    InstrMethods onl = new InstrMethods()
                    {
                        InmDesc = "ONL",
                        InmOnline = "Y",
                        RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                        Recordkey = "ONL"
                    };
                    dataAccessorMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));

                    // Set up dataAccessorMock as the object for the DataAccessor
                    transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                    // Set up repo response for reg billing rates
                    Collection<RegBillingRates> rbrs = new Collection<RegBillingRates>()
                    {
                        new RegBillingRates()
                        {
                            Recordkey = "123",
                            RgbrAmtCalcType = "A",
                            RgbrArCode = "ABC",
                            RgbrChargeAmt = 50m,
                            RgbrRule = "RULE1",
                        },
                        new RegBillingRates()
                        {
                            Recordkey = "124",
                            RgbrAmtCalcType = "F",
                            RgbrArCode = "DEF",
                            RgbrCrAmt = 100m
                        },
                    };
                    dataAccessorMock.Setup<Task<Collection<RegBillingRates>>>(cacc => cacc.BulkReadRecordAsync<RegBillingRates>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(rbrs);

                    var bookOptions = new ApplValcodes();
                    bookOptions.ValsEntityAssociation = new List<ApplValcodesVals>();
                    bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("R", "Required", "1", "R", "", "", ""));
                    bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Recommended", "2", "C", "", "", ""));
                    bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("O", "Optional", "2", "O", "", "", ""));
                    dataAccessorMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BOOK.OPTION", true)).ReturnsAsync(bookOptions);


                    // Construct section repository
                    sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettingsMock);

                    return sectionRepo;
                }

                private SectionRepository BuildInvalidSectionRepository()
                {
                    var transFactoryMock = new Mock<IColleagueTransactionFactory>();
                    apiSettingsMock = new ApiSettings("null");

                    // Set up data accessor for mocking 
                    var dataAccessorMock = new Mock<IColleagueDataReader>();
                    transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                    // Set up repo response for "all" section requests
                    Exception expectedFailure = new Exception("fail");
                    dataAccessorMock.Setup<Task<Collection<CourseSections>>>(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), true)).Throws(expectedFailure);

                    // Cache Mock
                    var localCacheMock = new Mock<ObjectCache>();
                    // Cache Provider Mock
                    var cacheProviderMock = new Mock<ICacheProvider>();
                    //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

                    // Construct section repository
                    sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettingsMock);

                    return sectionRepo;
                }

                private Collection<CourseSections> BuildSectionsResponse(IEnumerable<Section> sections)
                {
                    Collection<CourseSections> repoSections = new Collection<CourseSections>();
                    foreach (var section in sections)
                    {
                        var crsSec = new CourseSections();
                        crsSec.Recordkey = section.Id.ToString();
                        crsSec.SecAcadLevel = section.AcademicLevelCode;
                        crsSec.SecCeus = section.Ceus;
                        crsSec.SecCourse = section.CourseId.ToString();
                        crsSec.SecCourseLevels = section.CourseLevelCodes.ToList();
                        crsSec.SecDepts = section.Departments.Select(x => x.AcademicDepartmentCode).ToList();
                        crsSec.SecLocation = section.Location;
                        crsSec.SecMaxCred = section.MaximumCredits;
                        crsSec.SecVarCredIncrement = section.VariableCreditIncrement;
                        crsSec.SecMinCred = section.MinimumCredits;
                        crsSec.SecNo = section.Number;
                        crsSec.SecShortTitle = section.Title;
                        crsSec.SecStartDate = section.StartDate;
                        crsSec.SecEndDate = section.EndDate;
                        crsSec.SecTerm = section.TermId;
                        crsSec.SecOnlyPassNopassFlag = section.OnlyPassNoPass == true ? "Y" : "N";
                        crsSec.SecAllowPassNopassFlag = section.AllowPassNoPass == true ? "Y" : "N";
                        crsSec.SecAllowAuditFlag = section.AllowAudit == true ? "Y" : "N";
                        var csm = new List<string>() { "1", "2" };
                        crsSec.SecMeeting = csm;
                        var csf = new List<string>() { "1", "2" };
                        crsSec.SecFaculty = csf;
                        var sas = new List<string>() { "1", "2" };
                        crsSec.SecActiveStudents = sas;
                        crsSec.SecStatus = new List<string>() { "A", "P" };
                        crsSec.SecCourseTypes = section.CourseTypeCodes.ToList();
                        crsSec.SecAllowWaitlistFlag = section.AllowWaitlist == true ? "Y" : "N";
                        crsSec.SecCredType = section.CreditTypeCode;
                        crsSec.SecInstrMethods = new List<string>() { "LEC" };


                        // Reconstruct all Colleague requisite and corequisite fields from the data in the Requisites 
                        // post-conversion fields
                        crsSec.SecReqs = new List<string>();
                        crsSec.SecRecommendedSecs = new List<string>();
                        crsSec.SecCoreqSecs = new List<string>();
                        crsSec.SecMinNoCoreqSecs = null;
                        crsSec.SecOverrideCrsReqsFlag = section.OverridesCourseRequisites == true ? "Y" : "";
                        // pre-conversion fields
                        crsSec.SecCourseCoreqsEntityAssociation = new List<CourseSectionsSecCourseCoreqs>();
                        crsSec.SecCoreqsEntityAssociation = new List<CourseSectionsSecCoreqs>();
                        crsSec.SecStatusesEntityAssociation = section.Statuses.Select(x => new CourseSectionsSecStatuses(x.Date, ConvertSectionStatusToCode(x.Status))).ToList();
                        crsSec.SecDepartmentsEntityAssociation = new List<CourseSectionsSecDepartments>();
                        crsSec.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("MATH", 75m));
                        crsSec.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("PSYC", 25m));

                        foreach (var req in section.Requisites)
                        {
                            if (!string.IsNullOrEmpty(req.RequirementCode))
                            {
                                // post-conversion
                                crsSec.SecReqs.Add(req.RequirementCode);
                                // pre-conversion -- this does not convert
                            }
                            else if (!string.IsNullOrEmpty(req.CorequisiteCourseId))
                            {
                                // post-conversion -- this does not convert
                                // pre-conversion
                                crsSec.SecCourseCoreqsEntityAssociation.Add(
                                    new CourseSectionsSecCourseCoreqs(req.CorequisiteCourseId, (req.IsRequired == true) ? "Y" : "")
                                );
                            }

                        }

                        foreach (var req in section.SectionRequisites)
                        {
                            if (req.CorequisiteSectionIds != null && req.CorequisiteSectionIds.Count() > 0)
                            {
                                foreach (var secId in req.CorequisiteSectionIds)
                                {
                                    // post-conversion--put required and recommended into two separate lists
                                    if (req.IsRequired)
                                    {
                                        crsSec.SecCoreqSecs.Add(secId);
                                        // The minimum number is associated with the entire list of SecCoreqSecs
                                        crsSec.SecMinNoCoreqSecs = req.NumberNeeded;
                                    }
                                    else
                                    {
                                        crsSec.SecRecommendedSecs.Add(secId);
                                    }
                                    // pre-conversion--each requisite section has a required flag
                                    crsSec.SecCoreqsEntityAssociation.Add(
                                        new CourseSectionsSecCoreqs(secId, (req.IsRequired == true) ? "Y" : "")
                                        );
                                }
                            }
                        }

                        if (section.Books != null)
                        {
                            crsSec.SecBooks = new List<string>();
                            crsSec.SecBookOptions = new List<string>();
                            foreach (var book in section.Books)
                            {
                                crsSec.SecBooks.Add(book.BookId);
                                crsSec.SecBookOptions.Add(book.RequirementStatusCode);
                            }

                        }
                        crsSec.SecPortalSite = ""; // (!string.IsNullOrEmpty(section.LearningProvider) ? crsSec.Recordkey : "");
                        if (!string.IsNullOrEmpty(section.LearningProvider))
                        {
                            crsSec.SecPortalSite = section.Id;
                        }
                        else
                        {
                            crsSec.SecPortalSite = "";
                        }
                        if (section.Id == "1")
                        {
                            crsSec.SecXlist = "234";
                            crsSec.SecCapacity = 10;
                            crsSec.SecSubject = "MATH";
                            crsSec.SecCourseNo = "1000";
                        }
                        if (section.Id == "2")
                        {
                            crsSec.SecBookOptions = null;
                            crsSec.SecBooks = new List<string>();
                            crsSec.SecBooks.Add("book1");
                            crsSec.SecBooks.Add("book2");
                        }
                        repoSections.Add(crsSec);
                    }
                    return repoSections;
                }

                private Collection<CourseSecMeeting> BuildSectionMeetingsResponse(IEnumerable<Section> sections)
                {
                    Collection<CourseSecMeeting> repoSecMeetings = new Collection<CourseSecMeeting>();
                    int crsSecMId = 0;
                    foreach (var section in sections)
                    {
                        foreach (var mt in section.Meetings)
                        {
                            var crsSecM = new CourseSecMeeting();
                            crsSecMId += 1;
                            crsSecM.Recordkey = crsSecMId.ToString();
                            if (!string.IsNullOrEmpty(mt.Room))
                            {
                                crsSecM.CsmBldg = "ABLE";
                                crsSecM.CsmRoom = "A100";
                            }
                            crsSecM.CsmCourseSection = section.Id;
                            crsSecM.CsmInstrMethod = mt.InstructionalMethodCode;
                            crsSecM.CsmStartTime = mt.StartTime.HasValue ? mt.StartTime.Value.DateTime : (DateTime?)null;
                            crsSecM.CsmEndTime = mt.EndTime.HasValue ? mt.EndTime.Value.DateTime : (DateTime?)null;
                            foreach (var d in mt.Days)
                            {
                                switch (d)
                                {
                                    case DayOfWeek.Friday:
                                        crsSecM.CsmFriday = "Y";
                                        break;
                                    case DayOfWeek.Monday:
                                        crsSecM.CsmMonday = "Y";
                                        break;
                                    case DayOfWeek.Saturday:
                                        crsSecM.CsmSaturday = "Y";
                                        break;
                                    case DayOfWeek.Sunday:
                                        crsSecM.CsmSunday = "Y";
                                        break;
                                    case DayOfWeek.Thursday:
                                        crsSecM.CsmThursday = "Y";
                                        break;
                                    case DayOfWeek.Tuesday:
                                        crsSecM.CsmTuesday = "Y";
                                        break;
                                    case DayOfWeek.Wednesday:
                                        crsSecM.CsmWednesday = "Y";
                                        break;
                                    default:
                                        break;
                                }
                            }
                            repoSecMeetings.Add(crsSecM);
                        }

                    }
                    return repoSecMeetings;
                }

                private Collection<CourseSecFaculty> BuildSectionFacultyResponse(IEnumerable<Section> sections)
                {
                    Collection<CourseSecFaculty> repoSecFaculty = new Collection<CourseSecFaculty>();
                    int crsSecFId = 0;
                    foreach (var section in sections)
                    {
                        foreach (var fac in section.FacultyIds)
                        {
                            var crsSecF = new CourseSecFaculty();
                            crsSecFId += 1;
                            crsSecF.Recordkey = crsSecFId.ToString();
                            crsSecF.CsfCourseSection = section.Id;
                            crsSecF.CsfFaculty = fac;
                            repoSecFaculty.Add(crsSecF);
                        }

                    }
                    return repoSecFaculty;
                }

                private Collection<CourseSecFaculty> BuildSectionFaculty0000049Term2012FAResponse(IEnumerable<Section> sections)
                {
                    Collection<CourseSecFaculty> repoSecFaculty = new Collection<CourseSecFaculty>();
                    int crsSecFId = 0;
                    foreach (var section in sections)
                    {
                        foreach (var fac in section.FacultyIds)
                        {
                            if (section.TermId.Equals("2012/FA") && fac.Equals("0000049"))
                            {
                                var crsSecF = new CourseSecFaculty();
                                crsSecFId += 1;
                                crsSecF.Recordkey = crsSecFId.ToString();
                                crsSecF.CsfCourseSection = section.Id;
                                crsSecF.CsfFaculty = fac;
                                repoSecFaculty.Add(crsSecF);
                            }
                        }

                    }
                    return repoSecFaculty;
                }

                private Collection<CourseSecFaculty> BuildSectionFaculty0000049Term2013SPResponse(IEnumerable<Section> sections)
                {
                    Collection<CourseSecFaculty> repoSecFaculty = new Collection<CourseSecFaculty>();
                    int crsSecFId = 0;
                    foreach (var section in sections)
                    {
                        foreach (var fac in section.FacultyIds)
                        {
                            if (section.TermId.Equals("2013/SP") && fac.Equals("0000049"))
                            {
                                var crsSecF = new CourseSecFaculty();
                                crsSecFId += 1;
                                crsSecF.Recordkey = crsSecFId.ToString();
                                crsSecF.CsfCourseSection = section.Id;
                                crsSecF.CsfFaculty = fac;
                                repoSecFaculty.Add(crsSecF);
                            }
                        }

                    }
                    return repoSecFaculty;
                }

                private Collection<CourseSecFaculty> BuildSectionFaculty0000049MultiTermResponse(IEnumerable<Section> sections)
                {
                    Collection<CourseSecFaculty> repoSecFaculty = new Collection<CourseSecFaculty>();
                    int crsSecFId = 0;
                    foreach (var section in sections)
                    {
                        foreach (var fac in section.FacultyIds)
                        {
                            if ((section.TermId.Equals("2012/FA") || section.TermId.Equals("2013/SP")) && fac.Equals("0000049"))
                            {
                                var crsSecF = new CourseSecFaculty();
                                crsSecFId += 1;
                                crsSecF.Recordkey = crsSecFId.ToString();
                                crsSecF.CsfCourseSection = section.Id;
                                crsSecF.CsfFaculty = fac;
                                repoSecFaculty.Add(crsSecF);
                            }
                        }

                    }
                    return repoSecFaculty;
                }

                private List<StudentCourseSectionStudents> BuildStudentCourseSecStudents(IEnumerable<Section> sections)
                {
                    var StudentCourseSectionStudents = new List<StudentCourseSectionStudents>();
                    Collection<StudentCourseSec> repoSCS = new Collection<StudentCourseSec>();
                    foreach (var section in sections)
                    {
                        foreach (var stu in section.ActiveStudentIds)
                        {
                            StudentCourseSectionStudents scss = new StudentCourseSectionStudents();
                            scss.CourseSectionIds = section.Id;
                            scss.StudentIds = stu;
                            StudentCourseSectionStudents.Add(scss);
                        }
                    }
                    return StudentCourseSectionStudents;
                }

                private Collection<PortalSites> BuildPortalSitesResponse(IEnumerable<Section> sections)
                {
                    Collection<PortalSites> repoPS = new Collection<PortalSites>();
                    foreach (var section in sections)
                    {
                        if (section.CourseId == "7272")
                        {
                            var ps = new PortalSites();
                            // normally some thing like "HIST-190-001-cs11347", but mock portal site Id with section ID
                            ps.Recordkey = section.Id;
                            if (section.Number == "98")
                            {
                                ps.PsLearningProvider = "";
                                ps.PsPrtlSiteGuid = section.Id;
                            }
                            if (section.Number == "97")
                            {
                                ps.PsLearningProvider = "MOODLE";
                                ps.PsPrtlSiteGuid = section.Id;
                            }
                            repoPS.Add(ps);
                        }
                    }
                    return repoPS;
                }

                private Collection<CourseSecXlists> BuildCrosslistResponse(IEnumerable<Section> sections)
                {
                    // currently built only for ILP testing
                    Collection<CourseSecXlists> repoXL = new Collection<CourseSecXlists>();
                    foreach (var section in sections)
                    {
                        if (section.Id == "1")
                        {
                            var xl = new CourseSecXlists();
                            xl.Recordkey = "232";
                            xl.CsxlPrimarySection = section.Id;
                            xl.CsxlCourseSections = new List<string>() { section.Id, "1", "5" };
                            xl.CsxlCapacity = 20;
                            xl.CsxlWaitlistMax = 5;
                            xl.CsxlWaitlistFlag = "Y";
                            repoXL.Add(xl);
                        }
                    }
                    return repoXL;
                }

                private Collection<CourseSecPending> BuildPendingSectionResponse(IEnumerable<Section> sections)
                {
                    Collection<CourseSecPending> repoPending = new Collection<CourseSecPending>();
                    foreach (var section in sections)
                    {
                        if (section.CourseId == "7272" && (section.Number == "98" || section.Number == "97"))
                        {
                            var cp = new CourseSecPending();
                            cp.Recordkey = section.Id;
                            if (section.Number == "98")
                            {
                                cp.CspReservedSeats = 1;
                            }
                            if (section.Number == "97")
                            {
                                cp.CspReservedSeats = 2;
                            }
                            repoPending.Add(cp);
                        }
                    }
                    return repoPending;
                }

                private Collection<WaitList> BuildWaitlistResponse(IEnumerable<Section> sections)
                {
                    Collection<WaitList> repoWaitlist = new Collection<WaitList>();
                    foreach (var section in sections)
                    {
                        if (section.CourseId == "7272" && (section.Number == "98" || section.Number == "97"))
                        {
                            var wl = new WaitList();
                            wl.Recordkey = section.Id;
                            if (section.Number == "98")
                            {
                                wl.WaitCourseSection = section.Id;
                                wl.WaitStatus = "P";
                                wl.WaitStudent = "111111";
                            }
                            if (section.Number == "97")
                            {
                                wl.WaitCourseSection = section.Id;
                                wl.WaitStatus = "P";
                                wl.WaitStudent = "22222";
                            }
                            repoWaitlist.Add(wl);
                        }
                    }
                    return repoWaitlist;
                }

                private CdDefaults BuildCourseParametersConvertedResponse()
                {
                    // Converted Response
                    CdDefaults defaults = new CdDefaults();
                    defaults.Recordkey = "CD.DEFAULTS";
                    defaults.CdReqsConvertedFlag = "Y";
                    return defaults;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_ImportGradesSectionNull()
            {
                await sectionRepo.ImportGradesAsync(null, false, true, GradesPutCallerTypes.ILP, false);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionRepository_ImportGradesStudentGradesNull()
            {
                var grades = new SectionGrades();
                grades.StudentGrades = null;
                await sectionRepo.ImportGradesAsync(grades, false, true, GradesPutCallerTypes.ILP, false);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionRepository_ImportGradesStudentGradesEmpty()
            {
                var grades = new SectionGrades();
                grades.StudentGrades = new List<StudentGrade>();
                await sectionRepo.ImportGradesAsync(grades, false, true, GradesPutCallerTypes.ILP, false);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueApiException))]
            public async Task SectionRepository_ImportGradesTransactionException()
            {
                var grades = GetSectionGrades();
                await sectionRepo.ImportGradesAsync(grades, false, true, GradesPutCallerTypes.ILP, false);
            }

            private SectionGrades GetSectionGrades()
            {
                var grades = new SectionGrades();
                grades.SectionId = "123";
                var grade = new StudentGrade();
                grade.StudentId = "101";
                grade.MidtermGrade1 = "A";
                grade.MidtermGrade2 = "B";
                grade.MidtermGrade3 = "C";
                grade.MidtermGrade4 = "D";
                grade.MidtermGrade5 = "E";
                grade.MidtermGrade6 = "F";
                grade.FinalGrade = "G";
                grade.FinalGradeExpirationDate = DateTime.Now;
                grade.LastAttendanceDate = DateTime.Now;
                grade.NeverAttended = true;
                grade.EffectiveStartDate = DateTime.Now;
                grade.EffectiveStartDate = DateTime.Now.AddDays(30);
                grades.StudentGrades = new List<StudentGrade>();
                grades.StudentGrades.Add(grade);

                return grades;
            }

            private ImportGradesFromILPResponse GetTransactionErrorResponse()
            {
                ImportGradesFromILPResponse response = new ImportGradesFromILPResponse();
                response.SectionId = GetSectionGrades().SectionId;
                response.ErrorCode = "MD-TID";

                return response;
            }
        }


        [TestClass]
        public class SectionRepository_EedmTests : SectionRepositoryTests
        {
            SectionRepository sectionRepo;
            CourseSections cs;
            Collection<CourseSections> courseSectionCollection = new Collection<CourseSections>();
            CourseSecMeeting csm;
            CourseSecFaculty csf;
            CdDefaults cdDefaults;
            PortalSites ps;
            string csId;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                //stuRepoHelperMock = new Mock<IStudentRepositoryHelper>();
                //stuRepoHelper = stuRepoHelperMock.Object;
                csId = "12345";

                cs = new CourseSections()
                {
                    RecordGuid = "23033dc3-06fc-4111-b910-77050b45cbe1",
                    Recordkey = csId,
                    RecordModelName = "sections",
                    SecAcadLevel = "UG",
                    SecActiveStudents = new List<string>(),
                    SecAllowAuditFlag = "N",
                    SecAllowPassNopassFlag = "N",
                    SecAllowWaitlistFlag = "Y",
                    SecBookOptions = new List<string>() { "R", "O" },
                    SecBooks = new List<string>() { "Book 1", "Book 2" },
                    SecCapacity = 30,
                    SecCeus = null,
                    SecCloseWaitlistFlag = "Y",
                    SecCourse = "210",
                    SecCourseLevels = new List<string>() { "100" },
                    SecCourseTypes = new List<string>() { "STND", "HONOR" },
                    SecCredType = "IN",
                    SecEndDate = new DateTime(2014, 12, 15),
                    SecFaculty = new List<string>(),
                    SecFacultyConsentFlag = "Y",
                    SecGradeScheme = "UGR",
                    SecInstrMethods = new List<string>() { "LEC", "LAB" },
                    SecLocation = "MAIN",
                    SecMaxCred = 6m,
                    SecMeeting = new List<string>(),
                    SecMinCred = 3m,
                    SecName = "MATH-4350-01",
                    SecNo = "01",
                    SecNoWeeks = 10,
                    SecOnlyPassNopassFlag = "N",
                    SecPortalSite = csId,
                    SecShortTitle = "Statistics",
                    SecStartDate = DateTime.Today.AddDays(-10),
                    SecTerm = "2014/FA",
                    SecTopicCode = "ABC",
                    SecVarCredIncrement = 1m,
                    SecWaitlistMax = 10,
                    SecWaitlistRating = "SR",
                    SecXlist = null,
                    SecFirstMeetingDate = new DateTime(2015, 10, 25),
                    SecLastMeetingDate = new DateTime(2017, 01, 02),
                    SecLearningProvider = "MOODLE",
                    SecSynonym = "0002334"
                };
                cs.SecEndDate = cs.SecStartDate.Value.AddDays(69);
                cs.SecContactEntityAssociation = new List<CourseSectionsSecContact>();
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LEC", 20.00m, 45.00m, "T", 37.50m));
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LAB", 10.00m, 15.00m, "T", 45.00m));
                cs.SecDepartmentsEntityAssociation = new List<CourseSectionsSecDepartments>();
                cs.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("MATH", 75m));
                cs.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("PSYC", 25m));
                cs.SecStatusesEntityAssociation = new List<CourseSectionsSecStatuses>();
                cs.SecStatusesEntityAssociation.Add(new CourseSectionsSecStatuses(new DateTime(2001, 5, 15), "A"));
                // Instr methods association - instructional method and load
                cs.SecContactEntityAssociation = new List<CourseSectionsSecContact>();
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LEC", 20.00m, 0m, "", 0m));
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LAB", 10.00m, 0m, "", 0m));
                // Pointer to CourseSecFaculty
                cs.SecFaculty.Add("1");
                // Pointer to CourseSecMeeting
                cs.SecMeeting.Add("1");

                //crosslistResponseData = BuildCrosslistResponse()

                BuildLdmConfiguration(dataReaderMock, out cdDefaults);

                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);

                // Set up repo response for course.sec.meeting
                csm = new CourseSecMeeting()
                {
                    Recordkey = "1",
                    CsmInstrMethod = "LEC",
                    CsmCourseSection = "12345",
                    CsmStartDate = DateTime.Today,
                    CsmEndDate = DateTime.Today.AddDays(27),
                    CsmStartTime = (new DateTime(1, 1, 1, 10, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmEndTime = (new DateTime(1, 1, 1, 11, 20, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmMonday = "Y"
                };
                MockRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", csm);

                // Set up repo response for course.sec.faculty
                csf = new CourseSecFaculty()
                {
                    Recordkey = "1",
                    CsfInstrMethod = "LEC",
                    CsfCourseSection = "12345",
                    CsfFaculty = "FAC1",
                    CsfFacultyPct = 100m,
                    CsfStartDate = cs.SecStartDate,
                    CsfEndDate = cs.SecEndDate,
                };
                MockRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", csf);

                MockRecordsAsync<CourseSecXlists>("COURSE.SEC.XLISTS", new Collection<CourseSecXlists>());
                MockRecordsAsync<CourseSecPending>("COURSE.SEC.PENDING", new Collection<CourseSecPending>());
                ps = new PortalSites() { Recordkey = csId, PsLearningProvider = "MOODLE", PsPrtlSiteGuid = csId };
                MockRecordsAsync<PortalSites>("PORTAL.SITES", new Collection<PortalSites>() { ps });
                MockRecordsAsync<WaitList>("WAIT.LIST", new Collection<WaitList>());
                MockRecordsAsync<AcadReqmts>("ACAD.REQMTS", new Collection<AcadReqmts>());

                MockRecordAsync<Dflts>("CORE.PARMS", new Dflts() { Recordkey = "DEFAULTS", DfltsCampusCalendar = "CAL" });
                // Mock data needed to read campus calendar
                var startTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 06, 00, 00);
                var endTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 23, 30, 00);
                MockRecordAsync<Data.Base.DataContracts.CampusCalendar>("CAL", new Data.Base.DataContracts.CampusCalendar() { Recordkey = "CAL", CmpcDesc = "Calendar", CmpcDayStartTime = startTime, CmpcDayEndTime = endTime, CmpcBookPastNoDays = "30", CmpcSpecialDays = specialDaysTestData.CampusSpecialDayIds });
                // Set up response for instructional methods and ST web defaults
                MockRecordsAsync<InstrMethods>("INSTR.METHODS", BuildValidInstrMethodResponse());

                // Set up repo response for section statuses
                var sectionStatuses = new ApplValcodes();
                sectionStatuses.ValsEntityAssociation = new List<ApplValcodesVals>();
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("A", "Active", "1", "A", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("I", "Inactive", "2", "I", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Cancelled", "", "C", "", "", ""));
                dataReaderMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", true)).ReturnsAsync(sectionStatuses);

                GetSectionWaitlistStatusResponse wlResp = new GetSectionWaitlistStatusResponse() { ErrorMessages = new List<string>(), Status = "Wlst" };

                // Mock the trxn getting the waitlist status
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetSectionWaitlistStatusRequest, GetSectionWaitlistStatusResponse>(It.IsAny<GetSectionWaitlistStatusRequest>())).ReturnsAsync(wlResp);


                //setup mocking for Stweb Defaults
                var stWebDflt = BuildStwebDefaults(); ;
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                    );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                   );

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    ));

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>() { "1" },
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                        new KeyCacheInfo()
                        {
                            KeyCacheMax = 5905,
                            KeyCacheMin = 1,
                            KeyCachePart = "000",
                            KeyCacheSize = 5905
                        },
                        new KeyCacheInfo()
                        {
                            KeyCacheMax = 7625,
                            KeyCacheMin = 5906,
                            KeyCachePart = "001",
                            KeyCacheSize = 1720
                        }
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                cs = null;
                courseSectionCollection = null;
                csm = null;
                csf = null;
                cdDefaults = null;
                ps = null;
                csId = null;
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionsAsync()
            {
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sublist, It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSES", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sublist);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                var regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Term>());
                var sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);


                var results = await sectionRepo.GetSectionsAsync(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "number", "learningProvider", "termId", "academicLevel", "course", "location",
                                                                       "status", "department", "", "");
                Assert.IsNotNull(results);
                var actuals = results.Item1;

                for (int i = 0; i < actuals.Count(); i++)
                {
                    var actual = actuals.ToList()[i];
                    var expected = courseSectionCollection.ToList()[i];
                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.Recordkey, actual.Id);
                    Assert.AreEqual(expected.SecAcadLevel, actual.AcademicLevelCode);
                    Assert.AreEqual(expected.SecCapacity, actual.Capacity);
                    Assert.AreEqual(expected.SecCeus, actual.Ceus);
                    Assert.AreEqual(expected.SecCourse, actual.CourseId);
                    Assert.AreEqual(expected.SecCredType, actual.CreditTypeCode);
                    Assert.AreEqual(expected.SecEndDate, actual.EndDate);
                    Assert.AreEqual(expected.SecFacultyConsentFlag.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false, actual.IsInstructorConsentRequired);
                    Assert.AreEqual(expected.SecFirstMeetingDate, actual.FirstMeetingDate);
                    Assert.AreEqual(expected.SecLastMeetingDate, actual.LastMeetingDate);
                    Assert.AreEqual(expected.SecLearningProvider, actual.LearningProvider);
                    Assert.AreEqual(expected.SecLocation, actual.Location);
                    Assert.AreEqual(expected.SecMaxCred, actual.MaximumCredits);
                    Assert.AreEqual(expected.SecMinCred, actual.MinimumCredits);
                    Assert.AreEqual(expected.SecName, actual.Name);
                    Assert.AreEqual(expected.SecNo, actual.Number);
                }
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionsAsync_LimitingKeysNull()
            {
                dataReaderMock.Setup(repo => repo.SelectAsync("COURSES", It.IsAny<string[]>(), "SAVING CRS.SECTIONS")).ReturnsAsync(new string[] { });
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>() { "" },
                    TotalCount = 0,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                        new KeyCacheInfo()
                        {
                            KeyCacheMax = 5905,
                            KeyCacheMin = 1,
                            KeyCachePart = "000",
                            KeyCacheSize = 5905
                        },
                        new KeyCacheInfo()
                        {
                            KeyCacheMax = 7625,
                            KeyCacheMin = 5906,
                            KeyCachePart = "001",
                            KeyCacheSize = 1720
                        }
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                var results = await sectionRepo.GetSectionsAsync(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "number", "learningProvider", "termId", "academicLevel", "course", "location",
                                                                       "status", "department", "", "");
                Assert.AreEqual(false, results.Item1.Any());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionsAsync_TermId()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>() { "" },
                    TotalCount = 0,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                var results = await sectionRepo.GetSectionsAsync(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "number", "learningProvider", "termId", "academicLevel", "", "location",
                                                                       "status", "department", "", "");
                Assert.AreEqual(false, results.Item1.Any());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionsAsync_Subject()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>() { "" },
                    TotalCount = 0,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                        new KeyCacheInfo()
                        {
                            KeyCacheMax = 5905,
                            KeyCacheMin = 1,
                            KeyCachePart = "000",
                            KeyCacheSize = 5905
                        },
                        new KeyCacheInfo()
                        {
                            KeyCacheMax = 7625,
                            KeyCacheMin = 5906,
                            KeyCachePart = "001",
                            KeyCacheSize = 1720
                        }
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                var results = await sectionRepo.GetSectionsAsync(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "number", "learningProvider", "", "academicLevel", "", "location",
                                                                       "status", "department", "subject", "");
                Assert.AreEqual(false, results.Item1.Any());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionsAsync_Number()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>() { "" },
                    TotalCount = 0,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                        new KeyCacheInfo()
                        {
                            KeyCacheMax = 5905,
                            KeyCacheMin = 1,
                            KeyCachePart = "000",
                            KeyCacheSize = 5905
                        },
                        new KeyCacheInfo()
                        {
                            KeyCacheMax = 7625,
                            KeyCacheMin = 5906,
                            KeyCachePart = "001",
                            KeyCacheSize = 1720
                        }
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                var results = await sectionRepo.GetSectionsAsync(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "number", "learningProvider", "", "academicLevel", "", "location",
                                                                       "status", "department", "", "");
                Assert.AreEqual(false, results.Item1.Any());
                Assert.AreEqual(0, results.Item2);

            }

            [TestMethod]
            public async Task SectionRepository_GetSectionsAsync_Instructor()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1" });

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>() { "" },
                    TotalCount = 0,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                        new KeyCacheInfo()
                        {
                            KeyCacheMax = 5905,
                            KeyCacheMin = 1,
                            KeyCachePart = "000",
                            KeyCacheSize = 5905
                        },
                        new KeyCacheInfo()
                        {
                            KeyCacheMax = 7625,
                            KeyCacheMin = 5906,
                            KeyCachePart = "001",
                            KeyCacheSize = 1720
                        }
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                var results = await sectionRepo.GetSectionsAsync(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "", "learningProvider", "", "academicLevel", "", "location",
                                                                       "status", "department", "", "instructor");
                Assert.AreEqual(false, results.Item1.Any());
                Assert.AreEqual(0, results.Item2);

            }

            #region For perf test new method

            [TestMethod]
            public async Task SectionRepository_GetSections2Async()
            {
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sublist, It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSES", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sublist);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                var regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Term>());
                var sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);


                var results = await sectionRepo.GetSections2Async(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "number", "learningProvider", "termId", "academicLevel", It.IsAny<List<string>>(), "location",
                                                                       "status", "department", It.IsAny<List<string>>(), "");
                Assert.IsNotNull(results);
                var actuals = results.Item1;

                for (int i = 0; i < actuals.Count(); i++)
                {
                    var actual = actuals.ToList()[i];
                    var expected = courseSectionCollection.ToList()[i];
                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.Recordkey, actual.Id);
                    Assert.AreEqual(expected.SecAcadLevel, actual.AcademicLevelCode);
                    Assert.AreEqual(expected.SecCapacity, actual.Capacity);
                    Assert.AreEqual(expected.SecCeus, actual.Ceus);
                    Assert.AreEqual(expected.SecCourse, actual.CourseId);
                    Assert.AreEqual(expected.SecCredType, actual.CreditTypeCode);
                    Assert.AreEqual(expected.SecEndDate, actual.EndDate);
                    Assert.AreEqual(expected.SecFacultyConsentFlag.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false, actual.IsInstructorConsentRequired);
                    Assert.AreEqual(expected.SecFirstMeetingDate, actual.FirstMeetingDate);
                    Assert.AreEqual(expected.SecLastMeetingDate, actual.LastMeetingDate);
                    Assert.AreEqual(expected.SecLearningProvider, actual.LearningProvider);
                    Assert.AreEqual(expected.SecLocation, actual.Location);
                    Assert.AreEqual(expected.SecMaxCred, actual.MaximumCredits);
                    Assert.AreEqual(expected.SecMinCred, actual.MinimumCredits);
                    Assert.AreEqual(expected.SecName, actual.Name);
                    Assert.AreEqual(expected.SecNo, actual.Number);
                }
            }

            [TestMethod]
            public async Task SectionRepository_GetSections2Async_LimitingKeysNull()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                dataReaderMock.Setup(repo => repo.SelectAsync("COURSES", It.IsAny<string[]>(), "SAVING CRS.SECTIONS")).ReturnsAsync(() => null);
                var results = await sectionRepo.GetSections2Async(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "number", "learningProvider", "termId", "academicLevel", new List<string>() { "course" }, "location",
                                                                       "location", "status", It.IsAny<List<string>>(), "subject", new List<string>() { "1", "2" }, "");
                Assert.AreEqual(false, results.Item1.Any());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionRepository_GetSections2Async_TermId()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(() => null);

                var results = await sectionRepo.GetSections2Async(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "number", "learningProvider", "termId", "academicLevel", new List<string>() { "course" }, "location",
                                                                       "status", "department", It.IsAny<List<string>>(), "");
                Assert.AreEqual(false, results.Item1.Any());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionRepository_GetSections2Async_Subject()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(() => null);

                var results = await sectionRepo.GetSections2Async(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "number", "learningProvider", "", "academicLevel", It.IsAny<List<string>>(), "location",
                                                                       "status", "department", new List<string>() { "subject" });
                Assert.AreEqual(false, results.Item1.Any());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionRepository_GetSections2Async_Number()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(() => null);

                var results = await sectionRepo.GetSections2Async(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "number", "learningProvider", "", "academicLevel", It.IsAny<List<string>>(), "location",
                                                                       "status", "department", It.IsAny<List<string>>(), "");
                Assert.AreEqual(false, results.Item1.Any());
                Assert.AreEqual(0, results.Item2);

            }

            [TestMethod]
            public async Task SectionRepository_GetSections2Async_Instructor()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllSections:",
                    Entity = "COURSE.SECTIONS",
                    Sublist = new List<string>(),
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                    {
                    }
                };
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1" });

                var results = await sectionRepo.GetSections2Async(0, 3, "Title", "2016/01/01", "2016/12/31", "code", "", "learningProvider", "", "academicLevel", It.IsAny<List<string>>(), "",
                                                                       "status", "department", null, "", It.IsAny<List<string>>(), "instructor");
                Assert.AreEqual(false, results.Item1.Any());
                Assert.AreEqual(0, results.Item2);

            }

            #endregion

            [TestMethod]
            public async Task SectionRepository_GetSectionGuidsCollectionAsync_ZeroResults()
            {
                string guid = Guid.NewGuid().ToString();
                Dictionary<string, RecordKeyLookupResult> dict = new Dictionary<string, RecordKeyLookupResult>();
                RecordKeyLookupResult rkResult = new RecordKeyLookupResult() { Guid = guid, ModelName = "Model" };
                dict.Add("abc", rkResult);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(dict);

                var results = await sectionRepo.GetSectionGuidsCollectionAsync(It.IsAny<List<string>>());
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionGuidsCollectionAsync()
            {
                string guid = Guid.NewGuid().ToString();
                Dictionary<string, RecordKeyLookupResult> dict = new Dictionary<string, RecordKeyLookupResult>();
                RecordKeyLookupResult rkResult = new RecordKeyLookupResult() { Guid = guid, ModelName = "Model" };
                dict.Add("Model+abc", rkResult);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(dict);

                var results = await sectionRepo.GetSectionGuidsCollectionAsync(new List<string>() { "1" });
                Assert.AreEqual(guid, results["abc"]);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueApiException))]
            public async Task SectionRepository_GetSectionGuidsCollectionAsync_Exception()
            {
                string guid = Guid.NewGuid().ToString();
                Dictionary<string, RecordKeyLookupResult> dict = new Dictionary<string, RecordKeyLookupResult>();
                RecordKeyLookupResult rkResult = new RecordKeyLookupResult() { Guid = guid, ModelName = "Model" };
                dict.Add("abc", rkResult);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(dict);

                var results = await sectionRepo.GetSectionGuidsCollectionAsync(new List<string>() { "1" });
            }

            [TestMethod]
            public async Task SectionRepository_PutSection2Async()
            {
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sublist, It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CourseSections>(cs.SecCourse, It.IsAny<bool>()))
                    .ReturnsAsync(cs);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                var regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Term>());
                var sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var updateResponse = new UpdateCourseSectionsResponse()
                {
                    SecGuid = cs.RecordGuid,
                    CourseSectionsId = cs.SecCourse,

                };
                transManagerMock.Setup(i => i.ExecuteAsync<UpdateCourseSectionsRequest, UpdateCourseSectionsResponse>(It.IsAny<UpdateCourseSectionsRequest>())).ReturnsAsync(updateResponse);

                var sectionStatusItem = new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now);
                var departments = new List<OfferingDepartment>()
                {
                    new OfferingDepartment(cs.SecDepartmentsEntityAssociation.FirstOrDefault().SecDeptsAssocMember ,100)
                };

                var section = new Section(cs.Recordkey, cs.SecCourse, cs.SecNo, Convert.ToDateTime(cs.SecStartDate),
                    cs.SecMinCred, cs.SecCeus, cs.SecShortTitle, cs.SecCredType, departments, cs.SecCourseLevels,
                    cs.SecAcadLevel, new List<SectionStatusItem>()
                    {
                        sectionStatusItem
                    });
                section.BillingMethod = "T";
                section.IsCrossListedSection = false;
                //end get section
                var result = await sectionRepo.PutSection2Async(section);
                Assert.IsNotNull(result);
                Assert.AreEqual(cs.SecCourse, result.CourseId);
                Assert.AreEqual(cs.RecordGuid, result.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_PutSection2Async_Error()
            {
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sublist, It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CourseSections>(cs.SecCourse, It.IsAny<bool>()))
                    .ReturnsAsync(cs);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                var regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Term>());
                var sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var updateResponse = new UpdateCourseSectionsResponse()
                {
                    UpdateCourseSectionErrors = new List<UpdateCourseSectionErrors>()
                    {
                        new UpdateCourseSectionErrors() {ErrorCodes = "01", ErrorMessages="Error" }
                    },
                    UpdateCourseSectionWarnings = new List<UpdateCourseSectionWarnings>()
                    {
                        new UpdateCourseSectionWarnings() {WarningCodes = "01", WarningMessages ="Warning, Warning" }
                    }
                };
                transManagerMock.Setup(i => i.ExecuteAsync<UpdateCourseSectionsRequest, UpdateCourseSectionsResponse>(It.IsAny<UpdateCourseSectionsRequest>())).ReturnsAsync(updateResponse);

                var sectionStatusItem = new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now);
                var departments = new List<OfferingDepartment>()
                {
                    new OfferingDepartment(cs.SecDepartmentsEntityAssociation.FirstOrDefault().SecDeptsAssocMember ,100)
                };

                var section = new Section(cs.Recordkey, cs.SecCourse, cs.SecNo, Convert.ToDateTime(cs.SecStartDate),
                    cs.SecMinCred, cs.SecCeus, cs.SecShortTitle, cs.SecCredType, departments, cs.SecCourseLevels,
                    cs.SecAcadLevel, new List<SectionStatusItem>()
                    {
                        sectionStatusItem
                    });
                section.BillingMethod = "T";
                section.IsCrossListedSection = false;
                await sectionRepo.PutSection2Async(section);
            }

            [TestMethod]
            public async Task SectionRepository_PostSection2Async()
            {
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sublist, It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CourseSections>(cs.SecCourse, It.IsAny<bool>()))
                    .ReturnsAsync(cs);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                var regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Term>());
                var sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var updateResponse = new UpdateCourseSectionsResponse()
                {
                    SecGuid = cs.RecordGuid,
                    CourseSectionsId = cs.SecCourse,

                };
                transManagerMock.Setup(i => i.ExecuteAsync<UpdateCourseSectionsRequest, UpdateCourseSectionsResponse>(It.IsAny<UpdateCourseSectionsRequest>())).ReturnsAsync(updateResponse);

                var sectionStatusItem = new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now);
                var departments = new List<OfferingDepartment>()
                {
                    new OfferingDepartment(cs.SecDepartmentsEntityAssociation.FirstOrDefault().SecDeptsAssocMember ,100)
                };

                var section = new Section(cs.Recordkey, cs.SecCourse, cs.SecNo, Convert.ToDateTime(cs.SecStartDate),
                    cs.SecMinCred, cs.SecCeus, cs.SecShortTitle, cs.SecCredType, departments, cs.SecCourseLevels,
                    cs.SecAcadLevel, new List<SectionStatusItem>()
                    {
                        sectionStatusItem
                    });
                section.BillingMethod = "T";
                section.IsCrossListedSection = false;

                var result = await sectionRepo.PostSection2Async(section);
                Assert.IsNotNull(result);
                Assert.AreEqual(cs.SecCourse, result.CourseId);
                Assert.AreEqual(cs.RecordGuid, result.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_PostSection2Async_Error()
            {
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sublist, It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CourseSections>(cs.SecCourse, It.IsAny<bool>()))
                    .ReturnsAsync(cs);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                var regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Term>());
                var sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var updateResponse = new UpdateCourseSectionsResponse()
                {
                    UpdateCourseSectionErrors = new List<UpdateCourseSectionErrors>()
                    {
                        new UpdateCourseSectionErrors() {ErrorCodes = "01", ErrorMessages="Error" }
                    },
                    UpdateCourseSectionWarnings = new List<UpdateCourseSectionWarnings>()
                    {
                        new UpdateCourseSectionWarnings() {WarningCodes = "01", WarningMessages ="Warning, Warning" }
                    }
                };
                transManagerMock.Setup(i => i.ExecuteAsync<UpdateCourseSectionsRequest, UpdateCourseSectionsResponse>(It.IsAny<UpdateCourseSectionsRequest>())).ReturnsAsync(updateResponse);

                var sectionStatusItem = new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now);
                var departments = new List<OfferingDepartment>()
                {
                    new OfferingDepartment(cs.SecDepartmentsEntityAssociation.FirstOrDefault().SecDeptsAssocMember ,100)
                };

                var section = new Section(cs.Recordkey, cs.SecCourse, cs.SecNo, Convert.ToDateTime(cs.SecStartDate),
                    cs.SecMinCred, cs.SecCeus, cs.SecShortTitle, cs.SecCredType, departments, cs.SecCourseLevels,
                    cs.SecAcadLevel, new List<SectionStatusItem>()
                    {
                        sectionStatusItem
                    });
                section.BillingMethod = "T";
                section.IsCrossListedSection = false;

                await sectionRepo.PostSection2Async(section);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionByGuidAsync()
            {
                string id = "23033dc3-06fc-4111-b910-77050b45cbe1";
                var guidLookUp = new GuidLookup[] { new GuidLookup(id) };
                GuidLookupResult guidLookupResult = new GuidLookupResult() { Entity = "COURSE.SECTIONS", PrimaryKey = csId };
                Dictionary<string, GuidLookupResult> guidLookupDict = new Dictionary<string, GuidLookupResult>();
                guidLookupDict.Add("COURSE.SECTIONS", guidLookupResult);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);

                var results = await sectionRepo.GetSectionByGuidAsync(id);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionByGuid2Async()
            {
                string id = "23033dc3-06fc-4111-b910-77050b45cbe1";
                var guidLookUp = new GuidLookup[] { new GuidLookup(id) };
                GuidLookupResult guidLookupResult = new GuidLookupResult() { Entity = "COURSE.SECTIONS", PrimaryKey = csId };
                Dictionary<string, GuidLookupResult> guidLookupDict = new Dictionary<string, GuidLookupResult>();
                guidLookupDict.Add("COURSE.SECTIONS", guidLookupResult);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);

                var results = await sectionRepo.GetSectionByGuid2Async(id);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionAsync_ArgumentNullException()
            {
                var results = await sectionRepo.GetSectionAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRepository_GetSectionAsync_CourseSection_Null_KeyNotFoundException()
            {
                var results = await sectionRepo.GetSectionAsync("BadKey");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRepository_GetSectionByGuidAsync_Dictionary_Null_KeyNotFoundException()
            {
                string id = "23033dc3-06fc-4111-b910-77050b45cbe1";
                var guidLookUp = new GuidLookup[] { new GuidLookup(id) };
                GuidLookupResult guidLookupResult = null;
                Dictionary<string, GuidLookupResult> guidLookupDict = new Dictionary<string, GuidLookupResult>();
                guidLookupDict.Add("COURSE.SECTIONS", guidLookupResult);
                dataReaderMock.Setup(dr => dr.SelectAsync(guidLookUp)).ReturnsAsync(guidLookupDict);

                var results = await sectionRepo.GetSectionByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRepository_GetSectionByGuidAsync_NullFoundEntry_KeyNotFoundException()
            {
                string id = "23033dc3-06fc-4111-b910-77050b45cbe1";
                var guidLookUp = new GuidLookup[] { new GuidLookup(id) };
                GuidLookupResult guidLookupResult = new GuidLookupResult() { Entity = "COURSE.SECTIONS", PrimaryKey = csId };
                Dictionary<string, GuidLookupResult> guidLookupDict = new Dictionary<string, GuidLookupResult>();
                guidLookupDict.Add("COURSE.SECTIONS", null);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);

                var results = await sectionRepo.GetSectionByGuidAsync(id);
            }

            private Collection<CourseSecFaculty> BuildSectionFacultyResponse(IEnumerable<Section> sections)
            {
                Collection<CourseSecFaculty> repoSecFaculty = new Collection<CourseSecFaculty>();
                int crsSecFId = 0;
                foreach (var section in sections)
                {
                    foreach (var fac in section.FacultyIds)
                    {
                        var crsSecF = new CourseSecFaculty();
                        //crsSecFId += 1;
                        crsSecF.Recordkey = section.Id;
                        crsSecF.CsfCourseSection = section.Id;
                        crsSecF.CsfFaculty = fac;
                        crsSecF.CsfInstrMethod = "ONL";
                        crsSecF.CsfStartDate = section.StartDate;
                        crsSecF.CsfEndDate = section.EndDate;
                        crsSecF.RecordGuid = Guid.NewGuid().ToString();
                        repoSecFaculty.Add(crsSecF);
                    }

                }
                return repoSecFaculty;
            }


            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task SectionRepository_PostSectionFacultyAsync_NullSection()
            //{
            //    var actual = await sectionRepo.PostSectionFacultyAsync(null, "92364642-4CF0-4640-B657-DC76CB7E289B");
            //}


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRepository_DeleteSectionFacultyAsync_NullGuid()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";
                var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

                await sectionRepo.DeleteSectionFacultyAsync(sectionFaculty, null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_DeleteSectionFacultyAsync_RepositoryException()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";

                var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

                var deleteSectionFacultyErrors = new DeleteSectionInstructorsErrors() { ErrorCodes = "1", ErrorMessages = "Error" };
                var deleteSectionFacultyWarnings = new DeleteSectionInstructorsWarnings { WarningCodes = "2", WarningMessages = "Warning" };

                var deleteResponse = new DeleteSectionInstructorsResponse()
                {

                    DeleteSectionInstructorsErrors = new List<DeleteSectionInstructorsErrors>() { deleteSectionFacultyErrors },
                    DeleteSectionInstructorsWarnings = new List<DeleteSectionInstructorsWarnings> { deleteSectionFacultyWarnings }
                };
                transManagerMock.Setup(i => i.ExecuteAsync<DeleteSectionInstructorsRequest, DeleteSectionInstructorsResponse>(It.IsAny<DeleteSectionInstructorsRequest>())).ReturnsAsync(deleteResponse);
                await sectionRepo.DeleteSectionFacultyAsync(sectionFaculty, guid);

            }

            [TestMethod]
            public async Task SectionRepository_DeleteSectionFacultyAsync()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";

                var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

                var deleteSectionFacultyErrors = new DeleteSectionInstructorsErrors() { ErrorCodes = "1", ErrorMessages = "Error" };
                var deleteSectionFacultyWarnings = new DeleteSectionInstructorsWarnings { WarningCodes = "2", WarningMessages = "Warning" };

                var deleteResponse = new DeleteSectionInstructorsResponse() { };

                transManagerMock.Setup(i => i.ExecuteAsync<DeleteSectionInstructorsRequest, DeleteSectionInstructorsResponse>(It.IsAny<DeleteSectionInstructorsRequest>())).ReturnsAsync(deleteResponse);
                await sectionRepo.DeleteSectionFacultyAsync(sectionFaculty, guid);
            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task SectionRepository_PostSectionFacultyAsync_NullGuid()
            //{
            //    var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";
            //    var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

            //    var actual = await sectionRepo.PostSectionFacultyAsync(sectionFaculty, null);
            //}

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_PostSectionFacultyAsync_RepositoryException()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";

                var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

                var updateSectionFacultyErrors = new UpdateSectionFacultyErrors() { ErrorCodes = "1", ErrorMessages = "Error" };
                var updateSectionFacultyWarnings = new UpdateSectionFacultyWarnings { WarningCodes = "2", WarningMessages = "Warning" };

                var updateResponse = new UpdateSectionFacultyResponse()
                {
                    CourseSecFacultyId = "12345",
                    CsfGuid = guid,
                    UpdateSectionFacultyWarnings = new List<UpdateSectionFacultyWarnings> { updateSectionFacultyWarnings },
                    UpdateSectionFacultyErrors = new List<UpdateSectionFacultyErrors>() { updateSectionFacultyErrors }

                };
                transManagerMock.Setup(i => i.ExecuteAsync<UpdateSectionFacultyRequest, UpdateSectionFacultyResponse>(It.IsAny<UpdateSectionFacultyRequest>())).ReturnsAsync(updateResponse);
                await sectionRepo.PostSectionFacultyAsync(sectionFaculty, guid);

            }

            [TestMethod]
            public async Task SectionRepository_PostSectionFacultyAsync()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);

                var sectionFacultyResponseData = new CourseSecFaculty()
                {
                    Recordkey = "1",
                    RecordGuid = guid,
                    CsfInstrMethod = "ONL",
                    CsfStartDate = new DateTime(2016, 9, 1),
                    CsfEndDate = new DateTime(2017, 9, 1),
                    CsfFaculty = "1",
                    CsfCourseSection = "12345"

                };

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CourseSecFaculty>(It.IsAny<GuidLookup>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);
                var courseSecMeetings = new Collection<CourseSecMeeting>();
                courseSecMeetings.Add(new CourseSecMeeting() { Recordkey = "1", CsmInstrMethod = "ONL", CsmFriday = "Y", CsmCourseSection = "12345", CsmFrequency = "W", CsmStartDate = new DateTime(2016, 9, 1), CsmEndDate = new DateTime(2017, 12, 12) });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSecMeetings);

                var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

                var updateResponse = new UpdateSectionFacultyResponse()
                {
                    CourseSecFacultyId = "12345",
                    CsfGuid = guid,

                };
                transManagerMock.Setup(i => i.ExecuteAsync<UpdateSectionFacultyRequest, UpdateSectionFacultyResponse>(It.IsAny<UpdateSectionFacultyRequest>())).ReturnsAsync(updateResponse);
                var actual = await sectionRepo.PostSectionFacultyAsync(sectionFaculty, guid);
                Assert.IsNotNull(actual);

                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual("1", actual.Id);
                Assert.AreEqual(new DateTime(2017, 9, 1), actual.EndDate);
                Assert.AreEqual("1", actual.FacultyId);
                Assert.AreEqual("ONL", actual.InstructionalMethodCode);
                Assert.AreEqual(0, actual.MeetingLoadFactor);
                Assert.AreEqual(true, actual.PrimaryIndicator);
                Assert.AreEqual(0, actual.ResponsibilityPercentage);
                Assert.AreEqual("12345", actual.SectionId);
                Assert.AreEqual(new DateTime(2016, 9, 1), actual.StartDate);

            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task SectionRepository_PutSectionFacultyAsync_NullSection()
            //{
            //    var actual = await sectionRepo.PutSectionFacultyAsync(null, "92364642-4CF0-4640-B657-DC76CB7E289B");
            //}


            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task SectionRepository_PutSectionFacultyAsync_NullGuid()
            //{
            //    var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";
            //    var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

            //    var actual = await sectionRepo.PutSectionFacultyAsync(sectionFaculty, null);
            //}

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_PutSectionFacultyAsync_RepositoryException()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";

                var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

                var updateSectionFacultyErrors = new UpdateSectionFacultyErrors() { ErrorCodes = "1", ErrorMessages = "Error" };
                var updateSectionFacultyWarnings = new UpdateSectionFacultyWarnings { WarningCodes = "2", WarningMessages = "Warning" };

                var updateResponse = new UpdateSectionFacultyResponse()
                {
                    CourseSecFacultyId = "12345",
                    CsfGuid = guid,
                    UpdateSectionFacultyWarnings = new List<UpdateSectionFacultyWarnings> { updateSectionFacultyWarnings },
                    UpdateSectionFacultyErrors = new List<UpdateSectionFacultyErrors>() { updateSectionFacultyErrors }

                };
                transManagerMock.Setup(i => i.ExecuteAsync<UpdateSectionFacultyRequest, UpdateSectionFacultyResponse>(It.IsAny<UpdateSectionFacultyRequest>())).ReturnsAsync(updateResponse);
                await sectionRepo.PutSectionFacultyAsync(sectionFaculty, guid);

            }
            [TestMethod]
            public async Task SectionRepository_PutSectionFacultyAsync()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);

                var sectionFacultyResponseData = new CourseSecFaculty()
                {
                    Recordkey = "1",
                    RecordGuid = guid,
                    CsfInstrMethod = "ONL",
                    CsfStartDate = new DateTime(2016, 9, 1),
                    CsfEndDate = new DateTime(2017, 9, 1),
                    CsfFaculty = "1",
                    CsfCourseSection = "12345"

                };

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CourseSecFaculty>(It.IsAny<GuidLookup>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);
                var courseSecMeetings = new Collection<CourseSecMeeting>();
                courseSecMeetings.Add(new CourseSecMeeting() { Recordkey = "1", CsmInstrMethod = "ONL", CsmFriday = "Y", CsmCourseSection = "12345", CsmFrequency = "W", CsmStartDate = new DateTime(2016, 9, 1), CsmEndDate = new DateTime(2017, 12, 12) });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSecMeetings);

                var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

                var updateResponse = new UpdateSectionFacultyResponse()
                {
                    CourseSecFacultyId = "12345",
                    CsfGuid = guid,

                };
                transManagerMock.Setup(i => i.ExecuteAsync<UpdateSectionFacultyRequest, UpdateSectionFacultyResponse>(It.IsAny<UpdateSectionFacultyRequest>())).ReturnsAsync(updateResponse);
                var actual = await sectionRepo.PutSectionFacultyAsync(sectionFaculty, guid);
                Assert.IsNotNull(actual);

                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual("1", actual.Id);
                Assert.AreEqual(new DateTime(2017, 9, 1), actual.EndDate);
                Assert.AreEqual("1", actual.FacultyId);
                Assert.AreEqual("ONL", actual.InstructionalMethodCode);
                Assert.AreEqual(0, actual.MeetingLoadFactor);
                Assert.AreEqual(true, actual.PrimaryIndicator);
                Assert.AreEqual(0, actual.ResponsibilityPercentage);
                Assert.AreEqual("12345", actual.SectionId);
                Assert.AreEqual(new DateTime(2016, 9, 1), actual.StartDate);

            }


            [TestMethod]
            public async Task SectionRepository_GetSectionFaculty()
            {

                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sublist);

                var sectionFacultyResponseData = new Collection<CourseSecFaculty>();
                sectionFacultyResponseData.Add(new CourseSecFaculty()
                {
                    Recordkey = "1",
                    RecordGuid = "92364642-4CF0-4640-B657-DC76CB7E289B",
                    CsfInstrMethod = "ONL",
                    CsfStartDate = new DateTime(2016, 9, 1),
                    CsfEndDate = new DateTime(2017, 9, 1),
                    CsfFaculty = "1",
                    CsfCourseSection = "12345",
                    CsfFacultyPct = 10

                });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var courseSecMeetings = new Collection<CourseSecMeeting>();
                courseSecMeetings.Add(new CourseSecMeeting() { Recordkey = "1", CsmInstrMethod = "ONL", CsmFriday = "Y", CsmCourseSection = "12345", CsmFrequency = "W", CsmStartDate = new DateTime(2016, 9, 1), CsmEndDate = new DateTime(2017, 12, 12) });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSecMeetings);

                var results = await sectionRepo.GetSectionFacultyAsync(0, 1, "", "", new List<string>());
                Assert.IsNotNull(results);
                var actuals = results.Item1;

                for (int i = 0; i < actuals.Count(); i++)
                {
                    var actual = actuals.ToList()[i];
                    var expected = courseSectionCollection.ToList()[i];
                    Assert.IsNotNull(actual);

                    Assert.AreEqual("92364642-4CF0-4640-B657-DC76CB7E289B", actual.Guid);
                    Assert.AreEqual("1", actual.Id);
                    Assert.AreEqual(new DateTime(2017, 9, 1), actual.EndDate);
                    Assert.AreEqual("1", actual.FacultyId);
                    Assert.AreEqual("ONL", actual.InstructionalMethodCode);
                    Assert.AreEqual(0, actual.MeetingLoadFactor);
                    Assert.AreEqual(true, actual.PrimaryIndicator);
                    Assert.AreEqual(10, actual.ResponsibilityPercentage);
                    Assert.AreEqual("12345", actual.SectionId);
                    Assert.AreEqual(new DateTime(2016, 9, 1), actual.StartDate);

                }
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionFacultyUsingSection()
            {

                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sublist);

                var sectionFacultyResponseData = new Collection<CourseSecFaculty>();
                sectionFacultyResponseData.Add(new CourseSecFaculty()
                {
                    Recordkey = "1",
                    RecordGuid = "92364642-4CF0-4640-B657-DC76CB7E289B",
                    CsfInstrMethod = "ONL",
                    CsfStartDate = new DateTime(2016, 9, 1),
                    CsfEndDate = new DateTime(2017, 9, 1),
                    CsfFaculty = "1",
                    CsfCourseSection = "12345",
                    CsfFacultyPct = 10

                });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var courseSecMeetings = new Collection<CourseSecMeeting>();
                courseSecMeetings.Add(new CourseSecMeeting() { Recordkey = "1", CsmInstrMethod = "ONL", CsmFriday = "Y", CsmCourseSection = "12345", CsmFrequency = "W", CsmStartDate = new DateTime(2016, 9, 1), CsmEndDate = new DateTime(2017, 12, 12) });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSecMeetings);

                var results = await sectionRepo.GetSectionFacultyAsync(0, 1, "12345", "", new List<string>());
                Assert.IsNotNull(results);
                var actuals = results.Item1;

                for (int i = 0; i < actuals.Count(); i++)
                {
                    var actual = actuals.ToList()[i];
                    var expected = courseSectionCollection.ToList()[i];
                    Assert.IsNotNull(actual);

                    Assert.AreEqual("92364642-4CF0-4640-B657-DC76CB7E289B", actual.Guid);
                    Assert.AreEqual("1", actual.Id);
                    Assert.AreEqual(new DateTime(2017, 9, 1), actual.EndDate);
                    Assert.AreEqual("1", actual.FacultyId);
                    Assert.AreEqual("ONL", actual.InstructionalMethodCode);
                    Assert.AreEqual(0, actual.MeetingLoadFactor);
                    Assert.AreEqual(true, actual.PrimaryIndicator);
                    Assert.AreEqual(10, actual.ResponsibilityPercentage);
                    Assert.AreEqual("12345", actual.SectionId);
                    Assert.AreEqual(new DateTime(2016, 9, 1), actual.StartDate);

                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionFacultyByGuidAsync_Null()
            {
                var actual = await sectionRepo.GetSectionFacultyByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionFacultyByGuidAsync_Empty()
            {
                var actual = await sectionRepo.GetSectionFacultyByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionFacultyByGuidAsync_CourseSecFaculty_Null()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CourseSecFaculty>(It.IsAny<GuidLookup>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                var actual = await sectionRepo.GetSectionFacultyByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionFacultyByGuidAsync_Invalid()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);

                var sectionFacultyResponseData = new CourseSecFaculty()
                {
                    Recordkey = "1",
                    RecordGuid = guid,
                    CsfInstrMethod = "ONL",
                    CsfStartDate = new DateTime(2016, 9, 1),
                    CsfEndDate = new DateTime(2017, 9, 1),
                    CsfFaculty = "1",
                    CsfCourseSection = "12345"

                };
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CourseSecFaculty>(It.IsAny<GuidLookup>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                await sectionRepo.GetSectionFacultyByGuidAsync(guid);

            }

            [TestMethod]
            public async Task SectionRepository_GetSectionFacultyByGuidAsync()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);

                var sectionFacultyResponseData = new CourseSecFaculty()
                {
                    Recordkey = "1",
                    RecordGuid = guid,
                    CsfInstrMethod = "ONL",
                    CsfStartDate = new DateTime(2016, 9, 1),
                    CsfEndDate = new DateTime(2017, 9, 1),
                    CsfFaculty = "1",
                    CsfCourseSection = "12345"

                };

                // dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);
                //faculty =  await DataReader.ReadRecordAsync<CourseSecFaculty>(new GuidLookup(guid, null));
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CourseSecFaculty>(It.IsAny<GuidLookup>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);


                var courseSecMeetings = new Collection<CourseSecMeeting>();
                courseSecMeetings.Add(new CourseSecMeeting() { Recordkey = "1", CsmInstrMethod = "ONL", CsmFriday = "Y", CsmCourseSection = "12345", CsmFrequency = "W", CsmStartDate = new DateTime(2016, 9, 1), CsmEndDate = new DateTime(2017, 12, 12) });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSecMeetings);

                var actual = await sectionRepo.GetSectionFacultyByGuidAsync(guid);
                var expected = courseSectionCollection.FirstOrDefault(x => x.RecordGuid == guid);
                Assert.IsNotNull(actual);

                Assert.AreEqual("92364642-4CF0-4640-B657-DC76CB7E289B", actual.Guid);
                Assert.AreEqual("1", actual.Id);
                Assert.AreEqual(new DateTime(2017, 9, 1), actual.EndDate);
                Assert.AreEqual("1", actual.FacultyId);
                Assert.AreEqual("ONL", actual.InstructionalMethodCode);
                Assert.AreEqual(0, actual.MeetingLoadFactor);
                Assert.AreEqual(true, actual.PrimaryIndicator);
                Assert.AreEqual(0, actual.ResponsibilityPercentage);
                Assert.AreEqual("12345", actual.SectionId);
                Assert.AreEqual(new DateTime(2016, 9, 1), actual.StartDate);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionFaculty_EmptyLimitOffset()
            {

                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);

                var sectionFacultyResponseData = new Collection<CourseSecFaculty>();
                sectionFacultyResponseData.Add(new CourseSecFaculty()
                {
                    Recordkey = "1",
                    RecordGuid = "92364642-4CF0-4640-B657-DC76CB7E289B",
                    CsfInstrMethod = "ONL",
                    CsfStartDate = new DateTime(2016, 9, 1),
                    CsfEndDate = new DateTime(2017, 9, 1),
                    CsfFaculty = "1",
                    CsfCourseSection = "12345"

                });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);
                var courseSecMeetings = new Collection<CourseSecMeeting>();
                courseSecMeetings.Add(new CourseSecMeeting() { Recordkey = "1", CsmInstrMethod = "ONL", CsmFriday = "Y", CsmCourseSection = "12345", CsmFrequency = "W", CsmStartDate = new DateTime(2016, 9, 1), CsmEndDate = new DateTime(2017, 12, 12) });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSecMeetings);

                var results = await sectionRepo.GetSectionFacultyAsync(0, 0, "", "", new List<string>());
                Assert.IsNotNull(results);
                var actuals = results.Item1;

                for (int i = 0; i < actuals.Count(); i++)
                {
                    var actual = actuals.ToList()[i];
                    var expected = courseSectionCollection.ToList()[i];
                    Assert.IsNotNull(actual);

                    Assert.AreEqual("92364642-4CF0-4640-B657-DC76CB7E289B", actual.Guid);
                    Assert.AreEqual("1", actual.Id);
                    Assert.AreEqual(new DateTime(2017, 9, 1), actual.EndDate);
                    Assert.AreEqual("1", actual.FacultyId);
                    Assert.AreEqual("ONL", actual.InstructionalMethodCode);
                    Assert.AreEqual(0, actual.MeetingLoadFactor);
                    Assert.AreEqual(true, actual.PrimaryIndicator);
                    Assert.AreEqual(0, actual.ResponsibilityPercentage);
                    Assert.AreEqual("12345", actual.SectionId);
                    Assert.AreEqual(new DateTime(2016, 9, 1), actual.StartDate);

                }
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionMeetingIdFromGuid_GuidLookupSuccess()
            {

                // Set up for GUID lookups
                var id = "12345";
                var id2 = "9876";
                var id3 = "0012345";

                var guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                var guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                var guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                var guidLookup = new GuidLookup(guid);
                var guidLookupResult = new GuidLookupResult() { Entity = "COURSE.SEC.MEETING", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                var recordLookup = new RecordKeyLookup("COURSE.SEC.MEETING", id, false);
                var recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    if (gla.Any(gl => gl.Guid == guid2))
                    {
                        guidLookupDict.Add(guid2, null);
                    }
                    if (gla.Any(gl => gl.Guid == guid3))
                    {
                        guidLookupDict.Add(guid3, new GuidLookupResult() { Entity = "COURSE.SEC.MEETING", PrimaryKey = id3 });
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var result = await sectionRepo.GetSectionMeetingIdFromGuidAsync(guid);
                Assert.AreEqual(id, result);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionFacultyIdFromGuid_GuidLookupSuccess()
            {

                // Set up for GUID lookups
                var id = "12345";
                var id2 = "9876";
                var id3 = "0012345";

                var guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                var guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                var guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                var guidLookup = new GuidLookup(guid);
                var guidLookupResult = new GuidLookupResult() { Entity = "COURSE.SEC.FACULTY", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                var recordLookup = new RecordKeyLookup("COURSE.SEC.FACULTY", id, false);
                var recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    if (gla.Any(gl => gl.Guid == guid2))
                    {
                        guidLookupDict.Add(guid2, null);
                    }
                    if (gla.Any(gl => gl.Guid == guid3))
                    {
                        guidLookupDict.Add(guid3, new GuidLookupResult() { Entity = "COURSE.SEC.FACULTY", PrimaryKey = id3 });
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var result = await sectionRepo.GetSectionFacultyIdFromGuidAsync(guid);
                Assert.AreEqual(id, result);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionFaculty_withFilters()
            {

                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);

                var sectionFacultyResponseData = new Collection<CourseSecFaculty>();
                sectionFacultyResponseData.Add(new CourseSecFaculty()
                {
                    Recordkey = "1",
                    RecordGuid = "92364642-4CF0-4640-B657-DC76CB7E289B",
                    CsfInstrMethod = "ONL",
                    CsfStartDate = new DateTime(2016, 9, 1),
                    CsfEndDate = new DateTime(2017, 9, 1),
                    CsfFaculty = "1",
                    CsfCourseSection = "12345"

                });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var courseSecMeetings = new Collection<CourseSecMeeting>();
                courseSecMeetings.Add(new CourseSecMeeting() { Recordkey = "1", CsmInstrMethod = "ONL", CsmFriday = "Y", CsmCourseSection = "12345", CsmFrequency = "W", CsmStartDate = new DateTime(2016, 9, 1), CsmEndDate = new DateTime(2017, 12, 12) });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSecMeetings);

                var results = await sectionRepo.GetSectionFacultyAsync(0, 1, "12345", "1", new List<string>() { "1" });
                Assert.IsNotNull(results);
                var actuals = results.Item1;

                for (int i = 0; i < actuals.Count(); i++)
                {
                    var actual = actuals.ToList()[i];
                    var expected = courseSectionCollection.ToList()[i];
                    Assert.IsNotNull(actual);

                    Assert.AreEqual("92364642-4CF0-4640-B657-DC76CB7E289B", actual.Guid);
                    Assert.AreEqual("1", actual.Id);
                    Assert.AreEqual(new DateTime(2017, 9, 1), actual.EndDate);
                    Assert.AreEqual("1", actual.FacultyId);
                    Assert.AreEqual("ONL", actual.InstructionalMethodCode);
                    Assert.AreEqual(0, actual.MeetingLoadFactor);
                    Assert.AreEqual(true, actual.PrimaryIndicator);
                    Assert.AreEqual(0, actual.ResponsibilityPercentage);
                    Assert.AreEqual("12345", actual.SectionId);
                    Assert.AreEqual(new DateTime(2016, 9, 1), actual.StartDate);

                }
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionsSearchableAsync_Yes()
            {
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sublist, It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                var regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Term>());
                var sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var allCourseTypes = new TestCourseTypeRepository().Get().ToList();
                var courseTypeValcodeResponse = BuildValcodeResponse(allCourseTypes);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", It.IsAny<bool>())).ReturnsAsync(courseTypeValcodeResponse);

                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var contractTypes = allCourseTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "COURSE.TYPES", contractTypes.Code }),
                            new RecordKeyLookupResult() { Guid = contractTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                string fileName = "CORE.PARMS";
                string field = "LDM.DEFAULTS";
                var ldmDefaults = new LdmDefaults() { LdmdRegUsersId = "1" };
                dataReaderMock.Setup(i => i.ReadRecordAsync<LdmDefaults>(fileName, field, It.IsAny<bool>())).ReturnsAsync(ldmDefaults);

                var regUser = new RegUsers();
                regUser.Recordkey = "REGUSERID";
                regUser.RguRegControls = new List<string>() { "REGCTLID", "OTHER" };
                dataReaderMock.Setup<Task<RegUsers>>(cacc => cacc.ReadRecordAsync<RegUsers>("REG.USERS", "REGUSERID", false)).ReturnsAsync(regUser);
                var regCtl = new RegControls();
                regCtl.Recordkey = "REGCTLID";
                regCtl.RgcSectionLookupCriteria = new List<string>() { "WITH CRS.EXTERNAL.SOURCE=''", "AND WITH SEC.COURSE.TYPES NE 'PSE'" };
                dataReaderMock.Setup(cacc => cacc.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true)).ReturnsAsync(new Collection<RegControls>() { regCtl });

                var results = await sectionRepo.GetSectionsSearchableAsync(0, 3, "yes");

                Assert.IsNotNull(results);
                var actuals = results.Item1;

                for (int i = 0; i < actuals.Count(); i++)
                {
                    var actual = actuals.ToList()[i];
                    var expected = courseSectionCollection.ToList()[i];
                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.Recordkey, actual.Id);
                    Assert.AreEqual(expected.SecAcadLevel, actual.AcademicLevelCode);
                    Assert.AreEqual(expected.SecCapacity, actual.Capacity);
                    Assert.AreEqual(expected.SecCeus, actual.Ceus);
                    Assert.AreEqual(expected.SecCourse, actual.CourseId);
                    Assert.AreEqual(expected.SecCredType, actual.CreditTypeCode);
                    Assert.AreEqual(expected.SecEndDate, actual.EndDate);
                    Assert.AreEqual(expected.SecFacultyConsentFlag.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false, actual.IsInstructorConsentRequired);
                    Assert.AreEqual(expected.SecFirstMeetingDate, actual.FirstMeetingDate);
                    Assert.AreEqual(expected.SecLastMeetingDate, actual.LastMeetingDate);
                    Assert.AreEqual(expected.SecLearningProvider, actual.LearningProvider);
                    Assert.AreEqual(expected.SecLocation, actual.Location);
                    Assert.AreEqual(expected.SecMaxCred, actual.MaximumCredits);
                    Assert.AreEqual(expected.SecMinCred, actual.MinimumCredits);
                    Assert.AreEqual(expected.SecName, actual.Name);
                    Assert.AreEqual(expected.SecNo, actual.Number);
                }
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionsSearchable1Async_Yes()
            {
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sublist, It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                var regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Term>());
                var sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var allCourseTypes = new TestCourseTypeRepository().Get().ToList();
                var courseTypeValcodeResponse = BuildValcodeResponse(allCourseTypes);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", It.IsAny<bool>())).ReturnsAsync(courseTypeValcodeResponse);

                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var contractTypes = allCourseTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "COURSE.TYPES", contractTypes.Code }),
                            new RecordKeyLookupResult() { Guid = contractTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                string fileName = "CORE.PARMS";
                string field = "LDM.DEFAULTS";
                var ldmDefaults = new LdmDefaults() { LdmdRegUsersId = "1" };
                dataReaderMock.Setup(i => i.ReadRecordAsync<LdmDefaults>(fileName, field, It.IsAny<bool>())).ReturnsAsync(ldmDefaults);

                var regUser = new RegUsers();
                regUser.Recordkey = "REGUSERID";
                regUser.RguRegControls = new List<string>() { "REGCTLID", "OTHER" };
                dataReaderMock.Setup<Task<RegUsers>>(cacc => cacc.ReadRecordAsync<RegUsers>("REG.USERS", "REGUSERID", false)).ReturnsAsync(regUser);
                var regCtl = new RegControls();
                regCtl.Recordkey = "REGCTLID";
                regCtl.RgcSectionLookupCriteria = new List<string>() { "WITH CRS.EXTERNAL.SOURCE=''", "AND WITH SEC.COURSE.TYPES NE 'PSE'" };
                dataReaderMock.Setup(cacc => cacc.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true)).ReturnsAsync(new Collection<RegControls>() { regCtl });

                var results = await sectionRepo.GetSectionsSearchable1Async(0, 3, "yes");

                Assert.IsNotNull(results);
                var actuals = results.Item1;

                for (int i = 0; i < actuals.Count(); i++)
                {
                    var actual = actuals.ToList()[i];
                    var expected = courseSectionCollection.ToList()[i];
                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.Recordkey, actual.Id);
                    Assert.AreEqual(expected.SecAcadLevel, actual.AcademicLevelCode);
                    Assert.AreEqual(expected.SecCapacity, actual.Capacity);
                    Assert.AreEqual(expected.SecCeus, actual.Ceus);
                    Assert.AreEqual(expected.SecCourse, actual.CourseId);
                    Assert.AreEqual(expected.SecCredType, actual.CreditTypeCode);
                    Assert.AreEqual(expected.SecEndDate, actual.EndDate);
                    Assert.AreEqual(expected.SecFacultyConsentFlag.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false, actual.IsInstructorConsentRequired);
                    Assert.AreEqual(expected.SecFirstMeetingDate, actual.FirstMeetingDate);
                    Assert.AreEqual(expected.SecLastMeetingDate, actual.LastMeetingDate);
                    Assert.AreEqual(expected.SecLearningProvider, actual.LearningProvider);
                    Assert.AreEqual(expected.SecLocation, actual.Location);
                    Assert.AreEqual(expected.SecMaxCred, actual.MaximumCredits);
                    Assert.AreEqual(expected.SecMinCred, actual.MinimumCredits);
                    Assert.AreEqual(expected.SecName, actual.Name);
                    Assert.AreEqual(expected.SecNo, actual.Number);
                }
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionsSearchableAsync_Hidden()
            {
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sublist, It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                var regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Term>());
                var sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var allCourseTypes = new TestCourseTypeRepository().Get().ToList();
                var courseTypeValcodeResponse = BuildValcodeResponse(allCourseTypes);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", It.IsAny<bool>())).ReturnsAsync(courseTypeValcodeResponse);

                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var contractTypes = allCourseTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "COURSE.TYPES", contractTypes.Code }),
                            new RecordKeyLookupResult() { Guid = contractTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                string fileName = "CORE.PARMS";
                string field = "LDM.DEFAULTS";
                var ldmDefaults = new LdmDefaults() { LdmdRegUsersId = "1" };
                dataReaderMock.Setup(i => i.ReadRecordAsync<LdmDefaults>(fileName, field, It.IsAny<bool>())).ReturnsAsync(ldmDefaults);

                var regUser = new RegUsers();
                regUser.Recordkey = "REGUSERID";
                regUser.RguRegControls = new List<string>() { "REGCTLID", "OTHER" };
                dataReaderMock.Setup<Task<RegUsers>>(cacc => cacc.ReadRecordAsync<RegUsers>("REG.USERS", "REGUSERID", false)).ReturnsAsync(regUser);
                var regCtl = new RegControls();
                regCtl.Recordkey = "REGCTLID";
                regCtl.RgcSectionLookupCriteria = new List<string>() { "WITH CRS.EXTERNAL.SOURCE=''", "AND WITH SEC.COURSE.TYPES NE 'PSE'" };
                dataReaderMock.Setup(cacc => cacc.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true)).ReturnsAsync(new Collection<RegControls>() { regCtl });

                var results = await sectionRepo.GetSectionsSearchableAsync(0, 3, "hidden");

                Assert.IsNotNull(results);
                var actuals = results.Item1;

                for (int i = 0; i < actuals.Count(); i++)
                {
                    var actual = actuals.ToList()[i];
                    var expected = courseSectionCollection.ToList()[i];
                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.Recordkey, actual.Id);
                    Assert.AreEqual(expected.SecAcadLevel, actual.AcademicLevelCode);
                    Assert.AreEqual(expected.SecCapacity, actual.Capacity);
                    Assert.AreEqual(expected.SecCeus, actual.Ceus);
                    Assert.AreEqual(expected.SecCourse, actual.CourseId);
                    Assert.AreEqual(expected.SecCredType, actual.CreditTypeCode);
                    Assert.AreEqual(expected.SecEndDate, actual.EndDate);
                    Assert.AreEqual(expected.SecFacultyConsentFlag.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false, actual.IsInstructorConsentRequired);
                    Assert.AreEqual(expected.SecFirstMeetingDate, actual.FirstMeetingDate);
                    Assert.AreEqual(expected.SecLastMeetingDate, actual.LastMeetingDate);
                    Assert.AreEqual(expected.SecLearningProvider, actual.LearningProvider);
                    Assert.AreEqual(expected.SecLocation, actual.Location);
                    Assert.AreEqual(expected.SecMaxCred, actual.MaximumCredits);
                    Assert.AreEqual(expected.SecMinCred, actual.MinimumCredits);
                    Assert.AreEqual(expected.SecName, actual.Name);
                    Assert.AreEqual(expected.SecNo, actual.Number);
                }
            }


            [TestMethod]
            public async Task SectionRepository_GetSectionsSearchableAsync_No()
            {
                string[] sublist = new string[] { "1" };
                courseSectionCollection.Add(cs);
                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SECTIONS", It.IsAny<string>())).ReturnsAsync(sublist);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sublist, It.IsAny<bool>())).ReturnsAsync(courseSectionCollection);

                dataReaderMock.Setup(dr => dr.SelectAsync("COURSE.SEC.FACULTY", It.IsAny<string>())).ReturnsAsync(sublist);
                var regSections = await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Term>());
                var sectionFacultyResponseData = BuildSectionFacultyResponse(regSections);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(sectionFacultyResponseData);

                var allCourseTypes = new TestCourseTypeRepository().Get().ToList();
                var courseTypeValcodeResponse = BuildValcodeResponse(allCourseTypes);
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", It.IsAny<bool>())).ReturnsAsync(courseTypeValcodeResponse);

                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var contractTypes = allCourseTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "COURSE.TYPES", contractTypes.Code }),
                            new RecordKeyLookupResult() { Guid = contractTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                string fileName = "CORE.PARMS";
                string field = "LDM.DEFAULTS";
                var ldmDefaults = new LdmDefaults() { LdmdRegUsersId = "1" };
                dataReaderMock.Setup(i => i.ReadRecordAsync<LdmDefaults>(fileName, field, It.IsAny<bool>())).ReturnsAsync(ldmDefaults);

                var regUser = new RegUsers();
                regUser.Recordkey = "REGUSERID";
                regUser.RguRegControls = new List<string>() { "REGCTLID", "OTHER" };
                dataReaderMock.Setup<Task<RegUsers>>(cacc => cacc.ReadRecordAsync<RegUsers>("REG.USERS", "REGUSERID", false)).ReturnsAsync(regUser);
                var regCtl = new RegControls();
                regCtl.Recordkey = "REGCTLID";
                regCtl.RgcSectionLookupCriteria = new List<string>() { "WITH CRS.EXTERNAL.SOURCE=''", "AND WITH SEC.COURSE.TYPES NE 'PSE'" };
                dataReaderMock.Setup(cacc => cacc.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true)).ReturnsAsync(new Collection<RegControls>() { regCtl });

                var results = await sectionRepo.GetSectionsSearchableAsync(0, 3, "no");

                Assert.IsNotNull(results);
                var actuals = results.Item1;

                for (int i = 0; i < actuals.Count(); i++)
                {
                    var actual = actuals.ToList()[i];
                    var expected = courseSectionCollection.ToList()[i];
                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.Recordkey, actual.Id);
                    Assert.AreEqual(expected.SecAcadLevel, actual.AcademicLevelCode);
                    Assert.AreEqual(expected.SecCapacity, actual.Capacity);
                    Assert.AreEqual(expected.SecCeus, actual.Ceus);
                    Assert.AreEqual(expected.SecCourse, actual.CourseId);
                    Assert.AreEqual(expected.SecCredType, actual.CreditTypeCode);
                    Assert.AreEqual(expected.SecEndDate, actual.EndDate);
                    Assert.AreEqual(expected.SecFacultyConsentFlag.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false, actual.IsInstructorConsentRequired);
                    Assert.AreEqual(expected.SecFirstMeetingDate, actual.FirstMeetingDate);
                    Assert.AreEqual(expected.SecLastMeetingDate, actual.LastMeetingDate);
                    Assert.AreEqual(expected.SecLearningProvider, actual.LearningProvider);
                    Assert.AreEqual(expected.SecLocation, actual.Location);
                    Assert.AreEqual(expected.SecMaxCred, actual.MaximumCredits);
                    Assert.AreEqual(expected.SecMinCred, actual.MinimumCredits);
                    Assert.AreEqual(expected.SecName, actual.Name);
                    Assert.AreEqual(expected.SecNo, actual.Number);
                }
            }

        }

        [TestClass]
        public class SectionRepository_UpdateSectionBookAsyncTests : SectionRepositoryTests
        {
            string secGuid;
            Section section;
            DateTime secStartDate, secEndDate;
            List<OfferingDepartment> depts = new List<OfferingDepartment>();
            List<SectionStatusItem> statuses = new List<SectionStatusItem>();
            List<SectionFaculty> faculty = new List<SectionFaculty>();
            Book newBook;
            SectionTextbook textbook;

            UpdateSectionTextbooksRequest request;
            UpdateSectionTextbooksResponse response;

            SectionRepository sectionRepo;
            CourseSections cs;
            CourseSecMeeting csm;
            CourseSecFaculty csf;
            CdDefaults cdDefaults;
            string csId;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();

                secGuid = Guid.NewGuid().ToString();
                secStartDate = new DateTime(2014, 9, 2);
                secEndDate = new DateTime(2014, 12, 5);
                depts.Add(new OfferingDepartment("RECR", 75m));
                var courseLevelCodes = new List<string>() { "100", "CE" };
                statuses.Add(new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2011, 9, 28)));
                section = new Section("1", "1", "01", secStartDate, 3.00m, null, "Underwater Basketweaving", "IN", depts, courseLevelCodes, "CE", statuses) { Guid = secGuid };


                request = new UpdateSectionTextbooksRequest();
                response = new UpdateSectionTextbooksResponse()
                {
                    OutErrorMsgs = new List<string>(),
                    OutWarningMsgs = new List<string>()
                };

                newBook = new Book("Book1", "Isbn", "Title", "Author", "Publisher", "Copyright", "Edition", true, 10m, 20m, "Comment", "External Comments", "altId1", "altId2", "altId3");


                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateSectionTextbooksRequest, UpdateSectionTextbooksResponse>(It.IsAny<UpdateSectionTextbooksRequest>())).ReturnsAsync(response);

                csId = "12345";

                cs = new CourseSections()
                {
                    RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                    Recordkey = csId,
                    RecordModelName = "sections",
                    SecAcadLevel = "UG",
                    SecActiveStudents = new List<string>(),
                    SecAllowAuditFlag = "N",
                    SecAllowPassNopassFlag = "N",
                    SecAllowWaitlistFlag = "Y",
                    SecBookOptions = new List<string>() { "R", "O", "C" },
                    SecBooks = new List<string>() { "Book 1", "Book 2", "Book 3" },
                    SecCapacity = 30,
                    SecCeus = null,
                    SecCloseWaitlistFlag = "Y",
                    SecCourse = "210",
                    SecCourseLevels = new List<string>() { "100" },
                    SecCourseTypes = new List<string>() { "STND", "HONOR" },
                    SecCredType = "IN",
                    SecEndDate = new DateTime(2014, 12, 15),
                    SecFaculty = new List<string>(),
                    SecFacultyConsentFlag = "Y",
                    SecGradeScheme = "UGR",
                    SecInstrMethods = new List<string>() { "LEC", "LAB" },
                    SecLocation = "MAIN",
                    SecMaxCred = 6m,
                    SecMeeting = new List<string>(),
                    SecMinCred = 3m,
                    SecName = "MATH-4350-01",
                    SecNo = "01",
                    SecNoWeeks = 10,
                    SecOnlyPassNopassFlag = "N",
                    SecPortalSite = csId,
                    SecShortTitle = "Statistics",
                    SecStartDate = DateTime.Today.AddDays(-10),
                    SecTerm = "2014/FA",
                    SecTopicCode = "ABC",
                    SecVarCredIncrement = 1m,
                    SecWaitlistMax = 10,
                    SecWaitlistRating = "SR",
                    SecXlist = null,
                    SecHideInCatalog = "Y",
                    SecSynonym = "992211"
                };
                cs.SecEndDate = cs.SecStartDate.Value.AddDays(69);
                cs.SecContactEntityAssociation = new List<CourseSectionsSecContact>();
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LEC", 20.00m, 45.00m, "T", 37.50m));
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LAB", 10.00m, 15.00m, "T", 45.00m));
                cs.SecDepartmentsEntityAssociation = new List<CourseSectionsSecDepartments>();
                cs.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("MATH", 75m));
                cs.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("PSYC", 25m));
                cs.SecStatusesEntityAssociation = new List<CourseSectionsSecStatuses>();
                cs.SecStatusesEntityAssociation.Add(new CourseSectionsSecStatuses(new DateTime(2001, 5, 15), "A"));
                // Instr methods association - instructional method and load
                cs.SecContactEntityAssociation = new List<CourseSectionsSecContact>();
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LEC", 20.00m, 0m, "", 0m));
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LAB", 10.00m, 0m, "", 0m));
                // Pointer to CourseSecFaculty
                cs.SecFaculty.Add("1");
                // Pointer to CourseSecMeeting
                cs.SecMeeting.Add("1");

                BuildLdmConfiguration(dataReaderMock, out cdDefaults);

                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);

                // Set up repo response for course.sec.meeting
                csm = new CourseSecMeeting()
                {
                    Recordkey = "1",
                    CsmInstrMethod = "LEC",
                    CsmCourseSection = "12345",
                    CsmStartDate = DateTime.Today,
                    CsmEndDate = DateTime.Today.AddDays(27),
                    CsmStartTime = (new DateTime(1, 1, 1, 10, 0, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmEndTime = (new DateTime(1, 1, 1, 11, 20, 0) as DateTime?).ToTimeOfDayDateTimeOffset(colleagueTimeZone).ToLocalDateTime(colleagueTimeZone),
                    CsmMonday = "Y"
                };
                MockRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", csm);

                // Set up repo response for course.sec.faculty
                csf = new CourseSecFaculty()
                {
                    Recordkey = "1",
                    CsfInstrMethod = "LEC",
                    CsfCourseSection = "12345",
                    CsfFaculty = "FAC1",
                    CsfFacultyPct = 100m,
                    CsfStartDate = cs.SecStartDate,
                    CsfEndDate = cs.SecEndDate,
                };


                dataReaderMock.Setup(rdr => rdr.ReadRecordAsync<CourseSections>(section.Id, It.IsAny<bool>())).ReturnsAsync(cs);

                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

            }


            [TestCleanup]
            public void CleanUp()
            {
                section = null;
                depts = null;
                statuses = null;
                faculty = null;
                request = null;
                response = null;
                sectionRepo = null;
                cs = null;
                csm = null;
                csf = null;
                cdDefaults = null;
                csId = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_UpdateSectionBookAsync_EmptySectionId()
            {
                textbook = new SectionTextbook(newBook, string.Empty, "R", SectionBookAction.Update);

                var result = await sectionRepo.UpdateSectionBookAsync(textbook);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_UpdateSectionBookAsync_NullSectionId()
            {
                textbook = new SectionTextbook(newBook, null, "R", SectionBookAction.Add);

                var result = await sectionRepo.UpdateSectionBookAsync(textbook);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_UpdateSectionBookAsync_NullTextbook()
            {
                textbook = null;

                var result = await sectionRepo.UpdateSectionBookAsync(textbook);
            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task SectionRepository_PostSectionFacultyAsync_NullGuid()
            //{
            //    var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";
            //    var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

            //    var actual = await sectionRepo.PostSectionFacultyAsync(sectionFaculty, null);
            //}

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_PostSectionFacultyAsync_RepositoryException()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";

                var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

                var updateSectionFacultyErrors = new UpdateSectionFacultyErrors() { ErrorCodes = "1", ErrorMessages = "Error" };
                var updateSectionFacultyWarnings = new UpdateSectionFacultyWarnings { WarningCodes = "2", WarningMessages = "Warning" };

                var updateResponse = new UpdateSectionFacultyResponse()
                {
                    CourseSecFacultyId = "12345",
                    CsfGuid = guid,
                    UpdateSectionFacultyWarnings = new List<UpdateSectionFacultyWarnings> { updateSectionFacultyWarnings },
                    UpdateSectionFacultyErrors = new List<UpdateSectionFacultyErrors>() { updateSectionFacultyErrors }

                };
                transManagerMock.Setup(i => i.ExecuteAsync<UpdateSectionFacultyRequest, UpdateSectionFacultyResponse>(It.IsAny<UpdateSectionFacultyRequest>())).ReturnsAsync(updateResponse);
                await sectionRepo.PostSectionFacultyAsync(sectionFaculty, guid);

            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task SectionRepository_PutSectionFacultyAsync_NullSection()
            //{
            //    var actual = await sectionRepo.PutSectionFacultyAsync(null, "92364642-4CF0-4640-B657-DC76CB7E289B");
            //}


            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task SectionRepository_PutSectionFacultyAsync_NullGuid()
            //{
            //    var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";
            //    var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

            //    var actual = await sectionRepo.PutSectionFacultyAsync(sectionFaculty, null);
            //}

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_PutSectionFacultyAsync_RepositoryException()
            {
                var guid = "92364642-4CF0-4640-B657-DC76CB7E289B";

                var sectionFaculty = new SectionFaculty(guid, "1", "12345", "12345", "OLN", new DateTime(2016, 9, 1), new DateTime(2017, 9, 1), 0);

                var updateSectionFacultyErrors = new UpdateSectionFacultyErrors() { ErrorCodes = "1", ErrorMessages = "Error" };
                var updateSectionFacultyWarnings = new UpdateSectionFacultyWarnings { WarningCodes = "2", WarningMessages = "Warning" };

                var updateResponse = new UpdateSectionFacultyResponse()
                {
                    CourseSecFacultyId = "12345",
                    CsfGuid = guid,
                    UpdateSectionFacultyWarnings = new List<UpdateSectionFacultyWarnings> { updateSectionFacultyWarnings },
                    UpdateSectionFacultyErrors = new List<UpdateSectionFacultyErrors>() { updateSectionFacultyErrors }

                };
                transManagerMock.Setup(i => i.ExecuteAsync<UpdateSectionFacultyRequest, UpdateSectionFacultyResponse>(It.IsAny<UpdateSectionFacultyRequest>())).ReturnsAsync(updateResponse);
                await sectionRepo.PutSectionFacultyAsync(sectionFaculty, guid);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionFacultyByGuidAsync_Null()
            {
                var actual = await sectionRepo.GetSectionFacultyByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionFacultyByGuidAsync_Empty()
            {
                var actual = await sectionRepo.GetSectionFacultyByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionFacultyByGuidAsync_CourseSecFaculty_Null()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CourseSecFaculty>(It.IsAny<GuidLookup>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                var actual = await sectionRepo.GetSectionFacultyByGuidAsync("");
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionMeetingIdFromGuid_GuidLookupSuccess()
            {

                // Set up for GUID lookups
                var id = "12345";
                var id2 = "9876";
                var id3 = "0012345";

                var guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                var guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                var guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                var guidLookup = new GuidLookup(guid);
                var guidLookupResult = new GuidLookupResult() { Entity = "COURSE.SEC.MEETING", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                var recordLookup = new RecordKeyLookup("COURSE.SEC.MEETING", id, false);
                var recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    if (gla.Any(gl => gl.Guid == guid2))
                    {
                        guidLookupDict.Add(guid2, null);
                    }
                    if (gla.Any(gl => gl.Guid == guid3))
                    {
                        guidLookupDict.Add(guid3, new GuidLookupResult() { Entity = "COURSE.SEC.MEETING", PrimaryKey = id3 });
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var result = await sectionRepo.GetSectionMeetingIdFromGuidAsync(guid);
                Assert.AreEqual(id, result);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionFacultyIdFromGuid_GuidLookupSuccess()
            {

                // Set up for GUID lookups
                var id = "12345";
                var id2 = "9876";
                var id3 = "0012345";

                var guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                var guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                var guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                var guidLookup = new GuidLookup(guid);
                var guidLookupResult = new GuidLookupResult() { Entity = "COURSE.SEC.FACULTY", PrimaryKey = id };
                var guidLookupDict = new Dictionary<string, GuidLookupResult>();
                var recordLookup = new RecordKeyLookup("COURSE.SEC.FACULTY", id, false);
                var recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    if (gla.Any(gl => gl.Guid == guid))
                    {
                        guidLookupDict.Add(guid, guidLookupResult);
                    }
                    if (gla.Any(gl => gl.Guid == guid2))
                    {
                        guidLookupDict.Add(guid2, null);
                    }
                    if (gla.Any(gl => gl.Guid == guid3))
                    {
                        guidLookupDict.Add(guid3, new GuidLookupResult() { Entity = "COURSE.SEC.FACULTY", PrimaryKey = id3 });
                    }
                    return Task.FromResult(guidLookupDict);
                });

                var result = await sectionRepo.GetSectionFacultyIdFromGuidAsync(guid);
                Assert.AreEqual(id, result);
            }
        }

        [TestClass]
        public class SectionRepository_GetSectionMeetingInstancesAsync : SectionRepositoryTests
        {
            SectionRepository sectionRepo;
            CourseSections section;
            Collection<CalendarSchedules> calendarSchedules;
            Collection<CourseSecMeeting> courseSecMeetings;
            string sectionId;
            List<string> secCalendarSchedules;
            List<string> secMeeting;

            [TestInitialize]
            public void GetSectionMeetingInstancesAsync_Initialize()
            {
                MainInitialize();
                sectionRepo = BuildValidSectionRepository();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionMeetingInstancesAsync_Null_SectionId()
            {
                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSectionMeetingInstancesAsync_Null_Section()
            {
                dataReaderMock.Setup<Task<CourseSections>>(acc => acc.ReadRecordAsync<CourseSections>(sectionId, false)).Returns(Task.FromResult<CourseSections>(null));
                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
            }

            [TestMethod]
            public async Task GetSectionMeetingInstancesAsync_Null_SecCalendarSchedules()
            {
                section.SecCalendarSchedules = null;
                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(0, instances.Count());
            }

            [TestMethod]
            public async Task GetSectionMeetingInstancesAsync_Empty_SecCalendarSchedules()
            {
                section.SecCalendarSchedules = new List<string>();
                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(0, instances.Count());
            }

            [TestMethod]
            [Ignore]
            public async Task GetSectionMeetingInstancesAsync_Returns_SectionMeetingInstance_Objects()
            {
                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(calendarSchedules.Count, instances.Count());
                var instancesList = instances.ToList();
                Assert.IsNull(instancesList[1].StartTime);
                Assert.IsNull(instancesList[1].EndTime);
                Assert.IsNotNull(instancesList[0].InstructionalMethod);
                Assert.IsNull(instancesList[1].InstructionalMethod);
            }

            [TestMethod]
            public async Task GetSectionMeetingInstancesAsync_Null_CalsData()
            {
                dataReaderMock.Setup<Task<Collection<CalendarSchedules>>>(acc => acc.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<CalendarSchedules>>(null));
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(0, instances.Count());
            }

            [TestMethod]
            public async Task GetSectionMeetingInstancesAsync_Empty_CalsData()
            {
                dataReaderMock.Setup<Task<Collection<CalendarSchedules>>>(acc => acc.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<CalendarSchedules>>(new Collection<CalendarSchedules>()));
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new Ellucian.Colleague.Domain.Student.Tests.TestTermRepository(), apiSettings);

                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(0, instances.Count());
            }

            [TestMethod]
            public async Task GetSectionMeetingInstancesAsync_Null_CalsDate()
            {
                calendarSchedules[0].CalsDate = null;
                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(calendarSchedules.Where(cs => cs.CalsDate.HasValue).Count(), instances.Count());
            }

            [TestMethod]
            public async Task GetSectionMeetingInstancesAsync_CalsDate_01011986()
            {
                calendarSchedules[0].CalsDate = new DateTime(1968, 1, 1);
                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(calendarSchedules.Where(cs => cs.CalsDate > new DateTime(1968, 1, 1)).Count(), instances.Count());
            }

            [TestMethod]
            public async Task GetSectionMeetingInstancesAsync_Null_CalsPointer()
            {
                calendarSchedules[0].CalsPointer = null;
                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(calendarSchedules.Where(cs => !string.IsNullOrWhiteSpace(cs.CalsPointer)).Count(), instances.Count());
            }

            [TestMethod]
            [Ignore]
            public async Task GetSectionMeetingInstancesAsync_Null_SectionMeetingData()
            {
                dataReaderMock.Setup<Task<Collection<CourseSecMeeting>>>(acc => acc.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<CourseSecMeeting>>(null));
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(calendarSchedules.Count, instances.Count());
                var instancesList = instances.ToList();
                Assert.IsNull(instancesList[0].InstructionalMethod);
                Assert.IsNull(instancesList[1].InstructionalMethod);
            }

            [TestMethod]
            [Ignore]
            public async Task GetSectionMeetingInstancesAsync_Empty_SectionMeetingData()
            {
                dataReaderMock.Setup<Task<Collection<CourseSecMeeting>>>(acc => acc.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<CourseSecMeeting>>(new Collection<CourseSecMeeting>()));
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(calendarSchedules.Count, instances.Count());
                var instancesList = instances.ToList();
                Assert.IsNull(instancesList[0].InstructionalMethod);
                Assert.IsNull(instancesList[1].InstructionalMethod);
            }

            [TestMethod]
            [Ignore]
            public async Task GetSectionMeetingInstancesAsync_No_Matching_SectionMeetingData()
            {
                calendarSchedules[1].CalsCourseSecMeeting = "ABCD";
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                var instances = await sectionRepo.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(calendarSchedules.Count, instances.Count());
                var instancesList = instances.ToList();
                Assert.IsNotNull(instancesList[0].InstructionalMethod);
                Assert.IsNull(instancesList[1].InstructionalMethod);
            }

            private SectionRepository BuildValidSectionRepository()
            {
                sectionId = "0123";
                secCalendarSchedules = new List<string>() { "2345", "3456" };
                secMeeting = new List<string>() { "4567", "5678" };
                section = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecCalendarSchedules = secCalendarSchedules,
                    SecMeeting = secMeeting
                };
                calendarSchedules = new Collection<CalendarSchedules>()
                {
                    new CalendarSchedules()
                    {
                        Recordkey = secCalendarSchedules[0],
                        CalsDate = DateTime.Today.AddDays(1),
                        CalsDescription = "CalendarSchedules1",
                        CalsPointer = "6789",
                        CalsStartTime = DateTime.Now.AddHours(-3),
                        CalsEndTime = DateTime.Now.AddHours(-2),
                        CalsCourseSecMeeting = secMeeting[0]
                    },
                    new CalendarSchedules()
                    {
                        Recordkey = secCalendarSchedules[1],
                        CalsDate = DateTime.Today.AddDays(3),
                        CalsDescription = "CalendarSchedules2",
                        CalsPointer = "7890",
                        CalsCourseSecMeeting = secMeeting[1]
                    }
                };
                courseSecMeetings = new Collection<CourseSecMeeting>()
                {
                    new CourseSecMeeting()
                    {
                        Recordkey = secMeeting[0],
                        CsmInstrMethod = "I"
                    },
                    new CourseSecMeeting()
                    {
                        Recordkey = secMeeting[1],
                        CsmInstrMethod = string.Empty
                    }
                };
                dataReaderMock.Setup<Task<CourseSections>>(acc => acc.ReadRecordAsync<CourseSections>(sectionId, false)).Returns(Task.FromResult<CourseSections>(section));
                dataReaderMock.Setup<Task<Collection<CalendarSchedules>>>(acc => acc.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<CalendarSchedules>>(calendarSchedules));
                dataReaderMock.Setup<Task<Collection<CourseSecMeeting>>>(acc => acc.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<CourseSecMeeting>>(courseSecMeetings));

                // Set up repo response for the temporary international parameter item
                Data.Base.DataContracts.IntlParams intlParams = new Data.Base.DataContracts.IntlParams();
                intlParams.HostDateDelimiter = "/";
                intlParams.HostShortDateFormat = "MDY";
                dataReaderMock.Setup<Task<Data.Base.DataContracts.IntlParams>>(iacc => iacc.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                // Mock data needed to read campus calendar
                var startTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 06, 00, 00);
                var endTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 23, 30, 00);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true))
                    .ReturnsAsync(new Dflts() { DfltsCampusCalendar = "CAL" });
                dataReaderMock.Setup(r => r.ReadRecordAsync<Data.Base.DataContracts.CampusCalendar>("CAL", true))
                    .ReturnsAsync(new Data.Base.DataContracts.CampusCalendar() { Recordkey = "CAL", CmpcDesc = "Calendar", CmpcDayStartTime = startTime, CmpcDayEndTime = endTime, CmpcBookPastNoDays = "30", CmpcSpecialDays = specialDaysTestData.CampusSpecialDayIds });
                // Set up repo response for section statuses
                var sectionStatuses = new ApplValcodes();
                sectionStatuses.ValsEntityAssociation = new List<ApplValcodesVals>();
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("A", "Active", "1", "A", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("I", "Inactive", "2", "I", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Cancelled", "", "C", "", "", ""));
                dataReaderMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", true)).ReturnsAsync(sectionStatuses);

                //setup mocking for CourseSecMeeting
                var crsSecMeet = BuildCourseSecMeetingDefaults();
                dataReaderMock.Setup<Task<CourseSecMeeting>>(dr => dr.ReadRecordAsync<CourseSecMeeting>(It.IsAny<GuidLookup>(), true)).Returns(Task.FromResult(crsSecMeet));

                //setup mocking for Stweb Defaults
                var stWebDflt = BuildStwebDefaults(); ;
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                    );

                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
                   (id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                   );
                MockRecordsAsync<InstrMethods>("INSTR.METHODS", BuildValidInstrMethodResponse());

                // Set up repo response for reg billing rates
                Collection<RegBillingRates> rbrs = new Collection<RegBillingRates>()
                {
                    new RegBillingRates()
                    {
                        Recordkey = "123",
                        RgbrAmtCalcType = "A",
                        RgbrArCode = "ABC",
                        RgbrChargeAmt = 50m,
                        RgbrRule = "RULE1",
                    },
                    new RegBillingRates()
                    {
                        Recordkey = "124",
                        RgbrAmtCalcType = "F",
                        RgbrArCode = "DEF",
                        RgbrCrAmt = 100m
                    },
                };
                dataReaderMock.Setup<Task<Collection<RegBillingRates>>>(cacc => cacc.BulkReadRecordAsync<RegBillingRates>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(rbrs);


                var bookOptions = new ApplValcodes();
                bookOptions.ValsEntityAssociation = new List<ApplValcodesVals>();
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("R", "Required", "1", "R", "", "", ""));
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Recommended", "2", "C", "", "", ""));
                bookOptions.ValsEntityAssociation.Add(new ApplValcodesVals("O", "Optional", "2", "O", "", "", ""));
                dataReaderMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BOOK.OPTION", true)).ReturnsAsync(bookOptions);
                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                return sectionRepo;
            }

        }

        [TestClass]
        public class SectionRepository_GetSectionRosterAsync_Tests : SectionRepositoryTests
        {
            SectionRepository sectionRepo;
            CourseSections courseSection;
            Collection<StudentCourseSec> studentCourseSecs;
            Collection<CourseSecFaculty> courseSecFacultys;
            string sectionId;

            [TestInitialize]
            public async void Initialize()
            {
                MainInitialize();
                sectionId = "12345";
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecActiveStudents = new List<string>() { "123", "124", "125", "126", "127" },
                    SecFaculty = new List<string>() { "234", "235" }
                };
                studentCourseSecs = new Collection<StudentCourseSec>()
                {
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[0], ScsCourseSection = sectionId, ScsStudent = "0001234", RecordGuid = "3bf856dd-44a6-40f8-95fc-65916611aa87" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[1], ScsCourseSection = sectionId, ScsStudent = "0001235", RecordGuid = "f7665fc1-a206-40bb-be27-eedcce762945" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[2], ScsCourseSection = sectionId, ScsStudent = "0001236", RecordGuid = "7461344d-d7ea-4bc5-a864-a3f971cd64d0" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[3], ScsCourseSection = sectionId, ScsStudent = "0001237", RecordGuid = "ca2384b2-a541-421d-be8b-2045f993f752" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[4], ScsCourseSection = sectionId, ScsStudent = "0001238", RecordGuid = "fa313328-abaf-44c6-855c-ca07c31ecc94" },
                };
                courseSecFacultys = new Collection<CourseSecFaculty>()
                {
                    new CourseSecFaculty() { RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(), Recordkey = courseSection.SecFaculty[0], CsfCourseSection = sectionId, CsfFaculty = "0001239" },
                    new CourseSecFaculty() { RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(), Recordkey = courseSection.SecFaculty[1], CsfCourseSection = sectionId, CsfFaculty = "0001240" },
                };

                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                MockRecordsAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecs);
                MockRecordsAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", courseSecFacultys);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionRosterAsync_null_SectionId_throws_exception()
            {
                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRepository_GetSectionRosterAsync_invalid_CourseSections_record_throws_exception()
            {
                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync("INVALID");
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionRosterAsync_verify_FacultyIds()
            {
                // Data setup
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecFaculty = new List<string>() { null, "235" }
                };
                courseSecFacultys = new Collection<CourseSecFaculty>()
                {
                    new CourseSecFaculty() { RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(), Recordkey = courseSection.SecFaculty[1], CsfCourseSection = sectionId, CsfFaculty = "0001240" },
                };
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                MockRecordsAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", courseSecFacultys);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(sectionId);

                // Assertions
                Assert.IsNotNull(entity);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.AreEqual(courseSection.SecFaculty.Count - 1, entity.FacultyIds.Count);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionRosterAsync_verify_StudentIds()
            {
                // Data setup
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecActiveStudents = new List<string>() { null, "123", "124", "125", "126", "127" }
                };
                studentCourseSecs = new Collection<StudentCourseSec>()
                {
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[1], ScsCourseSection = sectionId, ScsStudent = "0001234", RecordGuid = "b0d591b7-910d-4cf1-a5f1-ae94ea828844" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[2], ScsCourseSection = sectionId, ScsStudent = "0001235", RecordGuid = "192b593c-abbe-41f0-b094-5c68aa4c143d" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[3], ScsCourseSection = sectionId, ScsStudent = "0001236", RecordGuid = "4e523bc4-10ba-4f01-8ccc-e6a22ca101d3" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[4], ScsCourseSection = sectionId, ScsStudent = "0001237", RecordGuid = "65a061fa-44d6-4717-b631-3954a9a1e307" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[5], ScsCourseSection = sectionId, ScsStudent = "0001238", RecordGuid = "9ca48250-e199-4b95-bab3-03e4c74fdfcf" },
                };
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                MockRecordsAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecs);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(sectionId);

                // Assertions
                Assert.IsNotNull(entity);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.AreEqual(courseSection.SecActiveStudents.Count - 1, entity.StudentIds.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task SectionRepository_GetSectionRosterAsync_CourseSecFaculty_records_null_throws_exception()
            {
                // Data setup
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecActiveStudents = new List<string>() { "123", "124", "125", "126", "127" },
                    SecFaculty = new List<string>() { "234", "235" }
                };
                courseSecFacultys = null;
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                MockRecordsAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecs);
                MockRecordsAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", courseSecFacultys);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(sectionId);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task SectionRepository_GetSectionRosterAsync_StudentCourseSec_records_null_throws_exception()
            {
                // Data setup
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecActiveStudents = new List<string>() { "123", "124", "125", "126", "127" }
                };
                studentCourseSecs = null;
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                MockRecordsAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecs);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(sectionId);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionRosterAsync_verify_FacultyIds_with_null_CourseSecFaculty_CsfFaculty()
            {
                // Data setup
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecFaculty = new List<string>() { "234", "235" }
                };
                courseSecFacultys = new Collection<CourseSecFaculty>()
                {
                    new CourseSecFaculty() { RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(), Recordkey = courseSection.SecFaculty[0], CsfCourseSection = sectionId, CsfFaculty = "0001239" },
                    new CourseSecFaculty() { RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(), Recordkey = courseSection.SecFaculty[1], CsfCourseSection = sectionId, CsfFaculty = null },
                };
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                MockRecordsAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", courseSecFacultys);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(sectionId);

                // Assertions
                Assert.IsNotNull(entity);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.AreEqual(courseSection.SecFaculty.Count - 1, entity.FacultyIds.Count);
                loggerMock.Verify(x => x.Error(It.IsAny<string>()));
            }


            [TestMethod]
            public async Task SectionRepository_GetSectionRosterAsync_verify_StudentIds_with_null_StudentCourseSec_ScsStudent()
            {
                // Data setup
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecActiveStudents = new List<string>() { "123", "124", "125", "126", "127" }
                };
                studentCourseSecs = new Collection<StudentCourseSec>()
                {
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[0], ScsCourseSection = sectionId, ScsStudent = "0001234", RecordGuid = "e44e11c3-4010-49bd-9a57-e533f17962fb" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[1], ScsCourseSection = sectionId, ScsStudent = "0001235", RecordGuid = "93fbd484-dd0e-4435-904f-9681a8b07585" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[2], ScsCourseSection = sectionId, ScsStudent = "0001236", RecordGuid = "235ce315-22d4-48e8-b14f-d28dd2401dc0" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[3], ScsCourseSection = sectionId, ScsStudent = "0001237", RecordGuid = "1c856468-cb3d-49fc-a95d-545e81fc53a5" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[4], ScsCourseSection = sectionId, ScsStudent = null, RecordGuid = "b076b2b7-530e-41cf-9323-9515fd7ff5cd" },
                };
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                MockRecordsAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecs);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(sectionId);

                // Assertions
                Assert.IsNotNull(entity);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.AreEqual(courseSection.SecActiveStudents.Count - 1, entity.StudentIds.Count);
                loggerMock.Verify(x => x.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionRosterAsync_missing_CourseSecFaculty_record_logs_and_continue()
            {
                // Data setup
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecFaculty = new List<string>() { "234", "235" }
                };
                courseSecFacultys = new Collection<CourseSecFaculty>()
                {
                    new CourseSecFaculty() { RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(), Recordkey = courseSection.SecFaculty[0], CsfCourseSection = sectionId, CsfFaculty = "0001239" },
                };
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                MockRecordsAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", courseSecFacultys);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(sectionId);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.AreEqual(1, entity.FacultyIds.Count());
                Assert.AreEqual(0, entity.StudentIds.Count());
                Assert.AreEqual("0001239", entity.FacultyIds[0]);
                loggerMock.Verify(x => x.Error("Unable to retrieve COURSE.SEC.FACULTY data for IDs: 235"));
            }


            [TestMethod]
            public async Task SectionRepository_GetSectionRosterAsync_missing_StudentCourseSec_record_logs_and_continue()
            {
                // Data setup
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecActiveStudents = new List<string>() { "123", "124", "125", "126", "128", "129" }
                };
                studentCourseSecs = new Collection<StudentCourseSec>()
                {
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[0], ScsCourseSection = sectionId, ScsStudent = "0001234", RecordGuid = "b717abc7-dbf0-460e-888a-5162ff4f53e5"},
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[1], ScsCourseSection = sectionId, ScsStudent = "0001235", RecordGuid = "011f87f4-67c5-419b-9516-e13214a0a79c" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[2], ScsCourseSection = sectionId, ScsStudent = "0001236", RecordGuid = "49595e8e-3a9e-4213-b82a-07a9bd4660a9" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[3], ScsCourseSection = sectionId, ScsStudent = "0001237", RecordGuid = "d87f8d2f-8e60-453d-b681-82201fbbce4b" }
                };
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                MockRecordsAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecs);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(sectionId);
                Assert.AreEqual(4, entity.StudentIds.Count());
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.AreEqual(0, entity.FacultyIds.Count());
                Assert.AreEqual("0001234", entity.StudentIds[0]);
                Assert.AreEqual("0001235", entity.StudentIds[1]);
                Assert.AreEqual("0001236", entity.StudentIds[2]);
                Assert.AreEqual("0001237", entity.StudentIds[3]);
                loggerMock.Verify(x => x.Error("Unable to retrieve STUDENT.COURSE.SECTION data for IDs: 128, 129"));
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionRosterAsync_invalid_CourseSecFaculty_record_logs_and_continue()
            {
                // Data setup
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecFaculty = new List<string>() { "234", "235" }
                };
                courseSecFacultys = new Collection<CourseSecFaculty>()
                {
                    new CourseSecFaculty() { RecordGuid = Guid.NewGuid().ToString().ToLowerInvariant(), Recordkey = courseSection.SecFaculty[0], CsfCourseSection = sectionId, CsfFaculty = "0001239" },
                    null
                };
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<CourseSecFaculty>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(courseSecFacultys);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(sectionId);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.AreEqual(1, entity.FacultyIds.Count());
                Assert.AreEqual(0, entity.StudentIds.Count());
                Assert.AreEqual("0001239", entity.FacultyIds[0]);
                loggerMock.Verify(x => x.Error("Unable to retrieve COURSE.SEC.FACULTY data for IDs: 235"));
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionRosterAsync_invalid_StudentCourseSec_record_logs_and_continue()
            {
                // Data setup
                courseSection = new CourseSections()
                {
                    Recordkey = sectionId,
                    SecActiveStudents = new List<string>() { "123", "124", "125", "126", "128" }
                };
                studentCourseSecs = new Collection<StudentCourseSec>()
                {
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[0], ScsCourseSection = sectionId, ScsStudent = "0001234" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[1], ScsCourseSection = sectionId, ScsStudent = "0001235" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[2], ScsCourseSection = sectionId, ScsStudent = "0001236" },
                    new StudentCourseSec() { Recordkey = courseSection.SecActiveStudents[3], ScsCourseSection = sectionId, ScsStudent = "0001237" },
                    null
                };
                MockRecordAsync<CourseSections>("COURSE.SECTIONS", courseSection);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentCourseSecs);

                // Construct section repository
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);

                // Call repository
                var entity = await sectionRepo.GetSectionRosterAsync(sectionId);
                Assert.AreEqual(4, entity.StudentIds.Count());
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.AreEqual(0, entity.FacultyIds.Count());
                Assert.AreEqual("0001234", entity.StudentIds[0]);
                Assert.AreEqual("0001235", entity.StudentIds[1]);
                Assert.AreEqual("0001236", entity.StudentIds[2]);
                Assert.AreEqual("0001237", entity.StudentIds[3]);
                loggerMock.Verify(x => x.Error("Unable to retrieve STUDENT.COURSE.SECTION data for IDs: 128"));
            }
        }


        [TestClass]
        public class SectionRepository_GetSectionEventsICalAsync : SectionRepositoryTests
        {
            Dictionary<string, Section> regSectionsDict;
            IEnumerable<Event> allCals;
            SectionRepository sectionRepo;
            List<Section> sections;
            Section section1;
            Section section2;
            string cacheKey = "";

            Collection<CalendarSchedules> section1first5Data;
            Collection<CalendarSchedules> section2first3Data;
            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };



            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();
                sections = new List<Section>();
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("1111111", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.Name = "Section 1";
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.Guid = Guid.NewGuid().ToString();
                section1.AddSectionMeeting(new SectionMeeting("s1", "1111111", "lec", null, null, "daily"));
                section1.AddSectionMeeting(new SectionMeeting("s2", "1111111", "lab", null, null, "daily"));
                sections.Add(section1);

                //section 2 with primary section meetings
                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("2222222", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.Name = "Section 2";
                section2.TermId = "2012/FA";
                section2.PrimarySectionId = "1111111";
                section2.EndDate = new DateTime(2012, 12, 21);
                section2.Guid = Guid.NewGuid().ToString();
                section2.UpdatePrimarySectionMeetings(new List<SectionMeeting>() {
                    new SectionMeeting("s1", "1111111", "lec", null, null, "daily"),
                     new SectionMeeting("s2", "1111111", "lab", null, null, "daily")
                });
                sections.Add(section2);

                allCals = new TestEventRepository().Get();
                regSectionsDict = new Dictionary<string, Section>();

                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                cacheKey = sectionRepo.BuildFullCacheKey("AllRegistrationSections");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(regSectionsDict).Verifiable();


            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionEventsICalAsync_CalendarscheduleType_Is_Null()
            {
                List<string> secs = new List<string>();
                secs.Add("1111111");
                IEnumerable<Event> cals = await sectionRepo.GetSectionEventsICalAsync(null, secs, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionEventsICalAsync_Sections_Is_Null()
            {
                List<string> secs = new List<string>();
                IEnumerable<Event> cals = await sectionRepo.GetSectionEventsICalAsync("CS", null, null, null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionRepository_GetSectionEventsICalAsync_Sections_Are_Empty()
            {
                List<string> secs = new List<string>();
                IEnumerable<Event> cals = await sectionRepo.GetSectionEventsICalAsync("CS", secs, null, null);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionEventsICalAsync_CannotRetrieveCachedSections()
            {
                regSectionsDict = new Dictionary<string, Section>();
                List<string> secs = new List<string>();
                secs.Add("1111111");


                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(new Collection<CourseSections>()));
                string message = "Following calendarSchedulePointers have no corresponding Section information in a cache, their calendar schedules will not be retrieved. " + "1111111";
                loggerMock.Setup(x => x.Info(It.Is<string>(y => y == message))).Verifiable();
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(regSectionsDict).Verifiable();
                IEnumerable<Event> cals = await sectionRepo.GetSectionEventsICalAsync("CS", secs, null, null);
                loggerMock.Verify();

                Assert.IsNotNull(cals);
                Assert.AreEqual(0, cals.Count());

            }
            [TestMethod]
            public async Task SectionRepository_GetSectionEventsICalAsyncGet_OneSectionwith5Days()
            {

                regSectionsDict["1111111"] = section1;


                List<string> secs = new List<string>();
                secs.Add("1111111");
                IEnumerable<Event> s1f5days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "1111111").AsEnumerable();
                section1first5Data = BuildCalendarSchedulesResponse(s1f5days);

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", It.IsAny<string>(), true)).ReturnsAsync(section1first5Data);
                IEnumerable<Event> cals = await sectionRepo.GetSectionEventsICalAsync("CS", secs, null, null);
                // make sure the setup is correct
                Assert.AreEqual(5, section1first5Data.Count());
                // make sure we return the correct cals count
                Assert.AreEqual(section1first5Data.Count(), cals.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionEventsICalAsync_TwoSections_WithPrimaryMeetings()
            {

                regSectionsDict.Add("1111111", section1);
                regSectionsDict.Add("2222222", section2);

                List<string> secs = new List<string>();
                secs.Add("1111111");
                secs.Add("2222222");
                IEnumerable<Event> s1f5days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "1111111").AsEnumerable();
                section1first5Data = BuildCalendarSchedulesResponse(s1f5days);
                string criteria1 = "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '1111111'   BY CALS.DATE BY CALS.START.TIME";
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", criteria1, true)).ReturnsAsync(section1first5Data);

                IEnumerable<Event> cals = await sectionRepo.GetSectionEventsICalAsync("CS", secs, null, null);
                // cross-listed sections will have same section meetings as of primary
                Assert.AreEqual(10, section1first5Data.Count() * 2);
                // make sure we return the correct cals count
                Assert.AreEqual(section1first5Data.Count() * 2, cals.Count());
                //validate description of first section events. same as CalsDescription
                Assert.AreEqual(section1first5Data[0].CalsDescription, cals.ToList()[0].Description);

                //validate description of 2nd section events. It is cross-listed section with primary meetings therefore will have description of its own rather than primary scetion
                Assert.AreEqual(string.Join(" ", section2.Name, section2.Title), cals.ToList()[5].Description);
                Assert.AreEqual("Section 2 Introduction to Art", cals.ToList()[6].Description);

            }

            [TestMethod]
            public async Task SectionRepository_GetSectionEventsICalAsync_CannotRetrieveCalendarSchedules_ForCachedSections()
            {
                regSectionsDict["1111111"] = section1;
                List<string> secs = new List<string>();
                secs.Add("1111111");
                string criteria1 = "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '1111111'   BY CALS.DATE BY CALS.START.TIME";
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", criteria1, true)).ReturnsAsync(new Collection<CalendarSchedules>());
                IEnumerable<Event> cals = await sectionRepo.GetSectionEventsICalAsync("CS", secs, null, null);
                Assert.IsNotNull(cals);
                Assert.AreEqual(0, cals.Count());

            }

            [TestMethod]
            public async Task SectionRepository_GetSectionEventsICalAsync_TwoSections()
            {

                regSectionsDict.Add("1111111", section1);
                regSectionsDict.Add("2222222", section2);

                section2.UpdatePrimarySectionMeetings(new List<SectionMeeting>());
                section2.PrimarySectionId = "2222222";

                List<string> secs = new List<string>();
                secs.Add("1111111");
                secs.Add("2222222");
                IEnumerable<Event> s1f5days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "1111111").AsEnumerable();
                IEnumerable<Event> s2f3days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "2222222").AsEnumerable();
                section1first5Data = BuildCalendarSchedulesResponse(s1f5days);
                section2first3Data = BuildCalendarSchedulesResponse(s2f3days);
                string criteria1 = "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '1111111'   BY CALS.DATE BY CALS.START.TIME";
                string criteria2 = "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '2222222'   BY CALS.DATE BY CALS.START.TIME";
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", criteria1, true)).ReturnsAsync(section1first5Data);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", criteria2, true)).ReturnsAsync(section2first3Data);

                IEnumerable<Event> cals = await sectionRepo.GetSectionEventsICalAsync("CS", secs, null, null);
                // cross-listed sections will have same section meetings as of primary
                Assert.AreEqual(8, section1first5Data.Count() + section2first3Data.Count());
                // make sure we return the correct cals count
                Assert.AreEqual(section1first5Data.Count() + section2first3Data.Count(), cals.Count());
                //validate description of first section events. same as CalsDescription
                Assert.AreEqual(section1first5Data[0].CalsDescription, cals.ToList()[0].Description);
                Assert.AreEqual(section2first3Data[0].CalsDescription, cals.ToList()[6].Description);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionEventsICalAsync_TwoSections_WithSectionsRepeated()
            {

                regSectionsDict.Add("1111111", section1);
                regSectionsDict.Add("2222222", section2);

                section2.UpdatePrimarySectionMeetings(new List<SectionMeeting>());
                section2.PrimarySectionId = "2222222";

                List<string> secs = new List<string>();
                secs.Add("1111111");
                secs.Add("2222222");
                secs.Add("1111111");
                string message = "Section with id 1111111 is already in dictionary with its calendar schedules";
                loggerMock.Setup(x => x.Info(It.Is<string>(y => y == message))).Verifiable();


                IEnumerable<Event> s1f5days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "1111111").AsEnumerable();
                IEnumerable<Event> s2f3days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "2222222").AsEnumerable();
                section1first5Data = BuildCalendarSchedulesResponse(s1f5days);
                section2first3Data = BuildCalendarSchedulesResponse(s2f3days);
                string criteria1 = "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '1111111'   BY CALS.DATE BY CALS.START.TIME";
                string criteria2 = "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '2222222'   BY CALS.DATE BY CALS.START.TIME";
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", criteria1, true)).ReturnsAsync(section1first5Data);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", criteria2, true)).ReturnsAsync(section2first3Data);

                IEnumerable<Event> cals = await sectionRepo.GetSectionEventsICalAsync("CS", secs, null, null);
                loggerMock.Verify();
                // cross-listed sections will have same section meetings as of primary
                Assert.AreEqual(8, section1first5Data.Count() + section2first3Data.Count());
                // make sure we return the correct cals count
                Assert.AreEqual(section1first5Data.Count() + section2first3Data.Count(), cals.Count());
                //validate description of first section events. same as CalsDescription
                Assert.AreEqual(section1first5Data[0].CalsDescription, cals.ToList()[0].Description);
                Assert.AreEqual(section2first3Data[0].CalsDescription, cals.ToList()[6].Description);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionEventsICalAsync_DataReaderForCalendarScheduled_Returns_Exception()
            {

                regSectionsDict.Add("1111111", section1);
                regSectionsDict.Add("2222222", section2);

                section2.UpdatePrimarySectionMeetings(new List<SectionMeeting>());
                section2.PrimarySectionId = "2222222";

                List<string> secs = new List<string>();
                secs.Add("1111111");
                secs.Add("2222222");
                IEnumerable<Event> s1f5days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "1111111").AsEnumerable();
                IEnumerable<Event> s2f3days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "2222222").AsEnumerable();
                section1first5Data = BuildCalendarSchedulesResponse(s1f5days);
                section2first3Data = BuildCalendarSchedulesResponse(s2f3days);
                string criteria1 = "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '1111111'   BY CALS.DATE BY CALS.START.TIME";
                string criteria2 = "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '2222222'   BY CALS.DATE BY CALS.START.TIME";
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", criteria1, true)).ReturnsAsync(section1first5Data);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", criteria2, true)).Throws(new Exception());

                IEnumerable<Event> cals = await sectionRepo.GetSectionEventsICalAsync("CS", secs, null, null);
                // cross-listed sections will have same section meetings as of primary
                Assert.AreEqual(5, section1first5Data.Count());
                // make sure we return the correct cals count
                Assert.AreEqual(section1first5Data.Count(), cals.Count());
                //validate description of first section events. same as CalsDescription
                Assert.AreEqual(section1first5Data[0].CalsDescription, cals.ToList()[0].Description);
            }


            private Collection<CalendarSchedules> BuildCalendarSchedulesResponse(IEnumerable<Event> events)
            {
                Collection<CalendarSchedules> repoCalendarSchedules = new Collection<CalendarSchedules>();
                foreach (var calEvent in events)
                {
                    var repoCal = new CalendarSchedules();
                    repoCal.CalsBuildings = calEvent.Buildings.ToList();
                    repoCal.CalsRooms = calEvent.Rooms.ToList();
                    repoCal.Recordkey = calEvent.Id.ToString();
                    repoCal.CalsDate = new DateTime(calEvent.Start.Year, calEvent.Start.Month, calEvent.Start.Day, 0, 0, 0);
                    repoCal.CalsDescription = calEvent.Description;
                    repoCal.CalsLocation = calEvent.LocationCode;
                    repoCal.CalsStartTime = calEvent.Start;
                    repoCal.CalsEndTime = calEvent.End;
                    repoCal.CalsPointer = calEvent.Pointer;
                    repoCal.CalsType = calEvent.Type;
                    repoCalendarSchedules.Add(repoCal);
                    repoCal.CalsBldgRoomEntityAssociation = new List<CalendarSchedulesCalsBldgRoom>();
                    for (int roomIdx = 0; roomIdx < repoCal.CalsRooms.Count(); roomIdx++)
                    {
                        var assocMember = new CalendarSchedulesCalsBldgRoom()
                        {
                            CalsBuildingsAssocMember = repoCal.CalsBuildings[roomIdx],
                            CalsRoomsAssocMember = repoCal.CalsRooms[roomIdx]
                        };
                        repoCal.CalsBldgRoomEntityAssociation.Add(assocMember);
                    }
                }

                return repoCalendarSchedules;
            }
        }


        [TestClass]
        public class SectionRepository_GetSectionsRetrievalDateRangeAsync_PrivateMethod_Tests : SectionRepositoryTests
        {
            MethodInfo methodInfo = null;
            List<Section> regSections = new List<Section>();
            List<Term> registrationTerms = new List<Term>();
            StwebDefaults stWebDflt;

            SectionRepository sectionRepo;

            [TestInitialize]
            public void Initialize()
            {
                base.MainInitialize();

                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                stWebDflt = BuildStwebDefaults();
                dataReaderMock.Setup<Task<StwebDefaults>>(ps => ps.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false)).Returns(Task.FromResult(stWebDflt));


                methodInfo = typeof(SectionRepository).GetMethod("GetSectionsRetrievalDateRangeAsync", BindingFlags.NonPublic | BindingFlags.Instance);

                Term term1 = new Term("2012/FA", "Fall 2012", new DateTime(2012, 9, 1), new DateTime(2012, 12, 15), 2012, 1, true, true, "2012/FA", true);
                Term term2 = new Term("2013/SP", "Spring 2013", new DateTime(2013, 1, 1), new DateTime(2013, 5, 15), 2012, 2, true, true, "2013/SP", true);
                registrationTerms.Add(term1);
                registrationTerms.Add(term2);


            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                regSections = null;
                sectionRepo = null;
            }


            [TestMethod]
            public async Task SectionRepository_RegStartEndDates_outside_term_dates()
            {

                stWebDflt.StwebRegStartDate = new DateTime(2012, 10, 20);
                stWebDflt.StwebRegEndDate = new DateTime(2013, 04, 15);
                object[] parameters = { registrationTerms };
                Tuple<DateTime, DateTime> selectedDates = await (Task<Tuple<DateTime, DateTime>>)methodInfo.Invoke(sectionRepo, parameters);
                Assert.IsNotNull(selectedDates);
                Assert.AreEqual(selectedDates.Item1, new DateTime(2012, 9, 1));
                Assert.AreEqual(selectedDates.Item2, new DateTime(2013, 05, 15));
            }
            [TestMethod]
            public async Task SectionRepository_RegStartEndDates_within_term_dates()
            {

                stWebDflt.StwebRegStartDate = new DateTime(2012, 08, 20);
                stWebDflt.StwebRegEndDate = new DateTime(2013, 06, 15);
                object[] parameters = { registrationTerms };
                Tuple<DateTime, DateTime> selectedDates = await (Task<Tuple<DateTime, DateTime>>)methodInfo.Invoke(sectionRepo, parameters);
                Assert.IsNotNull(selectedDates);
                Assert.AreEqual(selectedDates.Item1, new DateTime(2012, 08, 20));
                Assert.AreEqual(selectedDates.Item2, new DateTime(2013, 06, 15));
            }
            [TestMethod]
            public async Task SectionRepository_RegStartEndDates_StartDate_within_term_dates()
            {

                stWebDflt.StwebRegStartDate = new DateTime(2012, 10, 20);
                stWebDflt.StwebRegEndDate = new DateTime(2013, 06, 15);
                object[] parameters = { registrationTerms };
                Tuple<DateTime, DateTime> selectedDates = await (Task<Tuple<DateTime, DateTime>>)methodInfo.Invoke(sectionRepo, parameters);
                Assert.IsNotNull(selectedDates);
                Assert.AreEqual(selectedDates.Item1, new DateTime(2012, 09, 01));
                Assert.AreEqual(selectedDates.Item2, new DateTime(2013, 06, 15));
            }
            [TestMethod]
            public async Task SectionRepository_RegStartEndDates_EndDate_within_term_dates()
            {

                stWebDflt.StwebRegStartDate = new DateTime(2012, 08, 20);
                stWebDflt.StwebRegEndDate = new DateTime(2013, 04, 15);
                object[] parameters = { registrationTerms };
                Tuple<DateTime, DateTime> selectedDates = await (Task<Tuple<DateTime, DateTime>>)methodInfo.Invoke(sectionRepo, parameters);
                Assert.IsNotNull(selectedDates);
                Assert.AreEqual(selectedDates.Item1, new DateTime(2012, 08, 20));
                Assert.AreEqual(selectedDates.Item2, new DateTime(2013, 05, 15));
            }

            [TestMethod]
            public async Task SectionRepository__RegStartEndDates_are_null()
            {


                object[] parameters = { registrationTerms };
                Tuple<DateTime, DateTime> selectedDates = await (Task<Tuple<DateTime, DateTime>>)methodInfo.Invoke(sectionRepo, parameters);
                Assert.IsNotNull(selectedDates);
                Assert.AreEqual(selectedDates.Item1, new DateTime(2012, 9, 1));
                Assert.AreEqual(selectedDates.Item2, new DateTime(2013, 5, 15));
            }
            [TestMethod]
            public async Task SectionRepository__RegStartEndDates_StartDate_IsNull()
            {

                stWebDflt.StwebRegEndDate = new DateTime(2013, 06, 15);
                object[] parameters = { registrationTerms };
                Tuple<DateTime, DateTime> selectedDates = await (Task<Tuple<DateTime, DateTime>>)methodInfo.Invoke(sectionRepo, parameters);
                Assert.IsNotNull(selectedDates);
                Assert.AreEqual(selectedDates.Item1, new DateTime(2012, 9, 1));
                Assert.AreEqual(selectedDates.Item2, new DateTime(2013, 06, 15));
            }
            [TestMethod]
            public async Task SectionRepository__RegStartEndDates_EndDate_IsNull()
            {

                stWebDflt.StwebRegStartDate = new DateTime(2012, 08, 20);
                object[] parameters = { registrationTerms };
                Tuple<DateTime, DateTime> selectedDates = await (Task<Tuple<DateTime, DateTime>>)methodInfo.Invoke(sectionRepo, parameters);
                Assert.IsNotNull(selectedDates);
                Assert.AreEqual(selectedDates.Item1, new DateTime(2012, 08, 20));
                Assert.AreEqual(selectedDates.Item2, new DateTime(2013, 05, 15));
            }
            [TestMethod]
            public async Task SectionRepository_RegStartEndDates_StartDate_is_more_than_end_date()
            {

                stWebDflt.StwebRegEndDate = new DateTime(2012, 08, 20);
                stWebDflt.StwebRegStartDate = new DateTime(2013, 06, 15);
                object[] parameters = { registrationTerms };
                Tuple<DateTime, DateTime> selectedDates = await (Task<Tuple<DateTime, DateTime>>)methodInfo.Invoke(sectionRepo, parameters);
                Assert.IsNotNull(selectedDates);
                Assert.AreEqual(selectedDates.Item1, new DateTime(2012, 09, 01));
                Assert.AreEqual(selectedDates.Item2, new DateTime(2013, 05, 15));
            }

        }

        [TestClass]
        public class SectionRepository_GetSectionMidtermGradingCompleteAsync : SectionRepositoryTests
        {
            SectionRepository sectionRepo;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_GetSectionMidtermGradingCompleteAsync_SectionIdIsNull()
            {
                var entity = await sectionRepo.GetSectionMidtermGradingCompleteAsync(null);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionMidtermGradingCompleteAsync_NoStatuses()
            {
                // Test the logic that still returns an entity when there is no SEC.GRADING.STATUS record in the database.
                string sectionId = "123";
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<SecGradingStatus>("SEC.GRADING.STATUS", sectionId, It.IsAny<bool>())).ReturnsAsync(() => null);

                SectionMidtermGradingComplete entity = await sectionRepo.GetSectionMidtermGradingCompleteAsync(sectionId);

                Assert.AreEqual(entity.SectionId, sectionId);
                Assert.AreEqual(entity.MidtermGrading1Complete.Count, 0);
                Assert.AreEqual(entity.MidtermGrading2Complete.Count, 0);
                Assert.AreEqual(entity.MidtermGrading3Complete.Count, 0);
                Assert.AreEqual(entity.MidtermGrading4Complete.Count, 0);
                Assert.AreEqual(entity.MidtermGrading5Complete.Count, 0);
                Assert.AreEqual(entity.MidtermGrading6Complete.Count, 0);
            }

            [TestMethod]
            public async Task SectionRepository_GetSectionMidtermGradingCompleteAsync_FullData()
            {
                // Test that all six midterm grade status lists will be populated with multiple values in the database record.
                string sectionId = "123";

                // Setup a database record
                SecGradingStatus dbRecord = new SecGradingStatus();
                dbRecord.Recordkey = sectionId;
                dbRecord.SgsMidGrade1CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade1CmplOpers = new List<string>();
                dbRecord.SgsMidGrade1CmplTimes = new List<DateTime?>();
                dbRecord.SgsMidGrade2CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade2CmplOpers = new List<string>();
                dbRecord.SgsMidGrade2CmplTimes = new List<DateTime?>();
                dbRecord.SgsMidGrade3CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade3CmplOpers = new List<string>();
                dbRecord.SgsMidGrade3CmplTimes = new List<DateTime?>();
                dbRecord.SgsMidGrade4CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade4CmplOpers = new List<string>();
                dbRecord.SgsMidGrade4CmplTimes = new List<DateTime?>();
                dbRecord.SgsMidGrade5CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade5CmplOpers = new List<string>();
                dbRecord.SgsMidGrade5CmplTimes = new List<DateTime?>();
                dbRecord.SgsMidGrade6CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade6CmplOpers = new List<string>();
                dbRecord.SgsMidGrade6CmplTimes = new List<DateTime?>();

                dbRecord.SgsMidGrade1CmplDates.Add(new DateTime(2010, 1, 1));
                dbRecord.SgsMidGrade1CmplOpers.Add("Oper1");
                dbRecord.SgsMidGrade1CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 1));

                dbRecord.SgsMidGrade1CmplDates.Add(new DateTime(2010, 1, 2));
                dbRecord.SgsMidGrade1CmplOpers.Add("Oper2");
                dbRecord.SgsMidGrade1CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 2));

                dbRecord.SgsMidGrade2CmplDates.Add(new DateTime(2010, 1, 3));
                dbRecord.SgsMidGrade2CmplOpers.Add("Oper3");
                dbRecord.SgsMidGrade2CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 3));

                dbRecord.SgsMidGrade2CmplDates.Add(new DateTime(2010, 1, 4));
                dbRecord.SgsMidGrade2CmplOpers.Add("Oper4");
                dbRecord.SgsMidGrade2CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 4));

                dbRecord.SgsMidGrade3CmplDates.Add(new DateTime(2010, 1, 5));
                dbRecord.SgsMidGrade3CmplOpers.Add("Oper5");
                dbRecord.SgsMidGrade3CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 5));

                dbRecord.SgsMidGrade3CmplDates.Add(new DateTime(2010, 1, 6));
                dbRecord.SgsMidGrade3CmplOpers.Add("Oper6");
                dbRecord.SgsMidGrade3CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 6));

                dbRecord.SgsMidGrade4CmplDates.Add(new DateTime(2010, 1, 7));
                dbRecord.SgsMidGrade4CmplOpers.Add("Oper7");
                dbRecord.SgsMidGrade4CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 7));

                dbRecord.SgsMidGrade4CmplDates.Add(new DateTime(2010, 1, 8));
                dbRecord.SgsMidGrade4CmplOpers.Add("Oper8");
                dbRecord.SgsMidGrade4CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 8));

                dbRecord.SgsMidGrade5CmplDates.Add(new DateTime(2010, 1, 9));
                dbRecord.SgsMidGrade5CmplOpers.Add("Oper9");
                dbRecord.SgsMidGrade5CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 9));

                dbRecord.SgsMidGrade5CmplDates.Add(new DateTime(2010, 1, 10));
                dbRecord.SgsMidGrade5CmplOpers.Add("Oper10");
                dbRecord.SgsMidGrade5CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 10));

                dbRecord.SgsMidGrade6CmplDates.Add(new DateTime(2010, 1, 11));
                dbRecord.SgsMidGrade6CmplOpers.Add("Oper11");
                dbRecord.SgsMidGrade6CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 11));

                dbRecord.SgsMidGrade6CmplDates.Add(new DateTime(2010, 1, 12));
                dbRecord.SgsMidGrade6CmplOpers.Add("Oper12");
                dbRecord.SgsMidGrade6CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 12));

                dbRecord.buildAssociations();

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<SecGradingStatus>("SEC.GRADING.STATUS", sectionId, It.IsAny<bool>())).ReturnsAsync(dbRecord);

                SectionMidtermGradingComplete entity = await sectionRepo.GetSectionMidtermGradingCompleteAsync(sectionId);

                Assert.AreEqual(entity.SectionId, sectionId);
                Assert.AreEqual(entity.MidtermGrading1Complete.Count, 2);
                Assert.AreEqual(entity.MidtermGrading2Complete.Count, 2);
                Assert.AreEqual(entity.MidtermGrading3Complete.Count, 2);
                Assert.AreEqual(entity.MidtermGrading4Complete.Count, 2);
                Assert.AreEqual(entity.MidtermGrading5Complete.Count, 2);
                Assert.AreEqual(entity.MidtermGrading6Complete.Count, 2);

                // Test that each the date and time pairs in the database were converted to universal time
                DateTime? aDate;
                DateTime? aTime;

                aDate = new DateTime(2010, 1, 1);
                aTime = new DateTime(1900, 1, 1, 1, 1, 1);
                DateTimeOffset? Grade1_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 2);
                aTime = new DateTime(1900, 1, 1, 1, 1, 2);
                DateTimeOffset? Grade1_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 3);
                aTime = new DateTime(1900, 1, 1, 1, 1, 3);
                DateTimeOffset? Grade2_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 4);
                aTime = new DateTime(1900, 1, 1, 1, 1, 4);
                DateTimeOffset? Grade2_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 5);
                aTime = new DateTime(1900, 1, 1, 1, 1, 5);
                DateTimeOffset? Grade3_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 6);
                aTime = new DateTime(1900, 1, 1, 1, 1, 6);
                DateTimeOffset? Grade3_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 7);
                aTime = new DateTime(1900, 1, 1, 1, 1, 7);
                DateTimeOffset? Grade4_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 8);
                aTime = new DateTime(1900, 1, 1, 1, 1, 8);
                DateTimeOffset? Grade4_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 9);
                aTime = new DateTime(1900, 1, 1, 1, 1, 9);
                DateTimeOffset? Grade5_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 10);
                aTime = new DateTime(1900, 1, 1, 1, 1, 10);
                DateTimeOffset? Grade5_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 11);
                aTime = new DateTime(1900, 1, 1, 1, 1, 11);
                DateTimeOffset? Grade6_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 12);
                aTime = new DateTime(1900, 1, 1, 1, 1, 12);
                DateTimeOffset? Grade6_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);

                Assert.AreEqual(entity.MidtermGrading1Complete[0].DateAndTime, Grade1_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading1Complete[0].CompleteOperator, "Oper1");
                Assert.AreEqual(entity.MidtermGrading1Complete[1].DateAndTime, Grade1_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading1Complete[1].CompleteOperator, "Oper2");
                Assert.AreEqual(entity.MidtermGrading2Complete[0].DateAndTime, Grade2_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading2Complete[0].CompleteOperator, "Oper3");
                Assert.AreEqual(entity.MidtermGrading2Complete[1].DateAndTime, Grade2_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading2Complete[1].CompleteOperator, "Oper4");
                Assert.AreEqual(entity.MidtermGrading3Complete[0].DateAndTime, Grade3_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading3Complete[0].CompleteOperator, "Oper5");
                Assert.AreEqual(entity.MidtermGrading3Complete[1].DateAndTime, Grade3_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading3Complete[1].CompleteOperator, "Oper6");
                Assert.AreEqual(entity.MidtermGrading4Complete[0].DateAndTime, Grade4_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading4Complete[0].CompleteOperator, "Oper7");
                Assert.AreEqual(entity.MidtermGrading4Complete[1].DateAndTime, Grade4_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading4Complete[1].CompleteOperator, "Oper8");
                Assert.AreEqual(entity.MidtermGrading5Complete[0].DateAndTime, Grade5_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading5Complete[0].CompleteOperator, "Oper9");
                Assert.AreEqual(entity.MidtermGrading5Complete[1].DateAndTime, Grade5_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading5Complete[1].CompleteOperator, "Oper10");
                Assert.AreEqual(entity.MidtermGrading6Complete[0].DateAndTime, Grade6_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading6Complete[0].CompleteOperator, "Oper11");
                Assert.AreEqual(entity.MidtermGrading6Complete[1].DateAndTime, Grade6_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading6Complete[1].CompleteOperator, "Oper12");
            }
        }

        [TestClass]
        public class SectionRepository_PostSectionMidtermGradingCompleteAsync : SectionRepositoryTests
        {
            SectionRepository sectionRepo;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_PostSectionMidtermGradingCompleteAsync_SectionIdIsNull()
            {
                var entity = await sectionRepo.PostSectionMidtermGradingCompleteAsync(null, 1, "0012345", DateTimeOffset.Now);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_PostSectionMidtermGradingCompleteAsync_MidtermGradeNumberIsNull()
            {
                var entity = await sectionRepo.PostSectionMidtermGradingCompleteAsync("12345", null, "0012345", DateTimeOffset.Now);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionRepository_PostSectionMidtermGradingCompleteAsync_MidtermGradeNumberTooLow()
            {
                var entity = await sectionRepo.PostSectionMidtermGradingCompleteAsync("12345", 0, "0012345", DateTimeOffset.Now);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionRepository_PostSectionMidtermGradingCompleteAsync_MidtermGradeNumberTooHigh()
            {
                var entity = await sectionRepo.PostSectionMidtermGradingCompleteAsync("12345", 7, "0012345", DateTimeOffset.Now);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_PostSectionMidtermGradingCompleteAsync_CompleteOperatorIsNull()
            {
                var entity = await sectionRepo.PostSectionMidtermGradingCompleteAsync("12345", 1, null, DateTimeOffset.Now);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_PostSectionMidtermGradingCompleteAsync_DateAndTimeIsNull()
            {
                var entity = await sectionRepo.PostSectionMidtermGradingCompleteAsync("12345", 1, "0012345", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task SectionRepository_PostSectionMidtermGradingCompleteAsync_CTX_Exception()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<AddMidtermGradeCompleteRequest, AddMidtermGradeCompleteResponse>(It.IsAny<AddMidtermGradeCompleteRequest>())).ThrowsAsync(new Exception("CTX exception."));
                var entity = await sectionRepo.PostSectionMidtermGradingCompleteAsync("12345", 1, "0012345", DateTimeOffset.Now);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task SectionRepository_PostSectionMidtermGradingCompleteAsync_CTX_Error()
            {
                AddMidtermGradeCompleteResponse ctxResponse = new AddMidtermGradeCompleteResponse()
                {
                    AErrorMsg = "CTX error"
                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<AddMidtermGradeCompleteRequest, AddMidtermGradeCompleteResponse>(It.IsAny<AddMidtermGradeCompleteRequest>())).ReturnsAsync(ctxResponse);
                var entity = await sectionRepo.PostSectionMidtermGradingCompleteAsync("12345", 1, "0012345", DateTimeOffset.Now);
            }

            [TestMethod]
            public async Task SectionRepository_PostSectionMidtermGradingCompleteAsync_Valid()
            {
                // Test that all six midterm grade status lists will be populated with multiple values in the database record.
                string sectionId = "123";

                // Setup a database record
                SecGradingStatus dbRecord = new SecGradingStatus();
                dbRecord.Recordkey = sectionId;
                dbRecord.SgsMidGrade1CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade1CmplOpers = new List<string>();
                dbRecord.SgsMidGrade1CmplTimes = new List<DateTime?>();
                dbRecord.SgsMidGrade2CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade2CmplOpers = new List<string>();
                dbRecord.SgsMidGrade2CmplTimes = new List<DateTime?>();
                dbRecord.SgsMidGrade3CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade3CmplOpers = new List<string>();
                dbRecord.SgsMidGrade3CmplTimes = new List<DateTime?>();
                dbRecord.SgsMidGrade4CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade4CmplOpers = new List<string>();
                dbRecord.SgsMidGrade4CmplTimes = new List<DateTime?>();
                dbRecord.SgsMidGrade5CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade5CmplOpers = new List<string>();
                dbRecord.SgsMidGrade5CmplTimes = new List<DateTime?>();
                dbRecord.SgsMidGrade6CmplDates = new List<DateTime?>();
                dbRecord.SgsMidGrade6CmplOpers = new List<string>();
                dbRecord.SgsMidGrade6CmplTimes = new List<DateTime?>();

                dbRecord.SgsMidGrade1CmplDates.Add(new DateTime(2010, 1, 1));
                dbRecord.SgsMidGrade1CmplOpers.Add("Oper1");
                dbRecord.SgsMidGrade1CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 1));

                dbRecord.SgsMidGrade1CmplDates.Add(new DateTime(2010, 1, 2));
                dbRecord.SgsMidGrade1CmplOpers.Add("Oper2");
                dbRecord.SgsMidGrade1CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 2));

                dbRecord.SgsMidGrade2CmplDates.Add(new DateTime(2010, 1, 3));
                dbRecord.SgsMidGrade2CmplOpers.Add("Oper3");
                dbRecord.SgsMidGrade2CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 3));

                dbRecord.SgsMidGrade2CmplDates.Add(new DateTime(2010, 1, 4));
                dbRecord.SgsMidGrade2CmplOpers.Add("Oper4");
                dbRecord.SgsMidGrade2CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 4));

                dbRecord.SgsMidGrade3CmplDates.Add(new DateTime(2010, 1, 5));
                dbRecord.SgsMidGrade3CmplOpers.Add("Oper5");
                dbRecord.SgsMidGrade3CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 5));

                dbRecord.SgsMidGrade3CmplDates.Add(new DateTime(2010, 1, 6));
                dbRecord.SgsMidGrade3CmplOpers.Add("Oper6");
                dbRecord.SgsMidGrade3CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 6));

                dbRecord.SgsMidGrade4CmplDates.Add(new DateTime(2010, 1, 7));
                dbRecord.SgsMidGrade4CmplOpers.Add("Oper7");
                dbRecord.SgsMidGrade4CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 7));

                dbRecord.SgsMidGrade4CmplDates.Add(new DateTime(2010, 1, 8));
                dbRecord.SgsMidGrade4CmplOpers.Add("Oper8");
                dbRecord.SgsMidGrade4CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 8));

                dbRecord.SgsMidGrade5CmplDates.Add(new DateTime(2010, 1, 9));
                dbRecord.SgsMidGrade5CmplOpers.Add("Oper9");
                dbRecord.SgsMidGrade5CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 9));

                dbRecord.SgsMidGrade5CmplDates.Add(new DateTime(2010, 1, 10));
                dbRecord.SgsMidGrade5CmplOpers.Add("Oper10");
                dbRecord.SgsMidGrade5CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 10));

                dbRecord.SgsMidGrade6CmplDates.Add(new DateTime(2010, 1, 11));
                dbRecord.SgsMidGrade6CmplOpers.Add("Oper11");
                dbRecord.SgsMidGrade6CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 11));

                dbRecord.SgsMidGrade6CmplDates.Add(new DateTime(2010, 1, 12));
                dbRecord.SgsMidGrade6CmplOpers.Add("Oper12");
                dbRecord.SgsMidGrade6CmplTimes.Add(new DateTime(1, 1, 1, 1, 1, 12));

                dbRecord.buildAssociations();

                AddMidtermGradeCompleteResponse ctxResponse = new AddMidtermGradeCompleteResponse()
                {
                    AErrorMsg = null
                };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<AddMidtermGradeCompleteRequest, AddMidtermGradeCompleteResponse>(It.IsAny<AddMidtermGradeCompleteRequest>())).ReturnsAsync(ctxResponse);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<SecGradingStatus>("SEC.GRADING.STATUS", sectionId, It.IsAny<bool>())).ReturnsAsync(dbRecord);

                var entity = await sectionRepo.PostSectionMidtermGradingCompleteAsync(sectionId, 1, "0012345", DateTimeOffset.Now);

                Assert.AreEqual(entity.SectionId, sectionId);
                Assert.AreEqual(entity.MidtermGrading1Complete.Count, 2);
                Assert.AreEqual(entity.MidtermGrading2Complete.Count, 2);
                Assert.AreEqual(entity.MidtermGrading3Complete.Count, 2);
                Assert.AreEqual(entity.MidtermGrading4Complete.Count, 2);
                Assert.AreEqual(entity.MidtermGrading5Complete.Count, 2);
                Assert.AreEqual(entity.MidtermGrading6Complete.Count, 2);

                // Test that each the date and time pairs in the database were converted to universal time
                DateTime? aDate;
                DateTime? aTime;

                aDate = new DateTime(2010, 1, 1);
                aTime = new DateTime(1900, 1, 1, 1, 1, 1);
                DateTimeOffset? Grade1_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 2);
                aTime = new DateTime(1900, 1, 1, 1, 1, 2);
                DateTimeOffset? Grade1_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 3);
                aTime = new DateTime(1900, 1, 1, 1, 1, 3);
                DateTimeOffset? Grade2_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 4);
                aTime = new DateTime(1900, 1, 1, 1, 1, 4);
                DateTimeOffset? Grade2_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 5);
                aTime = new DateTime(1900, 1, 1, 1, 1, 5);
                DateTimeOffset? Grade3_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 6);
                aTime = new DateTime(1900, 1, 1, 1, 1, 6);
                DateTimeOffset? Grade3_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 7);
                aTime = new DateTime(1900, 1, 1, 1, 1, 7);
                DateTimeOffset? Grade4_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 8);
                aTime = new DateTime(1900, 1, 1, 1, 1, 8);
                DateTimeOffset? Grade4_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 9);
                aTime = new DateTime(1900, 1, 1, 1, 1, 9);
                DateTimeOffset? Grade5_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 10);
                aTime = new DateTime(1900, 1, 1, 1, 1, 10);
                DateTimeOffset? Grade5_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 11);
                aTime = new DateTime(1900, 1, 1, 1, 1, 11);
                DateTimeOffset? Grade6_1_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);
                aDate = new DateTime(2010, 1, 12);
                aTime = new DateTime(1900, 1, 1, 1, 1, 12);
                DateTimeOffset? Grade6_2_DateAndTime = aTime.ToPointInTimeDateTimeOffset(aDate, colleagueTimeZone);

                Assert.AreEqual(entity.MidtermGrading1Complete[0].DateAndTime, Grade1_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading1Complete[0].CompleteOperator, "Oper1");
                Assert.AreEqual(entity.MidtermGrading1Complete[1].DateAndTime, Grade1_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading1Complete[1].CompleteOperator, "Oper2");
                Assert.AreEqual(entity.MidtermGrading2Complete[0].DateAndTime, Grade2_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading2Complete[0].CompleteOperator, "Oper3");
                Assert.AreEqual(entity.MidtermGrading2Complete[1].DateAndTime, Grade2_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading2Complete[1].CompleteOperator, "Oper4");
                Assert.AreEqual(entity.MidtermGrading3Complete[0].DateAndTime, Grade3_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading3Complete[0].CompleteOperator, "Oper5");
                Assert.AreEqual(entity.MidtermGrading3Complete[1].DateAndTime, Grade3_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading3Complete[1].CompleteOperator, "Oper6");
                Assert.AreEqual(entity.MidtermGrading4Complete[0].DateAndTime, Grade4_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading4Complete[0].CompleteOperator, "Oper7");
                Assert.AreEqual(entity.MidtermGrading4Complete[1].DateAndTime, Grade4_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading4Complete[1].CompleteOperator, "Oper8");
                Assert.AreEqual(entity.MidtermGrading5Complete[0].DateAndTime, Grade5_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading5Complete[0].CompleteOperator, "Oper9");
                Assert.AreEqual(entity.MidtermGrading5Complete[1].DateAndTime, Grade5_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading5Complete[1].CompleteOperator, "Oper10");
                Assert.AreEqual(entity.MidtermGrading6Complete[0].DateAndTime, Grade6_1_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading6Complete[0].CompleteOperator, "Oper11");
                Assert.AreEqual(entity.MidtermGrading6Complete[1].DateAndTime, Grade6_2_DateAndTime);
                Assert.AreEqual(entity.MidtermGrading6Complete[1].CompleteOperator, "Oper12");
            }
        }

        [TestClass]
        public class SectionRepository_GetInstantEnrollmentSectionsAsync : SectionRepositoryTests
        {
            SectionRepository sectionRepo;
            Collection<RegUsers> regUsersCollection = new Collection<RegUsers>();
            Mock<IColleagueTransactionInvoker> mockManager;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();

                //mock datareader for STWEB.DEFAULTS
                var stWebDflt = BuildStwebDefaults(); //has regUserId = REGUSERID
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult((stWebDflt.Recordkey == id) ? stWebDflt : null)
                    );


                //mock datareader for bulk read REG.USERS
                regUsersCollection.Add(new RegUsers() { Recordkey = "REGUSERID", RguRegControls = new List<string>() { "REGCTLID-1", "REGCTLID-2" } });
                dataReaderMock.Setup<Task<Collection<RegUsers>>>(dr => dr.BulkReadRecordAsync<RegUsers>("REG.USERS", It.IsAny<string>(), true)).ReturnsAsync(new Collection<RegUsers>(regUsersCollection));

                //mock transaction invoker
                mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                GetIESectionsListResponse wlResp = new GetIESectionsListResponse() { SectionIds = new List<string>() { "1", "2" } };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetIESectionsListRequest, GetIESectionsListResponse>(It.IsAny<GetIESectionsListRequest>())).ReturnsAsync(wlResp);

                var sectionStatuses = new ApplValcodes();
                sectionStatuses.ValsEntityAssociation = new List<ApplValcodesVals>();
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("A", "Active", "1", "A", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("I", "Inactive", "2", "I", "", "", ""));
                sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Cancelled", "", "C", "", "", ""));
                dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", true)).Returns(Task.FromResult(sectionStatuses));

                //MOCK DATAREADER FOR reg.controls
                var regCtl1 = new RegControls();
                regCtl1.Recordkey = "REGCTLID-1";
                regCtl1.RgcSectionLookupCriteria = new List<string>() { "criteria-1", "criteria-2" };
                var regCtl2 = new RegControls();
                regCtl2.Recordkey = "REGCTLID-2";
                var regCtl3 = new RegControls();
                regCtl3.Recordkey = "REGCTLID-3";
                regCtl3.RgcSectionLookupCriteria = new List<string>() { "criteria-3" };
                dataReaderMock.Setup<Task<Collection<RegControls>>>(cacc => cacc.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true)).ReturnsAsync(new Collection<RegControls>() { regCtl1, regCtl2, regCtl3 });

                //mock datareader for select course.sections
                var sectionIdsFromLookupCriteria = new string[] { "1" };
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), "criteria-1 criteria-2")).ReturnsAsync(sectionIdsFromLookupCriteria);

                sectionIdsFromLookupCriteria = new string[] { "2" };
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), "criteria-3")).ReturnsAsync(sectionIdsFromLookupCriteria);


                var csData = new Collection<CourseSections>()
                {
                    new CourseSections()
                    {
                        Recordkey = "1",
                        SecCourse = "1",
                        SecNo = "01",
                        SecStartDate = DateTime.Today.AddDays(30),
                        SecMinCred = 3m,
                        SecCeus = null,
                        SecShortTitle = "Sec 1",
                        SecCredType = "IN",
                        SecPortalSite = string.Empty,
                        SecStatusesEntityAssociation = new List<CourseSectionsSecStatuses>()
                        {
                            new CourseSectionsSecStatuses(DateTime.Today, "A")
                        },
                        SecDepartmentsEntityAssociation = new List<CourseSectionsSecDepartments>()
                        {
                            new CourseSectionsSecDepartments("BUSN", 100M)
                        },
                        SecCourseLevels = new List<string>() { "100" },
                        SecAcadLevel = "CE"
                    },
                    new CourseSections()
                    {
                        Recordkey = "2",
                        SecCourse = "1",
                        SecNo = "02",
                        SecStartDate = DateTime.Today.AddDays(30),
                        SecMinCred = 3m,
                        SecCeus = null,
                        SecShortTitle = "Sec 2",
                        SecCredType = "IN",
                        SecPortalSite = string.Empty,
                        SecStatusesEntityAssociation = new List<CourseSectionsSecStatuses>()
                        {
                            new CourseSectionsSecStatuses(DateTime.Today, "A")
                        },
                        SecDepartmentsEntityAssociation = new List<CourseSectionsSecDepartments>()
                        {
                            new CourseSectionsSecDepartments("BUSN", 100M)
                        },
                        SecCourseLevels = new List<string>() { "100" },
                        SecAcadLevel = "CE"
                    }
                };
                var bulkReadOutput = new BulkReadOutput<CourseSections>()
                {
                    BulkRecordsRead = csData,
                    InvalidKeys = null,
                    InvalidRecords = null
                };
                dataReaderMock.Setup(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CourseSections>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(bulkReadOutput);

                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueSessionExpiredException))]
            public async Task SectionRepository_GetInstantEnrollmentSectionsAsync_RepositoryThrowsColleagueExpiredException()
            {
                dataReaderMock.Setup(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CourseSections>(
                    It.IsAny<string[]>(), It.IsAny<bool>())).Returns(() =>
                    {
                        throw new ColleagueSessionExpiredException("session timeout");
                    });

                await sectionRepo.GetInstantEnrollmentSectionsAsync();
            }

            [TestMethod]
            public async Task IE_SectionFromCTX_MatchesWithRegControl()
            {
                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.IsNotNull(sections);
                Assert.AreEqual(2, sections.Count());
                Assert.AreEqual("1", sections.ToList()[0].Id);
                Assert.AreEqual("2", sections.ToList()[1].Id);
            }

            //when REG CONTROL criteria does not work on sections from CTX
            [TestMethod]
            public async Task IE_SectionFromCTX_DoesNotMatchesWithRegControl()
            {
                var sectionIdsFromLookupCriteria = new string[] { };
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sectionIdsFromLookupCriteria);

                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.IsNotNull(sections);
                Assert.AreEqual(0, sections.Count());
            }

            //When REG CONTROL CRITERIA RETURNS null
            [TestMethod]
            public async Task IE_SectionFromCTX_DoesNotMatchesWithRegControl_AndReturnsNull()
            {
                string[] sectionIdsFromLookupCriteria = null;
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sectionIdsFromLookupCriteria);

                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.AreEqual(0, sections.Count());
            }

            //When CTX return null sections
            [TestMethod]
            public async Task IE_SectionsFromCTX_IsNull()
            {
                GetIESectionsListResponse wlResponse = new GetIESectionsListResponse() { SectionIds = null };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetIESectionsListRequest, GetIESectionsListResponse>(It.IsAny<GetIESectionsListRequest>())).ReturnsAsync(wlResponse);
                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.IsNotNull(sections);
                Assert.AreEqual(0, sections.Count());
            }

            //When CTX return empty sections
            [TestMethod]
            public async Task IE_SectionsFromCTX_IsEmpty()
            {
                GetIESectionsListResponse wlResp = new GetIESectionsListResponse() { SectionIds = new List<string>() };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetIESectionsListRequest, GetIESectionsListResponse>(It.IsAny<GetIESectionsListRequest>())).ReturnsAsync(wlResp);
                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.IsNotNull(sections);
                Assert.AreEqual(0, sections.Count());
            }

            //When CTX returns empty sections but reguser returns more
            [TestMethod]
            public async Task IE_EmptySectionsFromCTX_regUserHaveSections()
            {
                GetIESectionsListResponse wlResponse = new GetIESectionsListResponse() { SectionIds = new List<string>() };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetIESectionsListRequest, GetIESectionsListResponse>(It.IsAny<GetIESectionsListRequest>())).ReturnsAsync(wlResponse);
                string[] sectionIdsFromLookupCriteria = new string[] { "4", "5" };
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sectionIdsFromLookupCriteria);
                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.IsNotNull(sections);
                Assert.AreEqual(0, sections.Count());

            }

            //When CTX return sections but reguser have null sectionss
            [TestMethod]
            public async Task IE_SectionsFromCTX_RegUsersHaveSections_Is_Null()
            {
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                string[] sectionIdsFromLookupCriteria = null;
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sectionIdsFromLookupCriteria);
                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.IsNotNull(sections);
                Assert.AreEqual(0, sections.Count());
            }

            //When CTX  returns  sections but reguser is empty
            [TestMethod]
            public async Task IE_SectionsFromCTX_RegUsersHaveEmptySections()
            {
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                string[] sectionIdsFromLookupCriteria = new string[] { };
                dataReaderMock.Setup(acc => acc.SelectAsync("COURSE.SECTIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(sectionIdsFromLookupCriteria);
                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.IsNotNull(sections);
                Assert.AreEqual(0, sections.Count());
            }

            //When CTX returns sections but there is no REG.USER on CEWP (nameless is used)
            [TestMethod]
            public async Task IE_NoRegUsers_NamelessUsed()
            {

                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.Recordkey = "STWEB.DEFAULTS";
                stwebDefaults.StwebRegUsersId = "";
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult(stwebDefaults)
                    );
                regUsersCollection.Clear();
                regUsersCollection.Add(new RegUsers() { Recordkey = "NAMELESS", RguRegControls = new List<string>() { "REGCTLID-3" } });
                dataReaderMock.Setup<Task<Collection<RegUsers>>>(dr => dr.BulkReadRecordAsync<RegUsers>("REG.USERS", It.IsAny<string>(), true)).ReturnsAsync(new Collection<RegUsers>(regUsersCollection));
                //validate REG.USERS is queried with NAMELESS REG.USER.ID and sections are returned for NAMELESS criteria
                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.IsNotNull(sections);
                Assert.AreEqual(2, sections.Count());
                Assert.AreEqual("2", sections.ToList()[1].Id);
            }

            //When REG.USERS record does not exist
            [TestMethod]
            public async Task IE_NoRegUsersRecordExist_ReturnsNull()
            {

                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.Recordkey = "STWEB.DEFAULTS";
                stwebDefaults.StwebRegUsersId = "DoesNotExist";
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult(stwebDefaults)
                    );
                //When no REG.USERS record, assume datareader returns null
                dataReaderMock.Setup<Task<Collection<RegUsers>>>(dr => dr.BulkReadRecordAsync<RegUsers>("REG.USERS", It.IsAny<string>(), true)).ReturnsAsync(() => null);

                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.IsNotNull(sections);
                Assert.AreEqual(2, sections.Count());
                Assert.AreEqual("1", sections.ToList()[0].Id);
                Assert.AreEqual("2", sections.ToList()[1].Id);
            }

            //When REG.USERS record exist but there are no corresponding REG.CONTROL.
            [TestMethod]
            public async Task IE_RegControls_isNull()
            {
                dataReaderMock.Setup<Task<Collection<RegControls>>>(cacc => cacc.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true)).ReturnsAsync(() => null);

                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.AreEqual(2, sections.Count());
                Assert.AreEqual("1", sections.ToList()[0].Id);
                Assert.AreEqual("2", sections.ToList()[1].Id);
            }

            //When  REG.CONTROL exist but there is no corresponding lookup criteria
            [TestMethod]
            public async Task IE_RegControlsExist_NoLookupCriteria()
            {

                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.Recordkey = "STWEB.DEFAULTS";
                stwebDefaults.StwebRegUsersId = "WEBREG";
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>(
                    (param, id, repl) => Task.FromResult(stwebDefaults)
                    );

                regUsersCollection.Clear();
                regUsersCollection.Add(new RegUsers() { Recordkey = "WEBREG", RguRegControls = new List<string>() { "REGCTLID-2" } });
                dataReaderMock.Setup<Task<Collection<RegUsers>>>(dr => dr.BulkReadRecordAsync<RegUsers>("REG.USERS", It.IsAny<string>(), true)).ReturnsAsync(new Collection<RegUsers>(regUsersCollection));

                IEnumerable<Section> sections = await sectionRepo.GetInstantEnrollmentSectionsAsync();
                Assert.AreEqual(2, sections.Count());
                Assert.AreEqual("1", sections.ToList()[0].Id);
                Assert.AreEqual("2", sections.ToList()[1].Id);
            }
        }


        [TestClass]
        public class SectionRepository_SectionCensusCertification : SectionRepository_GeneralTests
        {
            [TestInitialize]
            public new void Initialize()
            {
                base.Initialize();
                //modify course sections data contract collection here- sectionResponseData is DataContract returned after COURSE.SECTIONS bulkread mock
                sectionsResponseData[0].SecCertCensusEntityAssociation = new List<CourseSectionsSecCertCensus>();

                //one of the COURSE.SECTION have null certs
                sectionsResponseData[1].SecCertCensusEntityAssociation = null;

                //COURSE.SECTION have certs with different types of data
                sectionsResponseData[2].SecCertCensusEntityAssociation = new List<CourseSectionsSecCertCensus>();
                sectionsResponseData[2].SecCertCensusEntityAssociation.Add(new CourseSectionsSecCertCensus());
                sectionsResponseData[2].SecCertCensusEntityAssociation.Add(new CourseSectionsSecCertCensus(null, null, null, null, null, null));
                sectionsResponseData[2].SecCertCensusEntityAssociation.Add(new CourseSectionsSecCertCensus(new DateTime(2021, 01, 01), new DateTime(2021, 01, 02), new DateTime(2021, 01, 02, 12, 20, 30), "personid", "1", "after a month completion"));
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_WhenCensusIsEmpty()
            {
                //this returns sections from mocked section repo. 
                var sections = (await sectionRepo.GetRegistrationSectionsAsync(registrationTerms)).ToList();

                Assert.AreEqual(sectionsResponseData[0].Recordkey, sections[0].Id);
                Assert.AreEqual(0, sections[0].SectionCertifiedCensuses.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_WhenCensusIsNull_InDataContract()
            {
                //this returns sections from mocked section repo. 
                var sections = (await sectionRepo.GetRegistrationSectionsAsync(registrationTerms)).ToList();

                Assert.AreEqual(sectionsResponseData[1].Recordkey, sections[1].Id);
                Assert.IsNotNull(sections[1].SectionCertifiedCensuses);
                Assert.AreEqual(0, sections[1].SectionCertifiedCensuses.Count());
            }

            [TestMethod]
            public async Task SectionRepository_GetRegistrationSections_WhenCensusIsNotNull()
            {
                //this returns sections from mocked section repo. 
                var sections = (await sectionRepo.GetRegistrationSectionsAsync(registrationTerms)).ToList();

                Assert.AreEqual(sectionsResponseData[2].Recordkey, sections[2].Id);
                Assert.IsNotNull(sections[2].SectionCertifiedCensuses);
                //data contract has 3
                Assert.AreEqual(3, sectionsResponseData[2].SecCertCensusEntityAssociation.Count());
                //but when data contract is mapped to entity, null census dates are ignored
                Assert.AreEqual(1, sections[2].SectionCertifiedCensuses.Count());

                //only non-nulls are stored
                Assert.AreEqual(sectionsResponseData[2].SecCertCensusEntityAssociation[2].SecCertCensusDatesAssocMember, sections[2].SectionCertifiedCensuses[0].CensusCertificationDate);
                Assert.AreEqual(sectionsResponseData[2].SecCertCensusEntityAssociation[2].SecCertRecordedDatesAssocMember, sections[2].SectionCertifiedCensuses[0].CensusCertificationRecordedDate);
                Assert.AreEqual(sectionsResponseData[2].SecCertCensusEntityAssociation[2].SecCertRecordedTimesAssocMember, sections[2].SectionCertifiedCensuses[0].CensusCertificationRecordedTime);
                Assert.AreEqual(sectionsResponseData[2].SecCertCensusEntityAssociation[2].SecCertPositionsAssocMember, sections[2].SectionCertifiedCensuses[0].CensusCertificationPosition);
                Assert.AreEqual(sectionsResponseData[2].SecCertCensusEntityAssociation[2].SecCertPositionLabelsAssocMember, sections[2].SectionCertifiedCensuses[0].CensusCertificationLabel);
                Assert.AreEqual(sectionsResponseData[2].SecCertCensusEntityAssociation[2].SecCertPersonIdsAssocMember, sections[2].SectionCertifiedCensuses[0].PersonId);
            }
        }

        [TestClass]
        //inheriting from existing test class because that class already mocks for GetSection for the given section id
        public class SectionRepository_UpdateSectionCensusCertification : SectionRepository_TestAllFields
        {
            string sectionId = string.Empty;
            Mock<IColleagueTransactionInvoker> mockManager = new Mock<IColleagueTransactionInvoker>();
            UpdateSectionCensusCertificationResponse sectionCensusResponse = new UpdateSectionCensusCertificationResponse();
            Section sectionToUse = null;

            [TestInitialize]
            public new void Initialize()
            {
                base.Initialize();
                //both these variables are populated from base class Initialize method
                //csId is section Id declare in base test class
                sectionId = csId;
                //result is Section entity declared in base test class
                sectionToUse = result;

                //section census certified response from CTX
                sectionCensusResponse.AError = false;
                sectionCensusResponse.AlErrorMsgs = null;

                //update COURSE.SECTIONS to have census certified dates

                cs.SecCertCensusEntityAssociation = new List<CourseSectionsSecCertCensus>()
                {
                    new CourseSectionsSecCertCensus(new DateTime(2021, 01, 02),null, null, "personId", "1","something"),
                    new CourseSectionsSecCertCensus(new DateTime(2021, 02, 02), new DateTime(2021, 02, 03), new DateTime(2021, 02, 03, 04, 05, 06), "personId-2", "2","something-2")
                };

                MockRecordAsync<CourseSections>("COURSE.SECTIONS", cs, cs.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRepository_UpdateSectionCensusCertification_SectionId_IsNull()
            {
                var sections = await sectionRepo.CreateSectionCensusCertificationAsync(null, new DateTime(2021, 01, 02), "1", "final test", DateTime.Today, DateTime.Now, "personId");
            }

            //when ctx returns error messages
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_UpdateSectionCensusCertification_CTX_ReturnsErrorMessages()
            {
                sectionCensusResponse.AError = true;
                sectionCensusResponse.AlErrorMsgs = new List<string>() { "error-msg-1", "error-msg-2" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateSectionCensusCertificationRequest, UpdateSectionCensusCertificationResponse>(It.IsAny<UpdateSectionCensusCertificationRequest>())).ReturnsAsync(sectionCensusResponse);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                var certifiedCensus = await sectionRepo.CreateSectionCensusCertificationAsync(sectionId, new DateTime(2021, 01, 02), "1", "something", DateTime.Today, DateTime.Now, "personId");
                loggerMock.Verify(l => l.Error(string.Format("Error returned by CTX for the section {0} while updating certification for Census position {1}", sectionId, "1")));
            }

            //when ctx return error message flag but no errors
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_UpdateSectionCensusCertification_CTX_ReturnsNoErrorMessages_ButErrorFlagIsTrue()
            {
                sectionCensusResponse.AError = true;
                sectionCensusResponse.AlErrorMsgs = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateSectionCensusCertificationRequest, UpdateSectionCensusCertificationResponse>(It.IsAny<UpdateSectionCensusCertificationRequest>())).ReturnsAsync(sectionCensusResponse);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                var certifiedCensus = await sectionRepo.CreateSectionCensusCertificationAsync(sectionId, new DateTime(2021, 01, 02), "1", "something", DateTime.Today, DateTime.Now, "personId");
                loggerMock.Verify(l => l.Error(string.Format("Error Flag returned by CTX for the section {0} while updating certification for Census position {1} is true but the errors collection does not have any errors returned ", sectionId, "1")));
            }

            //when ctx return error message flag is empty but no errors
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task SectionRepository_UpdateSectionCensusCertification_CTX_ReturnsEmptyErrorMessages_ButErrorFlagIsTrue()
            {
                sectionCensusResponse.AError = true;
                sectionCensusResponse.AlErrorMsgs = new List<string>();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateSectionCensusCertificationRequest, UpdateSectionCensusCertificationResponse>(It.IsAny<UpdateSectionCensusCertificationRequest>())).ReturnsAsync(sectionCensusResponse);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                var sectionCensusCertified = await sectionRepo.CreateSectionCensusCertificationAsync(sectionId, new DateTime(2021, 01, 02), "1", "something", DateTime.Today, DateTime.Now, "personId");
                loggerMock.Verify(l => l.Error(string.Format("Error Flag returned by CTX for the section {0} while updating certification for Census position {1} is true but the errors collection does not have any errors returned ", sectionId, "1")));
            }

            //when census exist for census date and  position but not certified
            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task SectionRepository_UpdateSectionCensusCertification_CTX_Success_But_Census_Missing()
            {
                sectionCensusResponse.AError = false;
                sectionCensusResponse.AlErrorMsgs = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateSectionCensusCertificationRequest, UpdateSectionCensusCertificationResponse>(It.IsAny<UpdateSectionCensusCertificationRequest>())).ReturnsAsync(sectionCensusResponse);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                var sectionCensusCertified = await sectionRepo.CreateSectionCensusCertificationAsync(sectionId, new DateTime(2021, 01, 02), "1", "something", DateTime.Today, DateTime.Now, "personId");
                loggerMock.Verify(l => l.Error(string.Format("There is no certified census for the section {0} for the date {1} at position {2}. ", sectionId, new DateTime(2021, 01, 02), "1")));
            }

            //when census exist for census date and  position but not certified
            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task SectionRepository_UpdateSectionCensusCertification_CTX_Success_But_Census_Not_Certified()
            {
                sectionCensusResponse.AError = false;
                sectionCensusResponse.AlErrorMsgs = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateSectionCensusCertificationRequest, UpdateSectionCensusCertificationResponse>(It.IsAny<UpdateSectionCensusCertificationRequest>())).ReturnsAsync(sectionCensusResponse);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettings);
                var sectionCensusCertified = await sectionRepo.CreateSectionCensusCertificationAsync(sectionId, new DateTime(2021, 01, 02), "1", "something", DateTime.Today, DateTime.Now, "personId");
                loggerMock.Verify(l => l.Error(string.Format("Census for the section {0} for the date {1} at position {2} couldn't be certified.", sectionId, new DateTime(2021, 01, 02), "1")));
            }

            //when section retrieved after return from CTX have census certified data
            [TestMethod]
            public async Task SectionRepository_UpdateSectionCensusCertification_CTX_Success_Census_Certified()
            {
                sectionCensusResponse.AError = false;
                sectionCensusResponse.AlErrorMsgs = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateSectionCensusCertificationRequest, UpdateSectionCensusCertificationResponse>(It.IsAny<UpdateSectionCensusCertificationRequest>())).ReturnsAsync(sectionCensusResponse);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                sectionRepo = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new Ellucian.Colleague.Domain.Student.Tests.TestTermRepository(), apiSettings);
                var sectionCensusCertified = await sectionRepo.CreateSectionCensusCertificationAsync(sectionId, new DateTime(2021, 02, 02), "2", "something-2", new DateTime(2021, 02, 03), new DateTime(2021, 02, 03, 04, 05, 06), "personId-2");
                Assert.IsNotNull(sectionCensusCertified);
                Assert.AreEqual(new DateTime(2021, 02, 02), sectionCensusCertified.CensusCertificationDate);
                Assert.AreEqual("personId-2", sectionCensusCertified.PersonId);
                Assert.AreEqual("2", sectionCensusCertified.CensusCertificationPosition);
                Assert.AreEqual("something-2", sectionCensusCertified.CensusCertificationLabel);
                Assert.AreEqual(new DateTime(2021, 02, 03), sectionCensusCertified.CensusCertificationRecordedDate);
                Assert.AreEqual(new DateTime(2021, 02, 03, 04, 05, 06), sectionCensusCertified.CensusCertificationRecordedTime);
            }
        }

        [TestClass]
        public class DepartmentalOversightRepository_SearchSectionByName
        {
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<IColleagueDataReader> anonymousDataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;
            protected IEnumerable<Colleague.Domain.Student.Entities.Term> terms;
            public IEnumerable<DeptOversightSearchResult> searchResults;
            ApiSettings apiSettingsMock;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            CourseSections cs;
            string csId = "12345";

            SectionRepository sectionRepository;
            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataReaderMock = new Mock<IColleagueDataReader>();
                // Logger mock
                loggerMock = new Mock<ILogger>();
                // Set up transaction manager for mocking 
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                apiSettingsMock = new ApiSettings("null");
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                cs = new CourseSections()
                {
                    RecordGuid = "23033dc3-06fc-4111-b910-77050b45cbe1",
                    Recordkey = csId,
                    RecordModelName = "sections",
                    SecAcadLevel = "UG",
                    SecActiveStudents = new List<string>(),
                    SecAllowAuditFlag = "N",
                    SecAllowPassNopassFlag = "N",
                    SecAllowWaitlistFlag = "Y",
                    SecBookOptions = new List<string>() { "R", "O" },
                    SecBooks = new List<string>() { "Book 1", "Book 2" },
                    SecCapacity = 30,
                    SecCeus = null,
                    SecCloseWaitlistFlag = "Y",
                    SecCourse = "210",
                    SecCourseLevels = new List<string>() { "100" },
                    SecCourseTypes = new List<string>() { "STND", "HONOR" },
                    SecCredType = "IN",
                    SecEndDate = new DateTime(2014, 12, 15),
                    SecFaculty = new List<string>(),
                    SecFacultyConsentFlag = "Y",
                    SecGradeScheme = "UGR",
                    SecInstrMethods = new List<string>() { "LEC", "LAB" },
                    SecLocation = "MAIN",
                    SecMaxCred = 6m,
                    SecMeeting = new List<string>(),
                    SecMinCred = 3m,
                    SecName = "MATH-4350-01",
                    SecNo = "01",
                    SecNoWeeks = 10,
                    SecOnlyPassNopassFlag = "N",
                    SecPortalSite = csId,
                    SecShortTitle = "Statistics",
                    SecStartDate = DateTime.Today.AddDays(-10),
                    SecTerm = "2014/FA",
                    SecTopicCode = "ABC",
                    SecVarCredIncrement = 1m,
                    SecWaitlistMax = 10,
                    SecWaitlistRating = "SR",
                    SecXlist = null,
                    SecFirstMeetingDate = new DateTime(2015, 10, 25),
                    SecLastMeetingDate = new DateTime(2017, 01, 02),
                    SecLearningProvider = "MOODLE",
                    SecSynonym = "0002334"
                };
                cs.SecEndDate = cs.SecStartDate.Value.AddDays(69);
                cs.SecContactEntityAssociation = new List<CourseSectionsSecContact>();
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LEC", 20.00m, 45.00m, "T", 37.50m));
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LAB", 10.00m, 15.00m, "T", 45.00m));
                cs.SecDepartmentsEntityAssociation = new List<CourseSectionsSecDepartments>();
                cs.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("MATH", 75m));
                cs.SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments("PSYC", 25m));
                cs.SecStatusesEntityAssociation = new List<CourseSectionsSecStatuses>();
                cs.SecStatusesEntityAssociation.Add(new CourseSectionsSecStatuses(new DateTime(2001, 5, 15), "A"));
                // Instr methods association - instructional method and load
                cs.SecContactEntityAssociation = new List<CourseSectionsSecContact>();
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LEC", 20.00m, 0m, "", 0m));
                cs.SecContactEntityAssociation.Add(new CourseSectionsSecContact("LAB", 10.00m, 0m, "", 0m));
                // Pointer to CourseSecFaculty
                cs.SecFaculty.Add("1");
                // Pointer to CourseSecMeeting
                cs.SecMeeting.Add("1");


                // Build the test repository
                sectionRepository = new SectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new TestTermRepository(), apiSettingsMock);
                terms = new List<Colleague.Domain.Student.Entities.Term>()
                {
                     new Colleague.Domain.Student.Entities.Term("T1", "T1", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false),
                     new Colleague.Domain.Student.Entities.Term("T2", "T2", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false),
                     new Colleague.Domain.Student.Entities.Term("T3", "T3", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false)
                };
                searchResults = new List<DeptOversightSearchResult>();
                ICollection<OfferingDepartment> depts = new List<OfferingDepartment>() { new OfferingDepartment("D1", 50m), new OfferingDepartment("D2", 50m) };
                ICollection<string> clcs = new List<string>() { "100", "200", "300", "400" };
                List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
                section1 = new Domain.Student.Entities.Section("11", "11", "01", new DateTime(2011, 1, 1), 3, null, "Section1 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section1.TermId = "T1"; section1.EndDate = new DateTime(2011, 1, 20); section1.AddFaculty("0000001");

                section2 = new Domain.Student.Entities.Section("12", "11", "02", new DateTime(2011, 2, 1), 3, null, "Section2 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section2.TermId = "T2"; section2.EndDate = new DateTime(2011, 2, 20); section2.AddFaculty("0000001");

                section3 = new Domain.Student.Entities.Section("13", "12", "01", new DateTime(2011, 3, 1), 3, null, "Section3 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section3.TermId = "T3"; section3.EndDate = new DateTime(2011, 3, 20); section3.AddFaculty("0000002");
            }

            [TestCleanup]
            public void TestCleanup()
            {
                sectionRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SearchSectionByNameAsync_ThrowArgumentNullExceptionNull()
            {
                var result = await sectionRepository.GetDeptOversightSectionDetails(null, new List<Term>(), new List<string>());
            }
        }

        #region Private helper methods - static so they can be used in any of the above subclasses

        private static void BuildLdmConfiguration(Mock<IColleagueDataReader> dataReaderMock, out CdDefaults cdDefaults)
        {
            cdDefaults = BuildCdDefaults();

            var sectionStatuses = new ApplValcodes();
            sectionStatuses.ValsEntityAssociation = new List<ApplValcodesVals>();
            sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("A", "Active", "1", "A", "", "", ""));
            sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("I", "Inactive", "2", "I", "", "", ""));
            sectionStatuses.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Cancelled", "", "C", "", "", ""));

            // Set up repo response for waitlist statuses
            var waitlistCodeResponse = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValExternalRepresentationAssocMember = "Active", ValActionCode1AssocMember = "1" },
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValExternalRepresentationAssocMember = "Enrolled", ValActionCode1AssocMember = "2"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "D", ValExternalRepresentationAssocMember = "Dropped", ValActionCode1AssocMember = "3"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "P", ValExternalRepresentationAssocMember = "Permission to Register", ValActionCode1AssocMember = "4"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValExternalRepresentationAssocMember = "Expired", ValActionCode1AssocMember = "5"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "C", ValExternalRepresentationAssocMember = "Cancelled", ValActionCode1AssocMember = "6"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "L", ValExternalRepresentationAssocMember = "Closed", ValActionCode1AssocMember = "7"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "OS", ValExternalRepresentationAssocMember = "Other Section Enrollment", ValActionCode1AssocMember = "8"}
                }
            };

            dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", true)).Returns(Task.FromResult(sectionStatuses));
            dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "WAIT.LIST.STATUSES", true)).Returns(Task.FromResult(waitlistCodeResponse));

            dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(rkla =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var rkl in rkla)
                    {
                        result.Add(rkl.ResultKey, new RecordKeyLookupResult() { Guid = Guid.NewGuid().ToString().ToLowerInvariant() });
                    }
                    return Task.FromResult(result);
                });
            // Mock the read of instructional methods


            InstrMethods lec = new InstrMethods()
            {
                InmDesc = "LEC",
                InmOnline = "N",
                RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed08",
                Recordkey = "LEC"
            };
            InstrMethods lab = new InstrMethods()
            {
                InmDesc = "LAB",
                InmOnline = "N",
                RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed09",
                Recordkey = "LAB"
            };
            InstrMethods onl = new InstrMethods()
            {
                InmDesc = "ONL",
                InmOnline = "Y",
                RecordGuid = "8f9e26e6-6fa3-4764-885b-542f7daaed10",
                Recordkey = "ONL"
            };
            dataReaderMock.Setup<Task<Collection<InstrMethods>>>(acc => acc.BulkReadRecordAsync<InstrMethods>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<Collection<InstrMethods>>(new Collection<InstrMethods>()
                    {
                        lec,lab,onl
                    }));
        }

        private static CdDefaults BuildCdDefaults()
        {
            var cdDefaults = new CdDefaults()
            {
                CdAllowAuditFlag = "Y",
                CdAllowPassNopassFlag = "Y",
                CdAllowWaitlistFlag = "Y",
                CdCourseDelimiter = "-",
                CdFacultyConsentFlag = "N",
                CdInstrMethods = "LEC",
                CdOnlyPassNopassFlag = "N",
                CdReqsConvertedFlag = "N",
                CdWaitlistRating = "SR"
            };
            return cdDefaults;
        }

        private static StwebDefaults BuildStwebDefaults()
        {
            string template = "abc?a={4}&b={5}&c={0}&d={1}&e={3}&f={2}&a={4}&b={5}&c={0}&d={1}&e={3}&f={2}";
            StwebDefaults stwebDefaults = new StwebDefaults();
            stwebDefaults.Recordkey = "STWEB.DEFAULTS";
            stwebDefaults.StwebBookstoreUrlTemplate = template;
            stwebDefaults.StwebRegUsersId = "REGUSERID";
            return stwebDefaults;
        }

        private static CourseSecMeeting BuildCourseSecMeetingDefaults()
        {
            CourseSecMeeting courseSecMeet = new CourseSecMeeting();
            courseSecMeet.Recordkey = "12345";
            courseSecMeet.CsmInstrMethod = "LAB";
            return courseSecMeet;
        }

        private static Collection<InstrMethods> BuildValidInstrMethodResponse()
        {
            var instructionalMethods = new Collection<InstrMethods>();
            instructionalMethods.Add(new InstrMethods() { RecordGuid = Guid.NewGuid().ToString(), Recordkey = "LEC", InmDesc = "Lecture", InmOnline = "" });
            instructionalMethods.Add(new InstrMethods() { RecordGuid = Guid.NewGuid().ToString(), Recordkey = "LAB", InmDesc = "Lab", InmOnline = "N" });
            instructionalMethods.Add(new InstrMethods() { RecordGuid = Guid.NewGuid().ToString(), Recordkey = "ONL", InmDesc = "Online", InmOnline = "Y" });
            return instructionalMethods;
        }

        private static string ConvertSectionStatusToCode(SectionStatus sectionStatus)
        {
            switch (sectionStatus)
            {
                case SectionStatus.Active:
                    return "A";
                case SectionStatus.Cancelled:
                    return "C";
                case SectionStatus.Inactive:
                default:
                    return "I";
            }
        }

        private static int CountMeetingDaysPerWeek(CourseSecMeeting meeting)
        {
            int count = 0;
            if (meeting.CsmSunday == "Y") count += 1;
            if (meeting.CsmMonday == "Y") count += 1;
            if (meeting.CsmTuesday == "Y") count += 1;
            if (meeting.CsmWednesday == "Y") count += 1;
            if (meeting.CsmThursday == "Y") count += 1;
            if (meeting.CsmFriday == "Y") count += 1;
            if (meeting.CsmSaturday == "Y") count += 1;
            return count;
        }

        private static ApplValcodes BuildValcodeResponse(IEnumerable<CourseType> courseTypes)
        {
            ApplValcodes courseTypesResponse = new ApplValcodes();
            courseTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
            foreach (var item in courseTypes)
            {
                courseTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, item.Categorization, item.Code, "3", "", ""));
            }
            return courseTypesResponse;
        }
        private static Dictionary<string, RecordKeyLookupResult> BuildCourseTypeGuidResponse(IEnumerable<CourseType> courseTypes)
        {
            Dictionary<string, RecordKeyLookupResult> courseTypesDictionary = new Dictionary<string, RecordKeyLookupResult>();

            foreach (var item in courseTypes)
            {
                // will assume no guids are found

            }
            return courseTypesDictionary;
        }

        private static void BuildValidReferenceDataRepository(IEnumerable<CourseType> allCourseTypes, ApplValcodes courseTypeValcodeResponse)
        {
            // transaction factory mock
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();
            // Cache Provider Mock
            var cacheProviderMock = new Mock<ICacheProvider>();
            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>();

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Setup response to assessmentSpecialCircumstane valcode read
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", It.IsAny<bool>())).ReturnsAsync(courseTypeValcodeResponse);
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
            {
                var result = new Dictionary<string, RecordKeyLookupResult>();
                foreach (var recordKeyLookup in recordKeyLookups)
                {
                    var courseType = allCourseTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                    result.Add(string.Join("+", new string[] { "ST.VALCODES", "COURSE.TYPES", courseType.Code }),
                        new RecordKeyLookupResult() { Guid = courseType.Guid });
                }
                return Task.FromResult(result);
            });


        }
        #endregion
    }
}