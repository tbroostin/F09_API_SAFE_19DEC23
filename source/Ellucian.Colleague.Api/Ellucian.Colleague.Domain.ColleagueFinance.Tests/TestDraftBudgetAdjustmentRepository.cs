// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestDraftBudgetAdjustmentRepository : IDraftBudgetAdjustmentsRepository
    {
        public class AdjustmentLineRecord
        {
            public string glAccount;
            public decimal fromAmount;
            public decimal toAmount;
        }
        public class DraftBudgetAdjustmentRecord
        {
            public string reason;
            public string id;
            public string personId;
            public string initiator;
            public string comments;
            public DateTime transactionDate;
            public List<string> nextApprovers;
            public List<AdjustmentLineRecord> adjustmentLines;
        }

        public List<DraftBudgetAdjustmentRecord> draftBaRecords = new List<DraftBudgetAdjustmentRecord>()
        {
            new DraftBudgetAdjustmentRecord()
            {
                reason = "Draft budget adjutment 1 reason",
                id = "1",
                personId = "0000001",
                initiator = "",
                comments = "",
                transactionDate = new DateTime(),
                nextApprovers = null,
                adjustmentLines = null
            },
            new DraftBudgetAdjustmentRecord()
            {
                reason = "Draft budget adjutment 2 reason",
                id = "2",
                personId = "0000001",
                initiator = "Initiator 2",
                comments = "Comments for draft budget adjustment 2.",
                transactionDate = new DateTime(),
                nextApprovers = null,
                adjustmentLines = new List<AdjustmentLineRecord>()
                {
                    new AdjustmentLineRecord()
                    {
                        glAccount = "",
                        fromAmount = 22m,
                        toAmount = 0m
                    }
                }
            },
            new DraftBudgetAdjustmentRecord()
            {
                reason = "Draft budget adjutment 3 reason",
                id = "3",
                personId = "0000001",
                initiator = "",
                comments = "",
                transactionDate = DateTime.Now,
                nextApprovers = null,
                adjustmentLines = null
            },
            new DraftBudgetAdjustmentRecord()
            {
                reason = "Draft budget adjutment 4 reason",
                id = "4",
                personId = "0000001",
                initiator = "Initiator 4",
                comments = "",
                transactionDate = DateTime.Now.AddDays(4),
                nextApprovers = null,
                adjustmentLines = new List<AdjustmentLineRecord>()
                {
                    new AdjustmentLineRecord()
                    {
                        glAccount = "",
                        fromAmount = 0m,
                        toAmount = 44m
                    },
                     new AdjustmentLineRecord()
                    {
                        glAccount = "11_00_01_00_11111_54444",
                        fromAmount = 0m,
                        toAmount = 0m
                    },
                }
            },
            new DraftBudgetAdjustmentRecord()
            {
                reason = "Draft budget adjutment 5 reason",
                id = "5",
                personId = "0000001",
                initiator = "Initator 5",
                comments = "Comments for draft budget adjustment 5.",
                transactionDate = DateTime.Now.AddDays(5),
                nextApprovers = null,
                adjustmentLines = new List<AdjustmentLineRecord>()
                {
                    new AdjustmentLineRecord()
                    {
                        glAccount = "",
                        fromAmount = 55m,
                        toAmount = 0m
                    },
                     new AdjustmentLineRecord()
                    {
                        glAccount = "11_00_01_00_11111_55555",
                        fromAmount = 56m,
                        toAmount = 0m
                    },
                    new AdjustmentLineRecord()
                    {
                        glAccount = "11_00_01_00_11111_55557",
                        fromAmount = 0m,
                        toAmount = 0m
                    },
                }
            }
        };

        public Task<DraftBudgetAdjustment> SaveAsync(DraftBudgetAdjustment budgetAdjustment)
        {
            throw new NotImplementedException();
        }

        public Task<DraftBudgetAdjustment> GetAsync(string recordId)
        {
            var draftBudgetAdjustmentRecord = draftBaRecords.FirstOrDefault(i => i.id == recordId);
            var draftBudgetAdjutmentEntity = new DraftBudgetAdjustment(draftBudgetAdjustmentRecord.reason);
            draftBudgetAdjutmentEntity.Id = draftBudgetAdjustmentRecord.id;
            draftBudgetAdjutmentEntity.Comments = draftBudgetAdjustmentRecord.comments;
            draftBudgetAdjutmentEntity.Initiator = draftBudgetAdjustmentRecord.initiator;
            draftBudgetAdjutmentEntity.PersonId = draftBudgetAdjustmentRecord.personId;
            draftBudgetAdjutmentEntity.TransactionDate = draftBudgetAdjustmentRecord.transactionDate;
            if (draftBudgetAdjustmentRecord.nextApprovers != null && draftBudgetAdjustmentRecord.nextApprovers.Any())
            {
                foreach (var nextApprover in draftBudgetAdjustmentRecord.nextApprovers)
                {
                    if (nextApprover != null)
                    {
                        var nextApproverEntity = new NextApprover(nextApprover);
                        nextApproverEntity.SetNextApproverName(nextApprover);
                        draftBudgetAdjutmentEntity.NextApprovers.Add(nextApproverEntity);
                    }
                }
            }
            if (draftBudgetAdjustmentRecord.adjustmentLines != null && draftBudgetAdjustmentRecord.adjustmentLines.Any())
            {
                foreach (var line in draftBudgetAdjustmentRecord.adjustmentLines)
                {
                    if (line != null)
                    {
                        var adjustmentLine = new DraftAdjustmentLine();
                        adjustmentLine.GlNumber = string.IsNullOrEmpty(line.glAccount) ? "" : line.glAccount;
                        adjustmentLine.FromAmount = line.fromAmount;
                        adjustmentLine.ToAmount = line.toAmount;
                        draftBudgetAdjutmentEntity.AdjustmentLines.Add(adjustmentLine);
                    }
                }
            }
            return Task.FromResult(draftBudgetAdjutmentEntity);
        }

        public Task DeleteAsync(string recordId)
        {
            throw new NotImplementedException();
        }


        //----------------------------------------------------------------------------------------------
        // The following are DRAFT.BUDGET.ENTRIES data contracts used for Budget Adjustment summary data
        //----------------------------------------------------------------------------------------------

        // Create a list of DRAF.BUDGET.ENTRIES data contracts.
        public List<DraftBudgetEntries> draftBudgetEntriesRecords = new List<DraftBudgetEntries>()
        {
            new DraftBudgetEntries
            {
                Recordkey = "1",
                DbgteReason = "Draft budget adjutment 1 reason",
                DbgteTrDate = DateTime.Now.AddDays(1),
                DraftBudgetEntriesAddopr = "0000001",
                DbgteDataEntityAssociation = new List<DraftBudgetEntriesDbgteData>()
            },
            new DraftBudgetEntries
            {
                Recordkey = "2",
                DbgteReason = "Draft budget adjutment 2 reason",
                DbgteTrDate = DateTime.Now.AddDays(2),
                DbgteAuthor = "Initiator 2",
                DraftBudgetEntriesAddopr = "0000001",
                DbgteDataEntityAssociation = new List<DraftBudgetEntriesDbgteData>()
                {
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "",
                        DbgteCreditAssocMember = 22m,
                        DbgteDebitAssocMember = 0m
                    }
                }
            },
            new DraftBudgetEntries
            {
                Recordkey = "3",
                DbgteReason = "Draft budget adjutment 3 reason",
                DbgteTrDate = DateTime.Now.AddDays(3),
                DbgteAuthor = "",
                DraftBudgetEntriesAddopr = "0000001",
                DbgteDataEntityAssociation = new List<DraftBudgetEntriesDbgteData>()
                {
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "",
                        DbgteCreditAssocMember = 0m,
                        DbgteDebitAssocMember = 33.33m
                    }
                }
            },
            new DraftBudgetEntries
            {
                Recordkey = "4",
                DbgteReason = "Draft budget adjutment 4 reason",
                DbgteTrDate = DateTime.Now.AddDays(4),
                DbgteAuthor = "Initiator 4",
                DraftBudgetEntriesAddopr = "0000001",
                DbgteDataEntityAssociation = new List<DraftBudgetEntriesDbgteData>()
                {
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "",
                        DbgteCreditAssocMember = 0m,
                        DbgteDebitAssocMember = 44m
                    },
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "11_00_01_00_11111_54444",
                        DbgteCreditAssocMember = 0m,
                        DbgteDebitAssocMember = 0m
                    }
                }
            },
            new DraftBudgetEntries
            {
                Recordkey = "5",
                DbgteReason = "Draft budget adjutment 5 reason",
                DbgteTrDate = DateTime.Now.AddDays(5),
                DbgteAuthor = "Initiator 5",
                DraftBudgetEntriesAddopr = "0000001",
                DbgteDataEntityAssociation = new List<DraftBudgetEntriesDbgteData>()
                {
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "",
                        DbgteCreditAssocMember = 55m,
                        DbgteDebitAssocMember = 0m
                    },
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "11_00_01_00_11111_55555",
                        DbgteCreditAssocMember = 56m,
                        DbgteDebitAssocMember = 0m
                    },
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "11_00_01_00_11111_55557",
                        DbgteCreditAssocMember = 0m,
                        DbgteDebitAssocMember = 0m
                    }
                }
            }
        };

        // Return the DRAFT.BUDGET.ENTRIES records.
        public async Task<Collection<DraftBudgetEntries>> GetDraftBudgetEntriesRecords()
        {
            var draftBudgetEntriesDataContracts = new Collection<DraftBudgetEntries>();
            foreach (var draftContract in draftBudgetEntriesRecords)
            {
                draftBudgetEntriesDataContracts.Add(draftContract);
            }
            return await Task.FromResult(draftBudgetEntriesDataContracts);
        }
    }
}
