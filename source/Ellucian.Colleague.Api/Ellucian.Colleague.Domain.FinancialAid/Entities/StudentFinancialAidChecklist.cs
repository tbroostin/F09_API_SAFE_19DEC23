/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// A list of checklist items that a student needs to complete for a given year.
    /// </summary>
    [Serializable]
    public class StudentFinancialAidChecklist
    {
        /// <summary>
        /// The Student's Colleague PERSON id
        /// </summary>
        public string StudentId { get { return studentId; } }
        private readonly string studentId;

        /// <summary>
        /// The checklist year
        /// </summary>
        public string AwardYear { get { return awardYear; } }
        private readonly string awardYear;

        /// <summary>
        /// List of checklist items and their Control Status
        /// </summary>
        public List<StudentChecklistItem> ChecklistItems { get; set; }

        /// <summary>
        /// Constructor for StudentFinancialAidChecklist
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student to whom this FA Checklist belongs</param>
        /// <param name="awardYear">The Financial Aid award year this checklist applies to </param>
        public StudentFinancialAidChecklist(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId);
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException(awardYear);
            }

            this.studentId = studentId;
            this.awardYear = awardYear;
            ChecklistItems = new List<StudentChecklistItem>();
        }

        /// <summary>
        /// Two StudentFinancialAidChecklist objects are equal when their AwardYear and StudentId are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var studentFinancialAidChecklist = obj as StudentFinancialAidChecklist;

            if (studentFinancialAidChecklist.AwardYear == this.AwardYear &&
                studentFinancialAidChecklist.StudentId == this.StudentId)
            {

                return true;
            }

            return false;
        }

        /// <summary>
        /// Computes the HashCode based on the AwardYear and StudentId
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return AwardYear.GetHashCode() ^ StudentId.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of the StudentFinancialAidChecklist object's AwardYear and StudentId
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}*{1}", AwardYear, StudentId);
        }

    }
}
