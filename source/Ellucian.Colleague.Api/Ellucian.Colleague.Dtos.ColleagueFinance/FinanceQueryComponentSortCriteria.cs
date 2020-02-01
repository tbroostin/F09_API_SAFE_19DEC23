// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{/// <summary>
///  Finance Query GL Component Sort Criteria
/// </summary>
    public class FinanceQueryComponentSortCriteria
    {
       /// <summary>
       /// Component name
       /// </summary>
        public string ComponentName { get; set; }
        /// <summary>
        /// Order of the sort for the component
        /// </summary>
        public short Order { get; set; }
        /// <summary>
        /// Flag to determine whether subtotalling is also required/displayed for this sort component
        /// </summary>
        public bool? IsDisplaySubTotal { get; set; }

    }
}
