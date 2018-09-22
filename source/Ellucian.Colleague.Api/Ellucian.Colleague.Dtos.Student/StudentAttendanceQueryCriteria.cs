// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to define criteria for student attendances queries
    /// </summary>
    public class StudentAttendanceQueryCriteria
    {
        /// <summary>
        /// Section Id for which attendances are requested. 
        /// Must provide at least one section Id to query student attendances.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// If true, include attendances for any cross listed sections 
        /// </summary>
        public bool IncludeCrossListedAttendances { get; set; }

        /// <summary>
        /// Date for which attendances are requested. If null, attendances for all dates for this section will be returned
        /// </summary>
        public DateTime? AttendanceDate { get; set; }
       
    }
}
