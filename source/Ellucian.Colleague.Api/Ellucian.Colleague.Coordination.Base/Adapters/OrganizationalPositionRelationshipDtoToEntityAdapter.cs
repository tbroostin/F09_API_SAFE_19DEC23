// Copyright 2017 Ellucian Company L.P. and its affiliates.
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
    /// Converts an OrganizationalPositionRelationship from a DTO to an Entity
    /// </summary>
    public class OrganizationalPositionRelationshipDtoToEntityAdapter : AutoMapperAdapter<Dtos.Base.OrganizationalPositionRelationship, Domain.Base.Entities.OrganizationalPositionRelationship>
    {
        public OrganizationalPositionRelationshipDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Converts an OrganizationalPositionRelationship from a DTO to an Entity
        /// </summary>
        /// <param name="source">The Organizational PositionRelationship DTO to convert</param>
        /// <returns>The Organizational PositionRelationship entity</returns>
        public override Domain.Base.Entities.OrganizationalPositionRelationship MapToType(Dtos.Base.OrganizationalPositionRelationship source)
        {
            var result = new Domain.Base.Entities.OrganizationalPositionRelationship(source.Id, source.OrganizationalPositionId, source.OrganizationalPositionTitle, source.RelatedOrganizationalPositionId, source.RelatedOrganizationalPositionTitle, source.RelationshipCategory);

            return result;
        }
    }
}
