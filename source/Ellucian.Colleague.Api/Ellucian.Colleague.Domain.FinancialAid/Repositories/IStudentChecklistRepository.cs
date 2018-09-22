/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface for a StudentChecklistRepository
    /// </summary>
   public interface IStudentChecklistRepository
   {
       /// <summary>
       /// Create a StudentChecklist in the database
       /// </summary>
       /// <param name="checklist">The checklist to create</param>
       /// <returns>The created checklist</returns>
       Task<StudentFinancialAidChecklist> CreateStudentChecklistAsync(StudentFinancialAidChecklist checklist);

       /// <summary>
       /// Get a list of StudentFinancialAidChecklist objects for the given years
       /// </summary>
       /// <param name="studentId">The Colleague PERSON id of the student for whom to get checklists</param>
       /// <param name="years">The years for which to get checklists. If a student doesn't have a checklist 
       /// for one of the years, no checklist for that year will be returned</param>
       /// <returns></returns>
       Task<IEnumerable<StudentFinancialAidChecklist>> GetStudentChecklistsAsync(string studentId, IEnumerable<string> years);

       /// <summary>
       /// Get a single StudentFinancialAidChecklist object
       /// </summary>
       /// <param name="studentId">The Colleague PERSON id of the student for whom to get a checklist</param>
       /// <param name="year">The award year for which to get a checklist</param>
       /// <returns></returns>
       /// <exception cref="KeyNotFoundException">Thrown if student has no checklist for the year</exception>
       Task<StudentFinancialAidChecklist> GetStudentChecklistAsync(string studentId, string year);
   }
}
