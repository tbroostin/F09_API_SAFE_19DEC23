// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents the class configuration for GL.
    /// </summary>
    [Serializable]
    public class GeneralLedgerClassConfiguration
    {
        /// <summary>
        /// The name of the subcomponent which represents the GL Class.
        /// </summary>
        public string ClassificationName { get { return classificationName; } }
        private readonly string classificationName;

        /// <summary>
        /// The GL Class subcomponent start position.
        /// </summary>
        public int GlClassStartPosition { get; set; }

        /// <summary>
        /// The GL Class subcomponent length.
        /// </summary>
        public int GlClassLength { get; set; }

        /// <summary>
        /// List of GL asset values
        /// </summary>
        public ReadOnlyCollection<string> AssetClassValues { get; private set; }
        private readonly List<string> assetClassValues = new List<string>();

        /// <summary>
        /// List of GL liability values
        /// </summary>
        public ReadOnlyCollection<string> LiabilityClassValues { get; private set; }
        private readonly List<string> liabilityClassValues = new List<string>();

        /// <summary>
        /// List of GL fund balance values
        /// </summary>
        public ReadOnlyCollection<string> FundBalanceClassValues { get; private set; }
        private readonly List<string> fundBalanceClassValues = new List<string>();

        /// <summary>
        /// List of GL expense values.
        /// </summary>
        public ReadOnlyCollection<string> ExpenseClassValues { get; private set; }
        private readonly List<string> expenseClassValues = new List<string>();

        /// <summary>
        /// List of GL revenue values.
        /// </summary>
        public ReadOnlyCollection<string> RevenueClassValues { get; private set; }
        private readonly List<string> revenueClassValues = new List<string>();

        /// <summary>
        /// Initialize the GL Class configuration.
        /// </summary>
        /// <param name="className">Classification name</param>
        /// <param name="expenseValues">GL expense class values</param>
        /// <param name="revenueValues">GL revenue class values</param>
        /// <param name="assetValues">GL asset class values</param>
        /// <param name="liabilityValues">GL liability class values</param>
        /// <param name="fundBalanceValues">GL fund balance class values</param>
        public GeneralLedgerClassConfiguration(string className, IEnumerable<string> expenseValues,
            IEnumerable<string> revenueValues, IEnumerable<string> assetValues, IEnumerable<string> liabilityValues,
            IEnumerable<string> fundBalanceValues)
        {
            if (string.IsNullOrEmpty(className))
                throw new ArgumentNullException("className", "className must have a value");

            this.classificationName = className;
            ExpenseClassValues = expenseClassValues.AsReadOnly();
            RevenueClassValues = revenueClassValues.AsReadOnly();
            AssetClassValues = assetClassValues.AsReadOnly();
            LiabilityClassValues = liabilityClassValues.AsReadOnly();
            FundBalanceClassValues = fundBalanceClassValues.AsReadOnly();

            if (expenseValues != null)
            {
                foreach (var value in expenseValues)
                {
                    if (!string.IsNullOrEmpty(value))
                        this.expenseClassValues.Add(value);
                }
            }

            if (revenueValues != null)
            {
                foreach (var value in revenueValues)
                {
                    if (!string.IsNullOrEmpty(value))
                        this.revenueClassValues.Add(value);
                }
            }

            if (assetValues != null)
            {
                foreach (var value in assetValues)
                {
                    if (!string.IsNullOrEmpty(value))
                        this.assetClassValues.Add(value);
                }
            }

            if (liabilityValues != null)
            {
                foreach (var value in liabilityValues)
                {
                    if (!string.IsNullOrEmpty(value))
                        this.liabilityClassValues.Add(value);
                }
            }

            if (fundBalanceValues != null)
            {
                foreach (var value in fundBalanceValues)
                {
                    if (!string.IsNullOrEmpty(value))
                        this.fundBalanceClassValues.Add(value);
                }
            }
        }
    }
}
 