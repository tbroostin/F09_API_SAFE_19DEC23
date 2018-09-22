// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
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

                applValcodes = new ApplValcodes() { Recordkey = "1" };

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
                dataReaderMock.Setup(r => r.SelectAsync("ACAD.PROGRAMS", It.IsAny<string[]>(),It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(ids);
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string>())).ReturnsAsync(new string[] { "1*1" });
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1*1" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentPrograms>() { studentPrograms });
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<InstitutionsAttend>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(institutionsAttendData));
                dataReaderMock.Setup(r => r.ReadRecordAsync<InstitutionsAttend>(It.IsAny<string>(), true)).ReturnsAsync(institutionsAttendData[0]);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentAcademicProgramByGuidAsync_ArgumentNullException_When_Guid_Null()
            {
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(null, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetStudentAcademicProgramByGuidAsync_Invalid_StudentAcademicProgramId()
            {
                dicResult[guid].PrimaryKey = null;
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(guid, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetStudentAcademicProgramByGuidAsync_StudentProgram_NotAvailable_For_Guid()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ReturnsAsync(null);
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(guid, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetStudentAcademicProgramByGuidAsync_Exception()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), true)).ThrowsAsync(new Exception());
                await studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(guid, defaultInstitutionId);
            }

            [TestMethod]
            public async Task GetStudentAcademicProgramByGuidAsync()
            {
                var result = await studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(guid, defaultInstitutionId);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetStudentAcademicProgramsAsync()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";

                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetStudentAcademicPrograms2Async()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2"};
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), ids, ids, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetStudentAcademicPrograms2Async_Null_CredList()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetStudentAcademicPrograms2Async_Null_CatalogAndProgram_NotNull_NoCount()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, "123", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task GetStudentAcademicPrograms2Async_Null_CatalogAndProgram_NoStudent()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), "", "123", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(result.Item2, 2);
            }

            [TestMethod]
            public async Task GetStudentAcademicPrograms2Async_Null_CatalogAndProgram_StuProgsLimitingKeys_Null()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                dataReaderMock.Setup(r => r.SelectAsync("STUDENT.PROGRAMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), "", "123", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task GetStudentAcademicPrograms2Async_Null_Catalog_NoCount()
            {
                academicCredentials.FirstOrDefault().AcadPersonId = "2";
                List<string> ids = new List<string>() { "1", "2" };
                var result = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, "456", DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, "", "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), null, ids, guid, "active");

                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task GetStudentAcademicPrograms2Async_NoStudentDataContract()
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
            public async Task GetStudentAcademicProgramsAsync_RepositoryException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Students>(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetStudentAcademicProgramsAsync_ArgumentException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Students>(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), new List<string> { guid }, new List<string> { guid }, guid, "active");
            }

            [TestMethod]
            public async Task GetStudentAcademicPrograms2Async_RepositoryException()
            {
                //dataReaderMock.Setup(d => d.ReadRecordAsync<Students>("STUDENTS", true)).ThrowsAsync(new RepositoryException());
                List<string> ids = new List<string>() { "1", "2" };
                var results = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, DateTime.Today.AddDays(-20).ToString(), ids, ids, guid, "active");

                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task GetStudentAcademicProgramsAsync_Invalid_Statuses_From_Repository()
            {
                studentPrograms.StprStatus = new List<string>() { };

                //dataReaderMock.Setup(d => d.ReadRecordAsync<Students>("STUDENTS", true)).ThrowsAsync(new RepositoryException());

                var results = await studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, 0, 1, true, guid, DateTime.Today.AddDays(-100).ToString(),
                    DateTime.Today.AddDays(100).ToString(), guid, guid, "active", guid, guid, guid, "", new List<string> { guid }, new List<string> { guid }, guid, "active");

                Assert.IsNotNull(results);
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
            public async Task CreateStudentAcademicProgramAsync_ArgumentNullException_When_Guid_Null()
            {
                await studentAcademicProgramRepository.CreateStudentAcademicProgramAsync(null, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CreateStudentAcademicProgramAsync_RepositoryException()
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
            public async Task CreateStudentAcademicProgramAsync()
            {
                studentPrograms.StprStatus = new List<string>() { };
                studentPrograms.StprEndDate = new List<DateTime?>() { };
                var result = await studentAcademicProgramRepository.CreateStudentAcademicProgramAsync(studentAcademicProgram, defaultInstitutionId);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateStudentAcademicProgramAsync_ArgumentNullException_When_Guid_Null()
            {
                await studentAcademicProgramRepository.UpdateStudentAcademicProgramAsync(null, defaultInstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task UpdateStudentAcademicProgramAsync_RepositoryException()
            {
                response.Error = true;
                response.UpdateStuAcadProgramError = new List<UpdateStuAcadProgramError>()
                {
                    new UpdateStuAcadProgramError() { ErrorCode = "ERROR", ErrorMessage = "MESSAGE" },
                    new UpdateStuAcadProgramError() { ErrorMessage = "MESSAGE", }
                };
                await studentAcademicProgramRepository.UpdateStudentAcademicProgramAsync(studentAcademicProgram, defaultInstitutionId);
            }

            [TestMethod]
            public async Task UpdateStudentAcademicProgramAsync_Create_StudentAcademicProgram()
            {
                studentPrograms.StprStatus = new List<string>() { };
                studentPrograms.StprEndDate = new List<DateTime?>() { null };

                var firstResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "STUDENT.PROGRAMS", PrimaryKey = null } } };
                dataReaderMock.SetupSequence(r => r.Select(It.IsAny<GuidLookup[]>())).Returns(firstResult);
                var result = await studentAcademicProgramRepository.UpdateStudentAcademicProgramAsync(studentAcademicProgram, defaultInstitutionId);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task UpdateStudentAcademicProgramAsync()
            {
                studentPrograms.StprStatus = new List<string>() { };
                studentPrograms.StprEndDate = new List<DateTime?>() { null };

                var firstResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "STUDENT.PROGRAMS", PrimaryKey = null } } };
                dataReaderMock.SetupSequence(r => r.Select(It.IsAny<GuidLookup[]>())).Returns(firstResult);
                var result = await studentAcademicProgramRepository.UpdateStudentAcademicProgramAsync(studentAcademicProgram, defaultInstitutionId);

                Assert.IsNotNull(result);
            }
        }
    }
}