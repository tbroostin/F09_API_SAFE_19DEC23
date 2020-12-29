// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestDocumentApprovalRepository : IDocumentApprovalRepository
    {
        // Mock an ApprovalItem class
        public class ApprovalItemRecord
        {
            public string DocumentType;
            public string DocumentId;
            public string ItemId;
            public string ChangeDate;
            public string ChangeTime;
        }

        // Mock an ApprovalDocument class
        public class ApprovalDocumentRecord
        {
            public string Id;
            public string Number;
            public string DocumentType;
            public DateTime Date;
            public string VendorName;
            public decimal NetAmount;
            public decimal OverBudgetAmount;
            public string ChangeDate;
            public string ChangeTime;
            public List<ApprovalItemRecord> DocumentItems;
        }

        // Mock a DocumentApproval class
        public class DocumentApprovalRecord
        {
            public bool CanOverrideFundsAvailability;
            public List<ApprovalDocumentRecord> ApprovalDocuments;
        }

        // Create a DocumentApproval record
        public DocumentApprovalRecord documentApprovalRecord = new DocumentApprovalRecord()
        {
            CanOverrideFundsAvailability = false,
            ApprovalDocuments = new List<ApprovalDocumentRecord>()
            {
                new ApprovalDocumentRecord()
                {
                    Id = "1325",
                    Number = "0001196",
                    DocumentType = "REQ",
                    Date = new DateTime(),
                    VendorName = "Susty",
                    NetAmount = 100m,
                    ChangeDate = "12345",
                    ChangeTime = "33333",
                    DocumentItems = new List<ApprovalItemRecord>()
                    {
                        new ApprovalItemRecord()
                        {
                            DocumentType = "REQ",
                            DocumentId = "1325",
                            ItemId = "7237",
                            ChangeDate = "12345",
                            ChangeTime = "33333"
                        }
                    }
                }
            }
        };

        // Mock an ApprovedDocumentResponse class
        public class ApprovedDocumentResponseRecord
        {
            public string DocumentType;
            public string DocumentId;
            public string DocumentNumber;
            public string DocumentStatus;
            public List<string> DocumentMessages;
        }
        
        // Mock a DocumentApprovalResponse class
        public class DocumentApprovalResponseRecord
        {
            public List<ApprovedDocumentResponseRecord> UpdatedApprovalDocumentResponses;
            public List<ApprovedDocumentResponseRecord> NotUpdatedApprovalDocumentResponses;
        }
        
        // Create a DocumentApprovalResponse record
        public DocumentApprovalResponseRecord documentApprovalResponseRecord = new DocumentApprovalResponseRecord()
        {
            UpdatedApprovalDocumentResponses = new List<ApprovedDocumentResponseRecord>()
            {
                new ApprovedDocumentResponseRecord()
                {
                    DocumentType = "REQ",
                    DocumentId = "1325",
                    DocumentNumber = "0001196",
                    DocumentStatus = "Outstanding",
                    DocumentMessages = new List<string>() { "No additional approvals are required." }
                }
            }
        };

        // Mock the repository GetAsync method that returns the DocumentApproval record
        public Task<DocumentApproval> GetAsync(string staffLoginId)
        {
            var returnDocumentApprovalRecord = documentApprovalRecord;

            var documentApprovalEntity = new DocumentApproval();
            documentApprovalEntity.CanOverrideFundsAvailability = false;
            documentApprovalEntity.ApprovalDocuments = new List<ApprovalDocument>();
            foreach (var document in returnDocumentApprovalRecord.ApprovalDocuments)
            {
                var approvalDocEntity = new ApprovalDocument();
                approvalDocEntity.Id = document.Id;
                approvalDocEntity.Number = document.Number; 
                approvalDocEntity.DocumentType = document.DocumentType;
                approvalDocEntity.Date = document.Date;
                approvalDocEntity.VendorName = document.VendorName;
                approvalDocEntity.NetAmount = document.NetAmount;
                approvalDocEntity.ChangeDate = document.ChangeDate;
                approvalDocEntity.ChangeTime = document.ChangeTime;
                approvalDocEntity.DocumentItems = new List<ApprovalItem>();
                foreach (var item in document.DocumentItems)
                {
                    var approvalItemEntity = new ApprovalItem();
                    approvalItemEntity.DocumentType = item.DocumentType;
                    approvalItemEntity.DocumentId = item.DocumentId;
                    approvalItemEntity.ItemId = item.ItemId;
                    approvalItemEntity.ChangeDate = item.ChangeDate;
                    approvalItemEntity.ChangeTime = item.ChangeTime;
                    approvalDocEntity.DocumentItems.Add(approvalItemEntity);
                }
                documentApprovalEntity.ApprovalDocuments.Add(approvalDocEntity);
            }
            return Task.FromResult(documentApprovalEntity);
        }

        // Mock the repository UpdateDocumentApprovalAsync method that returns the DocumentApprovalResponse record
        public Task<DocumentApprovalResponse> UpdateDocumentApprovalAsync(string staffLoginId, List<ApprovalDocumentRequest> approvalDocumentRequests)
        {
            var returnDocumentApprovalResponseRecord = documentApprovalResponseRecord;

            var documentApprovalResponseEntity = new DocumentApprovalResponse();
            documentApprovalResponseEntity.UpdatedApprovalDocumentResponses = new List<ApprovalDocumentResponse>();
            foreach (var response in returnDocumentApprovalResponseRecord.UpdatedApprovalDocumentResponses)
            {
                var approvalDocumentResponseEntity = new ApprovalDocumentResponse();
                approvalDocumentResponseEntity.DocumentType = response.DocumentType;
                approvalDocumentResponseEntity.DocumentId = response.DocumentId;
                approvalDocumentResponseEntity.DocumentNumber = response.DocumentNumber;
                approvalDocumentResponseEntity.DocumentStatus = response.DocumentStatus;
                approvalDocumentResponseEntity.DocumentMessages = new List<string>();
                approvalDocumentResponseEntity.DocumentMessages.AddRange(response.DocumentMessages);
                documentApprovalResponseEntity.UpdatedApprovalDocumentResponses.Add(approvalDocumentResponseEntity);
            }
            return Task.FromResult(documentApprovalResponseEntity);
        }
    }
}