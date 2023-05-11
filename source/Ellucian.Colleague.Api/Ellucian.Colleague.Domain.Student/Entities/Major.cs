// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Majors Code Table with additional fields
    /// </summary>
    [Serializable]
    public class Major : CodeItem
    {
        /// <summary>
        /// Division Code assigned to this Major
        /// </summary>
        public string DivisionCode { get; set; }
        /// <summary>
        /// Boolean Flag for active Majors
        /// </summary>
        public bool ActiveFlag { get; set; }
        /// <summary>
        /// Federal Course Classification assigned to this Major
        /// </summary>
        public string FederalCourseClassification { get; set; }
        /// <summary>
        /// List of Local Course Classifications assigned to this Major
        /// </summary>
        public List<string> LocalCourseClassifications { get; set; }

        public Major(string code, string desc)
            : base(code, desc)
        {

        }
    }
}
