// Copyright 2019 Ellucian Company L.P. and its affiliates
using System;
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A record of a faculty member indicating that midterm grading is complete.   
    /// </summary>
    public class GradingCompleteIndication
    {
        /// <summary>
        /// Person ID of the faculty member that indicated that midterm grading is complete
        /// </summary>
        public string CompleteOperator { get; set; }

        /// <summary>
        /// Date and time at which the faculty member indicated that midterm grading is complete
        /// </summary>
        public DateTimeOffset DateAndTime{ get; set; }

    }
}
