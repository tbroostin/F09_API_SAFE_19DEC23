/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Student Financial Aid Checklist Service
    /// </summary>
    public interface IStudentChecklistService
    {
       /// <summary>
       /// Create a new checklist for a student for a year
       /// </summary>
       /// <param name="studentId">The Colleague PERSON id of the student for whom to create a checklist</param>
       /// <param name="year">The award year for which to create a checklist</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
       /// <returns>List of student financial aid checklist objects per year</returns>
       /// <exception cref="ApplicationException">Thrown if there is an error creating the StudentChecklist</exception>
       /// <exception cref="ExistingResourceException">Thrown if a checklist already exists for this year for this student</exception>
       Task<StudentFinancialAidChecklist> CreateStudentChecklistAsync(string studentId, string year, bool getActiveYearsOnly = false);

       /// <summary>
       /// Get all checklists for a student
       /// </summary>
       /// <param name="studentId">The Colleague PERSON id of the student for whom to get checklists</param>
       /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
       /// <returns>List of student checklists</returns>
       Task<IEnumerable<StudentFinancialAidChecklist>> GetAllStudentChecklistsAsync(string studentId, bool getActiveYearsOnly = false);

       /// <summary>
       /// Get a student checklist for a given year
       /// </summary>
       /// <param name="studentId">The Colleague PERSON id of the student for whom to get the checklist</param>
       /// <param name="year">The award year for which to get a checklist</param>
       /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
       /// <returns>A single student checklist</returns>
       Task<StudentFinancialAidChecklist> GetStudentChecklistAsync(string studentId, string year, bool getActiveYearsOnly = false);

        /// <summary>
        /// Get a parent's profile data.
        /// </summary>
        /// <param name="parentId">Id of the parent</param>
        /// <param name="studentId">Id of the student</param>
        /// <param name="useCache">True/False flag to use the cache or not</param>
        /// <returns>Profile data for a parent</returns>
        Task<Dtos.Base.Profile> GetMpnProfileAsync(string parentId, string studentId, bool useCache = true);
    }
}
