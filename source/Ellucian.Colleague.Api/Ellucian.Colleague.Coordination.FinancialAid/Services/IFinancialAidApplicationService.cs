/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface for the FinancialAidApplication
    /// </summary>    
    public interface IFinancialAidApplicationService
    {
        /// <summary>
        /// Invoke the Repository Method for getting FinancialAidApplications
        /// </summary>
        /// <param name="studentId">The student's system Ids</param>
        /// <returns>List of Financial Aid Application DTO objects</returns>
        [Obsolete("Obsolete as of API version 1.7. Deprecated. Get FinancialAidApplication2s using IFafsaService and IProfileApplicationService")]
        IEnumerable<FinancialAidApplication> GetFinancialAidApplications(string studentId);
    }
}
