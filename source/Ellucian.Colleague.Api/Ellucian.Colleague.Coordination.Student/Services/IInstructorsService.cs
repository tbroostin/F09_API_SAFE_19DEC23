//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Instructors services
    /// </summary>
    public interface IInstructorsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Instructor>, int>> GetInstructorsAsync(int offset, int limit, string instructor, string primaryLocation, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.Instructor> GetInstructorByGuidAsync(string id);

        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Instructor2>, int>> GetInstructors2Async(int offset, int limit, string instructor, string primaryLocation, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.Instructor2> GetInstructorByGuid2Async(string id);
    }
}