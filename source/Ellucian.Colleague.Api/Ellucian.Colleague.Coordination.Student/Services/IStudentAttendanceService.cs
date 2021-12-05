// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for managing student attendances
    /// </summary>
    public interface IStudentAttendanceService
    {
        /// <summary>
        /// Retrieves student attendance items based on criteria asynchronously
        /// </summary>
        /// <param name="criteria">Student attendance query criteria</param>
        /// <param name="useCache">Use cached course section data when retrieving academic credit records of specified section(s)</param>
        /// <returns>List of Student Attendances</returns>
        Task<IEnumerable<StudentAttendance>> QueryStudentAttendancesAsync(StudentAttendanceQueryCriteria criteria, bool useCache = true);
        /// <summary>
        /// Updates a student attendance record for a student, section, meeting time.
        /// </summary>
        /// <param name="studentAttendance">The Student Attendance to update.</param>
        /// <returns>The updated student attendance</returns>
        Task<StudentAttendance> UpdateStudentAttendanceAsync(StudentAttendance studentAttendance);
        /// <summary>
        /// Updates student attendance information for students in a specific meeting instance of a section. 
        /// </summary>
        /// <param name="sectionAttendance">The Section Attendance to update.</param>
        /// <returns>The updated section attendance</returns>
        Task<SectionAttendanceResponse> UpdateSectionAttendance2Async(SectionAttendance sectionAttendance);
        /// <summary>
        /// Updates student attendance information for students in a specific meeting instance of a section. 
        /// </summary>
        /// <param name="sectionAttendance">The Section Attendance to update.</param>
        /// <returns>The updated section attendance</returns>
        Task<SectionAttendanceResponse> UpdateSectionAttendanceAsync(SectionAttendance sectionAttendance);
    }
}
