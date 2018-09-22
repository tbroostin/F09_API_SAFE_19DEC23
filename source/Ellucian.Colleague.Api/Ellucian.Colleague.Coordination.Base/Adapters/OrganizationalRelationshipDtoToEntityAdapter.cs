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
    /// Converts an OrganizationalRelationship from a DTO to an Entity
    /// </summary>
    public class OrganizationalRelationshipDtoToEntityAdapter : AutoMapperAdapter<Dtos.Base.OrganizationalRelationship, Domain.Base.Entities.OrganizationalRelationship>
    {
        public OrganizationalRelationshipDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Converts an OrganizationalRelationship from a DTO to an Entity
        /// </summary>
        /// <param name="source">The Organizational Relationship DTO to convert</param>
        /// <returns>The Organizational Relationship entity</returns>
        public override Domain.Base.Entities.OrganizationalRelationship MapToType(Dtos.Base.OrganizationalRelationship source)
        {
            var result = new Domain.Base.Entities.OrganizationalRelationship(source.Id, source.OrganizationalPersonPositionId, source.RelatedOrganizationalPersonPositionId, source.Category);

            return result;
        }
    }
}
