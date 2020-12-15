using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.BudgetManagement;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
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
    public class BudgetPhaseLineItemsServiceTests_GET_GETALL_V12 : BudgetUser
    {
        #region DECLARATION

        protected Domain.Entities.Role viewBudgetPhaseLineItems = new Domain.Entities.Role(1, "VIEW.BUDGET.PHASE.LINE.ITEMS");

        private Mock<IBudgetRepository> budgetRepositoryMock;
        private Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;

        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private BudgetUser currentUserFactory;

        private BudgetPhaseLineItemsService budgetPhaseLineItemsService;

        private Tuple<IEnumerable<BudgetWork>, int> budgetPahseLineItemsTuple;
        private IEnumerable<BudgetWork> budgetPahseLineItems;
        private AccountingStringComponentValues accountingStringComponentValues;

        private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            budgetRepositoryMock = new Mock<IBudgetRepository>();
            referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            loggerMock = new Mock<ILogger>();

            currentUserFactory = new BudgetUser();

            budgetPhaseLineItemsService = new BudgetPhaseLineItemsService(budgetRepositoryMock.Object, referenceDataRepositoryMock.Object, adapterRegistryMock.Object,
                currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);

            InitializeTestData();

            InitializeMock();
        }

        [TestCleanup]
        public void Cleanup()
        {
            budgetRepositoryMock = null;
            referenceDataRepositoryMock = null;
            adapterRegistryMock = null;
            currentUserFactory = null;
            roleRepositoryMock = null;
            loggerMock = null;
            configurationRepositoryMock = null;
        }

        private void InitializeTestData()
        {
            budgetPahseLineItems = new List<BudgetWork>()
                {
                    new BudgetWork("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1")
                    {
                        BudgetPhase = "1",
                        AccountingStringComponentValue = "1",
                        LineAmount = 100,
                        HostCountry = "USA",
                        Comments = new List<string>() { "mock comment" }
                    },
                    new BudgetWork("2a59eed8-5fe7-4120-b1cf-f23266b9e874", "1")
                    {
                        AccountingStringComponentValue = "1",
                        HostCountry = "CAN",
                        Comments = new List<string>() { "mock comment" }
                    }
                };

            budgetPahseLineItemsTuple = new Tuple<IEnumerable<BudgetWork>, int>(budgetPahseLineItems, budgetPahseLineItems.Count());

            accountingStringComponentValues = new AccountingStringComponentValues() { AccountNumber = "1" };
        }

        private void InitializeMock()
        {
            viewBudgetPhaseLineItems.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BudgetManagementPermissionCodes.ViewBudgetPhaseLineItems));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { viewBudgetPhaseLineItems });

            budgetRepositoryMock.Setup(b => b.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), false)).ReturnsAsync(budgetPahseLineItemsTuple);
            budgetRepositoryMock.Setup(b => b.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>())).ReturnsAsync(budgetPahseLineItems.FirstOrDefault());
            budgetRepositoryMock.Setup(b => b.GetBudgetPhasesGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            budgetRepositoryMock.Setup(b => b.GetBudgetPhasesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            referenceDataRepositoryMock.Setup(r => r.GetAccountingStringComponentValuesGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            referenceDataRepositoryMock.Setup(r => r.GetAccountingStringComponentValueByGuid(It.IsAny<string>()))
                .ReturnsAsync(accountingStringComponentValues);

            referenceDataRepositoryMock.Setup(r => r.GetAccountingStringComponentValueByGuid(It.IsAny<string>())).ReturnsAsync(accountingStringComponentValues);
        }

        #endregion

        #region GETALL

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task BudgetPhaseLineItemService_GetBudgetPhaseLineItemsAsync_PermissionException()
        {
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task BudgetPhaseLineItemService_GetBudgetPhaseLineItemsAsync_BudgetPhaseGuid_NotFound()
        {
            budgetRepositoryMock.Setup(b => b.GetBudgetPhasesGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);
            await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task BudgetPhaseLineItemService_GetBudgetPhaseLineItemsAsync_AccountingStringComponentValue_NotFound()
        {
            referenceDataRepositoryMock.Setup(r => r.GetAccountingStringComponentValuesGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);
            await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, null, null);
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemService_GetBudgetPhaseLineItemsAsync()
        {
            var result = await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, null, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Item2, 2);
            Assert.AreEqual(result.Item1.FirstOrDefault().Id, guid);
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemService_GetAll_With_BudgetPhase_Filter_KeyNotFoundException()
        {
            budgetRepositoryMock.Setup(b => b.GetBudgetPhasesIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

            var result = await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, guid, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Item2, 0);
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemService_GetAll_With_BudgetPhase_Filter()
        {
            var result = await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, guid, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Item2, 2);
            Assert.AreEqual(result.Item1.FirstOrDefault().Id, guid);
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemService_GetAll_With_AccountingStringComponentValue_Filter_KeyNotFoundException()
        {
            referenceDataRepositoryMock.Setup(r => r.GetAccountingStringComponentValueByGuid(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

            var result = await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, null, new List<string>() { guid });

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Item2, 0);
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemService_GetAll_With_Multiple_AccountingStringComponentValue_Filters()
        {
            var result = await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, null, new List<string>() { guid, guid });

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Item2, 0);
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemService_GetAll_With_AccountingStringComponentValue_Filter_Returns_Null()
        {
            referenceDataRepositoryMock.Setup(r => r.GetAccountingStringComponentValueByGuid(It.IsAny<string>())).ReturnsAsync(null);

            var result = await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, null, new List<string>() { guid });

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Item2, 0);
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemService_GetAll_With_AccountingStringComponentValue_Filter()
        {
            var result = await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, null, new List<string>() { guid });

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Item2, 2);
            Assert.AreEqual(result.Item1.FirstOrDefault().Id, guid);
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemService_GetBudgetPhaseLineItemsAsync_With_Both_Filters()
        {
            var result = await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsAsync(0, 2, guid, new List<string>() { guid });

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Item2, 2);
            Assert.AreEqual(result.Item1.FirstOrDefault().Id, guid);
        }

        #endregion

        #region GET

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task BudgetPhaseLineItemService_GetBudgetPhaseLineItemsByGuidAsync_PermissionException()
        {
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BudgetPhaseLineItemService_GetBudgetPhaseLineItemsByGuidAsync_KeyNotFoundException()
        {
            budgetRepositoryMock.Setup(b => b.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BudgetPhaseLineItemService_GetBudgetPhaseLineItemsByGuidAsync_InvalidOperationException()
        {
            budgetRepositoryMock.Setup(b => b.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
            await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task BudgetPhaseLineItemService_GetBudgetPhaseLineItemsByGuidAsync_ArgumentException()
        {
            budgetRepositoryMock.Setup(b => b.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
            await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task BudgetPhaseLineItemService_GetByGuid_BudgetPhase_Notfound()
        {
            budgetRepositoryMock.Setup(b => b.GetBudgetPhasesGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);
            await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task BudgetPhaseLineItemService_GetByGuid_AccoutingStringComponentValue_Notfound()
        {
            referenceDataRepositoryMock.Setup(b => b.GetAccountingStringComponentValuesGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);
            await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsByGuidAsync(guid);
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemService_GetBudgetPhaseLineItemsByGuidAsync()
        {
            var result = await budgetPhaseLineItemsService.GetBudgetPhaseLineItemsByGuidAsync(guid);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, budgetPahseLineItems.FirstOrDefault().RecordGuid);
        }

        #endregion
    }
}