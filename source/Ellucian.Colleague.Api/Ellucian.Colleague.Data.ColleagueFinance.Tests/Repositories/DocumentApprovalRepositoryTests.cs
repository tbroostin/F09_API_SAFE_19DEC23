// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Data.Colleague.Exceptions;
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

        // For approved documents history
        public TestDocumentApprovalRepository testDocumentApprovalRepository;
        public IEnumerable<ApprovedDocument> testApprovedDocuments;
        public IEnumerable<ApprovedDocument> actualApprovedDocuments;
        public ApprovedDocumentFilterCriteria filterCriteria;
        public TxGetApprovedDocumentsResponse txGetApprovedDocumentsResponse;

        // Create a document approval domain entity.
        public DocumentApproval documentApprovalRecordEntity = new DocumentApproval()
        {
            CanOverrideFundsAvailability = false,
            FundsAvailabilityOn = false,
            AllowReturns = false,
            ApprovalDocuments = new List<ApprovalDocument>()
            {
                 new ApprovalDocument()
                 {
                      Id = "1325",
                      Number = "P0001196",
                      DocumentType = "PO",
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
                              DocumentType = "PO",
                              DocumentId = "1325",
                              ItemId = "7237",
                              ChangeDate = "12345",
                              ChangeTime = "33300"
                          }
                      },
                      DocumentApprovers = new List<Approver>()
                      {
                          new Approver("TGL")
                          {
                              ApprovalDate = DateTime.Now
                          },
                          new Approver("gtt")
                          {
                              ApprovalDate = null
                          }
                      },
                      AssociatedDocuments = new List<AssociatedDocument>()
                      {
                          new AssociatedDocument()
                          {
                              Type = "REQ",
                              Id = "123",
                              Number = "0001234"
                          },
                          new AssociatedDocument()
                          {
                              Type = "REQ",
                              Id = "124",
                              Number = "0001235"
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

        // Create a list of approval document requests for testing the update method for document return.
        private List<ApprovalDocumentRequest> validApprovalReturnDocumentRequests = new List<ApprovalDocumentRequest>()
        {
            new ApprovalDocumentRequest()
            {
                Approve = false,
                DocumentType = "REQ",
                DocumentId = "1325",
                DocumentNumber = "0001196",
                Return = true,
                ReturnComments = "Need more info",
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
            }
        };

        // Create a list of approval document requests for testing the update method for document return.
        private List<ApprovalDocumentRequest> invalidApprovalReturnDocumentRequests = new List<ApprovalDocumentRequest>()
        {
            new ApprovalDocumentRequest()
            {
                Approve = false,
                DocumentType = "REQ",
                DocumentId = "1325",
                DocumentNumber = "0001196",
                Return = true,
                ReturnComments = "Need more info",
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
                Approve = false,
                DocumentType = "REQ",
                DocumentId = "1326",
                DocumentNumber = "0001197",
                Return = true,
                ReturnComments = "Need more info",
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
                Approve = true,
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
            testDocumentApprovalRepository = new TestDocumentApprovalRepository();
            actualRepository = new DocumentApprovalRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            documentApprovalRecord = new DocumentApproval();
            documentApprovalResponseRecord = new DocumentApprovalResponse();
            filterCriteria = new ApprovedDocumentFilterCriteria()
            {
                DocumentType = null,
                VendorIds = null,
                DocumentDateFrom = null,
                DocumentDateTo = null,
                ApprovalDateFrom = null,
                ApprovalDateTo = null
            };
            PopulateApproverName();
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
            txGetApprovedDocumentsResponse = null;
            testApprovedDocuments = null;
            actualApprovedDocuments = null;
            filterCriteria = null;
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
            Assert.AreEqual(documentApprovalRecord.AllowReturns, documentApprovalEntity.AllowReturns);
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

                // Confirm that the number of approval document entities is the same as the number of approval document returned by the CTX.
                Assert.AreEqual(documentApprovalEntity.ApprovalDocuments.Count(), documentApprovalRecord.ApprovalDocuments.Count());

                // Confirm the document items information.
                foreach (var item in approvalDocument.DocumentItems)
                {
                    var matchingApprovalItemRecord = matchingApprovalDocumentRecord.DocumentItems.FirstOrDefault(x => x.ItemId == item.ItemId
                        && x.DocumentType == item.DocumentType
                        && x.DocumentId == item.DocumentId
                        && x.ChangeDate == item.ChangeDate
                        && x.ChangeTime == item.ChangeTime);
                    Assert.IsNotNull(matchingApprovalItemRecord);
                }

                // Confirm that the number of approval document entities is the same as the number of approval document returned by the CTX.
                Assert.AreEqual(documentApprovalEntity.ApprovalDocuments.Count(), documentApprovalRecord.ApprovalDocuments.Count());

                // Confirm the approver entity information.
                foreach (var approverEntity in approvalDocument.DocumentApprovers)
                {
                    var matchingApprovalInformationRecord = matchingApprovalDocumentRecord.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverEntity.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(matchingApprovalInformationRecord);
                    Assert.AreEqual(matchingApprovalInformationRecord.ApprovalName, approverEntity.ApprovalName);

                    string approvalDateRecord = "";
                    string approvalDateEntity = "";
                    if (matchingApprovalInformationRecord.ApprovalDate.HasValue)
                    {
                        approvalDateRecord = Convert.ToDateTime(matchingApprovalInformationRecord.ApprovalDate).ToShortDateString();
                    }
                    if (approverEntity.ApprovalDate.HasValue)
                    {
                        approvalDateEntity = Convert.ToDateTime(approverEntity.ApprovalDate).ToShortDateString();
                    }
                    Assert.AreEqual(approvalDateRecord, approvalDateEntity);
                }

                // Confirm the associated document information.
                foreach (var associatedDocument in approvalDocument.AssociatedDocuments)
                {
                    var matchingAssociatedDocumentRecord = matchingApprovalDocumentRecord.AssociatedDocuments.FirstOrDefault(x => x.Type == associatedDocument.Type
                        && x.Id == associatedDocument.Id
                        && x.Number == associatedDocument.Number);
                    Assert.IsNotNull(matchingAssociatedDocumentRecord);
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
        public async Task GetAsync_EmptyArgument()
        {
            documentApprovalEntity = await actualRepository.GetAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueSessionExpiredException))]
        public async Task GetAsync_ExpiredColleagueCtx()
        {
            var request = new TxGetApprovalDocumentsRequest()
            {
                AApprovalId = "GTT"
            };
            var response = new TxGetApprovalDocumentsResponse();
            response.AFaRequired = false;
            response.AFundsOverride = false;
            response.AlApprovalDocuments = new List<AlApprovalDocuments>();
            response.AlAssociatedDocuments = new List<AlAssociatedDocuments>();
            response.AlDocumentApprovals = new List<AlDocumentApprovals>();
            response.AlDocumentItems = new List<AlDocumentItems>();
            transManagerMock.Setup(tio => tio.ExecuteAsync<TxGetApprovalDocumentsRequest, TxGetApprovalDocumentsResponse>(It.IsAny<TxGetApprovalDocumentsRequest>())).Throws(new ColleagueSessionExpiredException("timeout"));

            documentApprovalEntity = await actualRepository.GetAsync("GTT");
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

        [TestMethod]
        [ExpectedException(typeof(ColleagueSessionExpiredException))]
        public async Task UpdateDocumentApprovalAsync_ExpiredColleagueCtx()
        {
            var request = new TxUpdateDocumentApprovalsRequest()
            {
                AApprovalId = "GTT",
                AlDocTypes = new List<string>(),
                AlDocIds = new List<string>(),
                AlDocNumbers = new List<string>(),
                AlDocNextAppr = new List<string>(),
                AlDocOverAmts = new List<decimal?>(),
                AlDocTimestamps = new List<string>(),
                AlDocItems = new List<string>(),
                AlItemTimestamps = new List<string>()
            };
            var response = new TxUpdateDocumentApprovalsResponse();
            response.AlNonUpdatedDocuments = new List<AlNonUpdatedDocuments>();
            response.AlUpdatedDocuments = new List<AlUpdatedDocuments>();

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxUpdateDocumentApprovalsRequest, TxUpdateDocumentApprovalsResponse>(It.IsAny<TxUpdateDocumentApprovalsRequest>())).Throws(new ColleagueSessionExpiredException("timeout"));

            documentApprovalResponseEntity = await actualRepository.UpdateDocumentApprovalAsync("GTT", approvalDocumentRequests);
        }

        #region Approval returns tests

        [TestMethod]
        public async Task UpdateDocumentApprovalAsync_ReturnDocumentSuccess()
        {
            var purDefaults = new PurDefaults();
            purDefaults.PurApprAllowReturnFlag = "Y";
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.PurDefaults>("CF.PARMS", "PUR.DEFAULTS", It.IsAny<bool>()))
                .ReturnsAsync(purDefaults);

            // build a CTX response for the update endpoint
            updateDocumentApprovalResponse = new TxUpdateDocumentApprovalsResponse()
            {
                AlUpdatedDocuments = new List<AlUpdatedDocuments>()
                {
                    new AlUpdatedDocuments()
                    {
                        AlApprDocTypes = "REQ",
                        AlApprDocIds = "1325",
                        AlApprDocNumbers = "0001196",
                        AlApprDocStatuses = "NotApproved",
                        AlApprDocMsgs = ""
                    }
                },
                AlNonUpdatedDocuments = new List<AlNonUpdatedDocuments>()
            };

            transManagerMock.Setup(tm => tm.ExecuteAsync<TxUpdateDocumentApprovalsRequest, TxUpdateDocumentApprovalsResponse>(It.IsAny<TxUpdateDocumentApprovalsRequest>())).Returns(() =>
            {
                return Task.FromResult(updateDocumentApprovalResponse);
            });

            documentApprovalResponseRecord = await UpdateDocumentApprovalAsync("GTT", validApprovalReturnDocumentRequests);
            documentApprovalResponseEntity = await actualRepository.UpdateDocumentApprovalAsync("GTT", validApprovalReturnDocumentRequests);

            Assert.IsNotNull(documentApprovalResponseEntity.UpdatedApprovalDocumentResponses);
            Assert.IsNotNull(documentApprovalResponseEntity.NotUpdatedApprovalDocumentResponses);
            Assert.IsTrue(documentApprovalResponseEntity.UpdatedApprovalDocumentResponses.Count == 1);
            Assert.IsTrue(documentApprovalResponseEntity.NotUpdatedApprovalDocumentResponses.Count == 0);

            var updatedDocument = documentApprovalResponseEntity.UpdatedApprovalDocumentResponses.FirstOrDefault();
            var matchingUpdatedDocumentRecord = documentApprovalResponseRecord.UpdatedApprovalDocumentResponses.FirstOrDefault(x => x.DocumentType == updatedDocument.DocumentType
                && x.DocumentId == updatedDocument.DocumentId
                && x.DocumentNumber == updatedDocument.DocumentNumber);
            Assert.IsNotNull(matchingUpdatedDocumentRecord);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UpdateDocumentApprovalAsync_ReturnContainsMoreThanOneDocument()
        {
            documentApprovalResponseEntity = await actualRepository.UpdateDocumentApprovalAsync("GTT", invalidApprovalReturnDocumentRequests);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UpdateDocumentApprovalAsync_ReturnDocumentPurDefaultsIsNull()
        {
            documentApprovalResponseRecord = await UpdateDocumentApprovalAsync("GTT", validApprovalReturnDocumentRequests);
            documentApprovalResponseEntity = await actualRepository.UpdateDocumentApprovalAsync("GTT", validApprovalReturnDocumentRequests);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UpdateDocumentApprovalAsync_ReturnDocumentAllowReturnFlagIsFalse()
        {
            var purDefaults = new PurDefaults();
            purDefaults.PurApprAllowReturnFlag = "N";
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.PurDefaults>("CF.PARMS", "PUR.DEFAULTS", It.IsAny<bool>()))
                .ReturnsAsync(purDefaults);

            documentApprovalResponseRecord = await UpdateDocumentApprovalAsync("GTT", validApprovalReturnDocumentRequests);
            documentApprovalResponseEntity = await actualRepository.UpdateDocumentApprovalAsync("GTT", validApprovalReturnDocumentRequests);
        }

        #endregion

        #endregion

        #region QueryApprovedDocumentsAsync Tests

        [TestMethod]
        public async Task QueryApprovedDocumentsAsync_Success_NoFilter()
        {
            testApprovedDocuments = await testDocumentApprovalRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);
            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);

            Assert.AreEqual(testApprovedDocuments.Count(), actualApprovedDocuments.Count());

            foreach (var approvedDocument in actualApprovedDocuments)
            {
                var testMatchingApprovedDocument = testApprovedDocuments.FirstOrDefault(x => x.Id == approvedDocument.Id
                    && x.Number == approvedDocument.Number
                    && x.DocumentType == approvedDocument.DocumentType
                    && x.Date == approvedDocument.Date
                    && x.VendorName == approvedDocument.VendorName
                    && x.NetAmount == approvedDocument.NetAmount);
                Assert.IsNotNull(testMatchingApprovedDocument);

                // Confirm the approver entity information.
                foreach (var approverEntity in approvedDocument.DocumentApprovers)
                {
                    var testMatchingApprovedInformation = testMatchingApprovedDocument.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverEntity.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(testMatchingApprovedInformation);
                    Assert.AreEqual(testMatchingApprovedInformation.ApprovalName, approverEntity.ApprovalName);

                    string testApprovalDate = "";
                    string approvalDateRepo = "";
                    if (testMatchingApprovedInformation.ApprovalDate.HasValue)
                    {
                        testApprovalDate = Convert.ToDateTime(testMatchingApprovedInformation.ApprovalDate).ToShortDateString();
                    }
                    if (approverEntity.ApprovalDate.HasValue)
                    {
                        approvalDateRepo = Convert.ToDateTime(approverEntity.ApprovalDate).ToShortDateString();
                    }
                    Assert.AreEqual(testApprovalDate, approvalDateRepo);
                }
            }
        }

        [TestMethod]
        public async Task QueryApprovedDocumentsAsync_Success_FilterWithDocumentTypeReq()
        {
            filterCriteria.DocumentType = new List<string>() { "REQ" };
            testApprovedDocuments = await testDocumentApprovalRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);
            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);

            Assert.AreEqual(testApprovedDocuments.Count(), actualApprovedDocuments.Count());

            foreach (var approvedDocument in actualApprovedDocuments)
            {
                var testMatchingApprovedDocument = testApprovedDocuments.FirstOrDefault(x => x.Id == approvedDocument.Id
                    && x.Number == approvedDocument.Number
                    && x.DocumentType == approvedDocument.DocumentType
                    && x.Date == approvedDocument.Date
                    && x.VendorName == approvedDocument.VendorName
                    && x.NetAmount == approvedDocument.NetAmount);
                Assert.IsNotNull(testMatchingApprovedDocument);

                // Confirm the approver entity information.
                foreach (var approverEntity in approvedDocument.DocumentApprovers)
                {
                    var testMatchingApprovedInformation = testMatchingApprovedDocument.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverEntity.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(testMatchingApprovedInformation);
                    Assert.AreEqual(testMatchingApprovedInformation.ApprovalName, approverEntity.ApprovalName);

                    string testApprovalDate = "";
                    string approvalDateRepo = "";
                    if (testMatchingApprovedInformation.ApprovalDate.HasValue)
                    {
                        testApprovalDate = Convert.ToDateTime(testMatchingApprovedInformation.ApprovalDate).ToShortDateString();
                    }
                    if (approverEntity.ApprovalDate.HasValue)
                    {
                        approvalDateRepo = Convert.ToDateTime(approverEntity.ApprovalDate).ToShortDateString();
                    }
                    Assert.AreEqual(testApprovalDate, approvalDateRepo);
                }
            }
        }

        [TestMethod]
        public async Task QueryApprovedDocumentsAsync_Success_FilterWithDocumentTypePoAndVou()
        {
            filterCriteria.DocumentType = new List<string>() { "PO", "VOU" };
            testApprovedDocuments = await testDocumentApprovalRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);
            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);

            Assert.AreEqual(testApprovedDocuments.Count(), actualApprovedDocuments.Count());

            foreach (var approvedDocument in actualApprovedDocuments)
            {
                var testMatchingApprovedDocument = testApprovedDocuments.FirstOrDefault(x => x.Id == approvedDocument.Id
                    && x.Number == approvedDocument.Number
                    && x.DocumentType == approvedDocument.DocumentType
                    && x.Date == approvedDocument.Date
                    && x.VendorName == approvedDocument.VendorName
                    && x.NetAmount == approvedDocument.NetAmount);
                Assert.IsNotNull(testMatchingApprovedDocument);

                // Confirm the approver entity information.
                foreach (var approverEntity in approvedDocument.DocumentApprovers)
                {
                    var testMatchingApprovedInformation = testMatchingApprovedDocument.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverEntity.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(testMatchingApprovedInformation);
                    Assert.AreEqual(testMatchingApprovedInformation.ApprovalName, approverEntity.ApprovalName);

                    string testApprovalDate = "";
                    string approvalDateRepo = "";
                    if (testMatchingApprovedInformation.ApprovalDate.HasValue)
                    {
                        testApprovalDate = Convert.ToDateTime(testMatchingApprovedInformation.ApprovalDate).ToShortDateString();
                    }
                    if (approverEntity.ApprovalDate.HasValue)
                    {
                        approvalDateRepo = Convert.ToDateTime(approverEntity.ApprovalDate).ToShortDateString();
                    }
                    Assert.AreEqual(testApprovalDate, approvalDateRepo);
                }
            }
        }

        [TestMethod]
        public async Task QueryApprovedDocumentsAsync_Success_FilterWithDocumentFromDate()
        {
            filterCriteria.DocumentDateFrom = DateTime.Today.AddDays(-4);
            testApprovedDocuments = await testDocumentApprovalRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);
            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);

            Assert.AreEqual(testApprovedDocuments.Count(), actualApprovedDocuments.Count());

            foreach (var approvedDocument in actualApprovedDocuments)
            {
                var testMatchingApprovedDocument = testApprovedDocuments.FirstOrDefault(x => x.Id == approvedDocument.Id
                    && x.Number == approvedDocument.Number
                    && x.DocumentType == approvedDocument.DocumentType
                    && x.Date == approvedDocument.Date
                    && x.VendorName == approvedDocument.VendorName
                    && x.NetAmount == approvedDocument.NetAmount);
                Assert.IsNotNull(testMatchingApprovedDocument);

                // Confirm the approver entity information.
                foreach (var approverEntity in approvedDocument.DocumentApprovers)
                {
                    var testMatchingApprovedInformation = testMatchingApprovedDocument.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverEntity.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(testMatchingApprovedInformation);
                    Assert.AreEqual(testMatchingApprovedInformation.ApprovalName, approverEntity.ApprovalName);

                    string testApprovalDate = "";
                    string approvalDateRepo = "";
                    if (testMatchingApprovedInformation.ApprovalDate.HasValue)
                    {
                        testApprovalDate = Convert.ToDateTime(testMatchingApprovedInformation.ApprovalDate).ToShortDateString();
                    }
                    if (approverEntity.ApprovalDate.HasValue)
                    {
                        approvalDateRepo = Convert.ToDateTime(approverEntity.ApprovalDate).ToShortDateString();
                    }
                    Assert.AreEqual(testApprovalDate, approvalDateRepo);
                }
            }
        }

        [TestMethod]
        public async Task QueryApprovedDocumentsAsync_Success_FilterWithDocumentToDate()
        {
            filterCriteria.DocumentDateTo = DateTime.Today.AddDays(-10);
            testApprovedDocuments = await testDocumentApprovalRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);
            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);

            Assert.AreEqual(testApprovedDocuments.Count(), actualApprovedDocuments.Count());

            foreach (var approvedDocument in actualApprovedDocuments)
            {
                var testMatchingApprovedDocument = testApprovedDocuments.FirstOrDefault(x => x.Id == approvedDocument.Id
                    && x.Number == approvedDocument.Number
                    && x.DocumentType == approvedDocument.DocumentType
                    && x.Date == approvedDocument.Date
                    && x.VendorName == approvedDocument.VendorName
                    && x.NetAmount == approvedDocument.NetAmount);
                Assert.IsNotNull(testMatchingApprovedDocument);

                // Confirm the approver entity information.
                foreach (var approverEntity in approvedDocument.DocumentApprovers)
                {
                    var testMatchingApprovedInformation = testMatchingApprovedDocument.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverEntity.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(testMatchingApprovedInformation);
                    Assert.AreEqual(testMatchingApprovedInformation.ApprovalName, approverEntity.ApprovalName);

                    string testApprovalDate = "";
                    string approvalDateRepo = "";
                    if (testMatchingApprovedInformation.ApprovalDate.HasValue)
                    {
                        testApprovalDate = Convert.ToDateTime(testMatchingApprovedInformation.ApprovalDate).ToShortDateString();
                    }
                    if (approverEntity.ApprovalDate.HasValue)
                    {
                        approvalDateRepo = Convert.ToDateTime(approverEntity.ApprovalDate).ToShortDateString();
                    }
                    Assert.AreEqual(testApprovalDate, approvalDateRepo);
                }
            }
        }

        [TestMethod]
        public async Task QueryApprovedDocumentsAsync_Success_FilterWithApprovalFromDate()
        {
            filterCriteria.ApprovalDateFrom = DateTime.Today.AddDays(-4);
            testApprovedDocuments = await testDocumentApprovalRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);
            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);

            Assert.AreEqual(testApprovedDocuments.Count(), actualApprovedDocuments.Count());

            foreach (var approvedDocument in actualApprovedDocuments)
            {
                var testMatchingApprovedDocument = testApprovedDocuments.FirstOrDefault(x => x.Id == approvedDocument.Id
                    && x.Number == approvedDocument.Number
                    && x.DocumentType == approvedDocument.DocumentType
                    && x.Date == approvedDocument.Date
                    && x.VendorName == approvedDocument.VendorName
                    && x.NetAmount == approvedDocument.NetAmount);
                Assert.IsNotNull(testMatchingApprovedDocument);

                // Confirm the approver entity information.
                foreach (var approverEntity in approvedDocument.DocumentApprovers)
                {
                    var testMatchingApprovedInformation = testMatchingApprovedDocument.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverEntity.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(testMatchingApprovedInformation);
                    Assert.AreEqual(testMatchingApprovedInformation.ApprovalName, approverEntity.ApprovalName);

                    string testApprovalDate = "";
                    string approvalDateRepo = "";
                    if (testMatchingApprovedInformation.ApprovalDate.HasValue)
                    {
                        testApprovalDate = Convert.ToDateTime(testMatchingApprovedInformation.ApprovalDate).ToShortDateString();
                    }
                    if (approverEntity.ApprovalDate.HasValue)
                    {
                        approvalDateRepo = Convert.ToDateTime(approverEntity.ApprovalDate).ToShortDateString();
                    }
                    Assert.AreEqual(testApprovalDate, approvalDateRepo);
                }
            }
        }

        [TestMethod]
        public async Task QueryApprovedDocumentsAsync_Success_FilterWithApprovalToDate()
        {
            filterCriteria.ApprovalDateTo = DateTime.Today.AddDays(-1);
            testApprovedDocuments = await testDocumentApprovalRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);
            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);

            Assert.AreEqual(testApprovedDocuments.Count(), actualApprovedDocuments.Count());

            foreach (var approvedDocument in actualApprovedDocuments)
            {
                var testMatchingApprovedDocument = testApprovedDocuments.FirstOrDefault(x => x.Id == approvedDocument.Id
                    && x.Number == approvedDocument.Number
                    && x.DocumentType == approvedDocument.DocumentType
                    && x.Date == approvedDocument.Date
                    && x.VendorName == approvedDocument.VendorName
                    && x.NetAmount == approvedDocument.NetAmount);
                Assert.IsNotNull(testMatchingApprovedDocument);

                // Confirm the approver entity information.
                foreach (var approverEntity in approvedDocument.DocumentApprovers)
                {
                    var testMatchingApprovedInformation = testMatchingApprovedDocument.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverEntity.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(testMatchingApprovedInformation);
                    Assert.AreEqual(testMatchingApprovedInformation.ApprovalName, approverEntity.ApprovalName);

                    string testApprovalDate = "";
                    string approvalDateRepo = "";
                    if (testMatchingApprovedInformation.ApprovalDate.HasValue)
                    {
                        testApprovalDate = Convert.ToDateTime(testMatchingApprovedInformation.ApprovalDate).ToShortDateString();
                    }
                    if (approverEntity.ApprovalDate.HasValue)
                    {
                        approvalDateRepo = Convert.ToDateTime(approverEntity.ApprovalDate).ToShortDateString();
                    }
                    Assert.AreEqual(testApprovalDate, approvalDateRepo);
                }
            }
        }

        [TestMethod]
        public async Task QueryApprovedDocumentsAsync_Success_FilterWithApprovalToDateOutofRange()
        {
            filterCriteria.ApprovalDateTo = DateTime.Today.AddDays(-20);
            testApprovedDocuments = await testDocumentApprovalRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);
            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync("TGL", filterCriteria);

            Assert.AreEqual(testApprovedDocuments.Count(), actualApprovedDocuments.Count());

            foreach (var approvedDocument in actualApprovedDocuments)
            {
                var testMatchingApprovedDocument = testApprovedDocuments.FirstOrDefault(x => x.Id == approvedDocument.Id
                    && x.Number == approvedDocument.Number
                    && x.DocumentType == approvedDocument.DocumentType
                    && x.Date == approvedDocument.Date
                    && x.VendorName == approvedDocument.VendorName
                    && x.NetAmount == approvedDocument.NetAmount);
                Assert.IsNotNull(testMatchingApprovedDocument);

                // Confirm the approver entity information.
                foreach (var approverEntity in approvedDocument.DocumentApprovers)
                {
                    var testMatchingApprovedInformation = testMatchingApprovedDocument.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverEntity.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(testMatchingApprovedInformation);
                    Assert.AreEqual(testMatchingApprovedInformation.ApprovalName, approverEntity.ApprovalName);

                    string testApprovalDate = "";
                    string approvalDateRepo = "";
                    if (testMatchingApprovedInformation.ApprovalDate.HasValue)
                    {
                        testApprovalDate = Convert.ToDateTime(testMatchingApprovedInformation.ApprovalDate).ToShortDateString();
                    }
                    if (approverEntity.ApprovalDate.HasValue)
                    {
                        approvalDateRepo = Convert.ToDateTime(approverEntity.ApprovalDate).ToShortDateString();
                    }
                    Assert.AreEqual(testApprovalDate, approvalDateRepo);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryApprovedDocumentsAsync_NullStaffIdArgument()
        {
            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync(null, filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryApprovedDocumentsAsync_EmptyStaffIdArgument()
        {
            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync("", filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueSessionExpiredException))]
        public async Task QueryApprovedDocumentsAsync_ExpiredColleagueCtx()
        {
            var request = new TxGetApprovedDocumentsRequest()
            {
                AApprovalId = "GTT"
            };
            var response = new TxGetApprovedDocumentsResponse();
            response.AlApprovedDocuments = new List<AlApprovedDocuments>();
            response.AlDocumentApprovers = new List<AlDocumentApprovers>();

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxGetApprovedDocumentsRequest, TxGetApprovedDocumentsResponse>(It.IsAny<TxGetApprovedDocumentsRequest>())).Throws(new ColleagueSessionExpiredException("timeout"));

            actualApprovedDocuments = await actualRepository.QueryApprovedDocumentsAsync("GTT", filterCriteria);
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

            // build a CTX response for the GET endpoint
            getDocumentApprovalResponse = new TxGetApprovalDocumentsResponse()
            {
                AFundsOverride = false,
                AlApprovalDocuments = new List<AlApprovalDocuments>()
                {
                       new AlApprovalDocuments()
                       {
                           AlDocId = "1325",
                           AlDocNumber = "P0001196",
                           AlDocType = "PO",
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
                          AlItemDocType = "PO",
                          AlItemDocId = "1325",
                          AlItemId = "7237",
                          AlItemChangeDate = "12345",
                          AlItemChangeTime = "33300"
                      }
                },
                AlDocumentApprovals = new List<AlDocumentApprovals>()
                {
                     new AlDocumentApprovals()
                     {
                         AlApprDocType = "PO",
                         AlApprDocId = "1325",
                         AlApprId = "TGL",
                         AlApprName = "Teresa Approver Uppercase",
                         AlApprDate = DateTime.Now
                     },
                     new AlDocumentApprovals()
                     {
                         AlApprDocType = "PO",
                         AlApprDocId = "1325",
                         AlApprId = "gtt",
                         AlApprName = "gary thorne approver lowercase",
                         AlApprDate = null
                     }
                },
                AlAssociatedDocuments = new List<AlAssociatedDocuments>()
                {
                    new AlAssociatedDocuments()
                    {
                         AlAssocDocDocType = "PO",
                         AlAssocDocDocId = "1325",
                         AlAssocDocType = "REQ",
                         AlAssocDocId = "123",
                         AlAssocDocNumber = "0001234"
                    },
                    new AlAssociatedDocuments()
                    {
                         AlAssocDocDocType = "PO",
                         AlAssocDocDocId = "1325",
                         AlAssocDocType = "REQ",
                         AlAssocDocId = "124",
                         AlAssocDocNumber = "0001235"
                    }
                }
            };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxGetApprovalDocumentsRequest, TxGetApprovalDocumentsResponse>(It.IsAny<TxGetApprovalDocumentsRequest>())).Returns(() =>
            {
                return Task.FromResult(getDocumentApprovalResponse);
            });

            // build a CTX response for the update endpoint
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

            txGetApprovedDocumentsResponse = new TxGetApprovedDocumentsResponse()
            {
                AlApprovedDocuments = new List<AlApprovedDocuments>()
                {
                    new AlApprovedDocuments()
                    {
                        AlDocId = "1111",
                        AlDocNumber = "0001111",
                        AlDocType = "REQ",

                        AlDocVendorName = "Requisition Vendor Name",
                        AlDocDate = DateTime.Today.AddDays(-10),
                        AlDocNetAmt = 1111m,

                    },
                    new AlApprovedDocuments()
                    {
                        AlDocId = "1112",
                        AlDocNumber = "0001112",
                        AlDocType = "REQ",
                        AlDocStatus = "Outstanding",
                        AlDocDate = DateTime.Today.AddDays(-11),
                        AlDocVendorName = "Requisition Vendor Name",
                        AlDocNetAmt = 1112m,
                    },
                    new AlApprovedDocuments()
                    {
                        AlDocId = "2222",
                        AlDocNumber = "P0002222",
                        AlDocType = "PO",
                        AlDocStatus = "Not Approved",
                        AlDocDate = DateTime.Today.AddDays(-5),
                        AlDocVendorName = "Purchase Order Vendor Name",
                        AlDocNetAmt = 2222m,
                    },
                    new AlApprovedDocuments()
                    {
                        AlDocId = "2223",
                        AlDocNumber = "P0002223",
                        AlDocType = "PO",
                        AlDocStatus = "Backordered",
                        AlDocDate = DateTime.Today.AddDays(-4),
                        AlDocVendorName = "Purchase Order Vendor Name",
                        AlDocNetAmt = 2223m,
                    },
                    new AlApprovedDocuments()
                    {
                        AlDocId = "3333",
                        AlDocNumber = "V0003333",
                        AlDocType = "VOU",
                        AlDocStatus = "Paid",
                        AlDocDate = DateTime.Today.AddDays(-2),
                        AlDocVendorName = "Voucher Vendor Name",
                        AlDocNetAmt = 3333m,
                    }
                },
                AlDocumentApprovers = new List<AlDocumentApprovers>()
                {
                    new AlDocumentApprovers()
                    {
                        AlApprDocType = "REQ",
                        AlApprDocId = "1111",
                        AlApprId = "TGL",
                        AlApprName = "Teresa Approver Uppercase",
                        AlApprDate = DateTime.Today.AddDays(-5)
                    },
                    new AlDocumentApprovers()
                    {
                        AlApprDocType = "REQ",
                        AlApprDocId = "1111",
                        AlApprId = "gtt",
                        AlApprName = "gary thorne approver lowercase",
                        AlApprDate = null
                    },
                    new AlDocumentApprovers()
                    {
                        AlApprDocType = "REQ",
                        AlApprDocId = "1112",
                        AlApprId = "TGL",
                        AlApprName = "Teresa Approver Uppercase",
                        AlApprDate = DateTime.Today.AddDays(-6)
                    },
                    new AlDocumentApprovers()
                    {
                        AlApprDocType = "PO",
                        AlApprDocId = "2222",
                        AlApprId = "TGL",
                        AlApprName = "Teresa Approver Uppercase",
                        AlApprDate = DateTime.Today.AddDays(-4)
                    },
                    new AlDocumentApprovers()
                    {
                        AlApprDocType = "PO",
                        AlApprDocId = "2222",
                        AlApprId = "gtt",
                        AlApprName = "gary thorne approver lowercase",
                        AlApprDate = null
                    },
                    new AlDocumentApprovers()
                    {
                        AlApprDocType = "PO",
                        AlApprDocId = "2223",
                        AlApprId = "TGL",
                        AlApprName = "Teresa Approver Uppercase",
                        AlApprDate = DateTime.Today.AddDays(-3)
                    },
                    new AlDocumentApprovers()
                    {
                        AlApprDocType = "PO",
                        AlApprDocId = "2223",
                        AlApprId = "gtt",
                        AlApprName = "gary thorne approver lowercase",
                        AlApprDate = null
                    },
                    new AlDocumentApprovers()
                    {
                        AlApprDocType = "VOU",
                        AlApprDocId = "3333",
                        AlApprId = "gtt",
                        AlApprName = "gary thorne approver lowercase",
                        AlApprDate = null
                    },
                    new AlDocumentApprovers()
                    {
                        AlApprDocType = "VOU",
                        AlApprDocId = "3333",
                        AlApprId = "TGL",
                        AlApprName = "Teresa Approver Uppercase",
                        AlApprDate = DateTime.Today.AddDays(-1)
                    },
                }
            };

            transManagerMock.Setup(tad => tad.ExecuteAsync<TxGetApprovedDocumentsRequest, TxGetApprovedDocumentsResponse>(It.IsAny<TxGetApprovedDocumentsRequest>())).Returns(() =>
            {
                return Task.FromResult(txGetApprovedDocumentsResponse);
            });
        }

        // Return a document approval domain entity.
        private async Task<DocumentApproval> GetAsync(string id)
        {
            return await Task.FromResult(documentApprovalRecordEntity);
        }

        // Return a document approval response domain entity.
        private async Task<DocumentApprovalResponse> UpdateDocumentApprovalAsync(string id, List<ApprovalDocumentRequest> approvalDocumentRequests)
        {
            return await Task.FromResult(documentApprovalResponseRecordEntity);
        }

        // Return a document approval response domain entity for a request requiring a budget override that does not have one.
        private async Task<DocumentApprovalResponse> UpdateDocumentApprovalRequiringOverrideAsync(string id, List<ApprovalDocumentRequest> approvalDocumentRequests)
        {
            return await Task.FromResult(documentApprovalResponseRequiringOverrideRecordEntity);
        }

        private void PopulateApproverName()
        {
            foreach (var approvalDocument in documentApprovalRecordEntity.ApprovalDocuments)
            {
                foreach (var approverInformation in approvalDocument.DocumentApprovers)
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
        }

        #endregion
    }
}