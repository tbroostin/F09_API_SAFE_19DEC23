using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IScholarshipApplicationService
    {
        Task<ScholarshipApplicationResponseDto> GetScholarshipApplicationAsync(string personId);
        Task<ScholarshipApplicationResponseDto> UpdateScholarshipApplicationAsync(ScholarshipApplicationRequestDto request);
    }
}