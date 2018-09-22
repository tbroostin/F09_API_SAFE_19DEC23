// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;


namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Enumeration of payment methods
    /// </summary>
    [Serializable]
    public enum PaymentMethod
    {
        NotSet,

        /// <summary>
        /// check
        /// </summary>
        Check,

        /// <summary>
        /// directDeposit
        /// </summary>
         Directdeposit,

        /// <summary>
        /// wire
        /// </summary>
        Wire,

        /// <summary>
        /// eCheck
        /// </summary>
        Echeck,

        /// <summary>
        /// creditCard
        /// </summary>
        Creditcard,

        /// <summary>
        /// debitCard
        /// </summary>
        Debitcard
    }
}