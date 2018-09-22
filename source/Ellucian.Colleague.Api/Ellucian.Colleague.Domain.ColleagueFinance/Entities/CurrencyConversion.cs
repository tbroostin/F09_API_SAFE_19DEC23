// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Currency Conversion
    /// </summary>
    [Serializable]
    public class CurrencyConversion : CodeItem
    {

        public CurrencyCodes? CurrencyCode { get; set; }
  
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyConversion"/> class.
        /// </summary>
        public CurrencyConversion(string code, string description)
            : base(code, description)
        {
            
        }
    }
}
