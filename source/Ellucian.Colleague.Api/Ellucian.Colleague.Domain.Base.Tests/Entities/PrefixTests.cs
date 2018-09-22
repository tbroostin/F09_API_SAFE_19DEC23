// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PrefixTests
    {
        private string code;
        private string description;
        private string abbreviation;
        private Prefix prefix;

        [TestInitialize]
        public void Initialize()
        {
            code = "MRS";
            description = "Mrs";
            abbreviation = "Mrs.";
            prefix = new Prefix(code, description, abbreviation);
        }

        [TestClass]
        public class PrefixConstructor : PrefixTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrefixConstructorNullCode()
            {
                prefix = new Prefix(null, description, abbreviation);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrefixConstructorEmptyCode()
            {
                prefix = new Prefix(string.Empty, description, abbreviation);
            }

            [TestMethod]
            public void PrefixConstructorValidCode()
            {
                prefix = new Prefix(code, description, abbreviation);
                Assert.AreEqual(code, prefix.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrefixConstructorNullDescription()
            {
                prefix = new Prefix(code, null, abbreviation);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrefixConstructorEmptyDescription()
            {
                prefix = new Prefix(code, string.Empty, abbreviation);
            }

            [TestMethod]
            public void PrefixConstructorValidDescription()
            {
                prefix = new Prefix(code, description, abbreviation);
                Assert.AreEqual(description, prefix.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrefixConstructorNullAbbreviation()
            {
                prefix = new Prefix(code, description, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrefixConstructorEmptyAbbreviation()
            {
                prefix = new Prefix(code, description, string.Empty);
            }

            [TestMethod]
            public void PrefixConstructorValidDescriptionAbbreviation()
            {
                prefix = new Prefix(code, description, abbreviation);
                Assert.AreEqual(abbreviation, prefix.Abbreviation);
            }
        }
    }
}
