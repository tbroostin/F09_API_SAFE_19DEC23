//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for FinancialAidFundClassifications services
    /// </summary>
    public interface IFinancialAidFundClassificationsService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidFundClassifications>> GetFinancialAidFundClassificationsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.FinancialAidFundClassifications> GetFinancialAidFundClassificationsByGuidAsync(string id);
    }
}
