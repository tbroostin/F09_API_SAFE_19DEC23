// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Moq;
using slf4net;
using System;
using System.Linq;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities.TransferWork;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class TransferWorkServiceTests
    {
        // Sets up a Current user that is a student and one that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Role advisorRole = new Role(105, "Advisor");

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
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            public class AdvisorUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000111",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Advisor",
                            Roles = new List<string>() { "Advisor" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetStudentTransferWorkAsync_AsStudentUser : CurrentUserSetup
        {
            private Mock<IStudentTransferWorkRepository> transferworkRepositoryMock;
            private IStudentTransferWorkRepository transferworkRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private List<Ellucian.Colleague.Domain.Student.Entities.TransferWork.TransferEquivalencies> transferWorkResults;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private TransferWorkService transferWorkService;
            private Domain.Student.Entities.Student student;
            private Domain.Student.Entities.Student unauthorizedStudent;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                transferworkRepositoryMock = new Mock<IStudentTransferWorkRepository>();
                transferworkRepository = transferworkRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;

                // Mock student repo response
                student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                unauthorizedStudent = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepositoryMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(unauthorizedStudent);

                // Mock the StudentTransferWorkRepository response
                ExternalCourseWork externalCourseWork = new ExternalCourseWork("ENGL 100", "Engl 100", Convert.ToDecimal(3), "14", null);
                ExternalNonCourseWork externalNonCourseWork = new ExternalNonCourseWork("ENGL-101", "ENGL-101", "14", Convert.ToDecimal(3), null, "Active");
                EquivalentCoursCredit equivalentCoursCredit = new EquivalentCoursCredit("626", "ENGL-100", "The Short Story", Convert.ToDecimal(3), "14", "UG", "TR", "TR");
                EquivalentGeneralCredit equivalentGeneralCredit = new EquivalentGeneralCredit("Math-english", "Math-mathenglish", "HIST", "100", Convert.ToDecimal(3), "UG", "TR", "TR", new List<string>() { "MATH" });

                Equivalency equivalency = new Equivalency();
                equivalency.AddAcademicProgram("BUSN.BA");
                equivalency.AddExternalCourseWork(externalCourseWork);
                equivalency.AddExternalNonCourseWork(externalNonCourseWork);
                equivalency.AddEquivalentCourseCredit(equivalentCoursCredit);
                equivalency.AddEquivalentGeneralCredit(equivalentGeneralCredit);

                transferWorkResults = new List<TransferEquivalencies>()
                {
                    new TransferEquivalencies("0000129", new List<Equivalency>() { equivalency })
                };

                transferworkRepositoryMock.Setup(x => x.GetStudentTransferWorkAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TransferWork.TransferEquivalencies>>(transferWorkResults));

                // Mock Adapters - Entity to DTO Convertor
                var externalCourseWorkDtoAdapter = new AutoMapperAdapter<ExternalCourseWork, Dtos.Student.TransferWork.ExternalCourseWork>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<ExternalCourseWork, Dtos.Student.TransferWork.ExternalCourseWork>()).Returns(externalCourseWorkDtoAdapter);

                var externalNonCourseWorkDtoAdapter = new AutoMapperAdapter<ExternalNonCourseWork, Dtos.Student.TransferWork.ExternalNonCourseWork>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<ExternalNonCourseWork, Dtos.Student.TransferWork.ExternalNonCourseWork>()).Returns(externalNonCourseWorkDtoAdapter);

                var equivalentCourseCreditDtoAdapter = new AutoMapperAdapter<EquivalentCoursCredit, Dtos.Student.TransferWork.EquivalentCourseCredit>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<EquivalentCoursCredit, Dtos.Student.TransferWork.EquivalentCourseCredit>()).Returns(equivalentCourseCreditDtoAdapter);

                var equivalentGeneralCreditDtoAdapter = new AutoMapperAdapter<EquivalentGeneralCredit, Dtos.Student.TransferWork.EquivalentGeneralCredit>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<EquivalentGeneralCredit, Dtos.Student.TransferWork.EquivalentGeneralCredit>()).Returns(equivalentGeneralCreditDtoAdapter);

                var equivalencyDtoAdapter = new AutoMapperAdapter<Equivalency, Dtos.Student.TransferWork.Equivalency>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Equivalency, Dtos.Student.TransferWork.Equivalency>()).Returns(equivalencyDtoAdapter);

                var transferEquivalenciesDtoAdapter = new AutoMapperAdapter<TransferEquivalencies, Dtos.Student.TransferWork.TransferEquivalencies>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<TransferEquivalencies, Dtos.Student.TransferWork.TransferEquivalencies>()).Returns(transferEquivalenciesDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                transferWorkService = new TransferWorkService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, transferworkRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transferworkRepository = null;
                studentRepository = null;
                adapterRegistry = null;
                roleRepo = null;
                baseConfigurationRepository = null;
                referenceDataRepository = null;
                configurationRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfNotSelf()
            {
                await transferWorkService.GetStudentTransferWorkAsync("0004002");
            }

            [TestMethod]
            public async Task ReturnsAllTransferSummaryForStudent()
            {
                var result = await transferWorkService.GetStudentTransferWorkAsync("0000894");
                Assert.AreEqual(transferWorkResults.Count(), result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfRepositoryThrowsException()
            {
                transferworkRepositoryMock.Setup(x => x.GetStudentTransferWorkAsync(It.IsAny<string>())).Throws(new ArgumentNullException());
                var result = await transferWorkService.GetStudentTransferWorkAsync("0000894");
            }
        }

        [TestClass]
        public class GetStudentTransferWorkAsync_AsStudentAdvisorUser : CurrentUserSetup
        {
            private Mock<IStudentTransferWorkRepository> transferworkRepositoryMock;
            private IStudentTransferWorkRepository transferworkRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private List<Ellucian.Colleague.Domain.Student.Entities.TransferWork.TransferEquivalencies> transferWorkResults;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private TransferWorkService transferWorkService;
            private Domain.Student.Entities.Student student;
            private Domain.Student.Entities.Student unauthorizedStudent;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                transferworkRepositoryMock = new Mock<IStudentTransferWorkRepository>();
                transferworkRepository = transferworkRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;

                // Mock student repo response
                student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                unauthorizedStudent = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepositoryMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(unauthorizedStudent);

                // Mock student repo response
                student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                unauthorizedStudent = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepositoryMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(unauthorizedStudent);

                // Mock the StudentTransferWorkRepository response
                ExternalCourseWork externalCourseWork = new ExternalCourseWork("ENGL 100", "Engl 100", Convert.ToDecimal(3), "14", null);
                ExternalNonCourseWork externalNonCourseWork = new ExternalNonCourseWork("ENGL-101", "ENGL-101", "14", Convert.ToDecimal(3), null, "Active");
                EquivalentCoursCredit equivalentCoursCredit = new EquivalentCoursCredit("626", "ENGL-100", "The Short Story", Convert.ToDecimal(3), "14", "UG", "TR", "TR");
                EquivalentGeneralCredit equivalentGeneralCredit = new EquivalentGeneralCredit("Math-english", "Math-mathenglish", "HIST", "100", Convert.ToDecimal(3), "UG", "TR", "TR", new List<string>() { "MATH" });

                Equivalency equivalency = new Equivalency();
                equivalency.AddAcademicProgram("BUSN.BA");
                equivalency.AddExternalCourseWork(externalCourseWork);
                equivalency.AddExternalNonCourseWork(externalNonCourseWork);
                equivalency.AddEquivalentCourseCredit(equivalentCoursCredit);
                equivalency.AddEquivalentGeneralCredit(equivalentGeneralCredit);

                transferWorkResults = new List<TransferEquivalencies>()
                {
                    new TransferEquivalencies("0000129", new List<Equivalency>() { equivalency })
                };

                transferworkRepositoryMock.Setup(x => x.GetStudentTransferWorkAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TransferWork.TransferEquivalencies>>(transferWorkResults));

                // Mock Adapters - Entity to DTO Convertor
                var externalCourseWorkDtoAdapter = new AutoMapperAdapter<ExternalCourseWork, Dtos.Student.TransferWork.ExternalCourseWork>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<ExternalCourseWork, Dtos.Student.TransferWork.ExternalCourseWork>()).Returns(externalCourseWorkDtoAdapter);

                var externalNonCourseWorkDtoAdapter = new AutoMapperAdapter<ExternalNonCourseWork, Dtos.Student.TransferWork.ExternalNonCourseWork>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<ExternalNonCourseWork, Dtos.Student.TransferWork.ExternalNonCourseWork>()).Returns(externalNonCourseWorkDtoAdapter);

                var equivalentCourseCreditDtoAdapter = new AutoMapperAdapter<EquivalentCoursCredit, Dtos.Student.TransferWork.EquivalentCourseCredit>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<EquivalentCoursCredit, Dtos.Student.TransferWork.EquivalentCourseCredit>()).Returns(equivalentCourseCreditDtoAdapter);

                var equivalentGeneralCreditDtoAdapter = new AutoMapperAdapter<EquivalentGeneralCredit, Dtos.Student.TransferWork.EquivalentGeneralCredit>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<EquivalentGeneralCredit, Dtos.Student.TransferWork.EquivalentGeneralCredit>()).Returns(equivalentGeneralCreditDtoAdapter);

                var equivalencyDtoAdapter = new AutoMapperAdapter<Equivalency, Dtos.Student.TransferWork.Equivalency>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Equivalency, Dtos.Student.TransferWork.Equivalency>()).Returns(equivalencyDtoAdapter);

                var transferEquivalenciesDtoAdapter = new AutoMapperAdapter<TransferEquivalencies, Dtos.Student.TransferWork.TransferEquivalencies>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<TransferEquivalencies, Dtos.Student.TransferWork.TransferEquivalencies>()).Returns(transferEquivalenciesDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                transferWorkService = new TransferWorkService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, transferworkRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transferworkRepository = null;
                studentRepository = null;
                adapterRegistry = null;
                roleRepo = null;
                baseConfigurationRepository = null;
                referenceDataRepository = null;
                configurationRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorWhenAdvisorDoesNotHaveCorrectRole()
            {
                await transferWorkService.GetStudentTransferWorkAsync("0000111");
            }

            [TestMethod]
            public async Task ReturnAll_ViewAssignedAdviseesPermission()
            {
                // Arrange
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock student repo responses so that the student has this assigned advisor
                student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student.AddAdvisement("0000111", null, null, "major");
                student.AddAdvisor("0000111");
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = new StudentAccess("0000894");
                studentAccess.AddAdvisement("0000111", null, null, "major");
                List<string> ids = new List<string>() { "0000894" };
                List<StudentAccess> listStudentAccess = new List<StudentAccess>() { studentAccess };
                studentRepositoryMock.Setup(repo => repo.GetStudentAccessAsync(ids)).ReturnsAsync(listStudentAccess);

                // Act
                var result = await transferWorkService.GetStudentTransferWorkAsync("0000894");
                // Assert
                Assert.AreEqual(transferWorkResults.Count(), result.Count());
            }
        }
    }
}
