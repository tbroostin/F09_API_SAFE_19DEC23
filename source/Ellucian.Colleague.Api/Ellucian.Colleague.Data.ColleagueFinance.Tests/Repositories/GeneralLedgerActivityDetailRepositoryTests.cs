// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    /// <summary>
    ///  Test the methods in the Cost Centr repository.
    /// </summary>
    [TestClass]
    public class GeneralLedgerActivityDetailRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup
        private TestGlAccountRepository testGlAccountRepository;
        private GeneralLedgerActivityDetailRepository glaRepository;
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository = null;
        private ApplValcodes glSourceCodes = null;
        private GlAccts glAcctsDataContract;
        private FdDescs fdDescsDataContract;
        private FcDescs fcDescsDataContract;
        private LoDescs loDescsDataContract;
        private SoDescs soDescsDataContract;
        private UnDescs unDescsDataContract;
        private ObDescs obDescsDataContract;

        private BudgetDevDefaults budgetDevDefaultsDataContract;
        private GenLdgr genLdgrDataContract;
        private BudWork budWorkDataContract;

        private Collection<GlaFyr> glaFyrDataContracts;
        private EncFyr encFyrDataContract;
        private GetGlAccountDescriptionResponse glAccountDescriptionResponse;

        private string todaysFiscalYear = DateTime.Now.ToString("yyyy");
        private string futureFiscalYear = (Convert.ToDecimal(DateTime.Now.ToString("yyyy")) + 1).ToString();
        private string pastFiscalYear = (Convert.ToDecimal(DateTime.Now.ToString("yyyy")) - 1).ToString();

        private string param1_glAccount;
        private string param2_fiscalYear;
        private CostCenterStructure param3_costCenterStructure;

        private string glClassName = "GL.CLASS";
        private List<string> glExpenseValues = new List<string>() { "5", "7" };
        private List<string> glRevenueValues = new List<string>() { "4", "6" };
        private List<string> glAssetValues = new List<string>() { "1" };
        private List<string> glLiabilityValues = new List<string>() { "2" };
        private List<string> glFundBalValues = new List<string>() { "3" };

        IList<string> majorComponentStartPosition = new List<string>() { "1", "4", "7", "10", "13", "19" };

        [TestInitialize]
        public void Initialize()
        {
            this.testGlAccountRepository = new TestGlAccountRepository();

            this.param1_glAccount = "10_00_01_01_33333_51001";
            this.param2_fiscalYear = todaysFiscalYear;
            this.param3_costCenterStructure = this.testGlAccountRepository.CostCenterStructure;

            this.MockInitialize();

            glaRepository = new GeneralLedgerActivityDetailRepository(cacheProvider, transFactory, logger);
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();

            // Set up the data contract(s)
            PopulateGlAcctsDataContract();
            PopulateGlaDataContracts();
            PopulateEncDataContract();
            BuildGlSourceCodes();
            PopulateDescsDataContracts();
            BuildJustificationNotes();
            InitializeMockStatements();
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.param1_glAccount = null;
            this.param2_fiscalYear = null;
            this.param3_costCenterStructure = null;
            glaRepository = null;
            testGlConfigurationRepository = null;
            glAccountDescriptionResponse = null;
            glaFyrDataContracts = null;
            glAcctsDataContract = null;
            fdDescsDataContract = null;
            fcDescsDataContract = null;
            loDescsDataContract = null;
            soDescsDataContract = null;
            unDescsDataContract = null;
            obDescsDataContract = null;
            budgetDevDefaultsDataContract = null;
            genLdgrDataContract = null;
            budWorkDataContract = null;
            encFyrDataContract = null;
            glSourceCodes = null;
        }
        #endregion

        #region Tests

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_FullDataSet_Success()
        {
            BuildJustificationNotes();

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
            }

            // Sum of the debit minus credit for each selected actuals GLA.FYR data contract.
            var expectedActualsGlaAmount = expectedActualsGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total actual amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedActualsGlaAmount, activityDetailDomain.ActualAmount);

            // The number of JE transactions has to be the same.
            IEnumerable<GlTransaction> jeActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "JE").ToList();
            var glaJeRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "JE").ToList();
            Assert.AreEqual(jeActualsDomainTransactions.Count(), glaJeRecords.Count());

            // The number of PJ transactions has to be the same.
            IEnumerable<GlTransaction> pjActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "PJ").ToList();
            var glaPjRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "PJ").ToList();
            Assert.AreEqual(pjActualsDomainTransactions.Count(), glaPjRecords.Count());

            // Keep the count of actual transactions.
            var actualsDomainTransactionCount = jeActualsDomainTransactions.Count() + pjActualsDomainTransactions.Count();

            // Sum of the debit minus credit for each selected budget GLA.FYR data contract.
            var expectedBudgetGlaAmount = expectedBudgetGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total budget amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedBudgetGlaAmount, activityDetailDomain.BudgetAmount);

            // The number of AB transactions has to be the same.
            IEnumerable<GlTransaction> abBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "AB").ToList();
            var glaAbRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "AB").ToList();
            Assert.AreEqual(abBudgetDomainTransactions.Count(), glaAbRecords.Count());

            // The number of BU transactions has to be the same.
            IEnumerable<GlTransaction> buBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BU").ToList();
            var glaBuRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BU").ToList();
            Assert.AreEqual(buBudgetDomainTransactions.Count(), glaBuRecords.Count());

            // The number of BC transactions has to be the same.
            IEnumerable<GlTransaction> bcBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BC").ToList();
            var glaBcRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BC").ToList();
            Assert.AreEqual(bcBudgetDomainTransactions.Count(), glaBcRecords.Count());

            // Keep the count of budget transactions.
            var budgetDomainTransactionCount = abBudgetDomainTransactions.Count() + buBudgetDomainTransactions.Count() + bcBudgetDomainTransactions.Count();

            // Sum of the amount from each encumbrance entry in the ENC.FYR data contract.
            var expectedEncumbranceEncAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember);
            // Add the sum from the requisition entries.
            expectedEncumbranceEncAmount += encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember);
            // Assert the total encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceEncAmount, activityDetailDomain.EncumbranceAmount);

            // The number of encumbrance transactions has to be the same as the number of entries in the ENC.FYR record.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance || x.GlTransactionType == GlTransactionType.Requisition).ToList().ToList();
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encumbranceRecordCount);

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance
            // data contract has to match the number of transactions in the domain.
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount, activityDetailDomain.Transactions.Count());

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(actualsDomainTransactionCount + budgetDomainTransactionCount + encumbranceRecordCount, activityDetailDomain.Transactions.Count());

            // Loop through the actuals data contracts and compare each property with the actuals domain transactions.
            foreach (var glaDataContract in expectedActualsGlaRecords)
            {
                GlTransaction actualTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(actualTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, actualTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, actualTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, actualTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, actualTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, actualTransaction.TransactionDate);
            }

            // Loop through the budget data contracts and compare each property with the budget domain transactions.
            foreach (var glaDataContract in expectedBudgetGlaRecords)
            {
                GlTransaction budgetTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(budgetTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, budgetTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, budgetTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, budgetTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, budgetTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, budgetTransaction.TransactionDate);
            }

            // Loop through the encumbrace entries in the data contract and compare each property with the encumbrance type domain transactions.
            foreach (var encPoTransaction in encFyrDataContract.EncPoEntityAssociation)
            {
                GlTransaction encumbranceTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Source == "EP"
                    && x.ReferenceNumber == encPoTransaction.EncPoNoAssocMember);

                Assert.IsNotNull(encumbranceTransaction);
                Assert.AreEqual(encPoTransaction.EncPoAmtAssocMember, encumbranceTransaction.Amount);
                Assert.AreEqual(encPoTransaction.EncPoNoAssocMember, encumbranceTransaction.ReferenceNumber);
                Assert.AreEqual(encPoTransaction.EncPoIdAssocMember, encumbranceTransaction.DocumentId);
                Assert.AreEqual(encPoTransaction.EncPoVendorAssocMember, encumbranceTransaction.Description);
                Assert.AreEqual("EP", encumbranceTransaction.Source);
                Assert.AreEqual(encPoTransaction.EncPoDateAssocMember, encumbranceTransaction.TransactionDate);
            }

            // Loop through the requisition entries in the data contract and compare each property with the requisition type domain transactions.
            foreach (var encReqTransaction in encFyrDataContract.EncReqEntityAssociation)
            {
                GlTransaction encumbranceTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Source == "ER"
                    && x.ReferenceNumber == encReqTransaction.EncReqNoAssocMember);

                Assert.IsNotNull(encumbranceTransaction);
                Assert.AreEqual(encReqTransaction.EncReqAmtAssocMember, encumbranceTransaction.Amount);
                Assert.AreEqual(encReqTransaction.EncReqNoAssocMember, encumbranceTransaction.ReferenceNumber);
                Assert.AreEqual(encReqTransaction.EncReqIdAssocMember, encumbranceTransaction.DocumentId);
                Assert.AreEqual(encReqTransaction.EncReqVendorAssocMember, encumbranceTransaction.Description);
                Assert.AreEqual(encReqTransaction.EncReqDateAssocMember, encumbranceTransaction.TransactionDate);
            }

            // Get the actuals memo amount from the GL.ACCTS data contract and compare it to the domain entity amount.
            var actualsPendingPosting = glAcctsDataContract.GlActualMemos[1];
            Assert.AreEqual(actualsPendingPosting, activityDetailDomain.MemoActualsAmount);

            // Get the budget memo amount from the GL.ACCTS data contract and compare it to the domain entity amount.
            var budgetPendingPosting = glAcctsDataContract.GlBudgetMemos[1];
            Assert.AreEqual(budgetPendingPosting, activityDetailDomain.MemoBudgetAmount);

            // Compare the Unit ID between contract and domain.
            Assert.AreEqual(unDescsDataContract.Recordkey, activityDetailDomain.UnitId);

            // Compare the cost center description between contract and domain.
            Assert.AreEqual("Fund 10 : Source 01", activityDetailDomain.Name);

            Assert.AreEqual(true, activityDetailDomain.ShowJustificationNotes);
            Assert.AreEqual("Line 1\r\n\r\n Line 3", activityDetailDomain.JustificationNotes);
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_FullDataSet_Success_No_Justification_Notes()
        {
            genLdgrDataContract = null;

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
            }

            // Sum of the debit minus credit for each selected actuals GLA.FYR data contract.
            var expectedActualsGlaAmount = expectedActualsGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total actual amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedActualsGlaAmount, activityDetailDomain.ActualAmount);

            // The number of JE transactions has to be the same.
            IEnumerable<GlTransaction> jeActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "JE").ToList();
            var glaJeRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "JE").ToList();
            Assert.AreEqual(jeActualsDomainTransactions.Count(), glaJeRecords.Count());

            // The number of PJ transactions has to be the same.
            IEnumerable<GlTransaction> pjActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "PJ").ToList();
            var glaPjRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "PJ").ToList();
            Assert.AreEqual(pjActualsDomainTransactions.Count(), glaPjRecords.Count());

            // Keep the count of actual transactions.
            var actualsDomainTransactionCount = jeActualsDomainTransactions.Count() + pjActualsDomainTransactions.Count();

            // Sum of the debit minus credit for each selected budget GLA.FYR data contract.
            var expectedBudgetGlaAmount = expectedBudgetGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total budget amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedBudgetGlaAmount, activityDetailDomain.BudgetAmount);

            // The number of AB transactions has to be the same.
            IEnumerable<GlTransaction> abBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "AB").ToList();
            var glaAbRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "AB").ToList();
            Assert.AreEqual(abBudgetDomainTransactions.Count(), glaAbRecords.Count());

            // The number of BU transactions has to be the same.
            IEnumerable<GlTransaction> buBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BU").ToList();
            var glaBuRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BU").ToList();
            Assert.AreEqual(buBudgetDomainTransactions.Count(), glaBuRecords.Count());

            // The number of BC transactions has to be the same.
            IEnumerable<GlTransaction> bcBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BC").ToList();
            var glaBcRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BC").ToList();
            Assert.AreEqual(bcBudgetDomainTransactions.Count(), glaBcRecords.Count());

            // Keep the count of budget transactions.
            var budgetDomainTransactionCount = abBudgetDomainTransactions.Count() + buBudgetDomainTransactions.Count() + bcBudgetDomainTransactions.Count();

            // Sum of the amount from each encumbrance entry in the ENC.FYR data contract.
            var expectedEncumbranceEncAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember);
            // Add the sum from the requisition entries.
            expectedEncumbranceEncAmount += encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember);
            // Assert the total encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceEncAmount, activityDetailDomain.EncumbranceAmount);

            // The number of encumbrance transactions has to be the same as the number of entries in the ENC.FYR record.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance || x.GlTransactionType == GlTransactionType.Requisition).ToList().ToList();
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encumbranceRecordCount);

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance
            // data contract has to match the number of transactions in the domain.
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount, activityDetailDomain.Transactions.Count());

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(actualsDomainTransactionCount + budgetDomainTransactionCount + encumbranceRecordCount, activityDetailDomain.Transactions.Count());

            // Loop through the actuals data contracts and compare each property with the actuals domain transactions.
            foreach (var glaDataContract in expectedActualsGlaRecords)
            {
                GlTransaction actualTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(actualTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, actualTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, actualTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, actualTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, actualTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, actualTransaction.TransactionDate);
            }

            // Loop through the budget data contracts and compare each property with the budget domain transactions.
            foreach (var glaDataContract in expectedBudgetGlaRecords)
            {
                GlTransaction budgetTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(budgetTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, budgetTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, budgetTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, budgetTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, budgetTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, budgetTransaction.TransactionDate);
            }

            // Loop through the encumbrace entries in the data contract and compare each property with the encumbrance type domain transactions.
            foreach (var encPoTransaction in encFyrDataContract.EncPoEntityAssociation)
            {
                GlTransaction encumbranceTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Source == "EP"
                    && x.ReferenceNumber == encPoTransaction.EncPoNoAssocMember);

                Assert.IsNotNull(encumbranceTransaction);
                Assert.AreEqual(encPoTransaction.EncPoAmtAssocMember, encumbranceTransaction.Amount);
                Assert.AreEqual(encPoTransaction.EncPoNoAssocMember, encumbranceTransaction.ReferenceNumber);
                Assert.AreEqual(encPoTransaction.EncPoIdAssocMember, encumbranceTransaction.DocumentId);
                Assert.AreEqual(encPoTransaction.EncPoVendorAssocMember, encumbranceTransaction.Description);
                Assert.AreEqual("EP", encumbranceTransaction.Source);
                Assert.AreEqual(encPoTransaction.EncPoDateAssocMember, encumbranceTransaction.TransactionDate);
            }

            // Loop through the requisition entries in the data contract and compare each property with the requisition type domain transactions.
            foreach (var encReqTransaction in encFyrDataContract.EncReqEntityAssociation)
            {
                GlTransaction encumbranceTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Source == "ER"
                    && x.ReferenceNumber == encReqTransaction.EncReqNoAssocMember);

                Assert.IsNotNull(encumbranceTransaction);
                Assert.AreEqual(encReqTransaction.EncReqAmtAssocMember, encumbranceTransaction.Amount);
                Assert.AreEqual(encReqTransaction.EncReqNoAssocMember, encumbranceTransaction.ReferenceNumber);
                Assert.AreEqual(encReqTransaction.EncReqIdAssocMember, encumbranceTransaction.DocumentId);
                Assert.AreEqual(encReqTransaction.EncReqVendorAssocMember, encumbranceTransaction.Description);
                Assert.AreEqual(encReqTransaction.EncReqDateAssocMember, encumbranceTransaction.TransactionDate);
            }

            // Get the actuals memo amount from the GL.ACCTS data contract and compare it to the domain entity amount.
            var actualsPendingPosting = glAcctsDataContract.GlActualMemos[1];
            Assert.AreEqual(actualsPendingPosting, activityDetailDomain.MemoActualsAmount);

            // Get the budget memo amount from the GL.ACCTS data contract and compare it to the domain entity amount.
            var budgetPendingPosting = glAcctsDataContract.GlBudgetMemos[1];
            Assert.AreEqual(budgetPendingPosting, activityDetailDomain.MemoBudgetAmount);

            // Compare the Unit ID between contract and domain.
            Assert.AreEqual(unDescsDataContract.Recordkey, activityDetailDomain.UnitId);

            // Compare the cost center description between contract and domain.
            Assert.AreEqual("Fund 10 : Source 01", activityDetailDomain.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryGlActivityDetailAsync_Null_GlAccount()
        {
            // Null the GL Account.
            this.param1_glAccount = null;

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryGlActivityDetailAsync_Empty_GlAccount()
        {
            // Null the GL Account.
            this.param1_glAccount = "";

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryGlActivityDetailAsync_Null_FiscalYear()
        {
            // Null the fiscal year.
            this.param2_fiscalYear = null;

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryGlActivityDetailAsync_Empty_FiscalYear()
        {
            // Null the fiscal year.
            this.param2_fiscalYear = "";

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_Null_GlSourceCodesTable()
        {
            glSourceCodes = null;
            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();
            Assert.AreEqual(0, activityDetailDomain.Transactions.Count());
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_Null_GlAccountRecord()
        {
            // Null the GL Account record.
            glAcctsDataContract = null;

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
            }

            // Sum of the debit minus credit for each selected actuals GLA.FYR data contract.
            var expectedActualsGlaAmount = expectedActualsGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total actual amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedActualsGlaAmount, activityDetailDomain.ActualAmount);

            // The number of JE transactions has to be the same.
            IEnumerable<GlTransaction> jeActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "JE").ToList();
            var glaJeRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "JE").ToList();
            Assert.AreEqual(jeActualsDomainTransactions.Count(), glaJeRecords.Count());

            // The number of PJ transactions has to be the same.
            IEnumerable<GlTransaction> pjActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "PJ").ToList();
            var glaPjRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "PJ").ToList();
            Assert.AreEqual(pjActualsDomainTransactions.Count(), glaPjRecords.Count());

            // Keep the count of actual transactions.
            var actualsDomainTransactionCount = jeActualsDomainTransactions.Count() + pjActualsDomainTransactions.Count();

            // Sum of the debit minus credit for each selected budget GLA.FYR data contract.
            var expectedBudgetGlaAmount = expectedBudgetGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total budget amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedBudgetGlaAmount, activityDetailDomain.BudgetAmount);

            // The number of AB transactions has to be the same.
            IEnumerable<GlTransaction> abBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "AB").ToList();
            var glaAbRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "AB").ToList();
            Assert.AreEqual(abBudgetDomainTransactions.Count(), glaAbRecords.Count());

            // The number of BU transactions has to be the same.
            IEnumerable<GlTransaction> buBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BU").ToList();
            var glaBuRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BU").ToList();
            Assert.AreEqual(buBudgetDomainTransactions.Count(), glaBuRecords.Count());

            // The number of BC transactions has to be the same.
            IEnumerable<GlTransaction> bcBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BC").ToList();
            var glaBcRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BC").ToList();
            Assert.AreEqual(bcBudgetDomainTransactions.Count(), glaBcRecords.Count());

            // Keep the count of budget transactions.
            var budgetDomainTransactionCount = abBudgetDomainTransactions.Count() + buBudgetDomainTransactions.Count() + bcBudgetDomainTransactions.Count();

            // Sum of the amount from each encumbrance entry in the ENC.FYR data contract.
            var expectedEncumbranceEncAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember);
            // Add the sum from the requisition entries.
            expectedEncumbranceEncAmount += encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember);
            // Assert the total encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceEncAmount, activityDetailDomain.EncumbranceAmount);

            // The number of encumbrance transactions has to be the same as the number of entries in the ENC.FYR record.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance || x.GlTransactionType == GlTransactionType.Requisition).ToList().ToList();
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encumbranceRecordCount);

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance
            // data contract has to match the number of transactions in the domain.
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount, activityDetailDomain.Transactions.Count());

            // Lastly, make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(actualsDomainTransactionCount + budgetDomainTransactionCount + encumbranceRecordCount, activityDetailDomain.Transactions.Count());

            // Loop through the actuals data contracts and compare each property with the actuals domain transactions.
            foreach (var glaDataContract in expectedActualsGlaRecords)
            {
                GlTransaction actualTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(actualTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, actualTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, actualTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, actualTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, actualTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, actualTransaction.TransactionDate);
            }

            // Loop through the budget data contracts and compare each property with the budget domain transactions.
            foreach (var glaDataContract in expectedBudgetGlaRecords)
            {
                GlTransaction budgetTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(budgetTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, budgetTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, budgetTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, budgetTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, budgetTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, budgetTransaction.TransactionDate);
            }

            // Loop through the encumbrace entries in the data contract and compare each property with the encumbrance type domain transactions.
            foreach (var encPoTransaction in encFyrDataContract.EncPoEntityAssociation)
            {
                GlTransaction encumbranceTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Source == "EP"
                    && x.ReferenceNumber == encPoTransaction.EncPoNoAssocMember);

                Assert.IsNotNull(encumbranceTransaction);
                Assert.AreEqual(encPoTransaction.EncPoAmtAssocMember, encumbranceTransaction.Amount);
                Assert.AreEqual(encPoTransaction.EncPoNoAssocMember, encumbranceTransaction.ReferenceNumber);
                Assert.AreEqual(encPoTransaction.EncPoIdAssocMember, encumbranceTransaction.DocumentId);
                Assert.AreEqual(encPoTransaction.EncPoVendorAssocMember, encumbranceTransaction.Description);
                Assert.AreEqual("EP", encumbranceTransaction.Source);
                Assert.AreEqual(encPoTransaction.EncPoDateAssocMember, encumbranceTransaction.TransactionDate);
            }

            // Loop through the requisition entries in the data contract and compare each property with the requisition type domain transactions.
            foreach (var encReqTransaction in encFyrDataContract.EncReqEntityAssociation)
            {
                GlTransaction encumbranceTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Source == "ER"
                    && x.ReferenceNumber == encReqTransaction.EncReqNoAssocMember);

                Assert.IsNotNull(encumbranceTransaction);
                Assert.AreEqual(encReqTransaction.EncReqAmtAssocMember, encumbranceTransaction.Amount);
                Assert.AreEqual(encReqTransaction.EncReqNoAssocMember, encumbranceTransaction.ReferenceNumber);
                Assert.AreEqual(encReqTransaction.EncReqIdAssocMember, encumbranceTransaction.DocumentId);
                Assert.AreEqual(encReqTransaction.EncReqVendorAssocMember, encumbranceTransaction.Description);
                Assert.AreEqual(encReqTransaction.EncReqDateAssocMember, encumbranceTransaction.TransactionDate);
            }

            // Assert the actuals pending posting is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.MemoActualsAmount);

            // Assert the abudget pending posting is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.MemoBudgetAmount);
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_Null_MemosEntityAssociation()
        {
            // Null the GL Account record.
            glAcctsDataContract.MemosEntityAssociation = null;

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
            }

            // Sum of the debit minus credit for each selected actuals GLA.FYR data contract.
            var expectedActualsGlaAmount = expectedActualsGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total actual amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedActualsGlaAmount, activityDetailDomain.ActualAmount);

            // The number of JE transactions has to be the same.
            IEnumerable<GlTransaction> jeActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "JE").ToList();
            var glaJeRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "JE").ToList();
            Assert.AreEqual(jeActualsDomainTransactions.Count(), glaJeRecords.Count());

            // The number of PJ transactions has to be the same.
            IEnumerable<GlTransaction> pjActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "PJ").ToList();
            var glaPjRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "PJ").ToList();
            Assert.AreEqual(pjActualsDomainTransactions.Count(), glaPjRecords.Count());

            // Keep the count of actual transactions.
            var actualsDomainTransactionCount = jeActualsDomainTransactions.Count() + pjActualsDomainTransactions.Count();

            // Sum of the debit minus credit for each selected budget GLA.FYR data contract.
            var expectedBudgetGlaAmount = expectedBudgetGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total budget amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedBudgetGlaAmount, activityDetailDomain.BudgetAmount);

            // The number of AB transactions has to be the same.
            IEnumerable<GlTransaction> abBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "AB").ToList();
            var glaAbRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "AB").ToList();
            Assert.AreEqual(abBudgetDomainTransactions.Count(), glaAbRecords.Count());

            // The number of BU transactions has to be the same.
            IEnumerable<GlTransaction> buBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BU").ToList();
            var glaBuRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BU").ToList();
            Assert.AreEqual(buBudgetDomainTransactions.Count(), glaBuRecords.Count());

            // The number of BC transactions has to be the same.
            IEnumerable<GlTransaction> bcBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BC").ToList();
            var glaBcRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BC").ToList();
            Assert.AreEqual(bcBudgetDomainTransactions.Count(), glaBcRecords.Count());

            // Keep the count of budget transactions.
            var budgetDomainTransactionCount = abBudgetDomainTransactions.Count() + buBudgetDomainTransactions.Count() + bcBudgetDomainTransactions.Count();

            // Sum of the amount from each encumbrance entry in the ENC.FYR data contract.
            var expectedEncumbranceEncAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember);
            // Add the sum from the requisition entries.
            expectedEncumbranceEncAmount += encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember);
            // Assert the total encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceEncAmount, activityDetailDomain.EncumbranceAmount);

            // The number of encumbrance transactions has to be the same as the number of entries in the ENC.FYR record.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance || x.GlTransactionType == GlTransactionType.Requisition).ToList().ToList();
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encumbranceRecordCount);

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance
            // data contract has to match the number of transactions in the domain.
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount, activityDetailDomain.Transactions.Count());

            // Lastly, make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(actualsDomainTransactionCount + budgetDomainTransactionCount + encumbranceRecordCount, activityDetailDomain.Transactions.Count());

            // Loop through the actuals data contracts and compare each property with the actuals domain transactions.
            foreach (var glaDataContract in expectedActualsGlaRecords)
            {
                GlTransaction actualTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(actualTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, actualTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, actualTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, actualTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, actualTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, actualTransaction.TransactionDate);
            }

            // Loop through the budget data contracts and compare each property with the budget domain transactions.
            foreach (var glaDataContract in expectedBudgetGlaRecords)
            {
                GlTransaction budgetTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(budgetTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, budgetTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, budgetTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, budgetTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, budgetTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, budgetTransaction.TransactionDate);
            }

            // Loop through the encumbrace entries in the data contract and compare each property with the encumbrance type domain transactions.
            foreach (var encPoTransaction in encFyrDataContract.EncPoEntityAssociation)
            {
                GlTransaction encumbranceTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Source == "EP"
                    && x.ReferenceNumber == encPoTransaction.EncPoNoAssocMember);

                Assert.IsNotNull(encumbranceTransaction);
                Assert.AreEqual(encPoTransaction.EncPoAmtAssocMember, encumbranceTransaction.Amount);
                Assert.AreEqual(encPoTransaction.EncPoNoAssocMember, encumbranceTransaction.ReferenceNumber);
                Assert.AreEqual(encPoTransaction.EncPoIdAssocMember, encumbranceTransaction.DocumentId);
                Assert.AreEqual(encPoTransaction.EncPoVendorAssocMember, encumbranceTransaction.Description);
                Assert.AreEqual("EP", encumbranceTransaction.Source);
                Assert.AreEqual(encPoTransaction.EncPoDateAssocMember, encumbranceTransaction.TransactionDate);
            }

            // Loop through the requisition entries in the data contract and compare each property with the requisition type domain transactions.
            foreach (var encReqTransaction in encFyrDataContract.EncReqEntityAssociation)
            {
                GlTransaction encumbranceTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Source == "ER"
                    && x.ReferenceNumber == encReqTransaction.EncReqNoAssocMember);

                Assert.IsNotNull(encumbranceTransaction);
                Assert.AreEqual(encReqTransaction.EncReqAmtAssocMember, encumbranceTransaction.Amount);
                Assert.AreEqual(encReqTransaction.EncReqNoAssocMember, encumbranceTransaction.ReferenceNumber);
                Assert.AreEqual(encReqTransaction.EncReqIdAssocMember, encumbranceTransaction.DocumentId);
                Assert.AreEqual(encReqTransaction.EncReqVendorAssocMember, encumbranceTransaction.Description);
                Assert.AreEqual(encReqTransaction.EncReqDateAssocMember, encumbranceTransaction.TransactionDate);
            }

            // Assert the actuals pending posting is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.MemoActualsAmount);

            // Assert the abudget pending posting is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.MemoBudgetAmount);
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_Null_glAccountFundsAssociation()
        {
            // Null the GL Account record.
            glAcctsDataContract.MemosEntityAssociation[1] = null;

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
            }

            // Sum of the debit minus credit for each selected actuals GLA.FYR data contract.
            var expectedActualsGlaAmount = expectedActualsGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total actual amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedActualsGlaAmount, activityDetailDomain.ActualAmount);

            // The number of JE transactions has to be the same.
            IEnumerable<GlTransaction> jeActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "JE").ToList();
            var glaJeRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "JE").ToList();
            Assert.AreEqual(jeActualsDomainTransactions.Count(), glaJeRecords.Count());

            // The number of PJ transactions has to be the same.
            IEnumerable<GlTransaction> pjActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "PJ").ToList();
            var glaPjRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "PJ").ToList();
            Assert.AreEqual(pjActualsDomainTransactions.Count(), glaPjRecords.Count());

            // Keep the count of actual transactions.
            var actualsDomainTransactionCount = jeActualsDomainTransactions.Count() + pjActualsDomainTransactions.Count();

            // Sum of the debit minus credit for each selected budget GLA.FYR data contract.
            var expectedBudgetGlaAmount = expectedBudgetGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total budget amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedBudgetGlaAmount, activityDetailDomain.BudgetAmount);

            // The number of AB transactions has to be the same.
            IEnumerable<GlTransaction> abBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "AB").ToList();
            var glaAbRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "AB").ToList();
            Assert.AreEqual(abBudgetDomainTransactions.Count(), glaAbRecords.Count());

            // The number of BU transactions has to be the same.
            IEnumerable<GlTransaction> buBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BU").ToList();
            var glaBuRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BU").ToList();
            Assert.AreEqual(buBudgetDomainTransactions.Count(), glaBuRecords.Count());

            // The number of BC transactions has to be the same.
            IEnumerable<GlTransaction> bcBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BC").ToList();
            var glaBcRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BC").ToList();
            Assert.AreEqual(bcBudgetDomainTransactions.Count(), glaBcRecords.Count());

            // Keep the count of budget transactions.
            var budgetDomainTransactionCount = abBudgetDomainTransactions.Count() + buBudgetDomainTransactions.Count() + bcBudgetDomainTransactions.Count();

            // Sum of the amount from each encumbrance entry in the ENC.FYR data contract.
            var expectedEncumbranceEncAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember);
            // Add the sum from the requisition entries.
            expectedEncumbranceEncAmount += encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember);
            // Assert the total encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceEncAmount, activityDetailDomain.EncumbranceAmount);

            // The number of encumbrance transactions has to be the same as the number of entries in the ENC.FYR record.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance || x.GlTransactionType == GlTransactionType.Requisition).ToList().ToList();
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encumbranceRecordCount);

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance
            // data contract has to match the number of transactions in the domain.
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount, activityDetailDomain.Transactions.Count());

            // Lastly, make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(actualsDomainTransactionCount + budgetDomainTransactionCount + encumbranceRecordCount, activityDetailDomain.Transactions.Count());

            // Loop through the actuals data contracts and compare each property with the actuals domain transactions.
            foreach (var glaDataContract in expectedActualsGlaRecords)
            {
                GlTransaction actualTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(actualTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, actualTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, actualTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, actualTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, actualTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, actualTransaction.TransactionDate);
            }

            // Loop through the budget data contracts and compare each property with the budget domain transactions.
            foreach (var glaDataContract in expectedBudgetGlaRecords)
            {
                GlTransaction budgetTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(budgetTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, budgetTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, budgetTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, budgetTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, budgetTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, budgetTransaction.TransactionDate);
            }

            // Loop through the encumbrace entries in the data contract and compare each property with the encumbrance type domain transactions.
            foreach (var encPoTransaction in encFyrDataContract.EncPoEntityAssociation)
            {
                GlTransaction encumbranceTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Source == "EP"
                    && x.ReferenceNumber == encPoTransaction.EncPoNoAssocMember);

                Assert.IsNotNull(encumbranceTransaction);
                Assert.AreEqual(encPoTransaction.EncPoAmtAssocMember, encumbranceTransaction.Amount);
                Assert.AreEqual(encPoTransaction.EncPoNoAssocMember, encumbranceTransaction.ReferenceNumber);
                Assert.AreEqual(encPoTransaction.EncPoIdAssocMember, encumbranceTransaction.DocumentId);
                Assert.AreEqual(encPoTransaction.EncPoVendorAssocMember, encumbranceTransaction.Description);
                Assert.AreEqual("EP", encumbranceTransaction.Source);
                Assert.AreEqual(encPoTransaction.EncPoDateAssocMember, encumbranceTransaction.TransactionDate);
            }

            // Loop through the requisition entries in the data contract and compare each property with the requisition type domain transactions.
            foreach (var encReqTransaction in encFyrDataContract.EncReqEntityAssociation)
            {
                GlTransaction encumbranceTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Source == "ER"
                    && x.ReferenceNumber == encReqTransaction.EncReqNoAssocMember);

                Assert.IsNotNull(encumbranceTransaction);
                Assert.AreEqual(encReqTransaction.EncReqAmtAssocMember, encumbranceTransaction.Amount);
                Assert.AreEqual(encReqTransaction.EncReqNoAssocMember, encumbranceTransaction.ReferenceNumber);
                Assert.AreEqual(encReqTransaction.EncReqIdAssocMember, encumbranceTransaction.DocumentId);
                Assert.AreEqual(encReqTransaction.EncReqVendorAssocMember, encumbranceTransaction.Description);
                Assert.AreEqual(encReqTransaction.EncReqDateAssocMember, encumbranceTransaction.TransactionDate);
            }

            // Assert the actuals pending posting is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.MemoActualsAmount);

            // Assert the abudget pending posting is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.MemoBudgetAmount);
        }

        [TestMethod]
        [Ignore]
        public async Task QueryGlActivityDetailAsync_ActualsPendingPosting_Only_Success()
        {
            // No GLA records.
            glaFyrDataContracts = new Collection<GlaFyr>();

            // Remove any encumbrances.
            encFyrDataContract = null;

            glAcctsDataContract = new GlAccts()
            {
                Recordkey = "10_00_01_01_33333_51001",

                // Use the dynamic dates so the test doesn't fail in next year.
                AvailFundsController = new List<string>() { futureFiscalYear, todaysFiscalYear, pastFiscalYear },
                GlFreezeFlags = new List<string>() { "O", "O", "O" },
                GlActualMemos = new List<decimal?>() { 0m, 54.32m, 0m },
                GlActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbranceMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                GlRequisitionMemos = new List<decimal?>() { 0m, 0m, 0m },
                FaActualMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbranceMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                FaRequisitionMemo = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetLinkage = new List<string>() { null, null, null },
                GlIncludeInPool = new List<string>() { null, null, null },
                GlPooledType = new List<string>() { null, null, null }
            };
            glAcctsDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Assert the total encumbrance amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.EncumbranceAmount);

            // Assert the total actual amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.ActualAmount);

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The GL account domain entity should not have any transactions.
            Assert.AreEqual(0, activityDetailDomain.Transactions.Count());

            // Get the actuals memo amount from the GL.ACCTS data contract and compare it to the domain entity amount.
            var actualsPendingPosting = glAcctsDataContract.GlActualMemos[1];
            Assert.AreEqual(actualsPendingPosting, activityDetailDomain.MemoActualsAmount);

            // Assert the budget pending amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.MemoBudgetAmount);
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_ActualsPostedOnly_Success()
        {
            // Remove any encumbrances.
            encFyrDataContract = null;

            // Update the GLA data contracts with only the actuals ones.
            var actualsGlaFyrDataContracts = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "PJ" && x.GlaSource == "JE"));
            glaFyrDataContracts = new ObservableCollection<GlaFyr>(actualsGlaFyrDataContracts);

            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "PJ" || x.GlaSource == "JE")).ToList();

            glAcctsDataContract = new GlAccts()
            {
                Recordkey = "10_00_01_01_33333_51001",
                AvailFundsController = new List<string>() { "2017", "2016", "2015" },
                GlFreezeFlags = new List<string>() { "O", "O", "O" },
                GlActualMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlActualPosted = new List<decimal?>() { 0m, 284.11m, 0m },
                GlBudgetMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbranceMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                GlRequisitionMemos = new List<decimal?>() { 0m, 0m, 0m },
                FaActualMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbranceMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                FaRequisitionMemo = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetLinkage = new List<string>() { null, null, null },
                GlIncludeInPool = new List<string>() { null, null, null },
                GlPooledType = new List<string>() { null, null, null }
            };
            glAcctsDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // List of GLA records that are encumbrances.
            var expectedEncumbranceGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "3")
                {
                    expectedEncumbranceGlaRecords.Add(dataContract);
                }
            }

            // Assert there are no budget records.
            Assert.AreEqual(expectedBudgetGlaRecords.Count(), 0);

            // Assert there are no encumbrance records.
            Assert.AreEqual(expectedEncumbranceGlaRecords.Count(), 0);

            // Sum of the debit minus credit for each selected actuals GLA.FYR data contract.
            var expectedActualsGlaAmount = expectedActualsGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total actual amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedActualsGlaAmount, activityDetailDomain.ActualAmount);

            // The number of JE transactions has to be the same.
            IEnumerable<GlTransaction> jeActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "JE").ToList();
            var glaJeRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "JE").ToList();
            Assert.AreEqual(jeActualsDomainTransactions.Count(), glaJeRecords.Count());

            // The number of PJ transactions has to be the same.
            IEnumerable<GlTransaction> pjActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "PJ").ToList();
            var glaPjRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "PJ").ToList();
            Assert.AreEqual(pjActualsDomainTransactions.Count(), glaPjRecords.Count());

            // Keep the count of actual transactions.
            var actualsDomainTransactionCount = jeActualsDomainTransactions.Count() + pjActualsDomainTransactions.Count();

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(actualsDomainTransactionCount, activityDetailDomain.Transactions.Count());

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // Assert the total encumbrance amount is $0.00.
            Assert.AreEqual(activityDetailDomain.EncumbranceAmount, 0m);

            // The number of encumbrance transactions is 0.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            Assert.AreEqual(encDomainTransactions.Count(), 0);

            // The number of requisition transactions is 0.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            Assert.AreEqual(reqDomainTransactions.Count(), 0);

            // The total number of transactions in the domain entity has to equal the number of actual transactions.
            Assert.AreEqual(actualsDomainTransactionCount, activityDetailDomain.Transactions.Count());

            // Loop through the actuals data contracts and compare each property with the actuals domain transactions.
            foreach (var glaDataContract in expectedActualsGlaRecords)
            {
                GlTransaction actualTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(actualTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, actualTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, actualTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, actualTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, actualTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, actualTransaction.TransactionDate);
            }

            // Get the actuals memo amount from the GL.ACCTS data contract and assert that it is $0.00.
            var actualsPendingPosting = glAcctsDataContract.GlActualMemos[1];
            Assert.AreEqual(activityDetailDomain.MemoActualsAmount, 0m);

            // Assert the budget pending amount is $0.00.
            Assert.AreEqual(activityDetailDomain.MemoBudgetAmount, 0m);
        }

        [TestMethod]
        [Ignore]
        public async Task QueryGlActivityDetailAsync_ActualsPostedAndPendingPosting_Success()
        {
            // Remove any encumbrances.
            encFyrDataContract = null;

            // Update the GLA data contracts with only the actuals ones.
            var actualsGlaFyrDataContracts = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "PJ" && x.GlaSource == "JE"));
            glaFyrDataContracts = new ObservableCollection<GlaFyr>(actualsGlaFyrDataContracts);

            glAcctsDataContract = new GlAccts()
            {
                Recordkey = "10_00_01_01_33333_51001",

                // Use the dynamic dates so the test doesn't fail in next year.
                AvailFundsController = new List<string>() { futureFiscalYear, todaysFiscalYear, pastFiscalYear },
                GlFreezeFlags = new List<string>() { "O", "O", "O" },
                GlActualMemos = new List<decimal?>() { 0m, 54.32m, 0m },
                GlActualPosted = new List<decimal?>() { 0m, 284.11m, 0m },
                GlBudgetMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbranceMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                GlRequisitionMemos = new List<decimal?>() { 0m, 0m, 0m },
                FaActualMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbranceMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                FaRequisitionMemo = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetLinkage = new List<string>() { null, null, null },
                GlIncludeInPool = new List<string>() { null, null, null },
                GlPooledType = new List<string>() { null, null, null }
            };
            glAcctsDataContract.buildAssociations();

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "PJ" || x.GlaSource == "JE")).ToList();
            // Remove any encumbrances.
            encFyrDataContract = null;

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // List of GLA records that are encumbrances.
            var expectedEncumbranceGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "3")
                {
                    expectedEncumbranceGlaRecords.Add(dataContract);
                }

            }

            // Assert there are no budget records.
            Assert.AreEqual(expectedBudgetGlaRecords.Count(), 0);

            // Assert there are no encumbrance records.
            Assert.AreEqual(expectedEncumbranceGlaRecords.Count(), 0);

            // Sum of the debit minus credit for each selected actuals GLA.FYR data contract.
            var expectedActualsGlaAmount = expectedActualsGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total actual amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedActualsGlaAmount, activityDetailDomain.ActualAmount);

            // The number of JE transactions has to be the same.
            IEnumerable<GlTransaction> jeActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "JE").ToList();
            var glaJeRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "JE").ToList();
            Assert.AreEqual(jeActualsDomainTransactions.Count(), glaJeRecords.Count());

            // The number of PJ transactions has to be the same.
            IEnumerable<GlTransaction> pjActualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "PJ").ToList();
            var glaPjRecords = expectedActualsGlaRecords.Where(x => x.GlaSource == "PJ").ToList();
            Assert.AreEqual(pjActualsDomainTransactions.Count(), glaPjRecords.Count());

            // Keep the count of actual transactions.
            var actualsDomainTransactionCount = jeActualsDomainTransactions.Count() + pjActualsDomainTransactions.Count();

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(actualsDomainTransactionCount, activityDetailDomain.Transactions.Count());

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // Assert the total encumbrance amount is $0.00.
            Assert.AreEqual(activityDetailDomain.EncumbranceAmount, 0m);

            // The number of encumbrance transactions is 0.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            Assert.AreEqual(encDomainTransactions.Count(), 0);

            // The number of requisition transactions is 0.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            Assert.AreEqual(reqDomainTransactions.Count(), 0);

            // The total number of transactions in the domain entity has to equal the number of actual transactions.
            Assert.AreEqual(actualsDomainTransactionCount, activityDetailDomain.Transactions.Count());

            // Loop through the actuals data contracts and compare each property with the actuals domain transactions.
            foreach (var glaDataContract in expectedActualsGlaRecords)
            {
                GlTransaction actualTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(actualTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, actualTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, actualTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, actualTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, actualTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, actualTransaction.TransactionDate);
            }

            // Get the actuals memo amount from the GL.ACCTS data contract and compare it to the domain entity amount.
            var actualsPendingPosting = glAcctsDataContract.GlActualMemos[1];
            Assert.AreEqual(actualsPendingPosting, activityDetailDomain.MemoActualsAmount);

            // Assert the budget pending amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.MemoBudgetAmount);
        }

        [TestMethod]
        [Ignore]
        public async Task QueryGlActivityDetailAsync_BudgetPendingPosting_Only_Success()
        {
            // No GLA records.
            glaFyrDataContracts = new Collection<GlaFyr>();

            // Remove any encumbrances.
            encFyrDataContract = null;

            glAcctsDataContract = new GlAccts()
            {
                Recordkey = "10_00_01_01_33333_51001",

                // Use the dynamic dates so the test doesn't fail in next year.
                AvailFundsController = new List<string>() { futureFiscalYear, todaysFiscalYear, pastFiscalYear },
                GlFreezeFlags = new List<string>() { "O", "O", "O" },
                GlActualMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetMemos = new List<decimal?>() { 0m, 9876.50m, 0m },
                GlBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbranceMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                GlRequisitionMemos = new List<decimal?>() { 0m, 0m, 0m },
                FaActualMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbranceMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                FaRequisitionMemo = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetLinkage = new List<string>() { null, null, null },
                GlIncludeInPool = new List<string>() { null, null, null },
                GlPooledType = new List<string>() { null, null, null }
            };
            glAcctsDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Assert the total encumbrance amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.EncumbranceAmount);

            // Assert the total actual amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.ActualAmount);

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The GL account domain entity should not have any transactions.
            Assert.AreEqual(0, activityDetailDomain.Transactions.Count());

            // Get the budget memo amount from the GL.ACCTS data contract and compare it to the domain entity amount.
            var budgetPendingPosting = glAcctsDataContract.GlBudgetMemos[1];
            Assert.AreEqual(budgetPendingPosting, activityDetailDomain.MemoBudgetAmount);

            // Assert the actuals pending amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.MemoActualsAmount);
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_BudgetPostedOnly_Success()
        {
            // Remove any encumbrances.
            encFyrDataContract = null;

            // Update the GLA data contracts with only the actuals ones.
            var budgetGlaFyrDataContracts = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "AB" || x.GlaSource == "BU" || x.GlaSource == "BC"));
            glaFyrDataContracts = new ObservableCollection<GlaFyr>(budgetGlaFyrDataContracts);

            glAcctsDataContract = new GlAccts()
            {
                Recordkey = "10_00_01_01_33333_51001",
                AvailFundsController = new List<string>() { "2017", "2016", "2015" },
                GlFreezeFlags = new List<string>() { "O", "O", "O" },
                GlActualMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetPosted = new List<decimal?>() { 0m, 4060m, 0m },
                GlEncumbranceMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                GlRequisitionMemos = new List<decimal?>() { 0m, 0m, 0m },
                FaActualMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbranceMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                FaRequisitionMemo = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetLinkage = new List<string>() { null, null, null },
                GlIncludeInPool = new List<string>() { null, null, null },
                GlPooledType = new List<string>() { null, null, null }
            };
            glAcctsDataContract.buildAssociations();

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "AB" | x.GlaSource == "BU" || x.GlaSource == "BC")).ToList();

            // Remove any encumbrances.
            encFyrDataContract = null;

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // List of GLA records that are encumbrances.
            var expectedEncumbranceGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "3")
                {
                    expectedEncumbranceGlaRecords.Add(dataContract);
                }
            }

            // Assert there are no actuals records.
            Assert.AreEqual(expectedActualsGlaRecords.Count(), 0);

            // Assert there are no encumbrance records.
            Assert.AreEqual(expectedEncumbranceGlaRecords.Count(), 0);

            // Sum of the debit minus credit for each selected budget GLA.FYR data contract.
            var expectedBudgetGlaAmount = expectedBudgetGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total BUDGET amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedBudgetGlaAmount, activityDetailDomain.BudgetAmount);

            // The number of AB transactions has to be the same.
            IEnumerable<GlTransaction> abBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "AB").ToList();
            var glaAbRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "AB").ToList();
            Assert.AreEqual(abBudgetDomainTransactions.Count(), glaAbRecords.Count());

            // The number of BU transactions has to be the same.
            IEnumerable<GlTransaction> buBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BU").ToList();
            var glaBuRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BU").ToList();
            Assert.AreEqual(buBudgetDomainTransactions.Count(), glaBuRecords.Count());

            // The number of BC transactions has to be the same.
            IEnumerable<GlTransaction> bcBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BC").ToList();
            var glaBcRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BC").ToList();
            Assert.AreEqual(bcBudgetDomainTransactions.Count(), glaBcRecords.Count());

            // Keep the count of budget transactions.
            var budgetDomainTransactionCount = abBudgetDomainTransactions.Count() + buBudgetDomainTransactions.Count() + bcBudgetDomainTransactions.Count();

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(budgetDomainTransactionCount, activityDetailDomain.Transactions.Count());

            // Assert the total actuals amount is $0.00.
            Assert.AreEqual(activityDetailDomain.ActualAmount, 0m);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // Assert the total encumbrance amount is $0.00.
            Assert.AreEqual(activityDetailDomain.EncumbranceAmount, 0);

            // The number of encumbrance transactions is 0.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            Assert.AreEqual(encDomainTransactions.Count(), 0);

            // The number of requisition transactions is 0.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            Assert.AreEqual(reqDomainTransactions.Count(), 0);

            // The number of budget transactions is has to match the budget contracts.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            var glaBudgetRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "AB" || x.GlaSource == "BU" || x.GlaSource == "BC").ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), glaBudgetRecords.Count());

            // The total number of transactions in the domain entity has to equal the number of budget transactions.
            Assert.AreEqual(budgetDomainTransactionCount, activityDetailDomain.Transactions.Count());

            // Loop through the budget data contracts and compare each property with the budget domain transactions.
            foreach (var glaDataContract in expectedBudgetGlaRecords)
            {
                GlTransaction budgetTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(budgetTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, budgetTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, budgetTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, budgetTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, budgetTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, budgetTransaction.TransactionDate);
            }

            // Get the budget memo amount from the GL.ACCTS data contract and assert it is $0.00.
            var budgetPendingPosting = glAcctsDataContract.GlBudgetMemos[1];
            Assert.AreEqual(activityDetailDomain.MemoBudgetAmount, 0m);

            // Assert the actuals pending amount is $0.00.
            Assert.AreEqual(activityDetailDomain.MemoActualsAmount, 0m);
        }

        [TestMethod]
        [Ignore]
        public async Task QueryGlActivityDetailAsync_BudgetPostedAndPendingPosting_Success()
        {
            // Remove any encumbrances.
            encFyrDataContract = null;

            // Update the GLA data contracts with only the actuals ones.
            var budgetGlaFyrDataContracts = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "AB" || x.GlaSource == "BU" || x.GlaSource == "BC"));
            glaFyrDataContracts = new ObservableCollection<GlaFyr>(budgetGlaFyrDataContracts);

            glAcctsDataContract = new GlAccts()
            {
                Recordkey = "10_00_01_01_33333_51001",

                // Use the dynamic dates so the test doesn't fail in next year.
                AvailFundsController = new List<string>() { futureFiscalYear, todaysFiscalYear, pastFiscalYear },
                GlFreezeFlags = new List<string>() { "O", "O", "O" },
                GlActualMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetMemos = new List<decimal?>() { 0m, 9876.50m, 0m },
                GlBudgetPosted = new List<decimal?>() { 0m, 4060m, 0m },
                GlEncumbranceMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                GlRequisitionMemos = new List<decimal?>() { 0m, 0m, 0m },
                FaActualMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbranceMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                FaRequisitionMemo = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetLinkage = new List<string>() { null, null, null },
                GlIncludeInPool = new List<string>() { null, null, null },
                GlPooledType = new List<string>() { null, null, null }
            };
            glAcctsDataContract.buildAssociations();

            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "AB" | x.GlaSource == "BU" || x.GlaSource == "BC")).ToList();

            // Remove any encumbrances.
            encFyrDataContract = null;

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // List of GLA records that are encumbrances.
            var expectedEncumbranceGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "3")
                {
                    expectedEncumbranceGlaRecords.Add(dataContract);
                }
            }

            // Assert there are no actuals records.
            Assert.AreEqual(expectedActualsGlaRecords.Count(), 0);

            // Assert there are no encumbrance records.
            Assert.AreEqual(expectedEncumbranceGlaRecords.Count(), 0);

            // Sum of the debit minus credit for each selected budget GLA.FYR data contract.
            var expectedBudgetGlaAmount = expectedBudgetGlaRecords.Sum(x => x.GlaDebit - x.GlaCredit);
            // Assert the total BUDGET amount from the data contracts is equal to the one from the domain entities.
            Assert.AreEqual(expectedBudgetGlaAmount, activityDetailDomain.BudgetAmount);

            // The number of AB transactions has to be the same.
            IEnumerable<GlTransaction> abBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "AB").ToList();
            var glaAbRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "AB").ToList();
            Assert.AreEqual(abBudgetDomainTransactions.Count(), glaAbRecords.Count());

            // The number of BU transactions has to be the same.
            IEnumerable<GlTransaction> buBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BU").ToList();
            var glaBuRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BU").ToList();
            Assert.AreEqual(buBudgetDomainTransactions.Count(), glaBuRecords.Count());

            // The number of BC transactions has to be the same.
            IEnumerable<GlTransaction> bcBudgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.Source == "BC").ToList();
            var glaBcRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "BC").ToList();
            Assert.AreEqual(bcBudgetDomainTransactions.Count(), glaBcRecords.Count());

            // Keep the count of budget transactions.
            var budgetDomainTransactionCount = abBudgetDomainTransactions.Count() + buBudgetDomainTransactions.Count() + bcBudgetDomainTransactions.Count();

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(budgetDomainTransactionCount, activityDetailDomain.Transactions.Count());

            // Assert the total actuals amount is $0.00.
            Assert.AreEqual(activityDetailDomain.ActualAmount, 0m);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // Assert the total encumbrance amount is $0.00.
            Assert.AreEqual(activityDetailDomain.EncumbranceAmount, 0);

            // The number of encumbrance transactions is 0.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            Assert.AreEqual(encDomainTransactions.Count(), 0);

            // The number of requisition transactions is 0.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            Assert.AreEqual(reqDomainTransactions.Count(), 0);

            // The number of budget transactions is has to match the budget contracts.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            var glaBudgetRecords = expectedBudgetGlaRecords.Where(x => x.GlaSource == "AB" || x.GlaSource == "BU" || x.GlaSource == "BC").ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), glaBudgetRecords.Count());

            // The total number of transactions in the domain entity has to equal the number of budget transactions.
            Assert.AreEqual(budgetDomainTransactionCount, activityDetailDomain.Transactions.Count());

            // Loop through the budget data contracts and compare each property with the budget domain transactions.
            foreach (var glaDataContract in expectedBudgetGlaRecords)
            {
                GlTransaction budgetTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.Id == glaDataContract.Recordkey);

                Assert.IsNotNull(budgetTransaction);
                Assert.AreEqual(glaDataContract.GlaDebit - glaDataContract.GlaCredit, budgetTransaction.Amount);
                Assert.AreEqual(glaDataContract.GlaRefNo, budgetTransaction.ReferenceNumber);
                Assert.AreEqual(glaDataContract.GlaDescription, budgetTransaction.Description);
                Assert.AreEqual(glaDataContract.GlaSource, budgetTransaction.Source);
                Assert.AreEqual(glaDataContract.GlaTrDate, budgetTransaction.TransactionDate);
            }

            // Get the budget memo amount from the GL.ACCTS data contract and compare it to the domain entity amount.
            var budgetPendingPosting = glAcctsDataContract.GlBudgetMemos[1];
            Assert.AreEqual(budgetPendingPosting, activityDetailDomain.MemoBudgetAmount);

            // Assert the actuals pending amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.MemoActualsAmount);
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_Encumbrances_Only_Success()
        {
            // Update the GLA data contracts with only the encumbrance ones.
            var encumbrancesGlaFyrDataContracts = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "EP"));
            glaFyrDataContracts = new ObservableCollection<GlaFyr>(encumbrancesGlaFyrDataContracts);

            // Remove requisition entries
            encFyrDataContract.EncReqId = new List<string>();
            encFyrDataContract.EncReqNo = new List<string>();
            encFyrDataContract.EncReqVendor = new List<string>();
            encFyrDataContract.EncReqAmt = new List<Decimal?>();
            encFyrDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Sum of the amount from each encumbrance entry.
            var expectedEncumbranceAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember);
            // Assert the total encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceAmount, activityDetailDomain.EncumbranceAmount);

            // Assert that the domain number of transactions is equal to the number of encumbrance transactions
            // in the ENC data contract and the GLA records are ignored.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            var encRecordCount = encFyrDataContract.EncPoEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encRecordCount);

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(encDomainTransactions.Count(), activityDetailDomain.Transactions.Count());

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // Assert the total actuals amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.ActualAmount);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // The number of requisition transactions is 0.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            Assert.AreEqual(reqDomainTransactions.Count(), 0);

            // Loop through the encumbrance data contracts and compare each property with the encumbrance domain transactions.
            foreach (var enc in encFyrDataContract.EncPoEntityAssociation)
            {
                GlTransaction encTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == enc.EncPoNoAssocMember);

                Assert.IsNotNull(encTransaction);
                Assert.AreEqual(enc.EncPoAmtAssocMember, encTransaction.Amount);
                Assert.AreEqual(enc.EncPoNoAssocMember, encTransaction.ReferenceNumber);
                Assert.AreEqual(enc.EncPoIdAssocMember, encTransaction.DocumentId);
                Assert.AreEqual(enc.EncPoVendorAssocMember, encTransaction.Description);
                Assert.AreEqual(enc.EncPoDateAssocMember, encTransaction.TransactionDate);
            }
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_EncumbrancesAndRequisitions_Success()
        {
            // Update the GLA data contracts with only the encumbrance ones.
            var encumbrancesGlaFyrDataContracts = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "EP"));
            glaFyrDataContracts = new ObservableCollection<GlaFyr>(encumbrancesGlaFyrDataContracts);

            // Update GL.ACCTS data contract to make sure it is not used.
            glAcctsDataContract = new GlAccts()
            {
                Recordkey = "10_00_01_01_33333_51001",
                AvailFundsController = new List<string>() { "2017", "2016", "2015" },
                GlFreezeFlags = new List<string>() { "O", "O", "O" },
                GlActualMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetMemos = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                GlEncumbranceMemos = new List<decimal?>() { 0m, 144.43m, 0m },
                GlEncumbrancePosted = new List<decimal?>() { 0m, 11.11m, 0m },
                GlRequisitionMemos = new List<decimal?>() { 0m, 0m, 0m },
                FaActualMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbranceMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                FaRequisitionMemo = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetLinkage = new List<string>() { null, null, null },
                GlIncludeInPool = new List<string>() { null, null, null },
                GlPooledType = new List<string>() { null, null, null }
            };
            glAcctsDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Sum of the amount from each encumbrance entry.
            var expectedEncumbranceAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember);
            // Sum of the amount from each requisition entry.
            var expectedRequisitionAmount = encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember);
            // Assert the total requisition amount plus the encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceAmount + expectedRequisitionAmount, activityDetailDomain.EncumbranceAmount);

            // Assert that the domain number of encumbrance transactions is equal to the number of encumbrance transactions
            // in the ENC data contract and the GLA records are ignored.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            var encRecordCount = encFyrDataContract.EncPoEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encRecordCount);

            // Assert that the domain number of requisition transactions is equal to the number of 
            // requisition transactions in the ENC data contract.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            var requisitionRecordCount = encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(reqDomainTransactions.Count(), requisitionRecordCount);

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(encDomainTransactions.Count() + reqDomainTransactions.Count(), activityDetailDomain.Transactions.Count());

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // Assert the total actuals amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.ActualAmount);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // Loop through the encumbrance data contracts and compare each property with the encumbrance domain transactions.
            foreach (var enc in encFyrDataContract.EncPoEntityAssociation)
            {
                GlTransaction encTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == enc.EncPoNoAssocMember);

                Assert.IsNotNull(encTransaction);
                Assert.AreEqual(enc.EncPoAmtAssocMember, encTransaction.Amount);
                Assert.AreEqual(enc.EncPoNoAssocMember, encTransaction.ReferenceNumber);
                Assert.AreEqual(enc.EncPoIdAssocMember, encTransaction.DocumentId);
                Assert.AreEqual(enc.EncPoVendorAssocMember, encTransaction.Description);
                Assert.AreEqual(enc.EncPoDateAssocMember, encTransaction.TransactionDate);
            }

            // Loop through the requisition data contracts and compare each property with the requisition domain transactions.
            foreach (var req in encFyrDataContract.EncReqEntityAssociation)
            {
                GlTransaction reqTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == req.EncReqNoAssocMember);

                Assert.IsNotNull(reqTransaction);
                Assert.AreEqual(req.EncReqAmtAssocMember, reqTransaction.Amount);
                Assert.AreEqual(req.EncReqNoAssocMember, reqTransaction.ReferenceNumber);
                Assert.AreEqual(req.EncReqIdAssocMember, reqTransaction.DocumentId);
                Assert.AreEqual(req.EncReqVendorAssocMember, reqTransaction.Description);
                Assert.AreEqual(req.EncReqDateAssocMember, reqTransaction.TransactionDate);
            }
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_Requisitions_Only_Success()
        {
            // No GLA records.
            glaFyrDataContracts = new Collection<GlaFyr>();

            // Remove encumbrance entries.
            encFyrDataContract.EncPoAmt = new List<Decimal?>();
            encFyrDataContract.EncPoDate = new List<DateTime?>();
            encFyrDataContract.EncPoId = new List<string>();
            encFyrDataContract.EncPoNo = new List<string>();
            encFyrDataContract.EncPoSource = new List<string>();
            encFyrDataContract.EncPoVendor = new List<string>();
            encFyrDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Sum of the amount from each requisition entry.
            var expectedRequisitionAmount = encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember);
            // Assert the total requisition amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedRequisitionAmount, activityDetailDomain.EncumbranceAmount);

            // Assert that the domain number of transactions is equal to the number of 
            // requisition transactions in the ENC data contract.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            var requisitionRecordCount = encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(reqDomainTransactions.Count(), requisitionRecordCount);

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(reqDomainTransactions.Count(), activityDetailDomain.Transactions.Count());

            // Assert the total actual amount is $0.00.
            Assert.AreEqual(activityDetailDomain.ActualAmount, 0m);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(activityDetailDomain.BudgetAmount, 0m);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // The number of encumbrance transactions is 0.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            Assert.AreEqual(encDomainTransactions.Count(), 0);

            // Loop through the requisition data contracts and compare each property with the requisition domain transactions.
            foreach (var req in encFyrDataContract.EncReqEntityAssociation)
            {
                GlTransaction reqTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == req.EncReqNoAssocMember);

                Assert.IsNotNull(reqTransaction);
                Assert.AreEqual(req.EncReqAmtAssocMember, reqTransaction.Amount);
                Assert.AreEqual(req.EncReqNoAssocMember, reqTransaction.ReferenceNumber);
                Assert.AreEqual(req.EncReqIdAssocMember, reqTransaction.DocumentId);
                Assert.AreEqual(req.EncReqVendorAssocMember, reqTransaction.Description);
                Assert.AreEqual(req.EncReqDateAssocMember, reqTransaction.TransactionDate);
            }
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_GlaTransactionNullRecordkey()
        {
            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            // Null the key of the first data contract.
            glaFyrDataContracts[0].Recordkey = null;

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
            }

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance data contract
            // has to match the number of transactions in the domain minus one because the data contract with the null key.
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount - 1, activityDetailDomain.Transactions.Count());
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_GlaTransactionEmptyRecordkey()
        {
            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            // Null the key of the first data contract.
            glaFyrDataContracts[0].Recordkey = "";

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
            }

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance data contract
            // has to match the number of transactions in the domain minus one because the data contract with the empty key.
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount - 1, activityDetailDomain.Transactions.Count());
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync__GlaTransactionNullGlAccountNumber()
        {
            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            // Remove the GL account from the key of the first data contract.
            glaFyrDataContracts[0].Recordkey = "*1";

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                var sp2Code = matchingSource.ValActionCode2AssocMember;

                // Actuals have an action code of 1.
                if (sp2Code == "1")
                {
                    expectedActualsGlaRecords.Add(dataContract);
                }
                else if (sp2Code == "2")
                {
                    expectedBudgetGlaRecords.Add(dataContract);
                }
            }

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance data contract
            // has to match the number of transactions in the domain minus one because the data contract with the null GL account.
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount - 1, activityDetailDomain.Transactions.Count());
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_GlaTransactionNullSource()
        {
            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            // Null the source of the first data contract.
            glaFyrDataContracts[0].GlaSource = null;

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                if (!string.IsNullOrEmpty(source))
                {
                    var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                    var sp2Code = matchingSource.ValActionCode2AssocMember;

                    // Actuals have an action code of 1.
                    if (sp2Code == "1")
                    {
                        expectedActualsGlaRecords.Add(dataContract);
                    }
                    else if (sp2Code == "2")
                    {
                        expectedBudgetGlaRecords.Add(dataContract);
                    }
                }
            }

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance data contract
            // has to match the number of transactions in the domain because the data contract with the null source code.
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount, activityDetailDomain.Transactions.Count());
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_GlaTransactionEmptySource()
        {
            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            // Null the source of the first data contract.
            glaFyrDataContracts[0].GlaSource = "";

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                if (!string.IsNullOrEmpty(source))
                {
                    var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                    var sp2Code = matchingSource.ValActionCode2AssocMember;

                    // Actuals have an action code of 1.
                    if (sp2Code == "1")
                    {
                        expectedActualsGlaRecords.Add(dataContract);
                    }
                    else if (sp2Code == "2")
                    {
                        expectedBudgetGlaRecords.Add(dataContract);
                    }
                }
            }

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance data contract
            // has to match the number of transactions in the domain because the data contract with the empty source code.
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount, activityDetailDomain.Transactions.Count());
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_GlaTransactionNullReferenceNumber()
        {
            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            // Null the source of the first data contract.
            glaFyrDataContracts[0].GlaRefNo = null;

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                if (!string.IsNullOrEmpty(source))
                {
                    var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                    var sp2Code = matchingSource.ValActionCode2AssocMember;

                    // Actuals have an action code of 1.
                    if (sp2Code == "1")
                    {
                        expectedActualsGlaRecords.Add(dataContract);
                    }
                    else if (sp2Code == "2")
                    {
                        expectedBudgetGlaRecords.Add(dataContract);
                    }
                }
            }

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance data contract has to
            // match the number of transactions in the domain minus one because the data contract with the null reference number.
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount - 1, activityDetailDomain.Transactions.Count());
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_GlaTransactionEmptyReferenceNumber()
        {
            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            // Null the source of the first data contract.
            glaFyrDataContracts[0].GlaRefNo = "";

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                if (!string.IsNullOrEmpty(source))
                {
                    var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                    var sp2Code = matchingSource.ValActionCode2AssocMember;

                    // Actuals have an action code of 1.
                    if (sp2Code == "1")
                    {
                        expectedActualsGlaRecords.Add(dataContract);
                    }
                    else if (sp2Code == "2")
                    {
                        expectedBudgetGlaRecords.Add(dataContract);
                    }
                }
            }

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance data contract has to
            // match the number of transactions in the domain minus one because the data contract with the empty reference number.
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount - 1, activityDetailDomain.Transactions.Count());
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_GlaTransactionNullTransactionDate()
        {
            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            // Null the source of the first data contract.
            glaFyrDataContracts[0].GlaTrDate = null;

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                if (!string.IsNullOrEmpty(source))
                {
                    var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                    var sp2Code = matchingSource.ValActionCode2AssocMember;

                    // Actuals have an action code of 1.
                    if (sp2Code == "1")
                    {
                        expectedActualsGlaRecords.Add(dataContract);
                    }
                    else if (sp2Code == "2")
                    {
                        expectedBudgetGlaRecords.Add(dataContract);
                    }
                }
            }

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance data contract has to
            // match the number of transactions in the domain minus one because the data contract with the null transaction date.
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount - 1, activityDetailDomain.Transactions.Count());
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_GlaTransactionInvalidGlSourceCode()
        {
            // Get GLA records for the one GL account for the fiscal year.
            var expectedGlaRecords = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount)).ToList();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == "JE").ValActionCode2AssocMember = "9";

            // List of GLA records that are actuals.
            var expectedActualsGlaRecords = new List<GlaFyr>();

            // List of GLA records that are budget.
            var expectedBudgetGlaRecords = new List<GlaFyr>();

            // Loop through the selected GLA records and separate actuals from encumbrances and budget.
            foreach (var dataContract in expectedGlaRecords)
            {
                var source = dataContract.GlaSource;
                if (!string.IsNullOrEmpty(source))
                {
                    var matchingSource = glSourceCodes.ValsEntityAssociation.FirstOrDefault(x => x.ValInternalCodeAssocMember == source);
                    var sp2Code = matchingSource.ValActionCode2AssocMember;

                    // Actuals have an action code of 1.
                    if (sp2Code == "1")
                    {
                        expectedActualsGlaRecords.Add(dataContract);
                    }
                    else if (sp2Code == "2")
                    {
                        expectedBudgetGlaRecords.Add(dataContract);
                    }
                }
            }

            // The number of selected data contracts for actuals and budget from GLA plus the one encumbrance data contract has to
            // match the number of transactions in the domain minus the two JE ones because the invalid source code assigned to it.
            var encumbranceRecordCount = encFyrDataContract.EncPoEntityAssociation.Count() + encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(expectedActualsGlaRecords.Count() + expectedBudgetGlaRecords.Count() + encumbranceRecordCount + 2, activityDetailDomain.Transactions.Count());
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_EncTransactionNullReferenceNumber()
        {
            // Update the GLA data contracts with only the encumbrance ones.
            var encumbrancesGlaFyrDataContracts = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "EP"));
            glaFyrDataContracts = new ObservableCollection<GlaFyr>(encumbrancesGlaFyrDataContracts);

            // Null out one of the encumbrance's document number.
            encFyrDataContract.EncPoNo = new List<string>() { "P000001", null, "RV0000044" };
            encFyrDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Sum of the amount from each encumbrance entry minus the one with the null document number.
            var expectedEncumbranceAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember) - encFyrDataContract.EncPoAmt[1];
            // Sum of the amount from each requisition entry.
            var expectedRequisitionAmount = encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember);
            // Assert the total requisition amount plus the encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceAmount + expectedRequisitionAmount, activityDetailDomain.EncumbranceAmount);

            // Assert that the domain number of encumbrance transactions is equal to the number of encumbrance transactions
            // in the ENC data contract minus 1 and the GLA records are ignored.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            var encRecordCount = encFyrDataContract.EncPoEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encRecordCount - 1);

            // Assert that the domain number of requisition transactions is equal to the number of 
            // requisition transactions in the ENC data contract.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            var requisitionRecordCount = encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(reqDomainTransactions.Count(), requisitionRecordCount);

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(encDomainTransactions.Count() + reqDomainTransactions.Count(), activityDetailDomain.Transactions.Count());

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // Assert the total actuals amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.ActualAmount);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // Loop through the encumbrance data contracts and compare each property with the encumbrance domain transactions.
            foreach (var enc in encFyrDataContract.EncPoEntityAssociation)
            {
                if (!string.IsNullOrEmpty(enc.EncPoNoAssocMember))
                {
                    GlTransaction encTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == enc.EncPoNoAssocMember);

                    Assert.IsNotNull(encTransaction);
                    Assert.AreEqual(enc.EncPoAmtAssocMember, encTransaction.Amount);
                    Assert.AreEqual(enc.EncPoNoAssocMember, encTransaction.ReferenceNumber);
                    Assert.AreEqual(enc.EncPoIdAssocMember, encTransaction.DocumentId);
                    Assert.AreEqual(enc.EncPoVendorAssocMember, encTransaction.Description);
                    Assert.AreEqual(enc.EncPoDateAssocMember, encTransaction.TransactionDate);
                }
            }

            // Loop through the requisition data contracts and compare each property with the requisition domain transactions.
            foreach (var req in encFyrDataContract.EncReqEntityAssociation)
            {
                GlTransaction reqTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == req.EncReqNoAssocMember);

                Assert.IsNotNull(reqTransaction);
                Assert.AreEqual(req.EncReqAmtAssocMember, reqTransaction.Amount);
                Assert.AreEqual(req.EncReqNoAssocMember, reqTransaction.ReferenceNumber);
                Assert.AreEqual(req.EncReqIdAssocMember, reqTransaction.DocumentId);
                Assert.AreEqual(req.EncReqVendorAssocMember, reqTransaction.Description);
                Assert.AreEqual(req.EncReqDateAssocMember, reqTransaction.TransactionDate);
            }
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_EncTransactionEmptyReferenceNumber()
        {
            // Update the GLA data contracts with only the encumbrance ones.
            var encumbrancesGlaFyrDataContracts = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "EP"));
            glaFyrDataContracts = new ObservableCollection<GlaFyr>(encumbrancesGlaFyrDataContracts);

            // Null out one of the encumbrance's document number.
            encFyrDataContract.EncPoNo = new List<string>() { "P000001", "", "RV0000044" };
            encFyrDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Sum of the amount from each encumbrance entry minus the one with the null document number.
            var expectedEncumbranceAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember) - encFyrDataContract.EncPoAmt[1];
            // Sum of the amount from each requisition entry.
            var expectedRequisitionAmount = encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember);
            // Assert the total requisition amount plus the encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceAmount + expectedRequisitionAmount, activityDetailDomain.EncumbranceAmount);

            // Assert that the domain number of encumbrance transactions is equal to the number of encumbrance transactions
            // in the ENC data contract minus 1 and the GLA records are ignored.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            var encRecordCount = encFyrDataContract.EncPoEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encRecordCount - 1);

            // Assert that the domain number of requisition transactions is equal to the number of 
            // requisition transactions in the ENC data contract.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            var requisitionRecordCount = encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(reqDomainTransactions.Count(), requisitionRecordCount);

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(encDomainTransactions.Count() + reqDomainTransactions.Count(), activityDetailDomain.Transactions.Count());

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // Assert the total actuals amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.ActualAmount);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // Loop through the encumbrance data contracts and compare each property with the encumbrance domain transactions.
            foreach (var enc in encFyrDataContract.EncPoEntityAssociation)
            {
                if (!string.IsNullOrEmpty(enc.EncPoNoAssocMember))
                {
                    GlTransaction encTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == enc.EncPoNoAssocMember);

                    Assert.IsNotNull(encTransaction);
                    Assert.AreEqual(enc.EncPoAmtAssocMember, encTransaction.Amount);
                    Assert.AreEqual(enc.EncPoNoAssocMember, encTransaction.ReferenceNumber);
                    Assert.AreEqual(enc.EncPoIdAssocMember, encTransaction.DocumentId);
                    Assert.AreEqual(enc.EncPoVendorAssocMember, encTransaction.Description);
                    Assert.AreEqual(enc.EncPoDateAssocMember, encTransaction.TransactionDate);
                }
            }

            // Loop through the requisition data contracts and compare each property with the requisition domain transactions.
            foreach (var req in encFyrDataContract.EncReqEntityAssociation)
            {
                GlTransaction reqTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == req.EncReqNoAssocMember);

                Assert.IsNotNull(reqTransaction);
                Assert.AreEqual(req.EncReqAmtAssocMember, reqTransaction.Amount);
                Assert.AreEqual(req.EncReqNoAssocMember, reqTransaction.ReferenceNumber);
                Assert.AreEqual(req.EncReqIdAssocMember, reqTransaction.DocumentId);
                Assert.AreEqual(req.EncReqVendorAssocMember, reqTransaction.Description);
                Assert.AreEqual(req.EncReqDateAssocMember, reqTransaction.TransactionDate);
            }
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_EncTransactionNullTransactionDate()
        {
            // Update the GLA data contracts with only the encumbrance ones.
            var encumbrancesGlaFyrDataContracts = glaFyrDataContracts.Where(x => x.Recordkey.Contains(param1_glAccount) && (x.GlaSource == "EP"));
            glaFyrDataContracts = new ObservableCollection<GlaFyr>(encumbrancesGlaFyrDataContracts);

            // Null out one of the encumbrance's document number.
            encFyrDataContract.EncPoDate = new List<DateTime?>() { DateTime.Now, null, DateTime.Now };
            encFyrDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Sum of the amount from each encumbrance entry.
            var expectedEncumbranceAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember) - encFyrDataContract.EncPoAmt[1];
            // Sum of the amount from each requisition entry.
            var expectedRequisitionAmount = encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember);
            // Assert the total requisition amount plus the encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceAmount + expectedRequisitionAmount, activityDetailDomain.EncumbranceAmount);

            // Assert that the domain number of encumbrance transactions is equal to the number of encumbrance transactions
            // in the ENC data contract minus 1 and the GLA records are ignored.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            var encRecordCount = encFyrDataContract.EncPoEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encRecordCount - 1);

            // Assert that the domain number of requisition transactions is equal to the number of 
            // requisition transactions in the ENC data contract.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            var requisitionRecordCount = encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(reqDomainTransactions.Count(), requisitionRecordCount);

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(encDomainTransactions.Count() + reqDomainTransactions.Count(), activityDetailDomain.Transactions.Count());

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // Assert the total actuals amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.ActualAmount);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // Loop through the encumbrance data contracts and compare each property with the encumbrance domain transactions.
            foreach (var enc in encFyrDataContract.EncPoEntityAssociation)
            {
                if (enc.EncPoDateAssocMember.HasValue)
                {
                    GlTransaction encTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == enc.EncPoNoAssocMember);

                    Assert.IsNotNull(encTransaction);
                    Assert.AreEqual(enc.EncPoAmtAssocMember, encTransaction.Amount);
                    Assert.AreEqual(enc.EncPoNoAssocMember, encTransaction.ReferenceNumber);
                    Assert.AreEqual(enc.EncPoIdAssocMember, encTransaction.DocumentId);
                    Assert.AreEqual(enc.EncPoVendorAssocMember, encTransaction.Description);
                    Assert.AreEqual(enc.EncPoDateAssocMember, encTransaction.TransactionDate);
                }
            }

            // Loop through the requisition data contracts and compare each property with the requisition domain transactions.
            foreach (var req in encFyrDataContract.EncReqEntityAssociation)
            {
                GlTransaction reqTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == req.EncReqNoAssocMember);

                Assert.IsNotNull(reqTransaction);
                Assert.AreEqual(req.EncReqAmtAssocMember, reqTransaction.Amount);
                Assert.AreEqual(req.EncReqNoAssocMember, reqTransaction.ReferenceNumber);
                Assert.AreEqual(req.EncReqIdAssocMember, reqTransaction.DocumentId);
                Assert.AreEqual(req.EncReqVendorAssocMember, reqTransaction.Description);
                Assert.AreEqual(req.EncReqDateAssocMember, reqTransaction.TransactionDate);
            }
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_ReqTransactionNullReferenceNumber()
        {
            // No GLA records.
            glaFyrDataContracts = new Collection<GlaFyr>();

            // Null out one of the requisition's document number.
            encFyrDataContract.EncReqNo = new List<string>() { "0000022", null };
            encFyrDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Sum of the amount from each requisition entry minus the one with the null document number.
            var expectedEncumbranceAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember);
            // Sum of the amount from each encumbrance entry.
            var expectedRequisitionAmount = encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember) - encFyrDataContract.EncReqAmt[1];
            // Assert the total requisition amount plus the encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceAmount + expectedRequisitionAmount, activityDetailDomain.EncumbranceAmount);

            // Assert that the domain number of requisition transactions is equal to the
            // number of requisition transactions in the ENC data contract minus 1.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            var reqRecordCount = encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(reqDomainTransactions.Count(), reqRecordCount - 1);

            // Assert that the domain number of encumbrance transactions is equal to the number of 
            // encumbrance transactions in the ENC data contract.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            var encRecordCount = encFyrDataContract.EncPoEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encRecordCount);

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(encDomainTransactions.Count() + reqDomainTransactions.Count(), activityDetailDomain.Transactions.Count());

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // Assert the total actuals amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.ActualAmount);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // Loop through the encumbrance data contracts and compare each property with the encumbrance domain transactions.
            foreach (var enc in encFyrDataContract.EncPoEntityAssociation)
            {
                GlTransaction encTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == enc.EncPoNoAssocMember);

                Assert.IsNotNull(encTransaction);
                Assert.AreEqual(enc.EncPoAmtAssocMember, encTransaction.Amount);
                Assert.AreEqual(enc.EncPoNoAssocMember, encTransaction.ReferenceNumber);
                Assert.AreEqual(enc.EncPoIdAssocMember, encTransaction.DocumentId);
                Assert.AreEqual(enc.EncPoVendorAssocMember, encTransaction.Description);
                Assert.AreEqual(enc.EncPoDateAssocMember, encTransaction.TransactionDate);
            }

            // Loop through the requisition data contracts and compare each property with the requisition domain transactions.
            foreach (var req in encFyrDataContract.EncReqEntityAssociation)
            {
                if (!string.IsNullOrEmpty(req.EncReqNoAssocMember))
                {

                    GlTransaction reqTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == req.EncReqNoAssocMember);

                    Assert.IsNotNull(reqTransaction);
                    Assert.AreEqual(req.EncReqAmtAssocMember, reqTransaction.Amount);
                    Assert.AreEqual(req.EncReqNoAssocMember, reqTransaction.ReferenceNumber);
                    Assert.AreEqual(req.EncReqIdAssocMember, reqTransaction.DocumentId);
                    Assert.AreEqual(req.EncReqVendorAssocMember, reqTransaction.Description);
                    Assert.AreEqual(req.EncReqDateAssocMember, reqTransaction.TransactionDate);
                }
            }
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_ReqTransactionEmptyReferenceNumber()
        {
            // No GLA records.
            glaFyrDataContracts = new Collection<GlaFyr>();

            // Null out one of the requisition's document number.
            encFyrDataContract.EncReqNo = new List<string>() { "0000022", "" };
            encFyrDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Sum of the amount from each requisition entry minus the one with the null document number.
            var expectedEncumbranceAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember);
            // Sum of the amount from each encumbrance entry.
            var expectedRequisitionAmount = encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember) - encFyrDataContract.EncReqAmt[1];
            // Assert the total requisition amount plus the encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceAmount + expectedRequisitionAmount, activityDetailDomain.EncumbranceAmount);

            // Assert that the domain number of requisition transactions is equal to the
            // number of requisition transactions in the ENC data contract minus 1.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            var reqRecordCount = encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(reqDomainTransactions.Count(), reqRecordCount - 1);

            // Assert that the domain number of encumbrance transactions is equal to the number of 
            // encumbrance transactions in the ENC data contract.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            var encRecordCount = encFyrDataContract.EncPoEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encRecordCount);

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(encDomainTransactions.Count() + reqDomainTransactions.Count(), activityDetailDomain.Transactions.Count());

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // Assert the total actuals amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.ActualAmount);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // Loop through the encumbrance data contracts and compare each property with the encumbrance domain transactions.
            foreach (var enc in encFyrDataContract.EncPoEntityAssociation)
            {
                GlTransaction encTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == enc.EncPoNoAssocMember);

                Assert.IsNotNull(encTransaction);
                Assert.AreEqual(enc.EncPoAmtAssocMember, encTransaction.Amount);
                Assert.AreEqual(enc.EncPoNoAssocMember, encTransaction.ReferenceNumber);
                Assert.AreEqual(enc.EncPoIdAssocMember, encTransaction.DocumentId);
                Assert.AreEqual(enc.EncPoVendorAssocMember, encTransaction.Description);
                Assert.AreEqual(enc.EncPoDateAssocMember, encTransaction.TransactionDate);
            }

            // Loop through the requisition data contracts and compare each property with the requisition domain transactions.
            foreach (var req in encFyrDataContract.EncReqEntityAssociation)
            {
                if (!string.IsNullOrEmpty(req.EncReqNoAssocMember))
                {

                    GlTransaction reqTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == req.EncReqNoAssocMember);

                    Assert.IsNotNull(reqTransaction);
                    Assert.AreEqual(req.EncReqAmtAssocMember, reqTransaction.Amount);
                    Assert.AreEqual(req.EncReqNoAssocMember, reqTransaction.ReferenceNumber);
                    Assert.AreEqual(req.EncReqIdAssocMember, reqTransaction.DocumentId);
                    Assert.AreEqual(req.EncReqVendorAssocMember, reqTransaction.Description);
                    Assert.AreEqual(req.EncReqDateAssocMember, reqTransaction.TransactionDate);
                }
            }
        }

        [TestMethod]
        public async Task QueryGlActivityDetailAsync_ReqTransactionNullTransactionDate()
        {
            // No GLA records.
            glaFyrDataContracts = new Collection<GlaFyr>();

            // Null out one of the encumbrance's document number.
            encFyrDataContract.EncReqDate = new List<DateTime?>() { DateTime.Now, null };
            encFyrDataContract.buildAssociations();

            this.param3_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

            // Populate the domain entities.
            GlAccountActivityDetail activityDetailDomain = await QueryGlActivityDetailAsync();

            // Sum of the amount from each requisition entry minus the one with the null document number.
            var expectedEncumbranceAmount = encFyrDataContract.EncPoEntityAssociation.Sum(x => x.EncPoAmtAssocMember);
            // Sum of the amount from each encumbrance entry.
            var expectedRequisitionAmount = encFyrDataContract.EncReqEntityAssociation.Sum(x => x.EncReqAmtAssocMember) - encFyrDataContract.EncReqAmt[1];
            // Assert the total requisition amount plus the encumbrance amount from the data contract is equal to the one from the domain entities.
            Assert.AreEqual(expectedEncumbranceAmount + expectedRequisitionAmount, activityDetailDomain.EncumbranceAmount);

            // Assert that the domain number of requisition transactions is equal to the
            // number of requisition transactions in the ENC data contract minus 1.
            IEnumerable<GlTransaction> reqDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).ToList();
            var reqRecordCount = encFyrDataContract.EncReqEntityAssociation.Count();
            Assert.AreEqual(reqDomainTransactions.Count(), reqRecordCount - 1);

            // Assert that the domain number of encumbrance transactions is equal to the number of 
            // encumbrance transactions in the ENC data contract.
            IEnumerable<GlTransaction> encDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).ToList();
            var encRecordCount = encFyrDataContract.EncPoEntityAssociation.Count();
            Assert.AreEqual(encDomainTransactions.Count(), encRecordCount);

            // Make sure the GL account domain entity has the correct number of transactions.
            Assert.AreEqual(encDomainTransactions.Count() + reqDomainTransactions.Count(), activityDetailDomain.Transactions.Count());

            // Assert the total budget amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.BudgetAmount);

            // The number of budget transactions is 0.
            IEnumerable<GlTransaction> budgetDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Budget).ToList();
            Assert.AreEqual(budgetDomainTransactions.Count(), 0);

            // Assert the total actuals amount is $0.00.
            Assert.AreEqual(0m, activityDetailDomain.ActualAmount);

            // The number of actuals transactions is 0.
            IEnumerable<GlTransaction> actualsDomainTransactions = activityDetailDomain.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Actual).ToList();
            Assert.AreEqual(actualsDomainTransactions.Count(), 0);

            // Loop through the encumbrance data contracts and compare each property with the encumbrance domain transactions.
            foreach (var enc in encFyrDataContract.EncPoEntityAssociation)
            {
                GlTransaction encTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == enc.EncPoNoAssocMember);

                Assert.IsNotNull(encTransaction);
                Assert.AreEqual(enc.EncPoAmtAssocMember, encTransaction.Amount);
                Assert.AreEqual(enc.EncPoNoAssocMember, encTransaction.ReferenceNumber);
                Assert.AreEqual(enc.EncPoIdAssocMember, encTransaction.DocumentId);
                Assert.AreEqual(enc.EncPoVendorAssocMember, encTransaction.Description);
                Assert.AreEqual(enc.EncPoDateAssocMember, encTransaction.TransactionDate);
            }

            // Loop through the requisition data contracts and compare each property with the requisition domain transactions.
            foreach (var req in encFyrDataContract.EncReqEntityAssociation)
            {
                if (req.EncReqDateAssocMember.HasValue)
                {
                    GlTransaction reqTransaction = activityDetailDomain.Transactions.FirstOrDefault(x => x.ReferenceNumber == req.EncReqNoAssocMember);

                    Assert.IsNotNull(reqTransaction);
                    Assert.AreEqual(req.EncReqAmtAssocMember, reqTransaction.Amount);
                    Assert.AreEqual(req.EncReqNoAssocMember, reqTransaction.ReferenceNumber);
                    Assert.AreEqual(req.EncReqIdAssocMember, reqTransaction.DocumentId);
                    Assert.AreEqual(req.EncReqVendorAssocMember, reqTransaction.Description);
                    Assert.AreEqual(req.EncReqDateAssocMember, reqTransaction.TransactionDate);
                }
            }
        }

        #endregion

        #region Private methods

        private void PopulateGlaDataContracts()
        {
            /*
             * Budget.....: AB, BC, BU
             * Actual.....: PJ, JE
             * Encumbrance: EP
             */
            glaFyrDataContracts = new Collection<GlaFyr>()
            {
                // GLA.FYR data contracts for each GL account have to have a unique sequential number.
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*1",
                    GlaSource = "PJ",
                    GlaRefNo = "V0000001",
                    GlaDescription = "Voucher #1",
                    GlaDebit = 100m,
                    GlaCredit = 0m,
                    GlaTrDate = DateTime.Now
                },
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*2",
                    GlaSource = "PJ",
                    GlaRefNo = "V0000001",
                    GlaDescription = "Voucher #1",
                    GlaDebit = 0m,
                    GlaCredit = 100m,
                    GlaTrDate = DateTime.Now
                },
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*3",
                    GlaSource = "PJ",
                    GlaRefNo = "V0000002",
                    GlaDescription = "Voucher #2",
                    GlaDebit = 222m,
                    GlaCredit = 0m,
                    GlaTrDate = DateTime.Now
                },
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*4",
                    GlaSource = "JE",
                    GlaRefNo = "J000001",
                    GlaDescription = "Journal Entry #1",
                    GlaDebit = 40m,
                    GlaCredit = 0m,
                    GlaTrDate = DateTime.Now
                },
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*5",
                    GlaSource = "JE",
                    GlaRefNo = "J000002",
                    GlaDescription = "Journal Entry #2",
                    GlaDebit = 33.33m,
                    GlaCredit = 0m,
                    GlaTrDate = DateTime.Now
                },
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*6",
                    GlaSource = "PJ",
                    GlaRefNo = "V0000003",
                    GlaDescription = "Voucher #3",
                    GlaDebit = 0m,
                    GlaCredit = 11.22m,
                    GlaTrDate = DateTime.Now
                },
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*7",
                    GlaSource = "EP",
                    GlaRefNo = "P000001",
                    GlaDescription = "Consulting Associates for PO1",
                    GlaDebit = 11.11m,
                    GlaCredit = 0m,
                    GlaTrDate = DateTime.Now
                },
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*8",
                    GlaSource = "BU",
                    GlaRefNo = "B111111",
                    GlaDescription = "Budget for office supplies",
                    GlaDebit = 1000m,
                    GlaCredit = 0m,
                    GlaTrDate = DateTime.Now
                },
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*9",
                    GlaSource = "BU",
                    GlaRefNo = "B111112",
                    GlaDescription = "More budget for office supplies",
                    GlaDebit = 1010m,
                    GlaCredit = 0m,
                    GlaTrDate = DateTime.Now
                },
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*10",
                    GlaSource = "AB",
                    GlaRefNo = "B111113",
                    GlaDescription = "Adjust budget for office supplies",
                    GlaDebit = 1020m,
                    GlaCredit = 0m,
                    GlaTrDate = DateTime.Now
                },
                new GlaFyr()
                {
                    Recordkey = "10_00_01_01_33333_51001*11",
                    GlaSource = "BC",
                    GlaRefNo = "B111114",
                    GlaDescription = "Budget contingency for office supplies",
                    GlaDebit = 1030m,
                    GlaCredit = 0m,
                    GlaTrDate = DateTime.Now
                }
            };
        }

        private void PopulateEncDataContract()
        {
            encFyrDataContract = new EncFyr()
            {
                Recordkey = "10_00_01_01_33333_51001",
                EncPoId = new List<string>() { "11", "99", "44" },
                EncPoNo = new List<string>() { "P000001", "B0000009", "RV0000044" },
                EncPoVendor = new List<string>() { "Consulting Associates for PO1", "Consulting Associates for BPO9", "Consulting Associates for RV44" },
                // The PO is posted; the BPO and the RV are not.
                EncPoAmt = new List<Decimal?>() { 11.11m, 99.99m, 44.44m },
                EncPoDate = new List<DateTime?>() { DateTime.Now, DateTime.Now, DateTime.Now },
                EncPoSource = new List<string>() { "", "", "" },
                EncReqId = new List<string>() { "22", "66" },
                EncReqNo = new List<string>() { "0000022", "0000066" },
                EncReqVendor = new List<string>() { "Consulting Associates for R22", "Consulting Associates for R66" },
                // Requisition amounts have to match the amount for the fiscal year in the field GlRequisitionMemos in GL.ACCTS.
                EncReqAmt = new List<Decimal?>() { 22.22m, 66.66m },
                EncReqDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }
            };
            encFyrDataContract.buildAssociations();
        }

        private void PopulateGlAcctsDataContract()
        {
            glAcctsDataContract = new GlAccts()
            {
                Recordkey = "10_00_01_01_33333_51001",
                AvailFundsController = new List<string>() { futureFiscalYear, todaysFiscalYear, pastFiscalYear },
                GlFreezeFlags = new List<string>() { "O", "O", "O" },
                GlActualMemos = new List<decimal?>() { 0m, 54.32m, 0m },
                GlActualPosted = new List<decimal?>() { 0m, 284.11m, 0m },
                GlBudgetMemos = new List<decimal?>() { 0m, 9876.50m, 0m },
                GlBudgetPosted = new List<decimal?>() { 0m, 4060m, 0m },
                GlEncumbranceMemos = new List<decimal?>() { 0m, 144.43m, 0m },
                GlEncumbrancePosted = new List<decimal?>() { 0m, 11.11m, 0m },
                GlRequisitionMemos = new List<decimal?>() { 0m, 88.88m, 0m },
                FaActualMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaActualPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaBudgetPosted = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbranceMemo = new List<decimal?>() { 0m, 0m, 0m },
                FaEncumbrancePosted = new List<decimal?>() { 0m, 0m, 0m },
                FaRequisitionMemo = new List<decimal?>() { 0m, 0m, 0m },
                GlBudgetLinkage = new List<string>() { null, null, null },
                GlIncludeInPool = new List<string>() { null, null, null },
                GlPooledType = new List<string>() { null, null, null }
            };
            glAcctsDataContract.buildAssociations();
        }

        private void PopulateDescsDataContracts()
        {
            fdDescsDataContract = new FdDescs()
            {
                Recordkey = "10",
                FdDescription = "Fund 10"
            };
            fcDescsDataContract = new FcDescs()
            {
                Recordkey = "00",
                FcDescription = "Function 00"
            };
            loDescsDataContract = new LoDescs()
            {
                Recordkey = "01",
                LoDescription = "Location 01"
            };
            soDescsDataContract = new SoDescs()
            {
                Recordkey = "01",
                SoDescription = "Source 01"
            };
            unDescsDataContract = new UnDescs()
            {
                Recordkey = "33333",
                UnDescription = "Unit 33333"
            };
            obDescsDataContract = new ObDescs()
            {
                Recordkey = "51001",
                ObDescription = "Object 51001"
            };
        }

        private void BuildGlSourceCodes()
        {
            // Mock up GL.SOURCE.CODES
            glSourceCodes = new ApplValcodes();
            glSourceCodes.ValInternalCode = new List<string>()
            {
                "AA",
                "AB",
                "AE",
                "BC",
                "BU",
                "EP",
                "ER",
                "PJ",
                "JE",
                "BU"
            };
            glSourceCodes.ValActionCode2 = new List<string>()
            {
                "1",
                "2",
                "3",
                "2",
                "2",
                "3",
                "4",
                "1",
                "1",
                "2"
            };
            glSourceCodes.buildAssociations();
        }

        private void BuildJustificationNotes()
        {
            budgetDevDefaultsDataContract = new BudgetDevDefaults()
            {
                BudDevShowNotes = "Y"
            };

            genLdgrDataContract = new GenLdgr()
            {
                GenLdgrBudgetId = "FY2020"
            };

            var notes = new List<string>();
            notes.Add("Line 1");
            notes.Add("");
            notes.Add("Line 3");

            budWorkDataContract = new BudWork()
            {
                BwNotes = notes
            };
        }

        private void InitializeMockStatements()
        {
            // Mock the bulkread for GLA.FYR records.
            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(glaFyrDataContracts);
            });

            // Mock the bulkread for ENC.FYR records.
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<EncFyr>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(encFyrDataContract);
            });

            // Mock the ReadRecordAsync method to return the GL.ACCTS record.
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<GlAccts>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(glAcctsDataContract);
            });

            // Mock the ReadRecordAsync method to return an ApplValcodes data contract.
            dataReaderMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(glSourceCodes);
            });

            // Mock the ReadRecordAsync method to return the componenttype.DESCS record.
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<FdDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(fdDescsDataContract);
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<FcDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(fcDescsDataContract);
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<LoDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(loDescsDataContract);
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<SoDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(soDescsDataContract);
            });
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<ObDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(obDescsDataContract);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudgetDevDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(budgetDevDefaultsDataContract);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<GenLdgr>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(genLdgrDataContract);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudWork>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(budWorkDataContract);
            });

            glAccountDescriptionResponse = new GetGlAccountDescriptionResponse();
            glAccountDescriptionResponse.GlAccountIds = new List<string>() { this.param1_glAccount };
            glAccountDescriptionResponse.GlDescriptions = new List<string>() { "Test description for the GL account" };

            transManagerMock.Setup(tio => tio.ExecuteAsync<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(It.IsAny<GetGlAccountDescriptionRequest>())).Returns(() =>
            {
                return Task.FromResult(glAccountDescriptionResponse);
            });
        }

        private async Task<GlAccountActivityDetail> QueryGlActivityDetailAsync()
        {
            GeneralLedgerClassConfiguration glClassConfiguration = new GeneralLedgerClassConfiguration(glClassName, glExpenseValues, glRevenueValues, glAssetValues, glLiabilityValues, glFundBalValues);
            return await glaRepository.QueryGlActivityDetailAsync(this.param1_glAccount, this.param2_fiscalYear, this.param3_costCenterStructure, glClassConfiguration,
                majorComponentStartPosition);
            // Task.Run(() => new GlAccountActivityDetail("12"));
            
        }

        #endregion
    }
}