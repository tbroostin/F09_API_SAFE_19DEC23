// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RegistrationReasonTests
    {
        [TestClass]
        public class RegistrationReason_Constructor_Tests
        {
            private string code;
            private string desc;
            private RegistrationReason entity;

            [TestInitialize]
            public void Initialize()
            {
                code = "FUN";
                desc = "Just for fun";
                entity = new RegistrationReason(code, desc);
            }

            [TestMethod]
            public void RegistrationReason_Code()
            {
                Assert.AreEqual(code, entity.Code);
            }

            [TestMethod]
            public void RegistrationReason_Description()
            {
                Assert.AreEqual(desc, entity.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationReason_CodeNullException()
            {
                new RegistrationReason(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationReason_DescNullException()
            {
                new RegistrationReason(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationReasonCodeEmptyException()
            {
                new RegistrationReason(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationReasonDescEmptyException()
            {
                new RegistrationReason(code, string.Empty);
            }

        }
    }
}
