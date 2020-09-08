// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
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
    /// Perform actions on draft budget adjustments.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class DocumentApprovalRepository : BaseColleagueRepository, IDocumentApprovalRepository
    {
        public static char _SM = Convert.ToChar(DynamicArray.SM);

        public DocumentApprovalRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get the document approval for the user.
        /// </summary>
        /// <param name="staffLoginId">Staff Login Id.</param>
        /// <returns>A document approval domain entity.</returns>
        public async Task<DocumentApproval> GetAsync(string staffLoginId)
        {
            // the staff login ID cannot be null or empty.
            if (string.IsNullOrEmpty(staffLoginId))
            {
                throw new ArgumentNullException("Staff Id cannot be null or empty.");
            }

            // Instantiate the return object.
            DocumentApproval documentApproval = new DocumentApproval();

            // Build the CTX request.
            var request = new TxGetApprovalDocumentsRequest()
            {
                AApprovalId = staffLoginId
            };
            
            // Execute the CTX request.
            var response = await transactionInvoker.ExecuteAsync<TxGetApprovalDocumentsRequest, TxGetApprovalDocumentsResponse>(request);
            
            // Build the document approval entity from the CTX response.
            if (response != null)
            {
                documentApproval.CanOverrideFundsAvailability = response.AFundsOverride;
                documentApproval.FundsAvailabilityOn = response.AFaRequired;

                if (response.AlApprovalDocuments != null && response.AlApprovalDocuments.Any())
                {
                    documentApproval.ApprovalDocuments = new List<ApprovalDocument>();
                    foreach (var approvalDocument in response.AlApprovalDocuments)
                    {
                        if (approvalDocument != null)
                        {
                            ApprovalDocument document = new ApprovalDocument();
                            document.Id = approvalDocument.AlDocId;
                            document.Number = approvalDocument.AlDocNumber;
                            document.DocumentType = approvalDocument.AlDocType;
                            document.VendorName = approvalDocument.AlDocVendorName;
                            document.Date = approvalDocument.AlDocDate.HasValue ? approvalDocument.AlDocDate.Value : DateTime.Now;
                            document.NetAmount = approvalDocument.AlDocNetAmt.HasValue ? approvalDocument.AlDocNetAmt.Value : 0m;
                            document.OverBudgetAmount = approvalDocument.AlDocOverAmt.HasValue ? approvalDocument.AlDocOverAmt.Value : 0m;
                            document.ChangeDate = approvalDocument.AlDocChangeDate;
                            document.ChangeTime = approvalDocument.AlDocChangeTime;
                            document.DocumentItems = new List<ApprovalItem>();
                            if (response.AlDocumentItems != null && response.AlDocumentItems.Any())
                            {
                                List<AlDocumentItems> documentItemsContracts = new List<AlDocumentItems>();
                                documentItemsContracts = response.AlDocumentItems.Where(x => x.AlItemDocType == approvalDocument.AlDocType && x.AlItemDocId == approvalDocument.AlDocId).ToList();

                                foreach (var itemContract in documentItemsContracts)
                                {
                                    ApprovalItem approvalDocumentItem = new ApprovalItem();
                                    approvalDocumentItem.DocumentType = itemContract.AlItemDocType;
                                    approvalDocumentItem.DocumentId = itemContract.AlItemDocId;
                                    approvalDocumentItem.ItemId = itemContract.AlItemId;
                                    approvalDocumentItem.ChangeDate = itemContract.AlItemChangeDate;
                                    approvalDocumentItem.ChangeTime = itemContract.AlItemChangeTime;
                                    document.DocumentItems.Add(approvalDocumentItem);
                                }                               
                            }
                            documentApproval.ApprovalDocuments.Add(document);
                        }
                    }
                }
            }
            return documentApproval;
        }

        /// <summary>
        /// Update approval information on a group of documents.
        /// </summary>
        /// <param name="staffLoginId">Staff Login Id.</param>
        /// <param name="approvalDocumentRequests">List of approval document requests.</param>
        /// <returns>A document approval response.</returns>
        public async Task<DocumentApprovalResponse> UpdateDocumentApprovalAsync(string staffLoginId, List<ApprovalDocumentRequest> approvalDocumentRequests)
        {
            // the staff login ID cannot be null or empty.
            if (string.IsNullOrEmpty(staffLoginId))
            {
                throw new ArgumentNullException("Staff Id cannot be null or empty.");
            }

            // there must be at least one document approval request.
            if (approvalDocumentRequests == null || !(approvalDocumentRequests.Any()))
            {
                throw new ArgumentNullException("There must be at least one document approval request.");
            }

            // determine if funds availability is turned on for GL numbers or for projects
            var glStruct = new Glstruct();
            glStruct = await DataReader.ReadRecordAsync<Glstruct>("ACCOUNT.PARAMETERS", "ACCT.STRUCTURE");
            if (glStruct == null)
            {
                // GLSTRUCT must exist for Colleague Financials to function properly
                throw new ConfigurationException("GL account structure is not defined.");
            }

            var paParms = new PaParms();
            paParms = await DataReader.ReadRecordAsync<PaParms>("CF.PARMS", "PA.PARMS");

            bool fundsAvailability = false;
            if (glStruct.AcctCheckAvailFunds == "Y")
            {
                fundsAvailability = true;
            }
            if (!fundsAvailability)
                {
                if (paParms != null)
                {
                    if (paParms.PapCheckAvailFunds == "Y")
                    {
                        fundsAvailability = true;
                    }
                }

            }
            // Instantiate the return objects.
            DocumentApprovalResponse documentApprovalResponse = new DocumentApprovalResponse();
            documentApprovalResponse.UpdatedApprovalDocumentResponses = new List<ApprovalDocumentResponse>();
            documentApprovalResponse.NotUpdatedApprovalDocumentResponses = new List<ApprovalDocumentResponse>();

            // Loop through the list of document approval requests, and build the contents of the CTX request
            List<string> documentTypes = new List<string>();
            List<string> documentIds = new List<string>();
            List<string> documentNumbers = new List<string>();
            List<string> documentNextApprovers = new List<string>();
            List<decimal?> documentOverAmounts = new List<decimal?>();
            List<string> documentTimestamps = new List<string>();
            List<string> documentItems = new List<string>();
            List<string> itemTimestamps = new List<string>();

            foreach (var approvalDocument in approvalDocumentRequests)
            {
                // if the document is marked to be approved by the user, add the information about
                // the document to the CTX request. Otherwise, add the document information to the 
                // response for non-updated documents.
                if (approvalDocument != null)
                {
                    if (approvalDocument.Approve == true)
                    {
                        // if the document does not have an over budget amount, or they have over budget amount and
                        // an override has been supplied or funds availability is turned off, add the document to the transaction.
                        if (approvalDocument.OverBudgetAmount == null || approvalDocument.OverBudgetAmount == 0 
                            || (approvalDocument.OverBudgetAmount > 0 && approvalDocument.OverrideBudget && fundsAvailability)
                            || !fundsAvailability)
                        {
                            documentTypes.Add(approvalDocument.DocumentType);
                            documentIds.Add(approvalDocument.DocumentId);
                            documentNumbers.Add(approvalDocument.DocumentNumber);
                            documentNextApprovers.Add(approvalDocument.NextApprover);
                            documentOverAmounts.Add(approvalDocument.OverBudgetAmount);
                            documentTimestamps.Add(approvalDocument.ChangeDate + "*" + approvalDocument.ChangeTime);
                            foreach (var documentItem in approvalDocument.DocumentItems)
                            {
                                documentItems.Add(documentItem.DocumentType + "*" + documentItem.DocumentId + "*" + documentItem.ItemId);
                                itemTimestamps.Add(documentItem.ChangeDate + "*" + documentItem.ChangeTime);
                            }
                        }
                        else
                        {
                            if (approvalDocument.OverBudgetAmount > 0 && !approvalDocument.OverrideBudget && fundsAvailability)
                            {
                                ApprovalDocumentResponse document = new ApprovalDocumentResponse();
                                document.DocumentType = approvalDocument.DocumentType;
                                document.DocumentId = approvalDocument.DocumentId;
                                document.DocumentNumber = approvalDocument.DocumentNumber;
                                List<string> messages = new List<string>();
                                string message = "The document requires an over budget amount override for approval.";
                                messages.Add(message);
                                document.DocumentMessages = messages;
                                documentApprovalResponse.NotUpdatedApprovalDocumentResponses.Add(document);
                            }
                        }
                    }
                    else
                    {
                        ApprovalDocumentResponse document = new ApprovalDocumentResponse();
                        document.DocumentType = approvalDocument.DocumentType;
                        document.DocumentId = approvalDocument.DocumentId;
                        document.DocumentNumber = approvalDocument.DocumentNumber;
                        List<string> messages = new List<string>();
                        string message = "The document is not marked for approval.";
                        messages.Add(message);
                        document.DocumentMessages = messages;
                        documentApprovalResponse.NotUpdatedApprovalDocumentResponses.Add(document);
                    }
                }
            }

            // Build the CTX request.
            var request = new TxUpdateDocumentApprovalsRequest()
            {
                AApprovalId = staffLoginId,
                AlDocTypes = documentTypes,
                AlDocIds = documentIds,
                AlDocNumbers = documentNumbers,
                AlDocNextAppr = documentNextApprovers,
                AlDocOverAmts = documentOverAmounts,
                AlDocTimestamps = documentTimestamps,
                AlDocItems = documentItems,
                AlItemTimestamps = itemTimestamps
            };

            // Execute the CTX request.
            var response = await transactionInvoker.ExecuteAsync<TxUpdateDocumentApprovalsRequest, TxUpdateDocumentApprovalsResponse>(request);

            // if the response is not null, build the list of approved document objects and 
            // the list of non-updated document objects.
            if (response != null)
            {
                // get the information about the documents that have been approved.
                if (response.AlUpdatedDocuments != null && response.AlUpdatedDocuments.Any())
                {
                    foreach (var approvedDocument in response.AlUpdatedDocuments)
                    {
                        if (approvedDocument != null)
                        {
                            ApprovalDocumentResponse document = new ApprovalDocumentResponse();
                            document.DocumentType = approvedDocument.AlApprDocTypes;
                            document.DocumentId = approvedDocument.AlApprDocIds;
                            document.DocumentNumber = approvedDocument.AlApprDocNumbers;
                            document.DocumentStatus = approvedDocument.AlApprDocStatuses;

                            // there may be multiple (sub-valued) messages for each approved document.
                            if (approvedDocument.AlApprDocMsgs != null)
                            {
                                string[] subvalues = approvedDocument.AlApprDocMsgs.Split(_SM);
                                List<string> messages = new List<string>();
                                messages.AddRange(subvalues);
                                document.DocumentMessages = messages;
                            }
                            documentApprovalResponse.UpdatedApprovalDocumentResponses.Add(document);
                        }
                    }
                }

                // get the information about the documents that have not been approved. The document status
                // is not populated for non-updated documents.
                if (response.AlNonUpdatedDocuments != null && response.AlNonUpdatedDocuments.Any())
                {
                    foreach (var notApprovedDocument in response.AlNonUpdatedDocuments)
                    {
                        if (notApprovedDocument != null)
                        {
                            ApprovalDocumentResponse document = new ApprovalDocumentResponse();
                            document.DocumentType = notApprovedDocument.AlNoupdtTypes;
                            document.DocumentId = notApprovedDocument.AlNoupdtIds;
                            document.DocumentNumber = notApprovedDocument.AlNoupdtNumbers;

                            // there may be multiple (sub-valued) messages for each non-updated document.
                            if (notApprovedDocument.AlNoupdtReasons != null)
                            {
                                string[] subvalues = notApprovedDocument.AlNoupdtReasons.Split(_SM);
                                List<string> messages = new List<string>();
                                messages.AddRange(subvalues);
                                document.DocumentMessages = messages;
                            }
                            documentApprovalResponse.NotUpdatedApprovalDocumentResponses.Add(document);
                        }
                    }
                }
            }
            return documentApprovalResponse;
        }
    }
}
