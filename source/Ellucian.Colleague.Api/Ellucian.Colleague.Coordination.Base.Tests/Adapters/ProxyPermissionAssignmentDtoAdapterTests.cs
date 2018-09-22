// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class ProxyPermissionAssignmentDtoAdapterTests
    {
        private ProxyPermissionAssignmentDtoAdapter adapter;
        private Dtos.Base.ProxyPermissionAssignment dto;
        private Domain.Base.Entities.ProxyPermissionAssignment entity;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();
            adapter = new ProxyPermissionAssignmentDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            dto = new Dtos.Base.ProxyPermissionAssignment()
            {
                ProxySubjectId = "0001234",
                ProxySubjectApprovalDocumentText = new System.Collections.Generic.List<string>() { "This is some text.", "Here is some more." },
                Permissions = new System.Collections.Generic.List<Dtos.Base.ProxyAccessPermission>()
                {
                    new Dtos.Base.ProxyAccessPermission() { EffectiveDate = DateTime.Today.AddDays(-3), IsGranted = true, ProxySubjectId = "0001234", ProxyUserId = "0001235", ProxyWorkflowCode = "SFMAP", StartDate = DateTime.Today.AddDays(-3), ReauthorizationDate = null },
                    new Dtos.Base.ProxyAccessPermission() { EffectiveDate = DateTime.Today, IsGranted = false, ProxySubjectId = "0001234", ProxyUserId = "0001235", ProxyWorkflowCode = "SFAA", StartDate = DateTime.Today, EndDate = DateTime.Today, ReauthorizationDate = null },
                },
                ProxyEmailAddress = "proxyemail@ellucian.com.com",
                ProxyEmailType = "PRI"
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyPermissionAssignmentDtoAdapter_NullSource()
        {
            entity = adapter.MapToType(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyPermissionAssignmentDtoAdapter_NullPermissions()
        {
            var nullPerms = dto;
            nullPerms.Permissions = null;
            entity = adapter.MapToType(nullPerms);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyPermissionAssignmentDtoAdapter_NoPermissions()
        {
            var noPerms = dto;
            noPerms.Permissions = new System.Collections.Generic.List<Dtos.Base.ProxyAccessPermission>();
            entity = adapter.MapToType(noPerms);
        }

        [TestMethod]
        public void ProxyPermissionAssignmentDtoAdapter_Valid()
        {
            entity = adapter.MapToType(dto);
            Assert.AreEqual(dto.ProxySubjectId, entity.ProxySubjectId);
            CollectionAssert.AreEqual(dto.ProxySubjectApprovalDocumentText, entity.ProxySubjectApprovalDocumentText);
            Assert.AreEqual(dto.Permissions.Count, entity.Permissions.Count);
            Assert.IsFalse(entity.IsReauthorizing);
            for (int i = 0; i < dto.Permissions.Count; i++)
            {
                Assert.AreEqual(dto.Permissions[i].ApprovalEmailDocumentId, entity.Permissions[i].ApprovalEmailDocumentId);
                Assert.AreEqual(dto.Permissions[i].DisclosureReleaseDocumentId, entity.Permissions[i].DisclosureReleaseDocumentId);
                Assert.AreEqual(dto.Permissions[i].EffectiveDate, entity.Permissions[i].EffectiveDate);
                Assert.AreEqual(dto.Permissions[i].EndDate, entity.Permissions[i].EndDate);
                Assert.AreEqual(dto.Permissions[i].ReauthorizationDate, entity.Permissions[i].ReauthorizationDate);
                Assert.AreEqual(dto.Permissions[i].Id, entity.Permissions[i].Id);
                Assert.AreEqual(dto.Permissions[i].IsGranted, entity.Permissions[i].IsGranted);
                Assert.AreEqual(dto.Permissions[i].ProxySubjectId, entity.Permissions[i].ProxySubjectId);
                Assert.AreEqual(dto.Permissions[i].ProxyUserId, entity.Permissions[i].ProxyUserId);
                Assert.AreEqual(dto.Permissions[i].ProxyWorkflowCode, entity.Permissions[i].ProxyWorkflowCode);
                Assert.AreEqual(dto.Permissions[i].StartDate, entity.Permissions[i].StartDate);
            }
            Assert.AreEqual(dto.ProxyEmailAddress, entity.ProxyEmailAddress);
            Assert.AreEqual(dto.ProxyEmailType, entity.ProxyEmailType);
        }
    }
}
