﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// A list of associated general ledger transactions.
    /// </summary>
    [DataContract]
    public class GeneralLedgerTransactionDtoProperty2
    {
        /// <summary>
        /// The type of the general ledger transaction (e.g. journal entry, encumbrance, budget).
        /// </summary>
        [JsonProperty("type")]
        public Dtos.EnumProperties.GeneralLedgerTransactionType? Type { get; set; }

        /// <summary>
        /// A source reference number/document number for the transaction.
        /// </summary>
        [JsonProperty("referenceNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// A sequential number associated with the transaction.
        /// </summary>
        [JsonProperty("transactionNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TransactionNumber { get; set; }

        /// <summary>
        /// The date the transaction is credited/debited to the account in the general ledger (i.e. posting date).
        /// </summary>
        [JsonProperty("ledgerDate")]
        public DateTimeOffset? LedgerDate { get; set; }

        /// <summary>
        /// A supplementary date assigned to the transaction based on the transaction type.
        /// </summary>
        [JsonProperty("transactionTypeReferenceDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset? TransactionTypeReferenceDate { get; set; }

        /// <summary>
        /// The person or organization associated with the transaction.
        /// </summary>
        [JsonProperty("reference", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GeneralLedgerReferenceDtoProperty Reference { get; set; }

        /// <summary>
        /// The detailed accounting lines associated with the transaction.
        /// </summary>
        [JsonProperty("transactionDetailLines")]
        public List<GeneralLedgerDetailDtoProperty2> TransactionDetailLines { get; set; }
    }
}