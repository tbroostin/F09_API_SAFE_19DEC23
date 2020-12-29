// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Finance query filter criteria.
    /// </summary>
    public class FinanceQueryCriteria : BaseGlComponentQueryCriteria
    {
        /// <summary>
        /// A list of sort component criteria
        /// </summary>
        public List<FinanceQueryComponentSortCriteria> ComponentSortCriteria { get; set; }

        /// <summary>
        /// List of project reference no's to filter gl accounts.
        /// </summary>
        public List<string> ProjectReferenceNos { get; set; }        
    }

}

