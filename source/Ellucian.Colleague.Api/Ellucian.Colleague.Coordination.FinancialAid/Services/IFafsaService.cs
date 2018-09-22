/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface for the FafsaService
    /// </summary>
    public interface IFafsaService
    {
        /// <summary>
        /// Invoke the Repository Method for getting FAFSA Information
        /// </summary>
        /// <param name="criteria">DTO Object containing comma delimited string of Ids, award year, and term</param>
        /// <returns>List of DTO Objects containing FAFSA Data</returns>
        Task<IEnumerable<Fafsa>> QueryFafsaAsync(FafsaQueryCriteria criteria);

        /// <summary>
        /// Get a list of all FAFSAs that a student submitted and corrected for all award years
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get FAFSAs</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active award years data only</param>
        /// <returns>A list of all FAFSAs from the given student id</returns>
        Task<IEnumerable<Fafsa>> GetStudentFafsasAsync(string studentId, bool getActiveYearsOnly = false);
    }
}
