/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// AwardPackageChangeRequest object provides identifies potential and pending changes to a student's award package.
    /// Some requests are be applied automatically. Other requests end up in a queue to be reviewed by a Financial Aid Counselor.
    /// </summary>
    [Serializable]
    public class AwardPackageChangeRequest
    {
        /// <summary>
        /// The database Id of the Award Change Request record.
        /// Can only be set if it's already empty.
        /// If the Id has a value, you cannot change the value of this attribute.
        /// </summary>
        public string Id
        {
            get { return id; }
            set
            {
                if (string.IsNullOrEmpty(id))
                {
                    id = value;
                }
            }

        }
        private string id;

        /// <summary>
        /// The StudentId to whom this change request applies. 
        /// </summary>
        public string StudentId { get { return studentId; } }
        private readonly string studentId;

        /// <summary>
        /// The AwardYear to which this change request applies
        /// </summary>
        public string AwardYearId { get { return awardYearId; } }
        private readonly string awardYearId;

        /// <summary>
        /// The AwardId to which this change request applies
        /// </summary>
        public string AwardId { get { return awardId; } }
        private readonly string awardId;

        /// <summary>
        /// The Colleague PERSON id of the financial aid counselor to whom this request is assigned       
        /// </summary>        
        public string AssignedToCounselorId { get; set; }

        /// <summary>
        /// The date and time this Change Request was submitted and entered into the database.
        /// </summary>
        public DateTimeOffset? CreateDateTime { get; set; }

        /// <summary>
        /// This is a list of AwardPeriodChangeRequest objects. 
        /// Only award periods specified in this list will be updated.
        /// </summary>
        public List<AwardPeriodChangeRequest> AwardPeriodChangeRequests { get; set; }

        /// <summary>
        /// This is an override for the check that all subsidized awards are accepted/declined.
        /// </summary>
        public bool OverrideUnsubsidizedLoanCheck { get; set; }

        public bool IsForStudentAward(StudentAward studentAward)
        {
            if (studentAward == null) return false;

            if (studentAward.StudentAwardYear.Code == this.AwardYearId &&
                studentAward.StudentId == this.StudentId &&
                studentAward.Award.Code == this.AwardId)
            {
                return true;
            }
            return false;
        }

        public AwardPackageChangeRequest(string id, string studentId, string awardYearId, string awardId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYearId))
            {
                throw new ArgumentNullException("awardYearId");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId");
            }

            this.id = id;
            this.studentId = studentId;
            this.awardYearId = awardYearId;
            this.awardId = awardId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var changeRequest = obj as AwardPackageChangeRequest;

            if (changeRequest.Id == this.Id ||
                (changeRequest.StudentId == this.StudentId &&
                changeRequest.AwardId == this.AwardId &&
                changeRequest.AwardYearId == this.AwardYearId))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return StudentId.GetHashCode() ^ AwardYearId.GetHashCode() ^ AwardId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}*{2}", AwardYearId, StudentId, AwardId);
        }
    }
}
