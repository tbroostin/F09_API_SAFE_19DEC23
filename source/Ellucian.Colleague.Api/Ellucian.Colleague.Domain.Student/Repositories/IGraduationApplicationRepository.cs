// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IGraduationApplicationRepository
    {
        /// <summary>
        /// Creates a graduation Application for a specific student and program asynchronously.
        /// </summary>
        /// <param name="graduationApplication">The graduation application object to add</param>
        /// <returns><see cref="GraduationApplication"/>The graduation application retrieved after it was added</returns>
        Task<GraduationApplication> CreateGraduationApplicationAsync(GraduationApplication graduationApplication);

        /// <summary>
        /// Returns the requested graduation application for given student Id and program Code asynchronously.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="programCode">Program Code</param>
        /// <returns> <see cref="GraduationApplication"/>The requested graduation application</returns>
        Task<GraduationApplication> GetGraduationApplicationAsync(string studentId, string programCode);

        /// <summary>
        /// Returns the list of  graduation applications for given student Id asynchronously.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>list of <see cref="GraduationApplication"/>graduation applications</returns>
        Task<List<GraduationApplication>> GetGraduationApplicationsAsync(string studentId);

        /// <summary>
        /// Updates a graduation Application for a specific student and program asynchronously.
        /// </summary>
        /// <param name="graduationApplication">The graduation application object to update</param>
        /// <returns><see cref="GraduationApplication"/>The graduation application retrieved after it was updated</returns>
        Task<GraduationApplication> UpdateGraduationApplicationAsync(GraduationApplication graduationApplication);

        /// <summary>
        /// Returns the graduation application fee for given student Id and program Code asynchronously.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="programCode">Program Code</param>
        /// <returns> <see cref="GraduationApplicationFee"/>The requested graduation application fee information</returns>
        Task<GraduationApplicationFee> GetGraduationApplicationFeeAsync(string studentId, string programCode);
    }
}
