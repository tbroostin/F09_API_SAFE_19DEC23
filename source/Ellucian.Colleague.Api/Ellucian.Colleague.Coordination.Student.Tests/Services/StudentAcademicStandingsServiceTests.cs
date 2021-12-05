/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos.EnumProperties;
using slf4net;
using Ellucian.Web.Security;
using Role = Ellucian.Colleague.Domain.Entities.Role;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentAcademicStandingsServiceTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory CurrentUserFactory;
        public Mock<IPersonRepository> personRepositoryMock;
        public Mock<ITermRepository> termRepositoryMock;
        public Mock<IStudentRepository> studentRepositoryMock;
        public Mock<IStudentStandingRepository> studentStandingRepositoryMock;
        public Mock<IStudentReferenceDataRepository> referenceDataRepository;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


        private StudentAcademicStandingsService studentAcademicStandingsService;
        private IEnumerable<StudentStanding> allStudentStandings;
        private List<Dtos.StudentAcademicStandings> studentAcademicStandingsCollection;

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicProgram> allAcademicPrograms;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> allAcademicLevels;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicStanding2> allAcademicStandings;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> allTerms;

        private string id = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d";
        private string personGuid = "b371fba4-797d-4c2c-8adc-bedd6d9db730";
        private string personId = "0001585";

        protected Role viewStudentRole = new Role(1, "VIEW.STUDENT.INFORMATION");

        [TestInitialize]
        public async void Initialize()
        {
            MockInitialize();
            BuildData();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            //Mock roles repo and permission
            var permissionViewAnyStudent = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcadStandings);
            viewStudentRole.AddPermission(permissionViewAnyStudent);
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() {viewStudentRole});


            referenceDataRepository = new Mock<IStudentReferenceDataRepository>();
            studentAcademicStandingsService = new StudentAcademicStandingsService(
                personRepositoryMock.Object, termRepositoryMock.Object,
                studentStandingRepositoryMock.Object, referenceDataRepository.Object,
                adapterRegistryMock.Object, CurrentUserFactory,
                roleRepositoryMock.Object, loggerMock.Object, studentRepositoryMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            CurrentUserFactory = null;
            personRepositoryMock = null;
            termRepositoryMock = null;
            studentRepositoryMock = null;
            studentStandingRepositoryMock = null;
            studentAcademicStandingsService = null;
            allStudentStandings = null;
        }
        
        [TestMethod]
        public async Task StudentAcademicStandings_GetAll()
        {
            referenceDataRepository.Setup(i => i.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(allAcademicPrograms);
            referenceDataRepository.Setup(i => i.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(allAcademicLevels);
            referenceDataRepository.Setup(i => i.GetAcademicStandings2Async(It.IsAny<bool>())).ReturnsAsync(allAcademicStandings);

            termRepositoryMock.Setup(t => t.GetAsync(It.IsAny<bool>())).ReturnsAsync(allTerms);
            Dictionary<string, string> personGuidCollection = new Dictionary<string, string>
            {
                { personId, personGuid }
            };
            personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuidCollection);

            var studentStandingsTuple
                = new Tuple<IEnumerable<StudentStanding>, int>(allStudentStandings, allStudentStandings.Count());

            studentStandingRepositoryMock.Setup(s => s.GetStudentStandingsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(studentStandingsTuple);

            var actuals = await studentAcademicStandingsService.GetStudentAcademicStandingsAsync(allStudentStandings.Count(), 0, false);

            Assert.IsNotNull(actuals);
            Assert.IsTrue(actuals.Item1.Count() > 0);
            Assert.IsTrue(actuals.Item2 > 0);

            foreach (var actual in actuals.Item1)
            {
                var expected = allStudentStandings.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.OverrideReason, expected.OverrideReason);
                if (!string.IsNullOrEmpty(expected.Level))
                {
                   var academicLevel =  allAcademicLevels.FirstOrDefault(x => x.Code == expected.Level);
                    Assert.IsNotNull(academicLevel);
                    Assert.AreEqual(academicLevel.Guid, actual.Level.Id);
                }
                if (!string.IsNullOrEmpty(expected.Program))
                {
                    var academicProgram = allAcademicPrograms.FirstOrDefault(x => x.Code == expected.Program);
                    Assert.IsNotNull(academicProgram);
                    Assert.AreEqual(academicProgram.Guid, actual.Program.Id);
                }
                if (!string.IsNullOrEmpty(expected.StandingCode))
                {
                    var academicStandings = allAcademicStandings.FirstOrDefault(x => x.Code == expected.StandingCode);
                    Assert.IsNotNull(academicStandings);
                    Assert.AreEqual(academicStandings.Guid, actual.Standing.Id);
                }

                if (!string.IsNullOrEmpty(expected.CalcStandingCode))
                {
                    var academicStandings = allAcademicStandings.FirstOrDefault(x => x.Code == expected.CalcStandingCode);
                    Assert.IsNotNull(academicStandings);
                    Assert.AreEqual(academicStandings.Guid, actual.OverrideStanding.Id);
                }

                switch (expected.Type)
                {
                    case StudentStandingType.AcademicLevel:
                        Assert.IsTrue( actual.Type == StudentAcademicStandingsType.Level);
                        break;
                    case StudentStandingType.Term:
                        Assert.IsTrue(actual.Type == StudentAcademicStandingsType.Academicperiod);
                        break;
                    case StudentStandingType.Program:
                        Assert.IsTrue(actual.Type == StudentAcademicStandingsType.Program);
                        break;
                    default:
                        break;
                }

            }
        }
  
        [TestMethod]
        public async Task StudentAcademicStandings_GetAll_True()
        {
            referenceDataRepository.Setup(i => i.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(allAcademicPrograms);
            referenceDataRepository.Setup(i => i.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(allAcademicLevels);
            referenceDataRepository.Setup(i => i.GetAcademicStandings2Async(It.IsAny<bool>())).ReturnsAsync(allAcademicStandings);

            termRepositoryMock.Setup(t => t.GetAsync(It.IsAny<bool>())).ReturnsAsync(allTerms);
            Dictionary<string, string> personGuidCollection = new Dictionary<string, string>
            {
                { personId, personGuid }
            };
            personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuidCollection);

            var studentStandingsTuple
                = new Tuple<IEnumerable<StudentStanding>, int>(allStudentStandings, allStudentStandings.Count());

            studentStandingRepositoryMock.Setup(s => s.GetStudentStandingsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(studentStandingsTuple);

            var actuals = await studentAcademicStandingsService.GetStudentAcademicStandingsAsync(allStudentStandings.Count(), 0, true);

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals.Item1)
            {
                var expected = allStudentStandings.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.OverrideReason, expected.OverrideReason);
                if (!string.IsNullOrEmpty(expected.Level))
                {
                    var academicLevel = allAcademicLevels.FirstOrDefault(x => x.Code == expected.Level);
                    Assert.IsNotNull(academicLevel);
                    Assert.AreEqual(academicLevel.Guid, actual.Level.Id);
                }
                if (!string.IsNullOrEmpty(expected.Program))
                {
                    var academicProgram = allAcademicPrograms.FirstOrDefault(x => x.Code == expected.Program);
                    Assert.IsNotNull(academicProgram);
                    Assert.AreEqual(academicProgram.Guid, actual.Program.Id);
                }
                if (!string.IsNullOrEmpty(expected.StandingCode))
                {
                    var academicStandings = allAcademicStandings.FirstOrDefault(x => x.Code == expected.StandingCode);
                    Assert.IsNotNull(academicStandings);
                    Assert.AreEqual(academicStandings.Guid, actual.Standing.Id);
                }

                if (!string.IsNullOrEmpty(expected.CalcStandingCode))
                {
                    var academicStandings = allAcademicStandings.FirstOrDefault(x => x.Code == expected.CalcStandingCode);
                    Assert.IsNotNull(academicStandings);
                    Assert.AreEqual(academicStandings.Guid, actual.OverrideStanding.Id);
                }

                switch (expected.Type)
                {
                    case StudentStandingType.AcademicLevel:
                        Assert.IsTrue(actual.Type == StudentAcademicStandingsType.Level);
                        break;
                    case StudentStandingType.Term:
                        Assert.IsTrue(actual.Type == StudentAcademicStandingsType.Academicperiod);
                        break;
                    case StudentStandingType.Program:
                        Assert.IsTrue(actual.Type == StudentAcademicStandingsType.Program);
                        break;
                    default:
                        break;
                }
            }
        }
  
        [TestMethod]
        public async Task StudentAcademicStandings_GetById()
        {
            referenceDataRepository.Setup(i => i.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(allAcademicPrograms);
            referenceDataRepository.Setup(i => i.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(allAcademicLevels);
            referenceDataRepository.Setup(i => i.GetAcademicStandings2Async(It.IsAny<bool>())).ReturnsAsync(allAcademicStandings);

            termRepositoryMock.Setup(t => t.GetAsync(It.IsAny<bool>())).ReturnsAsync(allTerms);
            Dictionary<string, string> personGuidCollection = new Dictionary<string, string>
            {
                { personId, personGuid }
            };
            personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuidCollection);
            var expected = allStudentStandings.FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));

            studentStandingRepositoryMock.Setup(s => s.GetStudentStandingByGuidAsync(id)).ReturnsAsync(expected);
            
            var actual = await studentAcademicStandingsService.GetStudentAcademicStandingsByGuidAsync(id);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Guid, actual.Id);
            Assert.AreEqual(expected.OverrideReason, expected.OverrideReason);
            if (!string.IsNullOrEmpty(expected.Level))
            {
                var academicLevel = allAcademicLevels.FirstOrDefault(x => x.Code == expected.Level);
                Assert.IsNotNull(academicLevel);
                Assert.AreEqual(academicLevel.Guid, actual.Level.Id);
            }
            if (!string.IsNullOrEmpty(expected.Program))
            {
                var academicProgram = allAcademicPrograms.FirstOrDefault(x => x.Code == expected.Program);
                Assert.IsNotNull(academicProgram);
                Assert.AreEqual(academicProgram.Guid, actual.Program.Id);
            }
            if (!string.IsNullOrEmpty(expected.StandingCode))
            {
                var academicStandings = allAcademicStandings.FirstOrDefault(x => x.Code == expected.StandingCode);
                Assert.IsNotNull(academicStandings);
                Assert.AreEqual(academicStandings.Guid, actual.Standing.Id);
            }

            if (!string.IsNullOrEmpty(expected.CalcStandingCode))
            {
                var academicStandings = allAcademicStandings.FirstOrDefault(x => x.Code == expected.CalcStandingCode);
                Assert.IsNotNull(academicStandings);
                Assert.AreEqual(academicStandings.Guid, actual.OverrideStanding.Id);
            }

            switch (expected.Type)
            {
                case StudentStandingType.AcademicLevel:
                    Assert.IsTrue(actual.Type == StudentAcademicStandingsType.Level);
                    break;
                case StudentStandingType.Term:
                    Assert.IsTrue(actual.Type == StudentAcademicStandingsType.Academicperiod);
                    break;
                case StudentStandingType.Program:
                    Assert.IsTrue(actual.Type == StudentAcademicStandingsType.Program);
                    break;
                default:
                    break;
            }
        }

    
        [TestMethod]
        [ExpectedException(typeof (IntegrationApiException))]
        public async Task StudentAcademicStandings_GetById_Exception()
        {
            try
            {
                await studentAcademicStandingsService.GetStudentAcademicStandingsByGuidAsync("abc");
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsTrue(ex.Errors.Count > 0, "Error Count");
                bool messageFound = false;
                foreach (var error in ex.Errors)
                {
                    if (error.Message == "No Student Academic Standings was found for guid abc" && error.Code == "GUID.Not.Found")
                    {
                        messageFound = true;
                    }
                }
                Assert.IsTrue(messageFound, "Appropriate Error Message found in error collection.");
                throw ex;
            }
        }
    
        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task StudentAcademicStandings_GetById_ArgumentNullException()
        {
            await studentAcademicStandingsService.GetStudentAcademicStandingsByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentAcademicStandings_GetById_KeyNotFoundException()
        {
            studentStandingRepositoryMock.Setup(s => s.GetStudentStandingByGuidAsync(id)).Throws<KeyNotFoundException>();

            try
            {
                await studentAcademicStandingsService.GetStudentAcademicStandingsByGuidAsync(id);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsTrue(ex.Errors.Count > 0, "Error Count");
                bool messageFound = false;
                foreach (var error in ex.Errors)
                {
                    if (error.Message == "No Student Academic Standings was found for guid " + id && error.Code == "GUID.Not.Found")
                    {
                        messageFound = true;
                    }
                }
                Assert.IsTrue(messageFound, "Appropriate Error Message found in error collection.");
                throw ex;
            }
        }

      
        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentAcademicStandings_GetById_InvalidOperationException()
        {
            studentStandingRepositoryMock.Setup(s => s.GetStudentStandingByGuidAsync(id)).Throws<InvalidOperationException>();

            try
            {
                await studentAcademicStandingsService.GetStudentAcademicStandingsByGuidAsync(id);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsTrue(ex.Errors.Count > 0, "Error Count");
                bool messageFound = false;
                foreach (var error in ex.Errors)
                {
                    if (error.Message == "No Student Academic Standings was found for guid " + id && error.Code == "GUID.Not.Found")
                    {
                        messageFound = true;
                    }
                }
                Assert.IsTrue(messageFound, "Appropriate Error Message found in error collection.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentAcademicStandings_GetById_RepositoryException()
        {
            studentStandingRepositoryMock.Setup(s => s.GetStudentStandingByGuidAsync(id)).Throws<RepositoryException>();
                
            await studentAcademicStandingsService.GetStudentAcademicStandingsByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task StudentAcademicStandings_GetById_ArgumentException()
        {
            studentStandingRepositoryMock.Setup(s => s.GetStudentStandingByGuidAsync(id)).Throws<ArgumentException>();

            await studentAcademicStandingsService.GetStudentAcademicStandingsByGuidAsync(id);
        }

        private async void BuildData()
        {
            allAcademicPrograms = await new TestAcademicProgramRepository().GetAsync();
            allAcademicLevels = await new TestAcademicLevelRepository().GetAsync();
            allAcademicStandings = await new TestStudentReferenceDataRepository().GetAcademicStandings2Async(false);
            allTerms = new TestTermRepository().Get();

            allStudentStandings = new List<StudentStanding>()
            {
                new StudentStanding("1", "0001585", "PROB", DateTime.Now)
                {
                    Guid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                    Program = "BA-MATH",
                    Level = "UG",
                    CalcStandingCode = "PROB",
                    Type = StudentStandingType.AcademicLevel,
                    Term = "2001/FA"
                },
                new StudentStanding("2", "0001585", "PROB", DateTime.Now)
                {
                    Guid = "502b4820-2c20-4066-b31f-5fcb420eb3f8",
                    Program = "AA-NURS",
                    Level = "GR",
                    CalcStandingCode = "PROB",
                    Type = StudentStandingType.Program
                },
                new StudentStanding("3", "0001585", "PROB", DateTime.Now)
                {
                    Guid = "c46a1225-8b6c-46cf-a119-78e1a54c603d",
                    Program = "BA-MATH",
                    Level = "UG",
                    CalcStandingCode = "PROB",
                    Type = StudentStandingType.Term,
                    OverrideReason = "Student is in good standing."
                }

            };
        }

        public void MockInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            CurrentUserFactory = new StudentUserFactory();

            personRepositoryMock = new Mock<IPersonRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            studentRepositoryMock = new Mock<IStudentRepository>();
            studentStandingRepositoryMock = new Mock<IStudentStandingRepository>();

        }


        public class StudentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Johnny",
                        PersonId = "0000894",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "VIEW.STUDENT.INFORMATION" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}
