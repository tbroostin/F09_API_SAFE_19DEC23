// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.BudgetManagement.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Tests.Services
{
    [TestClass]
    public class BudgetDevelopmentConfigurationServiceTests
    {

        #region Initialize and Cleanup
        private BudgetDevelopmentConfigurationService service = null;
        private TestBudgetDevelopmentConfigurationRepository testBuDevConfigurationRepository;

        private GeneralLedgerCurrentUser.UserFactory userFactory = new GeneralLedgerCurrentUser.UserFactory();
        private Mock<IRoleRepository> roleRepositoryMock;
        protected Domain.Entities.Role glUserRoleAllPermissions = new Domain.Entities.Role(333, "Budget.Adjustor");
        public Mock<ILogger> logger;
        public Mock<IAdapterRegistry> adapterRegistry;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistry = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>();

            // Mock the repository for the role that has all permissions.
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleAllPermissions });

            testBuDevConfigurationRepository = new TestBudgetDevelopmentConfigurationRepository();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var buDevConfigDtoAdapter = new AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetConfiguration, Dtos.BudgetManagement.BudgetConfiguration>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetConfiguration, Dtos.BudgetManagement.BudgetConfiguration>()).Returns(buDevConfigDtoAdapter);

            var buDevConfigComparablesDtoAdapter = new AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetConfigurationComparable, Dtos.BudgetManagement.BudgetConfigurationComparable>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetConfigurationComparable, Dtos.BudgetManagement.BudgetConfigurationComparable>()).Returns(buDevConfigComparablesDtoAdapter);


            // Set up the service
            service = new BudgetDevelopmentConfigurationService(testBuDevConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            testBuDevConfigurationRepository = null;
            userFactory = null;
            roleRepositoryMock = null;
            logger = null;
            adapterRegistry = null;
        }
        #endregion

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_Success()
        {
            var buDevConfigEntity = await testBuDevConfigurationRepository.GetBudgetDevelopmentConfigurationAsync();
            buDevConfigEntity.RemoveBudgetConfigurationComparables();
            var buDevConfigDto = await service.GetBudgetDevelopmentConfigurationAsync();

            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigDto.BudgetId);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, buDevConfigDto.BudgetTitle);
            Assert.AreEqual(buDevConfigDto.BudgetStatus, Dtos.BudgetManagement.BudgetStatus.Working);
            Assert.AreEqual(buDevConfigDto.BudgetConfigurationComparables.Count, 5);
            foreach (var comparable in buDevConfigDto.BudgetConfigurationComparables)
            {
                var matchingComparable = buDevConfigEntity.BudgetConfigurationComparables.FirstOrDefault(x => x.SequenceNumber == comparable.SequenceNumber);
                Assert.AreEqual(comparable.ComparableHeader, matchingComparable.ComparableHeader);
                Assert.AreEqual(comparable.ComparableId, matchingComparable.ComparableId);
                Assert.AreEqual(comparable.ComparableYear, matchingComparable.ComparableYear);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_OnlyC1()
        {
            // Use only one comparable
            var buDevConfigEntity = await testBuDevConfigurationRepository.GetBudgetDevelopmentConfigurationAsync();
            buDevConfigEntity.RemoveBudgetConfigurationComparables();
            buDevConfigEntity.AddBudgetConfigurationComparable(new BudgetConfigurationComparable {ComparableHeader = "2016 Actual", ComparableId = "C2", ComparableYear = "21016", SequenceNumber = 1 });

            var repositoryOneComparableMock = new Mock<IBudgetDevelopmentConfigurationRepository>();
            repositoryOneComparableMock.Setup(x => x.GetBudgetDevelopmentConfigurationAsync()).ReturnsAsync(buDevConfigEntity);

            service = new BudgetDevelopmentConfigurationService(repositoryOneComparableMock.Object, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);
            var buDevConfigDto = await service.GetBudgetDevelopmentConfigurationAsync();

            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigDto.BudgetId);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, buDevConfigDto.BudgetTitle);
            Assert.AreEqual(buDevConfigDto.BudgetStatus, Dtos.BudgetManagement.BudgetStatus.Working);
            Assert.AreEqual(buDevConfigDto.BudgetConfigurationComparables.Count, 1);
            foreach (var comparable in buDevConfigDto.BudgetConfigurationComparables)
            {
                var matchingComparable = buDevConfigEntity.BudgetConfigurationComparables.FirstOrDefault(x => x.SequenceNumber == comparable.SequenceNumber);
                Assert.AreEqual(comparable.ComparableHeader, matchingComparable.ComparableHeader);
                Assert.AreEqual(comparable.ComparableId, matchingComparable.ComparableId);
                Assert.AreEqual(comparable.ComparableYear, matchingComparable.ComparableYear);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_NoComparables()
        {
            var buDevConfigEntity = await testBuDevConfigurationRepository.GetBudgetDevelopmentConfigurationAsync();
            buDevConfigEntity.RemoveBudgetConfigurationComparables();

            var repositoryNoComparablesMock = new Mock<IBudgetDevelopmentConfigurationRepository>();
            repositoryNoComparablesMock.Setup(x => x.GetBudgetDevelopmentConfigurationAsync()).ReturnsAsync(buDevConfigEntity);

            service = new BudgetDevelopmentConfigurationService(repositoryNoComparablesMock.Object, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevConfigDto = await service.GetBudgetDevelopmentConfigurationAsync();

            Assert.AreEqual(buDevConfigEntity.BudgetId, buDevConfigDto.BudgetId);
            Assert.AreEqual(buDevConfigEntity.BudgetTitle, buDevConfigDto.BudgetTitle);
            Assert.AreEqual(buDevConfigDto.BudgetStatus, Dtos.BudgetManagement.BudgetStatus.Working);
            Assert.AreEqual(buDevConfigDto.BudgetConfigurationComparables.Count, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_NullEntity()
        {
            // Mock the service to return a null domain entity.
            var repositoryNullDomainMock = new Mock<IBudgetDevelopmentConfigurationRepository>();
            repositoryNullDomainMock.Setup(x => x.GetBudgetDevelopmentConfigurationAsync()).ReturnsAsync(null as BudgetConfiguration);

            service = new BudgetDevelopmentConfigurationService(repositoryNullDomainMock.Object, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevConfigDto = await service.GetBudgetDevelopmentConfigurationAsync();
            Assert.IsNull(buDevConfigDto.BudgetId);
            Assert.IsNull(buDevConfigDto.BudgetTitle);
            Assert.AreEqual(buDevConfigDto.BudgetStatus, Dtos.BudgetManagement.BudgetStatus.New);
        }
    }
}