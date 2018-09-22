// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentPetitionServiceTests
    {
        // Sets up a Current user that is a faculty
        public abstract class CurrentUserSetup
        {
            protected Role facultyRole = new Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George Smith",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "GSmith",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }


        [TestClass]
        public class GetStudentPetitions : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepositoryMock;
            private IRoleRepository roleRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentPetitionRepository> studentPetitionRepositoryMock;
            private IStudentPetitionRepository studentPetitionRepository;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private IStudentReferenceDataRepository referenceDataRepository;
            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private IStudentPetitionService studentPetitionService;
            private List<StudentPetition> studentPetitionsData;
            private Section sectionData;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;

                studentPetitionRepositoryMock = new Mock<IStudentPetitionRepository>();
                studentPetitionRepository = studentPetitionRepositoryMock.Object;

                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepositoryMock.Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                logger = new Mock<ILogger>().Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock section response
                sectionData = new TestSectionRepository().GetAsync().Result.First();
                sectionData.AddFaculty("1111100");
                sectionRepositoryMock.Setup(repository => repository.GetSectionAsync("SEC1")).Returns(Task.FromResult(sectionData));

                // Mock student petition response
                studentPetitionsData = BuildStudentPetitionsRepositoryResponse();
                studentPetitionRepositoryMock.Setup(repository => repository.GetStudentPetitionsAsync(It.IsAny<string>())).Returns(Task.FromResult(studentPetitionsData.AsEnumerable()));

                // Mock Adapters
                var petitionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>()).Returns(petitionDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                studentPetitionService = new StudentPetitionService(adapterRegistry, studentPetitionRepository, studentRepository, sectionRepository, referenceDataRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                studentPetitionRepository = null;
                studentRepository = null;
                sectionRepository = null;
                roleRepository = null;
                studentPetitionService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentPetitionAsync_ThrowsExceptionIfStudentStringNull()
            {
                studentPetitionRepositoryMock.Setup(repository => repository.GetStudentPetitionsAsync(It.IsAny<string>())).Throws(new PermissionsException());
                var studentPetitionDto = await studentPetitionService.GetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentPetitionAsync_ThrowsExceptionIfStudentStringIsEmpty()
            {
                studentPetitionRepositoryMock.Setup(repository => repository.GetStudentPetitionsAsync(It.IsAny<string>())).Throws(new PermissionsException());
                var studentPetitionDto = await studentPetitionService.GetAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentPetitionAsync_RethrowsExceptionFromSectionRepository()
            {
                studentPetitionRepositoryMock.Setup(repository => repository.GetStudentPetitionsAsync(It.IsAny<string>())).Throws(new Exception());
                var studentPetitionDto = await studentPetitionService.GetAsync("0000011");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentPetitionAsync_ThrowsExceptionIfCurrentUserIsNotSelf()
            {
                var studentPetitionDto = await studentPetitionService.GetAsync("001122");
            }

            [TestMethod]
            public async Task GetStudentPetitionAsync_ReturnsPetitionsIfCurrentUserIsSelf()
            {
                studentPetitionRepositoryMock.Setup(repository => repository.GetStudentPetitionsAsync("0000011")).Returns(Task.FromResult(studentPetitionsData.AsEnumerable()));

                var studentPetitionDto = await studentPetitionService.GetAsync("0000011");

                Assert.AreEqual(studentPetitionsData.Count(), studentPetitionDto.Count());
                Assert.AreEqual(studentPetitionsData.ElementAt(0).StudentId, studentPetitionDto.ElementAt(0).StudentId);
                Assert.AreEqual(studentPetitionsData.ElementAt(3).ReasonCode, studentPetitionDto.ElementAt(3).ReasonCode);
                Assert.AreEqual(studentPetitionsData.ElementAt(2).ReasonCode, studentPetitionDto.ElementAt(2).ReasonCode);
            }

            private List<StudentPetition> BuildStudentPetitionsRepositoryResponse()
            {
                List<StudentPetition> petitions = new List<StudentPetition>();
                var studentPetition1 = new StudentPetition(id: "1", courseId: "ART-101", sectionId: "SEC1", studentId: "0000123", type: StudentPetitionType.StudentPetition, statusCode: "status") { ReasonCode = "OVHM" };
                petitions.Add(studentPetition1);

                var studentPetition2 = new StudentPetition(id: "2", courseId: "ART-101", sectionId: "SEC1", studentId: "0000123", type: StudentPetitionType.StudentPetition, statusCode: "status") { Comment = "Student 456 ART-101 Petition comment.", ReasonCode = null };
                petitions.Add(studentPetition2);

                var studentPetition3 = new StudentPetition(id: "3", courseId: "ART-101", sectionId: "SEC1", studentId: "0000123", type: StudentPetitionType.StudentPetition, statusCode: "status") { Comment = "Student 111 ART-101 Petition comment.", ReasonCode = string.Empty };
                petitions.Add(studentPetition3);

                var studentPetition4 = new StudentPetition(id: "4", courseId: "ART-101", sectionId: "SEC1", studentId: "0000123", type: StudentPetitionType.StudentPetition, statusCode: "status") { Comment = "Student 789 ART-101 Petition comment. Line1 \ncomment line2\ncomment line3 the end", ReasonCode = "ICHI" };
                petitions.Add(studentPetition4);

                var facultyConsent1 = new StudentPetition(id: "1", courseId: "ART-101", sectionId: "SEC1", studentId: "0000123", type: StudentPetitionType.FacultyConsent, statusCode: "status") { Comment = null, ReasonCode = "ICHI" };
                petitions.Add(facultyConsent1);

                var facultyConsent2 = new StudentPetition(id: "2", courseId: "ART-101", sectionId: "SEC1", studentId: "0000123", type: StudentPetitionType.FacultyConsent, statusCode: "status") { Comment = "Student 456 ART-101 Consent comment.", ReasonCode = null };
                petitions.Add(facultyConsent2);

                var facultyConsent4 = new StudentPetition(id: "4", courseId: "ART-101", sectionId: "SEC1", studentId: "0000123", type: StudentPetitionType.FacultyConsent, statusCode: "status") { Comment = "Student 789 ART-101 Consent comment. Line1 \ncomment line2\ncomment line3 the end", ReasonCode = "OVHM" };
                petitions.Add(facultyConsent4);

                return petitions;
            }

        }
    }
}