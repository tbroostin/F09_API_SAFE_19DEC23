//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

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
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.HousingAssignment>, int>> GetHousingAssignmentsAsync(int offset, int limit, Dtos.HousingAssignment criteriaFilter, bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.HousingAssignment> GetHousingAssignmentByGuidAsync(string id, bool bypassCache = false);

        Task<Dtos.HousingAssignment> UpdateHousingAssignmentAsync(string guid, Dtos.HousingAssignment housingAssignment);

        Task<Dtos.HousingAssignment> CreateHousingAssignmentAsync(Dtos.HousingAssignment housingAssignment);

        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.HousingAssignment2>, int>> GetHousingAssignments2Async(int offset, int limit, Dtos.HousingAssignment2 criteriaFilter, bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.HousingAssignment2> GetHousingAssignmentByGuid2Async(string id, bool bypassCache = false);

        Task<Dtos.HousingAssignment2> UpdateHousingAssignment2Async(string guid, Dtos.HousingAssignment2 housingAssignment);

        Task<Dtos.HousingAssignment2> CreateHousingAssignment2Async(Dtos.HousingAssignment2 housingAssignment);
    }
}