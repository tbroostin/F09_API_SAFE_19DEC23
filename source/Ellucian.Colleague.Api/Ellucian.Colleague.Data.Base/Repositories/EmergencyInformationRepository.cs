// Copyright 2016-2017 Ellucian Company L.P. and its affiliatesusing System
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
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
                    var insuranceInfo = emergencyInfoContract.EmerInsuranceInfo.Replace(Convert.ToChar(DynamicArray.VM), '\n');
                    emergencyInfoEntity.InsuranceInformation = insuranceInfo;
                }

                // The new Additional Emergency Information ("comment") field is where people can 
                // self-disclose additional information that might be necessary in case of emergency.
                // Similarly to Insurance Informationm, we need to convert value marks to new line
                // characters because we want to maintain any formatting (line-to-line) that the user may
                // have entered.
                if (!string.IsNullOrEmpty(emergencyInfoContract.EmerAddnlInformation))
                {
                    var additionalInfo = emergencyInfoContract.EmerAddnlInformation.Replace(Convert.ToChar(DynamicArray.VM), '\n');
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
                logger.Error("Error reading emergency information for person " + personId);
                logger.Error(e.Message);  
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
                        throw new Exception(errorMessage);
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
            updateEmergencyInformationRequest.OptOut = emergencyInformation.OptOut ? "y": "n";

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
            string[] personContactIds = new string[]{ };
            
            if (!string.IsNullOrEmpty(person))
            {
                personContactIds = await DataReader.SelectAsync("PERSON.EMER", new string[] {person}, "");
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

            return new Tuple<IEnumerable<PersonContact>, int>(personContactList, totalCount); 
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
                personContactsList.Add(personContact);
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

        #endregion
    }
}
