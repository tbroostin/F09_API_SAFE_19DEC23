// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.

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
        public class ApprovalInformationRecord
        {
            public string DocumentType;
            public string DocumentId;
            public string ApprovalId;
            public string ApprovalName;
            public DateTime? ApprovalDate;
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
            public List<ApprovalInformationRecord> DocumentApprovals;
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
                    },
                    DocumentApprovals = new List<ApprovalInformationRecord>()
                    {
                        new ApprovalInformationRecord
                        {
                            DocumentType = "REQ",
                            DocumentId = "1325",
                            ApprovalId = "TGL",
                            ApprovalName = "Teresa Approver Uppercase",
                            ApprovalDate = DateTime.Now
                        },
                        new ApprovalInformationRecord
                        {
                            DocumentType = "REQ",
                            DocumentId = "1325",
                            ApprovalId = "gtt",
                            ApprovalName = "gary thorne approver lowercase",
                            ApprovalDate = null
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
                approvalDocEntity.DocumentApprovers = new List<Approver>();
                foreach (var approval in document.DocumentApprovals)
                {
                    var approvalInformationEntity = new Approver(approval.ApprovalId);
                    approvalInformationEntity.SetApprovalName(approval.ApprovalName);
                    approvalInformationEntity.ApprovalDate = approval.ApprovalDate;
                    approvalDocEntity.DocumentApprovers.Add(approvalInformationEntity);
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


        public IEnumerable<ApprovedDocument> testApprovedDocumentEntities = new List<ApprovedDocument>()
        {
            new ApprovedDocument("1111")
            {
                Number = "0001111",
                DocumentType = "REQ",
                Status = "Not Approved",
                Date = DateTime.Today.AddDays(-10),
                VendorName = "Requisition Vendor Name",
                NetAmount = 1111m,
                DocumentApprovers = new List<Approver>()
                {
                    new Approver("TGL")
                    {
                        ApprovalDate = DateTime.Today.AddDays(-5)
                    },
                    new Approver("gtt")
                    {
                        ApprovalDate = null
                    }
                }
            },
            new ApprovedDocument("1112")
            {
                Number = "0001112",
                DocumentType = "REQ",
                Status = "Outstanding",
                Date = DateTime.Today.AddDays(-11),
                VendorName = "Requisition Vendor Name",
                NetAmount = 1112m,
                DocumentApprovers = new List<Approver>()
                {
                    new Approver("TGL")
                    {
                        ApprovalDate = DateTime.Today.AddDays(-6)
                    }
                }
            },
            new ApprovedDocument("2222")
            {
                Number = "P0002222",
                DocumentType = "PO",
                Status = "Not Approved",
                Date = DateTime.Today.AddDays(-5),
                VendorName = "Purchase Order Vendor Name",
                NetAmount = 2222m,
                DocumentApprovers = new List<Approver>()
                {
                    new Approver("TGL")
                    {
                        ApprovalDate = DateTime.Today.AddDays(-4)
                    },
                    new Approver("gtt")
                    {
                        ApprovalDate = null
                    }
                }
            },
            new ApprovedDocument("2223")
            {
                Number = "P0002223",
                DocumentType = "PO",
                Status = "Backordered",
                Date = DateTime.Today.AddDays(-4),
                VendorName = "Purchase Order Vendor Name",
                NetAmount = 2223m,
                DocumentApprovers = new List<Approver>()
                {
                    new Approver("TGL")
                    {
                        ApprovalDate = DateTime.Today.AddDays(-3)
                    },
                    new Approver("gtt")
                    {
                        ApprovalDate = null
                    }
                }
            },
            new ApprovedDocument("3333")
            {
                Number = "V0003333",
                DocumentType = "VOU",
                Status = "Paid",
                Date = DateTime.Today.AddDays(-2),
                VendorName = "Voucher Vendor Name",
                NetAmount = 3333m,
                DocumentApprovers = new List<Approver>()
                {
                    new Approver("TGL")
                    {
                        ApprovalDate = DateTime.Today.AddDays(-1)
                    },
                    new Approver("gtt")
                    {
                        ApprovalDate = null
                    }
                }
            }
        };
        public async Task<IEnumerable<ApprovedDocument>> QueryApprovedDocumentsAsync(string staffLoginId, ApprovedDocumentFilterCriteria filterCriteria)
        {

            foreach (var approvedDocument in testApprovedDocumentEntities)
            {
                foreach (var approverInformation in approvedDocument.DocumentApprovers)
                {
                    if (approverInformation.ApproverId == "TGL")
                    {
                        approverInformation.SetApprovalName("Teresa Approver Uppercase");
                    }
                    else if (approverInformation.ApproverId == "gtt")
                    {
                        approverInformation.SetApprovalName("gary thorne approver lowercase");
                    }
                }
            }
            return await Task.FromResult(testApprovedDocumentEntities);
        }
    }
}