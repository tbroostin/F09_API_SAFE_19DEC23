// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// The outcome status of the person matching request.
    /// </summary>
    [Serializable]
    public enum PersonMatchRequestStatus
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// newPerson
        /// </summary>
        NewPerson,

        /// <summary>
        /// secondary
        /// </summary>
        ExistingPerson,

        /// <summary>
        /// reviewRequired
        /// </summary>
        ReviewRequired,

        /// <summary>
        /// rejectedRequest
        /// </summary>
        RejectedRequest
    }
}
