//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// AwardLetterRepository class exposes database access to Colleague Award Letters. It
    /// gathers data from numerous tables based on student data and creates AwardLetter
    /// objects.
    /// </summary>
    public interface IAwardLetterRepository
    {
        /// <summary>
        /// This method gets award letters for a student across all the years a student has
        /// financial aid data.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to generate award letters</param>
        /// <returns>A list of year-specific award letters for the given student.</returns>
        IEnumerable<AwardLetter> GetAwardLetters(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Fafsa> fafsaRecords);

        /// <summary>
        /// This method gets an award letter for a student for a the given award year.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to generate award letter</param>
        /// <param name="studentAwardYear">The award year for which to get the award letter</param>
        /// <returns>An award letter object specific to the given award year</returns>
        AwardLetter GetAwardLetter(string studentId, StudentAwardYear studentAwardYear, Fafsa fafsaRecord);

        /// <summary>
        /// This method updates an Award Letter, specifically the award letter's Accepted Date
        /// </summary>
        /// <param name="awardLetter">The award letter object that contains the data with which to update the database</param>
        /// <returns>An award letter object with updated data</returns>
        AwardLetter UpdateAwardLetter(AwardLetter awardLetter, StudentAwardYear studentAwardYear, Fafsa fafsaRecord);
    }
}
