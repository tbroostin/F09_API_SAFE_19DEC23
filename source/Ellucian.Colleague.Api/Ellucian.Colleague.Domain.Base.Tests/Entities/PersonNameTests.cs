// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonNameTests
    {
        private string first;
        private string middle;
        private string last;
        private PersonName name;

        [TestInitialize]
        public void Initialize()
        {
            first = "First";
            middle = "Middle";
            last = "Last";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonName_Constructor_NullLast()
        {
            name = new PersonName(first, middle, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonName_Constructor_EmptyLast()
        {
            name = new PersonName(first, middle, string.Empty);
        }

        [TestMethod]
        public void PersonName_Constructor_GoodMiddle()
        {
            name = new PersonName(first, middle, last);
            Assert.AreEqual(first, name.GivenName);
            Assert.AreEqual(middle, name.MiddleName);
            Assert.AreEqual(last, name.FamilyName);
        }

        [TestMethod]
        public void PersonName_Constructor_GoodNullMiddle()
        {
            name = new PersonName(first, null, last);
            Assert.AreEqual(first, name.GivenName);
            Assert.AreEqual(string.Empty, name.MiddleName);
            Assert.AreEqual(last, name.FamilyName);
        }

        [TestMethod]
        public void PersonName_Constructor_GoodEmptyMiddle()
        {
            name = new PersonName(first, string.Empty, last);
            Assert.AreEqual(first, name.GivenName);
            Assert.AreEqual(string.Empty, name.MiddleName);
            Assert.AreEqual(last, name.FamilyName);
        }

        [TestMethod]
        public void PersonName_Equals_True()
        {
            name = new PersonName(first, middle, last);
            var name2 = new PersonName(first, middle, last);
            Assert.IsTrue(name.Equals(name2));
            Assert.IsTrue(name2.Equals(name));
        }

        [TestMethod]
        public void PersonName_Equals_False()
        {
            name = new PersonName(first, "", last);
            var name2 = new PersonName(first, middle, last);
            Assert.IsFalse(name.Equals(name2));
            Assert.IsFalse(name2.Equals(name));
        }
    }
}
