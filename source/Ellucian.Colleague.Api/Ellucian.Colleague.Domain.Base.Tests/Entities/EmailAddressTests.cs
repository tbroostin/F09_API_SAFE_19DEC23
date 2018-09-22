using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class EmailAddressTests
    {
        [TestClass]
        public class EmailAddressConstructorTests
        {
            string emailAddressValue;
            string typeCode;
            EmailAddress emailAddress;

            [TestInitialize]
            public void Initialize()
            {
                emailAddressValue = "john.smith@yahoo.com";
                typeCode = "PER";
                emailAddress = new EmailAddress(emailAddressValue, typeCode);
            }

            [TestMethod]
            public void NewEmailAddress_Properties()
            {
                Assert.AreEqual(emailAddressValue, emailAddress.Value);
                Assert.AreEqual(typeCode, emailAddress.TypeCode);
                Assert.IsFalse(emailAddress.IsPreferred);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddEmailAddress_NullValue()
            {
                EmailAddress em = new EmailAddress(null, typeCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddEmailAddress_NullType()
            {
                EmailAddress em = new EmailAddress(emailAddressValue, null);
            }

            [TestMethod]
            public void NewEmailAddress_IsPreferred()
            {
                EmailAddress preferredEmailAddress = new EmailAddress(emailAddressValue, typeCode);
                preferredEmailAddress.IsPreferred = true;
                Assert.AreEqual(emailAddressValue, preferredEmailAddress.Value);
                Assert.AreEqual(typeCode, preferredEmailAddress.TypeCode);
                Assert.IsTrue(preferredEmailAddress.IsPreferred);
            }

        }

        [TestClass]
        public class EmailAddressEqualsTests
        {
            string emailAddressValue;
            string typeCode;
            EmailAddress emailAddress;

            [TestInitialize]
            public void Initialize()
            {
                emailAddressValue = "john.smith@yahoo.com";
                typeCode = "PER";
                emailAddress = new EmailAddress(emailAddressValue, typeCode);
            }

            [TestMethod]
            public void NewEmailAddress_Equals()
            {
                EmailAddress emailAddress2 = new EmailAddress(emailAddressValue, typeCode);
                Assert.IsTrue(emailAddress.Equals(emailAddress2));

            }

            [TestMethod]
            public void NewEmailAddress_NotEquals()
            {
                EmailAddress emailAddress2 = new EmailAddress(emailAddressValue, "PAR");
                Assert.IsFalse(emailAddress.Equals(emailAddress2));
            }

            [TestMethod]
            public void NewEmailAddress_NotEquals2()
            {
                EmailAddress emailAddress2 = new EmailAddress("jon.smith@yahoo.com", "PER");
                Assert.IsFalse(emailAddress.Equals(emailAddress2));
            }

            [TestMethod]
            public void NewEmailAddress_NotEquals3()
            {
                Assert.IsFalse(emailAddress.Equals("emailAddress"));
            }

            [TestMethod]
            public void NewEmailAddress_NotEqualsNull()
            {
                Assert.IsFalse(emailAddress.Equals(null));
            }
        }

        [TestClass]
        public class EmailAddressToStringTests
        {
            string emailAddressValue;
            string typeCode;
            EmailAddress emailAddress;

            [TestInitialize]
            public void Initialize()
            {
                emailAddressValue = "john.smith@yahoo.com";
                typeCode = "PER";
                emailAddress = new EmailAddress(emailAddressValue, typeCode);
            }

            [TestMethod]
            public void EmailAddress_ToString()
            {
                var result = emailAddress.ToString();
                Assert.AreEqual(emailAddressValue, result);
            }
        }

        [TestClass]
        public class EmailAddressGetHashCodeTests
        {
            string emailAddressValue;
            string typeCode;
            EmailAddress emailAddress;
            EmailAddress emailAddress2;

            [TestInitialize]
            public void Initialize()
            {
                emailAddressValue = "john.smith@yahoo.com";
                typeCode = "PER";
                emailAddress = new EmailAddress(emailAddressValue, typeCode);
                emailAddress2 = new EmailAddress(emailAddressValue, typeCode);
            }

            [TestMethod]
            public void EmailAddress_GetHashCode_Equals()
            {
                Assert.AreEqual(emailAddress.GetHashCode(), emailAddress2.GetHashCode());
            }

            [TestMethod]
            public void EmailAddress_GetHashCode_NotEquals1()
            {
                emailAddress2 = new EmailAddress(emailAddressValue, "PAR");
                Assert.AreNotEqual(emailAddress.GetHashCode(), emailAddress2.GetHashCode());
            }

            [TestMethod]
            public void EmailAddress_GetHashCode_NotEquals2()
            {
                emailAddress2 = new EmailAddress("jon.smith@yahoo.com", typeCode);
                Assert.AreNotEqual(emailAddress.GetHashCode(), emailAddress2.GetHashCode());
            }

            [TestMethod]
            public void EmailAddress_GetHashCode_NotEquals3()
            {
                emailAddress2 = new EmailAddress("jon.smith@yahoo.com", "PAR");
                Assert.AreNotEqual(emailAddress.GetHashCode(), emailAddress2.GetHashCode());
            }
        }
    }
}
