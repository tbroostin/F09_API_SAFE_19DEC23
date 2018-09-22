// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class ApprovalServiceTests : GenericUserFactory
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory currentUserFactory;

        public Mock<IApprovalRepository> approvalRepositoryMock;
        public TestApprovalRepository testRepository;

        public ApprovalService approvalService;        


        public void BaseInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            currentUserFactory = new GenericUserFactory.UserFactory();
            approvalRepositoryMock = new Mock<IApprovalRepository>();
            testRepository = new TestApprovalRepository();            
        }

        public void BaseCleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            roleRepositoryMock = null;
            currentUserFactory = null;
            approvalRepositoryMock = null;
            testRepository = null;
            approvalService = null;
        }

        public void BuildApprovalService()
        {
            approvalService = new ApprovalService(approvalRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }
    }

    [TestClass]
    public class GetApprovalDocumentTests : ApprovalServiceTests
    {
        private AutoMapperAdapter<Colleague.Domain.Base.Entities.ApprovalDocument, Colleague.Dtos.Base.ApprovalDocument> approvalDocumentEntityToDtoAdapter;
        

        [TestInitialize]
        public void Initialize()
        {
            BaseInitialize();
            approvalRepositoryMock.Setup(r => r.GetApprovalDocument(It.IsAny<string>())).Returns<string>((id) => testRepository.GetApprovalDocument(id));
            approvalDocumentEntityToDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.ApprovalDocument, Dtos.Base.ApprovalDocument>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.ApprovalDocument, Colleague.Dtos.Base.ApprovalDocument>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.ApprovalDocument, 
                Colleague.Dtos.Base.ApprovalDocument>()).Returns(approvalDocumentEntityToDtoAdapter);
            BuildApprovalService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            BaseCleanup();
        }

        /// <summary>
        /// User is self
        /// </summary>
        [TestMethod]
        public void ReturnedDocumentIsNotNullTest()
        {
            var existingId = testRepository.approvalDocuments.First().recordKey;
            Assert.IsNotNull(approvalService.GetApprovalDocument(existingId));
        }

        /// <summary>
        /// No person id on the document
        /// </summary>
        [TestMethod]
        public void NoDocumentPersonId_DocumentIsNotNullTest()
        {
            var document = testRepository.approvalDocuments.First();
            document.personId = null;
            var existingId = document.recordKey;

            var actualDocument = approvalService.GetApprovalDocument(existingId);
            Assert.IsNotNull(actualDocument);
            Assert.IsNull(actualDocument.PersonId);
        }

        /// <summary>
        /// User is neither self, nor proxy, nor admin
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public void UnauthorizedUser_PermissionsExceptionThrownTest()
        {
            var existingId = testRepository.approvalDocuments.First(a => a.personId != currentUserFactory.CurrentUser.PersonId).recordKey;
            approvalService.GetApprovalDocument(existingId);
        }

        /// <summary>
        /// User is proxy
        /// </summary>
        [TestMethod]
        public void UserIsProxy_DocumentIsNotNullTest()
        {
            currentUserFactory = new GenericUserFactory.StudentUserFactoryWithProxy();
            BuildApprovalService();
            var existingId = testRepository.approvalDocuments.First(a => a.personId == currentUserFactory.CurrentUser.ProxySubjects.First().PersonId).recordKey;
            Assert.IsNotNull(approvalService.GetApprovalDocument(existingId));
            
        }

        /// <summary>
        /// User is proxy for a different person
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public void UserIsProxyForDifferentPerson_PermissionsExceptionThrownTest()
        {
            currentUserFactory = new GenericUserFactory.StudentUserFactoryWithProxy();
            BuildApprovalService();
            var existingId = testRepository.approvalDocuments.First(a => a.personId != currentUserFactory.CurrentUser.ProxySubjects.First().PersonId).recordKey;
            approvalService.GetApprovalDocument(existingId);
        }

        /// <summary>
        /// User is admin
        /// </summary>
        [TestMethod]
        public void UserIsAdmin_DocumentIsNotNullTest()
        {
            currentUserFactory = new GenericUserFactory.FinanceAdminUserFactory();
            financeAdminRole.AddPermission(new Permission(BasePermissionCodes.ViewStudentAccountActivity));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { financeAdminRole });
            BuildApprovalService();
            var existingId = testRepository.approvalDocuments.First().recordKey;
            Assert.IsNotNull(approvalService.GetApprovalDocument(existingId));

        }

        /// <summary>
        /// User is admin with no permissions
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public void UserIsAdminNoPermissions_PermissionsExceptionThrownTest()
        {
            currentUserFactory = new GenericUserFactory.FinanceAdminUserFactory();
            BuildApprovalService();
            var existingId = testRepository.approvalDocuments.First().recordKey;
            approvalService.GetApprovalDocument(existingId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoDocumentId_ArgumentNullExceptionThrownTest()
        {
            approvalService.GetApprovalDocument(null);
        }

        [TestMethod]
        public void NoDocumentRetrieved_NoDocumentReturnedTest()
        {
            approvalRepositoryMock.Setup(r => r.GetApprovalDocument(It.IsAny<string>())).Returns((ApprovalDocument)null);
            BuildApprovalService();
            var existingId = testRepository.approvalDocuments.First().recordKey;
            Assert.IsNull(approvalService.GetApprovalDocument(existingId));
        }
    }

    [TestClass]
    public class GetApprovalResponseTests : ApprovalServiceTests
    {
        private AutoMapperAdapter<Colleague.Domain.Base.Entities.ApprovalResponse, Colleague.Dtos.Base.ApprovalResponse> approvalResponseEntityToDtoAdapter;


        [TestInitialize]
        public void Initialize()
        {
            BaseInitialize();
            approvalRepositoryMock.Setup(r => r.GetApprovalResponse(It.IsAny<string>())).Returns<string>((id) => testRepository.GetApprovalResponse(id));
            approvalResponseEntityToDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.ApprovalResponse, Dtos.Base.ApprovalResponse>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.ApprovalResponse, Colleague.Dtos.Base.ApprovalResponse>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.ApprovalResponse,
                Colleague.Dtos.Base.ApprovalResponse>()).Returns(approvalResponseEntityToDtoAdapter);
            BuildApprovalService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            BaseCleanup();
        }

        /// <summary>
        /// User is self
        /// </summary>
        [TestMethod]
        public void ReturnedResponseIsNotNullTest()
        {
            var existingId = testRepository.responseRecords.First().recordKey;
            Assert.IsNotNull(approvalService.GetApprovalResponse(existingId));
        }

        /// <summary>
        /// User is neither self, nor proxy, nor admin
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public void UnauthorizedUser_PermissionsExceptionThrownTest()
        {
            var existingId = testRepository.responseRecords.First(a => a.personId != currentUserFactory.CurrentUser.PersonId).recordKey;
            approvalService.GetApprovalResponse(existingId);
        }

        /// <summary>
        /// User is proxy
        /// </summary>
        [TestMethod]
        public void UserIsProxy_ResponseIsNotNullTest()
        {
            currentUserFactory = new GenericUserFactory.StudentUserFactoryWithProxy();
            BuildApprovalService();
            var existingId = testRepository.responseRecords.First(a => a.personId == currentUserFactory.CurrentUser.ProxySubjects.First().PersonId).recordKey;
            Assert.IsNotNull(approvalService.GetApprovalResponse(existingId));

        }

        /// <summary>
        /// User is proxy for a different person
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public void UserIsProxyForDifferentPerson_PermissionsExceptionThrownTest()
        {
            currentUserFactory = new GenericUserFactory.StudentUserFactoryWithProxy();
            BuildApprovalService();
            var existingId = testRepository.responseRecords.First(a => a.personId != currentUserFactory.CurrentUser.ProxySubjects.First().PersonId).recordKey;
            approvalService.GetApprovalResponse(existingId);
        }

        /// <summary>
        /// User is admin
        /// </summary>
        [TestMethod]
        public void UserIsAdmin_ResponseIsNotNullTest()
        {
            currentUserFactory = new GenericUserFactory.FinanceAdminUserFactory();
            financeAdminRole.AddPermission(new Permission(BasePermissionCodes.ViewStudentAccountActivity));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { financeAdminRole });
            BuildApprovalService();
            var existingId = testRepository.responseRecords.First().recordKey;
            Assert.IsNotNull(approvalService.GetApprovalResponse(existingId));

        }

        /// <summary>
        /// User is admin with no permissions
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public void UserIsAdminNoPermissions_PermissionsExceptionThrownTest()
        {
            currentUserFactory = new GenericUserFactory.FinanceAdminUserFactory();
            BuildApprovalService();
            var existingId = testRepository.responseRecords.First().recordKey;
            approvalService.GetApprovalResponse(existingId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoResponseId_ArgumentNullExceptionThrownTest()
        {
            approvalService.GetApprovalResponse(null);
        }

        [TestMethod]
        public void NoResponseRetrieved_NoResponseReturnedTest()
        {
            approvalRepositoryMock.Setup(r => r.GetApprovalResponse(It.IsAny<string>())).Returns((ApprovalResponse)null);
            BuildApprovalService();
            var existingId = testRepository.responseRecords.First().recordKey;
            Assert.IsNull(approvalService.GetApprovalResponse(existingId));
        }
    }
}
