// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Defines a text document
    /// </summary>
    [Serializable]
    public class TextDocument
    {
        private readonly List<string> _Text;

        /// <summary>
        /// A list of strings containing the document text
        /// </summary>
        public List<string> Text { get { return _Text; } }

        /// <summary>
        /// The document subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Creates a text document object
        /// </summary>
        /// <param name="text">List of text for the document</param>
        public TextDocument(IEnumerable<string> text)
        {
            if (text == null || text.Count() == 0)
            {
                throw new ArgumentNullException("text", "Text must be specified.");
            }

            _Text = new List<string>(text);
        }
    }
}
