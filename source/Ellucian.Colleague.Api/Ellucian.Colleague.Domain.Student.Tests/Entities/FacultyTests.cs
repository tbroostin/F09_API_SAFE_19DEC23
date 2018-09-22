using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities 
{
    [TestClass]
    public class FacultyTests {
        [TestClass]
        public class FacultyAddEmailAddressTests {
            string id;
            string lastName;
            Faculty fac;
            EmailAddress email;

            [TestInitialize]
            public void Initialize() {
                id = "0012345";
                lastName = "Smith";
                email = new EmailAddress("john.smith@gmail.com", "PER");
                fac = new Faculty(id, lastName);
                fac.AddEmailAddress(email);
            }

            //[TestMethod]
            //public void AddEmailAddress_Success()
            //{
            //    Assert.AreEqual(1, fac._PersonEmailAddresses.Count());
            //    Assert.AreEqual(email, fac.PersonEmailAddresses.First());
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddEmailAddress_NullAddress() {
                fac.AddEmailAddress(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddEmailAddress_DuplicateAddress() {
                fac.AddEmailAddress(email);
            }
        }

        [TestClass]
        public class FacultyAddPhoneTests {
            string id;
            string lastName;
            Faculty fac;
            Phone phone;

            [TestInitialize]
            public void Initialize() {
                id = "0012345";
                lastName = "Smith";
                phone = new Phone("111-222-3333", "PER", "4444");
                fac = new Faculty(id, lastName);
                fac.AddPhone(phone);
            }

            //[TestMethod]
            //public void AddPhone_Success()
            //{
            //    Assert.AreEqual(1, fac.PersonalPhones.Count());
            //    Assert.AreEqual(phone, fac.PersonalPhones.ElementAt(0));
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddPhone_NullPhone() {
                fac.AddPhone(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddPhone_DuplicateException() {
                // Try adding a phone number with same number, type and extension.
                Phone phone2 = new Phone("111-222-3333", "PER", "4444");
                fac.AddPhone(phone2);
            }

            //[TestMethod]
            //public void AddPhone_Multiple()
            //{
            //    Phone phone2 = new Phone("333-4444");
            //    fac.AddPhone(phone2);
            //    Assert.AreEqual(2, fac.PersonalPhones.Count());
            //}
        }

        [TestClass]
        public class FacultyGetEmailAddressesTests {
            string id;
            string lastName;
            Faculty fac;
            EmailAddress email1;
            EmailAddress email2;
            EmailAddress email3;

            [TestInitialize]
            public void Initialize() {
                id = "0012345";
                lastName = "Smith";
                email1 = new EmailAddress("john.smith@gmail.com", "PER");
                email2 = new EmailAddress("faculty@abc.edu", "FAC");
                email3 = new EmailAddress("another@xyz.com", "FAC");

                fac = new Faculty(id, lastName);
                fac.AddEmailAddress(email1);
                fac.AddEmailAddress(email2);
                fac.AddEmailAddress(email3);
            }

            [TestMethod]
            public void GetFacultyEmailAddresses_Success() {
                IEnumerable<string> facultyEmails = fac.GetFacultyEmailAddresses("FAC");
                Assert.AreEqual(2, facultyEmails.Count());
                Assert.AreEqual(email2.Value, facultyEmails.ElementAt(0));
                Assert.AreEqual(email3.Value, facultyEmails.ElementAt(1));
            }

            [TestMethod]
            public void GetFacultyEmailAddresses_NoneMatch() {
                IEnumerable<string> facultyEmails = fac.GetFacultyEmailAddresses("XYZ");
                Assert.AreEqual(0, facultyEmails.Count());
            }

            [TestMethod]
            public void GetFacultyEmailAddresses_NoEmails() {
                Faculty fac2 = new Faculty(id, lastName);
                IEnumerable<string> facultyEmails = fac2.GetFacultyEmailAddresses("XYZ");
                Assert.AreEqual(0, facultyEmails.Count());
            }
        }

        [TestClass]
        public class FacultyGetPhonesTests {
            string id;
            string lastName;
            Faculty faculty;

            [TestInitialize]
            public void Initialize() {
                id = "0012345";
                lastName = "Smith";

                faculty = new Faculty(id, lastName);
                faculty.AddPhone(new Phone("Faculty Phone 1", "FAC", "Ext F1"));
                faculty.AddPhone(new Phone("Non Faculty Phone", "PER"));
            }

            [TestMethod]
            public void FacultyWithNoPhones() {
                Faculty facultyNoPhones = new Faculty(id, lastName);
                IEnumerable<Phone> facPhones = facultyNoPhones.GetFacultyPhones("FAC");
                Assert.AreEqual(0, facPhones.Count());

            }

            [TestMethod]
            public void FacultyWithPhones_NoneMatch() {
                IEnumerable<Phone> facPhones = faculty.GetFacultyPhones("XYZ");
                Assert.AreEqual(0, facPhones.Count());
            }

            [TestMethod]
            public void FacultyWithPhone_Matches() {
                IEnumerable<Phone> facPhones = faculty.GetFacultyPhones("FAC");
                Assert.AreEqual(1, facPhones.Count());
                Phone phone1 = facPhones.Where(p => p.Number == "Faculty Phone 1").FirstOrDefault();
                Assert.AreEqual("Faculty Phone 1", phone1.Number);
                Assert.AreEqual("FAC", phone1.TypeCode);
                Assert.AreEqual("Ext F1", phone1.Extension);
            }
        }
    }
}
