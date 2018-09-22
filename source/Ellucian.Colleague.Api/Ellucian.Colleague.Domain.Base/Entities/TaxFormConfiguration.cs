// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// This is the tax form configuration entity that contains
    /// information about tax forms parameters.
    /// </summary>
    [Serializable]
    public class TaxFormConfiguration
    {
        /// <summary>
        /// The tax form for which we are getting the consent paragraphs
        /// W-2, 1095-C, 1098-T, etc.
        /// </summary>
        private readonly TaxForms taxFormId;

        /// <summary>
        /// Public getter for private tax form id
        /// </summary>
        public TaxForms TaxFormId { get { return this.taxFormId; } }

        /// <summary>
        /// The tax form consent given and consent withheld paragraphs.
        /// </summary>
        public TaxFormConsentParagraph ConsentParagraphs { get; set; }

        /// <summary>
        /// Set of availability dates for the tax form tax years.
        /// </summary>
        private readonly List<TaxFormAvailability> availabilities = new List<TaxFormAvailability>();

        /// <summary>
        /// Public getter for private availability
        /// </summary>
        public ReadOnlyCollection<TaxFormAvailability> Availabilities { get; private set; }

        /// <summary>
        /// Constructor that initializes the Tax Form Configuration object.
        /// </summary>
        /// <param name="taxFormId">The tax form</param>
        public TaxFormConfiguration(TaxForms taxFormId)
        {
            this.taxFormId = taxFormId;
            ConsentParagraphs = new TaxFormConsentParagraph();
            Availabilities = availabilities.AsReadOnly();
        }

        /// <summary>
        /// Add an availability object to the list 
        /// </summary>
        /// <param name="availability">availability (year and date)</param>
        public void AddAvailability(TaxFormAvailability availability)
        {
            if (availability == null)
            {
                throw new ArgumentNullException("availability", "Availability object cannot be null");
            }

            bool isInList = false;

            if (availabilities != null)
            {
                foreach (var yearDate in availabilities)
                {
                    if (yearDate.TaxYear == availability.TaxYear)
                    {
                        isInList = true;
                    }

                }

                if (!isInList)
                {
                    availabilities.Add(availability);
                }
            }
        }

        /// <summary>
        /// Remove the availability that matches the tax year
        /// </summary>
        /// <param name="taxYear">Tax year</param>
        public void RemoveAvailability(string taxYear)
        {
            this.availabilities.RemoveAll(x => x.TaxYear == taxYear);
        }
    }
}
