// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Commerce Tax Code Rates
    /// </summary>
    [Serializable]
    public class CommerceTaxCodeRate : GuidCodeItem
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
        /// Used to calculate tax amounts
        /// </summary>
        public Decimal? ApTaxPercent { get; set; }

        /// <summary>
        /// used in determining how much to expense to cost centers, and consequently, how much the institution
        /// is due back from the government.
        /// </summary>
        public Decimal? ApTaxRebatePercent { get; set; }

        /// <summary>
        /// Some places charge value added taxes excluding sales tax, others charge value added taxes including sales tax, or
        /// other combinations.Taxes at the same compounding sequence are calculated based on the total so far without
        ///any other taxes.
        /// </summary>
        public int? ApTaxCompoundingSequence { get; set; }

        /// <summary>
        /// When tax amounts are calculated, the rates that are effective for the transaction date are used.
        /// </summary>
        public DateTime? ApTaxEffectiveDate { get; set; }

        /// <summary>
        /// This exemption is used in determining the refund or investment tax credit(ITC).
        /// </summary>
        public Decimal? ApTaxExemptPercent { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommerceTaxCodeRate"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the Commerce Tax Code Rate</param>
        /// <param name="description">Description or Title of the Commerce Tax Code Rate</param>
        public CommerceTaxCodeRate(string guid, string code, string description)
            : base(guid, code, description)
        {

        }
    }
}