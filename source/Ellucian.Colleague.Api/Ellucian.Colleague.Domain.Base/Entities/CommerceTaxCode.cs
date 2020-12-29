// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Commerce Tax Code
    /// </summary>
    [Serializable]
    public class CommerceTaxCode : GuidCodeItem
    {

        /// <summary>
        /// The use-tax fields are used to indicate that special processing for the tax code exists.
        /// </summary>
        public bool UseTaxFlag { get; set; }

        /// <summary>
        ///  Allow entry of code on purchase orders and vouchers
        /// </summary>
        public bool AppurEntryFlag { get; set; }

        /// <summary>
        /// The first date that the tax code is effective.
        /// </summary>
        public List<DateTime?> ApTaxEffectiveDates { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommerceTaxCode"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the Commerce Tax Code</param>
        /// <param name="description">Description or Title of the Commerce Tax Code </param>
        public CommerceTaxCode(string guid, string code, string description)
            : base (guid, code, description)
        {

        }
    }
}
