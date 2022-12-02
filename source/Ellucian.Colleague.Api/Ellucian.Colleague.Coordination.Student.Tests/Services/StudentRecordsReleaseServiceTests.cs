// Copyright 2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentRecordsReleaseServiceTests : CurrentUserSetup
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Student");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetStudentRecordsReleaseInformation : StudentRecordsReleaseServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IStudentRecordsReleaseService studentRecordsReleaseService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository _personBaseRepository;

            private Mock<IStudentRecordsReleaseRepository> studentRecordsReleaseRepositoryMock;
            private IStudentRecordsReleaseRepository studentRecordsReleaseRepository;

            private List<StudentRecordsReleaseInfo> studentRecordsReleaseInfoData;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                _personBaseRepository = personBaseRepoMock.Object;

                studentRecordsReleaseRepositoryMock = new Mock<IStudentRecordsReleaseRepository>();
                studentRecordsReleaseRepository = studentRecordsReleaseRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Mock StudentRecordsReleaseInfo response
                studentRecordsReleaseInfoData = BuildStudentRecordsReleaseInfoResponse();
                studentRecordsReleaseRepositoryMock.Setup(repository => repository.GetStudentRecordsReleaseInfoAsync(It.IsAny<string>())).Returns(Task.FromResult(studentRecordsReleaseInfoData.AsEnumerable()));

                // Mock Adapters
                var studentRecordsReleaseInfoDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo>()).Returns(studentRecordsReleaseInfoDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                personBaseRepoMock.Setup(repo => repo.IsStudentAsync("0000015")).ReturnsAsync(true);

                studentRecordsReleaseService = new StudentRecordsReleaseService(studentRecordsReleaseRepository, studentRepository, _personBaseRepository, configurationRepository, adapterRegistry, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                studentRecordsReleaseService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                studentRecordsReleaseRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRecordsReleaseInfoAsync_ThrowsExceptionIfStudentStringNull()
            {
                studentRecordsReleaseRepositoryMock.Setup(repository => repository.GetStudentRecordsReleaseInfoAsync(It.IsAny<string>())).Throws(new PermissionsException());
                var studentRecordsReleaseInfoDto = await studentRecordsReleaseService.GetStudentRecordsReleaseInformationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRecordsReleaseInfoAsync_ThrowsExceptionIfStudentStringIsEmpty()
            {
                studentRecordsReleaseRepositoryMock.Setup(repository => repository.GetStudentRecordsReleaseInfoAsync(It.IsAny<string>())).Throws(new PermissionsException());
                var studentRecordsReleaseInfoDto = await studentRecordsReleaseService.GetStudentRecordsReleaseInformationAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentRecordsReleaseInfoAsync_RethrowsExceptionFromRepository()
            {
                studentRecordsReleaseRepositoryMock.Setup(repository => repository.GetStudentRecordsReleaseInfoAsync(It.IsAny<string>())).Throws(new Exception());
                var studentRecordsReleaseInfoDto = await studentRecordsReleaseService.GetStudentRecordsReleaseInformationAsync("0000015");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentRecordsReleaseInfoAsync_ThrowsExceptionIfCurrentUserIsNotStudent()
            {
                var studentRecordsReleaseInfoDto = await studentRecordsReleaseService.GetStudentRecordsReleaseInformationAsync("0000011");
            }


            [TestMethod]
            public async Task GetStudentRecordsReleaseInfoAsync_ReturnsStudentRecordsReleaseInfo()
            {
                studentRecordsReleaseRepositoryMock.Setup(repository => repository.GetStudentRecordsReleaseInfoAsync("0000015")).Returns(Task.FromResult(studentRecordsReleaseInfoData.AsEnumerable()));

                var studentRecordsReleaseInfoDto = await studentRecordsReleaseService.GetStudentRecordsReleaseInformationAsync("0000015");

                // Spot-check resulting dto
                Assert.AreEqual(studentRecordsReleaseInfoData.Count(), studentRecordsReleaseInfoData.Count());
                Assert.AreEqual(studentRecordsReleaseInfoData.ElementAt(0).StudentId, studentRecordsReleaseInfoDto.ElementAt(0).StudentId);
                Assert.AreEqual(studentRecordsReleaseInfoData.ElementAt(0).PIN, studentRecordsReleaseInfoDto.ElementAt(0).PIN);
                Assert.AreEqual(studentRecordsReleaseInfoData.ElementAt(1).RelationType, studentRecordsReleaseInfoDto.ElementAt(1).RelationType);
                Assert.AreEqual(studentRecordsReleaseInfoData.ElementAt(1).StartDate, studentRecordsReleaseInfoDto.ElementAt(1).StartDate);
                CollectionAssert.AreEqual(studentRecordsReleaseInfoData.ElementAt(1).AccessAreas, studentRecordsReleaseInfoDto.ElementAt(1).AccessAreas);
            } 

            private List<StudentRecordsReleaseInfo> BuildStudentRecordsReleaseInfoResponse()
            {
                List<StudentRecordsReleaseInfo> studentsRecordsReleaseInfo = new List<StudentRecordsReleaseInfo>();
                var studentsRecordsReleaseInfo1 = new StudentRecordsReleaseInfo
                {
                    Id = "1",
                    StudentId = "0000015",
                    FirstName = "John",
                    LastName = "Peter",
                    PIN = "9999",
                    RelationType = "Father",
                    AccessAreas = new List<string>() { "GRD", "PHONE" },
                    StartDate = new DateTime(2022, 05, 10),
                    EndDate = new DateTime(2022, 05, 20),
                };
                studentsRecordsReleaseInfo.Add(studentsRecordsReleaseInfo1);

                var studentsRecordsReleaseInfo2 = new StudentRecordsReleaseInfo
                {
                    Id = "2",
                    StudentId = "0000015",
                    FirstName = "John",
                    LastName = "Peter",
                    PIN = "9888",
                    RelationType = "Father",
                    AccessAreas = new List<string>() { "ADR", "PHONE" },
                    StartDate = new DateTime(2022, 05, 20),
                    EndDate = new DateTime(2022, 05, 25),

                };
                studentsRecordsReleaseInfo.Add(studentsRecordsReleaseInfo2);

                return studentsRecordsReleaseInfo;
            }
        }
        [TestClass]
        public class GetStudentRecordsReleaseDenyAccess : StudentRecordsReleaseServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IStudentRecordsReleaseService studentRecordsReleaseService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository _personBaseRepository;

            private Mock<IStudentRecordsReleaseRepository> studentRecordsReleaseRepositoryMock;
            private IStudentRecordsReleaseRepository studentRecordsReleaseRepository;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                _personBaseRepository = personBaseRepoMock.Object;

                studentRecordsReleaseRepositoryMock = new Mock<IStudentRecordsReleaseRepository>();
                studentRecordsReleaseRepository = studentRecordsReleaseRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Mock StudentRecordsReleaseDenyAccess response
                var studentRecordsReleaseDenyAccessData = new StudentRecordsReleaseDenyAccess()
                {
                    DenyAccessToAll = true
                };

                studentRecordsReleaseRepositoryMock.Setup(repository => repository.GetStudentRecordsReleaseDenyAccessAsync(It.IsAny<string>())).Returns(Task.FromResult(studentRecordsReleaseDenyAccessData));

                // Mock Adapters
                var studentRecordsReleaseDenyAccessDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseDenyAccess, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseDenyAccess>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseDenyAccess, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseDenyAccess>()).Returns(studentRecordsReleaseDenyAccessDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                personBaseRepoMock.Setup(repo => repo.IsStudentAsync("0000015")).ReturnsAsync(true);

                studentRecordsReleaseService = new StudentRecordsReleaseService(studentRecordsReleaseRepository, studentRepository, _personBaseRepository, configurationRepository, adapterRegistry, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                studentRecordsReleaseService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                studentRecordsReleaseRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRecordsReleaseDenyAccessAsync_ThrowsExceptionIfStudentStringNull()
            {
                studentRecordsReleaseRepositoryMock.Setup(repository => repository.GetStudentRecordsReleaseDenyAccessAsync(It.IsAny<string>())).Throws(new PermissionsException());
                var studentRecordsReleaseDenyAccessDto = await studentRecordsReleaseService.GetStudentRecordsReleaseDenyAccessAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRecordsReleaseDenyAccessAsync_ThrowsExceptionIfStudentStringIsEmpty()
            {
                studentRecordsReleaseRepositoryMock.Setup(repository => repository.GetStudentRecordsReleaseDenyAccessAsync(It.IsAny<string>())).Throws(new PermissionsException());
                var studentRecordsReleaseDenyAccessDto = await studentRecordsReleaseService.GetStudentRecordsReleaseDenyAccessAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentRecordsReleaseDenyAccessAsync_RethrowsExceptionFromRepository()
            {
                studentRecordsReleaseRepositoryMock.Setup(repository => repository.GetStudentRecordsReleaseDenyAccessAsync(It.IsAny<string>())).Throws(new Exception());
                var studentRecordsReleaseDenyAccessDto = await studentRecordsReleaseService.GetStudentRecordsReleaseDenyAccessAsync("0000015");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentRecordsReleaseDenyAccessAsync_ThrowsExceptionIfCurrentUserIsNotStudent()
            {
                var studentRecordsReleaseDenyAccessDto = await studentRecordsReleaseService.GetStudentRecordsReleaseDenyAccessAsync("0000011");
            }


            [TestMethod]
            public async Task GetStudentRecordsReleaseDenyAccessAsync_ReturnsStudentRecordsReleaseDenyAccess()
            {
                var studentRecordsReleaseDenyAccessData = new StudentRecordsReleaseDenyAccess()
                {
                    DenyAccessToAll = true
                };

                studentRecordsReleaseRepositoryMock.Setup(repository => repository.GetStudentRecordsReleaseDenyAccessAsync("0000015")).Returns(Task.FromResult(studentRecordsReleaseDenyAccessData));

                var studentRecordsReleaseDenyAccessDto = await studentRecordsReleaseService.GetStudentRecordsReleaseDenyAccessAsync("0000015");

                // check resulting dto
                Assert.AreEqual(studentRecordsReleaseDenyAccessData.DenyAccessToAll, studentRecordsReleaseDenyAccessDto.DenyAccessToAll);
            }

        }
        [TestClass]
        public class AddStudentRecordsRelease : StudentRecordsReleaseServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IStudentRecordsReleaseService studentRecordsReleaseService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository _personBaseRepository;

            private Mock<IStudentRecordsReleaseRepository> studentRecordsReleaseRepositoryMock;
            private IStudentRecordsReleaseRepository studentRecordsReleaseRepository;

            private Dtos.Student.StudentRecordsReleaseInfo addStudentRecordsReleaseInfo;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                _personBaseRepository = personBaseRepoMock.Object;

                studentRecordsReleaseRepositoryMock = new Mock<IStudentRecordsReleaseRepository>();
                studentRecordsReleaseRepository = studentRecordsReleaseRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                AddStudentReleaseRecordResponse createtranResponse = new AddStudentReleaseRecordResponse()
                {
                    OutStudentRecordsReleaseId = "31",
                    OutErrorMessages = new List<string>(),
                    OutError = "0"
                };

                Domain.Student.Entities.StudentRecordsReleaseInfo domainObject = new StudentRecordsReleaseInfo()
                {
                    Id = "31",
                    StudentId = "0000015",
                    FirstName = "First",
                    LastName = "Last",
                    PIN = "1111",
                    RelationType = "Mother",
                    AccessAreas = new List<string>() { "GRADE", "PHONE" }
                };

                studentRecordsReleaseRepositoryMock.Setup(repo => repo.AddStudentRecordsReleaseInfoAsync(It.IsAny<StudentRecordsReleaseInfo>())).ReturnsAsync(domainObject);

                // Mock Adapters
                var StudentRecordsReleaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo, Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo>(adapterRegistry,logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo, Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo>()).Returns(StudentRecordsReleaseDtoAdapter);

                var responseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo>()).Returns(responseDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                personBaseRepoMock.Setup(repo => repo.IsStudentAsync("0000015")).ReturnsAsync(true);

                studentRecordsReleaseService = new StudentRecordsReleaseService(studentRecordsReleaseRepository,studentRepository,_personBaseRepository,configurationRepository,adapterRegistry,currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                studentRecordsReleaseService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                studentRecordsReleaseRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddStudentRecordsRelease_NullArgument()
            {
                var serviceResult = await studentRecordsReleaseService.AddStudentRecordsReleaseInfoAsync(null);
            }

            [TestMethod]
            public async Task AddStudentRecordsRelease_Success()
            {
                addStudentRecordsReleaseInfo = new Dtos.Student.StudentRecordsReleaseInfo()
                {
                    StudentId = "0000015",
                    FirstName = "First",
                    LastName = "Last",
                    PIN = "1111",
                    RelationType = "Mother",
                    AccessAreas = new List<string>(){ "GRADE", "PHONE" }
                };



                var createResponse = await studentRecordsReleaseService.AddStudentRecordsReleaseInfoAsync(addStudentRecordsReleaseInfo);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.Id);
                Assert.AreEqual("0000015", createResponse.StudentId);
                Assert.AreEqual("First", createResponse.FirstName);
                Assert.AreEqual("Last", createResponse.LastName);
                Assert.AreEqual("Mother", createResponse.RelationType);
                Assert.AreEqual("1111", createResponse.PIN);
                Assert.AreEqual(2, createResponse.AccessAreas.Count());
            }
        }

        [TestClass]
        public class EndStudentRecordsRelease : StudentRecordsReleaseServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IStudentRecordsReleaseService studentRecordsReleaseService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository _personBaseRepository;

            private Mock<IStudentRecordsReleaseRepository> studentRecordsReleaseRepositoryMock;
            private IStudentRecordsReleaseRepository studentRecordsReleaseRepository;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                _personBaseRepository = personBaseRepoMock.Object;

                studentRecordsReleaseRepositoryMock = new Mock<IStudentRecordsReleaseRepository>();
                studentRecordsReleaseRepository = studentRecordsReleaseRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                Domain.Student.Entities.StudentRecordsReleaseInfo domainObject = new StudentRecordsReleaseInfo()
                {
                    Id = "31",
                    StudentId = "0000015",
                    FirstName = "First",
                    LastName = "Last",
                    PIN = "1111",
                    RelationType = "Mother",
                    AccessAreas = new List<string>() { "GRADE", "PHONE" },
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now
                };

                studentRecordsReleaseRepositoryMock.Setup(repo => repo.GetStudentRecordsReleaseInfoByIdAsync(It.IsAny<string>())).ReturnsAsync(domainObject);                
                studentRecordsReleaseRepositoryMock.Setup(repo => repo.GetStudentRecordsReleaseInfoAsync(It.IsAny<string>())).ReturnsAsync(new List<StudentRecordsReleaseInfo> { domainObject });
                studentRecordsReleaseRepositoryMock.Setup(repo => repo.DeleteStudentRecordsReleaseInfoAsync(It.IsAny<string>())).ReturnsAsync(domainObject);

                // Mock Adapters
                var StudentRecordsReleaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo, Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo, Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo>()).Returns(StudentRecordsReleaseDtoAdapter);

                var responseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo>()).Returns(responseDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                personBaseRepoMock.Setup(repo => repo.IsStudentAsync("0000015")).ReturnsAsync(true);

                studentRecordsReleaseService = new StudentRecordsReleaseService(studentRecordsReleaseRepository, studentRepository, _personBaseRepository, configurationRepository, adapterRegistry, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                studentRecordsReleaseService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                studentRecordsReleaseRepository = null;
            }

            [TestMethod]
            public async Task EndStudentRecordsRelease_Success()
            {
                var studentRecordsReleaseInfo = new Dtos.Student.StudentRecordsReleaseInfo()
                {
                    StudentId = "0000015",
                    FirstName = "First",
                    LastName = "Last",
                    PIN = "1111",
                    RelationType = "Mother",
                    AccessAreas = new List<string>() { "GRADE", "PHONE" },
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now
                };

                var deleteResponse = await studentRecordsReleaseService.DeleteStudentRecordsReleaseInfoAsync("0000015", "31");
                Assert.IsNotNull(deleteResponse);
                Assert.AreEqual("31", deleteResponse.Id);
                Assert.AreEqual("0000015", deleteResponse.StudentId);
                Assert.AreEqual("First", deleteResponse.FirstName);
                Assert.AreEqual("Last", deleteResponse.LastName);
                Assert.AreEqual("Mother", deleteResponse.RelationType);
                Assert.AreEqual("1111", deleteResponse.PIN);
                Assert.AreEqual(2, deleteResponse.AccessAreas.Count());
                Assert.AreEqual(DateTime.Now.Date, deleteResponse.StartDate.Value.Date);
                Assert.AreEqual(DateTime.Now.Date, deleteResponse.EndDate.Value.Date);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EndStudentRecordsRelease_NullParameter()
            {
                var deleteResponse = await studentRecordsReleaseService.DeleteStudentRecordsReleaseInfoAsync(null, null);
            }
        }

        [TestClass]
        public class DenyStudentRecordsReleaseAccessAsync : StudentRecordsReleaseServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IStudentRecordsReleaseService studentRecordsReleaseService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository _personBaseRepository;

            private Mock<IStudentRecordsReleaseRepository> studentRecordsReleaseRepositoryMock;
            private IStudentRecordsReleaseRepository studentRecordsReleaseRepository;

            private Dtos.Student.DenyStudentRecordsReleaseAccessInformation denyStudentRecordsReleaseAccessInfo;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                _personBaseRepository = personBaseRepoMock.Object;

                studentRecordsReleaseRepositoryMock = new Mock<IStudentRecordsReleaseRepository>();
                studentRecordsReleaseRepository = studentRecordsReleaseRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;
               
                StudentRecordsReleaseDenyAccessAllResponse denyAccessAllTranResponse = new StudentRecordsReleaseDenyAccessAllResponse()
                {

                    OutError = "0",
                    OutErrorMessages = new List<string>()
                };
                List<StudentRecordsReleaseInfo> updatedStudentsRecordsReleaseInfo = new List<StudentRecordsReleaseInfo>();
                var  domainObject = new StudentRecordsReleaseInfo()
                {
                    Id = "31",
                    StudentId = "0000015",
                    FirstName = "First",
                    LastName = "Last",
                    PIN = "1111",
                    RelationType = "Mother",
                    AccessAreas = new List<string>() { "GRADE", "PHONE" },
                    StartDate = new DateTime(2022, 05, 10),
                    EndDate = DateTime.Today
                };
                updatedStudentsRecordsReleaseInfo.Add(domainObject);

                DenyStudentRecordsReleaseAccessInformation denyStudentRecordsReleaseAccessInfo = new DenyStudentRecordsReleaseAccessInformation()
                {
                    DenyAccessToAll = true,
                    StudentId = "0000015"
                };
                studentRecordsReleaseRepositoryMock.Setup(repo => repo.DenyStudentRecordsReleaseAccessAsync(It.IsAny<DenyStudentRecordsReleaseAccessInformation>())).ReturnsAsync(updatedStudentsRecordsReleaseInfo);

                // Mock Adapters
                var StudentRecordsReleaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo, Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo, Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo>()).Returns(StudentRecordsReleaseDtoAdapter);

                var DenyStudentRecordsReleaseAccessInfoDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.DenyStudentRecordsReleaseAccessInformation, Ellucian.Colleague.Domain.Student.Entities.DenyStudentRecordsReleaseAccessInformation>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.DenyStudentRecordsReleaseAccessInformation, Ellucian.Colleague.Domain.Student.Entities.DenyStudentRecordsReleaseAccessInformation>()).Returns(DenyStudentRecordsReleaseAccessInfoDtoAdapter);

                var responseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo>()).Returns(responseDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                personBaseRepoMock.Setup(repo => repo.IsStudentAsync("0000015")).ReturnsAsync(true);

                studentRecordsReleaseService = new StudentRecordsReleaseService(studentRecordsReleaseRepository, studentRepository, _personBaseRepository, configurationRepository, adapterRegistry, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                studentRecordsReleaseService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                studentRecordsReleaseRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task DenyStudentRecordsReleaseAccessAsync_NullArgument()
            {
                var serviceResult = await studentRecordsReleaseService.DenyStudentRecordsReleaseAccessAsync(null);
            }

            [TestMethod]
            public async Task DenyStudentRecordsReleaseAccessAsync_Success()
            {
                List<Dtos.Student.StudentRecordsReleaseInfo> updatedStudentsRecordsReleaseInfo = new List<Dtos.Student.StudentRecordsReleaseInfo>();
                var domainObject = new Dtos.Student.StudentRecordsReleaseInfo()
                {
                    Id = "31",
                    StudentId = "0000015",
                    FirstName = "First",
                    LastName = "Last",
                    PIN = "1111",
                    RelationType = "Mother",
                    AccessAreas = new List<string>() { "GRADE", "PHONE" },
                    StartDate = new DateTime(2022, 05, 10),
                    EndDate = DateTime.Today
                };
                updatedStudentsRecordsReleaseInfo.Add(domainObject);

                denyStudentRecordsReleaseAccessInfo = new Dtos.Student.DenyStudentRecordsReleaseAccessInformation()
                {
                    DenyAccessToAll = true,
                    StudentId = "0000015"
                };

                var denyAccessAllResponse = await studentRecordsReleaseService.DenyStudentRecordsReleaseAccessAsync(denyStudentRecordsReleaseAccessInfo);
                Assert.IsNotNull(denyAccessAllResponse);
                Assert.AreEqual(updatedStudentsRecordsReleaseInfo.ElementAt(0).Id, denyAccessAllResponse.ElementAt(0).Id);
                Assert.AreEqual(updatedStudentsRecordsReleaseInfo.ElementAt(0).StudentId, denyAccessAllResponse.ElementAt(0).StudentId);
                Assert.AreEqual(updatedStudentsRecordsReleaseInfo.ElementAt(0).FirstName, denyAccessAllResponse.ElementAt(0).FirstName);
                Assert.AreEqual(updatedStudentsRecordsReleaseInfo.ElementAt(0).LastName, denyAccessAllResponse.ElementAt(0).LastName);
                Assert.AreEqual(updatedStudentsRecordsReleaseInfo.ElementAt(0).PIN, denyAccessAllResponse.ElementAt(0).PIN);
                Assert.AreEqual(updatedStudentsRecordsReleaseInfo.ElementAt(0).RelationType, denyAccessAllResponse.ElementAt(0).RelationType);
                CollectionAssert.AreEqual(updatedStudentsRecordsReleaseInfo.ElementAt(0).AccessAreas, denyAccessAllResponse.ElementAt(0).AccessAreas);
                Assert.AreEqual(updatedStudentsRecordsReleaseInfo.ElementAt(0).StartDate, denyAccessAllResponse.ElementAt(0).StartDate);
                Assert.AreEqual(updatedStudentsRecordsReleaseInfo.ElementAt(0).EndDate, denyAccessAllResponse.ElementAt(0).EndDate);
            }
        }

    }
}
