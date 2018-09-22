using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Services
{
    public interface IFacultyContractDomainService
    {
        Task<IEnumerable<FacultyContract>> GetFacultyContractsByFacultyIdAsync(string facultyId);
    }
}
