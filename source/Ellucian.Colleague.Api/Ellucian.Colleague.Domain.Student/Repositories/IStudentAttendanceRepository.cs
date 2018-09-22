// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Repository for Student Attendance data
    /// </summary>
    public interface IStudentAttendanceRepository
    {

        /// <summary>
        /// Query Student Attendance Information
        /// </summary>
        /// <param name="sectionIds">list of section Ids</param>
        /// <param name="studentId">student Id (optional)</param>
        /// <param name="date">Attendance date (optional)</param>
        /// <returns>List of student attendances for given sectionIds and studentId.
        /// If studentId is provided then only attendance for that studentId is returned for the sections provided.
        /// If StudentId is provided then attendance is only returned for the sections from a list to which student is registered for
        Task<IEnumerable<StudentAttendance>> GetStudentAttendancesAsync(List<string> sectionIds, DateTime? date);

        /// <summary>
        /// Updates a student attendance record for a student, section, meeting time.
        /// </summary>
        /// <param name="studentAttendance">The Student Attendance to update.</param>
        /// <param name="sectionMeetingInstances">The calendar schedules (Meeting instances) for the particular sectoin</param>
        /// <returns>The updated student attendance</returns>
        Task<StudentAttendance> UpdateStudentAttendanceAsync(StudentAttendance studentAttendance, IEnumerable<SectionMeetingInstance> sectionMeetingInstances);

        /// <summary>
        /// Updates student attendance information for a specific meeting of a specific section
        /// </summary>
        /// <param name="sectionAttendance">The Section Attendance to update.</param>
        /// <returns>The updated section attendance</returns>
        Task<SectionAttendanceResponse> UpdateSectionAttendanceAsync(SectionAttendance sectionAttendance, List<string> crosslistSectionIds);
       
        /// <summary>
        /// Query Student Section Attendances Information.
        /// StudentId must be provided.
        /// If sectionsIds is not provided then attendances from all the sections for the given studentId is retrieved.
        /// If sectionIds is provided then attendances from only those sections which are in list and belongs to given student are retrieved.
        /// </summary>
        /// <param name="sectionIds">list of section Ids(optional)</param>
        /// <param name="studentId">student Id </param>
        /// <returns ><see cref="StudentSectionsAttendances"/> SectionWise Student Attendances </returns>
        Task<StudentSectionsAttendances> GetStudentSectionAttendancesAsync(string studentId, IEnumerable<string> sectionIds);


    }
}
