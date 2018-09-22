// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Defines a text document
    /// </summary>
    public class TextDocument
    {
        /// <summary>
        /// The document text
        /// </summary>
        public List<string> Text { get; set; }

        /// <summary>
        /// The document subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// TextDocument constructor
        /// </summary>
        public TextDocument()
        {
            Text = new List<string>();
        }
    }
}
