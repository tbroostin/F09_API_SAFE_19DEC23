// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ProxyAccessPermissionTests
    {
        private string id;
        private string proxySubjectId;
        private string proxyUserId;
        private string workflowId;
        private DateTime dateGranted, yesterday, tomorrow;

        [TestInitialize]
        public void ProxyAccessPermission_Initialize()
        {
            id = "123";
            proxySubjectId = "0123456";
            proxyUserId = "1234567";
            workflowId = "SFMAP";
            dateGranted = DateTime.Today;
            yesterday = DateTime.Today.AddDays(-1);
            tomorrow = DateTime.Today.AddDays(1);
        }

        [TestMethod]
        public void ProxyAccessPermission_Constructor_NullId()
        {
            var result = new ProxyAccessPermission(null, proxySubjectId, proxyUserId, workflowId, dateGranted);
            Assert.IsNull(result.Id);
        }

        [TestMethod]
        public void ProxyAccessPermission_Constructor_EmptyId()
        {
            var result = new ProxyAccessPermission(string.Empty, proxySubjectId, proxyUserId, workflowId, dateGranted);
            Assert.AreEqual(string.Empty, result.Id);
        }

        [TestMethod]
        public void ProxyAccessPermission_Constructor_Id()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, dateGranted);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ProxyAccessPermission_Constructor_Id_InvalidChange()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, dateGranted);
            result.Id = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyAccessPermission_Constructor_NullProxySubjectId()
        {
            var result = new ProxyAccessPermission(id, null, proxyUserId, workflowId, dateGranted);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyAccessPermission_Constructor_EmptyProxySubjectId()
        {
            var result = new ProxyAccessPermission(id, string.Empty, proxyUserId, workflowId, dateGranted);
        }

        [TestMethod]
        public void ProxyAccessPermission_Constructor_ValidProxySubjectId()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, dateGranted);
            Assert.AreEqual(proxyUserId, result.ProxyUserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyAccessPermission_Constructor_NullProxyUserId()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, null, workflowId, dateGranted);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyAccessPermission_Constructor_EmptyProxyUserId()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, string.Empty, workflowId, dateGranted);
        }

        [TestMethod]
        public void ProxyAccessPermission_Constructor_ValidProxyUserId()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, dateGranted);
            Assert.AreEqual(proxyUserId, result.ProxyUserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyAccessPermission_Constructor_NullWorkflowId()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, null, dateGranted);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyAccessPermission_Constructor_EmptyWorkflowId()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, string.Empty, dateGranted);
        }

        [TestMethod]
        public void ProxyAccessPermission_Constructor_ValidWorkflowId()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, dateGranted);
            Assert.AreEqual(workflowId, result.ProxyWorkflowCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyAccessPermission_Constructor_DateGrantedInvalid()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, DateTime.MinValue);
        }

        [TestMethod]
        public void ProxyAccessPermission_Constructor_DateGrantedValid()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, dateGranted);
            Assert.AreEqual(dateGranted, result.StartDate);
        }

        [TestMethod]
        public void ProxyAccessPermission_Id_SetValueNull()
        {
            var result = new ProxyAccessPermission(null, proxySubjectId, proxyUserId, workflowId, dateGranted);
            result.Id = null;
            Assert.IsNull(result.Id);
        }

        [TestMethod]
        public void ProxyAccessPermission_Id_SetValueEmpty()
        {
            var result = new ProxyAccessPermission(null, proxySubjectId, proxyUserId, workflowId, dateGranted);
            result.Id = string.Empty;
            Assert.IsNull(result.Id);
        }

        [TestMethod]
        public void ProxyAccessPermission_Id_SetValueValid()
        {
            var result = new ProxyAccessPermission(null, proxySubjectId, proxyUserId, workflowId, dateGranted);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ProxyAccessPermission_Id_SetValueInvalid()
        {
            var result = new ProxyAccessPermission(null, proxySubjectId, proxyUserId, workflowId, dateGranted);
            result.Id = id;
            result.Id = null;
        }

        [TestMethod]
        public void ProxyAccessPermission_EffectiveDate_NoEndDate()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, yesterday);
            Assert.AreEqual(yesterday, result.EffectiveDate);
        }

        [TestMethod]
        public void ProxyAccessPermission_EffectiveDate_EndDateIsPast()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, yesterday.AddDays(-3));
            result.EndDate = yesterday;
            Assert.AreEqual(yesterday, result.EffectiveDate);
        }

        [TestMethod]
        public void ProxyAccessPermission_EffectiveDate_EndDateIsToday()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, yesterday);
            result.EndDate = DateTime.Today;
            Assert.AreEqual(DateTime.Today, result.EffectiveDate);
        }

        [TestMethod]
        public void ProxyAccessPermission_EffectiveDate_EndDateIsFuture()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, yesterday);
            result.EndDate = tomorrow;
            Assert.AreEqual(yesterday, result.EffectiveDate);
        }

        [TestMethod]
        public void ProxyAccessPermission_IsGranted_EndDateIsPast()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, yesterday);
            result.EndDate = yesterday;
            Assert.IsFalse(result.IsGranted);
        }

        [TestMethod]
        public void ProxyAccessPermission_IsGranted_EndDateIsToday()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, yesterday);
            result.EndDate = dateGranted;
            Assert.IsFalse(result.IsGranted);
        }

        [TestMethod]
        public void ProxyAccessPermission_IsGranted_EndDateIsFuture()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, yesterday);
            result.EndDate = tomorrow;
            Assert.IsTrue(result.IsGranted);
        }

        [TestMethod]
        public void ProxyAccessPermission_IsGranted_StartDateIsPast()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, yesterday);
            Assert.IsTrue(result.IsGranted);
        }

        [TestMethod]
        public void ProxyAccessPermission_IsGranted_StartDateIsToday()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, dateGranted);
            Assert.IsTrue(result.IsGranted);
        }

        [TestMethod]
        public void ProxyAccessPermission_IsGranted_StartDateIsFuture()
        {
            var result = new ProxyAccessPermission(id, proxySubjectId, proxyUserId, workflowId, tomorrow);
            Assert.IsFalse(result.IsGranted);
        }
    }
}
