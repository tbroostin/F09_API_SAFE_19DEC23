//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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
    public class EmployeeLeavePlansServiceTests : HumanResourcesServiceTestsSetup
    {
        [TestClass]
        public class EmployeeLeavePlansServiceTests_GET_AND_GETALL
        {
            #region DECLARATION
            protected Ellucian.Colleague.Domain.Entities.Role getEmpLeavePlansRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.EMPL.LEAVE.PLANS");
            private Mock<ISupervisorsRepository> supervisorsRepositoryMock;
            private Mock<IEmployeeLeavePlansRepository> empLeavePlansRepositoryMock;
            private Mock<ILeavePlansRepository> leavePlansRepositoryMock;
            private Mock<ILeaveBalanceConfigurationRepository> leaveBalanceConfiguratioRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IHumanResourcesReferenceDataRepository> humanResourcesReferenceDataRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ICurrentUserFactory currentUserFactory;
            private EmployeeLeavePlansService empLeavePlansService;
            private Tuple<IEnumerable<Perleave>, int> tupleEmployeeLeavePlans;
            private Perleave domainEmployeeLeavePlans;
            private List<LeavePlan> domainLeavePlans;
            private List<LeaveType> leaveTypes;
            private List<EarningType2> earningTypes2;
            private List<EmployeeLeavePlan> employeeLeavePlans;
            private ITypeAdapter<Domain.HumanResources.Entities.EmployeeLeavePlan, Dtos.HumanResources.EmployeeLeavePlan> employeeLeavePlanAdapter;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                supervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
                empLeavePlansRepositoryMock = new Mock<IEmployeeLeavePlansRepository>();
                leavePlansRepositoryMock = new Mock<ILeavePlansRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                humanResourcesReferenceDataRepository = new Mock<IHumanResourcesReferenceDataRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                leaveBalanceConfiguratioRepositoryMock = new Mock<ILeaveBalanceConfigurationRepository>();
                InitializeTestData();
                InitializeMock();

                empLeavePlansService = new EmployeeLeavePlansService(supervisorsRepositoryMock.Object, empLeavePlansRepositoryMock.Object, leavePlansRepositoryMock.Object, humanResourcesReferenceDataRepository.Object, adapterRegistryMock.Object, currentUserFactory,
                    roleRepositoryMock.Object, personRepositoryMock.Object, configurationRepositoryMock.Object, leaveBalanceConfiguratioRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                empLeavePlansRepositoryMock = null;
                leavePlansRepositoryMock = null;
                personRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                configurationRepositoryMock = null;
                humanResourcesReferenceDataRepository = null;
                supervisorsRepositoryMock = null;
            }

            private void InitializeTestData()
            {
                domainEmployeeLeavePlans = new Perleave(guid, "1", DateTime.Today, "0003914", "leave1")
                {
                    EndDate = DateTime.Today.AddDays(10),

                };
                domainLeavePlans = new List<LeavePlan>();
                domainLeavePlans.Add(new LeavePlan(guid, "leave1", DateTime.Today, "title", "VAC", "s", new List<string> { "VAC" })
                {
                    AllowNegative = "Y",
                    YearlyStartDate = DateTime.Today.AddDays(10),
                    RollOverLeaveType = "1",
                    AccuralFrequency = "1"
                });
                tupleEmployeeLeavePlans = new Tuple<IEnumerable<Perleave>, int>(new List<Perleave>() { domainEmployeeLeavePlans }, 1);

                leaveTypes = new List<LeaveType>();
                leaveTypes.Add(new LeaveType(guid, "VAC", "Vacation") { TimeType = LeaveTypeCategory.Vacation });
                earningTypes2 = new List<EarningType2>();
                earningTypes2.Add(new EarningType2(guid, "VAC", "Vacation"));

                List<string> earningTypeIdList = new List<string>();
                earningTypeIdList.Add("VAC");
                earningTypeIdList.Add("VAC2");

                employeeLeavePlans = new List<EmployeeLeavePlan>();
                employeeLeavePlans.Add(new EmployeeLeavePlan("foo", "0003914", DateTime.Today, null, "1", "vacation", DateTime.Today, null,
                    LeaveTypeCategory.Vacation, "VAC", "Vacation", DateTime.Today, 10.00m, 1, 1, true, earningTypeIdList, 80, 50, 50, 10, "P", false, new DateTime(2022, 1, 1), true, true));

            }

            private void InitializeMock()
            {
                employeeLeavePlanAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.EmployeeLeavePlan, Dtos.HumanResources.EmployeeLeavePlan>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(r => r.GetAdapter<Domain.HumanResources.Entities.EmployeeLeavePlan, Dtos.HumanResources.EmployeeLeavePlan>()).Returns(employeeLeavePlanAdapter);
                currentUserFactory = new CurrentUserSetup.PersonEmployeeLeavePlansUserFactory();
                getEmpLeavePlansRole.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewEmployeeLeavePlans));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getEmpLeavePlansRole });
                empLeavePlansRepositoryMock.Setup(l => l.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleEmployeeLeavePlans);
                empLeavePlansRepositoryMock.Setup(l => l.GetEmployeeLeavePlansByGuidAsync(It.IsAny<string>())).ReturnsAsync(domainEmployeeLeavePlans);
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansAsync(false)).ReturnsAsync(domainLeavePlans);
                leavePlansRepositoryMock.Setup(l => l.GetLeavePlansV2Async(false)).ReturnsAsync(domainLeavePlans);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1234");

                humanResourcesReferenceDataRepository.Setup(hrrdr => hrrdr.GetEarningTypesAsync(false)).ReturnsAsync(earningTypes2);
                humanResourcesReferenceDataRepository.Setup(hrrdr => hrrdr.GetLeaveTypesAsync(false)).ReturnsAsync(leaveTypes);

                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("0003914", guid);
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);

                var leavePlanGuidCollection = new Dictionary<string, string>();
                foreach (var lp in domainLeavePlans)
                {
                    leavePlanGuidCollection.Add(lp.Id, lp.Guid);
                }
                leavePlansRepositoryMock.Setup(p => p.GetLeavplanGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(leavePlanGuidCollection);


                empLeavePlansRepositoryMock.Setup(elp => elp.GetEmployeeLeavePlansByEmployeeIdsAsync(It.IsAny<IEnumerable<string>>(),
                    It.IsAny<IEnumerable<LeavePlan>>(), It.IsAny<IEnumerable<LeaveType>>(), It.IsAny<IEnumerable<EarningType2>>(), It.IsAny<bool>()))
                    .ReturnsAsync(employeeLeavePlans);
            }

            #endregion

            [TestMethod]
            public async Task EmployeeLeavePlansService_GetEmployeeLeavePlansAsync_Repository_Returns_Null()
            {
                empLeavePlansRepositoryMock.Setup(l => l.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(() => null);
                var result = await empLeavePlansService.GetEmployeeLeavePlansAsync(0, 100);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task EmployeeLeavePlansService_GetEmployeeLeavePlansByGuidAsync()
            {
                domainEmployeeLeavePlans = new Perleave(guid, "1", DateTime.Today, "person1", "leave1") { };
                var result = await empLeavePlansService.GetEmployeeLeavePlansByGuidAsync(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmployeeLeavePlansService_GetEmployeeLeavePlansByGuidAsync_KeyNotFoundException()
            {
                empLeavePlansRepositoryMock.Setup(l => l.GetEmployeeLeavePlansByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await empLeavePlansService.GetEmployeeLeavePlansByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EmployeeLeavePlansService_GetEmployeeLeavePlansAsync_Exception_From_Repository()
            {
                empLeavePlansRepositoryMock.Setup(l => l.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new Exception());
                await empLeavePlansService.GetEmployeeLeavePlansAsync(0, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EmployeeLeavePlansService_GetEmployeeLeavePlansAsync_Invalid_Leave()
            {
                tupleEmployeeLeavePlans = new Tuple<IEnumerable<Perleave>, int>(new List<Perleave>() { new Perleave(guid, "1", DateTime.Today, "person1", "leave1") { } }, 1);
                leavePlansRepositoryMock.Setup(r => r.GetLeavePlansAsync(false)).ReturnsAsync(() => null);
                empLeavePlansRepositoryMock.Setup(l => l.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleEmployeeLeavePlans);
                await empLeavePlansService.GetEmployeeLeavePlansAsync(0, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EmployeeLeavePlansService_GetEmployeeLeavePlansAsync_Invalid_Leave_null()
            {
                tupleEmployeeLeavePlans = new Tuple<IEnumerable<Perleave>, int>(new List<Perleave>() { new Perleave(guid, "1", DateTime.Today, "person1", "leave1") { } }, 1);
                var leavePlans = new LeavePlan("1", "leave2", DateTime.Today, "title", "1", "s");
                leavePlansRepositoryMock.Setup(r => r.GetLeavePlansAsync(false)).ReturnsAsync(new List<LeavePlan>() { leavePlans });
                empLeavePlansRepositoryMock.Setup(l => l.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleEmployeeLeavePlans);
                await empLeavePlansService.GetEmployeeLeavePlansAsync(0, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EmployeeLeavePlansService_GetEmployeeLeavePlansAsync_Invalid_Person()
            {
                tupleEmployeeLeavePlans = new Tuple<IEnumerable<Perleave>, int>(new List<Perleave>() { new Perleave(guid, "1", DateTime.Today, "invalid", "leave1") { } }, 1);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                empLeavePlansRepositoryMock.Setup(l => l.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleEmployeeLeavePlans);
                await empLeavePlansService.GetEmployeeLeavePlansAsync(0, 100);
            }

            [TestMethod]
            public async Task EmployeeLeavePlansService_GetEmployeeLeavePlansAsync()
            {
                tupleEmployeeLeavePlans = new Tuple<IEnumerable<Perleave>, int>(new List<Perleave>() { new Perleave(guid, "1", DateTime.Today, "0003914", "leave1") { } }, 1);
                empLeavePlansRepositoryMock.Setup(l => l.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tupleEmployeeLeavePlans);

                var result = await empLeavePlansService.GetEmployeeLeavePlansAsync(0, 100);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 1);
                Assert.AreEqual(result.Item1.FirstOrDefault().Id, guid);
            }

            [TestMethod]
            public async Task GetEmployeeLeavePlansV2Async_ExecutesNoErrors()
            {
                var actual = await empLeavePlansService.GetEmployeeLeavePlansV2Async();
                var expected = employeeLeavePlans[0];
                var actualDto = actual.FirstOrDefault(a => a.Id == expected.Id);

                Assert.AreEqual(expected.Id, actualDto.Id);
                Assert.AreEqual(expected.LeavePlanId, actualDto.LeavePlanId);
            }

            [TestMethod, ExpectedException(typeof(ArgumentException))]
            public async Task GetEmployeeLeavePlansV2Async_NoEarnTypesThowsError()
            {
                humanResourcesReferenceDataRepository.Setup(hrrdr => hrrdr.GetEarningTypesAsync(false)).ReturnsAsync(() => null);
                var actual = await empLeavePlansService.GetEmployeeLeavePlansV2Async();
            }

            [TestMethod]
            public async Task GetEmployeeLeavePlansV2Async_NoLeaveTypesLogsError()
            {
                humanResourcesReferenceDataRepository.Setup(hrrdr => hrrdr.GetLeaveTypesAsync(false)).ReturnsAsync(() => null);
                var actual = await empLeavePlansService.GetEmployeeLeavePlansV2Async();
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Count());
                loggerMock.Verify(m => m.Error("No leave categories defined."), Times.Once);
            }

            [TestMethod]
            public async Task GetEmployeeLeavePlansV2Async_NoPlansLogsError()
            {
                leavePlansRepositoryMock.Setup(lp => lp.GetLeavePlansV2Async(false)).ReturnsAsync(() => null);
                var actual = await empLeavePlansService.GetEmployeeLeavePlansV2Async();
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Count());
                loggerMock.Verify(m => m.Error("No leave plans defined."), Times.Once);
            }

            [TestMethod]
            public async Task GetEmployeeLeavePlansV2Async_EffectivePersonIdIsCurrentUser()
            {

            }
        }

        [TestClass]
        public class GetEmployeeLeavePlansV2 : EmployeeLeavePlansServiceTests
        {
            public EmployeeLeavePlansService serviceUnderTest;
            public string UserForAdminPermissionCheck {
                get {
                    return "0003917";
                }
            }

            public List<EmployeeLeavePlan> employeeLeavePlans = new List<EmployeeLeavePlan>()
            {
                new EmployeeLeavePlan("1", "0003914", new DateTime(2017, 1, 1), null, "VACH", "Vacation Hourly", new DateTime(2000, 1, 1), null, LeaveTypeCategory.Vacation, "VAC", "", new DateTime(2000,1,1), 20m, 1, 1, true, new List<string> { "VAC", "VAC1" }, 80, 50, 50, 10, "P", false, new DateTime(2022, 1,1),  true),
                new EmployeeLeavePlan("1", "0003915", new DateTime(2017, 1, 1), null, "VACH", "Vacation Hourly",  new DateTime(2000, 1, 1), null, LeaveTypeCategory.Vacation, "VAC", "", new DateTime(2000,1,1), 20m, 1, 1, true, new List<string> { "VAC", "VAC1" }, 80, 50, 50, 10, "P", false, new DateTime(2022, 1,1), true),
                new EmployeeLeavePlan("1", "0003916", new DateTime(2017, 1, 1), null, "VACH", "Vacation Hourly",  new DateTime(2000, 1, 1), null, LeaveTypeCategory.Vacation, "VAC", "", new DateTime(2000,1,1), 20m, 1, 1, true, new List<string> { "VAC", "VAC1" }, 80, 50, 50, 10, "P", false, new DateTime(2022, 1,1), true),
                new EmployeeLeavePlan("5", "0003917", new DateTime(2017, 1, 1), null, "SICH", "Sick Hourly",  new DateTime(2000, 1, 1), null, LeaveTypeCategory.Sick, "SIC", "", new DateTime(2000,1,1), 20m, 1, 1, true, new List<string> { "SIC", "SICK" }, 80, 50, 50, 10, "P", false, new DateTime(2022, 1,1), true),
            };

            public Mock<ISupervisorsRepository> supervisorsRepositoryMock;
            public Mock<IEmployeeLeavePlansRepository> employeeLeavePlansRepositoryMock;
            public Mock<ILeavePlansRepository> leavePlansRepositoryMock;
            public Mock<IHumanResourcesReferenceDataRepository> humanResourcesReferenceDataRepositoryMock;

            public Mock<IPersonRepository> personRepositoryMock;
            public Mock<IConfigurationRepository> configurationRepositoryMock;

            public EmployeeUserFactory employeeUserFactory;
            public SupervisorUserFactory supervisorUserFactory;
            public bool useEmployeeFactory; //default is true, when false, the currentuser mock will serve the supervisor factoru

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                supervisorsRepositoryMock = new Mock<ISupervisorsRepository>();
                employeeLeavePlansRepositoryMock = new Mock<IEmployeeLeavePlansRepository>();
                leavePlansRepositoryMock = new Mock<ILeavePlansRepository>();
                humanResourcesReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();

                var personRepositoryMock = new Mock<IPersonRepository>();
                var configurationRepositoryMock = new Mock<IConfigurationRepository>();
                var leaveBalanceConfiguratioRepositoryMock = new Mock<ILeaveBalanceConfigurationRepository>();

                serviceUnderTest = new EmployeeLeavePlansService(supervisorsRepositoryMock.Object,
                    employeeLeavePlansRepositoryMock.Object,
                    leavePlansRepositoryMock.Object,
                    humanResourcesReferenceDataRepositoryMock.Object,
                    adapterRegistryMock.Object,
                    employeeCurrentUserFactoryMock.Object,
                    roleRepositoryMock.Object,
                    personRepositoryMock.Object,
                    configurationRepositoryMock.Object,
                    leaveBalanceConfiguratioRepositoryMock.Object,
                    loggerMock.Object);

                employeeUserFactory = new EmployeeUserFactory();
                supervisorUserFactory = new SupervisorUserFactory();
                useEmployeeFactory = true;

                employeeCurrentUserFactoryMock.Setup(f => f.CurrentUser)
                    .Returns(() => useEmployeeFactory ? employeeUserFactory.CurrentUser : supervisorUserFactory.CurrentUser);
                adapterRegistryMock.Setup(ar => ar.GetAdapter<EmployeeLeavePlan, Dtos.HumanResources.EmployeeLeavePlan>())
                    .Returns(new AutoMapperAdapter<EmployeeLeavePlan, Dtos.HumanResources.EmployeeLeavePlan>(adapterRegistryMock.Object, loggerMock.Object));

                employeeLeavePlansRepositoryMock.Setup(r => r.GetEmployeeLeavePlansByEmployeeIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<LeavePlan>>(), It.IsAny<IEnumerable<LeaveType>>(), It.IsAny<IEnumerable<EarningType2>>(), It.IsAny<bool>()))
                    .Returns<IEnumerable<string>, IEnumerable<LeavePlan>, IEnumerable<LeaveType>, IEnumerable<EarningType2>, bool>(
                        (employeeIds, plans, types, earnTypes, includeLeavePlansWithNoEarningsTypes) => Task.FromResult(employeeLeavePlans.Where(plan => employeeIds.Contains(plan.EmployeeId))));

                supervisorsRepositoryMock.Setup(r => r.GetSuperviseesBySupervisorAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(employeeLeavePlans.Select(plan => plan.EmployeeId)));

                //these next three setups have no effect on the objects returned by the employeeLeavePlansRepositoryMock
                humanResourcesReferenceDataRepositoryMock.Setup(r => r.GetEarningTypesAsync(It.IsAny<bool>()))
                    .Returns<bool>(x => Task.FromResult<IEnumerable<EarningType2>>(new List<EarningType2>() { new EarningType2(Guid.NewGuid().ToString(), "VAC", "Vacation Earnings") }));

                humanResourcesReferenceDataRepositoryMock.Setup(r => r.GetLeaveTypesAsync(It.IsAny<bool>()))
                    .Returns<bool>(x => Task.FromResult<IEnumerable<LeaveType>>(new List<LeaveType>() { new LeaveType(Guid.NewGuid().ToString(), "code", "description") }));

                leavePlansRepositoryMock.Setup(r => r.GetLeavePlansV2Async(It.IsAny<bool>()))
                    .Returns<bool>(x => Task.FromResult<IEnumerable<LeavePlan>>(new List<LeavePlan>() { new LeavePlan(Guid.NewGuid().ToString(), "id", new DateTime(2017, 1, 1), "tilte", "type", "accrual", null) }));
            }

            [TestMethod]
            public async Task GetEmployeeLeavePlansV2Async_ExecutesNoErrors()
            {
                var actual = await serviceUnderTest.GetEmployeeLeavePlansV2Async();
                var expected = employeeLeavePlans.Where(e => e.EmployeeId == employeeCurrentUserFactory.CurrentUser.PersonId).ToList();

                Assert.AreEqual(expected.Count, actual.Count());
                Assert.IsTrue(actual.All(a => a.EmployeeId == employeeCurrentUserFactory.CurrentUser.PersonId));

            }

            [TestMethod, ExpectedException(typeof(ArgumentException))]
            public async Task GetEmployeeLeavePlansV2Async_NoEarnTypesThowsError()
            {
                humanResourcesReferenceDataRepositoryMock.Setup(r => r.GetEarningTypesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var actual = await serviceUnderTest.GetEmployeeLeavePlansV2Async();
            }

            [TestMethod]
            public async Task GetEmployeeLeavePlansV2Async_NoLeaveTypesLogsError()
            {
                humanResourcesReferenceDataRepositoryMock.Setup(hrrdr => hrrdr.GetLeaveTypesAsync(false)).ReturnsAsync(() => null);
                var actual = await serviceUnderTest.GetEmployeeLeavePlansV2Async();
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Count());
                loggerMock.Verify(m => m.Error("No leave categories defined."), Times.Once);
            }

            [TestMethod]
            public async Task GetEmployeeLeavePlansV2Async_NoPlansLogsError()
            {
                leavePlansRepositoryMock.Setup(lp => lp.GetLeavePlansV2Async(false)).ReturnsAsync(() => null);
                var actual = await serviceUnderTest.GetEmployeeLeavePlansV2Async();
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Count());
                loggerMock.Verify(m => m.Error("No leave plans defined."), Times.Once);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CannotGetEffectivePersonData_NoProxyAccess()
            {
                await serviceUnderTest.GetEmployeeLeavePlansV2Async("0003915");
            }

            [TestMethod]
            public async Task GetEffectivePersonData_EmployeeProxyAccess()
            {
                employeeUserFactory.ProxyClaim = new ProxySubjectClaims()
                {
                    PersonId = "0003915",
                    Permissions = new List<string>() { Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval.Value }
                };
                var actual = await serviceUnderTest.GetEmployeeLeavePlansV2Async("0003915");

                Assert.IsTrue(actual.All(a => a.EmployeeId == "0003915"));
            }

            [TestMethod]
            public async Task GetEffectivePersonData_SupervisorProxyAccess()
            {
                useEmployeeFactory = false;
                roleRepositoryMock.Setup(r => r.Roles)
                   .Returns(() => (supervisorUserFactory.CurrentUser.Roles).Select(roleTitle =>
                   {
                       var role = new Domain.Entities.Role(roleTitle.GetHashCode(), roleTitle);

                       role.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewSuperviseeData));

                       return role;
                   }));

                supervisorUserFactory.ProxyClaim = new ProxySubjectClaims()
                {
                    PersonId = "foobar", //proxying for supervisor id "foobar"
                    Permissions = new List<string>() { Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval.Value },
                };

                //supervisor "foobar" is the supervisor for all the employees in the test data. expect the service to return all employees
                var actual = await serviceUnderTest.GetEmployeeLeavePlansV2Async("foobar");

                Assert.AreEqual(employeeLeavePlans.Count, actual.Count());

            }

            [TestMethod]
            public async Task GetEmployeeLeavePlansV2Async_GetEffectivePersonData_AdminAccess()
            {

                roleRepositoryMock.Setup(r => r.Roles)
                   .Returns(() => (employeeCurrentUserFactory.CurrentUser.Roles).Select(roleTitle =>
                   {
                       var role = new Domain.Entities.Role(roleTitle.GetHashCode(), roleTitle);

                       role.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewAllTimeHistory));

                       return role;
                   }));


                //logged in user with ID"0003914" should be able to view any of the leave plans specified in employeeLeavePlans
                var actual = await serviceUnderTest.GetEmployeeLeavePlansV2Async(UserForAdminPermissionCheck);

                Assert.AreEqual(actual.Count(), 1);
                Assert.AreEqual(actual.ToList()[0].EmployeeId, UserForAdminPermissionCheck);

            }


            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetEmployeeLeavePlansV2Async_GetEffectivePersonData_NoAdminAccess()
            {
                //logged in user with ID"0003914" should be able to view any of the leave plans specified in employeeLeavePlans
                var actual = await serviceUnderTest.GetEmployeeLeavePlansV2Async(UserForAdminPermissionCheck);

            }

            [TestMethod]
            public async Task GetEmployeeLeavePlansV2Async_PerLeaveAccrualDetails()
            {
                var actual = await serviceUnderTest.GetEmployeeLeavePlansV2Async();
                var expected = employeeLeavePlans.Where(e => e.EmployeeId == employeeCurrentUserFactory.CurrentUser.PersonId).ToList();

                Assert.IsNotNull(actual.First().AccrualLimit);
                Assert.IsNotNull(actual.First().AccrualRate);
                Assert.IsNotNull(actual.First().AccrualMaxCarryOver);
                Assert.IsNotNull(actual.First().AccrualMaxRollOver);
                Assert.AreEqual(expected.First().AccrualLimit, actual.First().AccrualLimit);
                Assert.AreEqual(expected.First().AccrualRate, actual.First().AccrualRate);
                Assert.AreEqual(expected.First().AccrualMaxCarryOver, actual.First().AccrualMaxCarryOver);
                Assert.AreEqual(expected.First().AccrualMaxRollOver, actual.First().AccrualMaxRollOver);
            }
        }

    }
}