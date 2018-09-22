// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Location Type
    /// </summary>
    [Serializable]
    public class LocationTypeItem : GuidCodeItem
    {
        /// <summary>
        /// The <see cref="LocationType">location type object</see> for the address
        /// </summary>
        private LocationType _type;
        public LocationType Type { get { return _type; } }
        
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationTypeItem"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier for the Location type item</param>
        /// /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="entityType">The entity type</param>
        /// <param name="personEmailType">The related Person Location Type</param>
        /// <param name="organizationEmailType">The related Organization Location Type</param>
        public LocationTypeItem(string guid, string code, string description,
            EntityType entityType, PersonLocationType personLocationType, OrganizationLocationType organizationLocationType)
            : base(guid, code, description)
        {
            _type = new LocationType(entityType, personLocationType, organizationLocationType);
        }
    }
}
