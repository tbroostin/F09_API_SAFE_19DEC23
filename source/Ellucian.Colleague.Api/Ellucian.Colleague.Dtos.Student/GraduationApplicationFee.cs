// Copyright 2015 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about any graduation application fee that is charged for submitting a graduation application.
    /// This information is based on the student and program for which they are applying for graduation.
    /// </summary>
    public class GraduationApplicationFee
    {
        /// <summary>
        /// Student applying for graduation
        /// </summary>
        public string StudentId { get; set; }
        
        /// <summary>
        /// Program code specific to the graduation application fee.
        /// </summary>
        public string ProgramCode { get; set; }
        
        /// <summary>
        /// Amount that is charged for the graduation application
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// The distribution code to be used when making a payment on this graduation application fee.
        /// </summary>
        public string PaymentDistributionCode { get; set; }
    }
}
