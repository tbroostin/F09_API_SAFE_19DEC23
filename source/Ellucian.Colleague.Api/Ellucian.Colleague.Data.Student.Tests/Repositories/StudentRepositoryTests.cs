// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
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
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentRepositoryTests_GetDataModelStudents : BasePersonSetup
    {
        private Collection<Students> studentData;
        private Collection<StudentPrograms> studentProgramData;
        private Collection<StudentAcadLevels> studentAcadLevelData;

        private List<Ellucian.Colleague.Domain.Student.Entities.Student> _studentEntities;

        private StudentRepository _studentRepository;
        private ApplValcodes _studentProgramStatuses;

        private Dictionary<string, GuidLookupResult> dictResult;

        private string guid = "65eb3f15-139a-4884-9463-281872cbf2d3";
        
        int offset = 0;
        int limit = 200;

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();
            BuildData();
            _studentRepository = BuildStudentsRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
            transFactoryMock = null;
            dataReaderMock = null;
            cacheProviderMock = null;
            localCacheMock = null;
            _studentRepository = null;
        }
        private void BuildData()
        {
            _studentEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Student>()
            {
                new Ellucian.Colleague.Domain.Student.Entities.Student("65eb3f15-139a-4884-9463-281872cbf2d3", "0000123", new List<string>() {"BA.ENGL"}, null, "*", false, null)
                {

                }
            };

            _studentProgramStatuses = new ApplValcodes()
            {
                Recordkey = "STUDENT.PROGRAM.STATUSES",
                ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "P",
                            ValExternalRepresentationAssocMember = "P",
                            ValActionCode1AssocMember = "1"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "A",
                            ValExternalRepresentationAssocMember = "A",
                            ValActionCode1AssocMember = "2"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "G",
                            ValExternalRepresentationAssocMember = "G",
                            ValActionCode1AssocMember = "3"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "C",
                            ValExternalRepresentationAssocMember = "C",
                            ValActionCode1AssocMember = "4"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "W",
                            ValExternalRepresentationAssocMember = "W",
                            ValActionCode1AssocMember = "5"
                        }
                    },
            };
        }

        private StudentRepository BuildStudentsRepository()
        {
            apiSettings = new ApiSettings("TEST");
            //var x = new StudentTypeInfo("ALUM", new DateTime(2018, 10, 31));
            //var x2 = new List<StudentTypeInfo>() { x };
            //var x3 = new List<StudentTypeInfo>() { new StudentTypeInfo("ALUM", new DateTime(2018, 10, 31)) };
        
            var studentRecord1 = new Students()
            {
                RecordGuid = "65eb3f15-139a-4884-9463-281872cbf2d3",
                Recordkey = "0000123",
                StuAcadPrograms = new List<string>() { "BA.ENGL"},
                StuTypes = new List<string>() { "ALUM"},
                //StuTypeInfo = new List<StudentTypeInfo>() { new StudentTypeInfo("ALUM", new DateTime(2018, 10, 31)) }
                StuAcadLevels = new List<string>() { "UG"}
            };
            var studentRecord2 = new Students()
            {
                RecordGuid = "a0861d51-8939-41f4-a9d1-c9244e3bd6b0",
                Recordkey = "0000789"
            };
            studentData = new Collection<Students>();
            studentData.Add(studentRecord1);
            studentData.Add(studentRecord2);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Students>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentData);

            var studentProgram1 = new StudentPrograms()
            {
                RecordGuid = "46fdbd32-31fc-45a2-989d-4a442d2dcc15",
                Recordkey = "0000123*BA.ENGL",
                StprStartDate = new List<DateTime?> { new DateTime(2010, 10, 31) },
                StprStatus = new List<string> { "A"}
            };
            studentProgramData = new Collection<StudentPrograms>();
            studentProgramData.Add(studentProgram1);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentProgramData);

            var studentAcadLevel1 = new StudentAcadLevels()
            {                
                Recordkey = "0000123*UG",
                StaClass = "FR",
                StaStartTerm = "2010/FA"
            };
            studentAcadLevelData = new Collection<StudentAcadLevels>();
            studentAcadLevelData.Add(studentAcadLevel1);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<StudentAcadLevels>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentAcadLevelData);
            
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", It.IsAny<bool>())).ReturnsAsync (_studentProgramStatuses);

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "0000123", "0000789" });

            dictResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "STUDENTS", PrimaryKey = "0000123" } }
                };

            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dictResult);

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var student = studentData.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, student == null ? null : new GuidLookupResult() { Entity = "STUDENTS", PrimaryKey = student.Recordkey });
                }
                return Task.FromResult(result);
            });

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            var mockManager = new Mock<IColleagueTransactionInvoker>();

            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

            var getPersonFilterResponse = new GetPersonFilterResponse()
            {
                PersonIds = new List<string>() { "0003397", "0000010" },
                TotalRecords = 2,
                ErrorMessages = null
            };
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetPersonFilterRequest, GetPersonFilterResponse>(It.IsAny<GetPersonFilterRequest>())).ReturnsAsync(getPersonFilterResponse);

            // Construct repository
            _studentRepository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            return _studentRepository;
        }

        #region V7

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentRepository_GetDataModelStudentGuid_ArgumentNullException()
        {
            await _studentRepository.GetDataModelStudentFromGuidAsync(null);
        }

        [TestMethod]
        public async Task StudentRepository_GetDataModelStudentGuid_EmptySublist()
        {
            var result = await _studentRepository.GetDataModelStudentsAsync(0, 0, false, null, null, null, null);
            Assert.AreEqual(result.Item1.Count(), 0, "Record count");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentRepository_GetDataModelStudentFromGuid_ArgumentNullException()
        {
            var result = await _studentRepository.GetDataModelStudentFromGuidAsync(null);
        }

        [TestMethod]
        public async Task StudentRepository_GetDataModelStudentFromGuid()
        {
            LdmGuid ldmGuid = new LdmGuid()
            {
                LdmGuidEntity = "STUDENTS",
                LdmGuidLdmName = "students",
                LdmGuidPrimaryKey = "0000123",
                Recordkey = guid
            };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            Collection<PersonSt> personSts = new Collection<PersonSt>()
            {
                new PersonSt()
                {
                    Recordkey = "0000123",
                    PstStudentAcadCred = new List<string>(){ "1" }
                }
            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<PersonSt>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(personSts);
            Collection<StudentAcadCred> stAcadCreds = new Collection<StudentAcadCred>()
            {

            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<StudentAcadCred>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(stAcadCreds);

            var result = await _studentRepository.GetDataModelStudentFromGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentRepository_GetDataModelStudentFromGuid_KeyNotFoundException()
        {
            LdmGuid ldmGuid = null;
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            var result = await _studentRepository.GetDataModelStudentFromGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentRepository_GetDataModelStudentFromGuid_RepositoryException()
        {
            LdmGuid ldmGuid = new LdmGuid()
            {
                LdmGuidEntity = "STUDENTS",
                LdmGuidLdmName = "student-academic-periods",
                LdmGuidPrimaryKey = "0000123",
                Recordkey = guid
            };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            var result = await _studentRepository.GetDataModelStudentFromGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentRepository_GetDataModelStudentFromGuid_RepositoryException_PrimaryKey_Null()
        {
            LdmGuid ldmGuid = new LdmGuid()
            {
                LdmGuidEntity = "STUDENTS",
                LdmGuidLdmName = "students",
                LdmGuidPrimaryKey = "",
                Recordkey = guid
            };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            var result = await _studentRepository.GetDataModelStudentFromGuidAsync(guid);
        }

        #endregion V7


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentRepository_GetDataModelStudentGuid2_ArgumentNullException()
        {
            await _studentRepository.GetDataModelStudentFromGuid2Async(null);
        }

        [TestMethod]
        public async Task StudentRepository_GetDataModelStudentGuid2_EmptySublist()
        {
            var result = await _studentRepository.GetDataModelStudents2Async(0, 0, false, null, null, null, null);            
            Assert.AreEqual(result.Item1.Count(), 0, "Record count");
        }

        [TestMethod]
        public async Task StudentRepository_GetDataModelStudents2()
        {
            var result = await _studentRepository.GetDataModelStudents2Async(offset, limit, false, null, null, null, null);
            Assert.AreEqual(result.Item1.Count(), 2, "Record count");
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentRepository_GetDataModelStudentFromGuid2_ArgumentNullException()
        {
            var result = await _studentRepository.GetDataModelStudentFromGuid2Async(null);
        }

        [TestMethod]
        public async Task StudentRepository_GetDataModelStudentFromGuid2()
        {
            LdmGuid ldmGuid = new LdmGuid()
            {
                LdmGuidEntity = "STUDENTS",
                LdmGuidLdmName  = "students",
                LdmGuidPrimaryKey = "0000123",
                Recordkey = guid
            };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            var result = await _studentRepository.GetDataModelStudentFromGuid2Async(guid);
            Assert.AreEqual(result.StudentGuid, guid, "Guid");
            Assert.AreEqual(result.Id, "0000123", "StudentId");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentRepository_GetDataModelStudentFromGuid2_KeyNotFoundException()
        {
            LdmGuid ldmGuid = null;
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            var result = await _studentRepository.GetDataModelStudentFromGuid2Async(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentRepository_GetDataModelStudentFromGuid2_RepositoryException()
        {
            LdmGuid ldmGuid = new LdmGuid()
            {
                LdmGuidEntity = "STUDENTS",
                LdmGuidLdmName = "student-academic-periods",
                LdmGuidPrimaryKey = "0000123",
                Recordkey = guid
            };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            var result = await _studentRepository.GetDataModelStudentFromGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentRepository_GetDataModelStudentFromGuid2_RepositoryException_PrimaryKey_Null()
        {
            LdmGuid ldmGuid = new LdmGuid()
            {
                LdmGuidEntity = "STUDENTS",
                LdmGuidLdmName = "students",
                LdmGuidPrimaryKey = "",
                Recordkey = guid
            };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            var result = await _studentRepository.GetDataModelStudentFromGuid2Async(guid);
        }
    }
        
    [TestClass]
    public class StudentRepositoryTests : BasePersonSetup
    {
        private TestStudentRepository testStudentRepository;
        private StudentRepository studentRepository;
        const string CacheName = "Ellucian.Web.Student.Data.Colleague.Repository.StudentRepository";
        private string knownStudentId1;
        private string knownStudentId2;
        private string knownStudentId3;
        private string unknownStudentId;
        private string[] acadPrograms;
        private string quote;
        private string educationalGoalDescription;

        private Collection<DataContracts.Students> students;
        private Collection<DataContracts.Applicants> applicants;
        private Collection<Base.DataContracts.PersonSt> personStRecords;
        private Collection<DataContracts.StudentAdvisement> studentAdvisements;
        private Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;
        private Collection<FinAid> finAidRecords;
        private Collection<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson> foreignPeople;

        [TestInitialize]
        public async void Initialize()
        {
            testStudentRepository = new TestStudentRepository();
            students = new Collection<DataContracts.Students>();
            applicants = new Collection<DataContracts.Applicants>();
            personStRecords = new Collection<Base.DataContracts.PersonSt>();
            studentAdvisements = new Collection<DataContracts.StudentAdvisement>();
            people = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>();
            finAidRecords = BuildFinAidRecords(testStudentRepository.faStudentData);
            foreignPeople = new Collection<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson>();
            quote = '"'.ToString();
            educationalGoalDescription = "Masters degree";
            // Initialize person setup and Mock framework
            PersonSetupInitialize();

            await SetupData();

            studentRepository = BuildMockStudentRepository();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            studentRepository = null;
        }

        [TestMethod]
        // generic test for a known ID, verify that the same ID is passed out.
        public async Task Get_StudentByID()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId1);
            Assert.AreEqual(knownStudentId1, student.Id);
        }

        [TestMethod]
        // generic test for a known ID, verify that the same ID is passed out.
        public async Task Get_StudentsByIDs()
        {
            List<string> studentIds = new List<string>();
            studentIds.Add(knownStudentId1);
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData);
            var student = students.ElementAt(0);
            Assert.AreEqual(knownStudentId1, student.Id);
        }

        [TestMethod]
        // generic test for a known ID, verify that the same ID is passed out.
        public async Task Get_StudentsByIDs_called_with_duplicateIds()
        {
            List<string> studentIds = new List<string>();
            studentIds.Add(knownStudentId1);
            studentIds.Add(knownStudentId1);
            studentIds.Add(knownStudentId2);
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData);
            dataReaderMock.Verify(d => d.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Students>(new string[] { knownStudentId1 , knownStudentId1, knownStudentId2 }, It.IsAny<Boolean>()));
        }

        [TestMethod]
        public async Task Get_StudentsByIDs_called_with_emptyIds()
        {
            List<string> studentIds = new List<string>();
            studentIds.Add(knownStudentId1);
            studentIds.Add(string.Empty);
            studentIds.Add(knownStudentId2);
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData);
            dataReaderMock.Verify(d => d.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Students>(new string[] { knownStudentId1,  knownStudentId2 }, It.IsAny<Boolean>()));
        }

        [TestMethod]
        public async Task Get_StudentsByIDs_called_with_nullIds()
        {
            List<string> studentIds = new List<string>();
            studentIds.Add(knownStudentId1);
            studentIds.Add(null);
            studentIds.Add(knownStudentId2);
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData);
            dataReaderMock.Verify(d => d.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Students>(new string[] { knownStudentId1, knownStudentId2 }, It.IsAny<Boolean>()));
        }

        [TestMethod]
        public async Task Get_StudentsByIDs_called_with_duplicates_null_empty_Ids()
        {
            List<string> studentIds = new List<string>();
            studentIds.Add(knownStudentId1);
            studentIds.Add(null);
            studentIds.Add(null);
            studentIds.Add(knownStudentId2);
            studentIds.Add(string.Empty);
            studentIds.Add(knownStudentId1);
            studentIds.Add(string.Empty);
            studentIds.Add(knownStudentId2);
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData);
            dataReaderMock.Verify(d => d.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Students>(new string[] { knownStudentId1, knownStudentId2, knownStudentId1, knownStudentId2 }, It.IsAny<Boolean>()));
        }

        [TestMethod]
        public async Task GetStudentsByIdAsync_NoEmailsMarkedPreferred()
        {
            // Verify returned emails and preferredEmail
            var studentIdList = new List<string>() { knownStudentId1 };
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> studentResults = await studentRepository.GetStudentsByIdAsync(studentIdList, termData, citizenshipStatusData, false);
            var student1 = studentResults.Where(s => s.Id == knownStudentId1).FirstOrDefault();
            Assert.AreEqual("dsmith@yahoo.com", student1.PreferredEmailAddress.Value);

        }

        [TestMethod]
        public async Task GetStudentsByIdAsync_GetEmailMarkedPreferred()
        {
            // Verify returned emails and preferredEmail
            var studentIdList = new List<string>() { knownStudentId2 };
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> studentResults = await studentRepository.GetStudentsByIdAsync(studentIdList, termData, citizenshipStatusData, false);
            var student2 = studentResults.Where(s => s.Id == knownStudentId2).FirstOrDefault();
            Assert.AreEqual("djones@yahoo.com", student2.PreferredEmailAddress.Value);

        }

        [TestMethod]
        // test for returning student advisements
        public async Task Get_StudentsByIDs_Advisements()
        {
            List<string> studentIds = new List<string>();
            studentIds.Add(knownStudentId2);
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            studentRepository = BuildMockStudentRepository();
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData);
            var student = students.ElementAt(0);
            Assert.AreEqual(knownStudentId2, student.Id);
            Assert.AreEqual(1, student.Advisements.Count());
            Assert.AreEqual(1, student.AdvisorIds.Count());

        }

        [TestMethod]
        // generic test for an unknown ID, verify not found
        public async Task Get_StudentByID_UnknownID()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(unknownStudentId);
            Assert.IsNull(student);
        }

        [TestMethod]
        // test a student with no acad programs
        public async Task Get_StudentByID_NoPrograms()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId1);
            Assert.AreEqual(0, student.ProgramIds.Count);
        }

        [TestMethod]
        // test a student with one acad program
        public async Task Get_StudentByID_OneProgram()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId2);
            Assert.AreEqual(1, student.ProgramIds.Count);
            Assert.AreEqual(acadPrograms[0], student.ProgramIds[0]);
        }

        [TestMethod]
        // test a student with two acad programs - also enforces same order from db
        // This student has 3 programs in setup, but third has previous end date.
        public async Task Get_StudentByID_TwoPrograms()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId3);
            Assert.AreEqual(2, student.ProgramIds.Count);
            Assert.AreEqual(acadPrograms[0], student.ProgramIds[0]);
            Assert.AreEqual(acadPrograms[1], student.ProgramIds[1]);
        }

        [TestMethod]
        // test a student with no degree plans
        public async Task Get_StudentByID_NoDegPlan()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId3);
            Assert.AreEqual(false, student.DegreePlanId.HasValue);
        }

        [TestMethod]
        // test a student with one degree plans
        public async Task Get_StudentByID_OneDegPlan()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId1);
            Assert.AreEqual(true, student.DegreePlanId.HasValue);
        }

        [TestMethod]
        // test a student with two degree plans
        public async Task Get_StudentByID_TwoDegPlan()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId2);
            Assert.AreEqual(true, student.DegreePlanId.HasValue);
        }

        [TestMethod]
        // test a student with one academic credit
        public async Task Get_StudentByID_OneAcadCred()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId1);
            Assert.IsNotNull(student.AcademicCreditIds);
            Assert.AreEqual("19000", student.AcademicCreditIds[0]);
        }

        [TestMethod]
        // test a student with no academic credit, but an empty string comes back from the data accessor
        public async Task Get_StudentByID_NoAcadCred_BadAccessor()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId2);
            Assert.IsNotNull(student.AcademicCreditIds);
            Assert.AreEqual(0, student.AcademicCreditIds.Count);
        }

        [TestMethod]
        public async Task Get_StudentById_NoAdvisors()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId1);
            Assert.AreEqual(0, student.AdvisorIds.Count());
        }

        [TestMethod]
        public async Task Get_StudentById_ReturnAdvisors()
        {
            // Excludes advisor with an end date.
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId2);
            Assert.AreEqual(1, student.AdvisorIds.Count());
        }

        [TestMethod]
        public async Task Get_StudentById_ReturnsFirstEmail()
        {
            // Get first email if none are flagged as preferred
            Domain.Student.Entities.Student student1 = await studentRepository.GetAsync(knownStudentId1);
            Assert.AreEqual("dsmith@yahoo.com", student1.PreferredEmailAddress.Value);
        }

        [TestMethod]
        public async Task Get_StudentById_ReturnsPreferredEmail()
        {
            // Get email that is flagged as preferred
            Domain.Student.Entities.Student student2 = await studentRepository.GetAsync(knownStudentId2);
            Assert.AreEqual("djones@yahoo.com", student2.PreferredEmailAddress.Value);
        }

        [TestMethod]
        public async Task Get_StudentById_ReturnsNoEmail()
        {
            Domain.Student.Entities.Student student3 = await studentRepository.GetAsync(knownStudentId3);
            Assert.IsNull(student3.PreferredEmailAddress);
        }

        [TestMethod]
        public async Task Get_StudentById_TwoRestrictions()
        {
            Domain.Student.Entities.Student student1 = await studentRepository.GetAsync(knownStudentId1);
            Assert.AreEqual(2, student1.StudentRestrictionIds.Count());
        }

        [TestMethod]
        public async Task Get_StudentById_NoRestrictions()
        {
            Domain.Student.Entities.Student student2 = await studentRepository.GetAsync(knownStudentId2);
            Assert.AreEqual(0, student2.StudentRestrictionIds.Count());
        }

        [TestMethod]
        public async Task Get_StudentById_EducationalGoals()
        {
            Domain.Student.Entities.Student student2 = await studentRepository.GetAsync(knownStudentId2);
            Assert.AreEqual(educationalGoalDescription, student2.EducationalGoal);
        }

        [TestMethod]
        // test a student with a personal pronoun
        public async Task Get_StudentByID_PersonalPronounCode()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId1);
            Assert.IsNotNull(student.PersonalPronounCode);
            Assert.AreEqual("XHE", student.PersonalPronounCode);
        }

        //[TestMethod]
        //public async Task Get_StudentById_GetFirstCounselorIdInListTest()
        //{
        //    var expectedCounselorId = testStudentRepository.faStudentData.First(f => f.studentId == knownStudentId1).faCounselors.First().counselorId;
        //    Domain.Student.Entities.Student student1 = await studentRepository.GetAsync(knownStudentId1);
        //    Assert.AreEqual(expectedCounselorId, student1.FinancialAidCounselorId);
        //}

        [TestMethod]
        public async Task CounselorId_FirstInListAssignedWithNullStartEndDates()
        {
            var expectedCounselorId = "1234567";
            finAidRecords.First(f => f.Recordkey == knownStudentId1).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = null,
                        FaCounselorEndDateAssocMember = null
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-1),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(1)
                    }
                };

            studentRepository = BuildMockStudentRepository();
            var actualStudent = await studentRepository.GetAsync(knownStudentId1);

            Assert.AreEqual(expectedCounselorId, actualStudent.FinancialAidCounselorId);
        }

        [TestMethod]
        public async Task CounselorId_FirstInListAssignedWithTodayInBetweenStartEndDates()
        {
            var expectedCounselorId = "1234567";
            finAidRecords.First(f => f.Recordkey == knownStudentId1).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-1),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(1)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }
                };

            studentRepository = BuildMockStudentRepository();
            var actualStudent = await studentRepository.GetAsync(knownStudentId1);

            Assert.AreEqual(expectedCounselorId, actualStudent.FinancialAidCounselorId);
        }

        [TestMethod]
        public async Task CounselorId_FirstInListAssignedWithTodayEqualToStartEndDates()
        {
            var expectedCounselorId = "1234567";
            finAidRecords.First(f => f.Recordkey == knownStudentId1).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today,
                        FaCounselorEndDateAssocMember = DateTime.Today
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }
                };

            studentRepository = BuildMockStudentRepository();
            var actualStudent = await studentRepository.GetAsync(knownStudentId1);

            Assert.AreEqual(expectedCounselorId, actualStudent.FinancialAidCounselorId);
        }

        [TestMethod]
        public async Task CounselorId_FirstInListAssignedWithTodayBeforeEndDate_NoStartDate()
        {
            var expectedCounselorId = "1234567";
            finAidRecords.First(f => f.Recordkey == knownStudentId1).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = null,
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(1)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }
                };

            studentRepository = BuildMockStudentRepository();
            var actualStudent = await studentRepository.GetAsync(knownStudentId1);

            Assert.AreEqual(expectedCounselorId, actualStudent.FinancialAidCounselorId);
        }

        [TestMethod]
        public async Task CounselorId_FirstInListAssignedWithTodayAfterStartDate_NoEndDate()
        {
            var expectedCounselorId = "1234567";
            finAidRecords.First(f => f.Recordkey == knownStudentId1).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-1),
                        FaCounselorEndDateAssocMember = null
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }
                };

            studentRepository = BuildMockStudentRepository();
            var actualStudent = await studentRepository.GetAsync(knownStudentId1);

            Assert.AreEqual(expectedCounselorId, actualStudent.FinancialAidCounselorId);
        }

        [TestMethod]
        public async Task CounselorId_SkipCounselorIfStartDateAfterToday()
        {
            var expectedCounselorId = "1234567";
            finAidRecords.First(f => f.Recordkey == knownStudentId1).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(1),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }
                };

            studentRepository = BuildMockStudentRepository();
            var actualStudent = await studentRepository.GetAsync(knownStudentId1);

            Assert.AreEqual(expectedCounselorId, actualStudent.FinancialAidCounselorId);
        }

        [TestMethod]
        public async Task CounselorId_SkipCounselorIfEndDateBeforeToday()
        {
            var expectedCounselorId = "1234567";
            finAidRecords.First(f => f.Recordkey == knownStudentId1).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(-1)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }
                };

            studentRepository = BuildMockStudentRepository();
            var actualStudent = await studentRepository.GetAsync(knownStudentId1);

            Assert.AreEqual(expectedCounselorId, actualStudent.FinancialAidCounselorId);
        }



        [TestMethod]
        public async Task Get_StudentById_NullFinAidRecord_NoCounselorIdTest()
        {
            var finAid = finAidRecords.First(f => f.Recordkey == knownStudentId2);
            finAidRecords.Remove(finAid);

            studentRepository = BuildMockStudentRepository();
            Domain.Student.Entities.Student student2 = await studentRepository.GetAsync(knownStudentId2);
            Assert.IsTrue(string.IsNullOrEmpty(student2.FinancialAidCounselorId));
        }

        [TestMethod]
        public async Task Get_StudentById_NullCounselorRecords_NoCounselorIdTest()
        {
            var finAid = finAidRecords.First(f => f.Recordkey == knownStudentId2);
            finAid.FaCounselorsEntityAssociation = null;

            studentRepository = BuildMockStudentRepository();
            Domain.Student.Entities.Student student2 = await studentRepository.GetAsync(knownStudentId2);
            Assert.IsTrue(string.IsNullOrEmpty(student2.FinancialAidCounselorId));
        }

        [TestMethod]
        public async Task Get_StudentById_EmptyCounselorList_NoCounselorIdTest()
        {
            var finAid = finAidRecords.First(f => f.Recordkey == knownStudentId2);
            finAid.FaCounselorsEntityAssociation = new List<FinAidFaCounselors>();

            studentRepository = BuildMockStudentRepository();
            Domain.Student.Entities.Student student2 = await studentRepository.GetAsync(knownStudentId2);
            Assert.IsTrue(string.IsNullOrEmpty(student2.FinancialAidCounselorId));
        }

        // Test PersonDisplayName property

        [TestMethod]
        public async Task Student_Get_PersonDisplayName_Chosen()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId1);
            Assert.IsNotNull(student.PersonDisplayName);
            Assert.AreEqual("ChosenLast, ChosenFirst", student.PersonDisplayName.FullName);
        }

        [TestMethod]
        public async Task GetStudentsByID_InheritFalse_PersonDisplayName_Chosen()
        {
            List<string> studentIds = new List<string>();
            studentIds.Add(knownStudentId1);
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData, false);
            var student = students.ElementAt(0);
            Assert.IsNotNull(student.PersonDisplayName);
            Assert.AreEqual("ChosenLast, ChosenFirst", student.PersonDisplayName.FullName);
        }

        [TestMethod]
        public async Task Student_Get_PersonDisplayName_Formatted()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId2);
            Assert.IsNotNull(student.PersonDisplayName);
            Assert.AreEqual("YYY Formatted Name", student.PersonDisplayName.FullName);
        }

        [TestMethod]
        public async Task GetStudentsByID_InheritFalse_PersonDisplayName_Formatted()
        {
            List<string> studentIds = new List<string>();
            studentIds.Add(knownStudentId2);
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData, false);
            var student = students.ElementAt(0);
            Assert.IsNotNull(student.PersonDisplayName);
            Assert.AreEqual("YYY Formatted Name", student.PersonDisplayName.FullName);
        }

        [TestMethod]
        public async Task Student_Get_PersonDisplayName_PreferredOverride()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId3);
            Assert.IsNotNull(student.PersonDisplayName);
            Assert.AreEqual("Mr. L. Legal Middle Legal Last, Esq.", student.PersonDisplayName.FullName);
        }

        [TestMethod]
        public async Task GetStudentsByID_InheritFalse_PersonDisplayName_PreferredStandard()
        {
            List<string> studentIds = new List<string>();
            studentIds.Add(knownStudentId2);
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData, false);
            var student = students.ElementAt(0);
            Assert.IsNotNull(student.PersonDisplayName);
            Assert.AreEqual("YYY Formatted Name", student.PersonDisplayName.FullName);
        }

        [TestMethod]
        public async Task Student_Get_PersonDisplayName_PreferredStandard()
        {
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId2);
            Assert.IsNotNull(student.PersonDisplayName);
            Assert.AreEqual("YYY Formatted Name", student.PersonDisplayName.FullName);
        }

        [TestMethod]
        public async Task Student_Get_PersonDisplayName_NullWhenNoParam()
        {
            var emptyStwebDefault = new StwebDefaults();
            dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).ReturnsAsync(emptyStwebDefault);
            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId2);
            Assert.IsNull(student.PersonDisplayName);
        }

        [TestMethod]
        public async Task GetStudentsByID_InheritFalse_PersonDisplayName_NullWhenNoParam()
        {
            var emptyStwebDefault = new StwebDefaults();
            dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).ReturnsAsync(emptyStwebDefault);
            List<string> studentIds = new List<string>();
            studentIds.Add(knownStudentId2);
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData, false);
            var student = students.ElementAt(0);
            Assert.IsNull(student.PersonDisplayName);
        }

        [TestMethod]
        public async Task Student_Get_TestNameElements_OverrideCodeIM()
        {
            // mock data reader for getting the STUDENT Name Addr Hierarchy
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "STUDENT", true))
                .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                {
                    Recordkey = "STUDENT",
                    NahNameHierarchy = new List<string>() { "PF" }
                });
            //// mock data accessor PERSON response - Using Person Preferred Override of IM

            Ellucian.Colleague.Data.Base.DataContracts.Person newPerson = new Ellucian.Colleague.Data.Base.DataContracts.Person()
            {
                Recordkey = "0000003",
                LastName = "LastName",
                FirstName = "FirstName",
                MiddleName = "MiddleName",
                PersonChosenLastName = "ChosenLast",
                PersonChosenFirstName = "ChosenFirst",
                PersonChosenMiddleName = "ChosenMiddle",
                Prefix = "Mr.",
                Suffix = "Esq.",
                PreferredName = "IM",
                PFormatEntityAssociation = new List<PersonPFormat>() { new PersonPFormat() { PersonFormattedNameTypesAssocMember = "XXX", PersonFormattedNamesAssocMember = "XXX Formatted Name" }, new PersonPFormat() { PersonFormattedNameTypesAssocMember = "YYY", PersonFormattedNamesAssocMember = "YYY Formatted Name" } }
            };
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.Person>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", knownStudentId3, true)).ReturnsAsync(newPerson);


            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId3);
            Assert.IsNotNull(student.PersonDisplayName);
            Assert.AreEqual("LastName", student.LastName);
            Assert.AreEqual("FirstName", student.FirstName);
            Assert.AreEqual("MiddleName", student.MiddleName);
            Assert.AreEqual("Mr.", student.Prefix);
            Assert.AreEqual("Esq.", student.Suffix);
            Assert.AreEqual("Mr. F. MiddleName LastName, Esq.", student.PreferredNameOverride);
            Assert.AreEqual("Mr. F. MiddleName LastName, Esq.", student.MailLabelNameOverride);
            Assert.AreEqual(2, student.FormattedNames.Count());
            Assert.AreEqual("ChosenFirst", student.ChosenFirstName);
            Assert.AreEqual("ChosenMiddle", student.ChosenMiddleName);
            Assert.AreEqual("ChosenLast", student.ChosenLastName);
            Assert.AreEqual("Mr. F. MiddleName LastName, Esq.", student.PreferredName);
            Assert.AreEqual("Mr. F. MiddleName LastName, Esq.", student.PersonDisplayName.FullName);

        }

        [TestMethod]
        public async Task Student_Get_Name_OverrideCodeII()
        {
            // mock data reader for getting the STUDENT Name Addr Hierarchy
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "STUDENT", true))
                .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                {
                    Recordkey = "STUDENT",
                    NahNameHierarchy = new List<string>() { "PF" }
                });
            //// mock data accessor PERSON response - Using Person Preferred Override of IM

            Ellucian.Colleague.Data.Base.DataContracts.Person newPerson = new Ellucian.Colleague.Data.Base.DataContracts.Person()
            {
                Recordkey = "0000003",
                LastName = "LastName",
                FirstName = "FirstName",
                MiddleName = "MiddleName",
                Prefix = "Mr.",
                Suffix = "Esq.",
                PreferredName = "II",
            };
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.Person>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", knownStudentId3, true)).ReturnsAsync(newPerson);


            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId3);
            Assert.AreEqual("Mr. F. M. LastName, Esq.", student.PreferredNameOverride);
            Assert.AreEqual("Mr. F. M. LastName, Esq.", student.MailLabelNameOverride);
            Assert.AreEqual("Mr. F. M. LastName, Esq.", student.PreferredName);
            Assert.AreEqual("Mr. F. M. LastName, Esq.", student.PersonDisplayName.FullName);

        }

        [TestMethod]
        public async Task Student_Get_Name_OverrideCodeFM()
        {
            // mock data reader for getting the STUDENT Name Addr Hierarchy
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "STUDENT", true))
                .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                {
                    Recordkey = "STUDENT",
                    NahNameHierarchy = new List<string>() { "PF" }
                });
            //// mock data accessor PERSON response - Using Person Preferred Override of IM

            Ellucian.Colleague.Data.Base.DataContracts.Person newPerson = new Ellucian.Colleague.Data.Base.DataContracts.Person()
            {
                Recordkey = "0000003",
                LastName = "LastName",
                FirstName = "FirstName",
                MiddleName = "MiddleName",
                Prefix = "Mr.",
                Suffix = "Esq.",
                PreferredName = "FM",
            };
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.Person>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", knownStudentId3, true)).ReturnsAsync(newPerson);


            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId3);
            Assert.AreEqual("Mr. FirstName MiddleName LastName, Esq.", student.PreferredNameOverride);
            Assert.AreEqual("Mr. FirstName MiddleName LastName, Esq.", student.MailLabelNameOverride);
            Assert.AreEqual("Mr. FirstName MiddleName LastName, Esq.", student.PreferredName);
            Assert.AreEqual("Mr. FirstName MiddleName LastName, Esq.", student.PersonDisplayName.FullName);

        }

        [TestMethod]
        public async Task Student_Get_Name_OverrideCodeFI()
        {
            // mock data reader for getting the STUDENT Name Addr Hierarchy
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "STUDENT", true))
                .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                {
                    Recordkey = "STUDENT",
                    NahNameHierarchy = new List<string>() { "PF" }
                });
            //// mock data accessor PERSON response - Using Person Preferred Override of IM

            Ellucian.Colleague.Data.Base.DataContracts.Person newPerson = new Ellucian.Colleague.Data.Base.DataContracts.Person()
            {
                Recordkey = "0000003",
                LastName = "LastName",
                FirstName = "FirstName",
                MiddleName = "MiddleName",
                Prefix = "Mr.",
                Suffix = "Esq.",
                PreferredName = "FI",
            };
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.Person>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", knownStudentId3, true)).ReturnsAsync(newPerson);


            Domain.Student.Entities.Student student = await studentRepository.GetAsync(knownStudentId3);
            Assert.AreEqual("Mr. FirstName M. LastName, Esq.", student.PreferredNameOverride);
            Assert.AreEqual("Mr. FirstName M. LastName, Esq.", student.MailLabelNameOverride);
            Assert.AreEqual("Mr. FirstName M. LastName, Esq.", student.PreferredName);
            Assert.AreEqual("Mr. FirstName M. LastName, Esq.", student.PersonDisplayName.FullName);

        }

        /* Note: The test setup is broken up between SetupData(), which assigns the class vars,
         * and BuildMockStudentRepository(), which creates a mocked student repo.
         */
        private async Task SetupData()
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
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId1, StuAcadPrograms = null });
            applicants.Add(new DataContracts.Applicants() { Recordkey = knownStudentId1 });
            personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000001", PstStudentAcadCred = new List<string>() { "19000" }, PstAdvisement = new List<string>(), PstRestrictions = new List<string>() { "R001", "R002" } });
            Ellucian.Colleague.Data.Base.DataContracts.Person person1 = new Ellucian.Colleague.Data.Base.DataContracts.Person()
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
            Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson ForeignPerson1 = new Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson() { Recordkey = "0000001" };
            person1.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
            PersonPeopleEmail ppe1 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "dsmith@yahoo.com", PersonEmailTypesAssocMember = "HOME", PersonPreferredEmailAssocMember = "" };
            person1.PeopleEmailEntityAssociation.Add(ppe1);
            PersonPeopleEmail ppe2 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "junk@yahoo.com", PersonEmailTypesAssocMember = "BUS", PersonPreferredEmailAssocMember = "" };
            person1.PeopleEmailEntityAssociation.Add(ppe2);
            person1.MaritalStatus = "M";
            person1.PerEthnics = new List<string> { "W", "H" };
            person1.PerRaces = new List<string> { "CA", "PI" };
            students[0].StuTypeInfoEntityAssociation = new List<StudentsStuTypeInfo>();
            students[0].StuTypeInfoEntityAssociation.Add(new StudentsStuTypeInfo() { StuTypesAssocMember = "FF", StuTypeDatesAssocMember = DateTime.Today });
            people.Add(person1);
            foreignPeople.Add(ForeignPerson1);
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
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId2, StuAcadPrograms = new List<string> { acadPrograms[0] } });
            applicants.Add(new DataContracts.Applicants() { Recordkey = knownStudentId2 });
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "21", StadStudent = "0000002", StadFaculty = "0000036", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = null });
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "22", StadStudent = "0000002", StadFaculty = "0000045", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = new DateTime(2012, 8, 1) });
            Ellucian.Colleague.Data.Base.DataContracts.Person person2 = new Ellucian.Colleague.Data.Base.DataContracts.Person()
            {
                Recordkey = "0000002",
                LastName = "Jones",
                FirstName = "Legal First",
                PFormatEntityAssociation = new List<PersonPFormat>() { new PersonPFormat() { PersonFormattedNameTypesAssocMember = "XXX", PersonFormattedNamesAssocMember = "XXX Formatted Name" }, new PersonPFormat() { PersonFormattedNameTypesAssocMember = "YYY", PersonFormattedNamesAssocMember = "YYY Formatted Name" } }
            };
            Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson ForeignPerson2 = new Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson() { Recordkey = "0000002" };
            person2.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
            PersonPeopleEmail ppe3 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "junk@yahoo.com", PersonEmailTypesAssocMember = "HOME", PersonPreferredEmailAssocMember = "" };
            person2.PeopleEmailEntityAssociation.Add(ppe3);
            PersonPeopleEmail ppe4 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "djones@yahoo.com", PersonEmailTypesAssocMember = "BUS", PersonPreferredEmailAssocMember = "Y" };
            person2.PeopleEmailEntityAssociation.Add(ppe4);
            people.Add(person2);
            foreignPeople.Add(ForeignPerson2);
            /* 
             * student 0000003
             * > two programs
             * > no degree plan
             * > three acad creds
             * > no email addresses on file
             */
            knownStudentId3 = "0000003";
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId3, StuAcadPrograms = new List<string> { acadPrograms[0], acadPrograms[1] } });
            applicants.Add(new DataContracts.Applicants() { Recordkey = knownStudentId3 });
            personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000003", PstStudentAcadCred = new List<string>() { "19003", "19004", "19005" }, PstAdvisement = new List<string>(), PstRestrictions = new List<string>() });
            Ellucian.Colleague.Data.Base.DataContracts.Person person3 = new Ellucian.Colleague.Data.Base.DataContracts.Person()
            {
                Recordkey = "0000003",
                LastName = "Legal Last",
                FirstName = "Legal First",
                MiddleName = "Legal Middle",
                Prefix = "Mr.",
                Suffix = "Esq.",
                PreferredName = "IM"
            };
            Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson ForeignPerson3 = new Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson() { Recordkey = "0000003" };
            person3.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
            people.Add(person3);
            foreignPeople.Add(ForeignPerson3);
        }

        private Collection<FinAid> BuildFinAidRecords(List<TestStudentRepository.FaStudent> faStudentData)
        {
            var finAidCollection = new Collection<FinAid>();
            foreach (var faStudentRecord in faStudentData)
            {
                var finAidDataContract = new FinAid();
                finAidDataContract.Recordkey = faStudentRecord.studentId;
                finAidDataContract.FaCounselorsEntityAssociation = new List<FinAidFaCounselors>();
                foreach (var counselorItem in faStudentRecord.faCounselors)
                {
                    var finAidFaCounselorsDataContract = new FinAidFaCounselors();
                    finAidFaCounselorsDataContract.FaCounselorAssocMember = counselorItem.counselorId;
                    finAidFaCounselorsDataContract.FaCounselorStartDateAssocMember = counselorItem.startDate;
                    finAidFaCounselorsDataContract.FaCounselorEndDateAssocMember = counselorItem.endDate;

                    finAidDataContract.FaCounselorsEntityAssociation.Add(finAidFaCounselorsDataContract);
                }
                finAidDataContract.FaCounselor = finAidDataContract.FaCounselorsEntityAssociation.Select(f => f.FaCounselorAssocMember).ToList();
                finAidDataContract.FaCounselorStartDate = finAidDataContract.FaCounselorsEntityAssociation.Select(f => f.FaCounselorStartDateAssocMember).ToList();
                finAidDataContract.FaCounselorEndDate = finAidDataContract.FaCounselorsEntityAssociation.Select(f => f.FaCounselorEndDateAssocMember).ToList();

                finAidCollection.Add(finAidDataContract);
            }
            return finAidCollection;
        }

        private StudentRepository BuildMockStudentRepository()
        {
            // mock data accessor STUDENTS response - valid id 0000001
            string[] knownStudentIds = { knownStudentId1 };
            dataReaderMock.Setup<Task<DataContracts.Students>>(a => a.ReadRecordAsync<DataContracts.Students>(knownStudentId1, true)).ReturnsAsync(students[0]);
            dataReaderMock.Setup<Task<Collection<DataContracts.Students>>>(a => a.BulkReadRecordAsync<DataContracts.Students>(knownStudentIds, true)).ReturnsAsync(new Collection<DataContracts.Students> { students[0] });

            // mock data accessor APPLICANTS response - valid id 0000001
            dataReaderMock.Setup<Task<DataContracts.Applicants>>(a => a.ReadRecordAsync<DataContracts.Applicants>(knownStudentId1, true)).ReturnsAsync(applicants[0]);
            dataReaderMock.Setup<Task<Collection<DataContracts.Applicants>>>(a => a.BulkReadRecordAsync<DataContracts.Applicants>(knownStudentIds, true)).ReturnsAsync(new Collection<DataContracts.Applicants> { applicants[0] });

            // mock data accessor PERSON response - valid id 0000001
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.Person>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", knownStudentId1, true)).ReturnsAsync(people[0]);
            dataReaderMock.Setup<Task<Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>>>(a => a.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", knownStudentIds, true)).ReturnsAsync(new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> { people[0] });

            // mock data accessor FOREIGN.PERSON response - valid id 0000001
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson>(knownStudentId1, true)).ReturnsAsync(foreignPeople[0]);
            dataReaderMock.Setup<Task<Collection<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson>>>(a => a.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson>(knownStudentIds, true)).ReturnsAsync(new Collection<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson> { foreignPeople[0] });

            // mock data accessor STUDENTS response - valid id 0000002
            string[] knownStudentIds2 = { knownStudentId2 };
            dataReaderMock.Setup<Task<DataContracts.Students>>(a => a.ReadRecordAsync<DataContracts.Students>(knownStudentId2, true)).ReturnsAsync(students[1]);
            dataReaderMock.Setup<Task<Collection<DataContracts.Students>>>(a => a.BulkReadRecordAsync<DataContracts.Students>(knownStudentIds2, true)).ReturnsAsync(new Collection<DataContracts.Students> { students[1] });

            // mock data accessor APPLICANTS response - valid id 0000002
            dataReaderMock.Setup<Task<DataContracts.Applicants>>(a => a.ReadRecordAsync<DataContracts.Applicants>(knownStudentId2, true)).ReturnsAsync(applicants[1]);

            // mock data accessor PERSON response - valid id 0000002
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.Person>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", knownStudentId2, true)).ReturnsAsync(people[1]);
            dataReaderMock.Setup<Task<Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>>>(a => a.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", knownStudentIds2, true)).ReturnsAsync(new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> { people[1] });

            // mock data accessor FOREIGN.PERSON response - valid id 0000002
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson>(knownStudentId2, true)).ReturnsAsync(foreignPeople[1]);

            // mock data accessor STUDENTS response - valid id 0000003
            dataReaderMock.Setup<Task<DataContracts.Students>>(a => a.ReadRecordAsync<DataContracts.Students>(knownStudentId3, true)).ReturnsAsync(students[2]);
            string[] knownStudentIds3 = { knownStudentId3 };
            dataReaderMock.Setup<Task<Collection<DataContracts.Students>>>(a => a.BulkReadRecordAsync<DataContracts.Students>(knownStudentIds3, true)).ReturnsAsync(new Collection<DataContracts.Students> { students[2] });
            
            // mock data accessor APPLICANTS response - valid id 0000003
            dataReaderMock.Setup<Task<DataContracts.Applicants>>(a => a.ReadRecordAsync<DataContracts.Applicants>(knownStudentId3, true)).ReturnsAsync(applicants[2]);

            // mock data accessor PERSON response - valid id 0000003
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.Person>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", knownStudentId3, true)).ReturnsAsync(people[2]);
            dataReaderMock.Setup<Task<Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>>>(a => a.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", knownStudentIds3, true)).ReturnsAsync(new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> { people[2] });
            // mock data accessor FOREIGN.PERSON response - valid id 0000003
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.ForeignPerson>(knownStudentId3, true)).ReturnsAsync(foreignPeople[2]);

            // mock data accessor STUDENTS response - unknown id 9999999
            DataContracts.Students nullStudent = null;
            dataReaderMock.Setup<Task<DataContracts.Students>>(a => a.ReadRecordAsync<DataContracts.Students>(unknownStudentId, true)).ReturnsAsync(nullStudent);

            Ellucian.Colleague.Data.Base.DataContracts.Person nullPerson = null;
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.Person>>(a => a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", unknownStudentId, true)).ReturnsAsync(nullPerson);

            // mock data accessor STUDENT.PROGRAM.STATUSES
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
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

            // mock data accessor PERSON.ETHNICS
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.ETHNICS", true))
                .ReturnsAsync(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "W", "H" },
                    ValExternalRepresentation = new List<string>() { "White", "Hispanic" },
                    ValActionCode1 = new List<string>() { "W", "H" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "W",
                            ValExternalRepresentationAssocMember = "White",
                            ValActionCode1AssocMember = "W"
                        },
                                                new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "H",
                            ValExternalRepresentationAssocMember = "Hispanic",
                            ValActionCode1AssocMember = "H"
                        }
                    }
                });

            // mock data accessor PERSON.RACES
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.RACES", true))
                .ReturnsAsync(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "CA", "PI" },
                    ValExternalRepresentation = new List<string>() { "Caucasian", "Pacific Islander" },
                    ValActionCode1 = new List<string>() { "1", "2" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "CA",
                            ValExternalRepresentationAssocMember = "Caucasian",
                            ValActionCode1AssocMember = "1"
                        },
                         new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "PI",
                            ValExternalRepresentationAssocMember = "Pacific Islander",
                            ValActionCode1AssocMember = "2"
                        }
                    }
                });

            // mock data accessor MARITAL.STATUSES
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "MARITAL.STATUSES", true))
                .ReturnsAsync(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "S", "M", "D", "W" },
                    ValExternalRepresentation = new List<string>() { "Single", "Married", "Divorced", "Widowed" },
                    ValActionCode1 = new List<string>() { "1", "2", "3", "4" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "S",
                            ValExternalRepresentationAssocMember = "Single",
                            ValActionCode1AssocMember = "1"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "M",
                            ValExternalRepresentationAssocMember = "Married",
                            ValActionCode1AssocMember = "2"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "D",
                            ValExternalRepresentationAssocMember = "Divorced",
                            ValActionCode1AssocMember = "3"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "W",
                            ValExternalRepresentationAssocMember = "Widowed",
                            ValActionCode1AssocMember = "W"
                        }
                    }
                });

            // mock data accessor EDUCATION.GOALS
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
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

            // mock DEFAULTS from CORE.PARMS
            dataReaderMock.Setup<Task<Defaults>>(a =>
                a.ReadRecordAsync<Defaults>("CORE.PARMS", "DEFAULTS", true))
                .ReturnsAsync(new Defaults()
                {
                    DefaultHostCorpId = "0000024",

                });

            // mock data accessor STUDENT.PROGRAMS

            string[] programs1 = { };
            dataReaderMock.Setup<Task<Collection<DataContracts.StudentPrograms>>>(a => a.BulkReadRecordAsync<DataContracts.StudentPrograms>(programs1, true)).ReturnsAsync(new Collection<DataContracts.StudentPrograms>());

            string[] programs2 = { "0000002*BA.CPSC" };
            dataReaderMock.Setup<Task<Collection<DataContracts.StudentPrograms>>>(a =>
                a.BulkReadRecordAsync<DataContracts.StudentPrograms>(programs2, true))
                .ReturnsAsync(new Collection<DataContracts.StudentPrograms>()
                {
                    new DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId2 + "*" + acadPrograms[0],
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprCatalog = "2012"
                    }
                });

            //dataReaderMock.Setup<Task<Collection<DataContracts.StudentPrograms>>(a => a.BulkReadRecordAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", "@ID LIKE '" + knownStudentId3 + "*...'"))
            string[] programs3 = { "0000003*BA.CPSC", "0000003*BA-MATH" };
            dataReaderMock.Setup<Task<Collection<DataContracts.StudentPrograms>>>(a => a.BulkReadRecordAsync<DataContracts.StudentPrograms>(programs3, true))
                .ReturnsAsync(new Collection<DataContracts.StudentPrograms>()
                {
                    new DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId3 + "*" + acadPrograms[0],
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprCatalog = "2012"
                    },
                    new DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId3 + "*" + acadPrograms[1],
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprCatalog = "2011"
                    },
                    new DataContracts.StudentPrograms
                    {   // ended student program
                        Recordkey = knownStudentId3 + "*" + "BA.UNDC",
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprEndDate = new List<DateTime?>() {DateTime.Today.AddDays(-60)},
                        StprCatalog = "2010"
                    }
                });
            dataReaderMock.Setup<Task<Collection<DataContracts.StudentPrograms>>>(a => a.BulkReadRecordAsync<DataContracts.StudentPrograms>("STUDENT.PROGRAMS", "@ID LIKE '" + unknownStudentId + "*...'", true)).ReturnsAsync(new Collection<DataContracts.StudentPrograms>());

            // mock data accessor PERSON.ST response - valid id 0000001
            dataReaderMock.Setup<Task<Base.DataContracts.PersonSt>>(a => a.ReadRecordAsync<Base.DataContracts.PersonSt>(knownStudentId1, true)).ReturnsAsync(personStRecords[0]);
            dataReaderMock.Setup<Task<Collection<Base.DataContracts.PersonSt>>>(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(knownStudentIds, true)).ReturnsAsync(new Collection<Base.DataContracts.PersonSt> { personStRecords[0] });

            // mock data accessor PERSON.ST response - valid id 0000002
            dataReaderMock.Setup<Task<Base.DataContracts.PersonSt>>(a => a.ReadRecordAsync<Base.DataContracts.PersonSt>(knownStudentId2, true)).ReturnsAsync(personStRecords[1]);
            dataReaderMock.Setup<Task<Collection<Base.DataContracts.PersonSt>>>(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(knownStudentIds2, true)).ReturnsAsync(new Collection<Base.DataContracts.PersonSt> { personStRecords[1] });

            // mock data accessor PERSON.ST response - valid id 0000003
            dataReaderMock.Setup<Task<Base.DataContracts.PersonSt>>(a => a.ReadRecordAsync<Base.DataContracts.PersonSt>(knownStudentId3, true)).ReturnsAsync(personStRecords[2]);

            // mock data accessor DEGREE_PLANS response  - for student 0000001
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId1 + "'")).ReturnsAsync(new string[] { "1" });

            // mock data accessor DEGREE_PLANS response  - for student 0000002
            var multipleDegreePlanKeys = new string[] { "2", "3" };
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId2 + "'")).ReturnsAsync(multipleDegreePlanKeys);

            // mock data accessor DEGREE_PLANS response  - for student 0000003
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId3 + "'")).ReturnsAsync(new string[] { });

            // mock data accessor STUDENT.ADVISEMENT for students with no advisor
            dataReaderMock.Setup<Task<Collection<DataContracts.StudentAdvisement>>>(d => d.BulkReadRecordAsync<DataContracts.StudentAdvisement>(new List<string>().ToArray(), true)).ReturnsAsync(new Collection<DataContracts.StudentAdvisement>());

            // mock data accessor STUDENT.ADVISEMENT for student 0000002
            string[] student1AdvismentIds = new List<string>() { "21", "22" }.ToArray();
            dataReaderMock.Setup<Task<Collection<DataContracts.StudentAdvisement>>>(d => d.BulkReadRecordAsync<DataContracts.StudentAdvisement>(student1AdvismentIds, true)).ReturnsAsync(studentAdvisements);

            //mock data accessor FIN.AID for each student
            foreach (var personId in personIds)
            {
                var finAidResponse = finAidRecords.FirstOrDefault(f => f.Recordkey == personId);
                dataReaderMock.Setup(d => d.ReadRecordAsync<FinAid>(personId, true)).ReturnsAsync(finAidResponse);
            }

            var stWebDflt = BuildStwebDefaults();
            //Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true);
            dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).ReturnsAsync(stWebDflt);

            // mock data reader for getting the STUDENT Name Addr Hierarchy
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "STUDENT", true))
                .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                {
                    Recordkey = "STUDENT",
                    NahNameHierarchy = new List<string>() { "YYY", "CHL", "PF" }
                });


            StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

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
    public class StudentRepositoryTests_GetGradeRestrictions : BasePersonSetup
    {
        const string CacheName = "Ellucian.Web.Student.Data.Colleague.Repository.StudentRepository";
        string unrestrictedStudentId = "1111111";
        string restrictedStudentId = "2222222";
        private StudentRepository studentRepository;
        Domain.Student.Entities.GradeRestriction gradeRestriction1;
        Domain.Student.Entities.GradeRestriction gradeRestriction2;
        GetGradeViewRestrictionsRequest updateRequest;

        [TestInitialize]
        public async void Initialize()
        {
            PersonSetupInitialize();

            studentRepository = BuildMockStudentRepository();
            gradeRestriction1 = await studentRepository.GetGradeRestrictionsAsync(unrestrictedStudentId);
            gradeRestriction2 = await studentRepository.GetGradeRestrictionsAsync(restrictedStudentId);
        }

        [TestCleanup]
        public void TestCleanup()
        {

        }

        [TestMethod]
        public async Task UnrestrictedStudent_IsRestricted()
        {
            Assert.AreEqual(false, gradeRestriction1.IsRestricted);
        }

        [TestMethod]
        public async Task UnrestrictedStudent_Reasons()
        {
            Assert.IsNotNull(gradeRestriction1.Reasons);
            Assert.AreEqual(0, gradeRestriction1.Reasons.Count());

        }

        [TestMethod]
        public async Task RestrictedStudent_IsRestricted()
        {
            Assert.AreEqual(true, gradeRestriction2.IsRestricted);
        }

        [TestMethod]
        public async Task RestrictedStudent_Reasons()
        {
            Assert.AreEqual(2, gradeRestriction2.Reasons.Count());
        }

        private StudentRepository BuildMockStudentRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();
            var loggerMock = new Mock<ILogger>();
            // Set up data accessor for mocking (needed for get)
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Set up transaction manager for mocking (needed for add and update)
            var mockManager = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

            // Set up the response for getting a GradeRequest for an unrestricted student
            var gradeRestrictionResponse1 = new GetGradeViewRestrictionsResponse();
            gradeRestrictionResponse1.IsRestricted = "N";
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetGradeViewRestrictionsRequest, GetGradeViewRestrictionsResponse>(It.Is<GetGradeViewRestrictionsRequest>(r => r.PersonId == "1111111"))).ReturnsAsync(gradeRestrictionResponse1).Callback<GetGradeViewRestrictionsRequest>(req => updateRequest = req);

            // Set up the response for getting a GradeRequest for an unrestricted student
            var gradeRestrictionResponse2 = new GetGradeViewRestrictionsResponse();
            gradeRestrictionResponse2.IsRestricted = "Y";
            gradeRestrictionResponse2.Reasons = new List<string>() { "Library fine", "Room damage" };
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetGradeViewRestrictionsRequest, GetGradeViewRestrictionsResponse>(It.Is<GetGradeViewRestrictionsRequest>(r => r.PersonId == "2222222"))).ReturnsAsync(gradeRestrictionResponse2).Callback<GetGradeViewRestrictionsRequest>(req => updateRequest = req);

            // mock data accessor STUDENT.PROGRAM.STATUSES
            dataAccessorMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", false))
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

            StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return repository;
        }
    }

    [TestClass]
    public class StudentRepository_GetMany_Tests : BasePersonSetup
    {
        private TestStudentRepository testStudentRepository;
        private StudentRepository studentRepository;
        const string CacheName = "Ellucian.Web.Student.Data.Colleague.Repository.StudentRepository";
        private string knownStudentId1;
        private string knownStudentId2;
        private string knownStudentId3;
        private IEnumerable<string> ids;
        private string unknownStudentId;
        private string[] acadPrograms;
        private string quote;

        private Collection<DataContracts.Students> students;
        private Collection<Base.DataContracts.PersonSt> personStRecords;
        private Collection<DataContracts.StudentAdvisement> studentAdvisements;
        private Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;
        private Collection<FinAid> finAidRecords;

        [TestInitialize]
        public void Initialize()
        {
            testStudentRepository = new TestStudentRepository();
            students = new Collection<DataContracts.Students>();
            personStRecords = new Collection<Base.DataContracts.PersonSt>();
            studentAdvisements = new Collection<DataContracts.StudentAdvisement>();
            people = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>();
            finAidRecords = BuildFinAidRecords(testStudentRepository.faStudentData);
            quote = '"'.ToString();

            // Initialize person setup and Mock framework
            PersonSetupInitialize();

            SetupData();

            studentRepository = BuildMockStudentRepository();
        }



        [TestCleanup]
        public void TestCleanup()
        {
            studentRepository = null;
        }

        [TestMethod]
        public async Task CheckPerRaceFromEntities()
        {
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetAsync(ids);
            Assert.IsNull(students.First().RaceCodes);
        }


        [TestMethod]
        public async Task Get_StudentIds_Multiple()
        {
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetAsync(ids);
            Assert.AreEqual(3, students.Count());
        }

        [TestMethod]
        public async Task Get_Students_UnknownID()
        {
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetAsync(new List<string>() { unknownStudentId });
            Assert.AreEqual(0, students.Count());
        }

        [TestMethod]
        public async Task Get_Students_TestProperties()
        {

            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.GetAsync(ids);
            Domain.Student.Entities.Student student = students.Where(s => s.Id == knownStudentId2).FirstOrDefault();

            var expectedFaCounselorId = finAidRecords.First(a => a.Recordkey == knownStudentId2)
                .FaCounselorsEntityAssociation.First(c =>
                    (!c.FaCounselorEndDateAssocMember.HasValue ||
                    DateTime.Today <= c.FaCounselorEndDateAssocMember.Value) &&
                    (!c.FaCounselorStartDateAssocMember.HasValue ||
                    DateTime.Today >= c.FaCounselorStartDateAssocMember)
                    ).FaCounselorAssocMember;

            // Excludes advisor with an end date.
            Assert.AreEqual(1, student.AdvisorIds.Count());
            Assert.AreEqual(1, student.ProgramIds.Count);
            Assert.AreEqual(acadPrograms[0], student.ProgramIds[0]);
            Assert.AreEqual(true, student.DegreePlanId.HasValue);
            Assert.AreEqual("djones@yahoo.com", student.PreferredEmailAddress.Value);
            Assert.AreEqual(expectedFaCounselorId, student.FinancialAidCounselorId);
        }

        private async Task SetupData()
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
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId1, StuAcadPrograms = null });
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "11", StadStudent = "0000001", StadFaculty = "0000036", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = null });
            personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000001", PstStudentAcadCred = new List<string>() { "19000" }, PstAdvisement = new List<string>() { "11" }, PstRestrictions = new List<string>() { "R001", "R002" } });
            Ellucian.Colleague.Data.Base.DataContracts.Person person1 = new Ellucian.Colleague.Data.Base.DataContracts.Person() { Recordkey = "0000001", LastName = "Smith" };
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
            personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000002", PstStudentAcadCred = new List<string>() { "" }, PstAdvisement = new List<string>() { "21", "22" }, PstRestrictions = new List<string>() });
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId2, StuAcadPrograms = new List<string> { acadPrograms[0] } });
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "21", StadStudent = "0000002", StadFaculty = "0000036", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = null });
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "22", StadStudent = "0000002", StadFaculty = "0000045", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = new DateTime(2012, 8, 1) });
            Ellucian.Colleague.Data.Base.DataContracts.Person person2 = new Ellucian.Colleague.Data.Base.DataContracts.Person() { Recordkey = "0000002", LastName = "Jones" };
            person2.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
            PersonPeopleEmail ppe3 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "junk@yahoo.com", PersonEmailTypesAssocMember = "HOME", PersonPreferredEmailAssocMember = "" };
            person2.PeopleEmailEntityAssociation.Add(ppe3);
            PersonPeopleEmail ppe4 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "djones@yahoo.com", PersonEmailTypesAssocMember = "BUS", PersonPreferredEmailAssocMember = "Y" };
            person2.PeopleEmailEntityAssociation.Add(ppe4);
            people.Add(person2);
            /* 
             * student 0000003
             * > two programs
             * > no degree plan
             * > three acad creds
             * > no email addresses on file
             */
            knownStudentId3 = "0000003";
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId3, StuAcadPrograms = new List<string> { acadPrograms[0], acadPrograms[1] } });
            personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000003", PstStudentAcadCred = new List<string>() { "19003", "19004", "19005" }, PstAdvisement = new List<string>(), PstRestrictions = new List<string>() });
            Ellucian.Colleague.Data.Base.DataContracts.Person person3 = new Ellucian.Colleague.Data.Base.DataContracts.Person() { Recordkey = "0000003", LastName = "Jones" };
            person3.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
            people.Add(person3);

            ids = new List<string>() { knownStudentId1, knownStudentId2, knownStudentId3 };
        }

        private Collection<FinAid> BuildFinAidRecords(List<TestStudentRepository.FaStudent> faStudentData)
        {
            var finAidCollection = new Collection<FinAid>();
            foreach (var faStudentRecord in faStudentData)
            {
                var finAidDataContract = new FinAid();
                finAidDataContract.Recordkey = faStudentRecord.studentId;
                finAidDataContract.FaCounselorsEntityAssociation = new List<FinAidFaCounselors>();
                foreach (var counselorItem in faStudentRecord.faCounselors)
                {
                    var finAidFaCounselorsDataContract = new FinAidFaCounselors();
                    finAidFaCounselorsDataContract.FaCounselorAssocMember = counselorItem.counselorId;
                    finAidFaCounselorsDataContract.FaCounselorStartDateAssocMember = counselorItem.startDate;
                    finAidFaCounselorsDataContract.FaCounselorEndDateAssocMember = counselorItem.endDate;

                    finAidDataContract.FaCounselorsEntityAssociation.Add(finAidFaCounselorsDataContract);
                }
                finAidDataContract.FaCounselor = finAidDataContract.FaCounselorsEntityAssociation.Select(f => f.FaCounselorAssocMember).ToList();
                finAidDataContract.FaCounselorStartDate = finAidDataContract.FaCounselorsEntityAssociation.Select(f => f.FaCounselorStartDateAssocMember).ToList();
                finAidDataContract.FaCounselorEndDate = finAidDataContract.FaCounselorsEntityAssociation.Select(f => f.FaCounselorEndDateAssocMember).ToList();

                finAidCollection.Add(finAidDataContract);
            }
            return finAidCollection;
        }

        private StudentRepository BuildMockStudentRepository()
        {

            // mock data accessor STUDENTS response - valid ids 0000001, 0000002, 0000003
            dataReaderMock.Setup<Task<Collection<Students>>>(a => a.BulkReadRecordAsync<Students>(It.IsAny<string[]>(), true)).ReturnsAsync(students);
            dataReaderMock.Setup<Task<Collection<Students>>>(a => a.BulkReadRecordAsync<Students>(ids.ToArray(), true)).ReturnsAsync(students);

            // mock data accessor PERSON response - valid ids 0000001, 0000002, 0000003
            dataReaderMock.Setup<Task<Collection<Person>>>(a => a.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(people);
            dataReaderMock.Setup<Task<Collection<Person>>>(a => a.BulkReadRecordAsync<Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);
            //dataReaderMock.Setup<Task<Person>(a => a.ReadRecordAsync<Person>("PERSON", "0000001", true)).ReturnsAsync(people.ElementAt(0));
            //dataReaderMock.Setup<Task<Person>(a => a.ReadRecordAsync<Person>("PERSON", "0000002", true)).ReturnsAsync(people.ElementAt(1));
            //dataReaderMock.Setup<Task<Person>(a => a.ReadRecordAsync<Person>("PERSON", "0000003", true)).ReturnsAsync(people.ElementAt(2));

            // mock data accessor STUDENT.PROGRAM.STATUSES
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
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

            // mock data accessor STUDENT.PROGRAMS
            Collection<StudentPrograms> studentPrograms = new Collection<StudentPrograms>() {new DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId3 + "*" + acadPrograms[0],
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprCatalog = "2012"
                    },
                    new DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId3 + "*" + acadPrograms[1],
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprCatalog = "2011"
                    },
                    new DataContracts.StudentPrograms
                    {   // ended student program
                        Recordkey = knownStudentId3 + "*" + "BA.UNDC",
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprEndDate = new List<DateTime?>() {DateTime.Today.AddDays(-60)},
                        StprCatalog = "2010"
                    }, new DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId2 + "*" + acadPrograms[0],
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprCatalog = "2012"
                    } };
            dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(a => a.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(studentPrograms);

            // mock data accessor PERSON.ST response - valid ids 0000001, 0000002, 0000003
            dataReaderMock.Setup<Task<Collection<PersonSt>>>(a => a.BulkReadRecordAsync<PersonSt>(It.IsAny<string[]>(), true)).ReturnsAsync(personStRecords);

            // mock data accessor DEGREE_PLANS response  - for student 0000001
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId1 + "'")).ReturnsAsync(new string[] { "1" });

            // mock data accessor DEGREE_PLANS response  - for student 0000002
            var multipleDegreePlanKeys = new string[] { "2", "3" };
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId2 + "'")).ReturnsAsync(multipleDegreePlanKeys);

            // mock data accessor DEGREE_PLANS response  - for student 0000003
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId3 + "'")).ReturnsAsync(new string[] { });

            // mock data accessor STUDENT.ADVISEMENT for student 0000002
            //string[] studentAdvismentIds = new List<string>() { "11", "21", "22" }.ToArray();
            //string[] student1AdvismentIds = new List<string>() { "11" }.ToArray();
            //string[] student2AdvismentIds = new List<string>() { "21", "22" }.ToArray();

            dataReaderMock.Setup<Task<Collection<StudentAdvisement>>>(d => d.BulkReadRecordAsync<StudentAdvisement>(It.IsAny<string[]>(), true)).ReturnsAsync(studentAdvisements);
            //dataReaderMock.Setup<Task<Collection<StudentAdvisement>>(d => d.BulkReadRecordAsync<StudentAdvisement>(studentAdvismentIds, true)).ReturnsAsync(studentAdvisements);
            //dataReaderMock.Setup<Task<Collection<StudentAdvisement>>(d => d.BulkReadRecordAsync<StudentAdvisement>(student1AdvismentIds, true)).ReturnsAsync(studentAdvisements);
            //dataReaderMock.Setup<Task<Collection<StudentAdvisement>>(d => d.BulkReadRecordAsync<StudentAdvisement>(student2AdvismentIds, true)).ReturnsAsync(studentAdvisements);

            //mock data accessor for FIN.AID records
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<FinAid>(It.IsAny<string[]>(), true)).ReturnsAsync(finAidRecords);

            //StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return repository;
        }
    }

    [TestClass]
    public class StudentRepository_CheckRegistrationEligibility : BasePersonSetup
    {
        private StudentRepository studentRepository;
        const string CacheName = "Ellucian.Web.Student.Data.Colleague.Repository.StudentRepository";
        private string studentId1;
        private string studentId2;

        private Collection<DataContracts.Students> students;
        private Collection<Base.DataContracts.PersonSt> personStRecords;
        private Collection<DataContracts.StudentAdvisement> studentAdvisements;
        private Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;

        [TestInitialize]
        public void Initialize()
        {
            students = new Collection<DataContracts.Students>();
            personStRecords = new Collection<Base.DataContracts.PersonSt>();
            studentAdvisements = new Collection<DataContracts.StudentAdvisement>();
            people = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>();

            // Initialize person setup and Mock framework
            PersonSetupInitialize();
            SetupData();

            studentRepository = BuildMockStudentRepository();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            studentRepository = null;
        }

        [TestMethod]
        public async Task CheckRegistrationEligibility_PassNoMessages()
        {
            // eligible
            var response1 = new CheckRegistrationEligibilityResponse() { Eligible = true, Messages = new List<string>() };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<CheckRegistrationEligibilityRequest, CheckRegistrationEligibilityResponse>(It.IsAny<CheckRegistrationEligibilityRequest>()))
                .ReturnsAsync(response1);
            var result = await studentRepository.CheckRegistrationEligibilityAsync(studentId2);
            Assert.AreEqual(0, result.Messages.Count());
            Assert.AreEqual(true, result.IsEligible);
        }

        [TestMethod]
        public async Task CheckRegistrationEligibility_PassWithMessages()
        {
            // eligible
            var response1 = new CheckRegistrationEligibilityResponse() { Eligible = true, Messages = new List<string>() { "eligible" } };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<CheckRegistrationEligibilityRequest, CheckRegistrationEligibilityResponse>(It.IsAny<CheckRegistrationEligibilityRequest>()))
                .ReturnsAsync(response1);
            var result = await studentRepository.CheckRegistrationEligibilityAsync(studentId2);
            Assert.AreEqual(1, result.Messages.Count());
            Assert.AreEqual(true, result.IsEligible);
        }

        [TestMethod]
        public async Task CheckRegistrationEligibility_FailWithMessages()
        {
            // not eligible
            var response2 = new CheckRegistrationEligibilityResponse() { Eligible = false, Messages = new List<string>() { "not eligible", "message2" } };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<CheckRegistrationEligibilityRequest, CheckRegistrationEligibilityResponse>(It.IsAny<CheckRegistrationEligibilityRequest>()))
                .ReturnsAsync(response2);
            var result = await studentRepository.CheckRegistrationEligibilityAsync(studentId2);
            Assert.AreEqual(2, result.Messages.Count());
            Assert.AreEqual(false, result.IsEligible);
        }

        [TestMethod]
        public async Task CheckRegistrationEligibility_FailNoMessages()
        {
            // eligible
            var response1 = new CheckRegistrationEligibilityResponse() { Eligible = false, Messages = new List<string>() };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<CheckRegistrationEligibilityRequest, CheckRegistrationEligibilityResponse>(It.IsAny<CheckRegistrationEligibilityRequest>()))
                .ReturnsAsync(response1);
            var result = await studentRepository.CheckRegistrationEligibilityAsync(studentId2);
            Assert.AreEqual(0, result.Messages.Count());
            Assert.AreEqual(false, result.IsEligible);
            Assert.AreEqual(false, result.HasOverride);
        }

        [TestMethod]
        public async Task CheckRegistrationEligibility_Ineligible()
        {
            // not eligible, user has override
            var response = new CheckRegistrationEligibilityResponse() { Eligible = false, Messages = new List<string>() { "not eligible", "message2" }, HasOverride = false };
            response.Terms = new List<Transactions.Terms>();
            response.Terms.Add(new Transactions.Terms() { TermPriorityOverride = false });
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<CheckRegistrationEligibilityRequest, CheckRegistrationEligibilityResponse>(It.IsAny<CheckRegistrationEligibilityRequest>()))
                .ReturnsAsync(response);
            var result = await studentRepository.CheckRegistrationEligibilityAsync(studentId2);
            Assert.AreEqual(2, result.Messages.Count());
            Assert.AreEqual(false, result.IsEligible);
            Assert.AreEqual(false, result.HasOverride);
        }

        [TestMethod]
        public async Task CheckRegistrationEligibility_IneligibleHasOverride()
        {
            // not eligible, user has override
            var response2 = new CheckRegistrationEligibilityResponse() { Eligible = false, Messages = new List<string>() { "not eligible", "message2" }, HasOverride = true };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<CheckRegistrationEligibilityRequest, CheckRegistrationEligibilityResponse>(It.IsAny<CheckRegistrationEligibilityRequest>()))
                .ReturnsAsync(response2);
            var result = await studentRepository.CheckRegistrationEligibilityAsync(studentId2);
            Assert.AreEqual(2, result.Messages.Count());
            Assert.AreEqual(false, result.IsEligible);
            Assert.AreEqual(true, result.HasOverride);
        }

        [TestMethod]
        public async Task CheckRegistrationEligibility_TermTests()
        {
            var regResponse = await BuildEligibilityResponse();
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<CheckRegistrationEligibilityRequest, CheckRegistrationEligibilityResponse>(It.IsAny<CheckRegistrationEligibilityRequest>()))
                .ReturnsAsync(regResponse);
            var result = await studentRepository.CheckRegistrationEligibilityAsync(studentId2);
            Assert.AreEqual(true, result.IsEligible);
            Assert.AreEqual(false, result.HasOverride);
            Assert.AreEqual(6, result.Terms.Count);
            var testRegElig = await new TestStudentRepository().CheckRegistrationEligibilityAsync("student");
            foreach (var term in testRegElig.Terms)
            {
                var testTerm = result.Terms.Where(t => t.TermCode == term.TermCode).FirstOrDefault();
                if (testTerm != null)
                {
                    Assert.AreEqual(term.Status, testTerm.Status);
                    Assert.AreEqual(term.CheckPriority, testTerm.CheckPriority);
                    Assert.AreEqual(term.Message, testTerm.Message);
                    Assert.AreEqual(term.PriorityOverridable, testTerm.PriorityOverridable);
                    Assert.AreEqual(term.AnticipatedTimeForAdds, testTerm.AnticipatedTimeForAdds);
                }
                else
                {
                    // Missing a term - this is an error.
                    throw new Exception();
                }
            }
        }

        private async Task SetupData()
        {
            /* 
             * student 0000001
             * > no programs
             * > one degree plan
             * > one acad cred
             * > no advisors
             * > 2 emails - neither marked as preferred
             */
            studentId1 = "0000001";
            studentId2 = "0000002";

            students.Add(new DataContracts.Students() { Recordkey = studentId1, StuAcadPrograms = null });
            students.Add(new DataContracts.Students() { Recordkey = studentId2, StuAcadPrograms = null });

            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "11", StadStudent = "0000001", StadFaculty = "0000036", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = null });

            personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000001", PstStudentAcadCred = new List<string>() { "19000" }, PstAdvisement = new List<string>() { "11" }, PstRestrictions = new List<string>() { "R001", "R002" } });
            personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000002", PstStudentAcadCred = new List<string>() { "19000" }, PstAdvisement = new List<string>() { "11" }, PstRestrictions = new List<string>() { "R001", "R002" } });

            Ellucian.Colleague.Data.Base.DataContracts.Person person1 = new Ellucian.Colleague.Data.Base.DataContracts.Person() { Recordkey = "0000001", LastName = "Smith" };
            person1.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
            PersonPeopleEmail ppe1 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "dsmith@yahoo.com", PersonEmailTypesAssocMember = "HOME", PersonPreferredEmailAssocMember = "" };
            person1.PeopleEmailEntityAssociation.Add(ppe1);
            people.Add(person1);

            Ellucian.Colleague.Data.Base.DataContracts.Person person2 = new Ellucian.Colleague.Data.Base.DataContracts.Person() { Recordkey = "0000002", LastName = "Smith" };
            person2.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
            PersonPeopleEmail ppe2 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "junk@yahoo.com", PersonEmailTypesAssocMember = "BUS", PersonPreferredEmailAssocMember = "" };
            person2.PeopleEmailEntityAssociation.Add(ppe2);
            people.Add(person2);

        }

        private StudentRepository BuildMockStudentRepository()
        {

            // mock data accessor STUDENTS response - valid ids 0000001, 0000002
            dataReaderMock.Setup<Task<Collection<Students>>>(a => a.BulkReadRecordAsync<Students>(It.IsAny<string[]>(), true)).ReturnsAsync(students);

            // mock data accessor PERSON response - valid ids 0000001, 0000002
            dataReaderMock.Setup<Task<Collection<Person>>>(a => a.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(people);

            // mock data accessor STUDENT.PROGRAM.STATUSES
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
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

            // mock data accessor STUDENT.PROGRAMS
            dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(a => a.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentPrograms>());

            // mock data accessor PERSON.ST response - valid ids 0000001, 0000002
            dataReaderMock.Setup<Task<Collection<PersonSt>>>(a => a.BulkReadRecordAsync<PersonSt>(It.IsAny<string[]>(), true)).ReturnsAsync(personStRecords);

            // mock data accessor DEGREE_PLANS response  - for student 0000001, 0000002
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + studentId1 + "'")).ReturnsAsync(new string[] { "1" });
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + studentId2 + "'")).ReturnsAsync(new string[] { "2" });

            StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return repository;
        }

        private async Task<CheckRegistrationEligibilityResponse> BuildEligibilityResponse()
        {
            var regElig = await new TestStudentRepository().CheckRegistrationEligibilityAsync("test");
            var response = new CheckRegistrationEligibilityResponse();
            response.HasOverride = regElig.HasOverride;
            response.Messages = new List<string>();
            response.Eligible = regElig.IsEligible;
            foreach (var term in regElig.Terms)
            {
                var newTerm = new Ellucian.Colleague.Data.Student.Transactions.Terms() { TermCode = term.TermCode, TermCheckPriority = term.CheckPriority, TermPriorityOverride = term.PriorityOverridable, TermAddMessages = term.Message };
                if (term.TermCode == "term1" || term.TermCode == "term3")
                {
                    newTerm.TermAddAllowed = true;
                }
                if (term.TermCode == "term3" || term.TermCode == "term4" || term.TermCode == "term5")
                {
                    newTerm.TermAddCheckDate = new DateTime(2020, 9, 1, 2, 12, 0);
                }
                if (term.TermCode == "term6")
                {
                    newTerm.TermAddCheckDate = new DateTime(2012, 9, 1, 2, 12, 0);
                }
                response.Terms.Add(newTerm);
            }

            return response;
        }
    }

    [TestClass]
    public class StudentRepository_GetTranscriptRestrictions : BasePersonSetup
    {
        private StudentRepository studentRepository;
        const string CacheName = "Ellucian.Web.Student.Data.Colleague.Repository.StudentRepository";
        private string studentId1;
        private string studentId2;
        private string studentId3;
        private string studentId4;

        [TestInitialize]
        public void Initialize()
        {

            // Initialize person setup and Mock framework
            PersonSetupInitialize();
            SetupData();
            studentRepository = BuildMockStudentRepository();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            studentRepository = null;
        }

        [TestMethod]
        public async Task GetTranscriptRestrictions_Empty_Success()
        {
            IEnumerable<TranscriptRestriction> result = await studentRepository.GetTranscriptRestrictionsAsync(studentId1);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetTranscriptRestrictions_One_Success()
        {
            IEnumerable<TranscriptRestriction> result = await studentRepository.GetTranscriptRestrictionsAsync(studentId2);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetTranscriptRestrictions_MissingStudent_Throws()
        {
            IEnumerable<TranscriptRestriction> result = await studentRepository.GetTranscriptRestrictionsAsync(studentId3);
        }

        [TestMethod]
        public async Task GetTranscriptRestrictions_Multi_Success()
        {
            IEnumerable<TranscriptRestriction> result = await studentRepository.GetTranscriptRestrictionsAsync(studentId4);
            Assert.AreEqual(2, result.Count());
        }


        private void SetupData()
        {
            studentId1 = "00000001";
            studentId2 = "00000002";
            studentId3 = "00000003";
            studentId4 = "00000004";
        }

        private StudentRepository BuildMockStudentRepository()
        {

            var mockManager = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

            // Set up the responses for getting a Transcript Restriction request
            var transcriptRestrictionsRequest1 = new GetTranscriptHoldsRequest();
            var transcriptRestrictionsResponse1 = new GetTranscriptHoldsResponse() { HoldGroup = new List<HoldGroup>() };
            var transcriptRestrictionsRequest2 = new GetTranscriptHoldsRequest();
            var transcriptRestrictionsResponse2 = new GetTranscriptHoldsResponse() { HoldGroup = new List<HoldGroup>() { new HoldGroup { FailRuleIds = "LIBR", FailRuleMsgs = "Library" } } };
            var transcriptRestrictionsRequest3 = new GetTranscriptHoldsRequest();
            var transcriptRestrictionsResponse3 = new GetTranscriptHoldsResponse() { HoldGroup = new List<HoldGroup>() { new HoldGroup { FailRuleIds = "fail", FailRuleMsgs = "failed", FailRuleTypes = "X" } } };
            var transcriptRestrictionsRequest4 = new GetTranscriptHoldsRequest();
            var transcriptRestrictionsResponse4 = new GetTranscriptHoldsResponse()
            {
                HoldGroup = new List<HoldGroup>() { new HoldGroup { FailRuleIds = "fail1", FailRuleMsgs = "failed1", FailRuleTypes = "T" },
                                                                                                                                 new HoldGroup { FailRuleIds = "fail2", FailRuleMsgs = "failed2", FailRuleTypes = "G" }}
            };


            mockManager.Setup(mgr => mgr.ExecuteAsync<GetTranscriptHoldsRequest, GetTranscriptHoldsResponse>(It.Is<GetTranscriptHoldsRequest>(r => r.APersonId == studentId1))).ReturnsAsync(transcriptRestrictionsResponse1);
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetTranscriptHoldsRequest, GetTranscriptHoldsResponse>(It.Is<GetTranscriptHoldsRequest>(r => r.APersonId == studentId2))).ReturnsAsync(transcriptRestrictionsResponse2);
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetTranscriptHoldsRequest, GetTranscriptHoldsResponse>(It.Is<GetTranscriptHoldsRequest>(r => r.APersonId == studentId3))).ReturnsAsync(transcriptRestrictionsResponse3);
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetTranscriptHoldsRequest, GetTranscriptHoldsResponse>(It.Is<GetTranscriptHoldsRequest>(r => r.APersonId == studentId4))).ReturnsAsync(transcriptRestrictionsResponse4);

            StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return repository;
        }
    }


    [TestClass]
    public class StudentRepository_GetTranscriptRestrictions2 : BasePersonSetup
    {
        private StudentRepository studentRepository;
        const string CacheName = "Ellucian.Web.Student.Data.Colleague.Repository.StudentRepository";
        private string studentId1;
        private string studentId2;
        private string studentId3;
        private string studentId4;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize person setup and Mock framework
            PersonSetupInitialize();
            SetupData();
            studentRepository = BuildMockStudentRepository();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            studentRepository = null;
        }

        [TestMethod]
        public async Task GetTranscriptRestrictions2_Empty_Success()
        {
            IEnumerable<TranscriptRestriction> result = await studentRepository.GetTranscriptRestrictionsAsync(studentId1);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetTranscriptRestrictions2_One_Success()
        {
            IEnumerable<TranscriptRestriction> result = await studentRepository.GetTranscriptRestrictionsAsync(studentId2);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetTranscriptRestrictions2_MissingStudent_Throws()
        {
            IEnumerable<TranscriptRestriction> result = await studentRepository.GetTranscriptRestrictionsAsync(studentId3);
        }

        [TestMethod]
        public async Task GetTranscriptRestrictions2_Multi_Success()
        {
            IEnumerable<TranscriptRestriction> result = await studentRepository.GetTranscriptRestrictionsAsync(studentId4);
            Assert.AreEqual(2, result.Count());
        }


        private void SetupData()
        {
            studentId1 = "00000001";
            studentId2 = "00000002";
            studentId3 = "00000003";
            studentId4 = "00000004";
        }

        private StudentRepository BuildMockStudentRepository()
        {
            var mockManager = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

            // Set up the responses for getting a Transcript Restriction request
            var transcriptRestrictionsRequest1 = new GetTranscriptHoldsRequest();
            var transcriptRestrictionsResponse1 = new GetTranscriptHoldsResponse() { HoldGroup = new List<HoldGroup>() };
            var transcriptRestrictionsRequest2 = new GetTranscriptHoldsRequest();
            var transcriptRestrictionsResponse2 = new GetTranscriptHoldsResponse() { HoldGroup = new List<HoldGroup>() { new HoldGroup { FailRuleIds = "LIBR", FailRuleMsgs = "Library" } } };
            var transcriptRestrictionsRequest3 = new GetTranscriptHoldsRequest();
            var transcriptRestrictionsResponse3 = new GetTranscriptHoldsResponse() { HoldGroup = new List<HoldGroup>() { new HoldGroup { FailRuleIds = "fail", FailRuleMsgs = "failed", FailRuleTypes = "X" } } };
            var transcriptRestrictionsRequest4 = new GetTranscriptHoldsRequest();
            var transcriptRestrictionsResponse4 = new GetTranscriptHoldsResponse()
            {
                HoldGroup = new List<HoldGroup>() { new HoldGroup { FailRuleIds = "fail1", FailRuleMsgs = "failed1", FailRuleTypes = "T" },
                                                    new HoldGroup { FailRuleIds = "fail2", FailRuleMsgs = "failed2", FailRuleTypes = "G" }}
            };

            mockManager.Setup(mgr => mgr.ExecuteAsync<GetTranscriptHoldsRequest, GetTranscriptHoldsResponse>(It.Is<GetTranscriptHoldsRequest>(r => r.APersonId == studentId1))).ReturnsAsync(transcriptRestrictionsResponse1);
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetTranscriptHoldsRequest, GetTranscriptHoldsResponse>(It.Is<GetTranscriptHoldsRequest>(r => r.APersonId == studentId2))).ReturnsAsync(transcriptRestrictionsResponse2);
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetTranscriptHoldsRequest, GetTranscriptHoldsResponse>(It.Is<GetTranscriptHoldsRequest>(r => r.APersonId == studentId3))).ReturnsAsync(transcriptRestrictionsResponse3);
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetTranscriptHoldsRequest, GetTranscriptHoldsResponse>(It.Is<GetTranscriptHoldsRequest>(r => r.APersonId == studentId4))).ReturnsAsync(transcriptRestrictionsResponse4);

            StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return repository;
        }
    }

    [TestClass]
    public class StudentRepository_Search_Tests : BasePersonSetup
    {
        private TestStudentRepository testStudentRepository;
        private StudentRepository studentRepository;
        const string CacheName = "Ellucian.Web.Student.Data.Colleague.Repository.StudentRepository";
        private string knownStudentId1;
        private string knownStudentId2;
        private string knownStudentId3;
        private IEnumerable<string> ids;
        private string unknownStudentId;
        private string[] acadPrograms;
        private string quote;

        private Collection<FinAid> finAidRecords;

        private Collection<DataContracts.Students> students;
        private Collection<Base.DataContracts.PersonSt> personStRecords;
        private Collection<DataContracts.StudentAdvisement> studentAdvisements;
        private Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;

        private Collection<DataContracts.Students> students1;
        private Collection<Base.DataContracts.PersonSt> personStRecords1;
        private Collection<DataContracts.StudentAdvisement> studentAdvisements1;
        private Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people1;

        private Collection<DataContracts.Students> students2;
        private Collection<Base.DataContracts.PersonSt> personStRecords2;
        private Collection<DataContracts.StudentAdvisement> studentAdvisements2;
        private Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people2;

        [TestInitialize]
        public void Initialize()
        {
            testStudentRepository = new TestStudentRepository();
            finAidRecords = BuildFinAidRecords(testStudentRepository.faStudentData);

            students = new Collection<DataContracts.Students>();
            personStRecords = new Collection<Base.DataContracts.PersonSt>();
            studentAdvisements = new Collection<DataContracts.StudentAdvisement>();
            people = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>();

            students1 = new Collection<DataContracts.Students>();
            personStRecords1 = new Collection<Base.DataContracts.PersonSt>();
            studentAdvisements1 = new Collection<DataContracts.StudentAdvisement>();
            people1 = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>();

            students2 = new Collection<DataContracts.Students>();
            personStRecords2 = new Collection<Base.DataContracts.PersonSt>();
            studentAdvisements2 = new Collection<DataContracts.StudentAdvisement>();
            people2 = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>();


            quote = '"'.ToString();

            // Initialize person setup and Mock framework
            PersonSetupInitialize();

            SetupData();

            studentRepository = BuildMockStudentRepository();
        }



        [TestCleanup]
        public void TestCleanup()
        {
            studentRepository = null;
        }


        [TestMethod]
        public async Task Search_Student_Single()
        {
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.SearchAsync("Onename", null, null, null, null, null);
            Assert.AreEqual(1, students.Count());
        }

        [TestMethod]
        public async Task Search_Student_WithBirthDate()
        {
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.SearchAsync("Onename", null, new DateTime(1995, 11, 24), null, null, null);
            Assert.AreEqual(1, students.Count());
        }

        [TestMethod]
        public async Task Search_Student_Multiple()
        {
            IEnumerable<Domain.Student.Entities.Student> students = await studentRepository.SearchAsync("Twonames", null, null, null, null, null);
            Assert.AreEqual(2, students.Count());
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
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId1, StuAcadPrograms = null });
            students1.Add(new DataContracts.Students() { Recordkey = knownStudentId1, StuAcadPrograms = null });
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "11", StadStudent = "0000001", StadFaculty = "0000036", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = null });
            studentAdvisements1.Add(new DataContracts.StudentAdvisement() { Recordkey = "11", StadStudent = "0000001", StadFaculty = "0000036", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = null });
            personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000001", PstStudentAcadCred = new List<string>() { "19000" }, PstAdvisement = new List<string>() { "11" }, PstRestrictions = new List<string>() { "R001", "R002" } });
            personStRecords1.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000001", PstStudentAcadCred = new List<string>() { "19000" }, PstAdvisement = new List<string>() { "11" }, PstRestrictions = new List<string>() { "R001", "R002" } });
            Ellucian.Colleague.Data.Base.DataContracts.Person person1 = new Ellucian.Colleague.Data.Base.DataContracts.Person() { Recordkey = "0000001", LastName = "Smith" };
            person1.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
            PersonPeopleEmail ppe1 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "dsmith@yahoo.com", PersonEmailTypesAssocMember = "HOME", PersonPreferredEmailAssocMember = "" };
            person1.PeopleEmailEntityAssociation.Add(ppe1);
            PersonPeopleEmail ppe2 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "junk@yahoo.com", PersonEmailTypesAssocMember = "BUS", PersonPreferredEmailAssocMember = "" };
            person1.PeopleEmailEntityAssociation.Add(ppe2);
            person1.BirthDate = new DateTime(1995, 11, 24);
            people.Add(person1);
            people1.Add(person1);
            people2.Add(person1);
            /* 
             * student 0000002
             * > one program
             * > two degree plans
             * > two acad creds
             * > one current advisor (but also a past advisor)
             * > two emails - second marked as preferred
             */
            knownStudentId2 = "0000002";
            personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000002", PstStudentAcadCred = new List<string>() { "" }, PstAdvisement = new List<string>() { "21", "22" }, PstRestrictions = new List<string>() });
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId2, StuAcadPrograms = new List<string> { acadPrograms[0] } });
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "21", StadStudent = "0000002", StadFaculty = "0000036", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = null });
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "22", StadStudent = "0000002", StadFaculty = "0000045", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = new DateTime(2012, 8, 1) });
            Ellucian.Colleague.Data.Base.DataContracts.Person person2 = new Ellucian.Colleague.Data.Base.DataContracts.Person() { Recordkey = "0000002", LastName = "Jones" };
            person2.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
            PersonPeopleEmail ppe3 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "junk@yahoo.com", PersonEmailTypesAssocMember = "HOME", PersonPreferredEmailAssocMember = "" };
            person2.PeopleEmailEntityAssociation.Add(ppe3);
            PersonPeopleEmail ppe4 = new PersonPeopleEmail() { PersonEmailAddressesAssocMember = "djones@yahoo.com", PersonEmailTypesAssocMember = "BUS", PersonPreferredEmailAssocMember = "Y" };
            person2.PeopleEmailEntityAssociation.Add(ppe4);
            people.Add(person2);
            people2.Add(person2);
            /* 
             * student 0000003
             * > two programs
             * > no degree plan
             * > three acad creds
             * > no email addresses on file
             */
            knownStudentId3 = "0000003";
            students.Add(new DataContracts.Students() { Recordkey = knownStudentId3, StuAcadPrograms = new List<string> { acadPrograms[0], acadPrograms[1] } });
            personStRecords.Add(new Base.DataContracts.PersonSt() { Recordkey = "0000003", PstStudentAcadCred = new List<string>() { "19003", "19004", "19005" }, PstAdvisement = new List<string>(), PstRestrictions = new List<string>() });
            Ellucian.Colleague.Data.Base.DataContracts.Person person3 = new Ellucian.Colleague.Data.Base.DataContracts.Person() { Recordkey = "0000003", LastName = "Jones" };
            person3.PeopleEmailEntityAssociation = new List<PersonPeopleEmail>();
            people.Add(person3);

            ids = new List<string>() { knownStudentId1, knownStudentId2, knownStudentId3 };
        }

        private Collection<FinAid> BuildFinAidRecords(List<TestStudentRepository.FaStudent> faStudentData)
        {
            var finAidCollection = new Collection<FinAid>();
            foreach (var faStudentRecord in faStudentData)
            {
                var finAidDataContract = new FinAid();
                finAidDataContract.Recordkey = faStudentRecord.studentId;
                finAidDataContract.FaCounselorsEntityAssociation = new List<FinAidFaCounselors>();
                foreach (var counselorItem in faStudentRecord.faCounselors)
                {
                    var finAidFaCounselorsDataContract = new FinAidFaCounselors();
                    finAidFaCounselorsDataContract.FaCounselorAssocMember = counselorItem.counselorId;
                    finAidFaCounselorsDataContract.FaCounselorStartDateAssocMember = counselorItem.startDate;
                    finAidFaCounselorsDataContract.FaCounselorEndDateAssocMember = counselorItem.endDate;

                    finAidDataContract.FaCounselorsEntityAssociation.Add(finAidFaCounselorsDataContract);
                }
                finAidDataContract.FaCounselor = finAidDataContract.FaCounselorsEntityAssociation.Select(f => f.FaCounselorAssocMember).ToList();
                finAidDataContract.FaCounselorStartDate = finAidDataContract.FaCounselorsEntityAssociation.Select(f => f.FaCounselorStartDateAssocMember).ToList();
                finAidDataContract.FaCounselorEndDate = finAidDataContract.FaCounselorsEntityAssociation.Select(f => f.FaCounselorEndDateAssocMember).ToList();

                finAidCollection.Add(finAidDataContract);
            }
            return finAidCollection;
        }

        private StudentRepository BuildMockStudentRepository()
        {



            // mock transaction factory build query string response
            var mockManager = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

            GetPersonLookupStringResponse strResp = new GetPersonLookupStringResponse() { IndexString = ";PERSON.PARTIAL.NAME.INDEX LSTNM_FI " };
            GetPersonLookupStringResponse strResp2 = new GetPersonLookupStringResponse() { IndexString = ";PERSON.PARTIAL.NAME.INDEX LSTNM_FI2 " };
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.Is<GetPersonLookupStringRequest>(r => r.SearchString.Contains("Onename")))).ReturnsAsync(strResp);
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.Is<GetPersonLookupStringRequest>(r => r.SearchString.Contains("Twonames")))).ReturnsAsync(strResp2);

            string[] idResp = { "0000001" };
            string[] emptyResp = { };

            // mock data accessor STUDENTS response - valid ids 0000001, 0000002, 0000003
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", "WITH PERSON.PARTIAL.NAME.INDEX EQ \"LSTNM_FI\"")).ReturnsAsync(idResp);

            string[] idResp2 = { "0000001", "0000002" };
            IEnumerable<string> idResp2List = idResp2.ToList();
            // mock data accessor STUDENTS response - valid ids 0000001, 0000002, 0000003
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", "WITH PERSON.PARTIAL.NAME.INDEX EQ \"LSTNM_FI2\"")).ReturnsAsync(idResp2);


            // mock data accessor STUDENTS response - valid ids 0000001, 0000002, 0000003

            Students stuRec1 = new Students();
            stuRec1.Recordkey = "0000001";
            stuRec1.StuAcadLevels = new List<string>();
            stuRec1.StuAcadPrograms = new List<string>();


            Students stuRec2 = new Students();
            stuRec2.Recordkey = "0000002";
            stuRec2.StuAcadLevels = new List<string>();
            stuRec2.StuAcadPrograms = new List<string>();

            students1.Add(stuRec1);
            students2.Add(stuRec1);
            students2.Add(stuRec2);

            //string[] idResp3 = { "0000002" };
            //Collection<Students> students3 = new Collection<Students> { };
            //students3.Add(stuRec2);
            dataReaderMock.Setup<Task<Collection<Students>>>(a => a.BulkReadRecordAsync<Students>(idResp, true)).ReturnsAsync(students1);
            //dataReaderMock.Setup<Task<Collection<Students>>>(a => a.BulkReadRecordAsync<Students>(idResp3.ToArray(), true)).ReturnsAsync(students3);
            //dataReaderMock.Setup<Task<Collection<Students>>(a => a.BulkReadRecordAsync<Students>(It.IsAny<string[]>(), true)).ReturnsAsync(students);
            dataReaderMock.Setup<Task<Collection<Students>>>(a => a.BulkReadRecordAsync<Students>(idResp2List.ToArray(), true)).ReturnsAsync(students2);
            dataReaderMock.Setup<Task<Collection<Students>>>(a => a.BulkReadRecordAsync<Students>(idResp2, true)).ReturnsAsync(students2);


            // mock data accessor PERSON response - valid ids 0000001, 0000002, 0000003
            dataReaderMock.Setup<Task<Collection<Person>>>(a => a.BulkReadRecordAsync<Person>("PERSON", idResp, true)).ReturnsAsync(people1);
            dataReaderMock.Setup<Task<Collection<Person>>>(a => a.BulkReadRecordAsync<Person>("PERSON", idResp2, true)).ReturnsAsync(people2);
            dataReaderMock.Setup<Task<Collection<Person>>>(a => a.BulkReadRecordAsync<Person>("PERSON", idResp2List.ToArray(), true)).ReturnsAsync(people2);
            //dataReaderMock.Setup<Task<Collection<Person>>(a => a.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(people);

            // mock data accessor STUDENT.PROGRAM.STATUSES
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
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

            // mock data accessor STUDENT.PROGRAMS
            Collection<StudentPrograms> studentPrograms = new Collection<StudentPrograms>() {new DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId3 + "*" + acadPrograms[0],
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprCatalog = "2012"
                    },
                    new DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId3 + "*" + acadPrograms[1],
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprCatalog = "2011"
                    },
                    new DataContracts.StudentPrograms
                    {   // ended student program
                        Recordkey = knownStudentId3 + "*" + "BA.UNDC",
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprEndDate = new List<DateTime?>() {DateTime.Today.AddDays(-60)},
                        StprCatalog = "2010"
                    }, new DataContracts.StudentPrograms
                    {
                        Recordkey = knownStudentId2 + "*" + acadPrograms[0],
                        StprStartDate = new List<DateTime?>() { new DateTime()},
                        StprStatus = new List<string>() { "A" },
                        StprCatalog = "2012"
                    } };
            dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(a => a.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(studentPrograms);

            // mock data accessor PERSON.ST response - valid ids 0000001, 0000002, 0000003
            dataReaderMock.Setup<Task<Collection<PersonSt>>>(a => a.BulkReadRecordAsync<PersonSt>(It.IsAny<string[]>(), true)).ReturnsAsync(personStRecords1);

            // mock data accessor DEGREE_PLANS response  - for student 0000001
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId1 + "'")).ReturnsAsync(new string[] { "1" });

            // mock data accessor DEGREE_PLANS response  - for student 0000002
            var multipleDegreePlanKeys = new string[] { "2", "3" };
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId2 + "'")).ReturnsAsync(multipleDegreePlanKeys);

            // mock data accessor DEGREE_PLANS response  - for student 0000003
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId3 + "'")).ReturnsAsync(new string[] { });

            // mock data accessor STUDENT.ADVISEMENT for student 0000002
            string[] student2AdvismentIds = new List<string>() { "11", "21", "22" }.ToArray();
            dataReaderMock.Setup<Task<Collection<StudentAdvisement>>>(d => d.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", student2AdvismentIds, true)).ReturnsAsync(studentAdvisements);


            // mock data accessor STUDENT.ADVISEMENT for student 0000002
            string[] student1AdvismentIds = new List<string>() { "11" }.ToArray();
            dataReaderMock.Setup<Task<Collection<StudentAdvisement>>>(d => d.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", student1AdvismentIds, true)).ReturnsAsync(studentAdvisements);

            // mock data accessor for PERSON search
            var multiplePersonKeys = new string[] { "0000001", "0000002" };
            var onePersonKey = new string[] { "0000001" };
            var noKeys = new string[] { };


            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", "WITH  LAST.NAME EQ \"Twonames\"")).ReturnsAsync(multiplePersonKeys);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", "WITH  LAST.NAME EQ \"Onename\"")).ReturnsAsync(onePersonKey);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", "WITH  ID EQ '0000001'")).ReturnsAsync(onePersonKey);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", "WITH  ID EQ '0000001' '0000002'")).ReturnsAsync(multiplePersonKeys);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", onePersonKey, "")).ReturnsAsync(onePersonKey);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", multiplePersonKeys, "")).ReturnsAsync(multiplePersonKeys);



            string[] enumKeys = { "0000001", "0000002" };
            dataReaderMock.Setup<Task<Collection<Students>>>(a => a.BulkReadRecordAsync<Students>(enumKeys, true)).ReturnsAsync(students2);


            // Test birth date fix.  ICONV date bad.  Outside date format good.
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", "WITH  ID EQ '0000001' AND BIRTH.DATE EQ '11/24/1995'")).ReturnsAsync(onePersonKey);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", "WITH  ID EQ '0000001' AND BIRTH.DATE EQ '24/11/1995'")).ReturnsAsync(onePersonKey);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", "WITH  ID EQ '0000001' AND BIRTH.DATE EQ '1995/11/24'")).ReturnsAsync(onePersonKey);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", "WITH  ID EQ '0000001' AND BIRTH.DATE EQ '81815'")).ReturnsAsync(noKeys);

            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", onePersonKey, "WITH BIRTH.DATE EQ '11/24/1995'")).ReturnsAsync(onePersonKey);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", onePersonKey, "WITH BIRTH.DATE EQ '24/11/1995'")).ReturnsAsync(onePersonKey);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", onePersonKey, "WITH BIRTH.DATE EQ '1995/11/24'")).ReturnsAsync(onePersonKey);
            dataReaderMock.Setup<Task<string[]>>(a => a.SelectAsync("PERSON", onePersonKey, "WITH BIRTH.DATE EQ '81815'")).ReturnsAsync(noKeys);





            //mock data accessor for FIN.AID records
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<FinAid>(It.IsAny<string[]>(), true)).ReturnsAsync(finAidRecords);


            // mock data accessor for INTERNATIONAL.PARAMETERS
            Data.Base.DataContracts.IntlParams intlParams = new Base.DataContracts.IntlParams();
            intlParams.HostDateDelimiter = "/";
            intlParams.HostShortDateFormat = "MDY";
            dataReaderMock.Setup(d => d.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);

            //StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return repository;
        }


    }

    [TestClass]
    public class StudentRepository_GetTranscript : BasePersonSetup
    {
        private StudentRepository studentRepository;
        private string studentId;
        private string transcriptGrouping;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize person setup and Mock framework
            PersonSetupInitialize();

            studentId = "0005454";
            transcriptGrouping = "";
            studentRepository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            studentRepository = null;
        }

        [TestMethod]
        public async Task GetTranscript_ReturnsText()
        {
            char _VM = Convert.ToChar(DynamicArray.VM);
            var response1 = new CreatePlainTextTranscriptResponse() { TranscriptText = "Transcript Text Line1" + _VM + "Transcript Text Line2" };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreatePlainTextTranscriptRequest, CreatePlainTextTranscriptResponse>(It.IsAny<CreatePlainTextTranscriptRequest>()))
                .ReturnsAsync(response1);
            var result = await studentRepository.GetTranscriptAsync(studentId, transcriptGrouping);
            Assert.AreEqual("Transcript Text Line1\nTranscript Text Line2", result);

        }

        [TestMethod]
        public async Task GetTranscript_ReturnsEmptyText()
        {
            var response1 = new CreatePlainTextTranscriptResponse() { TranscriptText = "" };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreatePlainTextTranscriptRequest, CreatePlainTextTranscriptResponse>(It.IsAny<CreatePlainTextTranscriptRequest>()))
                .ReturnsAsync(response1);
            var result = await studentRepository.GetTranscriptAsync(studentId, transcriptGrouping);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetTranscript_ThrowsColleagueTransactionException()
        {
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreatePlainTextTranscriptRequest, CreatePlainTextTranscriptResponse>(It.IsAny<CreatePlainTextTranscriptRequest>()))
                .Throws(new ColleagueTransactionException("Message"));
            var result = await studentRepository.GetTranscriptAsync(studentId, transcriptGrouping);
        }
    }

    [TestClass]
    public class StudentRepository_GetStudentAccess : BasePersonSetup
    {
        private TestStudentRepository testStudentRepository;
        private StudentRepository studentRepository;
        const string CacheName = "Ellucian.Web.Student.Data.Colleague.Repository.StudentRepository";
        private string studentId1;
        private string studentId2;
        private IEnumerable<string> ids;

        private Collection<DataContracts.StudentAdvisement> studentAdvisements;

        [TestInitialize]
        public void Initialize()
        {
            testStudentRepository = new TestStudentRepository();
            studentAdvisements = new Collection<DataContracts.StudentAdvisement>();

            // Initialize person setup and Mock framework
            PersonSetupInitialize();

            SetupData();

            studentRepository = BuildMockStudentRepository();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            studentRepository = null;
        }

        private void SetupData()
        {
            /* 
             * student1 
             * > one current advisor, one past advisor
             */
            studentId1 = "0000001";
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "21", StadStudent = "0000001", StadFaculty = "0000036", StadStartDate = new DateTime(2013, 7, 1), StadEndDate = null });
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "22", StadStudent = "0000001", StadFaculty = "0000045", StadStartDate = new DateTime(2012, 7, 1), StadEndDate = new DateTime(2012, 8, 1) });

            /* 
             * student2
             * > two current advisors
             */
            studentId2 = "0000002";
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "23", StadStudent = "0000002", StadFaculty = "0000045", StadStartDate = null, StadEndDate = null });
            studentAdvisements.Add(new DataContracts.StudentAdvisement() { Recordkey = "24", StadStudent = "0000002", StadFaculty = "0000036", StadStartDate = new DateTime(2014, 5, 1), StadEndDate = new DateTime(2050, 12, 30) });

            ids = new List<string>() { studentId1, studentId2 };
        }

        private StudentRepository BuildMockStudentRepository()
        {

            dataReaderMock.Setup<Task<Collection<StudentAdvisement>>>(d => d.BulkReadRecordAsync<StudentAdvisement>(It.IsAny<string[]>(), true)).ReturnsAsync(studentAdvisements);

            StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return repository;
        }

        [TestMethod]
        public async Task GetStudentAccess_ReturnsCurrentStudentAdvisements()
        {
            var studentAccessEntities = await studentRepository.GetStudentAccessAsync(ids);
            Assert.AreEqual(2, studentAccessEntities.Count());
            // Student1 advisements
            var studentAccess = studentAccessEntities.Where(a => a.Id == studentId1).First();
            // Student 1 has one active advisement
            Assert.AreEqual(1, studentAccess.Advisements.Count());
            // Student2 advisements
            studentAccess = studentAccessEntities.Where(a => a.Id == studentId2).First();
            // student 2 has two active advisements
            Assert.AreEqual(2, studentAccess.Advisements.Count());
            // Check that the second advisement for student 2 has come through with all fields initialized properly
            Assert.AreEqual(studentAdvisements.ElementAt(3).StadFaculty, studentAccess.Advisements.ElementAt(1).AdvisorId);
            Assert.AreEqual(studentAdvisements.ElementAt(3).StadStartDate, studentAccess.Advisements.ElementAt(1).StartDate);
            Assert.AreEqual(studentAdvisements.ElementAt(3).StadEndDate, studentAccess.Advisements.ElementAt(1).EndDate);
            Assert.AreEqual(studentAdvisements.ElementAt(3).StadType, studentAccess.Advisements.ElementAt(1).AdvisorType);

        }
        [TestMethod]
        public async Task SearchIdsAsync()
        {
            List<string> studentIds = new List<string>() { "student1", "student2" };
            dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("STUDENTS", "WITH STU.TERMS = '2015/FA'")).Returns(Task.FromResult(studentIds.ToArray()));
            var result = await studentRepository.SearchIdsAsync("2015/FA");
            Assert.AreEqual(studentIds.Count(), result.Count());
        }
    }

    [TestClass]
    public class StudentRepository_GetResidencyStatuses : BaseRepositorySetup
    {
        StudentRepository studentRepo;

        public void MainInitialize()
        {
            base.MockInitialize();
            studentRepo = new StudentRepository(cacheProvider, transFactory, logger, apiSettings);
        }

        [TestClass]
        public class ResidentTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<ResidencyStatus> allResidentTypes;
            string valcodeName;
            ApiSettings apiSettings;

            StudentRepository studentRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allResidentTypes = new TestStudentRepository().GetResidencyStatusesAsync(false).Result;

                studentRepo = BuildValidReferenceDataRepository();
                valcodeName = studentRepo.BuildFullCacheKey("AllResidencyStatuses");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allResidentTypes = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentRepository_GetResidencyStatusesAsync_False()
            {
                var results = await studentRepo.GetResidencyStatusesAsync(false);
                Assert.AreEqual(allResidentTypes.Count(), results.Count());

                foreach (var residentType in allResidentTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == residentType.Guid);

                    Assert.AreEqual(residentType.Code, result.Code);
                    Assert.AreEqual(residentType.Description, result.Description);
                    Assert.AreEqual(residentType.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentRepository_GetResidencyStatusesAsync_True()
            {
                var results = await studentRepo.GetResidencyStatusesAsync(true);
                Assert.AreEqual(allResidentTypes.Count(), results.Count());

                foreach (var residentType in allResidentTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == residentType.Guid);

                    Assert.AreEqual(residentType.Code, result.Code);
                    Assert.AreEqual(residentType.Description, result.Description);
                    Assert.AreEqual(residentType.Guid, result.Guid);
                }

            }

            private StudentRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.ResidencyStatuses>();
                foreach (var item in allResidentTypes)
                {
                    DataContracts.ResidencyStatuses record = new DataContracts.ResidencyStatuses();
                    record.RecordGuid = item.Guid;
                    record.ResDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.ResidencyStatuses>("RESIDENCY.STATUSES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allResidentTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "RESIDENCY.STATUSES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                studentRepo = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return studentRepo;
            }
        }
    }

    [TestClass]
    public class StudentRepository_CheckRegistrationEligibilityEthos : BaseRepositorySetup
    {
        #region DECLARATIONS

        private StudentRepository studentRepository;
        private Transactions.CheckRegEligibilityEthosResponse response;

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            studentRepository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            InitializeTestData();
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }

        private void InitializeTestData()
        {
            response = new Transactions.CheckRegEligibilityEthosResponse()
            {
                Messages = new List<string>() { "Message" },
                Eligible = true,
                HasOverride = false,
                EthosTerms = new List<EthosTerms>()
                {
                    new EthosTerms()
                    {
                        TermCode = "1",
                        TermAddCheckDate = DateTime.Today,
                        TermPriorityOverride = true,
                    },
                    new EthosTerms()
                    {
                        TermCode = "2",
                        TermAddCheckDate = DateTime.Today,
                        TermPriorityOverride = false,
                        TermAddAllowed = true
                    },
                    new EthosTerms()
                    {
                        TermCode = "3",
                        TermAddCheckDate = DateTime.Today.AddDays(10),
                        TermPriorityOverride = false,
                        TermAddAllowed = false
                    },
                    new EthosTerms()
                    {
                        TermCode = "4",
                        TermAddCheckDate = DateTime.Today,
                        TermPriorityOverride = false,
                        TermAddAllowed = false
                    },
                }
            };
        }

        #endregion

        [TestMethod]
        public async Task StudentRepository_CheckRegistrationEligibilityEthosAsync()
        {
            transManagerMock.Setup(r => r.ExecuteAsync<CheckRegEligibilityEthosRequest, CheckRegEligibilityEthosResponse>(It.IsAny<CheckRegEligibilityEthosRequest>())).ReturnsAsync(response);

            var result = await studentRepository.CheckRegistrationEligibilityEthosAsync("1", new List<string>() { "Term" });

            Assert.IsNotNull(result);
            Assert.AreEqual(response.EthosTerms.FirstOrDefault().TermCode, result.Terms.FirstOrDefault().TermCode);
            Assert.AreEqual(response.Eligible, result.IsEligible);
        }

        [TestMethod]
        public async Task StudentRepository_CheckRegistrationEligibilityEthosAsync_Exception()
        {
            response.EthosTerms.FirstOrDefault().TermCode = null;
            response.EthosTerms.FirstOrDefault().TermPriorityOverride = false;
            response.Eligible = false;

            transManagerMock.Setup(r => r.ExecuteAsync<CheckRegEligibilityEthosRequest, CheckRegEligibilityEthosResponse>(It.IsAny<CheckRegEligibilityEthosRequest>())).ReturnsAsync(response);

            var result = await studentRepository.CheckRegistrationEligibilityEthosAsync("1", new List<string>() { "Term" });

            Assert.IsNotNull(result);
            Assert.AreEqual(response.EthosTerms.Count - 1, result.Terms.Count);
            Assert.AreEqual(response.Eligible, result.IsEligible);
        }
    }

    [TestClass]
    public class StudentRepository_RegisterAsync : BaseRepositorySetup
    {

        private StudentRepository studentRepository;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            studentRepository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }
        
        [TestMethod]
        // Verify that the respository RegisterAsync succsesfully passes data from the entity through to the CTX
        // Also test that error messages returned from the CTX are returned in the response domain entity.
        public async Task StudentRepository_RegisterAsyncSuccess()            
        {
            // A RegistrationRequest entity that will be passed to RegisterAsync
            string studentID = "astudent";
            List<Domain.Student.Entities.SectionRegistration> sectionRegs = new List<Domain.Student.Entities.SectionRegistration>();
            Domain.Student.Entities.SectionRegistration sectReg1 = new SectionRegistration();
            sectReg1.SectionId = "section1";
            sectReg1.Action = RegistrationAction.Drop;
            sectReg1.Credits = 3.5m;
            sectReg1.DropReasonCode = "Reason1";
            sectionRegs.Add(sectReg1);
            Domain.Student.Entities.SectionRegistration sectReg2 = new SectionRegistration();
            sectReg2.SectionId = "section2";
            sectReg2.Action = RegistrationAction.Add;
            sectReg2.Credits = 2.5m;
            sectReg2.DropReasonCode = null;
            sectionRegs.Add(sectReg2);
            Domain.Student.Entities.SectionRegistration sectReg3 = new SectionRegistration();
            sectReg3.SectionId = "section3";
            sectReg3.Action = RegistrationAction.Add;
            sectReg3.Credits = 3m;
            sectReg3.DropReasonCode = null;
            sectionRegs.Add(sectReg3);

            Domain.Student.Entities.RegistrationRequest regRequest = new RegistrationRequest(studentID, sectionRegs);

            // Mock the transction invoker for the registration CTX that receives the data that should be passed based on this domain RegistrationRequest entity
            List<Ellucian.Colleague.Data.Student.Transactions.Messages> responseMsgs = new List<Messages>();
            Ellucian.Colleague.Data.Student.Transactions.Messages msg1 = new Messages();
            msg1.Message = "Message1";
            msg1.MessageSection = "MsgSec1";
            Ellucian.Colleague.Data.Student.Transactions.Messages msg2 = new Messages();
            msg2.Message = "Message2";
            msg2.MessageSection = "MsgSec2";
            responseMsgs.Add(msg1);
            responseMsgs.Add(msg2);

            RegisterForSectionsResponse regForSecResponse = new RegisterForSectionsResponse();
            regForSecResponse.IpcRegId = "SuccessfulReg";
            regForSecResponse.Messages = responseMsgs;
            regForSecResponse.RegisteredSectionIds = new List<string>() { sectReg3.SectionId };

            transManagerMock.Setup(tm => tm.ExecuteAsync<RegisterForSectionsRequest, RegisterForSectionsResponse>(
                It.Is<RegisterForSectionsRequest>(rr => rr.StudentId == studentID &&
                rr.Sections.Count == sectionRegs.Count &&
                rr.Sections[0].SectionIds == sectionRegs[0].SectionId &&
                rr.Sections[0].SectionAction == sectionRegs[0].Action.ToString() &&
                rr.Sections[0].SectionCredits == sectionRegs[0].Credits &&
                rr.Sections[0].SectionDropReasonCode == sectionRegs[0].DropReasonCode &&
                rr.Sections[1].SectionIds == sectionRegs[1].SectionId &&
                rr.Sections[1].SectionAction == sectionRegs[1].Action.ToString() &&
                rr.Sections[1].SectionCredits == sectionRegs[1].Credits &&
                rr.Sections[1].SectionDropReasonCode == sectionRegs[1].DropReasonCode))).ReturnsAsync(regForSecResponse);

            RegistrationResponse regResponse = await studentRepository.RegisterAsync(regRequest);

            Assert.IsTrue(regResponse.PaymentControlId == regForSecResponse.IpcRegId);
            Assert.IsTrue(regResponse.Messages.Count == responseMsgs.Count);
            Assert.IsTrue(regResponse.Messages[0].SectionId == responseMsgs[0].MessageSection);
            Assert.IsTrue(regResponse.Messages[0].Message == responseMsgs[0].Message);
            Assert.IsTrue(regResponse.Messages[1].SectionId == responseMsgs[1].MessageSection);
            Assert.IsTrue(regResponse.Messages[1].Message == responseMsgs[1].Message);
            CollectionAssert.AreEqual(regForSecResponse.RegisteredSectionIds, regResponse.RegisteredSectionIds);
        }

        [TestMethod]
        // Verify that the respository RegisterAsync succsesfully passes data from the entity through to the CTX
        // Also test that error messages returned from the CTX are returned in the response domain entity.
        public async Task StudentRepository_RegisterAsyncSuccess_Response_contains_null_RegisteredSectionIds()
        {
            // A RegistrationRequest entity that will be passed to RegisterAsync
            string studentID = "astudent";
            List<Domain.Student.Entities.SectionRegistration> sectionRegs = new List<Domain.Student.Entities.SectionRegistration>();
            Domain.Student.Entities.SectionRegistration sectReg1 = new SectionRegistration();
            sectReg1.SectionId = "section1";
            sectReg1.Action = RegistrationAction.Drop;
            sectReg1.Credits = 3.5m;
            sectReg1.DropReasonCode = "Reason1";
            sectionRegs.Add(sectReg1);
            Domain.Student.Entities.SectionRegistration sectReg2 = new SectionRegistration();
            sectReg2.SectionId = "section2";
            sectReg2.Action = RegistrationAction.Add;
            sectReg2.Credits = 2.5m;
            sectReg2.DropReasonCode = null;
            sectionRegs.Add(sectReg2);

            Domain.Student.Entities.RegistrationRequest regRequest = new RegistrationRequest(studentID, sectionRegs);

            // Mock the transction invoker for the registration CTX that receives the data that should be passed based on this domain RegistrationRequest entity
            List<Ellucian.Colleague.Data.Student.Transactions.Messages> responseMsgs = new List<Messages>();
            Ellucian.Colleague.Data.Student.Transactions.Messages msg1 = new Messages();
            msg1.Message = "Message1";
            msg1.MessageSection = "MsgSec1";
            Ellucian.Colleague.Data.Student.Transactions.Messages msg2 = new Messages();
            msg2.Message = "Message2";
            msg2.MessageSection = "MsgSec2";
            responseMsgs.Add(msg1);
            responseMsgs.Add(msg2);

            RegisterForSectionsResponse regForSecResponse = new RegisterForSectionsResponse();
            regForSecResponse.IpcRegId = "SuccessfulReg";
            regForSecResponse.Messages = responseMsgs;
            regForSecResponse.RegisteredSectionIds = null;

            transManagerMock.Setup(tm => tm.ExecuteAsync<RegisterForSectionsRequest, RegisterForSectionsResponse>(
                It.Is<RegisterForSectionsRequest>(rr => rr.StudentId == studentID &&
                rr.Sections.Count == sectionRegs.Count &&
                rr.Sections[0].SectionIds == sectionRegs[0].SectionId &&
                rr.Sections[0].SectionAction == sectionRegs[0].Action.ToString() &&
                rr.Sections[0].SectionCredits == sectionRegs[0].Credits &&
                rr.Sections[0].SectionDropReasonCode == sectionRegs[0].DropReasonCode &&
                rr.Sections[1].SectionIds == sectionRegs[1].SectionId &&
                rr.Sections[1].SectionAction == sectionRegs[1].Action.ToString() &&
                rr.Sections[1].SectionCredits == sectionRegs[1].Credits &&
                rr.Sections[1].SectionDropReasonCode == sectionRegs[1].DropReasonCode))).ReturnsAsync(regForSecResponse);

            RegistrationResponse regResponse = await studentRepository.RegisterAsync(regRequest);

            Assert.IsTrue(regResponse.PaymentControlId == regForSecResponse.IpcRegId);
            Assert.IsTrue(regResponse.Messages.Count == responseMsgs.Count);
            Assert.IsTrue(regResponse.Messages[0].SectionId == responseMsgs[0].MessageSection);
            Assert.IsTrue(regResponse.Messages[0].Message == responseMsgs[0].Message);
            Assert.IsTrue(regResponse.Messages[1].SectionId == responseMsgs[1].MessageSection);
            Assert.IsTrue(regResponse.Messages[1].Message == responseMsgs[1].Message);
            CollectionAssert.AreEqual(new List<string>(), regResponse.RegisteredSectionIds);
        }

        [TestMethod]
        // Verify that the respository RegisterAsync succsesfully passes data from the entity through to the CTX
        // Also test that error messages returned from the CTX are returned in the response domain entity.
        public async Task StudentRepository_RegisterAsyncSuccess_Response_RegisteredSectionIds_contains_null_empty_and_duplicate_IDs()
        {
            // A RegistrationRequest entity that will be passed to RegisterAsync
            string studentID = "astudent";
            List<Domain.Student.Entities.SectionRegistration> sectionRegs = new List<Domain.Student.Entities.SectionRegistration>();
            Domain.Student.Entities.SectionRegistration sectReg1 = new SectionRegistration();
            sectReg1.SectionId = "section1";
            sectReg1.Action = RegistrationAction.Drop;
            sectReg1.Credits = 3.5m;
            sectReg1.DropReasonCode = "Reason1";
            sectionRegs.Add(sectReg1);
            Domain.Student.Entities.SectionRegistration sectReg2 = new SectionRegistration();
            sectReg2.SectionId = "section2";
            sectReg2.Action = RegistrationAction.Add;
            sectReg2.Credits = 2.5m;
            sectReg2.DropReasonCode = null;
            sectionRegs.Add(sectReg2);
            Domain.Student.Entities.SectionRegistration sectReg3 = new SectionRegistration();
            sectReg3.SectionId = "section3";
            sectReg3.Action = RegistrationAction.Add;
            sectReg3.Credits = 3m;
            sectReg3.DropReasonCode = null;
            sectionRegs.Add(sectReg3);

            Domain.Student.Entities.RegistrationRequest regRequest = new RegistrationRequest(studentID, sectionRegs);

            // Mock the transction invoker for the registration CTX that receives the data that should be passed based on this domain RegistrationRequest entity
            List<Ellucian.Colleague.Data.Student.Transactions.Messages> responseMsgs = new List<Messages>();
            Ellucian.Colleague.Data.Student.Transactions.Messages msg1 = new Messages();
            msg1.Message = "Message1";
            msg1.MessageSection = "MsgSec1";
            Ellucian.Colleague.Data.Student.Transactions.Messages msg2 = new Messages();
            msg2.Message = "Message2";
            msg2.MessageSection = "MsgSec2";
            responseMsgs.Add(msg1);
            responseMsgs.Add(msg2);

            RegisterForSectionsResponse regForSecResponse = new RegisterForSectionsResponse();
            regForSecResponse.IpcRegId = "SuccessfulReg";
            regForSecResponse.Messages = responseMsgs;
            regForSecResponse.RegisteredSectionIds = new List<string>() { null, string.Empty, sectReg3.SectionId, sectReg3.SectionId };

            transManagerMock.Setup(tm => tm.ExecuteAsync<RegisterForSectionsRequest, RegisterForSectionsResponse>(
                It.Is<RegisterForSectionsRequest>(rr => rr.StudentId == studentID &&
                rr.Sections.Count == sectionRegs.Count &&
                rr.Sections[0].SectionIds == sectionRegs[0].SectionId &&
                rr.Sections[0].SectionAction == sectionRegs[0].Action.ToString() &&
                rr.Sections[0].SectionCredits == sectionRegs[0].Credits &&
                rr.Sections[0].SectionDropReasonCode == sectionRegs[0].DropReasonCode &&
                rr.Sections[1].SectionIds == sectionRegs[1].SectionId &&
                rr.Sections[1].SectionAction == sectionRegs[1].Action.ToString() &&
                rr.Sections[1].SectionCredits == sectionRegs[1].Credits &&
                rr.Sections[1].SectionDropReasonCode == sectionRegs[1].DropReasonCode))).ReturnsAsync(regForSecResponse);

            RegistrationResponse regResponse = await studentRepository.RegisterAsync(regRequest);

            Assert.IsTrue(regResponse.PaymentControlId == regForSecResponse.IpcRegId);
            Assert.IsTrue(regResponse.Messages.Count == responseMsgs.Count);
            Assert.IsTrue(regResponse.Messages[0].SectionId == responseMsgs[0].MessageSection);
            Assert.IsTrue(regResponse.Messages[0].Message == responseMsgs[0].Message);
            Assert.IsTrue(regResponse.Messages[1].SectionId == responseMsgs[1].MessageSection);
            Assert.IsTrue(regResponse.Messages[1].Message == responseMsgs[1].Message);
            CollectionAssert.AreEqual(new List<string>() { sectReg3.SectionId }, regResponse.RegisteredSectionIds);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),"NullErrorMessageStudent")]
        // Verify the error handling of an error message returned from the CTX.
        public async Task StudentRepository_ErrorMessageFromCTX()
        {
            // A RegistrationRequest entity that will be passed to RegisterAsync
            string studentID = "NullErrorMessageStudent";
            List<Domain.Student.Entities.SectionRegistration> sectionRegs = new List<Domain.Student.Entities.SectionRegistration>();
            Domain.Student.Entities.RegistrationRequest regRequest = new RegistrationRequest(studentID, sectionRegs);

            // Mock the transction invoker for the registration CTX that receives the data that should be passed based on this domain RegistrationRequest entity
            RegisterForSectionsResponse regForSecResponse = new RegisterForSectionsResponse();
            regForSecResponse.ErrorMessage = "ErrorMessageFromCTX";
            transManagerMock.Setup(tm => tm.ExecuteAsync<RegisterForSectionsRequest, RegisterForSectionsResponse>(
                It.Is<RegisterForSectionsRequest>(rr => rr.StudentId == studentID))).ReturnsAsync(regForSecResponse);

            RegistrationResponse regResponse = await studentRepository.RegisterAsync(regRequest);
        }

    }


    [TestClass]
    public class StudentRepository_SearchByNameForExactMatchAsync
    {

        protected Mock<IColleagueTransactionFactory> transFactoryMock;
        protected Mock<ObjectCache> localCacheMock;
        protected Mock<ICacheProvider> cacheProviderMock;
        protected Mock<IColleagueDataReader> dataReaderMock;
        protected Mock<IColleagueDataReader> anonymousDataReaderMock;
        protected Mock<ILogger> loggerMock;
        protected Mock<IColleagueTransactionInvoker> transManagerMock;
        ApiSettings apiSettingsMock;
        protected Mock<StudentRepository> studentRepoMock;

        StudentRepository studentRepository;

        string knownStudentId1 = "111";
        string knownStudentId2 = "222";

        private static Collection<Base.DataContracts.Person> BuildPeople(string[] ids)
        {
            var people = new List<Base.DataContracts.Person>();
            foreach (var id in ids)
            {
                people.Add(new Base.DataContracts.Person() { Recordkey = id, LastName = id });
            }
            return new Collection<Base.DataContracts.Person>(people);
        }
        private static Collection<Students> BuildStudents(string[] ids)
        {
            var students = new List<Students>();
            foreach (var id in ids)
            {
                students.Add(new Students() { Recordkey = id });
            }
            return new Collection<Students>(students);
        }

        private static Collection<Base.DataContracts.PersonSt> BuildPersonStRecords(string[] ids)
        {
            var students = new List<Base.DataContracts.PersonSt>();
            foreach (var id in ids)
            {
                students.Add(new Base.DataContracts.PersonSt() { Recordkey = id });
            }
            return new Collection<Base.DataContracts.PersonSt>(students);
        }

        [TestInitialize]
        public void Initialize()
        {
            // Initialize person setup and Mock framework
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            // Cache Mock
            localCacheMock = new Mock<ObjectCache>();
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
            //mock PID2 configuration
            dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>(a =>
               a.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
               .Returns(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
               {
                   Recordkey = "PREFERRED",
                   NahNameHierarchy = new List<string>() { "MA", "XYZ", "PF" }
               });

            var dflts = new Dflts()
            {
                DfltsRecordDenialMsg = "Record not accesible due to a privacy request"
            };
            dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true)).ReturnsAsync(dflts);
            // Build the test repository
            studentRepository = BuildMockStudentRepository();
        }

        private StudentRepository BuildMockStudentRepository()
        {

            StudentRepository repository = new StudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

            return repository;
        }

        private void MockStudentData(string studentId)
        {
            dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPeople(s)));
            dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildStudents(s)));
            dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPersonStRecords(s)));
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.StudentAdvisement>)null);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.ForeignPerson>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Base.DataContracts.ForeignPerson>)null);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.Applicants>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.Applicants>)null);

            // Mock the read of the Preferred Name Address Hierarchy for the Preferred Name
            dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>(a =>
                a.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                .Returns(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                {
                    Recordkey = "PREFERRED",
                    NahNameHierarchy = new List<string>() { "MA", "XYZ", "PF" }
                });

            //// Mock the call for getting the preferred address
            transManagerMock.Setup<TxGetHierarchyAddressResponse>(
                manager => manager.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(
                    It.IsAny<TxGetHierarchyAddressRequest>())
                ).Returns<TxGetHierarchyAddressRequest>(request =>
                {
                    return new TxGetHierarchyAddressResponse() { OutAddressId = studentId, OutAddressLabel = new List<string>() { "AdressLabel" } };
                });

        }

        private void MockStudentData(string studentId1, string studentId2)
        {
            // Mock individual reads for each student
            MockStudentData(studentId1);
            MockStudentData(studentId2);
            // Mock bulk reads that must return a collection
            dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true))
                .ReturnsAsync(new Collection<Base.DataContracts.Person>()
                {
                        new Base.DataContracts.Person() { Recordkey = studentId1, LastName = "last" + studentId1 },
                        new Base.DataContracts.Person() { Recordkey = studentId2, LastName = "last" + studentId2 }
                });
            dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true))
                .ReturnsAsync(new Collection<Student.DataContracts.Students>()
                {
                        new Student.DataContracts.Students() { Recordkey = studentId1, StuAcadPrograms = null },
                        new Student.DataContracts.Students() { Recordkey = studentId2, StuAcadPrograms = null }
                });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            studentRepository = null;
        }

        [TestMethod]
        public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfCalledWithEmptyNames()
        {
            var result = await studentRepository.GetStudentSearchByNameForExactMatchAsync(string.Empty, string.Empty, string.Empty);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfCalledWithNullNames()
        {
            var result = await studentRepository.GetStudentSearchByNameForExactMatchAsync(null);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SearchByNameForExactMatchAsync_ThrowArgumentNullExceptionIdLastNameIsNull()
        {
            var result = await studentRepository.GetStudentSearchByNameForExactMatchAsync(null, "firstname", "middlename");
        }

        [TestMethod]
        public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsEmptyList()
        {
            var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = new List<string>() };
            transManagerMock.Setup(manager => manager
                    .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                    .ReturnsAsync(lookupStringResponse);
            var lastName = "Gerbil";
            var result = await studentRepository.GetStudentSearchByNameForExactMatchAsync(lastName);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsKeyListNull()
        {
            var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = null };
            transManagerMock.Setup(manager => manager
                    .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                    .ReturnsAsync(lookupStringResponse);
            var lastName = "Gerbil";
            var result = await studentRepository.GetStudentSearchByNameForExactMatchAsync(lastName);
            Assert.AreEqual(0, result.Count());
        }
        [TestMethod]
        public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsNull()
        {
            transManagerMock.Setup(manager => manager
                    .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                    .ReturnsAsync(null);
            var lastName = "Gerbil";
            var result = await studentRepository.GetStudentSearchByNameForExactMatchAsync(lastName);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfStudentSelectReturnsNull()
        {
            var personIdsList = new string[] { "001", "002", "003" };
            string[] nullStudentIdsList = null;
            var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
            transManagerMock.Setup(manager => manager
                    .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                    .ReturnsAsync(lookupStringResponse);
            dataReaderMock.Setup(acc => acc.Select("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullStudentIdsList);
            var lastName = "Gerbil";
            var result = await studentRepository.GetStudentSearchByNameForExactMatchAsync(lastName);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfStudentSelectReturnsEmptyList()
        {
            var personIdsList = new string[] { "001", "002", "003" };
            var blankStudentIdsList = new string[] { };
            var lastName = "Gerbil";
            var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
            transManagerMock.Setup(manager => manager
                    .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                    .ReturnsAsync(lookupStringResponse);
            dataReaderMock.Setup(acc => acc.Select("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(blankStudentIdsList);
            var result = await studentRepository.GetStudentSearchByNameForExactMatchAsync(lastName);
            Assert.AreEqual(0, result.Count());
        }

      

        [TestMethod]
        public async Task SearchByNameForExactMatchAsync_ReturnsAdviseesRequestedReviewFirst()
        {
            var personIdsList = new string[] { knownStudentId1, knownStudentId2 };
            var requestedReviewList = new string[] { knownStudentId2 };
            var blankStudentIdsList = new string[] { };
            var lastName = "Gerbil";
            var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
            transManagerMock.Setup(manager => manager
                    .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                    .ReturnsAsync(lookupStringResponse);

            // mock for FilterByEntity
            dataReaderMock.Setup(acc => acc.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(personIdsList);
            // mock for GetCurrentPage, students who have requested review
            dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(requestedReviewList);
            dataReaderMock.Setup(a => a.SelectAsync("PERSON", requestedReviewList, It.IsAny<string>())).ReturnsAsync(requestedReviewList);
            // mock for GetCurrentPage, all students
            dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", personIdsList, It.IsAny<string>())).ReturnsAsync(personIdsList);

            // mock data for retrieving all person/student information
            MockStudentData(knownStudentId1, knownStudentId2);

            var result = await studentRepository.GetStudentSearchByNameForExactMatchAsync(lastName, null, null);

            Assert.AreEqual(knownStudentId2, result.ElementAt(0).Id);
            Assert.AreEqual(knownStudentId1, result.ElementAt(1).Id);
            Assert.AreEqual(2, result.Count());
        }
    }

}