//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// TaxForms
    /// </summary>
    [Serializable]
    public class TaxForms2 : GuidCodeItem
    {

        public string defaultTaxBox { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TaxForms"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public TaxForms2(string guid, string code, string description, string sp1)
            : base(guid, code, description)
        {
            defaultTaxBox = sp1;
        }
    }
}