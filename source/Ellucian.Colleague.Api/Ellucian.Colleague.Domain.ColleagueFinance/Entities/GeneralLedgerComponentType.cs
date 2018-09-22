// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This enumeration contains the different types of GL components plus the entire GL account.
    /// </summary>
    [Serializable]
    public enum GeneralLedgerComponentType
    {
        /// <summary>
        /// Function component
        /// </summary>
        Function,

        /// <summary>
        /// Fund component
        /// </summary>
        Fund,

        /// <summary>
        /// Location component
        /// </summary>
        Location,

        /// <summary>
        /// Object component
        /// </summary>
        Object,

        /// <summary>
        /// Source component
        /// </summary>
        Source,

        /// <summary>
        /// Unit component
        /// </summary>
        Unit,

        /// <summary>
        /// Entire GL account
        /// </summary>
        FullAccount
    }
}