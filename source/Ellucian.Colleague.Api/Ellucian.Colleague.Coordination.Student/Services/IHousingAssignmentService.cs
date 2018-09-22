//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for HousingAssignments services
    /// </summary>
    public interface IHousingAssignmentService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.HousingAssignment>, int>> GetHousingAssignmentsAsync(int offset, int limit, bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.HousingAssignment> GetHousingAssignmentByGuidAsync(string id, bool bypassCache = false);

        Task<Dtos.HousingAssignment> UpdateHousingAssignmentAsync(string guid, Dtos.HousingAssignment housingAssignment);

        Task<Dtos.HousingAssignment> CreateHousingAssignmentAsync(Dtos.HousingAssignment housingAssignment);
    }
}