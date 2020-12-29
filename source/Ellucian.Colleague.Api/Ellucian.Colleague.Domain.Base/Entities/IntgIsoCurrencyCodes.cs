//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// IntgIsoCurrencyCodes
    /// </summary>
    [Serializable]
    public class IntgIsoCurrencyCodes : GuidCodeItem
    {
        // active/inactive flag
        public string Status { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntgIsoCurrencyCodes"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="description">The status.</param>
        public IntgIsoCurrencyCodes(string guid, string code, string description, string status)
            : base(guid, code, description)
        {
            Status = status;
        }
    }
}