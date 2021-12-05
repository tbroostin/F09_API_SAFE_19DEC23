//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class EmployeeLeaveTransactionsServiceTests_V11 : CurrentUserSetup
    {
        [TestClass]
        public class EmployeeLeaveTransactionsServiceTests_GET_AND_GETALL
        {
            #region DECLARATION
            protected Ellucian.Colleague.Domain.Entities.Role getEmpLeaveTransRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.EMPL.LEAVE.TRANSACTIONS");
            private Mock<IEmployeeLeavePlansRepository> empLeavePlansRepositoryMock;
            private Mock<IEmployeeLeaveTransactionsRepository> empLeaveTransRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ICurrentUserFactory currentUserFactory;

            private EmployeeLeaveTransactionsService empLeaveTransService;

            private Tuple<IEnumerable<PerleaveDetails>, int> tupleEmployeeLeaveTransactions;
            private PerleaveDetails domainEmployeeLeaveTransactions;
            private EmployeeLeaveTransactions dtoEmployeeLeaveTransactions;
            private Perleave empleave;


            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                empLeavePlansRepositoryMock = new Mock<IEmployeeLeavePlansRepository>();
                empLeaveTransRepositoryMock = new Mock<IEmployeeLeaveTransactionsRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();


                InitializeTestData();

                InitializeMock();

                empLeaveTransService = new EmployeeLeaveTransactionsService(empLeaveTransRepositoryMock.Object, empLeavePlansRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory,
                    roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                empLeavePlansRepositoryMock = null;
                empLeaveTransRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                configurationRepositoryMock = null;
            }

            private void InitializeTestData()
            {
                empleave = new Perleave(guid, "empleave1", DateTime.Today, "person1", "plan1");
                domainEmployeeLeaveTransactions = new PerleaveDetails(guid, "1", DateTime.Today, "empleave1")
                {
                    LeaveHours = 40,
                    AvailableHours = 20

                };

                tupleEmployeeLeaveTransactions = new Tuple<IEnumerable<PerleaveDetails>, int>(new List<PerleaveDetails>() { domainEmployeeLeaveTransactions }, 1);
            }

            private void InitializeMock()
            {
                currentUserFactory = new CurrentUserSetup.PersonEmployeeLeaveTransactionUserFactory();

                getEmpLeaveTransRole.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewEmployeeLeaveTransactions));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getEmpLeaveTransRole });
                empLeaveTransRepositoryMock.Setup(l => l.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleEmployeeLeaveTransactions);
                empLeaveTransRepositoryMock.Setup(l => l.GetEmployeeLeaveTransactionsByIdAsync(It.IsAny<string>())).ReturnsAsync(domainEmployeeLeaveTransactions);
                empLeavePlansRepositoryMock.Setup(r => r.GetEmployeeLeavePlansByIdAsync(It.IsAny<string>())).ReturnsAsync(empleave);

                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add(domainEmployeeLeaveTransactions.EmployeeLeaveId, domainEmployeeLeaveTransactions.Guid);
               
                empLeavePlansRepositoryMock.Setup(r => r.GetPerleaveGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);


            }

            #endregion

            [TestMethod]
            public async Task EmployeeLeaveTransactionsService_GetEmployeeLeaveTransactionsAsync_Repository_Returns_Null()
            {
                empLeaveTransRepositoryMock.Setup(l => l.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(() => null);
                var result = await empLeaveTransService.GetEmployeeLeaveTransactionsAsync(0, 100);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task EmployeeLeaveTransactionsService_GetEmployeeLeaveTransactionsByGuidAsync()
            {
                domainEmployeeLeaveTransactions = new PerleaveDetails(guid, "1", DateTime.Today, "empleave1") { };
                var result = await empLeaveTransService.GetEmployeeLeaveTransactionsByGuidAsync(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, guid);
            }

            [TestMethod]
            public async Task EmployeeLeaveTransactionsService_GetEmployeeLeaveTransactionsByGuidAsync_Taken()
            {
                domainEmployeeLeaveTransactions = new PerleaveDetails(guid, "1", DateTime.Today, "empleave1") { };
                domainEmployeeLeaveTransactions.LeaveHours = new decimal(-20);
                empLeaveTransRepositoryMock.Setup(l => l.GetEmployeeLeaveTransactionsByIdAsync(It.IsAny<string>())).ReturnsAsync(domainEmployeeLeaveTransactions);
                var result = await empLeaveTransService.GetEmployeeLeaveTransactionsByGuidAsync(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, guid);
            }
            
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task EmployeeLeaveTransactionsService_GetEmployeeLeaveTransactionsByGuidAsync_InvalidOperation()
            {
                empLeaveTransRepositoryMock.Setup(l => l.GetEmployeeLeaveTransactionsByIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                await empLeaveTransService.GetEmployeeLeaveTransactionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmployeeLeaveTransactionsService_GetEmployeeLeaveTransactionsByGuidAsync_KeyNotFoundException()
            {
                empLeaveTransRepositoryMock.Setup(l => l.GetEmployeeLeaveTransactionsByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await empLeaveTransService.GetEmployeeLeaveTransactionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EmployeeLeaveTransactionsService_GetEmployeeLeaveTransactionsAsync_Exception_From_Repository()
            {
                empLeaveTransRepositoryMock.Setup(l => l.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new Exception());
                await empLeaveTransService.GetEmployeeLeaveTransactionsAsync(0, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EmployeeLeaveTransactionsService_GetEmployeeLeaveTransactionsAsync_EmpLeave_Null_From_Repos()
            {
                // empLeavePlansRepositoryMock.Setup(r => r.GetEmployeeLeavePlansByIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                empLeavePlansRepositoryMock.Setup(r => r.GetPerleaveGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(new Dictionary<string, string>());

                await empLeaveTransService.GetEmployeeLeaveTransactionsAsync(0, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EmployeeLeaveTransactionsService_GetEmployeeLeaveTransactionsAsync_Invalid_EmpLeave()
            {
                tupleEmployeeLeaveTransactions = new Tuple<IEnumerable<PerleaveDetails>, int>(new List<PerleaveDetails>() { new PerleaveDetails(guid, "1", DateTime.Today, "empleave2") { } }, 1);
                empLeavePlansRepositoryMock.Setup(r => r.GetEmployeeLeavePlansByIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                empLeaveTransRepositoryMock.Setup(l => l.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleEmployeeLeaveTransactions);
                await empLeaveTransService.GetEmployeeLeaveTransactionsAsync(0, 100);
            }

            [TestMethod]
            public async Task EmployeeLeaveTransactionsService_GetEmployeeLeaveTransactionsAsync()
            {
                tupleEmployeeLeaveTransactions = new Tuple<IEnumerable<PerleaveDetails>, int>(new List<PerleaveDetails>() { new PerleaveDetails(guid, "1", DateTime.Today, "empleave1") { } }, 1);
                empLeaveTransRepositoryMock.Setup(l => l.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleEmployeeLeaveTransactions);

                var result = await empLeaveTransService.GetEmployeeLeaveTransactionsAsync(0, 100);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 1);
                Assert.AreEqual(result.Item1.FirstOrDefault().Id, guid);
            }

                       
        }

    }
}