//Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Languages
    /// </summary>
    [Serializable]
    public class Language2 : GuidCodeItem
    {
        /// <summary>
        /// ISO code
        /// </summary>
        public string IsoCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Languages"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public Language2(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}