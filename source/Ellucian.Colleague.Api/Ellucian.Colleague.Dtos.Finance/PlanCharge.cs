// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A charge assigned to a payment plan
    /// </summary>
    public class PlanCharge
    {
        /// <summary>
        /// ID of the payment plan charge
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the payment plan to which the plan charge belongs
        /// </summary>
        public string PlanId { get; set; }

        /// <summary>
        /// The ID of the charge component of the plan charge
        /// </summary>
        public Charge Charge { get; set; }

        /// <summary>
        /// Amount of the plan charge
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Determines whether the plan charge is a setup fee
        /// </summary>
        public bool IsSetupCharge { get; set; }

        /// <summary>
        /// Determines whether the plan charge is fully allocated to a payment plan
        /// </summary>
        public bool IsAutomaticallyModifiable { get; set; }
    }
}
