// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// This entity contains information about a consent action
    /// associated with viewing a tax form online for a specific person.
    /// </summary>
    [Serializable]
    public class TaxFormConsent
    {
        /// <summary>
        /// Private variable for the person id.
        /// </summary>
        private readonly string personId;

        /// <summary>
        /// This is the public getter for the private person id.
        /// </summary>
        public string PersonId { get{ return personId; } }

        /// <summary>
        /// Private variable for the tax form.
        /// </summary>
        private TaxForms taxForm;

        /// <summary>
        /// This is the public getter for the private tax form.
        /// </summary>
        public TaxForms TaxForm { get{ return taxForm; } }

        /// <summary>
        /// Private variable for indicating consent to view the form online.
        /// </summary>
        private bool hasConsented;

        /// <summary>
        /// This is the public getter for the private consent.
        /// </summary>
        public bool HasConsented { get { return hasConsented; } }

        /// <summary>
        /// Private variable for datetime associated with the consent choice.
        /// </summary>
        private DateTimeOffset timeStamp;

        /// <summary>
        /// This is the public getter for the private timestamp.
        /// </summary>
        public DateTimeOffset TimeStamp { get { return timeStamp; } }

        /// <summary>
        /// This constructor initializes a tax form consent entity.
        /// </summary>
        /// <param name="personId">This is the person id.</param>
        /// <param name="taxForm">This is the tax form.</param>
        /// <param name="hasConsented">This indicates whether consent has been given.</param>
        /// <param name="timeStamp">This is the date and time of the consent choice.</param>
        public TaxFormConsent(string personId, TaxForms taxForm, bool hasConsented, DateTimeOffset timeStamp)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is a required field.");
            
            this.personId = personId;
            this.taxForm = taxForm;
            this.hasConsented = hasConsented;
            this.timeStamp = timeStamp;
        }
    }
}
