// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an Citizenship Status
    /// </summary>
    [Serializable]
    public enum CitizenshipStatusType
    {
        /// <summary>
        /// Citizen
        /// </summary>
        Citizen,
        /// <summary>
        /// Non-Citizen
        /// </summary>
        NonCitizen,
    }
}