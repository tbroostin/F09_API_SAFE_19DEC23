// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class DocumentApprovalServiceTests
    {
        #region Initialize and Cleanup
        public Mock<IAdapterRegistry> documentApprovalAdapterRegistryMock;
        public Mock<ILogger> loggerMock;

        private DocumentApprovalService documentApprovalService;
        private DocumentApprovalService noStaffLoginservice;
        private DocumentApprovalService serviceForNoPermission;
        private DocumentApprovalService expiredSessionService;

        private Mock<IDocumentApprovalRepository> repositoryMock = new Mock<IDocumentApprovalRepository>();
        private Mock<IDocumentApprovalRepository> expiredRepositoryMock = new Mock<IDocumentApprovalRepository>();

        private Mock<IStaffRepository> staffRepositoryMock = new Mock<IStaffRepository>();
        private Mock<IStaffRepository> noStaffLoginIdStaffRepositoryMock = new Mock<IStaffRepository>();

        private TestDocumentApprovalRepository testDocumentApprovalRepository;
        private Mock<IDocumentApprovalRepository> testDocumentApprovalRepositoryNoPermissionMock;
        private Mock<IDocumentApprovalRepository> testDocumentApprovalRepositoryNullDomainMock;
        private Domain.ColleagueFinance.Entities.DocumentApproval documentApprovalEntity;

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;

        // Name of view document approval permission.
        private Domain.Entities.Permission permissionViewDocumentApproval;

        // Name of role that is assigned to DocumentApprovalUser in the General Ledger Current User class.
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(335, "View Document Approval");

        // Definition of two users used in the service tests; one with the view permission and one with no permissions.
        private GeneralLedgerCurrentUser.DocumentApprovalUser documentApprovalUser = new GeneralLedgerCurrentUser.DocumentApprovalUser();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionsUser = new GeneralLedgerCurrentUser.UserFactoryNone();

        private List<Domain.ColleagueFinance.Entities.ApprovalDocumentRequest> approvalDocumentRequests;
        private Dtos.ColleagueFinance.DocumentApprovalRequest documentApprovalRequest;

        public Domain.ColleagueFinance.Entities.ApprovedDocumentFilterCriteria entityFilterCriteria;
        public Dtos.ColleagueFinance.ApprovedDocumentFilterCriteria dtoFilterCriteria;

        [TestInitialize]
        public void Initialize()
        {
            // Create permission domain entities for view.
            permissionViewDocumentApproval = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewDocumentApproval);

            // Assign the view document approval permission to the role that is assigned to the user (userFactory) defined in 
            // the GeneralLedgerCurrentUser class.
            glUserRoleViewPermissions.AddPermission(permissionViewDocumentApproval);

            // Mock the Document Approval repository.
            repositoryMock = new Mock<IDocumentApprovalRepository>();
            expiredRepositoryMock = new Mock<IDocumentApprovalRepository>();

            // Mock the role repository for the role that has the view permissions.
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleViewPermissions });
            roleRepository = roleRepositoryMock.Object;

            // Mock the document approval repositories; one for a successful Get and one for user with no permissions.
            testDocumentApprovalRepository = new TestDocumentApprovalRepository();
            testDocumentApprovalRepositoryNoPermissionMock = new Mock<IDocumentApprovalRepository>();
            testDocumentApprovalRepositoryNullDomainMock = new Mock<IDocumentApprovalRepository>();

            documentApprovalEntity = testDocumentApprovalRepository.GetAsync("GTT").Result;

            testDocumentApprovalRepositoryNoPermissionMock.Setup(y => y.GetAsync(It.IsAny<string>())).ReturnsAsync(documentApprovalEntity);
            testDocumentApprovalRepositoryNullDomainMock.Setup(y => y.GetAsync(It.IsAny<string>())).ReturnsAsync(null as Domain.ColleagueFinance.Entities.DocumentApproval);

            // Mock the logger
            loggerMock = new Mock<ILogger>();

            // Mock the staff login ID result from the staff repository.
            staffRepositoryMock.Setup(srm => srm.GetStaffLoginIdForPersonAsync(It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult("GTT");
            });

            // Mock a repository that returns a Colleague Session Expired Exception for the three service methods.
            expiredRepositoryMock.Setup(srm => srm.GetAsync(It.IsAny<string>())).Returns(() =>
            {
                throw new ColleagueSessionExpiredException("timeout");
            });

            expiredRepositoryMock.Setup(srm => srm.QueryApprovedDocumentsAsync(It.IsAny<string>(), It.IsAny<Ellucian.Colleague.Domain.ColleagueFinance.Entities.ApprovedDocumentFilterCriteria>())).Returns(() =>
             {
                 throw new ColleagueSessionExpiredException("timeout");
             });

            expiredRepositoryMock.Setup(srm => srm.UpdateDocumentApprovalAsync(It.IsAny<string>(), It.IsAny<List<Ellucian.Colleague.Domain.ColleagueFinance.Entities.ApprovalDocumentRequest>>())).Returns(() =>
             {
                 throw new ColleagueSessionExpiredException("timeout");
             });

            // Mock a null staff login ID result from the staff repository.
            noStaffLoginIdStaffRepositoryMock.Setup(srm => srm.GetStaffLoginIdForPersonAsync(It.IsAny<string>())).Throws<ApplicationException>();

            // Define and mock adapters
            documentApprovalAdapterRegistryMock = new Mock<IAdapterRegistry>();
            var documentApprovalEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.DocumentApproval, Dtos.ColleagueFinance.DocumentApproval>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.DocumentApproval, Dtos.ColleagueFinance.DocumentApproval>()).Returns(documentApprovalEntityToDtoAdapter);

            var approvalDocumentEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.ApprovalDocument, Dtos.ColleagueFinance.ApprovalDocument>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.ApprovalDocument, Dtos.ColleagueFinance.ApprovalDocument>()).Returns(approvalDocumentEntityToDtoAdapter);

            var approvalItemEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.ApprovalItem, Dtos.ColleagueFinance.ApprovalItem>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.ApprovalItem, Dtos.ColleagueFinance.ApprovalItem>()).Returns(approvalItemEntityToDtoAdapter);

            var approvalInformationEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>()).Returns(approvalInformationEntityToDtoAdapter);

            var associatedDocumentEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.AssociatedDocument, Dtos.ColleagueFinance.AssociatedDocument>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.AssociatedDocument, Dtos.ColleagueFinance.AssociatedDocument>()).Returns(associatedDocumentEntityToDtoAdapter);

            var documentApprovalResponseEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.DocumentApprovalResponse, Dtos.ColleagueFinance.DocumentApprovalResponse>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.DocumentApprovalResponse, Dtos.ColleagueFinance.DocumentApprovalResponse>()).Returns(documentApprovalResponseEntityToDtoAdapter);

            var approvalDocumentResponseEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.ApprovalDocumentResponse, Dtos.ColleagueFinance.ApprovalDocumentResponse>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.ApprovalDocumentResponse, Dtos.ColleagueFinance.ApprovalDocumentResponse>()).Returns(approvalDocumentResponseEntityToDtoAdapter);

            var documentApprovalRequestDtoToEntityAdapter = new AutoMapperAdapter<Dtos.ColleagueFinance.DocumentApprovalRequest, Domain.ColleagueFinance.Entities.DocumentApprovalRequest>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Dtos.ColleagueFinance.DocumentApprovalRequest, Domain.ColleagueFinance.Entities.DocumentApprovalRequest>()).Returns(documentApprovalRequestDtoToEntityAdapter);

            var approvalDocumentRequestDtoToEntityAdapter = new AutoMapperAdapter<Dtos.ColleagueFinance.ApprovalDocumentRequest, Domain.ColleagueFinance.Entities.ApprovalDocumentRequest>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Dtos.ColleagueFinance.ApprovalDocumentRequest, Domain.ColleagueFinance.Entities.ApprovalDocumentRequest>()).Returns(approvalDocumentRequestDtoToEntityAdapter);

            var approvalItemDtoToEntityAdapter = new AutoMapperAdapter<Dtos.ColleagueFinance.ApprovalItem, Domain.ColleagueFinance.Entities.ApprovalItem>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Dtos.ColleagueFinance.ApprovalItem, Domain.ColleagueFinance.Entities.ApprovalItem>()).Returns(approvalItemDtoToEntityAdapter);

            var approvedDocumentEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.ApprovedDocument, Dtos.ColleagueFinance.ApprovedDocument>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.ApprovedDocument, Dtos.ColleagueFinance.ApprovedDocument>()).Returns(approvedDocumentEntityToDtoAdapter);

            var approvedDocumentFilterCriteriaDtoToEntityAdapter = new AutoMapperAdapter<Dtos.ColleagueFinance.ApprovedDocumentFilterCriteria, Domain.ColleagueFinance.Entities.ApprovedDocumentFilterCriteria>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Dtos.ColleagueFinance.ApprovedDocumentFilterCriteria, Domain.ColleagueFinance.Entities.ApprovedDocumentFilterCriteria>()).Returns(approvedDocumentFilterCriteriaDtoToEntityAdapter);

            // Build a service for getting and updating the document approval.
            documentApprovalService = new DocumentApprovalService(testDocumentApprovalRepository, staffRepositoryMock.Object, documentApprovalAdapterRegistryMock.Object,
                                            documentApprovalUser, roleRepository, loggerMock.Object);

            // Build a service with a staff repository that does not have a staff record.
            expiredSessionService = new DocumentApprovalService(expiredRepositoryMock.Object, staffRepositoryMock.Object, documentApprovalAdapterRegistryMock.Object,
                documentApprovalUser, roleRepository, loggerMock.Object);

            // Build a service with a staff repository that does not have a staff record.
            noStaffLoginservice = new DocumentApprovalService(testDocumentApprovalRepository, noStaffLoginIdStaffRepositoryMock.Object, documentApprovalAdapterRegistryMock.Object,
                documentApprovalUser, roleRepository, loggerMock.Object);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new DocumentApprovalService(testDocumentApprovalRepositoryNoPermissionMock.Object, staffRepositoryMock.Object, documentApprovalAdapterRegistryMock.Object,
                noPermissionsUser, roleRepository, loggerMock.Object);

            approvalDocumentRequests = new List<Domain.ColleagueFinance.Entities.ApprovalDocumentRequest>()
            {
                new Domain.ColleagueFinance.Entities.ApprovalDocumentRequest()
                {
                    Approve = true,
                    DocumentType = "REQ",
                    DocumentId = "1325",
                    DocumentNumber = "0001196",
                    ChangeDate = "12345",
                    ChangeTime = "33333",
                    DocumentItems = new List<Domain.ColleagueFinance.Entities.ApprovalItem>()
                    {
                        new Domain.ColleagueFinance.Entities.ApprovalItem()
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

            documentApprovalRequest = new Dtos.ColleagueFinance.DocumentApprovalRequest()
            {
                ApprovalDocumentRequests = new List<Dtos.ColleagueFinance.ApprovalDocumentRequest>()
                {
                    new Dtos.ColleagueFinance.ApprovalDocumentRequest()
                    {
                        Approve = true,
                        DocumentType = "REQ",
                        DocumentId = "1325",
                        DocumentNumber = "0001196",
                        ChangeDate = "12345",
                        ChangeTime = "33333",
                        DocumentItems = new List<Dtos.ColleagueFinance.ApprovalItem>()
                        {
                            new Dtos.ColleagueFinance.ApprovalItem()
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

            dtoFilterCriteria = new Dtos.ColleagueFinance.ApprovedDocumentFilterCriteria()
            {
                DocumentType = null,
                VendorIds = null,
                DocumentDateFrom = null,
                DocumentDateTo = null,
                ApprovalDateFrom = null,
                ApprovalDateTo = null
            };
            entityFilterCriteria = new Domain.ColleagueFinance.Entities.ApprovedDocumentFilterCriteria();
        }

        [TestCleanup]
        public void Cleanup()
        {
            noStaffLoginservice = null;
            documentApprovalService = null;
            serviceForNoPermission = null;
            expiredSessionService = null;

            repositoryMock = null;
            expiredRepositoryMock = null;

            staffRepositoryMock = null;
            noStaffLoginIdStaffRepositoryMock = null;

            roleRepositoryMock = null;
            roleRepository = null;

            glUserRoleViewPermissions = null;
            documentApprovalUser = null;
        }
        #endregion

        #region Get a document approval tests
        [TestMethod]
        public async Task GetAsync_Success()
        {
            // Confirm that the Get Async service method return a DTO with the same information that is in the 
            // test document approval domain entity.
            var documentApprovalDomain = await testDocumentApprovalRepository.GetAsync("GTT");
            var documentApprovalDto = await documentApprovalService.GetAsync();

            // Confirm that the data in the document approval DTO matches the domain entity
            Assert.AreEqual(documentApprovalDto.CanOverrideFundsAvailability, documentApprovalDomain.CanOverrideFundsAvailability);

            // Confirm that for each approval document DTO that there is a matching test approval document
            // domain entity that has matching values.
            foreach (var approvalDocumentDto in documentApprovalDto.ApprovalDocuments)
            {
                var matchingApprovalDocumentEntity = documentApprovalDomain.ApprovalDocuments.FirstOrDefault(x => x.Id == approvalDocumentDto.Id
                    && x.Number == approvalDocumentDto.Number
                    && x.DocumentType == approvalDocumentDto.DocumentType
                    && x.Date == approvalDocumentDto.Date
                    && x.VendorName == approvalDocumentDto.VendorName
                    && x.NetAmount == approvalDocumentDto.NetAmount
                    && x.ChangeDate == approvalDocumentDto.ChangeDate
                    && x.ChangeTime == approvalDocumentDto.ChangeTime);
                Assert.IsNotNull(matchingApprovalDocumentEntity);

                foreach (var approvalItemDto in approvalDocumentDto.DocumentItems)
                {
                    var matchingApprovalItemEntity = matchingApprovalDocumentEntity.DocumentItems.FirstOrDefault(x => x.DocumentType == approvalItemDto.DocumentType
                    && x.DocumentId == approvalItemDto.DocumentId
                    && x.ItemId == approvalItemDto.ItemId
                    && x.ChangeDate == approvalItemDto.ChangeDate
                    && x.ChangeTime == approvalItemDto.ChangeTime);
                    Assert.IsNotNull(matchingApprovalItemEntity);
                }

                // Confirm that the number of approval entities is the same as the number of approval DTOs.
                Assert.AreEqual(documentApprovalDto.ApprovalDocuments.Count(), documentApprovalDomain.ApprovalDocuments.Count());
                foreach (var approverDto in approvalDocumentDto.DocumentApprovers)
                {
                    var matchingApprovalInformationEntity = matchingApprovalDocumentEntity.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverDto.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(matchingApprovalInformationEntity);
                    Assert.AreEqual(matchingApprovalInformationEntity.ApprovalName, approverDto.ApprovalName);
                    Assert.AreEqual(matchingApprovalInformationEntity.ApprovalDate, approverDto.ApprovalDate);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetAsync_PermissionException()
        {
            await serviceForNoPermission.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetAsync_NoStaffLoginId_PermissionException()
        {
            await noStaffLoginservice.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueSessionExpiredException))]
        public async Task GetAsync_RepositoryReturnsColleagueExpiredException()
        {
            await expiredSessionService.GetAsync();
        }

        #endregion

        #region Update a document approval tests
        [TestMethod]
        public async Task UpdateDocumentApprovalRequestAsync_Success()
        {
            // Confirm that the UpdateDocumentApprovalRequestAsync service method returns a DTO with
            // the same information that is in the test document approval domain entity.
            var documentApprovalDomain = await testDocumentApprovalRepository.UpdateDocumentApprovalAsync("GTT", approvalDocumentRequests);
            var documentApprovalResponseDto = await documentApprovalService.UpdateDocumentApprovalRequestAsync(documentApprovalRequest);

            // Confirm that the data in the document approval response DTO matches the domain entity
            foreach (var updatedApprovalDocumentDto in documentApprovalResponseDto.UpdatedApprovalDocumentResponses)
            {
                var matchingUpdatedApprovalDocumentEntity = documentApprovalDomain.UpdatedApprovalDocumentResponses.FirstOrDefault(x => x.DocumentId == updatedApprovalDocumentDto.DocumentId
                    && x.DocumentType == updatedApprovalDocumentDto.DocumentType
                    && x.DocumentNumber == updatedApprovalDocumentDto.DocumentNumber
                    && x.DocumentStatus == updatedApprovalDocumentDto.DocumentStatus);
                Assert.IsNotNull(matchingUpdatedApprovalDocumentEntity);
            }

            foreach (var nonUpdatedApprovalDocumentDto in documentApprovalResponseDto.NotUpdatedApprovalDocumentResponses)
            {
                var matchingNonUpdatedApprovalDocumentEntity = documentApprovalDomain.NotUpdatedApprovalDocumentResponses.FirstOrDefault(x => x.DocumentId == nonUpdatedApprovalDocumentDto.DocumentId
                    && x.DocumentType == nonUpdatedApprovalDocumentDto.DocumentType
                    && x.DocumentNumber == nonUpdatedApprovalDocumentDto.DocumentNumber
                    && x.DocumentStatus == nonUpdatedApprovalDocumentDto.DocumentStatus);
                Assert.IsNotNull(matchingNonUpdatedApprovalDocumentEntity);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task UpdateDocumentApprovalRequestAsync_PermissionException()
        {
            await serviceForNoPermission.UpdateDocumentApprovalRequestAsync(documentApprovalRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task UpdateDocumentApprovalRequestAsync_NoStaffLoginId_PermissionException()
        {
            await noStaffLoginservice.UpdateDocumentApprovalRequestAsync(documentApprovalRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueSessionExpiredException))]
        public async Task UpdateDocumentApprovalRequestAsync_RepositoryReturnsColleagueExpiredException()
        {
            await expiredSessionService.UpdateDocumentApprovalRequestAsync(documentApprovalRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateDocumentApprovalRequestAsync_NullDocumentApprovalRequestException()
        {
            // Confirm that the UpdateDocumentApprovalRequestAsync service method returns a DTO with
            // the same information that is in the test document approval domain entity.
            var documentApprovalDomain = await testDocumentApprovalRepository.UpdateDocumentApprovalAsync("GTT", approvalDocumentRequests);
            var documentApprovalResponseDto = await documentApprovalService.UpdateDocumentApprovalRequestAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateDocumentApprovalRequestAsync_NoApprovalDocumentRequests()
        {
            // Confirm that the UpdateDocumentApprovalRequestAsync service method returns a DTO with
            // the same information that is in the test document approval domain entity.
            var documentApprovalDomain = await testDocumentApprovalRepository.UpdateDocumentApprovalAsync("GTT", approvalDocumentRequests);
            documentApprovalRequest.ApprovalDocumentRequests = null;
            var documentApprovalResponseDto = await documentApprovalService.UpdateDocumentApprovalRequestAsync(documentApprovalRequest);
        }
        #region Approval returns tests

        [TestMethod]
        public async Task UpdateDocumentApprovalRequestAsync_ReturnDocumentSuccess()
        {
            // Confirm that the UpdateDocumentApprovalRequestAsync service method returns a DTO with
            // the same information that is in the test document approval domain entity.
            var documentApprovalDomain = await testDocumentApprovalRepository.UpdateDocumentApprovalAsync("GTT", approvalDocumentRequests);
            documentApprovalRequest = new Dtos.ColleagueFinance.DocumentApprovalRequest();
            documentApprovalRequest.ApprovalDocumentRequests = new List<Dtos.ColleagueFinance.ApprovalDocumentRequest>();
            var documentRequest = new Dtos.ColleagueFinance.ApprovalDocumentRequest()
            {
                Approve = false,
                DocumentType = "REQ",
                DocumentId = "1325",
                DocumentNumber = "0001196",
                Return = true,
                ReturnComments = "Need more info",
                ChangeDate = "12345",
                ChangeTime = "33333",
                DocumentItems = new List<Dtos.ColleagueFinance.ApprovalItem>()
                        {
                            new Dtos.ColleagueFinance.ApprovalItem()
                            {
                                DocumentType = "REQ",
                                DocumentId = "1325",
                                ItemId = "7237",
                                ChangeDate = "12345",
                                ChangeTime = "33333"
                            }
                        }
            };

            documentApprovalRequest.ApprovalDocumentRequests.Add(documentRequest);

            var documentApprovalResponseDto = await documentApprovalService.UpdateDocumentApprovalRequestAsync(documentApprovalRequest);

            Assert.IsNotNull(documentApprovalResponseDto.UpdatedApprovalDocumentResponses);
            Assert.IsNotNull(documentApprovalResponseDto.NotUpdatedApprovalDocumentResponses);
            Assert.IsTrue(documentApprovalResponseDto.UpdatedApprovalDocumentResponses.Count == 1);
            Assert.IsTrue(documentApprovalResponseDto.NotUpdatedApprovalDocumentResponses.Count == 0);
            // Confirm that the data in the document approval response DTO matches the domain entity
            var updatedApprovalDocumentDto = documentApprovalResponseDto.UpdatedApprovalDocumentResponses.FirstOrDefault();
            var matchingUpdatedApprovalDocumentEntity = documentApprovalDomain.UpdatedApprovalDocumentResponses.FirstOrDefault(x => x.DocumentId == updatedApprovalDocumentDto.DocumentId
                && x.DocumentType == updatedApprovalDocumentDto.DocumentType
                && x.DocumentNumber == updatedApprovalDocumentDto.DocumentNumber
                && x.DocumentId == updatedApprovalDocumentDto.DocumentId);
            Assert.IsNotNull(matchingUpdatedApprovalDocumentEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UpdateDocumentApprovalRequestAsync_ReturnContainsMoreThanOneDocument()
        {
            documentApprovalRequest = new Dtos.ColleagueFinance.DocumentApprovalRequest();
            documentApprovalRequest.ApprovalDocumentRequests = new List<Dtos.ColleagueFinance.ApprovalDocumentRequest>();

            var documentRequest1 = new Dtos.ColleagueFinance.ApprovalDocumentRequest()
            {
                Approve = false,
                DocumentType = "REQ",
                DocumentId = "1325",
                DocumentNumber = "0001196",
                Return = true,
                ReturnComments = "Need more info",
                ChangeDate = "12345",
                ChangeTime = "33333",
                DocumentItems = new List<Dtos.ColleagueFinance.ApprovalItem>()
                        {
                            new Dtos.ColleagueFinance.ApprovalItem()
                            {
                                DocumentType = "REQ",
                                DocumentId = "1325",
                                ItemId = "7237",
                                ChangeDate = "12345",
                                ChangeTime = "33333"
                            }
                        }
            };
            var documentRequest2 = new Dtos.ColleagueFinance.ApprovalDocumentRequest()
            {
                Approve = true,
                DocumentType = "REQ",
                DocumentId = "1325",
                DocumentNumber = "0001197",
                Return = false,
                ReturnComments = null,
                ChangeDate = "12345",
                ChangeTime = "33333",
                DocumentItems = new List<Dtos.ColleagueFinance.ApprovalItem>()
                        {
                            new Dtos.ColleagueFinance.ApprovalItem()
                            {
                                DocumentType = "REQ",
                                DocumentId = "1326",
                                ItemId = "7238",
                                ChangeDate = "12345",
                                ChangeTime = "33333"
                            }
                        }
            };
            documentApprovalRequest.ApprovalDocumentRequests.Add(documentRequest1);
            documentApprovalRequest.ApprovalDocumentRequests.Add(documentRequest2);
            var documentApprovalResponseDto = await documentApprovalService.UpdateDocumentApprovalRequestAsync(documentApprovalRequest);
        }
        
        #endregion

        #endregion

        #region QueryApprovedDocumentsAsync Tests

        [TestMethod]
        public async Task QueryApprovedDocumentsAsync_Success_NoFilterValues()
        {
            // Test the filter conversion from DTO to entity with no values.
            var approvedDocumentFilterCriteriaDtoToEntityAdapter = new AutoMapperAdapter<Dtos.ColleagueFinance.ApprovedDocumentFilterCriteria, Domain.ColleagueFinance.Entities.ApprovedDocumentFilterCriteria>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Dtos.ColleagueFinance.ApprovedDocumentFilterCriteria, Domain.ColleagueFinance.Entities.ApprovedDocumentFilterCriteria>()).Returns(approvedDocumentFilterCriteriaDtoToEntityAdapter);
            entityFilterCriteria = approvedDocumentFilterCriteriaDtoToEntityAdapter.MapToType(dtoFilterCriteria);

            Assert.AreEqual(entityFilterCriteria.DocumentType.Count(), 0);
            Assert.AreEqual(entityFilterCriteria.VendorIds.Count(), 0);
            Assert.AreEqual(entityFilterCriteria.DocumentDateFrom, dtoFilterCriteria.DocumentDateFrom);
            Assert.AreEqual(entityFilterCriteria.DocumentDateTo, dtoFilterCriteria.DocumentDateTo);
            Assert.AreEqual(entityFilterCriteria.ApprovalDateFrom, dtoFilterCriteria.ApprovalDateFrom);
            Assert.AreEqual(entityFilterCriteria.ApprovalDateTo, dtoFilterCriteria.ApprovalDateTo);

            // Confirm that the QueryApprovedDocumentsAsync service method returns a DTO with
            // the same information that is in the test approved document domain entity.
            var testApprovedDocumentEntity = await testDocumentApprovalRepository.QueryApprovedDocumentsAsync("TGL", entityFilterCriteria);
            var actualApprovedDocumentDto = await documentApprovalService.QueryApprovedDocumentsAsync(dtoFilterCriteria);

            foreach (var approvedDocDto in actualApprovedDocumentDto)
            {
                var testMatchingApprovedDocument = testApprovedDocumentEntity.FirstOrDefault(x => x.Id == approvedDocDto.Id
                    && x.Number == approvedDocDto.Number
                    && x.DocumentType == approvedDocDto.DocumentType
                    && x.Date == approvedDocDto.Date
                    && x.VendorName == approvedDocDto.VendorName
                    && x.NetAmount == approvedDocDto.NetAmount);
                Assert.IsNotNull(testMatchingApprovedDocument);

                // Confirm that the number of approval information entities and DTOs matchs.
                Assert.AreEqual(approvedDocDto.DocumentApprovers.Count(), testMatchingApprovedDocument.DocumentApprovers.Count());
                foreach (var approverDto in approvedDocDto.DocumentApprovers)
                {
                    var matchingApprovalInformationEntity = testMatchingApprovedDocument.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverDto.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(matchingApprovalInformationEntity);
                    Assert.AreEqual(matchingApprovalInformationEntity.ApprovalName, approverDto.ApprovalName);
                    Assert.AreEqual(matchingApprovalInformationEntity.ApprovalDate, approverDto.ApprovalDate);
                }
            }
        }

        [TestMethod]
        public async Task QueryApprovedDocumentsAsync_Success_WithFilterValues()
        {
            // Test the filter conversion from DTO to entity with values.
            dtoFilterCriteria.DocumentType = new List<string>() { "REQ", "PO", "VOU" };
            dtoFilterCriteria.VendorIds = new List<string>() { "0001666", "0000877", "0001272" };
            dtoFilterCriteria.DocumentDateFrom = DateTime.Today.AddDays(-5);
            dtoFilterCriteria.DocumentDateTo = DateTime.Today.AddDays(-4);
            dtoFilterCriteria.ApprovalDateFrom = DateTime.Today.AddDays(-3);
            dtoFilterCriteria.ApprovalDateTo = DateTime.Today.AddDays(-1);

            var approvedDocumentFilterCriteriaDtoToEntityAdapter = new AutoMapperAdapter<Dtos.ColleagueFinance.ApprovedDocumentFilterCriteria, Domain.ColleagueFinance.Entities.ApprovedDocumentFilterCriteria>(documentApprovalAdapterRegistryMock.Object, loggerMock.Object);
            documentApprovalAdapterRegistryMock.Setup(x => x.GetAdapter<Dtos.ColleagueFinance.ApprovedDocumentFilterCriteria, Domain.ColleagueFinance.Entities.ApprovedDocumentFilterCriteria>()).Returns(approvedDocumentFilterCriteriaDtoToEntityAdapter);
            entityFilterCriteria = approvedDocumentFilterCriteriaDtoToEntityAdapter.MapToType(dtoFilterCriteria);

            Assert.AreEqual(entityFilterCriteria.DocumentType.Count(), dtoFilterCriteria.DocumentType.Count());
            Assert.AreEqual(entityFilterCriteria.VendorIds.Count(), dtoFilterCriteria.VendorIds.Count());
            Assert.AreEqual(entityFilterCriteria.DocumentDateFrom, dtoFilterCriteria.DocumentDateFrom);
            Assert.AreEqual(entityFilterCriteria.DocumentDateTo, dtoFilterCriteria.DocumentDateTo);
            Assert.AreEqual(entityFilterCriteria.ApprovalDateFrom, dtoFilterCriteria.ApprovalDateFrom);
            Assert.AreEqual(entityFilterCriteria.ApprovalDateTo, dtoFilterCriteria.ApprovalDateTo);

            // Confirm that the QueryApprovedDocumentsAsync service method returns a DTO with
            // the same information that is in the test approved document domain entity.
            var testApprovedDocumentEntity = await testDocumentApprovalRepository.QueryApprovedDocumentsAsync("TGL", entityFilterCriteria);
            var actualApprovedDocumentDto = await documentApprovalService.QueryApprovedDocumentsAsync(dtoFilterCriteria);

            foreach (var approvedDocDto in actualApprovedDocumentDto)
            {
                var testMatchingApprovedDocument = testApprovedDocumentEntity.FirstOrDefault(x => x.Id == approvedDocDto.Id
                    && x.Number == approvedDocDto.Number
                    && x.DocumentType == approvedDocDto.DocumentType
                    && x.Date == approvedDocDto.Date
                    && x.VendorName == approvedDocDto.VendorName
                    && x.NetAmount == approvedDocDto.NetAmount);
                Assert.IsNotNull(testMatchingApprovedDocument);

                // Confirm that the number of approval information entities and DTOs matchs.
                Assert.AreEqual(approvedDocDto.DocumentApprovers.Count(), testMatchingApprovedDocument.DocumentApprovers.Count());
                foreach (var approverDto in approvedDocDto.DocumentApprovers)
                {
                    var matchingApprovalInformationEntity = testMatchingApprovedDocument.DocumentApprovers.FirstOrDefault(x => x.ApproverId.ToUpperInvariant() == approverDto.ApproverId.ToUpperInvariant());
                    Assert.IsNotNull(matchingApprovalInformationEntity);
                    Assert.AreEqual(matchingApprovalInformationEntity.ApprovalName, approverDto.ApprovalName);
                    Assert.AreEqual(matchingApprovalInformationEntity.ApprovalDate, approverDto.ApprovalDate);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task QueryApprovedDocumentsAsync_PermissionException()
        {
            await serviceForNoPermission.QueryApprovedDocumentsAsync(dtoFilterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task QueryApprovedDocumentsAsync_NoStaffLoginId_PermissionException()
        {
            await noStaffLoginservice.QueryApprovedDocumentsAsync(dtoFilterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueSessionExpiredException))]
        public async Task QueryApprovedDocumentsAsync_RepositoryReturnsColleagueExpiredException()
        {
            await expiredSessionService.QueryApprovedDocumentsAsync(dtoFilterCriteria);
        }
        #endregion
    }
}
