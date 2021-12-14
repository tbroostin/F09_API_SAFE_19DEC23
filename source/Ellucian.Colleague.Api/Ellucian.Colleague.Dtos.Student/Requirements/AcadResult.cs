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
    /// base class for CourseResult and CreditResult Dtos.
    /// </summary>
    
    public abstract class AcadResult
    {
       
        /// <summary>
        /// explananation for Acad credit or course. This implies whether acad credit/course that is applied is 'Extra' to requirement completion
        /// </summary>
        public AcadResultExplanation Explanation { get; set; }
        /// <summary>
        /// Replaced Status
        /// </summary>
        public ReplacedStatus ReplacedStatus { get; set; }
        /// <summary>
        /// Replacement Status
        /// </summary>
        public ReplacementStatus ReplacementStatus { get; set; }
       

    }

    /// <summary>
    /// enum to define explananation for Acad credit or course. This implies whether acad credit/course that is applied is 'Extra' to requirement completion.
    /// Default Value is 'None'.
    /// MinGrade indicates that the credit hasn't met MinGrade syntax. Such credit will not be applied but will remain related.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AcadResultExplanation
    {
        /// <summary>
        /// none
        /// </summary>
        None,
        /// <summary>
        /// extra
        /// </summary>
        Extra,
        /// <summary>
        /// To indicate that academic credit was not applied because it fails to meet Minimum Grade 
        /// </summary>
        MinGrade

    }
}
