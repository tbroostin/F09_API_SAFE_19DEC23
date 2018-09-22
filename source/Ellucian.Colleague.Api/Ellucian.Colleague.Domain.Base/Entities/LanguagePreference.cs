// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible language preferences
    /// </summary>
    [Serializable]
    public enum LanguagePreference
    {
        /// <summary>
        /// Primary
        /// </summary>
        Primary,
        /// <summary>
        /// Secondary
        /// </summary>
        Secondary
    }
}