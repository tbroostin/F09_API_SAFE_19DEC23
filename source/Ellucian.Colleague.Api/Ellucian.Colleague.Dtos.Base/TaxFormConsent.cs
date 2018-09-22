// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Represents a person's choice whether or not to receive their tax form online.
    /// </summary>
    public class TaxFormConsent
    {
        /// <summary>
        /// Identifier of the person who made the consent choice.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Identifier of the specific tax form.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TaxForms TaxForm { get; set; }

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
