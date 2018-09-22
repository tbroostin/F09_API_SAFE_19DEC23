// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// An AR Code
    /// </summary>
    public class ChargeCode
    {
        /// <summary>
        /// The nominal ID of the AR Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of the AR Code
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Payment priority for the AR Code - an AR Code with lower priority will be paid before an AR Code with higher priority
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// The Charge Group in which the AR Code will appear in Colleague Student Self-Service
        /// </summary>
        public int ChargeGroup { get; set; }
    }
}
