﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The details of transactions to be entered into the general ledger.
    /// </summary>
    [DataContract]
    public class GeneralLedgerTransaction3 : BaseModel2
    {
        /// <summary>
        /// An indicator that specifies if the authoritative source should just validate
        /// the accounting string or if it should post/update a general ledger transaction.
        /// </summary>
        [JsonProperty("processMode")]
        public Dtos.EnumProperties.ProcessMode2? ProcessMode { get; set; }

        /// <summary>
        /// The person submitting the general ledger transactions.
        /// </summary>
        [DataMember(Name = "submittedBy", EmitDefaultValue = false)]
        public GuidObject2 SubmittedBy { get; set; }

        /// <summary>
        /// The comment associated with the transaction.
        /// </summary>
        [JsonProperty("comment", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        //[DataMember(Name = "comment", EmitDefaultValue = false)]
        public string Comment { get; set; }

        /// <summary>
        /// A list of associated general ledger transactions.
        /// </summary>
        [JsonProperty("transactions")]
        public List<GeneralLedgerTransactionDtoProperty3> Transactions { get; set; }        
    }
}