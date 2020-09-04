/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface ISupervisorsRepository
    {
        Task<IEnumerable<string>> GetSuperviseesBySupervisorAsync(string personId);
        Task<IEnumerable<string>> GetSupervisorsBySuperviseeAsync(string personId);
        Task<Dictionary<string, List<string>>> GetSupervisorIdsForPositionsAsync(IEnumerable<string> positionIds);
        Task<IEnumerable<string>> GetSupervisorsByPositionIdAsync(string positionId, string superviseeId);
        Task<IEnumerable<string>> GetSuperviseesByPrimaryPositionForSupervisorAsync(string supervisorId);
        
    }
}
