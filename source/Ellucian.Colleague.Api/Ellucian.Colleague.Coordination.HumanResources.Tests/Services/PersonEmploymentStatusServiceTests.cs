/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
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
    public class PersonEmploymentStatusServiceTests : HumanResourcesServiceTestsSetup
    {
        public Mock<IPersonEmploymentStatusRepository> personEmploymentStatusRepositoryMock;

        public Mock<IPersonEmploymentStatusRepository> proxyPersonEmploymentStatusRepositoryMock;

        public Mock<ISupervisorsRepository> supervisorsRepositoryMock;

        public Mock<ISupervisorsRepository> proxySupervisorsRepositoryMock;

        public TestPersonEmploymentStatusRepository testPersonEmploymentStatusRepository;

        public TestPersonEmploymentStatusRepository proxyTestPersonEmploymentStatusRepository;

        public ITypeAdapter<Domain.HumanResources.Entities.PersonEmploymentStatus, Dtos.HumanResources.PersonEmploymentStatus> personEmploymentStatusEntityToDtoAdapter;

        public ITypeAdapter<Domain.HumanResources.Entities.PersonEmploymentStatus, Dtos.HumanResources.PersonEmploymentStatus> proxyPersonEmploymentStatusEntityToDtoAdapter;

        protected Domain.Entities.Role timeApprovalRole;
        private Domain.Entities.Permission timeEntryApprovalPermission;

        protected Domain.Entities.Role proxyTimeApprovalRole;
        private Domain.Entities.Permission proxyTimeEntryApprovalPermission;

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
                        PersonId = "0003914",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Employee",
                        Roles = new List<string>() { "EDIT_DD", "UPDATE.EMPLOYEE" },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0000001"
                        }
                    });
                }
            }
        }

        public PersonEmploymentStatusService actualService
        {
            get
            {
                return new PersonEmploymentStatusService(
                    personEmploymentStatusRepositoryMock.Object,
                    supervisorsRepositoryMock.Object,
                    adapterRegistryMock.Object,
                    employeeCurrentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }
        }

        public PersonEmploymentStatusService proxyActualService
        {
            get
            {
                return new PersonEmploymentStatusService(
                    proxyPersonEmploymentStatusRepositoryMock.Object,
                    proxySupervisorsRepositoryMock.Object,
                    proxyAdapterRegistryMock.Object,
                    employeeProxyCurrentUserFactory,
                    proxyRoleRepositoryMock.Object,
                    proxyLoggerMock.Object);
            }
        }

        public FunctionEqualityComparer<PersonEmploymentStatus> personEmploymentStatusDtoComparer;

        public void PersonEmploymentStatusServiceTestsInitialize()
        {
            MockInitialize();

            employeeProxyCurrentUserFactory = new EmployeeProxyUserFactory();
            supervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
            personEmploymentStatusRepositoryMock = new Mock<IPersonEmploymentStatusRepository>();
            testPersonEmploymentStatusRepository = new TestPersonEmploymentStatusRepository();

            proxySupervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
            proxyPersonEmploymentStatusRepositoryMock = new Mock<IPersonEmploymentStatusRepository>();
            proxyTestPersonEmploymentStatusRepository = new TestPersonEmploymentStatusRepository();

            personEmploymentStatusEntityToDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.PersonEmploymentStatus, PersonEmploymentStatus>(adapterRegistryMock.Object, loggerMock.Object);

            proxyPersonEmploymentStatusEntityToDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.PersonEmploymentStatus, PersonEmploymentStatus>(proxyAdapterRegistryMock.Object, proxyLoggerMock.Object);

            personEmploymentStatusRepositoryMock.Setup(r => r.GetPersonEmploymentStatusesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>((personIds) => testPersonEmploymentStatusRepository.GetPersonEmploymentStatusesAsync(personIds));

            proxyPersonEmploymentStatusRepositoryMock.Setup(r => r.GetPersonEmploymentStatusesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>((personIds) => proxyTestPersonEmploymentStatusRepository.GetPersonEmploymentStatusesAsync(personIds));

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.HumanResources.Entities.PersonEmploymentStatus, PersonEmploymentStatus>())
                .Returns(personEmploymentStatusEntityToDtoAdapter);

            proxyAdapterRegistryMock.Setup(r => r.GetAdapter<Domain.HumanResources.Entities.PersonEmploymentStatus, PersonEmploymentStatus>())
                .Returns(proxyPersonEmploymentStatusEntityToDtoAdapter);

            // permissions mock
            timeApprovalRole = new Domain.Entities.Role(76, "TIME MANAGEMENT SUPERVISOR");
            timeEntryApprovalPermission = new Ellucian.Colleague.Domain.Entities.Permission("APPROVE.REJECT.TIME.ENTRY");
            timeApprovalRole.AddPermission(timeEntryApprovalPermission);
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { timeApprovalRole });

            // proxy permissions mock
            proxyTimeApprovalRole = new Domain.Entities.Role(76, "TIME MANAGEMENT SUPERVISOR");
            proxyTimeEntryApprovalPermission = new Ellucian.Colleague.Domain.Entities.Permission("APPROVE.REJECT.TIME.ENTRY");
            proxyTimeApprovalRole.AddPermission(proxyTimeEntryApprovalPermission);
            proxyRoleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { proxyTimeApprovalRole });

            personEmploymentStatusDtoComparer = new FunctionEqualityComparer<PersonEmploymentStatus>(
                (p1, p2) => 
                    p1.Id == p2.Id && 
                    p1.PersonId == p2.PersonId && 
                    p1.PersonPositionId == p2.PersonPositionId && 
                    p1.PrimaryPositionId == p2.PrimaryPositionId && 
                    p1.StartDate == p2.StartDate &&
                    p1.EndDate == p2.EndDate,
                (p) => p.Id.GetHashCode());
        }
        

    [TestClass]
        public class GetPersonEmploymentStatussTests : PersonEmploymentStatusServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                PersonEmploymentStatusServiceTestsInitialize();
            }

            [TestMethod]
            public async Task RepositoryCalledWithCurrentUserIdTest()
            {
                await actualService.GetPersonEmploymentStatusesAsync();
                personEmploymentStatusRepositoryMock.Verify(r =>
                    r.GetPersonEmploymentStatusesAsync(It.Is<IEnumerable<string>>(list =>
                        list.Count() == 1 && list.ElementAt(0) == employeeCurrentUserFactory.CurrentUser.PersonId)));
            }

            [TestMethod]
            public async Task RepositoryCalledWithProxyUserIdTest()
            {
                await proxyActualService.GetPersonEmploymentStatusesAsync("0000001");
                proxyPersonEmploymentStatusRepositoryMock.Verify(r =>
                    r.GetPersonEmploymentStatusesAsync(It.Is<IEnumerable<string>>(list =>
                        list.Count() == 1 && list.ElementAt(0) == "0000001")));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RepositoryCalledWithNoAccessUserIdTest()
            {
                await proxyActualService.GetPersonEmploymentStatusesAsync("0000002");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RepositoryReturnsNullTest()
            {
                personEmploymentStatusRepositoryMock.Setup(r => r.GetPersonEmploymentStatusesAsync(It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>>((ids) => Task.FromResult<IEnumerable<Domain.HumanResources.Entities.PersonEmploymentStatus>>(null));

                try
                {
                    await actualService.GetPersonEmploymentStatusesAsync();
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
                var expected = (await testPersonEmploymentStatusRepository.GetPersonEmploymentStatusesAsync(new List<string>() { employeeCurrentUserFactory.CurrentUser.PersonId }))
                    .Select(ppEntity => personEmploymentStatusEntityToDtoAdapter.MapToType(ppEntity));

                var actual = await actualService.GetPersonEmploymentStatusesAsync();

                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), personEmploymentStatusDtoComparer);
            }
        }
    }
}
