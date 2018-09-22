// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    ///  Used to pass query criteria to retrieve account holder payment plan data
    /// </summary>
    public class PaymentPlanQueryCriteria
    {
        /// <summary>
        /// Collection of payment items
        /// </summary>
        public IEnumerable<BillingTermPaymentPlanInformation> BillingTerms { get; set; }
    }
}
