//Copyright 2015 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface to the StudentDefaultAwardPeriod Entities
    /// </summary>
    public interface IStudentDefaultAwardPeriodRepository
    {
        Task<IEnumerable<StudentDefaultAwardPeriod>> GetStudentDefaultAwardPeriodsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears);
    }
}
