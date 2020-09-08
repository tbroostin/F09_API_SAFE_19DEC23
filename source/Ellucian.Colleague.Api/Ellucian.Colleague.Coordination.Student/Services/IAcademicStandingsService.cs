// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AcademicStandings service
    /// </summary>
    public interface IAcademicStandingsService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicStanding>> GetAcademicStandingsAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.AcademicStanding> GetAcademicStandingByIdAsync(string id);
    }
}
