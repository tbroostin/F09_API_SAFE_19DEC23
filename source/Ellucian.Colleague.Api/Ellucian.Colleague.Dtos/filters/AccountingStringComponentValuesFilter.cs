// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The details of an accounting string component. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountingStringComponentValuesFilter
    {
           
        /// <summary>
        /// Accounting string component used in the accounting string.
        /// </summary>
        [JsonProperty("component")]
        [FilterProperty("criteria")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 Component { get; set; }
   
        /// <summary>
        /// Indicates if accounting string component value is available for transaction entry.
        /// </summary>
        [JsonProperty("transactionStatus")]
        [FilterProperty("criteria")]
        public AccountingTransactionStatus? TransactionStatus { get; set; }

        /// <summary>
        /// Type of the account component.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public AccountingStringComponentValuesType Type { get; set; }

        /// <summary>
        /// Type of the account component.  
        /// </summary>
        [JsonProperty("typeAccount")]
        [FilterProperty("criteria", Ignore = true)]
        public AccountingTypeAccount? TypeAccount { get; set; }

        /// <summary>
        /// Type of fund of the account component. Not used in Colleague
        /// </summary>
        [JsonProperty("typeFund", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria", Ignore = true)]
        public string TypeFund { get; set; }
    }
}