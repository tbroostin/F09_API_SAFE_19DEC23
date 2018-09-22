// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// The amount and currency code for a general ledger transaction
    /// used within the Higher Education Data Model.
    /// </summary>
    [Serializable]
    public class AmountAndCurrency
    {
        /// <summary>
        /// The monetary value for the specified currency.
        /// </summary>
        public decimal? Value { get; private set; }

        /// <summary>
        /// The ISO 4217 currency code
        /// </summary>
        public CurrencyCodes Currency { get; private set; }

        public AmountAndCurrency(decimal? value, CurrencyCodes? currency)
        {
            if (value == 0)
            {
                throw new ArgumentNullException("value", "The amount value must be a included.");
            }
            if (currency == null)
            {
                currency = CurrencyCodes.USD;
            }

            Value = value;
            Currency = currency.GetValueOrDefault();
        }
    }
}