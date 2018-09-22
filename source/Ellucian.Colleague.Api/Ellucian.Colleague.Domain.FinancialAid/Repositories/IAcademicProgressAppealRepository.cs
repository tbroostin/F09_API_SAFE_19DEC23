/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    public interface IAcademicProgressAppealRepository
    {
        /// <summary>
        /// Gets a set of academic progress appeals for the student
        /// </summary>
        /// <param name="studentId">The Colleague student id</param>
        /// <returns>List of Academic Progress Appeal records</returns>
        Task<IEnumerable<AcademicProgressAppeal>> GetStudentAcademicProgressAppealsAsync(string studentId);
    }
}
