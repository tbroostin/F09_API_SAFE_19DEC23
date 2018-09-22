//Copyright 2015 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.FinancialAid;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface method
    /// </summary>
    public interface IStudentDefaultAwardPeriodService
    {
        Task<IEnumerable<StudentDefaultAwardPeriod>> GetStudentDefaultAwardPeriodsAsync(string studentId);
    }
}
