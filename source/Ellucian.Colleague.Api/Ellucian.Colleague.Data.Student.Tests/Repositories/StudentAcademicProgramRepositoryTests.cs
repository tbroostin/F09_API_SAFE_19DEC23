// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentAcademicProgramRepositoryTests
    {
        [TestClass]
        public class StudentAcademicProgramRepositoryTests_GET_GETALL : BaseRepositorySetup
        {
            #region DECLARATIONS

            private StudentAcademicProgramRepository studentAcademicProgramRepository;

            private Dictionary<string, GuidLookupResult> dicResult;
            private StudentPrograms studentPrograms;
            private Collection<AcadPrograms> academicPrograms;
            private Collection<AcadCredentials> academicCredentials;
            private Collection<InstitutionsAttend> institutionsAttendData;
            private ApplValcodes applValcodes;
            private Students students;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";
            private string defaultInstitutionId = "1";
            private string[] ids;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                studentAcademicProgramRepository = new StudentAcademicProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                ids = new string[] { "1" };

                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "STUDENT.ACAD.PROGRAMS", PrimaryKey = "1*1" } } };

                academicPrograms = new Collection<AcadPrograms>()
                {
                    new AcadPrograms()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        AcpgMajors = new List<string>() { "1" },
                        AcpgMinors = new List<string>() { "1" },
                        AcpgSpecializations = new List<string>() { "1" },
                        AcpgCcds = new List<string>() { "1" },
                    }
                };

                academicCredentials = new Collection<AcadCredentials>()
                {
                    new AcadCredentials()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        AcadPersonId = "1",
                        AcadAcadProgram = "1",
                        AcadHonors = new List<string>() { "1" },
                        AcadMajors = new List<string>() { "1" },
                        AcadMinors = new List<string>() { "1" },
                        AcadSpecialization = new List<string>() { "1" },
                        AcadCcd = new List<string>() { "1" }
                    }
                };

                applValcodes = new ApplValcodes()
                {
                    Recordkey = "1",
                    ValsEntityAssociation = new List<ApplValcodesVals> {
                new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "A",
                            ValExternalRepresentationAssocMember = "Active",
                            ValActionCode1AssocMember = "2"
                        }
                    }
                };

                students = new Students()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    StuAcadPrograms = new List<string>() { "1" }
                };

                institutionsAttendData = new Collection<InstitutionsAttend>
                {
                    new InstitutionsAttend() {Recordkey="1*1", InstaAcadCredentials = new List<string> {"1" } }
                };
                studentPrograms = new StudentPrograms()
                {
                    RecordGuid = guid,
                    Recordkey = "1*1",
                    StprCatalog = "1",
                    StprStatus = new List<string>() { "active" },
                    StprEndDate = new List<DateTime?>() { DateTime.Today },
                    StprStartDate = new List<DateTime?>() { DateTime.Today },
                    StprAntCmplDate = DateTime.Today,
                    StprDept = "1",
                    StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>()
                    {
                        new StudentProgramsStprMajorList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>()
                    {
                        new StudentProgramsStprMinorList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>()
                    {
                        new StudentProgramsStprSpecialties("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1"),
                        new StudentProgramsStprSpecialties("2", DateTime.Today.AddDays(10), DateTime.Today.AddDays(100), "2")  //future dated spec
                    },
                    StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>()
                    {
                        new StudentProgramsStprCcdList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    }
                };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ReturnsAsync(studentPrograms);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(academicPrograms);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", It.IsAny<string[]>(), true)).ReturnsAsync(academicCredentials);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true)).ReturnsAsync(applValcodes);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Students>(It.IsAny<string>(), true)).ReturnsAsync(students);
                dataReaderMock.Setup(r => r.ReadRecordColumnsAsync("STUDENTS", It.IsAny<string>(),It.IsAny<string[]>())).ReturnsAsync(new Dictionary<string, string>() { { "STU.ACAD.PROGRAMS", "1" } });
                dataReaderMock.Setup(r => r.ReadRecordColumnsAsync("APPLICANTS", It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(new Dictionary<string, string>() { { "APP.APPLICATIONS", "1" } });
                dataReaderMock.Setup(r => r.BatchReadRecordColumnsAsync("APPLICATIONS", It.IsAny<string[]>(), It.IsAny<string[]>())).ReturnsAsync(new Dictionary<string, Dictionary<string, string>>() { { "1", new Dictionary<string, string>() { { "STU.ACAD.PROGRAMS", "1" } } } });
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.PROGRAMS", It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(new string[] { "1*1" });
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>())).ReturnsAsync(new string[] { "1*1" });
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1*1" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentPrograms>() { studentPrograms });
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<InstitutionsAttend>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(institutionsAttendData));
                dataReaderMock.Setup(r => r.ReadRecordAsync<InstitutionsAttend>(It.IsAny<string>(), true)).ReturnsAsync(institutionsAttendData[0]);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramByGuidAsync_ArgumentNullException_When_Guid_Null()
            {
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(null, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramByGuidAsync_Invalid_StudentAcademicProgramId()
            {
                dicResult[guid].PrimaryKey = null;
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(guid, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramByGuidAsync_StudentProgram_NotAvailable_For_Guid()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ReturnsAsync(null);
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(guid, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramByGuidAsync_Exception()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ThrowsAsync(new Exception());
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(guid, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramByGuid2Async_ArgumentNullException_When_Guid_Null()
            {
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuid2Async(null, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramByGuid2Async_Invalid_StudentAcademicProgramId()
            {
                dicResult[guid].PrimaryKey = null;
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuid2Async(guid, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramByGuid2Async_StudentProgram_NotAvailable_For_Guid()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ReturnsAsync(null);
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuid2Async(guid, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramByGuid2Async_Exception()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ThrowsAsync(new Exception());
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuid2Async(guid, defaultInstitutionId);
            }


            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramByGuidAsync()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ReturnsAsync(studentPrograms);
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(guid, defaultInstitutionId);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramByGuid2Async()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ReturnsAsync(studentPrograms);

                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuid2Async(guid, defaultInstitutionId);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicProgramsAsync()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms2Async()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), ids, ids, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms2Async_Null_CredList()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms2Async_Null_CatalogAndProgram_NotNull_NoCount()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, "123", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(result.Item2, 0);
            }


            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms2Async_Null_CatalogAndProgram_StuProgsLimitingKeys_Null()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), "", "123", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms2Async_Null_Catalog_NoCount()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, "", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms2Async_NoStudentDataContract()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Students>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramsAsync_RepositoryException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Students>(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramsAsync_ArgumentException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Students>(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms2Async_RepositoryException()
            {
                List<string> ids = new List<string>() { "1", "2" };
                var results = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), ids, ids, guid, "active");

                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicProgramsAsync_Invalid_Statuses_From_Repository()
            {
                studentPrograms.StprStatus = new List<string>() { };

                var results = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, "", new List<string> { guid }, new List<string> { guid }, guid, "active");

                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_ProgramFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, program: progcode);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_StartDateFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, startDate: studentProgramStartDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_EndDateFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, endDate: studentProgramEndDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }


            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_StartEndDateFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, endDate: studentProgramEndDate.ToString(), startDate: studentProgramStartDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_StudentFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, student: studentid);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }


            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_GraduatedAcademicPeriodFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, graduatedAcademicPeriod: "1");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_CatalogFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, catalog: studentPrograms.StprCatalog);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_StatusFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, status: "active");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }


            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_ProgramOwnerFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, programOwner: studentPrograms.StprDept);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_SiteFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, site: studentPrograms.StprLocation);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_GraduateOnFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                academicCredentials.FirstOrDefault().AcadEndDate = DateTime.Today;

                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, graduatedOn: academicCredentials.FirstOrDefault().AcadEndDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }


            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_StudentGraduateOnFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                academicCredentials.FirstOrDefault().AcadEndDate = DateTime.Today;

                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, student: studentid, graduatedOn: academicCredentials.FirstOrDefault().AcadEndDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms2Async_DegreeCredentialsFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1,
                    true, degreeCredentials: new List<string> { "1" });

                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.NotSet, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }
        }

        [TestClass]
        public class StudentAcademicProgramRepositoryTests_POST_AND_PUT : BaseRepositorySetup
        {
            #region DECLARATIONS

            private StudentAcademicProgramRepository studentAcademicProgramRepository;

            private Dictionary<string, GuidLookupResult> dicResult;
            private StudentPrograms studentPrograms;
            private Collection<AcadPrograms> academicPrograms;
            private Collection<AcadCredentials> academicCredentials;
            private ApplValcodes applValcodes;
            private Students students;
            private Collection<InstitutionsAttend> institutionsAttendData;

            private StudentAcademicProgram studentAcademicProgram;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";
            private string defaultInstitutionId = "1";
            private string[] ids;

            UpdateStuAcadProgramResponse response;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                studentAcademicProgramRepository = new StudentAcademicProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                response = new UpdateStuAcadProgramResponse() { StuProgGuid = guid };

                ids = new string[] { "1" };

                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "STUDENT.PROGRAMS", PrimaryKey = "1" } } };

                academicPrograms = new Collection<AcadPrograms>()
                {
                    new AcadPrograms()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        AcpgMajors = new List<string>() { "1" },
                        AcpgMinors = new List<string>() { "1" },
                        AcpgSpecializations = new List<string>() { "1" },
                        AcpgCcds = new List<string>() { "1" },
                    }
                };

                academicCredentials = new Collection<AcadCredentials>()
                {
                    new AcadCredentials()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        AcadPersonId = "1",
                        AcadAcadProgram = "1",
                        AcadHonors = new List<string>() { "1" },
                        AcadMajors = new List<string>() { "1" },
                        AcadMinors = new List<string>() { "1" },
                        AcadSpecialization = new List<string>() { "1" },
                        AcadCcd = new List<string>() { "1" }
                    }
                };

                applValcodes = new ApplValcodes()
                {
                    Recordkey = "1",
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                       new ApplValcodesVals("1", "2", "4", "4", "5", "6", "7"),
                       new ApplValcodesVals("1", "2", "2", "4", "5", "6", "7")
                    }
                };

                students = new Students()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    StuAcadPrograms = new List<string>() { "1" }
                };

                institutionsAttendData = new Collection<InstitutionsAttend>
                {
                    new InstitutionsAttend() {Recordkey="1*1", InstaAcadCredentials = new List<string> {"1" } }
                };
                studentPrograms = new StudentPrograms()
                {
                    RecordGuid = guid,
                    Recordkey = "1*1",
                    StprCatalog = "1",
                    StprStatus = new List<string>() { "active" },
                    StprEndDate = new List<DateTime?>() { DateTime.Today },
                    StprStartDate = new List<DateTime?>() { DateTime.Today },
                    StprAntCmplDate = DateTime.Today,
                    StprDept = "1",
                    StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>()
                    {
                        new StudentProgramsStprMajorList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>()
                    {
                        new StudentProgramsStprMinorList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>()
                    {
                        new StudentProgramsStprSpecialties("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>()
                    {
                        new StudentProgramsStprCcdList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    }
                };

                studentAcademicProgram = new StudentAcademicProgram("1", "1", "1", guid, DateTime.Today, "active");
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ReturnsAsync(studentPrograms);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(academicPrograms);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", It.IsAny<string[]>(), true)).ReturnsAsync(academicCredentials);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true)).ReturnsAsync(applValcodes);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Students>(It.IsAny<string>(), true)).ReturnsAsync(students);
                dataReaderMock.SetupSequence(r => r.Select(It.IsAny<GuidLookup[]>())).Returns(dicResult);

                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.PROGRAMS", It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>())).ReturnsAsync(new string[] { "1*1" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentPrograms>() { studentPrograms });
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<InstitutionsAttend>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(institutionsAttendData));
                dataReaderMock.Setup(r => r.ReadRecordAsync<InstitutionsAttend>(It.IsAny<string>(), true)).ReturnsAsync(institutionsAttendData[0]);
                transManagerMock.Setup(t => t.ExecuteAsync<UpdateStuAcadProgramRequest, UpdateStuAcadProgramResponse>(It.IsAny<UpdateStuAcadProgramRequest>())).ReturnsAsync(response);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAcademicProgramRepositoryTests_CreateStudentAcademicProgramAsync_ArgumentNullException_When_Guid_Null()
            {
                await studentAcademicProgramRepository.CreateStudentAcademicProgramAsync(null, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_CreateStudentAcademicProgramAsync_RepositoryException()
            {
                response.Error = true;
                response.UpdateStuAcadProgramError = new List<UpdateStuAcadProgramError>()
                {
                    new UpdateStuAcadProgramError() { ErrorCode = "ERROR", ErrorMessage = "MESSAGE" },
                    new UpdateStuAcadProgramError() { ErrorMessage = "MESSAGE", }
                };
                await studentAcademicProgramRepository.CreateStudentAcademicProgramAsync(studentAcademicProgram, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAcademicProgramRepositoryTests_UpdateStudentAcademicProgramAsync_ArgumentNullException_When_Guid_Null()
            {
                await studentAcademicProgramRepository.UpdateStudentAcademicProgramAsync(null, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_UpdateStudentAcademicProgramAsync_RepositoryException()
            {
                response.Error = true;
                response.UpdateStuAcadProgramError = new List<UpdateStuAcadProgramError>()
                {
                    new UpdateStuAcadProgramError() { ErrorCode = "ERROR", ErrorMessage = "MESSAGE" },
                    new UpdateStuAcadProgramError() { ErrorMessage = "MESSAGE", }
                };
                await studentAcademicProgramRepository.UpdateStudentAcademicProgramAsync(studentAcademicProgram, defaultInstitutionId);
            }
        }

        [TestClass]
        public class StudentAcademicProgramRepositoryTests_GET_GETALL_V3 : BaseRepositorySetup
        {
            #region DECLARATIONS

            private StudentAcademicProgramRepository studentAcademicProgramRepository;

            private Dictionary<string, GuidLookupResult> dicResult;
            private StudentPrograms studentPrograms;
            private Collection<AcadPrograms> academicPrograms;
            private Collection<AcadCredentials> academicCredentials;
            private Collection<InstitutionsAttend> institutionsAttendData;
            private ApplValcodes applValcodes;
            private Students students;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";
            private string defaultInstitutionId = "1";
            private string[] ids;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                studentAcademicProgramRepository = new StudentAcademicProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                ids = new string[] { "1" };

                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "STUDENT.ACAD.PROGRAMS", PrimaryKey = "1" } } };

                academicPrograms = new Collection<AcadPrograms>()
                {
                    new AcadPrograms()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        AcpgMajors = new List<string>() { "1" },
                        AcpgMinors = new List<string>() { "1" },
                        AcpgSpecializations = new List<string>() { "1" },
                        AcpgCcds = new List<string>() { "1" },
                    }
                };

                academicCredentials = new Collection<AcadCredentials>()
                {
                    new AcadCredentials()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        AcadPersonId = "1",
                        AcadAcadProgram = "1",
                        AcadHonors = new List<string>() { "1" },
                        AcadMajors = new List<string>() { "1" },
                        AcadMinors = new List<string>() { "1" },
                        AcadSpecialization = new List<string>() { "1" },
                        AcadCcd = new List<string>() { "1" }
                    }
                };

                applValcodes = new ApplValcodes()
                {
                    Recordkey = "1",
                    ValsEntityAssociation = new List<ApplValcodesVals> {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "A",
                            ValExternalRepresentationAssocMember = "Active",
                            ValActionCode1AssocMember = "2"
                        },
                        new ApplValcodesVals()
                        {
                           ValInternalCodeAssocMember = "G",
                            ValExternalRepresentationAssocMember = "Graduated",
                            ValActionCode1AssocMember = "3"
                        },
                        new ApplValcodesVals()
                        {
                           ValInternalCodeAssocMember = "C",
                            ValExternalRepresentationAssocMember = "Changed Program",
                            ValActionCode1AssocMember = "4"
                        },
                         new ApplValcodesVals()
                         {
                            ValInternalCodeAssocMember = "W",
                            ValExternalRepresentationAssocMember = "Withdrawn",
                            ValActionCode1AssocMember = "5"
                        }
                    }
                };

                students = new Students()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    StuAcadPrograms = new List<string>() { "1" }
                };

                institutionsAttendData = new Collection<InstitutionsAttend>
                {
                    new InstitutionsAttend() {Recordkey="1*1", InstaAcadCredentials = new List<string> {"1" } }
                };
                studentPrograms = new StudentPrograms()
                {
                    RecordGuid = guid,
                    Recordkey = "1*1",
                    StprCatalog = "1",
                    StprStatus = new List<string>() { "active" },
                    StprEndDate = new List<DateTime?>() { DateTime.Today },
                    StprStartDate = new List<DateTime?>() { DateTime.Today },
                    StprAntCmplDate = DateTime.Today,
                    StprDept = "1",
                    StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>()
                    {
                        new StudentProgramsStprMajorList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>()
                    {
                        new StudentProgramsStprMinorList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>()
                    {
                        new StudentProgramsStprSpecialties("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>()
                    {
                        new StudentProgramsStprCcdList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    }
                };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ReturnsAsync(studentPrograms);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(academicPrograms);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", It.IsAny<string[]>(), true)).ReturnsAsync(academicCredentials);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true)).ReturnsAsync(applValcodes);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Students>(It.IsAny<string>(), true)).ReturnsAsync(students);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.PROGRAMS", It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>())).ReturnsAsync(new string[] { "1*1" });
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1*1" });

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentPrograms>() { studentPrograms });

                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.StudentPrograms>() { BulkRecordsRead = new Collection<StudentPrograms>() { studentPrograms } };
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);


                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<InstitutionsAttend>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(institutionsAttendData));
                dataReaderMock.Setup(r => r.ReadRecordAsync<InstitutionsAttend>(It.IsAny<string>(), true)).ReturnsAsync(institutionsAttendData[0]);

                //var ids = new List<string>() { studentPrograms.Recordkey };
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllStudentAcademicPrograms:",
                    Entity = "STUDENT.PROGRAMSs",
                    Sublist = ids.ToList(),
                    TotalCount = ids.ToList().Count,
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
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
            }

            #endregion

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_NoFilters()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");

                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");


            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_StatusChanged()
            {
                studentPrograms.StprStatus = null;
                studentPrograms.StprEndDate = new List<DateTime?>() { DateTime.Today.AddDays(-1) };


                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual("C", studentAcademicProgram.Status, "status");


            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_StatusActive()
            {
                studentPrograms.StprStatus = null;
                studentPrograms.StprEndDate = null;


                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual("A", studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_Recruited()
            {
                studentPrograms.StprStartDate = new List<DateTime?>() { new DateTime() };
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                dataReaderMock.Setup(r =>
                    r.SelectAsync("APPLICATIONS",
                    "WITH APPL.APPLICANT EQ '" + studentid + "' AND WITH APPL.ACAD.PROGRAM EQ '" + progcode + "'"))
                    .ReturnsAsync(new string[] { "1", "2" });


                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(CurriculumObjectiveCategory.Recruited, studentAcademicProgram.CurriculumObjective, "objective");

            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_EmptyTuple()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), ids, ids, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async_Null_CredList()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async_Null_CatalogAndProgram_NotNull_NoCount()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                dataReaderMock.Setup(repo => repo.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, "123", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(result.Item2, 0);
            }

            //[TestMethod]
            //public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async_Null_CatalogAndProgram_StuProgsLimitingKeys_Null()
            //{
            //    academicCredentials.FirstOrDefault().AcadPersonId = "2";
            //    List<string> ids = new List<string>() { "1", "2" };
            //    dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
            //    var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
            //        DateTime.Today.AddDays(100).ToString(), "", "123", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

            //    Assert.AreEqual(result.Item2, 0);
            //}

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async_Null_Catalog_NoCount()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                dataReaderMock.Setup(repo => repo.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, "", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async_NoStudentDataContract()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Students>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                dataReaderMock.Setup(repo => repo.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual( 0, result.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async_RepositoryException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Students>(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async_RepositoryException_InvalidKeys ()
            {
                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.StudentPrograms>() {InvalidKeys = new string[] { "1" } , InvalidRecords = new Dictionary<string, string>()};
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);

                 var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async_RepositoryException_InvalidRecords()
            {
                var invalidRecords = new Dictionary<string, string>();
                invalidRecords.Add("1", "invalid data");

                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.StudentPrograms>() { InvalidRecords = invalidRecords, InvalidKeys = new string[] { } };
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async_ArgumentException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Students>(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms3Async_Invalid_Statuses_From_Repository()
            {
                studentPrograms.StprStatus = new List<string>() { };

                var results = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, "", new List<string> { guid }, new List<string> { guid }, guid, "active");

                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_ProgramFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, program: progcode);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_StartDateFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, startDate: studentProgramStartDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_EndDateFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, endDate: studentProgramEndDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_StartEndDateFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, endDate: studentProgramEndDate.ToString(), startDate: studentProgramStartDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_StudentFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, student: studentid);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_GraduatedAcademicPeriodFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, graduatedAcademicPeriod: "1");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_CatalogFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, catalog: studentPrograms.StprCatalog);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_StatusFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, status: "active");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_ProgramOwnerFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, programOwner: studentPrograms.StprDept);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_SiteFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, site: studentPrograms.StprLocation);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_GraduateOnFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                academicCredentials.FirstOrDefault().AcadEndDate = DateTime.Today;

                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, graduatedOn: academicCredentials.FirstOrDefault().AcadEndDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_StudentGraduateOnFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                academicCredentials.FirstOrDefault().AcadEndDate = DateTime.Today;

                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, student: studentid,  graduatedOn: academicCredentials.FirstOrDefault().AcadEndDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            //[TestMethod]
            //public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_DegreeCredentialsFilter()
            //{
            //    academicCredentials.FirstOrDefault().AcadPersonId = "2";
            //    string studentid = studentPrograms.Recordkey.Split('*')[0];
            //    string progcode = studentPrograms.Recordkey.Split('*')[1];

            //    var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
            //    var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);
            //    var ids = new List<string> { "1", "2" };
            //    dataReaderMock.Setup(repo => repo.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2"});
               
            //    var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
            //        true, degreeCredentials: new List<string> { "1" });

            //    Assert.IsNotNull(result);
            //    Assert.AreEqual(2, result.Item2);
            //    Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

            //    var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
            //    Assert.IsNotNull(studentAcademicProgram);
            //    Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
            //    Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
            //    Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
            //    Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
            //    Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
            //    Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
            //    Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
            //    Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
            //    Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
            //    Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            //}

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_MatriculatedFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, curriculumObjective: CurriculumObjectiveCategory.Matriculated);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_OutcomeFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                studentPrograms.StprStatus = new List<string>() { "G" };

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, curriculumObjective: CurriculumObjectiveCategory.Outcome);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Outcome, studentAcademicProgram.CurriculumObjective, "objective");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_RecruitedFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];
                studentPrograms.StprStatus = new List<string>() { "R" };
                studentPrograms.StprStartDate = new List<DateTime?>() { new DateTime() };

                dataReaderMock.Setup(r =>
                    r.SelectAsync("APPLICATIONS",
                    "WITH APPL.APPLICANT EQ '" + studentid + "' AND WITH APPL.ACAD.PROGRAM EQ '" + progcode + "' SAVING APPL.STATUS"))
                    .ReturnsAsync(new string[] { "R" });

                var appStatuses = new Collection<ApplicationStatuses>()
                {  new ApplicationStatuses() { RecordGuid = Guid.NewGuid().ToString(),
                    Recordkey = "AP", AppsSpecialProcessingCode = "3", AppsDesc = "Applied" },
                    new ApplicationStatuses() { RecordGuid = Guid.NewGuid().ToString(),
                    Recordkey = "R", AppsSpecialProcessingCode = "", AppsDesc = "Recruited" }};

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<ApplicationStatuses>("APPLICATION.STATUSES",
                    It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(appStatuses);

                dataReaderMock.SetupSequence(r =>
                    r.SelectAsync("APPLICATIONS", It.IsAny<string>(), It.IsAny<string[]>(), "?", It.IsAny<bool>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new string[] { studentid + "*" + progcode }))
                    .Returns(Task.FromResult(new string[] { }))
                    .Throws<InvalidOperationException>();

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, curriculumObjective: CurriculumObjectiveCategory.Recruited);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Recruited, studentAcademicProgram.CurriculumObjective, "objective");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms3Async_AppliedFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];
                studentPrograms.StprStatus = new List<string>() { "A" };
                studentPrograms.StprStartDate = new List<DateTime?>() { new DateTime() };


                var appStatuses = new Collection<ApplicationStatuses>()
                {  new ApplicationStatuses() { RecordGuid = Guid.NewGuid().ToString(),
                    Recordkey = "A", AppsSpecialProcessingCode = "3", AppsDesc = "Applied" }};


                dataReaderMock.Setup(r =>
                    r.SelectAsync("APPLICATIONS",
                    "WITH APPL.APPLICANT EQ '" + studentid + "' AND WITH APPL.ACAD.PROGRAM EQ '" + progcode + "' SAVING APPL.STATUS"))
                    .ReturnsAsync(new string[] { "A" });

                // var applicationDataContract = await DataReader.BulkReadRecordAsync<Applications>("APPLICATIONS", "WITH APPL.APPLICANT EQ '" + studentid + "' AND WITH APPL.ACAD.PROGRAM EQ '" + progcode + "'");
                Applications application = new Applications()
                { RecordGuid = Guid.NewGuid().ToString(), Recordkey = "1", ApplAcadProgram = progcode, ApplApplicant = studentid };
                application.ApplStatusesEntityAssociation = new List<ApplicationsApplStatuses>()
                {  new ApplicationsApplStatuses("A", DateTime.Now, "", DateTime.Now)};

                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns(Task.FromResult(new Collection<Applications> { application }));



                dataReaderMock.Setup(d => d.BulkReadRecordAsync<ApplicationStatuses>("APPLICATION.STATUSES",
                    It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(appStatuses);
             

                dataReaderMock.SetupSequence(r =>
                    r.SelectAsync("APPLICATIONS", It.IsAny<string>(), It.IsAny<string[]>(), "?", It.IsAny<bool>(), It.IsAny<int>()))                
                    .Returns(Task.FromResult(new string[] { studentid + "*" + progcode }))
                    .Returns(Task.FromResult(new string[] { }))
                    .Throws<InvalidOperationException>(); 
                  

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, 0, 1,
                    true, curriculumObjective: CurriculumObjectiveCategory.Applied);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Applied, studentAcademicProgram.CurriculumObjective, "objective");
            }
        }


        [TestClass]
        public class StudentAcademicProgramRepositoryTests_GET_GETALL_V4 : BaseRepositorySetup
        {
            #region DECLARATIONS

            private StudentAcademicProgramRepository studentAcademicProgramRepository;

            private Dictionary<string, GuidLookupResult> dicResult;
            private StudentPrograms studentPrograms;
            private Collection<AcadPrograms> academicPrograms;
            private Collection<AcadCredentials> academicCredentials;
            private Collection<InstitutionsAttend> institutionsAttendData;
            private ApplValcodes applValcodes;
            private Students students;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";
            private string defaultInstitutionId = "1";
            private string[] ids;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                studentAcademicProgramRepository = new StudentAcademicProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                ids = new string[] { "1" };

                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "STUDENT.ACAD.PROGRAMS", PrimaryKey = "1" } } };

                academicPrograms = new Collection<AcadPrograms>()
                {
                    new AcadPrograms()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        AcpgMajors = new List<string>() { "1" },
                        AcpgMinors = new List<string>() { "1" },
                        AcpgSpecializations = new List<string>() { "1" },
                        AcpgCcds = new List<string>() { "1" },
                    }
                };

                academicCredentials = new Collection<AcadCredentials>()
                {
                    new AcadCredentials()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        AcadPersonId = "1",
                        AcadAcadProgram = "1",
                        AcadHonors = new List<string>() { "1" },
                        AcadMajors = new List<string>() { "1" },
                        AcadMinors = new List<string>() { "1" },
                        AcadSpecialization = new List<string>() { "1" },
                        AcadCcd = new List<string>() { "1" }
                    }
                };

                applValcodes = new ApplValcodes()
                {
                    Recordkey = "1",
                    ValsEntityAssociation = new List<ApplValcodesVals> {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "A",
                            ValExternalRepresentationAssocMember = "Active",
                            ValActionCode1AssocMember = "2"
                        },
                        new ApplValcodesVals()
                        {
                           ValInternalCodeAssocMember = "G",
                            ValExternalRepresentationAssocMember = "Graduated",
                            ValActionCode1AssocMember = "3"
                        },
                        new ApplValcodesVals()
                        {
                           ValInternalCodeAssocMember = "C",
                            ValExternalRepresentationAssocMember = "Changed Program",
                            ValActionCode1AssocMember = "4"
                        },
                         new ApplValcodesVals()
                         {
                            ValInternalCodeAssocMember = "W",
                            ValExternalRepresentationAssocMember = "Withdrawn",
                            ValActionCode1AssocMember = "5"
                        }
                    }
                };

                students = new Students()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    StuAcadPrograms = new List<string>() { "1" }
                };

                institutionsAttendData = new Collection<InstitutionsAttend>
                {
                    new InstitutionsAttend() {Recordkey="1*1", InstaAcadCredentials = new List<string> {"1" } }
                };
                studentPrograms = new StudentPrograms()
                {
                    RecordGuid = guid,
                    Recordkey = "1*1",
                    StprCatalog = "1",
                    StprStatus = new List<string>() { "active" },
                    StprEndDate = new List<DateTime?>() { DateTime.Today },
                    StprStartDate = new List<DateTime?>() { DateTime.Today },
                    StprAntCmplDate = DateTime.Today,
                    StprDept = "1",
                    StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>()
                    {
                        new StudentProgramsStprMajorList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>()
                    {
                        new StudentProgramsStprMinorList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>()
                    {
                        new StudentProgramsStprSpecialties("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>()
                    {
                        new StudentProgramsStprCcdList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    }
                };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ReturnsAsync(studentPrograms);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(academicPrograms);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", It.IsAny<string[]>(), true)).ReturnsAsync(academicCredentials);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true)).ReturnsAsync(applValcodes);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Students>(It.IsAny<string>(), true)).ReturnsAsync(students);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.PROGRAMS", It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>())).ReturnsAsync(new string[] { "1*1" });
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1*1" });

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentPrograms>() { studentPrograms });

                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.StudentPrograms>() { BulkRecordsRead = new Collection<StudentPrograms>() { studentPrograms } };
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);


                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<InstitutionsAttend>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(institutionsAttendData));
                dataReaderMock.Setup(r => r.ReadRecordAsync<InstitutionsAttend>(It.IsAny<string>(), true)).ReturnsAsync(institutionsAttendData[0]);

                //var ids = new List<string>() { studentPrograms.Recordkey };
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllStudentAcademicPrograms:",
                    Entity = "STUDENT.PROGRAMS",
                    Sublist = ids.ToList(),
                    TotalCount = ids.ToList().Count,
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
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
            }

            #endregion

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_NoFilters()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                var ids = new List<string>() { studentPrograms.Recordkey };
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllStudentAcademicPrograms:",
                    Entity = "STUDENT.PROGRAMS",
                    Sublist = ids,
                    TotalCount = ids.Count,
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
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");

                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");


            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_StatusChanged()
            {
                studentPrograms.StprStatus = null;
                studentPrograms.StprEndDate = new List<DateTime?>() { DateTime.Today.AddDays(-1) };


                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual("C", studentAcademicProgram.Status, "status");


            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_StatusActive()
            {
                studentPrograms.StprStatus = null;
                studentPrograms.StprEndDate = null;


                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual("A", studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_Recruited()
            {
                studentPrograms.StprStartDate = new List<DateTime?>() { new DateTime() };
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                dataReaderMock.Setup(r =>
                    r.SelectAsync("APPLICATIONS",
                    "WITH APPL.APPLICANT EQ '" + studentid + "' AND WITH APPL.ACAD.PROGRAM EQ '" + progcode + "'"))
                    .ReturnsAsync(new string[] { "1", "2" });


                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(CurriculumObjectiveCategory.Recruited, studentAcademicProgram.CurriculumObjective, "objective");

            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_EmptyTuple()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllStudentAcademicPrograms:",
                    Entity = "STUDENT.PROGRAMS",
                    Sublist = ids,
                    TotalCount = ids.Count,
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
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true, "1", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), ids, ids, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async_Null_CredList()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async_Null_CatalogAndProgram_NotNull_NoCount()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { };
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllStudentAcademicPrograms:",
                    Entity = "STUDENT.PROGRAMS",
                    Sublist = ids,
                    TotalCount = ids.Count,
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
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, "123", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async_Null_CatalogAndProgram_StuProgsLimitingKeys_Null()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { };
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllStudentAcademicPrograms:",
                    Entity = "STUDENT.PROGRAMS",
                    Sublist = ids,
                    TotalCount = ids.Count,
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
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), "", "123", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async_Null_Catalog_NoCount()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { };
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllStudentAcademicPrograms:",
                    Entity = "STUDENT.PROGRAMS",
                    Sublist = ids,
                    TotalCount = ids.Count,
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
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, "", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async_NoStudentDataContract()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
              
                List<string> ids = new List<string>() {  };
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllStudentAcademicPrograms:",
                    Entity = "STUDENT.PROGRAMS",
                    Sublist = ids,
                    TotalCount = ids.Count,
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
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Students>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async_RepositoryException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Students>(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async_RepositoryException_InvalidKeys()
            {
                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.StudentPrograms>() { InvalidKeys = new string[] { "1" }, InvalidRecords = new Dictionary<string, string>() };
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async_RepositoryException_InvalidRecords()
            {
                var invalidRecords = new Dictionary<string, string>();
                invalidRecords.Add("1", "invalid data");

                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.StudentPrograms>() { InvalidRecords = invalidRecords, InvalidKeys = new string[] { } };
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async_ArgumentException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Students>(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_GetStudentAcademicPrograms4Async_Invalid_Statuses_From_Repository()
            {
                studentPrograms.StprStatus = new List<string>() { };

                var results = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, "", new List<string> { guid }, new List<string> { guid }, guid, "active");

                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_ProgramFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, program: progcode);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_StartDateFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, startDate: studentProgramStartDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_EndDateFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, endDate: studentProgramEndDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_StartEndDateFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, endDate: studentProgramEndDate.ToString(), startDate: studentProgramStartDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_StudentFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, student: studentid);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_GraduatedAcademicPeriodFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, graduatedAcademicPeriod: "1");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_CatalogFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, catalog: studentPrograms.StprCatalog);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_StatusFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, status: "active");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_ProgramOwnerFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, programOwner: studentPrograms.StprDept);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_SiteFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, site: studentPrograms.StprLocation);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_GraduateOnFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                academicCredentials.FirstOrDefault().AcadEndDate = DateTime.Today;

                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, graduatedOn: academicCredentials.FirstOrDefault().AcadEndDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_StudentGraduateOnFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                academicCredentials.FirstOrDefault().AcadEndDate = DateTime.Today;

                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, student: studentid, graduatedOn: academicCredentials.FirstOrDefault().AcadEndDate.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
                Assert.AreEqual(studentPrograms.StprDept, studentAcademicProgram.DepartmentCode, "deptCode");
                Assert.AreEqual(studentProgramStartDate, studentAcademicProgram.StartDate, "startDate");
                Assert.AreEqual(studentProgramEndDate, studentAcademicProgram.EndDate, "endDate");
                Assert.AreEqual(studentid, studentAcademicProgram.StudentId, "studentId");
                Assert.AreEqual(progcode, studentAcademicProgram.ProgramCode, "programCode");
                Assert.AreEqual(studentPrograms.StprStatus.ElementAt(0), studentAcademicProgram.Status, "status");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_MatriculatedFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, curriculumObjective: CurriculumObjectiveCategory.Matriculated);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Matriculated, studentAcademicProgram.CurriculumObjective, "objective");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_OutcomeFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];

                studentPrograms.StprStatus = new List<string>() { "G" };

                var studentProgramEndDate = studentPrograms.StprEndDate.ElementAt(0);
                var studentProgramStartDate = studentPrograms.StprStartDate.ElementAt(0);

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, curriculumObjective: CurriculumObjectiveCategory.Outcome);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Outcome, studentAcademicProgram.CurriculumObjective, "objective");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_RecruitedFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];
                studentPrograms.StprStatus = new List<string>() { "R" };
                studentPrograms.StprStartDate = new List<DateTime?>() { new DateTime() };

                dataReaderMock.Setup(r =>
                    r.SelectAsync("APPLICATIONS",
                    "WITH APPL.APPLICANT EQ '" + studentid + "' AND WITH APPL.ACAD.PROGRAM EQ '" + progcode + "' SAVING APPL.STATUS"))
                    .ReturnsAsync(new string[] { "R" });

                var appStatuses = new Collection<ApplicationStatuses>()
                {  new ApplicationStatuses() { RecordGuid = Guid.NewGuid().ToString(),
                    Recordkey = "AP", AppsSpecialProcessingCode = "3", AppsDesc = "Applied" },
                    new ApplicationStatuses() { RecordGuid = Guid.NewGuid().ToString(),
                    Recordkey = "R", AppsSpecialProcessingCode = "", AppsDesc = "Recruited" }};

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<ApplicationStatuses>("APPLICATION.STATUSES",
                    It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(appStatuses);

                dataReaderMock.SetupSequence(r =>
                    r.SelectAsync("APPLICATIONS", It.IsAny<string>(), It.IsAny<string[]>(), "?", It.IsAny<bool>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new string[] { studentid + "*" + progcode }))
                    .Returns(Task.FromResult(new string[] { }))
                    .Throws<InvalidOperationException>();

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, curriculumObjective: CurriculumObjectiveCategory.Recruited);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Recruited, studentAcademicProgram.CurriculumObjective, "objective");
            }

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTestsGetStudentAcademicPrograms4Async_AppliedFilter()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                string studentid = studentPrograms.Recordkey.Split('*')[0];
                string progcode = studentPrograms.Recordkey.Split('*')[1];
                studentPrograms.StprStatus = new List<string>() { "A" };
                studentPrograms.StprStartDate = new List<DateTime?>() { new DateTime() };


                var appStatuses = new Collection<ApplicationStatuses>()
                {  new ApplicationStatuses() { RecordGuid = Guid.NewGuid().ToString(),
                    Recordkey = "A", AppsSpecialProcessingCode = "3", AppsDesc = "Applied" }};


                dataReaderMock.Setup(r =>
                    r.SelectAsync("APPLICATIONS",
                    "WITH APPL.APPLICANT EQ '" + studentid + "' AND WITH APPL.ACAD.PROGRAM EQ '" + progcode + "' SAVING APPL.STATUS"))
                    .ReturnsAsync(new string[] { "A" });

                // var applicationDataContract = await DataReader.BulkReadRecordAsync<Applications>("APPLICATIONS", "WITH APPL.APPLICANT EQ '" + studentid + "' AND WITH APPL.ACAD.PROGRAM EQ '" + progcode + "'");
                Applications application = new Applications()
                { RecordGuid = Guid.NewGuid().ToString(), Recordkey = "1", ApplAcadProgram = progcode, ApplApplicant = studentid };
                application.ApplStatusesEntityAssociation = new List<ApplicationsApplStatuses>()
                {  new ApplicationsApplStatuses("A", DateTime.Now, "", DateTime.Now)};

                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns(Task.FromResult(new Collection<Applications> { application }));



                dataReaderMock.Setup(d => d.BulkReadRecordAsync<ApplicationStatuses>("APPLICATION.STATUSES",
                    It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(appStatuses);


                dataReaderMock.SetupSequence(r =>
                    r.SelectAsync("APPLICATIONS", It.IsAny<string>(), It.IsAny<string[]>(), "?", It.IsAny<bool>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new string[] { studentid + "*" + progcode }))
                    .Returns(Task.FromResult(new string[] { }))
                    .Throws<InvalidOperationException>();


                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, 0, 1,
                    true, curriculumObjective: CurriculumObjectiveCategory.Applied);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(typeof(List<StudentAcademicProgram>), result.Item1.GetType());

                var studentAcademicProgram = result.Item1.FirstOrDefault(x => x.Guid == guid);
                Assert.IsNotNull(studentAcademicProgram);
                Assert.AreEqual(studentPrograms.RecordGuid, studentAcademicProgram.Guid, "guid");
                Assert.AreEqual(studentPrograms.StprAntCmplDate, studentAcademicProgram.AnticipatedCompletionDate, "anticipatedCompletionDate");
                Assert.AreEqual(studentPrograms.StprCatalog, studentAcademicProgram.CatalogCode, "catalogCode");
                Assert.AreEqual(CurriculumObjectiveCategory.Applied, studentAcademicProgram.CurriculumObjective, "objective");
            }
        }

        [TestClass]
        public class StudentAcademicProgramRepositoryTests_POST_AND_PUT_V2 : BaseRepositorySetup
        {
            #region DECLARATIONS

            private StudentAcademicProgramRepository studentAcademicProgramRepository;

            private Dictionary<string, GuidLookupResult> dicResult;
            private StudentPrograms studentPrograms;
            private Collection<AcadPrograms> academicPrograms;
            private Collection<AcadCredentials> academicCredentials;
            private ApplValcodes applValcodes;
            private Students students;
            private Collection<InstitutionsAttend> institutionsAttendData;

            private StudentAcademicProgram studentAcademicProgram;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";
            private readonly string defaultInstitutionId = "1";
            private string[] ids;

            UpdateStudentProgramResponse response;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                studentAcademicProgramRepository = new StudentAcademicProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                response = new UpdateStudentProgramResponse() { StuProgGuid = guid };

                ids = new string[] { "1" };

                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "STUDENT.PROGRAMS", PrimaryKey = "1*1" } } };

                academicPrograms = new Collection<AcadPrograms>()
                {
                    new AcadPrograms()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        AcpgMajors = new List<string>() { "1" },
                        AcpgMinors = new List<string>() { "1" },
                        AcpgSpecializations = new List<string>() { "1" },
                        AcpgCcds = new List<string>() { "1" },
                    }
                };

                academicCredentials = new Collection<AcadCredentials>()
                {
                    new AcadCredentials()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        AcadPersonId = "1",
                        AcadAcadProgram = "1",
                        AcadHonors = new List<string>() { "1" },
                        AcadMajors = new List<string>() { "1" },
                        AcadMinors = new List<string>() { "1" },
                        AcadSpecialization = new List<string>() { "1" },
                        AcadCcd = new List<string>() { "1" }
                    }
                };

                applValcodes = new ApplValcodes()
                {
                    Recordkey = "1",
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                       new ApplValcodesVals("1", "2", "4", "4", "5", "6", "7"),
                       new ApplValcodesVals("1", "2", "2", "4", "5", "6", "7")
                    }
                };

                students = new Students()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    StuAcadPrograms = new List<string>() { "1" }
                };

                institutionsAttendData = new Collection<InstitutionsAttend>
                {
                    new InstitutionsAttend() {Recordkey="1*1", InstaAcadCredentials = new List<string> {"1" } }
                };
                studentPrograms = new StudentPrograms()
                {
                    RecordGuid = guid,
                    Recordkey = "1*1",
                    StprCatalog = "1",
                    StprStatus = new List<string>() { "active" },
                    StprEndDate = new List<DateTime?>() { DateTime.Today },
                    StprStartDate = new List<DateTime?>() { DateTime.Today },
                    StprAntCmplDate = DateTime.Today,
                    StprDept = "1",
                    StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>()
                    {
                        new StudentProgramsStprMajorList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>()
                    {
                        new StudentProgramsStprMinorList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>()
                    {
                        new StudentProgramsStprSpecialties("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    },
                    StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>()
                    {
                        new StudentProgramsStprCcdList("1", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(100), "1")
                    }
                };

                studentAcademicProgram = new StudentAcademicProgram("1", "1", "1", guid, DateTime.Today, "active") { StudentProgramToReplace = "1*MATH.AA" };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ReturnsAsync(studentPrograms);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(academicPrograms);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", It.IsAny<string[]>(), true)).ReturnsAsync(academicCredentials);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true)).ReturnsAsync(applValcodes);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Students>(It.IsAny<string>(), true)).ReturnsAsync(students);
                dataReaderMock.Setup(r => r.ReadRecordColumnsAsync("STUDENTS", It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(new Dictionary<string, string>() { { "STU.ACAD.PROGRAMS", "1" } });
                dataReaderMock.Setup(r => r.ReadRecordColumnsAsync("APPLICANTS", It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(new Dictionary<string, string>() { { "APP.APPLICATIONS", "1" } });
                dataReaderMock.Setup(r => r.BatchReadRecordColumnsAsync("APPLICATIONS", It.IsAny<string[]>(), It.IsAny<string[]>())).ReturnsAsync(new Dictionary<string, Dictionary<string, string>>() { { "1", new Dictionary<string, string>() { { "STU.ACAD.PROGRAMS", "1" } } } });
                dataReaderMock.SetupSequence(r => r.Select(It.IsAny<GuidLookup[]>())).Returns(dicResult);

                dataReaderMock.Setup(r => r.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.PROGRAMS", It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(new string[] { "1*1" });
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>())).ReturnsAsync(new string[] { "1*1" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentPrograms>() { studentPrograms });
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<InstitutionsAttend>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(institutionsAttendData));
                dataReaderMock.Setup(r => r.ReadRecordAsync<InstitutionsAttend>(It.IsAny<string>(), true)).ReturnsAsync(institutionsAttendData[0]);
                transManagerMock.Setup(t => t.ExecuteAsync<UpdateStudentProgramRequest, UpdateStudentProgramResponse>(It.IsAny<UpdateStudentProgramRequest>())).ReturnsAsync(response);
            }

            #endregion

            [TestMethod]
            public async Task StudentAcademicProgramRepositoryTests_CreateStudentAcademicProgram2Async_Success()
            {
                var actual = await studentAcademicProgramRepository.CreateStudentAcademicProgram2Async(studentAcademicProgram, defaultInstitutionId);
                Assert.IsInstanceOfType(actual, typeof(StudentAcademicProgram));
                Assert.AreEqual(studentPrograms.RecordGuid, actual.Guid, "Guid");
                Assert.AreEqual(studentPrograms.Recordkey.Split('*')[1], actual.ProgramCode, "Program");
                Assert.AreEqual(studentPrograms.StprStatus.FirstOrDefault(), actual.Status.ToString(), "Status", true);
                Assert.AreEqual(studentPrograms.StprCatalog, actual.CatalogCode, "Catalog");
                Assert.AreEqual(studentPrograms.StprStartDate.FirstOrDefault().Value, actual.StartDate.Value, "Start Date");
                Assert.AreEqual(studentPrograms.StprEndDate.FirstOrDefault().Value, actual.EndDate.Value, "End Date");
                Assert.AreEqual(studentPrograms.StprAntCmplDate.Value, actual.AnticipatedCompletionDate.Value, "Completion Date");
                Assert.AreEqual(studentPrograms.StprDept, actual.DepartmentCode, "Department");
                Assert.AreEqual(studentPrograms.StprMajorListEntityAssociation.FirstOrDefault().StprAddnlMajorsAssocMember, actual.StudentProgramMajors.FirstOrDefault(), "Majors");
                Assert.AreEqual(studentPrograms.StprMinorListEntityAssociation.FirstOrDefault().StprMinorsAssocMember, actual.StudentProgramMinors.FirstOrDefault(), "Minors");
                Assert.AreEqual(studentPrograms.StprSpecialtiesEntityAssociation.FirstOrDefault().StprSpecializationsAssocMember, actual.StudentProgramSpecializations.FirstOrDefault(), "Specializations");
                Assert.AreEqual(studentPrograms.StprCcdListEntityAssociation.FirstOrDefault().StprCcdsAssocMember, actual.StudentProgramCcds.FirstOrDefault(), "CCDs");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAcademicProgramRepositoryTests_CreateStudentAcademicProgram2Async_ArgumentNullException_When_Guid_Null()
            {
                await studentAcademicProgramRepository.CreateStudentAcademicProgram2Async(null, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_CreateStudentAcademicProgram2Async_RepositoryException()
            {
                response.Error = true;
                response.UpdateStudentProgramError = new List<UpdateStudentProgramError>()
                {
                    new UpdateStudentProgramError() { ErrorCode = "ERROR", ErrorMessage = "MESSAGE" },
                    new UpdateStudentProgramError() { ErrorMessage = "MESSAGE", }
                };
                await studentAcademicProgramRepository.CreateStudentAcademicProgram2Async(studentAcademicProgram, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAcademicProgramRepositoryTests_UpdateStudentAcademicProgram2Async_ArgumentNullException_When_Guid_Null()
            {
                await studentAcademicProgramRepository.UpdateStudentAcademicProgram2Async(null, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentAcademicProgramRepositoryTests_UpdateStudentAcademicProgram2Async_RepositoryException()
            {
                response.Error = true;
                response.UpdateStudentProgramError = new List<UpdateStudentProgramError>()
                {
                    new UpdateStudentProgramError() { ErrorCode = "ERROR", ErrorMessage = "MESSAGE" },
                    new UpdateStudentProgramError() { ErrorMessage = "MESSAGE", }
                };
                await studentAcademicProgramRepository.UpdateStudentAcademicProgram2Async(studentAcademicProgram, defaultInstitutionId);
            }
        }
    }
}