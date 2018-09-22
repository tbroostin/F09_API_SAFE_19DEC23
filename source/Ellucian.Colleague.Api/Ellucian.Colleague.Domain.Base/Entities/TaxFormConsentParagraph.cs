// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// This is the tax form consent paragraphs entity which contains
    /// information about tax form consents text.
    /// </summary>
    [Serializable]
    public class TaxFormConsentParagraph
    {
        /// <summary>
        /// This is the text that is displayed when a person consents
        /// to viewing their tax forms online.
        /// </summary>
        public string ConsentText { get; set; }

        /// <summary>
        /// This is the text that is displayed when a person withholds
        /// their consent to viewing their tax forms online.
        /// </summary>
        public string ConsentWithheldText { get; set; }
    }
}
