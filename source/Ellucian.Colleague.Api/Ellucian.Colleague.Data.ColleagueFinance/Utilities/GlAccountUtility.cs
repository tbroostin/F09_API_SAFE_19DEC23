// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Data.ColleagueFinance.Utilities
{
    /// <summary>
    /// Commonly-used functions for GL accounts.
    /// </summary>
    public class GlAccountUtility
    {
        /// <summary>
        /// Obtain the GL Class for a GL account.
        /// </summary>
        /// <param name="glAccount">A GL account number.</param>
        /// <param name="glClassConfiguration">General Ledger Class configuration.</param>
        /// <exception cref="ApplicationException">Application exception</exception>
        public static GlClass GetGlAccountGlClass(string glAccount, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            #region null checks
            if (string.IsNullOrEmpty(glAccount))
            {
                throw new ArgumentNullException("glAccount", "GL account is required.");
            }

            if (glClassConfiguration == null)
            {
                throw new ArgumentNullException("glClassConfiguration", "GL class configuration is required.");
            }
            #endregion

            var internalGlNumber = ConvertToInternalFormat(glAccount);

            GlClass glClass = new GlClass();
            string glAccountGlClass = internalGlNumber.Substring(glClassConfiguration.GlClassStartPosition, glClassConfiguration.GlClassLength);

            if (string.IsNullOrEmpty(glAccountGlClass))
            {
                throw new ApplicationException("Missing glClass for GL account: " + internalGlNumber);
            }

            if (glClassConfiguration.ExpenseClassValues.Contains(glAccountGlClass))
            {
                glClass = GlClass.Expense;
            }
            else if (glClassConfiguration.RevenueClassValues.Contains(glAccountGlClass))
            {
                glClass = GlClass.Revenue;
            }
            else if (glClassConfiguration.AssetClassValues.Contains(glAccountGlClass))
            {
                glClass = GlClass.Asset;
            }
            else if (glClassConfiguration.LiabilityClassValues.Contains(glAccountGlClass))
            {
                glClass = GlClass.Liability;
            }
            else if (glClassConfiguration.FundBalanceClassValues.Contains(glAccountGlClass))
            {
                glClass = GlClass.FundBalance;
            }
            else
            {
                throw new ApplicationException("Invalid glClass for GL account: " + internalGlNumber);
            }

            return glClass;
        }

        /// <summary>
        /// Calculate the cost center ID of a GL number
        /// </summary>
        /// <param name="glAccountNumber">GL number</param>
        /// <param name="costCenterStructure">Cost center definition used to caluclate the cost center ID from the GL number.</param>
        /// <returns>ID of the cost center to which the GL number belongs.</returns>
        public static string GetCostCenterId(string glAccountNumber, CostCenterStructure costCenterStructure)
        {
            // Determine which component description objects will be needed to calculate the cost center Id and Name.
            string costCenterId = string.Empty;

            if (!string.IsNullOrEmpty(glAccountNumber) && costCenterStructure != null)
            {
                var internalGlNumber = ConvertToInternalFormat(glAccountNumber);
                foreach (var component in costCenterStructure.CostCenterComponents)
                {
                    if (component != null)
                    {
                        costCenterId += internalGlNumber.Substring(component.StartPosition, component.ComponentLength);
                    }
                }
            }

            return costCenterId;
        }

        /// <summary>
        /// Determine if any of the supplied GL accounts violate the budget adjustment exclusion restrictions.
        /// </summary>
        /// <param name="glAccounts">List of GL accounts.</param>
        /// <param name="exclusions">Exclusions data.</param>
        /// <returns>List of error messages.</returns>
        public static List<string> EvaluateExclusionsForBudgetAdjustment(IEnumerable<string> glAccounts, BudgetAdjustmentAccountExclusions exclusions)
        {
            var messages = new List<string>();
            // Check that there are exclusion elements to evaluate against the GL accounts.
            if (glAccounts != null && exclusions != null && exclusions.ExcludedElements != null && exclusions.ExcludedElements.Any())
            {
                // Loop through the GL accounts passed in.
                foreach (var glAccount in glAccounts)
                {
                    if (!string.IsNullOrEmpty(glAccount))
                    {
                        var internalGlNumber = ConvertToInternalFormat(glAccount);

                        foreach (var excludedElement in exclusions.ExcludedElements)
                        {
                            if (excludedElement != null)
                            {
                                if (excludedElement.ExclusionComponent != null && excludedElement.ExclusionRange != null &&
                                        excludedElement.ExclusionRange.StartValue != null && excludedElement.ExclusionRange.EndValue != null)
                                {
                                    string glAccountValueToCompare = string.Empty;

                                    // The user can select to use the full GL account number as part of the criteria.
                                    if (excludedElement.ExclusionComponent.ComponentName.ToUpperInvariant() == "FULLACCOUNT")
                                    {
                                        // Remove the underscores from the GL account internal format if there are any.
                                        glAccountValueToCompare = internalGlNumber.Replace("_", "");
                                    }
                                    else
                                    {
                                        // Get the (sub)component piece as specified in the exclusions data.
                                        try
                                        {
                                            glAccountValueToCompare = internalGlNumber.Substring(Convert.ToInt32(excludedElement.ExclusionComponent.StartPosition), Convert.ToInt32(excludedElement.ExclusionComponent.ComponentLength));
                                        }
                                        catch (ArgumentOutOfRangeException)
                                        {
                                            messages.Add(String.Format("{0}: is not a valid GL account.", internalGlNumber));
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(glAccountValueToCompare))
                                    {
                                        // GL accounts can contain alphanumeric characters.
                                        glAccountValueToCompare = glAccountValueToCompare.ToUpperInvariant();

                                        // Figure out if the GL piece is supposed to be excluded.
                                        // The GL number is excluded if it falls within the range of exclusion values.
                                        if (String.Compare(glAccountValueToCompare, excludedElement.ExclusionRange.StartValue) >= 0 && String.Compare(glAccountValueToCompare, excludedElement.ExclusionRange.EndValue) <= 0)
                                        {
                                            messages.Add(String.Format("{0}:{1}: may not be used in budget adjustments.", excludedElement.ExclusionComponent.ComponentName, glAccountValueToCompare));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Exlusion criteria may cause the same piece to be flagged multiple times so we need to remove any duplicates.
            return messages.Distinct().ToList();
        }

        public static string ConvertToInternalFormat(string glAccount)
        {
            if (string.IsNullOrEmpty(glAccount))
            {
                throw new ArgumentNullException("glAccount", "GL account is required.");
            }

            if (glAccount.Replace("-", "").Length > 15)
            {
                return glAccount.Replace("-", "_");
            }
            else
            {
                return glAccount.Replace("-", "");
            }
        }
    }
}
