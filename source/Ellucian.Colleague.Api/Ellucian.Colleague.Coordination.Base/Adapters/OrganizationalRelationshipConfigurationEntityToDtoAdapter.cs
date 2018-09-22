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
    /// Adapter for OrganizationalRelationshipConfiguration entity to DTO
    /// </summary>
    public class OrganizationalRelationshipConfigurationEntityToDtoAdapter: AutoMapperAdapter<Domain.Base.Entities.OrganizationalRelationshipConfiguration, Dtos.Base.OrganizationalRelationshipConfiguration> 
    {
        /// <summary>
        /// Organizational Relationship Configuration Entity to DTO Adapter constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public OrganizationalRelationshipConfigurationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) 
        {
        }

        /// <summary>
        /// Maps an OrganizationalRelationshipConfiguration entity to DTO
        /// </summary>
        /// <param name="source">OrganizationalRelationshipConfiguration entity</param>
        /// <returns>OrganizationalRelationshipConfiguration DTO</returns>
        public override Dtos.Base.OrganizationalRelationshipConfiguration MapToType(Domain.Base.Entities.OrganizationalRelationshipConfiguration source)
        {
            var dto = new Dtos.Base.OrganizationalRelationshipConfiguration()
            {
                RelationshipTypeCodeMapping = new Dictionary<Dtos.Base.OrganizationalRelationshipType, List<string>>()
            };
            foreach (var sourceTypeKeyValue in source.RelationshipTypeCodeMapping)
            {
                Dtos.Base.OrganizationalRelationshipType dtoRelationshipType;
                switch (sourceTypeKeyValue.Key)
                {
                    case Domain.Base.Entities.OrganizationalRelationshipType.Manager:
                        dtoRelationshipType = Dtos.Base.OrganizationalRelationshipType.Manager;
                        break;
                    default:
                        dtoRelationshipType = Dtos.Base.OrganizationalRelationshipType.Unknown;
                        break;
                }
                dto.RelationshipTypeCodeMapping.Add(dtoRelationshipType, sourceTypeKeyValue.Value);
            }
            return dto;

        }
    }
}
