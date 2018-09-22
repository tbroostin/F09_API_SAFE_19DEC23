// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Used to represent the special processing codes available on UT VALCODE RELATIONSHIP.CATEGORIES
    /// </summary>
    [Serializable]
    public enum OrganizationalRelationshipType
    {
        /// <summary>
        /// Represents a Subordinate-Manager relationship
        /// Special Processing 1: ORG
        /// </summary>
        Manager
    }
}
