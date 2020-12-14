// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Tax form configuration DTO.
    /// Second version of TaxFormConfiguration because of the addition of the 1099-NEC
    /// tax form to the list of tax forms. Instead of using the TaxForms DTO, we
    /// moved to TaxFormTypes in the domain so, if new forms are added, the APIs
    /// do not have to be versioned again.
    /// </summary>
    public class TaxFormConfiguration2
    {
        /// <summary>
        /// The tax form for which we have the consent paragraphs
        /// W-2, 1095-C, 1098-T, etc.
        /// </summary>
        public string TaxForm { get; set; }

        /// <summary>
        /// Text that is displayed when consent is given for obtaining 
        /// the tax form information online.
        /// </summary>
        public string ConsentText { get; set; }

        /// <summary>
        /// Text that is displayed when consent is withheld for obtaining 
        /// the tax form information online.
        /// </summary>
        public string ConsentWithheldText { get; set; }

        /// <summary>
        /// For this tax form type, define if the user may view their form online when they have not consented to ONLY viewing their form online.
        /// </summary>
        public bool IsBypassingConsentPermitted { get; set; }
    }
}
