// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// This object defines optional graduation questions that are available for the graduation application and how they should be handled on the application.
    /// </summary>
    public class GraduationQuestion
    {
        /// <summary>
        /// Indicates the type of question so it can be properly placed on the graduation application
        /// </summary>
        public GraduationQuestionType Type { get; set; }
        /// <summary>
        /// Indicates whether an answer for this question is required in order to submit a graduation application
        /// </summary>
        public bool IsRequired { get; set; }
    }
}
