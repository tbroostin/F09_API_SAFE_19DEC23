// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Finance
{
    public interface IAccountActivityService
    {
        /// <summary>
        /// Get account activity for a student by term
        /// </summary>
        /// <param name="termId">ID of term</param>
        /// <param name="personId">Student ID</param>
        /// <returns>Account activity detail</returns>
        [Obsolete("Obsolete as of API version 1.8, use GetAccountActivityByTermForStudent2 instead")]
        DetailedAccountPeriod GetAccountActivityByTermForStudent(string termId, string personId);

        /// <summary>
        /// Get account activity for a student by term
        /// </summary>
        /// <param name="termId">ID of term</param>
        /// <param name="personId">Student ID</param>
        /// <returns>Account activity detail</returns>
        DetailedAccountPeriod GetAccountActivityByTermForStudent2(string termId, string personId);

        /// <summary>
        /// Get account activity periods for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Account activity period detail</returns>
        AccountActivityPeriods GetAccountActivityPeriodsForStudent(string studentId);

        /// <summary>
        /// Get account activity for a student by period (PCF)
        /// </summary>
        /// <param name="periods">List of terms for the desired period</param>
        /// <param name="startDate">Period start date</param>
        /// <param name="endDate">Period end date</param>
        /// <param name="personId">Student ID</param>
        /// <returns>Account activity detail</returns>
        [Obsolete("Obsolete as of API version 1.8, use PostAccountActivityByPeriodForStudent2 instead")]
        DetailedAccountPeriod PostAccountActivityByPeriodForStudent(IEnumerable<string> periods, DateTime? startDate, DateTime? endDate, string personId);
        
        /// <summary>
        /// Get account activity for a student by period (PCF)
        /// </summary>
        /// <param name="periods">List of terms for the desired period</param>
        /// <param name="startDate">Period start date</param>
        /// <param name="endDate">Period end date</param>
        /// <param name="personId">Student ID</param>
        /// <returns>Account activity detail</returns>
        DetailedAccountPeriod PostAccountActivityByPeriodForStudent2(IEnumerable<string> periods, DateTime? startDate, DateTime? endDate, string personId);
        /// <summary>
        /// Get deposits due for a person
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>List of deposits due</returns>
        IEnumerable<DepositDue> GetDepositsDue(string id);

        /// <summary>
        /// Get an accountholder
        /// </summary>
        /// <param name="id">Accountholder ID</param>
        /// <returns>Accountholder information</returns>
        AccountHolder GetAccountHolder(string id);

        /// <summary>
        /// Gets student award disbursement information for the specified award id and award year
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="awardYearCode">award year code</param>
        /// <param name="awardId">award id</param>
        /// <returns>Student Award Disbursement information</returns>
        Task<StudentAwardDisbursementInfo> GetStudentAwardDisbursementInfoAsync(string studentId, string awardYearCode, string awardId);

        /// <summary>
        /// Returns information about potentially untransmitted D7 financial aid, based on
        /// current charges, credits, and awarded aid.
        /// </summary>
        /// <param name="criteria">The <see cref="PotentialD7FinancialAidCriteria"/> criteria of
        /// potential financial aid for which to search.</param>
        /// <returns>Enumeration of <see cref="PotentialD7FinancialAid"/>  
        /// awards and potential award amounts.</returns>
        Task<IEnumerable<PotentialD7FinancialAid>> GetPotentialD7FinancialAidAsync(PotentialD7FinancialAidCriteria criteria);
    }
}
