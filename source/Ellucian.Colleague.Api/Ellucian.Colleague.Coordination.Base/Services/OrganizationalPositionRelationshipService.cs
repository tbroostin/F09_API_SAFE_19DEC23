// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class OrganizationalPositionRelationshipService : BaseCoordinationService, IOrganizationalPositionRelationshipService
    {
        private IOrganizationalPositionRelationshipRepository _organizationalPositionRelationshipRepository;
        public OrganizationalPositionRelationshipService(IOrganizationalPositionRelationshipRepository organizationalPositionRelationshipRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStaffRepository staffRepository = null, IConfigurationRepository configurationRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            _organizationalPositionRelationshipRepository = organizationalPositionRelationshipRepository;
        }

        /// <summary>
        /// Adds a new organizational position relationship
        /// </summary>
        /// <param name="organizationalPositionRelationship">organizational position relationship to add</param>
        /// <returns>The newly created organizational position relationship</returns>
        public async Task<Dtos.Base.OrganizationalPositionRelationship> AddAsync(Dtos.Base.OrganizationalPositionRelationship organizationalPositionRelationship)
        {
            if (organizationalPositionRelationship == null)
            {
                throw new ArgumentNullException("organizationalPositionRelationship", "organizationalPositionRelationship cannot be null.");

            }

            CheckUpdateOrganizationalRelationshipPermission();

            var dtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Base.OrganizationalPositionRelationship, Domain.Base.Entities.OrganizationalPositionRelationship>();
            var entityToAdd = dtoToEntityAdapter.MapToType(organizationalPositionRelationship);
            var addedEntity = await _organizationalPositionRelationshipRepository.AddAsync(entityToAdd);
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.OrganizationalPositionRelationship, Dtos.Base.OrganizationalPositionRelationship>();
            var addedDto = entityToDtoAdapter.MapToType(addedEntity);

            return addedDto;
        }

        /// <summary>
        /// Deletes an organizational position relationship
        /// </summary>
        /// <param name="organizationalPositionRelationshipId">Organizational position relationship id to delete</param>
        public async Task DeleteAsync(string organizationalPositionRelationshipId)
        {
            if (string.IsNullOrEmpty(organizationalPositionRelationshipId))
            {
                throw new ArgumentNullException("organizationalPositionRelationshipId", "organizationalPositionRelationshipId cannot be null or empty.");
            }
            CheckUpdateOrganizationalRelationshipPermission();
            await _organizationalPositionRelationshipRepository.DeleteAsync(organizationalPositionRelationshipId);
        }

        /// <summary>
        /// Throws a PermissionsException if the user cannot update organizational relationships
        /// </summary>
        /// 
        private void CheckUpdateOrganizationalRelationshipPermission()
        {
            if (HasPermission(BasePermissionCodes.UpdateOrganizationalRelationships))
            {
                return;
            }
            else
            {
                throw new PermissionsException("User does not have permission to add, update or delete organizational relationships.");
            }
        }
    }
}
