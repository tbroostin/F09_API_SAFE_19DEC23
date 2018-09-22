using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface ITermRepository
    {
        Task<IEnumerable<Term>> GetAsync(); 
        Task<Term> GetAsync(string id);
        Task<IEnumerable<Term>> GetAsync(bool clearCache);
        Task<IEnumerable<Term>> GetAsync(IEnumerable<string> ids);
        Task<IEnumerable<Term>> GetRegistrationTermsAsync();
        /// <summary>
        /// Wrapper around async, used by FinancialAid and Finance
        /// </summary>
        /// <param name="program"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        Term Get(string id);
        /// <summary>
        /// Wrapper around async, used by FinancialAid and Finance
        /// </summary>
        /// <param name="program"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        IEnumerable<Term> Get();

        IEnumerable<AcademicPeriod> GetAcademicPeriods(IEnumerable<Term> termEntities);
    }
}
