// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// This is the tax form configuration entity that contains
    /// information about tax forms parameters.
    /// Second version of TaxFormConfiguration because of the addition of the 1099-NEC
    /// tax form to the list of tax forms. Instead of using the TaxForms DTO, we
    /// moved to TaxFormTypes in the domain so, if new forms are added, the APIs
    /// do not have to be versioned again.
    /// </summary>
    [Serializable]
    public class TaxFormConfiguration2
    {
        /// <summary>
        /// The tax form for which we are getting the consent paragraphs
        /// W-2, 1095-C, 1098-T, etc.
        /// </summary>
        public string TaxForm { get { return this.taxForm; } }
        private readonly string taxForm;

        /// <summary>
        /// The tax form consent given and consent withheld paragraphs.
        /// </summary>
        public TaxFormConsentParagraph ConsentParagraphs { get; set; }

        /// <summary>
        /// Set of availability dates for the tax form tax years.
        /// </summary>
        public ReadOnlyCollection<TaxFormAvailability> Availabilities { get; private set; }
        private readonly List<TaxFormAvailability> availabilities = new List<TaxFormAvailability>();

        /// <summary>
        /// For this tax form type, define if the user may view their form online when they have not consented to ONLY viewing their form online.
        /// </summary>
        public bool IsBypassingConsentPermitted { get; set; }

        /// <summary>
        /// Constructor that initializes the Tax Form Configuration object.
        /// </summary>
        /// <param name="taxFormId">The tax form</param>
        public TaxFormConfiguration2(string taxForm)
        {
            this.taxForm = taxForm;
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
                throw new ArgumentNullException("availability", "Availability object cannot be null.");
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
        /// Remove the availability that matches the tax year.
        /// </summary>
        /// <param name="taxYear">Tax year</param>
        public void RemoveAvailability(string taxYear)
        {
            this.availabilities.RemoveAll(x => x.TaxYear == taxYear);
        }
    }
}
