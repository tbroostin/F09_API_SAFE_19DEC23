//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentCourseTransfers services
    /// </summary>
    public interface IStudentCourseTransferService : IBaseService
    {
        Task<Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>> GetStudentCourseTransfersAsync(int offset, int limit, bool bypassCache = false);
        Task<Dtos.StudentCourseTransfer> GetStudentCourseTransferByGuidAsync(string id, bool bypassCache = false);

        Task<Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>> GetStudentCourseTransfers2Async(int offset, int limit, bool bypassCache = false);
        Task<Dtos.StudentCourseTransfer> GetStudentCourseTransfer2ByGuidAsync(string id, bool bypassCache = false);
    }
}
