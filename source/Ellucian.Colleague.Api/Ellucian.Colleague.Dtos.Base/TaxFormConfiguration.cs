// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Tax form configuration DTO
    /// </summary>
    public class TaxFormConfiguration
    {
        /// <summary>
        /// The tax form for which we have the consent paragraphs
        /// W-2, 1095-C, 1098-T, etc.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TaxForms TaxFormId { get; set; }

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
    }
}
