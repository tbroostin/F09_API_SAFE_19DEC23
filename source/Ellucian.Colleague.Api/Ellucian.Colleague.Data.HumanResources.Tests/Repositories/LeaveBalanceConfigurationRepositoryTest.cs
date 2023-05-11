/*Copyright 2018-2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class LeaveBalanceConfigurationRepositoryTest
    {
        private LeaveBalanceConfigurationRepository leaveBalanceConfigurationRepository;
        private Mock<IColleagueTransactionFactory> transFactoryMock;
        private Mock<IColleagueDataReader> dataAccessorMock;
        private Mock<Web.Cache.ICacheProvider> cacheProviderMock;
        private Mock<ILogger> loggerMock;

        [TestInitialize]
        public async void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            cacheProviderMock = new Mock<Web.Cache.ICacheProvider>();
            dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            leaveBalanceConfigurationRepository = await BuildLeaveBalanceConfigurationRepository();

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
        }


        [TestCleanup]
        public void Cleanup()
        {
            cacheProviderMock = null;
            dataAccessorMock = null;
            transFactoryMock = null;
            leaveBalanceConfigurationRepository = null;
        }

        private async Task<LeaveBalanceConfigurationRepository> BuildLeaveBalanceConfigurationRepository()
        {
            // Set up data accessor for mocking (needed for get)
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
            var testLeaveBalanceConfigurationRepository = new TestLeaveBalanceConfigurationRepository();
            // set up repo response for getting leave balance configuration items
            var testLeaveBalanceConfigurationItems = await testLeaveBalanceConfigurationRepository.GetLeaveBalanceConfigurationAsync();
            List<string> list = new List<string>();
            list.Add("INAC");
            list.Add("CMPS");
            HrssDefaults value = new HrssDefaults()
            {
                HrssExcludeLeavePlanIds = list,
                HrssLeaveLkbk = testLeaveBalanceConfigurationItems.LeaveRequestLookbackDays,
                HrssLrUnsubmitWdrw = Convert.ToString(testLeaveBalanceConfigurationItems.LeaveRequestActionType),
                HrssLrAllowSuprvsrEdit = Convert.ToString(testLeaveBalanceConfigurationItems.AllowSupervisorToEditLeaveRequests)
            };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS", true)).ReturnsAsync(value);

            leaveBalanceConfigurationRepository = new LeaveBalanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            return leaveBalanceConfigurationRepository;
        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationRepository_GetLeaveBalanceConfigurationAsync()
        {            
            var leaveBalanceConfiguration = await leaveBalanceConfigurationRepository.GetLeaveBalanceConfigurationAsync();
            Assert.IsNotNull(leaveBalanceConfiguration);
            Assert.AreEqual("INAC", leaveBalanceConfiguration.ExcludedLeavePlanIds[0]);
        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationRepository_LeaveRequestLookbackDaysAreSetCorrectlyTest()
        {
            var leaveBalanceConfiguration = await leaveBalanceConfigurationRepository.GetLeaveBalanceConfigurationAsync();
            var expectedDefaultValue = 60;
            Assert.AreEqual(expectedDefaultValue, leaveBalanceConfiguration.LeaveRequestLookbackDays);
        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationRepository_LeaveRequestLookbackDaysSetToNullTest()
        {
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS", true)).ReturnsAsync(new HrssDefaults());
            var leaveBalanceConfiguration = await leaveBalanceConfigurationRepository.GetLeaveBalanceConfigurationAsync();
            Assert.IsNull(leaveBalanceConfiguration.LeaveRequestLookbackDays);
        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationRepository_GetLeaveBalanceConfigurationAsync_ReturnsNull()
        {
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS", true)).ReturnsAsync(new HrssDefaults());
            var leaveBalanceConfiguration = await leaveBalanceConfigurationRepository.GetLeaveBalanceConfigurationAsync();
            Assert.IsNotNull(leaveBalanceConfiguration);
            Assert.IsNull(leaveBalanceConfiguration.ExcludedLeavePlanIds);
        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationRepository_GetLeaveBalanceConfigurationAsync_ReturnsEmptyList()
        {
            List<string> list = new List<string>();
            HrssDefaults value = new HrssDefaults() { HrssExcludeLeavePlanIds = list };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS", true)).ReturnsAsync(value);

            var leaveBalanceConfiguration = await leaveBalanceConfigurationRepository.GetLeaveBalanceConfigurationAsync();
            Assert.IsNotNull(leaveBalanceConfiguration);
            Assert.IsNotNull(leaveBalanceConfiguration.ExcludedLeavePlanIds);
            Assert.IsTrue(leaveBalanceConfiguration.ExcludedLeavePlanIds.Count==0);

        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationRepository_LeaveRequestActionTypeIsSetCorrectlyTest()
        {
            var leaveBalanceConfiguration = await leaveBalanceConfigurationRepository.GetLeaveBalanceConfigurationAsync();
            var expectedDefaultValue = LeaveRequestActionType.R;
            Assert.AreEqual(expectedDefaultValue, leaveBalanceConfiguration.LeaveRequestActionType);
        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationRepository_LeaveRequestActionTypeSetToNotNullTest()
        {
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS", true)).ReturnsAsync(new HrssDefaults());
            var leaveBalanceConfiguration = await leaveBalanceConfigurationRepository.GetLeaveBalanceConfigurationAsync();
            Assert.IsNotNull(leaveBalanceConfiguration.LeaveRequestActionType);
        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationRepository_AllowSupervisorToEditLeaveRequestsSetToFalseTest()
        {
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS", true)).ReturnsAsync(new HrssDefaults());
            var leaveBalanceConfiguration = await leaveBalanceConfigurationRepository.GetLeaveBalanceConfigurationAsync();
            Assert.IsFalse(leaveBalanceConfiguration.AllowSupervisorToEditLeaveRequests);
        }        

        [TestMethod]
        public async Task LeaveBalanceConfigurationRepository_AllowSupervisorToEditLeaveRequestsAreSetCorrectlyTest()
        {
            var leaveBalanceConfiguration = await leaveBalanceConfigurationRepository.GetLeaveBalanceConfigurationAsync();
            var expectedDefaultValue = false;
            Assert.AreEqual(expectedDefaultValue, leaveBalanceConfiguration.AllowSupervisorToEditLeaveRequests);
        }
    }
}
