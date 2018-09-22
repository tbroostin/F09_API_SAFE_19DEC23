// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Student Financial Aid Awards
    /// </summary>
    public interface IStudentFinancialAidAwardService : IBaseService
    {
        Task<Dtos.StudentFinancialAidAward> GetByIdAsync(string id, bool restricted);    
        Task<Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>> GetAsync(int offset, int limit, bool bypassCache, bool restricted);

        Task<Dtos.StudentFinancialAidAward2> GetById2Async(string id, bool restricted);
        Task<Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>> Get2Async(int offset, int limit, bool bypassCache, bool restricted);
    }
}