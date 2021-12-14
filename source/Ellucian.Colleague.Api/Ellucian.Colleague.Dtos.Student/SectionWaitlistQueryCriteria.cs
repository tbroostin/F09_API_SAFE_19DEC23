// Copyright 2021 Ellucian Company L.P. and its affiliates

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to define criteria for waitlist related queries
    /// </summary>
    public class SectionWaitlistQueryCriteria
    {
        /// <summary>
        /// Section Ids for which waitlist data is requested. 
        /// </summary>
        public List<string> SectionIds { get; set; }

        /// <summary>
        /// If true, waitlist data for any cross listed sections will also be included
        /// </summary>
        public bool IncludeCrossListedSections { get; set; }

    }
}

