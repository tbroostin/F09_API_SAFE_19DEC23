// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AccountingCodes service
    /// </summary>
    public interface IAccountingCodesService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingCode>> GetAccountingCodesAsync(bool bypassCache);
        Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingCode2>> GetAccountingCodes2Async(AccountingCodeCategoryDtoProperty criteria, bool bypassCache);
        Task<Ellucian.Colleague.Dtos.AccountingCode> GetAccountingCodeByIdAsync(string id);
        Task<AccountingCode2> GetAccountingCode2ByIdAsync(string id, bool bypassCache);
    }
}
