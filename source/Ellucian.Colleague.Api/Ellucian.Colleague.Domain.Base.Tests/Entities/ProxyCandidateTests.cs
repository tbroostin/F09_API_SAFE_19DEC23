// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ProxyCandidateTests
    {
        private string subject = "Subject";
        private string relationship = "P";
        private List<string> nullPerms;
        private List<string> emptyPerms;
        private List<string> goodPerms;
        private string first = "first";
        private string last = "last";
        private string email = "me@email.com";
        private List<PersonMatchResult> nullResults;
        private List<PersonMatchResult> emptyResults;
        private List<PersonMatchResult> goodResults;

        [TestInitialize]
        public void ProxyCandidateTests_Initialize()
        {
            nullPerms = null;
            emptyPerms = new List<string>();
            goodPerms = new List<string>() { "SomePermission" };
            nullResults = null;
            emptyResults = new List<PersonMatchResult>();
            goodResults = new List<PersonMatchResult>() { new PersonMatchResult("personId", 90, "D") };
        }

        [TestMethod]
        public void ProxyCandidate_Constructor_Good()
        {
            var result = new ProxyCandidate(subject, relationship, goodPerms, first, last, email, goodResults);
            Assert.AreEqual(subject, result.ProxySubject);
            Assert.AreEqual(relationship, result.RelationType);
            Assert.AreEqual(goodPerms.Count, result.GrantedPermissions.Count());
            Assert.AreEqual(first, result.FirstName);
            Assert.AreEqual(last, result.LastName);
            Assert.AreEqual(email, result.EmailAddress);
            Assert.AreEqual(goodResults.Count, result.ProxyMatchResults.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_NullSubject()
        {
            var result = new ProxyCandidate(null, relationship, goodPerms, first, last, email, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_EmptySubject()
        {
            var result = new ProxyCandidate(string.Empty, relationship, goodPerms, first, last, email, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_NullRelationType()
        {
            var result = new ProxyCandidate(subject, null, goodPerms, first, last, email, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_EmptyRelationType()
        {
            var result = new ProxyCandidate(subject, string.Empty, goodPerms, first, last, email, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_NullPermissions()
        {
            var result = new ProxyCandidate(subject, relationship, nullPerms, first, last, email, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_EmptyPermissions()
        {
            var result = new ProxyCandidate(subject, relationship, emptyPerms, first, last, email, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_NullFirstName()
        {
            var result = new ProxyCandidate(subject, relationship, goodPerms, null, last, email, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_EmptyFirstName()
        {
            var result = new ProxyCandidate(subject, relationship, goodPerms, string.Empty, last, email, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_NullLastName()
        {
            var result = new ProxyCandidate(subject, relationship, goodPerms, first, null, email, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_EmptyLastName()
        {
            var result = new ProxyCandidate(subject, relationship, goodPerms, first, string.Empty, email, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_NullEmailAddress()
        {
            var result = new ProxyCandidate(subject, relationship, goodPerms, first, last, null, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_EmptyEmailAddress()
        {
            var result = new ProxyCandidate(subject, relationship, goodPerms, first, last, string.Empty, goodResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_NullProxyMatchResults()
        {
            var result = new ProxyCandidate(subject, relationship, goodPerms, first, last, email, nullResults);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyCandidate_Constructor_EmptyProxyMatchResults()
        {
            var result = new ProxyCandidate(subject, relationship, goodPerms, first, last, email, emptyResults);
        }

    }
}
