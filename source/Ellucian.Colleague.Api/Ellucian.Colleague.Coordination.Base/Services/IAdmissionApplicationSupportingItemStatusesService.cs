//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for AdmissionApplicationSupportingItemStatuses services
    /// </summary>
    public interface IAdmissionApplicationSupportingItemStatusesService : IBaseService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemStatus>> GetAdmissionApplicationSupportingItemStatusesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemStatus> GetAdmissionApplicationSupportingItemStatusByGuidAsync(string id);
    }
}
