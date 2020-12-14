// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RegistrationMarketingSourceTests
    {
        [TestClass]
        public class RegistrationMarketingSource_Constructor_Tests
        {
            private string code;
            private string desc;
            private RegistrationMarketingSource entity;

            [TestInitialize]
            public void Initialize()
            {
                code = "NEWSAD";
                desc = "From a newspaper ad";
                entity = new RegistrationMarketingSource(code, desc);
            }

            [TestMethod]
            public void RegistrationMarketingSource_Code()
            {
                Assert.AreEqual(code, entity.Code);
            }

            [TestMethod]
            public void RegistrationMarketingSource_Description()
            {
                Assert.AreEqual(desc, entity.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationMarketingSource_CodeNullException()
            {
                new RegistrationMarketingSource(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationMarketingSource_DescNullException()
            {
                new RegistrationMarketingSource(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationMarketingSourceCodeEmptyException()
            {
                new RegistrationMarketingSource(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationMarketingSourceDescEmptyException()
            {
                new RegistrationMarketingSource(code, string.Empty);
            }

        }
    }
}
