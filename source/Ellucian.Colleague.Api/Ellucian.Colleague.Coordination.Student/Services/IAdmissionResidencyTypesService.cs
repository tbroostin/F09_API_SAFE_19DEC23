//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdmissionResidencyTypes services
    /// </summary>
    public interface IAdmissionResidencyTypesService: IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionResidencyTypes>> GetAdmissionResidencyTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AdmissionResidencyTypes> GetAdmissionResidencyTypesByGuidAsync(string id);
    }
}
