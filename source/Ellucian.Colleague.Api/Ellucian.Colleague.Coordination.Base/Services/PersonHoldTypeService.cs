// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class PersonHoldTypeService: BaseCoordinationService, IPersonHoldTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        public PersonHoldTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, 
                                     ICurrentUserFactory currentUserFactory, IConfigurationRepository configurationRepository,
                                     IRoleRepository roleRepository,
                                     ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Gets all person hold types
        /// </summary>
        /// <returns>Collection of PersonHoldType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonHoldType>> GetPersonHoldTypesAsync(bool bypassCache = false)
        {
            var personHoldTypeCollection = new List<Ellucian.Colleague.Dtos.PersonHoldType>();

            var restrictionEntities = await _referenceDataRepository.GetRestrictionsWithCategoryAsync(bypassCache);
            if (restrictionEntities != null && restrictionEntities.Any())
            {
                foreach (var restriction in restrictionEntities)
                {
                    personHoldTypeCollection.Add(ConvertRestrictionEntityToPersonHoldTypeDto(restriction));
                }
            }
            return personHoldTypeCollection;
        }

        /// <summary>
        /// Get a restriction type from its GUID
        /// </summary>
        /// <returns>PersonHoldType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonHoldType> GetPersonHoldTypeByGuid2Async(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("personholdtype", "id cannot be null or empty.");
            }
            try
            {
                return ConvertRestrictionEntityToPersonHoldTypeDto((await _referenceDataRepository.GetRestrictionsWithCategoryAsync(true)).FirstOrDefault(rt => rt.Guid == id));
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Person hold type not found for GUID " + id, ex);
            }
            catch (Exception ex)
            {
                // The adapter will throw an exception if anything about the data is not acceptable to domain rules
                logger.Error("Error occurred getting person hold type dto: " + ex.Message);
                throw;
            }

        }

        /// <summary>
        /// Converts a Restriction domain entity to its corresponding RestrictionType2 DTO
        /// </summary>
        /// <param name="source">Restriction domain entity</param>
        /// <returns>RestrictionType2 DTO</returns>
        private Ellucian.Colleague.Dtos.PersonHoldType ConvertRestrictionEntityToPersonHoldTypeDto(Ellucian.Colleague.Domain.Base.Entities.Restriction source)
        {
            var restriction = new Ellucian.Colleague.Dtos.PersonHoldType();

            restriction.Id = source.Guid;
            restriction.Code = source.Code;
            restriction.Title = source.Description;
            restriction.Description = null;
            restriction.Category = ConvertCategoryEntityEnumToDtoEnum(source.RestIntgCategory);

            return restriction;
        }

        /// <summary>
        /// Converts the domain restriction category type entity restriction category type
        /// </summary>
        /// <param name="restrictionCategoryType"></param>
        /// <returns></returns>
        private PersonHoldCategoryTypes ConvertCategoryEntityEnumToDtoEnum(RestrictionCategoryType restrictionCategoryType)
        {
            switch (restrictionCategoryType)
            {
                case RestrictionCategoryType.Academic:
                    return PersonHoldCategoryTypes.Academic;
                case RestrictionCategoryType.Administrative:
                    return PersonHoldCategoryTypes.Administrative;
                case RestrictionCategoryType.Disciplinary:
                    return PersonHoldCategoryTypes.Disciplinary;
                case RestrictionCategoryType.Financial:
                    return PersonHoldCategoryTypes.Financial;
                case RestrictionCategoryType.Health:
                    return PersonHoldCategoryTypes.Health;
                default:
                    return PersonHoldCategoryTypes.Academic;
            }
        }
    }
}
