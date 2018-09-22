//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.FinancialAid;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Student NSLDS Information service interface
    /// </summary>
    public interface IStudentNsldsInformationService
    {
        /// <summary>
        /// Gets student NSLDS related information for the specified student id
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <returns>StudentNsldsInformation DTO</returns>
        Task<StudentNsldsInformation> GetStudentNsldsInformationAsync(string studentId);
    }
}
