// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that regarding any required application fee necessary in applying for student requests(transcript and enrollment verification).
    /// It is generic class that works for both transcript requests and enrollment verification requests.
    /// </summary>
    [Serializable]
    public class StudentRequestFee
    {
        // Private members
        private readonly string _requestId;
        private readonly string _studentId;
        private readonly decimal? _amount;
        private readonly string _paymentDistributionCode;

        /// <summary>
        /// Student applying for student request
        /// </summary>
        public string StudentId { get { return _studentId; } }

        /// <summary>
        /// Request id specific to the Student Request log File.
        /// </summary>
        public string RequestId { get { return _requestId; } }

        /// <summary>
        /// Amount that is charged for the student request.
        /// </summary>
        public decimal? Amount { get { return _amount; } }

        /// <summary>
        /// The distribution code to be used when making a payment on this student application  fee.
        /// </summary>
        public string PaymentDistributionCode { get { return _paymentDistributionCode; } }

        /// <summary>
        /// Constructor for the Student Request  Fee object
        /// </summary>
        /// <param name="amount">Amount of the fee to be charged</param>
        /// <param name="paymentDistributionCode">Payment distribution to be used in paying this fee.</param>
        public StudentRequestFee(string studentId, string requestId, decimal? amount, string paymentDistributionCode)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentNullException("requestId", "Request Id is required for the student request fee.");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id is required for the student request fee.");
            }
            if (amount.HasValue && amount < 0)
            {
                throw new ArgumentException("amount", "student fee amount must be greater or equal to 0.");
            }
            _requestId = requestId;
            _studentId = studentId;
            _amount = amount;
            _paymentDistributionCode = paymentDistributionCode;
        }
    }
}
