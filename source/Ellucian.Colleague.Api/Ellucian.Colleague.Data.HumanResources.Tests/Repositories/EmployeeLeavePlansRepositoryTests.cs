using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class EmployeeLeavePlansRepositoryTests : BaseRepositorySetup
    {
        [TestClass]
        public class EmployeeLeavePlansRepositoryTests_V11_GET_AND_GETALL : EmployeeLeavePlansRepositoryTests
        {
            #region DECLARATIONS

            private EmployeeLeavePlansRepository empLeavePlansRepository;
            private Dictionary<string, GuidLookupResult> dicResult;
            private Tuple<IEnumerable<Domain.HumanResources.Entities.Perleave>, int> tupleEmployeeLeavePlans;
            private List<string> perleaveKeys;
            private DataContracts.Perleave perLeaveData;
            private Collection<DataContracts.Perleave> empLeavePlans;
            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";
            private List<LeaveType> expectedLeaveTypes;
            private List<EarningType2> expectedEarningTypes;
            private List<LeavePlan> expectedLeavePlans;
            private List<string> employeeIds;
            private List<string> PerleaveDetailsKeys;
            private Perlvdtl perLeaveDetailsData;
            private Collection<Perlvdtl> empLeaveTrans;
            private GuidLookupResult guidLookupResult;
            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                InitializeTestData();
                empLeavePlansRepository = new EmployeeLeavePlansRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "PERLEAVE", PrimaryKey = "1" } } };
                perleaveKeys = new List<string>() { "1" };

                perLeaveData = new DataContracts.Perleave()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    PerlvHrpId = "person1",
                    PerlvStartDate = DateTime.Today,
                    PerlvAllowedDate = DateTime.Today.AddDays(10),

                    PerlvEndDate = DateTime.Today.AddDays(100),
                    PerlvLpnId = "leave1",

                };

                empLeavePlans = new Collection<DataContracts.Perleave>() { perLeaveData };

                expectedLeavePlans = new List<LeavePlan>();
                expectedLeaveTypes = new List<LeaveType>();
                expectedEarningTypes = new List<EarningType2>();
                expectedLeavePlans.Add(new LeavePlan(guid, "leave1", DateTime.Today, "Vacation", "VAC", "H", new List<string>() { "VAC" }));
                expectedLeaveTypes.Add(new LeaveType(guid, "VAC", "Vacation") { TimeType = LeaveTypeCategory.Vacation });
                expectedEarningTypes.Add(new EarningType2(guid, "VAC", "Vacation"));

                employeeIds = new List<string>() { "person1" };

                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.SelectAsync("PERLEAVE", It.IsAny<string>())).ReturnsAsync(perleaveKeys.ToArray());
                dataReaderMock.Setup(r => r.SelectAsync("PERLEAVE", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(perleaveKeys.ToArray());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<DataContracts.Perleave>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(empLeavePlans);
                dataReaderMock.Setup(r => r.ReadRecordAsync<DataContracts.Perleave>("PERLEAVE", It.IsAny<string>(), true)).ReturnsAsync(perLeaveData);

                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "PERLVDTL", PrimaryKey = "1" } } };
                PerleaveDetailsKeys = new List<string>() { "1" };

                perLeaveDetailsData = new Perlvdtl()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    PldHrpId = "person1",
                    PldDate = DateTime.Today,
                    PldHours = 20,
                    PldPerleaveId = "1",
                    PldForwardingBalance = 50,
                };

                empLeaveTrans = new Collection<Perlvdtl>() { perLeaveDetailsData };

                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.SelectAsync("PERLVDTL", It.IsAny<string>())).ReturnsAsync(PerleaveDetailsKeys.ToArray());
                dataReaderMock.Setup(r => r.SelectAsync("PERLVDTL", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(PerleaveDetailsKeys.ToArray());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Perlvdtl>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(empLeaveTrans);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Perlvdtl>("PERLVDTL", It.IsAny<string>(), true)).ReturnsAsync(perLeaveDetailsData);
            }
            #endregion

            [TestMethod]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansAsync_DataReader_Returns_Null()
            {
                dataReaderMock.Setup(r => r.SelectAsync("PERLEAVE", It.IsAny<string>())).ReturnsAsync(null);
                var result = await empLeavePlansRepository.GetEmployeeLeavePlansAsync(0, 1);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansAsync_DataReader_Empty()
            {
                dataReaderMock.Setup(r => r.SelectAsync("PERLEAVE", It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await empLeavePlansRepository.GetEmployeeLeavePlansAsync(0, 1);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansAsync_SelectAsync_Exception()
            {
                dataReaderMock.Setup(r => r.SelectAsync("PERLEAVE", It.IsAny<string>())).ThrowsAsync(new Exception());
                await empLeavePlansRepository.GetEmployeeLeavePlansAsync(0, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansAsync_BulkReadRecord_Excep()
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<DataContracts.Perleave>(It.IsAny<string[]>(), false)).ThrowsAsync(new Exception());
                await empLeavePlansRepository.GetEmployeeLeavePlansAsync(0, 1);
            }

            [TestMethod]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansAsync()
            {
                var result = await empLeavePlansRepository.GetEmployeeLeavePlansAsync(0, 1);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(guid, result.Item1.FirstOrDefault().Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansByIdAsync_GetRecordInfoFromGuidAsync_Null()
            {
                dicResult[guid].Entity = null;
                await empLeavePlansRepository.GetEmployeeLeavePlansByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansByIdAsync_GetRecordInfoFromGuidAsync_Invalid()
            {
                dicResult[guid].Entity = "PERLEAV";
                await empLeavePlansRepository.GetEmployeeLeavePlansByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansByIdAsync_RecorGuidAsync_Inv_Key()
            {
                dicResult[guid].PrimaryKey = null;
                await empLeavePlansRepository.GetEmployeeLeavePlansByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansByIdAsync_ReadRecordAsync_Record_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<DataContracts.Perleave>("PERLEAVE", It.IsAny<string>(), true)).ReturnsAsync(null);
                await empLeavePlansRepository.GetEmployeeLeavePlansByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansByGuidAsync_ReadRecordAsync_Record_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<DataContracts.Perleave>("PERLEAVE", It.IsAny<string>(), true)).ReturnsAsync(null);
                await empLeavePlansRepository.GetEmployeeLeavePlansByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmployeeLeavePlanRepository_GetEmployeeLeavePlansByIdAsync_ReadRecordAsync_Id_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<DataContracts.Perleave>("PERLEAVE", It.IsAny<string>(), true)).ReturnsAsync(null);
                await empLeavePlansRepository.GetEmployeeLeavePlansByIdAsync(string.Empty);
            }

            [TestMethod]
            public async Task EmployeeLeaveRepository_GetEmployeeLeavePlansByIdAsync()
            {
                var result = await empLeavePlansRepository.GetEmployeeLeavePlansByIdAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Guid);
            }

            //[TestMethod]
            //public async Task EmployeeLeaveRepository_GetEmployeeLeavePlansByGuidAsync()
            //{
                
            //    var result = await empLeavePlansRepository.GetEmployeeLeavePlansByGuidAsync(guid);

            //    Assert.IsNotNull(result);
            //    Assert.AreEqual(guid, result.Guid);
            //}


        }


        public TestEmployeeLeavePlanRepository testData;

        public void BaseInitialize()
        {
            MockInitialize();

            testData = new TestEmployeeLeavePlanRepository();

        }

        public EmployeeLeavePlansRepository repositoryUnderTest
        {
            get
            {
                return new EmployeeLeavePlansRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }
        }


        [TestClass]
        public class GetEmployeeLeavePlansByEmployeeIdTests : EmployeeLeavePlansRepositoryTests
        {

            public List<string> employeeIds
            {
                get
                {
                    return testData.employeeLeavePlanRecords.Select(r => r.personId).ToList();
                }
            }

            public async Task<IEnumerable<EmployeeLeavePlan>> getActual(IEnumerable<string> inputEmployeeIds = null)
            {
                return await repositoryUnderTest.GetEmployeeLeavePlansByEmployeeIdsAsync(inputEmployeeIds == null ? employeeIds : inputEmployeeIds, testData.leavePlans, testData.leaveTypes, testData.earnTypes);
            }

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                dataReaderMock.Setup(dr => dr.SelectAsync("PERLEAVE", "WITH PERLV.HRP.ID.INDEX EQ '?'", It.IsAny<string[]>(), "?", true, 425))
                    .Returns<string, string, string[], string, bool, int>((a, b, c, d, e, f) => Task.FromResult(testData.employeeLeavePlanRecords.Select(r => r.id).ToArray()));
                dataReaderMock.Setup(dr => dr.SelectAsync("PERLVDTL", "WITH PLD.HRP.ID EQ '?'", It.IsAny<string[]>(), "?", true, 425))
                    .Returns<string, string, string[], string, bool, int>((a, b, c, d, e, f) => Task.FromResult(testData.leaveTransactionRecords.Select(r => r.id).ToArray()));

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<DataContracts.Perleave>(It.IsAny<string[]>(), true))
                    .Returns<string[], bool>((keys, b) => Task.FromResult(new Collection<DataContracts.Perleave>(testData.employeeLeavePlanRecords.Select(r =>
                        new DataContracts.Perleave()
                        {
                            Recordkey = r.id,
                            PerlvAllowedDate = r.allowedDate,
                            PerlvBalance = r.balance,
                            PerlvEndDate = r.endDate,
                            PerlvHrpId = r.personId,
                            PerlvLpnId = r.leavePlanId,
                            PerlvStartDate = r.startDate,
                        }).ToList())));

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<Perlvdtl>(It.IsAny<string[]>(), true))
                    .Returns<string[], bool>((keys, b) => Task.FromResult(new Collection<Perlvdtl>(testData.leaveTransactionRecords.Select(r =>
                        new Perlvdtl()
                        {
                            Recordkey = r.id,
                            PldAction = r.action,
                            PldDate = r.date,
                            PldForwardingBalance = r.forwardingBalance,
                            PldHours = r.hours,
                            PldHrpId = r.personId,
                            PldLpnId = r.leavePlanId,
                            PldPerleaveId = r.employeeLeavePlanId
                        }).ToList())));

            }

            [TestMethod]
            public async Task GetEmployeeLeavePlansTest()
            {
                var actual = await getActual();

                Assert.AreEqual(testData.employeeLeavePlanRecords.Count, actual.Count());
                foreach (var plan in actual)
                {
                    var employeePlan = testData.employeeLeavePlanRecords.First(elp => elp.id == plan.Id);
                    var leavePlan = testData.leavePlans.FirstOrDefault(lp => lp.Id == plan.LeavePlanId);
                    var leaveType = testData.leaveTypes.FirstOrDefault(lt => lt.Code == leavePlan.Type);
                    var earnType = testData.earnTypes.FirstOrDefault(et => et.Code == plan.EarningsTypeId);
                    var details = testData.leaveTransactionRecords.Where(lt => lt.employeeLeavePlanId == plan.Id);

                    Assert.AreEqual(employeePlan.id, plan.Id);
                    Assert.AreEqual(employeePlan.personId, plan.EmployeeId);
                    Assert.AreEqual(employeePlan.startDate.Value, plan.StartDate);
                    Assert.AreEqual(employeePlan.endDate, plan.EndDate);
                    Assert.AreEqual(employeePlan.leavePlanId, plan.LeavePlanId);
                    Assert.AreEqual(leavePlan.Title, plan.LeavePlanDescription);
                    Assert.AreEqual(leavePlan.StartDate.Value, plan.LeavePlanStartDate);
                    Assert.AreEqual(leavePlan.EndDate, plan.LeavePlanEndDate);
                    Assert.AreEqual(leaveType.TimeType, plan.LeavePlanTypeCategory);
                    Assert.AreEqual(earnType.Code, leavePlan.EarningsTypes.First());
                    Assert.AreEqual(earnType.Description, plan.EarningsTypeDescription);

                    if (employeePlan.allowedDate.HasValue)
                    {
                        Assert.AreEqual(employeePlan.allowedDate.Value, plan.LeaveAllowedDate);
                    }
                    else
                    {
                        Assert.AreEqual(employeePlan.startDate.Value, plan.LeaveAllowedDate);
                    }

                    var expectedBalance = employeePlan.balance.HasValue ? employeePlan.balance.Value : 0;
                    Assert.AreEqual(expectedBalance, plan.PriorPeriodLeaveBalance);

                    var expectedStartMonth = leavePlan.YearlyStartDate.HasValue ? leavePlan.YearlyStartDate.Value.Month : 1;
                    var expectedStartDay = leavePlan.YearlyStartDate.HasValue ? leavePlan.YearlyStartDate.Value.Day : 1;

                    Assert.AreEqual(expectedStartMonth, plan.CurrentPlanYearStartDate.Month);
                    Assert.AreEqual(expectedStartDay, plan.CurrentPlanYearStartDate.Day);

                    var expectedAllowNegative = !string.IsNullOrWhiteSpace(leavePlan.AllowNegative) && !leavePlan.AllowNegative.Equals("N", StringComparison.CurrentCultureIgnoreCase);
                    Assert.AreEqual(expectedAllowNegative, plan.AllowNegativeBalance);

                    Assert.AreEqual(details.Count(), plan.SortedLeaveTransactions.Count);

                    foreach (var transaction in plan.SortedLeaveTransactions)
                    {
                        var detail = details.FirstOrDefault(d => d.id == transaction.Id.ToString());
                        Assert.AreEqual(detail.leavePlanId, transaction.LeavePlanDefinitionId);
                        Assert.AreEqual(detail.employeeLeavePlanId, transaction.EmployeeLeavePlanId);

                        var expectedDate = detail.date.ToPointInTimeDateTimeOffset(detail.date, apiSettings.ColleagueTimeZone);
                        Assert.AreEqual(expectedDate, transaction.Date);

                        var expectedHours = detail.hours ?? 0;
                        Assert.AreEqual(expectedHours, transaction.TransactionHours);
                        if (detail.action == "A")
                        {
                            Assert.AreEqual(LeaveTransactionType.Earned, transaction.Type);
                        }
                        else if (detail.action == "U")
                        {
                            Assert.AreEqual(LeaveTransactionType.Used, transaction.Type);
                        }
                        else if (detail.action == "J")
                        {
                            Assert.AreEqual(LeaveTransactionType.Adjusted, transaction.Type);
                        }

                        var expectedForwardingBalance = detail.forwardingBalance ?? 0;
                        Assert.AreEqual(expectedForwardingBalance, transaction.ForwardingBalance);
                    }
                }

            }



            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task EmployeeIdsRequiredTest()
            {

                var actual = await getActual(new List<string>());
            }

            [TestMethod]
            public async Task NoPerLeaveKeysTest()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("PERLEAVE", "WITH PERLV.HRP.ID.INDEX EQ '?'", It.IsAny<string[]>(), "?", true, 425))
                    .ReturnsAsync(null);
                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task NoLeavePlansTest()
            {
                testData.leavePlans = null;
                var actual = await getActual();
                Assert.IsFalse(actual.Any());
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task NoEarnTypesTest()
            {
                testData.earnTypes = null;
                var actual = await getActual();
                Assert.IsFalse(actual.Any());
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task NoLeaveTypesTest()
            {
                testData.leaveTypes = null;
                var actual = await getActual();
                Assert.IsTrue(actual.All(lp => lp.LeavePlanTypeCategory == LeaveTypeCategory.None));

            }

           

            [TestMethod]
            public async Task UndefinedLeavePlanOnEmployeePlanTest()
            {
                testData.employeeLeavePlanRecords.Add(new TestEmployeeLeavePlanRepository.EmployeeLeavePlanRecord()
                {
                    id = "123456789",
                    leavePlanId = "FOOBAR",
                    startDate = new DateTime(2018, 1, 1),
                    balance = 10,
                    personId = employeeIds[0]
                });

                var actual = await getActual();
                Assert.IsFalse(actual.Any(lp => lp.Id == "123456789"));
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));

            }

            [TestMethod]
            public async Task TimeTypeComesFromLeaveTypeTest()
            {
                var actual = await getActual();
                foreach (var plan in actual)
                {
                    var leavePlan = testData.leavePlans.First(p => p.Id == plan.LeavePlanId);
                    var leaveType = testData.leaveTypes.FirstOrDefault(t => t.Code == leavePlan.Type);
                    Assert.AreEqual(leaveType.TimeType, plan.LeavePlanTypeCategory);
                }
            } 

            [TestMethod]
            public async Task NoStartDateTest()
            {
                testData.employeeLeavePlanRecords.ForEach(lp => lp.startDate = null);
                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }


            [TestMethod]
            public async Task NoMatchingDetailsTest()
            {
                testData.leaveTransactionRecords.ForEach(lt => lt.employeeLeavePlanId = "foo");
                var actual = await getActual();

                Assert.AreEqual(0, actual.Sum(a => a.SortedLeaveTransactions.Count));
            }

            [TestMethod]
            public async Task NoTransactionDateTest()
            {
                testData.leaveTransactionRecords.ForEach(lt => lt.date = null);
                var actual = await getActual();
                Assert.AreEqual(0, actual.Sum(a => a.SortedLeaveTransactions.Count));
            }

            [TestMethod]
            public async Task ErrorBuildingTransactionsTest()
            {
                testData.leaveTransactionRecords.ForEach(lt => lt.leavePlanId = null);
                var actual = await getActual();
                Assert.AreEqual(0, actual.Sum(a => a.SortedLeaveTransactions.Count));

            }



        }
    }

}

