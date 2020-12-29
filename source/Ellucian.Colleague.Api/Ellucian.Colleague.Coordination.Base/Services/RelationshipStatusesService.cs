//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

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
    public class RelationshipStatusesService : BaseCoordinationService, IRelationshipStatusesService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public RelationshipStatusesService(

            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all relationship-statuses
        /// </summary>
        /// <returns>Collection of RelationshipStatuses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RelationshipStatuses>> GetRelationshipStatusesAsync(bool bypassCache = false)
        {
            var relationshipStatusesCollection = new List<Ellucian.Colleague.Dtos.RelationshipStatuses>();

            var relationshipStatusesEntities = await _referenceDataRepository.GetRelationshipStatusesAsync(bypassCache);
            if (relationshipStatusesEntities != null && relationshipStatusesEntities.Any())
            {
                foreach (var relationshipStatuses in relationshipStatusesEntities)
                {
                    relationshipStatusesCollection.Add(ConvertRelationshipStatusesEntityToDto(relationshipStatuses));
                }
            }
            return relationshipStatusesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a RelationshipStatuses from its GUID
        /// </summary>
        /// <returns>RelationshipStatuses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.RelationshipStatuses> GetRelationshipStatusesByGuidAsync(string guid)
        {
            try
            {
                return ConvertRelationshipStatusesEntityToDto((await _referenceDataRepository.GetRelationshipStatusesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("relationship-statuses not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("relationship-statuses not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a RelationStatuses domain entity to its corresponding RelationshipStatuses DTO
        /// </summary>
        /// <param name="source">RelationStatuses domain entity</param>
        /// <returns>RelationshipStatuses DTO</returns>
        private Ellucian.Colleague.Dtos.RelationshipStatuses ConvertRelationshipStatusesEntityToDto(RelationshipStatus source)
        {
            var relationshipStatuses = new Ellucian.Colleague.Dtos.RelationshipStatuses();

            relationshipStatuses.Id = source.Guid;
            relationshipStatuses.Code = source.Code;
            relationshipStatuses.Title = source.Description;
            relationshipStatuses.Description = null;           
                                                                        
            return relationshipStatuses;
        }

    }
   
}