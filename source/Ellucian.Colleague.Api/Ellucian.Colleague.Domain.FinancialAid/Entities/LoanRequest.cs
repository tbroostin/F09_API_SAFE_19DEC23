/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public class LoanRequest
    {
        /// <summary>
        /// The unique identifier of this LoanRequest
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The Colleague PERSON id of the student who submitted this loanRequest
        /// </summary>
        public string StudentId { get { return studentId; } }
        private readonly string studentId;

        /// <summary>
        /// The AwardYear for which this LoanRequest applies
        /// </summary>
        public string AwardYear { get { return awardYear; } }
        private readonly string awardYear;

        /// <summary>
        /// The date the student submitted this LoanRequest
        /// </summary>
        public DateTime RequestDate { get { return requestDate; } }
        private readonly DateTime requestDate;

        /// <summary>
        /// The total loan amount the student requested
        /// </summary>
        public int TotalRequestAmount { get { return totalRequestAmount; } }
        private readonly int totalRequestAmount;

        /// <summary>
        /// The list of loan periods
        /// </summary>
        public ReadOnlyCollection<LoanRequestPeriod> LoanRequestPeriods { get; private set; }
        private readonly List<LoanRequestPeriod> _loanRequestPeriods;

        /// <summary>
        /// Any comments the student entered along with the request.
        /// </summary>
        public string StudentComments { get; set; }

        /// <summary>
        /// The Colleague PERSON Id of the loan counselor this request was assigned to
        /// </summary>
        public string AssignedToId { get; set; }

        /// <summary>
        /// The current status of the loan request
        /// </summary>
        public LoanRequestStatus Status { get { return status; } }
        private LoanRequestStatus status;

        /// <summary>
        /// Update the LoanRequestStatus and the Colleague PERSON id of the loan counselor who 
        /// has modified the status. The modifierId will only update if the new LoanRequestStatus is
        /// different than the current LoanRequestStatus. The status date is also updated at this time.
        /// </summary>
        /// <param name="newStatus">The new LoanRequestStatus</param>
        /// <param name="modifierId">The Colleague PERSON id of the loan counselor who has modified the status.</param>
        /// <returns>True, if the status, date and modifier were updated successfully. False, otherwise.</returns>
        public bool UpdateStatus(LoanRequestStatus newStatus, string modifierId)
        {
            if (string.IsNullOrEmpty(modifierId))
            {
                throw new ArgumentNullException("modifierId");
            }

            if (Status != newStatus)
            {
                status = newStatus;
                statusDate = DateTime.Today;
                this.modifierId = modifierId;
                return true;
            }

            return false;
        }

        /// <summary>
        /// The latest date that the current status was updated.
        /// </summary>
        public DateTime StatusDate { get { return statusDate; } }
        private DateTime statusDate;

        /// <summary>
        /// The Colleague PERSON id of the loan counselor who last modified this request. If null or empty, 
        /// no one has modified this request yet.
        /// </summary>
        public string ModifierId { get { return modifierId; } }
        private string modifierId;

        /// <summary>
        /// Any comments loan counselors have entered about this loan request
        /// </summary>
        public string ModifierComments { get; set; }

        /// <summary>
        /// Instantiate a new LoanRequest object.
        /// </summary>
        /// <param name="id">The unique identifier of this object. Required</param>
        /// <param name="studentId">The Colleague PERSON id of the student who submitted this loanRequest. Required</param>
        /// <param name="awardYear">The AwardYear for which this LoanRequest applies. Required</param>
        /// <param name="requestDate">The date the student submitted this loan request. Required</param>
        /// <param name="totalRequestAmount">The total loan amount the student request. Required. Must be greater than 0</param>
        /// <param name="assignedToId">The Colleague PERSON id of the loan counselor assigned to this request. Not Required</param>
        /// <param name="status">The current status of the loan request. Required</param>
        /// <param name="statusDate">The latest date that the current status was updated. Required</param>
        /// <param name="modifierId">The Colleague PERSON id of the loan counselor who last modified this request</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the required arguments are null or empty. For totalLoanAmount, exception is thrown if argument is less than or equal to zero.</exception>
        public LoanRequest(string id, string studentId, string awardYear, DateTime requestDate, int totalRequestAmount,
            string assignedToId, LoanRequestStatus status, DateTime statusDate, string modifierId)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentNullException("id"); }
            if (string.IsNullOrEmpty(studentId)) { throw new ArgumentNullException("studentId"); }
            if (string.IsNullOrEmpty(awardYear)) { throw new ArgumentNullException("awardYear"); }
            if (totalRequestAmount <= 0) { throw new ArgumentException("totalRequestAmount must be greater than zero", "totalRequestAmount"); }

            this.id = id;
            this.studentId = studentId;
            this.awardYear = awardYear;
            this.requestDate = requestDate;
            this.totalRequestAmount = totalRequestAmount;
            this.AssignedToId = assignedToId;
            this.status = status;
            this.statusDate = statusDate;
            this.modifierId = modifierId;

            _loanRequestPeriods = new List<LoanRequestPeriod>();
            this.LoanRequestPeriods = _loanRequestPeriods.AsReadOnly();
        }

        /// <summary>
        /// Two LoanRequest objects are equal when they have the same Id
        /// </summary>
        /// <param name="obj">LoanRequest object to compare to this LoanRequest</param>
        /// <returns>True if the two objects are equal. False, otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var loanRequest = obj as LoanRequest;

            if (loanRequest.Id == this.Id)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the HashCode of this object based on the Id
        /// </summary>
        /// <returns>The LoanRequest object's HashCode</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Method to add loan request periods to the collection
        /// </summary>
        /// <param name="code">loan request period code</param>
        /// <param name="loanAmount">loan request period amount</param>
        /// <returns>true/false to indicate if loanPeriod was added or not</returns>
        public bool AddLoanPeriod(string code, int loanAmount)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "Loan period code cannot be empty.");
            }
            if (loanAmount < 0)
            {
                throw new ArgumentException("loanAmount", "loan period amount must be greater or equal to 0.");
            }
            if (!_loanRequestPeriods.Select(lrp => lrp.Code).Contains(code))
            {
                _loanRequestPeriods.Add(new LoanRequestPeriod(code, loanAmount));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to remove a loan request period from the collection
        /// </summary>
        /// <param name="code">loan request period code</param>
        /// <returns>true/false to indicate if a loan request was removed or not</returns>
        public bool RemoveLoanPeriod(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "loanPeriodCode cannot be null or empty");
            }
            if (_loanRequestPeriods.Select(lrp => lrp.Code).Contains(code))
            {
                LoanRequestPeriod periodToRemove = _loanRequestPeriods.First(lrp => lrp.Code == code);
                return _loanRequestPeriods.Remove(periodToRemove);
            }
            return false;
        }
    }
}
