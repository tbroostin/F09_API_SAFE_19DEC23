// Copyright 2015 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about any student request fee that is charged for submitting a student request.
    /// student request can be either transcript requests or enrollment verificaion requests.
    /// </summary>
    public class StudentRequestFee
    {
        /// <summary>
        /// Student applying for student requests
        /// </summary>
        public string StudentId { get; set; }
        
        /// <summary>
        ///Request Id specific to student request logs file.
        /// </summary>
        public string RequestId { get; set; }
        
        /// <summary>
        /// Amount that is charged for the student request fee.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// The distribution code to be used when making a payment on this student request fee.
        /// </summary>
        public string PaymentDistributionCode { get; set; }
    }
}
