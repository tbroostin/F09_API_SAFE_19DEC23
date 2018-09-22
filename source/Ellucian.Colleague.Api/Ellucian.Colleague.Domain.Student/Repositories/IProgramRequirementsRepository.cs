using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Base;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IProgramRequirementsRepository
    {
        //ICollection<ProgramRequirements> Get();  Do we really want this?
        /// <summary>
        /// Wrapper around async, used by FinancialAid AcademicProgressService
        /// </summary>
        /// <param name="program"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        ProgramRequirements Get(string program, string catalog);
        Task<ProgramRequirements> GetAsync(string program, string catalog);
    }
}
