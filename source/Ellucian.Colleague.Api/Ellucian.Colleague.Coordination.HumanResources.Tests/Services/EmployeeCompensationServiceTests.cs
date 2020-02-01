/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
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

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class EmployeeCompensationServiceTests : HumanResourcesServiceTestsSetup
    {
        private Mock<IEmployeeCompensationRepository> employeeCompensationRepositoryMock;
        private Mock<ISupervisorsRepository> supervisorsRepositoryMock;
        private TestEmployeeCompensationRepository testEmployeeCompensationRepository;
        private Mock<IHumanResourcesReferenceDataRepository> referenceDataRepositoryMock;
        private ICurrentUserFactory employeeCompensationUserFactory;
        private EmployeeCompensationService employeeCompensationService;

        public ITypeAdapter<Domain.HumanResources.Entities.EmployeeCompensation, Dtos.HumanResources.EmployeeCompensation> employeeCompEntityToDtoAdapter;

        [TestInitialize]
        public void EmployeeCompensationServiceInitialize()
        {
            MockInitialize();

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            supervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
            employeeCompensationRepositoryMock = new Mock<IEmployeeCompensationRepository>();
            testEmployeeCompensationRepository = new TestEmployeeCompensationRepository();
            referenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            employeeCompensationUserFactory = new EmployeeCompensationUserFactory();

            employeeCompensationService = new EmployeeCompensationService(testEmployeeCompensationRepository,
                                                                        referenceDataRepositoryMock.Object,
                                                                        adapterRegistryMock.Object, employeeCompensationUserFactory,
                                                                        roleRepositoryMock.Object, loggerMock.Object);


            employeeCompEntityToDtoAdapter = new EmployeeCompensationEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.HumanResources.Entities.EmployeeCompensation, Dtos.HumanResources.EmployeeCompensation>())
               .Returns(() =>
                   new EmployeeCompensationEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object));
            adapterRegistryMock.Setup(a => a.GetAdapter<Domain.HumanResources.Entities.EmployeeBended, Dtos.HumanResources.EmployeeBended>())
               .Returns(() => new AutoMapperAdapter<Domain.HumanResources.Entities.EmployeeBended, Dtos.HumanResources.EmployeeBended>(adapterRegistryMock.Object, loggerMock.Object));
            adapterRegistryMock.Setup(a => a.GetAdapter<Domain.HumanResources.Entities.EmployeeTax, Dtos.HumanResources.EmployeeTax>())
               .Returns(() => new AutoMapperAdapter<Domain.HumanResources.Entities.EmployeeTax, Dtos.HumanResources.EmployeeTax>(adapterRegistryMock.Object, loggerMock.Object));
            adapterRegistryMock.Setup(a => a.GetAdapter<Domain.HumanResources.Entities.EmployeeStipend, Dtos.HumanResources.EmployeeStipend>())
               .Returns(() => new AutoMapperAdapter<Domain.HumanResources.Entities.EmployeeStipend, Dtos.HumanResources.EmployeeStipend>(adapterRegistryMock.Object, loggerMock.Object));

        }

        [TestMethod]
        public async Task ExpectedEqualsActual_NullTest()
        {
            employeeCompensationService = new EmployeeCompensationService(employeeCompensationRepositoryMock.Object,
                                                                        referenceDataRepositoryMock.Object,
                                                                        adapterRegistryMock.Object, employeeCompensationUserFactory,
                                                                        roleRepositoryMock.Object, loggerMock.Object);
            var actual = await employeeCompensationService.GetEmployeeCompensationAsync();
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task ExpectedEqualsActualTest()
        {
            var actual = await employeeCompensationService.GetEmployeeCompensationAsync();

            EmployeeCompensation expected = await testEmployeeCompensationRepository.GetEmployeeCompensationAsync("0014697", null, false);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.PersonId, actual.PersonId);
            Assert.AreEqual(expected.SalaryAmount, actual.SalaryAmount);

        }

        [ExpectedException(typeof(PermissionsException))]
        [TestMethod]
        public async Task ExpectedEqualsActual_ExpectionTest()
        {
            var actual = await employeeCompensationService.GetEmployeeCompensationAsync("0014698");
        }

        [TestMethod]
        public async Task ExpectedEqualsActual_AdminTest()
        {
            roleRepositoryMock.Setup(r => r.Roles)
                  .Returns(() => (employeeCompensationUserFactory.CurrentUser.Roles).Select(roleTitle =>
                  {
                      var role = new Domain.Entities.Role(roleTitle.GetHashCode(), roleTitle);

                      role.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewAllTotalCompensation));

                      return role;
                  }));
            string EffectivePersonId = "0014698";
            var actual = await employeeCompensationService.GetEmployeeCompensationAsync(EffectivePersonId);
            EmployeeCompensation expected = await testEmployeeCompensationRepository.GetEmployeeCompensationAsync(EffectivePersonId, null, false);

            Assert.IsNotNull(actual);
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.PersonId, actual.PersonId);
            Assert.AreEqual(expected.SalaryAmount, actual.SalaryAmount);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            employeeCompensationUserFactory = null;
            employeeCompensationRepositoryMock = null;
        }
    }

    public class EmployeeCompensationUserFactory : ICurrentUserFactory
    {
        public ICurrentUser CurrentUser
        {
            get
            {
                return new CurrentUser(new Claims()
                {
                    ControlId = "123",
                    Name = "Natalie",
                    PersonId = "0014697",
                    SecurityToken = "321",
                    SessionTimeout = 30,
                    UserName = "Nataliegillon",
                    Roles = new List<string>() { "EMPLOYEE" },
                    SessionFixationId = "abc123"

                });
            }
        }
    }
}
