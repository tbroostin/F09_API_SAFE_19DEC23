//Copyright 2017-2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IFinancialAidFundCategoryService : IBaseService
    {
        Task<IEnumerable<Dtos.FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync(bool bypassCache);
        Task<Dtos.FinancialAidFundCategory> GetFinancialAidFundCategoryByGuidAsync(string guid);
    }
}
