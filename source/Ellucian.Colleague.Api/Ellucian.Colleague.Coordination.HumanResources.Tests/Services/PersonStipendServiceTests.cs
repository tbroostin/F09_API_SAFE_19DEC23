/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PersonStipendServiceTests : HumanResourcesServiceTestsSetup
    {
        public Mock<IPersonStipendRepository> personStipendRepositoryMock;
        public Mock<ISupervisorsRepository> supervisorsRepositoryMock;
        public TestPersonStipendRepository testPersonStipendRepository;
        public Mock<IHumanResourcesReferenceDataRepository> referenceDataRepositoryMock;
        public ICurrentUserFactory employeeProxyCurrentUserFactory;

        public class EmployeeProxyUserFactory : ICurrentUserFactory
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

        public PersonStipendService actualService
        {
            get
            {
                return new PersonStipendService(
                    personStipendRepositoryMock.Object,
                    supervisorsRepositoryMock.Object,
                    adapterRegistryMock.Object,
                    employeeCurrentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }
        }

        public PersonStipendService proxyActualService
        {
            get
            {
                return new PersonStipendService(
                    personStipendRepositoryMock.Object,
                    supervisorsRepositoryMock.Object,
                    adapterRegistryMock.Object,
                    employeeProxyCurrentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }
        }

        public FunctionEqualityComparer<PersonStipend> personStipendDtoComparer;

        public void PersonPositionWageServiceTestsInitialize()
        {
            MockInitialize();

            supervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
            personStipendRepositoryMock = new Mock<IPersonStipendRepository>();
            testPersonStipendRepository = new TestPersonStipendRepository();
            employeeProxyCurrentUserFactory = new EmployeeProxyUserFactory();
            referenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();

            personStipendRepositoryMock.Setup(r => r.GetPersonStipendAsync(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>((ids) => testPersonStipendRepository.GetPersonStipendAsync(ids));

            personStipendDtoComparer = new FunctionEqualityComparer<PersonStipend>(
                (p1, p2) => p1.Id == p2.Id && p1.PersonId == p2.PersonId && p1.PositionId == p2.PositionId && p1.StartDate == p2.StartDate,
                (p) => p.Id.GetHashCode());
        }

        [TestClass]
        public class GetPersonStipendAsyncTests : PersonStipendServiceTests
        {
            public string UserForAdminPermissionCheck
            {
                get
                {
                    return "0003916";
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                PersonPositionWageServiceTestsInitialize();
            }

            [TestMethod]
            public async Task RepositoryCalledWithCurrentUserIdTest()
            {
                await actualService.GetPersonStipendAsync();
                personStipendRepositoryMock.Verify(r =>
                    r.GetPersonStipendAsync(It.Is<IEnumerable<string>>(list => list.Count() == 1 && list.ElementAt(0) == employeeCurrentUserFactory.CurrentUser.PersonId)));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RepositoryReturnsNullTest()
            {
                personStipendRepositoryMock.Setup(r => r.GetPersonStipendAsync(It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>>((ids) => Task.FromResult<IEnumerable<Domain.HumanResources.Entities.PersonStipend>>(null));

                try
                {
                    await actualService.GetPersonStipendAsync();
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = (await testPersonStipendRepository.GetPersonStipendAsync(new List<string>() { employeeCurrentUserFactory.CurrentUser.PersonId }));

                var actual = await actualService.GetPersonStipendAsync();

                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), personStipendDtoComparer);

            }

            [TestMethod]
            public async Task RepositoryCalledWithProxyIdTest()
            {
                await proxyActualService.GetPersonStipendAsync("0000001");
                personStipendRepositoryMock.Verify(r =>
                    r.GetPersonStipendAsync(It.Is<IEnumerable<string>>(list =>
                        list.Count() == 1 && list.ElementAt(0) == "0000001")));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NoAccessWithRandomIdViaProxyService()
            {
                await proxyActualService.GetPersonStipendAsync("0000003");

            }

            [TestMethod]
            public async Task PersonPoStipends_RepositoryCurrentUserIdWithAdminPermissionTest()
            {
                roleRepositoryMock.Setup(r => r.Roles)
               .Returns(() => (employeeCurrentUserFactory.CurrentUser.Roles).Select(roleTitle =>
               {
                   var role = new Domain.Entities.Role(roleTitle.GetHashCode(), roleTitle);

                   role.AddPermission(new Domain.Entities.Permission("VIEW.ALL.TIME.HISTORY"));

                   return role;
               }));

                var res = await actualService.GetPersonStipendAsync(UserForAdminPermissionCheck);
                personStipendRepositoryMock.Verify(r =>
                    r.GetPersonStipendAsync(It.Is<IEnumerable<string>>(list =>
                        list.Count() == 1 && list.ElementAt(0) == UserForAdminPermissionCheck)));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PPersonPoStipends_RepositoryCurrentUserIdWithoutAdminPermissionTest()
            {
                await actualService.GetPersonStipendAsync(UserForAdminPermissionCheck);

            }
        }
    }
}
