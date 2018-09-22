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
    public class UserProfileConfigurationTests
    {
        [TestClass]
        public class Constructor
        {
            [TestMethod]
            public void ViewableAddressTypes()
            {
                var configuration = new UserProfileConfiguration();
                Assert.AreEqual(0, configuration.ViewableAddressTypes.Count());
            }

            [TestMethod]
            public void ViewablePhoneTypes()
            {
                var configuration = new UserProfileConfiguration();
                Assert.AreEqual(0, configuration.ViewablePhoneTypes.Count());
            }

            [TestMethod]
            public void ViewableEmailTypes()
            {
                var configuration = new UserProfileConfiguration();
                Assert.AreEqual(0, configuration.ViewableEmailTypes.Count());
            }

            [TestMethod]
            public void UpdatableEmailTypes()
            {
                var configuration = new UserProfileConfiguration();
                Assert.AreEqual(0, configuration.UpdatableEmailTypes.Count());
            }

            [TestMethod]
            public void AllBooleansFalse()
            {
                var configuration = new UserProfileConfiguration();
                Assert.IsFalse(configuration.AllAddressTypesAreViewable);
                Assert.IsFalse(configuration.AllEmailTypesAreUpdatable);
                Assert.IsFalse(configuration.AllEmailTypesAreViewable);
                Assert.IsFalse(configuration.AllPhoneTypesAreViewable);
                Assert.IsFalse(configuration.CanUpdateEmailWithoutPermission);
            }
        }

        [TestClass]
        public class UpdateAddressTypeConfiguration
        {
            [TestMethod]
            public void AddressTypes_FalseBoolean()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() {"HO","B"}, false, null);
                Assert.IsTrue(configuration.ViewableAddressTypes.Contains("HO"));
                Assert.IsTrue(configuration.ViewableAddressTypes.Contains("B"));
            }

            [TestMethod]
            public void NullViewableAddressTypes()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdateAddressTypeConfiguration(false, null, false, null);
                Assert.AreEqual(0, configuration.ViewableAddressTypes.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void BothTrueBooleanAndViewableAddressTypes_ThrowsException()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdateAddressTypeConfiguration(true, new List<string>() { "ho" }, false, null);
            }

            [TestMethod]
            public void DoesNotAddDuplicateAddressType()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() {"HO","HO"}, false, null);
                Assert.AreEqual(1, configuration.ViewableAddressTypes.Where(a => a == "HO").Count());
            }

            [TestMethod]
            public void AddressesMayNotBeUpdated()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() {"HO","HO"}, false, "WB");
                Assert.IsFalse(configuration.AddressesAreUpdatable);
                Assert.AreEqual(0, configuration.UpdatableAddressTypes.Count);
            }

            [TestMethod]
            public void WebAdrelTypeIsUpdatable()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() {"HO","HO"}, true, "WB");
                Assert.IsTrue(configuration.AddressesAreUpdatable);
                Assert.AreEqual("WB", configuration.UpdatableAddressTypes.First());
            }

        }

        [TestClass]
        public class UpdateEmailTypeConfiguration
        {
            [TestMethod]
            public void NoViewOrUpdate()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdateEmailTypeConfiguration(false, null, false, null);
                Assert.AreEqual(0, configuration.ViewableEmailTypes.Count());
                Assert.AreEqual(0, configuration.UpdatableEmailTypes.Count());
                Assert.IsFalse(configuration.AllEmailTypesAreViewable);
                Assert.IsFalse(configuration.AllEmailTypesAreUpdatable);
            }

            [TestMethod]
            public void AllEmailsViewableAndAllEmailsUpdatable()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdateEmailTypeConfiguration(true, null, true, null);
                Assert.AreEqual(0, configuration.ViewableEmailTypes.Count());
                Assert.AreEqual(0, configuration.UpdatableEmailTypes.Count());
                Assert.IsTrue(configuration.AllEmailTypesAreViewable);
                Assert.IsTrue(configuration.AllEmailTypesAreUpdatable);
            }

            [TestMethod]
            public void SomeEmailsViewableAndSomeEmailsUpdatable()
            {
                var configuration = new UserProfileConfiguration();
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
                var configuration = new UserProfileConfiguration();
                configuration.UpdateEmailTypeConfiguration(false, null, true, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmailsUpdatableThatAreNotViewable_ThrowsException()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "PRI" }, false, new List<string>() { "PRI", "SEC" });
            }
        }

        [TestClass]
        public class UpdatePhoneTypeConfiguration
        {
            [TestMethod]
            public void PhoneTypes_FalseBoolean()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdatePhoneTypeConfiguration(false, new List<string>() { "HO", "B" }, null);
                Assert.IsTrue(configuration.ViewablePhoneTypes.Contains("HO"));
                Assert.IsTrue(configuration.ViewablePhoneTypes.Contains("B"));
            }

            [TestMethod]
            public void NullViewablePhoneTypes()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdatePhoneTypeConfiguration(false, null, null);
                Assert.AreEqual(0, configuration.ViewablePhoneTypes.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void BothTrueBooleanAndViewablePhoneTypes_ThrowsException()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdatePhoneTypeConfiguration(true, new List<string>() { "ho" }, null);
            }

            [TestMethod]
            public void DoesNotAddDuplicatePhoneType()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdatePhoneTypeConfiguration(false, new List<string>() { "HO", "HO" }, null);
                Assert.AreEqual(1, configuration.ViewablePhoneTypes.Where(a => a == "HO").Count());
            }

            [TestMethod]
            public void SomePhonesViewableAndSomePhonesUpdatable()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdatePhoneTypeConfiguration(false, new List<string>() { "HO", "B" }, new List<string>() { "HO" });
                Assert.AreEqual(2, configuration.ViewablePhoneTypes.Count());
                Assert.AreEqual(1, configuration.UpdatablePhoneTypes.Count());
                Assert.IsFalse(configuration.AllPhoneTypesAreViewable);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void PhonesUpdatableThatAreNotViewable_ThrowsException()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdatePhoneTypeConfiguration(false, new List<string>() { "HO" }, new List<string>() { "HO", "B" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void PhonesAllViewable_SpecifyingUpdatable_ThrowsException()
            {
                var configuration = new UserProfileConfiguration();
                configuration.UpdatePhoneTypeConfiguration(true, null, new List<string>() { "HO", "B" });
            }
        }
    }
}
