/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Categorize a bank account into Checking and Savings Types
    /// </summary>
    [Serializable]
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
