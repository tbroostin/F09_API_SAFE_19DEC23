//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentFinancialAidAcademicProgressStatusesRepository
    {
        Task<Tuple<IEnumerable<Entities.SapResult>, int>> GetSapResultsAsync(int offset, int limit, string personId = "", string statusId = "", string typeId = "", bool bypassCache = false);
        Task<Entities.SapResult> GetSapResultByGuidAsync(string guid);
    }
}
