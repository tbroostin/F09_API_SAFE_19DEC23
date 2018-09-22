// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class SearchPersonTests
    {
        string personId = "0003315";
        string lastName = "Smith";
        SearchPerson sp;

        [TestInitialize]
        public void Initialize()
        {
            sp = new SearchPerson(personId, lastName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SearchPerson_Constructor_NullPersonId()
        {
            sp = new SearchPerson(null, lastName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SearchPerson_Constructor_EmptyPersonId()
        {
            sp = new SearchPerson(string.Empty, lastName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SearchPerson_Constructor_NullLastName()
        {
            sp = new SearchPerson(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SearchPerson_Constructor_EmptyLastName()
        {
            sp = new SearchPerson(personId, string.Empty);
        }

        [TestMethod]
        public void SearchPerson_Constructor_VerifyPersonId()
        {
            Assert.AreEqual(personId, sp.Id);
        }

        [TestMethod]
        public void SearchPerson_Constructor_VerifyLastName()
        {
            Assert.AreEqual(lastName, sp.LastName);
        }

        [TestMethod]
        public void SearchPerson_Equals_Null()
        {
            Assert.IsFalse(sp.Equals(null));
        }

        [TestMethod]
        public void SearchPerson_Equals_NonSearchPerson()
        {
            Assert.IsFalse(sp.Equals("abc"));
        }

        [TestMethod]
        public void SearchPerson_Equals_SameId()
        {
            var sp2 = new SearchPerson(personId, "Jones");
            Assert.IsTrue(sp.Equals(sp2));
        }

        [TestMethod]
        public void SearchPerson_GetHashCode()
        {
            var sp2 = new SearchPerson(personId, "Jones");
            Assert.AreEqual(sp.GetHashCode(), sp2.GetHashCode());
        }

        [TestMethod]
        public void SearchPerson_ToString()
        {
            Assert.AreEqual(personId, sp.ToString());
        }
    }
}
