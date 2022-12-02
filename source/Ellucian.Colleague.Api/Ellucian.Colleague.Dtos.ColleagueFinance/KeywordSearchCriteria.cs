// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Keyword Search criteria object.
    /// </summary>
    public class KeywordSearchCriteria
    {
        /// <summary>
        /// List of Record IDs.
        /// </summary>
        public List<string> Ids { get; set; }


        /// <summary>
        /// Search term.
        /// </summary>
        public string Keyword { get; set; }
    }
}
