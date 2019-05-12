using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories;

namespace Ellucian.Colleague.Domain.F09.Repositories
{
    public interface IStudentAlumniDirectoriesRepository
    {
        Task<DirectoriesResponse> GetStudentAlumniDirectoriesAsync(string personId);
        Task<DirectoriesResponse> UpdateStudentAlumniDirectoriesAsync(DirectoriesRequest applicationRequest);
    }
}
