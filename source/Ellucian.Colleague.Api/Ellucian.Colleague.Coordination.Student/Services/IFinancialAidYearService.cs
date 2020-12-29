//Copyright 2017-2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IFinancialAidYearService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidYear>> GetFinancialAidYearsAsync(string academicPeriodId = "", bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.FinancialAidYear> GetFinancialAidYearByGuidAsync(string guid);
    }
}
