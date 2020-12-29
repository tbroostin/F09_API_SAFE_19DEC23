// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentConfigurationService
    {
        Task<Dtos.Student.StudentProfileConfiguration> GetStudentProfileConfigurationAsync();
        /// <summary>
        /// Gets the course catalog configuration3 asynchronous.
        /// </summary>
        /// <returns>CourseCatalogConfiguration3 object</returns>
        Task<Dtos.Student.CourseCatalogConfiguration3> GetCourseCatalogConfiguration3Async();
    }
}
