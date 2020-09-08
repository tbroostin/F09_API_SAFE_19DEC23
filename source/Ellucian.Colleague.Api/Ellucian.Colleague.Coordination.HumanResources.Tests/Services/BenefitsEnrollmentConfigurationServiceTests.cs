/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Dto = Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
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
    public class BenefitsEnrollmentConfigurationServiceTests : HumanResourcesServiceTestsSetup
    {
        private Mock<IBenefitsEnrollmentConfigurationRepository> benefitsEnrollmentConfigurationRepositoryMock;
        private BenefitsEnrollmentConfigurationService service;
        private BenefitsEnrollmentConfiguration benefitsEnrollmentConfiguration;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            benefitsEnrollmentConfigurationRepositoryMock = new Mock<IBenefitsEnrollmentConfigurationRepository>();

            SetupData();
            SetupMocks();
            BuildService();
        }

        private void BuildService()
        {
            service = new BenefitsEnrollmentConfigurationService(benefitsEnrollmentConfigurationRepositoryMock.Object, adapterRegistryMock.Object, employeeCurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            benefitsEnrollmentConfigurationRepositoryMock = null;
            employeeCurrentUserFactory = null;
            service = null;
        }

        private void SetupMocks()
        {
            benefitsEnrollmentConfigurationRepositoryMock.Setup(i => i.GetBenefitsEnrollmentConfigurationAsync()).ReturnsAsync(benefitsEnrollmentConfiguration);

            adapterRegistryMock.Setup(a => a.GetAdapter<BenefitsEnrollmentConfiguration, Dto.BenefitsEnrollmentConfiguration>())
               .Returns(() => new AutoMapperAdapter<BenefitsEnrollmentConfiguration, Dto.BenefitsEnrollmentConfiguration>(adapterRegistryMock.Object, loggerMock.Object));

        }

        private void SetupData()
        {
            benefitsEnrollmentConfiguration = new BenefitsEnrollmentConfiguration()
            {
                RelationshipTypes = new List<string>() { "F", "M" },
                IsBenefitsEnrollmentEnabled = true
            };
        }

        #region GetBenefitsEnrollmentConfigurationAsync

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_Properties_Test()
        {
            var result = await service.GetBenefitsEnrollmentConfigurationAsync();
            Assert.AreEqual(2, result.RelationshipTypes.Count);
            Assert.IsTrue(result.IsBenefitsEnrollmentEnabled);
        }

        #endregion GetBenefitsEnrollmentConfigurationAsync

    }
}
