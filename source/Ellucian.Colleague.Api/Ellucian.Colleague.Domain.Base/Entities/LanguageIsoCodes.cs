
//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Languages
    /// </summary>
    [Serializable]
    public class LanguageIsoCodes : GuidCodeItem
    {
        /// <summary>
        /// Inactive Flag
        /// </summary>
        public string InactiveFlag { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageIsoCodes"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public LanguageIsoCodes(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}