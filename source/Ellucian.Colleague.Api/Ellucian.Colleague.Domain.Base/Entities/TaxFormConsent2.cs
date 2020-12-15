// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// This entity contains information about a consent action
    /// associated with viewing a tax form online for a specific person.
    /// Second version of TaxFormConsent because of the addition of the 1099-NEC
    /// tax form to the list of tax forms. Instead of using the TaxForms DTO, we
    /// moved to TaxFormTypes in the domain so, if new forms are added, the APIs
    /// do not have to be versioned again.
    /// </summary>
    [Serializable]
    public class TaxFormConsent2
    {
        /// <summary>
        /// The person ID.
        /// </summary>
        public string PersonId { get{ return personId; } }
        private readonly string personId;

        /// <summary>
        /// The tax form: 1099-MISC, W-2, etc.
        /// </summary>
        public string TaxForm { get{ return taxForm; } }
        private string taxForm;

        /// <summary>
        /// The consent to view the form online.
        /// </summary>
        public bool HasConsented { get { return hasConsented; } }
        private bool hasConsented;

        /// <summary>
        /// The datetime associated with the consent choice.
        /// </summary>
        public DateTimeOffset TimeStamp { get { return timeStamp; } }
        private DateTimeOffset timeStamp;

        /// <summary>
        /// This constructor initializes a tax form consent entity.
        /// </summary>
        /// <param name="personId">This is the person ID.</param>
        /// <param name="taxFormType">This is the tax form type: 1099-MISC, etc</param>
        /// <param name="hasConsented">This indicates whether consent has been given.</param>
        /// <param name="timeStamp">This is the date and time of the consent choice.</param>
        public TaxFormConsent2(string personId, string taxForm, bool hasConsented, DateTimeOffset timeStamp)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is a required field.");

            if (string.IsNullOrEmpty(taxForm))
                throw new ArgumentNullException("taxFormType", "The tax form type must be specified.");

            this.personId = personId;
            this.taxForm = taxForm;
            this.hasConsented = hasConsented;
            this.timeStamp = timeStamp;
        }
    }
}
