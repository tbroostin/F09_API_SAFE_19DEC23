// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to define criteria for academic credit queries
    /// </summary>
    public class AcademicCreditQueryCriteria
    {
        /// <summary>
        /// Section Id for which credits are requested. 
        /// Must provide at least one section Id to query academic credits.
        /// </summary>
        public List<string> SectionIds { get; set; }
        /// <summary>
        /// Determines whether dropped and withdrawn student academic credits should be included
        /// </summary>
        public List<CreditStatus> CreditStatuses { get; set; }
        /// <summary>
        /// If true, academic credits for any cross listed sections will also be included
        /// </summary>
        public bool IncludeCrossListedCredits { get; set; }
       
    }
}
