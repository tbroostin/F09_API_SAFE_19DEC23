// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Range of GL component values
    /// </summary>
    [Serializable]
    public class GeneralLedgerComponentRange
    {
        /// <summary>
        /// Component range start value.
        /// </summary>
        public string StartValue { get { return startValue; } }
        private readonly string startValue;

        /// <summary>
        /// Component range end value.
        /// </summary>
        public string EndValue { get { return endValue; } }
        private readonly string endValue;

        /// <summary>
        /// Initialize the component range ntity.
        /// </summary>
        /// <param name="startValue">Start value.</param>
        /// <param name="endValue">End value.</param>
        public GeneralLedgerComponentRange(string startValue, string endValue)
        {
            if (string.IsNullOrEmpty(startValue))
            {
                throw new ArgumentNullException("startValue", "Component range start value is a required field.");
            }

            if (string.IsNullOrEmpty(endValue))
            {
                throw new ArgumentNullException("endValue", "Component range end value is a required field.");
            }

            var startValueUpperCase = startValue.ToUpperInvariant();
            var endValueUpperCase = endValue.ToUpperInvariant();
            if (String.Compare(startValueUpperCase, endValueUpperCase, false) > 0)
            {
                throw new ApplicationException("The start value must preceed the end value.");
            }

            this.startValue = startValueUpperCase;
            this.endValue = endValueUpperCase;
        }
    }
}