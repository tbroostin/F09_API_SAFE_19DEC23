// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for approvals
    /// </summary>
    [RegisterType]
    public class ApprovalRepository : BaseColleagueRepository, IApprovalRepository
    {
        private readonly string _colleagueTimeZone;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider">Cache provider</param>
        /// <param name="transactionFactory">Transaction factory</param>
        /// <param name="logger">System logger</param>
        public ApprovalRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        /// <summary>
        /// Get an approval document
        /// </summary>
        /// <param name="id">Approval document ID</param>
        /// <returns>Approval document</returns>
        public ApprovalDocument GetApprovalDocument(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Approval document ID is required for document retrieval.");
            }

            ApprovalDocuments apprDoc = DataReader.ReadRecord<ApprovalDocuments>(id);
            if (apprDoc == null)
            {
                throw new KeyNotFoundException("Invalid ID for approval document: " + id);
            }

            return new ApprovalDocument(id, apprDoc.ApdText.Split(new string[] {"\r\n"}, StringSplitOptions.None));
        }

        /// <summary>
        /// Create an approval document
        /// </summary>
        /// <param name="approvalDocument">Approval document</param>
        /// <returns>Updated approval document</returns>
        public ApprovalDocument CreateApprovalDocument(ApprovalDocument approvalDocument)
        {
            if (approvalDocument == null)
            {
                throw new ArgumentNullException("approvalDocument", "Approval Document cannot be null");
            }

            // Execute the CreateApprovalDocument transaction
            var request = new CreateApprovalDocumentRequest()
            {
                ApprovalText = approvalDocument.Text.ToList(),
                PersonId = approvalDocument.PersonId
            };

            CreateApprovalDocumentResponse response = transactionInvoker.Execute<CreateApprovalDocumentRequest, CreateApprovalDocumentResponse>(request);

            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                if (!string.IsNullOrEmpty(response.ApprovalDocumentId))
                {
                    // Populate the ApprovalDocument.Id with the value returned by the transaction
                    approvalDocument.Id = response.ApprovalDocumentId;
                }
                else
                {
                    throw new KeyNotFoundException("Colleague transaction did not return a valid approval document ID");
                }
            }
            else
            {
                logger.Error(response.ErrorMessage);
                throw new InvalidOperationException(response.ErrorMessage);
            }
            return approvalDocument;
        }

        /// <summary>
        /// Get an approval response
        /// </summary>
        /// <param name="id">Approval response ID</param>
        /// <returns>
        /// Approval response
        /// </returns>
        /// <exception cref="System.ArgumentNullException">id;Approval document ID is required for document retrieval.</exception>
        /// <exception cref="System.ArgumentException">Invalid ID for approval document:  + id;id</exception>
        public ApprovalResponse GetApprovalResponse(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Approval document ID is required for document retrieval.");
            }

            ApprDocResponses apprResponse = DataReader.ReadRecord<ApprDocResponses>(id);
            if (apprResponse == null)
            {
                throw new KeyNotFoundException("Invalid ID for approval document: " + id);
            }

            DateTimeOffset apprDateTime = apprResponse.ApdrTime.ToPointInTimeDateTimeOffset(apprResponse.ApdrDate, _colleagueTimeZone).GetValueOrDefault();
            var result = new ApprovalResponse(id, apprResponse.ApdrApprovalDocument, apprResponse.ApdrPersonId, apprResponse.ApdrUserid, apprDateTime,
                (apprResponse.ApdrApproved == "Y"));
            
            return result;
        }

        /// <summary>
        /// Create an approval response
        /// </summary>
        /// <param name="approvalResponse">Approval response</param>
        /// <returns>Updated approval response</returns>
        public ApprovalResponse CreateApprovalResponse(ApprovalResponse approvalResponse)
        {
            if (approvalResponse == null)
            {
                throw new ArgumentNullException("approvalResponse", "Approval Response cannot be null");
            }

            //Execute the CreateApprovalResponse transaction
            var request = new CreateApprovalResponseRequest()
            {
                ApprovalDocumentId = approvalResponse.DocumentId,
                ApprovalUserid = approvalResponse.UserId,
                PersonId = approvalResponse.PersonId,
                ApprovalDate = approvalResponse.Received.Date,
                ApprovalTime = approvalResponse.Received.ToLocalDateTime(_colleagueTimeZone),
                IsApproved = approvalResponse.IsApproved,
            };
            CreateApprovalResponseResponse response = transactionInvoker.Execute<CreateApprovalResponseRequest, CreateApprovalResponseResponse>(request);
            
            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                if (!string.IsNullOrEmpty(response.ApprovalResponseId))
                {
                    // Populate the ApprovalResponse.Id with the value returned by the transaction
                    approvalResponse.Id = response.ApprovalResponseId;
                }
                else
                {
                    throw new KeyNotFoundException("Colleague transaction did not return a valid approval response ID");
                }
            }
            else
            {
                logger.Error(response.ErrorMessage);
                throw new InvalidOperationException(response.ErrorMessage);
            }
            return approvalResponse;
        }
    }
}
