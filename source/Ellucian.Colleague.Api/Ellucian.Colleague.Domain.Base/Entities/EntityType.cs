// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of entity types
    /// </summary>
    [Serializable]
    public enum EntityType
    {
        /// <summary>
        /// A contact who is a person.
        /// </summary>
        Person,

        /// <summary>
        /// A contact that is an organization.
        /// </summary>
        Organization
    }
}
