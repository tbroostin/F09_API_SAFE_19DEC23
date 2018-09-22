//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for CourseCategories services
    /// </summary>
    public interface ICourseCategoriesService : IBaseService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.CourseCategories>> GetCourseCategoriesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.CourseCategories> GetCourseCategoriesByGuidAsync(string id);
    }
}
