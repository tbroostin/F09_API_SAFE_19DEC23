// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    /// <summary>
    /// This class represents a set of available cost center test data.
    /// </summary>
    public class TestCostCenterRepository : ICostCenterRepository
    {
        public List<CostCenter> CostCenters = new List<CostCenter>();
        private CostCenterBuilder CostCenterBuilderObject;
        private List<CostCenterSubtotal> costCenterSubtotals = new List<CostCenterSubtotal>();
        private CostCenterGlAccountBuilder glAccountBuilder;

        private GeneralLedgerComponentDescription componentDescription1 = new GeneralLedgerComponentDescription("10", GeneralLedgerComponentType.Fund);
        private GeneralLedgerComponentDescription componentDescription2 = new GeneralLedgerComponentDescription("00", GeneralLedgerComponentType.Source);
        private GeneralLedgerComponentDescription componentDescription3 = new GeneralLedgerComponentDescription("AJK55", GeneralLedgerComponentType.Unit);

        private GlTransactionBuilder glTransactionBuilder;

        public TestCostCenterRepository()
        {
            CostCenterBuilderObject = new CostCenterBuilder();
            glAccountBuilder = new CostCenterGlAccountBuilder();

            glTransactionBuilder = new GlTransactionBuilder();

            this.GlaFyrTransactionEntities = GlaFyrDataContracts.Select(x => PopulateGlaFyrTransaction(x)).ToList();
            this.EncFyrTransactionEntities = EncFyrDataContracts.SelectMany(x => PopulateEncFyrTransaction(x)).ToList();

            #region Build the cost center domain entities
            // Build a budget pool.
            var testGlNumberRepository = new TestGlAccountRepository();
            var umbrella1 = testGlNumberRepository.WithLocation("UM").WithFunction("U1").GetFilteredGlNumbers().FirstOrDefault();
            var poolee1 = testGlNumberRepository.WithLocation("P1").GetFilteredGlNumbers().FirstOrDefault();
            var poolee2 = testGlNumberRepository.WithLocation("P2").GetFilteredGlNumbers().FirstOrDefault();

            // Create the umbrella account and poolees.
            var umbrellaGlAccount1 = glAccountBuilder.WithGlAccountNumber(umbrella1).WithPoolType(GlBudgetPoolType.Umbrella).Build();
            var pooleeGlAccount1 = glAccountBuilder.WithGlAccountNumber(poolee1).WithPoolType(GlBudgetPoolType.Poolee).Build();
            var pooleeGlAccount2 = glAccountBuilder.WithGlAccountNumber(poolee2).WithPoolType(GlBudgetPoolType.Poolee).Build();

            // Create the pool and add poolees
            var budgetPool1 = new GlBudgetPool(umbrellaGlAccount1);
            budgetPool1.IsUmbrellaVisible = true;
            budgetPool1.AddPoolee(pooleeGlAccount1);
            budgetPool1.AddPoolee(pooleeGlAccount2);

            // Build a budget pool.
            var umbrella2 = testGlNumberRepository.WithLocation("UM").WithFunction("U2").GetFilteredGlNumbers().FirstOrDefault();
            var poolee3 = testGlNumberRepository.WithLocation("P3").GetFilteredGlNumbers().FirstOrDefault();
            var poolee4 = testGlNumberRepository.WithLocation("P4").GetFilteredGlNumbers().FirstOrDefault();

            // Create the umbrella account and poolees.
            var umbrellaGlAccount2 = glAccountBuilder.WithGlAccountNumber(umbrella2).WithPoolType(GlBudgetPoolType.Umbrella).Build();
            var pooleeGlAccount3 = glAccountBuilder.WithGlAccountNumber(poolee3).WithPoolType(GlBudgetPoolType.Poolee).Build();
            var pooleeGlAccount4 = glAccountBuilder.WithGlAccountNumber(poolee4).WithPoolType(GlBudgetPoolType.Poolee).Build();

            // Create the pool and add poolees
            var budgetPool2 = new GlBudgetPool(umbrellaGlAccount2);
            budgetPool2.AddPoolee(pooleeGlAccount3);
            budgetPool2.AddPoolee(pooleeGlAccount4);

            // Create a subtotal.
            var newSubtotal = new CostCenterSubtotal("01", budgetPool1, GlClass.Expense);
            newSubtotal.AddBudgetPool(budgetPool2);

            // Create a new cost center and add it to the list.
            var newCostCenter = new CostCenter("1000AJK55", newSubtotal, new List<GeneralLedgerComponentDescription>() { componentDescription1, componentDescription2, componentDescription3 });
            CostCenters.Add(newCostCenter);
            #endregion
        }

        public async Task<IEnumerable<CostCenter>> GetCostCentersAsync(GeneralLedgerUser generalLedgerUser, CostCenterStructure costCenterStructure, GeneralLedgerClassConfiguration glClassConfiguration, string costCenterId, string fiscalYear, CostCenterQueryCriteria criteria, string personId)
        {
            if (generalLedgerUser.Id == "9999999")
            {
                return await Task.Run(() => new List<CostCenter>());
            }

            return await Task.Run(() => CostCenters);
        }

        #region GLS.FYR data
        public Collection<GlsFyr> GlsFyrDataContracts = new Collection<GlsFyr>();

        #endregion

        #region GLA.FYR data
        /*
         * Actual.....: PJ
         * Encumbrance: EP
         * Requisition: ER
         */
        public Collection<GlaFyr> GlaFyrDataContracts = new Collection<GlaFyr>()
        {
            // "1000000051001",
            // "1000000151001",
            // "1000000251001",
            // "1000000351001",
            // "1000000451001",
            #region Actuals
            new GlaFyr()
            {
                Recordkey = "100000005100100*1",
                GlaSource = "PJ",
                GlaRefNo = "V000001",
                GlaDescription = "Voucher #1",
                GlaDebit = 1000m,
                GlaCredit = 150m,
                GlaTrDate = DateTime.Now
            },
            new GlaFyr()
            {
                Recordkey = "100000015100100*1",
                GlaSource = "PJ",
                GlaRefNo = "V000002",
                GlaDescription = "Voucher #2",
                GlaDebit = 1500m,
                GlaCredit = 350m,
                GlaTrDate = DateTime.Now
            },
            new GlaFyr()
            {
                Recordkey = "1000000251001*1",
                GlaSource = "PJ",
                GlaRefNo = "V000003",
                GlaDescription = "Voucher #3",
                GlaDebit = 100m,
                GlaCredit = 50m,
                GlaTrDate = DateTime.Now
            },
            #endregion

            #region Encumbrances
            new GlaFyr()
            {
                Recordkey = "100000005100100*2",
                GlaSource = "EP",
                GlaRefNo = "B000001",
                GlaDescription = "BPO #1",
                GlaDebit = 1000m,
                GlaCredit = 150m,
                GlaTrDate = DateTime.Now
            },
            new GlaFyr()
            {
                Recordkey = "1000000351001*2",
                GlaSource = "EP",
                GlaRefNo = "P000002",
                GlaDescription = "PO #1",
                GlaDebit = 1500m,
                GlaCredit = 350m,
                GlaTrDate = DateTime.Now
            },
            new GlaFyr()
            {
                Recordkey = "1000000351001*3",
                GlaSource = "EP",
                GlaRefNo = "P000003",
                GlaDescription = "PO #2",
                GlaDebit = 100m,
                GlaCredit = 50m,
                GlaTrDate = DateTime.Now
            },
            #endregion
        };

        public List<GlTransaction> GlaFyrTransactionEntities { get; set; }

        private GlTransaction PopulateGlaFyrTransaction(GlaFyr transaction)
        {
            GlTransactionType type = GlTransactionType.Actual;
            switch (transaction.GlaSource)
            {
                case "PJ":
                    type = GlTransactionType.Actual;
                    break;
                case "EP":
                    type = GlTransactionType.Encumbrance;
                    break;
                case "ER":
                    type = GlTransactionType.Requisition;
                    break;
                default:
                    break;
            }

            return glTransactionBuilder.WithId(transaction.Recordkey)
                .WithTransactionType(type)
                .WithSource(transaction.GlaSource)
                .WithGlAccountNumber(transaction.Recordkey.Split('*').FirstOrDefault())
                .WithAmount((transaction.GlaDebit ?? 0) - (transaction.GlaCredit ?? 0))
                .WithReferenceNumber(transaction.GlaRefNo)
                .WithTransactionDate(transaction.GlaTrDate.Value)
                .WithDescription(transaction.GlaDescription)
                .Build();
        }
        #endregion

        #region ENC.FYR data
        public Collection<EncFyr> EncFyrDataContracts = new Collection<EncFyr>()
        {
            #region Requisition data
            new EncFyr()
            {
                Recordkey = "1000000351001",
                EncReqNo = new List<string>() { "0002343", "0005744" },
                EncReqDate = new List<DateTime?>() { DateTime.Now, DateTime.Now },
                EncReqVendor = new List<string>() { "Longerbeam Consulting, Inc.", "Susty Corporation, Inc." },
                EncReqAmt = new List<decimal?>() { 1000m, 550m },
                EncReqId = new List<string>() { "367", "390" },
            },
            new EncFyr()
            {
                Recordkey = "100000005100100",
                EncReqNo = new List<string>() { "0002345", "0005714" },
                EncReqDate = new List<DateTime?>() { DateTime.Now, DateTime.Now },
                EncReqVendor = new List<string>() { "Longerbeam Consulting, Inc.", "Susty Corporation, Inc." },
                EncReqAmt = new List<decimal?>() { 1050m, 150m },
                EncReqId = new List<string>() { "787", "442" },
            },

            // Requisition with null record key
            //new EncFyr()
            //{
            //    Recordkey = null,
            //    EncReqNo = new List<string>() { "0002343", "0005744" },
            //    EncReqDate = new List<DateTime?>() { DateTime.Now, DateTime.Now },
            //    EncReqVendor = new List<string>() { "Longerbeam Consulting, Inc.", "Susty Corporation, Inc." },
            //    EncReqAmt = new List<decimal?>() { 1000m, 550m },
            //    EncReqId = new List<string>() { "367", "390" },
            //},

            //// Requisition with null record key
            //new EncFyr()
            //{
            //    Recordkey = "",
            //    EncReqNo = new List<string>() { "0002343", "0005744" },
            //    EncReqDate = new List<DateTime?>() { DateTime.Now, DateTime.Now },
            //    EncReqVendor = new List<string>() { "Longerbeam Consulting, Inc.", "Susty Corporation, Inc." },
            //    EncReqAmt = new List<decimal?>() { 1000m, 550m },
            //    EncReqId = new List<string>() { "367", "390" },
            //},

            // Eight requisitions, each containing null/empty data, where approptiate.
            //new EncFyr()
            //{
            //    Recordkey = "11-01",
            //    EncReqNo = new List<string>()
            //    {
            //        "",
            //        null,
            //        "0004323",
            //        "0006680",
            //        "0001242",
            //        "0008945",
            //        "0008238",
            //        "0008774",
            //    },
            //    EncReqDate = new List<DateTime?>()
            //    {
            //        DateTime.Now,
            //        DateTime.Now,
            //        null,
            //        DateTime.Now,
            //        DateTime.Now,
            //        DateTime.Now,
            //        DateTime.Now,
            //        DateTime.Now,
            //    },
            //    EncReqVendor = new List<string>()
            //    {
            //        "Longerbeam Consulting, Inc.",
            //        "Susty Corporation, Inc.",
            //        "Longerbeam Consulting, Inc.",
            //        "",
            //        null,
            //        "Susty Corporation, Inc.",
            //        "Longerbeam Consulting, Inc.",
            //        "Susty Corporation, Inc.",
            //    },
            //    EncReqAmt = new List<decimal?>()
            //    {
            //        1000m,
            //        550m,
            //        995m,
            //        456m,
            //        1154m,
            //        null,
            //        549m,
            //        441m,
            //    },
            //    EncReqId = new List<string>()
            //    {
            //        "367",
            //        "390",
            //        "285",
            //        "456",
            //        "369",
            //        "951",
            //        "",
            //        null,
            //    },
            //},
            #endregion
        };

        public List<GlTransaction> EncFyrTransactionEntities { get; set; }

        public IEnumerable<GlTransaction> PopulateEncFyrTransaction(EncFyr transaction)
        {
            transaction.buildAssociations();
            var glTransactions = new List<GlTransaction>();

            foreach (var requisition in transaction.EncReqEntityAssociation)
            {
                glTransactions.Add(
                    glTransactionBuilder.WithId(requisition.EncReqIdAssocMember)
                    .WithTransactionType(GlTransactionType.Requisition)
                    .WithSource("placeholder")
                    .WithGlAccountNumber(transaction.Recordkey)
                    .WithAmount(requisition.EncReqAmtAssocMember ?? 0)
                    .WithReferenceNumber(requisition.EncReqNoAssocMember)
                    .WithTransactionDate(requisition.EncReqDateAssocMember.Value)
                    .WithDescription(requisition.EncReqVendorAssocMember)
                    .Build()
                );
            }

            return glTransactions;
        }
        #endregion
    }
}
