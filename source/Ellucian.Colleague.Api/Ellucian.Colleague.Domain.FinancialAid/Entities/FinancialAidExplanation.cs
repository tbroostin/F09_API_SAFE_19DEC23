//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// FinancialAidExplanation entity holds information about
    /// financial aid explanations of different types
    /// </summary>
    [Serializable]
    public class FinancialAidExplanation
    {
        /// <summary>
        /// Explanation type: e.g. PellLEU
        /// </summary>
        public FinancialAidExplanationType ExplanationType { get; set; }

        /// <summary>
        /// Explanation text
        /// </summary>
        public string ExplanationText { get; set; }

        /// <summary>
        /// Constructor that accepts an explanation string and type
        /// </summary>
        /// <param name="explanation">explanation text</param>
        /// <param name="type">explanaton type, e.g. PellLEU</param>
        public FinancialAidExplanation(string explanation, FinancialAidExplanationType type)
        {
            if (string.IsNullOrEmpty(explanation))
            {
                throw new ArgumentNullException("explanation");
            }
            ExplanationText = explanation;
            ExplanationType = type;
        }
    }
}
