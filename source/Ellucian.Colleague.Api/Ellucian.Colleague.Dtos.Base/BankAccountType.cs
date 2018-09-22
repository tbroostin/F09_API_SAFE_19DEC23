using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Categorize a bank account into Checking and Savings Types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BankAccountType
    {
        /// <summary>
        /// Checking Account Type
        /// </summary>
        Checking,

        /// <summary>
        /// Savings Account Type
        /// </summary>
        Savings
    }
}
