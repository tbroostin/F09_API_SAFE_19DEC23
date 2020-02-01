//Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for InstitutionEmployers Service
    /// </summary>
    public interface IInstitutionEmployersService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.InstitutionEmployers>> GetInstitutionEmployersAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.InstitutionEmployers> GetInstitutionEmployersByGuidAsync(string guid);
    }
}