// Copyright 2014 Ellucian Company L.P. and its affiliates.
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
    public class RestrictionTypeService : BaseCoordinationService, IRestrictionTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonRestrictionRepository _personRestrictionRepository;
        private const string _dataOrigin = "Colleague";

        public RestrictionTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         IPersonRepository personRepository,
                                         IPersonRestrictionRepository personRestrictionRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
            _personRestrictionRepository = personRestrictionRepository;
        }

        /// <summary>
        /// Gets all restriction types
        /// </summary>
        /// <returns>Collection of RestrictionType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RestrictionType>> GetRestrictionTypesAsync(bool bypassCache = false)
        {
            var restrictionTypeCollection = new List<Ellucian.Colleague.Dtos.RestrictionType>();

            var restrictionEntities = await _referenceDataRepository.GetRestrictionsAsync(bypassCache);
            if (restrictionEntities != null && restrictionEntities.Count() > 0)
            {
                foreach (var restriction in restrictionEntities)
                {
                    restrictionTypeCollection.Add(ConvertRestrictionEntityToRestrictionTypeDto(restriction));
                }
            }
            return restrictionTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Get a restriction type from its GUID
        /// </summary>
        /// <returns>RestrictionType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.RestrictionType> GetRestrictionTypeByGuidAsync(string guid)
        {
            try
            {
                return ConvertRestrictionEntityToRestrictionTypeDto((await _referenceDataRepository.GetRestrictionsAsync(true)).Where(rt => rt.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Restriction Type not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Gets all restriction types
        /// </summary>
        /// <returns>Collection of RestrictionType2 DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RestrictionType2>> GetRestrictionTypes2Async(bool bypassCache = false)
        {
            var restrictionTypeCollection = new List<Ellucian.Colleague.Dtos.RestrictionType2>();

            var restrictionEntities = await _referenceDataRepository.GetRestrictionsAsync(bypassCache);
            if (restrictionEntities != null && restrictionEntities.Count() > 0)
            {
                foreach (var restriction in restrictionEntities)
                {
                    restrictionTypeCollection.Add(ConvertRestrictionEntityToRestrictionTypeDto2(restriction));
                }
            }
            return restrictionTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Get a restriction type from its GUID
        /// </summary>
        /// <returns>RestrictionType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.RestrictionType2> GetRestrictionTypeByGuid2Async(string id)
        {
            try
            {
                return ConvertRestrictionEntityToRestrictionTypeDto2((await _referenceDataRepository.GetRestrictionsAsync(true)).Where(rt => rt.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Restriction Type not found for GUID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Converts a Restriction domain entity to its corresponding RestrictionType DTO
        /// </summary>
        /// <param name="source">Restriction domain entity</param>
        /// <returns>RestrictionType DTO</returns>
        private Ellucian.Colleague.Dtos.RestrictionType ConvertRestrictionEntityToRestrictionTypeDto(Ellucian.Colleague.Domain.Base.Entities.Restriction source)
        {
            var restriction = new Ellucian.Colleague.Dtos.RestrictionType();

            //restriction.Metadata = new Dtos.MetadataObject(_dataOrigin);
            restriction.Guid = source.Guid;
            restriction.Abbreviation = source.Code;
            restriction.Title = source.Description;
            restriction.Description = null;

            return restriction;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Converts a Restriction domain entity to its corresponding RestrictionType2 DTO
        /// </summary>
        /// <param name="source">Restriction domain entity</param>
        /// <returns>RestrictionType2 DTO</returns>
        private Ellucian.Colleague.Dtos.RestrictionType2 ConvertRestrictionEntityToRestrictionTypeDto2(Ellucian.Colleague.Domain.Base.Entities.Restriction source)
        {
            var restriction = new Ellucian.Colleague.Dtos.RestrictionType2();

            //restriction.Metadata = new Dtos.MetadataObject(_dataOrigin);
            restriction.Id = source.Guid;
            restriction.Code = source.Code;
            restriction.Title = source.Description;
            restriction.Description = null;

            return restriction;
        }
    }
}
