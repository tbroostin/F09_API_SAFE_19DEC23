// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// Implement the IJournalEntryRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class JournalEntryRepository : BaseColleagueRepository, IJournalEntryRepository
    {
        /// <summary>
        /// Constructor to instantiate a journal entry repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public JournalEntryRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get the journal entry requested
        /// </summary>
        /// <param name="id">Journal Entry ID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <param name="expenseAccounts">Set of GL Accounts to which the user has access.</param>
        /// <returns>A journal entry domain entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<JournalEntry> GetJournalEntryAsync(string id, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (expenseAccounts == null)
            {
                expenseAccounts = new List<string>();
            }

            // Read the journal entry record
            var journalEntry = await DataReader.ReadRecordAsync<JrnlEnts>(id);
            {
                if (journalEntry == null)
                {
                    throw new KeyNotFoundException(string.Format("Journal Entry record {0} does not exist.", id));
                }
            }

            // Translate the status code into its value
            JournalEntryStatus journalEntryStatus = new JournalEntryStatus();

            if (!string.IsNullOrEmpty(journalEntry.JrtsStatus))
            {
                switch (journalEntry.JrtsStatus.ToUpper())
                {
                    case "C":
                        journalEntryStatus = JournalEntryStatus.Complete;
                        break;
                    case "N":
                        journalEntryStatus = JournalEntryStatus.NotApproved;
                        break;
                    case "U":
                        journalEntryStatus = JournalEntryStatus.Unfinished;
                        break;
                    default:
                        // if we get here, we have corrupt data
                        throw new ApplicationException("Invalid status for journal entry: " + journalEntry.Recordkey);
                }
            }

            // Translate the type code into its value
            JournalEntryType journalEntryType = new JournalEntryType();

            if (!string.IsNullOrEmpty(journalEntry.JrtsSource.ToUpper()))
            {
                switch (journalEntry.JrtsSource.ToUpper())
                {
                    case "JE":
                        journalEntryType = JournalEntryType.General;
                        break;
                    case "AA":
                        journalEntryType = JournalEntryType.OpeningBalance;
                        break;
                    default:
                        // if we get here, we have corrupt data
                        throw new ApplicationException("Invalid type for journal entry: " + journalEntry.Recordkey);
                }
            }

            if (!journalEntry.JrtsTrDate.HasValue)
            {
                throw new ApplicationException("Missing transaction date for journal entry: " + journalEntry.Recordkey);
            }

            if (!journalEntry.JrnlEntsAddDate.HasValue)
            {
                throw new ApplicationException("Missing entered date for journal entry: " + journalEntry.Recordkey);
            }

            string journalEntryEnteredByName = journalEntry.JrnlEntsAddOperator;
            if (string.IsNullOrEmpty(journalEntry.JrnlEntsAddOperator))
            {
                throw new ApplicationException("Missing add operator for journal entry: " + journalEntry.Recordkey);
            }
            else
            {
                var oper = await DataReader.ReadRecordAsync<Opers>("UT.OPERS", journalEntry.JrnlEntsAddOperator.ToString());
                if ((oper != null) && (!string.IsNullOrEmpty(oper.SysUserName)))
                {
                    journalEntryEnteredByName = oper.SysUserName;
                }
            }

            var journalEntryDomainEntity = new JournalEntry(journalEntry.Recordkey, journalEntry.JrtsTrDate.Value, journalEntryStatus, journalEntryType, journalEntry.JrnlEntsAddDate.Value.Date, journalEntryEnteredByName);

            journalEntryDomainEntity.Author = journalEntry.JrtsAuthor;

            journalEntryDomainEntity.AutomaticReversal = false;
            if (journalEntry.JrtsReversalFlag.ToUpper() == "Y")
            {
                journalEntryDomainEntity.AutomaticReversal = true;
            }
            journalEntryDomainEntity.Comments = journalEntry.JrtsComments;

            // Read the OPERS records associated with the approval signatures and 
            // next approvers on the journal entry and build approver objects

            var operators = new List<string>();
            if (journalEntry.JrtsAuthorizations != null)
            {
                operators.AddRange(journalEntry.JrtsAuthorizations);
            }
            if (journalEntry.JrtsNextApprovalIds != null)
            {
                operators.AddRange(journalEntry.JrtsNextApprovalIds);
            }

            var uniqueOperators = operators.Distinct().ToList();
            if (uniqueOperators.Count > 0)
            {
                var Approvers = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", uniqueOperators.ToArray(), true);
                if ((Approvers != null) && (Approvers.Count > 0))
                {
                    // loop through the opers, create Approver objects, add the name, and if they
                    // are one of the approvers of the journal entry, add the approval date.
                    foreach (var appr in Approvers)
                    {
                        Approver approver = new Approver(appr.Recordkey);
                        var approverName = appr.SysUserName;
                        approver.SetApprovalName(approverName);
                        if ((journalEntry.JrtsAuthEntityAssociation != null) && (journalEntry.JrtsAuthEntityAssociation.Count > 0))
                        {
                            foreach (var approval in journalEntry.JrtsAuthEntityAssociation)
                            {
                                if (approval.JrtsAuthorizationsAssocMember == appr.Recordkey)
                                {
                                    approver.ApprovalDate = approval.JrtsAuthorizationDatesAssocMember.Value.Date;
                                }
                            }
                        }

                        // Add any approvals to the journal entry domain entity
                        journalEntryDomainEntity.AddApprover(approver);
                    }
                }
            }

            // Populate the journal entry items domain entities and add them to the journal entry domain entity
            if ((journalEntry.JrtsDataEntityAssociation != null) && (journalEntry.JrtsDataEntityAssociation.Count > 0))
            {
                // If the user has the full access GL role, they have access to all GL accounts;
                // there is no need to check for GL account access security. If they have partial 
                // access, we need to call the CTX to check security.
                bool hasGlAccess = false;
                List<string> glAccountsAllowed = new List<string>();
                if (glAccessLevel == GlAccessLevel.Full_Access)
                {
                    hasGlAccess = true;
                }
                else if (glAccessLevel == GlAccessLevel.Possible_Access)
                {
                    // Put together a list of unique GL accounts from all the journal entry items.
                    foreach (var glDist in journalEntry.JrtsDataEntityAssociation)
                    {
                        if (!string.IsNullOrEmpty(glDist.JrtsGlNoAssocMember))
                        {
                            if (expenseAccounts.Contains(glDist.JrtsGlNoAssocMember))
                            {
                                hasGlAccess = true;
                                glAccountsAllowed.Add(glDist.JrtsGlNoAssocMember);
                            }
                        }
                    }
                }

                // Now apply GL account security to the journal entry items.
                // If hasGlAccess is true, it indicates the user has full access or has some
                // access to the GL accounts in this journal entry. If hasGlAccess if false, 
                // no journal entry items will be added to the journal entry domain entity.

                if (hasGlAccess == true)
                {
                    foreach (var glDist in journalEntry.JrtsDataEntityAssociation)
                    {
                        if ((glAccessLevel == GlAccessLevel.Full_Access) || (glAccountsAllowed.Contains(glDist.JrtsGlNoAssocMember)))
                        {
                            if (string.IsNullOrEmpty(glDist.JrtsDescriptionAssocMember))
                            {
                                throw new ApplicationException("Missing description for item in journal entry: " + journalEntry.Recordkey);
                            }
                            if (string.IsNullOrEmpty(glDist.JrtsGlNoAssocMember))
                            {
                                throw new ApplicationException("Missing GL Account for item in journal entry: " + journalEntry.Recordkey);
                            }
                            var description = glDist.JrtsDescriptionAssocMember;
                            var glAccount = glDist.JrtsGlNoAssocMember;

                            // Create a new journal entry item object
                            JournalEntryItem item = new JournalEntryItem(description, glAccount);

                            item.Credit = glDist.JrtsCreditAssocMember;
                            item.Debit = glDist.JrtsDebitAssocMember;

                            // A journal entry item cannot have both a debit and credit
                            if (item.Debit.HasValue && item.Credit.HasValue)
                            {
                                throw new ApplicationException("Both a debit and a credit exist for item " + description + " in journal entry: " + journalEntry.Recordkey);
                            }

                            item.ProjectId = glDist.JrtsProjectsCfIdAssocMember;
                            item.ProjectLineItemId = glDist.JrtsPrjItemIdsAssocMember;

                            journalEntryDomainEntity.AddItem(item);
                        }
                    }

                    // Get all the GL account descriptions, the project numbers and the project line item codes
                    // for all journal entry items added to the journal entry.

                    if ((journalEntryDomainEntity.Items != null) && (journalEntryDomainEntity.Items.Count() > 0))
                    {
                        // Initialize master lists to get unique GL accounts, project IDs and project line item IDs,
                        // for all the items in the journal entry
                        List<string> itemGlAccounts = new List<string>();
                        List<string> itemProjectIds = new List<string>();
                        List<string> itemProjectLineIds = new List<string>();

                        List<string> glAccountDescriptions = new List<string>();

                        foreach (var item in journalEntryDomainEntity.Items)
                        {
                            if (!itemGlAccounts.Contains(item.GlAccountNumber))
                            {
                                itemGlAccounts.Add(item.GlAccountNumber);
                            }

                            if (!itemProjectIds.Contains(item.ProjectId))
                            {
                                itemProjectIds.Add(item.ProjectId);
                            }

                            if (!itemProjectLineIds.Contains(item.ProjectLineItemId))
                            {
                                itemProjectLineIds.Add(item.ProjectLineItemId);
                            }
                        }

                        // Obtain the descriptions for all the GL accounts at once

                        GetGlAccountDescriptionRequest request = new GetGlAccountDescriptionRequest()
                        {
                            GlAccountIds = itemGlAccounts,
                            Module = "SS"
                        };

                        GetGlAccountDescriptionResponse response = transactionInvoker.Execute<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(request);

                        if (response != null)
                        {
                            for (int i = 0; i < response.GlDescriptions.Count(); i++)
                            {
                                foreach (var item in journalEntryDomainEntity.Items)
                                {
                                    if (item.GlAccountNumber == response.GlAccountIds[i])
                                    {
                                        item.GlAccountDescription = response.GlDescriptions[i];
                                    }
                                }
                            }
                        }

                        // If there are project IDs, we need to get the project number and
                        // the project line item code for each project line item ID 
                        if ((itemProjectIds != null) && (itemProjectIds.Count > 0))
                        {
                            // Bulk read the necessary project records to obtain the project number
                            var projectRecords = await DataReader.BulkReadRecordAsync<Projects>(itemProjectIds.ToArray());
                            if ((projectRecords != null) && (projectRecords.Count > 0))
                            {
                                // For each project ID, assign the corresponding project number
                                foreach (var project in projectRecords)
                                {
                                    foreach (var item in journalEntryDomainEntity.Items)
                                    {
                                        if (project.Recordkey == item.ProjectId)
                                        {
                                            item.ProjectNumber = project.PrjRefNo;
                                        }
                                    }
                                }
                            }

                            // If there are no project IDs, there are no project line item IDs

                            if ((itemProjectLineIds != null) && (itemProjectLineIds.Count > 0))
                            {
                                // Bulk read the necessary project line item records to obtain the project line item code
                                var projectLineItemRecords = await DataReader.BulkReadRecordAsync<ProjectsLineItems>(itemProjectLineIds.ToArray());
                                if ((projectLineItemRecords != null) && (projectLineItemRecords.Count > 0))
                                {
                                    // For each project line item ID, assign the corresponding the project line item code
                                    foreach (var projectItem in projectLineItemRecords)
                                    {
                                        foreach (var item in journalEntryDomainEntity.Items)
                                        {
                                            if (projectItem.Recordkey == item.ProjectLineItemId)
                                            {
                                                item.ProjectLineItemCode = projectItem.PrjlnProjectItemCode;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Update the total credits and debits to reflect all items
                foreach (var glDist in journalEntry.JrtsDataEntityAssociation)
                {
                    journalEntryDomainEntity.TotalCredits += glDist.JrtsCreditAssocMember ?? 0;
                    journalEntryDomainEntity.TotalDebits += glDist.JrtsDebitAssocMember ?? 0;
                }
            }

            return journalEntryDomainEntity;
        }
    }
}
