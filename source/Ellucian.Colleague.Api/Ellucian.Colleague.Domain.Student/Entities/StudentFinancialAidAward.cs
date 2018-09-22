// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Used within the Ethos Data Model APIs to represent a Student Financial Aid award.
    /// </summary>
    [Serializable]
    public class StudentFinancialAidAward
    {
        /// <summary>
        /// Unique identifier (GUID) for Student Financial Aid Award
        ///  
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// Student who will be receiving the financial aid award.
        /// </summary>
        public string StudentId { get; private set; }

        /// <summary>
        /// The fund that is awarded to the student.
        /// </summary>
        public string AwardFundId { get; private set; }

        /// <summary>
        /// The year that the award is assigned.
        /// </summary>
        public string AidYearId { get; private set; }

        /// <summary>
        /// Award Period amounts and disbursements
        /// </summary>
        public List<StudentAwardHistoryByPeriod> AwardHistory { get; set; }

        /// <summary>
        /// Constructor for StudentFinancialAidAward object used for Ethos Data Model APIs.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="studentId"></param>
        /// <param name="awardFundId"></param>
        /// <param name="aidYearId"></param>
        public StudentFinancialAidAward(string guid, string studentId, string awardFundId, string aidYearId)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Guid is required when creating a StudentFinancialAidAward. ");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id is required when creating a StudentFinancialAidAward. ");
            }
            if (string.IsNullOrEmpty(awardFundId))
            {
                throw new ArgumentNullException("awardFundId", "Award Fund Id is required when creating a StudentFinancialAidAward. ");
            }
            if (string.IsNullOrEmpty(aidYearId))
            {
                throw new ArgumentNullException("aidYearId", "Aid Year is required when creating a StudentFinancialAidAward. ");
            }
            Guid = guid;
            StudentId = studentId;
            AwardFundId = awardFundId;
            AidYearId = aidYearId;
            AwardHistory = new List<StudentAwardHistoryByPeriod>();
        }
    }
}
