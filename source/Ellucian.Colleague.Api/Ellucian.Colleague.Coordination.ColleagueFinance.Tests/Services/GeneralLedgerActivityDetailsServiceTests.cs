// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class GeneralLedgerActivityDetailsServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        private GeneralLedgerActivityDetailsService glActivityDetailsService_HasGlAccounts;
        private GeneralLedgerActivityDetailsService glActivityDetailsService_EmptyGlAccounts;
        private Mock<IGeneralLedgerUserRepository> glUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
        private UserFactory userFactoryHasAccounts = new GeneralLedgerCurrentUser.UserFactory();
        private UserFactoryNone userFactoryNone = new GeneralLedgerCurrentUser.UserFactoryNone();
        private TestGeneralLedgerUserRepository testGlUserRepository = new TestGeneralLedgerUserRepository();
        private TestGlAccountActivityDetailRepository testGlAccountActivityRepository = new TestGlAccountActivityDetailRepository();
        private GeneralLedgerClassConfiguration glClassConfiguration = new GeneralLedgerClassConfiguration("GL.CLASS", new List<string> { "5", "7" }, new List<string> { "4" }, new List<string> { "1" }, new List<string> { "2" }, new List<string> { "3" });

        [TestInitialize]
        public void Initialize()
        {
            // Build the services object.
            var loggerObject = new Mock<ILogger>().Object;
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var entityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.GlAccountActivityDetail, Dtos.ColleagueFinance.GlAccountActivityDetail>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.GlAccountActivityDetail, Dtos.ColleagueFinance.GlAccountActivityDetail>()).Returns(entityToDtoAdapter);

            glUserRepositoryMock.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<GeneralLedgerClassConfiguration>())).Returns((string a, string b, GeneralLedgerClassConfiguration glClassConfiguration) =>
                {
                    return testGlUserRepository.GetGeneralLedgerUserAsync2(a, b, glClassConfiguration);
                });

            glActivityDetailsService_HasGlAccounts = new GeneralLedgerActivityDetailsService(
                testGlAccountActivityRepository,
                glUserRepositoryMock.Object,
                new TestGeneralLedgerConfigurationRepository(),
                adapterRegistry.Object,
                userFactoryHasAccounts,
                new Mock<IRoleRepository>().Object,
                loggerObject);

            glActivityDetailsService_EmptyGlAccounts = new GeneralLedgerActivityDetailsService(
                new TestGlAccountActivityDetailRepository(),
                glUserRepositoryMock.Object,
                new TestGeneralLedgerConfigurationRepository(),
                adapterRegistry.Object,
                userFactoryNone,
                new Mock<IRoleRepository>().Object,
                loggerObject);
        }

        [TestCleanup]
        public void Cleanup()
        {
            glActivityDetailsService_HasGlAccounts = null;
            glActivityDetailsService_EmptyGlAccounts = null;
        }
        #endregion

        #region Tests
        #region Exception scenarios
        [TestMethod]
        public async Task QueryGlAccountActivityDetailAsync_NullGlAccount()
        {
            var expectedParam = "glaccount";
            var actualParam = "";
            try
            {
                await Helper_QueryGlAccountActivityDetailAsync(null, "2016");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task QueryGlAccountActivityDetailAsync_EmptyGlAccount()
        {
            var expectedParam = "glaccount";
            var actualParam = "";
            try
            {
                await Helper_QueryGlAccountActivityDetailAsync("", "2016");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task QueryGlAccountActivityDetailAsync_NullFiscalYear()
        {
            var expectedParam = "fiscalyear";
            var actualParam = "";
            try
            {
                await Helper_QueryGlAccountActivityDetailAsync("10_00_01_00_33333_51000", null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task QueryGlAccountActivityDetailAsync_EmptyFiscalYear()
        {
            var expectedParam = "fiscalyear";
            var actualParam = "";
            try
            {
                await Helper_QueryGlAccountActivityDetailAsync("10_00_01_00_33333_51000", "");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task QueryGlAccountActivityDetailAsync_GlUserDoesNotContainGlNumberArgument()
        {
            var glNumber = "zzzzzzz";
            var expectedMessage = "You do not have access to the requested GL account " + glNumber;
            var actualMessage = "";
            try
            {
                await Helper_QueryGlAccountActivityDetailAsync(glNumber, "2016");
            }
            catch (PermissionsException pex)
            {
                actualMessage = pex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        [TestMethod]
        public async Task QueryGlAccountActivityDetailsAsync_Success()
        {
            string glClassName = "GL.CLASS";
            var glExpenseValues = new List<string>() { "5", "7" };
            var glRevenueValues = new List<string>() { "4", "6" };
            var glAssetValues = new List<string>() { "1" };
            var glLiabilityValues = new List<string>() { "2" };
            var glFundBalValues = new List<string>() { "3" };
            IList<string> majorComponentStartPosition = new List<string>() { "1", "4", "7", "10", "13", "19" };
            GeneralLedgerClassConfiguration glClassConfiguration = new GeneralLedgerClassConfiguration(glClassName, glExpenseValues, glRevenueValues, glAssetValues, glLiabilityValues, glFundBalValues);
            var expectedGlAccountActivity = await testGlAccountActivityRepository.QueryGlActivityDetailAsync("10_00_01_01_33333_51001", "2016", new CostCenterStructure(), glClassConfiguration, majorComponentStartPosition);
            var actualGlAccountActivity = await Helper_QueryGlAccountActivityDetailAsync("10_00_01_01_33333_51001", "2016");

            Assert.AreEqual(expectedGlAccountActivity.ActualAmount, actualGlAccountActivity.Actuals);
            Assert.AreEqual(expectedGlAccountActivity.BudgetAmount, actualGlAccountActivity.Budget);
            Assert.AreEqual(expectedGlAccountActivity.EncumbranceAmount, actualGlAccountActivity.Encumbrances);
            Assert.AreEqual(expectedGlAccountActivity.GlAccountDescription, actualGlAccountActivity.Description);
            Assert.AreEqual(expectedGlAccountActivity.GlAccountNumber, actualGlAccountActivity.GlAccountNumber);
            Assert.AreEqual(expectedGlAccountActivity.MemoActualsAmount, actualGlAccountActivity.MemoActuals);
            Assert.AreEqual(expectedGlAccountActivity.MemoBudgetAmount, actualGlAccountActivity.MemoBudget);
            Assert.AreEqual(expectedGlAccountActivity.Name, actualGlAccountActivity.Name);
            Assert.AreEqual(expectedGlAccountActivity.UnitId, actualGlAccountActivity.UnitId);
            Assert.AreEqual(expectedGlAccountActivity.Transactions.Count, actualGlAccountActivity.ActualsTransactions.Count
                + actualGlAccountActivity.EncumbranceTransactions.Count + actualGlAccountActivity.BudgetTransactions.Count);
        }
        #endregion

        #region Private methods
        private async Task<Dtos.ColleagueFinance.GlAccountActivityDetail> Helper_QueryGlAccountActivityDetailAsync(string glAccount, string fiscalYear)
        {
            return await glActivityDetailsService_HasGlAccounts.QueryGlAccountActivityDetailAsync(glAccount, fiscalYear);
        }
        #endregion
    }
}