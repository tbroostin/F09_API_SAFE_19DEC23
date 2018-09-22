// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ProxySubjectTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxySubject_Constructor_NullId()
        {
            var subject = new ProxySubject(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxySubject_Constructor_EmptyId()
        {
            var subject = new ProxySubject(string.Empty);
        }

        [TestMethod]
        public void ProxySubject_Constructor_Valid()
        {
            var subject = new ProxySubject("12345");
            Assert.AreEqual("12345", subject.Id);
            Assert.IsFalse(subject.Permissions.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxySubject_AddPermission_Null()
        {
            var subject = new ProxySubject("12345");
            subject.AddPermission(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProxySubject_AddPermission_InvalidPermission()
        {
            var subject = new ProxySubject("12345");
            subject.AddPermission(new ProxyAccessPermission("1", "123", "456", "SFAA", DateTime.Today));
        }

        [TestMethod]
        public void ProxySubject_AddPermission_ValidPermission()
        {
            var subject = new ProxySubject("12345");
            subject.AddPermission(new ProxyAccessPermission("1", "12345", "456", "SFAA", DateTime.Today));
            Assert.AreEqual(1, subject.Permissions.Count);
        }

        [TestMethod]
        public void ProxySubject_EffectiveDate_NoPermissions()
        {
            var subject = new ProxySubject("12345");
            Assert.IsNull(subject.EffectiveDate);
        }

        [TestMethod]
        public void ProxySubject_EffectiveDate_Permissions()
        {
            var subject = new ProxySubject("12345");
            subject.AddPermission(new ProxyAccessPermission("1", "12345", "456", "SFAA", DateTime.Today.AddDays(-3)));
            subject.AddPermission(new ProxyAccessPermission("1", "12345", "456", "SFAA", DateTime.Today.AddDays(-2)));
            Assert.AreEqual(DateTime.Today.AddDays(-2), subject.EffectiveDate);
        }

        [TestMethod]
        public void ProxySubject_ReauthorizationNeeded_False()
        {
            var subject = new ProxySubject("12345");
            subject.AddPermission(new ProxyAccessPermission("1", "12345", "456", "SFAA", DateTime.Today.AddDays(-3)));
            subject.AddPermission(new ProxyAccessPermission("1", "12345", "456", "SFAA", DateTime.Today.AddDays(-2)));
            Assert.IsFalse(subject.ReauthorizationIsNeeded);
        }

        [TestMethod]
        public void ProxySubject_ReauthorizationNeeded_True()
        {
            var subject = new ProxySubject("12345");
            subject.AddPermission(new ProxyAccessPermission("1", "12345", "456", "SFAA", DateTime.Today.AddDays(-3)) { ReauthorizationDate = DateTime.Today.AddDays(-3) });
            subject.AddPermission(new ProxyAccessPermission("1", "12345", "456", "SFAA", DateTime.Today.AddDays(-2)));
            Assert.IsTrue(subject.ReauthorizationIsNeeded);
        }
    }
}
