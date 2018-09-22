/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// StudentBudgetComponent DTO describes student specific estimated costs
    /// </summary>
    [Serializable]
    public class StudentBudgetComponent
    {
        /// <summary>
        /// Colleague PERSON id of the student to whom this budget component belongs
        /// </summary>
        public string StudentId { get { return _StudentId; } }
        private readonly string _StudentId;

        /// <summary>
        /// The Award Year this budget component belongs to
        /// </summary>
        public string AwardYear { get { return _AwardYear; } }
        private readonly string _AwardYear;

        /// <summary>
        /// The Code of the BudgetComponent object, which contains the component's description, shopping sheet group, etc.
        /// Use this Code and the AwardYear to find the BudgetComponent
        /// </summary>
        public string BudgetComponentCode { get { return _BudgetComponentCode; } }
        private readonly string _BudgetComponentCode;

        /// <summary>
        /// The budget amount for campus based costs that was originally assigned to this student. This amount
        /// does not change once the budget component is assigned to the student
        /// </summary>
        public int CampusBasedOriginalAmount { get { return _CampusBasedOriginalAmount; } }
        private readonly int _CampusBasedOriginalAmount;

        /// <summary>
        /// The override budget amount for campus based costs. Once the budget component is assigned the student's budget 
        /// amount is updated via this attribute. A null value indicates there is no override. To set a non-null value, the value
        /// must be greater than zero. Attempts to set this attribute to a value less than zero are ignored
        /// </summary>
        public int? CampusBasedOverrideAmount
        {
            get { return _CampusBasedOverrideAmount; }
            set
            {
                if (!value.HasValue || value.Value >= 0)
                {
                    _CampusBasedOverrideAmount = value;
                }

            }
        }
        private int? _CampusBasedOverrideAmount;

        /// <summary>
        /// Create a new StudentBudgetComponent
        /// </summary>
        /// <param name="awardYear">Required: The awardYear to which this component applies</param>
        /// <param name="studentId">Required: The Colleague PERSON id of the student to whom this component applies </param>
        /// <param name="budgetComponentCode">Required: The Code of the BudgetComponent that this student component derives from</param>
        /// <param name="campusBasedOriginalAmount">Required: The budget amount for campus based costs that was originally assigned to this student. Must be greater than or equal to zero.</param>
        /// <param name="campusBasedOverrideAmount">The override budget amount for campus based costs. The default is null, meaning there is no override. Values less than zero are ignored </param>
        /// <exception cref="ArgumentNullException">Thrown if any required arguments are null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the campusBasedOriginalAmount argument is less than zero.</exception>
        public StudentBudgetComponent(
            string awardYear,
            string studentId,
            string budgetComponentCode,
            int campusBasedOriginalAmount)
        {
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(budgetComponentCode))
            {
                throw new ArgumentNullException("budgetComponentCode");
            }
            if (campusBasedOriginalAmount < 0)
            {
                throw new ArgumentOutOfRangeException("campusBasedOriginalAmount", "amount must be greater than or equal to zero");
            }

            _AwardYear = awardYear;
            _StudentId = studentId;
            _BudgetComponentCode = budgetComponentCode;
            _CampusBasedOriginalAmount = campusBasedOriginalAmount;
        }

        /// <summary>
        /// Determines the equality of two StudentBudgetComponent objects. Equal if they share the same
        /// studentId, awardYear, and budgetComponentCode.
        /// </summary>
        /// <param name="obj">A StudentBudgetComponent object to compare to this one</param>
        /// <returns>True, if the two objects are equal. False, otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var studentBudgetComponent = obj as StudentBudgetComponent;

            if (studentBudgetComponent.StudentId == this.StudentId &&
                studentBudgetComponent.AwardYear == this.AwardYear &&
                studentBudgetComponent.BudgetComponentCode == this.BudgetComponentCode)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Computes the HashCode of this StudentBudgetComponent based on the AwardYear, StudentId, and BudgetComponentCode
        /// </summary>
        /// <returns>HashCode of this StudentBudgetComponent</returns>
        public override int GetHashCode()
        {
            return AwardYear.GetHashCode() ^ StudentId.GetHashCode() ^ BudgetComponentCode.GetHashCode();
        }

        /// <summary>
        /// Computes a string representation of this StudentBudgetComponent based on the AwardYear, StudentId, and BudgetComponentCode
        /// </summary>
        /// <returns>A string representation of this object</returns>
        public override string ToString()
        {
            return string.Format("{0}*{1}*{2}", BudgetComponentCode, StudentId, AwardYear);
        }
    }
}
