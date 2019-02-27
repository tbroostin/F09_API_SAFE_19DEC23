/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    public interface IAwardLetterHistoryRepository
    {
        #region Obsolete methods
        /// <summary>
        /// This method gets award letters for a student across all the years a student has
        /// financial aid data.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to generate award letters</param>
        /// <param name="studentAwardYears">list of student award years</param>
        /// <param name="allAwards">awards reference data</param>
        /// <returns>A list of year-specific award letters for the given student.</returns>
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetter2Async instead")]
        Task <IEnumerable<AwardLetter2>> GetAwardLettersAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards);

        /// <summary>
        /// This method gets an award letter for a student for a the given award year.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to generate award letter</param>
        /// <param name="studentAwardYear">The award year for which to get the award letter</param>
        /// <param name="allAwards">awards reference data</param>
        /// ,param name="createAwardLetterHistoryRecord">boolean flag to decide whether we should call the create method or not
        /// <returns>An award letter object specific to the given award year</returns>
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetter2Async instead")]
        Task <AwardLetter2> GetAwardLetterAsync(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, bool createAwardLetterHistoryRecord);

        /// <summary>
        /// Gets award letter history record by award letter record id
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="recordId">award letter history record id</param>
        /// <param name="studentAwardYears">list of student award years</param>        
        /// <param name="allAwards">reference award data</param>
        /// <returns>AwardLetter2 entity</returns>
        [Obsolete("Obsolete as of Api version 1.22, use GetAwardLetterById2Async instead")]
        Task<AwardLetter2> GetAwardLetterByIdAsync(string studentId, string recordId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards);

        /// <summary>
        /// Updates the Date Accepted for a single award letter
        /// </summary>
        /// <param name="studentId">student Id</param>
        /// <param name="studentAwardLetter">Award letter history record to update</param>
        /// <param name="studentAwardYear">StudentAwardYear record</param>
        /// <param name="allAwards">List of all award codes</param>
        /// <returns></returns>
        [Obsolete("Obsolete as of Api version 1.22, use UpdateAwardLetter2Async instead")]
        Task<AwardLetter2> UpdateAwardLetterAsync(string studentId, AwardLetter2 studentAwardLetter, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards);

        #endregion

        /// <summary>
        /// This method gets award letters for a student across all the years a student has
        /// financial aid data.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to generate award letters</param>
        /// <param name="studentAwardYears">list of student award years</param>
        /// <param name="allAwards">awards reference data</param>
        /// <returns>A list of year-specific award letters for the given student.</returns>
        Task<IEnumerable<AwardLetter3>> GetAwardLetters2Async(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards);

        /// <summary>
        /// This method gets an award letter for a student for a the given award year.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to generate award letter</param>
        /// <param name="studentAwardYear">The award year for which to get the award letter</param>
        /// <param name="allAwards">awards reference data</param>
        /// ,param name="createAwardLetterHistoryRecord">boolean flag to decide whether we should call the create method or not
        /// <returns>An award letter object specific to the given award year</returns>
        Task<AwardLetter3> GetAwardLetter2Async(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, bool createAwardLetterHistoryRecord);

        /// <summary>
        /// Gets award letter history record by award letter record id
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="recordId">award letter history record id</param>
        /// <param name="studentAwardYears">list of student award years</param>        
        /// <param name="allAwards">reference award data</param>
        /// <returns>AwardLetter3 entity</returns>
        Task<AwardLetter3> GetAwardLetterById2Async(string studentId, string recordId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards);

        /// <summary>
        /// Updates the Date Accepted for a single award letter
        /// </summary>
        /// <param name="studentId">student Id</param>
        /// <param name="studentAwardLetter">Award letter history record to update</param>
        /// <param name="studentAwardYear">StudentAwardYear record</param>
        /// <param name="allAwards">List of all award codes</param>
        /// <returns></returns>
        Task<AwardLetter3> UpdateAwardLetter2Async(string studentId, AwardLetter3 studentAwardLetter, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards);

    }
}
