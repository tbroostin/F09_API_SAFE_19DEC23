// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A Journal Entry DTO
    /// </summary>
    public class JournalEntry
    {
        /// <summary>
        /// The journal Entry Number
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The journal entry status
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public JournalEntryStatus Status { get; set; }

        /// <summary>
        /// The journal entry type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public JournalEntryType Type { get; set; }

        /// <summary>
        /// The journal entry posting date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The date in which the journal entry was created
        /// </summary>
        public DateTime EnteredDate { get; set; }

        /// <summary>
        /// The name of the person that added the journal entry
        /// </summary>
        public string EnteredByName { get; set; }

        /// <summary>
        /// The journal entry author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The journal entry comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// The journal entry total debits
        /// </summary>
        public decimal TotalDebits { get; set; }

        /// <summary>
        /// The journal entry total credits
        /// </summary>
        public decimal TotalCredits { get; set; }

        /// <summary>
        /// Indicate if journal entry was automatically reversed
        /// </summary>
        public bool AutomaticReversal { get; set; }

        /// <summary>
        /// Returns the list of approvers and next approvers for the journal entry
        /// </summary>
        public List<Approver> Approvers { get; set; }

        /// <summary>
        /// Returns the list of items for the journal entry
        /// </summary>
        public List<JournalEntryItem> Items { get; set; }
    }
}
