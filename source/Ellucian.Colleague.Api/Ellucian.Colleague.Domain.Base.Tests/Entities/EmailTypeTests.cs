// Copyright 2015-16 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class EmailTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private EmailTypeCategory emailTypeCategory;
        private EmailType emailType;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "PER";
            description = "Personal";
            emailTypeCategory = EmailTypeCategory.Personal;
        }

        [TestClass]
        public class EmailTypeConstructor : EmailTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmailTypeConstructorNullGuid()
            {
                emailType = new EmailType(null, code, description, emailTypeCategory);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmailTypeConstructorEmptyGuid()
            {
                emailType = new EmailType(string.Empty, code, description, emailTypeCategory);
            }

            [TestMethod]
            public void EmailTypeConstructorValidGuid()
            {
                emailType = new EmailType(guid, code, description, emailTypeCategory);
                Assert.AreEqual(guid, emailType.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmailTypeConstructorNullCode()
            {
                emailType = new EmailType(guid, null, description, emailTypeCategory);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmailTypeConstructorEmptyCode()
            {
                emailType = new EmailType(guid, string.Empty, description, emailTypeCategory);
            }

            [TestMethod]
            public void EmailTypeConstructorValidCode()
            {
                emailType = new EmailType(guid, code, description, emailTypeCategory);
                Assert.AreEqual(code, emailType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmailTypeConstructorNullDescription()
            {
                emailType = new EmailType(guid, code, null, emailTypeCategory);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmailTypeConstructorEmptyDescription()
            {
                emailType = new EmailType(guid, code, string.Empty, emailTypeCategory);
            }

            [TestMethod]
            public void EmailTypeConstructorValidDescription()
            {
                emailType = new EmailType(guid, code, description, emailTypeCategory);
                Assert.AreEqual(description, emailType.Description);
            }

            [TestMethod]
            public void EmailTypeConstructorValidEmailTypeCategory()
            {
                emailType = new EmailType(guid, code, description, emailTypeCategory);
                Assert.AreEqual(emailTypeCategory, emailType.EmailTypeCategory);
            }
        }
    }
}
