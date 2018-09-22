/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface to define the methods for actions on this repository
    /// </summary>
    public interface IFafsaRepository
    {
        /// <summary>
        /// Gets FAFSA for a list of student ids
        /// </summary>
        /// <param name="studentIds">List of Student Ids</param>
        /// <param name="awardYear">Award Year to get FAFSA Data for</param>
        /// <returns>List of FAFSA objects for the students specified</returns>
        Task<IEnumerable<Fafsa>> GetFafsaByStudentIdsAsync(IEnumerable<string> studentIds, string awardYear);

        /// <summary>
        /// Get a list of all FAFSAs that for all given students and award years corrected for the given award years
        /// </summary>
        /// <param name="studentIds">The Colleague PERSON ids of the students for whom to get FAFSAs</param>
        /// <param name="studentAwardYears">The award years for which to get FAFSA data</param>
        /// <returns>A list of all FAFSAs from the given student ids and award years</returns>
        Task<IEnumerable<Fafsa>> GetFafsasAsync(IEnumerable<string> studentIds, IEnumerable<string> awardYearCodes);

    }
}
