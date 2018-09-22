//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for InstructorStaffTypes services
    /// </summary>
    public interface IInstructorStaffTypesService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.InstructorStaffTypes>> GetInstructorStaffTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.InstructorStaffTypes> GetInstructorStaffTypesByGuidAsync(string id);
    }
}