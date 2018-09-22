/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.FinancialAid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface to an AcademicProgressService
    /// </summary>
    public interface IAcademicProgressService
    {
        /// <summary>
        /// Get a student's AcademicProgressEvaluation objects
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <returns>An enumerable of AcademicProgressEvaluation DTOs</returns>
        [Obsolete("Obsolete as of API 1.14. Use GetAcademicProgressEvaluations2Async")]
        Task<IEnumerable<AcademicProgressEvaluation>> GetAcademicProgressEvaluationsAsync(string studentId);

        /// <summary>
        /// Get student's AcademicProgressEvaluation2 objects
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <returns>An enumerable of AcademicProgressEvaluation2 DTOs</returns>
        Task<IEnumerable<AcademicProgressEvaluation2>> GetAcademicProgressEvaluations2Async(string studentId);
    }
}
