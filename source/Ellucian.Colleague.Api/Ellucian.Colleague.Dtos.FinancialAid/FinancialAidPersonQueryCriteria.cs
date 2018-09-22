// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Used to pass query criteria to retrieve financial aid
    /// persons (student/applicant) data
    /// </summary>
    public class FinancialAidPersonQueryCriteria
    {
        /// <summary>
        /// List of fa person ids to retrieve data for
        /// </summary>
        public IEnumerable<string> FinancialAidPersonIds { get; set; }

        /// <summary>
        /// Used when requesting a search of financial aid persons by name or Id. [last, first middle] or 
        /// [first middle last] or [last] or [Id] expected.
        /// Must contain at least 2 characters when provided.
        /// </summary>
        public string FinancialAidPersonQueryKeyword { get; set; }
    }
}
