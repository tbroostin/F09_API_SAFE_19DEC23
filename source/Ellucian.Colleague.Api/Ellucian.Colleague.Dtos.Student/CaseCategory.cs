// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Attributes of Case Category related to Retention Alert
    /// </summary>
    public class CaseCategory
    {
        /// <summary>
        /// The Category Id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// Case Category code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Case Category description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A list of case type ids in this category
        /// </summary>
        public IEnumerable<string> CaseTypes { get; set; }

        /// <summary>
        /// A list of case closure reason in this category
        /// </summary>
        public IEnumerable<string> CaseClosureReasons { get; set; }

        /// <summary>
        /// Case worker email hierarchies
        /// </summary>
        public IEnumerable<string> CaseWorkerEmailHierarchy { get; set; }

    }
}





        


       
 

