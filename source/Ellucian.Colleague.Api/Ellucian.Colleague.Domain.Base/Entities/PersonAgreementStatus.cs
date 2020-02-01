// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Status of a person agreement
    /// </summary>
    [Serializable]
    public enum PersonAgreementStatus
    {
        /// <summary>
        /// Indicates acceptance of the person agreement
        /// </summary>
        Accepted,
        /// <summary>
        /// Indicates declination of the person agreement
        /// </summary>
        Declined
    }
}
