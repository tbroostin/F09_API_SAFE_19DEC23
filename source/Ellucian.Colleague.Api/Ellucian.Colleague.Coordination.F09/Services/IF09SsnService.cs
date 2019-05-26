using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IF09SsnService
    {
        Task<F09SsnResponseDto> GetF09SsnAsync(string id);
        Task<F09SsnResponseDto> UpdateF09SsnAsync(F09SsnRequestDto request);
    }
}
