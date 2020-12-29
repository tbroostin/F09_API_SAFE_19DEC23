// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using System.Linq;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using System;

namespace Ellucian.Colleague.Data.ColleagueFinance.Utilities
{
    /// <summary>
    /// Provides various methods to perform operations on filter criteria.
    /// </summary>
    public class FilterUtility
    {
        /// <summary>
        /// Determines if the supplied filter is "wide open" (i.e. has criteria).
        /// </summary>
        /// <param name="criteria">Cost center query criteria</param>
        /// <returns>Boolean indicating whether or not the filter is "wide open".</returns>
        public static bool IsFilterWideOpen(BaseGlComponentQueryCriteria criteria)
        {
            // Are we limiting the results using individual components?
            if (criteria.ComponentCriteria.Where(x => x.IndividualComponentValues != null && x.IndividualComponentValues.Any()).Any())
            {
                return false;
            }

            // Are we limiting the results using range components?
            if (criteria.ComponentCriteria.Where(x => x.RangeComponentValues != null && x.RangeComponentValues.Any()).Any())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Apply any financial health indicator filter selection to the GL account being processed.
        /// </summary>
        /// <param name="expenseGlAccountDataContract">Data contract for the GL account</param>
        /// <param name="type">string containing "U" for umbrella and empty for a non-pooled GL account</param>
        /// <param name="fiscalYear">GL fiscal year</param>
        /// <param name="costCenterCriteria">Cost center filter criteria</param>
        /// <returns>Bolean indicating whether to include this expense GL account in the processing</returns>
        public static bool CheckFinancialHealthIndicatorFilterForGlAcct(GlAccts expenseGlAccountDataContract, string type, string fiscalYear, CostCenterQueryCriteria costCenterCriteria)
        {
            // Only one or more financial health indicator values have been selected. Use the filter criteria.

            // Obtain the amounts to calculate the financial health indicator values from the
            // from the corresponding fields for an umbrella vs a non-pooled GL account.
            var expenseGlAccountAmounts = expenseGlAccountDataContract.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);
            if (expenseGlAccountAmounts != null)
            {
                decimal budgetAmount = 0;
                decimal encumbranceAmount = 0;
                decimal actualAmount = 0;
                if (type == "U")
                {
                    budgetAmount = expenseGlAccountAmounts.FaBudgetPostedAssocMember.HasValue ? expenseGlAccountAmounts.FaBudgetPostedAssocMember.Value : 0m;
                    budgetAmount += expenseGlAccountAmounts.FaBudgetMemoAssocMember.HasValue ? expenseGlAccountAmounts.FaBudgetMemoAssocMember.Value : 0m;
                    encumbranceAmount = expenseGlAccountAmounts.FaEncumbrancePostedAssocMember.HasValue ? expenseGlAccountAmounts.FaEncumbrancePostedAssocMember.Value : 0m;
                    encumbranceAmount += expenseGlAccountAmounts.FaEncumbranceMemoAssocMember.HasValue ? expenseGlAccountAmounts.FaEncumbranceMemoAssocMember.Value : 0m;
                    encumbranceAmount += expenseGlAccountAmounts.FaRequisitionMemoAssocMember.HasValue ? expenseGlAccountAmounts.FaRequisitionMemoAssocMember.Value : 0m;
                    actualAmount = expenseGlAccountAmounts.FaActualPostedAssocMember.HasValue ? expenseGlAccountAmounts.FaActualPostedAssocMember.Value : 0m;
                    actualAmount += expenseGlAccountAmounts.FaActualMemoAssocMember.HasValue ? expenseGlAccountAmounts.FaActualMemoAssocMember.Value : 0m;

                }
                else
                {
                    budgetAmount = expenseGlAccountAmounts.GlBudgetPostedAssocMember.HasValue ? expenseGlAccountAmounts.GlBudgetPostedAssocMember.Value : 0m;
                    budgetAmount += expenseGlAccountAmounts.GlBudgetMemosAssocMember.HasValue ? expenseGlAccountAmounts.GlBudgetMemosAssocMember.Value : 0m;
                    encumbranceAmount = expenseGlAccountAmounts.GlEncumbrancePostedAssocMember.HasValue ? expenseGlAccountAmounts.GlEncumbrancePostedAssocMember.Value : 0m;
                    encumbranceAmount += expenseGlAccountAmounts.GlEncumbranceMemosAssocMember.HasValue ? expenseGlAccountAmounts.GlEncumbranceMemosAssocMember.Value : 0m;
                    encumbranceAmount += expenseGlAccountAmounts.GlRequisitionMemosAssocMember.HasValue ? expenseGlAccountAmounts.GlRequisitionMemosAssocMember.Value : 0m;
                    actualAmount = expenseGlAccountAmounts.GlActualPostedAssocMember.HasValue ? expenseGlAccountAmounts.GlActualPostedAssocMember.Value : 0m;
                    actualAmount += expenseGlAccountAmounts.GlActualMemosAssocMember.HasValue ? expenseGlAccountAmounts.GlActualMemosAssocMember.Value : 0m;
                }

                bool includeGlAccount = false;
                // Loop through each value in the filter to see if this GL account meets any of the criteria.
                foreach (var value in costCenterCriteria.FinancialThresholds)
                {
                    if (includeGlAccount == false)
                    {
                        switch (value)
                        {
                            case FinancialThreshold.OverThreshold:
                                if ((actualAmount + encumbranceAmount) > budgetAmount)
                                {
                                    includeGlAccount = true;
                                }
                                break;
                            case FinancialThreshold.NearThreshold:
                                if (((actualAmount + encumbranceAmount) > budgetAmount * 0.85m) && ((actualAmount + encumbranceAmount) <= budgetAmount))
                                {
                                    includeGlAccount = true;
                                }
                                break;
                            case FinancialThreshold.UnderThreshold:
                                if ((actualAmount + encumbranceAmount) <= budgetAmount * 0.85m)
                                {
                                    includeGlAccount = true;
                                }
                                break;
                        }
                    }
                }

                return includeGlAccount;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Apply any financial health indicator filter selection to the GL account being processed.
        /// </summary>
        /// <param name="expenseGlsFyrDataContract">GLS.FYR data contract for the GL account</param>
        /// <param name="type">string containing "U" for umbrella and empty for a non-pooled GL account.</param>
        /// <param name="costCenterCriteria">Cost center filter criteria</param>
        /// <returns>Bolean indicating whether to include this expense GL account in the processing</returns>
        public static bool CheckFinancialHealthIndicatorFilterForGlsFyr(GlsFyr expenseGlAccountGlsRecord, string type, CostCenterQueryCriteria costCenterCriteria)
        {
            // Only one or more financial health indicator values have been selected. Use the filter criteria.

            // Obtain the amounts to calculate the financial health indicator values from the
            // from the corresponding fields for an umbrella vs a non-pooled GL account.
            if (expenseGlAccountGlsRecord != null)
            {
                decimal budgetAmount = 0;
                decimal encumbranceAmount = 0;
                decimal actualAmount = 0;
                if (type == "U")
                {
                    budgetAmount = expenseGlAccountGlsRecord.BAlocDebitsYtd.HasValue ? expenseGlAccountGlsRecord.BAlocDebitsYtd.Value : 0m;
                    budgetAmount += expenseGlAccountGlsRecord.BAlocCreditsYtd.HasValue ? expenseGlAccountGlsRecord.BAlocCreditsYtd.Value : 0m;

                    foreach (var amount in expenseGlAccountGlsRecord.GlsFaMactuals)
                    {
                        actualAmount += amount.HasValue ? amount.Value : 0m;
                    }

                    foreach (var amount in expenseGlAccountGlsRecord.GlsFaMencumbrances)
                    {
                        encumbranceAmount += amount.HasValue ? amount.Value : 0m;
                    }
                }
                else
                {

                    budgetAmount = expenseGlAccountGlsRecord.BAlocDebitsYtd.HasValue ? expenseGlAccountGlsRecord.BAlocDebitsYtd.Value : 0m;
                    budgetAmount -= expenseGlAccountGlsRecord.BAlocCreditsYtd.HasValue ? expenseGlAccountGlsRecord.BAlocCreditsYtd.Value : 0m;
                    encumbranceAmount = expenseGlAccountGlsRecord.EOpenBal.HasValue ? expenseGlAccountGlsRecord.EOpenBal.Value : 0m;
                    encumbranceAmount += expenseGlAccountGlsRecord.EncumbrancesYtd.HasValue ? expenseGlAccountGlsRecord.EncumbrancesYtd.Value : 0m;
                    encumbranceAmount -= expenseGlAccountGlsRecord.EncumbrancesRelievedYtd.HasValue ? expenseGlAccountGlsRecord.EncumbrancesRelievedYtd.Value : 0m;
                    actualAmount = expenseGlAccountGlsRecord.DebitsYtd.HasValue ? expenseGlAccountGlsRecord.DebitsYtd.Value : 0m;
                    actualAmount -= expenseGlAccountGlsRecord.CreditsYtd.HasValue ? expenseGlAccountGlsRecord.CreditsYtd.Value : 0m;
                }

                bool includeGlAccount = false;
                // Loop through each value in the filter to see if this GL account meets any of the criteria.
                foreach (var value in costCenterCriteria.FinancialThresholds)
                {
                    if (includeGlAccount == false)
                    {
                        switch (value)
                        {
                            case FinancialThreshold.OverThreshold:
                                if ((actualAmount + encumbranceAmount) > budgetAmount)
                                {
                                    includeGlAccount = true;
                                }
                                break;
                            case FinancialThreshold.NearThreshold:
                                if (((actualAmount + encumbranceAmount) > budgetAmount * 0.85m) && ((actualAmount + encumbranceAmount) <= budgetAmount))
                                {
                                    includeGlAccount = true;
                                }
                                break;
                            case FinancialThreshold.UnderThreshold:
                                if ((actualAmount + encumbranceAmount) <= budgetAmount * 0.85m)
                                {
                                    includeGlAccount = true;
                                }
                                break;
                        }
                    }
                }

                return includeGlAccount;
            }
            else
            {
                return false;
            }
        }
    }
}