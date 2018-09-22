//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class LeaveBalanceConfigurationServiceTests
    {
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;

        private Mock<ILeaveBalanceConfigurationRepository> leaveBalanceConfigurationRepositoryMock;
        private LeaveBalanceConfigurationService leaveBalanceConfigurationService;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            leaveBalanceConfigurationRepositoryMock = new Mock<ILeaveBalanceConfigurationRepository>();

            leaveBalanceConfigurationService = new LeaveBalanceConfigurationService(leaveBalanceConfigurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactoryMock.Object,
                roleRepositoryMock.Object, loggerMock.Object, null, null);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;         
            roleRepositoryMock = null;
            loggerMock = null;
            currentUserFactoryMock = null;
            leaveBalanceConfigurationRepositoryMock = null;
        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationService_GetLeaveBalanceConfigurationAsync_Repository_Returns_Null()
        {
            leaveBalanceConfigurationRepositoryMock.Setup(l => l.GetLeaveBalanceConfigurationAsync()).ReturnsAsync(null);
            var result = await leaveBalanceConfigurationService.GetLeaveBalanceConfigurationAsync();
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationService_GetLeaveBalanceConfigurationAsync_Repository_Returns_ValidList()
        {
            List<string> list = new List<string>();
            list.Add("INAC");
            list.Add("CMPS");
            LeaveBalanceConfiguration value = new LeaveBalanceConfiguration()
            {
                ExcludedLeavePlanIds = list
            };           
            leaveBalanceConfigurationRepositoryMock.Setup(l => l.GetLeaveBalanceConfigurationAsync()).ReturnsAsync(value);
            var result = await leaveBalanceConfigurationService.GetLeaveBalanceConfigurationAsync();
            Assert.AreEqual("INAC", value.ExcludedLeavePlanIds[0]);
        }

        [TestMethod]
        public async Task LeaveBalanceConfigurationService_GetLeaveBalanceConfigurationAsync_Repository_Returns_EmptyList()
        {
            List<string> list = new List<string>();
            LeaveBalanceConfiguration value = new LeaveBalanceConfiguration()
            {
                ExcludedLeavePlanIds = list
            };
            leaveBalanceConfigurationRepositoryMock.Setup(l => l.GetLeaveBalanceConfigurationAsync()).ReturnsAsync(value);
            var result = await leaveBalanceConfigurationService.GetLeaveBalanceConfigurationAsync();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ExcludedLeavePlanIds);
            Assert.IsTrue(result.ExcludedLeavePlanIds.Count == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task LeaveBalanceConfigurationService_GetLeaveBalanceConfigurationAsync_Repository_Throws_Exception()
        {
            leaveBalanceConfigurationRepositoryMock.Setup(l => l.GetLeaveBalanceConfigurationAsync()).ThrowsAsync(new Exception());
            var result = await leaveBalanceConfigurationService.GetLeaveBalanceConfigurationAsync();
        }
    }
}
