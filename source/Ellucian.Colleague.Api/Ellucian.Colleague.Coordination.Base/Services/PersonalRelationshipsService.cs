// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Dmi.Runtime;
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
    /// <summary>
    /// Personal relationships
    /// </summary>
    [RegisterType]
    public class PersonalRelationshipsService : BaseCoordinationService, IPersonalRelationshipsService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonMatchingRequestsRepository _personMatchingRequestsRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IConfigurationRepository _configurationRepository;

        private IEnumerable<RelationType> relationTypes;
        private IEnumerable<Domain.Base.Entities.RelationshipStatus> relationshipStatuses;
        private List<string> defaultGuardianRelationships;

        public PersonalRelationshipsService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IRelationshipRepository relationshipRepository,
                                            IPersonRepository personRepository, IPersonMatchingRequestsRepository personMatchingRequestsRepository, IConfigurationRepository configurationRepository,
                                            ICurrentUserFactory currentUserFactory,
                                            IRoleRepository roleRepository,
                                            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
            //: base(adapterRegistry, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
            _personMatchingRequestsRepository = personMatchingRequestsRepository;
            _relationshipRepository = relationshipRepository;
            _configurationRepository = configurationRepository;
        }
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

        #region GET
        /// <summary>
        /// Gets all the personal relationships
        /// </summary>
        /// <returns>IEnumerable<Dtos.PersonalRelationship></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonalRelationship>, int>> GetAllPersonalRelationshipsAsync(int offset, int limit, bool bypassCache)
        {
                     
            List<Dtos.PersonalRelationship> personalRelationships = new List<Dtos.PersonalRelationship>();

            List<string> guardianWithInverseRels = await GetGuardianRelationsWithInverseList();

            var personalRelationshipEntities = await _relationshipRepository.GetAllAsync(offset, limit, bypassCache, guardianWithInverseRels);

            foreach (var personalRelationshipEntity in personalRelationshipEntities.Item1)
            {
                var personalRelationship = await ConvertEntityToDto(personalRelationshipEntity);
                personalRelationships.Add(personalRelationship);
            }
            return new Tuple<IEnumerable<Dtos.PersonalRelationship>, int>(personalRelationships, personalRelationshipEntities.Item2);
        }        

        /// <summary>
        /// Gets a personal relationship based on id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.PersonalRelationship</returns>
        public async Task<Dtos.PersonalRelationship> GetPersonalRelationshipByIdAsync(string id)
        {
            var personalRelationshipEntity = await _relationshipRepository.GetPersonRelationshipByIdAsync(id);

            bool isGurdianRelationship = await this.IsGuardianRelationship(personalRelationshipEntity.RelationshipType);
            if (isGurdianRelationship)
            {
                throw new InvalidOperationException("Guardian relationships should be retrieved using the person-guardians resource.");
            }

            Dtos.PersonalRelationship personalRelationship = await ConvertEntityToDto(personalRelationshipEntity);
            return personalRelationship;
        }

        /// <summary>
        /// Gets personal relationships based on filters
        /// </summary>
        /// <param name="subjectPerson"></param>
        /// <param name="relatedPerson"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonalRelationship>, int>> GetPersonalRelationshipsByFilterAsync(int offset, int limit, 
            string subjectPerson, string relatedPerson, string directRelationshipType, string directRelationshipDetailId)
        {
           
            var personRelationshipDtos = new List<Dtos.PersonalRelationship>();

            string newSubjectPerson = string.Empty, newRelatedPerson = string.Empty, newDirectRelationshipType = string.Empty,
                newDirectRelationshipDetailId = string.Empty;
            // Convert and validate all input parameters
            try
            {
                newSubjectPerson = string.IsNullOrEmpty(subjectPerson) ? string.Empty : await GetPersonId(subjectPerson);
                newRelatedPerson = string.IsNullOrEmpty(relatedPerson) ? string.Empty : await GetPersonId(relatedPerson);
                newDirectRelationshipType = string.IsNullOrEmpty(directRelationshipType) ? string.Empty : await ConvertRelationshipTypeToCode(directRelationshipType);
                newDirectRelationshipDetailId = string.IsNullOrEmpty(directRelationshipDetailId) ? string.Empty : await ConvertRelationshipTypeGuidToCode(directRelationshipDetailId);
            }
            catch (Exception)
            {
                return new Tuple<IEnumerable<Dtos.PersonalRelationship>, int>(personRelationshipDtos, 0);
            }

            List<string> guardianWithInverseRels = await GetGuardianRelationsWithInverseList();

            var personalRelationshipEntities = await _relationshipRepository.GetRelationshipsAsync(offset, limit, guardianWithInverseRels, newSubjectPerson, newRelatedPerson,
                newDirectRelationshipType, newDirectRelationshipDetailId);

            foreach (var personalRelationshipEntity in personalRelationshipEntities.Item1)
            {
                if (personalRelationshipEntity.Guid != null)
                {
                    var personalRelationshipDto = await ConvertEntityToDto(personalRelationshipEntity);
                    personRelationshipDtos.Add(personalRelationshipDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.PersonalRelationship>, int>(personRelationshipDtos, personalRelationshipEntities.Item2);
        }

        /// <summary>
        /// Get the person ID from a GUID
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        private async Task<string> GetPersonId(string person)
        {
            var id = await _personRepository.GetPersonIdFromGuidAsync(person);
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("Could not find person key with id: " + person);

            }
            return id;
        }
        
        #endregion


        #region Helper methods
        /// <summary>
        /// Gets guardian relations with their inverse relaton type list
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> GetGuardianRelationsWithInverseList()
        {
            List<string> guardianRels = new List<string>();
            if (relationTypes == null)
            {
                relationTypes = await this.GetRelationTypesAsync(true);
            }
            if (defaultGuardianRelationships == null)
            {
                defaultGuardianRelationships = await this.GetDefaultGuardianRelationTypes();
            }

            if (defaultGuardianRelationships != null)
            {
                guardianRels
                    .AddRange(relationTypes.Where(i => defaultGuardianRelationships.Contains(i.Code) ||
                                                       defaultGuardianRelationships.Contains(i.InverseRelType)).Select(r => r.Code)
                                           .ToArray());
            }
            return guardianRels.Any() ? guardianRels : null; ;
        }

        /// <summary>
        /// Determines if the relationship type is of Guardian type specified in CDHP form
        /// </summary>
        /// <param name="relationshipType"></param>
        /// <returns></returns>
        private async Task<bool> IsGuardianRelationship(string relationshipType)
        {
            bool isGuardianRelationship = false;

            if (relationTypes == null)
            {
                relationTypes = await this.GetRelationTypesAsync(true);
            }

            if (defaultGuardianRelationships == null)
            {
                defaultGuardianRelationships = await this.GetDefaultGuardianRelationTypes();
            }

            if (defaultGuardianRelationships != null && defaultGuardianRelationships.Any())
            {
                var subjectRelType = relationTypes.FirstOrDefault(x => x.Code.Equals(relationshipType, StringComparison.OrdinalIgnoreCase));
                if (subjectRelType != null)
                {
                    var guardRelType = relationTypes
                        .FirstOrDefault(x => x.Code.Equals(subjectRelType.Code, StringComparison.OrdinalIgnoreCase) ||
                                             x.Code.Equals(subjectRelType.InverseRelType, StringComparison.OrdinalIgnoreCase));

                    if (guardRelType != null && defaultGuardianRelationships.Contains(guardRelType.Code))
                    {
                        isGuardianRelationship = true;
                    }
                }
            }
            return isGuardianRelationship;
        }

        /// <summary>
        /// Gets the relation types collection
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<RelationType>> GetRelationTypesAsync(bool bypassCache)
        {
            if (relationTypes == null)
            {
                relationTypes = await _referenceDataRepository.GetRelationTypesAsync(bypassCache);
            }
            return relationTypes;
        }

        /// <summary>
        /// Gets list of all guardian relation types
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> GetDefaultGuardianRelationTypes()
        {
            if (defaultGuardianRelationships == null)
            {
                defaultGuardianRelationships = await _relationshipRepository.GetDefaultGuardianRelationshipTypesAsync(true);
            }
            return defaultGuardianRelationships;
        }

        /// <summary>
        /// Converts Relationship Type to code
        /// </summary>
        /// <param name="type">Relationship Type</param>
        /// <returns>Relationship Type Code</returns>
        private async Task<string> ConvertRelationshipTypeToCode(string type)
        {
            var relationships = await _referenceDataRepository.GetRelationTypesAsync(true);
            var relationshipCode = string.Empty;
            if (relationships != null && !string.IsNullOrEmpty(type))
            {
                try
                {
                    var relationship = relationships.FirstOrDefault(rel => rel.PersonRelType.ToString().Equals(type, StringComparison.OrdinalIgnoreCase));
                    if (relationship == null)
                        relationship = relationships.FirstOrDefault(rel => rel.MaleRelType.ToString().Equals(type, StringComparison.OrdinalIgnoreCase));
                    if (relationship == null)
                        relationship = relationships.FirstOrDefault(rel => rel.FemaleRelType.ToString().Equals(type, StringComparison.OrdinalIgnoreCase));
                    if (relationship == null)
                    {
                        throw new KeyNotFoundException("Could not find personal relationship entity with type: " + type);
                    }

                    if (relationship != null)
                    {
                        relationshipCode = relationship.Code;
                    }
                } 
                catch
                {
                    throw new KeyNotFoundException("Could not find personal relationship entity with type: " + type); 
                }
            }
            return relationshipCode;
        }

        /// <summary>
        /// Converts Relationship Type to code
        /// </summary>
        /// <param name="guid">Relationship Type</param>
        /// <returns>Relationship Type Code</returns>
        private async Task<string> ConvertRelationshipTypeGuidToCode(string guid)
        {
            var relationships = await _referenceDataRepository.GetRelationTypesAsync(true);
            var relationshipCode = string.Empty;
            if (relationships != null && !string.IsNullOrEmpty(guid))
            {
                try
                {
                    var relationship = relationships.FirstOrDefault(rel => rel.Guid == guid);
                    if (relationship == null)
                    {
                        throw new KeyNotFoundException("Could not find personal relationship entity with guid: " + guid);
                    }
                    relationshipCode = relationship.Code;
                }
                catch(Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            return relationshipCode;
        }

        /// <summary>
        /// Converts entity to dto
        /// </summary>
        /// <param name="personalRelationshipEntity"></param>
        /// <returns></returns>
        private async Task<Dtos.PersonalRelationship> ConvertEntityToDto(Relationship personalRelationshipEntity)
        {
            Dtos.PersonalRelationship personalRelationshipDto = new Dtos.PersonalRelationship();
            personalRelationshipDto.Id = personalRelationshipEntity.Guid;

            personalRelationshipDto.SubjectPerson = new Dtos.GuidObject2(personalRelationshipEntity.SubjectPersonGuid);
            personalRelationshipDto.RelatedPerson = new Dtos.GuidObject2(personalRelationshipEntity.RelationPersonGuid);
            personalRelationshipDto.Id = personalRelationshipEntity.Guid;
            personalRelationshipDto.Comment = personalRelationshipEntity.Comment;
            personalRelationshipDto.EndOn = personalRelationshipEntity.EndDate;
            personalRelationshipDto.StartOn = personalRelationshipEntity.StartDate;
            personalRelationshipDto.PersonalRelationshipStatus = string.IsNullOrEmpty(personalRelationshipEntity.Status) ? 
                null : 
                await ConvertEntityStatusToDtoStatus(personalRelationshipEntity.Status);

            Tuple<Dtos.Relationship, Dtos.Relationship> relationships = await ConvertEntityRelationshipTypesToDtoRelationshipTypes
                (personalRelationshipEntity.RelationshipType, personalRelationshipEntity.SubjectPersonGender, personalRelationshipEntity.RelationPersonGender);

            personalRelationshipDto.DirectRelationship = relationships.Item1;
            personalRelationshipDto.ReciprocalRelationship = relationships.Item2;
            return personalRelationshipDto;
        }

        /// <summary>
        /// Converts entity to dto relationships
        /// </summary>
        /// <param name="relType"></param>
        /// <param name="subjectGender"></param>
        /// <param name="relativesGender"></param>
        /// <returns></returns>
        private async Task<Tuple<Dtos.Relationship, Dtos.Relationship>> ConvertEntityRelationshipTypesToDtoRelationshipTypes(string relType, string subjectGender, string relativesGender)
        {
            Dtos.Relationship subjectRelation = new Dtos.Relationship();
            Dtos.Relationship relativeRelation = new Dtos.Relationship();

            if(relationTypes == null)
                relationTypes = await this.GetRelationTypesAsync(true);

            var subjectRelation_type = relationTypes.FirstOrDefault(x => x.Code.Equals(relType, StringComparison.OrdinalIgnoreCase));
            if (subjectRelation_type == null)
            {
                throw new KeyNotFoundException(string.Concat("Did not find relationship type for code: ", relType));
            }
            subjectRelation.Detail = new Dtos.GuidObject2(subjectRelation_type.Guid);

            var relativeRelation_type = relationTypes.FirstOrDefault(x => x.Code.Equals(subjectRelation_type.InverseRelType, StringComparison.OrdinalIgnoreCase));
            if (relativeRelation_type != null)
            {
                relativeRelation.Detail = new Dtos.GuidObject2(relativeRelation_type.Guid);
            }

            if (!string.IsNullOrEmpty(subjectRelation_type.Code))
            {
                try
                {
                    if (subjectRelation_type != null && !string.IsNullOrEmpty(subjectRelation_type.PersonRelType.ToString()))
                    {
                        subjectRelation.RelationshipType = GetRelationshipType(subjectGender, subjectRelation_type);
                    }
                    else
                    {
                        subjectRelation.RelationshipType = Dtos.PersonalRelationshipType.Other;
                    }

                    if (relativeRelation_type != null && !string.IsNullOrEmpty(relativeRelation_type.PersonRelType.ToString()))
                    {
                        relativeRelation.RelationshipType = GetRelationshipType(relativesGender, relativeRelation_type);
                    }
                    else
                    {
                        relativeRelation.RelationshipType = Dtos.PersonalRelationshipType.Other;
                    }
                }
                catch(Exception ex)
                {
                    logger.Error("Error occurred when converting relationship type: " + ex.Message);
                }
            }
            return new Tuple<Dtos.Relationship, Dtos.Relationship>(subjectRelation, relativeRelation);
        }

        /// <summary>
        /// Gets Dtos.PersonalRelationshipType
        /// </summary>
        /// <param name="gender"></param>
        /// <param name="entityRelationType"></param>
        /// <returns></returns>
        private Dtos.PersonalRelationshipType GetRelationshipType(string gender, RelationType entityRelationType)
        {
            Dtos.PersonalRelationshipType personalRelType = Dtos.PersonalRelationshipType.Other;
                 if ((gender != null ) && (gender.Equals("M", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(entityRelationType.MaleRelType.ToString())))
            {
                personalRelType = (Dtos.PersonalRelationshipType)Enum.Parse(typeof(Dtos.PersonalRelationshipType), entityRelationType.MaleRelType.ToString());
            }
            else if ((gender != null ) && (gender.Equals("F", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(entityRelationType.FemaleRelType.ToString())))
            {
                personalRelType = (Dtos.PersonalRelationshipType)Enum.Parse(typeof(Dtos.PersonalRelationshipType), entityRelationType.FemaleRelType.ToString());
            }
            else
            {
                if (!string.IsNullOrEmpty(entityRelationType.PersonRelType.ToString()))
                {
                    personalRelType = (Dtos.PersonalRelationshipType)Enum.Parse(typeof(Dtos.PersonalRelationshipType), entityRelationType.PersonRelType.ToString());
                }
            }
            return personalRelType;
        }

        /// <summary>
        /// Converts entity status 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private async Task<Dtos.GuidObject2> ConvertEntityStatusToDtoStatus(string status)
        {
            if (relationshipStatuses == null)
            { 
                relationshipStatuses = await _referenceDataRepository.GetRelationshipStatusesAsync(true);
            }
            var statusEntity = relationshipStatuses.FirstOrDefault(x => x.Code.Equals(status, StringComparison.OrdinalIgnoreCase));
            if (statusEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("Did not find relationship status for status code: ", status));
            }
            return new Dtos.GuidObject2(statusEntity.Guid);
        }


        #endregion

        #region v16.0.0 methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all person-relationships
        /// </summary>
        /// <returns>Collection of PersonalRelationships2 DTO objects</returns>

        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>, int>> GetPersonalRelationships2Async(int offset, int limit, string person = "", string relationshipType = "", string personFilterValue = "", bool bypassCache = false)
        {
            try
            {
                string personCode = string.Empty;
                string[] filterPersonIds = new List<string>().ToArray();


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
                            return new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(new List<Dtos.PersonalRelationships2>(), 0);
                        }
                    }

                    catch
                    {
                        return new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(new List<Dtos.PersonalRelationships2>(), 0);
                    }
                }

                if (!string.IsNullOrEmpty(person))
                {
                    try
                    {
                        personCode = await _personRepository.GetPersonIdFromGuidAsync(person);
                        if (string.IsNullOrEmpty(personCode))
                            return new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(new List<Dtos.PersonalRelationships2>(), 0);
                        else
                        {
                            Array.Resize(ref filterPersonIds, filterPersonIds.Length + 1);
                            filterPersonIds[filterPersonIds.Length - 1] = personCode;
                        }
                    }
                    catch
                    {
                        return new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(new List<Dtos.PersonalRelationships2>(), 0);
                    }
                }
                string relationshipTypeCode = string.Empty;
                string inverseRelationshipTypeCode = string.Empty;
                if (!string.IsNullOrEmpty(relationshipType))
                {
                    try
                    {
                        relationTypes = await GetRelationTypes2Async(bypassCache);
                        if (relationTypes != null && relationTypes.Any())
                        {
                            var relationshipTypeEntity = relationTypes.FirstOrDefault(x => x.Guid.Equals(relationshipType, StringComparison.OrdinalIgnoreCase));
                            if (relationshipTypeEntity == null)
                            {
                                return new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(new List<Dtos.PersonalRelationships2>(), 0);
                            }
                            relationshipTypeCode = relationshipTypeEntity.Code;
                            inverseRelationshipTypeCode = relationshipTypeEntity.InverseRelType;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(new List<Dtos.PersonalRelationships2>(), 0);
                        }
                    }
                    catch
                    {
                        return new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(new List<Dtos.PersonalRelationships2>(), 0);
                    }
                }
                var personRelationshipEntitiesTuple = await _relationshipRepository.GetRelationships2Async(offset, limit, filterPersonIds, relationshipTypeCode, inverseRelationshipTypeCode);
                if (personRelationshipEntitiesTuple != null)
                {
                    var personRelationshipEntities = personRelationshipEntitiesTuple.Item1.ToList();
                    var totalCount = personRelationshipEntitiesTuple.Item2;

                    if (personRelationshipEntities.Any())
                    {
                        List<Dtos.PersonalRelationships2> personRelationships = new List<Dtos.PersonalRelationships2>();

                        foreach (var personRelationshipEntity in personRelationshipEntities)
                        {
                            personRelationships.Add(await this.ConvertPersonalRelationships2EntityToDto(personRelationshipEntity));
                        }
                        if (IntegrationApiException != null)
                        {
                            throw IntegrationApiException;
                        }
                        return new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(personRelationships, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(new List<Dtos.PersonalRelationships2>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(new List<Dtos.PersonalRelationships2>(), 0);
            }
            catch (PermissionsException e)
            {
                throw new PermissionsException(e.Message);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

      

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PersonalRelationships2 from its GUID
        /// </summary>
        /// <returns>PersonalRelationships2 DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonalRelationships2> GetPersonalRelationships2ByGuidAsync(string guid, bool bypassCache = true)
        {
          
            try
            {
               var rela = await ConvertPersonalRelationships2EntityToDto(await _relationshipRepository.GetPersonalRelationshipById2Async(guid));
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return rela;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No personal relationship was found for guid " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No personal relationship was found for guid " + guid, ex);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Relationship domain entity to its corresponding PersonalRelationships2 DTO
        /// </summary>
        /// <param name="source">Relationship domain entity</param>
        /// <returns>PersonalRelationships2 DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PersonalRelationships2> ConvertPersonalRelationships2EntityToDto(Domain.Base.Entities.Relationship personRelationshipEntity)
        {
            if (personRelationshipEntity == null)
            {
                IntegrationApiExceptionAddError("Personal relationship enetity cannot be empty.", "Validation.Exception", string.Empty , string.Empty);
                throw IntegrationApiException;
            }
            Dtos.PersonalRelationships2 personRelationshipDto = new Dtos.PersonalRelationships2();
            if (!string.IsNullOrEmpty(personRelationshipEntity.Guid))
            {
                personRelationshipDto.Id = personRelationshipEntity.Guid;
            }
            else
            {
                IntegrationApiExceptionAddError("Unable to locate GUID", "GUID.Not.Found", personRelationshipEntity.Guid, personRelationshipEntity.Id);
            }

            if (!string.IsNullOrEmpty(personRelationshipEntity.SubjectPersonGuid))
                personRelationshipDto.SubjectPerson = new Dtos.GuidObject2(personRelationshipEntity.SubjectPersonGuid);
            else
            {
                IntegrationApiExceptionAddError("Unable to locate GUID for subject person Guid", "GUID.Not.Found", personRelationshipEntity.Guid, personRelationshipEntity.Id);
            }
            if (!string.IsNullOrEmpty(personRelationshipEntity.RelationPersonGuid))
            {
                var relatedPerson = new Dtos.PersonalRelationshipsRelatedPerson();
                relatedPerson.person = new Dtos.GuidObject2(personRelationshipEntity.RelationPersonGuid);
                personRelationshipDto.Related = relatedPerson;
            }
            else
            {
                IntegrationApiExceptionAddError("Unable to locate GUID for related person Guid", "GUID.Not.Found", personRelationshipEntity.Guid, personRelationshipEntity.Id);
            }
            if (!string.IsNullOrEmpty(personRelationshipEntity.Comment))
            {
                personRelationshipDto.Comment = personRelationshipEntity.Comment.Replace(Convert.ToChar(DynamicArray.VM), '\n')
                                                       .Replace(Convert.ToChar(DynamicArray.TM), ' ')
                                                       .Replace(Convert.ToChar(DynamicArray.SM), ' ');
            }
            personRelationshipDto.EndOn = personRelationshipEntity.EndDate;
            personRelationshipDto.StartOn = personRelationshipEntity.StartDate;
            if (!string.IsNullOrEmpty(personRelationshipEntity.Status))
            {
                try
                {
                    var status = await _referenceDataRepository.GetRelationshipStatusesGuidAsync(personRelationshipEntity.Status);
                    if (!string.IsNullOrEmpty(status))
                    {
                        personRelationshipDto.Status = new Dtos.GuidObject2(status);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError("Unable to locate GUID for relationship-statuses of " + personRelationshipEntity.Status, "GUID.Not.Found", personRelationshipEntity.Guid, personRelationshipEntity.Id);
                    }
                }
                catch
                {
                    IntegrationApiExceptionAddError("Unable to locate GUID for relationship-statuses of " + personRelationshipEntity.Status, "GUID.Not.Found", personRelationshipEntity.Guid, personRelationshipEntity.Id);
                }
            }

            if (!string.IsNullOrEmpty(personRelationshipEntity.RelationshipType))
            {
                try
                {
                    var subjectRelation_type = await _referenceDataRepository.GetRelationTypes3GuidAsync(personRelationshipEntity.RelationshipType);
                    if (subjectRelation_type != null)
                    {
                        if (!string.IsNullOrEmpty(subjectRelation_type.Item1))
                        {
                            personRelationshipDto.DirectRelationshipType = new Dtos.GuidObject2(subjectRelation_type.Item1);
                        }
                        if (!string.IsNullOrEmpty(subjectRelation_type.Item2))
                        {
                            personRelationshipDto.ReciprocalRelationshipType = new Dtos.GuidObject2(subjectRelation_type.Item2);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError("Unable to locate GUID for relationship-types of " + personRelationshipEntity.RelationshipType, "GUID.Not.Found", personRelationshipEntity.Guid, personRelationshipEntity.Id);
                    }
                }
                catch
                {
                    IntegrationApiExceptionAddError("Unable to locate GUID for relationship-types of " + personRelationshipEntity.RelationshipType, "GUID.Not.Found", personRelationshipEntity.Guid, personRelationshipEntity.Id);
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return personRelationshipDto;
        }

        /// <summary>
        /// Gets the relation types collection
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.Base.Entities.RelationType>> GetRelationTypes2Async(bool bypassCache)
        {
            if (relationTypes == null)
            {
                relationTypes = await _referenceDataRepository.GetRelationTypes3Async(bypassCache);
                if (relationTypes == null || !relationTypes.Any())
                {
                    throw new ArgumentNullException("PersonalRelationships", "Relationship types are missing");
                }
            }
            return relationTypes;
        }

        /// <summary>
        /// Gets the relation statuses  collection
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private IEnumerable<Domain.Base.Entities.RelationshipStatus> relationStatuses = null;
        private async Task<IEnumerable<Domain.Base.Entities.RelationshipStatus>> GetRelationshipStatusesAsync(bool bypassCache)
        {
            if (relationStatuses == null)
            {
                relationStatuses = await _referenceDataRepository.GetRelationshipStatusesAsync(bypassCache);               
            }
            return relationStatuses;
        }

        /// <summary>
        /// Update a PersonalRelationships2.
        /// </summary>
        /// <param name="PersonalRelationships2">The <see cref="PersonalRelationships2">personRelationships</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="PersonalRelationships2">personRelationships</see></returns>
        public async Task<Dtos.PersonalRelationships2> UpdatePersonalRelationships2Async(Dtos.PersonalRelationships2 personalRelationships)
        {
            try
            {
                // verify the user has the permission to update a personalRelationships
                //this.CheckCreatePersonalRelationshipsPermission();

                if (personalRelationships == null)
                {
                    IntegrationApiExceptionAddError("Must provide a personal relationships representation for update.", "Missing.Request.Body");
                    throw IntegrationApiException;
                }
                if (string.IsNullOrEmpty(personalRelationships.Id))
                {
                    IntegrationApiExceptionAddError("Must provide a personal relationships representation id for update.", "Missing.Request.ID");
                    throw IntegrationApiException;
                }
                //if the guid not a null guid. 
                if (personalRelationships.Id == Guid.Empty.ToString())
                {
                    IntegrationApiExceptionAddError("Must provide a non nil personal relationships id for update.", "Missing.Request.ID");
                    throw IntegrationApiException;
                }

                _relationshipRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                // get the ID associated with the incoming guid
                var personRelationshipsEntityId = await _relationshipRepository.GetPersonalRelationshipsIdFromGuidAsync(personalRelationships.Id);

                // map the DTO to entities
                var personRelationshipsEntity
                = await ConvertPersonalRelationships2DtoToEntityAsync(personRelationshipsEntityId, personalRelationships);

                // update the entity in the database
                var updatedPersonalRelationships2Entity =
                    await _relationshipRepository.UpdatePersonalRelationshipsAsync(personRelationshipsEntity);

                return await this.ConvertPersonalRelationships2EntityToDto(updatedPersonalRelationships2Entity);

            }
            catch (PermissionsException e)
            {
                throw new PermissionsException(e.Message);
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
            catch( IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }           
        }

        /// <summary>
        /// Create a PersonalRelationships2.
        /// </summary>
        /// <param name="personalRelationships">The <see cref="PersonalRelationships2">personRelationships</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PersonalRelationships2">personRelationships</see></returns>
        public async Task<Dtos.PersonalRelationships2> CreatePersonalRelationships2Async(Dtos.PersonalRelationships2 personalRelationships)
        {
            try
            {
                // verify the user has the permission to create a personalRelationships
                //this.CheckCreatePersonalRelationshipsPermission();
                if (personalRelationships == null)
                {
                    IntegrationApiExceptionAddError("Must provide a personal relationships representation for create.", "Missing.Request.Body");
                    throw IntegrationApiException;
                }
                 if (string.IsNullOrEmpty(personalRelationships.Id))
                {
                    IntegrationApiExceptionAddError("Must provide a personal relationships representation id for create.", "Missing.Request.ID");
                    throw IntegrationApiException;
                }
                //if the guid not a null guid. 
                if (personalRelationships.Id != Guid.Empty.ToString())
                {
                    IntegrationApiExceptionAddError("Must provide a nil personal relationships id for create.", "Missing.Request.ID");
                    throw IntegrationApiException;
                }
                _relationshipRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                var personRelationshipsEntity
                         = await ConvertPersonalRelationships2DtoToEntityAsync(string.Empty, personalRelationships);

                // create a personRelationships entity in the database
                var createdPersonalRelationships2 =
                    await _relationshipRepository.UpdatePersonalRelationshipsAsync(personRelationshipsEntity);
                // return the newly created personRelationships
                return await this.ConvertPersonalRelationships2EntityToDto(createdPersonalRelationships2);

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (PermissionsException e)
            {
                throw new PermissionsException(e.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        private async Task<Domain.Base.Entities.Relationship> ConvertPersonalRelationships2DtoToEntityAsync(string personRelationshipsId, Dtos.PersonalRelationships2 personRelationshipsDto, bool bypassCache = false)
        {
            if (personRelationshipsDto == null || string.IsNullOrEmpty(personRelationshipsDto.Id))
            {
                IntegrationApiExceptionAddError("Personal relationships body is required.", "Validation.Exception");
                throw IntegrationApiException;
            }
            else
            {
                var personId1 = string.Empty;
                var personId2 = string.Empty;
                var directRelationType = string.Empty;
                var reciRelationType = string.Empty;
                var inverseRelType = string.Empty;
                //get person id1
                if (personRelationshipsDto.SubjectPerson == null || string.IsNullOrEmpty(personRelationshipsDto.SubjectPerson.Id))
                {
                    IntegrationApiExceptionAddError("SubjectPerson.id is a required property.", "Missing.Required.Property", personRelationshipsId, personRelationshipsDto.Id);
                }
                else
                {
                    try
                    {
                        personId1 = await _personRepository.GetPersonIdFromGuidAsync(personRelationshipsDto.SubjectPerson.Id);
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError("SubjectPerson.id is not a valid GUID for persons.", "GUID.Not.Found", personRelationshipsId, personRelationshipsDto.Id);
                    }
                    if (String.IsNullOrEmpty(personId1))
                    {
                        IntegrationApiExceptionAddError("SubjectPerson.id is not a valid GUID for persons.", "GUID.Not.Found", personRelationshipsId, personRelationshipsDto.Id);
                    }
                }

                //get person id2
                if (personRelationshipsDto.Related == null || personRelationshipsDto.Related.person == null || string.IsNullOrEmpty(personRelationshipsDto.Related.person.Id))
                {
                    IntegrationApiExceptionAddError("Related.person.id is a required property.", "Missing.Required.Property", personRelationshipsId, personRelationshipsDto.Id);
                }
                else
                {
                    try
                    {
                        personId2 = await _personRepository.GetPersonIdFromGuidAsync(personRelationshipsDto.Related.person.Id);
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError("Related.person.id is not a valid GUID for persons.", "GUID.Not.Found", personRelationshipsId, personRelationshipsDto.Id);
                    }
                    if (String.IsNullOrEmpty(personId2))
                    {
                        IntegrationApiExceptionAddError("Related.person.id is not a valid GUID for persons.", "GUID.Not.Found", personRelationshipsId, personRelationshipsDto.Id);
                    }
                    else
                    {
                        if (personId1.Equals(personId2, StringComparison.OrdinalIgnoreCase))
                        {
                            IntegrationApiExceptionAddError("Subject person id cannot be the same as related person id. ", "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                        }
                    }
                }

                //convert and validate relationship types
                if (personRelationshipsDto.DirectRelationshipType == null || string.IsNullOrEmpty(personRelationshipsDto.DirectRelationshipType.Id))
                {
                    IntegrationApiExceptionAddError("DirectRelationshipType.id is a required property.", "Missing.Required.Property", personRelationshipsId, personRelationshipsDto.Id);
                }
                else
                {
                    List<Domain.Base.Entities.RelationType> relaTypes = null;
                    try
                    {
                        relaTypes = (await GetRelationTypes2Async(bypassCache)).ToList();
                    }
                    catch
                    { }
                    if (relaTypes == null || !relaTypes.Any())
                    {
                        IntegrationApiExceptionAddError("Relationship types not found.", "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                    }
                    else
                    {
                        var subjectRelation_type = relaTypes.FirstOrDefault(x => x.Guid.Equals(personRelationshipsDto.DirectRelationshipType.Id, StringComparison.OrdinalIgnoreCase));
                        if (subjectRelation_type == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Relationship types not found for guid {0}.", personRelationshipsDto.DirectRelationshipType.Id), "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                        }
                        else
                        {
                            //check to make sure the directRelationshipType does not have the organization only indicator set to Yes
                            if (subjectRelation_type.OrgIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase))
                            {
                                IntegrationApiExceptionAddError(string.Concat("The relationship type '", subjectRelation_type.Code, "' is for relationships involving nonpersons only. "), "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                            }
                            else
                            {
                                directRelationType = subjectRelation_type.Code;
                                reciRelationType = subjectRelation_type.InverseRelType;
                            }
                            //validate startOn and endOn
                            if (personRelationshipsDto.StartOn.HasValue)
                            {
                                if ((subjectRelation_type.Category.Equals("S", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("P", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("C", StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (personRelationshipsDto.StartOn > DateTime.Today)
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("Starton cannot be in the future for this type of relationship."), "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                                    }
                                }
                            }
                            if (personRelationshipsDto.EndOn.HasValue)
                            {
                                if ((subjectRelation_type.Category.Equals("S", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("P", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("C", StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (personRelationshipsDto.EndOn > DateTime.Today)
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("Endon cannot be in the future for this type of relationship."), "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                                    }
                                }
                            }
                        }
                    }
                }

                //if there is a reciprocal relationship
                if (personRelationshipsDto.ReciprocalRelationshipType != null)
                {
                    if (string.IsNullOrEmpty(personRelationshipsDto.ReciprocalRelationshipType.Id))
                    {
                        IntegrationApiExceptionAddError("ReciprocalRelationshipType.id is required with reciprocalRelationshipType is in the payload.", "Missing.Required.Property", personRelationshipsId, personRelationshipsDto.Id);
                    }
                    else
                    {
                        List<Domain.Base.Entities.RelationType> relaTypes = null;
                        try
                        {
                            relaTypes = (await GetRelationTypes2Async(bypassCache)).ToList();
                        }
                        catch
                        { }
                        if (relaTypes == null || !relaTypes.Any())
                        {
                            IntegrationApiExceptionAddError("Relationship types not found.", "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                        }
                        else
                        {
                            var relRelation_type = relaTypes.FirstOrDefault(x => x.Guid.Equals(personRelationshipsDto.ReciprocalRelationshipType.Id, StringComparison.OrdinalIgnoreCase));
                            if (relRelation_type == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Relationship types not found for guid {0}.", personRelationshipsDto.ReciprocalRelationshipType.Id), "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                            }
                            else
                            {
                                //just do this if you have direct relationship type
                                if (!string.IsNullOrEmpty(directRelationType))
                                {
                                    //check to make sure the relationship type has inverse relationship
                                    if (string.IsNullOrEmpty(reciRelationType))
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("The direct relationship type ", directRelationType, " does not have a inverse relationship type. "), "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                                    }
                                    else if (!reciRelationType.Equals(relRelation_type.Code, StringComparison.OrdinalIgnoreCase))
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("The inverse relationship type of ", reciRelationType, " does not match the reciprocal relationship type of ", relRelation_type.Code, "."), personRelationshipsId, personRelationshipsDto.Id);
                                    }
                                    inverseRelType = relRelation_type.Code;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(reciRelationType))
                    {
                        inverseRelType = reciRelationType;
                    }
                }

                if (personRelationshipsDto.StartOn.HasValue && personRelationshipsDto.EndOn.HasValue && personRelationshipsDto.StartOn > personRelationshipsDto.EndOn)
                {
                    IntegrationApiExceptionAddError("StartOn date cannot be greater than the endOn date.", personRelationshipsId, personRelationshipsDto.Id);
                }

                // Throw errors
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                //since all the validation is done. create the entity.
                Domain.Base.Entities.Relationship personalRelaEntity = null;
                try
                {
                    personalRelaEntity = new Domain.Base.Entities.Relationship(personRelationshipsId, personId1, personId2, directRelationType, inverseRelType, true, personRelationshipsDto.StartOn, personRelationshipsDto.EndOn);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Missing.Required.Property", personRelationshipsId, personRelationshipsDto.Id);
                    throw IntegrationApiException;
                }
                personalRelaEntity.Guid = personRelationshipsDto.Id;
                personalRelaEntity.SubjectPersonGuid = personRelationshipsDto.SubjectPerson.Id;
                personalRelaEntity.RelationPersonGuid = personRelationshipsDto.Related.person.Id;

                //validate status
                if (personRelationshipsDto.Status != null && !string.IsNullOrEmpty(personRelationshipsDto.Status.Id))
                {

                    var statuses = await GetRelationshipStatusesAsync(bypassCache);
                    if (statuses == null || !statuses.Any())
                    {
                        IntegrationApiExceptionAddError("Relationship statuses not found.", "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                    }
                    else
                    {
                        var statusEntity = statuses.FirstOrDefault(x => x.Guid.Equals(personRelationshipsDto.Status.Id, StringComparison.OrdinalIgnoreCase));
                        if (statusEntity == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Relationship statuses not found for guid {0}.", personRelationshipsDto.Status.Id), "Validation.Exception", personRelationshipsId, personRelationshipsDto.Id);
                        }
                        else
                        {
                            personalRelaEntity.Status = statusEntity.Code;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(personRelationshipsDto.Comment))
                {
                    personalRelaEntity.Comment = personRelationshipsDto.Comment.Replace(Convert.ToChar(DynamicArray.VM), '\n')
                                                   .Replace(Convert.ToChar(DynamicArray.TM), ' ')
                                                   .Replace(Convert.ToChar(DynamicArray.SM), ' '); ;
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return personalRelaEntity;
            }
        }

        /// <summary>
        /// Delete a person relationship from the database
        /// </summary>
        /// <param name="id">The requested personrelationship GUID</param>
        /// <returns></returns>
        public async Task DeletePersonalRelationshipsAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Must provide a person relationship guid for deletion.");
            }
            //CheckDeletePersonalRelationships2Permission();
            try
            {
                var relationship = await _relationshipRepository.GetPersonalRelationshipById2Async(guid);
                if (relationship == null)
                    throw new KeyNotFoundException("No personal relationship was found for guid " + guid);
                await _relationshipRepository.DeletePersonRelationshipAsync(relationship.Id);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No personal relationship was found for guid " + guid);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// Create a PersonalRelationshipInitiationProcess.
        /// </summary>
        /// <param name="personalRelationships">The <see cref="PersonalRelationshipInitiationProcess">personRelationships</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PersonalRelationshipInitiationProcess">personRelationships</see></returns>
        public async Task<object> CreatePersonalRelationshipInitiationProcessAsync(Dtos.PersonalRelationshipInitiationProcess personalRelationships)
        {
            try
            {
                if (personalRelationships == null)
                {
                    IntegrationApiExceptionAddError("Must provide a personal-relationship-initiation-process object for create.", "Missing.Request.Body");
                    throw IntegrationApiException;
                }
                _relationshipRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                var personRelationshipsEntity
                         = await ConvertPersonalRelationshipInitiationProcessDtoToEntityAsync(personalRelationships);

                // create a personRelationships entity in the database
                var createdPersonalRelationship =
                    await _relationshipRepository.CreatePersonalRelationshipInitiationProcessAsync(personRelationshipsEntity);

                if (createdPersonalRelationship == null)
                {
                    IntegrationApiExceptionAddError("An unexpected error has occurred. Unable to build response.", "Global.Internal.Error");
                    throw IntegrationApiException;
                }

                if (createdPersonalRelationship.Item1 != null && (createdPersonalRelationship.Item1.GetType()) == typeof(Relationship))
                {
                    // return the newly created personRelationships
                    return await ConvertPersonalRelationships2EntityToDto((Relationship)createdPersonalRelationship.Item1);
                }
                else if (!string.IsNullOrEmpty(createdPersonalRelationship.Item2))
                {
                    // return the newly created PersonMatchingRequests
                    var personMatchingRequest = await _personMatchingRequestsRepository.GetPersonMatchRequestsByIdAsync(createdPersonalRelationship.Item2);
                    if (personMatchingRequest == null)
                    {
                        IntegrationApiExceptionAddError("Unable to locate person-match-request for id '" + createdPersonalRelationship.Item2  + "'.", "Bad.Data");
                        throw IntegrationApiException;
                    }

                    Dictionary<string, string> personDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { personMatchingRequest.PersonId });
                    return ConvertPersonMatchRequestEntityToDtoAsync(personMatchingRequest, personDict);
                }
                else
                {
                    IntegrationApiExceptionAddError("An unexpected error has occurred. Unable to build response.", "Global.Internal.Error");
                    throw IntegrationApiException;
                }
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (PermissionsException e)
            {
                throw new PermissionsException(e.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        private async Task<PersonalRelationshipInitiation> ConvertPersonalRelationshipInitiationProcessDtoToEntityAsync(Dtos.PersonalRelationshipInitiationProcess personRelationshipsDto, bool bypassCache = false)
        {
            if (personRelationshipsDto == null)
            {
                IntegrationApiExceptionAddError("Personal relationships body is required.", "Validation.Exception");
                throw IntegrationApiException;
            }
            else
            {
                var personId1 = string.Empty;
                var personId2 = string.Empty;
                var directRelationType = string.Empty;
                var reciRelationType = string.Empty;
                var inverseRelType = string.Empty;
                //get person id1
                if (personRelationshipsDto.SubjectPerson == null || string.IsNullOrEmpty(personRelationshipsDto.SubjectPerson.Id))
                {
                    IntegrationApiExceptionAddError("SubjectPerson.id is a required property.", "Missing.Required.Property");
                }
                else
                {
                    try
                    {
                        personId1 = await _personRepository.GetPersonIdFromGuidAsync(personRelationshipsDto.SubjectPerson.Id);
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError("SubjectPerson.id is not a valid GUID for persons.", "GUID.Not.Found");
                    }
                    if (String.IsNullOrEmpty(personId1))
                    {
                        IntegrationApiExceptionAddError("SubjectPerson.id is not a valid GUID for persons.", "GUID.Not.Found");
                    }
                }

                //get person id2
                if (personRelationshipsDto.Related != null && personRelationshipsDto.Related.Person != null && !string.IsNullOrEmpty(personRelationshipsDto.Related.Person.Id))
                {
                    try
                    {
                        personId2 = await _personRepository.GetPersonIdFromGuidAsync(personRelationshipsDto.Related.Person.Id);
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError("related.person.id is not a valid GUID for persons.", "GUID.Not.Found");
                    }
                    if (String.IsNullOrEmpty(personId2))
                    {
                        IntegrationApiExceptionAddError("related.person.id is not a valid GUID for persons.", "GUID.Not.Found");
                    }
                    else
                    {
                        if (personId1.Equals(personId2, StringComparison.OrdinalIgnoreCase))
                        {
                            IntegrationApiExceptionAddError("Subject person id cannot be the same as related person id. ", "Validation.Exception");
                        }
                    }
                }

                //convert and validate relationship types
                if (personRelationshipsDto.RelationshipType == null || string.IsNullOrEmpty(personRelationshipsDto.RelationshipType.Id))
                {
                    IntegrationApiExceptionAddError("relationshipType.id is a required property.", "Missing.Required.Property");
                }
                else
                {
                    List<Domain.Base.Entities.RelationType> relaTypes = null;
                    try
                    {
                        relaTypes = (await GetRelationTypes2Async(bypassCache)).ToList();
                    }
                    catch
                    { }
                    if (relaTypes == null || !relaTypes.Any())
                    {
                        IntegrationApiExceptionAddError("Relationship types not found.", "Validation.Exception");
                    }
                    else
                    {
                        var subjectRelation_type = relaTypes.FirstOrDefault(x => x.Guid.Equals(personRelationshipsDto.RelationshipType.Id, StringComparison.OrdinalIgnoreCase));
                        if (subjectRelation_type == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Relationship types not found for guid {0}.", personRelationshipsDto.RelationshipType.Id), "Validation.Exception");
                        }
                        else
                        {
                            //check to make sure the directRelationshipType does not have the organization only indicator set to Yes
                            if (subjectRelation_type.OrgIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase))
                            {
                                IntegrationApiExceptionAddError(string.Concat("The relationship type '", subjectRelation_type.Code, "' is for relationships involving nonpersons only. "), "Validation.Exception");
                            }
                            else
                            {
                                directRelationType = subjectRelation_type.Code;
                                reciRelationType = subjectRelation_type.InverseRelType;
                            }

                            //validate startOn and endOn
                            if (personRelationshipsDto.StartOn != null && personRelationshipsDto.StartOn.HasValue)
                            {
                                if ((subjectRelation_type.Category.Equals("S", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("P", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("C", StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (personRelationshipsDto.StartOn > DateTime.Today)
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("Start on date cannot be in the future for this type of relationship."), "Validation.Exception");
                                    }
                                }
                            }
                            if (personRelationshipsDto.EndOn != null && personRelationshipsDto.EndOn.HasValue)
                            {
                                if ((subjectRelation_type.Category.Equals("S", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("P", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("C", StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (personRelationshipsDto.EndOn > DateTime.Today)
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("End on date cannot be in the future for this type of relationship."), "Validation.Exception");
                                    }
                                }
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(reciRelationType))
                {
                    inverseRelType = reciRelationType;
                }

                if (personRelationshipsDto.StartOn != null && personRelationshipsDto.EndOn != null && personRelationshipsDto.StartOn.HasValue && personRelationshipsDto.EndOn.HasValue && personRelationshipsDto.StartOn > personRelationshipsDto.EndOn)
                {
                    IntegrationApiExceptionAddError("Start on date cannot be greater than the end on date.");
                }

                //since all the validation is done. create the entity.
                PersonalRelationshipInitiation personalRelaEntity = new PersonalRelationshipInitiation();
                personalRelaEntity.PersonId = personId1;
                personalRelaEntity.RelatedPersonId = personId2;
                if (personRelationshipsDto.SubjectPerson != null && !string.IsNullOrEmpty(personRelationshipsDto.SubjectPerson.Id))
                    personalRelaEntity.SubjectPersonGuid = personRelationshipsDto.SubjectPerson.Id;
                if (personRelationshipsDto.Related != null && personRelationshipsDto.Related.Person != null)
                    personalRelaEntity.RelatedPersonGuid = personRelationshipsDto.Related.Person.Id;
                personalRelaEntity.RelationshipType = directRelationType;
                personalRelaEntity.InverseRelationshipType = inverseRelType;
                personalRelaEntity.StartDate = personRelationshipsDto.StartOn;
                personalRelaEntity.EndDate = personRelationshipsDto.EndOn;
                personalRelaEntity.Comment = personRelationshipsDto.Comment;

                //validate status
                if (personRelationshipsDto.Status != null)
                {
                    if (string.IsNullOrEmpty(personRelationshipsDto.Status.Id))
                    {
                        IntegrationApiExceptionAddError("The status.id is required when status is provided", "Missing.Required.Property");
                    }
                    else
                    {
                        var statuses = await GetRelationshipStatusesAsync(bypassCache);
                        if (statuses == null || !statuses.Any())
                        {
                            IntegrationApiExceptionAddError("Relationship statuses not found.", "Validation.Exception");
                        }
                        else
                        {
                            var statusEntity = statuses.FirstOrDefault(x => x.Guid.Equals(personRelationshipsDto.Status.Id, StringComparison.OrdinalIgnoreCase));
                            if (statusEntity == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Relationship statuses not found for guid {0}.", personRelationshipsDto.Status.Id), "Validation.Exception");
                            }
                            else
                            {
                                personalRelaEntity.Status = statusEntity.Code;
                            }
                        }
                    }
                }

                // Person Match Request Data
                personalRelaEntity.RequestType = "RELATION";
                
                if (personRelationshipsDto.Related != null && (personRelationshipsDto.Related.Person == null || string.IsNullOrEmpty(personRelationshipsDto.Related.Person.Id)))
                {
                    if (string.IsNullOrEmpty(personRelationshipsDto.Related.LastName) || string.IsNullOrEmpty(personRelationshipsDto.Related.FirstName))
                    {
                        IntegrationApiExceptionAddError("The properties related.firstName and related.lastName are required.", "Missing.Required.Property", personRelationshipsDto.Id);
                    }
                    personalRelaEntity.LastName = personRelationshipsDto.Related.LastName;
                    personalRelaEntity.FirstName = personRelationshipsDto.Related.FirstName;
                    personalRelaEntity.MiddleName = personRelationshipsDto.Related.MiddleName;
                }

                if (personRelationshipsDto.Related == null)
                {
                    IntegrationApiExceptionAddError("The properties related.person.id or related.firstName and related.lastName are required.", "Missing.Required.Property", personRelationshipsDto.Id);
                }
                else
                {
                    // Matching Criteria
                    var related = personRelationshipsDto.Related;

                    // Date of Birth
                    if (related.DateOfBirth != null)
                    {
                        personalRelaEntity.BirthDate = related.DateOfBirth;
                    }

                    // Address Data
                    if (related.Address != null)
                    {
                        if (related.Address.Type != null && !string.IsNullOrEmpty(related.Address.Type.Id))
                        {
                            var addressTypeEntity = (await GetAddressTypes2Async(false)).FirstOrDefault(at => at.Guid.Equals(related.Address.Type.Id, StringComparison.InvariantCultureIgnoreCase));
                            if (addressTypeEntity != null)
                            {
                                personalRelaEntity.AddressType = addressTypeEntity.Code;
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("Could not determine Address Type for Address.", "Validation.Exception", personRelationshipsDto.Id);
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError("The property address.type.id is required when an address is specified.", "Missing.Required.Property", personRelationshipsDto.Id);
                        }

                        if (related.Address.AddressLines == null || !related.Address.AddressLines.Any())
                        {
                            IntegrationApiExceptionAddError("The property address.addressLines is required when an address is specified.", "Missing.Required.Property", personRelationshipsDto.Id);
                        }
                        personalRelaEntity.AddressLines = related.Address.AddressLines;

                        if (related.Address.Place != null)
                        {
                            var addressDtoAddress = related.Address;
                            var addressCountry = new Dtos.AddressCountry();
                            if (addressDtoAddress.Place != null && addressDtoAddress.Place.Country != null && !string.IsNullOrEmpty(addressDtoAddress.Place.Country.Code.ToString()))
                            {
                                addressCountry = addressDtoAddress.Place.Country;
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("A country code is required for an address with a place defined.", "Validation.Exception", personRelationshipsDto.Id);
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
                                IntegrationApiExceptionAddError(string.Concat("Unable to locate country code '", addressCountry.Code, "'"), "Validation.Exception", personRelationshipsDto.Id);
                            }
                            else
                            {
                                personalRelaEntity.Country = country.Code;
                                switch (addressCountry.Code)
                                {
                                    case Dtos.EnumProperties.IsoCode.USA:
                                        personalRelaEntity.CorrectionDigit = string.IsNullOrEmpty(addressCountry.CorrectionDigit) ? null : addressCountry.CorrectionDigit;
                                        personalRelaEntity.CarrierRoute = string.IsNullOrEmpty(addressCountry.CarrierRoute) ? null : addressCountry.CarrierRoute;
                                        personalRelaEntity.DeliveryPoint = string.IsNullOrEmpty(addressCountry.DeliveryPoint) ? null : addressCountry.DeliveryPoint;
                                        break;
                                    default:
                                        if (!string.IsNullOrEmpty(addressCountry.CorrectionDigit) || !string.IsNullOrEmpty(addressCountry.CarrierRoute) || !string.IsNullOrEmpty(addressCountry.DeliveryPoint))
                                        {
                                            IntegrationApiExceptionAddError("correctionDigit, carrierRoute and deliveryPoint can only be specified when code is 'USA'.", "Validation.Exception", personRelationshipsDto.Id);
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
                                        personalRelaEntity.State = states.Code;
                                    }
                                    else
                                    {
                                        //throw an execption if the state is not valid for US or Canada. 
                                        IntegrationApiExceptionAddError(string.Format("'{0}' is not defined as a state or province. ", state), "Validation.Exception", personRelationshipsDto.Id);
                                    }
                                }
                                else
                                {
                                    personalRelaEntity.Region = addressCountry.Region == null
                                        ? null
                                        : addressCountry.Region.Code;
                                }
                            }

                            if (addressCountry.SubRegion != null && !string.IsNullOrEmpty(addressCountry.SubRegion.Code))
                            {
                                var county = (await GetCountiesAsync(false)).FirstOrDefault(c => c.Code == addressCountry.SubRegion.Code);
                                if (county != null)
                                    personalRelaEntity.County = county.Code;
                                else
                                    personalRelaEntity.SubRegion = addressCountry.SubRegion == null ? null : addressCountry.SubRegion.Code;
                            }

                            personalRelaEntity.City = string.IsNullOrEmpty(addressCountry.Locality) ? null : addressCountry.Locality;
                            personalRelaEntity.Zip = string.IsNullOrEmpty(addressCountry.PostalCode) ? null : addressCountry.PostalCode;
                            personalRelaEntity.PostalCode = string.IsNullOrEmpty(addressCountry.PostalCode) ? null : addressCountry.PostalCode;
                            personalRelaEntity.Locality = addressCountry.Locality;
                            personalRelaEntity.Region = addressCountry.Region == null ? null : addressCountry.Region.Code;
                            personalRelaEntity.SubRegion = addressCountry.SubRegion == null ? null : addressCountry.SubRegion.Code;
                        }
                    }

                    // Phone Number
                    if (related.Phone != null)
                    {
                        if (!string.IsNullOrEmpty(related.Phone.Number))
                        {
                            personalRelaEntity.Phone = related.Phone.Number;

                            if (related.Phone.Type != null && !string.IsNullOrEmpty(related.Phone.Type.Id))
                            {
                                var phoneTypeEntity = (await GetPhoneTypesAsync(false)).FirstOrDefault(cte => cte.Guid.Equals(related.Phone.Type.Id, StringComparison.InvariantCultureIgnoreCase));
                                if (phoneTypeEntity != null && !string.IsNullOrEmpty(phoneTypeEntity.Code))
                                {
                                    personalRelaEntity.PhoneType = phoneTypeEntity.Code;
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError("Could not determine Phone Type for phone number.", "Validation.Exception", personRelationshipsDto.Id);
                                }
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("The property phone.type.id is required when a phone number is specified.", "Missing.Required.Property", personRelationshipsDto.Id);
                            }
                        }
                    }

                    // email addresses
                    if (related.Email != null)
                    {
                        personalRelaEntity.Email = related.Email.Address;
                        if (!string.IsNullOrEmpty(related.Email.Address))
                        {
                            if (related.Email.Type != null && !string.IsNullOrEmpty(related.Email.Type.Id))
                            {
                                var emailTypeEntity = (await GetEmailTypesAsync(false)).FirstOrDefault(et => et.Guid.Equals(related.Email.Type.Id, StringComparison.InvariantCultureIgnoreCase));
                                if (emailTypeEntity != null)
                                {
                                    personalRelaEntity.EmailType = emailTypeEntity.Code;
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError("Could not determine Email Type for Email Address.", "Validation.Exception", personRelationshipsDto.Id);
                                }
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("The property email.type.id is required when an email address is specified.", "Missing.Required.Property", personRelationshipsDto.Id);
                            }
                        }
                    }
                }

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return personalRelaEntity;
            }
        }
        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="personDict"></param>
        /// <returns></returns>
        private Dtos.PersonMatchingRequests ConvertPersonMatchRequestEntityToDtoAsync(PersonMatchRequest source, Dictionary<string, string> personDict)
        {
            Dtos.PersonMatchingRequests dto = new Dtos.PersonMatchingRequests();

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
                if (personDict == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for person ID '{0}'", source.RecordKey), "Bad.Data", source.Guid, source.RecordKey);
                }
                else if (!personDict.TryGetValue(source.PersonId, out personGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for person ID '{0}'", source.RecordKey), "Bad.Data", source.Guid, source.RecordKey);
                    dto.Person = new Dtos.GuidObject2(personGuid);
                }
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
                                dto.Outcomes = new List<Dtos.PersonMatchingRequestsOutcomesDtoProperty>();
                            }
                            dto.Outcomes.Add(new Dtos.PersonMatchingRequestsOutcomesDtoProperty()
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

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return dto;
        }
    }

    #endregion
}

