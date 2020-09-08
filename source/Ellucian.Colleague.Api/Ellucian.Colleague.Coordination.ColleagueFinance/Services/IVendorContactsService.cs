//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{

    /// <summary>
    /// Interface for VendorContacts services
    /// </summary>
    public interface IVendorContactsService : IBaseService
    {
        /// <summary>
        /// Gets all vendor-contacts
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="VendorContacts">vendorContacts</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.VendorContacts>, int>> GetVendorContactsAsync(int offset, int limit, Dtos.VendorContacts criteria, bool bypassCache = false);

        /// <summary>
        /// Get a vendorContacts by guid.
        /// </summary>
        /// <param name="guid">Guid of the vendorContacts in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="VendorContacts">vendorContacts</see></returns>
        Task<Ellucian.Colleague.Dtos.VendorContacts> GetVendorContactsByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Create a vendor contact or returns person matching request.
        /// </summary>
        /// <param name="vendorContactInitiationProcess"></param>
        /// <returns></returns>
        Task<object> CreateVendorContactInitiationProcessAsync(VendorContactInitiationProcess vendorContactInitiationProcess);
    }
}