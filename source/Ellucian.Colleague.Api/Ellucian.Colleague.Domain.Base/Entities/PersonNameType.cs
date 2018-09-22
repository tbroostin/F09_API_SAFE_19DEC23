// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of a person name type
    /// </summary>
    [Serializable]
    public enum PersonNameType
    {
        /// <summary>
        /// personal
        /// </summary>
        Personal,
        
        /// <summary>
        /// birth
        /// </summary>
        Birth,

        /// <summary>
        /// legal
        /// </summary>
        Legal,

        /// <summary>
        /// chosen
        /// </summary>
        Chosen
    }
}