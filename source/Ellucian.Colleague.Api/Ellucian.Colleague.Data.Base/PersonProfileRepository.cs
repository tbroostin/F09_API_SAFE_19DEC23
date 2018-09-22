// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using slf4net;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Accesses Colleague for a person's contact information: Name, address, phones, emails.
    /// </summary>
    [RegisterType]
    public class ProfileRepository : PersonRepository
    {
        private static char _SM = Convert.ToChar(DynamicArray.SM);
        private string Quote = '"'.ToString();
        // one minute timeout -- to enable immediate re-reads.
        const int PersonProfileCacheTimeout = 1;

        /// <summary>
        /// Constructor for person profile repository.
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public ProfileRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }
        #region ValidationTables

        private IEnumerable<Country> GetCountries()
        {
            return GetCodeItem<Ellucian.Colleague.Data.Base.DataContracts.Countries, Country>("AllCountries", "COUNTRIES",
                d => new Country(d.Recordkey, d.CtryDesc, d.CtryIsoCode));
        }

        private ApplValcodes GetAddressRelationships()
        {

            return GetOrAddToCache<ApplValcodes>("AddressRelationships",
                () =>
                {
                    ApplValcodes relationshipTable = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES");
                    if (relationshipTable == null)
                    {
                        var errorMessage = "Unable to access ADREL.TYPES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return relationshipTable;
                }, Level1CacheTimeoutValue);
        }

        private ApplValcodes GetPhoneTypes()
        {

            return GetOrAddToCache<ApplValcodes>("PhoneTypes",
                () =>
                {
                    ApplValcodes phoneTypesTable = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES");
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

        #region ByPerson
        /// <summary>
        /// Get the address information, including phones, for a single Person
        /// </summary>
        /// <param name="personId">Person Key</param>
        /// <returns>Address Objects for preferred address and preferred residence</returns>
        public async Task<IEnumerable<Address>> GetPersonAddresses(string personId)
        {
            List<Address> addresses = new List<Address>();

            Data.Base.DataContracts.Person personData = await GetPersonData(personId);

            ICollection<Data.Base.DataContracts.Address> addressesData = await GetPersonAddressData(personData);

            foreach (var addressData in addressesData)
            {
                var addressId = addressData.Recordkey;
                Address address = new Address(addressId, personId);

                try
                {
                    address = BuildAddress(addressData, personData);
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
                catch (Exception ex)
                {
                    // Just log there was a problem and skip this address
                    logger.Info("Error occurred processing address " + addressId + " for person " + personId + ". Message: " + ex.Message + ". Skipping address.");
                }
            }
            return addresses;
        }

        /// <summary>
        /// Get the current phone numbers for a single Person
        /// </summary>
        /// <param name="personId">Person Key</param>
        /// <returns>PhoneNumber Objects for a person</returns>
        public async Task<PhoneNumber> GetPersonPhones(string personId)
        {
            PhoneNumber phoneNumber = new PhoneNumber(personId);

            var personData = await GetPersonData(personId);
            var addressesData = await GetPersonAddressData(personData);

            try
            {
                phoneNumber = BuildPhones(addressesData, personData);
            }
            catch (Exception ex)
            {
                logger.Info("Error building phone data for person " + personId + ". Message: " + ex.Message);
            }

            return phoneNumber;
        }

        public async Task<Profile> GetPersonProfile(string personId)
        {
            throw new NotImplementedException();

        }

        #endregion

        #region Private Methods

        private async Task<Data.Base.DataContracts.Person> GetPersonData(string personId)
        {
            Data.Base.DataContracts.Person personData = await GetOrAddToCacheAsync<Data.Base.DataContracts.Person>("Person" + personId,
                async () =>
                {
                    var personRecord = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Person>("PERSON", personId);
                    if (personRecord == null)
                    {
                        throw new ArgumentOutOfRangeException("Person Id " + personId + " is not returning any data. Person may be corrupted.");
                    }
                    return personRecord;
                }, PersonProfileCacheTimeout);
            return personData;
        }

        private async Task<ICollection<Data.Base.DataContracts.Address>> GetPersonAddressData(Data.Base.DataContracts.Person personData)
        {
            ICollection<Data.Base.DataContracts.Address> addressData = null;
            string[] addressIds = personData.PersonAddresses.ToArray();

            if (addressIds != null && addressIds.Count() > 0)
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
            return addressData;
        }

        private Address BuildAddress(Ellucian.Colleague.Data.Base.DataContracts.Address addressData, Ellucian.Colleague.Data.Base.DataContracts.Person person)
        {
            Address address = new Address("NEW", person.Recordkey);
            if (addressData != null)
            {
                var addressId = addressData.Recordkey;
                var personId = person.Recordkey;
                address = new Address(addressId, personId);

                // Update Person Data fields
                var assoc = person.PseasonEntityAssociation.Where(r => r.PersonAddressesAssocMember == addressId).FirstOrDefault();

                address.TypeCode = assoc.AddrTypeAssocMember;
                address.Type = assoc.AddrTypeAssocMember;
                if (!string.IsNullOrEmpty(address.Type))
                {
                    var codeAddressRelationship = GetAddressRelationships().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == address.Type).FirstOrDefault();
                    if (codeAddressRelationship != null)
                    {
                        address.Type = codeAddressRelationship.ValExternalRepresentationAssocMember;
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
                List<string> label = new List<string>();
                var country = addressData.Country;
                var codeCountry = GetCountries().Where(v => v.Code == country).FirstOrDefault();
                if (codeCountry != null)
                {
                    country = codeCountry.Description;
                }
                if (!string.IsNullOrEmpty(address.AddressModifier))
                {
                    label.Add(address.AddressModifier);
                }
                if (addressData.AddressLines.Count > 0)
                {
                    label.AddRange(addressData.AddressLines);
                }
                string cityStatePostalCode = GetCityStatePostalCode(addressData.City, addressData.State, addressData.Zip);
                if (!String.IsNullOrEmpty(cityStatePostalCode))
                {
                    label.Add(cityStatePostalCode);
                }
                if (!String.IsNullOrEmpty(addressData.Country))
                {
                    // Country name gets included in all caps
                    label.Add(country.ToUpper());
                }
                address.AddressLabel = label;
                address.AddressLines = addressData.AddressLines;
                address.City = addressData.City;
                address.State = addressData.State;
                address.PostalCode = addressData.Zip;
                address.County = addressData.County;
                address.Country = country;
                address.RouteCode = addressData.AddressRouteCode;
            }
            return address;
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

                // Personal Phone
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
                            LogDataError("Person personal phone information", personId, phoneData, ex);
                        }
                    }
                }

                foreach (var addressId in personData.PersonAddresses)
                {
                    // Address Local Phone Numbers
                    //var addressData = addressData.Where(a => a.Recordkey == addressId).FirstOrDefault();
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
                                logger.Error(ex.ToString());
                                logger.Info(phoneError);
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
                                    logger.Error(ex.ToString());
                                    logger.Info(phoneError);
                                }
                            }
                        }
                    }
                }
                return phoneNumber;
            }
            return null;
        }

        /// <summary>
        /// Build a string containing the city, state/province, and postal code
        /// </summary>
        /// <param name="city">City</param>
        /// <param name="state">State or Province</param>
        /// <param name="postalCode">Postal Code</param>
        /// <returns>Formatted string with all 3 components</returns>
        private string GetCityStatePostalCode(string city, string state, string postalCode)
        {
            StringBuilder line = new StringBuilder();

            if (!String.IsNullOrEmpty(city))
            {
                line.Append(city);
            }
            if (!String.IsNullOrEmpty(state))
            {
                if (line.Length > 0)
                {
                    line.Append(", ");
                }
                line.Append(state);
            }
            if (!String.IsNullOrEmpty(postalCode))
            {
                if (line.Length > 0)
                {
                    line.Append(" ");
                }
                line.Append(postalCode);
            }
            return line.ToString();
        }

        #endregion
    }

}
