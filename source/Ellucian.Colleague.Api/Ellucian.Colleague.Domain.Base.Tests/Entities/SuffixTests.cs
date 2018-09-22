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
    public class SuffixTests
    {
        private string code;
        private string description;
        private string abbreviation;
        private Suffix suffix;

        [TestInitialize]
        public void Initialize()
        {
            code = "SR";
            description = "Sr";
            abbreviation = "Sr.";
            suffix = new Suffix(code, description, abbreviation);
        }

        [TestClass]
        public class SuffixConstructor : SuffixTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SuffixConstructorNullCode()
            {
                suffix = new Suffix(null, description, abbreviation);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SuffixConstructorEmptyCode()
            {
                suffix = new Suffix(string.Empty, description, abbreviation);
            }

            [TestMethod]
            public void SuffixConstructorValidCode()
            {
                suffix = new Suffix(code, description, abbreviation);
                Assert.AreEqual(code, suffix.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SuffixConstructorNullDescription()
            {
                suffix = new Suffix(code, null, abbreviation);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SuffixConstructorEmptyDescription()
            {
                suffix = new Suffix(code, string.Empty, abbreviation);
            }

            [TestMethod]
            public void SuffixConstructorValidDescription()
            {
                suffix = new Suffix(code, description, abbreviation);
                Assert.AreEqual(description, suffix.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SuffixConstructorNullDescriptionAbbreviation()
            {
                suffix = new Suffix(code, description, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SuffixConstructorEmptyAbbreviation()
            {
                suffix = new Suffix(code, description, string.Empty);
            }

            [TestMethod]
            public void SuffixConstructorValidAbbreviation()
            {
                suffix = new Suffix(code, description, abbreviation);
                Assert.AreEqual(abbreviation, suffix.Abbreviation);
            }
        }
    }
}
