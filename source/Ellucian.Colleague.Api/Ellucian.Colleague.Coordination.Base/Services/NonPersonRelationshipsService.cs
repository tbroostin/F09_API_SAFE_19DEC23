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
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class NonPersonRelationshipsService : BaseCoordinationService, INonPersonRelationshipsService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IConfigurationRepository _configurationRepository;

        private IEnumerable<Domain.Base.Entities.RelationType> relationTypes;
        private IEnumerable<Domain.Base.Entities.RelationshipStatus> relationshipStatuses;

        public NonPersonRelationshipsService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IRelationshipRepository relationshipRepository,
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
        /// <returns>Collection of NonPersonRelationships DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.NonPersonRelationships>, int>> GetNonPersonRelationshipsAsync(int offset, int limit, string organization = "", string institution = "", string person = "", string relationshipType = "", bool bypassCache = false)
        {
            try
            {

                CheckGetNonPersonRelationshipPermission();
                string personCode = string.Empty;
                string orgId = string.Empty;
                string instId = string.Empty;
                if (!string.IsNullOrEmpty(person))
                {
                    personCode = await _personRepository.GetPersonIdFromGuidAsync(person);
                    if (string.IsNullOrEmpty(personCode))
                        return new Tuple<IEnumerable<Dtos.NonPersonRelationships>, int>(new List<Dtos.NonPersonRelationships>(), 0);
                    //throw new ArgumentException("Invalid Person '" + person + "' in the arguments");
                    else if (await _personRepository.IsCorpAsync(personCode))
                        return new Tuple<IEnumerable<Dtos.NonPersonRelationships>, int>(new List<Dtos.NonPersonRelationships>(), 0);
                    //throw new ArgumentException("Invalid person '" + person + "' in the arguments");
                }

                if (!string.IsNullOrEmpty(organization))
                {
                    orgId = await _personRepository.GetPersonIdFromGuidAsync(organization);
                    if (string.IsNullOrEmpty(orgId))
                        return new Tuple<IEnumerable<Dtos.NonPersonRelationships>, int>(new List<Dtos.NonPersonRelationships>(), 0);
                    //throw new ArgumentException("Invalid organization '" + organization + "' in the arguments");
                    else if (!await _personRepository.IsCorpAsync(orgId))
                        return new Tuple<IEnumerable<Dtos.NonPersonRelationships>, int>(new List<Dtos.NonPersonRelationships>(), 0);
                    //throw new ArgumentException("Invalid organization '" + organization + "' in the arguments");
                }

                if (!string.IsNullOrEmpty(institution))
                {
                    instId = await _personRepository.GetPersonIdFromGuidAsync(institution);
                    if (string.IsNullOrEmpty(instId))
                        return new Tuple<IEnumerable<Dtos.NonPersonRelationships>, int>(new List<Dtos.NonPersonRelationships>(), 0);
                    //throw new ArgumentException("Invalid institution '" + institution + "' in the arguments");
                    else if (!await _personRepository.IsCorpAsync(instId))
                        return new Tuple<IEnumerable<Dtos.NonPersonRelationships>, int>(new List<Dtos.NonPersonRelationships>(), 0);
                    //throw new ArgumentException("Invalid institution '" + institution + "' in the arguments");
                }
                string relationshipTypeCode = string.Empty;
                string inverseRelationshipTypeCode = string.Empty;
                if (!string.IsNullOrEmpty(relationshipType))
                {
                    if (relationTypes == null)
                        relationTypes = await this.GetRelationTypesAsync(true);
                    var relationshipTypeEntity = relationTypes.FirstOrDefault(x => x.Guid.Equals(relationshipType, StringComparison.OrdinalIgnoreCase));
                    if (relationshipTypeEntity == null)
                    {
                        throw new ArgumentException("Invalid relationshipType '" + relationshipType + "' in the arguments");
                    }
                    relationshipTypeCode = relationshipTypeEntity.Code;
                    inverseRelationshipTypeCode = relationshipTypeEntity.InverseRelType;
                }

                var personalRelationshipEntitiesTuple = await _relationshipRepository.GetNonPersonRelationshipsAsync(offset, limit, orgId, instId, personCode, relationshipTypeCode, inverseRelationshipTypeCode, bypassCache);

                if (personalRelationshipEntitiesTuple != null)
                {
                    var personalRelationshipEntities = personalRelationshipEntitiesTuple.Item1.ToList();
                    var totalCount = personalRelationshipEntitiesTuple.Item2;

                    if (personalRelationshipEntities.Any())
                    {
                        List<Dtos.NonPersonRelationships> personalRelationships = new List<Dtos.NonPersonRelationships>();

                        foreach (var personalRelationshipEntity in personalRelationshipEntities)
                        {
                            personalRelationships.Add(await this.ConvertNonPersonRelationshipsEntityToDto(personalRelationshipEntity, bypassCache));
                        }
                        return new Tuple<IEnumerable<Dtos.NonPersonRelationships>, int>(personalRelationships, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.NonPersonRelationships>, int>(new List<Dtos.NonPersonRelationships>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.NonPersonRelationships>, int>(new List<Dtos.NonPersonRelationships>(), 0);
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
        private void CheckGetNonPersonRelationshipPermission()
        {
            var hasPermission = HasPermission(BasePermissionCodes.ViewAnyNonPersonRelationship);
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view nonperson relationships.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a NonPersonRelationships from its GUID
        /// </summary>
        /// <returns>NonPersonRelationships DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.NonPersonRelationships> GetNonPersonRelationshipsByGuidAsync(string guid, bool bypassCache = true)
        {
            CheckGetNonPersonRelationshipPermission();
            try
            {
                return await ConvertNonPersonRelationshipsEntityToDto(await _relationshipRepository.GetNonPersonRelationshipByIdAsync(guid));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("nonperson-relationships not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("nonperson-relationships not found for GUID " + guid, ex);
            }
        }
        
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Relationship domain entity to its corresponding NonPersonRelationships DTO
        /// </summary>
        /// <param name="source">Relationship domain entity</param>
        /// <returns>NonPersonRelationships DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.NonPersonRelationships> ConvertNonPersonRelationshipsEntityToDto(Domain.Base.Entities.Relationship personRelationshipEntity, bool bypassCache = false)
        {
            Dtos.NonPersonRelationships personRelationshipDto = new Dtos.NonPersonRelationships();
            personRelationshipDto.Id = personRelationshipEntity.Guid;
            var nonPersonSubject = new NonpersonRelationshipsSubject();
            if (personRelationshipEntity.SubjectPersonInstFlag)
            {
                nonPersonSubject.institution = new Dtos.GuidObject2(personRelationshipEntity.SubjectPersonGuid);
                personRelationshipDto.Subject = nonPersonSubject;
            }
            else
            {
                if (personRelationshipEntity.SubjectPersonOrgFlag)
                {
                    nonPersonSubject.organization = new Dtos.GuidObject2(personRelationshipEntity.SubjectPersonGuid);
                    personRelationshipDto.Subject = nonPersonSubject;
                }
            }
            var nonPersonRel = new NonpersonRelationshipsRelated();
            if (personRelationshipEntity.RelationPersonInstFlag)
            {
                nonPersonRel.institution = new Dtos.GuidObject2(personRelationshipEntity.RelationPersonGuid);
                personRelationshipDto.Related = nonPersonRel;
            }
            else if (personRelationshipEntity.RelationPersonOrgFlag)
            {
                nonPersonRel.organization = new Dtos.GuidObject2(personRelationshipEntity.RelationPersonGuid);
                personRelationshipDto.Related = nonPersonRel;
            }
            else
            {
                nonPersonRel.person = new Dtos.GuidObject2(personRelationshipEntity.RelationPersonGuid);
                personRelationshipDto.Related = nonPersonRel;
            }
            personRelationshipDto.Comment = personRelationshipEntity.Comment;
            personRelationshipDto.EndOn = personRelationshipEntity.EndDate;
            personRelationshipDto.StartOn = personRelationshipEntity.StartDate;
            personRelationshipDto.Status = string.IsNullOrEmpty(personRelationshipEntity.Status) ?
                null :
                await ConvertEntityStatusToDtoStatus(personRelationshipEntity.Status, bypassCache);

            Tuple<Dtos.GuidObject2, Dtos.GuidObject2> relationships = await ConvertEntityRelationshipTypesToDtoRelationshipTypes
                (personRelationshipEntity.RelationshipType, bypassCache);
            if (relationships != null)
            {
                if (relationships.Item1 != null)
                    personRelationshipDto.DirectRelationshipType = relationships.Item1;
                if (relationships.Item2 != null)
                    personRelationshipDto.ReciprocalRelationshipType = relationships.Item2;
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
                relationTypes = await _referenceDataRepository.GetRelationTypesAsync(bypassCache);
                if (relationTypes == null || !relationTypes.Any())
                {
                    throw new ArgumentNullException("NonPersonRelationships", "Relationship types are missing");
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
                    throw new ArgumentNullException("NonPersonRelationships", "Relationship statuses are missing");
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
    }
}
