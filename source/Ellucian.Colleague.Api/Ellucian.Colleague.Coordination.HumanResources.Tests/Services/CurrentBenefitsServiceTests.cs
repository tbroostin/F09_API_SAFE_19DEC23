/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Web.Adapters;
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
    public class CurrentBenefitsServiceTests : HumanResourcesServiceTestsSetup
    {
        private Mock<ICurrentBenefitsRepository> currentBenefitsRepositoryMock;
        private TestCurrentBenefitsRepository testCurrentBenefitsRepository;
        private ICurrentUserFactory currentBenefitUserFactory;
        private CurrentBenefitsService currentBenefitsService;

        public ITypeAdapter<Domain.HumanResources.Entities.EmployeeBenefits, Dtos.HumanResources.EmployeeBenefits> employeeBenefitsEntityToDtoAdapter;

        [TestInitialize]
        public void EmployeeCompensationServiceInitialize()
        {
            MockInitialize();

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            currentBenefitsRepositoryMock = new Mock<ICurrentBenefitsRepository>();
            testCurrentBenefitsRepository = new TestCurrentBenefitsRepository();
            currentBenefitUserFactory = new CurrentBenefitUserFactory();

            currentBenefitsService = new CurrentBenefitsService(testCurrentBenefitsRepository, 
                adapterRegistryMock.Object, 
                currentBenefitUserFactory, 
                roleRepositoryMock.Object,
                loggerMock.Object);


            employeeBenefitsEntityToDtoAdapter = new EmployeeBenefitsEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.HumanResources.Entities.EmployeeBenefits, Dtos.HumanResources.EmployeeBenefits>())
               .Returns(() =>
                   new EmployeeBenefitsEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object));
            adapterRegistryMock.Setup(a => a.GetAdapter<Domain.HumanResources.Entities.CurrentBenefit, Dtos.HumanResources.CurrentBenefit>())
               .Returns(() => new AutoMapperAdapter<Domain.HumanResources.Entities.CurrentBenefit, Dtos.HumanResources.CurrentBenefit>(adapterRegistryMock.Object, loggerMock.Object));
          
        }

        [TestMethod]
        public async Task ExpectedEqualsActual_NullTest()
        {
            currentBenefitsService = new CurrentBenefitsService(currentBenefitsRepositoryMock.Object,
                 adapterRegistryMock.Object,
                 currentBenefitUserFactory,
                 roleRepositoryMock.Object,
                 loggerMock.Object);

            string effectivePersonId = null;
            var actual = await currentBenefitsService.GetEmployeesCurrentBenefitsAsync(effectivePersonId);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task ExpectedEqualsActualTest()
        {
            string effectivePersonId = null;
            var actual = await currentBenefitsService.GetEmployeesCurrentBenefitsAsync(effectivePersonId);
            EmployeeBenefits expected = await testCurrentBenefitsRepository.GetEmployeeCurrentBenefitsAsync("0014697");
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.PersonId, actual.PersonId);
        }

        [ExpectedException(typeof(PermissionsException))]
        [TestMethod]
        public async Task ExpectedEqualsActual_ExpectionTest()
        {
            var actual = await currentBenefitsService.GetEmployeesCurrentBenefitsAsync("0014500");
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            currentBenefitUserFactory = null;
            currentBenefitsRepositoryMock = null;
        }
    }

    public class CurrentBenefitUserFactory : ICurrentUserFactory
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
