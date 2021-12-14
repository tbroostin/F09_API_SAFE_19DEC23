/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
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
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PersonPositionServiceTests : HumanResourcesServiceTestsSetup
    {
        public Mock<IPersonPositionRepository> personPositionRepositoryMock;

        public Mock<ISupervisorsRepository> supervisorsRepositoryMock;

        public Mock<IPositionRepository> positionRepositoryMock;

        public TestPersonPositionRepository testPersonPositionRepository;

        public ITypeAdapter<Domain.HumanResources.Entities.PersonPosition, Dtos.HumanResources.PersonPosition> personPositionEntityToDtoAdapter;

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

        public PersonPositionService actualService
        {
            get
            {
                return new PersonPositionService(
                    personPositionRepositoryMock.Object,
                    supervisorsRepositoryMock.Object,
                    positionRepositoryMock.Object,
                    adapterRegistryMock.Object,
                    employeeCurrentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }
        }

        public PersonPositionService proxyActualService
        {
            get
            {
                return new PersonPositionService(
                    personPositionRepositoryMock.Object,
                    supervisorsRepositoryMock.Object,
                    positionRepositoryMock.Object,
                    adapterRegistryMock.Object,
                    employeeProxyCurrentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }
        }

        public FunctionEqualityComparer<PersonPosition> personPositionDtoComparer;

        public void PersonPositionServiceTestsInitialize()
        {
            MockInitialize();

            employeeProxyCurrentUserFactory = new EmployeeProxyUserFactory();
            supervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
            personPositionRepositoryMock = new Mock<IPersonPositionRepository>();
            positionRepositoryMock = new Mock<IPositionRepository>();
            testPersonPositionRepository = new TestPersonPositionRepository();

            personPositionEntityToDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.PersonPosition, PersonPosition>(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.HumanResources.Entities.PersonPosition, PersonPosition>())
                .Returns(personPositionEntityToDtoAdapter);

            personPositionRepositoryMock.Setup(r => r.GetPersonPositionsAsync(It.IsAny<IEnumerable<string>>(), null))
               .Returns<IEnumerable<string>, DateTime?>((personIds, date) => testPersonPositionRepository.GetPersonPositionsAsync(personIds, date));

            personPositionDtoComparer = new FunctionEqualityComparer<PersonPosition>(
                (p1, p2) => p1.Id == p2.Id && p1.PersonId == p2.PersonId && p1.PositionId == p2.PositionId && p1.StartDate == p2.StartDate,
                (p) => p.Id.GetHashCode());
        }

        [TestClass]
        public class GetPersonPositionsTests : PersonPositionServiceTests
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
                PersonPositionServiceTestsInitialize();
            }

            [TestMethod]
            public async Task RepositoryCalledWithCurrentUserIdTest()
            {
                await actualService.GetPersonPositionsAsync();
                personPositionRepositoryMock.Verify(r =>
                    r.GetPersonPositionsAsync(It.Is<IEnumerable<string>>(list =>
                        list.Count() == 1 && list.ElementAt(0) == employeeCurrentUserFactory.CurrentUser.PersonId), null));
            }

            [TestMethod]
            public async Task RepositoryCalledWithProxyIdTest()
            {
                await proxyActualService.GetPersonPositionsAsync("0000001");
                personPositionRepositoryMock.Verify(r =>
                    r.GetPersonPositionsAsync(It.Is<IEnumerable<string>>(list =>
                        list.Count() == 1 && list.ElementAt(0) == "0000001"), null));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RepositoryReturnsNullTest()
            {
                personPositionRepositoryMock.Setup(r => r.GetPersonPositionsAsync(It.IsAny<IEnumerable<string>>(), null))
                    .ReturnsAsync(() => null);

                try
                {
                    await actualService.GetPersonPositionsAsync();
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
                var expected = (await testPersonPositionRepository.GetPersonPositionsAsync(new List<string>() { employeeCurrentUserFactory.CurrentUser.PersonId }))
                    .Select(ppEntity => personPositionEntityToDtoAdapter.MapToType(ppEntity));

                var actual = await actualService.GetPersonPositionsAsync(employeeCurrentUserFactory.CurrentUser.PersonId);

                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), personPositionDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NoAccessWithRandomIdViaProxyService()
            {
                await proxyActualService.GetPersonPositionsAsync("0000003");

            }


            [TestMethod]
            public async Task PersonPositions_RepositoryCurrentUserIdWithAdminPermissionTest()
            {
                roleRepositoryMock.Setup(r => r.Roles)
               .Returns(() => (employeeCurrentUserFactory.CurrentUser.Roles).Select(roleTitle =>
               {
                   var role = new Domain.Entities.Role(roleTitle.GetHashCode(), roleTitle);

                   role.AddPermission(new Domain.Entities.Permission("VIEW.ALL.TIME.HISTORY"));

                   return role;
               }));

                await actualService.GetPersonPositionsAsync(UserForAdminPermissionCheck);
                personPositionRepositoryMock.Verify(r =>
                    r.GetPersonPositionsAsync(It.Is<IEnumerable<string>>(list =>
                        list.Count() == 1 && list.ElementAt(0) == UserForAdminPermissionCheck), null));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PersonPositions_RepositoryCurrentUserIdWithoutAdminPermissionTest()
            {
                await actualService.GetPersonPositionsAsync(UserForAdminPermissionCheck);

            }
        }
    }
}
