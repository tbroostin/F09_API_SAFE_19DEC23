// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ProxyPermissionAssignmentTests
    {
        private string proxySubjectId;
        private ProxyAccessPermission permission1, permission2;
        private List<ProxyAccessPermission> permissions, permissions2;


        [TestInitialize]
        public void ProxyPermissionAssignment_Initialize()
        {
            proxySubjectId = "0012345";
            permission1 = new ProxyAccessPermission(null, "0012345", "0098765", "SFMAP", DateTime.Today);
            permissions = new List<ProxyAccessPermission>() { permission1 };
            permission2 = new ProxyAccessPermission(null, "0054321", "0098765", "SFMAP", DateTime.Today);
            permissions2 = new List<ProxyAccessPermission>() { permission2 };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyPermissionAssignment_Constructor_NullProxySubjectId()
        {
            var result = new ProxyPermissionAssignment(null, permissions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyPermissionAssignment_Constructor_EmptyProxySubjectId()
        {
            var result = new ProxyPermissionAssignment(string.Empty, permissions);
        }

        [TestMethod]
        public void ProxyPermissionAssignment_Constructor_ValidProxySubjectId()
        {
            var result = new ProxyPermissionAssignment(proxySubjectId, permissions);
            Assert.AreEqual(proxySubjectId, result.ProxySubjectId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyPermissionAssignment_Constructor_NullPermissions()
        {
            var result = new ProxyPermissionAssignment(proxySubjectId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyPermissionAssignment_Constructor_NoPermissions()
        {
            var result = new ProxyPermissionAssignment(proxySubjectId, new List<ProxyAccessPermission>());
        }

        [TestMethod]
        public void ProxyPermissionAssignment_Constructor_ValidPermissions()
        {
            var result = new ProxyPermissionAssignment(proxySubjectId, permissions);
            CollectionAssert.AllItemsAreInstancesOfType(result.Permissions, typeof(ProxyAccessPermission));
            CollectionAssert.AreEqual(permissions, result.Permissions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProxyPermissionAssignment_Constructor_MultipleProxySubjectUsersOnPermissions()
        {
            permissions.Add(permission2);
            var result = new ProxyPermissionAssignment(proxySubjectId, permissions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProxyPermissionAssignment_Constructor_DifferentProxySubjectUsersOnPermissions()
        {
            var result = new ProxyPermissionAssignment(proxySubjectId, permissions2);
        }

        [TestMethod]
        public void ProxyPermissionAssignment_IsReauthorizing_Default()
        {
            var result = new ProxyPermissionAssignment(proxySubjectId, permissions);
            Assert.IsFalse(result.IsReauthorizing);
        }

        [TestMethod]
        public void ProxyPermissionAssignment_IsReauthorizing_False()
        {
            var result = new ProxyPermissionAssignment(proxySubjectId, permissions, false);
            Assert.IsFalse(result.IsReauthorizing);
        }

        [TestMethod]
        public void ProxyPermissionAssignment_IsReauthorizing_True()
        {
            var result = new ProxyPermissionAssignment(proxySubjectId, permissions, true);
            Assert.IsTrue(result.IsReauthorizing);
        }
    }
}
