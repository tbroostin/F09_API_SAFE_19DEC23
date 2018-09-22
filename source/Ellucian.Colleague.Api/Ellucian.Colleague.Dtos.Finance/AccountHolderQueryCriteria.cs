// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Used to pass query criteria to retrieve account holder data
    /// </summary>
    public class AccountHolderQueryCriteria
    {
        /// <summary>
        /// List of account holder ids to retrieve data for
        /// </summary>
        public IEnumerable<string> Ids { get; set; }
        /// <summary>
        /// Used when requesting a search of account holders by name or Id. 
        /// Either a Person ID or a first and last name is required. A middle name is optional.
        /// </summary>
        public string QueryKeyword { get; set; }
    }
}
