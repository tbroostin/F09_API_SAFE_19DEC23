// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IAcademicCreditRepository
    {
        /// <summary>
        /// Get a set of academic credits by ID
        /// </summary>
        /// <param name="academicCreditIds"></param>
        /// <param name="bestFit">Set to true if non-term credits should be given the term that most closely matches the credits date range </param>
        /// <param name="term">Term for filtering academic credit</param>
        /// <param name="filter">Flag indicating whether or not to filter credits based on status</param>
        /// <param name="includeDrops">Flag indicating whether or not to include dropped academic credits</param>
        /// <returns>A set of academic credits</returns>
        Task<IEnumerable<AcademicCredit>> GetAsync(ICollection<string> academicCreditIds, bool bestFit = false, bool filter = true, bool includeDrops = false);

        /// <summary>
        /// Get a set of academic credits by ID
        /// </summary>
        /// <param name="academicCreditIds"></param>
        /// <param name="filter"></param>
        /// <param name="includeDrops"></param>
        /// <returns></returns>
        Task<IEnumerable<AcademicCreditMinimum>> GetAcademicCreditMinimumAsync(ICollection<string> academicCreditIds, bool filter = true, bool includeDrops = false);

        /// <summary>
        /// get students AcademicCredit keys only.
        /// </summary>
        /// <param name="studentIds">List of Student IDs</param>
        /// <param name="bestFit">If set, all credit is put into a term that best fits within the dates</param>
        /// <param name="filter">if set, only return credits for the specified term.</param>
        /// <param name="includeDrops">Flag indicating whether or not to include dropped academic credits</param>
        /// <returns>Dictionary of Student Keys and AcademicCredit objects</returns>
        Task<Dictionary<string, List<AcademicCredit>>> GetAcademicCreditByStudentIdsAsync(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, bool includeDrops = false);
        Task<Dictionary<string, List<PilotAcademicCredit>>> GetPilotAcademicCreditsByStudentIdsAsync(IEnumerable<string> studentIds, AcademicCreditDataSubset subset, bool bestFit = false, bool filter = true, string term = null);
        Task<Dictionary<string, List<PilotAcademicCredit>>> GetPilotAcademicCreditsByStudentIds2Async(IEnumerable<string> studentIds, AcademicCreditDataSubset subset, bool bestFit = false, bool filter = true, string term = null);
        Task<bool> GetPilotCensusBooleanAsync();
        Task<CreditStatus> ConvertCreditStatusAsync(string statusCode);
        /// <summary>
        /// Retrieves all academic credits for a specific section Id.
        /// </summary>
        /// <param name="sectionId">Section Id</param>
        /// <returns>List of AcademicCredit entities specific to this section.</returns>
        Task<IEnumerable<AcademicCredit>> GetAcademicCreditsBySectionIdsAsync(IEnumerable<string> sectionIds);
        Task<Tuple<IEnumerable<StudentCourseTransfer>, int>> GetStudentCourseTransfersAsync(int offset, int limit, bool bypassCache);
        Task<StudentCourseTransfer> GetStudentCourseTransferByGuidAsync(string guid, bool bypassCache);

        /// <summary>
        /// Sorts a list of academic credits according to one or more sort specifications and returns a dictionary of
        /// the sorted academic credits, keyed by their sort specification ID
        /// </summary>
        /// <param name="acadCredits">Collection of <see cref="AcademicCredit"/> objects.</param>
        /// <param name="sortSpecIds">Collection of sort specification IDs</param>
        /// <returns>Dictionary of sorted academic credits, keyed by their sort specification ID</returns>
        Task<Dictionary<string, List<AcademicCredit>>> GetSortedAcademicCreditsBySortSpecificationIdAsync(IEnumerable<AcademicCredit> acadCredits, IEnumerable<string> sortSpecIds);

        Task<CreditType> GetCreditTypeAsync(string typecode);
        /// <summary>
        /// Retrieves all academic credits for specific section Ids. This will return all academic credits regardless of status.
        /// This also returns collection of invalid academic credit Ids, Ids that are missing from STUDENT.ACAD.CRED file.
        /// </summary>
        /// <param name="sectionId">Section Id</param>
        /// <returns>Tuple that contains List of AcademicCredit entities specific to the given sections and List of invalid academic credit Ids.</returns>
        Task<AcademicCreditsWithInvalidKeys> GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(IEnumerable<string> sectionIds);
        /// <summary>
        /// Filters and return academic credits based upon the criteria passed.
        /// Criteria is passed as unidata_uniquery syntax.
        /// Criteria will only be applied to STUDENT.ACAD.CRED records
        /// Criteria syntax is like "WITH <STUDENT.ACAD.CRED><FIELD NAME>"
        /// </summary>
        /// <param name="academicCredits">List of academic credits</param>
        /// <param name="criteria">criteria as per unidata_uniquery syntax</param>
        /// <returns>Filtered Academic credits as per criteria
        /// If no criteria is passed, returns all the academic credits
        /// </returns>
        Task<IEnumerable<AcademicCredit>> FilterAcademicCreditsAsync(IEnumerable<AcademicCredit> acadCredits, string criteria);


    }
}
