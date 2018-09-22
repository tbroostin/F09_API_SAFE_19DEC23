// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class RegistrationOptionsServiceTests
    {
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
        public class RegistrationOptionsService_GetRegistrationOptions:CurrentUserSetup
        {
            private RegistrationOptionsService registrationOptionsService;
            private Mock<IRegistrationOptionsRepository> registrationOptionsRepositoryMock;
            private IRegistrationOptionsRepository registrationOptionsRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private ILogger logger;
            private AutoMapperAdapter<Domain.Student.Entities.RegistrationOptions, Dtos.Student.RegistrationOptions> adapter;

            public Domain.Student.Entities.RegistrationOptions CreateNewOptions(string id)
            {
                return new Domain.Student.Entities.RegistrationOptions(id, new List<Domain.Student.Entities.GradingType>() { Domain.Student.Entities.GradingType.Graded });
            }

            [TestInitialize]
            public void Initialize()
            {
                registrationOptionsRepositoryMock = new Mock<IRegistrationOptionsRepository>();
                registrationOptionsRepository = registrationOptionsRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;

                // setup a current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Mock the adapter registry
                adapter = new AutoMapperAdapter<Domain.Student.Entities.RegistrationOptions, Dtos.Student.RegistrationOptions>(adapterRegistry, logger);
                adapterRegistryMock.Setup(adapterReg => adapterReg.GetAdapter<Domain.Student.Entities.RegistrationOptions, Dtos.Student.RegistrationOptions>()).Returns(adapter);

                // Mock up repository response
                registrationOptionsRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>())).Returns((List<string> ids) =>Task.FromResult( new List<Domain.Student.Entities.RegistrationOptions>() { CreateNewOptions(ids.ElementAt(0)) }.AsEnumerable()));

                registrationOptionsService = new RegistrationOptionsService(adapterRegistry, registrationOptionsRepository, currentUserFactory, roleRepo,studentRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                registrationOptionsRepository = null;
                registrationOptionsService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RegistrationOptionsService_GetRegistrationOptions_ByAdvisor_WithNoPermission()
            {
                var id = "12345";
                var results = await registrationOptionsService.GetRegistrationOptionsAsync(id);
                var expected = CreateNewOptions(id);
                var expectedDto = adapter.MapToType(expected);
                Assert.AreEqual(expectedDto.PersonId, results.PersonId);
                Assert.AreEqual(expectedDto.GradingTypes.Count(), results.GradingTypes.Count());
            }

            [TestMethod]
            public async Task RegistrationOptionsService_GetRegistrationOptions_ByAdvisor_WithViewAnyAdvisees()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var id = "12345";
                var results = await registrationOptionsService.GetRegistrationOptionsAsync(id);
                var expected = CreateNewOptions(id);
                var expectedDto = adapter.MapToType(expected);
                Assert.AreEqual(expectedDto.PersonId, results.PersonId);
                Assert.AreEqual(expectedDto.GradingTypes.Count(), results.GradingTypes.Count());
            }

            [TestMethod]
            public async Task RegistrationOptionsService_GetRegistrationOptions_ByAdvisor_ViewAssignedAdvisees()
            {
                Domain.Student.Entities.Student student1;

                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                var student1Access = student1.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<StudentAccess>() { student1Access }.AsEnumerable());
                var id = "0000894";
                var results = await registrationOptionsService.GetRegistrationOptionsAsync(id);
                var expected = CreateNewOptions(id);
                var expectedDto = adapter.MapToType(expected);
                Assert.AreEqual(expectedDto.PersonId, results.PersonId);
                Assert.AreEqual(expectedDto.GradingTypes.Count(), results.GradingTypes.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RegistrationOptionsService_GetRegistrationOptions_ArgumentNullException()
            {
                var results = await registrationOptionsService.GetRegistrationOptionsAsync(null);
            }
        }

        [TestClass]
        public class RegistrationOptionsService_GetRegistrationOptions_AsaStudent:CurrentUserSetup
        {
            private RegistrationOptionsService registrationOptionsService;
            private Mock<IRegistrationOptionsRepository> registrationOptionsRepositoryMock;
            private IRegistrationOptionsRepository registrationOptionsRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private ILogger logger;
            private AutoMapperAdapter<Domain.Student.Entities.RegistrationOptions, Dtos.Student.RegistrationOptions> adapter;

            public Domain.Student.Entities.RegistrationOptions CreateNewOptions(string id)
            {
                return new Domain.Student.Entities.RegistrationOptions(id, new List<Domain.Student.Entities.GradingType>() { Domain.Student.Entities.GradingType.Graded });
            }

            [TestInitialize]
            public void Initialize()
            {
                registrationOptionsRepositoryMock = new Mock<IRegistrationOptionsRepository>();
                registrationOptionsRepository = registrationOptionsRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;

                // setup a current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                // Mock the adapter registry
                adapter = new AutoMapperAdapter<Domain.Student.Entities.RegistrationOptions, Dtos.Student.RegistrationOptions>(adapterRegistry, logger);
                adapterRegistryMock.Setup(adapterReg => adapterReg.GetAdapter<Domain.Student.Entities.RegistrationOptions, Dtos.Student.RegistrationOptions>()).Returns(adapter);

                // Mock up repository response
                registrationOptionsRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>())).Returns((List<string> ids) => Task.FromResult(new List<Domain.Student.Entities.RegistrationOptions>() { CreateNewOptions(ids.ElementAt(0)) }.AsEnumerable()));

                registrationOptionsService = new RegistrationOptionsService(adapterRegistry, registrationOptionsRepository, currentUserFactory, roleRepo, studentRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                registrationOptionsRepository = null;
                registrationOptionsService = null;
            }

           
            [TestMethod]
            public async Task GetRegistrationOptions_BySelfStudent_Success()
            {
                var id = "0000894";
                var results = await registrationOptionsService.GetRegistrationOptionsAsync(id);
                var expected = CreateNewOptions(id);
                var expectedDto = adapter.MapToType(expected);
                Assert.AreEqual(expectedDto.PersonId, results.PersonId);
                Assert.AreEqual(expectedDto.GradingTypes.Count(), results.GradingTypes.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetRegistrationOptions_ByNotSelfStudent()
            {
                var id = "0000111";
                var results = await registrationOptionsService.GetRegistrationOptionsAsync(id);
                var expected = CreateNewOptions(id);
                var expectedDto = adapter.MapToType(expected);
                Assert.AreEqual(expectedDto.PersonId, results.PersonId);
                Assert.AreEqual(expectedDto.GradingTypes.Count(), results.GradingTypes.Count());
            }

           
        }
    }
}
