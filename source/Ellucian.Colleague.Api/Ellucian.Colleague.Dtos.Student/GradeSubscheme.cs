// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A grade subscheme
    /// </summary>
    public class GradeSubscheme
    {
        /// <summary>
        /// Grade Subscheme Code
        /// </summary>        
        public string Code { get; set; }

        /// <summary>
        /// Grade Subscheme Description
        /// </summary>        
        public string Description { get; set; }

        ///<summary>
        /// Grade Codes assigned to the grade subscheme
        /// </summary>
        public IEnumerable<string> GradeCodes { get; set; }


    }
}
