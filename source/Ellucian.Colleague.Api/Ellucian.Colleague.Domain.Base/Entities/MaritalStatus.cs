// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Marital status codes
    /// </summary>
    [Serializable]
    public class MaritalStatus : GuidCodeItem
    {
        /// <summary>
        /// The type of martial status
        /// </summary>
        public MaritalStatusType? Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaritalStatus"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public MaritalStatus(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}