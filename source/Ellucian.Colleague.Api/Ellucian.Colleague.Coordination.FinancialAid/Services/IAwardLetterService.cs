//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface for an AwardLetterService
    /// </summary>
    public interface IAwardLetterService
    {
        #region Obsolete Methods
    
        /// <summary>
        /// Get a student's award letters
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <returns>A list of award letters for all the award years for which the student has financial aid data</returns>
        [Obsolete("Obsolete as of API 1.9. Use GetAwardLetters2")]
        IEnumerable<AwardLetter> GetAwardLetters(string studentId);

        /// <summary>
        /// Get a student's award letters. Returns award letter objects even if there are no associated awards
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <returns>A list of award letters for all the award years for which the student has financial aid data</returns>
        [Obsolete("Obsolete as of API 1.10. Use GetAwardLetters3")]
        IEnumerable<AwardLetter> GetAwardLetters2(string studentId);

        /// <summary>
        /// Get a student's award letter for a particular year.
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <param name="awardYear">The award year for which to get the award letter</param>
        /// <returns>A single student award letter for the given year</returns>
        [Obsolete("Obsolete as of API 1.9. Use GetAwardLetters2")]
        AwardLetter GetAwardLetters(string studentId, string awardYear);

        /// <summary>
        /// Get a student's award letter for a particular year. Returns an award letter objects even if there are 
        /// no associated awards
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <param name="awardYear">The award year for which to get the award letter</param>
        /// <returns>A single student award letter for the given year</returns>
        [Obsolete("Obsolete as of API 1.10. Use GetAwardLetters3")]
        AwardLetter GetAwardLetters2(string studentId, string awardYear);

        /// <summary>
        /// Get a student's award letter for a particularly year as a byte array that represents a PDF report .
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <param name="awardYear">The award year for which to get the award letter</param>
        /// <param name="pathToReport">The path on the server to the report template</param>
        /// <param name="pathToLogo">The path on the server to the institutions logo image to be used on the report</param>
        /// <returns>A byte array representation of a PDF award letter report.</returns>
        [Obsolete("Obsolete as of API 1.9. Use GetAwardLetters2")]
        byte[] GetAwardLetters(string studentId, string awardYear, string pathToReport, string pathToLogo);

        /// <summary>
        /// Get a student's award letter for a particularly year as a byte array that represents a PDF report .
        /// Award letter object used to generate the report might not contain any awards
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <param name="awardYear">The award year for which to get the award letter</param>
        /// <param name="pathToReport">The path on the server to the .rdlc template</param>
        /// <param name="pathToLogo">The path on the server to the institutions logo image to be used on the report</param>
        /// <returns>A byte array representation of a PDF award letter report.</returns>
        [Obsolete("Obsolete as of API 1.10. Use GetAwardLetterReport3")]
        byte[] GetAwardLetters2(string studentId, string awardYear, string pathToReport, string pathToLogo);

        /// <summary>
        /// Get the byte array representation as a PDF of the given AwardLetter DTO
        /// </summary>
        /// <param name="awardLetter">AwardLetter DTO to use as the data source for producing the award letter report.</param>
        /// <param name="pathToLogo">The path on the server to the report template</param>
        /// <param name="pathToReport">The path on the server to the institutions logo image to be used on the report</param>
        /// <returns>A byte array representation of a PDF award letter report.</returns>
        [Obsolete("Obsolete as of API 1.10. Use GetAwardLetterReport2")]
        byte[] GetAwardLetters(AwardLetter awardLetterDto, string pathToReport, string pathToLogo);

        /// <summary>
        /// This method updates an Award Letter
        /// </summary>
        /// <param name="awardLetter">The award letter object that contains the updated data</param>
        /// <returns>An award letter object with updated data</returns>
        [Obsolete("Obsolete as of API 1.10. Use UpdateAwardLetter2")]
        AwardLetter UpdateAwardLetter(AwardLetter awardLetter);

        /// <summary>
        /// Get the most recent Award Letters from each active year.
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <returns>A list of most recent award letters within a year for all the award years in which the student has financial aid data</returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLetters4Async")]
        Task<IEnumerable<AwardLetter2>> GetAwardLetters3Async(string studentId);

        /// <summary>
        /// Get the most recent award letter for a particular year. Returns an award letter objects even if there are 
        /// no associated awards
        /// </summary>
        /// <param name="studentId">student id for whom to retrieve the award letter</param>
        /// <param name="awardYear">award year of the award letter</param>
        /// <returns>AwardLetter2 DTO</returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLetter4Async")]
        Task<AwardLetter2> GetAwardLetter3Async(string studentId, string awardYear);

        /// <summary>
        /// Asynchronous get method to retrieve an award letter DTO by student id and award letter record id
        /// </summary>
        /// <param name="studentId">student id /param>        
        /// <param name="recordId">award letter record id</param>
        /// <returns>AwardLetter2 DTO</returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLetterById2Async")]
        Task<AwardLetter2> GetAwardLetterByIdAsync(string studentId, string recordId);

        /// <summary>
        /// Gets the award letter report in byte array representation
        /// </summary>
        /// <param name="awardLetterDto">award letter dto</param>
        /// <param name="pathToReport">path to report</param>
        /// <param name="pathToLogo">path to logo to be displayed on the report</param>
        /// <returns>byte array representation of the award letter data</returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLetterReport4Async")]
        Task<byte[]> GetAwardLetterReport3Async(AwardLetter2 awardLetterDto, string pathToReport, string pathToLogo);

        /// <summary>
        /// Synchronous method that builds award letter report and converts it to the byte array
        /// </summary>
        /// <param name="awardLetterDto">award letter DTO</param>
        /// <param name="awardLetterConfigurationDto">award letter configuration DTO</param>
        /// <param name="pathToReport">path to report</param>
        /// <param name="pathToLogo">path to logo</param>
        /// <returns>byte array of award letter data</returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLetterReport3")]
        byte[] GetAwardLetterReport2(AwardLetter2 awardLetterDto, AwardLetterConfiguration awardLetterConfigurationDto, string pathToReport, string pathToLogo);

        /// <summary>
        /// This method updates an Award Letter
        /// </summary>
        /// <param name="awardLetter">The award letter object that contains the updated data</param>
        /// <returns>An award letter object with updated data</returns>
        [Obsolete("Obsolete as of API 1.22. Use UpdateAwardLetter3Async")]
        Task<AwardLetter2> UpdateAwardLetter2Async(AwardLetter2 awardLetter);

        #endregion

        /// <summary>
        /// Get the most recent Award Letters from each active year.
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <returns>A list of most recent award letters within a year for all the award years in which the student has financial aid data</returns>
        Task<IEnumerable<AwardLetter3>> GetAwardLetters4Async(string studentId);

        /// <summary>
        /// Get the most recent award letter for a particular year. Returns an award letter objects even if there are 
        /// no associated awards
        /// </summary>
        /// <param name="studentId">student id for whom to retrieve the award letter</param>
        /// <param name="awardYear">award year of the award letter</param>
        /// <returns>AwardLetter3 DTO</returns>
        Task<AwardLetter3> GetAwardLetter4Async(string studentId, string awardYear);

        /// <summary>
        /// Asynchronous get method to retrieve an award letter DTO by student id and award letter record id
        /// </summary>
        /// <param name="studentId">student id /param>        
        /// <param name="recordId">award letter record id</param>
        /// <returns>AwardLetter3 DTO</returns>
        Task<AwardLetter3> GetAwardLetterById2Async(string studentId, string recordId);

        /// <summary>
        /// Gets the award letter report in byte array representation
        /// </summary>
        /// <param name="awardLetterDto">award letter dto</param>
        /// <param name="pathToReport">path to report</param>
        /// <param name="pathToLogo">path to logo to be displayed on the report</param>
        /// <returns>byte array representation of the award letter data</returns>
        Task<byte[]> GetAwardLetterReport4Async(AwardLetter3 awardLetterDto, string pathToReport, string pathToLogo);

        /// <summary>
        /// Synchronous method that builds award letter report and converts it to the byte array
        /// </summary>
        /// <param name="awardLetterDto">award letter DTO</param>
        /// <param name="awardLetterConfigurationDto">award letter configuration DTO</param>
        /// <param name="pathToReport">path to report</param>
        /// <param name="pathToLogo">path to logo</param>
        /// <returns>byte array of award letter data</returns>
        byte[] GetAwardLetterReport3(AwardLetter3 awardLetterDto, AwardLetterConfiguration awardLetterConfigurationDto, string pathToReport, string pathToLogo);

        /// <summary>
        /// This method updates an Award Letter
        /// </summary>
        /// <param name="awardLetter">The award letter object that contains the updated data</param>
        /// <returns>An award letter object with updated data</returns>
        Task<AwardLetter3> UpdateAwardLetter3Async(AwardLetter3 awardLetter);
    }
}
