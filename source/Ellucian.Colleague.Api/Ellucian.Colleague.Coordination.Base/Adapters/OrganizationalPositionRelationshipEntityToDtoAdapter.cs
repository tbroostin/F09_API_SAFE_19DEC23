// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for OrganizationalPositionRelationship from entity to dto
    /// </summary>
    public class OrganizationalPositionRelationshipEntityToDtoAdapter : AutoMapperAdapter<OrganizationalPositionRelationship, Ellucian.Colleague.Dtos.Base.OrganizationalPositionRelationship>
    {

        /// <summary>
        /// Initializes an <see cref="OrganizationalPositionRelationshipEntityToDtoAdapter"/> adapter.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public OrganizationalPositionRelationshipEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<OrganizationalPersonPositionBase, Ellucian.Colleague.Dtos.Base.OrganizationalPersonPositionBase>();
        }
    }
}
