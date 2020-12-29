//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for VendorAddressUsages services
    /// </summary>
    public interface IVendorAddressUsagesService : IBaseService
    {

        /// <summary>
        /// Gets all vendor-address-usages
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="VendorAddressUsages">vendorAddressUsages</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.VendorAddressUsages>> GetVendorAddressUsagesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a vendorAddressUsages by guid.
        /// </summary>
        /// <param name="guid">Guid of the vendorAddressUsages in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="VendorAddressUsages">vendorAddressUsages</see></returns>
        Task<Ellucian.Colleague.Dtos.VendorAddressUsages> GetVendorAddressUsagesByGuidAsync(string guid, bool bypassCache = true);
    }
}