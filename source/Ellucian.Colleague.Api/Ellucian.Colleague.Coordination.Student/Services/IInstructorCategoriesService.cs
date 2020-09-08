//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for InstructorCategories services
    /// </summary>
    public interface IInstructorCategoriesService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.InstructorCategories>> GetInstructorCategoriesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.InstructorCategories> GetInstructorCategoriesByGuidAsync(string id);
    }
}