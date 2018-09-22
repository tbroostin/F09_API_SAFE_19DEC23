// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Address Types
    /// </summary>
    [Serializable]
    public class AddressType 
    {
        /// <summary>
        /// The <see cref="EntityType">entity type</see> for the address type
        /// </summary>
        private EntityType _entityType;
        public EntityType EntityType { get { return _entityType; } }

        /// <summary>
        /// The <see cref="PersonAddressType">type</see> of address type for this entity
        /// </summary>
        private PersonAddressType _personAddressType;
        public PersonAddressType PersonAddressType { get { return _personAddressType; } }

        /// <summary>
        /// The <see cref="OrganizationAddressType">type</see> of address type for this entity
        /// </summary>
        private OrganizationAddressType _organizationAddressType;
        public OrganizationAddressType OrganizationAddressType { get { return _organizationAddressType; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressType"/> class.
        /// </summary>
         public AddressType(EntityType entityType, PersonAddressType personAddressType, OrganizationAddressType organizationAddressType)
         {    
            _entityType = entityType;
            _personAddressType = personAddressType;
            _organizationAddressType = organizationAddressType;
        }
    }
}
