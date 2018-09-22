// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Used to pass query criteria to retrieve financial aid
    /// counselors data
    /// </summary>
    public class FinancialAidCounselorQueryCriteria
    {
        /// <summary>
        /// List of fa counselor ids to retrieve data for
        /// </summary>
        public IEnumerable<string> FinancialAidCounselorIds { get; set; }
    }
}
