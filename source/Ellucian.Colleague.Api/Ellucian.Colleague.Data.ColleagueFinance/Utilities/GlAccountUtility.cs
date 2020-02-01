// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <param name="majorComponentStartPosition">List of the major component start positions.</param>
        /// <exception cref="ApplicationException">Application exception</exception>
        public static GlClass GetGlAccountGlClass(string glAccount, GeneralLedgerClassConfiguration glClassConfiguration, IList<string> majorComponentStartPosition)
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

            if (majorComponentStartPosition == null)
            {
                throw new ArgumentNullException("majorComponentStartPosition", "The GL account major component start positions are required.");
            }
            #endregion

            var internalGlNumber = ConvertGlAccountToInternalFormat(glAccount, majorComponentStartPosition);

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
        /// <param name="glAccount">GL number</param>
        /// <param name="costCenterStructure">Cost center definition used to caluclate the cost center ID from the GL number.</param>
        /// <param name="majorComponentStartPosition">List of the major component start positions.</param>
        /// <returns>ID of the cost center to which the GL number belongs.</returns>
        public static string GetCostCenterId(string glAccount, CostCenterStructure costCenterStructure, IList<string> majorComponentStartPosition)
        {
            if (majorComponentStartPosition == null)
            {
                throw new ArgumentNullException("majorComponentStartPosition", "The GL account major component start positions are required.");
            }

            // Determine which component description objects will be needed to calculate the cost center Id and Name.
            string costCenterId = string.Empty;

            if (!string.IsNullOrEmpty(glAccount) && costCenterStructure != null)
            {
                var internalGlNumber = ConvertGlAccountToInternalFormat(glAccount, majorComponentStartPosition);
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
        /// <param name "glAccounts">List of GL accounts.</param>
        /// <param name="exclusions">Exclusions data.</param>
        /// <param name ="majorComponentStartPosition" > List of the major component start positions.</param>
        /// <returns>List of error messages.</returns>
        public static List<string> EvaluateExclusionsForBudgetAdjustment(IEnumerable<string> glAccounts, BudgetAdjustmentAccountExclusions exclusions, IList<string> majorComponentStartPosition)
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
                        var internalGlNumber = ConvertGlAccountToInternalFormat(glAccount, majorComponentStartPosition);

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

        /// <summary>
        /// Convert a GL account number to its internal format.
        /// </summary>
        /// <param name="glAccount">A GL account in internal or external format.</param>
        /// <param name="majorComponentStartPosition">List of the major component start positions.</param>
        /// <returns>A GL account in internal format.</returns>
        public static string ConvertGlAccountToInternalFormat(string glAccount, IList<string> majorComponentStartPosition)
        {
            if (string.IsNullOrEmpty(glAccount))
            {
                throw new ArgumentNullException("glAccount", "GL account is required.");
            }

            if (majorComponentStartPosition == null)
            {
                throw new ArgumentNullException("majorComponentStartPosition", "The GL account major component start positions are required.");
            }

            // The GL account may contain delimiters in the form of dashes or underscores, which are removed.
            // (It keeps previous support for the underscore as a delimiter.) Those are the only delimiters
            // that are allowed.
            // If the GL account is 15 characters or less, it is not stored with underscores in the database.
            // If the general ledger account is longer than 15 characters, it is stored with underscores
            // between the major components. Use the delimiter positions from the general ledger account 
            // parameters record to insert the underscores in the correct positions.

            if (glAccount.Replace("_", "").Replace("-", "").Length > 15)
            {
                string internalGlAccount = string.Empty;
                // Remove any delimiters if any exist.
                var tempInternalGlAccount = new StringBuilder(glAccount).Replace("_", "").Replace("-", "");

                List<string> startPositions = new List<string>();
                startPositions = majorComponentStartPosition.ToList();
                // The first start position is always 1 and we don't need to use it.
                startPositions.RemoveAt(0);
                List<int> positions = startPositions.ConvertAll(s => Int32.Parse(s));

                foreach (int pos in positions)
                {
                    // Only insert the underscore if the position is a positive number.
                    if (pos > 1)
                    {
                        // Because C# indexes begin at zero we need to subtract 2 from the position to insert the underscore in the correct position.
                        tempInternalGlAccount.Insert(pos - 2, '_');
                    }
                }
                return internalGlAccount = tempInternalGlAccount.ToString();
            }
            else
            {
                return glAccount.Replace("_", "").Replace("-", "");
            }
        }


        /// <summary>
        /// Convert a GL account number to its external format.
        /// </summary>
        /// <param name="glAccount">A GL account in internal format.</param>
        /// <param name="majorComponentStartPosition">List of the major component start positions.</param>
        /// <returns>A GL account in external format.</returns>
        public static string ConvertGlAccountToExternalFormat(string glAccount, IList<string> majorComponentStartPosition)
        {
            if (string.IsNullOrEmpty(glAccount))
            {
                throw new ArgumentNullException("glAccount", "GL account is required.");
            }

            if (majorComponentStartPosition == null)
            {
                throw new ArgumentNullException("majorComponentStartPosition", "The GL account major component start positions are required.");
            }

            // If the general ledger account is longer than 15 characters, it is stored with underscores
            // between the major components and we can convert these to hyphens.
            // If the GL account is 15 characters or less, it is not stored with underscores in the 
            // database. Obtain the delimiter positions from the general ledger account parameters
            // record and use them to insert the hyphens in the correct positions.
            string formattedGlAccount = string.Empty;
            if (glAccount.Replace("_", "").Length > 15)
            {
                formattedGlAccount = glAccount.Replace("_", "-");
            }
            else
            {
                List<string> reversedStartPosition = new List<string>();
                reversedStartPosition = majorComponentStartPosition.ToList();

                // The first start position is always 1 and we don't need to use it.
                reversedStartPosition.RemoveAt(0);
                reversedStartPosition.Reverse();
                List<int> positions = reversedStartPosition.ConvertAll(s => Int32.Parse(s));
                var tempFormattedGlAccount = new StringBuilder(glAccount);
                foreach (int pos in positions)
                {
                    // Only insert the hyphen if the position is a positive number
                    if (pos > 1)
                    {
                        // Because C# indexes begin at zero we subtract 1 from the position to insert the delimiter in the correct position.
                        tempFormattedGlAccount.Insert(pos - 1, '-');
                    }
                }
                formattedGlAccount = tempFormattedGlAccount.ToString();

            }
            return formattedGlAccount;
        }
    }
}
