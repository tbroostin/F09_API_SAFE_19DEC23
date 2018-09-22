// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A code representing the relationship between two people or organizations
    /// </summary>
    public class RelationshipType
    {
        /// <summary>
        /// Unique system Id for the relationship type
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of the relationship type
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Unique system Id for the inverse relationship type
        /// </summary>
        public string InverseCode { get; set; }
    }
}
