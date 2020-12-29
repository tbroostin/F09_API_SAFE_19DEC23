//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Box codes.
    /// </summary>
    [Serializable]
    public class BoxCodes : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoxCodes"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public BoxCodes(string guid, string code, string description, string taxCode)
            : base(guid, code, description)
        {
            this.TaxCode = taxCode;
        }

        public string TaxCode { get; private set; }
    }
}