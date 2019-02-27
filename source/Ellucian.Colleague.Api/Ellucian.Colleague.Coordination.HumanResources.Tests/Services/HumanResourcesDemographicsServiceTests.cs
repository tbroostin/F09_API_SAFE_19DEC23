/*Copyright 2017-2018 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using System.Collections.Generic;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class HumanResourcesDemographicsServiceTests : HumanResourcesServiceTestsSetup
    {
        public Mock<ISupervisorsRepository> supervisorsRepositoryMock;
        public Mock<ISupervisorsRepository> proxySupervisorsRepositoryMock;
        public Mock<IPersonBaseRepository> personBaseRepositoryMock;
        public Mock<IPersonBaseRepository> proxyPersonBaseRepositoryMock;
        public Mock<IEmployeeRepository> employeeRepositoryMock;
        public Mock<IEmployeeRepository> proxyEmployeeRepositoryMock;
        public ICurrentUserFactory currentUserFactory;
        public ICurrentUserFactory proxyCurrentUserFactory;
        public ICurrentUserFactory noAccessCurrentUserFactory;
        public HumanResourceDemographicsService humanResourceDemographicsService;
        public HumanResourceDemographicsService proxyHumanResourceDemographicsService;
        public HumanResourceDemographicsService noAccessHumanResourceDemographicsService;
        public List<Dtos.HumanResources.HumanResourceDemographics> humanResourceDemographicsDtoList;
        public List<Dtos.HumanResources.HumanResourceDemographics> proxyHumanResourceDemographicsDtoList;
        public List<Domain.HumanResources.Entities.HumanResourceDemographics> humanResourceDemographicsEntityList;
        public List<Domain.HumanResources.Entities.HumanResourceDemographics> proxyHumanResourceDemographicsEntityList;
        public List<Domain.Base.Entities.PersonBase> personBaseEntityList;
        public List<Domain.Base.Entities.PersonBase> proxyPersonBaseEntityList;
        public TestSupervisorsRepository testSupervisorRepository;
        public TestSupervisorsRepository testProxySupervisorRepository;
        public TestEmployeeRepository testEmployeeRepository;
        public TestEmployeeRepository testProxyEmployeeRepository;

        public class SupervisorProxyUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Alex",
                        PersonId = "0000008",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Employee",
                        Roles = new List<string>() { "TIME MANAGEMENT SUPERVISOR" },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0000001",
                            Permissions = new List<string> { Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval.Value }
                        }
                    });
                }
            }
        }

        //Overwrite the SupervisorUserFactory for user with proxy subject claim
        public class SupervisorNoProxyUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Alex",
                        PersonId = "0000008",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Employee",
                        Roles = new List<string>() { "NO ACCESS ROLE" },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0000001"
                        }
                    });
                }
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            supervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
            proxySupervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
            personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
            proxyPersonBaseRepositoryMock = new Mock<IPersonBaseRepository>();
            employeeRepositoryMock = new Mock<IEmployeeRepository>();
            proxyEmployeeRepositoryMock = new Mock<IEmployeeRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            proxyAdapterRegistryMock = new Mock<IAdapterRegistry>();
            currentUserFactory = new SupervisorUserFactory();
            proxyCurrentUserFactory = new SupervisorProxyUserFactory();
            noAccessCurrentUserFactory = new SupervisorNoProxyUserFactory();
            roleRepositoryMock = new Mock<IRoleRepository>();
            proxyRoleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            proxyLoggerMock = new Mock<ILogger>();
            humanResourceDemographicsDtoList = new List<Dtos.HumanResources.HumanResourceDemographics>();
            proxyHumanResourceDemographicsDtoList = new List<Dtos.HumanResources.HumanResourceDemographics>();
            humanResourceDemographicsEntityList = new List<Domain.HumanResources.Entities.HumanResourceDemographics>();
            proxyHumanResourceDemographicsEntityList = new List<Domain.HumanResources.Entities.HumanResourceDemographics>();
            personBaseEntityList = new List<Domain.Base.Entities.PersonBase>();
            proxyPersonBaseEntityList = new List<Domain.Base.Entities.PersonBase>();
            testSupervisorRepository = new TestSupervisorsRepository();
            testEmployeeRepository = new TestEmployeeRepository();
            testProxySupervisorRepository = new TestSupervisorsRepository();
            testProxyEmployeeRepository = new TestEmployeeRepository();

            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            proxyRoleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            var superRole = new Domain.Entities.Role(76, "TIME MANAGEMENT SUPERVISOR");
            superRole.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewSuperviseeData));
            superRole.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewAllEarningsStatements));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { superRole });

            var proxySuperRole = new Domain.Entities.Role(76, "TIME MANAGEMENT SUPERVISOR");
            proxySuperRole.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewSuperviseeData));
            proxySuperRole.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewAllEarningsStatements));
            proxyRoleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { proxySuperRole });

            supervisorsRepositoryMock.Setup(r => r.GetSupervisorsBySuperviseeAsync(It.IsAny<string>()))
                .Returns<string>(id => testSupervisorRepository.GetSupervisorsBySuperviseeAsync(id));

            supervisorsRepositoryMock.Setup(r => r.GetSuperviseesBySupervisorAsync(It.IsAny<string>()))
                .Returns<string>(id => testSupervisorRepository.GetSuperviseesBySupervisorAsync(id));

            proxySupervisorsRepositoryMock.Setup(r => r.GetSupervisorsBySuperviseeAsync(It.IsAny<string>()))
                .Returns(testProxySupervisorRepository.GetSupervisorsBySuperviseeAsync("08"));

            proxySupervisorsRepositoryMock.Setup(r => r.GetSuperviseesBySupervisorAsync(It.IsAny<string>()))
                .Returns(testProxySupervisorRepository.GetSuperviseesBySupervisorAsync("ANDRE3000"));

            employeeRepositoryMock.Setup(r => r.GetEmployeeKeysAsync(It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(testEmployeeRepository.GetEmployeeKeysAsync());

            proxyEmployeeRepositoryMock.Setup(r => r.GetEmployeeKeysAsync(It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(testProxyEmployeeRepository.GetEmployeeKeysAsync());

            personBaseRepositoryMock.Setup(r => r.GetPersonBaseAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((id, b) => Task.FromResult(personBaseEntityList.FirstOrDefault(p => p.Id == id)));

            personBaseRepositoryMock.Setup(r => r.GetPersonsBaseAsync(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(personBaseEntityList);

            proxyPersonBaseRepositoryMock.Setup(r => r.GetPersonsBaseAsync(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(proxyPersonBaseEntityList);

            var personBaseEntityToHumanResourceDemographicsDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>()).Returns(personBaseEntityToHumanResourceDemographicsDtoAdapter);

            var proxyPersonBaseEntityToHumanResourceDemographicsDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>(proxyAdapterRegistryMock.Object, proxyLoggerMock.Object);
            proxyAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>()).Returns(proxyPersonBaseEntityToHumanResourceDemographicsDtoAdapter);

            humanResourceDemographicsService = new HumanResourceDemographicsService(
                personBaseRepositoryMock.Object,
                supervisorsRepositoryMock.Object,
                employeeRepositoryMock.Object,
                adapterRegistryMock.Object,
                currentUserFactory,
                roleRepositoryMock.Object,
                loggerMock.Object);

            //Service for proxy, the proxyCurrentUserFactory has a proxy subject claim with an id
            proxyHumanResourceDemographicsService = new HumanResourceDemographicsService(
                proxyPersonBaseRepositoryMock.Object,
                proxySupervisorsRepositoryMock.Object,
                proxyEmployeeRepositoryMock.Object,
                proxyAdapterRegistryMock.Object,
                proxyCurrentUserFactory,
                proxyRoleRepositoryMock.Object,
                proxyLoggerMock.Object
                );

            BuildTestData();
        }

        [TestCleanup]
        public void CleanUp()
        {
            humanResourceDemographicsService = null;
            proxyHumanResourceDemographicsService = null;
            humanResourceDemographicsDtoList = null;
            proxyHumanResourceDemographicsDtoList = null;
            humanResourceDemographicsEntityList = null;
            proxyHumanResourceDemographicsEntityList = null;
            supervisorsRepositoryMock = null;
            proxySupervisorsRepositoryMock = null;
            personBaseRepositoryMock = null;
            proxyPersonBaseRepositoryMock = null;
            personBaseEntityList = null;
            proxyPersonBaseEntityList = null;
        }

        [TestClass]
        public class GetSpecificHumanResourceDemographicsTests : HumanResourcesDemographicsServiceTests
        {
            [TestMethod]
            public async Task ReturnsCorrectDataTest()
            {
                string inputPersonId = currentUserFactory.CurrentUser.PersonId;
                var personRec = personBaseEntityList[0];
                personBaseRepositoryMock.Setup(repo => repo.GetPersonBaseAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(personRec));
                var actuals = await humanResourceDemographicsService.GetSpecificHumanResourceDemographicsAsync(inputPersonId);
                Assert.AreEqual(personRec.LastName, actuals.LastName);
                Assert.AreEqual(personRec.FirstName, actuals.FirstName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullIdReturnsExceptionTest()
            {
                string inputPersonId = null;
                var actuals = await humanResourceDemographicsService.GetSpecificHumanResourceDemographicsAsync(inputPersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CannotAccessOtherResourcesWithoutPermissionTest()
            {
                string inputPersonId = "foo";
                roleRepositoryMock.Setup(r => r.Roles).Returns(() => new List<Domain.Entities.Role>());
                await humanResourceDemographicsService.GetSpecificHumanResourceDemographicsAsync(inputPersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CannotAccessOtherResourcesIfNotSupervisorOfResourceTest()
            {
                string inputPersonId = "foo";
                supervisorsRepositoryMock.Setup(r => r.GetSuperviseesBySupervisorAsync(It.IsAny<string>()))
                    .Returns<string>((supervisorId) => Task.FromResult<IEnumerable<string>>(new List<string>() { "bar", "notFoo" }));
                await humanResourceDemographicsService.GetSpecificHumanResourceDemographicsAsync(inputPersonId);
            }

            [TestMethod]
            public async Task CanAccessOtherResourcesIfHasPermissionAndSupervisorOfResourceTest()
            {
                string inputPersonId = "foo";
                supervisorsRepositoryMock.Setup(r => r.GetSuperviseesBySupervisorAsync(It.IsAny<string>()))
                    .Returns<string>((supervisorId) => Task.FromResult<IEnumerable<string>>(new List<string>() { inputPersonId, "bar" }));
                personBaseRepositoryMock.Setup(r => r.GetPersonBaseAsync(inputPersonId, true))
                    .Returns<string, bool>((id, b) => Task.FromResult(new Domain.Base.Entities.PersonBase(inputPersonId, "lastName")));
                var result = await humanResourceDemographicsService.GetSpecificHumanResourceDemographicsAsync(inputPersonId);

                Assert.AreEqual(inputPersonId, result.Id);
            }
        }

        [TestClass]
        public class GetHumanResourceDemographicsAsyncTests : HumanResourcesDemographicsServiceTests
        {
            [TestMethod]
            public async Task ReturnsCorrectDataTest()
            {
                var personRec = personBaseEntityList[0];
                personBaseRepositoryMock.Setup(repo => repo.GetPersonsBaseAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()))
                    .Returns(Task.FromResult(personBaseEntityList.AsEnumerable()));
                var actuals = await humanResourceDemographicsService.GetHumanResourceDemographicsAsync();
                Assert.AreEqual(personRec.LastName, actuals.FirstOrDefault(a => a.Id == "0003914").LastName);
            }

            [TestMethod]
            public async Task ReturnsCorrectProxyDataTest()
            {
                //User has proxy subject claim to this user id
                var personRec = proxyPersonBaseEntityList[0];
                proxyPersonBaseRepositoryMock.Setup(repo => repo.GetPersonsBaseAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()))
                    .Returns(Task.FromResult(proxyPersonBaseEntityList.AsEnumerable()));
                //Will have to set current user, but then pass proxy id
                var actuals = await proxyHumanResourceDemographicsService.GetHumanResourceDemographicsAsync("0000001");
                Assert.AreEqual(personRec.Id, actuals.FirstOrDefault(a => a.Id == "0000001").Id);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NonProxyableIdReturnsExceptionTest()
            {
                //User has proxy subject claim, but not to this user id
                var actuals = await proxyHumanResourceDemographicsService.GetHumanResourceDemographicsAsync("0000002");
            }
        }


        [TestClass]
        public class QueryHumanResourceDemographicsAsyncTests
        {

        }
        public void BuildTestData()
        {
            personBaseEntityList.Add(new Domain.Base.Entities.PersonBase(currentUserFactory.CurrentUser.PersonId, "Wallace")
            {
                FirstName = "Will",
                MiddleName = "G",
                PreferredName = "Willy"
            });

            proxyPersonBaseEntityList.Add(new Domain.Base.Entities.PersonBase("0000001", "Jessica"));
            proxyPersonBaseEntityList.Add(new Domain.Base.Entities.PersonBase("0000002", "Billy"));

            humanResourceDemographicsEntityList.Add(new Domain.HumanResources.Entities.HumanResourceDemographics("45", "first name45", "last name45", "my name45"));
            foreach (var entity in personBaseEntityList)
            {
                humanResourceDemographicsDtoList.Add(new Dtos.HumanResources.HumanResourceDemographics()
                {
                    Id = entity.Id,
                    FirstName = entity.FirstName,
                    LastName = entity.LastName,
                    PreferredName = entity.PreferredName
                });

                humanResourceDemographicsEntityList.Add(new Domain.HumanResources.Entities.HumanResourceDemographics(entity.Id, entity.FirstName, entity.LastName, entity.PreferredName));
            }

            proxyHumanResourceDemographicsEntityList.Add(new Domain.HumanResources.Entities.HumanResourceDemographics("08", "first name08", "last name08", "my name08"));
            foreach (var entity in proxyPersonBaseEntityList)
            {
                proxyHumanResourceDemographicsDtoList.Add(new Dtos.HumanResources.HumanResourceDemographics()
                {
                    Id = entity.Id,
                    FirstName = entity.FirstName,
                    LastName = entity.LastName,
                    PreferredName = entity.PreferredName
                });

                proxyHumanResourceDemographicsEntityList.Add(new Domain.HumanResources.Entities.HumanResourceDemographics(entity.Id, entity.FirstName, entity.LastName, entity.PreferredName));
            }
        }
    }
}
