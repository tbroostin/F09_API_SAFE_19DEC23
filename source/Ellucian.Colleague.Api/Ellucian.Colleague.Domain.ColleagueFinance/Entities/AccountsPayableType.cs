// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Class for AP Type
    /// </summary>
    [Serializable]
    public class AccountsPayableType : CodeItem
    {
        /// <summary>
        /// Bank Code
        /// </summary>
        public string BankCode { get; set;  }
    
        /// <summary>
        /// AP Type constructor
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public AccountsPayableType(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}