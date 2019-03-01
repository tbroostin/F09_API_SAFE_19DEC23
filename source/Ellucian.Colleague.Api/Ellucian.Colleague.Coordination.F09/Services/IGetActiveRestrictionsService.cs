using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IGetActiveRestrictionsService
    {
        Task<GetActiveRestrictionsResponseDto> GetActiveRestrictionsAsync(string personId);
    }
}