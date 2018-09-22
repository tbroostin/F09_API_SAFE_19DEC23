// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentMethodCategory
    {
        /// <summary>
        /// Payment by cash
        /// </summary>
        Cash,

        /// <summary>
        /// Payment by check, including electronic checks
        /// </summary>
        Check,

        /// <summary>
        /// Payment by credit card
        /// </summary>
        CreditCard,

        /// <summary>
        /// Payment made in stocks and/or securities
        /// </summary>
        StocksAndSecurities,

        /// <summary>
        /// Gifts of real, tangible property (e.g. real estate)
        /// </summary>
        RealTangibleProperty,

        /// <summary>
        /// Gifts of insurance and retirement benefits
        /// </summary>
        InsuranceAndRetirement,

        /// <summary>
        /// Gifts of contributed services
        /// </summary>
        ContributedServices,

        /// <summary>
        /// Other types of in-kind gifts
        /// </summary>
        OtherInKind,

        /// <summary>
        /// Payment via payroll deduction
        /// </summary>
        PayrollDeduction,

        /// <summary>
        /// Electronic funds transfer
        /// </summary>
        ElectronicFundsTransfer,

        /// <summary>
        /// Any other forms of payment accepted by the institution
        /// </summary>
        Other
    }
}
