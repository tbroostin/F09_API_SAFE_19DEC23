// Copyright 2016-2019 Ellucian Company L.P. and its affiliatesusing System;
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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

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
        private readonly IReferenceDataRepository _referenceDataRepository;
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
        public EmergencyInformationService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IEmergencyInformationRepository emergencyInformationRepository, IConfigurationRepository configurationRepository, IPersonBaseRepository personBaseRepository, IStaffRepository staffRepository, IPersonRepository personRepository, IReferenceDataRepository referenceDataRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            this._emergencyInformationRepository = emergencyInformationRepository;
            this._configurationRepository = configurationRepository;
            this._personBaseRepository = personBaseRepository;
            this._personRepository = personRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        //get emergency types
        private IEnumerable<Domain.Base.Entities.IntgPersonEmerTypes> _intgPersonEmerTypes = null;
        private async Task<IEnumerable<Domain.Base.Entities.IntgPersonEmerTypes>> GetIntgPersonEmerTypesAsync(bool ignoreCache)
        {
            if (_intgPersonEmerTypes == null)
            {
                _intgPersonEmerTypes = await _referenceDataRepository.GetIntgPersonEmerTypesAsync(ignoreCache);
            }
            return _intgPersonEmerTypes;
        }

        //get emergency phone types
        private IEnumerable<Domain.Base.Entities.IntgPersonEmerPhoneTypes> _intgPersonEmerPhoneTypes = null;
        private async Task<IEnumerable<Domain.Base.Entities.IntgPersonEmerPhoneTypes>> GetIntgPersonEmerPhoneTypesAsync(bool ignoreCache)
        {
            if (_intgPersonEmerPhoneTypes == null)
            {
                _intgPersonEmerPhoneTypes = await _referenceDataRepository.GetIntgPersonEmerPhoneTypesAsync(ignoreCache);
            }
            return _intgPersonEmerPhoneTypes;
        }

        //get relationship types
        private IEnumerable<Domain.Base.Entities.RelationType> _relationshipTypes = null;
        private async Task<IEnumerable<Domain.Base.Entities.RelationType>> GetRelationTypesAsync(bool ignoreCache)
        {
            if (_relationshipTypes == null)
            {
                _relationshipTypes = await _referenceDataRepository.GetRelationTypesAsync(ignoreCache);
            }
            return _relationshipTypes;
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
                string message = CurrentUser.PersonId + " requested but cannot view emergency information for person " + personId;
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
                string message = CurrentUser.PersonId + " requested but cannot view emergency information for person " + personId;
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
        public async Task<Tuple<IEnumerable<Dtos.PersonContactSubject>, int>> GetPersonEmergencyContactsAsync(int offset, int limit, bool bypassCache, string person = "")
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
        /// Get Person Emergency Contacts
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="person"></param>
        /// <returns>Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int>> GetPersonEmergencyContacts2Async(int offset, int limit, Dtos.PersonEmergencyContacts criteriaObj, string personFilterValue, bool bypassCache)
        {
            try
            {
                CheckUserPersonEmergencyContactsViewPermissions();

                string[] filterPersonIds = new List<string>().ToArray();
                string personId = string.Empty;

                if (!string.IsNullOrEmpty(personFilterValue))
                {
                    try
                    {
                        var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilterValue));
                        if (personFilterKeys != null)
                        {
                            filterPersonIds = personFilterKeys;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int>(new List<Dtos.PersonEmergencyContacts>(), 0);
                        }
                    }
                    catch
                    {
                        return new Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int>(new List<Dtos.PersonEmergencyContacts>(), 0);
                    }

                }

                if (criteriaObj.Person != null && !string.IsNullOrEmpty(criteriaObj.Person.Id))
                {
                    try
                    {
                        personId = await _personRepository.GetPersonIdFromGuidAsync(criteriaObj.Person.Id);
                    }
                    catch
                    {
                        return new Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int>(new List<Dtos.PersonEmergencyContacts>(), 0);
                    }

                }

                var personContactEntitesTuple = await _emergencyInformationRepository.GetPersonContacts2Async(offset, limit, bypassCache, personId, personFilterValue, filterPersonIds);
                if (personContactEntitesTuple != null)
                {
                    var personContactEntites = personContactEntitesTuple.Item1;
                    var totalCount = personContactEntitesTuple.Item2;
                    if (personContactEntites != null && personContactEntites.Any())
                    {
                        var personEmerDtos = new List<Dtos.PersonEmergencyContacts>();
                        var ids = personContactEntites.Where(x => (!string.IsNullOrEmpty(x.SubjectPersonId))).Select(x => x.SubjectPersonId).Distinct().ToList();
                        var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);                       
                            foreach (var perContact in personContactEntites)
                            {
                                var dtos = await ConvertPersonContactEntitiesToDto2Async(perContact, personGuidCollection);
                                if (dtos != null && dtos.Any())
                                {
                                    personEmerDtos.AddRange(dtos);
                                }
                            }
                            if (IntegrationApiException != null)
                            throw IntegrationApiException;
                        return new Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int>(personEmerDtos, totalCount);

                    }

                    return new Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int>(new List<Dtos.PersonEmergencyContacts>(), 0);

                }

                return new Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int>(new List<Dtos.PersonEmergencyContacts>(), 0);
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

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
        /// Gets Person Emergency Contact By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.PersonEmergencyContacts</returns>
        public async Task<Dtos.PersonEmergencyContacts> GetPersonEmergencyContactsByGuid2Async(string id, bool bypassCache)
        {
            try
            {
                CheckUserPersonEmergencyContactsViewPermissions();

                if (string.IsNullOrEmpty(id))
                {
                    IntegrationApiExceptionAddError("GUID is required to get a person emergency contact.", "guid");
                    throw IntegrationApiException;
                }
                var personContactEntity = await _emergencyInformationRepository.GetPersonContactById2Async(id);
                if (personContactEntity == null)
                {
                    throw new KeyNotFoundException(string.Concat("No emergency contact was found for guid ", id));
                }
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync
                   (new List<string>() { personContactEntity.SubjectPersonId });
                var dtos = await ConvertPersonContactEntitiesToDto2Async(personContactEntity, personGuidCollection);

                return dtos.FirstOrDefault(emer => emer.Id == id);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
        /// Converts entity to dto
        /// </summary>
        /// <param name="personContactEntity"></param>
        /// <returns>PersonEmergencyContacts</returns>
        private async Task<List<PersonEmergencyContacts>> ConvertPersonContactEntitiesToDto2Async(Domain.Base.Entities.PersonContact personContactEntity, Dictionary<string, string> personGuidCollection)
        {
            var personEmerContactDtos = new List<PersonEmergencyContacts>();
            if (personContactEntity.PersonContactDetails != null && personContactEntity.PersonContactDetails.Any())
            {
                foreach (var contact in personContactEntity.PersonContactDetails)
                {
                    var personEmerContactDto = new PersonEmergencyContacts();
                    //get the guid for each contact
                    personEmerContactDto.Id = contact.Guid;
                    // Make sure whe have a valid GUID for the record we are dealing with
                    if (string.IsNullOrEmpty(personEmerContactDto.Id))
                    {
                        IntegrationApiExceptionAddError("Unable to locate GUID", "GUID.Not.Found", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);
                    }
                    // get person Guid
                    if (personGuidCollection != null && personGuidCollection.Any())
                    {
                        var personGuid = string.Empty;
                        personGuidCollection.TryGetValue(personContactEntity.SubjectPersonId, out personGuid);
                        if (!string.IsNullOrEmpty(personGuid))
                        {
                            personEmerContactDto.Person = new Dtos.GuidObject2(personGuid);
                        }
                        else
                        {
                            IntegrationApiExceptionAddError("Unable to locate GUID for person Id" + personContactEntity.SubjectPersonId, "GUID.Not.Found", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);

                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError("Unable to locate GUID for person Id" + personContactEntity.SubjectPersonId, "GUID.Not.Found", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);
                    }
                    //get contact details
                    var perEmerContactDetail = new PersonEmergencyContactsContact();
                    // get contact.name
                    if (!string.IsNullOrEmpty(contact.ContactName))
                    {
                        var name = new PersonContactName();
                        name.FullName = contact.ContactName;
                        perEmerContactDetail.Name = name;
                    }
                    else
                    {
                        IntegrationApiExceptionAddError("Contact Name is required.", "Bad.Data", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);
                    }
                    //get contact typeS guid
                    var emerContactTypes = await GetIntgPersonEmerTypesAsync(false);
                    if (emerContactTypes == null)
                    {
                        IntegrationApiExceptionAddError("Unable to locate emergency-contact-types", "Global.Internal.Error", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);
                    }
                    else
                    {
                        perEmerContactDetail.Types = new List<GuidObject2>();
                        if (!string.IsNullOrEmpty(contact.ContactFlag) && contact.ContactFlag.Equals("Y", StringComparison.OrdinalIgnoreCase))
                        {
                            var type = emerContactTypes.FirstOrDefault(typ => typ.Code == "EMER");
                            if (type != null && !string.IsNullOrEmpty(type.Guid))
                            {
                                perEmerContactDetail.Types.Add(new GuidObject2(type.Guid));
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("Unable to locate GUid for emergency-contact-types of EMER", "Global.Internal.Error", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);
                            }
                        }

                        if (!string.IsNullOrEmpty(contact.MissingContactFlag) && contact.MissingContactFlag.Equals("Y", StringComparison.OrdinalIgnoreCase))
                        {
                            var type = emerContactTypes.FirstOrDefault(typ => typ.Code == "MISS");
                            if (type != null && !string.IsNullOrEmpty(type.Guid))
                            {
                                perEmerContactDetail.Types.Add(new GuidObject2(type.Guid));
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("Unable to locate GUid for emergency-contact-types of MISS", "Global.Internal.Error", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);
                            }
                        }
                    }
                    //get contact phones
                    if (!string.IsNullOrEmpty(contact.EveningPhone) || !string.IsNullOrEmpty(contact.DaytimePhone) || !string.IsNullOrEmpty(contact.OtherPhone))
                    {
                        var emerPhoneTypes = await GetIntgPersonEmerPhoneTypesAsync(false);
                        if (emerPhoneTypes == null)
                        {
                            IntegrationApiExceptionAddError("Unable to locate emergency-contact-phone-availabilities", "Global.Internal.Error", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);
                        }
                        else
                        {
                            perEmerContactDetail.Phones = new List<PersonEmergencyContactsPhones>();
                            if (!string.IsNullOrEmpty(contact.EveningPhone))
                            {
                                var type = emerPhoneTypes.FirstOrDefault(typ => typ.Code == "EVE");
                                if (type != null && !string.IsNullOrEmpty(type.Guid))
                                {
                                    var phone = new PersonEmergencyContactsPhones();
                                    phone.ContactAvailability = new GuidObject2(type.Guid);
                                    phone.Number = contact.EveningPhone;
                                    perEmerContactDetail.Phones.Add(phone);
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError("Unable to locate GUid for emergency-contact-phone-availabilities of EVE", "Global.Internal.Error", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);
                                }

                            }
                            if (!string.IsNullOrEmpty(contact.DaytimePhone))
                            {
                                var type = emerPhoneTypes.FirstOrDefault(typ => typ.Code == "DAY");
                                if (type != null && !string.IsNullOrEmpty(type.Guid))
                                {
                                    var phone = new PersonEmergencyContactsPhones();
                                    phone.ContactAvailability = new GuidObject2(type.Guid);
                                    phone.Number = contact.DaytimePhone;
                                    perEmerContactDetail.Phones.Add(phone);
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError("Unable to locate GUid for emergency-contact-phone-availabilities of DAY", "Global.Internal.Error", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);
                                }

                            }
                            if (!string.IsNullOrEmpty(contact.OtherPhone))
                            {
                                var type = emerPhoneTypes.FirstOrDefault(typ => typ.Code == "OTH");
                                if (type != null && !string.IsNullOrEmpty(type.Guid))
                                {
                                    var phone = new PersonEmergencyContactsPhones();
                                    phone.ContactAvailability = new GuidObject2(type.Guid);
                                    phone.Number = contact.OtherPhone;
                                    perEmerContactDetail.Phones.Add(phone);
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError("Unable to locate GUid for emergency-contact-phone-availabilities of OTH", "Global.Internal.Error", personContactEntity.PersonContactGuid, personContactEntity.PersonContactRecordKey);
                                }

                            }
                        }

                    }

                    //get relationship
                    if (!string.IsNullOrEmpty(contact.Relationship))
                    {
                        var rela = new PersonEmergencyContactsRelationship();
                        rela.Type = contact.Relationship;
                        perEmerContactDetail.Relationship = rela;
                    }

                    //set the details.
                    personEmerContactDto.Contact = perEmerContactDetail;
                    personEmerContactDtos.Add(personEmerContactDto);

                }

            }
            return personEmerContactDtos;
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
                string message = CurrentUser.PersonId + " attempted but cannot update emergency information for person " + emergencyInformation.PersonId;
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
        /// <para>Checks permissions to see if any emergency information is visible for the user</para>
        /// <para>
        /// Valid permissions are <see cref="BasePermissionCodes.ViewPersonEmergencyContacts"/>,
        /// <see cref="BasePermissionCodes.ViewPersonHealthConditions"/>, and
        /// <see cref="BasePermissionCodes.ViewPersonOtherEmergencyInformation"/>
        /// </para>
        /// </summary>
        /// <returns>True if the user can see any emergency information, otherwise false.</returns>
        private bool CanSeeAnyEmergencyInformation()
        {
            return HasPermission(BasePermissionCodes.ViewPersonEmergencyContacts) ||
                   HasPermission(BasePermissionCodes.ViewPersonOtherEmergencyInformation) ||
                   HasPermission(BasePermissionCodes.ViewPersonHealthConditions);
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

        /// <summary>
        /// Verifies if the user has the correct permissions to view a person's emergency contacts. 
        /// </summary>
        private void CheckUserPersonEmergencyContactsViewPermissions()
        {
            // access is ok if the current user has the view person contacts permission
            if (!HasPermission(BasePermissionCodes.ViewAnyPersonContact) && !HasPermission(BasePermissionCodes.UpdatePersonContact))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view person-emergency-contacts.");
                throw new PermissionsException("User is not authorized to view person-emergency-contacts.");
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to create a person's emergency contacts. 
        /// </summary>
        private void CheckUserPersonEmergencyContactsUpdatePermissions()
        {
            // access is ok if the current user has the view person contacts permission
            if (!HasPermission(BasePermissionCodes.UpdatePersonContact))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create/update person-emergency-contacts.");
                throw new PermissionsException("User is not authorized to create or update person-emergency-contacts.");
            }
        }


        /// <summary>
        /// Verifies if the user has the correct permissions to delete a person's emergency contacts. 
        /// </summary>
        private void CheckUserPersonEmergencyContactsDeletePermissions()
        {
            // access is ok if the current user has the view person contacts permission
            if (!HasPermission(BasePermissionCodes.DeletePersonContact))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to delete person-emergency-contacts.");
                throw new PermissionsException("User is not authorized to delete person-emergency-contacts.");
            }
        }

        /// <summary>
        /// Updates Person Emergency Contact
        /// </summary>
        /// <param name="personEmergencyContacts">personEmergencyContacts DTO</param>
        /// <returns>Dtos.PersonEmergencyContacts</returns>

        public async Task<PersonEmergencyContacts> UpdatePersonEmergencyContactsAsync(PersonEmergencyContacts personEmergencyContacts)
        {
            try
            {
                // if the payload is empty
                if (personEmergencyContacts == null)
                {
                    IntegrationApiExceptionAddError("Must provide a person emergency contact representation for update.", "Missing.Request.Body");
                    throw IntegrationApiException;
                }
                //if the guid is missing. 
                if (string.IsNullOrEmpty(personEmergencyContacts.Id))
                {
                    IntegrationApiExceptionAddError("Must provide a person emergency contact id for update.", "Missing.Request.ID");
                    throw IntegrationApiException;
                }
                //if the guid not a null guid. 
                if (personEmergencyContacts.Id == Guid.Empty.ToString())
                {
                    IntegrationApiExceptionAddError("Must provide a non nil person emergency contact id for update.", "Missing.Request.ID");
                    throw IntegrationApiException;
                }
                CheckUserPersonEmergencyContactsUpdatePermissions();
                _emergencyInformationRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // get the ID associated with the incoming guid
                string personEmergencyContactsEntityId = string.Empty;
                try
                {
                    personEmergencyContactsEntityId = await _emergencyInformationRepository.GetPersonEmergencyContactIdFromGuidAsync(personEmergencyContacts.Id);
                }
                catch (Exception)
                { // if the guid is not found then attempt to create
                }

                // map the DTO to entities
                Domain.Base.Entities.PersonContact personEmergencyContactsEntity
                = await ConvertPersonEmergencyContactsDtoToEntityAsync(personEmergencyContactsEntityId, personEmergencyContacts);

                // update the entity in the database
                var updatedPersonEmergencyContacts =
                    await _emergencyInformationRepository.UpdatePersonEmergencyContactsAsync(personEmergencyContactsEntity);
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync
                   (new List<string>() { updatedPersonEmergencyContacts.SubjectPersonId });
                var dtos = await ConvertPersonContactEntitiesToDto2Async(updatedPersonEmergencyContacts, personGuidCollection);
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return dtos.FirstOrDefault();

            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            
           
        }


        /// <summary>
        /// Creates Person Emergency Contact
        /// </summary>
        /// <param name="personEmergencyContacts">personEmergencyContacts DTO</param>
        /// <returns>Dtos.PersonEmergencyContacts</returns>


        public async Task<PersonEmergencyContacts> CreatePersonEmergencyContactsAsync(PersonEmergencyContacts personEmergencyContacts)
        {
          
            try
            {
                // if the payload is empty
                if (personEmergencyContacts == null)
                {
                    IntegrationApiExceptionAddError("Must provide a person emergency contact representation for create.", "Missing.Request.Body");
                    throw IntegrationApiException;
                }
                //if the guid is missing. 
                if (string.IsNullOrEmpty(personEmergencyContacts.Id))
                {
                    IntegrationApiExceptionAddError("Must provide a person emergency contact id for create.", "Missing.Request.ID");
                    throw IntegrationApiException;
                }
                //if the guid not a null guid. 
                if (personEmergencyContacts.Id != Guid.Empty.ToString())
                {
                    IntegrationApiExceptionAddError("Must provide a nil person emergency contact id for create.", "Missing.Request.ID");
                    throw IntegrationApiException;
                }

                CheckUserPersonEmergencyContactsUpdatePermissions();
                _emergencyInformationRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                // map the DTO to entities
                Domain.Base.Entities.PersonContact personEmergencyContactsEntity
                = await ConvertPersonEmergencyContactsDtoToEntityAsync(string.Empty, personEmergencyContacts);

                // update the entity in the database
                var updatedPersonEmergencyContacts =
                    await _emergencyInformationRepository.UpdatePersonEmergencyContactsAsync(personEmergencyContactsEntity);
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync
                   (new List<string>() { updatedPersonEmergencyContacts.SubjectPersonId });
                var dtos = await ConvertPersonContactEntitiesToDto2Async(updatedPersonEmergencyContacts, personGuidCollection);
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return dtos.FirstOrDefault();
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)            {
                
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

        }


        /// <summary>
        /// Deletes Person Emergency Contact
        /// </summary>
        /// <param name="guid">personEmergencyContacts Guid to be deleted</param>
        /// <returns></returns>

        public async Task DeletePersonEmergencyContactsAsync(string guid)
        {
            CheckUserPersonEmergencyContactsDeletePermissions();
            // get the ID associated with the incoming guid

            if (string.IsNullOrEmpty(guid))
            {
                IntegrationApiExceptionAddError("GUID is required to delete a person emergency contact.", "guid");
                throw IntegrationApiException;
            }
            try
            {
                var personEmergencyContactsEntity= await _emergencyInformationRepository.GetPersonContactById2Async(guid);
                if (personEmergencyContactsEntity == null)
                {
                    throw new KeyNotFoundException(string.Concat("No emergency contact was found for guid ", guid));
                }
                else
                {
                    if (personEmergencyContactsEntity.PersonContactDetails == null || !personEmergencyContactsEntity.PersonContactDetails.Any())
                    {
                        throw new KeyNotFoundException(string.Concat("No emergency contact was found for guid ", guid));
                    }
                }
                await _emergencyInformationRepository.DeletePersonEmergencyContactsAsync(personEmergencyContactsEntity);
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// Validate PersonEmergencyContacts DTO
        /// </summary>
        /// <param name="source"></param>
        private async Task<Domain.Base.Entities.PersonContact> ConvertPersonEmergencyContactsDtoToEntityAsync(string key, PersonEmergencyContacts source)
        {
            var personEmerId = string.Empty;
            var emerName = string.Empty;
            Domain.Base.Entities.PersonContact personEmerEntity = null;
            if (!string.IsNullOrEmpty(key))
            {
                personEmerId = key.Split('|')[0];
                emerName = key.Split('|')[1];

            }
            if (source == null)
            {
                IntegrationApiExceptionAddError("PersonEmergencyContacts body is required.", "Validation.Exception");
                throw IntegrationApiException;
            }

            if (string.IsNullOrWhiteSpace(source.Id))
            {
                IntegrationApiExceptionAddError("id is a required property.", "Missing.Required.Property");
            }
            // process person Id
            var personId = string.Empty;
            if ((source.Person == null) || (string.IsNullOrWhiteSpace(source.Person.Id)))
            {
                IntegrationApiExceptionAddError("Person.id is a required property.", "Missing.Required.Property", source.Id, personEmerId);
            }
            else
            {
                try
                {
                    personId = await _personRepository.GetPersonIdFromGuidAsync(source.Person.Id);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError("Person.id is not a valid GUID for persons.", "GUID.Not.Found", source.Id, personEmerId);
                }
                if (String.IsNullOrEmpty(personId))
                {
                    IntegrationApiExceptionAddError("Person.id is not a valid GUID for persons.", "GUID.Not.Found", source.Id, personEmerId);
                }
                else
                {
                    if (!string.IsNullOrEmpty(personEmerId) && personId != personEmerId)
                    {
                        IntegrationApiExceptionAddError("Person.id cannot be updated.", "Validation.Exception", source.Id, personEmerId);
                    }
                }
            }

            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            // create the entity
            try
            {

                if (!string.IsNullOrEmpty(personEmerId))
                {
                    personEmerEntity = new Domain.Base.Entities.PersonContact(source.Id, personEmerId, personId);
                }
                else
                {
                    personEmerEntity = new Domain.Base.Entities.PersonContact(source.Id, "NEW", personId);
                }

            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Missing.Required.Property", source.Id, personEmerId);
                throw IntegrationApiException;
            }
            
            if (source.Contact == null)
            {
                IntegrationApiExceptionAddError("Contact is a required property.", "Missing.Required.Property", source.Id, personEmerId);
            }
            else
            {
                var contactDetails = new Domain.Base.Entities.PersonContactDetails();
                // get the name
                if (source.Contact.Name == null)
                {
                    IntegrationApiExceptionAddError("Contact.Name is a required property.", "Missing.Required.Property", source.Id, personEmerId);
                }
                else
                {
                    //set the guid 
                    contactDetails.Guid = source.Id;
                    // if there is no full name then 
                    if (string.IsNullOrEmpty(source.Contact.Name.FullName))
                    {

                        var name = string.Empty;
                        var field_length = 57;
                        if (!string.IsNullOrEmpty(source.Contact.Name.FirstName))
                        {
                            name = source.Contact.Name.FirstName;
                        }
                        if (!string.IsNullOrEmpty(source.Contact.Name.MiddleName))
                        {
                            if (!string.IsNullOrEmpty(name))
                            {
                                //don't add middle name if the concatenation of first middle and last name is going to be longer than field length
                                if ((string.Concat(name, " ", source.Contact.Name.LastName," ", source.Contact.Name.MiddleName)).Length <= field_length)
                                {
                                    name = string.Concat(name, " ", source.Contact.Name.MiddleName);
                                }
                            }
                            else
                            {
                                name = source.Contact.Name.MiddleName;
                            }

                        }
                        if (!string.IsNullOrEmpty(source.Contact.Name.LastName))
                        {
                            if (!string.IsNullOrEmpty(name))
                            {
                                name = string.Concat(name, " ", source.Contact.Name.LastName);
                            }
                            else
                            {
                                name = source.Contact.Name.LastName;
                            }

                        }
                        if (string.IsNullOrEmpty(name))
                        {
                            IntegrationApiExceptionAddError("Contact.Name is a required property. At least one of the component of the name is required.", "Missing.Required.Property", source.Id, personEmerId);
                        }
                        else
                        {
                            contactDetails.ContactName = name;
                        }

                       

                    }
                    else
                    {
                   
                            contactDetails.ContactName = source.Contact.Name.FullName;
                        

                    }

                    if (!string.IsNullOrEmpty(emerName) &&  !string.IsNullOrEmpty(contactDetails.ContactName ) && emerName != contactDetails.ContactName)
                    {
                        IntegrationApiExceptionAddError("Name cannot be updated. To submit a new name this person-emergency-contacts record can be deleted and a new one created.", "Validation.Exception", source.Id, personEmerId);
                    }
                }
                // get the contact type
                if (source.Contact.Types == null || !source.Contact.Types.Any())
                {
                    IntegrationApiExceptionAddError("Contact.Types is a required property.", "Missing.Required.Property", source.Id, personEmerId);
                }
                else
                {
                    contactDetails.ContactFlag = "N";
                    contactDetails.MissingContactFlag = "N";
                    foreach (var type in source.Contact.Types)
                    {
                        if (!string.IsNullOrEmpty(type.Id))
                        {
                            var contactTypes = await GetIntgPersonEmerTypesAsync(false);
                            if (contactTypes == null || !contactTypes.Any())
                            {
                                IntegrationApiExceptionAddError("Emergency contact types not found.", "Validation.Exception", source.Id, personEmerId);
                            }
                            else
                            {
                                var contactType = contactTypes.FirstOrDefault(x => x.Guid == type.Id);
                                if (contactType == null)
                                {
                                    IntegrationApiExceptionAddError(string.Format("Emergency contact types not found for guid {0}.", type.Id), "Validation.Exception", source.Id, personEmerId);
                                }
                                else
                                {
                                    if (contactType.Code == "EMER")
                                    {
                                        contactDetails.ContactFlag = "Y";
                                    }
                                    if (contactType.Code == "MISS")
                                    {
                                        contactDetails.MissingContactFlag = "Y";
                                    }                                
                                }
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError("Id is required with contact.Types is in the payload.", "Missing.Required.Property", source.Id, personEmerId);
                        }
                    }
                }

                //get contact phones
                if (source.Contact.Phones != null && source.Contact.Phones.Any())
                {
                    int dayPhoneCtr = 0;
                    int evePhoneCtr = 0;
                    int othPhoneCtr = 0;
                    foreach (var phone in source.Contact.Phones)
                    {
                        if (string.IsNullOrEmpty(phone.Number))
                        {
                            IntegrationApiExceptionAddError("Phones.number is a required property.", "Missing.Required.Property", source.Id, personEmerId);
                        }
                        if (phone.ContactAvailability != null && !string.IsNullOrEmpty(phone.ContactAvailability.Id))
                        {
                            var phoneTypes = await GetIntgPersonEmerPhoneTypesAsync(false);
                            if (phoneTypes == null || !phoneTypes.Any())
                            {
                                IntegrationApiExceptionAddError("Emergency phone types not found.", "Validation.Exception", source.Id, personEmerId);
                            }
                            else
                            {
                                var phoneType = phoneTypes.FirstOrDefault(x => x.Guid == phone.ContactAvailability.Id);
                                if (phoneType == null)
                                {
                                    IntegrationApiExceptionAddError(string.Format("Emergency phone types not found for guid {0}.", phone.ContactAvailability.Id), "Validation.Exception", source.Id, personEmerId);
                                }
                                else
                                {
                                    if (phoneType.Code == "DAY")
                                    {
                                        contactDetails.DaytimePhone = ConvertPhoneNumber(phone.CountryCallingCode, phone.Number, phone.Extension);
                                        dayPhoneCtr++;
                                    }
                                    if (phoneType.Code == "EVE")
                                    {
                                        contactDetails.EveningPhone = ConvertPhoneNumber(phone.CountryCallingCode, phone.Number, phone.Extension);
                                        evePhoneCtr++;
                                    }
                                    if (phoneType.Code == "OTH")
                                    {
                                        contactDetails.OtherPhone = ConvertPhoneNumber(phone.CountryCallingCode, phone.Number, phone.Extension);
                                        othPhoneCtr++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            contactDetails.OtherPhone = ConvertPhoneNumber(phone.CountryCallingCode, phone.Number, phone.Extension);
                            othPhoneCtr++;
                        }
                    }
                    if (dayPhoneCtr > 1 || evePhoneCtr > 1 || othPhoneCtr > 1 )
                    {
                        IntegrationApiExceptionAddError("Duplicate contact availabilities are not permitted.", "Validation.Exception", source.Id, personEmerId);
                    }
                
                }

                //get relationship
                if (source.Contact.Relationship != null)
                {
                    if (source.Contact.Relationship.Detail != null && !string.IsNullOrEmpty(source.Contact.Relationship.Detail.Id))
                    {
                        var relTypes = await GetRelationTypesAsync(false);
                        if (relTypes == null || !relTypes.Any())
                        {
                            IntegrationApiExceptionAddError("Relationship types not found.", "Validation.Exception", source.Id, personEmerId);
                        }
                        else
                        {
                            var type = relTypes.FirstOrDefault(x => x.Guid == source.Contact.Relationship.Detail.Id);
                            if (type == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Relationship types not found for guid {0}.", source.Contact.Relationship.Detail.Id), "Validation.Exception", source.Id, personEmerId);
                            }
                            else
                            {
                                contactDetails.Relationship = type.Description;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(source.Contact.Relationship.Type))
                        {
                            contactDetails.Relationship = source.Contact.Relationship.Type;
                        }
                    }
                }
                if (contactDetails != null)
                {
                    personEmerEntity.PersonContactDetails = new List<Domain.Base.Entities.PersonContactDetails>() { contactDetails };
                }
            }
            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return personEmerEntity;
        }



        private string ConvertPhoneNumber(string countryCallingCode, string number, string extension)
        {
            var phone = string.Empty;
            if (!string.IsNullOrEmpty(countryCallingCode))
            {
                phone = string.Concat("+", countryCallingCode);
            }
            if (!string.IsNullOrEmpty(number))
            {
                if (!string.IsNullOrEmpty(phone))
                {
                    phone = string.Concat(phone, " ", number);
                }
                else
                {
                    phone = number;
                }
            }
            if (!string.IsNullOrEmpty(extension))
            {
                if (!string.IsNullOrEmpty(phone))
                {
                    phone = string.Concat(phone, " x ", extension);
                }
                else
                {
                    phone = extension;
                }
            }
            return phone;
        }
    }
}