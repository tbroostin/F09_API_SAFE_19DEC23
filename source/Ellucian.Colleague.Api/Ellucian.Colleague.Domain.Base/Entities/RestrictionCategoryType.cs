// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Restriction category types
    /// </summary>
    [Serializable]
    public enum RestrictionCategoryType
    {
        /// <summary>
        /// Academic
        /// </summary>
        Academic,
        /// <summary>
        /// Administrative
        /// </summary>
        Administrative,
        /// <summary>
        /// Disciplinary
        /// </summary>
        Disciplinary,
        /// <summary>
        /// Financial
        /// </summary>
        Financial,
        /// <summary>
        /// Health
        /// </summary>
        Health
    }
}
