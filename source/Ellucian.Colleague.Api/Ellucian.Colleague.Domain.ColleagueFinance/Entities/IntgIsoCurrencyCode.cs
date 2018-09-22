// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    [Serializable]
    public class IntgIsoCurrencyCode : GuidCodeItem
    {
        public string Category { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntgIsoCurrencyCode"/> class.
        /// </summary>
       
        public IntgIsoCurrencyCode(string guid, string code, string description, string type)
            : base(guid, code, description)
        {
            Category = type;
        }
    }
}