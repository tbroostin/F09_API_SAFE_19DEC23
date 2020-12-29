// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Contains vendor search criteria
    /// </summary>
    public class VendorSearchCriteria
    {

        /// <summary>
        /// Used when requesting a search of vendor by name or Id.
        /// </summary>
        public string QueryKeyword { get; set; }

        /// <summary>
        /// Used to determine the default tax form location for a vendor.
        /// </summary>
        public string ApType { get; set; }
    }
}
