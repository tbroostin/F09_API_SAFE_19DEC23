using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IUpdateStudentRestrictionService
    {
        Task<UpdateStudentRestrictionResponseDto> GetStudentRestrictionAsync(string personId);
        Task<UpdateStudentRestrictionResponseDto> UpdateStudentRestrictionAsync(UpdateStudentRestrictionRequestDto request);
    }
}