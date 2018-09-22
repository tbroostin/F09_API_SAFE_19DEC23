


//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// MilStatuses
    /// </summary>
    [Serializable]
    public class MilStatuses : GuidCodeItem
    {

        public VeteranStatusCategory? Category { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilStatuses"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public MilStatuses(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}