// Copyright 2014-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
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
        [Obsolete("Obsolete as of API version 1.36. Use GetFacultyGradingConfiguration2Async.")]
        Task<FacultyGradingConfiguration> GetFacultyGradingConfigurationAsync();

        /// <summary>
        /// Retrieves the faculty grading configuration asynchronously
        /// </summary>
        /// <returns></returns>
        Task<FacultyGradingConfiguration2> GetFacultyGradingConfiguration2Async();

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

        /// <summary>
        /// Retrieves the configuration information needed for Colleague Self-Service instant enrollment
        /// </summary>
        Task<InstantEnrollmentConfiguration> GetInstantEnrollmentConfigurationAsync();

        /// <summary>
        /// Retrieves the course catalog configuration asynchronously
        /// </summary>
        /// <returns>CourseCatalogConfiguration entity</returns>
        Task<CourseCatalogConfiguration> GetCourseCatalogConfiguration3Async();

        /// <summary>
        /// Retrieves the course catalog configuration asynchronously
        /// </summary>
        /// <returns>CourseCatalogConfiguration entity</returns>
        Task<CourseCatalogConfiguration> GetCourseCatalogConfiguration4Async();

        /// <summary>
        /// Gets the unofficial transcript configuration asynchronously.
        /// </summary>
        /// <returns>UnofficialTranscriptConfiguration entity</returns>
        Task<UnofficialTranscriptConfiguration> GetUnofficialTranscriptConfigurationAsync();

        /// <summary>
        /// Get My Progress configuration information
        /// </summary>
        ///  <returns>The MyProgressConfiguration entity</returns>
        Task<MyProgressConfiguration> GetMyProgressConfigurationAsync();

        /// <summary>
        /// Get the section census configuration information
        /// </summary>
        ///  <returns>The SectionCensusConfiguration entity</returns>
        [Obsolete("Obsolete as of API version 1.36. Use GetSectionCensusConfiguration2Async.")]
        Task<SectionCensusConfiguration> GetSectionCensusConfigurationAsync();

        /// <summary>
        /// Get the section census configuration2 information
        /// </summary>
        ///  <returns>The SectionCensusConfiguration2 entity</returns>
        Task<SectionCensusConfiguration2> GetSectionCensusConfiguration2Async();

        /// <summary>
        /// Retrieves Course Delimiter
        /// </summary>
        /// <returns></returns>
        Task<string> GetCourseDelimiterAsync();

        /// <summary>
        /// Get Academic Record configuration information
        /// </summary>
        /// <returns>The AcademicRecordConfiguration entity</returns>
        Task<AcademicRecordConfiguration> GetAcademicRecordConfigurationAsync();

        /// <summary>
        /// Retrieves course section availability information configuration
        /// </summary>
        Task<SectionAvailabilityInformationConfiguration> GetSectionAvailabilityInformationConfigurationAsync();

        /// <summary>
        /// Get the faculty attendance configuration information
        /// </summary>
        Task<FacultyAttendanceConfiguration> GetFacultyAttendanceConfigurationAsync();

        /// <summary>
        /// Get the student records release configuration information
        /// </summary>
        Task<StudentRecordsReleaseConfig> GetStudentRecordsReleaseConfigAsync();
    }
}
