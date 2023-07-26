// Copyright 2023 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Indicates the status of an academic progress evaluation
    /// </summary>
    public class HousingOption
    {
        /// <summary>
        /// The year for this HousingOption
        /// </summary>
        public string AwardYear { get; set; }
        /// <summary>
        /// Contains the category of the HousingOption
        /// </summary>
        public string HousingOptionCode { get; set; }
    }
}
