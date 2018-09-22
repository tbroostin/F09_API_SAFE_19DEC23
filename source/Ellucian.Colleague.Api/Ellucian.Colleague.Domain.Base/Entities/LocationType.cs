// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Location Types
    /// </summary>
    [Serializable]
    public class LocationType 
    {
        /// <summary>
        /// The <see cref="EntityType">entity type</see> for the location type
        /// </summary>
        private EntityType _entityType;
        public EntityType EntityType { get { return _entityType; } }

        /// <summary>
        /// The <see cref="PersonLocationType">type</see> of location type for this entity
        /// </summary>
        private PersonLocationType _personLocationType;
        public PersonLocationType PersonLocationType { get { return _personLocationType; } }

        /// <summary>
        /// The <see cref="OrganizationLocationType">type</see> of location type for this entity
        /// </summary>
        private OrganizationLocationType _organizationLocationType;
        public OrganizationLocationType OrganizationLocationType { get { return _organizationLocationType; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationType"/> class.
        /// </summary>
         public LocationType(EntityType entityType, PersonLocationType personLocationType, OrganizationLocationType organizationLocationType)
         {    
            _entityType = entityType;
            _personLocationType = personLocationType;
            _organizationLocationType = organizationLocationType;
        }
    }
}
