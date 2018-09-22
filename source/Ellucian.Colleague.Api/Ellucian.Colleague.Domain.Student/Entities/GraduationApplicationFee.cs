// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that regarding any required application fee necessary in applying for graduation.
    /// </summary>
    [Serializable]
    public class GraduationApplicationFee
    {
        // Private members
        private readonly string _programCode;
        private readonly string _studentId;
        private readonly decimal? _amount;
        private readonly string _paymentDistributionCode;

        /// <summary>
        /// Student applying for graduation
        /// </summary>
        public string StudentId { get { return _studentId; } }

        /// <summary>
        /// Program code specific to the graduation application fee.
        /// </summary>
        public string ProgramCode { get { return _programCode; } }

        /// <summary>
        /// Amount that is charged for the graduation application
        /// </summary>
        public decimal? Amount { get { return _amount; } }

        /// <summary>
        /// The distribution code to be used when making a payment on this graduation application fee.
        /// </summary>
        public string PaymentDistributionCode { get { return _paymentDistributionCode; } }

        /// <summary>
        /// Constructor for the Graduation Application Fee object
        /// </summary>
        /// <param name="amount">Amount of the fee to be charged</param>
        /// <param name="paymentDistributionCode">Payment distribution to be used in paying this fee.</param>
        public GraduationApplicationFee(string studentId, string programCode, decimal? amount, string paymentDistributionCode)
        {
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program code is required for the graduation application fee.");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id is required for the graduation application fee.");
            }
            if (amount.HasValue && amount < 0)
            {
                throw new ArgumentException("amount", "graduation fee amount must be greater or equal to 0.");
            }
            _programCode = programCode;
            _studentId = studentId;
            _amount = amount;
            _paymentDistributionCode = paymentDistributionCode;
        }
    }
}
