// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface to NonAcademicAttendanceRepository
    /// </summary>
    public interface INonAcademicAttendanceRepository
    {
        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person
        /// </summary>
        /// <param name="personId">Unique identifier for the person whose requirements are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person</returns>
        Task<IEnumerable<NonAcademicAttendanceRequirement>> GetNonacademicAttendanceRequirementsAsync(string personId);

        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendances">nonacademic events attended</see> for a person
        /// </summary>
        /// <param name="personId">Unique identifier for the person whose nonacademic attendances are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendances">nonacademic events attended</see> for a person</returns>
        Task<IEnumerable<NonAcademicAttendance>> GetNonacademicAttendancesAsync(string personId);
    }
}
