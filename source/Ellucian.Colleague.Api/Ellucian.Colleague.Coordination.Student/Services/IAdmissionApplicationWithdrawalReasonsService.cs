//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdmissionApplicationWithdrawalReasons services
    /// </summary>
    public interface IAdmissionApplicationWithdrawalReasonsService: IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationWithdrawalReasons>> GetAdmissionApplicationWithdrawalReasonsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AdmissionApplicationWithdrawalReasons> GetAdmissionApplicationWithdrawalReasonsByGuidAsync(string id);
    }
}