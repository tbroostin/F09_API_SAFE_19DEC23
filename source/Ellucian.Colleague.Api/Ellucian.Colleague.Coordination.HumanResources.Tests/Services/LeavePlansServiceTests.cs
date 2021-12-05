//Copyright 2017- Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class LeavePlansServiceTests_V11
    {
        [TestClass]
        public class LeavePlansServiceTests_GET_AND_GETALL
        {
            #region DECLARATION

            protected Domain.Entities.Role getStudentAcademicPrograms = new Domain.Entities.Role(1, "VIEW.STUDENT.ACADEMIC.PROGRAM");

            private Mock<IHumanResourcesReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<ILeavePlansRepository> leavePlansRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactory;

            private LeavePlansService leavePlansService;

            private Tuple<IEnumerable<LeavePlan>, int> tupleLeavePlans;
            private LeavePlan domainLeavePlans;
            private LeavePlans dtoLeavePlans;
            private IEnumerable<LeaveType> leaveTypes;
            private List<EmploymentFrequency> frequencies;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                referenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                leavePlansRepositoryMock = new Mock<ILeavePlansRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                currentUserFactory = new Mock<ICurrentUserFactory>();

                InitializeTestData();

                InitializeMock();

                leavePlansService = new LeavePlansService(referenceDataRepositoryMock.Object, leavePlansRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory.Object,
                    roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                referenceDataRepositoryMock = null;
                leavePlansRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                configurationRepositoryMock = null;
            }

            private void InitializeTestData()
            {
                leaveTypes = new List<LeaveType>() { new LeaveType(guid, "1", "description") };

                frequencies = new List<EmploymentFrequency>() { new EmploymentFrequency(guid, "1", "description", "eft") };

                domainLeavePlans = new LeavePlan(guid, "1", DateTime.Today, "title", "1", "s")
                {
                    AllowNegative = "Y",
                    YearlyStartDate = DateTime.Today.AddDays(10),
                    RollOverLeaveType = "1",
                    AccuralFrequency = "1"
                };

                tupleLeavePlans = new Tuple<IEnumerable<LeavePlan>, int>(new List<LeavePlan>() { domainLeavePlans }, 1);
            }

            private void InitializeMock()
            {
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleLeavePlans);
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansByIdAsync(It.IsAny<string>())).ReturnsAsync(domainLeavePlans);
                referenceDataRepositoryMock.Setup(r => r.GetLeaveTypesAsync(false)).ReturnsAsync(leaveTypes);
                foreach (var l in leaveTypes)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetLeaveTypesGuidAsync(l.Code)).ReturnsAsync(l.Guid);
                }

                referenceDataRepositoryMock.Setup(r => r.GetEmploymentFrequenciesAsync(false)).ReturnsAsync(frequencies);

                foreach (var f in frequencies)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetEmploymentFrequenciesGuidAsync(f.Code)).ReturnsAsync(f.Guid);
                }
            }

            #endregion      

            [TestMethod]
            public async Task LeavePlansService_GetLeavePlansAsync_Repository_Returns_Null()
            {
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(() => null);
                var result = await leavePlansService.GetLeavePlansAsync(0, 100);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task LeavePlansService_GetLeavePlansAsync_Exception_From_Repository()
            {
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new Exception());
                await leavePlansService.GetLeavePlansAsync(0, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task LeavePlansService_GetLeavePlansAsync_LeaveType_Null_From_Repository()
            {
                referenceDataRepositoryMock.Setup(r => r.GetLeaveTypesGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await leavePlansService.GetLeavePlansAsync(0, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task LeavePlansService_GetLeavePlansAsync_Invalid_LeaveType()
            {
                tupleLeavePlans = new Tuple<IEnumerable<LeavePlan>, int>(new List<LeavePlan>() { new LeavePlan(guid, "1", DateTime.Now, "title", "2", "s") { } }, 1);
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleLeavePlans);
                await leavePlansService.GetLeavePlansAsync(0, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task LeavePlansService_GetLeavePlansAsync_Invalid_RollOverLeaveType()
            {
                tupleLeavePlans = new Tuple<IEnumerable<LeavePlan>, int>(new List<LeavePlan>() { new LeavePlan(guid, "1", DateTime.Now, "title", "1", "h") { RollOverLeaveType = "2" } }, 1);
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleLeavePlans);
                await leavePlansService.GetLeavePlansAsync(0, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task LeavePlansService_GetLeavePlansAsync_AccuralFrequencies_Null_From_Repository()
            {
                tupleLeavePlans = new Tuple<IEnumerable<LeavePlan>, int>(new List<LeavePlan>() { new LeavePlan(guid, "1", DateTime.Now, "title", "1", "t") { AccuralFrequency = "1" } }, 1);
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleLeavePlans);
                referenceDataRepositoryMock.Setup(r => r.GetEmploymentFrequenciesGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await leavePlansService.GetLeavePlansAsync(0, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task LeavePlansService_GetLeavePlansAsync_Invalid_AccuralFrequency()
            {
                tupleLeavePlans.Item1.FirstOrDefault().AllowNegative = "N";
                tupleLeavePlans.Item1.FirstOrDefault().AccuralFrequency = "2";
                await leavePlansService.GetLeavePlansAsync(0, 100);
            }

            [TestMethod]
            public async Task LeavePlansService_GetLeavePlansAsync()
            {
                tupleLeavePlans = new Tuple<IEnumerable<LeavePlan>, int>(new List<LeavePlan>() { new LeavePlan(guid, "1", DateTime.Now, "title", "1", "p") { } }, 1);
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleLeavePlans);

                var result = await leavePlansService.GetLeavePlansAsync(0, 100);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 1);
                Assert.AreEqual(result.Item1.FirstOrDefault().Id, guid);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task LeavePlansService_GetLeavePlansByGuidAsync_KeyNotFoundException_From_Repository()
            {
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await leavePlansService.GetLeavePlansByGuidAsync(guid);
            }


            [TestMethod]
            public async Task LeavePlansService_GetLeavePlansByGuidAsync()
            {
                domainLeavePlans = new LeavePlan(guid, "1", DateTime.Today, "title", "1", "h") { };
                var result = await leavePlansService.GetLeavePlansByGuidAsync(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, guid);
            }
        }
    }
}
