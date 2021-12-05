// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.

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

        /// <summary>
        /// Gets the course catalog configuration4 asynchronous.
        /// </summary>
        /// <returns>CourseCatalogConfiguration4 object</returns>
        Task<Dtos.Student.CourseCatalogConfiguration4> GetCourseCatalogConfiguration4Async();

        /// <summary>
        /// Get Academic Record configuration information
        /// </summary>
        /// <returns>The AcademicRecordConfiguration object</returns>
        Task<Dtos.Student.AcademicRecordConfiguration> GetAcademicRecordConfigurationAsync();
    }
}
