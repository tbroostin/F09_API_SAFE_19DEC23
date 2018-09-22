using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Repository for Buyers
    /// </summary>
    public interface IBuyerRepository
    {

        /// <summary>
        /// Get a buyer by guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<Buyer> GetBuyerAsync(string guid);

        /// <summary>
        /// Get a collection of buyers
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Buyer>, int>> GetBuyersAsync(int offset, int limit, bool bypassCache);

        /// <summary>
        /// Get buyers Guid from Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>string id</returns>
        Task<string> GetBuyerGuidFromIdAsync(string id);

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        Task<string> GetBuyerIdFromGuidAsync(string guid);
    }
}
