// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    public interface IAidApplicationDemographicsRepository : IEthosExtended
    {
        /// <summary>
        /// Get Aid Application Demographics by Id
        /// </summary>
        /// <param name="id">Aid Application Demographics Id</param>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>AidApplicationDemographics/returns>
        Task<AidApplicationDemographics> GetAidApplicationDemographicsByIdAsync(string id);


        /// <summary>
        /// Get a collection of AidApplicationDemographics
        /// </summary>
        /// <returns>Collection of AidApplicationDemographics</returns>
        Task<Tuple<IEnumerable<AidApplicationDemographics>, int>> GetAidApplicationDemographicsAsync(int offset, int limit, string personId = "", string aidApplicationType="", string aidYear = "", string applicantAssignedId="");

        Task<AidApplicationDemographics> CreateAidApplicationDemographicsAsync(AidApplicationDemographics aidApplicationDemographics);

        Task<AidApplicationDemographics> UpdateAidApplicationDemographicsAsync(AidApplicationDemographics aidApplicationDemographics);

    }
}
