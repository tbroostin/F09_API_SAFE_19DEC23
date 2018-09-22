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
    public class LanguageTests
    {
        private string code;
        private string description;
        private Language language;

        [TestInitialize]
        public void Initialize()
        {
            code = "HIS";
            description = "Hispanic/Latino";
            language = new Language(code, description);
        }

        [TestClass]
        public class LanguageConstructor : LanguageTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LanguageConstructorNullCode()
            {
                language = new Language(null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LanguageConstructorEmptyCode()
            {
                language = new Language(string.Empty, description);
            }

            [TestMethod]
            public void LanguageConstructorValidCode()
            {
                language = new Language(code, description);
                Assert.AreEqual(code, language.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LanguageConstructorNullDescription()
            {
                language = new Language(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LanguageConstructorEmptyDescription()
            {
                language = new Language(code, string.Empty);
            }

            [TestMethod]
            public void LanguageConstructorValidDescription()
            {
                language = new Language(code, description);
                Assert.AreEqual(description, language.Description);
            }
        }
    }
}
