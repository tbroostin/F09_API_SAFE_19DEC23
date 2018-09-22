// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Organizational Position
    /// </summary>
    public class OrganizationalPosition
    {
        /// <summary>
        /// Organizational Position Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Organizational Position Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Relationships containing this position
        /// </summary>
        public IEnumerable<OrganizationalPositionRelationship> Relationships { get; set; }

    }
   
}
