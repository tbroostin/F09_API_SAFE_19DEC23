//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for InstructorCategories services
    /// </summary>
    public interface IInstructorCategoriesService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.InstructorCategories>> GetInstructorCategoriesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.InstructorCategories> GetInstructorCategoriesByGuidAsync(string id);
    }
}