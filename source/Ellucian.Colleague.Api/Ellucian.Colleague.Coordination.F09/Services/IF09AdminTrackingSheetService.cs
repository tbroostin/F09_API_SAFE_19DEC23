using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;


namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IF09AdminTrackingSheetService
    {
        Task<F09AdminTrackingSheetResponseDto> GetF09AdminTrackingSheetAsync(string personId);
    }
}
