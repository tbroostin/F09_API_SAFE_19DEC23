// Copyright 2015 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Represents a tax form statement for which a user can obtain a pdf printout
    /// </summary>
    public class TaxFormStatement
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
        /// Type of tax form.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Dtos.Base.TaxForms TaxForm { get; set; }

        /// <summary>
        /// Is this statement available for printing, is it the original, a correction, etc
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TaxFormNotations Notation { get; set; }
    }
}
