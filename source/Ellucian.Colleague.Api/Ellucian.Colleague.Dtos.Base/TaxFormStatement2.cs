// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Represents a tax form statement for which a user can obtain a pdf printout
    /// </summary>
    public class TaxFormStatement2
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
        /// Type of tax form.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TaxForms TaxForm { get; set; }

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
