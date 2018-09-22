// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for OrganizationalPersonPosition from entity to dto
    /// </summary>
    public class OrganizationalPersonPositionEntityToDtoAdapter : AutoMapperAdapter<OrganizationalPersonPosition, Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>
    {

        /// <summary>
        /// Initializes an <see cref="OrganizationalPersonPositionEntityToDtoAdapter"/> adapter.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public OrganizationalPersonPositionEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<OrganizationalRelationship, Ellucian.Colleague.Dtos.Base.OrganizationalRelationship>();
            AddMappingDependency<OrganizationalPositionRelationship, Ellucian.Colleague.Dtos.Base.OrganizationalPositionRelationship>();
        }
    }
}
