/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface to an AcademicProgressRepository
    /// </summary>
    public interface IAcademicProgressRepository
    {
        /// <summary>
        /// Get AcademicProgressEvaluation Entities from database data
        /// </summary>
        /// <param name="studentId">Colleague PERSON id of the student</param>
        /// <returns>A student's AcademicProgressEvaluation entities</returns>
        Task<IEnumerable<AcademicProgressEvaluationResult>> GetStudentAcademicProgressEvaluationResultsAsync(string studentId);

        /// <summary>
        /// Get ProgramDetail for the academic progress evaluation
        /// </summary>
        /// <param name="programCode">Program code</param>
        /// <param name="catalog">Program Catalog</param>
        /// <returns></returns>
        Task<AcademicProgressProgramDetail> GetStudentAcademicProgressProgramDetailAsync(string programCode, string catalog);
    }
}
