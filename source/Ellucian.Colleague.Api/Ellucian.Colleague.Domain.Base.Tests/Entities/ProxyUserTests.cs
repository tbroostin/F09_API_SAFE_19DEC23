// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ProxyUserTests
    {
        private string id;

        [TestInitialize]
        public void Initialize()
        {
            id = "0001234";
        }

        [TestCleanup]
        public void Cleanup()
        {
            id = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyUser_Constructor_NullId()
        {
            var entity = new ProxyUser(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyUser_Constructor_EmptyId()
        {
            var entity = new ProxyUser(string.Empty);
        }

        [TestMethod]
        public void ProxyUser_Id()
        {
            var entity = new ProxyUser(id);
            Assert.AreEqual(id, entity.Id);
        }

        [TestMethod]
        public void ProxyUser_Permissions()
        {
            var entity = new ProxyUser(id);
            Assert.AreEqual(0, entity.Permissions.Count);
        }

        [TestMethod]
        public void ProxyUser_EffectiveDate_AtLeastOnePermission()
        {
            var entity = new ProxyUser(id);
            entity.AddPermission(new ProxyAccessPermission("1", "0001235", id, "SFAA", DateTime.Today.AddDays(-3)));
            entity.AddPermission(new ProxyAccessPermission("2", "0001235", id, "SFMAP", DateTime.Today.AddDays(-1)));
            Assert.AreEqual(DateTime.Today.AddDays(-1), entity.EffectiveDate);
        }

        [TestMethod]
        public void ProxyUser_EffectiveDate_NoPermissions()
        {
            var entity = new ProxyUser(id);
            Assert.AreEqual(null, entity.EffectiveDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyUser_AddPermission_NullPermission()
        {
            var entity = new ProxyUser(id);
            entity.AddPermission(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProxyUser_AddPermission_PermissionProxyUserIdMismatch()
        {
            var entity = new ProxyUser(id);
            entity.AddPermission(new ProxyAccessPermission("1", "0001235", id + "1", "SFAA", DateTime.Today));
        }

        [TestMethod]
        public void ProxyUser_AddPermission_Valid()
        {
            var entity = new ProxyUser(id);
            entity.AddPermission(new ProxyAccessPermission("1", "0001235", id, "SFAA", DateTime.Today));
            Assert.AreEqual(1, entity.Permissions.Count);
        }
    }
}
