// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.BudgetManagement.DataContracts;
using Ellucian.Colleague.Data.BudgetManagement.Repositories;
using Ellucian.Colleague.Data.BudgetManagement.Transactions;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Tests;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
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
    public class WorkingBudget2RepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup

        public BudgetDevelopmentRepository actualRepository;
        public TestWorkingBudget2Repository testRepository;
        public TestBudgetDevelopmentConfigurationRepository testBuDevConfigRepository;
        public TestGeneralLedgerConfigurationRepository testGlConfigurationRepository;
        public TestGeneralLedgerAccountRepository testGlAcccountRepository;

        public DataContracts.Budget budgetRecord;
        public WorkingBudget2 workingBudget2;
        public BudgetConfiguration budgetConfiguration;
        public GeneralLedgerAccountStructure glAccountStructure;

        public string workingBudgetId;
        public string personId;

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
        public List<SortSubtotalComponentQueryCriteria> sortSubtotalComponentQueryCriteria;

        public List<string> justificationNotes;

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();

            actualRepository = new BudgetDevelopmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            testRepository = new TestWorkingBudget2Repository();
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
            sortSubtotalComponentQueryCriteria = new List<SortSubtotalComponentQueryCriteria>();

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
            justificationNotes = null;
        }
        #endregion

        #region GetBudgetDevelopmentWorkingBudget2Async tests

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessAllLineItemsNoFilter()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);

            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkRecords.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            foreach (var lineItem in budgetLineItems)
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
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_Success3SubtotalsForAllLineItemsInOnePage()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("FUND")
                {
                    IndividualComponentValues = new List<string>() { "11" }
                }
            };
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);

            criteria.SortSubtotalComponentQueryCriteria = new List<SortSubtotalComponentQueryCriteria>()
            {
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "Fund",
                        Order = 1,
                        IsDisplaySubTotal = true
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "Unit",
                        Order = 2,
                        IsDisplaySubTotal = true
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "BO",
                        SubtotalName = "Budget Officer",
                        Order = 3,
                        IsDisplaySubTotal = true
                 }
            };

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);

            var lineItems = workingBudget2.LineItems;
            var budgetLineItems = lineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkRecords.Count());
            var subtotalLineItems = lineItems.Where(sl => sl.SubtotalLineItem != null).OrderBy(sl => sl.SequenceNumber).ToArray();
            Assert.AreEqual(subtotalLineItems.Count(), 9);
            Assert.AreEqual(workingBudget2.TotalLineItems, budgetLineItems.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            // Validate the subtotal lines data.
            for (int i = 0; i < subtotalLineItems.Count(); i++)
            {
                var subtotal = subtotalLineItems[i];
                switch (i)
                {
                    case 0:
                        var bo1111Items = lineItems.Where(li => li.SequenceNumber > 0 && li.SequenceNumber < 4).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 4);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "1111");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 1");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo1111Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo1111Items.Select(x => x.BaseBudgetAmount).Sum());
                        var comparables = bo1111Items.SelectMany(c => c.BudgetComparables);
                        foreach (var comp in subtotal.SubtotalLineItem.SubtotalBudgetComparables)
                        {
                            if (comp.ComparableNumber == "C1")
                            {
                                var c1Amount = comparables.Where(x => x.ComparableNumber == "C1").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c1Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C2")
                            {
                                var c2Amount = comparables.Where(x => x.ComparableNumber == "C2").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c2Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C3")
                            {
                                var c3Amount = comparables.Where(x => x.ComparableNumber == "C3").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c3Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C4")
                            {
                                var c4Amount = comparables.Where(x => x.ComparableNumber == "C4").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c4Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C5")
                            {
                                var c5Amount = comparables.Where(x => x.ComparableNumber == "C5").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c5Amount.Sum());
                            }
                        }
                        break;
                    case 1:
                        var bo2100Items = lineItems.Where(li => li.SequenceNumber > 4 && li.SequenceNumber < 10 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 10);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "2100");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo2100Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo2100Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 2:
                        var bo3100Items = lineItems.Where(li => li.SequenceNumber > 10 && li.SequenceNumber < 13 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 13);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3100");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3100Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3100Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 3:
                        var bo3200Items = lineItems.Where(li => li.SequenceNumber > 13 && li.SequenceNumber < 16 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 16);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3200");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3200Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3200Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 4:
                        var dptm33333Items = lineItems.Where(li => li.SequenceNumber > 0 && li.SequenceNumber < 17 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 17);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "UNIT");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "33333");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, dptm33333Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, dptm33333Items.Select(x => x.BaseBudgetAmount).Sum());
                        var dptm33333Comparables = dptm33333Items.SelectMany(c => c.BudgetComparables);
                        foreach (var comp in subtotal.SubtotalLineItem.SubtotalBudgetComparables)
                        {
                            if (comp.ComparableNumber == "C1")
                            {
                                var c1Amount = dptm33333Comparables.Where(x => x.ComparableNumber == "C1").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c1Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C2")
                            {
                                var c2Amount = dptm33333Comparables.Where(x => x.ComparableNumber == "C2").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c2Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C3")
                            {
                                var c3Amount = dptm33333Comparables.Where(x => x.ComparableNumber == "C3").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c3Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C4")
                            {
                                var c4Amount = dptm33333Comparables.Where(x => x.ComparableNumber == "C4").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c4Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C5")
                            {
                                var c5Amount = dptm33333Comparables.Where(x => x.ComparableNumber == "C5").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c5Amount.Sum());
                            }
                        }
                        break;
                    case 5:
                        var bo3300Items = lineItems.Where(li => li.SequenceNumber > 17 && li.SequenceNumber < 20).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 20);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3300");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3300Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3300Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 6:
                        var bo3400Items = lineItems.Where(li => li.SequenceNumber > 20 && li.SequenceNumber < 24).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 24);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3400");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3400Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3400Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 7:
                        var dptm44444Items = lineItems.Where(li => li.SequenceNumber > 17 && li.SequenceNumber < 25 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 25);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "UNIT");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "44444");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, dptm44444Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, dptm44444Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 8:
                        var fund11Items = lineItems.Where(li => li.SequenceNumber > 0 && li.SequenceNumber < 24 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 26);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "FUND");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 1);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "11");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, fund11Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, fund11Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    default:
                        break;
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_Success2SubtotalsForAllLineItemsInOnePage()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("FUND")
                {
                    IndividualComponentValues = new List<string>() { "11" }
                }
            };
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);

            criteria.SortSubtotalComponentQueryCriteria = new List<SortSubtotalComponentQueryCriteria>()
            {
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "Fund",
                        Order = 1,
                        IsDisplaySubTotal = false
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "Unit",
                        Order = 2,
                        IsDisplaySubTotal = true
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "BO",
                        SubtotalName = "Budget Officer",
                        Order = 3,
                        IsDisplaySubTotal = true
                 }
            };

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);

            var lineItems = workingBudget2.LineItems;
            var budgetLineItems = lineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkRecords.Count());
            var subtotalLineItems = workingBudget2.LineItems.Where(sl => sl.SubtotalLineItem != null).OrderBy(sl => sl.SequenceNumber).ToArray();
            Assert.AreEqual(subtotalLineItems.Count(), 8);
            Assert.AreEqual(workingBudget2.TotalLineItems, budgetLineItems.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            // Validate the subtotal lines data.
            for (int i = 0; i < subtotalLineItems.Count(); i++)
            {
                var subtotal = subtotalLineItems[i];
                switch (i)
                {
                    case 0:
                        var bo1111Items = lineItems.Where(li => li.SequenceNumber > 0 && li.SequenceNumber < 4).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 4);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "1111");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 1");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo1111Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo1111Items.Select(x => x.BaseBudgetAmount).Sum());
                        var comparables = bo1111Items.SelectMany(c => c.BudgetComparables);
                        foreach (var comp in subtotal.SubtotalLineItem.SubtotalBudgetComparables)
                        {
                            if (comp.ComparableNumber == "C1")
                            {
                                var c1Amount = comparables.Where(x => x.ComparableNumber == "C1").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c1Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C2")
                            {
                                var c2Amount = comparables.Where(x => x.ComparableNumber == "C2").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c2Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C3")
                            {
                                var c3Amount = comparables.Where(x => x.ComparableNumber == "C3").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c3Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C4")
                            {
                                var c4Amount = comparables.Where(x => x.ComparableNumber == "C4").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c4Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C5")
                            {
                                var c5Amount = comparables.Where(x => x.ComparableNumber == "C5").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c5Amount.Sum());
                            }
                        }
                        break;
                    case 1:
                        var bo2100Items = lineItems.Where(li => li.SequenceNumber > 4 && li.SequenceNumber < 10 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 10);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "2100");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo2100Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo2100Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 2:
                        var bo3100Items = lineItems.Where(li => li.SequenceNumber > 10 && li.SequenceNumber < 13 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 13);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3100");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3100Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3100Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 3:
                        var bo3200Items = lineItems.Where(li => li.SequenceNumber > 13 && li.SequenceNumber < 16 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 16);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3200");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3200Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3200Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 4:
                        var dptm33333Items = lineItems.Where(li => li.SequenceNumber > 0 && li.SequenceNumber < 17 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 17);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "UNIT");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 1);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "33333");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, dptm33333Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, dptm33333Items.Select(x => x.BaseBudgetAmount).Sum());
                        var dptm33333Comparables = dptm33333Items.SelectMany(c => c.BudgetComparables);
                        foreach (var comp in subtotal.SubtotalLineItem.SubtotalBudgetComparables)
                        {
                            if (comp.ComparableNumber == "C1")
                            {
                                var c1Amount = dptm33333Comparables.Where(x => x.ComparableNumber == "C1").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c1Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C2")
                            {
                                var c2Amount = dptm33333Comparables.Where(x => x.ComparableNumber == "C2").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c2Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C3")
                            {
                                var c3Amount = dptm33333Comparables.Where(x => x.ComparableNumber == "C3").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c3Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C4")
                            {
                                var c4Amount = dptm33333Comparables.Where(x => x.ComparableNumber == "C4").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c4Amount.Sum());
                            }
                            if (comp.ComparableNumber == "C5")
                            {
                                var c5Amount = dptm33333Comparables.Where(x => x.ComparableNumber == "C5").Select(y => y.ComparableAmount);
                                Assert.AreEqual(comp.ComparableAmount, c5Amount.Sum());
                            }
                        }

                        break;
                    case 5:
                        var bo3300Items = lineItems.Where(li => li.SequenceNumber > 17 && li.SequenceNumber < 20).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 20);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3300");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3300Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3300Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 6:
                        var bo3400Items = lineItems.Where(li => li.SequenceNumber > 20 && li.SequenceNumber < 24).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 24);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3400");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3400Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3400Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    case 7:
                        var dptm44444Items = lineItems.Where(li => li.SequenceNumber > 17 && li.SequenceNumber < 25 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 25);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "UNIT");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 1);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "44444");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, dptm44444Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, dptm44444Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    default:
                        break;
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_Success3SubtotalsAtEndOfLastPage()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("FUND")
                {
                    IndividualComponentValues = new List<string>() { "11" }
                }
            };
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);

            criteria.SortSubtotalComponentQueryCriteria = new List<SortSubtotalComponentQueryCriteria>()
            {
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "Fund",
                        Order = 1,
                        IsDisplaySubTotal = true
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "Unit",
                        Order = 2,
                        IsDisplaySubTotal = true
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "BO",
                        SubtotalName = "Budget Officer",
                        Order = 3,
                        IsDisplaySubTotal = true
                 }
            };

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            var workingBudget2AllLineItems = await testRepository.GetBudgetDevelopmentAllLineItemsWorkingBudget2Async();
            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 14, 10);

            var subtotalLineItems = workingBudget2.LineItems.Where(sl => sl.SubtotalLineItem != null).OrderBy(sl => sl.SequenceNumber).ToArray();
            Assert.AreEqual(subtotalLineItems.Count(), 3);

            var bo3400Records = testRepository.budWorkRecords.Where(x => x.BwOfcrLink == "3400");
            long? bo3400BaseAmount = 0;
            foreach (var record in bo3400Records)
            {
                if (record.BwfreezeEntityAssociation != null && record.BwfreezeEntityAssociation.Any())
                {
                    var baseAmountRow = record.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                    if (baseAmountRow != null)
                    {
                        bo3400BaseAmount += baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                    }
                }
            }
            var dptm44444Records = testRepository.budWorkRecords.Where(x => x.BwOfcrLink == "3300" || x.BwOfcrLink == "3400");
            long? dptm44444BaseAmount = 0;
            foreach (var record in dptm44444Records)
            {
                if (record.BwfreezeEntityAssociation != null && record.BwfreezeEntityAssociation.Any())
                {
                    var baseAmountRow = record.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                    if (baseAmountRow != null)
                    {
                        dptm44444BaseAmount += baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                    }
                }
            }

            // Validate the subtotal lines data.
            for (int i = 0; i < subtotalLineItems.Count(); i++)
            {
                var subtotal = subtotalLineItems[i];
                switch (i)
                {
                    case 0:
                        Assert.AreEqual(subtotal.SequenceNumber, 4);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3400");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3400Records.Select(x => x.BwLineAmt).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3400BaseAmount);
                        break;
                    case 1:
                        var dptm44444Items = workingBudget2AllLineItems.LineItems.Where(li => li.SequenceNumber > 17 && li.SequenceNumber < 25 && li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 5);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "UNIT");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "44444");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, dptm44444Records.Select(x => x.BwLineAmt).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, dptm44444BaseAmount);
                        foreach (var comp in subtotal.SubtotalLineItem.SubtotalBudgetComparables)
                        {
                            if (comp.ComparableNumber == "C1")
                            {
                                var c1Amount = dptm44444Records.Select(c => c.BwComp1Amount).Sum();
                                Assert.AreEqual(comp.ComparableAmount, c1Amount);
                            }
                            if (comp.ComparableNumber == "C2")
                            {
                                var c2Amount = dptm44444Records.Select(c => c.BwComp2Amount).Sum();
                                Assert.AreEqual(comp.ComparableAmount, c2Amount);
                            }
                            if (comp.ComparableNumber == "C3")
                            {
                                var c3Amount = dptm44444Records.Select(c => c.BwComp3Amount).Sum();
                                Assert.AreEqual(comp.ComparableAmount, c3Amount);
                            }
                            if (comp.ComparableNumber == "C4")
                            {
                                var c4Amount = dptm44444Records.Select(c => c.BwComp4Amount).Sum();
                                Assert.AreEqual(comp.ComparableAmount, c4Amount);
                            }
                            if (comp.ComparableNumber == "C5")
                            {
                                var c5Amount = dptm44444Records.Select(c => c.BwComp5Amount).Sum();
                                Assert.AreEqual(comp.ComparableAmount, c5Amount);
                            }
                        }

                        break;
                    case 2:
                        var fund11Items = workingBudget2AllLineItems.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
                        Assert.AreEqual(subtotal.SequenceNumber, 6);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "FUND");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 1);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "11");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, fund11Items.Select(x => x.WorkingAmount).Sum());
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, fund11Items.Select(x => x.BaseBudgetAmount).Sum());
                        break;
                    default:
                        break;
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_Success3SubtotalsInSecondToLastPage()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("FUND")
                {
                    IndividualComponentValues = new List<string>() { "11" }
                }
            };
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);

            criteria.SortSubtotalComponentQueryCriteria = new List<SortSubtotalComponentQueryCriteria>()
            {
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "BO",
                        SubtotalName = "Budget Officer",
                        Order = 1,
                        IsDisplaySubTotal = true
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "Unit",
                        Order = 2,
                        IsDisplaySubTotal = true
                 }
            };

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 11, 4);

            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 4);
            var subtotalLineItems = workingBudget2.LineItems.Where(sl => sl.SubtotalLineItem != null).OrderBy(sl => sl.SequenceNumber).ToArray();
            Assert.AreEqual(subtotalLineItems.Count(), 4);
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            // Validate the subtotal lines data.
            for (int i = 0; i < subtotalLineItems.Count(); i++)
            {
                var subtotal = subtotalLineItems[i];
                switch (i)
                {
                    case 0:
                        Assert.AreEqual(subtotal.SequenceNumber, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "UNIT");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "33333");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, 109535);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, 6400);
                        foreach (var comp in subtotal.SubtotalLineItem.SubtotalBudgetComparables)
                        {
                            if (comp.ComparableNumber == "C1")
                            {
                                Assert.AreEqual(comp.ComparableAmount, 2052);
                            }
                            if (comp.ComparableNumber == "C2")
                            {
                                Assert.AreEqual(comp.ComparableAmount, 2054);
                            }
                            if (comp.ComparableNumber == "C3")
                            {
                                Assert.AreEqual(comp.ComparableAmount, 2056);
                            }
                            if (comp.ComparableNumber == "C4")
                            {
                                Assert.AreEqual(comp.ComparableAmount, 1024);
                            }
                            if (comp.ComparableNumber == "C5")
                            {
                                Assert.AreEqual(comp.ComparableAmount, 1025);
                            }
                        }
                        break;
                    case 1:
                        Assert.AreEqual(subtotal.SequenceNumber, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 1);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3200");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, 109535);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, 6400);
                        break;
                    case 2:
                        Assert.AreEqual(subtotal.SequenceNumber, 6);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "UNIT");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "44444");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, 108011);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, 6600);
                        break;
                    case 3:
                        Assert.AreEqual(subtotal.SequenceNumber, 7);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 1);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3300");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, 108011);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, 6600);
                        break;
                    default:
                        break;
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_Success3SubtotalsInSecondToLastPageWithBottomSubtotals()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("FUND")
                {
                    IndividualComponentValues = new List<string>() { "11" }
                }
            };
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);

            criteria.SortSubtotalComponentQueryCriteria = new List<SortSubtotalComponentQueryCriteria>()
            {
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "FUND",
                        Order = 1,
                        IsDisplaySubTotal = true
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "Unit",
                        Order = 2,
                        IsDisplaySubTotal = true
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "BO",
                        SubtotalName = "Budget Officer",
                        Order = 3,
                        IsDisplaySubTotal = true
                 }
            };

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 10, 4);

            var bo3200Records = testRepository.budWorkRecords.Where(x => x.BwOfcrLink == "3200");
            long? bo3200WorkingAmount = bo3200Records.Select(x => x.BwLineAmt).Sum();
            long? bo3200BaseAmount = 0;
            foreach (var record in bo3200Records)
            {
                if (record.BwfreezeEntityAssociation != null && record.BwfreezeEntityAssociation.Any())
                {
                    var baseAmountRow = record.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                    if (baseAmountRow != null)
                    {
                        bo3200BaseAmount += baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                    }
                }

            }
            var bo3300Records = testRepository.budWorkRecords.Where(x => x.BwOfcrLink == "3300");
            long? bo3300WorkingAmount = bo3300Records.Select(x => x.BwLineAmt).Sum();
            long? bo3300BaseAmount = 0;
            foreach (var record in bo3300Records)
            {
                if (record.BwfreezeEntityAssociation != null && record.BwfreezeEntityAssociation.Any())
                {
                    var baseAmountRow = record.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                    if (baseAmountRow != null)
                    {
                        bo3300BaseAmount += baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                    }
                }

            }

            var dptm33333Records = testRepository.budWorkRecords.Where(x => x.BwOfcrLink == "1111" || x.BwOfcrLink == "2100" || x.BwOfcrLink == "3100" || x.BwOfcrLink == "3200");
            long? dptm33333WorkingAmount = dptm33333Records.Select(x => x.BwLineAmt).Sum();
            long? dptm33333BaseAmount = 0;
            foreach (var record in dptm33333Records)
            {
                if (record.BwfreezeEntityAssociation != null && record.BwfreezeEntityAssociation.Any())
                {
                    var baseAmountRow = record.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                    if (baseAmountRow != null)
                    {
                        dptm33333BaseAmount += baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                    }
                }
            }

            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 4);
            var subtotalLineItems = workingBudget2.LineItems.Where(sl => sl.SubtotalLineItem != null).OrderBy(sl => sl.SequenceNumber).ToArray();
            Assert.AreEqual(subtotalLineItems.Count(), 3);
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            // Validate the subtotal lines data.
            for (int i = 0; i < subtotalLineItems.Count(); i++)
            {
                var subtotal = subtotalLineItems[i];
                switch (i)
                {
                    case 0:
                        Assert.AreEqual(subtotal.SequenceNumber, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3200");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3200WorkingAmount);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3200BaseAmount);
                        break;
                    case 1:
                        Assert.AreEqual(subtotal.SequenceNumber, 4);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "UNIT");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "33333");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, dptm33333WorkingAmount);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, dptm33333BaseAmount);
                        break;
                    case 2:
                        Assert.AreEqual(subtotal.SequenceNumber, 7);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "3300");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo3300WorkingAmount);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo3300BaseAmount);
                        break;
                    default:
                        break;
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_Success3SubtotalsInMiddlePageWithOnlyBottomSubtotals()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("FUND")
                {
                    IndividualComponentValues = new List<string>() { "11" }
                }
            };
            criteria = new WorkingBudgetQueryCriteria(componentCriteria);

            criteria.SortSubtotalComponentQueryCriteria = new List<SortSubtotalComponentQueryCriteria>()
            {
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "BO",
                        SubtotalName = "Budget Officer",
                        Order = 1,
                        IsDisplaySubTotal = true
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "Unit",
                        Order = 2,
                        IsDisplaySubTotal = true
                 },
                 new SortSubtotalComponentQueryCriteria()
                 {
                        SubtotalType = "GL",
                        SubtotalName = "FUND",
                        Order = 3,
                        IsDisplaySubTotal = true
                 },
            };

            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                Collection<BudOctl> boc1111Records = new Collection<BudOctl>(budOctlRecords.Where(x => x.Recordkey == "1111").ToList());
                return Task.FromResult(boc1111Records);
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 5, 3);

            var bo2100Records = testRepository.budWorkRecords.Where(x => x.BwOfcrLink == "2100");
            long? bo2100WorkingAmount = bo2100Records.Select(x => x.BwLineAmt).Sum();
            long? bo2100BaseAmount = 0;
            foreach (var record in bo2100Records)
            {
                if (record.BwfreezeEntityAssociation != null && record.BwfreezeEntityAssociation.Any())
                {
                    var baseAmountRow = record.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                    if (baseAmountRow != null)
                    {
                        bo2100BaseAmount += baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                    }
                }

            }
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 3);
            var subtotalLineItems = workingBudget2.LineItems.Where(sl => sl.SubtotalLineItem != null).OrderBy(sl => sl.SequenceNumber).ToArray();
            Assert.AreEqual(subtotalLineItems.Count(), 3);
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            // Validate the subtotal lines data.
            for (int i = 0; i < subtotalLineItems.Count(); i++)
            {
                var subtotal = subtotalLineItems[i];
                switch (i)
                {
                    case 0:
                        Assert.AreEqual(subtotal.SequenceNumber, 4);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "FUND");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 3);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "11");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo2100WorkingAmount);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo2100BaseAmount);
                        foreach (var comp in subtotal.SubtotalLineItem.SubtotalBudgetComparables)
                        {
                            if (comp.ComparableNumber == "C1")
                            {
                                var c1Amount = bo2100Records.Select(c => c.BwComp1Amount).Sum();
                                Assert.AreEqual(comp.ComparableAmount, c1Amount);
                            }
                            if (comp.ComparableNumber == "C2")
                            {
                                var c2Amount = bo2100Records.Select(c => c.BwComp2Amount).Sum();
                                Assert.AreEqual(comp.ComparableAmount, c2Amount);
                            }
                            if (comp.ComparableNumber == "C3")
                            {
                                var c3Amount = bo2100Records.Select(c => c.BwComp3Amount).Sum();
                                Assert.AreEqual(comp.ComparableAmount, c3Amount);
                            }
                            if (comp.ComparableNumber == "C4")
                            {
                                var c4Amount = bo2100Records.Select(c => c.BwComp4Amount).Sum();
                                Assert.AreEqual(comp.ComparableAmount, c4Amount);
                            }
                            if (comp.ComparableNumber == "C5")
                            {
                                var c5Amount = bo2100Records.Select(c => c.BwComp5Amount).Sum();
                                Assert.AreEqual(comp.ComparableAmount, c5Amount);
                            }
                        }
                        break;
                    case 1:
                        Assert.AreEqual(subtotal.SequenceNumber, 5);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "GL");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "UNIT");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 2);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "33333");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, null);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo2100WorkingAmount);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo2100BaseAmount);
                        break;
                    case 2:
                        Assert.AreEqual(subtotal.SequenceNumber, 6);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalType, "BO");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalName, "Budget Officer");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalOrder, 1);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalValue, "2100");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalDescription, "Opers name for login 2");
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalWorkingAmount, bo2100WorkingAmount);
                        Assert.AreEqual(subtotal.SubtotalLineItem.SubtotalBaseBudgetAmount, bo2100BaseAmount);
                        break;
                    default:
                        break;
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessAllLineItemsNoFilterNoAuthDate()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkRecords.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            foreach (var lineItem in budgetLineItems)
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
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessAllLineItemsFilterWithOnlyGlCriteria()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in budgetLineItems)
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
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessAllLineItemsFilterWithGlCriteriaAndBoIdsNullAuthDates()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in budgetLineItems)
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
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessFilterWithBoIdsNullAuthDates()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in budgetLineItems)
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
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, true);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessAllLineItemsFilterWithBoIdsNullAuthDatesSameBranch()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in budgetLineItems)
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
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessAllLineItemsWithFilterAndPaging()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 1, 2);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count() - 1);
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in budgetLineItems)
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
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessAllLineItemsWithFilterAndPagingRangesOutsideLimtis()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, -1, -1);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 1);
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            foreach (var lineItem in budgetLineItems)
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
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, true);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessBudOfcr2100LineItems()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, "0000002", glAccountStructure, 0, 6);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 6);
            Assert.AreEqual(workingBudget2.TotalLineItems, bct2100Records.SelectMany(x => x.BcWorkLineNo).Count());

            foreach (var lineItem in budgetLineItems)
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
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, true);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessBudOfcr34002200LineItems()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, "0000002", glAccountStructure, 0, 3);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), bwk3400Records.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, bwk3400Records.Count());

            foreach (var lineItem in budgetLineItems)
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
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_GetBudgetAuthDateForBudOfcr34002200LineItems()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, "0000002", glAccountStructure, 0, 3);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            // The user is budget officer 3400 and 2200 and its top responsibility unit is OB_FIN.
            // OB_FIN has a null authorization date, and its superior OB is also null. The BUDGET
            // finalization date applies, which is in the future.
            foreach (var lineItem in budgetLineItems)
            {
                Assert.AreEqual(lineItem.BudgetReportingUnit.HasAuthorizationDatePassed, false);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessFilteredReportingUnitsNoDescendants()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB_FIN and OB_FIN_IT without descendants.
            // OB_FIN does not have any line items assigned.
            foreach (var lineItem in budgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_IT");
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessFilteredReportingUnitsWithDescendants()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB_FIN and OB_FIN_IT with descendants.
            // OB_FIN does not have any line items assigned.
            foreach (var lineItem in budgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_IT" ||
                    lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_ACCT");
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessFilteredReportingUnitsNoDescendantsTwoReportingUnits()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB_ENROL and OB_ENROL_FA without descendants.
            foreach (var lineItem in budgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL" || lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL_FA");
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessFilteredReportingUnitsNoDescendantsTwoReportingUnitsAndDescendants()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB_ENROL and OB_ENROL_FA with descendants.
            foreach (var lineItem in budgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL_REV"
                                || lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL"
                                || lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL_FA");
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessFilteredReportingUnitsNoDescendantsTworeportingUnitsOneHasNoItems()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB and OB_FIN without descendants.
            // OB_FIN does not have any line items assigned.
            foreach (var lineItem in budgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB");
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_SuccessFilteredReportingUnitsWithDescendantsOneUnitHasNoItemsWithDescendants()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
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

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkIds.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budWorkIds.Count());

            // The user is budget officer 1111 and its top responsibility unit is OB
            // but the line items are filtered to OB_FIN and OB with descendants.
            // OB_FIN does not have any line items assigned.
            foreach (var lineItem in budgetLineItems)
            {
                Assert.IsTrue(lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_IT"
                    || lineItem.BudgetReportingUnit.ReportingUnitId == "OB_ENROL_FA"
                    || lineItem.BudgetReportingUnit.ReportingUnitId == "OB_FIN_ACCT");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NullBudgetId()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            workingBudgetId = null;

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_EmptyBudgetId()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            workingBudgetId = "";

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NullPersonId()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            personId = null;

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_EmptyPersonId()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            personId = "";

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoStaffRecord()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            staffDataContract = null;
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NullLoginId()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            staffDataContract.StaffLoginId = null;
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_EmptyLoginId()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            staffDataContract.StaffLoginId = "";
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 1);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoBudgetOfficerIds()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.SelectAsync("BUD.OFCR", It.IsAny<string>())).Returns(() =>
                {
                    return Task.FromResult(null as string[]);
                });
            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 1);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_EmptyListOfBudgetOfficerIds()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bo => bo.SelectAsync("BUD.OFCR", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(new string[] { });
            });
            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 1);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoBudgetOfficerRecords()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            budOfcrRecords = new Collection<BudOfcr>();
            dataReaderMock.Setup(bo => bo.BulkReadRecordAsync<BudOfcr>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(budOfcrRecords);
            });
            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoBudgetOfficerRecordsForLogin()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            staffDataContract.StaffLoginId = "NOMATCHLOGIN";
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<BudgetManagement.DataContracts.Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoBudOctlRecords()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(null as Collection<BudOctl>);
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoBudOctlRespIds()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            Collection<BudOctl> bocNoRespIds = new Collection<BudOctl>(budOctlRecords.Select(x => { x.BoBcId = new List<String>(); return x; }).ToArray());
            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudOctl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(bocNoRespIds);
            });
            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoBudCtrlRecords()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(null as Collection<BudCtrl>);
            });

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(null as Collection<BudWork>);
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoBudCtrlWorkLineNos()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            Collection<BudCtrl> bctNoRecords = new Collection<BudCtrl>(budCtrlRecords.Select(x => { x.BcWorkLineNo = new List<String>(); return x; }).ToArray());
            dataReaderMock.Setup(bc => bc.BulkReadRecordAsync<BudCtrl>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(bctNoRecords);
            });
            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoBudWorkRecords()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(null as Collection<BudWork>);
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_ZeroBudWorkRecords()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            dataReaderMock.Setup(bw => bw.BulkReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(new Collection<BudWork>());
            });

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), 0);
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoExpenseAccountInOneBudWork()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            budWorkRecords.FirstOrDefault().BwExpenseAcct = "";

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkRecords.Count() - 1);
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            foreach (var lineItem in budgetLineItems)
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
        public async Task GetBudgetDevelopmentWorkingBudget2Async_NoBaseVersionInOneBudWork()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            budWorkRecords.FirstOrDefault().BwfreezeEntityAssociation = null;

            workingBudget2 = await actualRepository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, criteria, personId, glAccountStructure, 0, 999);
            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            Assert.AreEqual(budgetLineItems.Count(), budWorkRecords.Count());
            Assert.AreEqual(workingBudget2.TotalLineItems, budCtrlRecords.SelectMany(x => x.BcWorkLineNo).Count());

            foreach (var lineItem in budgetLineItems)
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