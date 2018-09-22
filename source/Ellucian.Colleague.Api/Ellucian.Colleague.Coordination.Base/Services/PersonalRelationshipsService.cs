// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

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
    /// <summary>
    /// Personal relationships
    /// </summary>
    [RegisterType]
    public class PersonalRelationshipsService : BaseCoordinationService, IPersonalRelationshipsService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IConfigurationRepository _configurationRepository;

        private IEnumerable<RelationType> relationTypes;
        private IEnumerable<Domain.Base.Entities.PersonalRelationshipStatus> relationshipStatuses;
        private List<string> defaultGuardianRelationships;

        public PersonalRelationshipsService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IRelationshipRepository relationshipRepository,
                                            IPersonRepository personRepository, IConfigurationRepository configurationRepository,
                                            ICurrentUserFactory currentUserFactory,
                                            IRoleRepository roleRepository,
                                            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
            _relationshipRepository = relationshipRepository;
            _configurationRepository = configurationRepository;
        }

        #region GET
        /// <summary>
        /// Gets all the personal relationships
        /// </summary>
        /// <returns>IEnumerable<Dtos.PersonalRelationship></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonalRelationship>, int>> GetAllPersonalRelationshipsAsync(int offset, int limit, bool bypassCache)
        {
            CheckUserPersonalRelationshipViewPermissions();
          
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
            CheckUserPersonalRelationshipViewPermissions();

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
            CheckUserPersonalRelationshipViewPermissions();

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

        #region PUT
        /// <summary>
        /// Updates personal relationship
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personalRelationship"></param>
        /// <returns></returns>
        public Task<Dtos.PersonalRelationship> UpdatePersonalRelationshipAsync(string id, Dtos.PersonalRelationship personalRelationship)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region POST
        /// <summary>
        /// Creates new personal relationship
        /// </summary>
        /// <param name="personalRelationship"></param>
        /// <returns></returns>
        public Task<Dtos.PersonalRelationship> CreatePersonalRelationshipAsync(Dtos.PersonalRelationship personalRelationship)
        {
            throw new NotImplementedException();
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
            if (gender.Equals("M", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(entityRelationType.MaleRelType.ToString()))
            {
                personalRelType = (Dtos.PersonalRelationshipType)Enum.Parse(typeof(Dtos.PersonalRelationshipType), entityRelationType.MaleRelType.ToString());
            }
            else if (gender.Equals("F", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(entityRelationType.FemaleRelType.ToString()))
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
                relationshipStatuses = await _referenceDataRepository.GetPersonalRelationshipStatusesAsync(true);
            }
            var statusEntity = relationshipStatuses.FirstOrDefault(x => x.Code.Equals(status, StringComparison.OrdinalIgnoreCase));
            if (statusEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("Did not find relationship status for status code: ", status));
            }
            return new Dtos.GuidObject2(statusEntity.Guid);
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view a person.
        /// </summary>
        private void CheckUserPersonalRelationshipViewPermissions()
        {
            // access is ok if the current user has the view personal relationships permission
            if (!HasPermission(BasePermissionCodes.ViewAnyRelationship))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view personal-relationships.");
                throw new PermissionsException("User is not authorized to view personal-relationships.");
            }
        }

        #endregion
    }
}
