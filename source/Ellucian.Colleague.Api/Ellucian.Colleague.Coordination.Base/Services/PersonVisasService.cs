// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class PersonVisasService : BaseCoordinationService, IPersonVisasService
    {
        #region ..ctor
        private readonly IPersonVisaRepository _personVisasRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private ILogger _logger;


        private IEnumerable<VisaTypeGuidItem> _visaTypeEntities = null;
        /// <summary>
        /// Constructor for PersonVisasService
        /// </summary>
        /// <param name="personVisasRepository">personVisasRepository</param>
        /// <param name="personRepository">personRepository</param>
        /// <param name="logger">logger</param>
        public PersonVisasService(IAdapterRegistry adapterRegistry,
            IPersonVisaRepository personVisasRepository,
            IPersonRepository personRepository,
            IReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _personVisasRepository = personVisasRepository;
            _personRepository = personRepository;
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
            _logger = logger;
        }

        #endregion

        private async Task<IEnumerable<VisaTypeGuidItem>> GetVisaTypesAsync(bool bypassCache)
        {
            if (_visaTypeEntities == null)
            {
                _visaTypeEntities = await _referenceDataRepository.GetVisaTypesAsync(bypassCache);
            }
            return _visaTypeEntities;
        }

        #region GET

        /// <summary>
        /// Gets all person visas based on paging
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonVisa>, int>> GetAllAsync(int offset, int limit, string person, bool bypassCache)
        {
           
            var personVisaEntities = await _personVisasRepository.GetAllPersonVisasAsync(offset, limit, person, bypassCache);
            var personVisaDtos = new List<Dtos.PersonVisa>();

            if (personVisaEntities != null && personVisaEntities.Item2 > 0)
            {
                foreach (var personVisaEntity in personVisaEntities.Item1)
                {
                    Dtos.PersonVisa personVisaDto = await ConvertPersonVisaEntityToDtoAsync(personVisaEntity, bypassCache);
                    personVisaDtos.Add(personVisaDto);
                }
            }
            return personVisaDtos.Any() ? new Tuple<IEnumerable<Dtos.PersonVisa>, int>(personVisaDtos, personVisaEntities.Item2) :
                                          new Tuple<IEnumerable<Dtos.PersonVisa>, int>(personVisaDtos, 0);
        }

        /// <summary>
        /// Gets all person visas based on paging
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonVisa>, int>> GetAll2Async(int offset, int limit, string person, string visaTypeCategory, string visaTypeDetail, bool bypassCache)
        {

            var personVisaDtos = new List<Dtos.PersonVisa>();
            List<string> visaTypeCategories = null;

            #region filters

            if (!string.IsNullOrEmpty(visaTypeCategory))
            {
                if (visaTypeCategory.Equals("nonImmigrant") || visaTypeCategory.Equals("immigrant"))
                {
                    try
                    {
                        VisaTypeCategory visaTypeEnum = ConvertFilterVisaTypeCategoryToVisaTypeCategoryEntity(visaTypeCategory);
                        visaTypeCategories = (await GetVisaTypesAsync(bypassCache)).Where(v => v.VisaTypeCategory.Equals(visaTypeEnum)).Select(i => i.Code).ToList();
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.PersonVisa>, int>(personVisaDtos, 0);
                    }

                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.PersonVisa>, int>(personVisaDtos, 0);
                }
            }

            #endregion

            #region repository

            Tuple<IEnumerable<PersonVisa>, int> personVisaEntities = null;


            try {
                personVisaEntities = await _personVisasRepository.GetAllPersonVisas2Async(offset, limit, person, visaTypeCategories, visaTypeDetail, bypassCache);
            }

            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (personVisaEntities == null)
            {
                return new Tuple<IEnumerable<Dtos.PersonVisa>, int>(personVisaDtos, 0);
            }

            #endregion

            #region convert

            var personIds = personVisaEntities.Item1
                   .Where(x => (!string.IsNullOrEmpty(x.PersonId)))
                   .Select(x => x.PersonId).Distinct().ToList();

            var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);

            foreach (var personVisaEntity in personVisaEntities.Item1)
            {
                try
                {
                    personVisaDtos.Add(await ConvertPersonVisaEntityToDto2Async(personVisaEntity, personGuidCollection,  bypassCache));                  
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(string.Concat("An error occurred extracting the person-visa: " , ex.Message), "Bad.Data", personVisaEntity.Guid, personVisaEntity.PersonId);
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            #endregion

            return personVisaDtos.Any() ? new Tuple<IEnumerable<Dtos.PersonVisa>, int>(personVisaDtos, personVisaEntities.Item2) :
                                          new Tuple<IEnumerable<Dtos.PersonVisa>, int>(personVisaDtos, 0);

        }

        /// <summary>
        /// Gets person visa record by person visa guid
        /// </summary>
        /// <param name="id">the guid for person visas record</param>
        /// <returns>Dtos.PersonVisa</returns>
        public async Task<Dtos.PersonVisa> GetPersonVisaByIdAsync(string id)
        {
      
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Must provide a personVisa id for retrieval");
            }

            PersonVisa personVisaEntity = null;

            try
            {
                personVisaEntity = await _personVisasRepository.GetPersonVisaByIdAsync(id);
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("No person-visas was found for GUID '" + id + "'.");
            }

            if ((personVisaEntity == null) || (string.IsNullOrEmpty(personVisaEntity.Type)))
            {
                throw new KeyNotFoundException("No person-visas was found for GUID '" + id + "'.");
            }


            Dtos.PersonVisa personVisaDto = await ConvertPersonVisaEntityToDtoAsync(personVisaEntity, false);
            return personVisaDto;
        }

        /// <summary>
        /// Gets person visa record by person visa guid
        /// </summary>
        /// <param name="guid">the guid for person visa record</param>
        /// <returns>Dtos.PersonVisa</returns>
        public async Task<Dtos.PersonVisa> GetPersonVisaById2Async(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                 IntegrationApiExceptionAddError("Must provide a person-visas GUID for retrieval.", "Missing.GUID",
                   "", "", System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }

            PersonVisa personVisaEntity = null;

            try
            {
                personVisaEntity = await _personVisasRepository.GetPersonVisaByIdAsync(guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: guid);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("No person-visas was found for GUID '" + guid + "'.");
            }

            if ((personVisaEntity == null) || (string.IsNullOrEmpty(personVisaEntity.Type)))
            {
                IntegrationApiExceptionAddError("No person-visas was found for GUID '" + guid + "'.", "GUID.Not.Found",
                    guid, "", System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }

            var personIds = new List<string> { personVisaEntity.PersonId };
            var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);

            var personVisaDto = await ConvertPersonVisaEntityToDto2Async(personVisaEntity, personGuidCollection, false);

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return personVisaDto;
        }

        #endregion

        #region PUT/POST

        /// <summary>
        /// Creates a new person visa record
        /// </summary>
        /// <param name="personVisa">personVisa</param>
        /// <returns>Dtos.PersonVisa</returns>
        public async Task<Dtos.PersonVisa> PostPersonVisaAsync(Dtos.PersonVisa personVisa)
        {
            
            if (personVisa == null || personVisa.Person == null || string.IsNullOrEmpty(personVisa.Person.Id))
            {
                throw new KeyNotFoundException("person id is a required property for personVisas");
            }
            string id = string.Empty;
            return await this.PutPersonVisaAsync(id, personVisa);
        }

        /// <summary>
        /// Creates a new person visa record
        /// </summary>
        /// <param name="personVisa">personVisa</param>
        /// <returns>Dtos.PersonVisa</returns>
        public async Task<Dtos.PersonVisa> PostPersonVisa2Async(Dtos.PersonVisa personVisa)
        {
            if (personVisa == null)
                throw new ArgumentNullException("PersonVisa", "Must provide a personVisa for create.");

            return await this.PutPersonVisa2Async(string.Empty, personVisa);
        }

        /// <summary>
        /// Updates existing person visa record
        /// </summary>
        /// <param name="id">the guid for person visa record</param>
        /// <param name="personVisa">Dtos.PersonVisa</param>
        /// <returns>Dtos.PersonVisa</returns>
        public async Task<Dtos.PersonVisa> PutPersonVisaAsync(string id, Dtos.PersonVisa personVisa)
        {

            if (string.IsNullOrEmpty(id))
            {
                id = personVisa.Id;
            }
            if (personVisa == null || personVisa.Person == null || string.IsNullOrEmpty(personVisa.Person.Id))
            {
                throw new KeyNotFoundException("person id is a required property for personVisas");
            }

            Validate(personVisa);

            var personVisaLookUpResult = await _personVisasRepository.GetRecordInfoFromGuidAsync(id);
            var personLookUpResult = await _personVisasRepository.GetRecordInfoFromGuidAsync(personVisa.Person.Id);

            if (personLookUpResult == null)
            {
                throw new KeyNotFoundException("Person id associated to id " + personVisa.Person.Id + " not found.");
            }

            if (personVisaLookUpResult != null && !personVisaLookUpResult.PrimaryKey.Equals(personLookUpResult.PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException("The person id is for a different person than the id of the personVisas.");
            }

            if ((personVisaLookUpResult != null && string.IsNullOrEmpty(personVisaLookUpResult.PrimaryKey)) || string.IsNullOrEmpty(personLookUpResult.PrimaryKey))
            {
                throw new KeyNotFoundException("Person id or personVisa id were not found.");
            }

            _personVisasRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var personVisaRequest = await ConvertPersonVisaDtoToRequestAsync(personLookUpResult.PrimaryKey, personVisa);

            var personVisaResponse = await _personVisasRepository.UpdatePersonVisaAsync(personVisaRequest);

             return await ConvertPersonVisaEntityToDtoAsync(personVisaResponse, false);

        }

        /// <summary>
        /// Updates existing person visa record
        /// </summary>
        /// <param name="id">the guid for person visa record</param>
        /// <param name="personVisa">Dtos.PersonVisa</param>
        /// <returns>Dtos.PersonVisa</returns>
        public async Task<Dtos.PersonVisa> PutPersonVisa2Async(string id, Dtos.PersonVisa personVisa)
        {
            if (personVisa == null)
            {
                throw new ArgumentNullException("PersonVisa", "Must provide a personVisa for update.");
            }

            if (string.IsNullOrEmpty(id))
            {
                id = personVisa.Id;
            }

            Validate2(personVisa);

            _personVisasRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            #region create domain entity from request

            PersonVisaRequest personVisaRequest = null;

            var personVisaLookUpResult = await _personVisasRepository.GetRecordInfoFromGuidAsync(id);
            var personLookUpResult = await _personVisasRepository.GetRecordInfoFromGuidAsync(personVisa.Person.Id);

            if (personLookUpResult == null)
            {
                IntegrationApiExceptionAddError("Person id associated to id " + personVisa.Person.Id + " not found.", "Validation.Exception", personVisa.Id);
            }

            if (personVisaLookUpResult != null && personLookUpResult != null && !personVisaLookUpResult.PrimaryKey.Equals(personLookUpResult.PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
            {
                IntegrationApiExceptionAddError("The person id is for a different person than the id of the personVisas.", "Validation.Exception", personVisa.Id);
            }

            if ((personVisaLookUpResult != null && string.IsNullOrEmpty(personVisaLookUpResult.PrimaryKey)) ||
                (personLookUpResult != null && string.IsNullOrEmpty(personLookUpResult.PrimaryKey)))
            {
                IntegrationApiExceptionAddError("Person id or personVisa id were not found.", "Validation.Exception", personVisa.Id);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            try
            {
                personVisaRequest = await ConvertPersonVisaDtoToRequest2Async(personLookUpResult.PrimaryKey, personVisa);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not created/updated.  Error extracting request." + ex.Message, "Global.Internal.Error",
                   !string.IsNullOrEmpty(personVisa.Id) ? personVisa.Id : null
                  );
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            #region Create record from domain entity

            PersonVisa personVisaResponse = null;
            try
            {

                personVisaResponse = await _personVisasRepository.UpdatePersonVisaAsync(personVisaRequest);

                if (personVisaResponse == null)
                {
                    IntegrationApiExceptionAddError("An unexpected error occurred attempting to create/update the person-visa record. "
                        , "Global.Internal.Error", !string.IsNullOrEmpty(personVisa.Id) ? personVisa.Id : null);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("An unexpected error occurred attempting to create/update the person-visa record. " + ex.Message, "Global.Internal.Error",
                     string.IsNullOrEmpty(personVisa.Id) ? personVisa.Id : null);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            #endregion

            #region Build DTO response

            Dtos.PersonVisa personVisaDto = null;
            try
            {
                var personIds = new List<string> { personVisaResponse.PersonId };
                var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);

                personVisaDto = await ConvertPersonVisaEntityToDto2Async(personVisaResponse, personGuidCollection, false);
             
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record updated. Error building response. " + ex.Message, "Global.Internal.Error",
                       !string.IsNullOrEmpty(personVisaResponse.Guid) ? personVisaResponse.Guid : null,
                       !string.IsNullOrEmpty(personVisaResponse.PersonId) ? personVisaResponse.PersonId : null); ;
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            return personVisaDto;
        }

        #endregion

        #region Private methods 

        /// <summary>
        /// Check for all required fields
        /// </summary>
        /// <param name="personVisa">personVisa</param>
        private void Validate(Dtos.PersonVisa personVisa)
        {
            if (personVisa.Person != null && string.IsNullOrEmpty(personVisa.Person.Id))
            {
                throw new ArgumentNullException("Must provide an id for person.");
            }

            if (personVisa.VisaType == null)
            {
                throw new ArgumentNullException("Must provide a visaType category for update.");
            }

            if (personVisa.VisaType != null && personVisa.VisaType.VisaTypeCategory == null)
            {
                throw new ArgumentNullException("Must provide a visaType category for update.");
            }

            if (personVisa.VisaType != null && personVisa.VisaType.Detail != null && string.IsNullOrEmpty(personVisa.VisaType.Detail.Id))
            {
                throw new ArgumentNullException("Must provide an id category for visa type detail.");
            }

            if (personVisa.Entries != null && personVisa.Entries.Count() > 1)
            {
                throw new InvalidOperationException("Colleague only supports a single port of entry.");
            }

            if (personVisa.RequestedOn != null && personVisa.IssuedOn != null)
            {
                if (personVisa.RequestedOn > personVisa.IssuedOn)
                {
                    throw new InvalidOperationException("requestedOn date cannot be after issuedOn date.");
                }
            }

            if (personVisa.RequestedOn != null && personVisa.ExpiresOn != null)
            {
                if (personVisa.RequestedOn > personVisa.ExpiresOn)
                {
                    throw new InvalidOperationException("requestedOn date cannot be after expiresOn date.");
                }
            }

            if (personVisa.IssuedOn != null && personVisa.ExpiresOn != null)
            {
                if (personVisa.IssuedOn > personVisa.ExpiresOn)
                {
                    throw new InvalidOperationException("issuedOn date cannot be after expiresOn date.");
                }
            }

            if (personVisa.Entries != null && personVisa.Entries.Count() == 1 && personVisa.IssuedOn != null && personVisa.ExpiresOn != null)
            {
                if (personVisa.Entries.First().EnteredOn < personVisa.IssuedOn)
                {
                    throw new InvalidOperationException("enteredOn date cannot be before issuedOn date.");
                }

                if (personVisa.Entries.First().EnteredOn > personVisa.ExpiresOn)
                {
                    throw new InvalidOperationException("enteredOn date cannot be after expiresOn date.");
                }
            }
        }

        /// <summary>
        /// Check for all required fields
        /// </summary>
        /// <param name="personVisa">personVisa</param>
        private void Validate2(Dtos.PersonVisa personVisa)
        {
            if (personVisa == null)
            {
                IntegrationApiExceptionAddError("PersonVisa DTO must be provided.", "Missing.Request.Body");
                throw IntegrationApiException;
            }

            if ((personVisa.Person == null) || (personVisa.Person != null && string.IsNullOrEmpty(personVisa.Person.Id)))
            {
                IntegrationApiExceptionAddError("Must provide an id for person.", "Missing.Required.Property", personVisa.Id);
            }

            if (personVisa.VisaType == null)
            {
                IntegrationApiExceptionAddError("Must provide a visaType category.", "Missing.Required.Property", personVisa.Id);
            }

            if (personVisa.VisaType != null && personVisa.VisaType.VisaTypeCategory == null)
            {
                IntegrationApiExceptionAddError("Must provide a visaType category.", "Missing.Required.Property", personVisa.Id);
            }

            if (personVisa.VisaType != null && personVisa.VisaType.Detail != null && string.IsNullOrEmpty(personVisa.VisaType.Detail.Id))
            {
                IntegrationApiExceptionAddError("Must provide an id category for visa type detail.", "Missing.Required.Property", personVisa.Id);
            }

            if (personVisa.Entries != null && personVisa.Entries.Count() > 1)
            {
                IntegrationApiExceptionAddError("Colleague only supports a single port of entry.", "Validation.Exception", personVisa.Id);
            }

            if (personVisa.RequestedOn != null && personVisa.IssuedOn != null)
            {
                if (personVisa.RequestedOn > personVisa.IssuedOn)
                {
                    IntegrationApiExceptionAddError("requestedOn date cannot be after issuedOn date.", "Validation.Exception", personVisa.Id);
                }
            }

            if (personVisa.RequestedOn != null && personVisa.ExpiresOn != null)
            {
                if (personVisa.RequestedOn > personVisa.ExpiresOn)
                {
                    IntegrationApiExceptionAddError("requestedOn date cannot be after expiresOn date.", "Validation.Exception", personVisa.Id);
                }
            }

            if (personVisa.IssuedOn != null && personVisa.ExpiresOn != null)
            {
                if (personVisa.IssuedOn > personVisa.ExpiresOn)
                {
                    IntegrationApiExceptionAddError("issuedOn date cannot be after expiresOn date.", "Validation.Exception", personVisa.Id);
                }
            }

            if (personVisa.Entries != null && personVisa.Entries.Count() == 1 && personVisa.IssuedOn != null && personVisa.ExpiresOn != null)
            {
                if (personVisa.Entries.First().EnteredOn < personVisa.IssuedOn)
                {
                    IntegrationApiExceptionAddError("enteredOn date cannot be before issuedOn date.", "Validation.Exception", personVisa.Id);
                }

                if (personVisa.Entries.First().EnteredOn > personVisa.ExpiresOn)
                {
                    IntegrationApiExceptionAddError("enteredOn date cannot be after expiresOn date.", "Validation.Exception", personVisa.Id);
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

        }

        /// <summary>
        /// Erases visa related infor from the respective columns, its not a hard delete rather soft delete
        /// </summary>
        /// <param name="id">the guid for person visa record</param>
        public async Task DeletePersonVisaAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Must provide an id for person-visas.");
            }

            var personVisaLookUpResult = await _personVisasRepository.GetRecordInfoFromGuidAsync(id);
            if (personVisaLookUpResult == null)
            {
                throw new KeyNotFoundException("Person id associated to id " + id + " not found.");
            }

            await _personVisasRepository.DeletePersonVisaAsync(id, personVisaLookUpResult.PrimaryKey);
        }

        /// <summary>
        /// Converts person visa dto to person visa request
        /// </summary>
        /// <param name="entityId">entityId</param>
        /// <param name="personVisa">Dtos.PersonVisa</param>
        /// <returns>PersonVisaRequest</returns>
        private async Task<PersonVisaRequest> ConvertPersonVisaDtoToRequestAsync(string entityId, Dtos.PersonVisa personVisa)
        {
            PersonVisaRequest personVisaRequest = new PersonVisaRequest(personVisa.Id, entityId);
            if (personVisa.Entries != null && personVisa.Entries.Any())
            {
                personVisaRequest.EntryDate = (personVisa.Entries.First().EnteredOn == null) ? default(DateTime?) : personVisa.Entries.First().EnteredOn.Value.Date;
            }

            if (personVisa.ExpiresOn != null && personVisa.VisaStatus != null)
            {
                if (personVisa.ExpiresOn < DateTime.Today && personVisa.VisaStatus == Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Current)
                {
                    throw new InvalidOperationException("Person visa cannot be current and have an expiration date in the past.");
                }
                if (personVisa.ExpiresOn > DateTime.Today && personVisa.VisaStatus == Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Expired)
                {
                    throw new InvalidOperationException("Person visa status should be 'current' when expiresOn date is in the future.");
                }
            }

            if (personVisa.VisaStatus == Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Expired && personVisa.ExpiresOn == null)
            {
                throw new InvalidOperationException("The expiresOn date is missing but required when setting the status to 'expired'.");
            }

            personVisaRequest.ExpireDate = personVisa.ExpiresOn;
            personVisaRequest.IssueDate = (personVisa.IssuedOn == null) ? default(DateTime?) : personVisa.IssuedOn;
            personVisaRequest.PersonId = entityId;
            personVisaRequest.RequestDate = (personVisa.RequestedOn == null) ? default(DateTime?) : personVisa.RequestedOn;
            personVisaRequest.Status = (personVisa.ExpiresOn == null || personVisa.ExpiresOn >= DateTime.Today) ? "current" : "expired";
            personVisaRequest.StrGuid = personVisa.Id;
            personVisaRequest.VisaNo = personVisa.VisaId;

            personVisaRequest.VisaType = await ConvertDtoVisaTypeToEntityVisaTypeCategoryAsync(personVisa, false);

            return personVisaRequest;
        }

        /// <summary>
        /// Converts person visa dto to person visa request
        /// </summary>
        /// <param name="entityId">entityId</param>
        /// <param name="personVisa">Dtos.PersonVisa</param>
        /// <returns>PersonVisaRequest</returns>
        private async Task<PersonVisaRequest> ConvertPersonVisaDtoToRequest2Async(string entityId, Dtos.PersonVisa personVisa)
        {

            PersonVisaRequest personVisaRequest = new PersonVisaRequest(personVisa.Id, entityId);
            if (personVisa.Entries != null && personVisa.Entries.Any())
            {
                personVisaRequest.EntryDate = (personVisa.Entries.First().EnteredOn == null) ? default(DateTime?) : personVisa.Entries.First().EnteredOn.Value.Date;
            }

            if (personVisa.ExpiresOn != null && personVisa.VisaStatus != null)
            {
                if (personVisa.ExpiresOn < DateTime.Today && personVisa.VisaStatus == Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Current)
                {
                    IntegrationApiExceptionAddError("Person visa cannot be current and have an expiration date in the past.", "Validation.Exception", personVisa.Id);
                }
                if (personVisa.ExpiresOn > DateTime.Today && personVisa.VisaStatus == Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Expired)
                {
                    IntegrationApiExceptionAddError("Person visa status should be 'current' when expiresOn date is in the future.","Validation.Exception", personVisa.Id);
                }
            }

            if (personVisa.VisaStatus == Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Expired && personVisa.ExpiresOn == null)
            {
                IntegrationApiExceptionAddError("The expiresOn date is missing but required when setting the status to 'expired'.", "Validation.Exception", personVisa.Id);
            }

            personVisaRequest.ExpireDate = personVisa.ExpiresOn;
            personVisaRequest.IssueDate = (personVisa.IssuedOn == null) ? default(DateTime?) : personVisa.IssuedOn;
            personVisaRequest.PersonId = entityId;
            personVisaRequest.RequestDate = (personVisa.RequestedOn == null) ? default(DateTime?) : personVisa.RequestedOn;
            personVisaRequest.Status = (personVisa.ExpiresOn == null || personVisa.ExpiresOn >= DateTime.Today) ? "current" : "expired";
            personVisaRequest.StrGuid = personVisa.Id;
            personVisaRequest.VisaNo = personVisa.VisaId;

            personVisaRequest.VisaType = await ConvertDtoVisaTypeToEntityVisaTypeCategoryAsync(personVisa, false);

            return personVisaRequest;
        }

        /// <summary>
        /// Converts dto visa type to entity visa type category
        /// </summary>
        /// <param name="personVisa">Dtos.PersonVisa</param>
        /// <returns>string</returns>
        private async Task<string> ConvertDtoVisaTypeToEntityVisaTypeCategoryAsync(Dtos.PersonVisa personVisa, bool bypassCache)
        {
                    
            _visaTypeEntities = await this.GetVisaTypesAsync(bypassCache);
            
            //If VisaType.Detail is null then choose first one with nonImmigrant VisaTypeCategory
            VisaTypeGuidItem visaTypeEntity = null;
            string visaType = string.Empty;

            if (_visaTypeEntities != null)
            {
                if (personVisa.VisaType.Detail == null)
                {
                    var visaTypeCategory = (personVisa.VisaType.VisaTypeCategory == Dtos.VisaTypeCategory.Immigrant) ?
                        Ellucian.Colleague.Domain.Base.Entities.VisaTypeCategory.Immigrant :
                        Ellucian.Colleague.Domain.Base.Entities.VisaTypeCategory.NonImmigrant;

                    visaTypeEntity = _visaTypeEntities.FirstOrDefault(i => i.VisaTypeCategory == visaTypeCategory);
                }
                else
                {
                    visaTypeEntity = _visaTypeEntities.FirstOrDefault(i => i.Guid == personVisa.VisaType.Detail.Id);
                }

                if (visaTypeEntity == null)
                {
                    throw new KeyNotFoundException("Visa type not found.");
                }
                visaType = visaTypeEntity.Code;
            }
            return visaType;
        }

        /// <summary>
        /// Converts entity to dto
        /// </summary>
        /// <param name="personVisaEntity">PersonVisa</param>
        /// <returns>Dtos.PersonVisa</returns>
        private async Task<Dtos.PersonVisa> ConvertPersonVisaEntityToDtoAsync(PersonVisa personVisaEntity, bool bypassCache)
        {
            Dtos.PersonVisa personVisaDto = new Dtos.PersonVisa();
            personVisaDto.Id = personVisaEntity.Guid;
            personVisaDto.Person = new Dtos.GuidObject2(personVisaEntity.PersonGuid);
            personVisaDto.VisaType = await ConvertToVisaTypeDtoAsync(personVisaEntity.Type, bypassCache);
            if (!string.IsNullOrEmpty(personVisaEntity.VisaNumber))
            {
                personVisaDto.VisaId = personVisaEntity.VisaNumber;
            }
            personVisaDto.VisaStatus = ConvertToVisaStatusDto(personVisaEntity.ExpireDate);
            personVisaDto.RequestedOn = personVisaEntity.RequestDate.HasValue ? personVisaEntity.RequestDate.Value.Date : default(DateTime?);
            personVisaDto.IssuedOn = personVisaEntity.IssueDate.HasValue ? personVisaEntity.IssueDate.Value.Date : default(DateTime?);
            personVisaDto.ExpiresOn = personVisaEntity.ExpireDate.HasValue ? personVisaEntity.ExpireDate.Value.Date : default(DateTime?);
            personVisaDto.Entries = (personVisaEntity.EntryDate == null) ?
                null :
                new List<Dtos.PersonVisaEntry>()
                {
                    new Dtos.PersonVisaEntry(){ EnteredOn = personVisaEntity.EntryDate}
                };
            return personVisaDto;
        }

        /// <summary>
        /// Converts entity to dto
        /// </summary>
        /// <param name="personVisaEntity">PersonVisa</param>
        /// <returns>Dtos.PersonVisa</returns>
        private async Task<Dtos.PersonVisa> ConvertPersonVisaEntityToDto2Async(PersonVisa personVisaEntity,
             Dictionary<string, string> personGuidCollection, bool bypassCache)
        {

            if (personVisaEntity == null)
            {
                IntegrationApiExceptionAddError("personVisaEntity is a required parameter.", "Bad.Data");
                return null;
            }

           var personVisaDto = new Dtos.PersonVisa
            {
                Id = personVisaEntity.Guid               
            };

            if (personGuidCollection == null)
            {
                IntegrationApiExceptionAddError(string.Concat("Person GUID not found for person.id: '", personVisaEntity.PersonId, "'"), "GUID.Not.Found",
                    personVisaEntity.Guid, personVisaEntity.PersonId);
            }
            else
            {
                var personGuid = string.Empty;
                personGuidCollection.TryGetValue(personVisaEntity.PersonId, out personGuid);
                if (string.IsNullOrEmpty(personGuid))
                {
                    IntegrationApiExceptionAddError(string.Concat("Person GUID not found for person.id: '", personVisaEntity.PersonId, "'"), "GUID.Not.Found", 
                        personVisaEntity.Guid, personVisaEntity.PersonId);
                }
                else
                {
                    personVisaDto.Person = new Dtos.GuidObject2(personGuid);
                }
            }
            if (!string.IsNullOrEmpty(personVisaEntity.Type))
            {
                try
                {
                    var visaTypeEntities = await this.GetVisaTypesAsync(bypassCache);
                    if (visaTypeEntities == null)
                    {
                        IntegrationApiExceptionAddError("Unable to retrieve CORE.VALCODES-VISA.TYPES.", "Bad.Data", personVisaEntity.Guid, personVisaEntity.PersonId);
                    }
                    else
                    {

                        var visaTypeEntity = visaTypeEntities.FirstOrDefault(i => i.Code == personVisaEntity.Type);

                        if (visaTypeEntity == null)
                        {
                            IntegrationApiExceptionAddError(string.Concat("No GUID found, Entity: 'CORE.VALCODES-VISA.TYPES', Record ID: '", personVisaEntity.Type, "'"), "GUID.Not.Found"
                                , personVisaEntity.Guid, personVisaEntity.PersonId);
                        }
                        else
                        {
                            personVisaDto.VisaType = new Dtos.VisaType2
                            {
                                VisaTypeCategory = ConvertEntityVisaTypeCategoryToVisaTypeCategoryDto(visaTypeEntity.VisaTypeCategory),
                                Detail = new Dtos.GuidObject2(visaTypeEntity.Guid)
                            };
                        }
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", personVisaEntity.Guid, personVisaEntity.PersonId);
                }
            }                     

            if (!string.IsNullOrEmpty(personVisaEntity.VisaNumber))
            {
                personVisaDto.VisaId = personVisaEntity.VisaNumber;
            }

            Dtos.EnumProperties.VisaStatus visaStatus = Dtos.EnumProperties.VisaStatus.Current;

            // If FPER.VISA.EXPIRE.DATE has a value in Colleague and the date is before today then on GET this will be set to "expired" 
            // or after today's date otherwise it will be set to "current".  
            if (personVisaEntity.ExpireDate != null && personVisaEntity.ExpireDate < DateTime.Today)
            {
                visaStatus = Dtos.EnumProperties.VisaStatus.Expired;
            }
            personVisaDto.VisaStatus = visaStatus;

            personVisaDto.RequestedOn = personVisaEntity.RequestDate.HasValue ? personVisaEntity.RequestDate.Value.Date : default(DateTime?);
            personVisaDto.IssuedOn = personVisaEntity.IssueDate.HasValue ? personVisaEntity.IssueDate.Value.Date : default(DateTime?);
            personVisaDto.ExpiresOn = personVisaEntity.ExpireDate.HasValue ? personVisaEntity.ExpireDate.Value.Date : default(DateTime?);
            personVisaDto.Entries = (personVisaEntity.EntryDate == null) ?
                null :
                new List<Dtos.PersonVisaEntry>()
                {
                    new Dtos.PersonVisaEntry(){ EnteredOn = personVisaEntity.EntryDate}
                };
            return personVisaDto;
        }

        /// <summary>
        /// Converts visa status based on the expiresOn date
        /// </summary>
        /// <param name="expiresOn">expiresOn</param>
        /// <returns>Dtos.EnumProperties.VisaStatus</returns>
        private Dtos.EnumProperties.VisaStatus? ConvertToVisaStatusDto(DateTime? expiresOn)
        {
            Dtos.EnumProperties.VisaStatus visaStatus = Dtos.EnumProperties.VisaStatus.Current;

            // If FPER.VISA.EXPIRE.DATE has a value in Colleague and the date is before today then on GET this will be set to "expired" 
            // or after today's date otherwise it will be set to "current".  
            if (expiresOn != null && expiresOn < DateTime.Today)
            {
                visaStatus = Dtos.EnumProperties.VisaStatus.Expired;
            }

            return visaStatus;
        }

        /// <summary>
        /// converts visatype to dto
        /// </summary>
        /// <param name="visaType">visaType</param>
        /// <returns>Dtos.VisaType2</returns>
        private async Task<Dtos.VisaType2> ConvertToVisaTypeDtoAsync(string visaType, bool bypassCache)
        {
            Dtos.VisaType2 visaTypeDto = null;

            _visaTypeEntities = await this.GetVisaTypesAsync(bypassCache);


            Ellucian.Colleague.Domain.Base.Entities.VisaTypeGuidItem visaTypeEntity = null;
            if (_visaTypeEntities != null)
            {
                visaTypeEntity = _visaTypeEntities.FirstOrDefault(i => i.Code == visaType);
                if (visaTypeEntity == null)
                {
                    throw new KeyNotFoundException("Visa type associated with visa type " + visaType + " not found");
                }
                visaTypeDto = new Dtos.VisaType2();
                visaTypeDto.VisaTypeCategory = ConvertEntityVisaTypeCategoryToVisaTypeCategoryDto(visaTypeEntity.VisaTypeCategory);
                visaTypeDto.Detail = new Dtos.GuidObject2(visaTypeEntity.Guid);
            }

            return visaTypeDto;
        }

        /// <summary>
        /// converts entity visa type category to dto visa type category
        /// </summary>
        /// <param name="Domain.Base.Entities.visaTypeCategory">Domain.Base.Entities.visaTypeCategory</param>
        /// <returns>Dtos.VisaTypeCategory</returns>
        private Dtos.VisaTypeCategory ConvertEntityVisaTypeCategoryToVisaTypeCategoryDto(Domain.Base.Entities.VisaTypeCategory visaTypeCategory)
        {
            switch (visaTypeCategory)
            {
                case Ellucian.Colleague.Domain.Base.Entities.VisaTypeCategory.Immigrant:
                    return Dtos.VisaTypeCategory.Immigrant;
                case Ellucian.Colleague.Domain.Base.Entities.VisaTypeCategory.NonImmigrant:
                    return Dtos.VisaTypeCategory.NonImmigrant;
                default:
                    return Dtos.VisaTypeCategory.NonImmigrant;
            }
        }

        /// <summary>
        /// converts entity visa type category to dto visa type category
        /// </summary>
        /// <param name="Domain.Base.Entities.visaTypeCategory">Domain.Base.Entities.visaTypeCategory</param>
        /// <returns>Dtos.VisaTypeCategory</returns>
        private VisaTypeCategory ConvertFilterVisaTypeCategoryToVisaTypeCategoryEntity(string visaTypeCategory)
        {
            switch (visaTypeCategory)
            {
                case "immigrant":
                    return VisaTypeCategory.Immigrant;
                case "nonImmigrant":
                    return VisaTypeCategory.NonImmigrant;
                default:
                    throw new InvalidOperationException("Invalid visa type category filter value.");
            }
        }

        #endregion
    }
}