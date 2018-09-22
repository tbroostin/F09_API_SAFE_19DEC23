// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// <summary>
    /// Similar to a CreditResult, holds result of an evaluation of a planned course against a group
    /// </summary>    /// </summary>
    
    public class CourseResult:AcadResult
    {
        /// <summary>
        /// Id of the academic course
        /// </summary>
        public string CourseId { get; set; }
    }
}
