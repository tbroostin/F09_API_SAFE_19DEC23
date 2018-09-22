// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace Ellucian.Colleague.Dtos.Tests.Attributes
{
    [TestClass]
    public class FilterPropertyAttributeTests
    {
        private string name;
        private string[] names;
        private bool ignore;
        private FilterPropertyAttribute filterPropertyAttribute;

        [TestInitialize]
        public void Initialize()
        {
            name = "value";
            names = new string[] { "value" };
            ignore = false;
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FilterPropertyAttributeConstructorNullCode()
        {
            filterPropertyAttribute = new FilterPropertyAttribute("");
        }


        [TestMethod]
        public void filterPropertyAttributeConstructorValidName()
        {
            filterPropertyAttribute = new FilterPropertyAttribute(name);
            Assert.AreEqual(names[0], filterPropertyAttribute.Name[0]);
            Assert.AreEqual(ignore, filterPropertyAttribute.Ignore);
        }

        [TestMethod]
        public void filterPropertyAttributeConstructorValidNames()
        {
            filterPropertyAttribute = new FilterPropertyAttribute(names);
            Assert.AreEqual(names, filterPropertyAttribute.Name);
            Assert.AreEqual(ignore, filterPropertyAttribute.Ignore);
        }

        [TestMethod]
        public void filterPropertyAttributeConstructorValidNameIgnore()
        {
            filterPropertyAttribute = new FilterPropertyAttribute(name, true);
            Assert.AreEqual(names[0], filterPropertyAttribute.Name[0]);
            Assert.AreEqual(true, filterPropertyAttribute.Ignore);
        }

        [TestMethod]
        public void filterPropertyAttributeConstructorValidNamesIgnore()
        {
            filterPropertyAttribute = new FilterPropertyAttribute(names, true);
            Assert.AreEqual(names, filterPropertyAttribute.Name);
            Assert.AreEqual(true, filterPropertyAttribute.Ignore);
        }
    }
}
