// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible person match category types
    /// </summary>
    [Serializable]
    public enum PersonMatchCategoryType
    {
        /// <summary>
        /// This is a definite match
        /// </summary>
        Definite,
        /// <summary>
        /// This might be a match
        /// </summary>
        Potential,
    }
}
