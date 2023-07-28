/*Copyright 2021-2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// EmployeeLeaveTransaction DTO
    /// </summary>
    public class EmployeeLeaveTransaction
    {
        /// <summary>
        /// The database Id of the transaction
        /// </summary>
        [JsonProperty("id")]
        [Metadata("PLD.LPN.ID", DataDescription = "Unique identifier of this Employee leave transaction.")]
        public int Id { get; set; }

        /// <summary>
        /// The amount of leave to be added or subtracted from the employee leave plan balance.
        /// The amount entered may be either positive or negative. If positive, it is added to the total; if negative, it is subtracted from the total.
        /// If the Type is Earned the amount is expected to be positive, but negative amounts are accepted.
        /// If the Type is Used the amount is expected to be negative, but positive amounts are accepted.
        /// If the Type is Adjusted, the amount may be either positive or negative.
        /// </summary>
        [JsonProperty("transactionHours")]
        [Metadata(" PLD.HOURS", DataDescription = "Transaction Hours for Employee Leave Transaction.")]
        public decimal TransactionHours { get; set; }       

        /// <summary>
        /// The date the transaction occurred, or will occur.
        /// </summary>
        [JsonProperty("date")]
        [Metadata("PLD.DATE", DataDescription = " Transaction Date for Employee Leave Transaction.")]
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// The type of transation, Earned, Used, Adjusted. 
        /// </summary>
        [JsonProperty("type")]
        [Metadata("PLD.ACTION", DataDescription = "Transaction Type for Employee Leave Transaction.")]
        [JsonConverter(typeof(StringEnumConverter))]
        public LeaveTransactionType Type { get; set; }

        /// <summary>
        /// The balance of the employee leave plan after this transaction is applied - forwarding to the next transaction
        /// </summary>
        [JsonProperty("forwardingBalance")]
        [Metadata("PLD.FORWARDING.BALANCE", DataDescription = "forwarding balance for Employee Leave Transaction.")]
        public decimal ForwardingBalance { get; set; }
    }
}
