/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
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
    public class PersonPositionWageServiceTests : HumanResourcesServiceTestsSetup
    {
        public Mock<IPersonPositionWageRepository> personPositionWageRepositoryMock;
        public Mock<ISupervisorsRepository> supervisorsRepositoryMock;
        public TestPersonPositionWageRepository testPersonPositionWageRepository;

        public Mock<IHumanResourcesReferenceDataRepository> referenceDataRepositoryMock;        

        public PersonPositionWageEntityToDtoAdapter personPositionWageEntityToDtoAdapter;

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

        public PersonPositionWageService actualService
        {
            get
            {
                return new PersonPositionWageService(
                    personPositionWageRepositoryMock.Object,
                    supervisorsRepositoryMock.Object,
                    referenceDataRepositoryMock.Object,
                    adapterRegistryMock.Object,
                    employeeCurrentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }
        }

        public PersonPositionWageService proxyActualService
        {
            get
            {
                return new PersonPositionWageService(
                    personPositionWageRepositoryMock.Object,
                    supervisorsRepositoryMock.Object,
                    referenceDataRepositoryMock.Object,
                    adapterRegistryMock.Object,
                    employeeProxyCurrentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }
        }

        public FunctionEqualityComparer<PersonPositionWage> personPositionWageDtoComparer;

        public void PersonPositionWageServiceTestsInitialize()
        {
            MockInitialize();

            supervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
            personPositionWageRepositoryMock = new Mock<IPersonPositionWageRepository>();
            testPersonPositionWageRepository = new TestPersonPositionWageRepository();
            employeeProxyCurrentUserFactory = new EmployeeProxyUserFactory();
            referenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();

            personPositionWageEntityToDtoAdapter = new PersonPositionWageEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            personPositionWageRepositoryMock.Setup(r => r.GetPersonPositionWagesAsync(It.IsAny<IEnumerable<string>>(), null))
                .Returns<IEnumerable<string>, DateTime?>((ids, date) => testPersonPositionWageRepository.GetPersonPositionWagesAsync(ids, date));


            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.HumanResources.Entities.PersonPositionWage, Dtos.HumanResources.PersonPositionWage>())
                .Returns(() => personPositionWageEntityToDtoAdapter);

            personPositionWageDtoComparer = new FunctionEqualityComparer<PersonPositionWage>(
                (p1, p2) => p1.Id == p2.Id && p1.PersonId == p2.PersonId && p1.PositionId == p2.PositionId && p1.StartDate == p2.StartDate,
                (p) => p.Id.GetHashCode());
        }

        [TestClass]
        public class GetPersonPositionWagesTests : PersonPositionWageServiceTests
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
                await actualService.GetPersonPositionWagesAsync();
                personPositionWageRepositoryMock.Verify(r =>
                    r.GetPersonPositionWagesAsync(It.Is<IEnumerable<string>>(list => list.Count() == 1 && list.ElementAt(0) == employeeCurrentUserFactory.CurrentUser.PersonId),
                    null));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RepositoryReturnsNullTest()
            {
                personPositionWageRepositoryMock.Setup(r => r.GetPersonPositionWagesAsync(It.IsAny<IEnumerable<string>>(), null))
                    .ReturnsAsync(null);

                try
                {
                    await actualService.GetPersonPositionWagesAsync();
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
                var expected = (await testPersonPositionWageRepository.GetPersonPositionWagesAsync(new List<string>() { employeeCurrentUserFactory.CurrentUser.PersonId }))
                    .Select(ppEntity => personPositionWageEntityToDtoAdapter.MapToType(ppEntity));

                var actual = await actualService.GetPersonPositionWagesAsync(employeeCurrentUserFactory.CurrentUser.PersonId);

                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), personPositionWageDtoComparer);

            }

            [TestMethod]
            public async Task RepositoryCalledWithProxyIdTest()
            {
                await proxyActualService.GetPersonPositionWagesAsync("0000001");
                personPositionWageRepositoryMock.Verify(r =>
                    r.GetPersonPositionWagesAsync(It.Is<IEnumerable<string>>(list =>
                        list.Count() == 1 && list.ElementAt(0) == "0000001"), null));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NoAccessWithRandomIdViaProxyService()
            {
                await proxyActualService.GetPersonPositionWagesAsync("0000003");

            }

            [TestMethod]
            public async Task PersonPositionWages_RepositoryCurrentUserIdWithAdminPermissionTest()
            {
                roleRepositoryMock.Setup(r => r.Roles)
               .Returns(() => (employeeCurrentUserFactory.CurrentUser.Roles).Select(roleTitle =>
               {
                   var role = new Domain.Entities.Role(roleTitle.GetHashCode(), roleTitle);

                   role.AddPermission(new Domain.Entities.Permission("VIEW.ALL.TIME.HISTORY"));

                   return role;
               }));

                var res=await actualService.GetPersonPositionWagesAsync(UserForAdminPermissionCheck);
                personPositionWageRepositoryMock.Verify(r =>
                    r.GetPersonPositionWagesAsync(It.Is<IEnumerable<string>>(list =>
                        list.Count() == 1 && list.ElementAt(0) == UserForAdminPermissionCheck), null));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PersonPositionWages_RepositoryCurrentUserIdWithoutAdminPermissionTest()
            {
                await actualService.GetPersonPositionWagesAsync(UserForAdminPermissionCheck);

            }
        }
    }
}
