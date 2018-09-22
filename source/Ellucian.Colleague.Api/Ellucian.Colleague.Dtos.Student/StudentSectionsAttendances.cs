// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains section wise attendance information for a specific student
    /// </summary>
    public class StudentSectionsAttendances
    {
        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// SectionWise Attendances
        /// </summary>
        public IDictionary<string, List<StudentAttendance>> SectionWiseAttendances { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public StudentSectionsAttendances()
        {
            SectionWiseAttendances = new Dictionary<string, List<StudentAttendance>>();
        }
    }
}
