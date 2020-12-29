// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class DocumentApprovalRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup
        //declare document approval repository and domain entity variables.
        private DocumentApprovalRepository actualRepository;
        private DocumentApproval documentApprovalRecord;
        private DocumentApproval documentApprovalEntity;
        private DocumentApprovalResponse documentApprovalResponseRecord;
        private DocumentApprovalResponse documentApprovalResponseEntity;

        // declare data contract variables.
        private Glstruct glStruct;
        private PaParms paParms;

        // declare CTX response variables.
        private TxGetApprovalDocumentsResponse getDocumentApprovalResponse;
        private TxUpdateDocumentApprovalsResponse updateDocumentApprovalResponse;

        // Create a document approval domain entity.
        public DocumentApproval documentApprovalRecordEntity = new DocumentApproval()
        {
            CanOverrideFundsAvailability = false,
            FundsAvailabilityOn = false,
            ApprovalDocuments = new List<ApprovalDocument>()
            {
                 new ApprovalDocument()
                 {
                      Id = "1325",
                      Number = "0001196",
                      DocumentType = "REQ",
                      Date = DateTime.Today,
                      VendorName = "Susty",
                      NetAmount = 10m,
                      OverBudgetAmount = 0m,
                      ChangeDate = "12345",
                      ChangeTime = "33333",
                      DocumentItems = new List<ApprovalItem>()
                      {
                          new ApprovalItem()
                          {
                              DocumentType = "REQ",
                              DocumentId = "1325",
                              ItemId = "7237",
                              ChangeDate = "12345",
                              ChangeTime = "33300"
                          }
                      }
                 }
            }
        };

        // Create a document approval response domain entity.
        public DocumentApprovalResponse documentApprovalResponseRecordEntity = new DocumentApprovalResponse()
        {
            UpdatedApprovalDocumentResponses = new List<ApprovalDocumentResponse>()
            {
                 new ApprovalDocumentResponse()
                 {
                      DocumentType = "REQ",
                      DocumentId = "1325",
                      DocumentNumber = "0001196",
                      DocumentStatus = "Outstanding"
                 }
            },
            NotUpdatedApprovalDocumentResponses = new List<ApprovalDocumentResponse>()
            {
                new ApprovalDocumentResponse()
                {
                    DocumentType = "REQ",
                    DocumentId = "1326",
                    DocumentNumber = "0001197",
                    DocumentMessages = new List<string>() {"XYZ is an invalid next approver." }
                },
                new ApprovalDocumentResponse()
                {
                    DocumentType = "REQ",
                    DocumentId = "1327",
                    DocumentNumber = "0001198",
                    DocumentMessages = new List<string>() { "The document is not marked for approval." }
                }
            }
        };

        // Create a document approval response domain entity for a request that requires an override budget
        // flag but doesn't have it.
        public DocumentApprovalResponse documentApprovalResponseRequiringOverrideRecordEntity = new DocumentApprovalResponse()
        {
            NotUpdatedApprovalDocumentResponses = new List<ApprovalDocumentResponse>()
            {
                new ApprovalDocumentResponse()
                {
                    DocumentType = "PO",
                    DocumentId = "1330",
                    DocumentNumber = "P0001200",
                    DocumentMessages = new List<string>() { "The document requires an over budget amount override for approval." }
                }
            }
        };

        // Create a list of approval document requests for testing the update method.
        private List<ApprovalDocumentRequest> approvalDocumentRequests = new List<ApprovalDocumentRequest>()
        {
            new ApprovalDocumentRequest()
            {
                Approve = true,
                DocumentType = "REQ",
                DocumentId = "1325",
                DocumentNumber = "0001196",
                ChangeDate = "12345",
                ChangeTime = "33333",
                DocumentItems = new List<ApprovalItem>()
                {
                    new ApprovalItem()
                    {
                        DocumentType = "REQ",
                        DocumentId = "1325",
                        ItemId = "7237",
                        ChangeDate = "12345",
                        ChangeTime = "33333"
                    }
                 }
            },
            new ApprovalDocumentRequest()
            {
                Approve = true,
                DocumentType = "REQ",
                DocumentId = "1326",
                DocumentNumber = "0001197",
                ChangeDate = "12345",
                ChangeTime = "33555",
                DocumentItems = new List<ApprovalItem>()
                {
                    new ApprovalItem()
                    {
                        DocumentType = "REQ",
                        DocumentId = "1326",
                        ItemId = "7238",
                        ChangeDate = "12345",
                        ChangeTime = "33555"
                    }
                 }
            },
            new ApprovalDocumentRequest()
            {
                Approve = false,
                DocumentType = "REQ",
                DocumentId = "1327",
                DocumentNumber = "0001198",
                ChangeDate = "12345",
                ChangeTime = "33777",
                DocumentItems = new List<ApprovalItem>()
                {
                    new ApprovalItem()
                    {
                        DocumentType = "REQ",
                        DocumentId = "1327",
                        ItemId = "7239",
                        ChangeDate = "12345",
                        ChangeTime = "33777"
                    }
                 }
            }
        };

        // Create an approval document request for testing the update method where there is an overbudget amount
        // but not override budget boolean.
        private List<ApprovalDocumentRequest> approvalDocumentRequestsRequiringOverride = new List<ApprovalDocumentRequest>()
        {
            new ApprovalDocumentRequest()
            {
                Approve = true,
                OverrideBudget = false,
                DocumentType = "PO",
                DocumentId = "1330",
                DocumentNumber = "P0001200",
                OverBudgetAmount = 190,
                ChangeDate = "12345",
                ChangeTime = "33333",
                DocumentItems = new List<ApprovalItem>()
                {
                    new ApprovalItem()
                    {
                        DocumentType = "PO",
                        DocumentId = "1330",
                        ItemId = "7237",
                        ChangeDate = "12345",
                        ChangeTime = "33333"
                    }
                 }
            }
        };

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();
            actualRepository = new DocumentApprovalRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            documentApprovalRecord = new DocumentApproval();
            documentApprovalResponseRecord = new DocumentApprovalResponse();
            InitializeMockStatements();
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
            documentApprovalRecord = null;
            documentApprovalEntity = null;
            documentApprovalResponseRecord = null;
            documentApprovalResponseEntity = null;
            getDocumentApprovalResponse = null;
            updateDocumentApprovalResponse = null;
        }
        #endregion



        #region GetAsync Tests

        [TestMethod]
        public async Task GetAsync_Success()
        {
            documentApprovalRecord = await GetAsync("1");
            documentApprovalEntity = await actualRepository.GetAsync("GTT");

            Assert.AreEqual(documentApprovalRecord.CanOverrideFundsAvailability, documentApprovalEntity.CanOverrideFundsAvailability);
            Assert.AreEqual(documentApprovalRecord.FundsAvailabilityOn, documentApprovalEntity.FundsAvailabilityOn);
            foreach (var approvalDocument in documentApprovalEntity.ApprovalDocuments)
            {
                var matchingApprovalDocumentRecord = documentApprovalRecord.ApprovalDocuments.FirstOrDefault(x => x.Id == approvalDocument.Id
                    && x.Number == approvalDocument.Number
                    && x.DocumentType == approvalDocument.DocumentType
                    && x.Date == approvalDocument.Date
                    && x.VendorName == approvalDocument.VendorName
                    && x.NetAmount == approvalDocument.NetAmount
                    && x.OverBudgetAmount == approvalDocument.OverBudgetAmount
                    && x.ChangeDate == approvalDocument.ChangeDate
                    && x.ChangeTime == approvalDocument.ChangeTime);
                Assert.IsNotNull(matchingApprovalDocumentRecord);
                foreach (var item in approvalDocument.DocumentItems)
                {
                    var matchingApprovalItemRecord = matchingApprovalDocumentRecord.DocumentItems.FirstOrDefault(x => x.ItemId == item.ItemId
                        && x.DocumentType == item.DocumentType
                        && x.DocumentId == item.DocumentId
                        && x.ChangeDate == item.ChangeDate
                        && x.ChangeTime == item.ChangeTime);
                    Assert.IsNotNull(matchingApprovalItemRecord);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_NullArgument()
        {
            documentApprovalEntity = await actualRepository.GetAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentAsync_EmptyArgument()
        {
            documentApprovalEntity = await actualRepository.GetAsync("");
        }
        #endregion

        #region UpdateDocumentApprovalAsync Tests
        [TestMethod]
        public async Task UpdateDocumentApprovalAsync_Success()
        {
            documentApprovalResponseRecord = await UpdateDocumentApprovalAsync("GTT", approvalDocumentRequests);
            documentApprovalResponseEntity = await actualRepository.UpdateDocumentApprovalAsync("GTT", approvalDocumentRequests);

            foreach (var updatedDocument in documentApprovalResponseEntity.UpdatedApprovalDocumentResponses)
            {
                var matchingUpdatedDocumentRecord = documentApprovalResponseRecord.UpdatedApprovalDocumentResponses.FirstOrDefault(x => x.DocumentType == updatedDocument.DocumentType
                    && x.DocumentId == updatedDocument.DocumentId
                    && x.DocumentNumber == updatedDocument.DocumentNumber
                    && x.DocumentStatus == updatedDocument.DocumentStatus);
                Assert.IsNotNull(matchingUpdatedDocumentRecord);
            }
            foreach (var notupdatedDocument in documentApprovalResponseEntity.NotUpdatedApprovalDocumentResponses)
            {
                var matchingNotupdatedDocumentRecord = documentApprovalResponseRecord.NotUpdatedApprovalDocumentResponses.FirstOrDefault(x => x.DocumentType == notupdatedDocument.DocumentType
                    && x.DocumentId == notupdatedDocument.DocumentId
                    && x.DocumentNumber == notupdatedDocument.DocumentNumber
                    && x.DocumentStatus == notupdatedDocument.DocumentStatus);
                Assert.IsNotNull(matchingNotupdatedDocumentRecord);
            }
        }

        [TestMethod]
        public async Task UpdateDocumentApprovalAsync_NoOverrideBudgetBooleanWhenNeeded()
        {
            glStruct.AcctCheckAvailFunds = "Y";
            updateDocumentApprovalResponse = new TxUpdateDocumentApprovalsResponse()
            {
                AlUpdatedDocuments = new List<AlUpdatedDocuments>(),
                AlNonUpdatedDocuments = new List<AlNonUpdatedDocuments>()
            };
            documentApprovalResponseRecord = await UpdateDocumentApprovalRequiringOverrideAsync("GTT", approvalDocumentRequestsRequiringOverride);
            documentApprovalResponseEntity = await actualRepository.UpdateDocumentApprovalAsync("GTT", approvalDocumentRequestsRequiringOverride);

            foreach (var updatedDocument in documentApprovalResponseEntity.UpdatedApprovalDocumentResponses)
            {
                var matchingUpdatedDocumentRecord = documentApprovalResponseRecord.UpdatedApprovalDocumentResponses.FirstOrDefault(x => x.DocumentType == updatedDocument.DocumentType
                    && x.DocumentId == updatedDocument.DocumentId
                    && x.DocumentNumber == updatedDocument.DocumentNumber
                    && x.DocumentStatus == updatedDocument.DocumentStatus);
                Assert.IsNotNull(matchingUpdatedDocumentRecord);
            }
            foreach (var notupdatedDocument in documentApprovalResponseEntity.NotUpdatedApprovalDocumentResponses)
            {
                var matchingNotupdatedDocumentRecord = documentApprovalResponseRecord.NotUpdatedApprovalDocumentResponses.FirstOrDefault(x => x.DocumentType == notupdatedDocument.DocumentType
                    && x.DocumentId == notupdatedDocument.DocumentId
                    && x.DocumentNumber == notupdatedDocument.DocumentNumber
                    && x.DocumentStatus == notupdatedDocument.DocumentStatus);
                Assert.IsNotNull(matchingNotupdatedDocumentRecord);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateDocumentApprovalAsync_NullStaffLoginId()
        {
            documentApprovalResponseEntity = await actualRepository.UpdateDocumentApprovalAsync(null, approvalDocumentRequests);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateDocumentApprovalAsync_EmptyStaffLoginId()
        {
            documentApprovalResponseEntity = await actualRepository.UpdateDocumentApprovalAsync("", approvalDocumentRequests);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateDocumentApprovalAsync_NullApprovalDocumentRequests()
        {
            documentApprovalResponseEntity = await actualRepository.UpdateDocumentApprovalAsync("GTT", null);
        }
        #endregion

        #region Private methods

        private void InitializeMockStatements()
        {
            glStruct = new Glstruct();
            glStruct.AcctCheckAvailFunds = "N";
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glstruct>("ACCOUNT.PARAMETERS", "ACCT.STRUCTURE", It.IsAny<bool>()))
                .ReturnsAsync(glStruct);

            paParms = new PaParms();
            paParms.PapCheckAvailFunds = "N";
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.PaParms>("CF.PARMS", "PA.PARMS", It.IsAny<bool>()))
                .ReturnsAsync(paParms);

            getDocumentApprovalResponse = new TxGetApprovalDocumentsResponse()
            {
                AFundsOverride = false,
                AlApprovalDocuments = new List<AlApprovalDocuments>()
                  {
                       new AlApprovalDocuments()
                       {
                           AlDocId = "1325",
                           AlDocNumber = "0001196",
                           AlDocType = "REQ",
                           AlDocVendorName = "Susty",
                           AlDocDate = DateTime.Today,
                           AlDocNetAmt = 10m,
                           AlDocOverAmt = 0m,
                           AlDocChangeDate = "12345",
                           AlDocChangeTime = "33333"
                       }
                  },
                AlDocumentItems = new List<AlDocumentItems>()
                  {
                      new AlDocumentItems()
                      {
                          AlItemDocType = "REQ",
                          AlItemDocId = "1325",
                          AlItemId = "7237",
                          AlItemChangeDate = "12345",
                          AlItemChangeTime = "33300"
                      }
                  }
            };
            transManagerMock.Setup(tio => tio.ExecuteAsync<TxGetApprovalDocumentsRequest, TxGetApprovalDocumentsResponse>(It.IsAny<TxGetApprovalDocumentsRequest>())).Returns(() =>
            {
                return Task.FromResult(getDocumentApprovalResponse);
            });

            updateDocumentApprovalResponse = new TxUpdateDocumentApprovalsResponse()
            {
                AlUpdatedDocuments = new List<AlUpdatedDocuments>()
                 {
                     new AlUpdatedDocuments()
                     {
                         AlApprDocTypes = "REQ",
                         AlApprDocIds = "1325",
                         AlApprDocNumbers = "0001196",
                         AlApprDocStatuses = "Outstanding",
                         AlApprDocMsgs = ""
                     }
                 },
                AlNonUpdatedDocuments = new List<AlNonUpdatedDocuments>()
                {
                    new AlNonUpdatedDocuments()
                    {
                        AlNoupdtTypes = "REQ",
                        AlNoupdtIds = "1326",
                        AlNoupdtNumbers = "0001197",
                        AlNoupdtReasons = "XYZ is an invalid next approver."
                    },
                    new AlNonUpdatedDocuments()
                    {
                        AlNoupdtTypes = "REQ",
                        AlNoupdtIds = "1327",
                        AlNoupdtNumbers = "0001198",
                        AlNoupdtReasons = "The document is not marked for approval."
                    }
                }

            };
            transManagerMock.Setup(tm => tm.ExecuteAsync<TxUpdateDocumentApprovalsRequest, TxUpdateDocumentApprovalsResponse>(It.IsAny<TxUpdateDocumentApprovalsRequest>())).Returns(() =>
            {
                return Task.FromResult(updateDocumentApprovalResponse);
            });
        }

        // Return a document approval domain entity.
        private async Task<DocumentApproval> GetAsync(string id)
        {
            //var documentApprovalEntity = await Task.Run(() => documentApprovalRecordEntity.FirstOrDefault(x => x.Recordkey == id));
            return await Task.FromResult(documentApprovalRecordEntity);
        }

        // Return a document approval response domain entity.
        private async Task<DocumentApprovalResponse> UpdateDocumentApprovalAsync(string id, List<ApprovalDocumentRequest> approvalDocumentRequests )
        {
            //var documentApprovalEntity = await Task.Run(() => documentApprovalRecordEntity.FirstOrDefault(x => x.Recordkey == id));
            return await Task.FromResult(documentApprovalResponseRecordEntity);
        }

        // Return a document approval response domain entity for a request requiring a budget override that does not have one.
        private async Task<DocumentApprovalResponse> UpdateDocumentApprovalRequiringOverrideAsync(string id, List<ApprovalDocumentRequest> approvalDocumentRequests)
        {
            //var documentApprovalEntity = await Task.Run(() => documentApprovalRecordEntity.FirstOrDefault(x => x.Recordkey == id));
            return await Task.FromResult(documentApprovalResponseRequiringOverrideRecordEntity);
        }
        #endregion
    }
}