// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
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

        IEnumerable<VisaTypeGuidItem> visaTypeEntities = null;
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

        #region GET methods

        /// <summary>
        /// Gets all person visas based on paging
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonVisa>, int>> GetAllAsync(int offset, int limit, string person, bool bypassCache)
        {
            CheckUserPersonVisasViewPermissions();

            Tuple<IEnumerable<PersonVisa>, int> personVisaEntities = await _personVisasRepository.GetAllPersonVisasAsync(offset, limit, person, bypassCache);
            List<Dtos.PersonVisa> personVisaDtos = new List<Dtos.PersonVisa>();

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
            CheckUserPersonVisasViewPermissions();

            List<string> visaTypeCategories = null;

            if (!string.IsNullOrEmpty(visaTypeCategory))
            {
                if (visaTypeCategory.Equals("nonImmigrant") || visaTypeCategory.Equals("immigrant"))
                {
                    VisaTypeCategory visaTypeEnum = ConvertFilterVisaTypeCategoryToVisaTypeCategoryEntity(visaTypeCategory);
                    visaTypeCategories = (await _referenceDataRepository.GetVisaTypesAsync(bypassCache)).Where(v => v.VisaTypeCategory.Equals(visaTypeEnum)).Select(i => i.Code).ToList();

                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.PersonVisa>, int>(new List<Dtos.PersonVisa>(), 0); ;
                }
            }

            Tuple<IEnumerable<PersonVisa>, int> personVisaEntities = await _personVisasRepository.GetAllPersonVisas2Async(offset, limit, person, visaTypeCategories, visaTypeDetail, bypassCache);
            List<Dtos.PersonVisa> personVisaDtos = new List<Dtos.PersonVisa>();

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
        /// Gets person visa record by person visa id
        /// </summary>
        /// <param name="id">the guid for person visa record</param>
        /// <returns>Dtos.PersonVisa</returns>
        public async Task<Dtos.PersonVisa> GetPersonVisaByIdAsync(string id)
        {
            CheckUserPersonVisasViewPermissions();

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Must provide a personVisa id for retrieval");
            }

            PersonVisa personVisaEntity = await _personVisasRepository.GetPersonVisaByIdAsync(id);

            if(personVisaEntity == null)
            {
                throw new KeyNotFoundException("Person visa associated to id " + id + " not found.");
            }

            if (string.IsNullOrEmpty(personVisaEntity.Type))
            {
                throw new KeyNotFoundException("Person visa associated to id " + id + " not found.");
            }

            Dtos.PersonVisa personVisaDto = await ConvertPersonVisaEntityToDtoAsync(personVisaEntity, false);
            return personVisaDto;
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view a person's guardian.
        /// </summary>
        private void CheckUserPersonVisasViewPermissions()
        {
            // access is ok if the current user has the view person visas permission
            if (!HasPermission(BasePermissionCodes.ViewAnyPersonVisa))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view person-visas.");
                throw new PermissionsException("User is not authorized to view person-visas.");
            }
        }

        #endregion

        #region POST Method
        /// <summary>
        /// Creates a new person visa record
        /// </summary>
        /// <param name="personVisa">personVisa</param>
        /// <returns>Dtos.PersonVisa</returns>
        public async Task<Dtos.PersonVisa> PostPersonVisaAsync(Dtos.PersonVisa personVisa)
        {
            CheckUserPersonVisasCreatePermissions();

            if (personVisa == null || personVisa.Person == null || string.IsNullOrEmpty(personVisa.Person.Id))
            {
                throw new KeyNotFoundException("person id is a required property for personVisas");
            }
            string id = string.Empty;
            return await this.PutPersonVisaAsync(id, personVisa);
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to create a person's guardian.
        /// </summary>
        private void CheckUserPersonVisasCreatePermissions()
        {
            // access is ok if the current user has the create person visas permission
            if (!HasPermission(BasePermissionCodes.UpdateAnyPersonVisa))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create person-visas.");
                throw new PermissionsException("User is not authorized to create person-visas.");
            }
        }

        #endregion

        #region PUT Method
        /// <summary>
        /// Updates existing person visa record
        /// </summary>
        /// <param name="id">the guid for person visa record</param>
        /// <param name="personVisa">Dtos.PersonVisa</param>
        /// <returns>Dtos.PersonVisa</returns>
        public async Task<Dtos.PersonVisa> PutPersonVisaAsync(string id, Dtos.PersonVisa personVisa)
        {
            CheckUserPersonVisasUpdatePermissions();

            if (string.IsNullOrEmpty(id))
            {
                id = personVisa.Id;
            }
            if (personVisa == null || personVisa.Person == null || string.IsNullOrEmpty(personVisa.Person.Id))
            {
                throw new KeyNotFoundException("person id is a required property for personVisas");
            }

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

            PersonVisaRequest personVisaRequest = await ConvertPersonVisaDtoToRequestAsync(personLookUpResult.PrimaryKey, personVisa);

            PersonVisaResponse personVisaResponse = await _personVisasRepository.UpdatePersonVisaAsync(personVisaRequest);

            Dtos.PersonVisa personVisaDto = await this.GetPersonVisaByIdAsync(personVisaResponse.StrGuid);

            return personVisaDto;
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to update a person's guardian.
        /// </summary>
        private void CheckUserPersonVisasUpdatePermissions()
        {
            // access is ok if the current user has the update person visas permission
            if (!HasPermission(BasePermissionCodes.UpdateAnyPersonVisa))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update person-visas.");
                throw new PermissionsException("User is not authorized to update person-visas.");
            }
        }

        #endregion

        #region DELETE Method
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
        #endregion

        #region Convert Methods
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
                if (personVisa.ExpiresOn <= DateTime.Today && personVisa.VisaStatus == Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Current)
                {
                    throw new InvalidOperationException("Person visa cannot be current and have an expiration date in the past.");
                }
                if (personVisa.ExpiresOn >= DateTime.Today && personVisa.VisaStatus == Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Expired)
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
        /// Converts dto visa type to entity visa type category
        /// </summary>
        /// <param name="personVisa">Dtos.PersonVisa</param>
        /// <returns>string</returns>
        private async Task<string> ConvertDtoVisaTypeToEntityVisaTypeCategoryAsync(Dtos.PersonVisa personVisa, bool bypassCache)
        {
            if (visaTypeEntities == null)
            {
                visaTypeEntities = await _referenceDataRepository.GetVisaTypesAsync(bypassCache);
            }
            //If VisaType.Detail is null then choose first one with nonImmigrant VisaTypeCategory
            VisaTypeGuidItem visaTypeEntity = null;
            string visaType = string.Empty;

            if (visaTypeEntities != null)
            {
                if (personVisa.VisaType.Detail == null)
                {
                    var visaTypeCategory = (personVisa.VisaType.VisaTypeCategory == Dtos.VisaTypeCategory.Immigrant) ?
                        Ellucian.Colleague.Domain.Base.Entities.VisaTypeCategory.Immigrant :
                        Ellucian.Colleague.Domain.Base.Entities.VisaTypeCategory.NonImmigrant;

                    visaTypeEntity = visaTypeEntities.FirstOrDefault(i => i.VisaTypeCategory == visaTypeCategory);
                }
                else
                {
                    visaTypeEntity = visaTypeEntities.FirstOrDefault(i => i.Guid == personVisa.VisaType.Detail.Id);
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
            if (visaTypeEntities == null)
            {
                visaTypeEntities = await _referenceDataRepository.GetVisaTypesAsync(bypassCache);
            }

            Ellucian.Colleague.Domain.Base.Entities.VisaTypeGuidItem visaTypeEntity = null;
            if(visaTypeEntities != null)
            {
                visaTypeEntity = visaTypeEntities.FirstOrDefault(i => i.Code == visaType);
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

        /// <summary>
        /// converts entity visa type category to dto visa type category
        /// </summary>
        /// <param name="Domain.Base.Entities.visaTypeCategory">Domain.Base.Entities.visaTypeCategory</param>
        /// <returns>Dtos.VisaTypeCategory</returns>
        private Dtos.VisaTypeCategory ConvertFilterVisaTypeCategoryToVisaTypeCategoryDto(string visaTypeCategory)
        {
            switch (visaTypeCategory)
            {
                case "immigrant":
                    return Dtos.VisaTypeCategory.Immigrant;
                case "nonImmigrant":
                    return Dtos.VisaTypeCategory.NonImmigrant;
                default:
                    return Dtos.VisaTypeCategory.NotSet;
            }
        }
        #endregion
    }
}