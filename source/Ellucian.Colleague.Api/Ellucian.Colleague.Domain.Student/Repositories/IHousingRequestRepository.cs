// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IHousingRequestRepository : IEthosExtended
    {
        /// <summary>
        /// Gets paged housing requests
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<HousingRequest>, int>> GetHousingRequestsAsync(int offset, int limit, bool bypassCache);

        /// <summary>
        /// Gets housing request by guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<HousingRequest> GetHousingRequestByGuidAsync(string guid);

        /// <summary>
        /// Returns key for the housing request.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<string> GetHousingRequestKeyAsync(string guid);

        /// <summary>
        /// Gets all the guids for the person keys
        /// </summary>
        /// <param name="personRecordKeys"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetPersonGuidsAsync(IEnumerable<string> personRecordKeys);

        /// <summary>
        /// Create/Update housing request.
        /// </summary>
        /// <param name="housingRequestEntity"></param>
        /// <returns></returns>
        Task<HousingRequest> UpdateHousingRequestAsync(HousingRequest housingRequestEntity);
    }
}
