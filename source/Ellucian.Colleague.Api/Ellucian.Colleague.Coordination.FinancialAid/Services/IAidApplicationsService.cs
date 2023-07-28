/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface to an AidApplicationsService
    /// </summary>
    public interface IAidApplicationsService : IBaseService
    {
        /// <summary>
        /// Get a aid applications by ID.
        /// </summary>
        /// <param name="id">ID of the aid applications in Colleague.</param>
        /// <returns>The <see cref="AidApplications">aidApplications</see></returns>
        Task<AidApplications> GetAidApplicationsByIdAsync(string id);

        /// <summary>
        /// Gets all aid applications matching the criteria
        /// </summary>
        /// <returns>Collection of <see cref="AidApplications">aidApplications</see> objects</returns>
        Task<Tuple<IEnumerable<AidApplications>, int>> GetAidApplicationsAsync(int offset, int limit, AidApplications criteriaFilter);

        /// <summary>
        /// Create new aid applications record
        /// </summary>
        /// <param name="aidApplicationsDto"></param>
        /// <returns></returns>
        Task<AidApplications> PostAidApplicationsAsync(AidApplications aidApplicationsDto);

        /// <summary>
        /// Update the aid applications record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="aidApplicationsDto"></param>
        /// <returns></returns>
        Task<AidApplications> PutAidApplicationsAsync(string id, AidApplications aidApplicationsDto);

    }
}
