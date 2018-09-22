using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPersonBenefitDeductionRepository
    {
        Task<IEnumerable<PersonBenefitDeduction>> GetPersonBenefitDeductionsAsync(string personId);
        Task<IEnumerable<PersonBenefitDeduction>> GetPersonBenefitDeductionsAsync(IEnumerable<string> personIds);
    }
}
