//Copyright 2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentFinancialAidOfficeService : IBaseService
    {
        Task<IEnumerable<Dtos.FinancialAidOffice>> GetFinancialAidOfficesAsync(bool bypassCache = false);
        Task<Dtos.FinancialAidOffice> GetFinancialAidOfficeByGuidAsync(string guid, bool bypassCache = false);
    }
}