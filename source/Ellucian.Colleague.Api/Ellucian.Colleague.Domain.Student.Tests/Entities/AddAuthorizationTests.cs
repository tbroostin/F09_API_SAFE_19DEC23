//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AddAuthorizationTests
    {
        AddAuthorization addAuthorization;
        DateTimeOffset? today;
        string studentId;
        string sectionId;

        [TestInitialize]
        public void Initialize()
        {
            today = DateTime.Now;
            studentId = "AdvisorId";
            sectionId = "SectionId";
            addAuthorization = new AddAuthorization("id", sectionId);
        }

        [TestMethod]
        public void AddAuthorization_Id()
        {
            Assert.AreEqual("id", addAuthorization.Id);
        }

        [TestMethod]
        public void AddAuthorization_SectionId()
        {
            Assert.AreEqual(sectionId, addAuthorization.SectionId);
        }


        [TestMethod]
        public void AddAuthorization_RevokedDefaultsToFalse()
        {
            Assert.IsFalse(addAuthorization.IsRevoked);
        }

        [TestMethod]
        public void AddAuthorization_NullId()
        {
            AddAuthorization auth = new AddAuthorization(null, sectionId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddAuthorization_SectionIdNullException()
        {
            AddAuthorization auth = new AddAuthorization("id", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddAuthorization_SectionIdEmptyException()
        {
            AddAuthorization auth = new AddAuthorization("id", string.Empty);
        }



    }
}
