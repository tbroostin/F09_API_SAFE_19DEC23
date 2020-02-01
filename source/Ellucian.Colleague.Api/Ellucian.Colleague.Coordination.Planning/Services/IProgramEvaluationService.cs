// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Planning.Services
{
    /// <summary>
    /// This is a coordination service that collects data from the repositories and then calls a domain
    /// service to evaluate a student's degree requirements against their academic credits.
    /// </summary>
    public interface IProgramEvaluationService
    {
        /// <summary>
        /// Return a list of program evaluations for the given student and program
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCodes">List of program codes</param>
        /// /// <param name="catalogYear">The catalogYear code</param>
        /// <returns>A list of <see cref="ProgramEvaluation">ProgramEvaluation</see> objects</returns>
        Task<IEnumerable<Domain.Student.Entities.ProgramEvaluation>> EvaluateAsync(string studentid, List<string> programCodes, string catalogYear = null);

        /// <summary>
        /// Get program evaluation notices for the given student and program
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCode">Code of the program</param>
        /// <returns>List of evaluation notices for this student and program</returns>
        Task<IEnumerable<Dtos.Student.EvaluationNotice>> GetEvaluationNoticesAsync(string studentId, string programCode);
    }
}
