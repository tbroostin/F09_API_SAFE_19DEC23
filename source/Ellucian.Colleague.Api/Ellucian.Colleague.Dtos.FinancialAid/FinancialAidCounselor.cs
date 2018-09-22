//Copyright 2014 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// FinancialAidCounselor Dto provides general contact information for the counselor resource.
    /// </summary>
    public class FinancialAidCounselor
    {
        /// <summary>
        /// Counselor's Colleague PERSON id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The counselor's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The counselor's email address
        /// </summary>
        public string EmailAddress { get; set; }

    }
}
