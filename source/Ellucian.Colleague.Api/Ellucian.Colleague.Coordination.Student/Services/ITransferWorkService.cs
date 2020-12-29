// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student.TransferWork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for student transfer and non course equivalency work
    /// </summary>
    public interface ITransferWorkService
    {
        /// <summary>
        /// Get student transfer equivalency work for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Returns a list of transfer equivalencies for a student.</returns>
        Task<IEnumerable<TransferEquivalencies>> GetStudentTransferWorkAsync(string studentId);
    }
}
