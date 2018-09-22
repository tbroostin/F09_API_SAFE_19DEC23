/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// The StudentAward class defines a student's award for a particular award year.
    /// </summary>
    [Serializable]
    public class StudentAward
    {
        private readonly StudentAwardYear _StudentAwardYear;
        private readonly string _StudentId;
        private readonly Award _Award;
        private readonly bool _IsEligible;

        /// <summary>
        /// The AwardYear this StudentAward is assigned to. 
        /// </summary>
        public StudentAwardYear StudentAwardYear { get { return _StudentAwardYear; } }

        /// <summary>
        /// The PERSON Id of the student who owns this award
        /// </summary>
        public string StudentId { get { return _StudentId; } }

        /// <summary>
        /// The Award Id of this award
        /// </summary>
        public Award Award { get { return _Award; } }

        /// <summary>
        /// A unique set of StudentAwardPeriods, which breakdown this StudentAward into smaller pieces
        /// </summary>
        public List<StudentAwardPeriod> StudentAwardPeriods { get; set; }

        public decimal? AwardAmount
        {
            get
            {
                var nonNullAwardAmounts = StudentAwardPeriods.Where(p => p.AwardAmount.HasValue);
                if (nonNullAwardAmounts.Count() > 0)
                {
                    return nonNullAwardAmounts.Sum(p => p.AwardAmount.Value);
                }
                return null;
            }
        }

        /// <summary>
        /// Flag indicating whether or not the student passed the eligibility rules defined for this award
        /// </summary>
        public bool IsEligible { get { return _IsEligible; } }

        /// <summary>
        /// Flag indicating whether any of this StudentAwards StudentAwardPeriods are modifiable
        /// </summary>
        public bool IsAmountModifiable
        {
            get
            {
                return (StudentAwardPeriods.Any(p => p.IsAmountModifiable));
            }
        }

        /// <summary>
        /// Indicates whether or not the Student Award can be viewed by the consumer of this API endpoint.
        /// Generally when IsViewable is false, this object is not exposed by the API.
        /// </summary>
        public bool IsViewable
        {
            get
            {
                return (StudentAwardPeriods.Any(p => p.IsViewable));

            }
        }

        /// <summary>
        /// The existing AwardPackageChangeRequest Id that is pending for this StudentAward
        /// </summary>
        public string PendingChangeRequestId { get; set; }

        /// <summary>
        /// A StudentAward is identified by its AwardYear, StudentId, and AwardId. This constructor
        /// sets the values of those attributes.
        /// </summary>
        /// <param name="studentAwardYear">Award Year</param>
        /// <param name="studentId">StudentId</param>
        /// <param name="award">AwardId</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are empty or null</exception>
        public StudentAward(StudentAwardYear studentAwardYear, string studentId, Award award, bool isEligible)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (award == null)
            {
                throw new ArgumentNullException("awardId");
            }

            _StudentAwardYear = studentAwardYear;
            _StudentId = studentId;
            _Award = award;

            _IsEligible = isEligible;
            PendingChangeRequestId = string.Empty;

            StudentAwardPeriods = new List<StudentAwardPeriod>();

        }

        /// <summary>
        /// Two StudentAward objects are equal if their AwardYear, StudentId and AwardId attributes
        /// are equal.
        /// </summary>
        /// <param name="obj">The object to compare to this StudentAward</param>
        /// <returns>True if the AwardYear, StudentId and AwardId are equal. False, otherwise, or if the parameter object is null or not of type StudentAward</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var studentAward = obj as StudentAward;

            if (studentAward.StudentAwardYear.Equals(this.StudentAwardYear) &&
                studentAward.StudentId == this.StudentId &&
                studentAward.Award.Equals(this.Award))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Computes the HashCode of this object
        /// </summary>
        /// <returns>This object's HashCode</returns>
        public override int GetHashCode()
        {
            return StudentAwardYear.GetHashCode() ^ StudentId.GetHashCode() ^ Award.GetHashCode();
        }

        /// <summary>
        /// Gets a string representation of this object.
        /// </summary>
        /// <returns>The award year concatenated to the award code</returns>
        public override string ToString()
        {
            return StudentAwardYear.Code + " - " + Award.Code;
        }
    }
}
