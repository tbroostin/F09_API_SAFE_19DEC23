/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    public interface IStudentBudgetComponentRepository
    {
        Task<IEnumerable<StudentBudgetComponent>> GetStudentBudgetComponentsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears);
    }
}
