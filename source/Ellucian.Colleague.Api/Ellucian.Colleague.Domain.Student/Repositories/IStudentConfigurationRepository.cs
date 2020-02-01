// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Repository for student-related configuration items
    /// </summary>
    public interface IStudentConfigurationRepository
    {
        /// <summary>
        /// Gets student configuration parameters such as default email and phone types.
        /// </summary>
        /// <returns></returns>
        Task<StudentConfiguration> GetStudentConfigurationAsync();

        /// <summary>
        /// Gets configuration for curriculum processing
        /// </summary>
        /// <returns></returns>
        Task<CurriculumConfiguration> GetCurriculumConfigurationAsync();

        /// <summary>
        /// Retrieves graduation configuration for graduation applications asynchronously
        /// </summary>
        /// <returns></returns>
        Task<GraduationConfiguration> GetGraduationConfigurationAsync();

        /// <summary>
        /// Retrieves the student request configuration for transcript requests or enrollment verification requests asynchronously
        /// </summary>
        /// <returns></returns>
        Task<StudentRequestConfiguration> GetStudentRequestConfigurationAsync();

        /// <summary>
        /// Retrieves the faculty grading configuration asynchronously
        /// </summary>
        /// <returns></returns>
        Task<FacultyGradingConfiguration> GetFacultyGradingConfigurationAsync();

        /// <summary>
        /// Retrieves the course catalog configuration asynchronously
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete as of API version 1.26. Use GetCourseCatalogConfiguration2Async.")]
        Task<CourseCatalogConfiguration> GetCourseCatalogConfigurationAsync();

        /// <summary>
        /// Retrieves the course catalog configuration asynchronously
        /// </summary>
        /// <returns></returns>
        Task<CourseCatalogConfiguration> GetCourseCatalogConfiguration2Async();

        /// <summary>
        /// Retrieves the configuration information needed for registration processing asynchronously.
        /// </summary>
        Task<RegistrationConfiguration> GetRegistrationConfigurationAsync();

        /// <summary>
        /// Retrieves the student profile configurations asynchronously
        /// </summary>
        Task<StudentProfileConfiguration> GetStudentProfileConfigurationAsync();
    }
}
