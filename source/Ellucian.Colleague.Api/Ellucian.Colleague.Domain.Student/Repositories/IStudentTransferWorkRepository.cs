// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.TransferWork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for the student transfer and non course equivalency work repository
    /// </summary>
    public interface IStudentTransferWorkRepository
    {
        /// <summary>
        /// Get student transfer and non course equivalency work for a student
        /// </summary>
        /// <param name="studentId">Student Id of the student to retrieve the equivalent work.</param>
        /// <returns>Returns a list of institutional transfer and non course equivalency work for a student.</returns>
        Task<IEnumerable<TransferEquivalencies>> GetStudentTransferWorkAsync(string studentId);
    }
}
