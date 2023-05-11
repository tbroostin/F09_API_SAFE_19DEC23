// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// Perform actions on budget adjustments.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BudgetAdjustmentsRepository : BaseColleagueRepository, IBudgetAdjustmentsRepository
    {
        private const string ValidationCTXFailedErrorMessage = "Validation CTX failed to return successfully";
        public BudgetAdjustmentsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Create a new budget adjustment.
        /// </summary>
        /// <param name="budgetAdjustmentInput">Budget adjustment.</param>
        /// <param name="majorComponentStartPosition">List of the major component start positions.</param>
        /// <returns>Budget adjustment response</returns>
        public async Task<BudgetAdjustment> CreateAsync(BudgetAdjustment budgetAdjustmentInput, IList<string> majorComponentStartPosition)
        {
            // Set up the CTX request and execute.
            var glNumbers = new List<string>();
            foreach (var adjustmentLine in budgetAdjustmentInput.AdjustmentLines)
            {
                if (!(string.IsNullOrEmpty(adjustmentLine.GlNumber)))
                {
                    var internalGlNumber = GlAccountUtility.ConvertGlAccountToInternalFormat(adjustmentLine.GlNumber, majorComponentStartPosition);
                    glNumbers.Add(internalGlNumber);
                }
            }

            var fromAmountStrings = new List<string>();
            foreach (var amount in budgetAdjustmentInput.AdjustmentLines.Select(x => x.FromAmount).ToList())
            {
                fromAmountStrings.Add(amount == 0 ? "" : amount.ToString());
            }

            var toAmountStrings = new List<string>();
            foreach (var amount in budgetAdjustmentInput.AdjustmentLines.Select(x => x.ToAmount).ToList())
            {
                toAmountStrings.Add(amount == 0 ? "" : amount.ToString());
            }

            // Assign the next approvers to the CTX argument.
            var nextApproverIds = new List<String>();
            if (budgetAdjustmentInput.NextApprovers != null && budgetAdjustmentInput.NextApprovers.Any())
            {
                foreach (var nextApprover in budgetAdjustmentInput.NextApprovers)
                {
                    if (nextApprover != null)
                    {
                        nextApproverIds.Add(nextApprover.NextApproverId.ToUpperInvariant());
                    }
                }
            }

            logger.Debug("==> BudgetAdjustmentsRepository TR date: " + budgetAdjustmentInput.TransactionDate + " <==");

            // Assign an empty list of string to the approver CTX argument and an empty list of dates to the
            // approver dates CTX argument.
            var approverIds = new List<String>();
            var approverDates = new List<DateTime?>();
            BudgetAdjustment budgetAdjustmentOutput = null;

            // Pass a blank string as the budget entry ID to make sure it gets initialized properly when the CTX executes.
            // Normally (i.e. on GLBE) the FROM amount is the DEBIT and TO the CREDIT. However, we "swap the columns" since
            // we're dealing with budget amounts.
            var request = new TxUpdateBudgetAdjustmentRequest()
            {
                ABudgetEntryId = "",
                AlGlAccounts = glNumbers,
                AlDebits = toAmountStrings,
                AlCredits = fromAmountStrings,
                AReason = budgetAdjustmentInput.Reason,
                AAuthor = budgetAdjustmentInput.Initiator,
                AlComments = new List<string>() { budgetAdjustmentInput.Comments },
                APersonId = budgetAdjustmentInput.PersonId,
                ATrDate = DateTime.SpecifyKind(budgetAdjustmentInput.TransactionDate, DateTimeKind.Unspecified),
                AlNextApproverIds = nextApproverIds,
                AlApprovalIds = approverIds,
                AlApprovalDates = approverDates
            };

            try
            {
                var response = await transactionInvoker.ExecuteAsync<TxUpdateBudgetAdjustmentRequest, TxUpdateBudgetAdjustmentResponse>(request);

                // If the adjustment was created and posted then create a new instance of the budget adjustment, otherwise add the error message supplied by the CTX.

                if (!string.IsNullOrEmpty(response.ABudgetEntryId))
                {
                    budgetAdjustmentOutput = await GetBudgetAdjustmentAsync(response.ABudgetEntryId);
                    logger.Debug("==> BudgetAdjustmentsRepository OUT TR date: " + budgetAdjustmentOutput.TransactionDate + " <==");
                }
                else
                {
                    budgetAdjustmentInput.ErrorMessages = response.AlMessage;
                    budgetAdjustmentOutput = budgetAdjustmentInput;
                }
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "==> TxUpdateBudgetAdjustmentRequest session expired <==");
                throw;
            }

            return budgetAdjustmentOutput;
        }
        
        /// <summary>
        /// Update an existing budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment to be updated.</param>
        /// <param name="budgetAdjustment">The new budget adjustment data.</param>
        /// <param name="majorComponentStartPosition">List of the major component start positions.</param>
        /// <returns>The updated budget adjustment entity</returns>
        public async Task<BudgetAdjustment> UpdateAsync(string id, BudgetAdjustment budgetAdjustmentInput, IList<string> majorComponentStartPosition)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (budgetAdjustmentInput == null)
            {
                throw new ArgumentNullException("budgetAdjustmentInput");
            }

            // Set up the CTX request and execute.
            var glNumbers = new List<string>();

            foreach (var adjustmentLine in budgetAdjustmentInput.AdjustmentLines)
            {
                if (!(string.IsNullOrEmpty(adjustmentLine.GlNumber)))
                {
                    var internalGlNumber = GlAccountUtility.ConvertGlAccountToInternalFormat(adjustmentLine.GlNumber, majorComponentStartPosition);
                    glNumbers.Add(internalGlNumber);
                }
            }

            var fromAmountStrings = new List<string>();
            foreach (var amount in budgetAdjustmentInput.AdjustmentLines.Select(x => x.FromAmount).ToList())
            {
                fromAmountStrings.Add(amount == 0 ? "" : amount.ToString());
            }

            var toAmountStrings = new List<string>();
            foreach (var amount in budgetAdjustmentInput.AdjustmentLines.Select(x => x.ToAmount).ToList())
            {
                toAmountStrings.Add(amount == 0 ? "" : amount.ToString());
            }

            // Separate the comments lines.
            var comments = new List<string>(budgetAdjustmentInput.Comments.Split(DmiString._VM));

            // Assign the next approvers to the CTX argument.
            var nextApproverIds = new List<String>();
            if (budgetAdjustmentInput.NextApprovers != null && budgetAdjustmentInput.NextApprovers.Any())
            {
                foreach (var nextApprover in budgetAdjustmentInput.NextApprovers)
                {
                    if (nextApprover != null)
                    {
                        nextApproverIds.Add(nextApprover.NextApproverId.ToUpperInvariant());
                    }
                }
            }

            // Assign any approver IDs and dates that I have from the input entity to the approver ID and 
            // date CTX arguments.
            var approverIds = new List<String>();
            var approverDates = new List<DateTime?>();
            if (budgetAdjustmentInput.Approvers != null && budgetAdjustmentInput.Approvers.Any())
            {
                foreach (var approver in budgetAdjustmentInput.Approvers)
                {
                    if (approver != null)
                    {
                        if (approver.ApproverId == null)
                        {
                            LogDataError("Approver ID cannot be null for record ", id, ".");
                        }
                        else
                        {
                            approverIds.Add(approver.ApproverId.ToUpperInvariant());
                            approverDates.Add(approver.ApprovalDate);
                        }
                    }
                }
            }

            // Normally (i.e. on GLBE) the FROM amount is the DEBIT and TO the CREDIT. However, we "swap the columns" since
            // we're dealing with budget amounts.
            var request = new TxUpdateBudgetAdjustmentRequest()
            {
                ABudgetEntryId = id,
                AlGlAccounts = glNumbers,
                AlDebits = toAmountStrings,
                AlCredits = fromAmountStrings,
                AReason = budgetAdjustmentInput.Reason,
                AAuthor = budgetAdjustmentInput.Initiator,
                AlComments = comments,
                APersonId = budgetAdjustmentInput.PersonId,
                ATrDate = DateTime.SpecifyKind(budgetAdjustmentInput.TransactionDate, DateTimeKind.Unspecified),
                AlNextApproverIds = nextApproverIds,
                AlApprovalIds = approverIds,
                AlApprovalDates = approverDates
            };

            try
            {
                var response = await transactionInvoker.ExecuteAsync<TxUpdateBudgetAdjustmentRequest, TxUpdateBudgetAdjustmentResponse>(request);

                // If the adjustment was created and posted then create a new instance of the budget adjustment, otherwise add the error message supplied by the CTX.
                BudgetAdjustment budgetAdjustmentOutput = null;

                if (response.AlMessage == null || response.AlMessage.Count == 0)
                {
                    budgetAdjustmentOutput = await GetBudgetAdjustmentAsync(response.ABudgetEntryId);
                }
                else
                {
                    budgetAdjustmentInput.ErrorMessages = response.AlMessage;
                    budgetAdjustmentOutput = budgetAdjustmentInput;
                }

                return budgetAdjustmentOutput;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "==> TxUpdateBudgetAdjustmentRequest session expired <==");
                throw;
            }
        }

        /// <summary>
        /// Get a budget adjustment id
        /// </summary>
        /// <param name="id">The ID for the budget adjustment.</param>
        /// <returns>A budget adjustment.</returns>
        public async Task<BudgetAdjustment> GetBudgetAdjustmentAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var budgetAdjustmentRecord = await DataReader.ReadRecordAsync<BudgetEntries>(id);
            if (budgetAdjustmentRecord == null)
            {
                throw new KeyNotFoundException(string.Format("Budget adjustment record {0} does not exist.", id));
            }

            // Validate that this is a budget adjustment, that is, a budget entry with a "BU" source code.
            if (budgetAdjustmentRecord.BgteSource.ToUpperInvariant() != "BU")
            {
                throw new ApplicationException(string.Format("Budget adjustment record {0} does not have a BU source code.", id));
            }

            // Budget adjustments can be created in self-service and in Colleague. 
            // For records created in self-service, Envision stores in the ADD/CHG operator fields the person ID of the user.
            // For records created in Colleague, Envision stores in the ADD/CHG operator fields the login id of the user.

            // We need to determine if this budget adjustment contains the person Id in the ADD operator field, and if does not,
            // we need to get the person id from the login that is stored in the ADD operator field.

            // Make sure the AddOperator field is not null or empty.
            string personId = budgetAdjustmentRecord.BudgetEntriesAddopr;

            if (string.IsNullOrEmpty(personId))
            {
                throw new ApplicationException(string.Format("The budget adjustment {0} contains no Add Operator.", id));
            }

            // If the AddOperator field is all numeric, it is a person ID.
            // If it is not all numeric, is a login id.
            if (!personId.All(char.IsDigit))
            {
                // Initialize the criteria.
                var staffCriteria = "WITH STAFF.LOGIN.ID EQ '" + budgetAdjustmentRecord.BudgetEntriesAddopr + "'";
                // Select and read the staff record with the login for the user.
                var staffIds = await DataReader.SelectAsync("STAFF", staffCriteria);
                if (staffIds == null || staffIds.Count() == 0)
                {
                    throw new KeyNotFoundException(string.Format("No Staff record was found for login {0}.", budgetAdjustmentRecord.BudgetEntriesAddopr));
                }
                if (staffIds.Count() > 1)
                {
                    throw new ApplicationException(string.Format("More than on Staff record was found for login {0}.", budgetAdjustmentRecord.BudgetEntriesAddopr));
                }

                var staffId = staffIds.FirstOrDefault();
                var staffRecord = await DataReader.ReadRecordAsync<Staff>(staffId);
                if (staffRecord == null)
                {
                    LogDataError("Unable to read the STAFF record for ", personId, ".");
                    throw new KeyNotFoundException(string.Format("Unable to read the STAFF record for {0}.", staffIds));
                }
                personId = staffRecord.Recordkey;
            }

            // Populate the domain entity.
            string budgetAdjustmentId = budgetAdjustmentRecord.Recordkey;
            DateTime transactionDate = budgetAdjustmentRecord.BgteTrDate.Value;

            // An unfinished budget adjustment created in Colleague may not have any GL accounts.
            // In that case populate it with a space.
            string reason = " ";
            if (budgetAdjustmentRecord.BgteDataEntityAssociation != null && budgetAdjustmentRecord.BgteDataEntityAssociation.Any())
            {
                if (budgetAdjustmentRecord.BgteDataEntityAssociation.FirstOrDefault() != null)
                {
                    if (!string.IsNullOrEmpty(budgetAdjustmentRecord.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                    {
                        reason = budgetAdjustmentRecord.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember;
                    }
                }
            }

            var adjustmentLines = new List<AdjustmentLine>();

            // Create the domain entity.
            var budgetAdjustment = new BudgetAdjustment(budgetAdjustmentId, reason, transactionDate, personId);

            // Loop through the adjustment lines, if there are any.
            if (budgetAdjustmentRecord.BgteDataEntityAssociation != null && budgetAdjustmentRecord.BgteDataEntityAssociation.Any())
            {
                foreach (var associationMember in budgetAdjustmentRecord.BgteDataEntityAssociation)
                {
                    if (associationMember != null)
                    {
                        var glAccount = associationMember.BgteGlNoAssocMember;
                        var fromAmount = associationMember.BgteCreditAssocMember.HasValue ? associationMember.BgteCreditAssocMember.Value : 0m;
                        var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                        var adjustmentLine = new AdjustmentLine(glAccount, fromAmount, toAmount);
                        // Add the adjustment line to the budget adjustment list of adjustment lines.
                        budgetAdjustment.AddAdjustmentLine(adjustmentLine);
                    }
                }
            }

            // Translate and assign the status. Budget Adjustments can have one of several statuses.
            BudgetEntryStatus budgetEntryStatus = new BudgetEntryStatus();

            // Get the first status in the list of budget entry statuses, and check that it has a value.
            if (budgetAdjustmentRecord.BgteStatus != null && !string.IsNullOrEmpty(budgetAdjustmentRecord.BgteStatus.FirstOrDefault()))
            {
                switch (budgetAdjustmentRecord.BgteStatus.FirstOrDefault().ToUpper())
                {
                    case "U":
                        budgetEntryStatus = BudgetEntryStatus.Unfinished;
                        break;
                    case "N":
                        budgetEntryStatus = BudgetEntryStatus.NotApproved;
                        break;
                    case "C":
                        budgetEntryStatus = BudgetEntryStatus.Complete;
                        break;
                    default:
                        // if we get here, we have corrupt data.
                        throw new ApplicationException("Invalid status for budget adjustment: " + budgetAdjustmentRecord.Recordkey);
                }
            }
            else
            {
                throw new ApplicationException("Missing status for budget adjustment: " + budgetAdjustmentRecord.Recordkey);
            }
            budgetAdjustment.Status = budgetEntryStatus;

            // Assign the initiator
            budgetAdjustment.Initiator = budgetAdjustmentRecord.BgteAuthor;

            // Convert the comments into a paragraph.
            var comments = string.Empty;
            if (!string.IsNullOrEmpty(budgetAdjustmentRecord.BgteComments))
            {
                comments = CommentsUtility.ConvertCommentsToParagraphs(budgetAdjustmentRecord.BgteComments);
            }
            budgetAdjustment.Comments = comments;

            // If there are any next approvers, add them and get their names from their OPERS records.
            budgetAdjustment.NextApprovers = new List<NextApprover>();
            if (budgetAdjustmentRecord.BgteApprovalEntityAssociation != null && budgetAdjustmentRecord.BgteApprovalEntityAssociation.Any())
            {
                var operIds = new List<string>();
                operIds.AddRange(budgetAdjustmentRecord.BgteNextApprovalIds);
                // Make sure the list does not contain duplicates.
                // Read all the OPERS records at once.
                var uniqueOperIds = operIds.Distinct();
                if (uniqueOperIds != null && uniqueOperIds.Any())
                {
                    var operRecords = await DataReader.BulkReadRecordAsync<DataContracts.Opers>("UT.OPERS", uniqueOperIds.ToArray(), true);
                    // If a next approver ID does not have an OPERS record, it is not a valid one.
                    if (operRecords != null && operRecords.Any())
                    {
                        foreach (var oper in operRecords)
                        {
                            if (oper != null)
                            {
                                NextApprover nextApproverEntity = new NextApprover(oper.Recordkey);
                                nextApproverEntity.SetNextApproverName(oper.SysUserName);
                                budgetAdjustment.NextApprovers.Add(nextApproverEntity);
                            }
                        }
                    }
                }
            }

            // If there are any approvers, add them and get their names from their OPERS records,
            // and get the associated date, and add the approver to the budget adjustment entity.
            budgetAdjustment.Approvers = new List<Approver>();
            if (budgetAdjustmentRecord.BgteAuthEntityAssociation != null && budgetAdjustmentRecord.BgteAuthEntityAssociation.Any())
            {
                var operIds = new List<string>();
                operIds.AddRange(budgetAdjustmentRecord.BgteAuthorizations);
                // Make sure the list does not contain duplicates.
                // Read all the OPERS records at once.
                var uniqueOperIds = operIds.Distinct();
                if (uniqueOperIds != null && uniqueOperIds.Any())
                {
                    var operRecords = await DataReader.BulkReadRecordAsync<DataContracts.Opers>("UT.OPERS", uniqueOperIds.ToArray(), true);
                    // If an approver ID does not have an OPERS record, it is not a valid one.
                    if (operRecords != null && operRecords.Any())
                    {
                        foreach (var oper in operRecords)
                        {
                            if (oper != null)
                            {
                                Approver approverEntity = new Approver(oper.Recordkey);
                                approverEntity.SetApprovalName(oper.SysUserName);
                                //get the approval date for the approval
                                foreach (var auth in budgetAdjustmentRecord.BgteAuthEntityAssociation)
                                {
                                    if (auth.BgteAuthorizationsAssocMember == oper.Recordkey)
                                    {
                                        approverEntity.ApprovalDate = auth.BgteAuthorizationDatesAssocMember;
                                    }
                                }
                                budgetAdjustment.Approvers.Add(approverEntity);
                            }
                        }
                    }
                }
            }

            return budgetAdjustment;
        }

        /// <summary>
        /// Get a list of draft and non-draft budget adjustments for a person ID.
        /// The non-draft budget adjustments can also have been created in Colleague, and in this
        /// case we only want to retrieve those with a BU source code (BU is for budget adjustment).
        /// </summary>
        /// <param name="personId">The person ID for the current user.</param>
        /// <returns>List of budget adjustments summary domain entities.</returns>
        public async Task<IEnumerable<BudgetAdjustmentSummary>> GetBudgetAdjustmentsSummaryAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "The user's person ID must be specified.");
            }

            // Initialize the list of return entities.
            var budgetAdjustmentSummaryEntities = new List<BudgetAdjustmentSummary>();

            // First get all the draft budget adjustments for the user, if there are any.

            // Initialize the file name and criteria.
            var draftBudgetAdjustmentsFileName = "DRAFT.BUDGET.ENTRIES";
            // Draft budget entries are only created through self-service. For records created in self-service, 
            // Envision stores the person ID of the user logged in in the ADD/CHG operator fields.
            var criteria1 = "DRAFT.BUDGET.ENTRIES.ADDOPR EQ '" + personId + "'";

            // Select and bulk read the records for the user.
            var draftBudgetAdjustmentIds = await DataReader.SelectAsync(draftBudgetAdjustmentsFileName, criteria1);
            var draftContracts = await DataReader.BulkReadRecordAsync<DraftBudgetEntries>(draftBudgetAdjustmentsFileName, draftBudgetAdjustmentIds, true);

            // If there are any records selected, loop through them and obtain the data to
            // populate each budget adjustment summary domain entity.
            if (draftContracts != null && draftContracts.Any())
            {
                foreach (var contract in draftContracts)
                {
                    try
                    {
                        if (contract.Recordkey == null)
                        {
                            throw new NullReferenceException("RecordKey is a required field");
                        }

                        string reason = " ";
                        if (!string.IsNullOrEmpty(contract.DbgteReason))
                        {
                            reason = contract.DbgteReason;
                        }
                        // Populate the person ID of the user to check security in the service.
                        var draftPersonId = contract.DraftBudgetEntriesAddopr;
                        // Create a budget adjustment summary domain entity.
                        var budgetAdjustmentSummary = new BudgetAdjustmentSummary(reason, draftPersonId);

                        budgetAdjustmentSummary.DraftBudgetAdjustmentId = contract.Recordkey;
                        budgetAdjustmentSummary.TransactionDate = contract.DbgteTrDate.Value;

                        // Assign a draft status to these drafts.
                        budgetAdjustmentSummary.Status = BudgetEntryStatus.Draft;

                        // Loop through the adjustment lines, if there are any, and 
                        // get the total of the to amounts, in whichever line they have a value.
                        if (contract.DbgteDataEntityAssociation != null && contract.DbgteDataEntityAssociation.Any())
                        {
                            foreach (var associationMember in contract.DbgteDataEntityAssociation)
                            {
                                if (associationMember != null)
                                {
                                    var draftToAmount = associationMember.DbgteDebitAssocMember.HasValue ? associationMember.DbgteDebitAssocMember.Value : 0m;
                                    budgetAdjustmentSummary.ToAmount += draftToAmount;
                                }
                            }
                        }

                        // Add the draft budget adjustment to the list of draft budget adjustments domain entities.
                        if (budgetAdjustmentSummary != null)
                        {
                            budgetAdjustmentSummaryEntities.Add(budgetAdjustmentSummary);
                        }
                    }
                    catch (Exception e)
                    {
                        LogDataError("DraftBudgetEntries", personId, new Object(), e, e.Message);
                    }
                }
            }
            else
            {
                LogDataError("DraftBudgetEntries", personId, "No DRAFT.BUDGET.ENTRIES records selected");
            }

            // Now get all the budget adjustments for the user with a source of 'BU', if there are any.

            // Budget adjustments can be created in self-service and in Colleague. 
            // For records created in self-service, Envision stores in the ADD/CHG operator fields the person ID of the user.
            // For records created in Colleague, Envision stores in the ADD/CHG operator fields the login id of the user.
            // To get the login of the person ID, we need to read the STAFF record, and get the login id from there.
            var staffRecord = await DataReader.ReadRecordAsync<Staff>("STAFF", personId);
            if (staffRecord == null)
            {
                LogDataError("The user", personId, "does not have a STAFF record.");
                throw new ArgumentNullException("personId", "The user does not have a STAFF record.");
            }

            string personLogin = staffRecord.StaffLoginId;
            if (string.IsNullOrEmpty(staffRecord.StaffLoginId))
            {
                LogDataError("The user", personId, "does not have a login in their STAFF record.");
                throw new ArgumentNullException("personId", "The user does not have a login in their STAFF record.");
            }

            // Initialize the file name and criteria.
            var BudgetAdjustmentsFileName = "BUDGET.ENTRIES";
            var criteria2 = "WITH BGTE.SOURCE EQ 'BU'";
            criteria2 += " AND WITH (BUDGET.ENTRIES.ADDOPR EQ '" + personId + "' OR BUDGET.ENTRIES.ADDOPR EQ '" + personLogin + "')";

            // Select and bulk read the records for the user.
            var budgetAdjustmentIds = await DataReader.SelectAsync(BudgetAdjustmentsFileName, criteria2);
            var budgetAdjustmentContracts = await DataReader.BulkReadRecordAsync<BudgetEntries>(BudgetAdjustmentsFileName, budgetAdjustmentIds, true);

            // If there are any records selected, loop through them and obtain the data to
            // populate each budget adjustment summary domain entity.
            if (budgetAdjustmentContracts != null && budgetAdjustmentContracts.Any())
            {
                foreach (var contract in budgetAdjustmentContracts)
                {
                    try
                    {
                        if (contract.Recordkey == null)
                        {
                            throw new NullReferenceException("RecordKey is a required field");
                        }

                        if (contract.BudgetEntriesAddopr == personId || contract.BudgetEntriesAddopr == personLogin)
                        {
                            // Populate the reason with the first GL account description or a space if there are no GL accounts.
                            string reason = " ";
                            if (contract.BgteDataEntityAssociation != null && contract.BgteDataEntityAssociation.Any())
                            {
                                if (contract.BgteDataEntityAssociation.FirstOrDefault() != null)
                                {
                                    if (!string.IsNullOrEmpty(contract.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                                    {
                                        reason = contract.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember;
                                    }
                                }
                            }
                            // Populate the person ID of the user to check security in the service.
                            var id = personId;
                            // Create a budget adjustment summary domain entity.
                            var budgetAdjustmentSummary = new BudgetAdjustmentSummary(reason, id);

                            // Translate and assign the status. Budget Adjustments can have one of several statuses.
                            BudgetEntryStatus budgetEntryStatus = new BudgetEntryStatus();

                            // Get the first status in the list of budget entry statuses, and check that it has a value.
                            if (contract.BgteStatus != null && !string.IsNullOrEmpty(contract.BgteStatus.FirstOrDefault()))
                            {
                                switch (contract.BgteStatus.FirstOrDefault().ToUpper())
                                {
                                    case "U":
                                        budgetEntryStatus = BudgetEntryStatus.Unfinished;
                                        break;
                                    case "N":
                                        budgetEntryStatus = BudgetEntryStatus.NotApproved;
                                        break;
                                    case "C":
                                        budgetEntryStatus = BudgetEntryStatus.Complete;
                                        break;
                                    default:
                                        // if we get here, we have corrupt data.
                                        throw new ApplicationException("Invalid status for budget adjustment: " + contract.Recordkey);
                                }
                            }
                            else
                            {
                                throw new ApplicationException("Missing status for budget adjustment: " + contract.Recordkey);
                            }
                            budgetAdjustmentSummary.Status = budgetEntryStatus;

                            // Assign the budget entry number and the transaction date.
                            budgetAdjustmentSummary.BudgetAdjustmentNumber = contract.Recordkey;
                            budgetAdjustmentSummary.TransactionDate = contract.BgteTrDate.Value;

                            // Loop through the adjustment lines, if there are any, and 
                            // get the total of the to amounts, in whichever line they have a value.
                            if (contract.BgteDataEntityAssociation != null && contract.BgteDataEntityAssociation.Any())
                            {
                                foreach (var associationMember in contract.BgteDataEntityAssociation)
                                {
                                    if (associationMember != null)
                                    {
                                        var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                                        budgetAdjustmentSummary.ToAmount += toAmount;
                                    }
                                }
                            }

                            // Add the  budget adjustment to the list of  budget adjustments domain entities.
                            if (budgetAdjustmentSummary != null)
                            {
                                budgetAdjustmentSummaryEntities.Add(budgetAdjustmentSummary);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogDataError("BudgetEntries", personId, new Object(), e, e.Message);
                    }
                }
            }
            else
            {
                LogDataError("BudgetEntries", personId, "No BUDGET.ENTRIES records selected");
            }

            return budgetAdjustmentSummaryEntities;
        }

        /// <summary>
        /// Get a list of not approved budget adjustments for the person ID. This means return all the
        /// budget adjustments where the approval ID for the person is listed as a next approver ID.
        /// These budget adjustments must have a status of N(ot-approved) and a source of BU for 
        /// budget adjustment.These may have been created in Colleague or SS.
        /// </summary>
        /// <param name="personId">The person ID for the current user.</param>
        /// <returns>List of not-approved budget adjustments summary domain entities.</returns>
        public async Task<IEnumerable<BudgetAdjustmentPendingApprovalSummary>> GetBudgetAdjustmentsPendingApprovalSummaryAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "The user's person ID must be specified.");
            }

            // Initialize the list of return entities.
            var budgetAdjustmentSummaryEntities = new List<BudgetAdjustmentPendingApprovalSummary>();

            // Get all the budget adjustments for the user with a status of "N" and a source of 'BU', if there are any.

            // Budget adjustments can be created in self-service and in Colleague. 
            // For records created in self-service, Envision stores in the ADD/CHG operator fields the person ID of the user.
            // For records created in Colleague, Envision stores in the ADD/CHG operator fields the login id of the user.
            // To get the approval ID for the person ID, we need to get the login of the person ID from the STAFF record
            // and get the login id from there. The approval ID is the same as the login ID.
            var staffRecord = await DataReader.ReadRecordAsync<Staff>("STAFF", personId);
            if (staffRecord == null)
            {
                logger.Error("==> The user " + personId + " does not have a STAFF record <==");
                throw new ApplicationException("The user " + personId + " does not have a STAFF record.");
            }

            string personLogin = staffRecord.StaffLoginId;
            if (string.IsNullOrEmpty(staffRecord.StaffLoginId))
            {
                logger.Error("==> The user " + personId + " does not have a login in their STAFF record <==");
                throw new ApplicationException("The user " + personId + " does not have a login in their STAFF record.");
            }

            // Initialize the file name and criteria.
            var BudgetAdjustmentsFileName = "BUDGET.ENTRIES";
            var criteria2 = "WITH BGTE.SOURCE EQ 'BU' AND WITH BGTE.NEXT.APPROVAL.IDS EQ '" + personLogin + "'";

            // Select and bulk read the records for the user.
            var budgetAdjustmentIds = await DataReader.SelectAsync(BudgetAdjustmentsFileName, criteria2);
            var budgetAdjustmentContracts = await DataReader.BulkReadRecordAsync<BudgetEntries>(BudgetAdjustmentsFileName, budgetAdjustmentIds, true);

            // If there are any records selected, loop through them and obtain the data from the
            // ones that are not approved to populate each budget adjustment summary domain entity.
            if (budgetAdjustmentContracts != null && budgetAdjustmentContracts.Any())
            {
                // Loop through each budget adjustment contract.
                foreach (var contract in budgetAdjustmentContracts)
                {
                    if (contract.Recordkey == null)
                    {
                        logger.Error("This budget entry record does not have a key.");
                    }
                    else
                    {
                        // Process this budget entry if it is not approved and the person's login in the list of next approver IDs.
                        if (contract.BgteStatus != null && contract.BgteStatus.FirstOrDefault().ToUpperInvariant() == "N"
                            && contract.BgteNextApprovalIds != null && contract.BgteNextApprovalIds.Contains(personLogin))
                        {

                            // Create a budget adjustment summary domain entity.
                            var budgetAdjustmentSummary = new BudgetAdjustmentPendingApprovalSummary();

                            // Populate the reason with the first GL account description or a space if there are no GL accounts.
                            if (contract.BgteDataEntityAssociation != null && contract.BgteDataEntityAssociation.Any())
                            {
                                if (contract.BgteDataEntityAssociation.FirstOrDefault() != null)
                                {
                                    if (!string.IsNullOrEmpty(contract.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                                    {
                                        budgetAdjustmentSummary.Reason = contract.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember;
                                    }
                                }
                            }

                            // The add operator will be a person ID for budget adjustments created in SS or
                            // a login ID for those created in UI. Populate the appropriate property.
                            if (contract.BudgetEntriesAddopr.All(char.IsDigit) && (contract.BudgetEntriesAddopr.Length == 7 || contract.BudgetEntriesAddopr.Length == 10))
                            {
                                budgetAdjustmentSummary.InitiatorId = contract.BudgetEntriesAddopr;
                            }
                            else
                            {
                                budgetAdjustmentSummary.InitiatorLoginId = contract.BudgetEntriesAddopr;
                            }

                            // Assign the status.
                            budgetAdjustmentSummary.Status = BudgetEntryStatus.NotApproved;

                            // Assign the budget entry number and the transaction date.
                            budgetAdjustmentSummary.BudgetAdjustmentNumber = contract.Recordkey;
                            budgetAdjustmentSummary.TransactionDate = contract.BgteTrDate.HasValue ? contract.BgteTrDate.Value : default(DateTime);

                            // Loop through the adjustment lines, if there are any, and 
                            // get the total of the to amounts, in whichever line they have a value.
                            if (contract.BgteDataEntityAssociation != null && contract.BgteDataEntityAssociation.Any())
                            {
                                foreach (var associationMember in contract.BgteDataEntityAssociation)
                                {
                                    if (associationMember != null)
                                    {
                                        var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                                        budgetAdjustmentSummary.ToAmount += toAmount;
                                    }
                                }
                            }

                            // Add the  budget adjustment to the list of  budget adjustments domain entities.
                            if (budgetAdjustmentSummary != null)
                            {
                                budgetAdjustmentSummaryEntities.Add(budgetAdjustmentSummary);
                            }
                        }
                    }
                }

                if (budgetAdjustmentSummaryEntities != null && budgetAdjustmentSummaryEntities.Any())
                {
                    // Obtain the list of login IDs from the entities that have one.
                    List<string> loginIds = new List<string>();
                    loginIds = budgetAdjustmentSummaryEntities.Where(x => !string.IsNullOrEmpty(x.InitiatorLoginId)).Select(y => y.InitiatorLoginId).Distinct().ToList();

                    // Select their STAFF records to get the person ID.
                    if (loginIds != null && loginIds.Any())
                    {
                        // Initialize the criteria.
                        var staffCriteria = "WITH STAFF.LOGIN.ID EQ '?'";
                        // Select the staff records with the login for the user.
                        var staffIds = await DataReader.SelectAsync("STAFF", staffCriteria, loginIds.ToArray());
                        if (staffIds == null || staffIds.Count() == 0)
                        {
                            logger.Error("There are no STAFF records for any of the login IDs provided.");
                        }
                        else
                        {
                            // Read the selected records.
                            var staffContracts = await DataReader.BulkReadRecordAsync<Staff>("STAFF", staffIds);

                            if (staffContracts != null && staffContracts.Any())
                            {
                                // Loop through each staff contract.
                                foreach (var contract in staffContracts)
                                {
                                    if (contract != null)
                                    {
                                        if (contract.Recordkey == null)
                                        {
                                            logger.Error("This staff record does not have a key.");
                                        }
                                        else
                                        {
                                            // Populate the initiator id for those entities that have a login id equal to the one in this contract.
                                            var matchingLoginEntities = budgetAdjustmentSummaryEntities.Where(x => x.InitiatorLoginId == contract.StaffLoginId).ToList();
                                            foreach (var entity in matchingLoginEntities)
                                            {
                                                entity.InitiatorId = contract.Recordkey;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }

                    // Populate the Initiator Name by using a CTX.
                    // Initialize arguments for the CTX that will get the initiator name.
                    List<string> personIds = new List<string>();
                    List<string> hierarchies = new List<string>();

                    // Get a unique list of person IDs.
                    personIds = budgetAdjustmentSummaryEntities.Where(j => !string.IsNullOrEmpty(j.InitiatorId)).Select(i => i.InitiatorId).Distinct().ToList();

                    // Call a colleague transaction to get the initiator name based on their hierarchy.
                    if ((personIds != null) && (personIds.Any()))
                    {
                        for (int i = 0; i < personIds.Count; i++)
                        {
                            hierarchies.Add("PREFERRED");
                        }

                        GetHierarchyNamesForIdsRequest request = new GetHierarchyNamesForIdsRequest()
                        {
                            IoPersonIds = personIds,
                            IoHierarchies = hierarchies
                        };
                        GetHierarchyNamesForIdsResponse response = await transactionInvoker.ExecuteAsync<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(request);

                        try
                        {

                            // The transaction returns the hierarchy names. If the name is multivalued, 
                            // the transaction only returns the first value of the name.
                            if (response != null)
                            {
                                if (!((response.OutPersonNames == null) || (response.OutPersonNames.Count < 1)))
                                {
                                    for (int x = 0; x < response.IoPersonIds.Count(); x++)
                                    {
                                        var ioPersonId = response.IoPersonIds[x];
                                        var hierarchy = response.IoHierarchies[x];
                                        var name = response.OutPersonNames[x];
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            var samePersonIdEntities = budgetAdjustmentSummaryEntities.Where(i => i.InitiatorId == ioPersonId).ToList();
                                            foreach (var domainEntity in samePersonIdEntities)
                                            {
                                                domainEntity.InitiatorName = name;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (ColleagueSessionExpiredException csee)
                        {
                            logger.Error(csee, "==> GetHierarchyNamesForIdsRequest session expired <==");
                            throw;
                        }
                    }
                }
            }
            else
            {
                logger.Error("BudgetEntries " + personId + " No BUDGET.ENTRIES records selected <==");
            }
            return budgetAdjustmentSummaryEntities;
        }

        /// <summary>
        /// Validates the budget adjustment entity using a Colleague Transaction.
        /// </summary>
        /// <param name="budgetAdjustmentEntity">The entity to validate.</param>
        /// <param name="majorComponentStartPosition">List of the major component start positions.</param>
        /// <returns>List of strings that contain error messages from Colleague.</returns>
        public async Task<List<string>> ValidateBudgetAdjustmentAsync(BudgetAdjustment budgetAdjustmentEntity, IList<string> majorComponentStartPosition)
        {
            if (budgetAdjustmentEntity == null)
            {
                throw new ArgumentNullException("budgetAdjustmentEntity");
            }

            // Set up the CTX request and execute.
            TxValidateBudgetAdjustmentRequest request = new TxValidateBudgetAdjustmentRequest()
            {
                ATrDate = budgetAdjustmentEntity.TransactionDate,
                APersonId = budgetAdjustmentEntity.PersonId,
                AReason = budgetAdjustmentEntity.Reason
            };

            List<DateTime?> approvalDates = new List<DateTime?>();
            List<string> approvalIds = new List<string>();
            if (budgetAdjustmentEntity.Approvers != null)
            {
                foreach (var approver in budgetAdjustmentEntity.Approvers)
                {
                    approvalDates.Add(approver.ApprovalDate);
                    approvalIds.Add(approver.ApproverId);
                }
            }
            request.AlApprovalDates = approvalDates;
            request.AlApprovalIds = approvalIds;

            List<string> nextApproverIds = new List<string>();
            if (budgetAdjustmentEntity.NextApprovers != null)
            {
                foreach (var nextApprover in budgetAdjustmentEntity.NextApprovers)
                {
                    nextApproverIds.Add(nextApprover.NextApproverId);
                }
            }
            request.AlNextApproverIds = nextApproverIds;

            List<string> credits = new List<string>();
            List<string> debits = new List<string>();
            List<string> glAccounts = new List<string>();
            if (budgetAdjustmentEntity.AdjustmentLines != null)
            {
                foreach (var adjustmentLine in budgetAdjustmentEntity.AdjustmentLines)
                {
                    if (adjustmentLine != null && adjustmentLine.GlNumber != null)
                    {
                        glAccounts.Add(GlAccountUtility.ConvertGlAccountToInternalFormat(adjustmentLine.GlNumber, majorComponentStartPosition));
                        credits.Add(adjustmentLine.FromAmount == 0 ? "" : adjustmentLine.FromAmount.ToString());
                        debits.Add(adjustmentLine.ToAmount == 0 ? "" : adjustmentLine.ToAmount.ToString());
                    }
                }
            }
            request.AlCredits = credits;
            request.AlDebits = debits;
            request.AlGlAccounts = glAccounts;

            try
            {
                var response = await transactionInvoker.ExecuteAsync<TxValidateBudgetAdjustmentRequest, TxValidateBudgetAdjustmentResponse>(request);

                if (response == null)
                {
                    logger.Error(ValidationCTXFailedErrorMessage);
                    throw new ApplicationException(ValidationCTXFailedErrorMessage);
                }
                if (response.AlMessage == null)
                {
                    response.AlMessage = new List<string>();
                }
                return response.AlMessage;
            }
            catch (ApplicationException apex)
            {
                logger.Error(apex, "==> TxValidateBudgetAdjustmentRequest returned null <==");
                throw;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "==> TxValidateBudgetAdjustmentRequest session expired <==");
                throw;
            }
        }
    }
}
