//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentSectionWaitlists services
    /// </summary>
    public interface IStudentSectionWaitlistsService : IBaseService
    {
        Task<Tuple<IEnumerable<StudentSectionWaitlist>, int>> GetStudentSectionWaitlistsAsync(int offset, int limit, bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.StudentSectionWaitlist> GetStudentSectionWaitlistsByGuidAsync(string id);
    }
}
