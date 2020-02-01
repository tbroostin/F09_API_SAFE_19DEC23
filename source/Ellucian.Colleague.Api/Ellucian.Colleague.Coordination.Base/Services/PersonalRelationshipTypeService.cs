// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class PersonalRelationshipTypeService : BaseCoordinationService, IPersonalRelationshipTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        //private readonly IPersonRepository _personRepository;
        private const string _dataOrigin = "Colleague";

        public PersonalRelationshipTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                        // IPersonRepository personRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger,
                                         IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
           // _personRepository = personRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all personal relation types
        /// </summary>
        /// <returns>Collection of PersonalRelationshipType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RelationType>> GetPersonalRelationTypesAsync(bool bypassCache = false)
        {
            var relationTypeCollection = new List<Ellucian.Colleague.Dtos.RelationType>();

            var relationTypeEntities = (await _referenceDataRepository.GetRelationTypesAsync(bypassCache)).Where(rt => rt.OrgIndicator != "Y");
            if (relationTypeEntities != null && relationTypeEntities.Count() > 0)
            {
                foreach (var relationType in relationTypeEntities)
                {
                    Ellucian.Colleague.Dtos.RelationType relTyp = ConvertRelationTypeEntityToRelationshipTypeDto(relationType);
                    var inverse = relationTypeEntities.FirstOrDefault(i => i.Code.ToUpper() == relationType.InverseRelType.ToUpper());
                    relationTypeCollection.Add(ConvertInverseRelationTypeEntityToValidRelationshipTypeDtoAndAddToCollection(relTyp, inverse));
                }
            }
            return relationTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a relation type from its GUID
        /// </summary>
        /// <returns>RelationType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.RelationType> GetPersonalRelationTypeByGuidAsync(string guid)
        {
            var relationTypeEntities = (await _referenceDataRepository.GetRelationTypesAsync(true));
            try
            {
                RelationType relTypEntity = (relationTypeEntities).Where(rt => rt.Guid == guid).FirstOrDefault();
                if (relTypEntity == null)
                {
                    throw new KeyNotFoundException("Relation Type not found for GUID " + guid);
                }
                Ellucian.Colleague.Dtos.RelationType relTypDto = ConvertRelationTypeEntityToRelationshipTypeDto(relTypEntity);
                var inverse = relationTypeEntities.FirstOrDefault(i => i.Code.ToUpper() == relTypEntity.InverseRelType.ToUpper());
                return ConvertInverseRelationTypeEntityToValidRelationshipTypeDtoAndAddToCollection(relTypDto, inverse);
                //return ConvertRelationTypeEntityToRelationshipTypeDto((await _referenceDataRepository.GetRelationTypesAsync(true)).Where(rt => rt.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Relation Type not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all personal relationship statuses
        /// </summary>
        /// <returns>Collection of PersonalRelationshipStatus DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationshipStatus>> GetPersonalRelationshipStatusesAsync(bool bypassCache = false)
        {
            var personalRelationStatusCollection = new List<Ellucian.Colleague.Dtos.PersonalRelationshipStatus>();

            var personalRelationStatusEntities = await _referenceDataRepository.GetRelationshipStatusesAsync(bypassCache);
            if (personalRelationStatusEntities != null && personalRelationStatusEntities.Count() > 0)
            {
                foreach (var relationStatus in personalRelationStatusEntities)
                {
                    personalRelationStatusCollection.Add(ConvertPersonalRelationshipStatusEntityToPersonalRelationshipStatusDto(relationStatus));
                }
            }
            return personalRelationStatusCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a relation type from its GUID
        /// </summary>
        /// <returns>PersonalRelationshipStatus DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonalRelationshipStatus> GetPersonalRelationshipStatusByGuidAsync(string guid)
        {
            try
            {
                return ConvertPersonalRelationshipStatusEntityToPersonalRelationshipStatusDto((await _referenceDataRepository.GetRelationshipStatusesAsync(true)).Where(rt => rt.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Personal Relationship Status not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a RelationType domain entity to its corresponding RelationType DTO
        /// </summary>
        /// <param name="source">Email domain entity</param>
        /// <returns>PersonalRelationshipType DTO</returns>
        private Dtos.RelationType ConvertRelationTypeEntityToRelationshipTypeDto(RelationType source)
        {
            var personalRelationshipType = new Dtos.RelationType();
            personalRelationshipType.Id = source.Guid;
            personalRelationshipType.Code = source.Code;
            personalRelationshipType.Title = source.Description;
            personalRelationshipType.Description = null;
            personalRelationshipType.PersonalRelationshipType = (source.PersonRelType == null) ?
                Dtos.PersonalRelationshipType.Other : ConvertPersonPersonalRelationshipTypeDomainEnumToPersonPersonalRelationshipTypeDtoEnum(source.PersonRelType);
            
            return personalRelationshipType;
        }

        private Dtos.RelationType ConvertInverseRelationTypeEntityToValidRelationshipTypeDtoAndAddToCollection(Ellucian.Colleague.Dtos.RelationType source, RelationType inverse)
        {
            var validReciprocalRelationship = new List<Dtos.ValidReciprocalRelationship>();

            if (inverse != null)
            {
                var element = new Dtos.ValidReciprocalRelationship()
               {
                   PersonalRelationshipType = ConvertPersonPersonalRelationshipTypeDomainEnumToPersonPersonalRelationshipTypeDtoEnum(inverse.PersonRelType),
                   Detail = new Dtos.GuidObject2(inverse.Guid)
               };
                validReciprocalRelationship.Add(element);

                var element2 = new Dtos.ValidReciprocalRelationship()
                {
                    PersonalRelationshipType = ConvertPersonPersonalRelationshipTypeDomainEnumToPersonPersonalRelationshipTypeDtoEnum(inverse.MaleRelType),
                    Detail = new Dtos.GuidObject2(inverse.Guid)
                };
                validReciprocalRelationship.Add(element2);

                var element3 = new Dtos.ValidReciprocalRelationship()
                {
                    PersonalRelationshipType = ConvertPersonPersonalRelationshipTypeDomainEnumToPersonPersonalRelationshipTypeDtoEnum(inverse.FemaleRelType),
                    Detail = new Dtos.GuidObject2(inverse.Guid)
                };
                validReciprocalRelationship.Add(element3);
            }
            
            if ((validReciprocalRelationship != null) && (validReciprocalRelationship.Any()))
            {
                source.ValidReciprocalRelationship = validReciprocalRelationship;
             }

            return source;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a PersonalRelationshipStatus domain entity to its corresponding PersonalRelationshipStatus DTO
        /// </summary>
        /// <param name="source">PersonalRelationshipStatus domain entity</param>
        /// <returns>PersonalRelationshipStatus DTO</returns>
        private Dtos.PersonalRelationshipStatus ConvertPersonalRelationshipStatusEntityToPersonalRelationshipStatusDto(RelationshipStatus source)
        {
            var personalRelationshipStatus = new Dtos.PersonalRelationshipStatus();
            personalRelationshipStatus.Id = source.Guid;
            personalRelationshipStatus.Code = source.Code;
            personalRelationshipStatus.Title = source.Description;
            personalRelationshipStatus.Description = null;
           
            return personalRelationshipStatus;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a PersonPersonalRelationshipType domain enumeration value to its corresponding PersonPersonalRelationshipType DTO enumeration value
        /// </summary>
        /// <param name="source">PersonPersonalRelationshipType domain enumeration value</param>
        /// <returns>PersonPersonalRelationshipType DTO enumeration value</returns>
       private Dtos.PersonalRelationshipType ConvertPersonPersonalRelationshipTypeDomainEnumToPersonPersonalRelationshipTypeDtoEnum(PersonalRelationshipType? source)
        {
            if (source == null)
                return Dtos.PersonalRelationshipType.Other;

           switch (source)
            {
                case PersonalRelationshipType.Husband:
                    return Dtos.PersonalRelationshipType.Husband;
                case PersonalRelationshipType.Aunt:
                    return Dtos.PersonalRelationshipType.Aunt;
                case PersonalRelationshipType.Brother:
                    return Dtos.PersonalRelationshipType.Brother;
                case PersonalRelationshipType.BrotherInLaw:
                    return Dtos.PersonalRelationshipType.BrotherInLaw;
                case PersonalRelationshipType.Caregiver:
                    return Dtos.PersonalRelationshipType.Caregiver;
                case PersonalRelationshipType.Child:
                    return Dtos.PersonalRelationshipType.Child;
                case PersonalRelationshipType.ChildInLaw:
                    return Dtos.PersonalRelationshipType.ChildInLaw;
                case PersonalRelationshipType.ChildOfSibling:
                    return Dtos.PersonalRelationshipType.ChildOfSibling;
                case PersonalRelationshipType.Classmate:
                    return Dtos.PersonalRelationshipType.Classmate;
                case PersonalRelationshipType.Cousin:
                    return Dtos.PersonalRelationshipType.Cousin;
                case PersonalRelationshipType.Coworker:
                    return Dtos.PersonalRelationshipType.Coworker;
                case PersonalRelationshipType.Daughter:
                    return Dtos.PersonalRelationshipType.Daughter;
                case PersonalRelationshipType.DaughterInLaw:
                    return Dtos.PersonalRelationshipType.DaughterInLaw;
                case PersonalRelationshipType.Father:
                    return Dtos.PersonalRelationshipType.Father;
                case PersonalRelationshipType.FatherInLaw:
                    return Dtos.PersonalRelationshipType.FatherInLaw;
                case PersonalRelationshipType.Friend:
                    return Dtos.PersonalRelationshipType.Friend;
                case PersonalRelationshipType.GrandChild:
                    return Dtos.PersonalRelationshipType.GrandChild;
                case PersonalRelationshipType.GrandDaughter:
                    return Dtos.PersonalRelationshipType.GrandDaughter;
                case PersonalRelationshipType.GrandFather:
                    return Dtos.PersonalRelationshipType.GrandFather;
                case PersonalRelationshipType.GrandMother:
                    return Dtos.PersonalRelationshipType.GrandMother;
                case PersonalRelationshipType.GrandParent:
                    return Dtos.PersonalRelationshipType.GrandParent;
                case PersonalRelationshipType.GrandSon:
                    return Dtos.PersonalRelationshipType.GrandSon;
                case PersonalRelationshipType.Mother:
                    return Dtos.PersonalRelationshipType.Mother;
                case PersonalRelationshipType.Neighbor:
                    return Dtos.PersonalRelationshipType.Neighbor;
                case PersonalRelationshipType.Nephew:
                    return Dtos.PersonalRelationshipType.Nephew;
                case PersonalRelationshipType.Niece:
                    return Dtos.PersonalRelationshipType.Niece;
                case PersonalRelationshipType.Parent:
                    return Dtos.PersonalRelationshipType.Parent;
                case PersonalRelationshipType.ParentInLaw:
                    return Dtos.PersonalRelationshipType.ParentInLaw;
                case PersonalRelationshipType.Partner:
                    return Dtos.PersonalRelationshipType.Partner;
                case PersonalRelationshipType.Relative:
                    return Dtos.PersonalRelationshipType.Relative;
                case PersonalRelationshipType.Sibling:
                    return Dtos.PersonalRelationshipType.Sibling;
                case PersonalRelationshipType.SiblingInLaw:
                    return Dtos.PersonalRelationshipType.SiblingInLaw;
                case PersonalRelationshipType.SiblingOfParent:
                    return Dtos.PersonalRelationshipType.SiblingOfParent;
                case PersonalRelationshipType.Sister:
                    return Dtos.PersonalRelationshipType.Sister;
                case PersonalRelationshipType.Son:
                    return Dtos.PersonalRelationshipType.Son;
                case PersonalRelationshipType.SonInLaw:
                    return Dtos.PersonalRelationshipType.SonInLaw;
                case PersonalRelationshipType.Spouse:
                    return Dtos.PersonalRelationshipType.Spouse;
                case PersonalRelationshipType.StepBrother:
                    return Dtos.PersonalRelationshipType.StepBrother;
                case PersonalRelationshipType.StepChild:
                    return Dtos.PersonalRelationshipType.StepChild;
                case PersonalRelationshipType.StepDaughter:
                    return Dtos.PersonalRelationshipType.StepDaughter;
                case PersonalRelationshipType.StepFather:
                    return Dtos.PersonalRelationshipType.StepFather;
                case PersonalRelationshipType.StepMother:
                    return Dtos.PersonalRelationshipType.StepMother;
                case PersonalRelationshipType.StepParent:
                    return Dtos.PersonalRelationshipType.StepParent;
                case PersonalRelationshipType.StepSibling:
                    return Dtos.PersonalRelationshipType.StepSibling;
                case PersonalRelationshipType.StepSister:
                    return Dtos.PersonalRelationshipType.StepSister;
                case PersonalRelationshipType.StepSon:
                    return Dtos.PersonalRelationshipType.StepSon;
                case PersonalRelationshipType.Uncle:
                    return Dtos.PersonalRelationshipType.Uncle;
                case PersonalRelationshipType.Wife:
                    return Dtos.PersonalRelationshipType.Wife;
                default:
                    return Dtos.PersonalRelationshipType.Other;
            }
        }
    }
}
