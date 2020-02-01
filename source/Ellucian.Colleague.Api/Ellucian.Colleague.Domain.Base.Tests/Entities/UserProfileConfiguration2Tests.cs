using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class UserProfileConfiguration2Tests
    {
        [TestClass]
        public class UserProfileConfiguration2_Constructor
        {
            [TestMethod]
            public void ViewableAddressTypes()
            {
                var configuration = new UserProfileConfiguration2();
                Assert.AreEqual(0, configuration.ViewableAddressTypes.Count());
            }

            [TestMethod]
            public void ViewablePhoneTypes()
            {
                var configuration = new UserProfileConfiguration2();
                Assert.AreEqual(0, configuration.ViewablePhoneTypes.Count());
            }

            [TestMethod]
            public void ViewableEmailTypes()
            {
                var configuration = new UserProfileConfiguration2();
                Assert.AreEqual(0, configuration.ViewableEmailTypes.Count());
            }

            [TestMethod]
            public void UpdatableEmailTypes()
            {
                var configuration = new UserProfileConfiguration2();
                Assert.AreEqual(0, configuration.UpdatableEmailTypes.Count());
            }

            [TestMethod]
            public void UpdatableAddressTypes()
            {
                var configuration = new UserProfileConfiguration2();
                Assert.AreEqual(0, configuration.UpdatableAddressTypes.Count());
            }

            [TestMethod]
            public void ChangeRequestAddressTypes()
            {
                var configuration = new UserProfileConfiguration2();
                Assert.AreEqual(0, configuration.ChangeRequestAddressTypes.Count());
            }

            [TestMethod]
            public void AllBooleansFalse()
            {
                var configuration = new UserProfileConfiguration2();
                Assert.IsFalse(configuration.AllAddressTypesAreViewable);
                Assert.IsFalse(configuration.AllEmailTypesAreUpdatable);
                Assert.IsFalse(configuration.AllEmailTypesAreViewable);
                Assert.IsFalse(configuration.AllPhoneTypesAreViewable);
                Assert.IsFalse(configuration.CanUpdateEmailWithoutPermission);
            }
        }

        [TestClass]
        public class UserProfileConfiguration2_UpdateAddressTypeConfiguration
        {
            private List<AddressRelationType> allAddressRelationTypes;
            private List<AddressRelationType> noWebAddressRelationTypes;

            [TestInitialize]
            public void TestInitialize()
            {
                noWebAddressRelationTypes = new List<AddressRelationType>()
                {
                    new AddressRelationType("HO", "Home", "1", "PHA"),
                    new AddressRelationType("VA", "Vacation", "", ""),
                    new AddressRelationType("B", "Business", "3", ""),
                    new AddressRelationType("LO", "Local", "", ""),
                    new AddressRelationType("PA", "Parent", "", "")
                };
                allAddressRelationTypes = new List<AddressRelationType>()
                {
                    new AddressRelationType("HO", "Home", "1", "PHA"),
                    new AddressRelationType("VA", "Vacation", "", ""),
                    new AddressRelationType("B", "Business", "2", ""),
                    new AddressRelationType("LO", "Local", "", ""),
                    new AddressRelationType("PA", "Parent", "", ""),
                    new AddressRelationType("WB", "Web Obtained", "3", ""),
                    new AddressRelationType("WB2", "Web Obtained 2", "3", "")
                };
            }


            [TestMethod]
            public void AddressTypes_FalseBoolean()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() {"HO","B"}, new List<string>() { "HO" }, null);
                Assert.IsTrue(configuration.ViewableAddressTypes.Contains("HO"));
                Assert.IsTrue(configuration.ViewableAddressTypes.Contains("B"));
                Assert.IsTrue(configuration.UpdatableAddressTypes.Contains("HO"));
                Assert.IsFalse(configuration.AllAddressTypesAreViewable);
            }

            [TestMethod]
            public void NullViewableandUpdatableAddressTypes()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(false, null, null, null);
                Assert.AreEqual(0, configuration.ViewableAddressTypes.Count());
                Assert.AreEqual(0, configuration.UpdatableAddressTypes.Count());
                Assert.AreEqual(0, configuration.ChangeRequestAddressTypes.Count());
            }
            [TestMethod]
            public void EmptyViewableandUpdatableAddressTypes()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>(), new List<string>(), null);
                Assert.AreEqual(0, configuration.ViewableAddressTypes.Count());
                Assert.AreEqual(0, configuration.UpdatableAddressTypes.Count());
                Assert.AreEqual(0, configuration.ChangeRequestAddressTypes.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void BothTrueBooleanAndViewableAddressTypes_ThrowsException()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(true, new List<string>() { "ho" }, new List<string>(), null);
            }

            [TestMethod]
            public void DoesNotAddDuplicateViewableAddressType()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() {"HO","HO"}, new List<string>(), null);
                Assert.AreEqual(1, configuration.ViewableAddressTypes.Where(a => a == "HO").Count());
            }

            [TestMethod]
            public void DoesNotAddDuplicateUpdatableAddressType()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "HO" }, new List<string>() { "HO", "HO" }, null);
                Assert.AreEqual(1, configuration.UpdatableAddressTypes.Where(a => a == "HO").Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void UpdatableAddressTypeNotInViewableAddressList_ThrowsException()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() {"HO","B"}, new List<string>() { "HO", "WB" }, null);
            }

            [TestMethod]
            public void ChangeRequestAddressTypes_Null()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() {"HO","LO","WB"}, new List<string>() { "HO", "LO", "WB" }, null);
                Assert.AreEqual(3, configuration.ViewableAddressTypes.Count());
                Assert.AreEqual(3, configuration.UpdatableAddressTypes.Count());
                Assert.AreEqual(0, configuration.ChangeRequestAddressTypes.Count());
            }

            [TestMethod]
            public void ChangeRequestAddressTypes_Empty()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "HO", "LO", "WB" }, new List<string>() { "HO", "LO", "WB" }, new List<AddressRelationType>());
                Assert.AreEqual(3, configuration.ViewableAddressTypes.Count());
                Assert.AreEqual(3, configuration.UpdatableAddressTypes.Count());
                Assert.AreEqual(0, configuration.ChangeRequestAddressTypes.Count());
            }

            [TestMethod]
            public void ChangeRequestAddressTypes_NoWebTypeOnes()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "HO", "LO", "WB" }, new List<string>() { "HO", "LO", "WB" }, noWebAddressRelationTypes);
                Assert.AreEqual(3, configuration.ViewableAddressTypes.Count());
                Assert.AreEqual(3, configuration.UpdatableAddressTypes.Count());
                Assert.AreEqual(0, configuration.ChangeRequestAddressTypes.Count());
            }

            [TestMethod]
            public void ChangeRequestAddressTypes_ValidOnes()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "HO", "LO", "WB" }, new List<string>() { "HO", "LO", "WB" }, allAddressRelationTypes);
                Assert.AreEqual(3, configuration.ViewableAddressTypes.Count());
                Assert.AreEqual(3, configuration.UpdatableAddressTypes.Count());
                Assert.AreEqual(1, configuration.ChangeRequestAddressTypes.Count());
            }
        }

        [TestClass]
        public class UpdateEmailTypeConfiguration
        {
            [TestMethod]
            public void NoViewOrUpdate()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateEmailTypeConfiguration(false, null, false, null);
                Assert.AreEqual(0, configuration.ViewableEmailTypes.Count());
                Assert.AreEqual(0, configuration.UpdatableEmailTypes.Count());
                Assert.IsFalse(configuration.AllEmailTypesAreViewable);
                Assert.IsFalse(configuration.AllEmailTypesAreUpdatable);
            }

            [TestMethod]
            public void AllEmailsViewableAndAllEmailsUpdatable()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateEmailTypeConfiguration(true, null, true, null);
                Assert.AreEqual(0, configuration.ViewableEmailTypes.Count());
                Assert.AreEqual(0, configuration.UpdatableEmailTypes.Count());
                Assert.IsTrue(configuration.AllEmailTypesAreViewable);
                Assert.IsTrue(configuration.AllEmailTypesAreUpdatable);
            }

            [TestMethod]
            public void SomeEmailsViewableAndSomeEmailsUpdatable()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "PRI", "SEC" }, false, new List<string>() { "PRI" });
                Assert.AreEqual(2, configuration.ViewableEmailTypes.Count());
                Assert.AreEqual(1, configuration.UpdatableEmailTypes.Count());
                Assert.IsFalse(configuration.AllEmailTypesAreViewable);
                Assert.IsFalse(configuration.AllEmailTypesAreUpdatable);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AllEmailsUpdatableButNotAllEmailsViewable_ThrowsException()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateEmailTypeConfiguration(false, null, true, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmailsUpdatableThatAreNotViewable_ThrowsException()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "PRI" }, false, new List<string>() { "PRI", "SEC" });
            }
        }

        [TestClass]
        public class UpdatePhoneTypeConfiguration
        {
            [TestMethod]
            public void PhoneTypes_FalseBoolean()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdatePhoneTypeConfiguration(false, new List<string>() { "HO", "B" },new List<string>(), false);
                Assert.IsTrue(configuration.ViewablePhoneTypes.Contains("HO"));
                Assert.IsTrue(configuration.ViewablePhoneTypes.Contains("B"));
                Assert.AreEqual(0, configuration.UpdatablePhoneTypes.Count());

            }

            [TestMethod]
            public void NullViewablePhoneTypes()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdatePhoneTypeConfiguration(false, null, null, false);
                Assert.AreEqual(0, configuration.ViewablePhoneTypes.Count());
                Assert.AreEqual(0, configuration.UpdatablePhoneTypes.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void BothTrueBooleanAndViewablePhoneTypes_ThrowsException()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdatePhoneTypeConfiguration(true, new List<string>() { "HO", "B" }, new List<string>(), false);
            }

            [TestMethod]
            public void DoesNotAddDuplicateViewablePhoneType()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdatePhoneTypeConfiguration(false, new List<string>() { "HO", "HO" }, new List<string>(), false);
                Assert.AreEqual(1, configuration.ViewablePhoneTypes.Where(a => a == "HO").Count());
            }

            [TestMethod]
            public void DoesNotAddDuplicateUpdatablePhoneType()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdatePhoneTypeConfiguration(false, new List<string>() { "HO" }, new List<string>() { "HO", "HO" }, false);
                Assert.AreEqual(1, configuration.ViewablePhoneTypes.Where(a => a == "HO").Count());
                Assert.AreEqual(1, configuration.UpdatablePhoneTypes.Where(a => a == "HO").Count());
            }

            [TestMethod]
            public void SomePhonesViewableAndSomePhonesUpdatable()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdatePhoneTypeConfiguration(false, new List<string>() { "HO", "B" }, new List<string>() { "HO" }, false);
                Assert.AreEqual(2, configuration.ViewablePhoneTypes.Count());
                Assert.AreEqual(1, configuration.UpdatablePhoneTypes.Count());
                Assert.IsFalse(configuration.AllPhoneTypesAreViewable);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void PhonesUpdatableThatAreNotViewable_ThrowsException()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdatePhoneTypeConfiguration(false, new List<string>() { "HO" }, new List<string>() { "HO", "B" }, false);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void PhonesAllViewable_SpecifyingUpdatable_ThrowsException()
            {
                var configuration = new UserProfileConfiguration2();
                configuration.UpdatePhoneTypeConfiguration(true, null, new List<string>() { "HO", "B" }, false);
            }
        }
    }
}
