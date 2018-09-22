//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AttendanceCategories services
    /// </summary>
    public interface IAttendanceCategoriesService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.AttendanceCategories>> GetAttendanceCategoriesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.AttendanceCategories> GetAttendanceCategoriesByGuidAsync(string id);
    }
}