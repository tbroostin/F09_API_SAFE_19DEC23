// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IHousingAssignmentRepository : IEthosExtended
    {
        /// <summary>
        /// Gets paged housing assignments.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<HousingAssignment>, int>> GetHousingAssignmentsAsync(int offset, int limit, bool bypassCache);

        /// <summary>
        /// Gets housing request by guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<HousingAssignment> GetHousingAssignmentByGuidAsync(string guid);

        /// <summary>
        /// Gets all the guids for the person keys
        /// </summary>
        /// <param name="personRecordKeys"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetPersonGuidsAsync(IEnumerable<string> personRecordKeys);

        /// <summary>
        /// Create/Update housing assignment.
        /// </summary>
        /// <param name="housingAssignmentEntity"></param>
        /// <returns></returns>
        Task<HousingAssignment> UpdateHousingAssignmentAsync(HousingAssignment housingAssignmentEntity);

        /// <summary>
        /// Gets housing assignment key.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<string> GetHousingAssignmentKeyAsync(string guid);
    }
}
