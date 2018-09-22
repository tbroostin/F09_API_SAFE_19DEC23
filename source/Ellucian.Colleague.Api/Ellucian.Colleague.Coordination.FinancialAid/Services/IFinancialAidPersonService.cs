/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.FinancialAid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// FinancialAidPersonService interface
    /// </summary>
    public interface IFinancialAidPersonService
    {
        /// <summary>
        /// Searches for financial aid persons for the specified criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Set of Person DTOs</returns>
        Task<PrivacyWrapper<IEnumerable<Person>>> SearchFinancialAidPersonsAsync(FinancialAidPersonQueryCriteria criteria);
    }
}
