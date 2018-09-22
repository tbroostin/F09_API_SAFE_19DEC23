// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A code representing the relationship between two people or organizations
    /// </summary>
    [Serializable]
    public class RelationshipType : CodeItem
    {
        private string _inverseCode;

        /// <summary>
        /// The code denoting the reverse of the relationship (e.g., parent for child)
        /// </summary>
        public string InverseCode { get { return _inverseCode; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipType"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="inverseCode">The code for the inverse relationship.</param>
        public RelationshipType(string code, string description, string inverseCode)
            : base(code, description)
        {
            _inverseCode = string.IsNullOrEmpty(inverseCode) ? code : inverseCode;
        }
    }
}
