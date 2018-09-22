// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a Journal Entry 
    /// </summary>
    [Serializable]
    public class JournalEntry : LedgerEntryDocument
    {
        /// <summary>
        /// Private status for public getter
        /// </summary>
        private readonly JournalEntryStatus status;

        /// <summary>
        /// The journal entry status
        /// </summary>
        public JournalEntryStatus Status { get { return status; } }

        /// <summary>
        /// Private status for public getter
        /// </summary>
        private readonly JournalEntryType type;

        /// <summary>
        /// The journal entry type
        /// </summary>
        public JournalEntryType Type { get { return type; } }

        /// <summary>
        /// The journal entry author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Indicate if journal entry was automatically reversed
        /// </summary>
        public bool AutomaticReversal { get; set; }

        /// <summary>
        /// Private list of associated items for the journal entry
        /// </summary>
        private readonly List<JournalEntryItem> items = new List<JournalEntryItem>();

        /// <summary>
        /// Public getter for the private list of items associated to the journal entry
        /// </summary>
        public ReadOnlyCollection<JournalEntryItem> Items { get; private set; }

        /// <summary>
        /// The journal entry total debits
        /// </summary>
        public decimal TotalDebits { get; set; }

        /// <summary>
        /// The journal entry total credits
        /// </summary>
        public decimal TotalCredits { get; set; }

        /// <summary>
        /// Constructor that initializes a journal entry domain entity
        /// </summary>
        /// <param name="id">Journal Entry number</param>
        /// <param name="date">Journal Entry posting date</param>
        /// <param name="status">Journal Entry status</param>
        /// <param name="type">Journal Entry type</param>
        /// <param name="enteredDate">Journal Entry entered date</param>
        /// <param name="enteredBy">Journal Entry entered by whom</param>
        public JournalEntry(string id, DateTime date, JournalEntryStatus status, JournalEntryType type, DateTime enteredDate, string enteredBy)
            : base(id, date, enteredDate, enteredBy)
        {
            if (string.IsNullOrEmpty(enteredBy))
            {
                throw new ArgumentNullException("Entered By", "The vendor name cannot be null.");
            }

            this.status = status;
            this.type = type;
            this.AutomaticReversal = false;
            Items = items.AsReadOnly();
            this.TotalCredits = 0;
            this.TotalDebits = 0;
        }

        /// <summary>
        /// This method adds an item to the list of items for the journal entry
        /// </summary>
        /// <param name="item">A journal entry item object</param>
        public void AddItem(JournalEntryItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "Item cannot be null");
            }

            if (items != null)
            {
                items.Add(item);
            }
        }
    }
}
