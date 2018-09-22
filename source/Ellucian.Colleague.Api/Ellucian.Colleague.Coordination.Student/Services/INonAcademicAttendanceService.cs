// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface to NonAcademicAttendanceService
    /// </summary>
    public interface INonAcademicAttendanceService
    {
        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person
        /// </summary>
        /// <param name="personId">Unique identifier for the person whose requirements are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person</returns>
        Task<IEnumerable<NonAcademicAttendanceRequirement>> GetNonAcademicAttendanceRequirementsAsync(string personId);

        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendance">nonacademic events attended</see> for a person
        /// </summary>
        /// <param name="personId">Unique identifier for the person whose nonacademic attendances are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendance">nonacademic events attended</see> for a person</returns>
       Task<IEnumerable<NonAcademicAttendance>> GetNonAcademicAttendancesAsync(string personId);
    }
}
