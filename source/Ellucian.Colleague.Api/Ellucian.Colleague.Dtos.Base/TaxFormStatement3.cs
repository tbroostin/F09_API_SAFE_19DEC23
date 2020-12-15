// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Represents a tax form statement for which a user can obtain a pdf printout.
    /// Third version of TaxFormStatement because of the addition of the 1099-NEC
    /// tax form to the list of tax forms. Instead of using the TaxForms DTO, we
    /// moved to TaxFormTypes in the domain so, if new forms are added, the APIs
    /// do not have to be versioned again.
    /// </summary>
    public class TaxFormStatement3
    {
        /// <summary>
        /// Record ID where the pdf data is stored
        /// </summary>
        public string PdfRecordId { get; set; }

        /// <summary>
        /// Person to whom this tax statement is associated.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Year of the tax form.
        /// </summary>
        public string TaxYear { get; set; }

        /// <summary>
        /// Id of state representing the current tax form.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Type of tax form: 1099-MISC, W-2, etc.
        /// </summary>
        public string TaxForm { get; set; }

        /// <summary>
        /// Is this statement available for printing, is it the original, a correction, etc
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TaxFormNotations2 Notation { get; set; }

        /// <summary>
        /// Date of when the record was created.
        /// </summary>
        public DateTime? AddDate { get; set; }
    }
}
