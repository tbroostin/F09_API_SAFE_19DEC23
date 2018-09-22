// Copyright 2016-2018 Ellucian Company L.P. and its affiliatesusing System;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// EmergencyInformationService is an application that responds to a request for emergency information about a person
    /// </summary>
    [RegisterType]
    public class EmergencyInformationService : BaseCoordinationService, IEmergencyInformationService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IPersonBaseRepository _personBaseRepository;
        private readonly IEmergencyInformationRepository _emergencyInformationRepository;
        private readonly IConfigurationRepository _configurationRepository;
        /// <summary>
        /// Initializes a new instance of the <see cref="EmergencyInformationService"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="currentUserFactory">The current user factory.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="emergencyInformationRepository">The emergency information repository.</param>
        /// <param name="configurationRepository">The configuration repository.</param>
        /// <param name="personBaseRepository">The person base repository</param>
        /// <param name="staffRepository">The staff repository</param>
        /// <param name="personRepository">The person repository</param>
        public EmergencyInformationService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IEmergencyInformationRepository emergencyInformationRepository, IConfigurationRepository configurationRepository, IPersonBaseRepository personBaseRepository, IStaffRepository staffRepository, IPersonRepository personRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            this._emergencyInformationRepository = emergencyInformationRepository;
            this._configurationRepository = configurationRepository;
            this._personBaseRepository = personBaseRepository;
            this._personRepository = personRepository;
        }

        /// <summary>
        /// Get all the emergency information for a person. The person whose emergency information is returned
        /// must be same person as the current user.
        /// </summary>
        /// <param name="personId">The person's ID.</param>
        /// <returns>An EmergencyInformation object.</returns>
        [Obsolete("Obsolete as of API 1.16. This services old versions of the API. Use GetEmergencyInformation2Async instead.")]
        public async Task<EmergencyInformation> GetEmergencyInformationAsync(string personId)

        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A person's id is required to retrieve emergency information");
            }

            // For now, emergency information may only be viewed by that person. That may need to change with future
            // planned functionality for designated staff members who need to be able to access this information
            // about students or employees during actual emergency situations.
            if (!CurrentUser.IsPerson(personId))
            {
                string message = CurrentUser + " cannot view emergency information for person " + personId;
                logger.Info(message);
                throw new PermissionsException();
            }


            var emergencyInformation = _emergencyInformationRepository.Get(personId);

            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.EmergencyInformation, EmergencyInformation>();

            var emergencyInformationDto = adapter.MapToType(emergencyInformation);

            return emergencyInformationDto;
        }

        /// <summary>
        /// Get the emergency information for a person. Emergency information may be
        /// missing if the user is lacking permissions.
        /// </summary>
        /// <param name="personId">The person's ID.</param>
        /// <returns>An EmergencyInformation object with potential privacy restrictions.</returns>
        public async Task<PrivacyWrapper<EmergencyInformation>> GetEmergencyInformation2Async(string personId)

        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A person's id is required to retrieve emergency information");
            }

            // Only allow the user or those with special permissions to access emergency info
            if (!CurrentUser.IsPerson(personId) && !CanSeeAnyEmergencyInformation())
            {
                string message = CurrentUser + " cannot view emergency information for person " + personId;
                logger.Info(message);
                throw new PermissionsException();
            }


            var emergencyInformation = _emergencyInformationRepository.Get(personId);
            var personBase = await _personBaseRepository.GetPersonBaseAsync(personId);

            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.EmergencyInformation, EmergencyInformation>();

            var emergencyInformationDto = adapter.MapToType(emergencyInformation);
            if (personBase != null)
            {
                emergencyInformationDto.PersonName = personBase.PreferredName;
            }

            // Remove information from Dto if the user is lacking permissions
            bool restricted = false;
            if (!CurrentUser.IsPerson(personId))
            {
                // Check the user's privacy overrides against the person's privacy codes
                var hasPrivacyRestriction = string.IsNullOrEmpty(personBase.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(personBase.PrivacyStatusCode);
                emergencyInformationDto.PrivacyStatusCode = personBase.PrivacyStatusCode;

                if (hasPrivacyRestriction || !HasPermission(BasePermissionCodes.ViewPersonEmergencyContacts))
                {
                    emergencyInformationDto.EmergencyContacts = null;
                    emergencyInformationDto.OptOut = false;
                    restricted = true;
                }
                if (hasPrivacyRestriction || !HasPermission(BasePermissionCodes.ViewPersonOtherEmergencyInformation))
                {
                    emergencyInformationDto.AdditionalInformation = null;
                    emergencyInformationDto.HospitalPreference = null;
                    emergencyInformationDto.InsuranceInformation = null;
                    restricted = true;
                }
                if (hasPrivacyRestriction || !HasPermission(BasePermissionCodes.ViewPersonHealthConditions))
                {
                    emergencyInformationDto.HealthConditions = null;
                    restricted = true;
                }
            }


            return new PrivacyWrapper<EmergencyInformation>(emergencyInformationDto, restricted);
        }

        #region Person Emergency Contacts

        /// <summary>
        /// Get Person Emergency Contacts
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="person"></param>
        /// <returns>Tuple<IEnumerable<Dtos.PersonContactSubject>, int></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonContactSubject>, int>> GetPersonEmergencyContactsAsync(int offset, int limit, bool bypassCache, string person = "" )
        {
            CheckUserPersonContactsViewPermissions();

            string newPerson = string.Empty;
            if (!string.IsNullOrEmpty(person))
            {
                try
                {
                    newPerson = await _personRepository.GetPersonIdFromGuidAsync(person);
                }
                catch (Exception)
                {
                    // return empty set if exception thrown because unable to find a matching person GUID
                    return new Tuple<IEnumerable<Dtos.PersonContactSubject>, int>(new List<Dtos.PersonContactSubject>(), 0);
                }
                if (string.IsNullOrEmpty(newPerson))
                    return new Tuple<IEnumerable<Dtos.PersonContactSubject>, int>(new List<Dtos.PersonContactSubject>(), 0);
            }

            var personContactEntites = await _emergencyInformationRepository.GetPersonContactsAsync(offset, limit, bypassCache, newPerson);

            var personContactDtos = await ConvertPersonContactEntitesToDto(personContactEntites.Item1);

            return new Tuple<IEnumerable<Dtos.PersonContactSubject>, int>(personContactDtos, personContactEntites.Item2);
        }

        /// <summary>
        /// Gets Person Emergency Contact By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.PersonContactSubject</returns>
        public async Task<Dtos.PersonContactSubject> GetPersonEmergencyContactByIdAsync(string id)
        {
            CheckUserPersonContactsViewPermissions();

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Person contact id is required");
            }
            var personContactEntity = await _emergencyInformationRepository.GetPersonContactByIdAsync(id);
            var personContactDto = await ConvertPersonContactEntityToDto(personContactEntity);
            return personContactDto;
        }

        /// <summary>
        /// Converts entity to dto
        /// </summary>
        /// <param name="personContactEntites"></param>
        /// <returns>IEnumerable<PersonContactSubject></returns>
        private async Task<IEnumerable<PersonContactSubject>> ConvertPersonContactEntitesToDto(IEnumerable<Domain.Base.Entities.PersonContact> personContactEntites)
        {
            List<PersonContactSubject> personContactSubjects = new List<PersonContactSubject>();

            foreach (var personContactEntity in personContactEntites)
            {
                PersonContactSubject personContactSubject = await ConvertPersonContactEntityToDto(personContactEntity);

                personContactSubjects.Add(personContactSubject);
            }

            return personContactSubjects;
        }

        /// <summary>
        /// Converts entity to dto
        /// </summary>
        /// <param name="personContactEntity"></param>
        /// <returns>PersonContactSubject</returns>
        private async Task<PersonContactSubject> ConvertPersonContactEntityToDto(Domain.Base.Entities.PersonContact personContactEntity)
        {
            PersonContactSubject personContactSubject = new PersonContactSubject();
            personContactSubject.Id = personContactEntity.PersonContactGuid;
            personContactSubject.Person = await ConvertPersonEntityId(personContactEntity);
            personContactSubject.Contacts = ConvertContactEntityToDto(personContactEntity.PersonContactDetails);
            return personContactSubject;
        }

        /// <summary>
        /// Converts person entity id to guid
        /// </summary>
        /// <param name="personContactEntity"></param>
        /// <returns>GuidObject2</returns>
        private async Task<GuidObject2> ConvertPersonEntityId(Domain.Base.Entities.PersonContact personContactEntity)
        {
            string id = await _personBaseRepository.GetPersonGuidFromOpersAsync(personContactEntity.SubjectPersonId);
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException(string.Format("Did not find any person Id with: {0}", id));
            }
            return new GuidObject2(id);
        }

        /// <summary>
        /// Converts entity to dto
        /// </summary>
        /// <param name="personContactDetails"></param>
        /// <returns>PersonContact</returns>
        private IEnumerable<PersonContact> ConvertContactEntityToDto(IEnumerable<Domain.Base.Entities.PersonContactDetails> personContactDetails)
        {
            List<PersonContact> personContacts = new List<PersonContact>();

            if (personContactDetails.Any())
            {
                foreach (var personContactDetail in personContactDetails)
                {
                    PersonContact personContact = new PersonContact();
                    personContact.ContactAddress = string.IsNullOrEmpty(personContactDetail.ContactAddresses) ? null : ConvertEntityAddressToDto(personContactDetail.ContactAddresses);
                    personContact.ContactName = new PersonContactName() { FullName = personContactDetail.ContactName };
                    personContact.ContactRelationship = string.IsNullOrEmpty(personContactDetail.Relationship) ? null : personContactDetail.Relationship;
                    //change code once the specs changes are made
                    personContact.PersonContactPhone = ConvertToPhone(personContactDetail.DaytimePhone, personContactDetail.EveningPhone, personContactDetail.OtherPhone);
                    personContact.Types = ConvertToContactTypes(personContactDetail.ContactFlag, personContactDetail.MissingContactFlag);
                    personContacts.Add(personContact);
                }
            }
            return personContacts.Any() ? personContacts : null;
        }

        /// <summary>
        /// Converts day, evening & other type phone numbers to PersonContactPhoneDtoProperty
        /// </summary>
        /// <param name="dayTimePhone"></param>
        /// <param name="eveningPhone"></param>
        /// <param name="OtherPhone"></param>
        /// <returns>IEnumerable<PersonContactPhoneDtoProperty></returns>
        private IEnumerable<PersonContactPhoneDtoProperty> ConvertToPhone(string dayTimePhone, string eveningPhone, string OtherPhone)
        {
            List<PersonContactPhoneDtoProperty> personContactPhoneDtoPropertyList = new List<PersonContactPhoneDtoProperty>();

            if (!string.IsNullOrEmpty(dayTimePhone))
            {
                PersonContactPhoneDtoProperty dayTime = GetPersonContactPhoneDto(dayTimePhone, BestContactTimeType.Day);
                personContactPhoneDtoPropertyList.Add(dayTime);
            }
            if (!string.IsNullOrEmpty(eveningPhone))
            {
                PersonContactPhoneDtoProperty eveningTime = GetPersonContactPhoneDto(eveningPhone, BestContactTimeType.Evening);
                personContactPhoneDtoPropertyList.Add(eveningTime);
            }
            if (!string.IsNullOrEmpty(OtherPhone))
            {
                PersonContactPhoneDtoProperty otherTime = GetPersonContactPhoneDto(OtherPhone, BestContactTimeType.Anytime);
                personContactPhoneDtoPropertyList.Add(otherTime);
            }
            return personContactPhoneDtoPropertyList.Any() ? personContactPhoneDtoPropertyList : null;
        }

        /// <summary>
        /// Gets Contact phone dto
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="contactTimeType"></param>
        /// <returns>PersonContactPhoneDtoProperty</returns>
        private static PersonContactPhoneDtoProperty GetPersonContactPhoneDto(string phone, BestContactTimeType contactTimeType)
        {
            PersonContactPhoneDtoProperty personContactPhoneDtoProperty = new PersonContactPhoneDtoProperty()
            {
                ContactAvailability = contactTimeType,
                Number = phone,
                CountryCallingCode = null,
                Extension = null
            };
            return personContactPhoneDtoProperty;
        }

        /// <summary>
        /// Converts phone entity to dto
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        private PersonPhoneDtoProperty ConvertToPhone(string phoneNumber)
        {
            PersonPhoneDtoProperty personPhone = new PersonPhoneDtoProperty()
            {
                Number = phoneNumber,
                Type = new PersonPhoneTypeDtoProperty() { PhoneType = PersonPhoneTypeCategory.Other }
            };

            return personPhone;
        }

        /// <summary>
        /// Coverts contact to type
        /// </summary>
        /// <param name="emergencyContactFlag"></param>
        /// <param name="missingPersonContactFlag"></param>
        /// <returns>IEnumerable<PersonContactType></returns>
        private IEnumerable<PersonContactType> ConvertToContactTypes(string emergencyContactFlag, string missingPersonContactFlag)
        {
            List<PersonContactType> contactTypes = new List<PersonContactType>();
            if (!string.IsNullOrEmpty(emergencyContactFlag) && emergencyContactFlag.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                contactTypes.Add(PersonContactType.Emergency);
            }
            if (!string.IsNullOrEmpty(missingPersonContactFlag) && missingPersonContactFlag.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                contactTypes.Add(PersonContactType.MissingPerson);
            }
            return contactTypes.Any() ? contactTypes : null;
        }

        /// <summary>
        /// Converts string to dto object
        /// </summary>
        /// <param name="contactAddresses"></param>
        /// <returns>PersonContactAddress</returns>
        private PersonContactAddress ConvertEntityAddressToDto(string contactAddresses)
        {
            PersonContactAddress addr = new PersonContactAddress();
            addr.ContactFullAddress = new List<string>() { contactAddresses };
            return addr;
        }
        #endregion

        /// <summary>
        /// Update the emergency information for a person.
        /// </summary>
        /// <param name="emergencyInformation">An EmergencyInformation object.</param>
        /// <returns>An EmergencyInformation DTO.</returns>
        public EmergencyInformation UpdateEmergencyInformation(EmergencyInformation emergencyInformation)
        {
            if (emergencyInformation == null)
            {
                throw new ArgumentNullException("emergencyInformation", "Cannot update emergency information with a null object");
            }

            var emergencyConfigurationRepo = Task.Run(async () => { return await _configurationRepository.GetEmergencyInformationConfigurationAsync(); }).Result;

            if (!emergencyConfigurationRepo.AllowOptOut)
            {
                emergencyInformation.OptOut = false;
            }

            // Emergency information may only be updated in the self-service functionality by that person (not by someone else). 
            // If someone's emergency information needs to be updated by a staff member, the staff member should use the back
            // office Colleague form.  
            if (!CurrentUser.IsPerson(emergencyInformation.PersonId))
            {
                string message = CurrentUser + " cannot update emergency information for person " + emergencyInformation.PersonId;
                logger.Info(message);
                throw new PermissionsException();
            }

            var adapter = _adapterRegistry.GetAdapter<EmergencyInformation, Domain.Base.Entities.EmergencyInformation>();

            var emergencyInformationDomain = adapter.MapToType(emergencyInformation);

            var updatedEmergencyInformationDomain = _emergencyInformationRepository.UpdateEmergencyInformation(emergencyInformationDomain);

            var reverseAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.EmergencyInformation, EmergencyInformation>();

            var emergencyInformationDto = reverseAdapter.MapToType(updatedEmergencyInformationDomain);

            return emergencyInformationDto;
        }

        /// <summary>
        /// Checks permissions to see if any emergency information is visible for the user
        /// </summary>
        /// <returns>True if the user can see any emergency information, otherwise false.</returns>
        private bool CanSeeAnyEmergencyInformation()
        {
            return HasPermission(BasePermissionCodes.ViewPersonEmergencyContacts) ||
                   HasPermission(BasePermissionCodes.ViewPersonOtherEmergencyInformation) ||
                   HasPermission(BasePermissionCodes.ViewPersonEmergencyContacts);
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view a person's contact.
        /// </summary>
        private void CheckUserPersonContactsViewPermissions()
        {
            // access is ok if the current user has the view person contacts permission
            if (!HasPermission(BasePermissionCodes.ViewAnyPersonContact))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view person-contacts.");
                throw new PermissionsException("User is not authorized to view person-contacts.");
            }
        }

    }
}