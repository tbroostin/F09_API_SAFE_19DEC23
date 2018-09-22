using System;
using System.Threading.Tasks;
namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IJobChangeReasonService
    {
        Task<Ellucian.Colleague.Dtos.JobChangeReason> GetJobChangeReasonByGuidAsync(string guid);
        Task<System.Collections.Generic.IEnumerable<Ellucian.Colleague.Dtos.JobChangeReason>> GetJobChangeReasonsAsync(bool bypassCache = false);
    }
}
