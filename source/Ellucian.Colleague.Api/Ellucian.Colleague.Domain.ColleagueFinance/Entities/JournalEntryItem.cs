// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// An item that belongs to a journal entry
    /// </summary>
    [Serializable]
    public class JournalEntryItem : GlAccount
    {
        /// <summary>
        /// Private item description
        /// </summary>
        private readonly string description;

        /// <summary>
        /// Public getter for the item description
        /// </summary>
        public string Description { get { return description; } }

        /// <summary>
        /// Project ID for the journal entry item
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Project Number for the journal entry item
        /// </summary>
        public string ProjectNumber { get; set; }

        /// <summary>
        /// Project line item ID for the journal entry item
        /// </summary>
        public string ProjectLineItemId { get; set; }

        /// <summary>
        /// Project line item item code for the journal entry item
        /// </summary>
        public string ProjectLineItemCode { get; set; }

        /// <summary>
        /// The journal entry item debit amount
        /// </summary>
        public decimal? Debit { get; set; }

        /// <summary>
        /// The journal entry item credit amount
        /// </summary>
        public decimal? Credit { get; set; }

        /// <summary>
        /// Constructor that initializes a Journal Entry Item domain entity
        /// </summary>
        /// <param name="description">The journal entry item description</param>
        /// <param name="glAccount">The journal entry item GL account</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null</exception>
        public JournalEntryItem(string description, string glAccount)
            : base(glAccount)
        {
            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException("description", "Description is a required field");

            this.description = description;
        }
    }
}
