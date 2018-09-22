//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    public interface IStudentNsldsInformationRepository
    {
        /// <summary>
        /// Gets StudentNsldsInformation for the specified student
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <returns>StudentNsldsInformation entity</returns>
        Task<StudentNsldsInformation> GetStudentNsldsInformationAsync(string studentId);
    }
}
