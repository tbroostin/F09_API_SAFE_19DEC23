using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IHousingRequestService : IBaseService
    {
        /// <summary>
        /// Gets all housing requests.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.HousingRequest>, int>> GetHousingRequestsAsync(int offset, int limit, bool bypassCache = false);
        
        /// <summary>
        /// Gets housing request by guid.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.HousingRequest> GetHousingRequestByGuidAsync(string id, bool bypassCache = false);

        /// <summary>
        /// Updates housing request.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="housingRequest"></param>
        /// <returns></returns>
        Task<Dtos.HousingRequest> UpdateHousingRequestAsync(string guid, Dtos.HousingRequest housingRequest);

        /// <summary>
        /// Creates housing requests.
        /// </summary>
        /// <param name="housingRequest"></param>
        /// <returns></returns>
        Task<Dtos.HousingRequest> CreateHousingRequestAsync(Dtos.HousingRequest housingRequest);
    }
}
