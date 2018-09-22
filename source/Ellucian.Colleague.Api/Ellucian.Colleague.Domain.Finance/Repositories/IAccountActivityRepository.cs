// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Finance.Repositories
{
    /// <summary>
    /// Interface to the Account Activity repository
    /// </summary>
    public interface IAccountActivityRepository
    {
        /// <summary>
        /// Get the list of account periods for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>List of account periods</returns>
        IEnumerable<AccountPeriod> GetAccountPeriods(string studentId);

        /// <summary>
        /// Get the non-term period info for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Non-term period info</returns>
        AccountPeriod GetNonTermAccountPeriod(string studentId);

        /// <summary>
        /// Get a student's account activity for a specific term
        /// </summary>
        /// <param name="termId">Term ID</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>Account activity detail</returns>
        [Obsolete("Obsolete as of API version 1.8, use GetTermActivityForStudent2 instead")]
        DetailedAccountPeriod GetTermActivityForStudent(string termId, string studentId);

        /// <summary>
        /// Get a student's account activity for a specific term
        /// </summary>
        /// <param name="termId">Term ID</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>Account activity detail</returns>
        DetailedAccountPeriod GetTermActivityForStudent2(string termId, string studentId);

        /// <summary>
        /// Get a student's account activity for a specific period (PCF)
        /// </summary>
        /// <param name="termIds">List of terms in the period</param>
        /// <param name="startDate">Period start date</param>
        /// <param name="endDate">Period end date</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>Account activity detail</returns>
        [Obsolete("Obsolete as of API version 1.8, use GetPeriodActivityForStudent2 instead")]
        DetailedAccountPeriod GetPeriodActivityForStudent(IEnumerable<string> termIds, DateTime? startDate, DateTime? endDate, string studentId);

        /// <summary>
        /// Get a student's account activity for a specific period (PCF)
        /// </summary>
        /// <param name="termIds">List of terms in the period</param>
        /// <param name="startDate">Period start date</param>
        /// <param name="endDate">Period end date</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>Account activity detail</returns>
        DetailedAccountPeriod GetPeriodActivityForStudent2(IEnumerable<string> termIds, DateTime? startDate, DateTime? endDate, string studentId);

        /// <summary>
        /// Gets student award disbursement information for the specified award for the specified year
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="awardYearCode">award year code</param>
        /// <param name="awardId">award id</param>
        /// <param name="awardCategory">award category: loan, pell, teach</param>
        /// <returns></returns>
        Task<Entities.AccountActivity.StudentAwardDisbursementInfo> GetStudentAwardDisbursementInfoAsync(string studentId, string awardYearCode, string awardId, TIVAwardCategory awardCategory);
    }
}
