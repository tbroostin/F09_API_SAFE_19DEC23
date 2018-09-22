// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonFormattedNameTests
    {

        private string type;
        private string name;
        private PersonFormattedName pfn;

        [TestInitialize]
        public void Initialize()
        {
            type = "AAA";
            name = "This is a very long name description";
            pfn = new PersonFormattedName(type, name);
        }
        [TestCleanup]
        public void Cleanup()
        {
            pfn = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonFormattedNameTypeNull()
        {
            pfn = new PersonFormattedName(null, name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonFormattedNameTypeEmpty()
        {
            pfn = new PersonFormattedName(string.Empty, name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonFormattedName_NameNull()
        {
            pfn = new PersonFormattedName(type, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonFormattedName_NameEmpty()
        {
            pfn = new PersonFormattedName(type, string.Empty);
        }
        [TestMethod]
        public void PersonFormattedName_Valid()
        {
            pfn = new PersonFormattedName(type, name);
            Assert.AreEqual(type, pfn.Type);
            Assert.AreEqual(name, pfn.Name);
        }
    }
}