﻿//Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for EmploymentVocations services
    /// </summary>
    public interface IEmploymentVocationService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentVocation>> GetEmploymentVocationsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.EmploymentVocation> GetEmploymentVocationByGuidAsync(string id);
    }
}