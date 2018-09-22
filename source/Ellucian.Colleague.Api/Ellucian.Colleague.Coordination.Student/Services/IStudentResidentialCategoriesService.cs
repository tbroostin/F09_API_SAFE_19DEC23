//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentResidentialCategories services
    /// </summary>
    public interface IStudentResidentialCategoriesService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.StudentResidentialCategories>> GetStudentResidentialCategoriesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.StudentResidentialCategories> GetStudentResidentialCategoriesByGuidAsync(string id);
    }
}
