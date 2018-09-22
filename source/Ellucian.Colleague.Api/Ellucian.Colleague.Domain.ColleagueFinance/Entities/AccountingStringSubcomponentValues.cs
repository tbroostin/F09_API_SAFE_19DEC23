//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// FdDescs
    /// </summary>
    [Serializable]
    public class AccountingStringSubcomponentValues : GuidCodeItem
    {

        /// <summary>
        /// The explanation
        /// </summary>
        public string Explanation { get; set; }


        /// <summary>
        /// type
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FdDescs"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public AccountingStringSubcomponentValues(string guid, string code, string description, string type)
            : base(guid, code, description)
        {
            Type = type;
        }
    }
}