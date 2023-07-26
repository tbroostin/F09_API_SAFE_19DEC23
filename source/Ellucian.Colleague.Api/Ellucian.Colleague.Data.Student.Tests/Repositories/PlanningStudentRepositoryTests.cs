// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class PlanningStudentRepositoryTests
    {
        private static Collection<T> ToCollection<T>(IEnumerable<T> data)
        {
            return new Collection<T>(data.ToList());
        }

        [TestClass]
        public class PlanningStudentRepositoryTests_Get : BasePersonSetup
        {
            private TestStudentRepository testStudentRepository;
            private PlanningStudentRepository planningStudentRepository;
            private string knownStudentId1;
            private string knownStudentId2;
            private string knownStudentId3;
            private string knownStudentId4;
            private string unknownStudentId;
            private string[] acadPrograms;
            private string quote;
            private string educationalGoalDescription;
            private DateTime today = DateTime.Today;
            private DateTime now = DateTime.Now;

            private Collection<Student.DataContracts.Students> students;
            private Collection<Base.DataContracts.PersonSt> personStRecords;
            private Collection<Student.DataContracts.StudentAdvisement> studentAdvisements;
            private Collection<Base.DataContracts.Person> people;
            private Collection<Student.DataContracts.StudentPrograms> studentPrograms;

            [TestInitialize]
            public void Initialize()
            {
                testStudentRepository = new TestStudentRepository();
                students = new Collection<Student.DataContracts.Students>();
                personStRecords = new Collection<Base.DataContracts.PersonSt>();
                studentAdvisements = new Collection<Student.DataContracts.StudentAdvisement>();
                people = new Collection<Base.DataContracts.Person>();
                studentPrograms = new Collection<Student.DataContracts.StudentPrograms>();
                quote = '"'.ToString();
                educationalGoalDescription = "Masters degree";
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                SetupData();

                planningStudentRepository = BuildMockPlanningStudentRepository();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                planningStudentRepository = null;
            }

            [TestMethod]
            // generic test for a known ID, verify that the same ID is passed out.
            public async Task PlanningStudent_Get()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(knownStudentId1, student.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PlanningStudent_Get_ExceptionIfNullIdSupplied()
            {
                string nullId = null;
                await planningStudentRepository.GetAsync(nullId);
            }

            [TestMethod]
            // generic test for a known ID, verify that the same ID is passed out.
            public async Task PlanningStudent_Get_ManyIDs()
            {
                List<string> studentIds = new List<string>();
                studentIds.Add(knownStudentId1);
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = await planningStudentRepository.GetAsync(studentIds);
                var student = students.ElementAt(0);
                Assert.AreEqual(knownStudentId1, student.Id);
            }

            [TestMethod]
            // generic test for an unknown ID, verify not found
            public async Task PlanningStudent_Get_UnknownID()
            {
                Base.DataContracts.Person nullPerson = null;
                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.Person>("PERSON", unknownStudentId, true)).ReturnsAsync(nullPerson);

                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(unknownStudentId);
                Assert.IsNull(student);
            }

            [TestMethod]
            // test a student with no acad programs
            public async Task PlanningStudent_Get_NoPrograms()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(0, student.ProgramIds.Count);
            }

            [TestMethod]
            // test a student with one acad program
            public async Task PlanningStudent_Get_OneProgram()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.AreEqual(1, student.ProgramIds.Count);
                Assert.AreEqual(acadPrograms[0], student.ProgramIds[0]);
            }

            [TestMethod]
            // test a student with two acad programs - also enforces same order from db
            // This student has 3 programs in setup, but third has previous end date.
            public async Task PlanningStudent_Get_TwoPrograms()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId3);
                Assert.AreEqual(2, student.ProgramIds.Count);
                Assert.AreEqual(acadPrograms[0], student.ProgramIds[0]);
                Assert.AreEqual(acadPrograms[1], student.ProgramIds[1]);
            }

            [TestMethod]
            // test a student with one acad program
            public async Task PlanningStudent_Get_NoExceptionIfPersonStDataNotFound()
            {
                Collection<Base.DataContracts.PersonSt> emptyPersonStCollection = new Collection<Base.DataContracts.PersonSt>();
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(It.IsAny<string>(), true)).ReturnsAsync(emptyPersonStCollection);
                dataReaderMock.Setup(a => a.BulkReadRecordWithInvalidRecordsAsync<Base.DataContracts.PersonSt>(It.IsAny<string>(), true)).ReturnsAsync(
                    new BulkReadOutput<Base.DataContracts.PersonSt>() { BulkRecordsRead = emptyPersonStCollection });

                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.AreEqual(1, student.ProgramIds.Count);
                Assert.AreEqual(acadPrograms[0], student.ProgramIds[0]);
            }

            [TestMethod]
            // test a student with no degree plans
            public async Task PlanningStudent_Get_NoDegreePlan()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId3);
                Assert.AreEqual(false, student.DegreePlanId.HasValue);
            }

            [TestMethod]
            // test a student with one degree plans
            public async Task PlanningStudent_Get_OneDegPlan()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(true, student.DegreePlanId.HasValue);
            }

            [TestMethod]
            // test that the planning student is not cached when it has no degree plan id
            public async Task PlanningStudent_Get_NoDegreePlanNotWrittenToCached()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId3);

                Assert.AreEqual(knownStudentId3, student.Id);
                Assert.AreEqual(false, student.DegreePlanId.HasValue);

                // Verify the item was not (Times.Never) written to cache
                string fullCacheKey = planningStudentRepository.BuildFullCacheKey("PlanningStudent" + knownStudentId3);
                cacheProviderMock.Verify(x => x.Add(fullCacheKey, It.IsAny<Object>(), It.IsAny<CacheItemPolicy>(), null), Times.Never());
            }

            [TestMethod]
            public async Task PlanningStudent_Get_WhenFoundInCache_NoDatabaseReads()
            {
                string fullCacheKey = planningStudentRepository.BuildFullCacheKey("PlanningStudent" + knownStudentId1);
                cacheProviderMock.Setup(x => x.Contains(fullCacheKey, null)).Returns(true);
                // Mock the planning student that is returned from cache
                var cachedPlanningStudent = new Domain.Student.Entities.PlanningStudent(knownStudentId1, "Brown", 123, new List<string>() { "ENGL.BA" });
                cacheProviderMock.Setup(x => x.Get(fullCacheKey, null)).Returns(cachedPlanningStudent);

                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(true, student.DegreePlanId.HasValue);

                // Verify that bulk read was not (times.never) called
                dataReaderMock.Verify<Task<Collection<Base.DataContracts.Person>>>(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true), Times.Never());
                dataReaderMock.Verify<Task<BulkReadOutput<Base.DataContracts.Person>>>(a => a.BulkReadRecordWithInvalidRecordsAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true), Times.Never());

            }

            [TestMethod]
            public async Task PlanningStudent_Get_WithDegPlanIsWrittenToCache()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(true, student.DegreePlanId.HasValue);

                // Verify planning student object was written to cache
                string fullCacheKey = planningStudentRepository.BuildFullCacheKey("PlanningStudent" + knownStudentId1);
                cacheProviderMock.Verify(x => x.AddAndUnlockSemaphore(fullCacheKey, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            // test a student with two degree plans
            public async Task PlanningStudent_Get_TwoDegPlan()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.AreEqual(true, student.DegreePlanId.HasValue);
            }

            [TestMethod]
            public async Task Get_StudentById_NoAdvisors()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(0, student.AdvisorIds.Count());
            }

            [TestMethod]
            public async Task Get_StudentById_NoAdvisorsDoesNotSelectAdvisements()
            {
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(0, student.AdvisorIds.Count());

                // Verify that student.advisement was not (times.never) selected
                dataReaderMock.Verify<Task<string[]>>(a => a.SelectAsync("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<string>()), Times.Never());
            }

            [TestMethod]
            public async Task PlanningStudent_Get_Advisors()
            {
                // Excludes advisor with an end date.
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.AreEqual(2, student.AdvisorIds.Count());
            }

            [TestMethod]
            public async Task Planning_Student_Get_Advisement()
            {
                // Excludes advisor with an end date.
                Domain.Student.Entities.PlanningStudent student = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.AreEqual(2, student.Advisements.Count());
            }

            [TestMethod]
            public async Task Get_StudentById_FirstEmailAsPreferredIfNotFlagged()
            {
                // Get first email if none are flagged as preferred
                Domain.Student.Entities.PlanningStudent student1 = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual("dsmith@yahoo.com", student1.PreferredEmailAddress.Value);
            }

            [TestMethod]
            public async Task Planning_Student_Get_PreferredEmail()
            {
                // Get email that is flagged as preferred
                Domain.Student.Entities.PlanningStudent student2 = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.AreEqual("djones@yahoo.com", student2.PreferredEmailAddress.Value);
            }

            [TestMethod]
            public async Task PlanningStudent_Get_NullPreferredEmailAddressIfNoEmail()
            {
                Domain.Student.Entities.PlanningStudent student3 = await planningStudentRepository.GetAsync(knownStudentId3);
                Assert.IsNull(student3.PreferredEmailAddress);
            }

            [TestMethod]
            public async Task PlanningStudent_Get_InactiveProgramsRejected()
            {
                Domain.Student.Entities.PlanningStudent student4 = await planningStudentRepository.GetAsync(knownStudentId4);
                Assert.AreEqual(0, student4.ProgramIds.Count());
            }

            [TestMethod]
            public async Task PlanningStudent_Get_RegPriorities()
            {
                Domain.Student.Entities.PlanningStudent student2 = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.AreEqual(2, student2.RegistrationPriorityIds.Count());
            }

            [TestMethod]
            public async Task PlanningStudent_Get_InvalidEducationalGoalReturnedAsNull()
            {
                Domain.Student.Entities.PlanningStudent student1 = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(null, student1.EducationalGoal);
            }

            [TestMethod]
            public async Task PlanningStudent_Get_EducationalGoalsValcodeEmptyReturnsNullEducationalGoal()
            {
                ApplValcodes nullApplValcodeTable = null;
                dataReaderMock.Setup(a =>
                    a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "EDUCATION.GOALS", true))
                    .ReturnsAsync(nullApplValcodeTable);
                Domain.Student.Entities.PlanningStudent student2 = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(null, student2.EducationalGoal);
            }

            [TestMethod]
            public async Task PlanningStudent_Get_ReturnsNullStudentIfStudentProgramStatusesValcodeEmpty()
            {
                ApplValcodes nullApplValcodeTable = null;
                dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                    a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true))
                    .ReturnsAsync(nullApplValcodeTable);
                Domain.Student.Entities.PlanningStudent student4 = await planningStudentRepository.GetAsync(knownStudentId4);
                Assert.IsNull(student4);
            }

            [TestMethod]
            public async Task Planning_Student_Get_ProgramReturnedEvenIfStatusCodeNotFound()
            {
                dataReaderMock.Setup(a =>
                    a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true))
                    .ReturnsAsync(new ApplValcodes()
                    {
                        ValInternalCode = new List<string>() { "X" },
                        ValExternalRepresentation = new List<string>() { "Active" },
                        ValActionCode1 = new List<string>() { "2" },
                        ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "X",
                            ValExternalRepresentationAssocMember = "Active",
                            ValActionCode1AssocMember = "2"
                        }
                    }
                    });
                Domain.Student.Entities.PlanningStudent student2 = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.AreEqual(1, student2.ProgramIds.Count());
            }

            [TestMethod]
            public async Task Planning_Student_Get_PersonDisplayName_Chosen()
            {
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.IsNotNull(pstudent.PersonDisplayName);
                Assert.AreEqual("ChosenLast, ChosenFirst", pstudent.PersonDisplayName.FullName);
            }

            [TestMethod]
            public async Task Planning_Student_Get_PersonDisplayName_Formatted()
            {
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.IsNotNull(pstudent.PersonDisplayName);
                Assert.AreEqual("YYY Formatted Name", pstudent.PersonDisplayName.FullName);
            }

            [TestMethod]
            public async Task Planning_Student_Get_PersonDisplayName_PreferredOverride()
            {
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId3);
                Assert.IsNotNull(pstudent.PersonDisplayName);
                Assert.AreEqual("Mr. L. Legal Middle Legal Last, Esq.", pstudent.PersonDisplayName.FullName);
            }

            [TestMethod]
            public async Task Planning_Student_Get_PersonDisplayName_PreferredStandard()
            {
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.IsNotNull(pstudent.PersonDisplayName);
                Assert.AreEqual("YYY Formatted Name", pstudent.PersonDisplayName.FullName);
            }

            [TestMethod]
            public async Task Planning_Student_Get_PersonDisplayName_NullWhenNoParam()
            {
                var emptyStwebDefault = new StwebDefaults();
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).ReturnsAsync(emptyStwebDefault);
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.IsNull(pstudent.PersonDisplayName);
            }

            [TestMethod]
            public async Task Planning_Student_Get_No_CompletedAdvisements_Null_Association()
            {
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(0, pstudent.CompletedAdvisements.Count);
            }

            [TestMethod]
            public async Task Planning_Student_Get_No_CompletedAdvisements_Empty_Association()
            {
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId2);
                Assert.AreEqual(0, pstudent.CompletedAdvisements.Count);
            }

            [TestMethod]
            public async Task Planning_Student_Get_No_CompletedAdvisements_Association_Member_No_Date()
            {
                students[2].StuAdvisementsEntityAssociation[0].StuAdviseCompleteDateAssocMember = null;
                students[2].StuAdvisementsEntityAssociation[0].StuAdviseCompleteTimeAssocMember = now;
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId3);
                Assert.AreEqual(0, pstudent.CompletedAdvisements.Count);
            }

            [TestMethod]
            public async Task Planning_Student_Get_No_CompletedAdvisements_Association_Member_No_Time()
            {
                students[2].StuAdvisementsEntityAssociation[0].StuAdviseCompleteDateAssocMember = today;
                students[2].StuAdvisementsEntityAssociation[0].StuAdviseCompleteTimeAssocMember = null;
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId3);
                Assert.AreEqual(0, pstudent.CompletedAdvisements.Count);
            }

            [TestMethod]
            public async Task Planning_Student_Get_No_CompletedAdvisements_Association_Member_No_Date_or_Time()
            {
                students[2].StuAdvisementsEntityAssociation[0].StuAdviseCompleteDateAssocMember = null;
                students[2].StuAdvisementsEntityAssociation[0].StuAdviseCompleteTimeAssocMember = null;
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId3);
                Assert.AreEqual(0, pstudent.CompletedAdvisements.Count);
            }

            [TestMethod]
            public async Task Planning_Student_Get_CompletedAdvisements()
            {
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId4);
                Assert.AreEqual(1, pstudent.CompletedAdvisements.Count);
            }

            [TestMethod]
            public async Task Planning_Student_Get_PersonalPronounCode()
            {
                Domain.Student.Entities.PlanningStudent pstudent = await planningStudentRepository.GetAsync(knownStudentId1);
                Assert.IsNotNull(pstudent.PersonalPronounCode);
                Assert.AreEqual("XHE", pstudent.PersonalPronounCode);
            }

            private void SetupData()
            {
                unknownStudentId = "9999999";
                acadPrograms = new string[] { "BA.CPSC", "BA-MATH", "ABC", "DEF" };

                /* 
                 * student 0000001
                 * > no programs
                 * > one degree plan
                 * > one acad cred
                 * > no advisors
                 * > invalid educational goal
                 * > 2 emails - neither marked as preferred
                 */
                knownStudentId1 = "0000001";
                students.Add(new Student.DataContracts.Students() { Recordkey = knownStudentId1, StuAcadPrograms = null, StuAdvisementsEntityAssociation = null });
                personStRecords.Add(new Base.DataContracts.PersonSt()
                {
                    Recordkey = "0000001",
                    PstStudentAcadCred = new List<string>() { "19000" },
                    PstAdvisement = new List<string>(),
                    PstRestrictions = new List<string>() { "R001", "R002" },
                    EducGoalsEntityAssociation = new List<Base.DataContracts.PersonStEducGoals>() {
                        new PersonStEducGoals() {PstEducGoalsAssocMember = "X", PstEducGoalsChgdatesAssocMember = new DateTime(2012, 05, 01)}
                }
                });
                Base.DataContracts.Person person1 = new Base.DataContracts.Person()
                {
                    Recordkey = "0000001",
                    LastName = "Smith",
                    FirstName = "Legal First",
                    MiddleName = "Legal Middle",
                    PersonChosenLastName = "ChosenLast",
                    PersonChosenFirstName = "ChosenFirst",
                    Prefix = "Mr.",
                    PersonalPronoun = "XHE"
                };

                person1.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
                PersonPeopleEmail ppe1 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "dsmith@yahoo.com", PersonEmailTypesAssocMember = "HOME", PersonPreferredEmailAssocMember = "" };
                person1.PeopleEmailEntityAssociation.Add(ppe1);
                PersonPeopleEmail ppe2 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "junk@yahoo.com", PersonEmailTypesAssocMember = "BUS", PersonPreferredEmailAssocMember = "" };
                person1.PeopleEmailEntityAssociation.Add(ppe2);
                people.Add(person1);
                /* 
                 * student 0000002
                 * > one program
                 * > two degree plans
                 * > two acad creds
                 * > two current advisors, one with null start/end dates (but also a past advisor)
                 * > two emails - second marked as preferred
                 */
                knownStudentId2 = "0000002";
                personStRecords.Add(new Base.DataContracts.PersonSt()
                {
                    Recordkey = "0000002",
                    PstStudentAcadCred = new List<string>() { "" },
                    PstAdvisement = new List<string>() { "21", "22", "23" },
                    PstRestrictions = new List<string>(),
                    EducGoalsEntityAssociation = new List<Base.DataContracts.PersonStEducGoals>() {
                        new PersonStEducGoals() {PstEducGoalsAssocMember = "BA", PstEducGoalsChgdatesAssocMember = new DateTime(2012, 05, 01)},
                        new PersonStEducGoals() {PstEducGoalsAssocMember = "MA", PstEducGoalsChgdatesAssocMember = new DateTime(2013, 02, 03)}
                    }
                });
                // Bulk data read responses for this student
                students.Add(new Student.DataContracts.Students() { Recordkey = knownStudentId2, StuAcadPrograms = new List<string> { acadPrograms[0] }, StuRegPriorities = new List<string>() { "RP1", "RP2" }, StuAdvisementsEntityAssociation = new List<StudentsStuAdvisements>() });
                studentAdvisements.Add(new Student.DataContracts.StudentAdvisement() { Recordkey = "21", StadStudent = "0000002", StadFaculty = "0000036", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = null });
                studentAdvisements.Add(new Student.DataContracts.StudentAdvisement() { Recordkey = "22", StadStudent = "0000002", StadFaculty = "0000045", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = new DateTime(2012, 8, 1) });
                studentAdvisements.Add(new Student.DataContracts.StudentAdvisement() { Recordkey = "23", StadStudent = "0000002", StadFaculty = "0000057", StadStartDate = null, StadEndDate = null });
                Base.DataContracts.Person person2 = new Base.DataContracts.Person()
                {
                    Recordkey = "0000002",
                    LastName = "Jones",
                    FirstName = "Legal First",
                    PFormatEntityAssociation = new List<PersonPFormat>() { new PersonPFormat() { PersonFormattedNameTypesAssocMember = "XXX", PersonFormattedNamesAssocMember = "XXX Formatted Name" }, new PersonPFormat() { PersonFormattedNameTypesAssocMember = "YYY", PersonFormattedNamesAssocMember = "YYY Formatted Name" } }
                };
                person2.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
                PersonPeopleEmail ppe3 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "junk@yahoo.com", PersonEmailTypesAssocMember = "HOME", PersonPreferredEmailAssocMember = "" };
                person2.PeopleEmailEntityAssociation.Add(ppe3);
                PersonPeopleEmail ppe4 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "djones@yahoo.com", PersonEmailTypesAssocMember = "BUS", PersonPreferredEmailAssocMember = "Y" };
                person2.PeopleEmailEntityAssociation.Add(ppe4);
                people.Add(person2);
                studentPrograms.Add(
                        new Student.DataContracts.StudentPrograms
                        {
                            Recordkey = knownStudentId2 + "*" + acadPrograms[0],
                            StprStartDate = new List<DateTime?>() { new DateTime() },
                            StprStatus = new List<string>() { "A" },
                            StprCatalog = "2012"
                        });

                /* 
                 * student 0000003
                 * > two programs
                 * > no degree plan
                 * > three acad creds
                 * > no email addresses on file
                 */
                knownStudentId3 = "0000003";
                students.Add(new Student.DataContracts.Students()
                {
                    Recordkey = knownStudentId3,
                    StuAcadPrograms = new List<string> { acadPrograms[0], acadPrograms[1] },
                    StuAdvisementsEntityAssociation = new List<StudentsStuAdvisements>()
                    {
                        new StudentsStuAdvisements()
                        {
                            StuAdviseCompleteAdvisorAssocMember = "1234567"
                        }
                    }
                });
                personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000003", PstStudentAcadCred = new List<string>() { "19003", "19004", "19005" }, PstAdvisement = new List<string>(), PstRestrictions = new List<string>() });
                Base.DataContracts.Person person3 = new Base.DataContracts.Person()
                {
                    Recordkey = "0000003",
                    LastName = "Legal Last",
                    FirstName = "Legal First",
                    MiddleName = "Legal Middle",
                    Prefix = "Mr.",
                    Suffix = "Esq.",
                    PreferredName = "IM"
                };
                person3.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
                people.Add(person3);
                studentPrograms.Add(
                        new Student.DataContracts.StudentPrograms
                        {
                            Recordkey = knownStudentId3 + "*" + acadPrograms[0],
                            StprStartDate = new List<DateTime?>() { new DateTime() },
                            StprStatus = new List<string>() { "A" },
                            StprCatalog = "2012"
                        });
                studentPrograms.Add(
                        new Student.DataContracts.StudentPrograms
                        {
                            Recordkey = knownStudentId3 + "*" + acadPrograms[1],
                            StprStartDate = new List<DateTime?>() { new DateTime() },
                            StprStatus = new List<string>() { "A" },
                            StprCatalog = "2011"
                        });
                studentPrograms.Add(
                        new Student.DataContracts.StudentPrograms
                        {   // ended student program
                            Recordkey = knownStudentId3 + "*" + "BA.UNDC",
                            StprStartDate = new List<DateTime?>() { new DateTime() },
                            StprStatus = new List<string>() { "C" },
                            StprEndDate = new List<DateTime?>() { DateTime.Today.AddDays(-60) },
                            StprCatalog = "2010"
                        });

                /* 
                 * student 0000004
                 * > two inactive programs
                 * > no degree plan
                 * > no acad creds
                 * > null email addresses
                 */
                knownStudentId4 = "0000004";
                students.Add(new Student.DataContracts.Students()
                {
                    Recordkey = knownStudentId4,
                    StuAcadPrograms = new List<string> { acadPrograms[0], acadPrograms[1], acadPrograms[2], acadPrograms[3] },
                    StuAdvisementsEntityAssociation = new List<StudentsStuAdvisements>()
                    {
                        new StudentsStuAdvisements()
                        {
                            StuAdviseCompleteAdvisorAssocMember = "1234567",
                            StuAdviseCompleteDateAssocMember = today,
                            StuAdviseCompleteTimeAssocMember = now
                        }
                    }
                });
                Base.DataContracts.Person person4 = new Base.DataContracts.Person()
                {
                    Recordkey = "0000004",
                    LastName = "Legal Brown",
                    FirstName = "FirstName",
                    MiddleName = "H",
                    Prefix = "Mrs.",

                };
                person4.PeopleEmailEntityAssociation = null;
                people.Add(person4);
                // Program with inactive status action code
                studentPrograms.Add(
                    new Student.DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId4 + "*" + acadPrograms[0],
                        StprStartDate = new List<DateTime?>() { new DateTime(2015, 01, 01) },
                        StprStatus = new List<string>() { "P" },
                        StprCatalog = "2012"
                    });
                // program with another inactive status action code
                studentPrograms.Add(
                    new Student.DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId4 + "*" + acadPrograms[1],
                        StprStartDate = new List<DateTime?>() { new DateTime(2015, 01, 01) },
                        StprStatus = new List<string>() { "C" },
                        StprCatalog = "2011"
                    });
                // program with empty start dates
                studentPrograms.Add(
                    new Student.DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId4 + "*" + acadPrograms[2],
                        StprStartDate = new List<DateTime?>() { },
                        StprStatus = new List<string>() { "A" },
                        StprCatalog = "2011"
                    });
                // program with expired end date
                studentPrograms.Add(
                    new Student.DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId4 + "*" + acadPrograms[3],
                        StprStartDate = new List<DateTime?>() { new DateTime(2014, 01, 01), new DateTime(2013, 01, 01) },
                        StprEndDate = new List<DateTime?>() { new DateTime(2014, 05, 01), new DateTime(2013, 12, 31) },
                        StprStatus = new List<string>() { "A", "A" },
                        StprCatalog = "2011"
                    });

                // mock data accessor STUDENT.PROGRAM.STATUSES
                dataReaderMock.Setup(a =>
                    a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true))
                    .ReturnsAsync(new ApplValcodes()
                    {
                        ValInternalCode = new List<string>() { "A", "P", "C" },
                        ValExternalRepresentation = new List<string>() { "Active", "Pending", "Closed" },
                        ValActionCode1 = new List<string>() { "2", "4", "5" },
                        ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "A",
                            ValExternalRepresentationAssocMember = "Active",
                            ValActionCode1AssocMember = "2"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "P",
                            ValExternalRepresentationAssocMember = "Pending",
                            ValActionCode1AssocMember = "4"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "C",
                            ValExternalRepresentationAssocMember = "Closed",
                            ValActionCode1AssocMember = "5"
                        }
                    }
                    });

                // mock data accessor EDUCATION.GOALS
                dataReaderMock.Setup(a =>
                    a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "EDUCATION.GOALS", true))
                    .ReturnsAsync(new ApplValcodes()
                    {
                        ValInternalCode = new List<string>() { "MA" },
                        ValExternalRepresentation = new List<string>() { educationalGoalDescription },
                        ValsEntityAssociation = new List<ApplValcodesVals>()
                        {
                            new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "MA",
                                ValExternalRepresentationAssocMember = educationalGoalDescription,
                            }
                        }
                    });
            }

            private PlanningStudentRepository BuildMockPlanningStudentRepository()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(ToCollection(people.Where(x => s.Contains(x.Recordkey))))).Verifiable();
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(ToCollection(students.Where(x => s.Contains(x.Recordkey)))));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(ToCollection(personStRecords.Where(x => s.Contains(x.Recordkey)))));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(studentPrograms);

                //DataReader mock setup for BulkReadRecordWithInvalidRecordsAsync
                //for person
                dataReaderMock.Setup(
               accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true))
               .Returns<string[], bool>((s, b) =>
               {
                   return Task.FromResult(new BulkReadOutput<Base.DataContracts.Person>()
                   {
                       BulkRecordsRead = ToCollection(people.Where(x => s.Contains(x.Recordkey)))
                   });
               }).Verifiable();

                //for students

                dataReaderMock.Setup(
              accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true))
              .Returns<string[], bool>((s, b) =>
              {
                  return Task.FromResult(new BulkReadOutput<Student.DataContracts.Students>()
                  {
                      BulkRecordsRead = ToCollection(students.Where(x => s.Contains(x.Recordkey)))
                  });
              });

                //for personst
                dataReaderMock.Setup(
             accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true))
             .Returns<string[], bool>((s, b) =>
             {
                 return Task.FromResult(new BulkReadOutput<Base.DataContracts.PersonSt>()
                 {
                     BulkRecordsRead = ToCollection(personStRecords.Where(x => s.Contains(x.Recordkey)))
                 });
             });

                //student programs
                dataReaderMock.Setup(
             accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Student.DataContracts.StudentPrograms>(It.IsAny<string[]>(), true))
             .Returns<string[], bool>((s, b) =>
             {
                 return Task.FromResult(new BulkReadOutput<Student.DataContracts.StudentPrograms>()
                 {
                     BulkRecordsRead = studentPrograms
                 });
             });



                var foo = students.Where(x => x.Recordkey != "1").ToList();

                // mock data accessor DEGREE_PLANS response  - for student 0000001
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId1 + "'")).ReturnsAsync(new string[] { "1" });

                // mock data accessor DEGREE_PLANS response  - for student 0000002
                var multipleDegreePlanKeys = new string[] { "2", "3" };
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId2 + "'")).ReturnsAsync(multipleDegreePlanKeys);

                // mock data accessor DEGREE_PLANS response  - for student 0000003
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId3 + "'")).ReturnsAsync(new string[] { });

                // mock data accessor STUDENT.ADVISEMENT for students with no advisor
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(It.IsAny<string>(), true)).ReturnsAsync(new Collection<Student.DataContracts.StudentAdvisement>());
                dataReaderMock.Setup(
              accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Student.DataContracts.StudentAdvisement>(It.IsAny<string[]>(), true))
              .Returns<string[], bool>((s, b) =>
              {
                  return Task.FromResult(new BulkReadOutput<Student.DataContracts.StudentAdvisement>()
                  {
                      BulkRecordsRead = new Collection<Student.DataContracts.StudentAdvisement>()
                  });
              });
                // mock data accessor and selects STUDENT.ADVISEMENT for student 0000002
                string[] student2AdvismentIds = new List<string>() { "21", "22", "23" }.ToArray();
                dataReaderMock.Setup(a => a.SelectAsync("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(student2AdvismentIds).Verifiable();
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(student2AdvismentIds, true)).ReturnsAsync(studentAdvisements);
                dataReaderMock.Setup(
              accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Student.DataContracts.StudentAdvisement>(student2AdvismentIds, true))
              .Returns<string[], bool>((s, b) =>
              {
                  return Task.FromResult(new BulkReadOutput<Student.DataContracts.StudentAdvisement>()
                  {
                      BulkRecordsRead = studentAdvisements
                  });
              });

                var stWebDflt = BuildStwebDefaults();
                //Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true);
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).ReturnsAsync(stWebDflt);

                // mock data reader for getting the STUDENT Name Addr Hierarchy
                dataReaderMock.Setup<Task<Base.DataContracts.NameAddrHierarchy>>(a =>
                    a.ReadRecordAsync<Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "STUDENT", true))
                    .ReturnsAsync(new Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "STUDENT",
                        NahNameHierarchy = new List<string>() { "YYY", "CHL", "PF" }
                    });

                PlanningStudentRepository repository = new PlanningStudentRepository(
                    cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return repository;
            }

            private static StwebDefaults BuildStwebDefaults()
            {
                StwebDefaults stwebDefaults = new StwebDefaults();
                stwebDefaults.Recordkey = "STWEB.DEFAULTS";
                stwebDefaults.StwebDisplayNameHierarchy = "STUDENT";
                stwebDefaults.StwebRegUsersId = "REGUSERID";
                return stwebDefaults;
            }
        }

        [TestClass]
        public class PlanningStudentRepositoryTests_GetMany : BasePersonSetup
        {
            private TestStudentRepository testStudentRepository;
            private PlanningStudentRepository planningStudentRepository;
            private string knownStudentId1;
            private string knownStudentId2;
            private string knownStudentId3;
            private string unknownStudentId;
            private string[] acadPrograms;
            private string quote;
            private string educationalGoalDescription;

            private List<string> ids;
            private Collection<Student.DataContracts.Students> students;
            private Collection<Base.DataContracts.PersonSt> personStRecords;
            private Collection<Student.DataContracts.StudentAdvisement> studentAdvisements;
            private Collection<Base.DataContracts.Person> people;
            private Collection<Student.DataContracts.StudentPrograms> studentPrograms;

            [TestInitialize]
            public void Initialize()
            {
                testStudentRepository = new TestStudentRepository();
                students = new Collection<Student.DataContracts.Students>();
                personStRecords = new Collection<Base.DataContracts.PersonSt>();
                studentAdvisements = new Collection<Student.DataContracts.StudentAdvisement>();
                people = new Collection<Base.DataContracts.Person>();
                studentPrograms = new Collection<Student.DataContracts.StudentPrograms>();
                quote = '"'.ToString();
                educationalGoalDescription = "Masters degree";
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                SetupData();
                ids = new List<string>() { knownStudentId1, knownStudentId2, knownStudentId3 };

                planningStudentRepository = BuildMockPlanningStudentRepository();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                planningStudentRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PlanningStudents_GetMany_ExceptionIfNullIdsSupplied()
            {
                List<string> nullIds = null;
                await planningStudentRepository.GetAsync(nullIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PlanningStudents_GetMany_ExceptionIfEmptyIdsSupplied()
            {
                List<string> emptyIds = new List<string>();
                await planningStudentRepository.GetAsync(emptyIds);
            }

            [TestMethod]
            public async Task PlanningStudents_GetMany_ReturnsRequestedStudents()
            {
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = await planningStudentRepository.GetAsync(ids);
                Assert.AreEqual(3, students.Count());
            }

            [TestMethod]
            public async Task PlanningStudents_GetMany_StudentNotReturnsForUnknownId()
            {
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = await planningStudentRepository.GetAsync(new List<string>() { unknownStudentId });
                Assert.AreEqual(0, students.Count());
            }

            [TestMethod]
            public async Task PlanningStudents_GetMany_Properties()
            {
                // This is just a spot check, properties are thoroughly tested in single Get_ tests
                IEnumerable<Domain.Student.Entities.PlanningStudent> planningStudents = await planningStudentRepository.GetAsync(ids);
                Domain.Student.Entities.PlanningStudent planningStudent = planningStudents.Where(s => s.Id == knownStudentId2).FirstOrDefault();

                Assert.AreEqual(1, planningStudent.AdvisorIds.Count());
                Assert.AreEqual(1, planningStudent.ProgramIds.Count);
                Assert.AreEqual(acadPrograms[0], planningStudent.ProgramIds[0]);
                Assert.AreEqual(true, planningStudent.DegreePlanId.HasValue);
                Assert.AreEqual("djones@yahoo.com", planningStudent.PreferredEmailAddress.Value);
            }

            private void SetupData()
            {
                unknownStudentId = "9999999";
                acadPrograms = new string[] { "BA.CPSC", "BA-MATH" };

                /* 
                 * student 0000001
                 * > no programs
                 * > one degree plan
                 * > one acad cred
                 * > no advisors
                 * > 2 emails - neither marked as preferred
                 */
                knownStudentId1 = "0000001";
                students.Add(new Student.DataContracts.Students() { Recordkey = knownStudentId1, StuAcadPrograms = null });
                personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000001", PstStudentAcadCred = new List<string>() { "19000" }, PstAdvisement = new List<string>(), PstRestrictions = new List<string>() { "R001", "R002" } });
                Base.DataContracts.Person person1 = new Base.DataContracts.Person() { Recordkey = "0000001", LastName = "Smith" };
                person1.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
                PersonPeopleEmail ppe1 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "dsmith@yahoo.com", PersonEmailTypesAssocMember = "HOME", PersonPreferredEmailAssocMember = "" };
                person1.PeopleEmailEntityAssociation.Add(ppe1);
                PersonPeopleEmail ppe2 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "junk@yahoo.com", PersonEmailTypesAssocMember = "BUS", PersonPreferredEmailAssocMember = "" };
                person1.PeopleEmailEntityAssociation.Add(ppe2);
                people.Add(person1);
                /* 
                 * student 0000002
                 * > one program
                 * > two degree plans
                 * > two acad creds
                 * > one current advisor (but also a past advisor)
                 * > two emails - second marked as preferred
                 */
                knownStudentId2 = "0000002";
                personStRecords.Add(new Base.DataContracts.PersonSt()
                {
                    Recordkey = "0000002",
                    PstStudentAcadCred = new List<string>() { "" },
                    PstAdvisement = new List<string>() { "21", "22" },
                    PstRestrictions = new List<string>(),
                    EducGoalsEntityAssociation = new List<Base.DataContracts.PersonStEducGoals>() {
                        new PersonStEducGoals() {PstEducGoalsAssocMember = "BA", PstEducGoalsChgdatesAssocMember = new DateTime(2012, 05, 01)},
                        new PersonStEducGoals() {PstEducGoalsAssocMember = "MA", PstEducGoalsChgdatesAssocMember = new DateTime(2013, 02, 03)}
                    }
                });
                // Bulk data read responses for this student
                students.Add(new Student.DataContracts.Students() { Recordkey = knownStudentId2, StuAcadPrograms = new List<string> { acadPrograms[0] } });
                studentAdvisements.Add(new Student.DataContracts.StudentAdvisement() { Recordkey = "21", StadStudent = "0000002", StadFaculty = "0000036", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = null });
                studentAdvisements.Add(new Student.DataContracts.StudentAdvisement() { Recordkey = "22", StadStudent = "0000002", StadFaculty = "0000045", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = new DateTime(2012, 8, 1) });
                Base.DataContracts.Person person2 = new Base.DataContracts.Person() { Recordkey = "0000002", LastName = "Jones" };
                person2.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
                PersonPeopleEmail ppe3 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "junk@yahoo.com", PersonEmailTypesAssocMember = "HOME", PersonPreferredEmailAssocMember = "" };
                person2.PeopleEmailEntityAssociation.Add(ppe3);
                PersonPeopleEmail ppe4 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "djones@yahoo.com", PersonEmailTypesAssocMember = "BUS", PersonPreferredEmailAssocMember = "Y" };
                person2.PeopleEmailEntityAssociation.Add(ppe4);
                people.Add(person2);
                studentPrograms.Add(
                        new Student.DataContracts.StudentPrograms
                        {
                            Recordkey = knownStudentId2 + "*" + acadPrograms[0],
                            StprStartDate = new List<DateTime?>() { new DateTime() },
                            StprStatus = new List<string>() { "A" },
                            StprCatalog = "2012"
                        });

                /* 
                 * student 0000003
                 * > two programs
                 * > no degree plan
                 * > three acad creds
                 * > no email addresses on file
                 */
                knownStudentId3 = "0000003";
                students.Add(new Student.DataContracts.Students() { Recordkey = knownStudentId3, StuAcadPrograms = new List<string> { acadPrograms[0], acadPrograms[1] } });
                personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000003", PstStudentAcadCred = new List<string>() { "19003", "19004", "19005" }, PstAdvisement = new List<string>(), PstRestrictions = new List<string>() });
                Base.DataContracts.Person person3 = new Base.DataContracts.Person() { Recordkey = "0000003", LastName = "Jones" };
                person3.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
                people.Add(person3);
                studentPrograms.Add(
                        new Student.DataContracts.StudentPrograms
                        {
                            Recordkey = knownStudentId3 + "*" + acadPrograms[0],
                            StprStartDate = new List<DateTime?>() { new DateTime() },
                            StprStatus = new List<string>() { "A" },
                            StprCatalog = "2012"
                        });
                studentPrograms.Add(
                        new Student.DataContracts.StudentPrograms
                        {
                            Recordkey = knownStudentId3 + "*" + acadPrograms[1],
                            StprStartDate = new List<DateTime?>() { new DateTime() },
                            StprStatus = new List<string>() { "A" },
                            StprCatalog = "2011"
                        });
                studentPrograms.Add(
                        new Student.DataContracts.StudentPrograms
                        {   // ended student program
                            Recordkey = knownStudentId3 + "*" + "BA.UNDC",
                            StprStartDate = new List<DateTime?>() { new DateTime() },
                            StprStatus = new List<string>() { "A" },
                            StprEndDate = new List<DateTime?>() { DateTime.Today.AddDays(-60) },
                            StprCatalog = "2010"
                        });

                // mock data accessor STUDENT.PROGRAM.STATUSES
                dataReaderMock.Setup(a =>
                    a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true))
                    .ReturnsAsync(new ApplValcodes()
                    {
                        ValInternalCode = new List<string>() { "A" },
                        ValExternalRepresentation = new List<string>() { "Active" },
                        ValActionCode1 = new List<string>() { "2" },
                        ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "A",
                            ValExternalRepresentationAssocMember = "Active",
                            ValActionCode1AssocMember = "2"
                        }
                    }
                    });

                // mock data accessor EDUCATION.GOALS
                dataReaderMock.Setup(a =>
                    a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "EDUCATION.GOALS", true))
                    .ReturnsAsync(new ApplValcodes()
                    {
                        ValInternalCode = new List<string>() { "MA" },
                        ValExternalRepresentation = new List<string>() { educationalGoalDescription },
                        ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "MA",
                            ValExternalRepresentationAssocMember = educationalGoalDescription,
                        }
                    }
                    });
            }

            private PlanningStudentRepository BuildMockPlanningStudentRepository()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(ToCollection(people.Where(x => s.Contains(x.Recordkey)))));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(ToCollection(students.Where(x => s.Contains(x.Recordkey)))));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(ToCollection(personStRecords.Where(x => s.Contains(x.Recordkey)))));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(studentPrograms);

                //DataReader mock setup for BulkReadRecordWithInvalidRecordsAsync
                //for person
                dataReaderMock.Setup(
               accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true))
               .Returns<string[], bool>((s, b) =>
               {
                   return Task.FromResult(new BulkReadOutput<Base.DataContracts.Person>()
                   {
                       BulkRecordsRead = ToCollection(people.Where(x => s.Contains(x.Recordkey)))
                   });
               });

                //for students

                dataReaderMock.Setup(
              accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true))
              .Returns<string[], bool>((s, b) =>
              {
                  return Task.FromResult(new BulkReadOutput<Student.DataContracts.Students>()
                  {
                      BulkRecordsRead = ToCollection(students.Where(x => s.Contains(x.Recordkey)))
                  });
              });

                //for personst
                dataReaderMock.Setup(
             accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true))
             .Returns<string[], bool>((s, b) =>
             {
                 return Task.FromResult(new BulkReadOutput<Base.DataContracts.PersonSt>()
                 {
                     BulkRecordsRead = ToCollection(personStRecords.Where(x => s.Contains(x.Recordkey)))
                 });
             });

                //student programs
                dataReaderMock.Setup(
             accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Student.DataContracts.StudentPrograms>(It.IsAny<string[]>(), true))
             .Returns<string[], bool>((s, b) =>
             {
                 return Task.FromResult(new BulkReadOutput<Student.DataContracts.StudentPrograms>()
                 {
                     BulkRecordsRead = studentPrograms
                 });
             });

                // mock data accessor DEGREE_PLANS response  - for student 0000001
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId1 + "'")).ReturnsAsync(new string[] { "1" });

                // mock data accessor DEGREE_PLANS response  - for student 0000002
                var multipleDegreePlanKeys = new string[] { "2", "3" };
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId2 + "'")).ReturnsAsync(multipleDegreePlanKeys);

                // mock data accessor DEGREE_PLANS response  - for student 0000003
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId3 + "'")).ReturnsAsync(new string[] { });

                // mock data accessor STUDENT.ADVISEMENT for students with no advisor
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(new List<string>().ToArray(), true)).ReturnsAsync(new Collection<Student.DataContracts.StudentAdvisement>());
                dataReaderMock.Setup(
             accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Student.DataContracts.StudentAdvisement>(It.IsAny<string[]>(), true))
             .Returns<string[], bool>((s, b) =>
             {
                 return Task.FromResult(new BulkReadOutput<Student.DataContracts.StudentAdvisement>()
                 {
                     BulkRecordsRead = new Collection<Student.DataContracts.StudentAdvisement>()
                 });
             });

                // mock data accessor STUDENT.ADVISEMENT for student 0000002
                string[] student2AdvismentIds = new List<string>() { "21", "22" }.ToArray();
                dataReaderMock.Setup(a => a.SelectAsync("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(student2AdvismentIds);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(student2AdvismentIds, true)).ReturnsAsync(studentAdvisements);
                dataReaderMock.Setup(
              accessor => accessor.BulkReadRecordWithInvalidRecordsAsync<Student.DataContracts.StudentAdvisement>(student2AdvismentIds, true))
              .Returns<string[], bool>((s, b) =>
              {
                  return Task.FromResult(new BulkReadOutput<Student.DataContracts.StudentAdvisement>()
                  {
                      BulkRecordsRead = studentAdvisements
                  });
              });
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                PlanningStudentRepository repository = new PlanningStudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return repository;
            }
        }
    }

}