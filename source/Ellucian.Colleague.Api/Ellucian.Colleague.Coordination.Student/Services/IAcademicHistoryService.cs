// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IAcademicHistoryService
    {
        Task<Ellucian.Colleague.Dtos.Student.AcademicHistory> GetAcademicHistoryAsync(string studentId, bool bestFit, bool filter = true, string term = null);
        Task<Ellucian.Colleague.Dtos.Student.AcademicHistory2> GetAcademicHistory2Async(string studentId, bool bestFit, bool filter = true, string term = null);
        Task<Ellucian.Colleague.Dtos.Student.AcademicHistory3> GetAcademicHistory3Async(string studentId, bool bestFit = false, bool filter = true, string term = null);
        Task<Ellucian.Colleague.Dtos.Student.AcademicHistory4> GetAcademicHistory4Async(string studentId, bool bestFit = false, bool filter = true, string term = null, bool includeDrops = false);

        [Obsolete("Obsolete as of API 1.18. Use QueryAcademicHistory2Async.")]
        Task<IEnumerable<Dtos.Student.AcademicHistoryBatch>> QueryAcademicHistoryAsync(Dtos.Student.AcademicHistoryQueryCriteria criteria);
        Task<IEnumerable<Dtos.Student.AcademicHistoryBatch2>> QueryAcademicHistory2Async(Dtos.Student.AcademicHistoryQueryCriteria criteria);

        Task<IEnumerable<Dtos.Student.AcademicHistoryLevel>> QueryAcademicHistoryLevelAsync(Dtos.Student.AcademicHistoryQueryCriteria criteria);
        Task<IEnumerable<Dtos.Student.AcademicHistoryLevel2>> QueryAcademicHistoryLevel2Async(Dtos.Student.AcademicHistoryQueryCriteria criteria);
        Task<IEnumerable<Dtos.Student.AcademicHistoryLevel3>> QueryAcademicHistoryLevel3Async(Dtos.Student.AcademicHistoryQueryCriteria criteria);
        Task<IEnumerable<Dtos.Student.PilotAcademicHistoryLevel>> QueryPilotAcademicHistoryLevelAsync(Dtos.Student.AcademicHistoryQueryCriteria criteria);
        [Obsolete("Obsolete as of API 1.18. Use GetAcademicHistoryByIds2Async.")]
        Task<IEnumerable<Dtos.Student.AcademicHistoryBatch>> GetAcademicHistoryByIdsAsync(IEnumerable<string> studentIds, bool bestFit, bool filter = true, string term = null);
        /// <summary>
        /// Gets the <see cref="AcademicHistoryBatch2">academic history</see> for a student based on criteria
        /// </summary>
        /// <param name="studentIds">IDs of students for whom academic histories will be retrieved</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <returns>The <see cref="AcademicHistoryBatch2">academic history</see> for a student</returns>
        Task<IEnumerable<Dtos.Student.AcademicHistoryBatch2>> GetAcademicHistoryByIds2Async(IEnumerable<string> studentIds, bool bestFit, bool filter = true, string term = null);

        Task<Ellucian.Colleague.Dtos.Student.AcademicHistory2> ConvertAcademicCreditsToAcademicHistoryDtoAsync(string studentId, IEnumerable<AcademicCredit> academicCredits, Domain.Student.Entities.Student student = null);
        Task<Ellucian.Colleague.Dtos.Student.AcademicHistory3> ConvertAcademicCreditsToAcademicHistoryDto2Async(string studentId, IEnumerable<AcademicCredit> academicCredits, Domain.Student.Entities.Student student = null);
        Task<Ellucian.Colleague.Dtos.Student.AcademicHistory4> ConvertAcademicCreditsToAcademicHistoryDto4Async(string studentId, IEnumerable<AcademicCredit> academicCredits, Domain.Student.Entities.Student student = null);
        Task<IEnumerable<Dtos.Student.AcademicHistoryLevel>> GetAcademicHistoryLevelByIdsAsync(IEnumerable<string> studentIds, bool bestFit, bool filter = true, string term = null);
        Task<IEnumerable<Dtos.Student.AcademicHistoryLevel2>> GetAcademicHistoryLevel2ByIdsAsync(IEnumerable<string> studentIds, bool bestFit, bool filter = true, string term = null);
        Task<IEnumerable<Dtos.Student.PilotAcademicHistoryLevel>> GetPilotAcademicHistoryLevelByIdsAsync(IEnumerable<string> studentIds, bool bestFit, bool filter = true, string term = null);
        Task<IEnumerable<Dtos.Student.StudentEnrollment>> GetInvalidStudentEnrollmentAsync(IEnumerable<Dtos.Student.StudentEnrollment> enrollmentKeys);
        /// <summary>
        /// Returns a list of academic credits for the specified section Ids in the criteria
        /// </summary>
        /// <param name="criteria">Criteria that contains a list of sections and some other options</param>
        /// <returns>List of AcademicCredit2 Dtos</returns>
        [Obsolete("Obsolete as of API 1.18. Use QueryAcademicCredits2Async.")]
        Task<IEnumerable<Dtos.Student.AcademicCredit2>> QueryAcademicCreditsAsync(Dtos.Student.AcademicCreditQueryCriteria criteria);
        /// <summary>
        /// Returns a list of academic credits for the specified section Ids in the criteria
        /// </summary>
        /// <param name="criteria">Criteria that contains a list of sections and some other options</param>
        /// <returns>List of AcademicCredit3 Dtos</returns>
        Task<IEnumerable<Dtos.Student.AcademicCredit3>> QueryAcademicCredits2Async(Dtos.Student.AcademicCreditQueryCriteria criteria);
        /// <summary>
        /// Returns a list of academic credits records for the specified section Ids in the criteria
        /// Also returns list of invalid academic credits Ids that are missing from STUDENT.ACAD.CRED file.
        /// </summary>
        /// <param name="criteria">Criteria that contains a list of sections and some other options</param>
        /// <returns><see cref="AcademicCreditsWithInvalidKeys">AcademicCreditsWithInvalidKeys</see> Dtos</returns>
        Task<Dtos.Student.AcademicCreditsWithInvalidKeys> QueryAcademicCreditsWithInvalidKeysAsync(Dtos.Student.AcademicCreditQueryCriteria criteria);


    }
}
