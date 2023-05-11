// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class PersonMatchingRequestsService : BaseCoordinationService, IPersonMatchingRequestsService
    {

        private readonly IPersonMatchingRequestsRepository _personMatchingRequestsRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;

        #region Constructor

        public PersonMatchingRequestsService(
            IPersonMatchingRequestsRepository personMatchingRequestsRepository,
            IPersonRepository personRepository,
            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _personMatchingRequestsRepository = personMatchingRequestsRepository;
            _personRepository = personRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        #endregion

        #region Get all Reference Data

        public IEnumerable<Domain.Base.Entities.AddressType2> _addressTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.AddressType2>> GetAddressTypes2Async(bool bypassCache)
        {
            if (_addressTypes == null)
            {
                _addressTypes = await _referenceDataRepository.GetAddressTypes2Async(bypassCache);
            }
            return _addressTypes;
        }

        public IEnumerable<Domain.Base.Entities.Country> _countries { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.Country>> GetCountryCodesAsync(bool bypassCache)
        {
            if (_countries == null)
            {
                _countries = await _referenceDataRepository.GetCountryCodesAsync(bypassCache);
            }
            return _countries;
        }

        public IEnumerable<Domain.Base.Entities.County> _counties { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.County>> GetCountiesAsync(bool bypassCache)
        {
            if (_counties == null)
            {
                _counties = await _referenceDataRepository.GetCountiesAsync(bypassCache);
            }
            return _counties;
        }

        public IEnumerable<Domain.Base.Entities.State> _states { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.State>> GetStateCodesAsync(bool bypassCache)
        {
            if (_states == null)
            {
                _states = await _referenceDataRepository.GetStateCodesAsync(bypassCache);
            }
            return _states;
        }

        public IEnumerable<Domain.Base.Entities.EmailType> _emailTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.EmailType>> GetEmailTypesAsync(bool bypassCache)
        {
            if (_emailTypes == null)
            {
                _emailTypes = await _referenceDataRepository.GetEmailTypesAsync(bypassCache);
            }
            return _emailTypes;
        }

        public IEnumerable<Domain.Base.Entities.AltIdTypes> _alternateIdTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.AltIdTypes>> GetAlternateIdTypesAsync(bool bypassCache = false)
        {
            if (_alternateIdTypes == null)
            {
                _alternateIdTypes = await _referenceDataRepository.GetAlternateIdTypesAsync(bypassCache);
            }
            return _alternateIdTypes;
        }

        public IEnumerable<Domain.Base.Entities.PhoneType> _phoneTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.PhoneType>> GetPhoneTypesAsync(bool bypassCache)
        {
            if (_phoneTypes == null)
            {
                _phoneTypes = await _referenceDataRepository.GetPhoneTypesAsync(bypassCache);
            }
            return _phoneTypes;
        }

        #endregion

        #region GET Methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all person-matching-requests
        /// </summary>
        /// <returns>Collection of PersonMatchingRequests DTO objects</returns>
        public async Task<Tuple<IEnumerable<PersonMatchingRequests>, int>> GetPersonMatchingRequestsAsync(int offset, int limit, PersonMatchingRequests criteria,
             string personFilter, bool bypassCache)
        {
            var personMatchingRequestsCollection = new List<PersonMatchingRequests>();
            string newProspectId = string.Empty;
            string newEntryAcadPeriod = string.Empty;
            int totalCount = 0;
            PersonMatchRequest personMatchRequest = new PersonMatchRequest();
            List<PersonMatchingRequests> dtos = new List<PersonMatchingRequests>();

            try
            {             
                //convert criteria values.
                if (criteria != null)
                {
                    if (!string.IsNullOrEmpty(criteria.Originator))
                    {
                        personMatchRequest.Originator = criteria.Originator;
                    }
                    if (criteria.Outcomes != null && criteria.Outcomes.Any())
                    {
                        PersonMatchRequestType type = PersonMatchRequestType.NotSet;
                        PersonMatchRequestStatus status = PersonMatchRequestStatus.NotSet;
                        DateTimeOffset date = new DateTime();
                        foreach (var outcome in criteria.Outcomes)
                        {
                            if (outcome != null)
                            {
                                try
                                {
                                    if (outcome.Type != Dtos.EnumProperties.PersonMatchingRequestsType.NotSet)
                                    {
                                        type = (PersonMatchRequestType)Enum.Parse(typeof(PersonMatchRequestType), outcome.Type.ToString());
                                    }
                                    if (outcome.Status != Dtos.EnumProperties.PersonMatchingRequestsStatus.NotSet)
                                    {
                                        status = (PersonMatchRequestStatus)Enum.Parse(typeof(PersonMatchRequestStatus), outcome.Status.ToString());
                                    }
                                    personMatchRequest.AddPersonMatchRequestOutcomes(new PersonMatchRequestOutcomes(type, status, date));
                                }
                                catch
                                {
                                    return new Tuple<IEnumerable<PersonMatchingRequests>, int>(new List<PersonMatchingRequests>(), 0);
                                }
                            }
                        }
                    }
                }

                //convert person filter named query.
                string[] filterPersonIds = new List<string>().ToArray();

                if (!string.IsNullOrEmpty(personFilter))
                {
                    try
                    {
                        var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilter));
                        if (personFilterKeys != null && personFilterKeys.Any())
                        {
                            filterPersonIds = personFilterKeys;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<PersonMatchingRequests>, int>(new List<PersonMatchingRequests>(), 0);
                        }
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<PersonMatchingRequests>, int>(new List<PersonMatchingRequests>(), 0);
                    }
                }
                Tuple<IEnumerable<PersonMatchRequest>, int> entities = null;
                try
                {
                    entities = await _personMatchingRequestsRepository.GetPersonMatchRequestsAsync(offset, limit, personMatchRequest, filterPersonIds);
                    if (entities == null || !entities.Item1.Any())
                    {
                        return new Tuple<IEnumerable<PersonMatchingRequests>, int>(new List<PersonMatchingRequests>(), 0);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }

                totalCount = entities.Item2;
                //Build person guids dictionary
                var personIds = entities.Item1.Select(a => a.PersonId).Distinct().ToList();
                Dictionary<string, string> personDict = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                foreach (var entity in entities.Item1)
                {
                    dtos.Add(ConvertPersonMatchRequestEntityToDtoAsync(entity, personDict));
                }

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }

            return dtos.Any() ? new Tuple<IEnumerable<PersonMatchingRequests>, int>(dtos, totalCount) :
                new Tuple<IEnumerable<PersonMatchingRequests>, int>(new List<PersonMatchingRequests>(), 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PersonMatchingRequests from its GUID
        /// </summary>
        /// <returns>PersonMatchingRequests DTO object</returns>
        public async Task<PersonMatchingRequests> GetPersonMatchingRequestsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid");
                }

            
                var entity = await _personMatchingRequestsRepository.GetPersonMatchRequestsByIdAsync(guid);

                Dictionary<string, string> personDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { entity.PersonId });

                var dto = ConvertPersonMatchRequestEntityToDtoAsync(entity, personDict);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return dto;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No person-matching-requests was found for guid '{0}'.", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No person-matching-requests was found for guid '{0}'.", guid), ex);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion

        #region POST initiations prospects

        /// <summary>
        /// Create a PersonMatchingRequestsInitiationsProspects.
        /// </summary>
        /// <param name="personMatchingRequestsInitiationsProspects">The <see cref="PersonMatchingRequestsInitiationsProspects">personMatchingRequestsInitiationsProspects</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PersonMatchingRequests">PersonMatchingRequests</see></returns>
        public async Task<PersonMatchingRequests> CreatePersonMatchingRequestsInitiationsProspectsAsync(PersonMatchingRequestsInitiationsProspects personMatchingRequestsInitiationsProspects)
        {
            //Validate the request 
            ValidatePersonMatchingRequestsInitiationsProspects(personMatchingRequestsInitiationsProspects);
     
            _personMatchingRequestsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {

                var initiationsProspectsEntity
                         = await ConvertPersonMatchingRequestsInitiationsProspectsDtoToEntityAsync(personMatchingRequestsInitiationsProspects.Id, personMatchingRequestsInitiationsProspects);

                // create a PersonMatchingRequests entity in the database
                var createdPersonMatchRequest =
                    await _personMatchingRequestsRepository.CreatePersonMatchingRequestsInitiationsProspectsAsync(initiationsProspectsEntity);
                // return the newly created PersonMatchingRequests
                Dictionary<string, string> personDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { createdPersonMatchRequest.PersonId });

                var dto = ConvertPersonMatchRequestEntityToDtoAsync(createdPersonMatchRequest, personDict);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return dto ?? null;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message, ex.InnerException);
            }
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="personDict"></param>
        /// <param name="stAcadPrgDict"></param>
        /// <returns></returns>
        private PersonMatchingRequests ConvertPersonMatchRequestEntityToDtoAsync(PersonMatchRequest source, Dictionary<string, string> personDict)
        {
            PersonMatchingRequests dto = new PersonMatchingRequests();

            //id
            if (string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError("Could not find a GUID for person-matching-requests entity.", "Bad.Data", id: source.RecordKey);
            }
            dto.Id = source.Guid;
            if (!string.IsNullOrEmpty(source.Originator))
            {
                dto.Originator = source.Originator;
            }

            //prospect.id
            string personGuid;
            if (!string.IsNullOrEmpty(source.PersonId))
            {
                if (!personDict.TryGetValue(source.PersonId, out personGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for person ID '{0}'", source.RecordKey), "Bad.Data", source.Guid, source.RecordKey);
                }
                dto.Person = new GuidObject2(personGuid);
            }

            // outcomes
            if (source.Outcomes != null && source.Outcomes.Any())
            {
                foreach (var outcome in source.Outcomes)
                {
                    if (outcome != null)
                    {
                        try
                        {
                            if (dto.Outcomes == null)
                            {
                                dto.Outcomes = new List<PersonMatchingRequestsOutcomesDtoProperty>();
                            }
                            dto.Outcomes.Add(new PersonMatchingRequestsOutcomesDtoProperty()
                            {
                                Type = (Dtos.EnumProperties.PersonMatchingRequestsType)Enum.Parse(typeof(Dtos.EnumProperties.PersonMatchingRequestsType), outcome.Type.ToString()),
                                Status = (Dtos.EnumProperties.PersonMatchingRequestsStatus)Enum.Parse(typeof(Dtos.EnumProperties.PersonMatchingRequestsStatus), outcome.Status.ToString()),
                                Date = outcome.Date.DateTime
                            });
                        }
                        catch
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to translate outcome type of '{0}' or status of '{1}'.", outcome.Type.ToString(), outcome.Status.ToString()), "Bad.Data", source.Guid, source.RecordKey);
                        }
                    }
                }
            }

            return dto;
        }

        /// <summary>
        /// Converts submissions dto to entity.
        /// </summary>
        /// <returns></returns>
        private async Task<PersonMatchRequestInitiation> ConvertPersonMatchingRequestsInitiationsProspectsDtoToEntityAsync(string guid, PersonMatchingRequestsInitiationsProspects dto)
        {
            //id
            if (string.IsNullOrEmpty(dto.Id))
            {
                dto.Id = Guid.Empty.ToString();
            }
            PersonMatchRequestInitiation entity = new PersonMatchRequestInitiation();
            entity.Guid = dto.Id;
            entity.RequestType = "PROSPECTS";

            // Gender
            if (dto.Gender == Dtos.EnumProperties.GenderType2.Male)
            {
                entity.Gender = "M";
            }
            else
            {
                if (dto.Gender == Dtos.EnumProperties.GenderType2.Female)
                {
                    entity.Gender = "F";
                }
            }

            // Person Names
            if (dto.Names != null)
            {
                if (dto.Names.Legal != null)
                {
                    if (string.IsNullOrEmpty(dto.Names.Legal.LastName) || string.IsNullOrEmpty(dto.Names.Legal.FirstName))
                    {
                        IntegrationApiExceptionAddError("The properties names.legal.firstName and names.legal.lastName are required.", "Missing.Required.Property", dto.Id);
                    }
                    entity.LastName = dto.Names.Legal.LastName;
                    entity.FirstName = dto.Names.Legal.FirstName;
                    entity.MiddleName = dto.Names.Legal.MiddleName;
                }
                if (dto.Names.Former != null)
                {
                    entity.BirthLastName = dto.Names.Former.LastName;
                    entity.BirthFirstName = dto.Names.Former.FirstName;
                    entity.BirthMiddleName = dto.Names.Former.MiddleName;
                }
                if (dto.Names.Favored != null)
                {
                    entity.ChosenLastName = dto.Names.Favored.LastName;
                    entity.ChosenFirstName = dto.Names.Favored.FirstName;
                    entity.ChosenMiddleName = dto.Names.Favored.MiddleName;
                }
            }
            else
            {
                IntegrationApiExceptionAddError("The properties names.legal.firstName and names.legal.lastName are required.", "Missing.Required.Property", dto.Id);
            }
            
            // Matching Criteria
            if (dto.MatchingCriteria != null)
            {
                var matchCriteria = dto.MatchingCriteria;
                var validMatchCriteria = false;

                // Date of Birth
                if (matchCriteria.DateOfBirth != null)
                {
                    validMatchCriteria = true;
                    entity.BirthDate = matchCriteria.DateOfBirth;
                }

                // Credentials
                if (matchCriteria.Credential != null)
                {
                    if (!string.IsNullOrEmpty(matchCriteria.Credential.Value))
                    {
                        validMatchCriteria = true;
                        entity.TaxId = matchCriteria.Credential.Value;
                    }
                }

                // Alternative Credentials
                if (matchCriteria.AlternativeCredentials != null)
                {
                    foreach (var credential in matchCriteria.AlternativeCredentials)
                    {
                        if (!string.IsNullOrEmpty(credential.Value))
                        {
                            if (credential.Type != null && !string.IsNullOrEmpty(credential.Type.Id))
                            {
                                var credentialTypeEntity = (await GetAlternateIdTypesAsync(false)).FirstOrDefault(cte => cte.Guid.Equals(credential.Type.Id, StringComparison.InvariantCultureIgnoreCase));
                                if (credentialTypeEntity != null && !string.IsNullOrEmpty(credentialTypeEntity.Code))
                                {
                                    validMatchCriteria = true;
                                    entity.AddAlternateId(credential.Value, credentialTypeEntity.Code);
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError("Could not determine Alternate Credential Type and Value.", "Validation.Exception", dto.Id);
                                }
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("alternateCredential.type is required when alternateCredential.value is defined.", "Validation.Exception", dto.Id);
                            }
                        }
                    }
                }

                // Address Data
                if (matchCriteria.Address != null)
                {
                    validMatchCriteria = true;

                    if (matchCriteria.Address.Type != null && !string.IsNullOrEmpty(matchCriteria.Address.Type.Id))
                    {
                        var addressTypeEntity = (await GetAddressTypes2Async(false)).FirstOrDefault(at => at.Guid.Equals(matchCriteria.Address.Type.Id, StringComparison.InvariantCultureIgnoreCase));
                        if (addressTypeEntity != null)
                        {
                            entity.AddressType = addressTypeEntity.Code;
                        }
                        else
                        {
                            IntegrationApiExceptionAddError("Could not determine Address Type for Address.", "Validation.Exception", dto.Id);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError("The property address.type.id is required when an address is specified.", "Missing.Required.Property", dto.Id);
                    }

                    if (matchCriteria.Address.AddressLines == null || !matchCriteria.Address.AddressLines.Any())
                    {
                        IntegrationApiExceptionAddError("The property address.addressLines is required when an address is specified.", "Missing.Required.Property", dto.Id);
                    }
                    entity.AddressLines = matchCriteria.Address.AddressLines;

                    if (matchCriteria.Address.Place != null)
                    {
                        var addressDtoAddress = matchCriteria.Address;
                        var addressCountry = new Dtos.AddressCountry();
                        if (addressDtoAddress.Place != null && addressDtoAddress.Place.Country != null && !string.IsNullOrEmpty(addressDtoAddress.Place.Country.Code.ToString()))
                        {
                            addressCountry = addressDtoAddress.Place.Country;
                        }
                        else
                        {
                            IntegrationApiExceptionAddError("A country code is required for an address with a place defined.", "Validation.Exception", dto.Id);
                        }
                        //if there is more than one country that matches the ISO code, we need to pick the country that does not have "Not in use" set to Yes
                        Domain.Base.Entities.Country country = null;
                        var countryEntities = (await GetCountryCodesAsync(false)).Where(cc => cc.IsoAlpha3Code == addressCountry.Code.ToString());
                        if (countryEntities != null && countryEntities.Any())
                        {
                            if (countryEntities.Count() > 1)
                            {
                                country = countryEntities.FirstOrDefault(con => con.IsNotInUse == false);
                            }
                            if (country == null)
                            {
                                country = countryEntities.FirstOrDefault();
                            }
                        }
                        if (country == null)
                        {
                            IntegrationApiExceptionAddError(string.Concat("Unable to locate country code '", addressCountry.Code, "'"), "Validation.Exception", dto.Id);
                        }
                        else
                        {
                            entity.Country = country.Code;
                            switch (addressCountry.Code)
                            {
                                case Dtos.EnumProperties.IsoCode.USA:
                                    entity.CorrectionDigit = string.IsNullOrEmpty(addressCountry.CorrectionDigit) ? null : addressCountry.CorrectionDigit;
                                    entity.CarrierRoute = string.IsNullOrEmpty(addressCountry.CarrierRoute) ? null : addressCountry.CarrierRoute;
                                    entity.DeliveryPoint = string.IsNullOrEmpty(addressCountry.DeliveryPoint) ? null : addressCountry.DeliveryPoint;
                                    break;
                                default:
                                    if (!string.IsNullOrEmpty(addressCountry.CorrectionDigit) || !string.IsNullOrEmpty(addressCountry.CarrierRoute) || !string.IsNullOrEmpty(addressCountry.DeliveryPoint))
                                    {
                                        IntegrationApiExceptionAddError("correctionDigit, carrierRoute and deliveryPoint can only be specified when code is 'USA'.", "Validation.Exception", dto.Id);
                                    }
                                    break;
                            }
                        }

                        if (addressCountry.Region != null && !string.IsNullOrEmpty(addressCountry.Region.Code))
                        {
                            if (addressCountry.Code == IsoCode.USA || addressCountry.Code == IsoCode.CAN)
                            {
                                string state = "";
                                if (addressCountry.Region.Code.Contains("-"))
                                    state = addressCountry.Region.Code.Substring(3);
                                var states =
                                    (await GetStateCodesAsync(false)).FirstOrDefault(
                                        x => x.Code == state);

                                if (states != null)
                                {
                                    entity.State = states.Code;
                                }
                                else
                                {
                                    //throw an execption if the state is not valid for US or Canada. 
                                   IntegrationApiExceptionAddError(string.Format("'{0}' is not defined as a state or province. ", state), "Validation.Exception", dto.Id);
                                }
                            }
                            else
                            {
                                entity.Region = addressCountry.Region == null
                                    ? null
                                    : addressCountry.Region.Code;
                            }
                        }

                        if (addressCountry.SubRegion != null && !string.IsNullOrEmpty(addressCountry.SubRegion.Code))
                        {
                            var county = (await GetCountiesAsync(false)).FirstOrDefault(c => c.Code == addressCountry.SubRegion.Code);
                            if (county != null)
                                entity.County = county.Code;
                            else
                                entity.SubRegion = addressCountry.SubRegion == null ? null : addressCountry.SubRegion.Code;
                        }

                        entity.City = string.IsNullOrEmpty(addressCountry.Locality) ? null : addressCountry.Locality;
                        entity.Zip = string.IsNullOrEmpty(addressCountry.PostalCode) ? null : addressCountry.PostalCode;
                        entity.PostalCode = string.IsNullOrEmpty(addressCountry.PostalCode) ? null : addressCountry.PostalCode;
                        entity.Locality = addressCountry.Locality;
                        entity.Region = addressCountry.Region == null ? null : addressCountry.Region.Code;
                        entity.SubRegion = addressCountry.SubRegion == null ? null : addressCountry.SubRegion.Code;
                    }
                }

                // Phone Number
                if (matchCriteria.Phone != null)
                {
                    if (!string.IsNullOrEmpty(matchCriteria.Phone.Number))
                    {
                        validMatchCriteria = true;
                        entity.Phone = matchCriteria.Phone.Number;

                        if (matchCriteria.Phone.Type != null && !string.IsNullOrEmpty(matchCriteria.Phone.Type.Id))
                        {
                            var phoneTypeEntity = (await GetPhoneTypesAsync(false)).FirstOrDefault(cte => cte.Guid.Equals(matchCriteria.Phone.Type.Id, StringComparison.InvariantCultureIgnoreCase));
                            if (phoneTypeEntity != null && !string.IsNullOrEmpty(phoneTypeEntity.Code))
                            {
                                entity.PhoneType = phoneTypeEntity.Code;
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("Could not determine Phone Type for phone number.", "Validation.Exception", dto.Id);
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError("The property phone.type.id is required when a phone number is specified.", "Missing.Required.Property", dto.Id);
                        }
                    }
                }

                // email addresses
                if (matchCriteria.Email != null)
                {
                    validMatchCriteria = true;
                    entity.Email = matchCriteria.Email.Address;
                    if (!string.IsNullOrEmpty(matchCriteria.Email.Address))
                    {
                        if (matchCriteria.Email.Type != null && !string.IsNullOrEmpty(matchCriteria.Email.Type.Id))
                        {
                            var emailTypeEntity = (await GetEmailTypesAsync(false)).FirstOrDefault(et => et.Guid.Equals(matchCriteria.Email.Type.Id, StringComparison.InvariantCultureIgnoreCase));
                            if (emailTypeEntity != null)
                            {
                                entity.EmailType = emailTypeEntity.Code;
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("Could not determine Email Type for Email Address.", "Validation.Exception", dto.Id);
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError("The property email.type.id is required when an email address is specified.", "Missing.Required.Property", dto.Id);
                        }
                    }
                }

                if (!validMatchCriteria)
                {
                    IntegrationApiExceptionAddError("At least 1 of 6 options, dataOfBirth, credential, alternativeCredential, address, phone, or email must be specified.", "Missing.Required.Property", dto.Id);
                }
            }
            else
            {
                IntegrationApiExceptionAddError("At least 1 of 6 options, dataOfBirth, credential, alternativeCredential, address, phone, or email must be specified.", "Missing.Required.Property", dto.Id);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return entity;
        }

        /// <summary>
        /// Validate PersonMatchingRequestsInitiationsProspects DTO
        /// </summary>
        /// <param name="source"></param>
        private void ValidatePersonMatchingRequestsInitiationsProspects(PersonMatchingRequestsInitiationsProspects source)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("PersonMatchingRequestsInitiationsProspects body is required.", "Validation.Exception");
                throw IntegrationApiException;
            }

            if (string.IsNullOrWhiteSpace(source.Id))
            {
                IntegrationApiExceptionAddError("id is required.", "Missing.Required.Property");
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
        }
        #endregion
    }
}