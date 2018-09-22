// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// Perform actions on draft budget adjustments.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class DraftBudgetAdjustmentsRepository : BaseColleagueRepository, IDraftBudgetAdjustmentsRepository
    {
        public DraftBudgetAdjustmentsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Create or update a new draft budget adjustment.
        /// </summary>
        /// <param name="draftBudgetAdjustmentInput">Draft budget adjustment.</param>
        /// <returns>Draft budget adjustment</returns>
        public async Task<DraftBudgetAdjustment> SaveAsync(DraftBudgetAdjustment draftBudgetAdjustmentInput)
        {
            // loop through the adjustment lines, if there are any, and set the GL numbers, from amounts, and 
            // and to amounts that will be passed into the CTX.
            var glNumbers = new List<string>();
            var fromAmountStrings = new List<string>();
            var toAmountStrings = new List<string>();
            if (draftBudgetAdjustmentInput.AdjustmentLines != null)
            {
                foreach (var adjustmentLine in draftBudgetAdjustmentInput.AdjustmentLines)
                {
                    if (!(string.IsNullOrEmpty(adjustmentLine.GlNumber)))
                    {
                        var internalGlNumber = GlAccountUtility.ConvertToInternalFormat(adjustmentLine.GlNumber);
                        glNumbers.Add(internalGlNumber);
                    }
                    else
                    {
                        glNumbers.Add(string.Empty);
                    }

                    fromAmountStrings.Add(adjustmentLine.FromAmount == 0 ? "" : adjustmentLine.FromAmount.ToString());
                    toAmountStrings.Add(adjustmentLine.ToAmount == 0 ? "" : adjustmentLine.ToAmount.ToString());
                }
            }

            var nextApproverIds = new List<string>();
            if (draftBudgetAdjustmentInput.NextApprovers != null && draftBudgetAdjustmentInput.NextApprovers.Any())
            {
                foreach (var nextApprover in draftBudgetAdjustmentInput.NextApprovers)
                {
                    if (nextApprover != null)
                    {
                        nextApproverIds.Add(nextApprover.NextApproverId.ToUpperInvariant());
                    }
                }
            }

            // Determine the action for the CTX from the existence of a draft budget ID in the incoming draft budget
            // adjustment. Also, normally on GLBE, the FROM amount is the DEBIT and TO the CREDIT. However, we "swap the columns" since
            // we're dealing with budget amounts.
            string updateMode = "A";
            if (!string.IsNullOrEmpty(draftBudgetAdjustmentInput.Id))
            {
                updateMode = "U";
            }
            var request = new TxUpdateDraftBudgetAdjustmentRequest()
            {
                AAction = updateMode,
                ADraftBudgetEntryId = draftBudgetAdjustmentInput.Id,
                AlGlAccounts = glNumbers,
                AlDebits = toAmountStrings,
                AlCredits = fromAmountStrings,
                AReason = draftBudgetAdjustmentInput.Reason,
                AAuthor = draftBudgetAdjustmentInput.Initiator,
                AlComments = new List<string>() { draftBudgetAdjustmentInput.Comments },
                AlNextApproverIds = nextApproverIds,
                ATrDate = DateTime.SpecifyKind(draftBudgetAdjustmentInput.TransactionDate, DateTimeKind.Unspecified)
            };

            var response = await transactionInvoker.ExecuteAsync<TxUpdateDraftBudgetAdjustmentRequest, TxUpdateDraftBudgetAdjustmentResponse>(request);

            // If the adjustment was created and posted then create a new instance of the budget adjustment, otherwise add the error message supplied by the CTX.
            DraftBudgetAdjustment draftBudgetAdjustmentOutput = null;
            if (!string.IsNullOrEmpty(response.ADraftBudgetEntryId))
            {
                draftBudgetAdjustmentOutput = new DraftBudgetAdjustment(draftBudgetAdjustmentInput.Reason);
                draftBudgetAdjustmentOutput.AdjustmentLines = draftBudgetAdjustmentInput.AdjustmentLines;
                draftBudgetAdjustmentOutput.TransactionDate = draftBudgetAdjustmentInput.TransactionDate;
                draftBudgetAdjustmentOutput.Id = response.ADraftBudgetEntryId;
                draftBudgetAdjustmentOutput.Initiator = draftBudgetAdjustmentInput.Initiator;
                draftBudgetAdjustmentOutput.Comments = draftBudgetAdjustmentInput.Comments;

                draftBudgetAdjustmentOutput.NextApprovers = new List<NextApprover>();
                if (draftBudgetAdjustmentInput.NextApprovers != null && draftBudgetAdjustmentInput.NextApprovers.Any())
                {
                    foreach (var nextApprover in draftBudgetAdjustmentInput.NextApprovers)
                    {
                        if (nextApprover != null)
                        {
                            draftBudgetAdjustmentOutput.NextApprovers.Add(nextApprover);
                        }
                    }
                }
            }
            else
            {
                draftBudgetAdjustmentInput.ErrorMessages = response.AlErrorMessages;
                draftBudgetAdjustmentOutput = draftBudgetAdjustmentInput;
            }

            return draftBudgetAdjustmentOutput;
        }

        /// <summary>
        /// Get the draft budget adjustment for the specified ID.
        /// </summary>
        /// <param name="id">A draft budget adjustment ID.</param>
        /// <returns>A draft budget adjustment domain entity.</returns>
        public async Task<DraftBudgetAdjustment> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var draftBudgetAdjustmentRecord = await DataReader.ReadRecordAsync<DraftBudgetEntries>(id);
            if (draftBudgetAdjustmentRecord == null)
            {
                throw new KeyNotFoundException(string.Format("Draft budget adjustment record {0} does not exist.", id));
            }

            // Populate the domain entity.
            var draftBudgetAdjustment = new DraftBudgetAdjustment(draftBudgetAdjustmentRecord.DbgteReason);
            draftBudgetAdjustment.Id = draftBudgetAdjustmentRecord.Recordkey;
            draftBudgetAdjustment.PersonId = draftBudgetAdjustmentRecord.DraftBudgetEntriesAddopr;
            draftBudgetAdjustment.Initiator = draftBudgetAdjustmentRecord.DbgteAuthor;
            // The transaction date is required but check that it has data.
            if (draftBudgetAdjustmentRecord.DbgteTrDate.HasValue)
            {
                draftBudgetAdjustment.TransactionDate = draftBudgetAdjustmentRecord.DbgteTrDate.Value;
            }
            var comments = string.Empty;
            if (!string.IsNullOrEmpty(draftBudgetAdjustmentRecord.DbgteComments))
            {
                comments = CommentsUtility.ConvertCommentsToParagraphs(draftBudgetAdjustmentRecord.DbgteComments);
            }
            draftBudgetAdjustment.Comments = comments;

            // Loop through the adjustment lines, if there are any.
            if (draftBudgetAdjustmentRecord.DbgteDataEntityAssociation != null && draftBudgetAdjustmentRecord.DbgteDataEntityAssociation.Any())
            {
                draftBudgetAdjustment.AdjustmentLines = new List<DraftAdjustmentLine>();
                foreach (var associationMember in draftBudgetAdjustmentRecord.DbgteDataEntityAssociation)
                {
                    if (associationMember != null)
                    {
                        var adjustmentLine = new DraftAdjustmentLine();
                        adjustmentLine.GlNumber = associationMember.DbgteGlNoAssocMember;
                        // Normally (i.e. on GLBE) the FROM amount is the DEBIT and TO is the CREDIT. However, 
                        // we "swap the columns" since we're dealing with budget amounts.
                        adjustmentLine.FromAmount = associationMember.DbgteCreditAssocMember.HasValue ? associationMember.DbgteCreditAssocMember.Value : 0m;
                        adjustmentLine.ToAmount = associationMember.DbgteDebitAssocMember.HasValue ? associationMember.DbgteDebitAssocMember.Value : 0m;
                        draftBudgetAdjustment.AdjustmentLines.Add(adjustmentLine);
                    }
                }
            }

            draftBudgetAdjustment.NextApprovers = new List<NextApprover>();
            // If there are any next approvers, add them and get their names from their OPERS records.
            if (draftBudgetAdjustmentRecord.DbgteNextApprovalIds != null && draftBudgetAdjustmentRecord.DbgteNextApprovalIds.Any())
            {
                var operIds = new List<string>();
                operIds.AddRange(draftBudgetAdjustmentRecord.DbgteNextApprovalIds);
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
                            NextApprover nextApproverEntity = new NextApprover(oper.Recordkey);
                            nextApproverEntity.SetNextApproverName(oper.SysUserName);
                            draftBudgetAdjustment.NextApprovers.Add(nextApproverEntity);
                        }
                    }
                }
            }

            return draftBudgetAdjustment;
        }

        /// <summary>
        /// Deletes the draft budget adjustment for the specified id.
        /// </summary>
        /// <param name="id">A draft budget adjustment ID.</param>
        /// <returns>Nothing.</returns>
        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            TxDeleteDraftBudgetAdjustmentRequest request = new TxDeleteDraftBudgetAdjustmentRequest()
            {
                ADraftBeId = id
            };
            TxDeleteDraftBudgetAdjustmentResponse response = await transactionInvoker.ExecuteAsync<TxDeleteDraftBudgetAdjustmentRequest, TxDeleteDraftBudgetAdjustmentResponse>(request);

            if (response.AErrorCode == "MissingRecord")
            {
                logger.Error(response.AErrorMessage);
                throw new KeyNotFoundException(string.Format("Unable to delete record: no record was found with the specified id: {0}.", id));
            }
            if (!string.IsNullOrEmpty(response.AErrorCode))
            {
                logger.Error(response.AErrorMessage);
                throw new ApplicationException(string.Format("Unable to delete record with the specified id: {0}.", id));
            }
        }
    }
}
