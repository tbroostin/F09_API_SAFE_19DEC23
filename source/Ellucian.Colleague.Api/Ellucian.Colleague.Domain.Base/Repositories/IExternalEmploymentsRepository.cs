/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IExternalEmploymentsRepository
    {
        Task<Tuple<IEnumerable<ExternalEmployments>, int>> GetExternalEmploymentsAsync(int offset, int limit);    
        Task<ExternalEmployments> GetExternalEmploymentsByIdAsync(string id);
        Task<ExternalEmployments> GetExternalEmploymentsByGuidAsync(string guid);
    }
}