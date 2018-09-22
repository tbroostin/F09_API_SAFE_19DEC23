//Copyright 2014-2015 Ellucian Company L.P. and its affiliates
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface for the FinancialAidCounselorService
    /// </summary>
    public interface IFinancialAidCounselorService
    {
        /// <summary>
        /// Get a FinancialAidCounselor DTO for the given counselorId
        /// </summary>
        /// <param name="counselorId">The Colleague PERSON id of the counselor to get</param>
        /// <returns>FinancialAidCounselor DTO</returns>
        FinancialAidCounselor GetCounselor(string counselorId);

        /// <summary>
        /// Gets a list of FinancialAidCounselor DTOs for given counselor ids
        /// </summary>
        /// <param name="counselorIds">List of counselor ids</param>
        /// <returns>List of FinancialAidCounselor DTOs</returns>
        Task<IEnumerable<FinancialAidCounselor>> GetCounselorsByIdAsync(IEnumerable<string> counselorIds);
    }
}
