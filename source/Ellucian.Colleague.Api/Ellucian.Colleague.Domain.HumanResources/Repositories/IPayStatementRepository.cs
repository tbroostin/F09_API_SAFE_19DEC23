/* Copyright 2017 Ellucian Company L.P.and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPayStatementRepository
    {
        Task<IEnumerable<PayStatementSourceData>> GetPayStatementSourceDataAsync(IEnumerable<string> ids);
        Task<PayStatementSourceData> GetPayStatementSourceDataAsync(string id);
        Task<IEnumerable<PayStatementSourceData>> GetPayStatementSourceDataByPersonIdAsync(string personId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<PayStatementSourceData>> GetPayStatementSourceDataByPersonIdAsync(IEnumerable<string> personIds, DateTime? startDate = null, DateTime? endDate = null);
    }
}
