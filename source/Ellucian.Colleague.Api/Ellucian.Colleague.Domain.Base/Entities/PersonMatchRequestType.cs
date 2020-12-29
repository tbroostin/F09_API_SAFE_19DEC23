// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible person match category types
    /// </summary>
    [Serializable]
    public enum PersonMatchRequestType
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// initial
        /// </summary>
        Initial,

        /// <summary>
        /// final
        /// </summary>
        Final
    }
}
