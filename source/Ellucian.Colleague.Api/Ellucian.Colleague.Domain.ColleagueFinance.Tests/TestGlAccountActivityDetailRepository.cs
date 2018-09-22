// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    /// <summary>
    /// General Ledger Activity Details test data.
    /// </summary>
    public class TestGlAccountActivityDetailRepository : IGeneralLedgerActivityDetailRepository
    {
        private GlAccountActivityDetail glAccountActivityDetail = new GlAccountActivityDetail("10_00_01_01_33333_51001");
        private List<GlTransaction> glTransactionEntities = new List<GlTransaction>();
        private EncFyr encFyrDataContract;
        private GlAccts glAcctsDataContract;
        private string todaysFiscalYear = DateTime.Now.ToString("yyyy");
        private string futureFiscalYear = (Convert.ToDecimal(DateTime.Now.ToString("yyyy")) + 1).ToString();
        private string pastFiscalYear = (Convert.ToDecimal(DateTime.Now.ToString("yyyy")) - 1).ToString();


        /* Budget.....: AB, BC, BU
         * Actual.....: PJ, JE
         * Encumbrance: EP
        */

        private string[,] glaRecordsArray = {
            //           0                        1          2                                  3                         4              5
            { "10_00_01_01_33333_51001*1",      "PJ",   "V0000001",                             "Voucher #1",        "100.00",       "0.00" },
            { "10_00_01_01_33333_51001*2",      "PJ",   "V0000001",                             "Voucher #1",          "0.00",      "100.00"},
            { "10_00_01_01_33333_51001*3",      "PJ",   "V0000002",                             "Voucher #2",        "222.00",        "0.00"},
            { "10_00_01_01_33333_51001*4",      "JE",    "J000001",                       "Journal Entry #1",         "40.00",        "0.00"},
            { "10_00_01_01_33333_51001*5",      "JE",    "J000002",            "Journal Entry #2",                    "33.33",        "0.00"},
            { "10_00_01_01_33333_51001*6",      "PJ",   "V0000003",                             "Voucher #3",          "0.00",       "11.22"},
            { "10_00_01_01_33333_51001*7",      "EP",    "P000001",          "Consulting Associates for PO1",         "11.11",        "0.00"},
            { "10_00_01_01_33333_51001*8",      "BU",    "B111111",             "Budget for office supplies",       "1000.00",        "0.00"},
            { "10_00_01_01_33333_51001*9",      "BU",    "B111112",        "More budget for office supplies",       "1010.00",        "0.00"},
            { "10_00_01_01_33333_51001*10",     "AB",    "B111113",      "Adjust budget for office supplies",       "1020.00",        "0.00"},
            { "10_00_01_01_33333_51001*11",     "BC",    "B111114", "Budget contingency for office supplies",       "1030.00",        "0.00"}
        };

        public TestGlAccountActivityDetailRepository()
        {
            Populate();
        }

        public async Task<GlAccountActivityDetail> QueryGlActivityDetailAsync(string glAccount, string fiscalYear, CostCenterStructure costCenterStructure, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            var glAccountActivityDetailDomain = await Task.Run(() => glAccountActivityDetail);
            return glAccountActivityDetailDomain;
        }

        private void Populate()
        {
            // Populate the transactions from the GLA.FYR records.
            string glaId,
                   glaSource,
                   glNumber,
                   glaRefNo,
                   glaDescription;

            GlTransactionType type;

            Decimal? glaDebit,
                glaCredit;
            Decimal amount;

            DateTime glaTrDate;

            for (var i = 0; i < glaRecordsArray.GetLength(0); i++)
            {
                glaId = glaRecordsArray[i, 0];
                glaSource = glaRecordsArray[i, 1];

                switch (glaRecordsArray[i, 1])
                {
                    case "PJ":
                        type = GlTransactionType.Actual;
                        break;
                    case "JE":
                        type = GlTransactionType.Actual;
                        break;
                    case "AB":
                        type = GlTransactionType.Budget;
                        break;
                    case "BU":
                        type = GlTransactionType.Budget;
                        break;
                    case "BC":
                        type = GlTransactionType.Budget;
                        break;
                    case "EP":
                        type = GlTransactionType.Encumbrance;
                        break;
                    default:
                        throw new Exception("Invalid type specified in TestGlAccountActivityDetailRepository.");
                }

                glNumber = glaId.Split('*').FirstOrDefault();
                glaRefNo = glaRecordsArray[i, 2];
                glaDescription = glaRecordsArray[i, 3];
                glaDebit = Convert.ToDecimal(glaRecordsArray[i, 4]);
                glaCredit = Convert.ToDecimal(glaRecordsArray[i, 5]);
                amount = (glaDebit.HasValue ? glaDebit.Value : 0) - (glaCredit.HasValue ? glaCredit.Value : 0);
                glaTrDate = DateTime.Now;

                var glTransactionEntity = new GlTransaction(glaId, type, glaSource, glNumber, amount, glaRefNo, glaTrDate, glaDescription);

                // Add the transaction to the list.
                glTransactionEntities.Add(glTransactionEntity);
            }

            // Populate the transactions from the ENC.FYR record.

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

            // Use a counter to create the sequential part of the transaction id to mimic the ids in GLA.
            int encCounter = 0;

            // First get all the outstanding encumbrances.
            foreach (var transaction in encFyrDataContract.EncPoEntityAssociation)
            {
                // While GLA records have a sequential ID in the key, the ENC record does not.
                // Assign an id similar to the GLA records using the counter.
                encCounter += 1;
                string transactionId = "10_00_01_01_33333_51001*ENC*" + encCounter;

                var glTransactionEntity = new GlTransaction(transactionId, GlTransactionType.Encumbrance, "EP",
                    encFyrDataContract.Recordkey, transaction.EncPoAmtAssocMember ?? 0, transaction.EncPoNoAssocMember,
                    transaction.EncPoDateAssocMember.Value, transaction.EncPoVendorAssocMember);

                // If the encumbrance entry has an ID, assign it as the document ID.
                if (!string.IsNullOrEmpty(transaction.EncPoIdAssocMember))
                {
                    glTransactionEntity.DocumentId = transaction.EncPoIdAssocMember;
                }

                // Add the transaction to the list.
                glTransactionEntities.Add(glTransactionEntity);
            }

            // Then get all the outstanding requisitions.
            foreach (var transaction in encFyrDataContract.EncReqEntityAssociation)
            {
                // While GLA records have a sequential ID in the key, the ENC record does not.
                // Assign an id similar to the GLA records using the counter.
                encCounter += 1;
                string transactionId = "10_00_01_01_33333_51001*ENC*" + encCounter;

                var glTransactionEntity = new GlTransaction(transactionId, GlTransactionType.Requisition, "ER",
                    encFyrDataContract.Recordkey, transaction.EncReqAmtAssocMember ?? 0, transaction.EncReqNoAssocMember,
                    transaction.EncReqDateAssocMember.Value, transaction.EncReqVendorAssocMember);

                // If the requisition entry has an ID, assign it as the document ID.
                if (!string.IsNullOrEmpty(transaction.EncReqIdAssocMember))
                {
                    glTransactionEntity.DocumentId = transaction.EncReqIdAssocMember;
                }

                // Add the transaction to the list.
                glTransactionEntities.Add(glTransactionEntity);
            }

            // Populate the amounts pending posting from GL.ACCTS.

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

            var glAccountFundsAssociation = glAcctsDataContract.MemosEntityAssociation.FirstOrDefault(x => x != null && x.AvailFundsControllerAssocMember == todaysFiscalYear);
            if (glAccountFundsAssociation != null)
            {
                // Get the budget pending posting amount.
                glAccountActivityDetail.MemoBudgetAmount = glAccountFundsAssociation.GlBudgetMemosAssocMember.HasValue ? glAccountFundsAssociation.GlBudgetMemosAssocMember.Value : 0m;
                // Get the actuals pending posting amount.
                glAccountActivityDetail.MemoActualsAmount = glAccountFundsAssociation.GlActualMemosAssocMember.HasValue ? glAccountFundsAssociation.GlActualMemosAssocMember.Value : 0m;
            }

            // Add the transactions to the GL account domain entity.

            foreach (var transaction in glTransactionEntities)
            {
                // The list of transactions includes all four types: budget, actuals, encumbrances and requisitions.
                // Add the budget, actuals and encumbrances separately to obtain totals; requisitions are included in encumbrances.
                if (transaction.GlTransactionType == GlTransactionType.Budget)
                {
                    glAccountActivityDetail.BudgetAmount += transaction.Amount;
                }
                else if (transaction.GlTransactionType == GlTransactionType.Actual)
                {
                    glAccountActivityDetail.ActualAmount += transaction.Amount;
                }
                else
                {
                    glAccountActivityDetail.EncumbranceAmount += transaction.Amount;
                }

                glAccountActivityDetail.AddGlTransaction(transaction);
            }
        }
    }
}
