// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestBudgetAdjustmentRepository : IBudgetAdjustmentsRepository
    {
        //---------------------------------------------------------
        // The following data applies to Budget Adjustment records.
        //---------------------------------------------------------

        private List<BudgetAdjustment> budgetAdjustmentEntities = new List<BudgetAdjustment>();

        private string[,] budgetAdjustmentsArray = {
            //  0            1           2                  3       4                                       5            
            //  ID           Person ID   Initiator          Status  Tr Date                                 "Comments"
            {   "B000111",   "0000001",  "Ian Initiator",   "C",    DateTime.Now.ToString(),                "Comments for B000111; C ba with 2 GL accounts" },
            {   "B000222",   "0000001",  "Ian Initiator",   "C",    DateTime.Now.AddDays(2).ToString(),     "Comments for B000222; C ba with 4 GL accounts" },
            {   "B000333",   "0000001",  "Ian Initiator",   "N",    DateTime.Now.AddDays(3).ToString(),     "Comments for B000333; N ba with 2 GL accounts" },
            {   "B000444",   "0000001",  "Ian Initiator",   "U",    DateTime.Now.AddDays(4).ToString(),     "Comments for B000444; U ba with no GL accounts"},
            {   "B999998",   "0000002",  "Ian Initiator",   "N",    DateTime.Now.AddDays(5).ToString(),     "Comments for B999998; wrong login ID"          },
            {   "B999999",   "0000001",  "Ian Initiator",   "U",    DateTime.Now.AddDays(6).ToString(),     "Comments for B999999; incorrect source code"   }
        };

        private string[,] glDistributionArray = {
            //  0                           1                           2                                   3               4  
            //  GL Account                  Budget Adjustment ID        Description                         Debit           Credit   
            {   "11_10_00_01_20601_51000",  "B000111",                  "First GL account 51000 B111",      "11.00",        null       },
            {   "11_10_00_01_20601_51001",  "B000111",                  "Second GL account 51001",          null,           "11.00"    },

            {   "11_10_00_01_20601_51000",  "B000222",                  "First GL account 51000 B222",      "12.00",        null       },
            {   "11_10_00_01_20601_51001",  "B000222",                  "Second GL account 51001",          null,           "2.22"     },
            {   "11_10_00_01_20601_51002",  "B000222",                  "Third GL account 51002",           null,           "10.00"    },
            {   "11_10_00_01_20601_51003",  "B000222",                  "Fourth GL account 51003",          "4.22",         null       },

            {   "11_10_00_01_20601_51000",  "B000333",                  "First GL account 51000B 333",      "33.00",        null       },
            {   "11_10_00_01_20601_51001",  "B000333",                  "Second GL account 51001",          null,           "33.00"    },
        };

        private string[,] nextApproversArray = {
            //  0                            1
            //  Next Approver ID             Budget Adjustment ID
            {   "MEL",                      "B000222" },
            {   "AJK",                      "B000222" },

            {   "MEL",                      "B000333" },
            {   "AJK",                      "B000333" },
        };

        private string[,] approversArray = {
            //  0                           1                           2
            //  Approver ID                 Budget Adjustment ID        Approver Date   
            {   "GTT",                      "B000111",                  DateTime.Now.ToString() },
            {   "TGL",                      "B000111",                  DateTime.Now.ToString() },

            {   "GTT",                      "B000222",                  DateTime.Now.ToString() },
            {   "TGL",                      "B000222",                  DateTime.Now.ToString() },
            {   "AER",                      "B000222",                  DateTime.Now.ToString() },

            {   "GTT",                      "B000333",                  DateTime.Now.ToString() },
            {   "TGL",                      "B000333",                  DateTime.Now.ToString() },
        };
        public TestBudgetAdjustmentRepository()
        {
            CreateBudgetAdjustmentEntities();
        }

        public async Task<BudgetAdjustment> GetBudgetAdjustmentAsync(string id)
        {
            var budgetAdjustment = await Task.Run(() => budgetAdjustmentEntities.FirstOrDefault(x => x.Id == id));
            return budgetAdjustment;
        }

        public async Task<BudgetAdjustment> CreateAsync(BudgetAdjustment budgetAdjustment)
        {
            throw new NotImplementedException();
        }

        public async Task<BudgetAdjustment> UpdateAsync(string id, BudgetAdjustment budgetAdjustment)
        {
            return budgetAdjustment;
        }

        private void CreateBudgetAdjustmentEntities()
        {
            // Loop through the budget adjustments array and create budget adjustment domain entities.

            string id,
                   personId,
                   initiator,
                   comments,
                   reason;

            DateTime transactionDate;

            BudgetEntryStatus status;

            for (var i = 0; i < budgetAdjustmentsArray.GetLength(0); i++)
            {
                id = budgetAdjustmentsArray[i, 0];
                personId = budgetAdjustmentsArray[i, 1];
                initiator = budgetAdjustmentsArray[i, 2];

                switch (budgetAdjustmentsArray[i, 3])
                {
                    case "C":
                        status = BudgetEntryStatus.Complete;
                        break;
                    case "N":
                        status = BudgetEntryStatus.NotApproved;
                        break;
                    case "U":
                        status = BudgetEntryStatus.Unfinished;
                        break;
                    default:
                        throw new Exception("Invalid status specified in TestBudgetAdjustmentRepository.");
                }

                reason = " ";
                transactionDate = Convert.ToDateTime(budgetAdjustmentsArray[i, 4]);
                comments = budgetAdjustmentsArray[i, 5];

                for (var j = 0; j < glDistributionArray.GetLength(0); j++)
                {
                    var glAccountBudgetAdjustmentId = glDistributionArray[j, 1];
                    var glAccountDescription = glDistributionArray[j, 2];

                    if (glAccountBudgetAdjustmentId == id)
                    {
                        reason = glAccountDescription;
                        break;
                    }
                }

                BudgetAdjustment budgetAdjustment = new BudgetAdjustment(id, reason, transactionDate, personId);
                budgetAdjustment.Initiator = initiator;
                budgetAdjustment.Status = status;
                budgetAdjustment.Comments = comments;
                budgetAdjustmentEntities.Add(budgetAdjustment);
            }

            string glAccount,
                   glDistrBudgetAdjustmentId,
                   glDistrDescription;

            decimal glDistrDebit,
                    glDistrCredit;

            for (var i = 0; i < glDistributionArray.GetLength(0); i++)
            {
                glAccount = glDistributionArray[i, 0];
                glDistrBudgetAdjustmentId = glDistributionArray[i, 1];
                glDistrDescription = glDistributionArray[i, 2];

                glDistrDebit = Convert.ToDecimal(glDistributionArray[i, 3]);
                glDistrCredit = Convert.ToDecimal(glDistributionArray[i, 4]);

                var adjustmentLine = new AdjustmentLine(glAccount, glDistrCredit, glDistrDebit);

                foreach (var budgetAdjustment in budgetAdjustmentEntities)
                {
                    if (budgetAdjustment.Id == glDistrBudgetAdjustmentId)
                    {
                        budgetAdjustment.AddAdjustmentLine(adjustmentLine);
                    }
                }
            }

            string nextApprover,
                   nextApproverBudgetAdjustmentId;

            for (var i = 0; i < nextApproversArray.GetLength(0); i++)
            {
                nextApprover = nextApproversArray[i, 0];
                nextApproverBudgetAdjustmentId = nextApproversArray[i, 1];

                var nextApproverEntity = new NextApprover(nextApprover);

                foreach (var budgetAdjustment in budgetAdjustmentEntities)
                { 
                    if (budgetAdjustment.Id == nextApproverBudgetAdjustmentId)
                    {
                        budgetAdjustment.NextApprovers.Add(nextApproverEntity);
                    }
                }
            }

            string approver,
                   approverBudgetAdjustmentId;

            DateTime approverDate;

            for (var i = 0; i < approversArray.GetLength(0); i++)
            {
                approver = approversArray[i, 0];
                approverBudgetAdjustmentId = approversArray[i, 1];
                approverDate = Convert.ToDateTime(approversArray[i, 2]);

                var approverEntity = new Approver(approver);
                approverEntity.ApprovalDate = approverDate;
                foreach (var budgetAdjustment in budgetAdjustmentEntities)
                {
                    if (budgetAdjustment.Id == approverBudgetAdjustmentId)
                    {
                        budgetAdjustment.Approvers.Add(approverEntity);
                    }
                }
            }
        }


        #region Budget Entries Summary

        //-------------------------------------------------------------
        // The following data applies to Budget Adjustment summary data
        //-------------------------------------------------------------

        // Create a list of BUDGET.ENTRIES records.
        public List<BudgetEntries> budgetEntriesRecords = new List<BudgetEntries>()
        {
            new BudgetEntries
            {
                Recordkey = "B000111",
                BgteSource = "BU",
                BudgetEntriesAddopr = "0000001",
                BgteTrDate = DateTime.Now,
                BgteStatus = new List<string>() { "C" },
                BgteStatEntityAssociation = new List<BudgetEntriesBgteStat>()
                {
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "C",
                        BgteStatusDateAssocMember = DateTime.Now
                    }
                },
                BgteDataEntityAssociation = new List<BudgetEntriesBgteData>()
                {
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51000",
                        BgteDescriptionAssocMember = "First GL account 51000 B111",
                        BgteDebitAssocMember = 11m,
                        BgteCreditAssocMember = 0m
                    },
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51001",
                        BgteDescriptionAssocMember = "Second GL account 51001 B111",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 11m
                    }
                }
            },
            new BudgetEntries
            {
                Recordkey = "B000222",
                BgteSource = "BU",
                BudgetEntriesAddopr = "LOGINFORID0000001",
                BgteTrDate = DateTime.Now.AddDays(2),
                BgteStatus = new List<string>() { "N" },
                BgteStatEntityAssociation = new List<BudgetEntriesBgteStat>()
                {
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "N",
                        BgteStatusDateAssocMember = DateTime.Now
                    },
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "U",
                        BgteStatusDateAssocMember = DateTime.Now
                    }
                },
                BgteDataEntityAssociation = new List<BudgetEntriesBgteData>()
                {
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51000",
                        BgteDescriptionAssocMember = "First GL account 51000 B222",
                        BgteDebitAssocMember = 12m,
                        BgteCreditAssocMember = 0m
                    },
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51001",
                        BgteDescriptionAssocMember = "Second GL account 51001 B222",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 2.22m
                    },
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51002",
                        BgteDescriptionAssocMember = "First GL account 51002 B222",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 10m
                    },
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51003",
                        BgteDescriptionAssocMember = "Second GL account 51003 B222",
                        BgteDebitAssocMember = 4.22m,
                        BgteCreditAssocMember = 0m
                    }
                }
            },
            new BudgetEntries
            {
                Recordkey = "B000333",
                BgteSource = "BU",
                BudgetEntriesAddopr = "0000001",
                BgteTrDate = DateTime.Now.AddDays(3),
                BgteStatus = new List<string>() { "U" },
                BgteStatEntityAssociation = new List<BudgetEntriesBgteStat>()
                {
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "U",
                        BgteStatusDateAssocMember = DateTime.Now
                    }
                },
                BgteDataEntityAssociation = new List<BudgetEntriesBgteData>()
                {
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51000",
                        BgteDescriptionAssocMember = "First GL account 51000 B333",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 33m
                    }
                }
            },
            new BudgetEntries
            {
                Recordkey = "B000444",
                BgteSource = "BU",
                BudgetEntriesAddopr = "0000001",
                BgteTrDate = DateTime.Now.AddDays(4),
                BgteStatus = new List<string> { "U" },
                BgteStatEntityAssociation = new List<BudgetEntriesBgteStat>()
                {
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "U",
                        BgteStatusDateAssocMember = DateTime.Now
                    }
                },
            }
        };

        // Return the BUDGET.ENTRIES records.
        public async Task<Collection<BudgetEntries>> GetBudgetEntriesRecords()
        {
            var budgetEntriesDataContracts = new Collection<BudgetEntries>();
            foreach (var baContract in budgetEntriesRecords)
            {
                budgetEntriesDataContracts.Add(baContract);
            }
            return await Task.FromResult(budgetEntriesDataContracts);
        }

        // Create the budget adjustment summary domain entities for the budget entries data contracts.
        public List<BudgetAdjustmentSummary> summaries = new List<BudgetAdjustmentSummary>()
        {
            // For draft records.
            new BudgetAdjustmentSummary("Draft budget adjutment 1 reason", "0000001")
            {
                DraftBudgetAdjustmentId = "1",
                BudgetAdjustmentNumber = null,
                ToAmount = 0m,
                TransactionDate = DateTime.Now.AddDays(1)
            },
            new BudgetAdjustmentSummary("Draft budget adjutment 2 reason", "0000001")
            {
                DraftBudgetAdjustmentId = "2",
                BudgetAdjustmentNumber = null,
                ToAmount = 0m,
                TransactionDate = DateTime.Now.AddDays(2),
            },
            new BudgetAdjustmentSummary("Draft budget adjutment 3 reason", "0000001")
             {
                DraftBudgetAdjustmentId = "3",
                BudgetAdjustmentNumber = null,
                ToAmount = 33.33m,
                TransactionDate = DateTime.Now.AddDays(3)
            },
            new BudgetAdjustmentSummary("Draft budget adjutment 4 reason", "0000001")
             {
                DraftBudgetAdjustmentId = "4",
                BudgetAdjustmentNumber = null,
                ToAmount = 44m,
                TransactionDate = DateTime.Now.AddDays(4)
            },
            new BudgetAdjustmentSummary("Draft budget adjutment 5 reason", "0000001")
             {
                DraftBudgetAdjustmentId = "5",
                BudgetAdjustmentNumber = null,
                ToAmount = 111m,
                TransactionDate = DateTime.Now.AddDays(5)
            },

            // For budget entries records.
            new BudgetAdjustmentSummary("First GL account 51000 B111", "0000001")
            {
                DraftBudgetAdjustmentId = null,
                BudgetAdjustmentNumber = "B000111",
                ToAmount = 11m,
                TransactionDate = DateTime.Now
            },
            new BudgetAdjustmentSummary("First GL account 51000 B222", "0000001")
            {
                DraftBudgetAdjustmentId = null,
                BudgetAdjustmentNumber = "B000222",
                ToAmount = 12.22m,
                TransactionDate = DateTime.Now.AddDays(2),
            },
            new BudgetAdjustmentSummary("First GL account 51000 B333", "0000001")
             {
                DraftBudgetAdjustmentId = null,
                BudgetAdjustmentNumber = "B000333",
                ToAmount = 33m,
                TransactionDate = DateTime.Now.AddDays(3)
            },
            new BudgetAdjustmentSummary(" ", "0000001")
             {
                DraftBudgetAdjustmentId = null,
                BudgetAdjustmentNumber = "B000444",
                ToAmount = 0m,
                TransactionDate = DateTime.Now.AddDays(4)
            }
        };

        public async Task<IEnumerable<BudgetAdjustmentSummary>> GetBudgetAdjustmentsSummaryAsync(string personId)
        {
            return await Task.FromResult(summaries.Where(x => x.PersonId == personId).ToList());
        }

        #endregion

        #region Budget Entries Pending Approval Summary

        //------------------------------------------------------------------------------
        // The following data applies to Budget Adjustment pending approval summary data
        //------------------------------------------------------------------------------

        // Create a list of BUDGET.ENTRIES records pending approval.
        public List<BudgetEntries> budgetEntriesPendingApprovalRecords = new List<BudgetEntries>()
        {
            new BudgetEntries
            {
                Recordkey = "B009999",
                BgteSource = "BU",
                BudgetEntriesAddopr = "0010475",
                BgteTrDate = DateTime.Now,
                BgteNextApprovalIds = new List<string>() { "TGL", "GTT" },
                BgteStatus = new List<string>() { "N" },
                BgteStatEntityAssociation = new List<BudgetEntriesBgteStat>()
                {
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "N",
                        BgteStatusDateAssocMember = DateTime.Now
                    }
                },
                BgteDataEntityAssociation = new List<BudgetEntriesBgteData>()
                {
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51000",
                        BgteDescriptionAssocMember = "First GL account 51000 B9999",
                        BgteDebitAssocMember = 11m,
                        BgteCreditAssocMember = 0m
                    },
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51001",
                        BgteDescriptionAssocMember = "Second GL account 51001 B9999",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 11m
                    }
                }
            },
            new BudgetEntries
            {
                Recordkey = "B008888",
                BgteSource = "BU",
                BudgetEntriesAddopr = "0013272",
                BgteTrDate = DateTime.Now.AddDays(2),
                BgteNextApprovalIds = new List<string>() { "TGL" },
                BgteStatus = new List<string>() { "N" },
                BgteStatEntityAssociation = new List<BudgetEntriesBgteStat>()
                {
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "N",
                        BgteStatusDateAssocMember = DateTime.Now.AddDays(2)
                    },
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "U",
                        BgteStatusDateAssocMember = DateTime.Now.AddDays(2)
                    }
                },
                BgteDataEntityAssociation = new List<BudgetEntriesBgteData>()
                {
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51000",
                        BgteDescriptionAssocMember = "First GL account 51000 B8888",
                        BgteDebitAssocMember = 12m,
                        BgteCreditAssocMember = 0m
                    },
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51001",
                        BgteDescriptionAssocMember = "Second GL account 51001 B8888",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 2.22m
                    },
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51002",
                        BgteDescriptionAssocMember = "First GL account 51002 B8888",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 10m
                    },
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51003",
                        BgteDescriptionAssocMember = "Second GL account 51003 B8888",
                        BgteDebitAssocMember = 4.22m,
                        BgteCreditAssocMember = 0m
                    }
                }
            },
            new BudgetEntries
            {
                Recordkey = "B007777",
                BgteSource = "BU",
                BudgetEntriesAddopr = "0010475",
                BgteTrDate = DateTime.Now.AddDays(3),
                BgteNextApprovalIds = new List<string>() { "AJK", "TGL" },
                BgteStatus = new List<string>() { "N", "U" },
                BgteStatEntityAssociation = new List<BudgetEntriesBgteStat>()
                {
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "N",
                        BgteStatusDateAssocMember = DateTime.Now.AddDays(4)
                    },
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "U",
                        BgteStatusDateAssocMember = DateTime.Now.AddDays(3)
                    }
                },
                BgteDataEntityAssociation = new List<BudgetEntriesBgteData>()
                {
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51000",
                        BgteDescriptionAssocMember = "First GL account 51000 B7777",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 33m
                    }
                }
            },
            new BudgetEntries
            {
                Recordkey = "B006666",
                BgteSource = "BU",
                BudgetEntriesAddopr = "0013272",
                BgteTrDate = DateTime.Now.AddDays(4),
                BgteNextApprovalIds = new List<string>() { "TGL" },
                BgteStatus = new List<string> { "N" },
                BgteStatEntityAssociation = new List<BudgetEntriesBgteStat>()
                {
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "N",
                        BgteStatusDateAssocMember = DateTime.Now.AddDays(4)
                    }
                },
                BgteDataEntityAssociation = new List<BudgetEntriesBgteData>()
                {
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51000",
                        BgteDescriptionAssocMember = "First GL account 51000 B6666",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 44m
                    }
                }
            },
            new BudgetEntries
            {
                Recordkey = "B005555",
                BgteSource = "BU",
                BudgetEntriesAddopr = "0010475",
                BgteTrDate = DateTime.Now.AddDays(5),
                BgteNextApprovalIds = new List<string>() { "AJK", "TGL", "GTT" },
                BgteStatus = new List<string> { "N" },
                BgteStatEntityAssociation = new List<BudgetEntriesBgteStat>()
                {
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "N",
                        BgteStatusDateAssocMember = DateTime.Now.AddDays(5)
                    }
                },
                BgteDataEntityAssociation = new List<BudgetEntriesBgteData>()
                {
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51000",
                        BgteDescriptionAssocMember = "First GL account 51000 B5555",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 55m
                    }
                }
            }
        };

        // Return the BUDGET.ENTRIES records for the pending approval summaries.
        public async Task<Collection<BudgetEntries>> GetNotApprovedBudgetEntriesRecords()
        {
            var budgetEntriesDataContracts = new Collection<BudgetEntries>();
            foreach (var baContract in budgetEntriesPendingApprovalRecords)
            {
                budgetEntriesDataContracts.Add(baContract);
            }
            return await Task.FromResult(budgetEntriesDataContracts);
        }


        // Create the budget adjustment summary domain entities for the budget entries data contracts.

        public List<BudgetAdjustmentPendingApprovalSummary> pendingApprovalSummaries = new List<BudgetAdjustmentPendingApprovalSummary>()
        {
            new BudgetAdjustmentPendingApprovalSummary()
            {
                Reason = "First GL account 51000 B9999",
                InitiatorName = "Initiator Name for user 000001",
                BudgetAdjustmentNumber = "B009999",
                ToAmount = 11m,
                TransactionDate = DateTime.Now,
                Status = BudgetEntryStatus.NotApproved
            },
            new BudgetAdjustmentPendingApprovalSummary()
            {
                Reason = "First GL account 51000 B8888",
                InitiatorName = "Initiator Name for user 000001",
                BudgetAdjustmentNumber = "B008888",
                ToAmount = 12.22m,
                TransactionDate = DateTime.Now.AddDays(2),
                Status = BudgetEntryStatus.NotApproved
            },
            new BudgetAdjustmentPendingApprovalSummary()
             {
                Reason = "First GL account 51000 B7777",
                InitiatorName = "Initiator Name for user 000001",
                BudgetAdjustmentNumber = "B007777",
                ToAmount = 33m,
                TransactionDate = DateTime.Now.AddDays(3),
                Status = BudgetEntryStatus.NotApproved
            },
            new BudgetAdjustmentPendingApprovalSummary()
             {
                Reason = " ",
                InitiatorName = "Initiator Name for user 000001",
                BudgetAdjustmentNumber = "B006666",
                ToAmount = 0m,
                TransactionDate = DateTime.Now.AddDays(4),
                Status = BudgetEntryStatus.NotApproved
            },
            new BudgetAdjustmentPendingApprovalSummary()
            {
                Reason = "First GL account 51000 B5555",
                InitiatorName = "Initiator Name for user 000002",
                BudgetAdjustmentNumber = "B005555",
                ToAmount = 55m,
                TransactionDate = DateTime.Now.AddDays(5),
                Status = BudgetEntryStatus.NotApproved
            }
        };

        public async Task<IEnumerable<BudgetAdjustmentPendingApprovalSummary>> GetBudgetAdjustmentsPendingApprovalSummaryAsync(string personId)
        {
            return await Task.FromResult(pendingApprovalSummaries.ToList());
        }

        #endregion
    }
}
