// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Method by which payment plan is modified when its amount changes: 
    /// - EntirePlan recalculates the entire payment plan using the new amount of the payment plan.
    /// - RemainingPayments distributes the change in payment plan amount equally over all scheduled payments that have not yet been fully paid.
    /// - DecreaseLastPayment applies net decreases to the last scheduled payment on the plan, and net increases are distributed equally over all scheduled payments
    /// </summary>
    [Serializable]
    public enum PaymentPlanModificationMethod
    {
        /// <summary>
        /// Entire Plan - recalculates the entire payment plan using the new amount of the payment plan.
        /// </summary>
        EntirePlan,

        /// <summary>
        /// Remaining Payments - distributes payment plan amount change equally over all scheduled payments that have not yet been fully paid
        /// </summary>
        RemainingPayments,

        /// <summary>
        /// Decrease Last Payment - applies net plan amount decrease to the last scheduled payment, or applies net plan amount increase equally across all scheduled payments that have not yet been fully paid
        /// </summary>
        DecreaseLastPayment
    }
}
