// Copyright 2018 Ellucian Company L.P. and its affiliates.using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for managing student attendances
    /// </summary>
    public interface IStudentSectionAttendancesService
    {
        /// <summary>
        /// Retrieves student section attendances Dto based on a set of criteria.  StudentId is required in criteria.
        /// This returns student attendances for the given studentId and the given sectionIds.
        /// If no sectionId is provided then attendances from all the student's sections are returned. 
        /// </summary>
        /// <param name="criteria">Object that contains the sections and studentId for which attendances are requested</param>
        /// <returns>A dto that contains list of section wise student attendances</returns>
        Task<StudentSectionsAttendances> QueryStudentSectionAttendancesAsync(StudentSectionAttendancesQueryCriteria criteria);
    }
}
