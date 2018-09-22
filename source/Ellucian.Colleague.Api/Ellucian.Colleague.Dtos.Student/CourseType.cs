// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about the institutionally-defined course type
    /// </summary>
    public class CourseType
    {
        /// <summary>
        /// Unique code for this course type
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description for this course type
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Flag indicates whether this course type is a searchable or
        /// filterable type when searching for courses or course sections in the catalog.
        /// </summary>
        public bool ShowInCourseSearch { get; set; }
    }
}