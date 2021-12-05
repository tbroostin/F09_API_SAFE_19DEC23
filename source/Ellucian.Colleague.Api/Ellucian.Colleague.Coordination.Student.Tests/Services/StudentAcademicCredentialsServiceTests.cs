//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentAcademicCredentialsServiceTests : UserFactories.StudentUserFactory.StudentAcademicCredentialsUser
    {
        private Mock<IStudentAcademicCredentialsRepository> _studentAcademicCredentialsRepositoryMock;
        private Mock<IStudentAcademicProgramRepository> _studentAcademicProgramRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<ITermRepository> _termRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _sacUserMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private StudentAcademicCredentialsService _studentAcademicCredentialsService;
        ICurrentUserFactory userFactory;

        List<StudentAcademicCredential> entities = new List<StudentAcademicCredential>();
        List<StudentAcademicCredential> entitiesWithNoData = new List<StudentAcademicCredential>();
        Tuple<IEnumerable<StudentAcademicCredential>, int> tupleWithNodata;
        List<Dtos.StudentAcademicCredentials> studentAcademicCredential;
        PersonFilterFilter2 personFilterFilter2;
        Dtos.Filters.AcademicProgramsFilter academicProgramsFilter;
        private Domain.Entities.Permission permissionViewAnyStudentAcadCreds;
        protected Domain.Entities.Role _sacRole = new Domain.Entities.Role(1, "VIEW.STUDENT.ACADEMIC.CREDENTIALS");


        private const string studentAcademicCredentialsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            _studentAcademicCredentialsRepositoryMock = new Mock<IStudentAcademicCredentialsRepository>();
            _studentAcademicProgramRepositoryMock = new Mock<IStudentAcademicProgramRepository>();
            _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _termRepositoryMock = new Mock<ITermRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _sacUserMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();

            userFactory = new UserFactories.StudentUserFactory.StudentAcademicCredentialsUser();
            // Mock permissions
            permissionViewAnyStudentAcadCreds = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcademicCredentials);
            _sacRole.AddPermission(permissionViewAnyStudentAcadCreds);

            BuildAndMockData();

            _studentAcademicCredentialsService = new StudentAcademicCredentialsService(
                _studentAcademicCredentialsRepositoryMock.Object, _studentAcademicProgramRepositoryMock.Object,
                _personRepositoryMock.Object, _referenceDataRepositoryMock.Object, _studentReferenceDataRepositoryMock.Object,
                _termRepositoryMock.Object, _adapterRegistryMock.Object, userFactory, _roleRepositoryMock.Object,
               _configurationRepoMock.Object, _loggerMock.Object);
        }

        private void BuildAndMockData()
        {
            // role repo
            _sacRole.AddPermission(new Domain.Entities.Permission("VIEW.STUDENT.ACADEMIC.CREDENTIALS"));
            _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { _sacRole });

            entities = new List<StudentAcademicCredential>()
            {
                new StudentAcademicCredential(studentAcademicCredentialsGuid, "1")
                {
                    AcadAcadProgramId = "1",
                    AcadDisciplines = new List<string>()
                    {
                        "AT", "CU"
                    },
                    AcademicLevel = "UG",
                    AcademicPeriod = "ap1",
                    AcadHonors = new List<string>()
                    {
                        "ah1", "ah2"
                    },
                    AcadPersonId = "1",
                    AcadTerm = "2019/FA",
                    AcadThesis = "Thesis 1",
                    Ccds = new List<Tuple<string, DateTime?>>()
                    {
                        new Tuple<string, DateTime?>("CU", DateTime.Today)
                    },
                    Degrees = new List<Tuple<string, DateTime?>>()
                    {
                        new Tuple<string, DateTime?>("AT", DateTime.Today)
                    },
                    GraduatedOn = DateTime.Today.AddDays(-30),
                    StudentId = "1",
                    StudentProgramGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    AcadMajors = new List<string>()
                    {
                        "AT", "AC"
                    },
                    AcadMinors = new List<string>()
                    {
                        "CU"
                    },
                    AcadSpecializations = new List<string>()
                    {
                        "MT"
                    }
                }
            };
            entitiesWithNoData = new List<StudentAcademicCredential>()
            {
                new StudentAcademicCredential(studentAcademicCredentialsGuid, "1")
                {
                    AcadAcadProgramId = "",
                    AcadDisciplines = new List<string>()
                    {

                    },
                    AcademicLevel = "",
                    AcademicPeriod = "",
                    AcadHonors = new List<string>()
                    {
                        "", ""
                    },
                    AcadPersonId = "",
                    AcadTerm = "",
                    AcadThesis = "Thesis 1",
                    Ccds = new List<Tuple<string, DateTime?>>()
                    {
                        new Tuple<string, DateTime?>("", DateTime.Today)
                    },
                    Degrees = new List<Tuple<string, DateTime?>>()
                    {
                        new Tuple<string, DateTime?>("", DateTime.Today)
                    },
                    GraduatedOn = DateTime.Today.AddDays(-30),
                    StudentId = "",
                    StudentProgramGuid = ""
                }
            };
            Tuple<IEnumerable<StudentAcademicCredential>, int> tuple = new Tuple<IEnumerable<StudentAcademicCredential>, int>(entities, entities.Count());
            tupleWithNodata = new Tuple<IEnumerable<StudentAcademicCredential>, int>(entitiesWithNoData, entitiesWithNoData.Count());
            studentAcademicCredential = new List<StudentAcademicCredentials>() {
                new StudentAcademicCredentials()
                {
                    Credentials = new List<Dtos.DtoProperties.StudentAcademicCredentialsCredentials>()
                    {
                        new StudentAcademicCredentialsCredentials()
                        {
                            Credential = new GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                        },
                        new StudentAcademicCredentialsCredentials()
                        {
                            Credential = new GuidObject2("d2253ac7-9931-4560-b42f-1fccd43c952e"),
                        }
                    },
                    GraduatedOn = DateTime.Today.AddDays(-30),
                    Student = new GuidObject2("0f80bbb9-9089-46ab-859e-f7c3469eb863"),
                    AcademicLevel = new GuidObject2("558ca14c-718a-4b6e-8d92-77f498034f9f"),
                    StudentProgram = new GuidObject2("efab2e38-6d46-41f1-9955-a95e0250f1f3"),
                    GraduationAcademicPeriod = new GuidObject2("7f3aac22-e0b5-4159-b4e2-da158362c41b")
                }
            };
            personFilterFilter2 = new PersonFilterFilter2()
            {
                personFilter = new GuidObject2("b07ab144-cb26-4b4a-a098-82d034b6e41b")
            };
            academicProgramsFilter = new AcademicProgramsFilter()
            {
                AcademicPrograms = new GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
            };
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<StudentAcademicCredential>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(tuple);

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("1", "da8549d0-7271-46cf-8159-cb0ac5cd74b6");
            _personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");


            List<AcadCredential> acadCredentials = new List<AcadCredential>()
            {
                    new AcadCredential("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", Domain.Base.Entities.AcademicCredentialType.Degree),
                    new AcadCredential("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", Domain.Base.Entities.AcademicCredentialType.Degree),
                    new AcadCredential("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", Domain.Base.Entities.AcademicCredentialType.Certificate)
            };
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(acadCredentials);
            _referenceDataRepositoryMock.SetupSequence(repo => repo.GetAcadCredentialsGuidAsync(It.IsAny<string>()))
                .Returns(Task.FromResult("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"))
                .Returns(Task.FromResult("d2253ac7-9931-4560-b42f-1fccd43c952e"));

            List<Domain.Base.Entities.AcademicDiscipline> acadDiscipliness = new List<Domain.Base.Entities.AcademicDiscipline>()
            {
                    new Domain.Base.Entities.AcademicDiscipline("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", Domain.Base.Entities.AcademicDisciplineType.Major),
                    new Domain.Base.Entities.AcademicDiscipline("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", Domain.Base.Entities.AcademicDisciplineType.Major),
                    new Domain.Base.Entities.AcademicDiscipline("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", Domain.Base.Entities.AcademicDisciplineType.Minor),
                    new Domain.Base.Entities.AcademicDiscipline("e2253ac7-9931-4560-b42f-1fccd43c952e", "MT", "Math", Domain.Base.Entities.AcademicDisciplineType.Concentration)
            };
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(acadDiscipliness);
            _referenceDataRepositoryMock.SetupSequence(repo => repo.GetAcadDisciplinesGuidAsync(It.IsAny<string>()))
                .Returns(Task.FromResult("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"))
                .Returns(Task.FromResult("d2253ac7-9931-4560-b42f-1fccd43c952e"));

            List<Domain.Base.Entities.OtherHonor> otherHonors = new List<Domain.Base.Entities.OtherHonor>()
            {
                    new Domain.Base.Entities.OtherHonor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.OtherHonor("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.OtherHonor("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
            };
            _referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsAsync(It.IsAny<bool>())).ReturnsAsync(otherHonors);
            _referenceDataRepositoryMock.SetupSequence(repo => repo.GetOtherHonorsGuidAsync(It.IsAny<string>()))
                .Returns(Task.FromResult("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"))
                .Returns(Task.FromResult("d2253ac7-9931-4560-b42f-1fccd43c952e"));

            List<AcademicLevel> academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
            _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevels);
            _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsGuidAsync(It.IsAny<string>())).ReturnsAsync("558ca14c-718a-4b6e-8d92-77f498034f9f");
            //_referenceDataRepositoryMock.Setup(repo => repo.GetAcadCredentialsGuidAsync(It.IsAny<string>())).ReturnsAsync("");

            List<AcademicPeriod> academicPeriods = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new AcademicPeriod("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "2016 Spring", new DateTime(2016, 01, 01), new DateTime(2016, 05, 01), 2016, 1, "Spring", "", "", null),
                    new AcademicPeriod("7f3aac22-e0b5-4159-b4e2-da158362c41b", "2019/FA", "2019 Fall", new DateTime(2019, 09, 01), new DateTime(2019, 10, 15), 2019, 2, "Fall", "", "", null),
                    new AcademicPeriod("8f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Spr", "2017 Spring", new DateTime(2017, 01, 01), new DateTime(2017, 05, 01), 2017, 3, "Spring", "", "", null),
                    new AcademicPeriod("9f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Fall", "2017 Fall", new DateTime(2017, 09, 01), new DateTime(2017, 12, 31), 2017, 4, "Fall", "", "", null)
                };

            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(academicPeriods);
            List<Domain.Student.Entities.Term> terms = (new TestTermRepository().GetAsync()).Result.ToList();
            _termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync((terms));
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ReturnsAsync("7f3aac22-e0b5-4159-b4e2-da158362c41b");

            List<Domain.Student.Entities.AcademicProgram> academicPrograms = new List<Domain.Student.Entities.AcademicProgram>()
            {
                    new Domain.Student.Entities.AcademicProgram("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.AcademicProgram("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.AcademicProgram("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
            };
            _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(academicPrograms);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentAcademicCredentialsRepositoryMock = null;
            _studentAcademicProgramRepositoryMock = null;
            _personRepositoryMock = null;
            _referenceDataRepositoryMock = null;
            _studentReferenceDataRepositoryMock = null;
            _termRepositoryMock = null;
            _adapterRegistryMock = null;
            _sacUserMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
            _loggerMock = null;
            _studentAcademicCredentialsService = null;
        }

        [TestMethod]
        public async Task StudentAcademicCredentialsService_GetStudentAcademicCredentialsAsync()
        {
            _studentAcademicProgramRepositoryMock.Setup(repo => repo.GetStudentAcademicProgramIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1*1");
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                personFilterFilter2, academicProgramsFilter, It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);

            var result = results.Item1.FirstOrDefault();

            Assert.IsTrue(result.Disciplines.Any(x1 => x1.Id == "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"), "Contains Discipline Major");
            Assert.IsTrue(result.Disciplines.Any(x2 => x2.Id == "d2253ac7-9931-4560-b42f-1fccd43c952e"), "Contains Discipline Minor");
            Assert.IsTrue(result.Disciplines.Any(x3 => x3.Id == "e2253ac7-9931-4560-b42f-1fccd43c952e"), "Contains Discipline Specialization");

        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_NoCreds_Filter_EmptyResult()
        {
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_NoCred_Filter_EmptyResult()
        {
            studentAcademicCredential.FirstOrDefault().Credentials.FirstOrDefault().Credential.Id = "BAD_ID";
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_Bad_Student_Id_Filter_EmptyResult()
        {
            studentAcademicCredential.FirstOrDefault().Student.Id = "BAD_ID";
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");

            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_Bad_Student_Id_Filter_Exception_EmptyResult()
        {
            studentAcademicCredential.FirstOrDefault().Student.Id = "BAD_ID";
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_NoAcad_Levels_Filter_EmptyResult()
        {
            _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_Acad_Levels_Filter_EmptyResult()
        {
            studentAcademicCredential.FirstOrDefault().AcademicLevel.Id = "BAD_ID";
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_BadStudentAcademicProgram_Id_Filter_EmptyResult()
        {
            studentAcademicCredential.FirstOrDefault().StudentProgram.Id = "BAD_ID";
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_StudentAcademicProgram_Id__StudentId_Not_Samee_Filter_EmptyResult()
        {
            _studentAcademicProgramRepositoryMock.Setup(repo => repo.GetStudentAcademicProgramIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0001234*123");
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_StudentAcademicProgram_Id__Exception_Filter_EmptyResult()
        {
            _studentAcademicProgramRepositoryMock.Setup(repo => repo.GetStudentAcademicProgramIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_NoAcad_Periods_Filter_EmptyResult()
        {
            studentAcademicCredential.FirstOrDefault().StudentProgram = null;
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(new List<AcademicPeriod>() { });
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_GraduationAcademicPeriod_Filter_EmptyResult()
        {
            var stAcadCred = studentAcademicCredential.FirstOrDefault();
            stAcadCred.StudentProgram = null;
            stAcadCred.GraduationAcademicPeriod = new GuidObject2("1234");
            //_termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(new List<AcademicPeriod>() { });
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), studentAcademicCredential.FirstOrDefault(),
                It.IsAny<PersonFilterFilter2>(), It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_Person_Filter_NamedQuery_EmptyResult()
        {
            _referenceDataRepositoryMock.Setup(repo => repo.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            personFilterFilter2 = new PersonFilterFilter2()
            {
                personFilter = new GuidObject2("BAD_ID")
            };
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), null,
                personFilterFilter2, It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_Person_Filter_NamedQuery_Exception_EmptyResult()
        {
            _referenceDataRepositoryMock.Setup(repo => repo.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            personFilterFilter2 = new PersonFilterFilter2()
            {
                personFilter = new GuidObject2("BAD_ID")
            };
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), null,
                personFilterFilter2, It.IsAny<AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_Acad_Program_NamedQuery_EmptyResult()
        {
            _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
            academicProgramsFilter = new AcademicProgramsFilter()
            {
                AcademicPrograms = new GuidObject2("BAD_ID")
            };
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), null,
                null, academicProgramsFilter, It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsAsync_Acad_Program_NamedQuery_NullAcadProgram_EmptyResult()
        {
            academicProgramsFilter = new AcademicProgramsFilter()
            {
                AcademicPrograms = new GuidObject2("da02b8be-0ec0-4ba5-b414-a2bc13f63538")
            };
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), null,
                null, academicProgramsFilter, It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsAsync_IntigrationException()
        {
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(),
               It.IsAny<StudentAcademicCredential>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(tupleWithNodata);
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), null,
                null, null, It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_ArgumentNullException()
        {
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_IntegrationApiException()
        {
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("BAD_ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Item2_Empty_IntegrationApiException()
        {
            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "", "");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("BAD_ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Entity_Null_IntegrationApiException()
        {
            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "BAD_ID", "BAD_ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("BAD_ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_PersonDict_Null_IntegrationApiException()
        {
            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entities.FirstOrDefault());
            _personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(() => null);
            var results = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("BAD_ID", It.IsAny<bool>());
        }

        [TestMethod]
        public async Task GetStudentAcademicCredentialsByGuidAsync()
        {
            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entities.FirstOrDefault());
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Acad_Level_Null_IntegrationApiException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsGuidAsync(It.IsAny<string>())).ReturnsAsync("");
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Acad_Level_Null_IntegrationApiException2()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Degrees_Null_IntegrationApiException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcadCredentialsGuidAsync(It.IsAny<string>())).ReturnsAsync("");
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Degrees_Null_IntegrationApiException2()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcadCredentialsGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Major_Null_ArgumentNullException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            entity.AcadMajors = new List<string>() { "ART" };
            entity.AcadMinors = null;
            entity.AcadSpecializations = null;
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcadDisciplinesGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Major_NotFound_IntegrationApiException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            entity.AcadMajors = new List<string>() { "BAD" };
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_NoMajors_IntegrationApiException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            entity.AcadMinors = null;
            entity.AcadMajors = new List<string>() { "AT" };
            entity.AcadSpecializations = null;
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            List<Domain.Base.Entities.AcademicDiscipline> acadDiscipliness = new List<Domain.Base.Entities.AcademicDiscipline>()
            {
                    new Domain.Base.Entities.AcademicDiscipline("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "CU", "Athletic", Domain.Base.Entities.AcademicDisciplineType.Minor),
                               };
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(acadDiscipliness);

            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Minor_Null_ArgumentNullException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            entity.AcadMinors = new List<string>() { "ART" };
            entity.AcadMajors = null;
            entity.AcadSpecializations = null;
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcadDisciplinesGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Minor_NotFound_IntegrationApiException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            entity.AcadMinors = new List<string>() { "BAD" };
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            // _referenceDataRepositoryMock.Setup(repo => repo.GetAcadDisciplinesGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_NoMinors_IntegrationApiException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            entity.AcadMinors = new List<string>() { "CU" };
            entity.AcadMajors = null;
            entity.AcadSpecializations = null;
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            List<Domain.Base.Entities.AcademicDiscipline> acadDiscipliness = new List<Domain.Base.Entities.AcademicDiscipline>()
            {
                    new Domain.Base.Entities.AcademicDiscipline("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", Domain.Base.Entities.AcademicDisciplineType.Major),
                               };
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(acadDiscipliness);

            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Specialization_Null_ArgumentNullException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            entity.AcadMinors = null;
            entity.AcadMajors = null;
            entity.AcadSpecializations = new List<string>() { "MT" };
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcadDisciplinesGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_NoSpecializations_IntegrationApiException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            entity.AcadMinors = null;
            entity.AcadMajors = null;
            entity.AcadSpecializations = new List<string>() { "MT" };
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            List<Domain.Base.Entities.AcademicDiscipline> acadDiscipliness = new List<Domain.Base.Entities.AcademicDiscipline>()
            {
                    new Domain.Base.Entities.AcademicDiscipline("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", Domain.Base.Entities.AcademicDisciplineType.Major),
                               };
            _referenceDataRepositoryMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(acadDiscipliness);

            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Specialization_NotFound_IntegrationApiException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            entity.AcadSpecializations = new List<string>() { "BAD" };
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            //_referenceDataRepositoryMock.Setup(repo => repo.GetAcadDisciplinesGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_AcadHonors_Null_IntegrationApiException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsGuidAsync(It.IsAny<string>())).ReturnsAsync("");
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_AcadHonors_Null_IntegrationApiException2()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_AcadTerm_Null_IntegrationApiException()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ReturnsAsync("");
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_AcadTerm_Null_IntegrationApiException2()
        {
            var entity = entities.FirstOrDefault();

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task GetStudentAcademicCredentialsByGuidAsync_Gui_Null_IntegrationApiException2()
        {
            var entity = new StudentAcademicCredential()
            {
                AcadAcadProgramId = "1",
                AcadDisciplines = new List<string>()
                    {
                        "AT", "CU"
                    },
                AcademicLevel = "UG",
                AcademicPeriod = "ap1",
                AcadHonors = new List<string>()
                    {
                        "ah1", "ah2"
                    },
                AcadPersonId = "1",
                AcadTerm = "2019/FA",
                AcadThesis = "Thesis 1",
                Ccds = new List<Tuple<string, DateTime?>>()
                    {
                        new Tuple<string, DateTime?>("CU", DateTime.Today)
                    },
                Degrees = new List<Tuple<string, DateTime?>>()
                    {
                        new Tuple<string, DateTime?>("AT", DateTime.Today)
                    },
                GraduatedOn = DateTime.Today.AddDays(-30),
                StudentId = "1",
                StudentProgramGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"
            };

            Tuple<string, string, string> tuple = new Tuple<string, string, string>("ACAD.CREDENTIALS", "ID", "ID");
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetAcadCredentialKeyAsync(It.IsAny<string>())).ReturnsAsync(tuple);
            _studentAcademicCredentialsRepositoryMock.Setup(repo => repo.GetStudentAcademicCredentialByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);
            var result = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync("ID", It.IsAny<bool>());
        }
    }
}