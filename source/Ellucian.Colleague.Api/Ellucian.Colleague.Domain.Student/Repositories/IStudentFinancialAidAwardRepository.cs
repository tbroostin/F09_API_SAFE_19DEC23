// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a StudentFinancialAidAwards repository
    /// </summary>
    public interface IStudentFinancialAidAwardRepository
    {
        Task<StudentFinancialAidAward> GetByIdAsync(string id);
        Task<Tuple<IEnumerable<StudentFinancialAidAward>, int>> GetAsync(int offset, int limit, bool bypassCache, bool restricted, IEnumerable<string> unrestrictedFunds, IEnumerable<string> awardYears);

        Task<IEnumerable<string>> GetNotAwardedCategoriesAsync();
    }
}
