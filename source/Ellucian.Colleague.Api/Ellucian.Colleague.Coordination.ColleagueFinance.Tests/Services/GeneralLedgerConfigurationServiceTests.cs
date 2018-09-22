// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
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

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class GeneralLedgerConfigurationServiceTests
    {
        #region Initialize and Cleanup
        private GeneralLedgerConfigurationService service = null;
        private GeneralLedgerCurrentUser.GeneralLedgerUserAllAccounts currentUserFactory = new GeneralLedgerCurrentUser.GeneralLedgerUserAllAccounts();
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
        private Mock<IGeneralLedgerConfigurationRepository> glConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
        private GeneralLedgerFiscalYearConfiguration fiscalYearConfigurationEntity = new GeneralLedgerFiscalYearConfiguration(6, DateTime.Now.Year.ToString(), 2, 11, "Y");
        private GeneralLedgerClassConfiguration glClassConfigurationEntity = new GeneralLedgerClassConfiguration("GL.CLASS", new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>());
        private CostCenterStructure costCenterStructureEntity = new CostCenterStructure();
        
        private BudgetAdjustmentsEnabled adjustmentsEnabledEntity = new BudgetAdjustmentsEnabled(true);
        private BudgetAdjustmentParameters budgetAdjustmentParameters = new BudgetAdjustmentParameters(true, false, false);
        private IEnumerable<string> openFiscalYears;

        [TestInitialize]
        public void Initialize()
        {
            glConfigurationRepositoryMock.Setup(repo => repo.GetFiscalYearConfigurationAsync()).Returns(() =>
            {
                return Task.FromResult(fiscalYearConfigurationEntity);
            });

            glClassConfigurationEntity.GlClassStartPosition = 18;
            glClassConfigurationEntity.GlClassLength = 1;
            glConfigurationRepositoryMock.Setup(repo => repo.GetClassConfigurationAsync()).Returns(() =>
            {
                return Task.FromResult(glClassConfigurationEntity);
            });

            costCenterStructureEntity.AddCostCenterComponent(new GeneralLedgerComponent("FUND", true, GeneralLedgerComponentType.Fund, "1", "2"));
            glConfigurationRepositoryMock.Setup(repo => repo.GetCostCenterStructureAsync()).Returns(() =>
            {
                return Task.FromResult(costCenterStructureEntity);
            });

            openFiscalYears = new List<string>() { DateTime.Now.Year.ToString(), DateTime.Now.AddYears(-1).ToString() };
            glConfigurationRepositoryMock.Setup(repo => repo.GetAllOpenFiscalYears()).Returns(() =>
            {
                return Task.FromResult(openFiscalYears);
            });

            glConfigurationRepositoryMock.Setup(repo => repo.GetBudgetAdjustmentEnabledAsync()).Returns(() =>
            {
                return Task.FromResult(adjustmentsEnabledEntity);
            });

            glConfigurationRepositoryMock.Setup(repo => repo.GetBudgetAdjustmentParametersAsync()).Returns(() =>
            {
                return Task.FromResult(budgetAdjustmentParameters);
            });

            BuildService(glConfigurationRepositoryMock.Object, currentUserFactory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            testGlConfigurationRepository = null;
        }
        #endregion

        #region GetFiscalYearConfigurationAsync
        [TestMethod]
        public async Task HappyPath()
        {
            var configurationDto = await service.GetBudgetAdjustmentConfigurationAsync();
            Assert.AreEqual(fiscalYearConfigurationEntity.CurrentFiscalMonth, configurationDto.CurrentFiscalMonth);
            Assert.AreEqual(fiscalYearConfigurationEntity.CurrentFiscalYear, configurationDto.CurrentFiscalYear);
            Assert.AreEqual(fiscalYearConfigurationEntity.StartMonth, configurationDto.StartMonth);
            Assert.AreEqual(fiscalYearConfigurationEntity.StartOfFiscalYear, configurationDto.StartOfFiscalYear);
            Assert.AreEqual(fiscalYearConfigurationEntity.EndOfFiscalYear, configurationDto.EndOfFiscalYear);
            
            foreach (var fiscalYear in openFiscalYears)
            {
                Assert.IsTrue(configurationDto.OpenFiscalYears.Contains(fiscalYear));
            }
        }

        [TestMethod]
        public async Task FiscalYearConfigurationIsNull()
        {
            var expectedMessage = "Fiscal year configuration must be defined.";
            var actualMessage = "";
            try
            {
                fiscalYearConfigurationEntity = null;
                var configurationDto = await service.GetBudgetAdjustmentConfigurationAsync();
            }
            catch (ConfigurationException ex)
            {
                actualMessage = ex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #region GetBudgetAdjustmentEnabledAsync
        [TestMethod]
        public async Task GetBudgetAdjustmentEnabledAsync_Success()
        {
            this.adjustmentsEnabledEntity = new BudgetAdjustmentsEnabled(true);
            var enabledDto = await service.GetBudgetAdjustmentEnabledAsync();
            Assert.AreEqual(this.adjustmentsEnabledEntity.Enabled, enabledDto.Enabled);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentEnabledAsync_RepositoryReturnsNull()
        {
            this.adjustmentsEnabledEntity = null;
            var enabledDto = await service.GetBudgetAdjustmentEnabledAsync();

            Assert.IsFalse(enabledDto.Enabled);
        }
        #endregion

        #region Private methods
        private void BuildService(IGeneralLedgerConfigurationRepository testGlConfigurationRepository,
            ICurrentUserFactory userFactory)
        {
            // Use Mock to create mock implementations that are based on the same interfaces
            var roleRepository = new Mock<IRoleRepository>().Object;
            var loggerObject = new Mock<ILogger>().Object;

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var journalEntryDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.JournalEntry, Dtos.ColleagueFinance.JournalEntry>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.JournalEntry, Dtos.ColleagueFinance.JournalEntry>()).Returns(journalEntryDtoAdapter);

            var costCenterStructureAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerComponent, Ellucian.Colleague.Dtos.ColleagueFinance.GeneralLedgerComponent>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerComponent, Ellucian.Colleague.Dtos.ColleagueFinance.GeneralLedgerComponent>()).Returns(costCenterStructureAdapter);

            // Set up the services
            service = new GeneralLedgerConfigurationService(testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepository, loggerObject);
        }
        #endregion
    }
}