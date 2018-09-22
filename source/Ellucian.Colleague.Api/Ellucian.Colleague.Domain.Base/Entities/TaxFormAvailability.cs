// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Represents the availability of a tax form
    /// </summary>
    [Serializable]
    public class TaxFormAvailability
    {
        /// <summary>
        /// Backing variable for TaxYear
        /// </summary>
        private readonly string taxYear;

        /// <summary>
        /// Tax year
        /// </summary>
        public string TaxYear { get { return this.taxYear; } }

        /// <summary>
        /// Available date for tax forms that use dates to determine availablity.
        /// </summary>
        private readonly DateTime? availableDate;

        /// <summary>
        /// Available flag for tax forms that use Y/N flags to determine availability.
        /// </summary>
        private readonly bool available;

        /// <summary>
        /// Determine if the tax form is available.
        /// </summary>
        public bool Available
        {
            get
            {
                if (available || (availableDate.HasValue && DateTime.Compare(availableDate.Value, DateTime.Now) <= 0))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Initialize the availability object.
        /// </summary>
        /// <param name="taxYear">Tax Year</param>
        /// <param name="availableDate">Available Date</param>
        public TaxFormAvailability(string taxYear, DateTime? availableDate)
        {
            if (string.IsNullOrEmpty(taxYear))
                throw new ArgumentNullException("taxYear", "taxYear is required.");

            this.taxYear = taxYear;
            this.availableDate = availableDate;
            this.available = false;
        }

        public TaxFormAvailability(string taxYear, bool available)
        {
            if (string.IsNullOrEmpty(taxYear))
                throw new ArgumentNullException("taxYear", "taxYear is required.");

            this.taxYear = taxYear;
            this.availableDate = null;
            this.available = available;
        }
    }
}
