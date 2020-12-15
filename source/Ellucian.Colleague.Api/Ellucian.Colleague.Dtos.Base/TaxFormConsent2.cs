// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Represents a person's choice whether or not to receive their tax form online.
    /// Second version of TaxFormConsent because of the addition of the 1099-NEC
    /// tax form to the list of tax forms. Instead of using the TaxForms DTO, we
    /// moved to TaxFormTypes in the domain so, if new forms are added, the APIs
    /// do not have to be versioned again.
    /// </summary>
    public class TaxFormConsent2
    {
        /// <summary>
        /// Identifier of the person who made the consent choice.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Identifier of the specific tax form.
        /// </summary>
        public string TaxForm { get; set; }

        /// <summary>
        /// True means the person consented to view their tax form online; false means the person did not consent.
        /// </summary>
        public bool HasConsented { get; set; }

        /// <summary>
        /// Day and time when the consent choice was made.
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }
    }
}
