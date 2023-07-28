// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class ProfileRepositoryTests
    {
        [TestClass]
        public class ProfileRepositoryTests_AllMethods : BaseRepositorySetup
        {
            protected List<string> personIds;
            protected List<string> addressIds;
            protected Dictionary<string, Person> personRecords;
            protected Dictionary<string, Address> addressRecords;
            protected int PersonCount = 0;
            Collection<Person> personResponseData;
            Collection<Address> addressResponseData;
            ProfileRepository profileRepo;
            protected PersonConfirmations filledPersonConfirmations;
            protected PersonConfirmations unfilledPersonConfirmations;
            //protected ApiSettings apiSettings;

            #region Private data array setup

            private string[,] _personData = { // id,    last name,   firstname, middle, prefix, preferred addr, person addresses, adrel type
                                           {"0000304", "Washington", "George", "", "Gen.", "101", "101;102;103", "HO;HO;BU"},
                                           {"0000404", "Adams", "John", "Peter", "Mr.", "104", "104;105", ""},
                                           {"0000504", "Jefferson", "Thomas", "", "Dr.", "106", "106", ""},
                                           {"0000777", "Evans", "Ethan", "", "Mr.", "701", "701;702;703", "HO" + DmiString._SM + "V;HO" + DmiString._SM + "BU" + DmiString._SM + "ZZ;BU"},

                                           {"9999999", "Test", null, null, null, null, null, null},
                                           {"9999998", "Test", "Blank", "", "", "107", "107", ""}
                                       };

            private string[,] _addressData = {
                                           {"0000304", "101 ", "65498 Ft. Belvoir Hwy;Mount Vernon;Alexandria, VA 21348", "65498 Ft. Belvoir Hwy;Mount Vernon", "Alexandria", "VA", "21348", "USA", "United States of America", "Father of our Country","01/01/2012", ""},
                                           {"0000304", "102 ", "", "235 Beacon Hill Dr.", "Boston", "MA", "03549", "", "", "","","10/10/2025"},
                                           {"0000304", "103 ", "1 Champs d'Elyssie;U.S. Embassy;Paris;FRANCE", "1 Champs d'Elyssie", "Paris", "", "", "FR", "France", "Ambassador to France","",""},
                                           {"0000404", "104", "", "1812 Dolly Madison Dr.", "Arlington", "VA", "22146", "", "", "","03/01/2025",""},
                                           {"0000404", "105", "", "1787 Constitution Ave.", "Franklin", "TN", "34567", "", "", "", "1/1/2011","5/1/2012"},
                                           {"0000504", "106", "", "1600 Pennsylvania Ave.;The White House", "Washington", "DC", "12345", "", "", "POTUS","",""},
                                           {"0000504", "107", "", "7413 Clifton Quarry Dr.", "Clifton", "VA", "20121", "", "", "","",""},
                                           {"9999999", "108 ", null, null, null, null, null, null, null, null, null, null},
                                           {"9999998", "109 ", "", "", "", "", "", "", "", "", "",""},
                                           {"0000777", "701 ", "65498 Ft. Belvoir Hwy;Mount Vernon;Alexandria, VA 21348", "65498 Ft. Belvoir Hwy;Mount Vernon", "Alexandria", "VA", "21348", "USA", "United States of America", "Father of our Country","01/01/2012", ""},
                                           {"0000777", "702 ", "", "235 Beacon Hill Dr.", "Boston", "MA", "03549", "", "", "","","10/10/2025"},
                                           {"0000777", "703 ", "1 Champs d'Elyssie;U.S. Embassy;Paris;FRANCE", "1 Champs d'Elyssie", "Paris", "", "", "FR", "France", "Ambassador to France","",""},
                                       };

            private string[,] _phoneData = {
                                               {"703-332-9004","CP","", "Y"},
                                               {"304-899-4565","HO","", ""},
                                               {"0-1-9989-998-348","BU","4339", ""},
                                               {"414-335-9005","CP","", "n"}
                                       };

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                personRecords = SetupPersons(out personIds);
                addressRecords = SetupAddresses(out addressIds);
                MockInitialize();
                profileRepo = BuildValidRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                profileRepo = null;
            }

            [TestMethod]
            public async Task GetPersonAddressesAsync_CheckAllProperties()
            {
                IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = await profileRepo.GetPersonAddressesAsync("0000304");
                Ellucian.Colleague.Domain.Base.Entities.Address address = addresses.ElementAt(0);
                Assert.AreEqual("101", address.AddressId);
                Assert.AreEqual("65498 Ft. Belvoir Hwy", address.AddressLines.ElementAt(0));
                Assert.AreEqual("Mount Vernon", address.AddressLines.ElementAt(1));
                Assert.AreEqual("Alexandria", address.City);
                Assert.AreEqual("VA", address.State);
                Assert.AreEqual("21348", address.PostalCode);
                Assert.AreEqual("USofA", address.Country);
                Assert.AreEqual("USA", address.CountryCode);
                Assert.AreEqual(4, address.PhoneNumbers.Count());
                Assert.AreEqual("414-335-9005", address.PhoneNumbers.ElementAt(1).Number);
                Assert.AreEqual("703-332-9004", address.PhoneNumbers.ElementAt(0).Number);
                Assert.AreEqual("CP", address.PhoneNumbers.ElementAt(1).TypeCode);
                Assert.AreEqual("CP", address.PhoneNumbers.ElementAt(0).TypeCode);
                Assert.AreEqual("HO", address.TypeCode);
                Assert.AreEqual("Home", address.Type);
            }

            [TestMethod]
            public async Task AllCurrentAddressesIncluded()
            {
                IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = await profileRepo.GetPersonAddressesAsync("0000304");
                Assert.AreEqual(3, addresses.Count());
            }

            [TestMethod]
            public async Task GetPersonPhonesAsync_CheckAllProperties()
            {
                Ellucian.Colleague.Domain.Base.Entities.PhoneNumber phoneNumbers = await profileRepo.GetPersonPhonesAsync("0000304");
                Assert.AreEqual(4, phoneNumbers.PhoneNumbers.Count());
                var phone1 = phoneNumbers.PhoneNumbers.ElementAt(0);
                Assert.AreEqual("703-332-9004", phone1.Number);
                Assert.AreEqual("CP", phone1.TypeCode);
                Assert.AreEqual(true, phone1.IsAuthorizedForText);


                var phone4 = phoneNumbers.PhoneNumbers.ElementAt(1);
                Assert.AreEqual("414-335-9005", phone4.Number);
                Assert.AreEqual("CP", phone4.TypeCode);
                Assert.AreEqual(string.Empty, phone4.Extension);
                Assert.AreEqual(false, phone4.IsAuthorizedForText);

                var phone2 = phoneNumbers.PhoneNumbers.ElementAt(2);
                Assert.AreEqual("304-899-4565", phone2.Number);
                Assert.AreEqual("HO", phone2.TypeCode);
                Assert.IsNull(phone2.IsAuthorizedForText);

                var phone3 = phoneNumbers.PhoneNumbers.ElementAt(3);
                Assert.AreEqual("0-1-9989-998-348", phone3.Number);
                Assert.AreEqual("BU", phone3.TypeCode);
                Assert.AreEqual("4339", phone3.Extension);
                Assert.IsNull(phone3.IsAuthorizedForText);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetProfileAsync_NullArgument_ThrowException()
            {
                string personId = null;
                var profile = await profileRepo.GetProfileAsync(personId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetProfileAsync_EmptyArgument_ThrowException()
            {
                string personId = string.Empty;
                var profile = await profileRepo.GetProfileAsync(personId);
            }

            [TestMethod]
            public async Task GetProfileAsync_RetrievesPersonConfirmations()
            {
                // The first person should have received a completely filled out PersonConfirmations object
                var filledprofile = await profileRepo.GetProfileAsync(personIds.ElementAt(0));
                Assert.AreEqual(filledPersonConfirmations.ConfAddressesConfirmTime.ToPointInTimeDateTimeOffset(filledPersonConfirmations.ConfAddressesConfirmDate, apiSettings.ColleagueTimeZone), filledprofile.AddressConfirmationDateTime);
                Assert.AreEqual(filledPersonConfirmations.ConfEmailsConfirmTime.ToPointInTimeDateTimeOffset(filledPersonConfirmations.ConfEmailsConfirmDate, apiSettings.ColleagueTimeZone), filledprofile.EmailAddressConfirmationDateTime);
                Assert.AreEqual(filledPersonConfirmations.ConfPhonesConfirmTime.ToPointInTimeDateTimeOffset(filledPersonConfirmations.ConfPhonesConfirmDate, apiSettings.ColleagueTimeZone), filledprofile.PhoneConfirmationDateTime);
            }

            [TestMethod]
            public async Task UpdateProfileAsync_VerifyRequests()
            {
                var updateProfile = new Domain.Base.Entities.Profile("0000304", "Brown");
                updateProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 09, 16, 06, 15, 22, new TimeSpan(-1, 0, 0));
                updateProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 10, 16, 06, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 10, 14, 08, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("brown@hotmail.com", "pri"));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("bob@ellucian.com", "sec") { IsPreferred = true });
                updateProfile.LastChangedDateTime = new DateTimeOffset(2015, 09, 16, 11, 22, 33, new TimeSpan(0, 0, 0));
                updateProfile.AddAddress(new Domain.Base.Entities.Address("123", "0000304")
                {
                    AddressLines = new List<string>() { "Apt 3A", "10 Main Street" },
                    City = "Georgetown",
                    CountryCode = "CANADA",
                    EffectiveEndDate = new DateTime(2017, 5, 1),
                    EffectiveStartDate = new DateTime(2013, 2, 3),
                    County = "QUE",
                    PostalCode = "28495-2945",
                    State = "Quebec",
                    TypeCode = "H,WB"
                });
                updateProfile.AddAddress(new Domain.Base.Entities.Address("124", "0000304")
                {
                    AddressLines = new List<string>() { "1 Blue Street" },
                    City = "Columbus",
                    EffectiveStartDate = DateTime.Today,
                    County = "COL",
                    PostalCode = "30987",
                    State = "GA",
                    TypeCode = "WB"
                });
                updateProfile.Nickname = "Nick";
                updateProfile.ChosenFirstName = "Nicholas";
                updateProfile.ChosenMiddleName = "Cage";
                updateProfile.ChosenLastName = "Brown";
                updateProfile.GenderIdentityCode = "MAL";
                updateProfile.PersonalPronounCode = "HE";
                // set up request and response for update person confirmations
                UpdatePersonConfirmationsRequest updatePersonConfirmationsRequest = null;
                UpdatePersonConfirmationsResponse updatePersonConfirmationsResponse = new UpdatePersonConfirmationsResponse() { AErrorOccurred = "0" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonConfirmationsRequest, UpdatePersonConfirmationsResponse>(It.IsAny<UpdatePersonConfirmationsRequest>())).ReturnsAsync(updatePersonConfirmationsResponse).Callback<UpdatePersonConfirmationsRequest>(req => updatePersonConfirmationsRequest = req);
                // set up request and response for update person profile
                UpdatePersonProfileRequest updatePersonProfileRequest = null;
                UpdatePersonProfileResponse updatePersonProfileResponse = new UpdatePersonProfileResponse() { AErrorOccurred = "0" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonProfileRequest, UpdatePersonProfileResponse>(It.IsAny<UpdatePersonProfileRequest>())).ReturnsAsync(updatePersonProfileResponse).Callback<UpdatePersonProfileRequest>(req => updatePersonProfileRequest = req);
                var result = await profileRepo.UpdateProfileAsync(updateProfile);
                // Verify update Person confirmations request 
                Assert.AreEqual(updateProfile.Id, updatePersonConfirmationsRequest.APersonId);
                Assert.AreEqual(updateProfile.AddressConfirmationDateTime.ToLocalDateTime(colleagueTimeZone), updatePersonConfirmationsRequest.AAddressesConfirmDate);
                Assert.AreEqual(updateProfile.AddressConfirmationDateTime.ToLocalDateTime(colleagueTimeZone), updatePersonConfirmationsRequest.AAddressesConfirmTime);
                Assert.AreEqual(updateProfile.EmailAddressConfirmationDateTime.ToLocalDateTime(colleagueTimeZone), updatePersonConfirmationsRequest.AEmailsConfirmDate);
                Assert.AreEqual(updateProfile.EmailAddressConfirmationDateTime.ToLocalDateTime(colleagueTimeZone), updatePersonConfirmationsRequest.AEmailsConfirmTime);
                Assert.AreEqual(updateProfile.PhoneConfirmationDateTime.ToLocalDateTime(colleagueTimeZone), updatePersonConfirmationsRequest.APhonesConfirmDate);
                Assert.AreEqual(updateProfile.PhoneConfirmationDateTime.ToLocalDateTime(colleagueTimeZone), updatePersonConfirmationsRequest.APhonesConfirmTime);
                // Verify update person profile request
                Assert.AreEqual(updateProfile.Id, updatePersonProfileRequest.APersonId);
                Assert.AreEqual(updateProfile.EmailAddresses.ElementAt(0).Value, updatePersonProfileRequest.ProfileEmailAddresses.ElementAt(0).AlEmailAddress);
                Assert.AreEqual(updateProfile.EmailAddresses.ElementAt(0).TypeCode, updatePersonProfileRequest.ProfileEmailAddresses.ElementAt(0).AlEmailType);
                Assert.AreEqual(string.Empty, updatePersonProfileRequest.ProfileEmailAddresses.ElementAt(0).AlEmailPreferred);
                Assert.AreEqual(updateProfile.EmailAddresses.ElementAt(1).Value, updatePersonProfileRequest.ProfileEmailAddresses.ElementAt(1).AlEmailAddress);
                Assert.AreEqual(updateProfile.EmailAddresses.ElementAt(1).TypeCode, updatePersonProfileRequest.ProfileEmailAddresses.ElementAt(1).AlEmailType);
                Assert.AreEqual("Y", updatePersonProfileRequest.ProfileEmailAddresses.ElementAt(1).AlEmailPreferred);
                // Verify update person profile request addresses
                for (int i = 0; i < updateProfile.Addresses.Count(); i++)
                {
                    Assert.AreEqual(updateProfile.Addresses.ElementAt(i).AddressId, updatePersonProfileRequest.ProfileAddresses.ElementAt(i).AlAddressId);
                    Assert.AreEqual(updateProfile.Addresses.ElementAt(i).City, updatePersonProfileRequest.ProfileAddresses.ElementAt(i).AlAddressCity);
                    Assert.AreEqual(updateProfile.Addresses.ElementAt(i).CountryCode, updatePersonProfileRequest.ProfileAddresses.ElementAt(i).AlAddressCountry);
                    Assert.AreEqual(updateProfile.Addresses.ElementAt(i).EffectiveEndDate, updatePersonProfileRequest.ProfileAddresses.ElementAt(i).AlAddressEffectiveEnd);
                    Assert.AreEqual(updateProfile.Addresses.ElementAt(i).EffectiveStartDate, updatePersonProfileRequest.ProfileAddresses.ElementAt(i).AlAddressEffectiveStart);
                    Assert.AreEqual(updateProfile.Addresses.ElementAt(i).PostalCode, updatePersonProfileRequest.ProfileAddresses.ElementAt(i).AlAddressPostalCode);
                    Assert.AreEqual(updateProfile.Addresses.ElementAt(i).State, updatePersonProfileRequest.ProfileAddresses.ElementAt(i).AlAddressState);
                    Assert.AreEqual(updateProfile.Addresses.ElementAt(i).TypeCode.Replace(',', DmiString._SM), updatePersonProfileRequest.ProfileAddresses.ElementAt(i).AlAddressTypes);
                }

                // Verify update person identity options
                Assert.AreEqual(updateProfile.Nickname, updatePersonProfileRequest.ANickname);
                Assert.AreEqual(updateProfile.ChosenFirstName, updatePersonProfileRequest.AChosenFirstName);
                Assert.AreEqual(updateProfile.ChosenMiddleName, updatePersonProfileRequest.AChosenMiddleName);
                Assert.AreEqual(updateProfile.ChosenLastName, updatePersonProfileRequest.AChosenLastName);
                Assert.AreEqual(updateProfile.GenderIdentityCode, updatePersonProfileRequest.AGenderIdentity);
                Assert.AreEqual(updateProfile.PersonalPronounCode, updatePersonProfileRequest.APersonalPronoun);
            }

            [TestMethod]
            public async Task UpdateProfileAsync_SpecialCharactersAreReplacedInAddress()
            {
                var updateProfile = new Domain.Base.Entities.Profile("0000304", "Brown");
                updateProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 09, 16, 06, 15, 22, new TimeSpan(-1, 0, 0));
                updateProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 10, 16, 06, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 10, 14, 08, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("brown@hotmail.com", "pri"));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("bob@ellucian.com", "sec") { IsPreferred = true });
                updateProfile.LastChangedDateTime = new DateTimeOffset(2015, 09, 16, 11, 22, 33, new TimeSpan(0, 0, 0));
                updateProfile.AddAddress(new Domain.Base.Entities.Address("123", "0000304")
                {
                    AddressLines = new List<string>() { "Apt 3A", "10 Main Street" },
                    City = "Georgetown",
                    CountryCode = "CANADA",
                    EffectiveEndDate = new DateTime(2017, 5, 1),
                    EffectiveStartDate = new DateTime(2013, 2, 3),
                    County = "QUE",
                    PostalCode = "28495-2945",
                    State = "Quebec",
                    TypeCode = "H,WB"
                });
                updateProfile.AddAddress(new Domain.Base.Entities.Address("124", "0000304")
                {
                    AddressLines = new List<string>() { "1 Blue Street", "Wüûýþ Ward, Apt A" },
                    City = "Columbus",
                    EffectiveStartDate = DateTime.Today,
                    County = "COL",
                    PostalCode = "30987",
                    State = "GA",
                    TypeCode = "WB"
                });
                // set up request and response for update person confirmations
                UpdatePersonConfirmationsRequest updatePersonConfirmationsRequest = null;
                UpdatePersonConfirmationsResponse updatePersonConfirmationsResponse = new UpdatePersonConfirmationsResponse() { AErrorOccurred = "0" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonConfirmationsRequest, UpdatePersonConfirmationsResponse>(It.IsAny<UpdatePersonConfirmationsRequest>())).ReturnsAsync(updatePersonConfirmationsResponse).Callback<UpdatePersonConfirmationsRequest>(req => updatePersonConfirmationsRequest = req);
                // set up request and response for update person profile
                UpdatePersonProfileRequest updatePersonProfileRequest = null;
                UpdatePersonProfileResponse updatePersonProfileResponse = new UpdatePersonProfileResponse() { AErrorOccurred = "0" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonProfileRequest, UpdatePersonProfileResponse>(It.IsAny<UpdatePersonProfileRequest>())).ReturnsAsync(updatePersonProfileResponse).Callback<UpdatePersonProfileRequest>(req => updatePersonProfileRequest = req);
                var result = await profileRepo.UpdateProfileAsync(updateProfile);
                // Verify update person profile request addresses
                for (int i = 0; i < updateProfile.Addresses.Count(); i++)
                {
                    var expectedAddress = updateProfile.Addresses.ElementAt(i);
                    var actualAddress = updatePersonProfileRequest.ProfileAddresses.ElementAt(i);
                    Assert.AreEqual(expectedAddress.AddressId, actualAddress.AlAddressId);
                    Assert.AreEqual(expectedAddress.City, actualAddress.AlAddressCity);
                    Assert.AreEqual(expectedAddress.CountryCode, actualAddress.AlAddressCountry);
                    Assert.AreEqual(expectedAddress.EffectiveEndDate, actualAddress.AlAddressEffectiveEnd);
                    Assert.AreEqual(expectedAddress.EffectiveStartDate, actualAddress.AlAddressEffectiveStart);
                    Assert.AreEqual(expectedAddress.PostalCode, actualAddress.AlAddressPostalCode);
                    Assert.AreEqual(expectedAddress.State, actualAddress.AlAddressState);
                    Assert.AreEqual(expectedAddress.TypeCode.Replace(',', DmiString._SM), actualAddress.AlAddressTypes);
                    List<String> expectedLines = new List<string>();
                    foreach (var expectedLine in expectedAddress.AddressLines)
                    {
                        expectedLines.Add(
                            expectedLine.Replace(DmiString._TM, 'u')
                                        .Replace(DmiString._SM, 'u')
                                        .Replace(DmiString._VM, 'y')
                                        .Replace(DmiString._FM, 'y')
                        );

                    }

                    Assert.AreEqual(string.Join(DmiString.sSM, expectedLines.ToArray()), actualAddress.AlAddressLines);
                }
            }

            [TestMethod]
            public async Task UpdateProfileAsync_UpdatePersonConfirmationsRequest_ReturnsNoUpdateMade()
            {
                var updateProfile = new Domain.Base.Entities.Profile("0000304", "Brown");
                updateProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 09, 16, 06, 15, 22, new TimeSpan(-1, 0, 0));
                updateProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 10, 16, 06, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 10, 14, 08, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("brown@hotmail.com", "pri"));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("bob@ellucian.com", "sec") { IsPreferred = true });
                updateProfile.LastChangedDateTime = new DateTimeOffset(2015, 09, 16, 11, 22, 33, new TimeSpan(0, 0, 0));
                // set up request and response for update person confirmations
                UpdatePersonConfirmationsRequest updatePersonConfirmationsRequest = null;
                UpdatePersonConfirmationsResponse updatePersonConfirmationsResponse = new UpdatePersonConfirmationsResponse() { AErrorOccurred = "2" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonConfirmationsRequest, UpdatePersonConfirmationsResponse>(It.IsAny<UpdatePersonConfirmationsRequest>())).ReturnsAsync(updatePersonConfirmationsResponse).Callback<UpdatePersonConfirmationsRequest>(req => updatePersonConfirmationsRequest = req);
                // set up request and response for update person profile
                UpdatePersonProfileRequest updatePersonProfileRequest = null;
                UpdatePersonProfileResponse updatePersonProfileResponse = new UpdatePersonProfileResponse() { AErrorOccurred = "" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonProfileRequest, UpdatePersonProfileResponse>(It.IsAny<UpdatePersonProfileRequest>())).ReturnsAsync(updatePersonProfileResponse).Callback<UpdatePersonProfileRequest>(req => updatePersonProfileRequest = req);
                var result = await profileRepo.UpdateProfileAsync(updateProfile);
                Assert.AreEqual("0000304", result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task UpdateProfileAsync_UpdatePersonConfirmationsRequest_ReturnsErrorOccurred_ThrowsException()
            {
                var updateProfile = new Domain.Base.Entities.Profile("0000304", "Brown");
                updateProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 09, 16, 06, 15, 22, new TimeSpan(-1, 0, 0));
                updateProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 10, 16, 06, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 10, 14, 08, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("brown@hotmail.com", "pri"));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("bob@ellucian.com", "sec") { IsPreferred = true });
                updateProfile.LastChangedDateTime = new DateTimeOffset(2015, 09, 16, 11, 22, 33, new TimeSpan(0, 0, 0));
                // set up request and response for update person confirmations
                UpdatePersonConfirmationsRequest updatePersonConfirmationsRequest = null;
                UpdatePersonConfirmationsResponse updatePersonConfirmationsResponse = new UpdatePersonConfirmationsResponse() { AErrorOccurred = "1" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonConfirmationsRequest, UpdatePersonConfirmationsResponse>(It.IsAny<UpdatePersonConfirmationsRequest>())).ReturnsAsync(updatePersonConfirmationsResponse).Callback<UpdatePersonConfirmationsRequest>(req => updatePersonConfirmationsRequest = req);
                // set up request and response for update person profile
                UpdatePersonProfileRequest updatePersonProfileRequest = null;
                UpdatePersonProfileResponse updatePersonProfileResponse = new UpdatePersonProfileResponse() { AErrorOccurred = "" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonProfileRequest, UpdatePersonProfileResponse>(It.IsAny<UpdatePersonProfileRequest>())).ReturnsAsync(updatePersonProfileResponse).Callback<UpdatePersonProfileRequest>(req => updatePersonProfileRequest = req);
                var result = await profileRepo.UpdateProfileAsync(updateProfile);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task UpdateProfileAsync_UpdatePersonProfileRequest_ReturnsMismatchDateError_ThrowsException()
            {
                var updateProfile = new Domain.Base.Entities.Profile("0000304", "Brown");
                updateProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 09, 16, 06, 15, 22, new TimeSpan(-1, 0, 0));
                updateProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 10, 16, 06, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 10, 14, 08, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("brown@hotmail.com", "pri"));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("bob@ellucian.com", "sec") { IsPreferred = true });
                updateProfile.LastChangedDateTime = new DateTimeOffset(2015, 09, 16, 11, 22, 33, new TimeSpan(0, 0, 0));
                // set up request and response for update person confirmations
                UpdatePersonConfirmationsRequest updatePersonConfirmationsRequest = null;
                UpdatePersonConfirmationsResponse updatePersonConfirmationsResponse = new UpdatePersonConfirmationsResponse() { AErrorOccurred = "" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonConfirmationsRequest, UpdatePersonConfirmationsResponse>(It.IsAny<UpdatePersonConfirmationsRequest>())).ReturnsAsync(updatePersonConfirmationsResponse).Callback<UpdatePersonConfirmationsRequest>(req => updatePersonConfirmationsRequest = req);
                // set up request and response for update person profile
                UpdatePersonProfileRequest updatePersonProfileRequest = null;
                UpdatePersonProfileResponse updatePersonProfileResponse = new UpdatePersonProfileResponse() { AErrorOccurred = "2" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonProfileRequest, UpdatePersonProfileResponse>(It.IsAny<UpdatePersonProfileRequest>())).ReturnsAsync(updatePersonProfileResponse).Callback<UpdatePersonProfileRequest>(req => updatePersonProfileRequest = req);
                var result = await profileRepo.UpdateProfileAsync(updateProfile);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task UpdateProfileAsync_UpdatePersonProfileRequest_ReturnsErrorOccurred_ThrowsException()
            {
                var updateProfile = new Domain.Base.Entities.Profile("0000304", "Brown");
                updateProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 09, 16, 06, 15, 22, new TimeSpan(-1, 0, 0));
                updateProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 10, 16, 06, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 10, 14, 08, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("brown@hotmail.com", "pri"));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("bob@ellucian.com", "sec") { IsPreferred = true });
                updateProfile.LastChangedDateTime = new DateTimeOffset(2015, 09, 16, 11, 22, 33, new TimeSpan(0, 0, 0));
                // set up request and response for update person confirmations
                UpdatePersonConfirmationsRequest updatePersonConfirmationsRequest = null;
                UpdatePersonConfirmationsResponse updatePersonConfirmationsResponse = new UpdatePersonConfirmationsResponse() { AErrorOccurred = "" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonConfirmationsRequest, UpdatePersonConfirmationsResponse>(It.IsAny<UpdatePersonConfirmationsRequest>())).ReturnsAsync(updatePersonConfirmationsResponse).Callback<UpdatePersonConfirmationsRequest>(req => updatePersonConfirmationsRequest = req);
                // set up request and response for update person profile
                UpdatePersonProfileRequest updatePersonProfileRequest = null;
                UpdatePersonProfileResponse updatePersonProfileResponse = new UpdatePersonProfileResponse() { AErrorOccurred = "1" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonProfileRequest, UpdatePersonProfileResponse>(It.IsAny<UpdatePersonProfileRequest>())).ReturnsAsync(updatePersonProfileResponse).Callback<UpdatePersonProfileRequest>(req => updatePersonProfileRequest = req);
                var result = await profileRepo.UpdateProfileAsync(updateProfile);
            }

            [TestMethod]
            public async Task UpdateProfileAsync_UpdatePersonProfileRequest_NoUpdateMade_ReturnsProfile()
            {
                var updateProfile = new Domain.Base.Entities.Profile("0000304", "Brown");
                updateProfile.AddressConfirmationDateTime = new DateTimeOffset(2015, 09, 16, 06, 15, 22, new TimeSpan(-1, 0, 0));
                updateProfile.EmailAddressConfirmationDateTime = new DateTimeOffset(2015, 10, 16, 06, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.PhoneConfirmationDateTime = new DateTimeOffset(2015, 10, 14, 08, 17, 22, new TimeSpan(-1, 0, 0));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("brown@hotmail.com", "pri"));
                updateProfile.AddEmailAddress(new Domain.Base.Entities.EmailAddress("bob@ellucian.com", "sec") { IsPreferred = true });
                updateProfile.LastChangedDateTime = new DateTimeOffset(2015, 09, 16, 11, 22, 33, new TimeSpan(0, 0, 0));
                // set up request and response for update person confirmations
                UpdatePersonConfirmationsRequest updatePersonConfirmationsRequest = null;
                UpdatePersonConfirmationsResponse updatePersonConfirmationsResponse = new UpdatePersonConfirmationsResponse() { AErrorOccurred = "" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonConfirmationsRequest, UpdatePersonConfirmationsResponse>(It.IsAny<UpdatePersonConfirmationsRequest>())).ReturnsAsync(updatePersonConfirmationsResponse).Callback<UpdatePersonConfirmationsRequest>(req => updatePersonConfirmationsRequest = req);
                // set up request and response for update person profile
                UpdatePersonProfileRequest updatePersonProfileRequest = null;
                UpdatePersonProfileResponse updatePersonProfileResponse = new UpdatePersonProfileResponse() { AErrorOccurred = "3" };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonProfileRequest, UpdatePersonProfileResponse>(It.IsAny<UpdatePersonProfileRequest>())).ReturnsAsync(updatePersonProfileResponse).Callback<UpdatePersonProfileRequest>(req => updatePersonProfileRequest = req);
                var result = await profileRepo.UpdateProfileAsync(updateProfile);
                Assert.AreEqual("0000304", result.Id);
            }

            [TestMethod]
            public async Task GetProfileAsync_AddressesWithMultipleTypes_ReturnFormattedDescriptions()
            {

                string personId = "0000777";
                var profile = await profileRepo.GetProfileAsync(personId);
                Assert.AreEqual("Home, Vacation", profile.Addresses.First().Type);
                Assert.AreEqual("Home, Business, ZZ", profile.Addresses.ElementAt(1).Type);
                Assert.AreEqual("Business", profile.Addresses.ElementAt(2).Type);
            }

            private ProfileRepository BuildValidRepository()
            {
                // Mock the call for getting multiple person records
                personResponseData = BuildPersonResponseData(personRecords);
                dataReaderMock.Setup<Task<Collection<Data.Base.DataContracts.Person>>>(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(personResponseData);

                // Mock the call for a single person record
                var personId = personIds.ElementAt(0);
                var personResponse = personResponseData.ElementAt(0);
                dataReaderMock.Setup<Task<Person>>(acc => acc.ReadRecordAsync<Person>("PERSON", personId, true)).ReturnsAsync(personResponse);
                var personId2 = "0000777";
                var personResponse2 = personResponseData.Where(p => p.Recordkey == "0000777").First();
                dataReaderMock.Setup<Task<Person>>(acc => acc.ReadRecordAsync<Person>("PERSON", personId2, true)).ReturnsAsync(personResponse2);

                // Mock the personConfirmationsResponses
                filledPersonConfirmations = new PersonConfirmations
                {
                    ConfAddressesConfirmDate = new DateTime(2015, 1, 2, 0, 0, 0),
                    ConfAddressesConfirmTime = new DateTime(1, 1, 1, 15, 16, 17),
                    ConfEmailsConfirmDate = new DateTime(2015, 3, 4, 00, 00, 00),
                    ConfEmailsConfirmTime = new DateTime(1, 1, 1, 18, 19, 20),
                    ConfPhonesConfirmDate = new DateTime(2015, 5, 6, 00, 00, 00),
                    ConfPhonesConfirmTime = new DateTime(1, 1, 1, 21, 22, 23),
                };

                // Give the first person a filled out confirmation response
                dataReaderMock.Setup<Task<PersonConfirmations>>(acc => acc.ReadRecordAsync<PersonConfirmations>(personIds.ElementAt(0), true)).ReturnsAsync(filledPersonConfirmations);
                dataReaderMock.Setup<Task<PersonConfirmations>>(acc => acc.ReadRecordAsync<PersonConfirmations>("0000777", true)).ReturnsAsync(filledPersonConfirmations);

                // mock data accessor PHONE.TYPES
                dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                    a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES", true))
                    .ReturnsAsync(new ApplValcodes()
                    {
                        ValInternalCode = new List<string>() { "HO", "BU", "CP", "FX" },
                        ValExternalRepresentation = new List<string>() { "Home", "Business", "Cell Phone", "Fax" },
                        ValActionCode2 = new List<string>() { "H", "B", "", "" },
                        ValsEntityAssociation = new List<ApplValcodesVals>()
                        {
                            new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "HO",
                                ValExternalRepresentationAssocMember = "Home",
                                ValActionCode2AssocMember = "H"
                            },
                             new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "BU",
                                ValExternalRepresentationAssocMember = "Business",
                                ValActionCode2AssocMember = "B"
                            },
                            new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "CP",
                                ValExternalRepresentationAssocMember = "Cell Phone",
                                ValActionCode2AssocMember = ""
                            },
                            new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "FX",
                                ValExternalRepresentationAssocMember = "FAX",
                                ValActionCode2AssocMember = ""
                            }
                        }
                    });

                // mock data accessor ADREL.TYPES
                dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                    a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES", true))
                    .ReturnsAsync(new ApplValcodes()
                    {
                        ValInternalCode = new List<string>() { "HO", "BU", "V", "ZZ" },
                        ValExternalRepresentation = new List<string>() { "Home", "Business", "Vacation" },
                        ValsEntityAssociation = new List<ApplValcodesVals>()
                        {
                            new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "HO",
                                ValExternalRepresentationAssocMember = "Home"
                            },
                            new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "BU",
                                ValExternalRepresentationAssocMember = "Business"
                            },
                            new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "V",
                                ValExternalRepresentationAssocMember = "Vacation"
                            },
                            new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "ZZ",
                                ValExternalRepresentationAssocMember = ""
                            },
                        }
                    });

                // Mock read of configuration for web address change source
                var dfltsConfig = new Dflts() { DfltsWebAdrChgSource = "WB" };
                dataReaderMock.Setup<Task<Dflts>>(d => d.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true)).ReturnsAsync(dfltsConfig);

                var countries = new Collection<DataContracts.Countries>()
                {
                    new DataContracts.Countries() { Recordkey = "USA", CtryDesc = "USofA" },
                    new DataContracts.Countries() { Recordkey = "CAN", CtryDesc = "Canada" }
                };
                dataReaderMock.Setup<Task<Collection<DataContracts.Countries>>>(d => d.BulkReadRecordAsync<DataContracts.Countries>("COUNTRIES", "", true)).ReturnsAsync(countries);

                // Set up Address Response
                addressResponseData = BuildAddressResponse(addressRecords);
                Collection<Address> addressResponse = new Collection<Address>();
                for (int i = 0; i < 7; i++)
                {
                    addressResponse.Add(addressResponseData.ElementAt(i));
                }
                Collection<Address> person1AddressResponse = new Collection<Address>();
                person1AddressResponse.Add(addressResponse.ElementAt(0));
                person1AddressResponse.Add(addressResponse.ElementAt(1));
                person1AddressResponse.Add(addressResponse.ElementAt(2));
                Collection<Address> person2AddressResponse = new Collection<Address>();
                person2AddressResponse.Add(addressResponseData.Where(a => a.Recordkey == "701").First());
                person2AddressResponse.Add(addressResponseData.Where(a => a.Recordkey == "702").First());
                person2AddressResponse.Add(addressResponseData.Where(a => a.Recordkey == "703").First());
                var personAddressIds = personResponseData.ElementAt(0).PersonAddresses.ToArray();
                var person2AddressIds = personResponseData.Where(p => p.Recordkey == "0000777").First().PersonAddresses.ToArray();
                var allPersonAddressIds = personResponseData.SelectMany(p => p.PersonAddresses).Distinct().ToArray();

                // Set up address response
                List<string> addressIds = new List<string>();
                for (int i = 0; i < 7; i++)
                {
                    addressIds.Add(this.addressIds.ElementAt(i));
                }
                dataReaderMock.Setup<Task<Collection<Address>>>(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.Address>("ADDRESS", addressIds.ToArray(), true)).ReturnsAsync(addressResponse);
                dataReaderMock.Setup<Task<Collection<Address>>>(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.Address>("ADDRESS", allPersonAddressIds, true)).ReturnsAsync(addressResponse);
                dataReaderMock.Setup<Task<Collection<Address>>>(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.Address>("ADDRESS", personAddressIds, true)).ReturnsAsync(person1AddressResponse);
                dataReaderMock.Setup<Task<Collection<Address>>>(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.Address>("ADDRESS", person2AddressIds, true)).ReturnsAsync(person2AddressResponse);

                dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                    a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "PF" }
                    });
                // Construct address repository
                profileRepo = new ProfileRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return profileRepo;
            }
            private Dictionary<string, Person> SetupPersons(out List<string> ids)
            {
                ids = new List<string>();
                string[,] recordData = _personData;
                string[,] phoneData = _phoneData;
                //string[,] addressData = _addressData;
                //var addressCount = addressData.Length / 12;

                PersonCount = recordData.Length / 8;
                Dictionary<string, Person> records = new Dictionary<string, Person>();
                for (int i = 0; i < PersonCount; i++)
                {
                    string id = recordData[i, 0].TrimEnd();
                    string last = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                    string first = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd();
                    string middle = (recordData[i, 3] == null) ? null : recordData[i, 3].TrimEnd();
                    string preferredAddress = (recordData[i, 5] == null) ? null : recordData[i, 5].TrimEnd();
                    List<string> addresses = (recordData[i, 6] == null) ? new List<string>() : recordData[i, 6].TrimEnd().Split(';').ToList<string>();
                    List<string> adrelTypes = (recordData[i, 7] == null) ? new List<string>() : recordData[i, 7].TrimEnd().Split(';').ToList<string>();


                    DataContracts.Person record = new DataContracts.Person();
                    record.Recordkey = id;
                    record.LastName = last;
                    record.FirstName = first;
                    record.MiddleName = middle;
                    record.PreferredAddress = preferredAddress;
                    record.PersonAddresses = addresses;
                    record.AddrType = adrelTypes;


                    // Add Personal Phone numbers to Person data
                    record.PersonalPhoneNumber = new List<string>();
                    record.PersonalPhoneType = new List<string>();
                    record.PersonalPhoneExtension = new List<string>();
                    record.PersonalPhoneTextAuth = new List<string>();
                    int phoneCount = phoneData.Length / 4;
                    for (int ii = 0; ii < phoneCount; ii++)
                    {
                        string number = phoneData[ii, 0].TrimEnd();
                        string type = (phoneData[ii, 1] == null) ? String.Empty : phoneData[ii, 1].TrimEnd();
                        string ext = (phoneData[ii, 2] == null) ? String.Empty : phoneData[ii, 2].TrimEnd();
                        string auth = (phoneData[ii, 3] == null) ? String.Empty : phoneData[ii, 3].TrimEnd();

                        // Cell Phone is the only phone type stored in Personal Phone fields.
                        if (type == "CP")
                        {
                            record.PersonalPhoneNumber.Add(number);
                            record.PersonalPhoneType.Add(type);
                            record.PersonalPhoneExtension.Add(ext);
                            record.PersonalPhoneTextAuth.Add(auth);
                        }
                    }

                    record.buildAssociations();

                    ids.Add(id);
                    records.Add(id, record);
                }
                return records;
            }

            private Collection<Person> BuildPersonResponseData(Dictionary<string, Person> personRecords)
            {
                Collection<Person> personContracts = new Collection<Person>();
                foreach (var personItem in personRecords)
                {
                    personContracts.Add(personItem.Value);
                }
                return personContracts;
            }

            private Dictionary<string, Address> SetupAddresses(out List<string> ids)
            {
                ids = new List<string>();
                string[,] recordData = _addressData;
                string[,] phoneData = _phoneData;

                int recordCount = recordData.Length / 12;
                Dictionary<string, Address> results = new Dictionary<string, Address>();
                for (int i = 0; i < recordCount; i++)
                {
                    Address response = new Address();
                    string key = recordData[i, 0].TrimEnd();
                    string addressId = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                    List<string> label = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd().Split(';').ToList<string>();
                    List<string> lines = (recordData[i, 3] == null) ? null : recordData[i, 3].TrimEnd().Split(';').ToList<string>();
                    string city = (recordData[i, 4] == null) ? null : recordData[i, 4].TrimEnd();
                    string state = (recordData[i, 5] == null) ? null : recordData[i, 5].TrimEnd();
                    string zip = (recordData[i, 6] == null) ? null : recordData[i, 6].TrimEnd();
                    string country = (recordData[i, 7] == null) ? null : recordData[i, 7].TrimEnd();
                    string countryDesc = (recordData[i, 8] == null) ? null : recordData[i, 8].TrimEnd();
                    string modifier = (recordData[i, 9] == null) ? null : recordData[i, 9].TrimEnd();


                    response.Recordkey = addressId;
                    response.AddressLines = lines;
                    response.City = city;
                    response.State = state;
                    response.Zip = zip;
                    response.Country = country;

                    // Add Home, Business and Fax Phone numbers to Address data
                    // (Only Home Type should be returned from repository call.)
                    response.AddressPhones = new List<string>();
                    response.AddressPhoneType = new List<string>();
                    response.AddressPhoneExtension = new List<string>();
                    int phoneCount = phoneData.Length / 4;
                    for (int ii = 0; ii < phoneCount; ii++)
                    {
                        string number = phoneData[ii, 0].TrimEnd();
                        string type = (phoneData[ii, 1] == null) ? String.Empty : phoneData[ii, 1].TrimEnd();
                        string ext = (phoneData[ii, 2] == null) ? String.Empty : phoneData[ii, 2].TrimEnd();
                        // Text authorize is not applicable in the address phones.
                        // Cell phone is defined as a personal phone and stored with person.
                        if (type != "CP")
                        {
                            response.AddressPhones.Add(number);
                            response.AddressPhoneType.Add(type);
                            response.AddressPhoneExtension.Add(ext);
                        }
                    }

                    response.buildAssociations();
                    ids.Add(addressId);
                    results.Add(addressId, response);
                }
                return results;
            }

            private Collection<Address> BuildAddressResponse(Dictionary<string, Address> addressRecords)
            {
                Collection<Address> addressContracts = new Collection<Address>();
                foreach (var addressItem in addressRecords)
                {
                    addressContracts.Add(addressItem.Value);
                }
                return addressContracts;
            }
        }
    }
}
