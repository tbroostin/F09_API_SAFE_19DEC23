using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Planning.Entities;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{
    [TestClass]
    public class AdvisorTests
    {
        [TestClass]
        public class AdvisorConstructor
        {
            private string id;
            private string lastName;
            private Advisor advisorEntity;

            [TestInitialize]
            public void Initialize()
            {
                id = "0000001";
                lastName = "Smith";
                advisorEntity = new Advisor(id, lastName);
            }

            [TestCleanup]
            public void Cleanup()
            {
                advisorEntity = null;
            }

            [TestMethod]
            public void Advisor_Id()
            {
                Assert.AreEqual(id, advisorEntity.Id);    
            }

            [TestMethod]
            public void Advisor_LastName()
            {
                Assert.AreEqual(lastName, advisorEntity.LastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdvisorId_Null_ThrowsException()
            {
                new Advisor(null, lastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdvisorId_Empty_ThrowsException()
            {
                new Advisor(string.Empty, lastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdvisorLastName_Null_ThrowsException()
            {
                new Advisor(id, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdvisorLastName_Empty_ThrowsException()
            {
                new Advisor(id, string.Empty);
            }
        }

        [TestClass]
        public class AdvisorAddAdvisee
        {
            private string id;
            private string lastName;
            private Advisor advisorEntity;

            [TestInitialize]
            public void Initialize()
            {
                id = "0000001";
                lastName = "Smith";
                advisorEntity = new Advisor(id, lastName);
            }

            [TestCleanup]
            public void Cleanup()
            {
                advisorEntity = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddAdvisee_Advisee_Null()
            {
                advisorEntity.AddAdvisee(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddDuplicateAdvisee()
            {
                advisorEntity.AddAdvisee("123456");
                advisorEntity.AddAdvisee("123456");
            }

            [TestMethod]
            public void AddAdvisee_Success()
            {
                advisorEntity.AddAdvisee("123456");
                Assert.AreEqual("123456", advisorEntity.Advisees.ElementAt(0));
            }
        }

        [TestClass]
        public class AdvisorGetEmailAddress
        {
            private string id;
            private string lastName;
            Ellucian.Colleague.Domain.Planning.Entities.Advisor advisorEntity;

            [TestInitialize]
            public void Initialize()
            {
                id = "0000001";
                lastName = "Smith";
                advisorEntity = new Advisor(id, lastName);
            }

            [TestCleanup]
            public void Cleanup()
            {
                advisorEntity = null;
            }

            [TestMethod]
            public void GetsEmailAddress_ReturnsEmptyListIfNoEmailAddresses()
            {
                var emailAddresses = advisorEntity.GetEmailAddresses("FAC");
                Assert.AreEqual(0, emailAddresses.Count());
            }

            [TestMethod]
            public void GetsEmailAddress_ReturnsEmailAddressesOfSpecifiedType()
            {
                advisorEntity.AddEmailAddress(new Base.Entities.EmailAddress("sss@domain1.com", "ONE"));
                advisorEntity.AddEmailAddress(new Base.Entities.EmailAddress("not@domain2.com", "TWO"));
                advisorEntity.AddEmailAddress(new Base.Entities.EmailAddress("suzie@email.com", "ONE"));
                var emailAddresses = advisorEntity.GetEmailAddresses("ONE");
                Assert.AreEqual(2, emailAddresses.Count());
                Assert.AreEqual(advisorEntity.EmailAddresses.ElementAt(0).Value, emailAddresses.ElementAt(0));
                Assert.AreEqual(advisorEntity.EmailAddresses.ElementAt(2).Value, emailAddresses.ElementAt(1));
            }

        }

        [TestClass]
        public class AdvisorEquals
        {
            [TestMethod]
            public void SameId_Equal()
            {
                var advisor1 = new Advisor("00001", "name1");
                var advisor2 = new Advisor("00001", "name2");
                Assert.IsTrue(advisor1.Equals(advisor2));
            }

            [TestMethod]
            public void DifferentId_NotEqual()
            {
                var advisor1 = new Advisor("00001", "name1");
                var advisor2 = new Advisor("00002", "name2");
                Assert.IsFalse(advisor1.Equals(advisor2));
            }

            [TestMethod]
            public void SameNumberDifferentForm_NotEqual()
            {
                var advisor1 = new Advisor("00001", "name1");
                var advisor2 = new Advisor("1", "name2");
                Assert.IsFalse(advisor1.Equals(advisor2));
            }
        }
    }
}
