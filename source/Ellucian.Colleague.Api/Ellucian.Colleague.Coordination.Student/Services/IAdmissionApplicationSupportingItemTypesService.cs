//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdmissionApplicationSupportingItemTypes services
    /// </summary>
    public interface IAdmissionApplicationSupportingItemTypesService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes>> GetAdmissionApplicationSupportingItemTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes> GetAdmissionApplicationSupportingItemTypesByGuidAsync(string id);
    }
}