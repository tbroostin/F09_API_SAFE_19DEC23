//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class PersonRelationshipsService : BaseCoordinationService, IPersonRelationshipsService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IConfigurationRepository _configurationRepository;

        private IEnumerable<Domain.Base.Entities.RelationType> relationTypes;
        private IEnumerable<Domain.Base.Entities.RelationshipStatus> relationshipStatuses;

        public PersonRelationshipsService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IRelationshipRepository relationshipRepository,
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all person-relationships
        /// </summary>
        /// <returns>Collection of PersonRelationships DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonRelationships>, int>> GetPersonRelationshipsAsync(int offset, int limit, string person = "", string relationshipType = "", bool bypassCache = false)
        {
            try
            {

                CheckGetPersonRelationshipPermission();
                string personCode = string.Empty;
                if (!string.IsNullOrEmpty(person))
                {
                    personCode = await _personRepository.GetPersonIdFromGuidAsync(person);
                    if (string.IsNullOrEmpty(personCode))
                        throw new ArgumentException("Invalid Person '" + person + "' in the arguments");
                }

                string relationshipTypeCode = string.Empty;
                string inverseRelationshipTypeCode = string.Empty;
                if (!string.IsNullOrEmpty(relationshipType))
                {
                    if (relationTypes == null)
                        relationTypes = await this.GetRelationTypesAsync(bypassCache);
                    var relationshipTypeEntity = relationTypes.FirstOrDefault(x => x.Guid.Equals(relationshipType, StringComparison.OrdinalIgnoreCase));
                    if (relationshipTypeEntity == null)
                    {
                        throw new ArgumentException("Invalid relationshipType '" + relationshipType + "' in the arguments");
                    }
                    relationshipTypeCode = relationshipTypeEntity.Code;
                    inverseRelationshipTypeCode = relationshipTypeEntity.InverseRelType;
                }
                var personRelationshipEntitiesTuple = await _relationshipRepository.GetRelationships2Async(offset, limit, personCode, relationshipTypeCode, inverseRelationshipTypeCode);
                if (personRelationshipEntitiesTuple != null)
                {
                    var personRelationshipEntities = personRelationshipEntitiesTuple.Item1.ToList();
                    var totalCount = personRelationshipEntitiesTuple.Item2;

                    if (personRelationshipEntities.Any())
                    {
                        List<Dtos.PersonRelationships> personRelationships = new List<Dtos.PersonRelationships>();

                        foreach (var personRelationshipEntity in personRelationshipEntities)
                        {
                            personRelationships.Add(await this.ConvertPersonRelationshipsEntityToDto(personRelationshipEntity, bypassCache));
                        }
                        return new Tuple<IEnumerable<Dtos.PersonRelationships>, int>(personRelationships, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.PersonRelationships>, int>(new List<Dtos.PersonRelationships>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.PersonRelationships>, int>(new List<Dtos.PersonRelationships>(), 0);
            }
            catch (PermissionsException e)
            {
                throw new PermissionsException(e.Message);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

         /// <summary>
        /// Helper method to determine if the user has permission to view person relationship.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetPersonRelationshipPermission()
        {
            var hasPermission = HasPermission(BasePermissionCodes.ViewAnyPersonRelationship);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view person relationships.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PersonRelationships from its GUID
        /// </summary>
        /// <returns>PersonRelationships DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonRelationships> GetPersonRelationshipsByGuidAsync(string guid, bool bypassCache = true)
        {
            CheckGetPersonRelationshipPermission();
            try
            {
                return await ConvertPersonRelationshipsEntityToDto(await _relationshipRepository.GetPersonRelationshipById2Async(guid), bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("person-relationships not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("person-relationships not found for GUID " + guid, ex);
            }
        }

       /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Relationship domain entity to its corresponding PersonRelationships DTO
        /// </summary>
        /// <param name="source">Relationship domain entity</param>
        /// <returns>PersonRelationships DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PersonRelationships> ConvertPersonRelationshipsEntityToDto(Domain.Base.Entities.Relationship personRelationshipEntity, bool bypassCache = false)
        {
            Dtos.PersonRelationships personRelationshipDto = new Dtos.PersonRelationships();
            personRelationshipDto.Id = personRelationshipEntity.Guid;
            if (!string.IsNullOrEmpty(personRelationshipEntity.SubjectPersonGuid))
            personRelationshipDto.SubjectPerson = new Dtos.GuidObject2(personRelationshipEntity.SubjectPersonGuid);
            if (!string.IsNullOrEmpty(personRelationshipEntity.RelationPersonGuid))
            {
                var relatedPerson = new Dtos.PersonRelationshipsRelatedPerson();
                relatedPerson.person = new Dtos.GuidObject2(personRelationshipEntity.RelationPersonGuid);
                personRelationshipDto.Related = relatedPerson;
            }
            if (!string.IsNullOrEmpty(personRelationshipEntity.Comment))
            {
                personRelationshipDto.Comment = personRelationshipEntity.Comment.Replace(Convert.ToChar(DynamicArray.VM), '\n')
                                                       .Replace(Convert.ToChar(DynamicArray.TM), ' ')
                                                       .Replace(Convert.ToChar(DynamicArray.SM), ' '); 
            }
            personRelationshipDto.EndOn = personRelationshipEntity.EndDate;
            personRelationshipDto.StartOn = personRelationshipEntity.StartDate;
            personRelationshipDto.Status = string.IsNullOrEmpty(personRelationshipEntity.Status) ?
                null :
                await ConvertEntityStatusToDtoStatus(personRelationshipEntity.Status, bypassCache);
            if (personRelationshipEntity.RelationshipType != null)
            {
                Tuple<Dtos.GuidObject2, Dtos.GuidObject2> relationships = await ConvertEntityRelationshipTypesToDtoRelationshipTypes
                    (personRelationshipEntity.RelationshipType, bypassCache);
                if (relationships != null)
                {
                    if (relationships.Item1 != null)
                        personRelationshipDto.DirectRelationshipType = relationships.Item1;
                    if (relationships.Item2 != null)
                        personRelationshipDto.ReciprocalRelationshipType = relationships.Item2;
                }
            }
            return personRelationshipDto;
        }

        /// <summary>
        /// Gets the relation types collection
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.Base.Entities.RelationType>> GetRelationTypesAsync(bool bypassCache)
        {
            if (relationTypes == null)
            {
                relationTypes = await _referenceDataRepository.GetRelationTypes3Async(bypassCache);
                if (relationTypes == null || !relationTypes.Any())
                {
                    throw new ArgumentNullException("PersonRelationships", "Relationship types are missing");
                }
            }
            return relationTypes;
        }

        /// <summary>
        /// Converts entity status 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private async Task<Dtos.GuidObject2> ConvertEntityStatusToDtoStatus(string status, bool bypassCache)
        {
            if (relationshipStatuses == null)
            {
                relationshipStatuses = await _referenceDataRepository.GetRelationshipStatusesAsync(bypassCache);
                if (relationshipStatuses == null || !relationshipStatuses.Any())
                {
                    throw new ArgumentNullException("PersonRelationships", "Relationship statuses are missing");
                }
            }
            var statusEntity = relationshipStatuses.FirstOrDefault(x => x.Code.Equals(status, StringComparison.OrdinalIgnoreCase));
            if (statusEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("Did not find relationship status Id for status code: ", status));
            }
            return new Dtos.GuidObject2(statusEntity.Guid);
        }

        /// <summary>
        /// Converts enity to dto relationships
        /// </summary>
        /// <param name="relType"></param>
        /// <returns></returns>
        private async Task<Tuple<Dtos.GuidObject2, Dtos.GuidObject2>> ConvertEntityRelationshipTypesToDtoRelationshipTypes(string relType, bool bypassCache)
        {

            if (relationTypes == null)
                relationTypes = await this.GetRelationTypesAsync(bypassCache);
            var subjectRelation_type = relationTypes.FirstOrDefault(x => x.Code.Equals(relType, StringComparison.OrdinalIgnoreCase));
            if (subjectRelation_type == null)
            {
                throw new KeyNotFoundException(string.Concat("Did not find relationship type for code: ", relType));
            }
            var subjectRelation = new Dtos.GuidObject2(subjectRelation_type.Guid);
            var relativeRelation_type = relationTypes.FirstOrDefault(x => x.Code.Equals(subjectRelation_type.InverseRelType, StringComparison.OrdinalIgnoreCase));
            if (relativeRelation_type != null)
            {
                var relativeRelation = new Dtos.GuidObject2(relativeRelation_type.Guid);
                return new Tuple<Dtos.GuidObject2, Dtos.GuidObject2>(subjectRelation, relativeRelation);
            }
            return new Tuple<Dtos.GuidObject2, Dtos.GuidObject2>(subjectRelation, null);
        }

        /// <summary>
        /// Update a PersonRelationships.
        /// </summary>
        /// <param name="PersonRelationships">The <see cref="PersonRelationships">personRelationships</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="PersonRelationships">personRelationships</see></returns>
        public async Task<PersonRelationships> UpdatePersonRelationshipsAsync(PersonRelationships personRelationships)
        {
            if (personRelationships == null)
                throw new ArgumentNullException("PersonRelationships", "Must provide a PersonRelationships for update");
            if (string.IsNullOrEmpty(personRelationships.Id))
                throw new ArgumentNullException("PersonRelationships", "Must provide a guid for PersonRelationships update");

            // get the ID associated with the incoming guid
            var personRelationshipsEntityId = await _relationshipRepository.GetPersonRelationshipsIdFromGuidAsync(personRelationships.Id);
            if (!string.IsNullOrEmpty(personRelationshipsEntityId))
            {
                // verify the user has the permission to update a personRelationships
                this.CheckCreatePersonRelationshipsPermission();

                try
                {
                    // map the DTO to entities
                    var personRelationshipsEntity
                    = await ConvertPersonRelationshipsDtoToEntityAsync(personRelationshipsEntityId, personRelationships);

                    // update the entity in the database
                    var updatedPersonRelationshipsEntity =
                        await _relationshipRepository.UpdatePersonRelationshipsAsync(personRelationshipsEntity);

                    return await this.ConvertPersonRelationshipsEntityToDto(updatedPersonRelationshipsEntity, true);
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
            // perform a create instead
            return await CreatePersonRelationshipsAsync(personRelationships);
        }

        /// <summary>
        /// Create a PersonRelationships.
        /// </summary>
        /// <param name="personRelationships">The <see cref="PersonRelationships">personRelationships</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PersonRelationships">personRelationships</see></returns>
        public async Task<PersonRelationships> CreatePersonRelationshipsAsync(PersonRelationships personRelationships)
        {
            if (personRelationships == null)
                throw new ArgumentNullException("PersonRelationships", "Must provide a PersonRelationships for update");
            if (string.IsNullOrEmpty(personRelationships.Id))
                throw new ArgumentNullException("PersonRelationships", "Must provide a guid for PersonRelationships update");

            // verify the user has the permission to create a personRelationships
            this.CheckCreatePersonRelationshipsPermission();

            try
            {

                var personRelationshipsEntity
                         = await ConvertPersonRelationshipsDtoToEntityAsync(string.Empty, personRelationships);

                // create a personRelationships entity in the database
                var createdPersonRelationships =
                    await _relationshipRepository.CreatePersonRelationshipsAsync(personRelationshipsEntity);
                // return the newly created personRelationships
                return await this.ConvertPersonRelationshipsEntityToDto(createdPersonRelationships, true);

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        private async Task<Domain.Base.Entities.Relationship> ConvertPersonRelationshipsDtoToEntityAsync(string personRelationshipsId, PersonRelationships personRelationshipsDto, bool bypassCache = false)
        {
            if (personRelationshipsDto == null || string.IsNullOrEmpty(personRelationshipsDto.Id))
                throw new ArgumentNullException("PersonRelationships", "Must provide guid for a PersonRelationship");
            try
            {
                //get person id1
                if (personRelationshipsDto.SubjectPerson == null || string.IsNullOrEmpty(personRelationshipsDto.SubjectPerson.Id))
                {
                    throw new ArgumentNullException("SubjectPerson", " Subject Person ID is required for person-relationships.");
                }
                var personId1 = await _personRepository.GetPersonIdFromGuidAsync(personRelationshipsDto.SubjectPerson.Id);
                if (string.IsNullOrEmpty(personId1))
                {
                    throw new ArgumentException("Subject person is not an individual. ");
                }
                //get person id2
                if (personRelationshipsDto.Related == null || personRelationshipsDto.Related.person == null || string.IsNullOrEmpty(personRelationshipsDto.Related.person.Id))
                {
                    throw new ArgumentNullException("RelatedPerson", " Related Person ID is required for person-relationships.");
                }
                var personId2 = await _personRepository.GetPersonIdFromGuidAsync(personRelationshipsDto.Related.person.Id);
                if (string.IsNullOrEmpty(personId2))
                {
                    throw new ArgumentException("Related person is not an individual.");
                }
                if (personId1.Equals(personId2, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("The Subject Person ID cannot be the same as the Related Person ID. ");
                }
                //convert and validate relationship types
                if (personRelationshipsDto.DirectRelationshipType == null)
                {
                    throw new ArgumentNullException("DirectRelationshipType", " Direct Relationshiptype is required for person-relationships. ");
                }
                if (string.IsNullOrEmpty(personRelationshipsDto.DirectRelationshipType.Id))
                {
                    throw new ArgumentNullException("DirectRelationshipType", " Direct Relationshiptype ID is required for person-relationships.  ");
                }

                relationTypes = await this.GetRelationTypesAsync(bypassCache);
                var subjectRelation_type = relationTypes.FirstOrDefault(x => x.Guid.Equals(personRelationshipsDto.DirectRelationshipType.Id, StringComparison.OrdinalIgnoreCase));
                if (subjectRelation_type == null)
                {
                    throw new ArgumentNullException("DirectRelationshipType", " The direct relationship type is not valid for this relationship. ");
                }
                //check to make sure the directRelationshipType does not have the organization only indicator set to Yes
                if (subjectRelation_type.OrgIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(string.Concat("The relationship type '", subjectRelation_type.Code , "' is for relationships involving nonpersons only. "));
                }
                var inverseRelType = string.Empty;
                //if there is a reciprocal relationship
                if (personRelationshipsDto.ReciprocalRelationshipType != null)
                {
                    if (string.IsNullOrEmpty(personRelationshipsDto.ReciprocalRelationshipType.Id))
                    {
                        throw new ArgumentNullException("ReciprocalRelationshipType", " Reciprocal Relationshiptype ID is required.  ");
                    }

                    var relRelation_type = relationTypes.FirstOrDefault(x => x.Guid.Equals(personRelationshipsDto.ReciprocalRelationshipType.Id, StringComparison.OrdinalIgnoreCase));
                    if (relRelation_type == null)
                    {
                        throw new ArgumentException(string.Concat("The reciprocal relationship type is not valid for this relationship. "));
                    }
                    //check to make sure the relationship type has inverse relationship
                    if (string.IsNullOrEmpty(subjectRelation_type.InverseRelType))
                    {
                        throw new ArgumentException(string.Concat("The direct relationship type ", subjectRelation_type.Code, " does not have a inverse relationship type. "));
                    }
                    if (!subjectRelation_type.InverseRelType.Equals(relRelation_type.Code, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException(string.Concat("The inverse relationship type of ", subjectRelation_type.InverseRelType, " does not match the reciprocal relationship type of ", relRelation_type.Code, "."));
                    }
                    inverseRelType = relRelation_type.Code;
                }
                else
                {
                    if (!string.IsNullOrEmpty(subjectRelation_type.InverseRelType))
                    {
                        inverseRelType = subjectRelation_type.InverseRelType;
                    }
                }
                //validate startOn and endOn
                if (personRelationshipsDto.StartOn.HasValue)
                {
                    if ((subjectRelation_type.Category.Equals("S", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("P", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("C", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (personRelationshipsDto.StartOn > DateTime.Today)
                        {
                            throw new ArgumentException(string.Concat("Start on date cannot be in the future for this type of relationship. "));
                        }
                    }
                }
                if (personRelationshipsDto.EndOn.HasValue)
                {
                    if ((subjectRelation_type.Category.Equals("S", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("P", StringComparison.OrdinalIgnoreCase)) || (subjectRelation_type.Category.Equals("C", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (personRelationshipsDto.EndOn > DateTime.Today)
                        {
                            throw new ArgumentException(string.Concat("End on date cannot be in the future for this type of relationship. "));
                        }
                    }
                }
                if (personRelationshipsDto.StartOn.HasValue && personRelationshipsDto.EndOn.HasValue && personRelationshipsDto.StartOn > personRelationshipsDto.EndOn)
                {
                    throw new ArgumentException(string.Concat("The start on date cannot be greater than the end on date. "));
                }
              
                //since all the validation is done. create the entity.
                Domain.Base.Entities.Relationship response = new Domain.Base.Entities.Relationship(personRelationshipsId, personId1, personId2, subjectRelation_type.Code, inverseRelType, true, personRelationshipsDto.StartOn, personRelationshipsDto.EndOn);
                response.Guid = personRelationshipsDto.Id;
                response.SubjectPersonGuid = personRelationshipsDto.SubjectPerson.Id;
                response.RelationPersonGuid = personRelationshipsDto.Related.person.Id;

                //validate status
                if (personRelationshipsDto.Status != null)
                {
                    if (string.IsNullOrEmpty(personRelationshipsDto.Status.Id))
                    {
                        throw new ArgumentNullException("Status ID is required.  ");
                    }
                    if (relationshipStatuses == null)
                    {
                        relationshipStatuses = await _referenceDataRepository.GetRelationshipStatusesAsync(bypassCache);
                        if (relationshipStatuses == null || !relationshipStatuses.Any())
                        {
                            throw new ArgumentNullException("PersonRelationships", "Relationship statuses are missing");
                        }
                    }
                    var statusEntity = relationshipStatuses.FirstOrDefault(x => x.Guid.Equals(personRelationshipsDto.Status.Id, StringComparison.OrdinalIgnoreCase));
                    if (statusEntity == null)
                    {
                        throw new ArgumentNullException(string.Concat("Did not find relationship status code for status id: ", personRelationshipsDto.Status.Id));
                    }
                    response.Status = statusEntity.Code;
                }
                if (!string.IsNullOrEmpty(personRelationshipsDto.Comment))
                {
                    response.Comment = personRelationshipsDto.Comment.Replace(Convert.ToChar(DynamicArray.VM), '\n')
                                                   .Replace(Convert.ToChar(DynamicArray.TM), ' ')
                                                   .Replace(Convert.ToChar(DynamicArray.SM), ' '); ;
                }
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("An error occurred processing PersonRelationships ", ex.Message));
            }

        
    }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update PersonRelationships.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreatePersonRelationshipsPermission()
        {
            bool hasPermission = HasPermission(BasePermissionCodes.UpdatePersonRelationship);

            // User is not allowed to create or update PersonRelationships without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create/update PersonRelationships.");
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to delete PersonRelationships.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckDeletePersonRelationshipsPermission()
        {
            bool hasPermission = HasPermission(BasePermissionCodes.DeletePersonRelationship);

            // User is not allowed to delete PersonRelationships without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to delete PersonRelationships.");
            }
        }

        /// <summary>
        /// Delete a person relationship from the database
        /// </summary>
        /// <param name="id">The requested personrelationship GUID</param>
        /// <returns></returns>
        public async Task DeletePersonRelationshipsAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Must provide a person relationship guid for deletion.");
            }
            CheckDeletePersonRelationshipsPermission();
            try
            {
                var relationship = await _relationshipRepository.GetPersonRelationshipById2Async(guid);
                if (relationship == null)
                    throw new KeyNotFoundException("Person-relationship record not found for id: " + guid);
                await _relationshipRepository.DeletePersonRelationshipAsync(relationship.Id);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Person-relationship record not found for id: " + guid);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }
    }
}
