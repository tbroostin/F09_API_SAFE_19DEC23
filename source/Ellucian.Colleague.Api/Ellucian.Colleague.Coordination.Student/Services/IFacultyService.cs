// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Student;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Faculty services
    /// </summary>
    public interface IFacultyService 
    {
        Task<Faculty> GetAsync(string facultyId);

        /// <summary>
        /// OBSOLETE AS OF API 1.3, REPLACED BY GetFacultySections2.
        /// Get a list of registration sections taught by faculty ID
        /// </summary>
        /// <param name="facultyId">A faculty ID</param>
        /// <param name="startDate">Optional, startDate, ISO-8601, yyyy-mm-dd, defaults to today</param>
        /// <param name="endDate">Optional, endDate, ISO-8601, yyyy-mm-dd, defaults to startDate + 90 days. Must be greater than start date if specified</param>
        /// <returns>List of <see cref="Section">Section</see> objects></returns>
        [Obsolete("Obsolete as of API 1.3")]
        Task<PrivacyWrapper<IEnumerable<Section>>> GetFacultySectionsAsync(string facultyId, DateTime? startDate, DateTime? endDate, bool bestFit);

        /// <summary>
        /// Get a list of registration sections taught by faculty ID
        /// </summary>
        /// <param name="facultyId">A faculty ID</param>
        /// <param name="startDate">Optional, startDate, defaults to today</param>
        /// <param name="endDate">Optional, endDate, defaults to startDate + 90 days. Must be greater than start date if specified</param>
        /// <returns>List of <see cref="Section2">Section</see> objects></returns>
        [Obsolete("Obsolete as of API 1.5. Use latest version of this method.")]
        Task<PrivacyWrapper<IEnumerable<Section2>>> GetFacultySections2Async(string facultyId, DateTime? startDate, DateTime? endDate, bool bestFit);

        /// <summary>
        /// Get a list of registration sections taught by faculty ID
        /// </summary>
        /// <param name="facultyId">A faculty ID</param>
        /// <param name="startDate">Optional, startDate, ISO-8601, yyyy-mm-dd, defaults to today</param>
        /// <param name="endDate">Optional, endDate, ISO-8601, yyyy-mm-dd, defaults to startDate + 90 days. Must be greater than start date if specified</param>
        /// <returns>Collection of requested Section3 DTOs</returns>
        [Obsolete("Obsolete as of API 1.13.1. Use latest version of this method.")]
        Task<PrivacyWrapper<IEnumerable<Section3>>> GetFacultySections3Async(string facultyId, DateTime? startDate, DateTime? endDate, bool bestFit);

        /// <summary>
        /// Get a list of sections taught by faculty ID based on a date range or system parameters. If a start date is not specified sections will be returned based on 
        /// the allowed terms specified on Registration Web Parameters (RGWP), Class Schedule Web Parameters (CSWP) and Grading Web Parameters (GRWP).
        /// </summary>
        /// <param name="facultyId">A faculty ID - if not supplied an empty list of sections is returned.</param>
        /// <param name="startDate">Optional, startDate, ISO-8601, yyyy-mm-dd</param>
        /// <param name="endDate">Optional, endDate, ISO-8601, yyyy-mm-dd. If a start date is specified but end date is not, it will default to 90 days past start date. It must be greater than start date if specified, otherwise it will default to 90 days past start.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <param name="useCache">Flag indicating whether or not to use cached <see cref="Section3">course section</see> data. Defaults to true.</param>
        /// <returns>Collection of requested Section3 DTOs</returns>
        [Obsolete("Obsolete as of API 1.31. Use latest version of this method.")]
        Task<PrivacyWrapper<IEnumerable<Section3>>> GetFacultySections4Async(string facultyId, DateTime? startDate, DateTime? endDate, bool bestFit, bool useCache = true);

        /// <summary>
        /// Get a list of sections taught by faculty ID based on a date range or system parameters. If a start date is not specified sections will be returned based on 
        /// the allowed terms specified on Registration Web Parameters (RGWP), Class Schedule Web Parameters (CSWP) and Grading Web Parameters (GRWP).
        /// </summary>
        /// <param name="facultyId">A faculty ID - if not supplied an empty list of sections is returned.</param>
        /// <param name="startDate">Optional, startDate, ISO-8601, yyyy-mm-dd</param>
        /// <param name="endDate">Optional, endDate, ISO-8601, yyyy-mm-dd. If a start date is specified but end date is not, it will default to 90 days past start date. It must be greater than start date if specified, otherwise it will default to 90 days past start.</param>
        /// <param name="bestFit">Optional, true assigns a term to any non-term section based on the section start date. Defaults to false.</param>
        /// <param name="useCache">Flag indicating whether or not to use cached <see cref="Section3">course section</see> data. Defaults to true.</param>
        /// <returns>Collection of requested Section3 DTOs</returns>
        Task<PrivacyWrapper<IEnumerable<Section4>>> GetFacultySections5Async(string facultyId, DateTime? startDate, DateTime? endDate, bool bestFit, bool useCache = true);

        Task<IEnumerable<Faculty>> QueryFacultyAsync(FacultyQueryCriteria criteria);
        Task<IEnumerable<Faculty>> GetFacultyByIdsAsync(IEnumerable<string> facultyIds);
        Task<IEnumerable<string>> SearchFacultyIdsAsync(bool facultyOnlyFlag, bool advisorOnlyFlag);
        Task<IEnumerable<string>> GetFacultyPermissionsAsync();
        /// <summary>
        /// Returns the advising permissions for the authenticated user.
        /// </summary>
        /// <returns>Faculty permissions for the authenticated user.</returns>
        Task<FacultyPermissions> GetFacultyPermissions2Async();

        /// <summary>
        /// Returns the list of faculty office hours for the faculty ids
        /// </summary>
        /// <param name="facultyIds">A list of faculty id's</param>
        /// <returns></returns>
        Task<IEnumerable<FacultyOfficeHours>> GetFacultyOfficeHoursAsync(IEnumerable<string> facultyIds);
    }
}
