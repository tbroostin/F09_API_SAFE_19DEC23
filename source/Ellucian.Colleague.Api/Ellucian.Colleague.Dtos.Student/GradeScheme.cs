// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A grade scheme
    /// </summary>
    public class GradeScheme
    {
        /// <summary>
        /// Grade Scheme Code
        /// </summary>        
        public string Code { get; set; }

        /// <summary>
        /// Grade Scheme Description
        /// </summary>        
        public string Description { get; set; }

        ///<summary>
        /// Grade Codes assigned to the grade scheme
        /// </summary>
        public IEnumerable<string> GradeCodes { get; set; }


    }
}
