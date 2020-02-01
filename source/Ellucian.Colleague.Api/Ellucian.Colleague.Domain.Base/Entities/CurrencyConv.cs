// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Currency Conversion
    /// </summary>
    [Serializable]
    public class CurrencyConv : GuidCodeItem
    {
        /// <summary>
        /// Iso Code
        /// </summary>
        public string IsoCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyConv"/> class.
        /// </summary>
        public CurrencyConv(string guid, string code, string description, string isoCode)
            : base(guid, code, description)
        {
            IsoCode = isoCode;
        }
    }
}
