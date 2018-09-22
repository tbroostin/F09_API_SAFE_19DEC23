// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to define criteria for student section attendances queries
    /// </summary>
    public class StudentSectionAttendancesQueryCriteria
    {

        /// <summary>
        /// Student Id for which attendance is requested.
        /// Must be provided.
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Section Ids for which attendances are requested for the given StudentId. 
        /// It is an optional. 
        /// If no section is provided then by default attendances for all the sections that belong to given studentId are returned.
        /// </summary>
        public List<string> SectionIds { get; set; }
        /// <summary>
        /// constructor
        /// </summary>
        public StudentSectionAttendancesQueryCriteria()
        {
            SectionIds = new List<string>();
        }

    }
}
