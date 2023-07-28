// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IAidApplicationsRepository : IEthosExtended
    {
        /// <summary>
        /// Get Aid Applications by Id
        /// </summary>
        /// <param name="id">Aid Applications Id</param>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>AidApplications/returns>
        Task<AidApplications> GetAidApplicationsByIdAsync(string id);


        /// <summary>
        /// Get a collection of AidApplications
        /// </summary>
        /// <returns>Collection of AidApplications</returns>
        Task<Tuple<IEnumerable<AidApplications>, int>> GetAidApplicationsAsync(int offset, int limit, string appDemoId, string personId = "", string aidApplicationType = "", string aidYear = "", string applicantAssignedId = "");


        /// <summary>
        /// Create a new entity record for Aid applications
        /// </summary>
        /// <param name="aidApplications"></param>
        /// <returns></returns>
        Task<AidApplications> CreateAidApplicationsAsync(AidApplications aidApplications);

        /// <summary>
        /// Update an excisting record for Aid applications
        /// </summary>
        /// <param name="aidApplications"></param>
        /// <returns></returns>
        Task<AidApplications> UpdateAidApplicationsAsync(AidApplications aidApplications);

    }
}

