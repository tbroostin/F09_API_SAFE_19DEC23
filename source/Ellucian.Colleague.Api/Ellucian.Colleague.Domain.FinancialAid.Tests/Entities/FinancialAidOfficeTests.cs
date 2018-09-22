/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class FinancialAidOfficeTests
    {
        [TestClass]
        public class FinancialAidOfficeConstructorTests
        {
            private string Id;
            private List<string> LocationIds;
            private bool IsDefault;
            private string Name;
            private List<string> Address;
            private string PhoneNumber;
            private string EmailAddress;
            private string DirectorName;
            private List<FinancialAid.Entities.FinancialAidConfiguration> configurations;
            private string OpeId;
            private string TitleIVCode;
            private AcademicProgressConfiguration academicProgressConfiguration;
            private string defaultDisplayYearCode;
            

            private FinancialAidOffice Office;

            [TestInitialize]
            public void Initialize()
            {
                Id = "MAIN";
                LocationIds = new List<string>() { "MC", "CD" };
                IsDefault = true;
                Name = "Main Office";
                Address = new List<string>() { "2375 Fair Lakes Court", "Fairfax, VA 22033" };
                PhoneNumber = "555-555-5555";
                EmailAddress = "faoffice@ellucian.edu";
                DirectorName = "Crazy Eddie";
                OpeId = "00222333";
                configurations = new List<FinancialAid.Entities.FinancialAidConfiguration>()
                    {
                        new FinancialAid.Entities.FinancialAidConfiguration("MAIN","2014")
                        {
                            IsSelfServiceActive = true
                        }
                    };

                Office = new FinancialAidOffice(Id);
                TitleIVCode = "G67567";
                academicProgressConfiguration = new AcademicProgressConfiguration(Id);
                defaultDisplayYearCode = "2000";
            }


            [TestMethod]
            public void OfficeIdTest()
            {
                Assert.AreEqual(Id, Office.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OfficeIdRequiredTest()
            {
                new FinancialAidOffice("");
            }

            [TestMethod]
            public void LocationIdGetSetTest()
            {
                Office.LocationIds = LocationIds;
                CollectionAssert.AreEqual(LocationIds, Office.LocationIds);
            }

            [TestMethod]
            public void IsDefaultGetSetTest()
            {
                Office.IsDefault = IsDefault;
                Assert.AreEqual(IsDefault, Office.IsDefault);
            }

            [TestMethod]
            public void NameGetSetTest()
            {
                Office.Name = Name;
                Assert.AreEqual(Name, Office.Name);
            }

            [TestMethod]
            public void AddressInitializedTest()
            {
                Assert.IsNotNull(Office.AddressLabel);
                Assert.AreEqual(0, Office.AddressLabel.Count());
            }

            [TestMethod]
            public void AddressGetSetTest()
            {
                Office.AddressLabel = Address;
                for (int i = 0; i < Address.Count(); i++)
                {
                    Assert.AreEqual(Address[i], Office.AddressLabel[i]);
                }
            }

            [TestMethod]
            public void PhoneNumberGetSetTest()
            {
                Office.PhoneNumber = PhoneNumber;
                Assert.AreEqual(PhoneNumber, Office.PhoneNumber);
            }

            [TestMethod]
            public void EmailAddressGetSetTest()
            {
                Office.EmailAddress = EmailAddress;
                Assert.AreEqual(EmailAddress, Office.EmailAddress);
            }

            [TestMethod]
            public void DirectorNameGetSetTest()
            {
                Office.DirectorName = DirectorName;
                Assert.AreEqual(DirectorName, Office.DirectorName);
            }

            [TestMethod]
            public void ConfigurationInitTest()
            {
                Assert.IsNotNull(Office.Configurations);
            }

            [TestMethod]
            public void ConfigurationsGetSetTest()
            {
                Office.AddConfigurationRange(configurations);
                CollectionAssert.AreEqual(configurations, Office.Configurations);
            }

            [TestMethod]
            public void OpeIdGetSetTest()
            {
                Office.OpeId = OpeId;
                Assert.AreEqual(OpeId, Office.OpeId);
            }

            [TestMethod]
            public void TitleIVCodeGetSetTest()
            {
                Office.TitleIVCode = TitleIVCode;
                Assert.AreEqual(TitleIVCode, Office.TitleIVCode);
            }

            [TestMethod]
            public void AcademicProgressConfigurationGetSetTest()
            {
                Office.AcademicProgressConfiguration = academicProgressConfiguration;
                Assert.AreEqual(academicProgressConfiguration, Office.AcademicProgressConfiguration);
            }

            [TestMethod]
            public void DefaultDisplayYearCodeGetSetTest()
            {
                Office.DefaultDisplayYearCode = defaultDisplayYearCode;
                Assert.AreEqual(defaultDisplayYearCode, Office.DefaultDisplayYearCode);
            }
        }

        [TestClass]
        public class GetAddFinancialAidConfigurationTests
        {
            private string id;
            private FinancialAidOffice Office;
            private FinancialAidConfiguration configuration1;
            private FinancialAidConfiguration configuration2;

            [TestInitialize]
            public void Initialize()
            {
                id = "main";
                Office = new FinancialAidOffice(id);
                configuration1 = new FinancialAidConfiguration(id, "2014");
            }

            [TestMethod]
            public void AddConfigurationTest()
            {
                Office.AddConfiguration(configuration1);
                Assert.AreEqual(1, Office.Configurations.Count());
                Assert.AreEqual(configuration1, Office.Configurations.First());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ConfigurationRequiredTest()
            {
                Office.AddConfiguration(null);
            }

            [TestMethod]
            public void ConfigurationAssignedToDifferentOffice_NotAddedTest()
            {
                configuration2 = new FinancialAidConfiguration("foobar", "1998");
                Assert.IsFalse(Office.AddConfiguration(configuration2));
                Assert.AreEqual(0, Office.Configurations.Count());
            }

            [TestMethod]
            public void ConfigurationWithSameAwardYearAlreadyExists_NotAddedTest()
            {
                Office.AddConfiguration(configuration1);

                configuration2 = new FinancialAidConfiguration(id, "2014");
                Assert.IsFalse(Office.AddConfiguration(configuration2));
                Assert.AreEqual(1, Office.Configurations.Count());
            }

            [TestMethod]
            public void AddMultipleConfigurationsTest()
            {
                configuration2 = new FinancialAidConfiguration(id, "2013");
                Office.AddConfiguration(configuration1);
                Office.AddConfiguration(configuration2);
                Assert.AreEqual(2, Office.Configurations.Count());

                Assert.AreEqual(configuration2, Office.Configurations[1]);
                Assert.AreEqual(configuration1, Office.Configurations[0]);
            }

            [TestMethod]
            public void AddRangeTestTest()
            {
                configuration2 = new FinancialAidConfiguration(id, "2013");
                var list = new List<FinancialAidConfiguration>() { configuration1, configuration2 };
                Office.AddConfigurationRange(list);

                Assert.AreEqual(2, Office.Configurations.Count());

                Assert.AreEqual(configuration2, Office.Configurations[1]);
                Assert.AreEqual(configuration1, Office.Configurations[0]);
            }

            [TestMethod]
            public void AddRangeTest_DoNotAddNullsTest()
            {
                configuration2 = null;
                var list = new List<FinancialAidConfiguration>() { configuration1, configuration2 };
                Office.AddConfigurationRange(list);

                Assert.AreEqual(1, Office.Configurations.Count());

                Assert.AreEqual(configuration1, Office.Configurations[0]);
            }

            [TestMethod]
            public void AddRangeTest_DoNotAddWrongOfficesTest()
            {
                configuration2 = new FinancialAidConfiguration("foobar", "1998");
                var list = new List<FinancialAidConfiguration>() { configuration1, configuration2 };
                Office.AddConfigurationRange(list);

                Assert.AreEqual(1, Office.Configurations.Count());

                Assert.AreEqual(configuration1, Office.Configurations[0]);
            }

            [TestMethod]
            public void AddRangeTest_DoNotAddDuplicateConfigurationsTest()
            {
                Office.AddConfiguration(configuration1);

                configuration2 = new FinancialAidConfiguration(id, "2014");
                var list = new List<FinancialAidConfiguration>() { configuration2 };
                Office.AddConfigurationRange(list);

                Assert.AreEqual(1, Office.Configurations.Count());

                Assert.AreEqual(configuration1, Office.Configurations[0]);
            }

            [TestMethod]
            public void AddRangeTest_AddDistinctConfigurationsTest()
            {
                configuration2 = new FinancialAidConfiguration(id, "2014");
                var list = new List<FinancialAidConfiguration>() { configuration1, configuration2 };
                Office.AddConfigurationRange(list);

                Assert.AreEqual(1, Office.Configurations.Count());

                Assert.AreEqual(configuration1, Office.Configurations[0]);
            }

            [TestMethod]
            public void GetConfigurationTest()
            {
                Office.AddConfiguration(configuration1);
                Assert.AreEqual(configuration1, Office.GetConfiguration(configuration1.AwardYear));
            }

            [TestMethod]
            public void NullResultIfNoConfigurationWithAwardYearTest()
            {
                Office.AddConfiguration(configuration1);
                Assert.AreEqual(null, Office.GetConfiguration("foobar"));
            }

            [TestMethod]
            public void NullResultIfNoConfigurationsExistTest()
            {
                Assert.AreEqual(null, Office.GetConfiguration("foobar"));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequiredTest()
            {
                Office.GetConfiguration(null);
            }

        }

        [TestClass]
        public class FinancialAidOfficeEqualsTests
        {
            private string Id;

            private FinancialAidOffice Office;

            [TestInitialize]
            public void Initialize()
            {
                Id = "MAIN";
                Office = new FinancialAidOffice(Id);
            }

            [TestMethod]
            public void SameId_EqualsTest()
            {
                var testOffice = new FinancialAidOffice(Id);
                Assert.AreEqual(testOffice, Office);
            }

            [TestMethod]
            public void DiffId_NotEqualsTest()
            {
                var testOffice = new FinancialAidOffice("foobar");
                Assert.AreNotEqual(testOffice, Office);
            }

            [TestMethod]
            public void DiffLocation_EqualsTest()
            {
                Office.LocationIds = new List<string>() { "foo" };
                var testOffice = new FinancialAidOffice(Id) { LocationIds = new List<string>() { "bar" } };
                Assert.AreEqual(testOffice, Office);
            }

            [TestMethod]
            public void DiffIsDefault_EqualsTest()
            {
                Office.IsDefault = false;
                var testOffice = new FinancialAidOffice(Id) { IsDefault = true };
                Assert.AreEqual(testOffice, Office);
            }

            [TestMethod]
            public void DiffName_EqualsTest()
            {
                Office.Name = "foo";
                var testOffice = new FinancialAidOffice(Id) { Name = "bar" };
                Assert.AreEqual(testOffice, Office);
            }

            [TestMethod]
            public void DiffAddress_EqualsTest()
            {
                Office.AddressLabel.Add("foo");
                var testOffice = new FinancialAidOffice(Id);
                testOffice.AddressLabel.Add("bar");
                Assert.AreEqual(testOffice, Office);
            }

            [TestMethod]
            public void DiffPhoneNumber_EqualsTest()
            {
                Office.PhoneNumber = "foo";
                var testOffice = new FinancialAidOffice(Id) { PhoneNumber = "bar" };
                Assert.AreEqual(testOffice, Office);
            }

            [TestMethod]
            public void DiffEmail_EqualsTest()
            {
                Office.EmailAddress = "foo";
                var testOffice = new FinancialAidOffice(Id) { EmailAddress = "bar" };
                Assert.AreEqual(testOffice, Office);
            }

            [TestMethod]
            public void DiffDirectorName_EqualsTest()
            {
                Office.DirectorName = "foo";
                var testOffice = new FinancialAidOffice(Id) { DirectorName = "bar" };
                Assert.AreEqual(testOffice, Office);
            }

            [TestMethod]
            public void DiffTitleIVCode_EqualsTest()
            {
                Office.TitleIVCode = "T65748";
                var testOffice = new FinancialAidOffice(Id) { TitleIVCode = "H7876" };
                Assert.AreEqual(testOffice, Office);
            }
        }

        [TestClass]
        public class FinancialAidOfficeHashCodeTests
        {
            private string Id;

            private FinancialAidOffice Office;

            [TestInitialize]
            public void Initialize()
            {
                Id = "MAIN";
                Office = new FinancialAidOffice(Id);
            }

            [TestMethod]
            public void SameId_SameHashCodeTest()
            {
                var testOffice = new FinancialAidOffice(Id);
                Assert.AreEqual(testOffice.GetHashCode(), Office.GetHashCode());
            }

            [TestMethod]
            public void DiffId_NotSameHashCodeTest()
            {
                var testOffice = new FinancialAidOffice("foobar");
                Assert.AreNotEqual(testOffice.GetHashCode(), Office.GetHashCode());
            }

            [TestMethod]
            public void DiffLocation_SameHashCodeTest()
            {
                Office.LocationIds = new List<string>() { "foo" };
                var testOffice = new FinancialAidOffice(Id) { LocationIds = new List<string>() { "bar" } };
                Assert.AreEqual(testOffice.GetHashCode(), Office.GetHashCode());
            }

            [TestMethod]
            public void DiffIsDefault_SameHashCodeTest()
            {
                Office.IsDefault = false;
                var testOffice = new FinancialAidOffice(Id) { IsDefault = true };
                Assert.AreEqual(testOffice.GetHashCode(), Office.GetHashCode());
            }

            [TestMethod]
            public void DiffName_SameHashCodeTest()
            {
                Office.Name = "foo";
                var testOffice = new FinancialAidOffice(Id) { Name = "bar" };
                Assert.AreEqual(testOffice.GetHashCode(), Office.GetHashCode());
            }

            [TestMethod]
            public void DiffAddress_SameHashCodeTest()
            {
                Office.AddressLabel.Add("foo");
                var testOffice = new FinancialAidOffice(Id);
                testOffice.AddressLabel.Add("bar");
                Assert.AreEqual(testOffice.GetHashCode(), Office.GetHashCode());
            }

            [TestMethod]
            public void DiffPhoneNumber_SameHashCodeTest()
            {
                Office.PhoneNumber = "foo";
                var testOffice = new FinancialAidOffice(Id) { PhoneNumber = "bar" };
                Assert.AreEqual(testOffice.GetHashCode(), Office.GetHashCode());
            }

            [TestMethod]
            public void DiffEmail_SameHashCodeTest()
            {
                Office.EmailAddress = "foo";
                var testOffice = new FinancialAidOffice(Id) { EmailAddress = "bar" };
                Assert.AreEqual(testOffice.GetHashCode(), Office.GetHashCode());
            }

            [TestMethod]
            public void DiffDirectorName_SameHashCodeTest()
            {
                Office.DirectorName = "foo";
                var testOffice = new FinancialAidOffice(Id) { DirectorName = "bar" };
                Assert.AreEqual(testOffice.GetHashCode(), Office.GetHashCode());
            }

            [TestMethod]
            public void DiffTitleIVCode_SameHashCodeTest()
            {
                Office.TitleIVCode = "P6758";
                var testOffice = new FinancialAidOffice(Id) { TitleIVCode = "bar" };
                Assert.AreEqual(testOffice.GetHashCode(), Office.GetHashCode());
            }
        }
    }
}
