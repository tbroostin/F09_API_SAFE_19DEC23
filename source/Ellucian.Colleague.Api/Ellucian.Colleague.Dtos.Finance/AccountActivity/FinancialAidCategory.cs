// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Financial aid awarded and anticipated
    /// </summary>
    public partial class FinancialAidCategory
    {
        /// <summary>
        /// List of <see cref="ActivityFinancialAidItem">anticipated aid</see>
        /// </summary>
        public List<ActivityFinancialAidItem> AnticipatedAid { get; set; }

        /// <summary>
        /// List of <see cref="ActivityDateTermItem">disbursed aid</see>
        /// </summary>
        public List<ActivityDateTermItem> DisbursedAid { get; set; }
        
    }
}