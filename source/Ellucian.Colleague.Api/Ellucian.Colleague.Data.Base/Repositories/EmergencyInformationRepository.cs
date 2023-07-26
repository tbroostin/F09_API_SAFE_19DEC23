// Copyright 2016-2023 Ellucian Company L.P. and its affiliatesusing System
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EmergencyInformationRepository : BaseColleagueRepository, IEmergencyInformationRepository
    {
        private RepositoryException repoException = new RepositoryException();

        public EmergencyInformationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger) { }

        /// <summary>
        /// This retrieves the emergency information for a person.
        /// </summary>
        /// <param name="personId">Pass in the person's ID</param>
        /// <returns>Returns an EmergencyInformation object containing all the emergency information for the specified person</returns>
        public Domain.Base.Entities.EmergencyInformation Get(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID must be passed in.");
            }

            try
            {
                PersonEmer emergencyInfoContract = DataReader.ReadRecord<PersonEmer>(personId, false);


                EmergencyInformation emergencyInfoEntity = new EmergencyInformation(personId);

                if (emergencyInfoContract == null)
                {
                    return emergencyInfoEntity;
                }

                emergencyInfoEntity.HospitalPreference = emergencyInfoContract.EmerHospitalPref;

                emergencyInfoEntity.ConfirmedDate = emergencyInfoContract.EmerLastConfirmedDate;

                emergencyInfoEntity.OptOut = string.Equals(emergencyInfoContract.EmerOptout, "Y", StringComparison.OrdinalIgnoreCase);

                // We need to convert value marks to new line characters because we want to maintain any formatting
                // (line-to-line) that the user may have entered.
                if (!string.IsNullOrEmpty(emergencyInfoContract.EmerInsuranceInfo))
                {
                    var insuranceInfo = emergencyInfoContract.EmerInsuranceInfo.Replace(DmiString._VM, '\n');
                    emergencyInfoEntity.InsuranceInformation = insuranceInfo;
                }

                // The new Additional Emergency Information ("comment") field is where people can 
                // self-disclose additional information that might be necessary in case of emergency.
                // Similarly to Insurance Informationm, we need to convert value marks to new line
                // characters because we want to maintain any formatting (line-to-line) that the user may
                // have entered.
                if (!string.IsNullOrEmpty(emergencyInfoContract.EmerAddnlInformation))
                {
                    var additionalInfo = emergencyInfoContract.EmerAddnlInformation.Replace(DmiString._VM, '\n');
                    emergencyInfoEntity.AdditionalInformation = additionalInfo;
                }

                if (emergencyInfoContract.EmerContactsEntityAssociation != null)
                {
                    foreach (var emergencyContact in emergencyInfoContract.EmerContactsEntityAssociation)
                    {
                        EmergencyContact emergencyContactEntity = new EmergencyContact(emergencyContact.EmerNameAssocMember);

                        emergencyContactEntity.DaytimePhone = emergencyContact.EmerDaytimePhoneAssocMember;
                        emergencyContactEntity.EveningPhone = emergencyContact.EmerEveningPhoneAssocMember;
                        emergencyContactEntity.OtherPhone = emergencyContact.EmerOtherPhoneAssocMember;
                        emergencyContactEntity.EffectiveDate = emergencyContact.EmerContactDateAssocMember;
                        emergencyContactEntity.Relationship = emergencyContact.EmerRelationshipAssocMember;
                        if (emergencyContact.EmerEmergencyContactFlagAssocMember == "N")
                        {
                            emergencyContactEntity.IsEmergencyContact = false;
                        }
                        else
                        //(emergencyContact.EmerEmergencyContactFlagAssocMember == "Y" or null or empty)
                        {
                            emergencyContactEntity.IsEmergencyContact = true;
                        }

                        if (emergencyContact.EmerMissingContactFlagAssocMember == "Y")
                        {
                            emergencyContactEntity.IsMissingPersonContact = true;
                        }
                        else
                        // (emergencyContact.EmerMissingContactFlagAssocMember == "N" or null or empty)
                        {
                            emergencyContactEntity.IsMissingPersonContact = false;
                        }
                        emergencyContactEntity.Address = emergencyContact.EmerContactAddressAssocMember;

                        emergencyInfoEntity.AddEmergencyContact(emergencyContactEntity);
                    }
                }

                if (emergencyInfoContract.EmerHealthConditions != null)
                {
                    foreach (var healthCondition in emergencyInfoContract.EmerHealthConditions)
                    {
                        if (GetHealthConditions().ValsEntityAssociation.Select(x => x.ValInternalCodeAssocMember).Contains(healthCondition))
                        {
                            emergencyInfoEntity.AddHealthCondition(healthCondition);
                        }
                        else
                        {
                            // Do not throw an error, just log it.
                            logger.Error("Health Condition " + healthCondition + " is an invalid code.");
                        }
                    }
                }

                return emergencyInfoEntity;

            }

            catch (Exception e)
            {
                logger.Error(e, "Error reading emergency information for person " + personId);
                throw;
            }


        }



        private ApplValcodes GetHealthConditions()
        {
            return GetOrAddToCache<ApplValcodes>("AllHealthConditions",
                () =>
                {
                    ApplValcodes healthConditionsValcode = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "HEALTH.CONDITIONS");

                    if (healthConditionsValcode == null)
                    {
                        var errorMessage = "Unable to access HEALTH.CONDITIONS valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return healthConditionsValcode;
                }
                );
        }


        /// <summary>
        /// This method updates emergency information for a person.
        /// </summary>
        /// <param name="emergencyInformation">Pass in an EmergencyInformation object containing all the emergency information for a person.</param>
        /// <returns>Returns an EmergencyInformation object containing this person's updated emergency information from the database.</returns>
        public EmergencyInformation UpdateEmergencyInformation(EmergencyInformation emergencyInformation)
        {
            var updateEmergencyInformationRequest = new Ellucian.Colleague.Data.Base.Transactions.UpdateEmergencyInformationRequest();

            updateEmergencyInformationRequest.PersonId = emergencyInformation.PersonId;
            updateEmergencyInformationRequest.LastConfirmedDate = emergencyInformation.ConfirmedDate;
            updateEmergencyInformationRequest.HospitalPreference = emergencyInformation.HospitalPreference;
            updateEmergencyInformationRequest.OptOut = emergencyInformation.OptOut ? "y" : "n";

            // We may have line break characters in the data. Split them out and add each line separately
            // to preserve any line-to-line formatting the user entered. Note that these characters could be
            // \n or \r\n (two variations of a new line character) or \r (a carriage return). We will change
            // any of the new line or carriage returns to the same thing, and then split the string on that.
            string newLineCharacter = "\n";
            string alternateNewLineCharacter = "\r\n";
            string carriageReturnCharacter = "\r";
            string temporaryText1 = emergencyInformation.InsuranceInformation.Replace(alternateNewLineCharacter, newLineCharacter);
            string temporaryText2 = temporaryText1.Replace(carriageReturnCharacter, newLineCharacter);
            var insuranceLines = temporaryText2.Split('\n');
            foreach (var line in insuranceLines)
            {
                updateEmergencyInformationRequest.InsuranceInformation.Add(line);
            }

            // We may have line break characters in the data. Split them out and add each line separately
            // to preserve any line-to-line formatting the user entered.
            temporaryText1 = emergencyInformation.AdditionalInformation.Replace(alternateNewLineCharacter, newLineCharacter);
            temporaryText2 = temporaryText1.Replace(carriageReturnCharacter, newLineCharacter);
            var additionalInformationLines = temporaryText2.Split('\n');
            foreach (var line in additionalInformationLines)
            {
                updateEmergencyInformationRequest.AdditionalInformation.Add(line);
            }

            foreach (var contact in emergencyInformation.EmergencyContacts)
            {
                updateEmergencyInformationRequest.EmergencyContactName.Add(contact.Name);
                updateEmergencyInformationRequest.ContactEffectiveDate.Add(contact.EffectiveDate);
                updateEmergencyInformationRequest.ContactRelationships.Add(contact.Relationship);
                updateEmergencyInformationRequest.DaytimePhones.Add(contact.DaytimePhone);
                updateEmergencyInformationRequest.EveningPhone.Add(contact.EveningPhone);
                updateEmergencyInformationRequest.OtherPhones.Add(contact.OtherPhone);
                if (contact.IsEmergencyContact)
                {
                    updateEmergencyInformationRequest.EmergencyContactFlags.Add("Y");
                }
                else
                {
                    updateEmergencyInformationRequest.EmergencyContactFlags.Add("N");
                }
                if (contact.IsMissingPersonContact)
                {
                    updateEmergencyInformationRequest.MissingContactFlags.Add("Y");
                }
                else
                {
                    updateEmergencyInformationRequest.MissingContactFlags.Add("N");
                }
                updateEmergencyInformationRequest.ContactAddresses.Add(contact.Address);

            }

            foreach (var healthCondition in emergencyInformation.HealthConditions)
            {
                if (GetHealthConditions().ValsEntityAssociation.Select(x => x.ValInternalCodeAssocMember).Contains(healthCondition))
                {
                    updateEmergencyInformationRequest.HealthConditions.Add(healthCondition);
                }
                else
                {
                    var errorMessage = "Health Condition " + healthCondition + " is an invalid code.";
                    logger.Error(errorMessage);
                    throw new ArgumentException(errorMessage);
                }
            }

            UpdateEmergencyInformationResponse updateResponse = transactionInvoker.Execute<UpdateEmergencyInformationRequest, UpdateEmergencyInformationResponse>(updateEmergencyInformationRequest);

            if (updateResponse.ErrorMessages != null && updateResponse.ErrorMessages.Count() > 0)
            {

                // Set up variable to construct a single error message from the list of
                // returned messages, and an index of where we are in the list.
                string combinedErrorMessages = "";
                int indexPos = 0;

                // Loop through each error that was returned from the Colleague Transaction.
                foreach (var errorMessage in updateResponse.ErrorMessages)
                {
                    // Log each error. 
                    logger.Error("EmergencyInformationRepository Error: " + errorMessage);


                    // Create a string that combines all of the error messages.
                    indexPos += 1;
                    if (indexPos == 1)
                    {
                        combinedErrorMessages = errorMessage;
                    }
                    else
                    {
                        combinedErrorMessages += "; " + errorMessage;
                    }

                }

                // Throw an exception giving all the errors.
                throw new ArgumentException(combinedErrorMessages);

            }

            var outputEmergencyInformation = Get(emergencyInformation.PersonId);
            return outputEmergencyInformation;
        }

        #region PersonContacts

        /// <summary>
        /// Gets perosn contacts
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<PersonContact>, int>> GetPersonContactsAsync(int offset, int limit, bool bypassCache, string person = "")
        {
            string[] personContactIds = new string[] { };

            if (!string.IsNullOrEmpty(person))
            {
                personContactIds = await DataReader.SelectAsync("PERSON.EMER", new string[] { person }, "");
                //return empty if not found
            }
            else
            {
                personContactIds = await DataReader.SelectAsync("PERSON.EMER", "");
                Array.Sort(personContactIds);
            }

            var totalCount = personContactIds.Count();

            var personContactDataContracts = new List<PersonEmer>();

            var sublist = personContactIds.Skip(offset).Take(limit);

            var newPersonContactIds = sublist.ToArray();

            if (newPersonContactIds.Any())
            {
                var bulkData = await DataReader.BulkReadRecordAsync<PersonEmer>("PERSON.EMER", newPersonContactIds);
                personContactDataContracts.AddRange(bulkData);
            }

            IEnumerable<PersonContact> personContactList = BuildPersonContacts(personContactDataContracts);

            if (repoException != null && repoException.Errors != null && repoException.Errors.Any())
            {
                throw repoException;
            }

            return new Tuple<IEnumerable<PersonContact>, int>(personContactList, totalCount);
        }



        /// <summary>
        /// Gets person emergency contacts
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<PersonContact>, int>> GetPersonContacts2Async(int offset, int limit, bool bypassCache, string personId, string filterName, string[] filterPersonIds = null)
        {
            try
            {
                string[] personEmerIds = new string[] { };
                string[] personEmerNames = new string[] { };
                var personEmerkeys = new List<string> { };
                var cacheKey = string.Concat("PersonEmergencyContactKeys", filterName, personId);
                // if there is a person Id, just add it to the filterPersonIds array
                if (!string.IsNullOrEmpty(personId))
                {
                    if (!filterPersonIds.Contains(personId))
                    {
                        Array.Resize(ref filterPersonIds, filterPersonIds.Length + 1);
                        filterPersonIds[filterPersonIds.Length - 1] = personId;
                    }
                }
                string criteriaIds = "WITH EMER.NAME NE '' BY.EXP EMER.NAME";
                string criteriaName = "WITH EMER.NAME NE '' BY.EXP EMER.NAME SAVING EMER.NAME";

                if (offset == 0 && ContainsKey(BuildFullCacheKey(cacheKey)))
                {
                    ClearCache(new List<string> { cacheKey });
                }

                personEmerkeys = await GetOrAddToCacheAsync<List<string>>(cacheKey,
                async () =>
                {
                    var keys = new List<string> { };
                    if (filterPersonIds != null && filterPersonIds.Any())
                    {
                        personEmerIds = await DataReader.SelectAsync("PERSON.EMER", filterPersonIds, criteriaIds);
                        personEmerNames = await DataReader.SelectAsync("PERSON.EMER", filterPersonIds, criteriaName);
                    }
                    else
                    {
                        personEmerIds = await DataReader.SelectAsync("PERSON.EMER", criteriaIds);
                        personEmerNames = await DataReader.SelectAsync("PERSON.EMER", criteriaName);
                    }

                    //create a  key with personId | Emername
                    var idx = 0;

                    foreach (var emerId in personEmerIds)
                    {
                        var personEmerId = emerId.Split(DmiString._VM)[0];
                        var emerName = personEmerNames.ElementAt(idx).Split(new[] { '*' })[0];
                        keys.Add(String.Concat(personEmerId, "|", emerName));
                        idx++;
                    }
                    keys.Sort();
                    return keys;
                });
                //check for duplicate keys ( bad data)
                FindDupilicateKeys(personEmerkeys);
                var totalCount = personEmerkeys.Count();
                var keySublist = personEmerkeys.Skip(offset).Take(limit);

                if (keySublist != null && !keySublist.Any())
                {
                    return new Tuple<IEnumerable<PersonContact>, int>(new List<PersonContact>(), 0);
                }
                var subList = new List<string>();

                foreach (var key in keySublist)
                {
                    var emerKey = key.Split('|')[0];
                    subList.Add(emerKey);
                }
                var bulkData = await DataReader.BulkReadRecordAsync<PersonEmer>("PERSON.EMER", subList.Distinct().ToArray());

                var personContactList = await BuildPersonEmergencyContacts(bulkData.ToList(), keySublist.ToList());

                return new Tuple<IEnumerable<PersonContact>, int>(personContactList, totalCount);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
        }

        private void FindDupilicateKeys(List<string> keys)
        {
            var exception = new RepositoryException();
            var duplicates = keys.GroupBy(g => g).Where(w => w.Count() > 1).Select(s => s.Key);
            foreach (var d in duplicates)
            {
                exception.AddError(new RepositoryError("Bad.Data", string.Concat("Duplicate emergency contact name for emergency contact name ", d.Split('|')[1], "."))
                {
                    SourceId = d.Split('|')[0],
                    Id = string.Empty

                });
            }
            if (exception.Errors.Any())
            {
                throw exception;
            }
        }

        /// <summary>
        /// Returns a person contact
        /// </summary>
        /// <param name="ids">Key to person contact to be returned</param>
        /// <returns>personContact</returns>
        public async Task<PersonContact> GetPersonContactByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id)", "Must provide a person contact id");
            }

            var entity = await this.GetRecordInfoFromGuidAsync(id);
            if (entity == null || entity.Entity != "PERSON.EMER")
            {
                throw new KeyNotFoundException("No person contact information for id " + id + ".  Key not found.");
            }

            var personContactDataContract = await DataReader.ReadRecordAsync<PersonEmer>("PERSON.EMER", entity.PrimaryKey);
            var personContact = BuildPersonContact(personContactDataContract);

            return personContact;
        }

        /// <summary>
        /// Returns a person contact
        /// </summary>
        /// <param name="id">Key to person.emer</param>
        /// <returns>personContact</returns>
        public async Task<PersonContact> GetPersonContactById2Async(string id)
        {
            try
            {
                var validKey = await GetPersonEmergencyContactIdFromGuidAsync(id);
                var personContactDataContract = await DataReader.ReadRecordAsync<PersonEmer>("PERSON.EMER", validKey.Split('|')[0]);
                if (personContactDataContract == null)
                {
                    throw new KeyNotFoundException(string.Concat("No emergency contact was found for guid ", id));
                }
                var personContactDtos = await BuildPersonEmergencyContacts(new List<PersonEmer> { personContactDataContract }, new List<string> { validKey });
                if (personContactDtos == null || !personContactDtos.Any())
                {
                    throw new KeyNotFoundException(string.Concat("No emergency contact was found for guid ", id));
                }

                return personContactDtos.FirstOrDefault();
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Builds entity collection
        /// </summary>
        /// <param name="personContactDataContracts"></param>
        /// <returns>IEnumerable<PersonContact></returns>
        private IEnumerable<PersonContact> BuildPersonContacts(IEnumerable<PersonEmer> personContactDataContracts)
        {
            List<PersonContact> personContactsList = new List<PersonContact>();
            foreach (var personContactDataContract in personContactDataContracts)
            {
                PersonContact personContact = BuildPersonContact(personContactDataContract);
                if (personContact != null)
                {
                    personContactsList.Add(personContact);
                }
            }

            return personContactsList;
        }

        /// <summary>
        /// Builds entity
        /// </summary>
        /// <param name="personContactDataContract"></param>
        /// <returns>PersonContact</returns>
        private PersonContact BuildPersonContact(PersonEmer personContactDataContract)
        {
            if (string.IsNullOrEmpty(personContactDataContract.RecordGuid))
            {
                repoException.AddError(new RepositoryError("GUID.Not.Found", string.Concat("GUID not found for person-contacts for person ", personContactDataContract.Recordkey, "."))
                {
                    Id = personContactDataContract.Recordkey
                });
            }
            if (string.IsNullOrEmpty(personContactDataContract.Recordkey))
            {
                // should never happen - data contract with no record key
                repoException.AddError(new RepositoryError("Bad.Data", string.Concat("Record key not found for personContact data contract."))
                {
                    Id = personContactDataContract.Recordkey
                });
            }
            if (!string.IsNullOrEmpty(personContactDataContract.RecordGuid) && !string.IsNullOrEmpty(personContactDataContract.Recordkey))
            {
                PersonContact personContact = new PersonContact(personContactDataContract.RecordGuid, personContactDataContract.Recordkey, personContactDataContract.Recordkey);


                List<PersonContactDetails> personContactDetailsList = new List<PersonContactDetails>();

                foreach (var contact in personContactDataContract.EmerContactsEntityAssociation)
                {
                    PersonContactDetails contactDetails = new PersonContactDetails()
                    {
                        ContactAddresses = contact.EmerContactAddressAssocMember,
                        ContactFlag = contact.EmerEmergencyContactFlagAssocMember,
                        ContactName = contact.EmerNameAssocMember,
                        DaytimePhone = contact.EmerDaytimePhoneAssocMember,
                        EveningPhone = contact.EmerEveningPhoneAssocMember,
                        MissingContactFlag = contact.EmerMissingContactFlagAssocMember,
                        OtherPhone = contact.EmerOtherPhoneAssocMember,
                        Relationship = contact.EmerRelationshipAssocMember
                    };
                    personContactDetailsList.Add(contactDetails);
                }

                if (personContact.PersonContactDetails == null) personContact.PersonContactDetails = new List<PersonContactDetails>();
                personContact.PersonContactDetails = personContactDetailsList;
                return personContact;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Builds entity
        /// </summary>
        /// <param name="personContactDataContract"></param>
        /// <returns>PersonContact</returns>
        private async Task<IEnumerable<PersonContact>> BuildPersonEmergencyContacts(List<PersonEmer> personContactDataContracts, List<string> validKeys)
        {
            var personContacts = new List<PersonContact>();
            var exception = new RepositoryException();
            Dictionary<string, string> guidList = null;
            try
            {
                guidList = await GetPersonEmerNameGuidsCollectionAsync(validKeys, "PERSON.EMER");
            }
            catch
            {
                exception.AddError(new RepositoryError("GUID.Not.Found", "No GUIDs were found for PERSON.EMER"));
                throw exception;
            }

            if (personContactDataContracts != null && personContactDataContracts.Any())
            {
                foreach (var personContactDataContract in personContactDataContracts)
                {
                    if (string.IsNullOrEmpty(personContactDataContract.RecordGuid))
                    {
                        exception.AddError(new RepositoryError("GUID.Not.Found", string.Concat("GUID not found for person-contacts for person ", personContactDataContract.Recordkey, "."))
                        {
                            Id = personContactDataContract.Recordkey
                        });
                    }
                    if (string.IsNullOrEmpty(personContactDataContract.Recordkey))
                    {
                        // should never happen - data contract with no record key
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat("Record key not found for personContact data contract."))
                        {
                            Id = personContactDataContract.Recordkey
                        });
                    }
                    if (!string.IsNullOrEmpty(personContactDataContract.RecordGuid) && !string.IsNullOrEmpty(personContactDataContract.Recordkey))
                    {

                        PersonContact personContact = new PersonContact(personContactDataContract.RecordGuid, personContactDataContract.Recordkey, personContactDataContract.Recordkey);
                        List<PersonContactDetails> personContactDetailsList = new List<PersonContactDetails>();

                        foreach (var contact in personContactDataContract.EmerContactsEntityAssociation)
                        {
                            //we need to make sure person.emer.id|key is in the list of valid Keys
                            var key = string.Concat(personContactDataContract.Recordkey, "|", contact.EmerNameAssocMember);
                            var validRecord = validKeys.FirstOrDefault(x => x.Equals(key, StringComparison.Ordinal));
                            if (validRecord != null)
                            {
                                var emerNameGuid = string.Empty;
                                guidList.TryGetValue(key, out emerNameGuid);
                                if (string.IsNullOrEmpty(emerNameGuid))
                                {
                                    exception.AddError(new RepositoryError("GUID.Not.Found", string.Concat("GUID not found for person-emergency-contacts for ", personContactDataContract.Recordkey,
                                        " for emergency contact name ", key.Split('|')[1], "."))
                                    {
                                        SourceId = key.Split('|')[0]
                                    });
                                }
                                else
                                {
                                    PersonContactDetails contactDetails = new PersonContactDetails()
                                    {
                                        ContactAddresses = contact.EmerContactAddressAssocMember,
                                        ContactFlag = contact.EmerEmergencyContactFlagAssocMember,
                                        ContactName = contact.EmerNameAssocMember,
                                        DaytimePhone = contact.EmerDaytimePhoneAssocMember,
                                        EveningPhone = contact.EmerEveningPhoneAssocMember,
                                        MissingContactFlag = contact.EmerMissingContactFlagAssocMember,
                                        OtherPhone = contact.EmerOtherPhoneAssocMember,
                                        Relationship = contact.EmerRelationshipAssocMember
                                    };

                                    contactDetails.Guid = emerNameGuid;

                                    // do some data validation
                                    //the emergency contact flag cannot be null.
                                    if (string.IsNullOrEmpty(contactDetails.ContactFlag))
                                    {
                                        exception.AddError(new RepositoryError("Bad.Data", string.Concat("Emergency Contact cannot be null. It must be Yes or No for emergency contact name ", key.Split('|')[1], "."))
                                        {
                                            SourceId = key.Split('|')[0],
                                            Id = contactDetails.Guid

                                        });
                                    }

                                    //the missing contact flag cannot be null.
                                    if (string.IsNullOrEmpty(contactDetails.MissingContactFlag))
                                    {
                                        exception.AddError(new RepositoryError("Bad.Data", string.Concat("Missing-Person Contact cannot be null. It must be Yes or No for emergency contact name ", key.Split('|')[1], "."))
                                        {
                                            SourceId = key.Split('|')[0],
                                            Id = contactDetails.Guid

                                        });
                                    }

                                    //one of emergency contact flag or missing contact flag should be set to Y

                                    if (!string.IsNullOrEmpty(contactDetails.MissingContactFlag) && !string.IsNullOrEmpty(contactDetails.ContactFlag) && contactDetails.ContactFlag.Equals("N", StringComparison.OrdinalIgnoreCase) && contactDetails.MissingContactFlag.Equals("N", StringComparison.OrdinalIgnoreCase))
                                    {
                                        exception.AddError(new RepositoryError("Bad.Data", string.Concat("The emergency contact needs to be designated as an emergency contact or a missing person contact (or both) for emergency contact name ", key.Split('|')[1], "."))
                                        {
                                            SourceId = key.Split('|')[0],
                                            Id = contactDetails.Guid

                                        });
                                    }

                                    //the missing contact flag cannot be null.
                                    if (string.IsNullOrEmpty(contactDetails.ContactName))
                                    {
                                        exception.AddError(new RepositoryError("Bad.Data", string.Concat("Contact Name is required for emergency contact name ", key.Split('|')[1], "."))
                                        {
                                            SourceId = key.Split('|')[0],
                                            Id = contactDetails.Guid

                                        });
                                    }
                                    else
                                    {
                                        // there should not be any dupilcate emer name 
                                        var emerName = personContactDataContract.EmerContactsEntityAssociation.Where(x => x.EmerNameAssocMember.Equals(contactDetails.ContactName));
                                        if (emerName.Count() > 1)
                                        {
                                            exception.AddError(new RepositoryError("Bad.Data", string.Concat("Duplicate Emergency Contact Names found for emergency contact name ", key.Split('|')[1], "."))
                                            {
                                                SourceId = key.Split('|')[0],
                                                Id = contactDetails.Guid

                                            });
                                        }

                                    }

                                    personContactDetailsList.Add(contactDetails);
                                }
                            }
                        }
                        if (personContactDetailsList != null && personContactDetailsList.Any())
                        {
                            personContact.PersonContactDetails = personContactDetailsList;
                            personContacts.Add(personContact);
                        }
                    }
                }
            }

            if (exception.Errors.Any())
            {
                throw exception;
            }
            return personContacts;
        }

        /// <summary>
        /// Using a collection of PERSON.EMER ids, get a dictionary collection of associated secondary guids on EMER.NAME
        /// </summary>
        /// <param name="ids">collection of  ids</param>
        /// <returns>Dictionary consisting of a ids (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetPersonEmerNameGuidsCollectionAsync(IEnumerable<string> ids, string filename)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();

            try
            {
                var guidLookup = ids
                   .Where(s => !string.IsNullOrWhiteSpace(s))
                   .Distinct().ToList()
                   .ConvertAll(p => new RecordKeyLookup(filename, p.Split('|')[0], "EMER.NAME", p.Split('|')[1], false)).ToArray();

                var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(string.Concat(splitKeys[1], "|", splitKeys[2]), recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Format("Error occured while getting guids for {0}.", filename), ex); ;
            }

            return guidCollection;
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetPersonEmergencyContactIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new RepositoryException("guid is required.");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException(string.Concat("No emergency contact was found for guid ", guid));
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException(string.Concat("No emergency contact was found for guid ", guid));
            }

            if (foundEntry.Value.Entity != "PERSON.EMER")
            {
                throw new RepositoryException(string.Concat("The GUID specified: ", guid, " is used by a different resource: ", foundEntry.Value.Entity, " than expected: PERSON.EMER."));
            }
            if (string.IsNullOrEmpty(foundEntry.Value.PrimaryKey))
            {
                throw new RepositoryException(string.Concat("The GUID specified: ", guid, " from file: PERSON.EMER is not valid for person-emergency-contacts. "));
            }
            if (string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new RepositoryException(string.Concat("The GUID specified: ", guid, " for record key ", foundEntry.Value.PrimaryKey, " from file: PERSON.EMER is not valid for person-emergency-contacts. "));
            }
            return string.Concat(foundEntry.Value.PrimaryKey, "|", foundEntry.Value.SecondaryKey); ;
        }

        /// <summary>
        /// Update person-emergency-contacts 
        /// </summary>
        /// <param name="personEmergencyContactsEntity">personEmergencyContactsEntity entity</param>
        /// <returns>PersonContact entity</returns>
        public async Task<PersonContact> UpdatePersonEmergencyContactsAsync(PersonContact personEmergencyContactsEntity)
        {
            if (personEmergencyContactsEntity == null)
            {
                throw new ArgumentNullException("personEmergencyContactsEntity", "Must provide a person emergency contact to create.");
            }
            var extendedDataTuple = GetEthosExtendedDataLists();

            var request = new UpdatePersonEmerRequest();
            if (personEmergencyContactsEntity.PersonContactRecordKey != "NEW")
            {
                request.EmerId = personEmergencyContactsEntity.PersonContactRecordKey;
            }
            request.PersonId = personEmergencyContactsEntity.SubjectPersonId;
            if (personEmergencyContactsEntity.PersonContactDetails != null && personEmergencyContactsEntity.PersonContactDetails.Any())
            {
                foreach (var contact in personEmergencyContactsEntity.PersonContactDetails)
                {
                    request.ContactName = contact.ContactName;
                    request.DaytimePhone = contact.DaytimePhone;
                    request.EmerFlag = contact.ContactFlag;
                    request.EveningPhone = contact.EveningPhone;
                    request.MissFlag = contact.MissingContactFlag;
                    request.OtherPhone = contact.OtherPhone;
                    request.Relationship = contact.Relationship;
                    request.EmerNameGuid = contact.Guid;
                }
            }

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var response = await transactionInvoker.ExecuteAsync<UpdatePersonEmerRequest, UpdatePersonEmerResponse>(request);

            // If there is any error message - throw an exception
            if (response.UpdatePersonEmerErrors != null && response.UpdatePersonEmerErrors.Any())
            {
                var exception = new RepositoryException();
                foreach (var error in response.UpdatePersonEmerErrors)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCodes, " - ", error.ErrorMessages))
                    {
                        SourceId = request.EmerId,
                        Id = request.EmerNameGuid

                    });
                }
                throw exception;
            }
            return await GetPersonContactById2Async(response.EmerNameGuid);
        }

        /// <summary>
        /// Delete person-emergency-contacts 
        /// </summary>
        /// <param name="personEmergencyContactsEntity">personEmergencyContactsEntity entity</param>
        /// <returns>nothing</returns>
        /// 
        public async Task DeletePersonEmergencyContactsAsync(PersonContact personEmergencyContactsEntity)
        {
            if (personEmergencyContactsEntity == null)
            {
                throw new ArgumentNullException("personEmergencyContactsEntity", "Must provide a person emergency contact to delete.");
            }
            var request = new DeletePersonEmerRequest()
            {
                EmerNameGuid = personEmergencyContactsEntity.PersonContactDetails.FirstOrDefault().Guid
            };
            var response = await transactionInvoker.ExecuteAsync<DeletePersonEmerRequest, DeletePersonEmerResponse>(request);

            if (response.Error)
            {
                var exception = new RepositoryException();
                foreach (var error in response.DeletePersonEmerErrors)
                {

                    exception.AddError(new RepositoryError("Delete.Exception", string.Concat(error.ErrorCodes, " - ", error.ErrorMessages))
                    {
                        SourceId = personEmergencyContactsEntity.PersonContactRecordKey,
                        Id = personEmergencyContactsEntity.PersonContactGuid

                    });
                }
                throw exception;
            }
        }
        #endregion
    }
}
