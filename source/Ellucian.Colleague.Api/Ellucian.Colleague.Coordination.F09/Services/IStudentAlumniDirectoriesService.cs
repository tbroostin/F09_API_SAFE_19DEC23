using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IStudentAlumniDirectoriesService
    {
        Task<DirectoriesResponseDto> GetStudentAlumniDirectoriesAsync(string personId);
        Task<DirectoriesResponseDto> UpdateStudentAlumniDirectoriesAsync(DirectoriesRequestDto request);
    }
}