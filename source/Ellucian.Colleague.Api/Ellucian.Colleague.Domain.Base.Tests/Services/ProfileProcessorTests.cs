// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Domain.Base.Tests.Services
{
    [TestClass]
    public class ProfileProcessorTests
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Role personRole = new Role(105, "Student");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000045",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        [TestClass]
        public class VerifyProfileUpdateTests : CurrentUserSetup
        {
            private Profile newProfile;
            private Profile repoProfile;
            private UserProfileConfiguration configuration;
            private List<string> userPermissions;
            private ICurrentUser currentUser;
            private ILogger logger;
            private bool isProfileChanged;


            [TestInitialize]
            public void Initialize()
            {
                newProfile = new Profile("0000045", "Smith");
                newProfile.LastChangedDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                repoProfile = new Profile("0000045", "Smith");
                repoProfile.LastChangedDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                currentUser = new PersonUserFactory().CurrentUser;
                userPermissions = new List<string>();
                isProfileChanged = false;

                // Start with wide-open configuration that allows all emails to be updated without permissions. Refine later for specific tests
                configuration = new UserProfileConfiguration();
                configuration.UpdateEmailTypeConfiguration(true, null, true, null);
                configuration.CanUpdateEmailWithoutPermission = true;
                configuration.CanUpdatePhoneWithoutPermission = true;

                logger = new Mock<ILogger>().Object;
                ProfileProcessor.InitializeLogger(logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullNewProfile_ThrowsException()
            {
                ProfileProcessor.VerifyProfileUpdate(null, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullRepoProfile_ThrowsException()
            {
                ProfileProcessor.VerifyProfileUpdate(newProfile, null, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullConfiguration_ThrowsException()
            {
                ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, null, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullCurrentUser_ThrowsException()
            {
                ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, null, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullUserPermissions_ThrowsException()
            {
                ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, null, out isProfileChanged);
            }


            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void MismatchCurrentUserPersonId_ThrowsException()
            {
                newProfile = new Profile("0000123", "Smith");
                ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void MismatchProfilePersonId_ThrowsException()
            {
                repoProfile = new Profile("0000123", "Smith");
                ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void MismatchLastChangeDateTime_ThrowsException()
            {
                newProfile.LastChangedDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                repoProfile.LastChangedDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 25, new TimeSpan(-1, 0, 0));
                ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            public void NoChanges_ReturnsFalse()
            {
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                repoProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.IsFalse(isProfileChanged);
            }

            [TestMethod]
            public void EmailChanged_NoPermissionsRequired_AllEmailTypesUpdatable()
            {
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void EmailChanged_NoPermissionsRequired_NoEmailTypesUpdatable_ThrowsException()
            {
                configuration.UpdateEmailTypeConfiguration(true, null, false, null);
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            public void EmailChanged_MultipleNewEmailsIncoming()
            {
                var emailAddress1 = new EmailAddress("george1@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress1);
                var emailAddress2 = new EmailAddress("george2@ellucian.com", "ABC");
                newProfile.AddEmailAddress(emailAddress2);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(1), newProfile.EmailAddresses.ElementAt(1));
            }

            [TestMethod]
            public void EmailChanged_MultipleEmailsRemoved()
            {
                var emailAddress3 = new EmailAddress("george3@ellucian.com", "123");
                repoProfile.AddEmailAddress(emailAddress3);
                var emailAddress4 = new EmailAddress("george4@ellucian.com", "abc");
                repoProfile.AddEmailAddress(emailAddress4);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(0, verifiedProfile.EmailAddresses.Count());
            }

            [TestMethod]
            public void EmailChanged_MultipleEmailsAddedAndRemoved()
            {
                var emailAddress1 = new EmailAddress("george1@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress1);
                var emailAddress2 = new EmailAddress("george2@ellucian.com", "ABC");
                newProfile.AddEmailAddress(emailAddress2); 
                var emailAddress3 = new EmailAddress("george3@ellucian.com", "123");
                repoProfile.AddEmailAddress(emailAddress3);
                var emailAddress4 = new EmailAddress("george4@ellucian.com", "456");
                repoProfile.AddEmailAddress(emailAddress4);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(1), newProfile.EmailAddresses.ElementAt(1));

            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void EmailChanged_PermissionsRequired_UserWithNoPermissions_ThrowsException()
            {
                configuration.CanUpdateEmailWithoutPermission = false;
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            public void EmailChanged_PermissionsRequired_UserWithPermissions()
            {
                configuration.CanUpdateEmailWithoutPermission = false;
                userPermissions.Add(BasePermissionCodes.UpdateOwnEmail);
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void EmailChanged_NotUpdatableTypeIncoming_ThrowsException()
            {
                // Configuration allows only type ABC to be updated.
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "ABC", "XYZ" }, false, new List<string>() { "ABC" });
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            public void EmailChanged_IsUpdatableTypeIncoming()
            {
                // Configuration allows only type ABC to be updated.
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "ABC", "XYZ" }, false, new List<string>() { "ABC", "XYZ" });
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void EmailChanged_NotUpdatableTypeInRepo_ThrowsException()
            {
                // Configuration allows only type ABC to be updated.
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "ABC", "XYZ" }, false, new List<string>() { "ABC" });
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                repoProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);   
            }

            [TestMethod]
            public void EmailChanged_IsUpdatableTypeRemoved()
            {
                // Configuration allows only type ABC to be updated.
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "ABC", "XYZ" }, false, new List<string>() { "ABC", "XYZ" });
                var emailAddress1 = new EmailAddress("george1@ellucian.com", "ABC");
                newProfile.AddEmailAddress(emailAddress1);
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                repoProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(2, verifiedProfile.EmailAddresses.Count());
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(1), newProfile.EmailAddresses.ElementAt(1));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void EmailChanged_NoEmailsInUpdatableList_ThrowsException()
            {
                // Configuration allows only type ABC to be updated.
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "ABC", "XYZ" }, false, new List<string>());
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NullAddressConfirmationOverwritingExisting_ThrowsException()
            {
                newProfile.AddressConfirmationDateTime = null;
                repoProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NullEmailConfirmationOverwritingExisting_ThrowsException()
            {
                newProfile.EmailAddressConfirmationDateTime = null;
                repoProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NullPhoneConfirmationOverwritingExisting_ThrowsException()
            {
                newProfile.PhoneConfirmationDateTime = null;
                repoProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            public void IncludesConfirmationDateTime()
            {
                newProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                newProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 06, 01, 11, 24, 22, new TimeSpan(-1, 0, 0));
                newProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 05, 15, 05, 23, 22, new TimeSpan(-1, 0, 0));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(newProfile.AddressConfirmationDateTime, verifiedProfile.AddressConfirmationDateTime);
                Assert.AreEqual(newProfile.EmailAddressConfirmationDateTime, verifiedProfile.EmailAddressConfirmationDateTime);
                Assert.AreEqual(newProfile.PhoneConfirmationDateTime, verifiedProfile.PhoneConfirmationDateTime);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_NewPhone_NoPermissionRequiredAllAreViewableAndNoneUpdatable_ThrowsException()
            {
                configuration.UpdatePhoneTypeConfiguration(true, null, null);
                newProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            public void PhoneChanged_NewPhone_NoPermissionRequiredAndIsViewableAndUpdatable_IsAdded()
            {
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes);
                newProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(newProfile.Phones.ElementAt(0), verifiedProfile.Phones.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_NewPhone_NoPermissionRequiredAndIsViewabledButNotUpdatable_ThrowsException()
            {
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes);
                newProfile.AddPhone(new Phone("555-555-5555", "ZZZ"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            public void PhoneChanged_UpdatedPhone_ViewableAndUpdatable_IsUpdated()
            {
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes);
                newProfile.AddPhone(new Phone("555-555-1111", "AAA"));
                repoProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(newProfile.Phones.Count, verifiedProfile.Phones.Count);
                Assert.AreEqual(newProfile.Phones.ElementAt(0), verifiedProfile.Phones.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_UpdatedPhone_ViewableButNotUpdatable_ThrowsException()
            {
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes);
                newProfile.AddPhone(new Phone("555-555-1111", "ZZZ"));
                repoProfile.AddPhone(new Phone("555-555-5555", "ZZZ"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_UpdatedPhone_NoUpdatableTypes_ThrowsException()
            {
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes);
                newProfile.AddPhone(new Phone("555-555-1111", "AAA"));
                repoProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_UpdatedPhone_AllViewableNotUpdatable_ThrowsException()
            {
                configuration.UpdatePhoneTypeConfiguration(true, null, null);
                newProfile.AddPhone(new Phone("555-555-1111", "AAA"));
                repoProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_NewPhone_NotUpdatableAndUserDoesNotHavePermission_ThrowsException()
            {
                configuration.CanUpdatePhoneWithoutPermission = false;
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes);
                newProfile.AddPhone(new Phone("555-555-1111", "ZZZ"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_NewPhone_UpdatableButUserDoesNotHavePermission_ThrowsException()
            {
                configuration.CanUpdatePhoneWithoutPermission = false;
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes);
                newProfile.AddPhone(new Phone("555-555-1111", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_UpdatedPhone_UpdatableButUserDoesNotHavePermission_ThrowsException()
            {
                configuration.CanUpdatePhoneWithoutPermission = false;
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes);
                newProfile.AddPhone(new Phone("555-555-1111", "AAA"));
                repoProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_UpdatedPhone_NotUpdatableAndUserDoesNotHavePermission_ThrowsException()
            {
                configuration.CanUpdatePhoneWithoutPermission = false;
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes);
                newProfile.AddPhone(new Phone("555-555-1111", "ZZZ"));
                repoProfile.AddPhone(new Phone("555-555-5555", "ZZZ"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }


            [TestMethod]
            public void AddressVerification_MatchingIncomingAddressExcludedFromUpdate()
            {
                // Configuation allows addresses to be updated without permissions.
                configuration.UpdateAddressTypeConfiguration(true, null, true, "WB");
                configuration.CanUpdateAddressWithoutPermission = true;
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                newProfile.AddAddress(new Address("61", "0000100") { AddressLines = new List<string>() { "22 Center Street" }, City = "Somewhere Else", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                repoProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(1).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
            }

            [TestMethod]
            public void AddressVerification_MatchingIncomingAddressWithNoIdIncludedForUpdate()
            {
                // Configuation allows addresses to be updated without permissions.
                configuration.UpdateAddressTypeConfiguration(true, null, true, "WB");
                configuration.CanUpdateAddressWithoutPermission = true;
                newProfile.AddAddress(new Address("0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                newProfile.AddAddress(new Address("61", "0000100") { AddressLines = new List<string>() { "22 Center Street" }, City = "Somewhere Else", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                repoProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(2, verifiedProfile.Addresses.Count());
                Assert.IsTrue(string.IsNullOrEmpty(verifiedProfile.Addresses.ElementAt(0).AddressId));
                Assert.AreEqual(newProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
                Assert.AreEqual(newProfile.Addresses.ElementAt(1).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(1).AddressLines.ElementAt(0));
            }

            [TestMethod]
            public void AddressVerification_NewIncomingAddressIncludedForUpdate()
            {
                // Configuation allows addresses to be updated without permissions.
                configuration.UpdateAddressTypeConfiguration(true, null, true, "WB");
                configuration.CanUpdateAddressWithoutPermission = true;
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                newProfile.AddAddress(new Address("61", "0000100") { AddressLines = new List<string>() { "22 Center Street" }, City = "Somewhere Else", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                repoProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(1).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));                
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AddressVerification_AddressesAreUpdatableConfigurationFalse_ThrowsPermissionsException()
            {
                // Configuration: addresses not updatable
                configuration.UpdateAddressTypeConfiguration(true, null, false, "WB");
                configuration.CanUpdateAddressWithoutPermission = true;
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            public void AddressVerification_UserDoesNotHavePermissions_PermissionsNotRequired_IncludedForUpdate()
            {
                // Configuration: addresses are updatable but must have permissions
                configuration.UpdateAddressTypeConfiguration(true, null, true, "WB");
                configuration.CanUpdateAddressWithoutPermission = true;
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AddressVerification_UserDoesNotHavePermissions_PermissionsRequired_ThrowsException()
            {
                // Configuration: addresses are updatable but must have permissions
                configuration.UpdateAddressTypeConfiguration(true, null, true, "WB");
                configuration.CanUpdateAddressWithoutPermission = false;
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            public void AddressVerification_UserHasPermissions_PermissionsRequired_IncludedForUpdate()
            {
                // Configuration: addresses are updatable but must have permissions
                configuration.UpdateAddressTypeConfiguration(true, null, true, "WB");
                configuration.CanUpdateAddressWithoutPermission = false;
                userPermissions.Add(BasePermissionCodes.UpdateOwnAddress);
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AddressVerification_AddressTypeInvalid_ThrowsException()
            {
                // Configuration: addresses are updatable but must have permissions
                configuration.UpdateAddressTypeConfiguration(true, null, true, "WB");
                configuration.CanUpdateAddressWithoutPermission = false;
                userPermissions.Add(BasePermissionCodes.UpdateOwnAddress);
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "XY" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AddressVerification_AddressTypeMissing_ThrowsException()
            {
                // Configuration: addresses are updatable but must have permissions
                configuration.UpdateAddressTypeConfiguration(true, null, true, "WB");
                configuration.CanUpdateAddressWithoutPermission = false;
                userPermissions.Add(BasePermissionCodes.UpdateOwnAddress);
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = string.Empty });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
            }

        }

        [TestClass]
        public class VerifyProfileUpdate2Tests : CurrentUserSetup
        {
            private Profile newProfile;
            private Profile repoProfile;
            private UserProfileConfiguration2 configuration;
            private List<string> userPermissions;
            private ICurrentUser currentUser;
            private ILogger logger;
            private bool isProfileChanged;
            private List<AddressRelationType> allAdrelTypes;


            [TestInitialize]
            public void Initialize()
            {
                newProfile = new Profile("0000045", "Smith");
                newProfile.LastChangedDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                newProfile.Nickname = "TestNickName";
                newProfile.ChosenFirstName = "Test";
                newProfile.ChosenMiddleName = "Nick";
                newProfile.ChosenLastName = "Name";
                newProfile.GenderIdentityCode = "FEM";
                newProfile.PersonalPronounCode = "SHE";
                repoProfile = new Profile("0000045", "Smith");
                repoProfile.LastChangedDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                repoProfile.Nickname = "TestNickName";
                repoProfile.ChosenFirstName = "Test";
                repoProfile.ChosenMiddleName = "Nick";
                repoProfile.ChosenLastName = "Name";
                repoProfile.GenderIdentityCode = "FEM";
                repoProfile.PersonalPronounCode = "SHE";
                currentUser = new PersonUserFactory().CurrentUser;
                userPermissions = new List<string>();
                isProfileChanged = false;

                // Start with wide-open configuration that allows all emails to be updated without permissions. Refine later for specific tests
                configuration = new UserProfileConfiguration2();
                configuration.UpdateEmailTypeConfiguration(true, null, true, null);
                configuration.CanUpdateEmailWithoutPermission = true;
                configuration.CanUpdatePhoneWithoutPermission = true;

                allAdrelTypes = new List<AddressRelationType>()
                {
                    new AddressRelationType("H", "Home", "1", ""),
                    new AddressRelationType("B", "Business", "2", ""),
                    new AddressRelationType("CO", "Correction", "7", ""),
                    new AddressRelationType("LO", "Local", "", ""),
                    new AddressRelationType("WB", "Web Obtained", "3", ""),
                    new AddressRelationType("WB2", "Other Web", "3", "")
                };

                logger = new Mock<ILogger>().Object;
                ProfileProcessor.InitializeLogger(logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullNewProfile_ThrowsException()
            {
                ProfileProcessor.VerifyProfileUpdate2(null, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullRepoProfile_ThrowsException()
            {
                ProfileProcessor.VerifyProfileUpdate2(newProfile, null, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullConfiguration_ThrowsException()
            {
                ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, null, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullCurrentUser_ThrowsException()
            {
                ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, null, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullUserPermissions_ThrowsException()
            {
                ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, null, out isProfileChanged);
            }


            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void MismatchCurrentUserPersonId_ThrowsException()
            {
                newProfile = new Profile("0000123", "Smith");
                ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void MismatchProfilePersonId_ThrowsException()
            {
                repoProfile = new Profile("0000123", "Smith");
                ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void MismatchLastChangeDateTime_ThrowsException()
            {
                newProfile.LastChangedDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                repoProfile.LastChangedDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 25, new TimeSpan(-1, 0, 0));
                ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
            }

            [TestMethod]
            public void NoChanges_ReturnsFalse()
            {
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                repoProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.IsFalse(isProfileChanged);
            }

            [TestMethod]
            public void EmailChanged_NoPermissionsRequired_AllEmailTypesUpdatable()
            {
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
                Assert.AreEqual(verifiedProfile.Nickname, newProfile.Nickname);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void EmailChanged_NoPermissionsRequired_NoEmailTypesUpdatable_ThrowsException()
            {
                configuration.UpdateEmailTypeConfiguration(true, null, false, null);
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void EmailChanged_MultipleNewEmailsIncoming()
            {
                var emailAddress1 = new EmailAddress("george1@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress1);
                var emailAddress2 = new EmailAddress("george2@ellucian.com", "ABC");
                newProfile.AddEmailAddress(emailAddress2);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(1), newProfile.EmailAddresses.ElementAt(1));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void EmailChanged_MultipleEmailsRemoved()
            {
                var emailAddress3 = new EmailAddress("george3@ellucian.com", "123");
                repoProfile.AddEmailAddress(emailAddress3);
                var emailAddress4 = new EmailAddress("george4@ellucian.com", "abc");
                repoProfile.AddEmailAddress(emailAddress4);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(0, verifiedProfile.EmailAddresses.Count());
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void EmailChanged_MultipleEmailsAddedAndRemoved()
            {
                var emailAddress1 = new EmailAddress("george1@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress1);
                var emailAddress2 = new EmailAddress("george2@ellucian.com", "ABC");
                newProfile.AddEmailAddress(emailAddress2); 
                var emailAddress3 = new EmailAddress("george3@ellucian.com", "123");
                repoProfile.AddEmailAddress(emailAddress3);
                var emailAddress4 = new EmailAddress("george4@ellucian.com", "456");
                repoProfile.AddEmailAddress(emailAddress4);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(1), newProfile.EmailAddresses.ElementAt(1));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void EmailChanged_PermissionsRequired_UserWithNoPermissions_ThrowsException()
            {
                configuration.CanUpdateEmailWithoutPermission = false;
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void EmailChanged_PermissionsRequired_UserWithPermissions()
            {
                configuration.CanUpdateEmailWithoutPermission = false;
                userPermissions.Add(BasePermissionCodes.UpdateOwnEmail);
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void EmailChanged_NotUpdatableTypeIncoming_ThrowsException()
            {
                // Configuration allows only type ABC to be updated.
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "ABC", "XYZ" }, false, new List<string>() { "ABC" });
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void EmailChanged_IsUpdatableTypeIncoming()
            {
                // Configuration allows only type ABC to be updated.
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "ABC", "XYZ" }, false, new List<string>() { "ABC", "XYZ" });
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void EmailChanged_NotUpdatableTypeInRepo_ThrowsException()
            {
                // Configuration allows only type ABC to be updated.
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "ABC", "XYZ" }, false, new List<string>() { "ABC" });
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                repoProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void EmailChanged_IsUpdatableTypeRemoved()
            {
                // Configuration allows only type ABC to be updated.
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "ABC", "XYZ" }, false, new List<string>() { "ABC", "XYZ" });
                var emailAddress1 = new EmailAddress("george1@ellucian.com", "ABC");
                newProfile.AddEmailAddress(emailAddress1);
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                repoProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Id, newProfile.Id);
                Assert.AreEqual(verifiedProfile.LastName, newProfile.LastName);
                Assert.AreEqual(verifiedProfile.LastChangedDateTime, newProfile.LastChangedDateTime);
                Assert.AreEqual(2, verifiedProfile.EmailAddresses.Count());
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(0), newProfile.EmailAddresses.ElementAt(0));
                Assert.AreEqual(verifiedProfile.EmailAddresses.ElementAt(1), newProfile.EmailAddresses.ElementAt(1));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void EmailChanged_NoEmailsInUpdatableList_ThrowsException()
            {
                // Configuration allows only type ABC to be updated.
                configuration.UpdateEmailTypeConfiguration(false, new List<string>() { "ABC", "XYZ" }, false, new List<string>());
                var emailAddress = new EmailAddress("george@ellucian.com", "XYZ");
                newProfile.AddEmailAddress(emailAddress);
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NullAddressConfirmationOverwritingExisting_ThrowsException()
            {
                newProfile.AddressConfirmationDateTime = null;
                repoProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NullEmailConfirmationOverwritingExisting_ThrowsException()
            {
                newProfile.EmailAddressConfirmationDateTime = null;
                repoProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }
            

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NullPhoneConfirmationOverwritingExisting_ThrowsException()
            {
                newProfile.PhoneConfirmationDateTime = null;
                repoProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void IncludesConfirmationDateTime()
            {
                newProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 05, 01, 11, 23, 22, new TimeSpan(-1, 0, 0));
                newProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 06, 01, 11, 24, 22, new TimeSpan(-1, 0, 0));
                newProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 05, 15, 05, 23, 22, new TimeSpan(-1, 0, 0));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(newProfile.AddressConfirmationDateTime, verifiedProfile.AddressConfirmationDateTime);
                Assert.AreEqual(newProfile.EmailAddressConfirmationDateTime, verifiedProfile.EmailAddressConfirmationDateTime);
                Assert.AreEqual(newProfile.PhoneConfirmationDateTime, verifiedProfile.PhoneConfirmationDateTime);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_NewPhone_NoPermissionRequiredAllAreViewableAndNoneUpdatable_ThrowsException()
            {
                configuration.UpdatePhoneTypeConfiguration(true, null, null, true);
                newProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void PhoneChanged_NewPhone_NoPermissionRequiredAndIsViewableAndUpdatable_IsAdded()
            {
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes, true);
                newProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(newProfile.Phones.ElementAt(0), verifiedProfile.Phones.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_NewPhone_NoPermissionRequiredAndIsViewabledButNotUpdatable_ThrowsException()
            {
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes, false);
                newProfile.AddPhone(new Phone("555-555-5555", "ZZZ"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void PhoneChanged_UpdatedPhone_ViewableAndUpdatable_IsUpdated()
            {
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes, false);
                newProfile.AddPhone(new Phone("555-555-1111", "AAA"));
                repoProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(newProfile.Phones.Count, verifiedProfile.Phones.Count);
                Assert.AreEqual(newProfile.Phones.ElementAt(0), verifiedProfile.Phones.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_UpdatedPhone_ViewableButNotUpdatable_ThrowsException()
            {
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes, false);
                newProfile.AddPhone(new Phone("555-555-1111", "ZZZ"));
                repoProfile.AddPhone(new Phone("555-555-5555", "ZZZ"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_UpdatedPhone_NoUpdatableTypes_ThrowsException()
            {
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes, false);
                newProfile.AddPhone(new Phone("555-555-1111", "AAA"));
                repoProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_UpdatedPhone_AllViewableNotUpdatable_ThrowsException()
            {
                configuration.UpdatePhoneTypeConfiguration(true, null, null, true);
                newProfile.AddPhone(new Phone("555-555-1111", "AAA"));
                repoProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_NewPhone_NotUpdatableAndUserDoesNotHavePermission_ThrowsException()
            {
                configuration.CanUpdatePhoneWithoutPermission = false;
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes, false);
                newProfile.AddPhone(new Phone("555-555-1111", "ZZZ"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_NewPhone_UpdatableButUserDoesNotHavePermission_ThrowsException()
            {
                configuration.CanUpdatePhoneWithoutPermission = false;
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes, false);
                newProfile.AddPhone(new Phone("555-555-1111", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_UpdatedPhone_UpdatableButUserDoesNotHavePermission_ThrowsException()
            {
                configuration.CanUpdatePhoneWithoutPermission = false;
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes, false);
                newProfile.AddPhone(new Phone("555-555-1111", "AAA"));
                repoProfile.AddPhone(new Phone("555-555-5555", "AAA"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }
            
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PhoneChanged_UpdatedPhone_NotUpdatableAndUserDoesNotHavePermission_ThrowsException()
            {
                configuration.CanUpdatePhoneWithoutPermission = false;
                var viewablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC", "ZZZ" };
                var updatablePhoneTypes = new List<string>() { "AAA", "BBB", "CCC" };
                configuration.UpdatePhoneTypeConfiguration(false, viewablePhoneTypes, updatablePhoneTypes, false);
                newProfile.AddPhone(new Phone("555-555-1111", "ZZZ"));
                repoProfile.AddPhone(new Phone("555-555-5555", "ZZZ"));
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }


            [TestMethod]
            public void AddressVerification_MatchingIncomingAddressExcludedFromUpdate()
            {
                // Configuration allows addresses to be updated without permissions.
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "WB" }, new List<string>() { "WB" });
                configuration.CanUpdateAddressWithoutPermission = true;
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                newProfile.AddAddress(new Address("61", "0000100") { AddressLines = new List<string>() { "22 Center Street" }, City = "Somewhere Else", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                repoProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(1).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void AddressVerification_MatchingIncomingAddressWithNoIdIncludedForUpdate()
            {
                // Configuration allows addresses to be updated without permissions.
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "WB" }, new List<string>() { "WB" });
                configuration.CanUpdateAddressWithoutPermission = true;
                newProfile.AddAddress(new Address("0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                newProfile.AddAddress(new Address("61", "0000100") { AddressLines = new List<string>() { "22 Center Street" }, City = "Somewhere Else", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                repoProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(2, verifiedProfile.Addresses.Count());
                Assert.IsTrue(string.IsNullOrEmpty(verifiedProfile.Addresses.ElementAt(0).AddressId));
                Assert.AreEqual(newProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
                Assert.AreEqual(newProfile.Addresses.ElementAt(1).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(1).AddressLines.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void AddressVerification_NewIncomingAddressIncludedForUpdate()
            {
                // Configuration allows addresses to be updated without permissions.
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "WB" }, new List<string>() { "WB" });
                configuration.CanUpdateAddressWithoutPermission = true;
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                newProfile.AddAddress(new Address("61", "0000100") { AddressLines = new List<string>() { "22 Center Street" }, City = "Somewhere Else", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                repoProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(1).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AddressVerification_AddressIsNotUpdatable_ThrowsPermissionsException()
            {
                // Configuration: addresses not updatable
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "WB" }, new List<string>() { });
                configuration.CanUpdateAddressWithoutPermission = true;
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void AddressVerification_UserDoesNotHavePermissions_PermissionsNotRequired_IncludedForUpdate()
            {
                // Configuration: addresses are updatable without permissions
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "WB" }, new List<string>() { "WB" });
                configuration.CanUpdateAddressWithoutPermission = true;
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AddressVerification_UserDoesNotHavePermissions_PermissionsRequired_ThrowsException()
            {
                // Configuration: addresses are updatable but must have permissions
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "WB" }, new List<string>() { "WB" });
                configuration.CanUpdateAddressWithoutPermission = false;
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void AddressVerification_UserHasPermissions_PermissionsRequired_IncludedForUpdate()
            {
                // Configuration: addresses are updatable but must have permissions
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "WB" }, new List<string>() { "WB" });
                configuration.CanUpdateAddressWithoutPermission = false;
                userPermissions.Add(BasePermissionCodes.UpdateOwnAddress);
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "WB" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AddressVerification_AddressTypeInvalid_ThrowsException()
            {
                // Configuration: addresses are updatable but must have permissions
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "WB" }, new List<string>() { "WB" });
                configuration.CanUpdateAddressWithoutPermission = false;
                userPermissions.Add(BasePermissionCodes.UpdateOwnAddress);
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = "XY" });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AddressVerification_AddressTypeMissing_ThrowsException()
            {
                // Configuration: addresses are updatable but must have permissions
                configuration.UpdateAddressTypeConfiguration(false, new List<string>() { "WB" }, new List<string>() { "WB" });
                configuration.CanUpdateAddressWithoutPermission = false;
                userPermissions.Add(BasePermissionCodes.UpdateOwnAddress);
                newProfile.AddAddress(new Address("55", "0000100") { AddressLines = new List<string>() { "10 Main Street" }, City = "Anywhere", CountryCode = "USA", PostalCode = "55443", State = "ND", TypeCode = string.Empty });
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(1, verifiedProfile.Addresses.Count());
                Assert.AreEqual(newProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0), verifiedProfile.Addresses.ElementAt(0).AddressLines.ElementAt(0));
                //make sure personal identity data not removed
                Assert.AreEqual(verifiedProfile.ChosenFirstName, newProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, newProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void IdentityChanged_IsVerified()
            {
                newProfile.Nickname = "Nickname";
                newProfile.ChosenFirstName = "ChosenFirst";
                newProfile.ChosenMiddleName = "ChosenMiddle";
                newProfile.ChosenLastName = "ChosenLast";
                newProfile.GenderIdentityCode = "MAL";
                newProfile.PersonalPronounCode = "HE";
                repoProfile.Nickname = "NicknameOld";
                repoProfile.ChosenFirstName = "ChosenFirst";
                repoProfile.ChosenMiddleName = "ChosenMiddleOld";
                repoProfile.ChosenLastName = "ChosenLast";
                repoProfile.GenderIdentityCode = "FEM";
                repoProfile.PersonalPronounCode = "ZE";
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Nickname, newProfile.Nickname);
                Assert.AreEqual(verifiedProfile.ChosenFirstName, repoProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, repoProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

            [TestMethod]
            public void NoChangesMade()
            {
                var verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(newProfile, repoProfile, configuration, currentUser, userPermissions, out isProfileChanged);
                Assert.AreEqual(verifiedProfile.Nickname, newProfile.Nickname);
                Assert.AreEqual(verifiedProfile.ChosenFirstName, repoProfile.ChosenFirstName);
                Assert.AreEqual(verifiedProfile.ChosenMiddleName, newProfile.ChosenMiddleName);
                Assert.AreEqual(verifiedProfile.ChosenLastName, repoProfile.ChosenLastName);
                Assert.AreEqual(verifiedProfile.GenderIdentityCode, newProfile.GenderIdentityCode);
                Assert.AreEqual(verifiedProfile.PersonalPronounCode, newProfile.PersonalPronounCode);
            }

        }
    }
}
