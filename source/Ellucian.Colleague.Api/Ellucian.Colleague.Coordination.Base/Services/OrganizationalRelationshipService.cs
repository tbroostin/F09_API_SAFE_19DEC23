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
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// A service for reading and updating organizational relationships
    /// </summary>
    [RegisterType]
    public class OrganizationalRelationshipService : BaseCoordinationService, IOrganizationalRelationshipService
    {
        private IOrganizationalRelationshipRepository _organizationalRelationshipRepository;
        public OrganizationalRelationshipService(IOrganizationalRelationshipRepository organizationalRelationshipRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStaffRepository staffRepository = null) : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            _organizationalRelationshipRepository = organizationalRelationshipRepository;
        }

        /// <summary>
        /// Adds a new organizational relationship
        /// </summary>
        /// <param name="organizationalRelationship">The organizational relationship to add</param>
        /// <returns>The newly added organizational relationship</returns>
        public async Task<OrganizationalRelationship> AddAsync(OrganizationalRelationship organizationalRelationship)
        {
            if (organizationalRelationship == null)
            {
                throw new ArgumentNullException("organizationalRelationship", "Organizational relationship is required to add an organizational relationship.");
            }

            CheckUpdateOrganizationalRelationshipPermission();
            // Get DTO from service/controller

            // Create adapters
            // Mapped to entity
            var organizationalRelationshipDtoToEntityAdapter = _adapterRegistry.GetAdapter<OrganizationalRelationship, Domain.Base.Entities.OrganizationalRelationship>();

            var organizationalRelationshipEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.OrganizationalRelationship, OrganizationalRelationship>();

            // Repository = entities
            // DTO = Services

            // Give entity to repository, repository may not be exactly what we provided, so store in new variable
            var organizationalRelationshipEntity = organizationalRelationshipDtoToEntityAdapter.MapToType(organizationalRelationship);

            // Provide new entity to repo
            var organizationalRelationshipAddAsyncEntity = await _organizationalRelationshipRepository.AddAsync(organizationalRelationshipEntity);

            // Transform entity to DTO to return to the controller
            var organizationalRelationshipDto = organizationalRelationshipEntityToDtoAdapter.MapToType(organizationalRelationshipAddAsyncEntity);

            return organizationalRelationshipDto;
        }

        /// <summary>
        /// Deletes an organizational relationship
        /// </summary>
        /// <param name="id">The organizational relationship ID to delete</param>
        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to delete an organizational relationship.");
            }

            CheckUpdateOrganizationalRelationshipPermission();

            await _organizationalRelationshipRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Updates an organizational relationship
        /// </summary>
        /// <param name="organizationalRelationship">The organizational relationship to update</param>
        /// <returns>The updated organizational relationship</returns>
        public async Task<OrganizationalRelationship> UpdateAsync(OrganizationalRelationship organizationalRelationship)
        {
            if (organizationalRelationship == null)
            {
                throw new ArgumentNullException("organizationalRelationship", "Organizational relationship is required to update an organizational relationship.");
            }
           
            CheckUpdateOrganizationalRelationshipPermission();

            var organizationalRelationshipDtoToEntityAdapter = _adapterRegistry.GetAdapter<OrganizationalRelationship, Domain.Base.Entities.OrganizationalRelationship>();

            var organizationalRelationshipEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.OrganizationalRelationship, OrganizationalRelationship>();

            var organizationalRelationshipEntity = organizationalRelationshipDtoToEntityAdapter.MapToType(organizationalRelationship);

            var organizationalRelationshipUpdateAsyncEntity = await _organizationalRelationshipRepository.UpdateAsync(organizationalRelationshipEntity);

            var organizationalRelationshipDto = organizationalRelationshipEntityToDtoAdapter.MapToType(organizationalRelationshipUpdateAsyncEntity);

            return organizationalRelationshipDto;
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
