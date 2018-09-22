// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities;
using System;

namespace Ellucian.Colleague.Domain.Finance.Services
{
    public static class DueDateOverrideProcessor
    {
        /// <summary>
        /// Apply due date overrides to an AccountDue
        /// </summary>
        /// <param name="accountDue">AccountDue</param>
        public static void OverrideTermDueDates(DueDateOverrides dueDateOverrides, Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDue accountDue)
        {
            // If there are no term/non-term due date overrides, then do nothing
            if (dueDateOverrides == null || (dueDateOverrides.TermOverrides == null && dueDateOverrides.NonTermOverride == null))
            {
                return;
            }

            // If there is no account due or account due terms, then do nothing
            if (accountDue == null || (accountDue.AccountTerms == null))
            {
                return;
            }

            // Loop through due items and evaluate whether to override the due date
            foreach (var term in accountDue.AccountTerms)
            {
                if (term.AccountDetails != null)
                {
                    foreach (var dueItem in term.AccountDetails)
                    {
                        if (dueItem is Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)
                        {
                            //Do nothing, we will not override payment plan due dates. 
                        }
                        else if (dueItem is Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem)
                        {
                            if (dueDateOverrides.NonTermOverride != null && dueItem.AmountDue > 0)
                            {
                                dueItem.DueDate = dueDateOverrides.NonTermOverride;
                                //Now check to see if overdue.
                                dueItem.Overdue = (dueItem.DueDate.Value.Date < DateTime.Today);
                            }
                        }
                        else
                        {
                            DateTime termOverrideDate;
                            if (dueDateOverrides.TermOverrides.TryGetValue(dueItem.Term, out termOverrideDate) && dueItem.AmountDue > 0)
                            {
                                dueItem.DueDate = termOverrideDate;
                                //Now check to see if overdue.
                                dueItem.Overdue = (dueItem.DueDate.Value.Date < DateTime.Today);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Apply due date overrides to an AccountDuePeriod
        /// </summary>
        /// <param name="accountDue">AccountDuePeriod</param>
        public static void OverridePeriodDueDates(DueDateOverrides dueDateOverrides, Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDuePeriod accountDuePeriod)
        {
            // If there are no term/non-term due date overrides, then do nothing
            if (dueDateOverrides == null)
            {
                return;
            }

            if (dueDateOverrides.PastPeriodOverride != null)
            {
                OverridePeriodDueDate(accountDuePeriod.Past, dueDateOverrides.PastPeriodOverride);
            }
            if (dueDateOverrides.CurrentPeriodOverride != null)
            {
                OverridePeriodDueDate(accountDuePeriod.Current, dueDateOverrides.CurrentPeriodOverride);
            }
            if (dueDateOverrides.FuturePeriodOverride != null)
            {
                OverridePeriodDueDate(accountDuePeriod.Future, dueDateOverrides.FuturePeriodOverride);
            }
        }

        /// <summary>
        /// Apply due date overrides to an AccountDue based on a PCF override date
        /// </summary>
        /// <param name="accountDue">AccountDue</param>
        private static void OverridePeriodDueDate(Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDue accountDue, DateTime? overrideDate)
        {
            //If there is no due date override, no account due or no account due terms, then do nothing
            if (overrideDate == null || accountDue == null || accountDue.AccountTerms == null)
            {
                return;
            }

            // Loop through due items and evaluate whether to override the due date
            foreach (var term in accountDue.AccountTerms)
            {
                if (term.AccountDetails != null)
                {
                    foreach (var dueItem in term.AccountDetails)
                    {
                        if (dueItem is Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)
                        {
                            //Due nothing, we will not override payment plan due dates. 
                        }
                        else
                        {
                            if (dueItem.AmountDue > 0)
                            {
                                dueItem.DueDate = overrideDate;
                                //Now check to see if overdue.
                                dueItem.Overdue = (dueItem.DueDate.Value.Date < DateTime.Today);
                            }
                        }
                    }
                }
            }
        }
    }
}
