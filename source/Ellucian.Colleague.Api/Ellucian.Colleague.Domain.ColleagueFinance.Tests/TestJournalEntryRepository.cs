// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    /// <summary>
    /// This class represents a set of journal entries
    /// </summary>
    public class TestJournalEntryRepository : IJournalEntryRepository
    {
        private List<JournalEntry> journalEntries = new List<JournalEntry>();

        #region Define all data for journal entries
        private string[,] journalEntriesArray = {
            //   0              1       2       3               4                      5            6                7           8  
            //   Number       Status   Type     Entered Date    Entered By Name        Author      Posting Date      Reversal    Comments                          
            {   "J000001",     "C",    "JE",    "04/01/2015",   "Teresa Longerbeam",   "Author1",  "04/15/2015",     "true",     "Comments 1" },     
            {   "J000002",     "N",    "JE",    "04/02/2015",   "Gary Thorne",         "",         "04/16/2015",     "false",    "Comments 2" },
            {   "J000003",     "U",    "JE",    "04/03/2015",   "Andy Kleehammer",     "",         "04/17/2015",     "false",    "Comments 3" },

            {   "J000005",     "C",    "JE",    "04/30/2015",   "Teresa Longerbeam",   "",         "04/30/2015",     "false",    ""           },
            {   "J000006",     "C",    "AA",    "04/01/2015",   "Teresa Longerbeam",   "Author1",  "04/15/2015",     "true",     "Comments 1" },
            {   "J000007",     "U",    "AA",    "04/01/2015",   "Teresa Longerbeam",   "Author1",  "04/15/2015",     "true",     "Comments 1" },
            {   "J000008",     "C",    "AA",    "04/01/2015",   "Teresa Longerbeam",   "Author1",  "04/15/2015",     "true",     "Comments 1" },

            {   "J000111",     "C",    "JE",    "04/01/2015",   "Teresa Longerbeam",   "Author1",  "04/15/2015",     "true",     "Comments 1" }, 
            {   "J000112",     "C",    "JE",    "04/01/2015",   "Teresa Longerbeam",   "Author1",  "04/15/2015",     "true",     "Comments 1" },
            {   "J000113",     "C",    "JE",    "04/01/2015",   "Teresa Longerbeam",   "Author1",  "04/15/2015",     "true",     "Comments 1" }  
        };

        private string[,] journalEntryItemsArray = {
        //   0             1                    2                             3                      4         5           6             7           8             9
        //   Item ID       Description          GL Account                    Gl Acct desc           Prj ID    Prj No      Prj Itm ID    Prj Itm     Debit         Credit
        {    "J000001",    "First Item J1",     "11_00_01_00_33333_54005",    "GL account 54005",    "101",    "TGL-1",    "10101",      "MA",       "1,234.56",         null},
        {    "J000001",    "Second Item J1",    "11_00_01_00_33333_54011",    "GL account 54011",    "101",    "TGL-1",    "10102",      "CN",             null,   "2,678.91"},
        {    "J000001",    "Third Item J1",     "11_00_01_00_33333_54030",    "GL account 54030",    "101",    "TGL-1",    "10103",      "OF",       "2,345.56",         null},
        {    "J000001",    "Fourth Item J1",    "11_00_01_00_33333_54400",    "GL account 54400",    "102",    "TGL-2",    "10201",      "MA",         "741.87",         null},
        {    "J000001",    "Fifth Item J1",     "11_00_01_00_33333_51000",    "GL account 51000",    "103",    "TGL-3",    "10301",      "MA",             null,   "1,643.08"},

        {    "J000002",    "First Item J2",     "11_00_01_00_33333_51000",    "GL account 51000",    "103",    "TGL-3",    "10301",      "MA",             null,   "1,643.08"},

        {    "J000005",    "First Item J5",     "11_00_01_00_33333_51000",    "GL account 51000",    "103",    "TGL-3",    "10301",      "MA",         "555.55",     "555.55"},
        {    "J000007",    "null",              "11_00_01_00_33333_51000",    "GL account 51000",    "103",    "TGL-3",    "10301",      "MA",         "555.55",     "555.55"},    // Null description
        {    "J000008",    "Journal Ent",       "null"                   ,    "GL account 51000",    "103",    "TGL-3",    "10301",      "MA",         "555.55",     "555.55"},    // Null GL account

        {    "J000111",    "First Item J111",   "11_00_01_00_33333_54005",    "GL account 54005",    "101",    "TGL-1",    "10101",      "MA",       "1,234.56",         null},
        {    "J000111",    "Second Item J111",  "11_00_01_00_33333_54011",    "GL account 54011",    "101",    "TGL-1",    "10102",      "CN",             null,   "2,678.91"},
        {    "J000111",    "Third Item J111",   "11_00_01_00_33333_54030",    "GL account 54030",    "101",    "TGL-1",    "10103",      "OF",       "2,345.56",         null},
        {    "J000111",    "Fourth Item J111",  "11_00_01_00_33333_54400",    "GL account 54400",    "102",    "TGL-2",    "10201",      "MA",         "741.87",         null},
        {    "J000111",    "Fifth Item J111",   "11_00_01_00_33333_51000",    "GL account 51000",    "103",    "TGL-3",    "10301",      "MA",             null,   "1,643.08"},

        {    "J000112",    "First Item J112",   "11_00_01_00_33333_54005",    "GL account 54005",    "101",    "TGL-1",    "10101",      "MA",       "1,234.56",         null},
        {    "J000112",    "Second Item J112",  "11_00_01_00_33333_54011",    "GL account 54011",    "101",    "TGL-1",    "10102",      "CN",             null,   "2,678.91"},
        {    "J000112",    "Third Item J112",   "11_00_01_00_33333_54030",    "GL account 54030",    "101",    "TGL-1",    "10103",      "OF",       "2,345.56",         null},
        {    "J000112",    "Fourth Item J112",  "11_00_01_00_33333_54400",    "GL account 54400",    "102",    "TGL-2",    "10201",      "MA",         "741.87",         null},
        {    "J000112",    "Fifth Item J112",   "11_00_01_00_33333_51000",    "GL account 51000",    "103",    "TGL-3",    "10301",      "MA",             null,   "1,643.08"},

        {    "J000113",    "First Item J113",   "11_00_01_00_33333_54005",    "GL account 54005",    "101",    "TGL-1",    "10101",      "MA",       "1,234.56",         null},
        {    "J000113",    "Second Item J113",  "11_00_01_00_33333_54011",    "GL account 54011",    "101",    "TGL-1",    "10102",      "CN",             null,   "2,678.91"},
        {    "J000113",    "Third Item J113",   "11_00_01_00_33333_54030",    "GL account 54030",    "101",    "TGL-1",    "10103",      "OF",       "2,345.56",         null},
        {    "J000113",    "Fourth Item J113",  "11_00_01_00_33333_54400",    "GL account 54400",    "102",    "TGL-2",    "10201",      "MA",         "741.87",         null},
        {    "J000113",    "Fifth Item J113",   "11_00_01_00_33333_51000",    "GL account 51000",    "103",    "TGL-3",    "10301",      "MA",             null,   "1,643.08"}
                                                };

        private string[,] approversArray = {
        //  0           1                       2               3
        //  ID          Name                    Date            JE number
        {   "0000001",  "Teresa Longerbeam",    "4/1/2015",     "J000001" },
        {   "0000002",  "Gary Thorne",          "4/3/2015",     "J000001" },
        {   "0000003",  "Andy Kleehammer",      "null",         "J000001" },
        {   "0000004",  "Aimee Rodgers",        "null",         "J000001" }      
                                                };
        #endregion

        public TestJournalEntryRepository()
        {
            Populate();
        }

        public async Task<JournalEntry> GetJournalEntryAsync(string journalEntryId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts)
        {
            var journalEntry = await Task.Run(() => journalEntries.FirstOrDefault(x => x.Id == journalEntryId));
            return journalEntry;
        }

        private void Populate()
        {
            #region Populate Journal Entries

            // Loop through the journal entries array and create journal entries domain entities
            string journalEntryId,
                    enteredByName,
                    author,
                    comments,
            automaticReversal;

            JournalEntryType type;
            JournalEntryStatus status;

            DateTime enteredDate,
                     postingDate;

            for (var i = 0; i < journalEntriesArray.GetLength(0); i++)
            {
                journalEntryId = journalEntriesArray[i, 0];

                switch (journalEntriesArray[i, 1])
                {
                    case "C":
                        status = JournalEntryStatus.Complete;
                        break;
                    case "N":
                        status = JournalEntryStatus.NotApproved;
                        break;
                    case "U":
                        status = JournalEntryStatus.Unfinished;
                        break;
                    default:
                        throw new Exception("Invalid status specified in TestJournalEntryRepository.");
                }

                switch (journalEntriesArray[i, 2])
                {
                    case "JE":
                        type = JournalEntryType.General;
                        break;
                    case "AA":
                        type = JournalEntryType.OpeningBalance;
                        break;
                    default:
                        throw new Exception("Invalid type specified in TestJournalEntryRepository.");
                }

                enteredDate = Convert.ToDateTime(journalEntriesArray[i, 3]);
                enteredByName = journalEntriesArray[i, 4];
                author = journalEntriesArray[i, 5];
                postingDate = Convert.ToDateTime(journalEntriesArray[i, 6]);
                automaticReversal = journalEntriesArray[i, 7];
                comments = journalEntriesArray[i, 8];

                var journalEntry = new JournalEntry(journalEntryId, postingDate, status, type, enteredDate, enteredByName);

                journalEntry.Author = author;
                journalEntry.AutomaticReversal = Convert.ToBoolean(automaticReversal);
                journalEntry.Comments = comments;

                journalEntries.Add(journalEntry);
            }
            #endregion

            #region Populate approvers

            string approverId,
                   approvalName,
                   approvalJournalEntryNumber;

            DateTime? approvalDate;

            for (var i = 0; i < approversArray.GetLength(0); i++)
            {
                approverId = approversArray[i, 0];
                approvalName = approversArray[i, 1];

                if (approversArray[i, 2] == "null")
                {
                    approvalDate = null;
                }
                else
                {
                    approvalDate = Convert.ToDateTime(approversArray[i, 2]);
                }
                approvalJournalEntryNumber = approversArray[i, 3];
                var approverDomainEntity = new Approver(approverId);
                approverDomainEntity.SetApprovalName(approvalName);
                approverDomainEntity.ApprovalDate = approvalDate;

                foreach (var journalEntry in journalEntries)
                {
                    if (journalEntry.Id == approvalJournalEntryNumber)
                    {
                        journalEntry.AddApprover(approverDomainEntity);
                    }
                }
            }
            #endregion

            #region Populate journal entry items

            string journalEntryNumber,
                   description,
                   glAccount,
                   glAccountDescription,
                   projectId,
                   projectNumber,
                   projectLineItemId,
                   projectItemCode;

            decimal? debit,
                     credit;

            for (var i = 0; i < journalEntryItemsArray.GetLength(0); i++)
            {
                journalEntryNumber = journalEntryItemsArray[i, 0];
                description = journalEntryItemsArray[i, 1];
                glAccount = journalEntryItemsArray[i, 2];
                glAccountDescription = journalEntryItemsArray[i, 3];
                projectId = journalEntryItemsArray[i, 4];
                projectNumber = journalEntryItemsArray[i, 5];
                projectLineItemId = journalEntryItemsArray[i, 6];
                projectItemCode = journalEntryItemsArray[i, 7];
                if (!string.IsNullOrEmpty(journalEntryItemsArray[i, 8]))
                {
                    debit = Convert.ToDecimal(journalEntryItemsArray[i, 8]);
                }
                else
                {
                    debit = null;
                }

                if (!string.IsNullOrEmpty(journalEntryItemsArray[i, 9]))
                {
                    credit = Convert.ToDecimal(journalEntryItemsArray[i, 9]);
                }
                else
                {
                    credit = null;
                }

                var journalEntryItem = new JournalEntryItem(description, glAccount);

                journalEntryItem.GlAccountDescription = glAccountDescription;
                journalEntryItem.Credit = credit;
                journalEntryItem.Debit = debit;
                journalEntryItem.ProjectId = projectId;
                journalEntryItem.ProjectNumber = projectNumber;
                journalEntryItem.ProjectLineItemId = projectLineItemId;
                journalEntryItem.ProjectLineItemCode = projectItemCode;

                foreach (var journalEntry in journalEntries)
                {
                    if (journalEntry.Id == journalEntryNumber)
                    {
                        journalEntry.AddItem(journalEntryItem);

                        // Update the total credits and total debits
                        journalEntry.TotalCredits += credit ?? 0;
                        journalEntry.TotalDebits += debit ?? 0;
                    }
                }
            }

            #endregion
        }
    }
}
