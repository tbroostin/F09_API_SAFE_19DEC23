using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonTests
    {
        string guid;
        string id;
        string prefix;
        string lastname;
        string firstname;
        string middlename;
        string suffix;
        string nickname;
        DateTime? dateOfBirth;
        DateTime? deceasedDate;
        string governmentId;

        string preferredName;
        List<string> preferredAddress;

        [TestInitialize]
        public void Initialize()
        {
            guid = "5674f28b-b216-4055-b236-81a922d93b4c";
            id = "0012345";
            prefix = "Mr.";
            lastname = "Smith";
            firstname = "John";
            middlename = "Franklin";
            suffix = "Jr.";
            nickname = "Jo";
            dateOfBirth = new DateTime(1940, 4, 1);
            deceasedDate = new DateTime(2040, 3, 9);
            governmentId = "111-11-1111";

            preferredName = "Mr. John F. Smith";
            preferredAddress = new List<string> { "1234 Main St.", "Apartment 113B", "Fairfax, VA 22031" };
        }

        [TestMethod]
        public void PersonConstructorTest()
        {
            Person target = new Person(id, lastname);

            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(lastname, target.LastName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonConstructorExceptionNullLastNameArgument()
        {
            Person target = new Person(id, null);
        }

        [TestMethod]
        public void PersonBuildTest()
        {
            Person target = BuildPerson();

            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(lastname, target.LastName);
            Assert.AreEqual(firstname, target.FirstName);
            Assert.AreEqual(middlename, target.MiddleName);
            Assert.AreEqual(guid, target.Guid);
            Assert.AreEqual(prefix, target.Prefix);
            Assert.AreEqual(suffix, target.Suffix);
            Assert.AreEqual(nickname, target.Nickname);
            Assert.AreEqual(dateOfBirth, target.BirthDate);
            Assert.AreEqual(deceasedDate, target.DeceasedDate);
            Assert.AreEqual(governmentId, target.GovernmentId);            
            Assert.AreEqual(preferredName, target.PreferredName);
            Assert.AreEqual(preferredAddress, target.PreferredAddress);
        }

        [TestMethod]
        public void PersonAddId()
        {
            Person target = new Person(null, lastname);
            target.Id = "1234";
            Assert.AreEqual("1234", target.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PersonChangeIdException()
        {
            Person target = new Person(id, lastname);
            target.Id = "1234";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PersonChangeIdToNullException()
        {
            Person target = new Person(id, lastname);
            target.Id = null;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PersonChangeGuidException()
        {
            Person target = new Person(null, lastname);
            target.Guid = "26702670-478a-4486-9139-0b1030ce30ec";
            target.Guid = "957f179a-234f-45f8-8f21-63c7c749d470";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PersonChangeGuidToNullException()
        {
            Person target = new Person(id, lastname);
            target.Guid = "26702670-478a-4486-9139-0b1030ce30ec";
            target.Guid = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Person_AddEmailAddress_Null()
        {
            Person target = new Person(id, lastname);
            target.AddEmailAddress(null);
        }

        [TestMethod]
        public void Person_AddEmailAddress_Verify()
        {
            Person target = new Person(id, lastname);
            target.AddEmailAddress(new EmailAddress("abc@123.com","BUS"));
            Assert.AreEqual(1, target.EmailAddresses.Count);
            Assert.AreEqual("abc@123.com", target.EmailAddresses[0].Value);
            Assert.AreEqual("BUS", target.EmailAddresses[0].TypeCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Person_AddEmailAddress_Duplicate()
        {
            Person target = new Person(id, lastname);
            target.AddEmailAddress(new EmailAddress("abc@123.com", "BUS"));
            target.AddEmailAddress(new EmailAddress("abc@123.com", "BUS"));
        }

        [TestMethod]
        public void Person_GetEmailAddresses_NoneForType()
        {
            Person target = new Person(id, lastname);
            target.AddEmailAddress(new EmailAddress("abc@123.com", "BUS"));
            var emails = target.GetEmailAddresses("ABC");
            Assert.AreEqual(0, emails.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Person_AddPersonAlt_Null()
        {
            Person target = new Person(id, lastname);
            target.AddPersonAlt(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Person_AddPersonAlt_Duplicate()
        {
            Person target = new Person(id, lastname);
            PersonAlt alt1 = new PersonAlt("0012346", "Applicant");
            target.AddPersonAlt(alt1);
            target.AddPersonAlt(alt1);
        }

        [TestMethod]
        public void Person_AddPersonAlt_Valid()
        {
            Person target = new Person(id, lastname);
            PersonAlt alt1 = new PersonAlt("0012346", "Applicant");
            target.AddPersonAlt(alt1);
            Assert.AreEqual(1, target.PersonAltIds.Count);
        }

        [TestMethod]
        public void Person_EqualsNull_VerifyFalse()
        {
            Person target = new Person(id, lastname);
            Assert.IsFalse(target.Equals(null));
        }

        [TestMethod]
        public void Person_EqualsNonPerson_VerifyFalse()
        {
            Person target = new Person(id, lastname);
            Assert.IsFalse(target.Equals("abc"));
        }

        [TestMethod]
        public void Person_EqualsSameId_VerifyTrue()
        {
            Person target = new Person(id, lastname);
            Person target2 = new Person(id, lastname+"2");
            Assert.IsTrue(target.Equals(target2));
        }

        [TestMethod]
        public void Person_GetHashCode()
        {
            Person target = new Person(id, lastname);
            Person target2 = new Person(id, lastname + "2");
            Assert.AreEqual(target.GetHashCode(), target2.GetHashCode());
        }

        [TestMethod]
        public void Person_ToString()
        {
            var target = new Person(id, lastname).ToString();
            Assert.AreEqual(id, target);
        }

        private Person BuildPerson()
        {
            Person target = new Person(id, lastname);
            target.FirstName = firstname;
            target.MiddleName = middlename;
            target.PreferredName = preferredName;
            target.PreferredAddress = preferredAddress;
            target.Guid = guid;
            target.Prefix = prefix;
            target.Suffix = suffix;
            target.Nickname = nickname;
            target.BirthDate = dateOfBirth;
            target.DeceasedDate = deceasedDate;
            target.GovernmentId = governmentId;

            return target;
        }
    }
}
