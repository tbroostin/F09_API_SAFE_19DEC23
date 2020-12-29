// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Accesses Colleague for a person's contact information: Name, address, phones, emails.
    /// </summary>
    [RegisterType]
    public class ProfileRepository : PersonBaseRepository, IProfileRepository
    {
        private static char _SM = Convert.ToChar(DynamicArray.SM);
        // one minute timeout -- to enable immediate re-reads of the data contract.
        const int PersonProfileCacheTimeout = 1;

        private readonly string _colleagueTimeZone;

        /// <summary>
        /// Constructor for person profile repository.
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public ProfileRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }
        #region ValidationTables

        private async Task<IEnumerable<Country>> GetCountriesAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<Country>>("Countries",
                async () =>
                {
                    return await GetCodeItemAsync<Ellucian.Colleague.Data.Base.DataContracts.Countries, Country>("AllCountries", "COUNTRIES",
                        d => new Country(d.Recordkey, d.CtryDesc, d.CtryIsoCode));
                });
        }

        private async Task<ApplValcodes> GetAddressRelationshipsAsync()
        {

            return await GetOrAddToCacheAsync<ApplValcodes>("AddressRelationships",
                async () =>
                {
                    var relationshipTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES");
                    if (relationshipTable == null)
                    {
                        var errorMessage = "Unable to access ADREL.TYPES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return relationshipTable;
                }, Level1CacheTimeoutValue);
        }

        private async Task<ApplValcodes> GetPhoneTypesAsync()
        {

            return await GetOrAddToCacheAsync<ApplValcodes>("PhoneTypes",
                async () =>
                {
                    ApplValcodes phoneTypesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES");
                    if (phoneTypesTable == null)
                    {
                        var errorMessage = "Unable to access PHONE.TYPES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return phoneTypesTable;
                }, Level1CacheTimeoutValue);
        }

        #endregion

        #region Profile

        /// <summary>
        /// Get Base Person (PersonBase) entity. GetCached == true indicates that the cached Person object should
        /// be retrieved if available. If GetCached==false, the data should be retrieved fresh from Colleague.
        /// Retrieved Results are always added to cache.
        /// </summary>
        /// <param name="personId">Id of the person in Colleague.</param>
        /// <param name="useCache">Boolean indicates whether to get cached person data. Defaults to true.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        public async Task<Domain.Base.Entities.Profile> GetProfileAsync(string personId, bool useCache = true)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Must provide a personId to get a record.");

            Domain.Base.Entities.Profile profileEntity = await GetBaseAsync<Domain.Base.Entities.Profile>(personId,
                 person =>
                 {
                     Domain.Base.Entities.Profile entity = new Domain.Base.Entities.Profile(person.Recordkey, person.LastName);
                     return entity;
                 },
                 useCache);

            // Always pass "true" to retrieve from the person Cache since we just got the person contract above.
            // But follow the useCache argument for address information, which has not yet been retrieved.
            var addresses = await GetPersonAddressesAsync(personId, true, useCache);
            if (addresses != null)
            {
                foreach (var address in addresses)
                {
                    profileEntity.AddAddress(address);
                }
            }

            // Always pass "true" to retrieve from the person Cache and address cache since we just got both the person and address above.
            var phones = await GetPersonPhonesAsync(personId, true, true);
            if (phones != null && phones.PhoneNumbers != null)
            {
                foreach (var phone in phones.PhoneNumbers)
                {
                    profileEntity.AddPhone(phone);
                }
            }

            var personConfirmations = await GetPersonConfirmationsAsync(personId);
            if (personConfirmations != null)
            {
                // If there's a time present, combine it with the date, otherwise just use the date
                if (personConfirmations.ConfAddressesConfirmTime != null)
                {
                    profileEntity.AddressConfirmationDateTime = personConfirmations.ConfAddressesConfirmTime.ToPointInTimeDateTimeOffset(personConfirmations.ConfAddressesConfirmDate, _colleagueTimeZone);
                }
                else
                {
                    profileEntity.AddressConfirmationDateTime = personConfirmations.ConfAddressesConfirmDate;
                }
                if (personConfirmations.ConfEmailsConfirmTime != null)
                {
                    profileEntity.EmailAddressConfirmationDateTime = personConfirmations.ConfEmailsConfirmTime.ToPointInTimeDateTimeOffset(personConfirmations.ConfEmailsConfirmDate, _colleagueTimeZone);
                }
                else
                {
                    profileEntity.EmailAddressConfirmationDateTime = personConfirmations.ConfEmailsConfirmDate;
                }
                if (personConfirmations.ConfPhonesConfirmTime != null)
                {
                    profileEntity.PhoneConfirmationDateTime = personConfirmations.ConfPhonesConfirmTime.ToPointInTimeDateTimeOffset(personConfirmations.ConfPhonesConfirmDate, _colleagueTimeZone);
                }
                else
                {
                    profileEntity.PhoneConfirmationDateTime = personConfirmations.ConfPhonesConfirmDate;
                }
            }

            return profileEntity;
        }

        /// <summary>
        /// Get the address information, including phones, for a single Person
        /// </summary>
        /// <param name="personId">Person Key</param>
        /// <returns>Address Objects for preferred address and preferred residence</returns>
        public async Task<IEnumerable<Address>> GetPersonAddressesAsync(string personId, bool usePersonCache = true, bool useAddressCache = true)
        {
            List<Address> addresses = new List<Address>();

            Data.Base.DataContracts.Person personData = await GetPersonContractAsync(personId, usePersonCache);

            ICollection<Data.Base.DataContracts.Address> addressesData = await GetPersonAddressData(personData, useAddressCache);

            if (addressesData != null)
            {
                foreach (var addressData in addressesData)
                {
                    var addressId = addressData.Recordkey;
                    Address address = new Address(addressId, personId);

                    try
                    {
                        address = await BuildAddress(addressData, personData);
                        if (address != null)
                        {
                            var phoneNumber = BuildPhones(addressesData, personData);
                            if (phoneNumber.PhoneNumbers.Count() > 0)
                            {
                                foreach (var phone in phoneNumber.PhoneNumbers)
                                {
                                    address.AddPhone(phone);
                                }
                            }
                            addresses.Add(address);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Just log there was a problem and skip this address
                        logger.Info("Error occurred processing address for person.");
                        logger.Error(ex.Message);
                    }
                }
            }
            return addresses;
        }

        /// <summary>
        /// Get the current phone numbers for a single Person
        /// </summary>
        /// <param name="personId">Person Key</param>
        /// <returns>PhoneNumber Objects for a person</returns>
        public async Task<PhoneNumber> GetPersonPhonesAsync(string personId, bool usePersonCache = true, bool useAddressCache = true)
        {
            PhoneNumber phoneNumber = new PhoneNumber(personId);

            var personData = await GetPersonContractAsync(personId, usePersonCache);
            var addressesData = await GetPersonAddressData(personData, useAddressCache);

            try
            {
                phoneNumber = BuildPhones(addressesData, personData);
            }
            catch (Exception ex)
            {
                logger.Info("Error building phone data for person");
                logger.Error("Error building phone data for person: " + personId + Environment.NewLine + ex.Message);
            }

            return phoneNumber;
        }

        /// <summary>
        /// Update a person's Profile
        /// </summary>
        /// <param name="profile">The Profile to update</param>
        /// <returns>The updated Profile object</returns>
        public async Task<Profile> UpdateProfileAsync(Profile profile)
        {

            var profileUpdateRequest = new UpdatePersonProfileRequest()
            {
                APersonId = profile.Id,
                ALastChangeDate = profile.LastChangedDateTime.ToLocalDateTime(_colleagueTimeZone),
                ALastChangeTime = profile.LastChangedDateTime.ToLocalDateTime(_colleagueTimeZone)
            };

            foreach (var email in profile.EmailAddresses)
            {
                profileUpdateRequest.ProfileEmailAddresses.Add(new ProfileEmailAddresses()
                {
                    AlEmailAddress = email.Value,
                    AlEmailType = email.TypeCode,
                    AlEmailPreferred = email.IsPreferred == true ? "Y" : string.Empty
                });
            }

            // Update personal phone numbers separately from other phone numbers
            var allPhoneTypes = await GetPhoneTypesAsync();
            var personalPhoneTypes = allPhoneTypes.ValsEntityAssociation.Where(x => !string.IsNullOrEmpty(x.ValActionCode1AssocMember));
            foreach (var phone in profile.Phones)
            {
                if (personalPhoneTypes.Any(x => x.ValInternalCodeAssocMember == phone.TypeCode))
                {
                    profileUpdateRequest.ProfilePersonalPhones.Add(new ProfilePersonalPhones()
                    {
                        AlPersonalPhoneNumbers = phone.Number,
                        AlPersonalPhoneExtensions = phone.Extension,
                        AlPersonalPhoneTypes = phone.TypeCode

                    });
                }
            }

            // Update addresses
            var configuration = await GetConfigurationAsync();
            foreach (var address in profile.Addresses)
            {
                var sanitizedAddressLines = new List<string>();
                foreach (var line in address.AddressLines)
                {
                    // Replace reserved characters with a nearly-equivalent visual in the English alphabet (û, ü with u and ý, þ with y)
                    // We can't replace them with the traditional multi-character transliterations (e.g. ü to ue or þ to th) because
                    // that might push the line over the character limit if maxed out with the special character beforehand.
                    sanitizedAddressLines.Add(line.Replace(Convert.ToChar(DynamicArray.TM), 'u')
                                                  .Replace(Convert.ToChar(DynamicArray.SM), 'u')
                                                  .Replace(Convert.ToChar(DynamicArray.VM), 'y')
                                                  .Replace(Convert.ToChar(DynamicArray.FM), 'y'));
                }

                profileUpdateRequest.ProfileAddresses.Add(new ProfileAddresses()
                {
                    AlAddressCity = address.City,
                    AlAddressCountry = address.CountryCode,
                    AlAddressCounty = address.County,
                    AlAddressEffectiveEnd = address.EffectiveEndDate,
                    AlAddressEffectiveStart = address.EffectiveStartDate,
                    AlAddressId = address.AddressId,
                    AlAddressLines = (string.Join(Convert.ToChar(DynamicArray.SM).ToString(), sanitizedAddressLines.ToArray())),
                    AlAddressPostalCode = address.PostalCode,
                    AlAddressSource = configuration.DfltsWebAdrChgSource,
                    AlAddressState = address.State,
                    AlAddressTypes = address.TypeCode.Replace(',', Convert.ToChar(DynamicArray.SM))
                });
            }

            //update the identity
            profileUpdateRequest.ANickname = profile.Nickname;
            profileUpdateRequest.AChosenFirstName = profile.ChosenFirstName;
            profileUpdateRequest.AChosenMiddleName = profile.ChosenMiddleName;
            profileUpdateRequest.AChosenLastName = profile.ChosenLastName;
            profileUpdateRequest.AGenderIdentity = profile.GenderIdentityCode;
            profileUpdateRequest.APersonalPronoun = profile.PersonalPronounCode;

            var profileUpdateResponse = await transactionInvoker.ExecuteAsync<UpdatePersonProfileRequest, UpdatePersonProfileResponse>(profileUpdateRequest);

            if (profileUpdateResponse.AErrorOccurred == "3")
            {
                logger.Info("No changes detected, no update made to person profile for " + profile.Id); //is profile.Id PII?
            }
            else if (!string.IsNullOrEmpty(profileUpdateResponse.AErrorOccurred) && profileUpdateResponse.AErrorOccurred != "0")
            {
                var errorMessage = "Error(s) occurred updating person profile '" + profile.Id + "': ";
                errorMessage += profileUpdateResponse.AMsg;
                logger.Error(errorMessage);
                throw new InvalidOperationException("Error occurred updating person profile");
            }

            // If all the confirmations are null, don't send them to Colleague, it is an explicit error condition
            if (profile.AddressConfirmationDateTime != null || profile.EmailAddressConfirmationDateTime != null || profile.PhoneConfirmationDateTime != null)
            {
                var updateRequest = new UpdatePersonConfirmationsRequest()
                {
                    APersonId = profile.Id,
                    AAddressesConfirmDate = profile.AddressConfirmationDateTime.ToLocalDateTime(_colleagueTimeZone),
                    AAddressesConfirmTime = profile.AddressConfirmationDateTime.ToLocalDateTime(_colleagueTimeZone),
                    AEmailsConfirmDate = profile.EmailAddressConfirmationDateTime.ToLocalDateTime(_colleagueTimeZone),
                    AEmailsConfirmTime = profile.EmailAddressConfirmationDateTime.ToLocalDateTime(_colleagueTimeZone),
                    APhonesConfirmDate = profile.PhoneConfirmationDateTime.ToLocalDateTime(_colleagueTimeZone),
                    APhonesConfirmTime = profile.PhoneConfirmationDateTime.ToLocalDateTime(_colleagueTimeZone),
                };

                var updateResponse = await transactionInvoker.ExecuteAsync<UpdatePersonConfirmationsRequest, UpdatePersonConfirmationsResponse>(updateRequest);


                if (updateResponse.AErrorOccurred == "2")
                {
                    logger.Info("No changes detected, no update made to person confirmations for " + profile.Id);
                }
                else if (!string.IsNullOrEmpty(updateResponse.AErrorOccurred) && updateResponse.AErrorOccurred != "0")
                {
                    var errorMessage = "Error(s) occurred updating confirmations '" + profile.Id + "':";
                    errorMessage += updateResponse.AMsg;
                    logger.Error(errorMessage);
                    throw new InvalidOperationException("Error occurred updating confirmations");
                }
            }

            return await GetProfileAsync(profile.Id, false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets and caches Addresses data contracts for all the person's current addresses.
        /// </summary>
        /// <param name="personData"></param>
        /// <returns></returns>
        private async Task<ICollection<Data.Base.DataContracts.Address>> GetPersonAddressData(Data.Base.DataContracts.Person personData, bool useAddressCache)
        {
            ICollection<Data.Base.DataContracts.Address> addressData = null;
            string[] addressIds = personData.PersonAddresses.ToArray();

            if (addressIds != null && addressIds.Count() > 0)
            {
                if (useAddressCache)
                {
                    addressData = await GetOrAddToCacheAsync<ICollection<Data.Base.DataContracts.Address>>("Addresses" + personData.Recordkey,
                        async () =>
                        {
                            ICollection<Data.Base.DataContracts.Address> addressRecords = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", addressIds);
                            if (addressRecords == null || addressRecords.Count() == 0)
                            {
                                throw new ArgumentOutOfRangeException("Person Id " + personData.Recordkey + " is not returning address data.  Person or Address may be corrupted.");
                            }
                            return addressRecords;
                        }, PersonProfileCacheTimeout);
                }
                else
                {
                    addressData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", addressIds);
                    if (addressData == null || addressData.Count() == 0)
                    {
                        throw new ArgumentOutOfRangeException("Person Id " + personData.Recordkey + " is not returning address data.  Person or Address may be corrupted.");
                    }
                    await AddOrUpdateCacheAsync<ICollection<Data.Base.DataContracts.Address>>("Addresses" + personData.Recordkey, addressData, PersonProfileCacheTimeout);

                }

            }
            return addressData;
        }

        /// <summary>
        /// Attempts to build an address entity from the incoming data. If anything goes wrong, logs the info and returns null.
        /// </summary>
        /// <param name="addressData">Address data contract</param>
        /// <param name="person">Person data contract</param>
        /// <returns><see cref="Address">Address</see> entity</returns>
        private async Task<Address> BuildAddress(Ellucian.Colleague.Data.Base.DataContracts.Address addressData, Ellucian.Colleague.Data.Base.DataContracts.Person person)
        {
            try
            {
                var assoc = person.PseasonEntityAssociation.Where(r => r.PersonAddressesAssocMember == addressData.Recordkey).FirstOrDefault();
                if ((assoc.AddrEffectiveStartAssocMember == null || assoc.AddrEffectiveStartAssocMember <= DateTime.Today) &&
                    (assoc.AddrEffectiveEndAssocMember == null || assoc.AddrEffectiveEndAssocMember >= DateTime.Today))
                {
                    var addressId = addressData.Recordkey;
                    var personId = person.Recordkey;
                    var address = new Address(addressId, personId);

                    // Update Person Data fields

                    address.TypeCode = assoc.AddrTypeAssocMember;
                    address.Type = assoc.AddrTypeAssocMember;
                    if (!string.IsNullOrEmpty(address.TypeCode))
                    {
                        // AddrTypeAssocMember is actually an SM-delimited string of types, split to get all types
                        var addressTypeCodes = address.TypeCode.Split(_SM);
                        address.TypeCode = String.Join(",", addressTypeCodes);
                        var addressRelationships = await GetAddressRelationshipsAsync();
                        if (addressRelationships != null)
                        {
                            var addressTypeDescriptions = addressRelationships.ValsEntityAssociation
                                .Where(v => addressTypeCodes.Contains(v.ValInternalCodeAssocMember))
                                .Select(va => !string.IsNullOrEmpty(va.ValExternalRepresentationAssocMember) ? va.ValExternalRepresentationAssocMember : va.ValInternalCodeAssocMember);
                            if (addressTypeDescriptions.Count() > 0)
                            {
                                address.Type = String.Join(", ", addressTypeDescriptions);
                            }
                        }
                    }
                    address.AddressModifier = assoc.AddrModifierLineAssocMember;
                    address.EffectiveStartDate = assoc.AddrEffectiveStartAssocMember;
                    address.EffectiveEndDate = assoc.AddrEffectiveEndAssocMember;

                    // Set Preferred Flags
                    address.IsPreferredAddress = false;
                    address.IsPreferredResidence = false;
                    if (addressId == person.PreferredAddress) { address.IsPreferredAddress = true; }
                    if (addressId == person.PreferredResidence) { address.IsPreferredResidence = true; }

                    // Build address label
                    string countryDesc = null;
                    if (addressData.Country != null)
                    {
                        try
                        {
                            var countries = await GetCountriesAsync();
                            var country = countries.Where(v => v.Code == addressData.Country).FirstOrDefault();
                            countryDesc = country != null ? country.Description : addressData.Country;
                        }
                        catch (Exception ex)
                        {
                            logger.Info("Unable to find country entry for country.");
                            logger.Error(ex.Message);
                        }
                    }
                    address.AddressLabel = AddressProcessor.BuildAddressLabel(address.AddressModifier, addressData.AddressLines, addressData.City, addressData.State, addressData.Zip, addressData.Country, countryDesc);
                    address.AddressLines = addressData.AddressLines;
                    address.City = addressData.City;
                    address.State = addressData.State;
                    address.PostalCode = addressData.Zip;
                    address.County = addressData.County;
                    address.CountryCode = addressData.Country;
                    address.Country = countryDesc;
                    address.RouteCode = addressData.AddressRouteCode;
                    return address;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Returns a person's non-address related phone numbers as well as the phone numbers
        /// associated to an address entity.
        /// </summary>
        /// <param name="addressesData">Address Data Contract object for the address(es) being built</param>
        /// <param name="personData">Person Data Contract object for the person at this address</param>
        /// <returns>Returns a Phone Number entity which contains all phones for person and address</returns>
        private PhoneNumber BuildPhones(ICollection<Ellucian.Colleague.Data.Base.DataContracts.Address> addressesData, Ellucian.Colleague.Data.Base.DataContracts.Person personData)
        {
            if (personData != null)
            {
                var personId = personData.Recordkey;
                PhoneNumber phoneNumber = new PhoneNumber(personId);

                // Personal Phones
                if (personData.PerphoneEntityAssociation != null && personData.PerphoneEntityAssociation.Count > 0)
                {
                    foreach (var phoneData in personData.PerphoneEntityAssociation)
                    {
                        try
                        {
                            Phone personalPhone = new Phone(phoneData.PersonalPhoneNumberAssocMember, phoneData.PersonalPhoneTypeAssocMember, phoneData.PersonalPhoneExtensionAssocMember);
                            phoneNumber.AddPhone(personalPhone);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Unable to add phones for person {0}. Possible corrupt phone data.", personId);
                        }
                    }
                }

                if (personData.PersonAddresses != null)
                {
                    foreach (var addressId in personData.PersonAddresses)
                    {
                        // Address Local Phone Numbers
                        try
                        {
                            var assoc = personData.PseasonEntityAssociation.Where(r => r.PersonAddressesAssocMember == addressId).FirstOrDefault();

                            if (!string.IsNullOrEmpty(assoc.AddrLocalPhoneAssocMember))
                            {
                                // Address Local Phones in Person data
                                // This could be subvalued so need to split on subvalue mark ASCII 252.
                                string[] localPhones = assoc.AddrLocalPhoneAssocMember.Split(_SM);
                                string[] localPhoneExts = assoc.AddrLocalExtAssocMember.Split(_SM);
                                string[] localPhoneTypes = assoc.AddrLocalPhoneTypeAssocMember.Split(_SM);
                                for (int i = 0; i < localPhones.Length; i++)
                                {
                                    try
                                    {
                                        // add in the address override phones into the person's list of phones
                                        Phone personalPhone = new Phone(localPhones[i], localPhoneTypes[i], localPhoneExts[i]);
                                        phoneNumber.AddPhone(personalPhone);
                                    }
                                    catch (Exception ex)
                                    {
                                        var phoneError = "Person local phone information is invalid. PersonId: " + personId;

                                        // Log the original exception
                                        logger.Error(ex, phoneError);
                                    }
                                }
                            }
                            // Address Phones
                            var addressData = addressesData.Where(a => a.Recordkey == addressId).FirstOrDefault();
                            if (addressData != null)
                            {
                                if (addressData.AdrPhonesEntityAssociation != null && addressData.AdrPhonesEntityAssociation.Count > 0)
                                {
                                    foreach (var addrPhone in addressData.AdrPhonesEntityAssociation)
                                    {
                                        try
                                        {
                                            Phone addressPhone = new Phone(addrPhone.AddressPhonesAssocMember, addrPhone.AddressPhoneTypeAssocMember, addrPhone.AddressPhoneExtensionAssocMember);
                                            phoneNumber.AddPhone(addressPhone);
                                        }
                                        catch (Exception ex)
                                        {
                                            var phoneError = "Person address phone information is invalid. PersonId: " + personId;
                                            // Log the original exception
                                            logger.Error(ex, phoneError);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Exception occurred while trying to process phones for person " + personId + " address " + addressId);
                        }
                    }
                }
                return phoneNumber;
            }
            return null;
        }

        /// <summary>
        /// Gets the person's current confirmations
        /// </summary>
        /// <param name="personId">The id of the person</param>
        /// <returns>The <see cref="PersonConfirmations">PersonConfirmations</see> object containing confirmation data</returns>
        private async Task<DataContracts.PersonConfirmations> GetPersonConfirmationsAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Cannot retrieve confirmations with no ID");
            }
            DataContracts.PersonConfirmations personConfirmations = await DataReader.ReadRecordAsync<DataContracts.PersonConfirmations>(personId);

            return personConfirmations;
        }

        /// <summary>
        /// Gets 
        /// </summary>
        /// <returns></returns>
        private async Task<Data.Base.DataContracts.Dflts> GetConfigurationAsync()
        {
            var defaults = await GetOrAddToCacheAsync<Data.Base.DataContracts.Dflts>("Dflts",
                async () =>
                {
                    var dflts = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Dflts>("CORE.PARMS", "DEFAULTS");
                    if (dflts == null)
                    {
                        throw new ConfigurationException("Default configuration setup not complete.");
                    }
                    return dflts;
                }
            );
            return defaults;
        }

        #endregion
    }
}
