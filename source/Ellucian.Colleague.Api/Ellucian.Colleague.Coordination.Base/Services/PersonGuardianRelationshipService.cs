// Copyright 2015 Ellucian Company L.P. and its affiliates.
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
    /// Personal guardian relationships
    /// </summary>
    [RegisterType]
    public class PersonGuardianRelationshipService : BaseCoordinationService, IPersonGuardianRelationshipService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IConfigurationRepository _configurationRepository;

        private IEnumerable<RelationType> relationTypes;
        private List<string> defaultGuardianRelationships;

        public PersonGuardianRelationshipService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
            IRelationshipRepository relationshipRepository,
            IPersonRepository personRepository,
            IConfigurationRepository configurationRepository,
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
        /// Gets all or filtered person guardian relationships
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int>> GetPersonGuardianRelationshipsAllAndFilterAsync(int offset, int limit, string person = "")
        {
            CheckUserPersonGuardiansViewPermissions();

            List<Dtos.PersonGuardianRelationship> personGuardianRelationships = new List<Dtos.PersonGuardianRelationship>();

            // Convert and validate all input parameters
            var personId = string.Empty;
            if (!string.IsNullOrEmpty(person))
            {
                try
                {
                    personId = await _personRepository.GetPersonIdFromGuidAsync(person);
                    if (string.IsNullOrEmpty(personId))
                    {
                        throw new ArgumentException(string.Concat("GUID not found for person: ", person));
                    }
                }
                catch (KeyNotFoundException e)
                {
                    return new Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int>(new List<Dtos.PersonGuardianRelationship>(), 0);
                }
            }
            
            List<string> guardianWithInverseRelTypes = await GetGuardianRelationsWithInverseList();

            //If guardian relationship type not defined on CDHP then return empty collection
            if (guardianWithInverseRelTypes == null)
            {
                return new Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int>(new List<Dtos.PersonGuardianRelationship>(), 0);
            }

            var personalRelationshipEntities = await _relationshipRepository.GetAllGuardiansAsync(offset, limit, personId, guardianWithInverseRelTypes);

            foreach (var personalRelationshipEntity in personalRelationshipEntities.Item1)
            {
                var personalRelationship = await ConvertEntityToDto(personalRelationshipEntity);
                personGuardianRelationships.Add(personalRelationship);
            }
            return new Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int>(personGuardianRelationships, personalRelationshipEntities.Item2);
        }

        /// <summary>
        /// Gets person guardian relationship by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.PersonGuardianRelationship> GetPersonGuardianRelationshipByIdAsync(string id)
        {
            CheckUserPersonGuardiansViewPermissions();

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Guardian relationship id is required.");
            }

            List<string> guardianWithInverseRelTypes = await GetGuardianRelationsWithInverseList();

            //If guardian relationship type not defined on CDHP then return empty collection
            if (guardianWithInverseRelTypes == null)
            {
                throw new KeyNotFoundException("No person guardian was found for guid: " + id);
            }

            var personalRelationshipEntity = await _relationshipRepository.GetPersonGuardianRelationshipByIdAsync(id);
            /*
            //if the relationship has ended then throw the error
            if (personalRelationshipEntity.EndDate.HasValue && personalRelationshipEntity.EndDate.Value < DateTime.Today)
            {
                throw new KeyNotFoundException("No person guardian was found for guid: " + id);
            }
            */
            bool isGurdianRelationship = await this.IsGuardianRelationship(personalRelationshipEntity.RelationshipType);

            if (!isGurdianRelationship)
            {
                throw new KeyNotFoundException("No person guardian was found for guid: " + id);
            }

            Dtos.PersonGuardianRelationship personGuardianRelationship = await ConvertEntityToDto(personalRelationshipEntity);
            return personGuardianRelationship;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets guardian relations with its inverse relations
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> GetGuardianRelationsWithInverseList()
        {
            List<string> guardianRelTypes = new List<string>();
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
                guardianRelTypes
                    .AddRange(relationTypes.Where(i => defaultGuardianRelationships.Contains(i.Code) ||
                                                       defaultGuardianRelationships.Contains(i.InverseRelType))
                                           .Select(r => r.Code).ToArray());
            }
            return guardianRelTypes.Any() ? guardianRelTypes : null; ;
        }

        /// <summary>
        /// Converts entity to dto
        /// </summary>
        /// <param name="personalRelationshipEntity"></param>
        /// <returns></returns>
        private async Task<Dtos.PersonGuardianRelationship> ConvertEntityToDto(Relationship personalRelationshipEntity)
        {
            Dtos.PersonGuardianRelationship personGuardianRelationshipDto = new Dtos.PersonGuardianRelationship();
            personGuardianRelationshipDto.Id = personalRelationshipEntity.Guid;

            if (defaultGuardianRelationships == null)
            {
                defaultGuardianRelationships = await this.GetDefaultGuardianRelationTypes();
            }

            if (defaultGuardianRelationships.Contains(personalRelationshipEntity.RelationshipType))
            {
                personGuardianRelationshipDto.SubjectPerson = new Dtos.GuidObject2(personalRelationshipEntity.RelationPersonGuid); //new Dtos.GuidObject2(relPerson.Guid);
                personGuardianRelationshipDto.Guardians = new List<Dtos.DtoProperties.PersonGuardianDtoProperty>() 
                {
                    new Dtos.DtoProperties.PersonGuardianDtoProperty(){ Id = personalRelationshipEntity.SubjectPersonGuid }//subject.Guid}
                };
            }
            else
            {
                personGuardianRelationshipDto.SubjectPerson = new Dtos.GuidObject2(personalRelationshipEntity.SubjectPersonGuid);
                personGuardianRelationshipDto.Guardians = new List<Dtos.DtoProperties.PersonGuardianDtoProperty>() 
                {
                    new Dtos.DtoProperties.PersonGuardianDtoProperty(){ Id = personalRelationshipEntity.RelationPersonGuid}
                };
            }

            return personGuardianRelationshipDto;
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

                    if (guardRelType != null && (defaultGuardianRelationships.Contains(guardRelType.Code) || defaultGuardianRelationships.Contains(guardRelType.InverseRelType)))
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
        /// Verifies if the user has the correct permissions to view a person's guardian.
        /// </summary>
        private void CheckUserPersonGuardiansViewPermissions()
        {
            // access is ok if the current user has the view person guardians permission
            if (!HasPermission(BasePermissionCodes.ViewAnyPersonGuardian))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view person-guardians.");
                throw new PermissionsException("User is not authorized to view person-guardians.");
            }
        }
        #endregion
    }
}