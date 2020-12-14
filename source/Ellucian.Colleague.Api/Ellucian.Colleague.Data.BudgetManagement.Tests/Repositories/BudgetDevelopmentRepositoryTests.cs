// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.BudgetManagement.DataContracts;
using Ellucian.Colleague.Data.BudgetManagement.Repositories;
using Ellucian.Colleague.Data.BudgetManagement.Transactions;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Tests;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.BudgetManagement.Tests.Repositories
{
    [TestClass]
    public class BudgetDevelopmentRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup

        public BudgetDevelopmentRepository actualRepository;
        public TestBudgetDevelopmentRepository testRepository;
        public TestBudgetDevelopmentConfigurationRepository testBuDevConfigRepository;
        public TestGeneralLedgerConfigurationRepository testGlConfigurationRepository;
        public TestGeneralLedgerAccountRepository testGlAcccountRepository;

        public DataContracts.Budget budgetRecord;
        public WorkingBudget workingBudget;
        public BudgetConfiguration budgetConfiguration;
        public List<BudgetOfficer> budgetOfficers;
        public List<BudgetReportingUnit> reportingUnits;
        public List<BudgetLineItem> budgetLineItems;

        public string workingBudgetId;
        public string personId;
        public IList<string> majorComponentStartPositions;
        public BudgetManagement.DataContracts.Staff staffDataContract;
        public Collection<Opers> opersRecords;
        public string[] budOfcrIds;
        public Collection<BudOfcr> budOfcrRecords;
        public Collection<BudOfcr> budOfcrRecordsForFy2021_0000001;
        public Collection<BudOfcr> boRecords;
        public string[] budOctlIds;
        public string[] budCtrlIds;
        public string[] budWorkIds;
        public Collection<BudOctl> budOctlRecords;
        public Collection<BudCtrl> budCtrlRecords;
        public Collection<BudWork> budWorkRecords;
        public Collection<BudResp> budRespRecords;
        public Collection<BudCtrl> budCtrlRecordsWithOfcrIds;

        public WorkingBudgetQueryCriteria criteria;
        public List<ComponentQueryCriteria> componentCriteria;
        public List<ComponentRangeQueryCriteria> rangeCriteria;

        public List<string> budgetAccountIds;
        public List<long?> budgetAmounts;
        public List<string> justificationNotes;

        private TxUpdateWorkingBudgetLineItems2Response updateBudgetLineItems2Response;

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();

            actualRepository = new BudgetDevelopmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            testRepository = new TestBudgetDevelopmentRepository();
            testBuDevConfigRepository = new TestBudgetDevelopmentConfigurationRepository();
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGlAcccountRepository = new TestGeneralLedgerAccountRepository();

            budgetRecord = testRepository.BudgetContract;
            workingBudgetId = budgetRecord.Recordkey;
            personId = "0000001";
            staffDataContract = new BudgetManagement.DataContracts.Staff();
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";

            opersRecords = new Collection<Opers>()
                {
                    new Opers()
                    {
                        Recordkey = "LOGINFORID0000001",  SysPersonId = "0000001", SysUserName = "Opers name for login 1"
                    },
                    new Opers()
                    {
                        Recordkey = "LOGINFORID0000002",  SysPersonId = "0000002", SysUserName = "Opers name for login 2"
                    }
                };

            budOfcrIds = testRepository.budOfcrRecords.Select(i => i.Recordkey).ToArray();
            budCtrlIds = testRepository.budCtrlRecords.Select(c => c.Recordkey).ToArray();
            budOfcrRecords = new Collection<BudOfcr>();
            budOfcrRecords = testRepository.budOfcrRecords;
            budOfcrRecordsForFy2021_0000001 = testRepository.budOfcrRecordsForFy2021_0000001;
            budOctlRecords = testRepository.budOctlRecords;
            budCtrlRecords = testRepository.budCtrlRecords;
            budCtrlRecordsWithOfcrIds = testRepository.budCtrlRecordsWithOfcrIds;
            budWorkRecords = testRepository.budWorkRecords;
            budRespRecords = testRepository.budRespRecords;
            budWorkIds = testRepository.budWorkRecords.Select(i => i.Recordkey).ToArray();

            componentCriteria = new List<ComponentQueryCriteria>();
            rangeCriteria = new List<ComponentRangeQueryCriteria>();
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);

            budgetAccountIds = testRepository.budgetAccountIds;
            budgetAmounts = testRepository.budgetAmounts;
            justificationNotes = testRepository.justificationNotes;

            InitializeMockStatements();
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
            testBuDevConfigRepository = null;
            budgetRecord = null;
            staffDataContract = null;
            opersRecords = null;
            reportingUnits = null;
            budOfcrIds = null;
            budCtrlIds = null;
            budWorkIds = null;
            budOfcrRecords = null;
            budOfcrRecordsForFy2021_0000001 = null;
            budOctlRecords = null;
            budCtrlRecords = null;
            budCtrlRecordsWithOfcrIds = null;
            budWorkRecords = null;
            budRespRecords = null;
            criteria = null;
            budgetAccountIds = null;
            budgetAmounts = null;
            justificationNotes = null;
        }
        #endregion

        #region GetBudgetDevelopmentWorkingBudgetAsync tests

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessAllLineItemsNoFilter()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkRecords.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var bwkDataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);
                string bwkNotes = string.Empty;
                if (bwkDataContract.BwNotes != null && bwkDataContract.BwNotes.Any())
                {
                    bwkNotes = GetJustificationNotes(bwkDataContract.BwNotes);
                }
                var bctDataContract = budCtrlRecords.FirstOrDefault(y => y.Recordkey == lineItem.BudgetReportingUnit.ReportingUnitId);
                var brspDataContract = budRespRecords.FirstOrDefault(z => z.Recordkey == lineItem.BudgetReportingUnit.ReportingUnitId);

                Assert.AreEqual(lineItem.JustificationNotes, bwkNotes);
                Assert.AreEqual(lineItem.WorkingAmount, bwkDataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerId, bwkDataContract.BwOfcrLink);
                var operRecord = opersRecords.FirstOrDefault(o => o.Recordkey == lineItem.BudgetOfficer.BudgetOfficerLogin);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerLogin, operRecord.Recordkey);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerName, operRecord.SysUserName);
                Assert.AreEqual(lineItem.BudgetReportingUnit.ReportingUnitId, bwkDataContract.BwControlLink);
                Assert.AreEqual(lineItem.BudgetReportingUnit.Description, brspDataContract.BrDesc);
                Assert.AreEqual(lineItem.BudgetReportingUnit.AuthorizationDate, bctDataContract.BcAuthDate);

                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = bwkDataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }

            // The user is budget officer 1111 and its top responsibility unit is OB.
            // The authorization date for OB is in the future.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessAllLineItemsNoFilterNoAuthDate()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            foreach (var bct in budCtrlRecords)
            {
                bct.BcAuthDate = null;
            }
            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(budCtrlRecords);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkRecords.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var bwkDataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);
                var bctDataContract = budCtrlRecords.FirstOrDefault(y => y.Recordkey == lineItem.BudgetReportingUnit.ReportingUnitId);
                var brspDataContract = budRespRecords.FirstOrDefault(z => z.Recordkey == lineItem.BudgetReportingUnit.ReportingUnitId);

                Assert.AreEqual(lineItem.WorkingAmount, bwkDataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerId, bwkDataContract.BwOfcrLink);
                var operRecord = opersRecords.FirstOrDefault(o => o.Recordkey == lineItem.BudgetOfficer.BudgetOfficerLogin);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerLogin, operRecord.Recordkey);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerName, operRecord.SysUserName);
                Assert.AreEqual(lineItem.BudgetReportingUnit.ReportingUnitId, bwkDataContract.BwControlLink);
                Assert.AreEqual(lineItem.BudgetReportingUnit.Description, brspDataContract.BrDesc);
                Assert.AreEqual(lineItem.BudgetReportingUnit.AuthorizationDate, budgetRecord.BuFinalDate);

                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = bwkDataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }

            // The user is budget officer 1111 and its top responsibility unit is OB.
            // The authorization date for all responsibility units is empty, so the 
            // final date for the budget applies which is in the future.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessAllLineItemsFilterWithOnlyGlCriteria()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            var rangeCriteria = new List<ComponentRangeQueryCriteria>()
            {
                new ComponentRangeQueryCriteria("50000", "53999")
            };

            var componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("DEPARTMENT")
                {
                    IndividualComponentValues = new List<string>() { "33333" }
                },
                new ComponentQueryCriteria("OBJECT")
                {
                    RangeComponentValues = rangeCriteria
                }
            };
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);

            budWorkIds = new List<string>() { "11_00_01_00_33333_51111", "11_00_01_00_33333_52222", "11_00_01_00_33333_53333" }.ToArray();

            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => budWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var bwkDataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);
                var bctDataContract = budCtrlRecords.FirstOrDefault(y => y.Recordkey == lineItem.BudgetReportingUnit.ReportingUnitId);
                var brspDataContract = budRespRecords.FirstOrDefault(z => z.Recordkey == lineItem.BudgetReportingUnit.ReportingUnitId);

                Assert.AreEqual(lineItem.WorkingAmount, bwkDataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerId, bwkDataContract.BwOfcrLink);
                var operRecord = opersRecords.FirstOrDefault(o => o.Recordkey == lineItem.BudgetOfficer.BudgetOfficerLogin);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerLogin, operRecord.Recordkey);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerName, operRecord.SysUserName);
                Assert.AreEqual(lineItem.BudgetReportingUnit.ReportingUnitId, bwkDataContract.BwControlLink);
                Assert.AreEqual(lineItem.BudgetReportingUnit.Description, brspDataContract.BrDesc);
                Assert.AreEqual(lineItem.BudgetReportingUnit.AuthorizationDate, bctDataContract.BcAuthDate);

                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = bwkDataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }

            // The user is budget officer 1111 and its top responsibility unit is OB.
            // The authorization date for OB is in the future.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessAllLineItemsFilterWithGlCriteriaAndBoIdsNullAuthDates()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            var rangeCriteria = new List<ComponentRangeQueryCriteria>()
            {
                new ComponentRangeQueryCriteria("50000", "54005"),
                new ComponentRangeQueryCriteria("54400","54999")
            };

            var componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("DEPARTMENT")
                {
                    IndividualComponentValues = new List<string>() { "33333", "44444" }
                },
                new ComponentQueryCriteria("OBJECT")
                {
                    RangeComponentValues = rangeCriteria
                }
            };
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);
            criteria.BudgetOfficerIds = new List<string>() { "2100", "3400", "1111" };

            // When filtering by budget officer, only return those GL accounts assigned to the budget officers
            // in the filter, only considering child reporting units if they have a budget officer in the filter.
            budWorkIds = new List<string>() { "11_00_01_00_33333_51111", "11_00_01_00_33333_52222", "11_00_01_00_33333_53333", "11_00_01_00_33333_54005",
                "11_00_01_00_33333_54400", "11_00_01_00_44444_54400" }.ToArray();

            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => budWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            var bctFIN = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_FIN").FirstOrDefault();
            bctFIN.BcAuthDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_FIN", true)).Returns(() =>
            {
                return Task.FromResult(bctFIN);
            });
            var bctFIN_IT = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_FIN_IT").FirstOrDefault();
            bctFIN_IT.BcAuthDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_FIN_IT", true)).Returns(() =>
            {
                return Task.FromResult(bctFIN_IT);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var bwkDataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);
                var bctDataContract = budCtrlRecords.FirstOrDefault(y => y.Recordkey == lineItem.BudgetReportingUnit.ReportingUnitId);
                var brspDataContract = budRespRecords.FirstOrDefault(z => z.Recordkey == lineItem.BudgetReportingUnit.ReportingUnitId);

                Assert.AreEqual(lineItem.WorkingAmount, bwkDataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerId, bwkDataContract.BwOfcrLink);
                var operRecord = opersRecords.FirstOrDefault(o => o.Recordkey == lineItem.BudgetOfficer.BudgetOfficerLogin);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerLogin, operRecord.Recordkey);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerName, operRecord.SysUserName);
                Assert.AreEqual(lineItem.BudgetReportingUnit.ReportingUnitId, bwkDataContract.BwControlLink);
                Assert.AreEqual(lineItem.BudgetReportingUnit.Description, brspDataContract.BrDesc);
                if (lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_IT")
                {
                    var bctOB = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB").FirstOrDefault();
                    Assert.AreEqual(lineItem.BudgetReportingUnit.AuthorizationDate, bctOB.BcAuthDate);
                }
                else
                {
                    Assert.AreEqual(lineItem.BudgetReportingUnit.AuthorizationDate, bctDataContract.BcAuthDate);
                }

                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = bwkDataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }

            // One of the budget officers is 1111 and its top responsibility unit is OB.
            // The authorization date for OB is in the future.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessFilterWithBoIdsNullAuthDates()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                var boc2200Records = budOctlRecords.Where(x => x.Recordkey == "2200");
                var boc3300Records = budOctlRecords.Where(x => x.Recordkey == "3300");
                var boc3400Records = budOctlRecords.Where(x => x.Recordkey == "3400");
                var boc22003300 = boc2200Records.Concat(boc3300Records);
                var boc220033003400 = boc22003300.Concat(boc3400Records);
                Collection<BudOctl> boc220033003400Records = new Collection<BudOctl>(boc220033003400.ToList());

                return Task.FromResult(boc220033003400Records);
            });

            criteria.BudgetOfficerIds = new List<string>() { "2200", "3300", "3400" };

            // When filtering by budget officer, only return those GL accounts assigned to the budget officers
            // in the filter, only considering child reporting units if they have a budget officer in the filter.
            budWorkIds = new List<string>() { "11_00_01_00_44444_54005", "11_00_01_00_44444_54006", "11_00_01_00_44444_54011", "11_00_01_00_44444_44030", "11_00_01_00_44444_54400" }.ToArray();

            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => budWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            var bctFIN_ACCT = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_FIN_ACCT").FirstOrDefault();
            bctFIN_ACCT.BcAuthDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_FIN_ACCT", true)).Returns(() =>
            {
                return Task.FromResult(bctFIN_ACCT);
            });
            var bctFIN_IT = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_FIN_IT").FirstOrDefault();
            bctFIN_IT.BcAuthDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_FIN_IT", true)).Returns(() =>
            {
                return Task.FromResult(bctFIN_IT);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var bwkDataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);
                var brspDataContract = budRespRecords.FirstOrDefault(z => z.Recordkey == lineItem.BudgetReportingUnit.ReportingUnitId);
                var bctOB_FIN = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_FIN").FirstOrDefault();

                Assert.AreEqual(lineItem.WorkingAmount, bwkDataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerId, bwkDataContract.BwOfcrLink);
                var operRecord = opersRecords.FirstOrDefault(o => o.Recordkey == lineItem.BudgetOfficer.BudgetOfficerLogin);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerLogin, operRecord.Recordkey);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerName, operRecord.SysUserName);
                Assert.AreEqual(lineItem.BudgetReportingUnit.ReportingUnitId, bwkDataContract.BwControlLink);
                Assert.AreEqual(lineItem.BudgetReportingUnit.Description, brspDataContract.BrDesc);
                Assert.AreEqual(lineItem.BudgetReportingUnit.AuthorizationDate, bctOB_FIN.BcAuthDate);

                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = bwkDataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }

            // The user is budget officer 2200, 3300 and 3400. and its top responsibility unit is OB_FIN.
            // OB_FIN does not have any budget accounts assigned, so it is not in any line item.
            // The authorization date for OB_FIN is in the past.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, true);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessAllLineItemsFilterWithBoIdsNullAuthDatesSameBranch()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            // Changing the budget officer for OB_ENROL_FA from 1111 to 3100, so BUD.OCTL needs to be updated also.
            int index1 = budOctlRecords.ToList().FindIndex(x => x.Recordkey == "1111");
            budOctlRecords[index1].BoBcId = new List<string>() { "OB" };

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            // Changing the budget officer for OB_ENROL_FA from 1111 to 3100.
            int index2 = budCtrlRecords.ToList().FindIndex(x => x.Recordkey == "OB_ENROL_FA");
            budCtrlRecords[index2].BcBofId = "3100";
            criteria.BudgetOfficerIds = new List<string>() { "2100", "3100", "3200" };

            int index3 = budWorkRecords.ToList().FindIndex(x => x.Recordkey == "11_00_01_00_33333_54A95");
            budWorkRecords[index3].BwOfcrLink = "3100";
            int index4 = budWorkRecords.ToList().FindIndex(x => x.Recordkey == "11_00_01_00_33333_54N70");
            budWorkRecords[index4].BwOfcrLink = "3100";

            // When filtering by budget officer, only return those GL accounts assigned to the budget officers
            // in the filter, only considering child reporting units if they have a budget officer in the filter.
            budWorkIds = new List<string>() { "11_00_01_00_33333_54A95", "11_00_01_00_33333_54N70", "11_00_01_00_33333_54X35", "11_00_01_00_33333_55200",
            "11_00_01_00_33333_54005", "11_00_01_00_33333_54006", "11_00_01_00_33333_54011", "11_00_01_00_33333_44030", "11_00_01_00_33333_54400"}.ToArray();

            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => budWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            var bctENROL_FA = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_ENROL_FA").FirstOrDefault();
            bctENROL_FA.BcAuthDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_ENROL_FA", true)).Returns(() =>
            {
                return Task.FromResult(bctENROL_FA);
            });
            var bctENROL_REV = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_ENROL_REV").FirstOrDefault();
            bctENROL_REV.BcAuthDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_ENROL_REV", true)).Returns(() =>
            {
                return Task.FromResult(bctENROL_REV);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var bwkDataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);
                var brspDataContract = budRespRecords.FirstOrDefault(z => z.Recordkey == lineItem.BudgetReportingUnit.ReportingUnitId);
                var bctOB_ENROL = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_ENROL").FirstOrDefault();

                Assert.AreEqual(lineItem.WorkingAmount, bwkDataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerId, bwkDataContract.BwOfcrLink);
                var operRecord = opersRecords.FirstOrDefault(o => o.Recordkey == lineItem.BudgetOfficer.BudgetOfficerLogin);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerLogin, operRecord.Recordkey);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerName, operRecord.SysUserName);
                Assert.AreEqual(lineItem.BudgetReportingUnit.ReportingUnitId, bwkDataContract.BwControlLink);
                Assert.AreEqual(lineItem.BudgetReportingUnit.Description, brspDataContract.BrDesc);
                Assert.AreEqual(lineItem.BudgetReportingUnit.AuthorizationDate, bctOB_ENROL.BcAuthDate);

                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, bwkDataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = bwkDataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }

            // The user is budget officer 2100, 3111 and 3200. and its top responsibility unit is OB_ENROL.
            // OB_ENROL has an authorization date in the future.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessAllLineItemsWithFilterAndPaging()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            var rangeCriteria = new List<ComponentRangeQueryCriteria>()
            {
                new ComponentRangeQueryCriteria("50000", "53999")
            };

            var componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("DEPARTMENT")
                {
                    IndividualComponentValues = new List<string>() { "33333" }
                },
                new ComponentQueryCriteria("OBJECT")
                {
                    RangeComponentValues = rangeCriteria
                }
            };
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);

            budWorkIds = new List<string>() { "11_00_01_00_33333_51111", "11_00_01_00_33333_52222", "11_00_01_00_33333_53333" }.ToArray();
            dataReaderMock.Setup(bw => bw.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(budWorkIds);
            });

            var pagedBudWorkIds = new List<string>() { "11_00_01_00_33333_52222", "11_00_01_00_33333_53333" }.ToArray();
            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => pagedBudWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            var bctOB = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB").FirstOrDefault();
            bctOB.BcAuthDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB", true)).Returns(() =>
            {
                return Task.FromResult(bctOB);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 1, 2);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count() - 1);
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var dataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);

                Assert.AreEqual(lineItem.WorkingAmount, dataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerId, dataContract.BwOfcrLink);
                var operRecord = opersRecords.FirstOrDefault(o => o.Recordkey == lineItem.BudgetOfficer.BudgetOfficerLogin);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerLogin, operRecord.Recordkey);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerName, operRecord.SysUserName);

                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = dataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }

            // The user is budget officer 1111, and its top responsibility unit is OB.
            // OB has a null authorization date, so the BUDGET final date applies, which is in the future.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessAllLineItemsWithFilterAndPagingRangesOutsideLimtis()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            var rangeCriteria = new List<ComponentRangeQueryCriteria>()
            {
                new ComponentRangeQueryCriteria("50000", "53999")
            };

            var componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("DEPARTMENT")
                {
                    IndividualComponentValues = new List<string>() { "33333" }
                },
                new ComponentQueryCriteria("OBJECT")
                {
                    RangeComponentValues = rangeCriteria
                }
            };
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);

            budWorkIds = new List<string>() { "11_00_01_00_33333_51111", "11_00_01_00_33333_52222", "11_00_01_00_33333_53333" }.ToArray();
            dataReaderMock.Setup(bw => bw.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(budWorkIds);
            });

            var pagedBudWorkIds = new List<string>() { "11_00_01_00_33333_52222", "11_00_01_00_33333_53333" }.ToArray();
            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => pagedBudWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            var bctOB = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB").FirstOrDefault();
            bctOB.BcAuthDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB", true)).Returns(() =>
            {
                return Task.FromResult(bctOB);
            });

            budgetRecord.BuFinalDate = DateTime.Now.AddDays(-1);
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<DataContracts.Budget>("BUDGET", workingBudgetId, true)).Returns(() =>
            {
                return Task.FromResult(budgetRecord);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, -1, -1);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count() - 1);
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var dataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);

                Assert.AreEqual(lineItem.WorkingAmount, dataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerId, dataContract.BwOfcrLink);
                var operRecord = opersRecords.FirstOrDefault(o => o.Recordkey == lineItem.BudgetOfficer.BudgetOfficerLogin);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerLogin, operRecord.Recordkey);
                Assert.AreEqual(lineItem.BudgetOfficer.BudgetOfficerName, operRecord.SysUserName);

                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = dataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }
            // The user is budget officer 1111, and its top responsibility unit is OB.
            // OB has a null authorization date, so the BUDGET final date applies, which is mocked in the past.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, true);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessBudOfcr2100LineItems()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            staffDataContract.Recordkey = "0000002";
            staffDataContract.StaffLoginId = "LOGINFORID0000002";
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            var bofIds = new List<string>() { "2100" }.ToArray();
            dataReaderMock.Setup(bo => bo.SelectAsync("BUD.OFCR", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(bofIds);
            });
            Collection<BudOfcr> bof2100Records = new Collection<BudOfcr>(budOfcrRecords.Where(x => x.Recordkey == "2100").ToList());
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOfcr>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(bof2100Records);
            });
            Collection<BudOctl> boc2100Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "2100").ToArray());
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(boc2100Records);
            });

            Collection<BudCtrl> bct2100Records = new Collection<BudCtrl>(budCtrlRecords.Where(x => x.Recordkey.Contains("OB_ENROL")).ToList());
            foreach (var bct in bct2100Records)
            {
                if (bct.Recordkey == "OB_ENROL")
                {
                    bct.BcAuthDate = null;
                }
            }
            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(bct2100Records);
            });
            var bctOB = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB").FirstOrDefault();
            bctOB.BcAuthDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB", true)).Returns(() =>
            {
                return Task.FromResult(bctOB);
            });

            budgetRecord.BuFinalDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<DataContracts.Budget>("BUDGET", workingBudgetId, true)).Returns(() =>
            {
                return Task.FromResult(budgetRecord);
            });

            var bwk2100Ids = new List<string>() { "11_00_01_00_33333_54005", "11_00_01_00_33333_54006", "11_00_01_00_33333_54011", "11_00_01_00_33333_44030", "11_00_01_00_33333_54400",
                "11_00_01_00_33333_54A95", "11_00_01_00_33333_54N70", "11_00_01_00_33333_54X35", "11_00_01_00_33333_55200" };
            var bwk2100Records = new Collection<BudWork>();
            foreach (var bwkRecord in budWorkRecords)
            {
                if (bwk2100Ids.Contains(bwkRecord.Recordkey))
                {
                    bwk2100Records.Add(bwkRecord);
                }
            }
            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(bwk2100Records);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, "0000002", majorComponentStartPositions, 0, 6);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), bwk2100Records.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, bct2100Records.SelectMany(x => x.BcWorkLineNo).Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var dataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);

                Assert.AreEqual(lineItem.WorkingAmount, dataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = dataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }

            // The user is budget officer 2111, and its top responsibility unit is OB_ENROL which has a null authorization date.
            // OB_ENROL superior is OB which also has a null authorization date, so the BUDGET final date applies, which is mocked as null.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, true);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessBudOfcr34002200LineItems()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            staffDataContract.Recordkey = "0000002";
            staffDataContract.StaffLoginId = "LOGINFORID0000002";
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });

            var bofIds = new List<string>() { "3400", "2200" }.ToArray();
            dataReaderMock.Setup(bo => bo.SelectAsync("BUD.OFCR", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(bofIds);
            });
            Collection<BudOfcr> bof3400Records = new Collection<BudOfcr>(budOfcrRecords.Where(x => x.Recordkey == "3400").ToList());
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOfcr>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(bof3400Records);
            });
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                var boc2200Records = budOctlRecords.Where(x => x.Recordkey == "2200");
                var boc3400Records = budOctlRecords.Where(x => x.Recordkey == "3400");
                var boc22003400 = boc2200Records.Concat(boc3400Records);
                Collection<BudOctl> boc22003400Records = new Collection<BudOctl>(boc22003400.ToList());

                return Task.FromResult(boc22003400Records);
            });
            var bct2200Records = budCtrlRecords.Where(x => x.Recordkey == "OB_FIN");
            bct2200Records.FirstOrDefault().BcAuthDate = null;
            var bct3400Records = budCtrlRecords.Where(x => x.Recordkey == "OB_FIN_IT");
            var bct22003400 = bct2200Records.Concat(bct3400Records);
            Collection<BudCtrl> bct22003400Records = new Collection<BudCtrl>(bct22003400.ToList());
            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(bct22003400Records);
            });
            var bwk3400Ids = new List<string>() { "11_00_01_00_44444_54011", "11_00_01_00_44444_44030", "11_00_01_00_44444_54400" };
            var bwk3400Records = new Collection<BudWork>();
            foreach (var bwkRecord in budWorkRecords)
            {
                if (bwk3400Ids.Contains(bwkRecord.Recordkey))
                {
                    bwk3400Records.Add(bwkRecord);
                }
            }
            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(bwk3400Records);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, "0000002", majorComponentStartPositions, 0, 3);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), bwk3400Records.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, bwk3400Records.Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var dataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);

                Assert.AreEqual(lineItem.WorkingAmount, dataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = dataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }

            // The user is budget officer 3400 and 2200 and its top responsibility unit is OB_FIN.
            // OB_FIN has a null authorization date, so the OB date applies, which is in the future.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_GetBudgetAuthDateForBudOfcr34002200LineItems()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            staffDataContract.Recordkey = "0000002";
            staffDataContract.StaffLoginId = "LOGINFORID0000002";
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });

            var bofIds = new List<string>() { "3400", "2200" }.ToArray();
            dataReaderMock.Setup(bo => bo.SelectAsync("BUD.OFCR", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(bofIds);
            });
            Collection<BudOfcr> bof3400Records = new Collection<BudOfcr>(budOfcrRecords.Where(x => x.Recordkey == "3400").ToList());
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOfcr>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(bof3400Records);
            });
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                var boc2200Records = budOctlRecords.Where(x => x.Recordkey == "2200");
                var boc3400Records = budOctlRecords.Where(x => x.Recordkey == "3400");
                var boc22003400 = boc2200Records.Concat(boc3400Records);
                Collection<BudOctl> boc22003400Records = new Collection<BudOctl>(boc22003400.ToList());

                return Task.FromResult(boc22003400Records);
            });
            var bct2200Records = budCtrlRecords.Where(x => x.Recordkey == "OB_FIN");
            bct2200Records.FirstOrDefault().BcAuthDate = null;
            var bct3400Records = budCtrlRecords.Where(x => x.Recordkey == "OB_FIN_IT");
            var bct22003400 = bct2200Records.Concat(bct3400Records);
            Collection<BudCtrl> bct22003400Records = new Collection<BudCtrl>(bct22003400.ToList());
            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(bct22003400Records);
            });
            var bctOB = testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB").FirstOrDefault();
            bctOB.BcAuthDate = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB", true)).Returns(() =>
            {
                return Task.FromResult(bctOB);
            });
            var bwk3400Ids = new List<string>() { "11_00_01_00_44444_54011", "11_00_01_00_44444_44030", "11_00_01_00_44444_54400" };
            var bwk3400Records = new Collection<BudWork>();
            foreach (var bwkRecord in budWorkRecords)
            {
                if (bwk3400Ids.Contains(bwkRecord.Recordkey))
                {
                    bwk3400Records.Add(bwkRecord);
                }
            }
            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(bwk3400Records);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, "0000002", majorComponentStartPositions, 0, 3);

            // The user is budget officer 3400 and 2200 and its top responsibility unit is OB_FIN.
            // OB_FIN has a null authorization date, and its superior OB is also null. The BUDGET
            // finalization date applies, which is in the future.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessFilteredReportingUnitsNoDescendants()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            // When using filter criteria, only return those GL accounts assigned to the resulting line items
            // in the filter, only considering child reporting units if they meet the criteria in the filter.
            budWorkIds = new List<string>() { "11_00_01_00_44444_54011", "11_00_01_00_44444_44030", "11_00_01_00_44444_54400" }.ToArray();

            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => budWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            criteria = new WorkingBudgetQueryCriteria(componentCriteria);
            criteria.BudgetReportingUnitIds = new List<string>() { "OB_FIN_IT", "OB_FIN" };
            criteria.IncludeBudgetReportingUnitsChildren = false;

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB_FIN and OB_FIN_IT without descendants.
            // OB_FIN does not have any line items assigned.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_IT");
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessFilteredReportingUnitsWithDescendants()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            // When using filter criteria, only return those GL accounts assigned to the resulting line items
            // in the filter, only considering child reporting units if they meet the criteria in the filter.
            budWorkIds = new List<string>() { "11_00_01_00_44444_54005", "11_00_01_00_44444_54006", "11_00_01_00_44444_54011", "11_00_01_00_44444_44030", "11_00_01_00_44444_54400" }.ToArray();

            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => budWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            criteria = new WorkingBudgetQueryCriteria(componentCriteria);
            criteria.BudgetReportingUnitIds = new List<string>() { "OB_FIN_IT", "OB_FIN" };
            criteria.IncludeBudgetReportingUnitsChildren = true;

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB_FIN and OB_FIN_IT with descendants.
            // OB_FIN does not have any line items assigned.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                    Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_IT" ||
                        lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_ACCT");
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessFilteredReportingUnitsNoDescendantsTworeportingUnits()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            // When using filter criteria, only return those GL accounts assigned to the resulting line items
            // in the filter, only considering child reporting units if they meet the criteria in the filter.
            budWorkIds = new List<string>() { "11_00_01_00_33333_54005", "11_00_01_00_33333_54006", "11_00_01_00_33333_54011", "11_00_01_00_33333_44030",
                                              "11_00_01_00_33333_54400", "11_00_01_00_33333_54A95", "11_00_01_00_33333_54N70"}.ToArray();

            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => budWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            criteria = new WorkingBudgetQueryCriteria(componentCriteria);
            criteria.BudgetReportingUnitIds = new List<string>() { "OB_ENROL_FA", "OB_ENROL" };
            criteria.IncludeBudgetReportingUnitsChildren = false;

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB_ENROL and OB_ENROL_FA without descendants.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL" || lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL_FA");
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessFilteredReportingUnitsNoDescendantsTworeportingUnitsAndDescendants()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            // When using filter criteria, only return those GL accounts assigned to the resulting line items
            // in the filter, only considering child reporting units if they meet the criteria in the filter.
            budWorkIds = new List<string>() { "11_00_01_00_33333_54005", "11_00_01_00_33333_54006", "11_00_01_00_33333_54011", "11_00_01_00_33333_44030", "11_00_01_00_33333_54400",
                                              "11_00_01_00_33333_54A95", "11_00_01_00_33333_54N70", "11_00_01_00_33333_54X35", "11_00_01_00_33333_55200"}.ToArray();

            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => budWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            criteria = new WorkingBudgetQueryCriteria(componentCriteria);
            criteria.BudgetReportingUnitIds = new List<string>() { "OB_ENROL_FA", "OB_ENROL" };
            criteria.IncludeBudgetReportingUnitsChildren = true;

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB_ENROL and OB_ENROL_FA with descendants.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL_REV"
                                || lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL"
                                || lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL_FA");
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessFilteredReportingUnitsNoDescendantsTworeportingUnitsOneHasNoItems()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            // When using filter criteria, only return those GL accounts assigned to the resulting line items
            // in the filter, only considering child reporting units if they meet the criteria in the filter.
            budWorkIds = new List<string>() { "11_00_01_00_33333_51111", "11_00_01_00_33333_52222", "11_00_01_00_33333_53333" }.ToArray();

            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => budWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            criteria = new WorkingBudgetQueryCriteria(componentCriteria);
            criteria.BudgetReportingUnitIds = new List<string>() { "OB", "OB_FIN" };
            criteria.IncludeBudgetReportingUnitsChildren = false;

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB and OB_FIN without descendants.
            // OB_FIN does not have any line items assigned.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB");
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_SuccessFilteredReportingUnitsWithDescendantsOneUnitHasNoItemsWithDescendants()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            // When using filter criteria, only return those GL accounts assigned to the resulting line items
            // in the filter, only considering child reporting units if they meet the criteria in the filter.
            budWorkIds = new List<string>() {"11_00_01_00_33333_54A95", "11_00_01_00_33333_54N70", "11_00_01_00_44444_54005", "11_00_01_00_44444_54006",
                                       "11_00_01_00_44444_54011", "11_00_01_00_44444_44030", "11_00_01_00_44444_54400" }.ToArray();

            Collection<BudWork> filteredBudWorkRecords = new Collection<BudWork>(testRepository.budWorkRecords.Where(w => budWorkIds.Contains(w.Recordkey)).ToList());

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(filteredBudWorkRecords);
            });

            criteria = new WorkingBudgetQueryCriteria(componentCriteria);
            criteria.BudgetReportingUnitIds = new List<string>() { "OB_ENROL_FA", "OB_FIN" };
            criteria.IncludeBudgetReportingUnitsChildren = true;

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB_FIN and OB with descendants.
            // OB_FIN does not have any line items assigned.
            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_IT"
                    || lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL_FA"
                    || lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_ACCT");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NullBudgetId()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            workingBudgetId = null;

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_EmptyBudgetId()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            workingBudgetId = "";

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NullPersonId()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            personId = null;

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_EmptyPersonId()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            personId = "";

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoStaffRecord()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            staffDataContract = null;
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NullLoginId()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            staffDataContract.StaffLoginId = null;
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_EmptyLoginId()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            staffDataContract.StaffLoginId = "";
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 1);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoBudgetOfficerIds()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.SelectAsync("BUD.OFCR", It.IsAny<string>())).Returns(() =>
                {
                    return Task.FromResult(null as string[]);
                });
            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 1);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_EmptyListOfBudgetOfficerIds()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.SelectAsync("BUD.OFCR", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(new string[] { });
            });
            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 1);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoBudgetOfficerRecords()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            budOfcrRecords = new Collection<BudOfcr>();
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOfcr>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(budOfcrRecords);
            });
            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoBudgetOfficerRecordsForLogin()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            staffDataContract.StaffLoginId = "NOMATCHLOGIN";
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoBudOctlRecords()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(null as Collection<BudOctl>);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoBudOctlRespIds()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            Collection<BudOctl> bocNoRespIds = new Collection<BudOctl>(budOctlRecords.Select(x => { x.BoBcId = new List<String>(); return x; }).ToArray());
            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(bocNoRespIds);
            });
            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoBudCtrlRecords()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(null as Collection<BudCtrl>);
            });

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(null as Collection<BudWork>);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoBudCtrlWorkLineNos()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            Collection<BudCtrl> bctNoRecords = new Collection<BudCtrl>(budCtrlRecords.Select(x => { x.BcWorkLineNo = new List<String>(); return x; }).ToArray());
            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(bctNoRecords);
            });
            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoBudWorkRecords()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(null as Collection<BudWork>);
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_ZeroBudWorkRecords()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(new Collection<BudWork>());
            });

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoExpenseAccountInOneBudWork()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            budWorkRecords.FirstOrDefault().BwExpenseAcct = "";

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkRecords.Count() - 1);
            Assert.AreEqual(workingBudget.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var dataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);

                Assert.AreEqual(lineItem.WorkingAmount, dataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp5Amount);
                            break;
                    }
                }

                var baseAmountRow = dataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoBaseVersionInOneBudWork()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            budWorkRecords.FirstOrDefault().BwfreezeEntityAssociation = null;

            workingBudget = await actualRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, majorComponentStartPositions, 0, 999);

            Assert.AreEqual(workingBudget.BudgetLineItems.Count(), budWorkRecords.Count());
            Assert.AreEqual(workingBudget.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            foreach (var lineItem in workingBudget.BudgetLineItems)
            {
                var dataContract = budWorkRecords.FirstOrDefault(x => x.Recordkey == lineItem.BudgetAccountId);

                Assert.AreEqual(lineItem.WorkingAmount, dataContract.BwLineAmt);
                Assert.AreEqual(lineItem.BudgetComparables.Count(), budgetConfiguration.BudgetConfigurationComparables.Count());
                foreach (var comparable in lineItem.BudgetComparables)
                {
                    switch (comparable.ComparableNumber)
                    {
                        case "C1":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp1Amount);
                            break;
                        case "C2":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp2Amount);
                            break;
                        case "C3":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp3Amount);
                            break;
                        case "C4":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp4Amount);
                            break;
                        case "C5":
                            Assert.AreEqual(comparable.ComparableAmount, dataContract.BwComp5Amount);
                            break;
                    }
                }

                if (dataContract.BwfreezeEntityAssociation == null)
                {
                    Assert.AreEqual(lineItem.BaseBudgetAmount, 0);
                }
                else
                {
                    var baseAmountRow = dataContract.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                    var baseAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                    Assert.AreEqual(lineItem.BaseBudgetAmount, baseAmount);
                }
            }
        }
        #endregion

        #region UpdateBudgetDevelopmentBudgetLineItemsAsync tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateBudgetDevelopmentBudgetLineItemsAsync_NullBudgetId()
        {
            workingBudgetId = null;
            budgetLineItems = await actualRepository.UpdateBudgetDevelopmentBudgetLineItemsAsync(personId, workingBudgetId, budgetAccountIds, budgetAmounts, justificationNotes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateBudgetDevelopmentBudgetLineItemsAsync_NullBudgetLineItemAccountIds()
        {
            budgetAccountIds = null;
            budgetLineItems = await actualRepository.UpdateBudgetDevelopmentBudgetLineItemsAsync(personId, workingBudgetId, budgetAccountIds, budgetAmounts, justificationNotes);
            Assert.AreEqual(budgetLineItems.Count(), 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateBudgetDevelopmentBudgetLineItemsAsync_MismatchBudgetLineItemAmounts()
        {
            budgetAmounts = null;
            budgetLineItems = await actualRepository.UpdateBudgetDevelopmentBudgetLineItemsAsync(personId, workingBudgetId, budgetAccountIds, budgetAmounts, justificationNotes);
            Assert.AreEqual(budgetLineItems.Count(), 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateBudgetDevelopmentBudgetLineItemsAsync_MismatchBudgetLineItemNotes()
        {
            justificationNotes = new List<string>()
            {
                "Note one."
            };
            budgetLineItems = await actualRepository.UpdateBudgetDevelopmentBudgetLineItemsAsync(personId, workingBudgetId, budgetAccountIds, budgetAmounts, justificationNotes);
            Assert.AreEqual(budgetLineItems.Count(), 0);
        }

        [TestMethod]
        public async Task UpdateBudgetDevelopmentBudgetLineItemsAsync_Success()
        {
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            budgetLineItems = await actualRepository.UpdateBudgetDevelopmentBudgetLineItemsAsync(personId, workingBudgetId, budgetAccountIds, budgetAmounts, justificationNotes);

            Assert.AreEqual(budgetLineItems.Count(), budgetAccountIds.Count());
            foreach (var item in budgetLineItems)
            {
                Assert.IsTrue(budgetAccountIds.Contains(item.BudgetAccountId));
            }
        }
        #endregion

        #region GetBudgetDevelopmentBudgetLineItemsAsync tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentBudgetLineItemsAsync_NullBudgetId()
        {
            workingBudgetId = null;
            budgetLineItems = await actualRepository.GetBudgetDevelopmentBudgetLineItemsAsync(workingBudgetId, personId, budgetAccountIds);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentBudgetLineItemsAsync_NullPersonId()
        {
            personId = null;
            budgetLineItems = await actualRepository.GetBudgetDevelopmentBudgetLineItemsAsync(workingBudgetId, personId, budgetAccountIds);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentBudgetLineItemsAsync_NullBudgetAccountIds()
        {
            budgetAccountIds = null;
            budgetLineItems = await actualRepository.GetBudgetDevelopmentBudgetLineItemsAsync(workingBudgetId, personId, budgetAccountIds);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentBudgetLineItemsAsync_EmptyListBudgetAccountIds()
        {
            budgetAccountIds = new List<string>();
            budgetLineItems = await actualRepository.GetBudgetDevelopmentBudgetLineItemsAsync(workingBudgetId, personId, budgetAccountIds);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentBudgetLineItemsAsync_Success()
        {
            budgetLineItems = await actualRepository.GetBudgetDevelopmentBudgetLineItemsAsync(workingBudgetId, personId, budgetAccountIds);
            Assert.AreEqual(budgetLineItems.Count(), budgetAccountIds.Count());

            foreach (var item in budgetLineItems)
            {
                Assert.IsTrue(budgetAccountIds.Contains(item.BudgetAccountId));
            }
        }
        #endregion

        #region GetBudgetDevelopmentBudgetOfficersAsync tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentBudgetOfficersAsync_NullBudgetId()
        {
            workingBudgetId = null;
            budgetOfficers = await actualRepository.GetBudgetDevelopmentBudgetOfficersAsync(workingBudgetId, personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentBudgetOfficersAsync_EmptyBudgetId()
        {
            workingBudgetId = "";
            budgetOfficers = await actualRepository.GetBudgetDevelopmentBudgetOfficersAsync(workingBudgetId, personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentBudgetOfficersAsync_NullPersonId()
        {
            personId = null;
            budgetOfficers = await actualRepository.GetBudgetDevelopmentBudgetOfficersAsync(workingBudgetId, personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentBudgetOfficersAsync_EmptyPersonId()
        {
            personId = "";
            budgetOfficers = await actualRepository.GetBudgetDevelopmentBudgetOfficersAsync(workingBudgetId, personId);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentBudgetOfficersAsync_NoStaffRecord()
        {
            staffDataContract = null;
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            budgetOfficers = await actualRepository.GetBudgetDevelopmentBudgetOfficersAsync(workingBudgetId, personId);
            Assert.AreEqual(budgetOfficers.Count(), 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentBudgetOfficersAsync_NullLoginId()
        {
            staffDataContract.StaffLoginId = null;
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            budgetOfficers = await actualRepository.GetBudgetDevelopmentBudgetOfficersAsync(workingBudgetId, personId);
            Assert.AreEqual(budgetOfficers.Count(), 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentBudgetOfficersAsync_EmptyLoginId()
        {
            staffDataContract.StaffLoginId = "";
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            budgetOfficers = await actualRepository.GetBudgetDevelopmentBudgetOfficersAsync(workingBudgetId, personId);
            Assert.AreEqual(budgetOfficers.Count(), 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentBudgetOfficersAsync_Success()
        {
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(budCtrlRecordsWithOfcrIds);
            });

            budgetOfficers = await actualRepository.GetBudgetDevelopmentBudgetOfficersAsync(workingBudgetId, personId);

            Assert.AreEqual(budgetOfficers.Count(), budOfcrRecordsForFy2021_0000001.Count());


            foreach (var ofcr in budgetOfficers)
            {
                var dataContract = budOfcrRecords.FirstOrDefault(x => x.Recordkey == ofcr.BudgetOfficerId);

                var operRecord = opersRecords.FirstOrDefault(o => o.Recordkey == ofcr.BudgetOfficerLogin);
                Assert.AreEqual(ofcr.BudgetOfficerLogin, operRecord.Recordkey);
                Assert.AreEqual(ofcr.BudgetOfficerName, operRecord.SysUserName);

            }
        }
        #endregion

        #region GetBudgetDevelopmentBudgetResportingUnitsAsync tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_NullBudgetId()
        {
            reportingUnits = await actualRepository.GetBudgetDevelopmentReportingUnitsAsync(null, personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_EmptyBudgetId()
        {
            reportingUnits = await actualRepository.GetBudgetDevelopmentReportingUnitsAsync("", personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_NullPersonId()
        {
            reportingUnits = await actualRepository.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_EmptyPersonId()
        {
            reportingUnits = await actualRepository.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, "");
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_NoStaffRecord()
        {
            staffDataContract = null;
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            reportingUnits = await actualRepository.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, personId);
            Assert.AreEqual(reportingUnits.Count(), 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_NullLoginId()
        {
            staffDataContract.StaffLoginId = null;
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            reportingUnits = await actualRepository.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, personId);
            Assert.AreEqual(reportingUnits.Count(), 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_EmptyLoginId()
        {
            staffDataContract.StaffLoginId = "";
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            reportingUnits = await actualRepository.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, personId);
            Assert.AreEqual(reportingUnits.Count(), 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_Success()
        {
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            var bctSuiteFileName = "BCT." + workingBudgetId;
            dataReaderMock.Setup(bc => bc.SelectAsync(bctSuiteFileName, It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(budCtrlIds);
            });
            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(budCtrlRecords);
            });

            reportingUnits = await actualRepository.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, personId);

            Assert.AreEqual(reportingUnits.Count(), budCtrlRecords.Count());
            foreach (var unit in reportingUnits)
            {
                var dataContract = budCtrlRecords.FirstOrDefault(x => x.Recordkey == unit.ReportingUnitId);
                var brspRecord = budRespRecords.FirstOrDefault(o => o.Recordkey == unit.ReportingUnitId);
                Assert.AreEqual(unit.ReportingUnitId, dataContract.Recordkey);
                Assert.AreEqual(unit.AuthorizationDate, null);
                Assert.AreEqual(unit.Description, brspRecord.BrDesc);

            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_NoBocRecords()
        {
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(new Collection<BudOctl>());
            });

            reportingUnits = await actualRepository.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, personId);

            Assert.AreEqual(reportingUnits.Count(), 0);
        }
        #endregion

        #region Private methods

        private void InitializeMockStatements()
        {
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudgetDevDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.testBuDevConfigRepository.BudgetDevDefaultsContract);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<DataContracts.Budget>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(budgetRecord);
            });

            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });

            dataReaderMock.Setup<Task<Collection<Opers>>>(op => op.BulkReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string[]>(), true)).Returns(() =>
             {
                 return Task.FromResult(opersRecords);
             });

            dataReaderMock.Setup(bo => bo.SelectAsync("BUD.OFCR", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(budOfcrIds);
            });

            dataReaderMock.Setup(bw => bw.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(budWorkIds);
            });

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOfcr>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(budOfcrRecords);
            });

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(budOctlRecords);
            });

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(budWorkRecords);
            });

            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(budCtrlRecords);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB", true)).Returns(() =>
            {
                return Task.FromResult(testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB").FirstOrDefault());
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_ENROL", true)).Returns(() =>
            {
                return Task.FromResult(testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_ENROL").FirstOrDefault());
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_ENROL_FA", true)).Returns(() =>
            {
                return Task.FromResult(testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_ENROL_FA").FirstOrDefault());
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_ENROL_REV", true)).Returns(() =>
            {
                return Task.FromResult(testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_ENROL_REV").FirstOrDefault());
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_FIN", true)).Returns(() =>
            {
                return Task.FromResult(testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_FIN").FirstOrDefault());
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_FIN_ACCT", true)).Returns(() =>
            {
                return Task.FromResult(testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_FIN_ACCT").FirstOrDefault());
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudCtrl>(It.IsAny<string>(), "OB_FIN_IT", true)).Returns(() =>
            {
                return Task.FromResult(testRepository.budCtrlRecords.Where(i => i.Recordkey == "OB_FIN_IT").FirstOrDefault());
            });

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(budWorkRecords);
            });

            dataReaderMock.Setup(br => br.BulkReadRecordAsync<BudResp>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(budRespRecords);
            });

            updateBudgetLineItems2Response = new TxUpdateWorkingBudgetLineItems2Response()
            {
                budgetLineItems2 = new List<budgetLineItems2>()
                {
                    new budgetLineItems2()
                    {
                        AlLineGlNo = "11_00_01_00_33333_51111",
                        AlLineNewAmt = 50000,
                        AlLineNotes = "note one."
                    },
                    new budgetLineItems2()
                    {
                        AlLineGlNo = "11_00_01_00_33333_52222",
                        AlLineNewAmt = 80000,
                        AlLineNotes = "note two."
                    }
                }
            };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxUpdateWorkingBudgetLineItems2Request, TxUpdateWorkingBudgetLineItems2Response>(It.IsAny<TxUpdateWorkingBudgetLineItems2Request>())).Returns(() =>
            {
                return Task.FromResult(updateBudgetLineItems2Response);
            });
        }

        private string GetJustificationNotes(List<string> bwkJustificationNotes)
        {
            var notes = string.Empty;
            var justificationNotes = new StringBuilder();
            foreach (var note in bwkJustificationNotes)
            {
                if (string.IsNullOrWhiteSpace(note))
                {
                    justificationNotes.Append(Environment.NewLine + Environment.NewLine);
                }
                else
                {
                    if (justificationNotes.Length > 0)
                    {
                        justificationNotes.Append(" ");
                    }
                    justificationNotes.Append(note);
                }
            }

            notes = justificationNotes.ToString();
            return notes;
        }

        #endregion
    }
}