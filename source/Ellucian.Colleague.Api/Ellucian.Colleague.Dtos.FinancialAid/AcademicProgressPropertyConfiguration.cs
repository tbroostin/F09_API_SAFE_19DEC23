/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Describes how to display a particular property of an academic progress evaluation
    /// </summary>
    public class AcademicProgressPropertyConfiguration
    {
        /// <summary>
        /// The type of the property this configuration describes
        /// </summary>
        public AcademicProgressPropertyType Type { get; set; }

        /// <summary>
        /// The Label used to identify the property for display purposes
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// A description of what this property is and means for a student
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether or not to show this property to the student
        /// </summary>
        public bool IsHidden { get; set; }

    }
}
