// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// An approval document
    /// </summary>
    [Serializable]
    public class ApprovalDocument
    {
        // Private fields
        private string _id;
        private readonly List<string> _text;

        /// <summary>
        /// ID of approval document
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    throw new InvalidOperationException("Approval document ID cannot be changed.");
                }
            }
        }

        /// <summary>
        /// Text of approval document
        /// </summary>
        public ReadOnlyCollection<string> Text { get; private set; }

        /// <summary>
        /// Person ID for whom document applies
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Constructor for approval document
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="text">Document text</param>
        public ApprovalDocument(string id, IEnumerable<string> text)
        {
            if (text == null || text.Count() == 0)
            {
                throw new ArgumentNullException("text","Approval document text cannot be null or empty");
            }

            _id = id;
            _text = text.ToList();
            Text = _text.AsReadOnly();
        }
    }
}
