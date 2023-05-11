// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class PhoneNumberRepository : BaseColleagueRepository, IPhoneNumberRepository
    {
        private static char _SM = Convert.ToChar(DynamicArray.SM);

        public PhoneNumberRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }
        /// <summary>
        /// Get the current phone numbers for a single Person
        /// </summary>
        /// <param name="personId">Person Key</param>
        /// <returns>PhoneNumber Objects for a person</returns>
        public async Task<PhoneNumber> GetPersonPhonesAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person Id is required to retrieve the person's phone numbers.");
            }
            PhoneNumber phoneNumber = new PhoneNumber(personId);

            Ellucian.Colleague.Data.Base.DataContracts.Person person = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", personId);
            if (person == null)
            {
                throw new ArgumentOutOfRangeException("Person Id " + personId + " is not returning any data. Person may be corrupted.");
            }
            string[] addressIds = person.PersonAddresses.ToArray();
            ICollection<Ellucian.Colleague.Data.Base.DataContracts.Address> addressesData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", addressIds);

            if (addressesData == null)
            {
                throw new ArgumentOutOfRangeException("Person Id " + personId + " is not returning address data.  Person or Address may be corrupted.");
            }

            try
            {
                phoneNumber = BuildPhones(addressesData, person);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving phone number.");
                throw;
            }
            catch (Exception ex)
            {
                /// Don't do anything, just skip this address
                logger.Error(ex.Message, "Unable to build phone number.");
            }

            return phoneNumber;
        }
        /// <summary>
        /// Get a list of current phone numbers for a list of person keys
        /// </summary>
        /// <param name="personIds"></param>
        /// <returns>PhoneNumber Objects for a list of persons</returns>
        public async Task<IEnumerable<PhoneNumber>> GetPersonPhonesByIdsAsync(List<string> personIds)
        {
            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
            var error = false;

            ICollection<Ellucian.Colleague.Data.Base.DataContracts.Person> personData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", personIds.ToArray());
            ICollection<Ellucian.Colleague.Data.Base.DataContracts.Address> addressesData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", personData.SelectMany(p => p.PersonAddresses).Distinct().ToArray());

            if (personData == null)
            {
                // Return an empty list and note that the input yielded no results.
                logger.Error(string.Format("Selection of Person Data did not return any results from the list of input Ids: {0}", string.Join(",", personIds)));
            }
            foreach (var person in personData)
            {
                var personId = person.Recordkey;
                PhoneNumber phoneNumber = new PhoneNumber(personId);
                try
                {
                    phoneNumber = BuildPhones(addressesData, person);
                    if (phoneNumber != null)
                    {
                        phoneNumbers.Add(phoneNumber);
                    }
                }
                catch (Exception e)
                {
                    /// Just skip this phone number and log it.
                    logger.Error("Failed to build phone number. PersonId: " + personId);
                    logger.Error(e.GetBaseException().Message);
                    logger.Error(e.GetBaseException().StackTrace);
                    error = true;
                }
            }
            if (error && phoneNumbers.Count() == 0)
                throw new ColleagueWebApiException("Unexpected errors occurred. No phone number records returned. Check API error log.");

            return phoneNumbers;
        }

        private PhoneNumber BuildPhones(ICollection<Ellucian.Colleague.Data.Base.DataContracts.Address> addressesData, Ellucian.Colleague.Data.Base.DataContracts.Person personData)
        {
            if (personData != null)
            {
                var personId = personData.Recordkey;
                PhoneNumber phoneNumber = new PhoneNumber(personId);

                // Update Personal Phone
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
                            logger.Error(ex.Message);
                            
                        }
                    }
                }
                // Update Address Phone Numbers
                if (addressesData != null)
                {
                    foreach (var addressId in personData.PersonAddresses)
                    {
                        var addressData = addressesData.Where(a => a.Recordkey == addressId).FirstOrDefault();
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
                                    var phoneError = "Person local phone information is invalid.";

                                    // Log the original exception
                                    logger.Error(ex.ToString());
                                    logger.Info(phoneError);
                                }
                            }
                        }
                        // Update Address Phone
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
                                    var phoneError = "Person address phone information is invalid.";
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
        /// Get a list of Pilot primary and SMS phone numbers for a list of person keys
        /// </summary>
        /// <param name="personIds">Person IDs</param>
        /// <param name="pilotConfiguration">Pilot Configuration (User specified Primary Phone Types and SMS Phone Types)</param>
        /// <returns>PilotPhoneNumber Object for a list of persons - contains person ID, primary phone number, SMS phone number</returns>
        public async Task<IEnumerable<PilotPhoneNumber>> GetPilotPersonPhonesByIdsAsync(List<string> personIds, PilotConfiguration pilotConfiguration)        
        {
            List<PilotPhoneNumber> pilotPhoneNumbers = new List<PilotPhoneNumber>();             
            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
            var error = false;
            ICollection<Ellucian.Colleague.Data.Base.DataContracts.Person> personData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", personIds.ToArray());
            ICollection<Ellucian.Colleague.Data.Base.DataContracts.Address> addressesData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", personData.SelectMany(p => p.PersonAddresses).Distinct().ToArray());
            if (personData == null)
            {
                // Return an empty list and note that the input yielded no results.
                logger.Error(string.Format("Selection of Person Data did not return any results from the list of input Ids: {0}", string.Join(",", personIds)));
            }
            else
            {
                foreach (var person in personData)
                {
                    var personId = person.Recordkey;
                    PhoneNumber phoneNumber = new PhoneNumber(personId);
                    try
                    {
                        phoneNumber = BuildPhones(addressesData, person);
                        if (phoneNumber != null)
                        {
                            phoneNumbers.Add(phoneNumber);
                        }
                        var primaryPhoneNumber = "";
                        var smsPhoneNumber = "";
                        var primaryPhoneTypes = pilotConfiguration.PrimaryPhoneTypes;
                        bool foundPrimaryPhone = false;

                        if (primaryPhoneTypes.Count() == 0)
                        {
                            // No primary phone types provided by Pilot user.
                            // Use first phone number found (like Pilot 1.2 and earlier)
                            foreach (var phone in phoneNumbers)
                            {
                                if (phone.PersonId == personId)
                                {
                                    var matchingPhoneNumber = phone.PhoneNumbers.FirstOrDefault();
                                    primaryPhoneNumber = matchingPhoneNumber.Number;
                                    foundPrimaryPhone = true;
                                }
                            }
                        }
                        else
                        {
                            foreach (var phoneType in primaryPhoneTypes)
                            {
                                foreach (var phone in phoneNumbers)
                                {
                                    if (phone.PersonId == personId)
                                    {
                                        if (foundPrimaryPhone != true)
                                        {
                                            //Find phone number whose type matches the user-defined Primary phone type.                                        
                                            var matchingPhoneNumber = phone.PhoneNumbers.Where(pn => pn.TypeCode == phoneType).FirstOrDefault();
                                            if (matchingPhoneNumber != null)
                                            {
                                                // Save extracted primary phone number
                                                primaryPhoneNumber = matchingPhoneNumber.Number;
                                                foundPrimaryPhone = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var smsPhoneTypes = pilotConfiguration.SmsPhoneTypes;
                        if (smsPhoneTypes != null)
                        {
                            bool foundSmsPhone = false;
                            foreach (var phoneType in smsPhoneTypes)
                            {
                                foreach (var phone in phoneNumbers)
                                {
                                    if (phone.PersonId == personId)
                                    {
                                        if (foundSmsPhone != true)
                                        {
                                            //Find phone number whose type matches the user-defined SMS phone type.                                        
                                            var matchingPhoneNumber = phone.PhoneNumbers.Where(pn => pn.TypeCode == phoneType).FirstOrDefault();
                                            if (matchingPhoneNumber != null)
                                            {
                                                // Save extracted SMS phone number
                                                smsPhoneNumber = matchingPhoneNumber.Number;
                                                foundSmsPhone = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        // call constructor to add person to PilotPhoneNumber
                        // then add non-required primary and sms phone numbers
                        PilotPhoneNumber pilotPhoneNumber = new PilotPhoneNumber(personId);
                        pilotPhoneNumber.PrimaryPhoneNumber = primaryPhoneNumber;
                        pilotPhoneNumber.SmsPhoneNumber = smsPhoneNumber;
                        pilotPhoneNumbers.Add(pilotPhoneNumber);

                    }
                    catch (Exception e)
                    {
                        /// Just skip this person's phone number and log it.
                        logger.Error(e.Message);
                        error = true;
                    }
                }
            }
            if (error && phoneNumbers.Count() == 0)
                throw new ColleagueWebApiException("Unexpected errors occurred. No phone number records returned. Check API error log.");

            return pilotPhoneNumbers;
        }
    }
}