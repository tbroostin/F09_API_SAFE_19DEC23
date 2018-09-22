// Copyright 2017 Ellucian Company L.P. and its affiliates

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Financial charge associated with a course section
    /// </summary>
    public class SectionCharge
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Financial charge code
        /// </summary>
        public string ChargeCode { get; set; }

        /// <summary>
        /// Base charge amount
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// Flag indicating whether or not this charge is a flat fee
        /// </summary>
        public bool IsFlatFee { get; set; }

        /// <summary>
        /// Flag indicating whether or not this charge is applicable based on a rule
        /// </summary>
        public bool IsRuleBased { get; set; }
    }
}
