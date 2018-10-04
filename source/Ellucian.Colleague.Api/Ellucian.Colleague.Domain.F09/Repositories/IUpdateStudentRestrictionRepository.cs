using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Entities;

namespace Ellucian.Colleague.Domain.F09.Repositories
{
    public interface IUpdateStudentRestrictionRepository
    {
        Task<UpdateStudentRestrictionResponse> GetStudentRestrictionAsync(string personId);
        Task<UpdateStudentRestrictionResponse> UpdateStudentRestrictionAsync(UpdateStudentRestrictionRequest profileRequest);
    }
}
