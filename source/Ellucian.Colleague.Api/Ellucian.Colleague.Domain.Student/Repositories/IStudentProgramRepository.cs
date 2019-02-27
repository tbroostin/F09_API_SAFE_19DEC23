// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentProgramRepository
    {
        /// <summary>
        /// Wrapper around async, used by FinancialAid AcademicProgressService
        /// </summary>
        /// <param name="program"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        IEnumerable<StudentProgram> Get(string studentid);
        Task<IEnumerable<StudentProgram>> GetAsync(string studentid);
        Task<StudentProgram> GetAsync(string studentid, string programid);
        Task<IEnumerable<EvaluationNotice>> GetStudentProgramEvaluationNoticesAsync(string studentId, string programCode);
        Task<IEnumerable<StudentProgram>> GetStudentProgramsByIdsAsync(IEnumerable<string> studentIds, bool includeInactivePrograms = false, Term term = null, bool includeHistory = false);
        Task<List<StudentProgram>> GetStudentAcademicPeriodProfileStudentProgramInfoAsync(List<string> stuProgIds);
    }
}