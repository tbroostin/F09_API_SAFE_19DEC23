//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdmissionDecisionTypes services
    /// </summary>
    public interface IAdmissionDecisionTypesService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisionType2>> GetAdmissionDecisionTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AdmissionDecisionType2> GetAdmissionDecisionTypesByGuidAsync(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationStatusType>> GetAdmissionApplicationStatusTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AdmissionApplicationStatusType> GetAdmissionApplicationStatusTypesByGuidAsync(string id);
    }
}
